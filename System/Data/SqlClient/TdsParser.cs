namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal sealed class TdsParser
    {
        private static readonly IEnumerable<SqlDataRecord> __tvpEmptyValue = new List<SqlDataRecord>().AsReadOnly();
        private SqlErrorCollection _attentionErrors;
        private SqlErrorCollection _attentionWarnings;
        private SqlInternalConnectionTds _connHandler;
        private SqlInternalTransaction _currentTransaction;
        private int _defaultCodePage;
        private SqlCollation _defaultCollation;
        internal Encoding _defaultEncoding;
        private int _defaultLCID;
        private System.Data.SqlClient.EncryptionOptions _encryptionOption = _sniSupportedEncryptionOption;
        private object _ErrorCollectionLock = new object();
        private SqlErrorCollection _errors;
        private bool _fAsync;
        internal volatile bool _fAwaitingPreLogin;
        private bool _fMARS;
        internal volatile bool _fPreLoginErrorOccurred;
        internal volatile bool _fPreserveTransaction;
        internal volatile bool _fResetConnection;
        private const ulong _indeterminateSize = ulong.MaxValue;
        private bool _isKatmai;
        private bool _isShiloh;
        private bool _isShilohSP1;
        private bool _isYukon;
        private int _nonTransactedOpenResultCount;
        internal readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
        private static int _objectTypeCount;
        private SqlInternalTransaction _pendingTransaction;
        internal TdsParserStateObject _physicalStateObj;
        internal TdsParserStateObject _pMarsPhysicalConObj;
        internal AutoResetEvent _resetConnectionEvent;
        private long _retainedTransactionId;
        private string _server = "";
        private TdsParserSessionPool _sessionPool;
        private byte[] _sniSpnBuffer;
        private static System.Data.SqlClient.EncryptionOptions _sniSupportedEncryptionOption = SNILoadHandle.SingletonInstance.Options;
        internal TdsParserState _state;
        private SqlStatistics _statistics;
        private bool _statisticsIsInTransaction;
        private SqlErrorCollection _warnings;
        private const int ATTENTION_TIMEOUT = 0x1388;
        private byte[] datetimeBuffer = new byte[10];
        private static bool s_fSSPILoaded = false;
        private static readonly byte[] s_longDataHeader = new byte[] { 
            0x10, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
         };
        private static volatile uint s_maxSSPILength = 0;
        private static byte[] s_nicAddress;
        private static object s_tdsParserLock = new object();
        private static readonly byte[] s_xmlMetadataSubstituteSequence = new byte[] { 0xe7, 0xff, 0xff, 0, 0, 0, 0, 0 };
        private const string StateTraceFormatString = "\n\t         _physicalStateObj = {0}\n\t         _pMarsPhysicalConObj = {1}\n\t         _state = {2}\n\t         _server = {3}\n\t         _fResetConnection = {4}\n\t         _defaultCollation = {5}\n\t         _defaultCodePage = {6}\n\t         _defaultLCID = {7}\n\t         _defaultEncoding = {8}\n\t         _encryptionOption = {10}\n\t         _currentTransaction = {11}\n\t         _pendingTransaction = {12}\n\t         _retainedTransactionId = {13}\n\t         _nonTransactedOpenResultCount = {14}\n\t         _connHandler = {15}\n\t         _fAsync = {16}\n\t         _fMARS = {17}\n\t         _fAwaitingPreLogin = {18}\n\t         _fPreLoginErrorOccurred = {19}\n\t         _sessionPool = {20}\n\t         _isShiloh = {21}\n\t         _isShilohSP1 = {22}\n\t         _isYukon = {23}\n\t         _sniSpnBuffer = {24}\n\t         _errors = {25}\n\t         _warnings = {26}\n\t         _attentionErrors = {27}\n\t         _attentionWarnings = {28}\n\t         _statistics = {29}\n\t         _statisticsIsInTransaction = {30}\n\t         _fPreserveTransaction = {31}         _fParallel = {32}";

        internal TdsParser(bool MARS, bool fAsynchronous)
        {
            this._fMARS = MARS;
            this._fAsync = fAsynchronous;
            this._physicalStateObj = new TdsParserStateObject(this);
        }

        private bool AddSqlErrorToCollection(ref SqlErrorCollection temp, ref SqlErrorCollection InputCollection)
        {
            if (InputCollection == null)
            {
                return false;
            }
            bool flag = false;
            if (temp == null)
            {
                temp = new SqlErrorCollection();
            }
            for (int i = 0; i < InputCollection.Count; i++)
            {
                SqlError error = InputCollection[i];
                temp.Add(error);
                if (error.Class >= 20)
                {
                    flag = true;
                }
            }
            InputCollection = null;
            return (flag && (TdsParserState.Closed != this._state));
        }

        internal static decimal AdjustDecimalScale(decimal value, int newScale)
        {
            int num2 = (decimal.GetBits(value)[3] & 0xff0000) >> 0x10;
            if (newScale != num2)
            {
                SqlDecimal n = new SqlDecimal(value);
                return SqlDecimal.AdjustScale(n, newScale - num2, false).Value;
            }
            return value;
        }

        internal static SqlDecimal AdjustSqlDecimalScale(SqlDecimal d, int newScale)
        {
            if (d.Scale != newScale)
            {
                return SqlDecimal.AdjustScale(d, newScale - d.Scale, false);
            }
            return d;
        }

        internal void CheckResetConnection(TdsParserStateObject stateObj)
        {
            if (this._fResetConnection && !stateObj._fResetConnectionSent)
            {
                try
                {
                    if ((this._fAsync && this._fMARS) && !stateObj._fResetEventOwned)
                    {
                        stateObj._fResetEventOwned = this._resetConnectionEvent.WaitOne(TdsParserStaticMethods.GetTimeoutMilliseconds(stateObj.TimeoutTime), false);
                        if (stateObj._fResetEventOwned && stateObj.TimeoutHasExpired)
                        {
                            stateObj._fResetEventOwned = !this._resetConnectionEvent.Set();
                            stateObj.TimeoutTime = 0L;
                        }
                        if (!stateObj._fResetEventOwned)
                        {
                            stateObj.ResetBuffer();
                            this.Errors.Add(new SqlError(-2, 0, 11, this._server, SQLMessage.Timeout(), "", 0));
                            this.ThrowExceptionAndWarning();
                        }
                    }
                    if (this._fResetConnection)
                    {
                        if (this._fPreserveTransaction)
                        {
                            stateObj._outBuff[1] = (byte) (stateObj._outBuff[1] | 0x10);
                        }
                        else
                        {
                            stateObj._outBuff[1] = (byte) (stateObj._outBuff[1] | 8);
                        }
                        if (!this._fAsync || !this._fMARS)
                        {
                            this._fResetConnection = false;
                            this._fPreserveTransaction = false;
                        }
                        else
                        {
                            stateObj._fResetConnectionSent = true;
                        }
                    }
                    else if ((this._fAsync && this._fMARS) && stateObj._fResetEventOwned)
                    {
                        stateObj._fResetEventOwned = !this._resetConnectionEvent.Set();
                    }
                }
                catch (Exception)
                {
                    if ((this._fAsync && this._fMARS) && stateObj._fResetEventOwned)
                    {
                        stateObj._fResetConnectionSent = false;
                        stateObj._fResetEventOwned = !this._resetConnectionEvent.Set();
                    }
                    throw;
                }
            }
        }

        private void CommonProcessMetaData(TdsParserStateObject stateObj, _SqlMetaData col)
        {
            uint num5;
            int length = 0;
            if (this.IsYukonOrNewer)
            {
                num5 = stateObj.ReadUInt32();
            }
            else
            {
                num5 = stateObj.ReadUInt16();
            }
            byte num3 = stateObj.ReadByte();
            col.updatability = (byte) ((num3 & 11) >> 2);
            col.isNullable = 1 == (num3 & 1);
            col.isIdentity = 0x10 == (num3 & 0x10);
            stateObj.ReadByte();
            col.isColumnSet = 4 == (num3 & 4);
            byte tdsType = stateObj.ReadByte();
            if (tdsType == 0xf1)
            {
                col.length = 0xffff;
            }
            else if (this.IsVarTimeTds(tdsType))
            {
                col.length = 0;
            }
            else if (tdsType == 40)
            {
                col.length = 3;
            }
            else
            {
                col.length = this.GetTokenLength(tdsType, stateObj);
            }
            col.metaType = MetaType.GetSqlDataType(tdsType, num5, col.length);
            col.type = col.metaType.SqlDbType;
            if (this._isShiloh)
            {
                col.tdsType = col.isNullable ? col.metaType.NullableType : col.metaType.TDSType;
            }
            else
            {
                col.tdsType = tdsType;
            }
            if (this._isYukon)
            {
                if (240 == tdsType)
                {
                    this.ProcessUDTMetaData(col, stateObj);
                }
                if (col.length == 0xffff)
                {
                    col.metaType = MetaType.GetMaxMetaTypeFromMetaType(col.metaType);
                    col.length = 0x7fffffff;
                    if ((tdsType == 0xf1) && ((stateObj.ReadByte() & 1) != 0))
                    {
                        length = stateObj.ReadByte();
                        if (length != 0)
                        {
                            col.xmlSchemaCollectionDatabase = stateObj.ReadString(length);
                        }
                        length = stateObj.ReadByte();
                        if (length != 0)
                        {
                            col.xmlSchemaCollectionOwningSchema = stateObj.ReadString(length);
                        }
                        length = stateObj.ReadInt16();
                        if (length != 0)
                        {
                            col.xmlSchemaCollectionName = stateObj.ReadString(length);
                        }
                    }
                }
            }
            if (col.type == SqlDbType.Decimal)
            {
                col.precision = stateObj.ReadByte();
                col.scale = stateObj.ReadByte();
            }
            if (col.metaType.IsVarTime)
            {
                col.scale = stateObj.ReadByte();
                switch (col.metaType.SqlDbType)
                {
                    case SqlDbType.Time:
                        col.length = MetaType.GetTimeSizeFromScale(col.scale);
                        break;

                    case SqlDbType.DateTime2:
                        col.length = 3 + MetaType.GetTimeSizeFromScale(col.scale);
                        break;

                    case SqlDbType.DateTimeOffset:
                        col.length = 5 + MetaType.GetTimeSizeFromScale(col.scale);
                        break;
                }
            }
            if ((this._isShiloh && col.metaType.IsCharType) && (tdsType != 0xf1))
            {
                col.collation = this.ProcessCollation(stateObj);
                int codePage = this.GetCodePage(col.collation, stateObj);
                if (codePage == this._defaultCodePage)
                {
                    col.codePage = this._defaultCodePage;
                    col.encoding = this._defaultEncoding;
                }
                else
                {
                    col.codePage = codePage;
                    col.encoding = Encoding.GetEncoding(col.codePage);
                }
            }
            if (col.metaType.IsLong && !col.metaType.IsPlp)
            {
                if (this._isYukon)
                {
                    int num7 = 0xffff;
                    col.multiPartTableName = this.ProcessOneTable(stateObj, ref num7);
                }
                else
                {
                    length = stateObj.ReadUInt16();
                    string multipartName = stateObj.ReadString(length);
                    col.multiPartTableName = new MultiPartTableName(multipartName);
                }
            }
            length = stateObj.ReadByte();
            col.column = stateObj.ReadString(length);
            stateObj._receivedColMetaData = true;
        }

        internal void Connect(ServerInfo serverInfo, SqlInternalConnectionTds connHandler, bool ignoreSniOpenTimeout, long timerExpire, bool encrypt, bool trustServerCert, bool integratedSecurity)
        {
            if (this._state == TdsParserState.Closed)
            {
                this._connHandler = connHandler;
                if (SNILoadHandle.SingletonInstance.SNIStatus != 0)
                {
                    this.Errors.Add(this.ProcessSNIError(this._physicalStateObj));
                    this._physicalStateObj.Dispose();
                    this.ThrowExceptionAndWarning();
                }
                if (connHandler.ConnectionOptions.LocalDBInstance != null)
                {
                    LocalDBAPI.CreateLocalDBInstance(connHandler.ConnectionOptions.LocalDBInstance);
                }
                if (integratedSecurity)
                {
                    this.LoadSSPILibrary();
                    this._sniSpnBuffer = new byte[SNINativeMethodWrapper.SniMaxComposedSpnLength];
                    Bid.Trace("<sc.TdsParser.Connect|SEC> SSPI authentication\n");
                }
                else
                {
                    this._sniSpnBuffer = null;
                    Bid.Trace("<sc.TdsParser.Connect|SEC> SQL authentication\n");
                }
                byte[] instanceName = null;
                bool multiSubnetFailover = this._connHandler.ConnectionOptions.MultiSubnetFailover;
                this._physicalStateObj.CreatePhysicalSNIHandle(serverInfo.ExtendedServerName, ignoreSniOpenTimeout, timerExpire, out instanceName, this._sniSpnBuffer, false, this._fAsync, multiSubnetFailover);
                if (this._physicalStateObj.Status != 0)
                {
                    this.Errors.Add(this.ProcessSNIError(this._physicalStateObj));
                    this._physicalStateObj.Dispose();
                    Bid.Trace("<sc.TdsParser.Connect|ERR|SEC> Login failure\n");
                    this.ThrowExceptionAndWarning();
                }
                this._server = serverInfo.ResolvedServerName;
                if (connHandler.PoolGroupProviderInfo != null)
                {
                    connHandler.PoolGroupProviderInfo.AliasCheck((serverInfo.PreRoutingServerName == null) ? serverInfo.ResolvedServerName : serverInfo.PreRoutingServerName);
                }
                this._state = TdsParserState.OpenNotLoggedIn;
                this._physicalStateObj.SniContext = SniContext.Snix_PreLoginBeforeSuccessfullWrite;
                this._physicalStateObj.TimeoutTime = timerExpire;
                bool marsCapable = false;
                this.SendPreLoginHandshake(instanceName, encrypt);
                this._physicalStateObj.SniContext = SniContext.Snix_PreLogin;
                switch (this.ConsumePreLoginHandshake(encrypt, trustServerCert, out marsCapable))
                {
                    case PreLoginHandshakeStatus.SphinxFailure:
                        this._fMARS = false;
                        this._physicalStateObj._sniPacket = null;
                        this._physicalStateObj.SniContext = SniContext.Snix_Connect;
                        this._physicalStateObj.CreatePhysicalSNIHandle(serverInfo.ExtendedServerName, ignoreSniOpenTimeout, timerExpire, out instanceName, this._sniSpnBuffer, false, this._fAsync, multiSubnetFailover);
                        if (this._physicalStateObj.Status != 0)
                        {
                            this.Errors.Add(this.ProcessSNIError(this._physicalStateObj));
                            Bid.Trace("<sc.TdsParser.Connect|ERR|SEC> Login failure\n");
                            this.ThrowExceptionAndWarning();
                        }
                        break;

                    case PreLoginHandshakeStatus.InstanceFailure:
                        this._physicalStateObj.Dispose();
                        this._physicalStateObj.SniContext = SniContext.Snix_Connect;
                        this._physicalStateObj.CreatePhysicalSNIHandle(serverInfo.ExtendedServerName, ignoreSniOpenTimeout, timerExpire, out instanceName, this._sniSpnBuffer, true, this._fAsync, multiSubnetFailover);
                        if (this._physicalStateObj.Status != 0)
                        {
                            this.Errors.Add(this.ProcessSNIError(this._physicalStateObj));
                            Bid.Trace("<sc.TdsParser.Connect|ERR|SEC> Login failure\n");
                            this.ThrowExceptionAndWarning();
                        }
                        this.SendPreLoginHandshake(instanceName, encrypt);
                        if (this.ConsumePreLoginHandshake(encrypt, trustServerCert, out marsCapable) == PreLoginHandshakeStatus.InstanceFailure)
                        {
                            Bid.Trace("<sc.TdsParser.Connect|ERR|SEC> Login failure\n");
                            throw SQL.InstanceFailure();
                        }
                        break;
                }
                if (this._fMARS && marsCapable)
                {
                    this._sessionPool = new TdsParserSessionPool(this);
                }
                else
                {
                    this._fMARS = false;
                }
            }
        }

        private PreLoginHandshakeStatus ConsumePreLoginHandshake(bool encrypt, bool trustServerCert, out bool marsCapable)
        {
            marsCapable = this._fMARS;
            bool flag = false;
            this._fAwaitingPreLogin = true;
            this._physicalStateObj.ReadNetworkPacket();
            this._fAwaitingPreLogin = false;
            if ((this._physicalStateObj._inBytesRead == 0) || this._fPreLoginErrorOccurred)
            {
                if (encrypt)
                {
                    this.Errors.Add(new SqlError(20, 0, 20, this._server, SQLMessage.EncryptionNotSupportedByServer(), "", 0));
                    this._physicalStateObj.Dispose();
                    this.ThrowExceptionAndWarning();
                }
                return PreLoginHandshakeStatus.SphinxFailure;
            }
            byte[] buff = new byte[(this._physicalStateObj._inBytesRead - this._physicalStateObj._inBytesUsed) - this._physicalStateObj._inputHeaderLen];
            this._physicalStateObj.ReadByteArray(buff, 0, buff.Length);
            if (buff[0] == 170)
            {
                throw SQL.InvalidSQLServerVersionUnknown();
            }
            int num = 0;
            int index = 0;
            for (int i = buff[num++]; i != 0xff; i = buff[num++])
            {
                System.Data.SqlClient.EncryptionOptions options;
                switch (i)
                {
                    case 0:
                    {
                        index = (buff[num++] << 8) | buff[num++];
                        byte num1 = buff[num++];
                        byte num10 = buff[num++];
                        byte num7 = buff[index];
                        byte num11 = buff[index + 1];
                        byte num12 = buff[index + 2];
                        byte num13 = buff[index + 3];
                        flag = num7 >= 9;
                        if (!flag)
                        {
                            marsCapable = false;
                        }
                        goto Label_0327;
                    }
                    case 1:
                    {
                        index = (buff[num++] << 8) | buff[num++];
                        byte num14 = buff[num++];
                        byte num15 = buff[num++];
                        options = (System.Data.SqlClient.EncryptionOptions) buff[index];
                        switch (this._encryptionOption)
                        {
                            case System.Data.SqlClient.EncryptionOptions.OFF:
                                goto Label_01D3;

                            case System.Data.SqlClient.EncryptionOptions.NOT_SUP:
                                goto Label_01EC;
                        }
                        goto Label_0227;
                    }
                    case 2:
                        goto Label_02BC;

                    case 3:
                        num += 4;
                        goto Label_0327;

                    case 4:
                    {
                        index = (buff[num++] << 8) | buff[num++];
                        byte num18 = buff[num++];
                        byte num19 = buff[num++];
                        marsCapable = buff[index] != 0;
                        goto Label_0327;
                    }
                    default:
                        num += 4;
                        goto Label_0327;
                }
                if (options == System.Data.SqlClient.EncryptionOptions.NOT_SUP)
                {
                    this.Errors.Add(new SqlError(20, 0, 20, this._server, SQLMessage.EncryptionNotSupportedByServer(), "", 0));
                    this._physicalStateObj.Dispose();
                    this.ThrowExceptionAndWarning();
                }
                goto Label_0227;
            Label_01D3:
                if (options == System.Data.SqlClient.EncryptionOptions.OFF)
                {
                    this._encryptionOption = System.Data.SqlClient.EncryptionOptions.LOGIN;
                }
                else if (options == System.Data.SqlClient.EncryptionOptions.REQ)
                {
                    this._encryptionOption = System.Data.SqlClient.EncryptionOptions.ON;
                }
                goto Label_0227;
            Label_01EC:
                if (options == System.Data.SqlClient.EncryptionOptions.REQ)
                {
                    this.Errors.Add(new SqlError(20, 0, 20, this._server, SQLMessage.EncryptionNotSupportedByClient(), "", 0));
                    this._physicalStateObj.Dispose();
                    this.ThrowExceptionAndWarning();
                }
            Label_0227:
                if ((this._encryptionOption != System.Data.SqlClient.EncryptionOptions.ON) && (this._encryptionOption != System.Data.SqlClient.EncryptionOptions.LOGIN))
                {
                    goto Label_0327;
                }
                uint info = (uint) (((encrypt && !trustServerCert) ? 1 : 0) | (flag ? 2 : 0));
                if (SNINativeMethodWrapper.SNIAddProvider(this._physicalStateObj.Handle, SNINativeMethodWrapper.ProviderEnum.SSL_PROV, ref info) != 0)
                {
                    this.Errors.Add(this.ProcessSNIError(this._physicalStateObj));
                    this.ThrowExceptionAndWarning();
                }
                try
                {
                    goto Label_0327;
                }
                finally
                {
                    this._physicalStateObj._sniPacket.Dispose();
                    this._physicalStateObj._sniPacket = new SNIPacket(this._physicalStateObj.Handle);
                }
            Label_02BC:
                index = (buff[num++] << 8) | buff[num++];
                byte num16 = buff[num++];
                byte num17 = buff[num++];
                byte num6 = 1;
                byte num5 = buff[index];
                if (num5 == num6)
                {
                    return PreLoginHandshakeStatus.InstanceFailure;
                }
            Label_0327:
                if (num >= buff.Length)
                {
                    break;
                }
            }
            return PreLoginHandshakeStatus.Successful;
        }

        private static unsafe void CopyCharsToBytes(char[] source, int sourceOffset, byte[] dest, int destOffset, int charLength)
        {
            if (charLength < 0)
            {
                throw ADP.InvalidDataLength((long) charLength);
            }
            if (((sourceOffset + charLength) > source.Length) || (sourceOffset < 0))
            {
                throw ADP.IndexOutOfRange(sourceOffset);
            }
            int count = charLength * ADP.CharSize;
            if (((destOffset + count) > dest.Length) || (destOffset < 0))
            {
                throw ADP.IndexOutOfRange(destOffset);
            }
            fixed (char* chRef = source)
            {
                char* chPtr = chRef;
                chPtr += sourceOffset;
                fixed (byte* numRef = dest)
                {
                    byte* numPtr = numRef;
                    numPtr += destOffset;
                    NativeOledbWrapper.MemoryCopy((IntPtr) numPtr, (IntPtr) chPtr, count);
                }
            }
        }

        private static unsafe void CopyStringToBytes(string source, int sourceOffset, byte[] dest, int destOffset, int charLength)
        {
            if (charLength < 0)
            {
                throw ADP.InvalidDataLength((long) charLength);
            }
            if (((sourceOffset + charLength) > source.Length) || (sourceOffset < 0))
            {
                throw ADP.IndexOutOfRange(sourceOffset);
            }
            int count = charLength * ADP.CharSize;
            if (((destOffset + count) > dest.Length) || (destOffset < 0))
            {
                throw ADP.IndexOutOfRange(destOffset);
            }
            fixed (char* str = ((char*) source))
            {
                char* chPtr = str;
                chPtr += sourceOffset;
                fixed (byte* numRef = dest)
                {
                    byte* numPtr = numRef;
                    numPtr += destOffset;
                    NativeOledbWrapper.MemoryCopy((IntPtr) numPtr, (IntPtr) chPtr, count);
                    str = null;
                }
            }
        }

        internal TdsParserStateObject CreateSession()
        {
            TdsParserStateObject obj2 = new TdsParserStateObject(this, this._pMarsPhysicalConObj.Handle, this._fAsync);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.TdsParser.CreateSession|ADV> %d# created session %d\n", this.ObjectID, obj2.ObjectID);
            }
            return obj2;
        }

        internal void Deactivate(bool connectionIsDoomed)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.TdsParser.Deactivate|ADV> %d# deactivating\n", this.ObjectID);
            }
            if (Bid.IsOn(Bid.ApiGroup.StateDump))
            {
                Bid.Trace("<sc.TdsParser.Deactivate|STATE> %d#, %ls\n", this.ObjectID, this.TraceString());
            }
            if (this.MARSOn)
            {
                this._sessionPool.Deactivate();
            }
            if (!connectionIsDoomed && (this._physicalStateObj != null))
            {
                if (this._physicalStateObj._pendingData)
                {
                    this._physicalStateObj.CleanWire();
                }
                if (this._physicalStateObj.HasOpenResult)
                {
                    this._physicalStateObj.DecrementOpenResultCount();
                }
            }
            SqlInternalTransaction currentTransaction = this.CurrentTransaction;
            if ((currentTransaction != null) && currentTransaction.HasParentTransaction)
            {
                currentTransaction.CloseFromConnection();
            }
            this.Statistics = null;
        }

        internal void DecrementNonTransactedOpenResultCount()
        {
            Interlocked.Decrement(ref this._nonTransactedOpenResultCount);
        }

        internal void Disconnect()
        {
            if (this._sessionPool != null)
            {
                this._sessionPool.Dispose();
            }
            if (this._state != TdsParserState.Closed)
            {
                this._state = TdsParserState.Closed;
                this._physicalStateObj.SniContext = SniContext.Snix_Close;
                if (this._fMARS)
                {
                    try
                    {
                        this._physicalStateObj.Dispose();
                        if (this._pMarsPhysicalConObj != null)
                        {
                            this._pMarsPhysicalConObj.Dispose();
                        }
                        return;
                    }
                    finally
                    {
                        this._pMarsPhysicalConObj = null;
                    }
                }
                this._physicalStateObj.Dispose();
            }
        }

        internal void DisconnectTransaction(SqlInternalTransaction internalTransaction)
        {
            if ((this._currentTransaction != null) && (this._currentTransaction == internalTransaction))
            {
                this._currentTransaction = null;
            }
        }

        internal void EnableMars()
        {
            if (this._fMARS)
            {
                this._pMarsPhysicalConObj = this._physicalStateObj;
                uint num = 0;
                uint info = 0;
                num = SNINativeMethodWrapper.SNIAddProvider(this._pMarsPhysicalConObj.Handle, SNINativeMethodWrapper.ProviderEnum.SMUX_PROV, ref info);
                if (num != 0)
                {
                    this.Errors.Add(this.ProcessSNIError(this._physicalStateObj));
                    this.ThrowExceptionAndWarning();
                }
                IntPtr zero = IntPtr.Zero;
                if (this._fAsync)
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        this._pMarsPhysicalConObj.IncrementPendingCallbacks();
                        num = SNINativeMethodWrapper.SNIReadAsync(this._pMarsPhysicalConObj.Handle, ref zero);
                        if (zero != IntPtr.Zero)
                        {
                            SNINativeMethodWrapper.SNIPacketRelease(zero);
                        }
                    }
                    if (0x3e5 != num)
                    {
                        this.Errors.Add(this.ProcessSNIError(this._physicalStateObj));
                        this.ThrowExceptionAndWarning();
                    }
                }
                this._physicalStateObj = this.CreateSession();
            }
        }

        internal void FailureCleanup(TdsParserStateObject stateObj, Exception e)
        {
            int num = stateObj._outputPacketNumber;
            if (Bid.TraceOn)
            {
                Bid.Trace("<sc.TdsParser.FailureCleanup|ERR> Exception caught on ExecuteXXX: '%ls' \n", e.ToString());
            }
            if (stateObj.HasOpenResult)
            {
                stateObj.DecrementOpenResultCount();
            }
            stateObj.ResetBuffer();
            stateObj._outputPacketNumber = 1;
            if ((num != 1) && (this._state == TdsParserState.OpenLoggedIn))
            {
                stateObj.SendAttention();
                this.ProcessAttention(stateObj);
            }
            Bid.Trace("<sc.TdsParser.FailureCleanup|ERR> Exception rethrown. \n");
        }

        private void FireInfoMessageEvent(SqlConnection connection, TdsParserStateObject stateObj, SqlError error)
        {
            string serverVersion = null;
            if (this._state == TdsParserState.OpenLoggedIn)
            {
                serverVersion = this._connHandler.ServerVersion;
            }
            SqlErrorCollection errorCollection = new SqlErrorCollection();
            errorCollection.Add(error);
            SqlException exception = SqlException.CreateException(errorCollection, serverVersion);
            connection.OnInfoMessage(new SqlInfoMessageEventArgs(exception));
        }

        internal int GetAltRowId(TdsParserStateObject stateObj)
        {
            stateObj.ReadByte();
            return stateObj.ReadUInt16();
        }

        internal SNIHandle GetBestEffortCleanupTarget()
        {
            if (this._physicalStateObj != null)
            {
                return this._physicalStateObj.Handle;
            }
            return null;
        }

        internal int GetCodePage(SqlCollation collation, TdsParserStateObject stateObj)
        {
            int aNSICodePage = 0;
            if (collation.sortId != 0)
            {
                return TdsEnums.CODE_PAGE_FROM_SORT_ID[collation.sortId];
            }
            int lCID = collation.LCID;
            bool flag = false;
            try
            {
                aNSICodePage = CultureInfo.GetCultureInfo(lCID).TextInfo.ANSICodePage;
                flag = true;
            }
            catch (ArgumentException exception3)
            {
                ADP.TraceExceptionWithoutRethrow(exception3);
            }
            if (!flag || (aNSICodePage == 0))
            {
                CultureInfo info = null;
                switch (lCID)
                {
                    case 0x10411:
                    case 0x10412:
                    case 0x10404:
                    case 0x11004:
                    case 0x11404:
                    case 0x10804:
                    case 0x10c04:
                        lCID &= 0x3fff;
                        try
                        {
                            info = new CultureInfo(lCID);
                            flag = true;
                        }
                        catch (ArgumentException exception2)
                        {
                            ADP.TraceExceptionWithoutRethrow(exception2);
                        }
                        break;

                    case 0x827:
                        try
                        {
                            info = new CultureInfo(0x427);
                            flag = true;
                        }
                        catch (ArgumentException exception)
                        {
                            ADP.TraceExceptionWithoutRethrow(exception);
                        }
                        break;
                }
                if (!flag)
                {
                    this.ThrowUnsupportedCollationEncountered(stateObj);
                }
                if (info != null)
                {
                    aNSICodePage = info.TextInfo.ANSICodePage;
                }
            }
            return aNSICodePage;
        }

        internal ulong GetDataLength(SqlMetaDataPriv colmeta, TdsParserStateObject stateObj)
        {
            if (this._isYukon && colmeta.metaType.IsPlp)
            {
                return stateObj.ReadPlpLength(true);
            }
            return (ulong) this.GetTokenLength(colmeta.tdsType, stateObj);
        }

        internal byte[] GetDTCAddress(int timeout, TdsParserStateObject stateObj)
        {
            byte[] buffer = null;
            using (SqlDataReader reader = this.TdsExecuteTransactionManagerRequest(null, TdsEnums.TransactionManagerRequestType.GetDTCAddress, null, TdsEnums.TransactionManagerIsolationLevel.Unspecified, timeout, null, stateObj, true))
            {
                if ((reader != null) && reader.Read())
                {
                    long num2 = reader.GetBytes(0, 0L, null, 0, 0);
                    if (num2 <= 0x7fffffffL)
                    {
                        int length = (int) num2;
                        buffer = new byte[length];
                        reader.GetBytes(0, 0L, buffer, 0, length);
                    }
                }
            }
            return buffer;
        }

        internal int GetEncodingCharLength(string value, int numChars, int charOffset, Encoding encoding)
        {
            if ((value == null) || (value == ADP.StrEmpty))
            {
                return 0;
            }
            if (encoding == null)
            {
                if (this._defaultEncoding == null)
                {
                    this.ThrowUnsupportedCollationEncountered(null);
                }
                encoding = this._defaultEncoding;
            }
            char[] chars = value.ToCharArray(charOffset, numChars);
            return encoding.GetByteCount(chars, 0, numChars);
        }

        internal object GetNullSqlValue(SqlBuffer nullVal, SqlMetaDataPriv md)
        {
            switch (md.type)
            {
                case SqlDbType.BigInt:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Int64);
                    return nullVal;

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.VarBinary:
                case SqlDbType.Udt:
                    nullVal.SqlBinary = SqlBinary.Null;
                    return nullVal;

                case SqlDbType.Bit:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Boolean);
                    return nullVal;

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.String);
                    return nullVal;

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.DateTime);
                    return nullVal;

                case SqlDbType.Decimal:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Decimal);
                    return nullVal;

                case SqlDbType.Float:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Double);
                    return nullVal;

                case SqlDbType.Int:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Int32);
                    return nullVal;

                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Money);
                    return nullVal;

                case SqlDbType.Real:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Single);
                    return nullVal;

                case SqlDbType.UniqueIdentifier:
                    nullVal.SqlGuid = SqlGuid.Null;
                    return nullVal;

                case SqlDbType.SmallInt:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Int16);
                    return nullVal;

                case SqlDbType.Timestamp:
                case (SqlDbType.SmallInt | SqlDbType.Int):
                case (SqlDbType.Text | SqlDbType.Int):
                case (SqlDbType.Xml | SqlDbType.Bit):
                case (SqlDbType.TinyInt | SqlDbType.Int):
                case SqlDbType.Structured:
                    return nullVal;

                case SqlDbType.TinyInt:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Byte);
                    return nullVal;

                case SqlDbType.Variant:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Empty);
                    return nullVal;

                case SqlDbType.Xml:
                    nullVal.SqlCachedBuffer = SqlCachedBuffer.Null;
                    return nullVal;

                case SqlDbType.Date:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Date);
                    return nullVal;

                case SqlDbType.Time:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Time);
                    return nullVal;

                case SqlDbType.DateTime2:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.DateTime2);
                    return nullVal;

                case SqlDbType.DateTimeOffset:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.DateTimeOffset);
                    return nullVal;
            }
            return nullVal;
        }

        internal TdsParserStateObject GetSession(object owner)
        {
            TdsParserStateObject session = null;
            if (this.MARSOn)
            {
                session = this._sessionPool.GetSession(owner);
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.TdsParser.GetSession|ADV> %d# getting session %d from pool\n", this.ObjectID, session.ObjectID);
                }
                return session;
            }
            session = this._physicalStateObj;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.TdsParser.GetSession|ADV> %d# getting physical session %d\n", this.ObjectID, session.ObjectID);
            }
            return session;
        }

        internal int GetTokenLength(byte token, TdsParserStateObject stateObj)
        {
            if (this._isYukon)
            {
                if (token == 240)
                {
                    return -1;
                }
                if (token == 0xac)
                {
                    return -1;
                }
                if (token == 0xf1)
                {
                    return stateObj.ReadUInt16();
                }
            }
            switch ((token & 0x30))
            {
                case 0x20:
                case 0:
                    if ((token & 0x80) != 0)
                    {
                        return stateObj.ReadUInt16();
                    }
                    if ((token & 12) == 0)
                    {
                        return stateObj.ReadInt32();
                    }
                    return stateObj.ReadByte();

                case 0x30:
                    return ((((int) 1) << ((token & 12) >> 2)) & 0xff);

                case 0x10:
                    return 0;
            }
            return 0;
        }

        internal int IncrementNonTransactedOpenResultCount()
        {
            return Interlocked.Increment(ref this._nonTransactedOpenResultCount);
        }

        private bool IsBOMNeeded(MetaType type, object value)
        {
            if (type.NullableType == 0xf1)
            {
                Type type2 = value.GetType();
                if (type2 == typeof(SqlString))
                {
                    SqlString str3 = (SqlString) value;
                    if (!str3.IsNull)
                    {
                        SqlString str2 = (SqlString) value;
                        if (str2.Value.Length > 0)
                        {
                            SqlString str = (SqlString) value;
                            if ((str.Value[0] & '\x00ff') != 0xff)
                            {
                                return true;
                            }
                        }
                    }
                }
                else if ((type2 == typeof(string)) && (((string) value).Length > 0))
                {
                    if ((value != null) && ((((string) value)[0] & '\x00ff') != 0xff))
                    {
                        return true;
                    }
                }
                else if (type2 == typeof(SqlXml))
                {
                    if (!((SqlXml) value).IsNull)
                    {
                        return true;
                    }
                }
                else if (type2 == typeof(XmlReader))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsNull(MetaType mt, ulong length)
        {
            if (mt.IsPlp)
            {
                return (ulong.MaxValue == length);
            }
            return (((0xffffL == length) && !mt.IsLong) || (((0L == length) && !mt.IsCharType) && !mt.IsBinType));
        }

        private bool IsVarTimeTds(byte tdsType)
        {
            if ((tdsType != 0x29) && (tdsType != 0x2a))
            {
                return (tdsType == 0x2b);
            }
            return true;
        }

        private void LoadSSPILibrary()
        {
            if (!s_fSSPILoaded)
            {
                lock (s_tdsParserLock)
                {
                    if (!s_fSSPILoaded)
                    {
                        uint maxLength = 0;
                        if (SNINativeMethodWrapper.SNISecInitPackage(ref maxLength) != 0)
                        {
                            this.SSPIError(SQLMessage.SSPIInitializeError(), "InitSSPIPackage");
                        }
                        s_maxSSPILength = maxLength;
                        s_fSSPILoaded = true;
                    }
                }
            }
            if (s_maxSSPILength > 0x7fffffff)
            {
                throw SQL.InvalidSSPIPacketSize();
            }
        }

        internal ulong PlpBytesLeft(TdsParserStateObject stateObj)
        {
            if ((stateObj._longlen != 0L) && (stateObj._longlenleft == 0L))
            {
                stateObj.ReadPlpLength(false);
            }
            return stateObj._longlenleft;
        }

        internal ulong PlpBytesTotalLength(TdsParserStateObject stateObj)
        {
            if (stateObj._longlen == 18446744073709551614L)
            {
                return ulong.MaxValue;
            }
            if (stateObj._longlen == ulong.MaxValue)
            {
                return 0L;
            }
            return stateObj._longlen;
        }

        internal void PrepareResetConnection(bool preserveTransaction)
        {
            this._fResetConnection = true;
            this._fPreserveTransaction = preserveTransaction;
        }

        internal _SqlMetaDataSet ProcessAltMetaData(int cColumns, TdsParserStateObject stateObj)
        {
            _SqlMetaDataSet set = new _SqlMetaDataSet(cColumns);
            int[] numArray = new int[cColumns];
            set.id = stateObj.ReadUInt16();
            for (int i = stateObj.ReadByte(); i > 0; i--)
            {
                this.SkipBytes(2, stateObj);
            }
            for (int j = 0; j < cColumns; j++)
            {
                _SqlMetaData col = set[j];
                col.op = stateObj.ReadByte();
                col.operand = stateObj.ReadUInt16();
                this.CommonProcessMetaData(stateObj, col);
                if (ADP.IsEmpty(col.column))
                {
                    switch (col.op)
                    {
                        case 0x30:
                            col.column = "stdev";
                            break;

                        case 0x31:
                            col.column = "stdevp";
                            break;

                        case 50:
                            col.column = "var";
                            break;

                        case 0x33:
                            col.column = "varp";
                            break;

                        case 9:
                            goto Label_00FA;

                        case 0x4b:
                            col.column = "cnt";
                            break;

                        case 0x4d:
                            col.column = "sum";
                            break;

                        case 0x4f:
                            col.column = "avg";
                            break;

                        case 0x51:
                            col.column = "min";
                            break;

                        case 0x52:
                            col.column = "max";
                            break;

                        case 0x53:
                            col.column = "any";
                            break;

                        case 0x56:
                            col.column = "noop";
                            break;
                    }
                }
                goto Label_017A;
            Label_00FA:
                col.column = "cntb";
            Label_017A:
                numArray[j] = j;
            }
            set.indexMap = numArray;
            set.visibleColumns = cColumns;
            return set;
        }

        private void ProcessAttention(TdsParserStateObject stateObj)
        {
            if ((this._state != TdsParserState.Closed) && (this._state != TdsParserState.Broken))
            {
                lock (this._ErrorCollectionLock)
                {
                    this._attentionErrors = this._errors;
                    this._attentionWarnings = this._warnings;
                    this._errors = null;
                    this._warnings = null;
                    try
                    {
                        this.Run(RunBehavior.Attention, null, null, null, stateObj);
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        ADP.TraceExceptionWithoutRethrow(exception);
                        this._state = TdsParserState.Broken;
                        this._connHandler.BreakConnection();
                        throw;
                    }
                    this._errors = this._attentionErrors;
                    this._warnings = this._attentionWarnings;
                    this._attentionErrors = null;
                    this._attentionWarnings = null;
                }
            }
        }

        private _SqlMetaDataSet ProcessColInfo(_SqlMetaDataSet columns, SqlDataReader reader, TdsParserStateObject stateObj)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                _SqlMetaData data = columns[i];
                stateObj.ReadByte();
                data.tableNum = stateObj.ReadByte();
                byte num = stateObj.ReadByte();
                data.isDifferentName = 0x20 == (num & 0x20);
                data.isExpression = 4 == (num & 4);
                data.isKey = 8 == (num & 8);
                data.isHidden = 0x10 == (num & 0x10);
                if (data.isDifferentName)
                {
                    byte length = stateObj.ReadByte();
                    data.baseColumn = stateObj.ReadString(length);
                }
                if ((reader.TableNames != null) && (data.tableNum > 0))
                {
                    data.multiPartTableName = reader.TableNames[data.tableNum - 1];
                }
                if (data.isExpression)
                {
                    data.updatability = 0;
                }
            }
            return columns;
        }

        internal SqlCollation ProcessCollation(TdsParserStateObject stateObj)
        {
            return new SqlCollation { info = stateObj.ReadUInt32(), sortId = stateObj.ReadByte() };
        }

        internal ulong ProcessColumnHeader(SqlMetaDataPriv col, TdsParserStateObject stateObj, out bool isNull)
        {
            if (col.metaType.IsLong && !col.metaType.IsPlp)
            {
                byte num = stateObj.ReadByte();
                if (num != 0)
                {
                    this.SkipBytes(num, stateObj);
                    this.SkipBytes(8, stateObj);
                    isNull = false;
                    return this.GetDataLength(col, stateObj);
                }
                isNull = true;
                return 0L;
            }
            ulong dataLength = this.GetDataLength(col, stateObj);
            isNull = this.IsNull(col.metaType, dataLength);
            if (!isNull)
            {
                return dataLength;
            }
            return 0L;
        }

        private void ProcessDone(SqlCommand cmd, SqlDataReader reader, ref RunBehavior run, TdsParserStateObject stateObj)
        {
            int num2;
            ushort status = stateObj.ReadUInt16();
            ushort curCmd = stateObj.ReadUInt16();
            if (this._isYukon)
            {
                num2 = (int) stateObj.ReadInt64();
            }
            else
            {
                num2 = stateObj.ReadInt32();
                if (((this._state == TdsParserState.OpenNotLoggedIn) && (stateObj._inBytesRead > stateObj._inBytesUsed)) && (stateObj.PeekByte() == 0))
                {
                    num2 = stateObj.ReadInt32();
                }
            }
            if (0x20 == (status & 0x20))
            {
                stateObj._attentionReceived = true;
            }
            if ((cmd != null) && (0x10 == (status & 0x10)))
            {
                if (curCmd != 0xc1)
                {
                    cmd.InternalRecordsAffected = num2;
                }
                if (stateObj._receivedColMetaData || (curCmd != 0xc1))
                {
                    cmd.OnStatementCompleted(num2);
                }
            }
            stateObj._receivedColMetaData = false;
            if (((2 == (2 & status)) && (this._errors == null)) && (!stateObj._errorTokenReceived && (RunBehavior.Clean != (RunBehavior.Clean & run))))
            {
                this.Errors.Add(new SqlError(0, 0, 11, this._server, SQLMessage.SevereError(), "", 0));
                if ((reader != null) && !reader.IsInitialized)
                {
                    run = RunBehavior.UntilDone;
                }
            }
            if ((0x100 == (0x100 & status)) && (RunBehavior.Clean != (RunBehavior.Clean & run)))
            {
                this.Errors.Add(new SqlError(0, 0, 20, this._server, SQLMessage.SevereError(), "", 0));
                if ((reader != null) && !reader.IsInitialized)
                {
                    run = RunBehavior.UntilDone;
                }
            }
            this.ProcessSqlStatistics(curCmd, status, num2);
            if (1 != (status & 1))
            {
                stateObj._errorTokenReceived = false;
                if (stateObj._inBytesUsed >= stateObj._inBytesRead)
                {
                    stateObj._pendingData = false;
                }
            }
            if (!stateObj._pendingData && stateObj._hasOpenResult)
            {
                stateObj.DecrementOpenResultCount();
            }
        }

        private SqlEnvChange[] ProcessEnvChange(int tokenLength, TdsParserStateObject stateObj)
        {
            int num4 = 0;
            int index = 0;
            SqlEnvChange[] changeArray = new SqlEnvChange[3];
            while (tokenLength > num4)
            {
                int num3;
                ushort num5;
                if (index >= changeArray.Length)
                {
                    SqlEnvChange[] changeArray2 = new SqlEnvChange[changeArray.Length + 3];
                    for (int i = 0; i < changeArray.Length; i++)
                    {
                        changeArray2[i] = changeArray[i];
                    }
                    changeArray = changeArray2;
                }
                SqlEnvChange env = new SqlEnvChange {
                    type = stateObj.ReadByte()
                };
                changeArray[index] = env;
                index++;
                switch (env.type)
                {
                    case 1:
                    case 2:
                        this.ReadTwoStringFields(env, stateObj);
                        goto Label_03E0;

                    case 3:
                        this.ReadTwoStringFields(env, stateObj);
                        if (!(env.newValue == "iso_1"))
                        {
                            break;
                        }
                        this._defaultCodePage = 0x4e4;
                        this._defaultEncoding = Encoding.GetEncoding(this._defaultCodePage);
                        goto Label_03E0;

                    case 4:
                    {
                        this.ReadTwoStringFields(env, stateObj);
                        int size = int.Parse(env.newValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
                        if (this._physicalStateObj.SetPacketSize(size))
                        {
                            this._physicalStateObj._sniPacket.Dispose();
                            uint qInfo = (uint) size;
                            SNINativeMethodWrapper.SNISetInfo(this._physicalStateObj.Handle, SNINativeMethodWrapper.QTypes.SNI_QUERY_CONN_BUFSIZE, ref qInfo);
                            this._physicalStateObj._sniPacket = new SNIPacket(this._physicalStateObj.Handle);
                        }
                        goto Label_03E0;
                    }
                    case 5:
                        this.ReadTwoStringFields(env, stateObj);
                        this._defaultLCID = int.Parse(env.newValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
                        goto Label_03E0;

                    case 6:
                        this.ReadTwoStringFields(env, stateObj);
                        goto Label_03E0;

                    case 7:
                        env.newLength = stateObj.ReadByte();
                        if (env.newLength == 5)
                        {
                            env.newCollation = this.ProcessCollation(stateObj);
                            this._defaultCollation = env.newCollation;
                            int codePage = this.GetCodePage(env.newCollation, stateObj);
                            if (codePage != this._defaultCodePage)
                            {
                                this._defaultCodePage = codePage;
                                this._defaultEncoding = Encoding.GetEncoding(this._defaultCodePage);
                            }
                            this._defaultLCID = env.newCollation.LCID;
                        }
                        env.oldLength = stateObj.ReadByte();
                        if (env.oldLength == 5)
                        {
                            env.oldCollation = this.ProcessCollation(stateObj);
                        }
                        env.length = (3 + env.newLength) + env.oldLength;
                        goto Label_03E0;

                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 0x11:
                        env.newLength = stateObj.ReadByte();
                        if (env.newLength <= 0)
                        {
                            goto Label_02B0;
                        }
                        env.newLongValue = stateObj.ReadInt64();
                        goto Label_02B8;

                    case 13:
                        this.ReadTwoStringFields(env, stateObj);
                        goto Label_03E0;

                    case 15:
                        env.newLength = stateObj.ReadInt32();
                        env.newBinValue = new byte[env.newLength];
                        stateObj.ReadByteArray(env.newBinValue, 0, env.newLength);
                        env.oldLength = stateObj.ReadByte();
                        env.length = 5 + env.newLength;
                        goto Label_03E0;

                    case 0x10:
                    case 0x12:
                        this.ReadTwoBinaryFields(env, stateObj);
                        goto Label_03E0;

                    case 0x13:
                        this.ReadTwoStringFields(env, stateObj);
                        goto Label_03E0;

                    case 20:
                    {
                        env.newLength = stateObj.ReadUInt16();
                        byte protocol = stateObj.ReadByte();
                        ushort port = stateObj.ReadUInt16();
                        ushort length = stateObj.ReadUInt16();
                        string servername = stateObj.ReadString(length);
                        env.newRoutingInfo = new RoutingInfo(protocol, port, servername);
                        num5 = stateObj.ReadUInt16();
                        num3 = 0;
                        goto Label_03C9;
                    }
                    default:
                        goto Label_03E0;
                }
                string s = env.newValue.Substring(2);
                this._defaultCodePage = int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
                this._defaultEncoding = Encoding.GetEncoding(this._defaultCodePage);
                goto Label_03E0;
            Label_02B0:
                env.newLongValue = 0L;
            Label_02B8:
                env.oldLength = stateObj.ReadByte();
                if (env.oldLength > 0)
                {
                    env.oldLongValue = stateObj.ReadInt64();
                }
                else
                {
                    env.oldLongValue = 0L;
                }
                env.length = (3 + env.newLength) + env.oldLength;
                goto Label_03E0;
            Label_03BC:
                stateObj.ReadByte();
                num3++;
            Label_03C9:
                if (num3 < num5)
                {
                    goto Label_03BC;
                }
                env.length = (env.newLength + num5) + 5;
            Label_03E0:
                num4 += env.length;
            }
            return changeArray;
        }

        internal SqlError ProcessError(byte token, TdsParserStateObject stateObj)
        {
            int num2;
            int infoNumber = stateObj.ReadInt32();
            byte errorState = stateObj.ReadByte();
            byte errorClass = stateObj.ReadByte();
            int length = stateObj.ReadUInt16();
            string errorMessage = stateObj.ReadString(length);
            length = stateObj.ReadByte();
            if (length != 0)
            {
                stateObj.ReadString(length);
            }
            length = stateObj.ReadByte();
            string procedure = stateObj.ReadString(length);
            if (this._isYukon)
            {
                num2 = stateObj.ReadInt32();
            }
            else
            {
                num2 = stateObj.ReadUInt16();
                if ((this._state == TdsParserState.OpenNotLoggedIn) && (stateObj.PeekByte() == 0))
                {
                    num2 = (num2 << 0x10) + stateObj.ReadUInt16();
                }
            }
            return new SqlError(infoNumber, errorState, errorClass, this._server, errorMessage, procedure, num2);
        }

        private SqlLoginAck ProcessLoginAck(TdsParserStateObject stateObj)
        {
            SqlLoginAck ack = new SqlLoginAck();
            this.SkipBytes(1, stateObj);
            byte[] buff = new byte[4];
            stateObj.ReadByteArray(buff, 0, buff.Length);
            uint num3 = (uint) ((((((buff[0] << 8) | buff[1]) << 8) | buff[2]) << 8) | buff[3]);
            uint num6 = num3 & 0xff00ffff;
            uint num2 = (num3 >> 0x10) & 0xff;
            switch (num6)
            {
                case 0x72000002:
                    if (num2 != 9)
                    {
                        throw SQL.InvalidTDSVersion();
                    }
                    this._isYukon = true;
                    break;

                case 0x73000003:
                    if (num2 != 10)
                    {
                        throw SQL.InvalidTDSVersion();
                    }
                    this._isKatmai = true;
                    break;

                case 0x7000000:
                    switch (num2)
                    {
                        case 1:
                            this._isShiloh = true;
                            goto Label_00DF;
                    }
                    throw SQL.InvalidTDSVersion();

                case 0x71000001:
                    if (num2 != 0)
                    {
                        throw SQL.InvalidTDSVersion();
                    }
                    this._isShilohSP1 = true;
                    break;

                default:
                    throw SQL.InvalidTDSVersion();
            }
        Label_00DF:
            this._isYukon |= this._isKatmai;
            this._isShilohSP1 |= this._isYukon;
            this._isShiloh |= this._isShilohSP1;
            ack.isVersion8 = this._isShiloh;
            stateObj._outBytesUsed = stateObj._outputHeaderLen;
            byte length = stateObj.ReadByte();
            ack.programName = stateObj.ReadString(length);
            ack.majorVersion = stateObj.ReadByte();
            ack.minorVersion = stateObj.ReadByte();
            ack.buildNum = (short) ((stateObj.ReadByte() << 8) + stateObj.ReadByte());
            this._state = TdsParserState.OpenLoggedIn;
            if ((this._isYukon && this._fAsync) && this._fMARS)
            {
                this._resetConnectionEvent = new AutoResetEvent(true);
            }
            if (this._connHandler.ConnectionOptions.UserInstance && ADP.IsEmpty(this._connHandler.InstanceName))
            {
                this.Errors.Add(new SqlError(0, 0, 20, this.Server, SQLMessage.UserInstanceFailure(), "", 0));
                this.ThrowExceptionAndWarning();
            }
            return ack;
        }

        internal _SqlMetaDataSet ProcessMetaData(int cColumns, TdsParserStateObject stateObj)
        {
            _SqlMetaDataSet set = new _SqlMetaDataSet(cColumns);
            for (int i = 0; i < cColumns; i++)
            {
                this.CommonProcessMetaData(stateObj, set[i]);
            }
            return set;
        }

        private MultiPartTableName ProcessOneTable(TdsParserStateObject stateObj, ref int length)
        {
            ushort num;
            if (this._isShilohSP1)
            {
                MultiPartTableName name = new MultiPartTableName();
                byte num2 = stateObj.ReadByte();
                length--;
                switch (num2)
                {
                    case 4:
                        num = stateObj.ReadUInt16();
                        length -= 2;
                        name.ServerName = stateObj.ReadString(num);
                        num2 = (byte) (num2 - 1);
                        length -= num * 2;
                        break;

                    case 3:
                        num = stateObj.ReadUInt16();
                        length -= 2;
                        name.CatalogName = stateObj.ReadString(num);
                        length -= num * 2;
                        num2 = (byte) (num2 - 1);
                        break;

                    case 2:
                        num = stateObj.ReadUInt16();
                        length -= 2;
                        name.SchemaName = stateObj.ReadString(num);
                        length -= num * 2;
                        num2 = (byte) (num2 - 1);
                        break;

                    case 1:
                        num = stateObj.ReadUInt16();
                        length -= 2;
                        name.TableName = stateObj.ReadString(num);
                        length -= num * 2;
                        num2 = (byte) (num2 - 1);
                        break;
                }
                return name;
            }
            num = stateObj.ReadUInt16();
            length -= 2;
            string str = stateObj.ReadString(num);
            length -= num * 2;
            return new MultiPartTableName(MultipartIdentifier.ParseMultipartIdentifier(str, "[\"", "]\"", "SQL_TDSParserTableName", false));
        }

        internal void ProcessPendingAck(TdsParserStateObject stateObj)
        {
            if (stateObj._attentionSent)
            {
                this.ProcessAttention(stateObj);
            }
        }

        internal SqlReturnValue ProcessReturnValue(int length, TdsParserStateObject stateObj)
        {
            int tokenLength;
            uint num8;
            SqlReturnValue metaData = new SqlReturnValue {
                length = length
            };
            if (this._isYukon)
            {
                metaData.parmIndex = stateObj.ReadUInt16();
            }
            byte num2 = stateObj.ReadByte();
            if (num2 > 0)
            {
                metaData.parameter = stateObj.ReadString(num2);
            }
            stateObj.ReadByte();
            if (this.IsYukonOrNewer)
            {
                num8 = stateObj.ReadUInt32();
            }
            else
            {
                num8 = stateObj.ReadUInt16();
            }
            stateObj.ReadUInt16();
            byte tdsType = stateObj.ReadByte();
            if (tdsType == 0xf1)
            {
                tokenLength = 0xffff;
            }
            else if (this.IsVarTimeTds(tdsType))
            {
                tokenLength = 0;
            }
            else if (tdsType == 40)
            {
                tokenLength = 3;
            }
            else
            {
                tokenLength = this.GetTokenLength(tdsType, stateObj);
            }
            metaData.metaType = MetaType.GetSqlDataType(tdsType, num8, tokenLength);
            metaData.type = metaData.metaType.SqlDbType;
            if (this._isShiloh)
            {
                metaData.tdsType = metaData.metaType.NullableType;
                metaData.isNullable = true;
                if (tokenLength == 0xffff)
                {
                    metaData.metaType = MetaType.GetMaxMetaTypeFromMetaType(metaData.metaType);
                }
            }
            else
            {
                if (metaData.metaType.NullableType == tdsType)
                {
                    metaData.isNullable = true;
                }
                metaData.tdsType = tdsType;
            }
            if (metaData.type == SqlDbType.Decimal)
            {
                metaData.precision = stateObj.ReadByte();
                metaData.scale = stateObj.ReadByte();
            }
            if (metaData.metaType.IsVarTime)
            {
                metaData.scale = stateObj.ReadByte();
            }
            if (tdsType == 240)
            {
                this.ProcessUDTMetaData(metaData, stateObj);
            }
            if (metaData.type == SqlDbType.Xml)
            {
                if ((stateObj.ReadByte() & 1) != 0)
                {
                    num2 = stateObj.ReadByte();
                    if (num2 != 0)
                    {
                        metaData.xmlSchemaCollectionDatabase = stateObj.ReadString(num2);
                    }
                    num2 = stateObj.ReadByte();
                    if (num2 != 0)
                    {
                        metaData.xmlSchemaCollectionOwningSchema = stateObj.ReadString(num2);
                    }
                    short num7 = stateObj.ReadInt16();
                    if (num7 != 0)
                    {
                        metaData.xmlSchemaCollectionName = stateObj.ReadString(num7);
                    }
                }
            }
            else if (this._isShiloh && metaData.metaType.IsCharType)
            {
                metaData.collation = this.ProcessCollation(stateObj);
                int codePage = this.GetCodePage(metaData.collation, stateObj);
                if (codePage == this._defaultCodePage)
                {
                    metaData.codePage = this._defaultCodePage;
                    metaData.encoding = this._defaultEncoding;
                }
                else
                {
                    metaData.codePage = codePage;
                    metaData.encoding = Encoding.GetEncoding(metaData.codePage);
                }
            }
            bool isNull = false;
            ulong num5 = this.ProcessColumnHeader(metaData, stateObj, out isNull);
            int num4 = (num5 > 0x7fffffffL) ? 0x7fffffff : ((int) num5);
            if (metaData.metaType.IsPlp)
            {
                num4 = 0x7fffffff;
            }
            if (isNull)
            {
                this.GetNullSqlValue(metaData.value, metaData);
                return metaData;
            }
            this.ReadSqlValue(metaData.value, metaData, num4, stateObj);
            return metaData;
        }

        private void ProcessRow(_SqlMetaDataSet columns, object[] buffer, int[] map, TdsParserStateObject stateObj)
        {
            SqlBuffer nullVal = new SqlBuffer();
            for (int i = 0; i < columns.Length; i++)
            {
                bool flag;
                _SqlMetaData col = columns[i];
                ulong num2 = this.ProcessColumnHeader(col, stateObj, out flag);
                if (flag)
                {
                    this.GetNullSqlValue(nullVal, col);
                    buffer[map[i]] = nullVal.SqlValue;
                }
                else
                {
                    this.ReadSqlValue(nullVal, col, col.metaType.IsPlp ? 0x7fffffff : ((int) num2), stateObj);
                    buffer[map[i]] = nullVal.SqlValue;
                    if (stateObj._longlen != 0L)
                    {
                        throw new SqlTruncateException(System.Data.Res.GetString("SqlMisc_TruncationMaxDataMessage"));
                    }
                }
                nullVal.Clear();
            }
        }

        internal SqlError ProcessSNIError(TdsParserStateObject stateObj)
        {
            string sNIErrorMessage;
            SNINativeMethodWrapper.SNI_Error error = new SNINativeMethodWrapper.SNI_Error();
            SNINativeMethodWrapper.SNIGetLastError(error);
            switch (error.sniError)
            {
                case 0x2f:
                    throw SQL.MultiSubnetFailoverWithMoreThan64IPs();

                case 0x30:
                    throw SQL.MultiSubnetFailoverWithInstanceSpecified();

                case 0x31:
                    throw SQL.MultiSubnetFailoverWithNonTcpProtocol();
            }
            int index = Array.IndexOf<char>(error.errorMessage, '\0');
            if (index == -1)
            {
                sNIErrorMessage = string.Empty;
            }
            else
            {
                sNIErrorMessage = new string(error.errorMessage, 0, index);
            }
            string str4 = System.Data.Res.GetString(Enum.GetName(typeof(SniContext), stateObj.SniContext));
            string str2 = System.Data.Res.GetString(string.Format(null, "SNI_PN{0}", new object[] { (int) error.provider }));
            if (error.sniError == 0)
            {
                int startIndex = sNIErrorMessage.IndexOf(':');
                if (0 <= startIndex)
                {
                    int length = sNIErrorMessage.Length - 2;
                    startIndex += 2;
                    length -= startIndex;
                    if (length > 0)
                    {
                        sNIErrorMessage = sNIErrorMessage.Substring(startIndex, length);
                    }
                }
            }
            else
            {
                sNIErrorMessage = SQL.GetSNIErrorMessage((int) error.sniError);
                if (error.sniError == SNINativeMethodWrapper.SNI_LocalDBErrorCode)
                {
                    sNIErrorMessage = sNIErrorMessage + LocalDBAPI.GetLocalDBMessage((int) error.nativeError);
                }
            }
            return new SqlError((int) error.nativeError, 0, 20, this._server, string.Format(null, "{0} (provider: {1}, error: {2} - {3})", new object[] { str4, str2, (int) error.sniError, sNIErrorMessage }), error.function, (int) error.lineNumber);
        }

        private void ProcessSqlStatistics(ushort curCmd, ushort status, int count)
        {
            if (this._statistics != null)
            {
                if (this._statistics.WaitForDoneAfterRow)
                {
                    this._statistics.SafeIncrement(ref this._statistics._sumResultSets);
                    this._statistics.WaitForDoneAfterRow = false;
                }
                if (0x10 != (status & 0x10))
                {
                    count = 0;
                }
                switch (curCmd)
                {
                    case 210:
                        this._statisticsIsInTransaction = false;
                        return;

                    case 0xd3:
                    case 0xc2:
                        return;

                    case 0xd4:
                        if (!this._statisticsIsInTransaction)
                        {
                            this._statistics.SafeIncrement(ref this._statistics._transactions);
                        }
                        this._statisticsIsInTransaction = true;
                        return;

                    case 0xd5:
                        this._statisticsIsInTransaction = false;
                        return;

                    case 0x117:
                    case 0xc3:
                    case 0xc4:
                    case 0xc5:
                        this._statistics.SafeIncrement(ref this._statistics._iduCount);
                        this._statistics.SafeAdd(ref this._statistics._iduRows, (long) count);
                        if (!this._statisticsIsInTransaction)
                        {
                            this._statistics.SafeIncrement(ref this._statistics._transactions);
                        }
                        return;

                    case 0xc1:
                        this._statistics.SafeIncrement(ref this._statistics._selectCount);
                        this._statistics.SafeAdd(ref this._statistics._selectRows, (long) count);
                        return;

                    case 0x20:
                        this._statistics.SafeIncrement(ref this._statistics._cursorOpens);
                        return;
                }
            }
            else
            {
                switch (curCmd)
                {
                    case 210:
                    case 0xd5:
                        this._statisticsIsInTransaction = false;
                        break;

                    case 0xd3:
                        break;

                    case 0xd4:
                        this._statisticsIsInTransaction = true;
                        return;

                    default:
                        return;
                }
            }
        }

        private void ProcessSSPI(int receivedLength)
        {
            SniContext sniContext = this._physicalStateObj.SniContext;
            this._physicalStateObj.SniContext = SniContext.Snix_ProcessSspi;
            byte[] buff = new byte[receivedLength];
            this._physicalStateObj.ReadByteArray(buff, 0, receivedLength);
            byte[] sendBuff = new byte[s_maxSSPILength];
            uint sendLength = s_maxSSPILength;
            this.SSPIData(buff, (uint) receivedLength, sendBuff, ref sendLength);
            this.WriteByteArray(sendBuff, (int) sendLength, 0, this._physicalStateObj);
            this._physicalStateObj._outputMessageType = 0x11;
            this._physicalStateObj.WritePacket(1);
            this._physicalStateObj.SniContext = sniContext;
        }

        internal MultiPartTableName[] ProcessTableName(int length, TdsParserStateObject stateObj)
        {
            int index = 0;
            MultiPartTableName[] sourceArray = new MultiPartTableName[1];
            while (length > 0)
            {
                MultiPartTableName name = this.ProcessOneTable(stateObj, ref length);
                if (index == 0)
                {
                    sourceArray[index] = name;
                }
                else
                {
                    MultiPartTableName[] destinationArray = new MultiPartTableName[sourceArray.Length + 1];
                    Array.Copy(sourceArray, 0, destinationArray, 0, sourceArray.Length);
                    destinationArray[sourceArray.Length] = name;
                    sourceArray = destinationArray;
                }
                index++;
            }
            return sourceArray;
        }

        private void ProcessUDTMetaData(SqlMetaDataPriv metaData, TdsParserStateObject stateObj)
        {
            metaData.length = stateObj.ReadUInt16();
            int length = stateObj.ReadByte();
            if (length != 0)
            {
                metaData.udtDatabaseName = stateObj.ReadString(length);
            }
            length = stateObj.ReadByte();
            if (length != 0)
            {
                metaData.udtSchemaName = stateObj.ReadString(length);
            }
            length = stateObj.ReadByte();
            if (length != 0)
            {
                metaData.udtTypeName = stateObj.ReadString(length);
            }
            length = stateObj.ReadUInt16();
            if (length != 0)
            {
                metaData.udtAssemblyQualifiedName = stateObj.ReadString(length);
            }
        }

        internal void PropagateDistributedTransaction(byte[] buffer, int timeout, TdsParserStateObject stateObj)
        {
            this.TdsExecuteTransactionManagerRequest(buffer, TdsEnums.TransactionManagerRequestType.Propagate, null, TdsEnums.TransactionManagerIsolationLevel.Unspecified, timeout, null, stateObj, true);
        }

        internal void PutSession(TdsParserStateObject session)
        {
            if (this.MARSOn)
            {
                this._sessionPool.PutSession(session);
            }
        }

        private int[] ReadDecimalBits(int length, TdsParserStateObject stateObj)
        {
            int num;
            int[] numArray = stateObj._decimalBits;
            if (numArray == null)
            {
                numArray = new int[4];
            }
            else
            {
                for (num = 0; num < numArray.Length; num++)
                {
                    numArray[num] = 0;
                }
            }
            int num2 = length >> 2;
            for (num = 0; num < num2; num++)
            {
                numArray[num] = stateObj.ReadInt32();
            }
            return numArray;
        }

        internal int ReadPlpAnsiChars(ref char[] buff, int offst, int len, SqlMetaDataPriv metadata, TdsParserStateObject stateObj)
        {
            int num3 = 0;
            int num2 = 0;
            int num = 0;
            int num4 = 0;
            if (stateObj._longlen == 0L)
            {
                return 0;
            }
            num2 = len;
            if (stateObj._longlenleft == 0L)
            {
                stateObj.ReadPlpLength(false);
                if (stateObj._longlenleft == 0L)
                {
                    return 0;
                }
            }
            Encoding encoding = metadata.encoding;
            if (encoding == null)
            {
                if (this._defaultEncoding == null)
                {
                    this.ThrowUnsupportedCollationEncountered(stateObj);
                }
                encoding = this._defaultEncoding;
            }
            while (num2 > 0)
            {
                num = (int) Math.Min(stateObj._longlenleft, (ulong) num2);
                if ((stateObj._bTmp == null) || (stateObj._bTmp.Length < num))
                {
                    stateObj._bTmp = new byte[num];
                }
                num = stateObj.ReadPlpBytesChunk(stateObj._bTmp, 0, num);
                num3 = encoding.GetChars(stateObj._bTmp, 0, num, buff, offst);
                num2 -= num3;
                offst += num3;
                num4 += num3;
                if (stateObj._longlenleft == 0L)
                {
                    stateObj.ReadPlpLength(false);
                }
                if (stateObj._longlenleft == 0L)
                {
                    return num4;
                }
            }
            return num4;
        }

        internal int ReadPlpUnicodeChars(ref char[] buff, int offst, int len, TdsParserStateObject stateObj)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            if (stateObj._longlen != 0L)
            {
                num2 = len;
                if ((buff == null) && (stateObj._longlen != 18446744073709551614L))
                {
                    buff = new char[Math.Min((int) stateObj._longlen, len)];
                }
                if (stateObj._longlenleft != 0L)
                {
                    goto Label_0150;
                }
                stateObj.ReadPlpLength(false);
                if (stateObj._longlenleft != 0L)
                {
                    goto Label_0150;
                }
            }
            return 0;
        Label_0150:
            while (num2 > 0)
            {
                num = (int) Math.Min((stateObj._longlenleft + 1L) >> 1, (ulong) num2);
                if ((buff == null) || (buff.Length < (offst + num)))
                {
                    char[] dst = new char[offst + num];
                    if (buff != null)
                    {
                        Buffer.BlockCopy(buff, 0, dst, 0, offst * 2);
                    }
                    buff = dst;
                }
                if (num > 0)
                {
                    num = this.ReadPlpUnicodeCharsChunk(buff, offst, num, stateObj);
                    num2 -= num;
                    offst += num;
                    num3 += num;
                }
                if ((stateObj._longlenleft == 1L) && (num2 > 0))
                {
                    byte num5 = stateObj.ReadByte();
                    stateObj._longlenleft -= (ulong) 1L;
                    stateObj.ReadPlpLength(false);
                    byte num4 = stateObj.ReadByte();
                    stateObj._longlenleft -= (ulong) 1L;
                    buff[offst] = (char) (((num4 & 0xff) << 8) + (num5 & 0xff));
                    offst++;
                    num++;
                    num2--;
                    num3++;
                }
                if (stateObj._longlenleft == 0L)
                {
                    stateObj.ReadPlpLength(false);
                }
                if (stateObj._longlenleft == 0L)
                {
                    return num3;
                }
            }
            return num3;
        }

        private int ReadPlpUnicodeCharsChunk(char[] buff, int offst, int len, TdsParserStateObject stateObj)
        {
            if (stateObj._longlenleft == 0L)
            {
                return 0;
            }
            int num2 = len;
            if ((stateObj._longlenleft >> 1) < len)
            {
                num2 = (int) (stateObj._longlenleft >> 1);
            }
            for (int i = 0; i < num2; i++)
            {
                buff[offst + i] = stateObj.ReadChar();
            }
            stateObj._longlenleft -= ((ulong) num2) << 1;
            return num2;
        }

        private void ReadSqlDateTime(SqlBuffer value, byte tdsType, int length, byte scale, TdsParserStateObject stateObj)
        {
            stateObj.ReadByteArray(this.datetimeBuffer, 0, length);
            switch (tdsType)
            {
                case 40:
                    value.SetToDate(this.datetimeBuffer);
                    return;

                case 0x29:
                    value.SetToTime(this.datetimeBuffer, length, scale);
                    return;

                case 0x2a:
                    value.SetToDateTime2(this.datetimeBuffer, length, scale);
                    return;

                case 0x2b:
                    value.SetToDateTimeOffset(this.datetimeBuffer, length, scale);
                    return;
            }
        }

        private void ReadSqlDecimal(SqlBuffer value, int length, byte precision, byte scale, TdsParserStateObject stateObj)
        {
            bool positive = 1 == stateObj.ReadByte();
            length--;
            int[] bits = this.ReadDecimalBits(length, stateObj);
            value.SetToDecimal(precision, scale, positive, bits);
        }

        private void ReadSqlStringValue(SqlBuffer value, byte type, int length, Encoding encoding, bool isPlp, TdsParserStateObject stateObj)
        {
            string strEmpty;
            byte num = type;
            if (num <= 0x63)
            {
                if (num > 0x27)
                {
                    switch (num)
                    {
                        case 0x2f:
                            goto Label_004C;

                        case 0x63:
                            goto Label_006B;
                    }
                    return;
                }
                if ((num != 0x23) && (num != 0x27))
                {
                    return;
                }
            }
            else if (num <= 0xaf)
            {
                if ((num != 0xa7) && (num != 0xaf))
                {
                    return;
                }
            }
            else
            {
                switch (num)
                {
                    case 0xe7:
                    case 0xef:
                        goto Label_006B;
                }
                return;
            }
        Label_004C:
            if (encoding == null)
            {
                encoding = this._defaultEncoding;
            }
            value.SetToString(stateObj.ReadStringWithEncoding(length, encoding, isPlp));
            return;
        Label_006B:
            strEmpty = null;
            if (isPlp)
            {
                char[] buff = null;
                length = this.ReadPlpUnicodeChars(ref buff, 0, length >> 1, stateObj);
                if (length > 0)
                {
                    strEmpty = new string(buff, 0, length);
                }
                else
                {
                    strEmpty = ADP.StrEmpty;
                }
            }
            else
            {
                strEmpty = stateObj.ReadString(length >> 1);
            }
            value.SetToString(strEmpty);
        }

        internal void ReadSqlValue(SqlBuffer value, SqlMetaDataPriv md, int length, TdsParserStateObject stateObj)
        {
            if (md.metaType.IsPlp)
            {
                length = 0x7fffffff;
            }
            switch (md.tdsType)
            {
                case 0x22:
                case 0x25:
                case 0x2d:
                case 240:
                case 0xa5:
                case 0xad:
                {
                    byte[] buff = null;
                    if (md.metaType.IsPlp)
                    {
                        stateObj.ReadPlpBytes(ref buff, 0, length);
                    }
                    else
                    {
                        buff = new byte[length];
                        stateObj.ReadByteArray(buff, 0, length);
                    }
                    value.SqlBinary = new SqlBinary(buff, true);
                    return;
                }
                case 0x23:
                case 0x27:
                case 0x2f:
                case 0x63:
                case 0xef:
                case 0xe7:
                case 0xa7:
                case 0xaf:
                    this.ReadSqlStringValue(value, md.tdsType, length, md.encoding, md.metaType.IsPlp, stateObj);
                    return;

                case 40:
                case 0x29:
                case 0x2a:
                case 0x2b:
                    this.ReadSqlDateTime(value, md.tdsType, length, md.scale, stateObj);
                    return;

                case 0x6a:
                case 0x6c:
                    this.ReadSqlDecimal(value, length, md.precision, md.scale, stateObj);
                    return;

                case 0xf1:
                {
                    SqlCachedBuffer buffer2 = new SqlCachedBuffer(md, this, stateObj);
                    value.SqlCachedBuffer = buffer2;
                    return;
                }
            }
            this.ReadSqlValueInternal(value, md.tdsType, md.metaType.TypeId, length, stateObj);
        }

        internal void ReadSqlValueInternal(SqlBuffer value, byte tdsType, int typeId, int length, TdsParserStateObject stateObj)
        {
            int num4;
            switch (tdsType)
            {
                case 0x7f:
                    goto Label_011A;

                case 0xa5:
                case 0xad:
                case 0x22:
                case 0x25:
                case 0x2d:
                {
                    byte[] buff = new byte[length];
                    stateObj.ReadByteArray(buff, 0, length);
                    value.SqlBinary = new SqlBinary(buff, true);
                    return;
                }
                case 0x6d:
                    if (length != 4)
                    {
                        goto Label_013B;
                    }
                    goto Label_012D;

                case 110:
                    if (length == 4)
                    {
                        goto Label_0173;
                    }
                    goto Label_014E;

                case 0x6f:
                    if (length != 4)
                    {
                        goto Label_01A2;
                    }
                    goto Label_0187;

                case 0x7a:
                    goto Label_0173;

                case 0x62:
                    this.ReadSqlVariant(value, length, stateObj);
                    return;

                case 0x68:
                case 50:
                    value.Boolean = stateObj.ReadByte() != 0;
                    return;

                case 0x23:
                case 0x2e:
                case 0x2f:
                case 0x31:
                case 0x33:
                case 0x35:
                case 0x36:
                case 0x37:
                case 0x39:
                    return;

                case 0x24:
                {
                    byte[] buffer2 = new byte[length];
                    stateObj.ReadByteArray(buffer2, 0, length);
                    value.SqlGuid = new SqlGuid(buffer2, true);
                    return;
                }
                case 0x26:
                    if (length == 1)
                    {
                        break;
                    }
                    if (length == 2)
                    {
                        goto Label_00FE;
                    }
                    if (length != 4)
                    {
                        goto Label_011A;
                    }
                    goto Label_010C;

                case 0x30:
                    break;

                case 0x34:
                    goto Label_00FE;

                case 0x38:
                    goto Label_010C;

                case 0x3a:
                    goto Label_0187;

                case 0x3b:
                    goto Label_012D;

                case 60:
                    goto Label_014E;

                case 0x3d:
                    goto Label_01A2;

                case 0x3e:
                    goto Label_013B;

                default:
                    return;
            }
            value.Byte = stateObj.ReadByte();
            return;
        Label_00FE:
            value.Int16 = stateObj.ReadInt16();
            return;
        Label_010C:
            value.Int32 = stateObj.ReadInt32();
            return;
        Label_011A:
            value.Int64 = stateObj.ReadInt64();
            return;
        Label_012D:
            value.Single = stateObj.ReadSingle();
            return;
        Label_013B:
            value.Double = stateObj.ReadDouble();
            return;
        Label_014E:
            num4 = stateObj.ReadInt32();
            uint num3 = stateObj.ReadUInt32();
            long num2 = (num4 << 0x20) + num3;
            value.SetToMoney(num2);
            return;
        Label_0173:
            value.SetToMoney((long) stateObj.ReadInt32());
            return;
        Label_0187:
            value.SetToDateTime(stateObj.ReadUInt16(), stateObj.ReadUInt16() * SqlDateTime.SQLTicksPerMinute);
            return;
        Label_01A2:
            value.SetToDateTime(stateObj.ReadInt32(), (int) stateObj.ReadUInt32());
        }

        internal void ReadSqlVariant(SqlBuffer value, int lenTotal, TdsParserStateObject stateObj)
        {
            byte tdsType = stateObj.ReadByte();
            byte num2 = stateObj.ReadByte();
            byte propBytes = MetaType.GetSqlDataType(tdsType, 0, 0).PropBytes;
            int num9 = 2 + num2;
            int length = lenTotal - num9;
            switch (tdsType)
            {
                case 0xe7:
                case 0xef:
                case 0xa7:
                case 0xaf:
                    this.ProcessCollation(stateObj);
                    stateObj.ReadUInt16();
                    if (num2 > propBytes)
                    {
                        this.SkipBytes(num2 - propBytes, stateObj);
                    }
                    this.ReadSqlStringValue(value, tdsType, length, null, false, stateObj);
                    return;

                case 0xa5:
                case 0xad:
                    stateObj.ReadUInt16();
                    if (num2 > propBytes)
                    {
                        this.SkipBytes(num2 - propBytes, stateObj);
                    }
                    break;

                case 0xa6:
                case 0xae:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x2c:
                case 0x2d:
                case 0x2e:
                case 0x2f:
                case 0x31:
                case 0x33:
                case 0x35:
                case 0x36:
                case 0x37:
                case 0x39:
                case 0x6b:
                    return;

                case 0x7a:
                case 0x7f:
                case 0x24:
                case 0x30:
                case 50:
                case 0x34:
                case 0x38:
                case 0x3a:
                case 0x3b:
                case 60:
                case 0x3d:
                case 0x3e:
                    break;

                case 40:
                    this.ReadSqlDateTime(value, tdsType, length, 0, stateObj);
                    return;

                case 0x29:
                case 0x2a:
                case 0x2b:
                {
                    byte scale = stateObj.ReadByte();
                    if (num2 > propBytes)
                    {
                        this.SkipBytes(num2 - propBytes, stateObj);
                    }
                    this.ReadSqlDateTime(value, tdsType, length, scale, stateObj);
                    return;
                }
                case 0x6a:
                case 0x6c:
                {
                    byte precision = stateObj.ReadByte();
                    byte num7 = stateObj.ReadByte();
                    if (num2 > propBytes)
                    {
                        this.SkipBytes(num2 - propBytes, stateObj);
                    }
                    this.ReadSqlDecimal(value, 0x11, precision, num7, stateObj);
                    return;
                }
                default:
                    return;
            }
            this.ReadSqlValueInternal(value, tdsType, 0, length, stateObj);
        }

        private void ReadTwoBinaryFields(SqlEnvChange env, TdsParserStateObject stateObj)
        {
            env.newLength = stateObj.ReadByte();
            env.newBinValue = new byte[env.newLength];
            stateObj.ReadByteArray(env.newBinValue, 0, env.newLength);
            env.oldLength = stateObj.ReadByte();
            env.oldBinValue = new byte[env.oldLength];
            stateObj.ReadByteArray(env.oldBinValue, 0, env.oldLength);
            env.length = (3 + env.newLength) + env.oldLength;
        }

        private void ReadTwoStringFields(SqlEnvChange env, TdsParserStateObject stateObj)
        {
            env.newLength = stateObj.ReadByte();
            env.newValue = stateObj.ReadString(env.newLength);
            env.oldLength = stateObj.ReadByte();
            env.oldValue = stateObj.ReadString(env.oldLength);
            env.length = (3 + (env.newLength * 2)) + (env.oldLength * 2);
        }

        internal void RemoveEncryption()
        {
            if (SNINativeMethodWrapper.SNIRemoveProvider(this._physicalStateObj.Handle, SNINativeMethodWrapper.ProviderEnum.SSL_PROV) != 0)
            {
                this.Errors.Add(this.ProcessSNIError(this._physicalStateObj));
                this.ThrowExceptionAndWarning();
            }
            try
            {
            }
            finally
            {
                this._physicalStateObj._sniPacket.Dispose();
                this._physicalStateObj._sniPacket = new SNIPacket(this._physicalStateObj.Handle);
            }
        }

        internal void RollbackOrphanedAPITransactions()
        {
            SqlInternalTransaction currentTransaction = this.CurrentTransaction;
            if (((currentTransaction != null) && currentTransaction.HasParentTransaction) && currentTransaction.IsOrphaned)
            {
                currentTransaction.CloseFromConnection();
            }
        }

        internal bool Run(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj)
        {
            if ((TdsParserState.Broken == this.State) || (this.State == TdsParserState.Closed))
            {
                return true;
            }
            bool flag = false;
        Label_0016:
            if (stateObj._internalTimeout)
            {
                runBehavior = RunBehavior.Attention;
            }
            if ((TdsParserState.Broken == this.State) || (this.State == TdsParserState.Closed))
            {
                goto Label_0705;
            }
            byte token = stateObj.ReadByte();
            if ((((((token != 170) && (token != 0xab)) && ((token != 0xad) && (token != 0xe3))) && (((token != 0xac) && (token != 0x79)) && ((token != 160) && (token != 0xa1)))) && ((((token != 0x81) && (token != 0x88)) && ((token != 0xa4) && (token != 0xa5))) && (((token != 0xa9) && (token != 0xd3)) && ((token != 0xd1) && (token != 0xfd))))) && ((((token != 0xfe) && (token != 0xff)) && ((token != 0x39) && (token != 0xed))) && (((token != 0xae) && (token != 0x7c)) && ((token != 120) && (token != 0xed)))))
            {
                this._state = TdsParserState.Broken;
                this._connHandler.BreakConnection();
                Bid.Trace("<sc.TdsParser.Run|ERR> Potential multi-threaded misuse of connection, unexpected TDS token found %d#\n", this.ObjectID);
                throw SQL.ParsingError();
            }
            int tokenLength = this.GetTokenLength(token, stateObj);
            switch (token)
            {
                case 0xa4:
                    if (dataStream == null)
                    {
                        this.SkipBytes(tokenLength, stateObj);
                    }
                    else
                    {
                        dataStream.TableNames = this.ProcessTableName(tokenLength, stateObj);
                    }
                    goto Label_06D5;

                case 0xa5:
                    if (dataStream == null)
                    {
                        this.SkipBytes(tokenLength, stateObj);
                    }
                    else
                    {
                        _SqlMetaDataSet metaData = this.ProcessColInfo(dataStream.MetaData, dataStream, stateObj);
                        dataStream.SetMetaData(metaData, false);
                        dataStream.BrowseModeInfoConsumed = true;
                    }
                    goto Label_06D5;

                case 0xa9:
                    this.SkipBytes(tokenLength, stateObj);
                    goto Label_06D5;

                case 170:
                case 0xab:
                {
                    if (token == 170)
                    {
                        stateObj._errorTokenReceived = true;
                    }
                    SqlError error = this.ProcessError(token, stateObj);
                    if (RunBehavior.Clean == (RunBehavior.Clean & runBehavior))
                    {
                        if (error.Class >= 20)
                        {
                            this.Errors.Add(error);
                        }
                    }
                    else
                    {
                        SqlConnection connection = null;
                        if (this._connHandler != null)
                        {
                            connection = this._connHandler.Connection;
                        }
                        if (((connection != null) && connection.FireInfoMessageEventOnUserErrors) && (error.Class <= 0x10))
                        {
                            this.FireInfoMessageEvent(connection, stateObj, error);
                        }
                        else if (error.Class < 11)
                        {
                            this.Warnings.Add(error);
                        }
                        else if (error.Class < 20)
                        {
                            this.Errors.Add(error);
                            if ((dataStream != null) && !dataStream.IsInitialized)
                            {
                                runBehavior = RunBehavior.UntilDone;
                            }
                        }
                        else
                        {
                            this.Errors.Add(error);
                            runBehavior = RunBehavior.UntilDone;
                        }
                    }
                    goto Label_06D5;
                }
                case 0xac:
                {
                    SqlReturnValue rec = this.ProcessReturnValue(tokenLength, stateObj);
                    if (cmdHandler != null)
                    {
                        cmdHandler.OnReturnValue(rec);
                    }
                    goto Label_06D5;
                }
                case 0xad:
                {
                    SqlLoginAck ack = this.ProcessLoginAck(stateObj);
                    this._connHandler.OnLoginAck(ack);
                    goto Label_06D5;
                }
                case 0x88:
                {
                    if (stateObj._cleanupAltMetaDataSetArray == null)
                    {
                        stateObj._cleanupAltMetaDataSetArray = new _SqlMetaDataSetCollection();
                    }
                    _SqlMetaDataSet altMetaDataSet = this.ProcessAltMetaData(tokenLength, stateObj);
                    stateObj._cleanupAltMetaDataSetArray.SetAltMetaData(altMetaDataSet);
                    if (dataStream != null)
                    {
                        dataStream.SetAltMetaDataSet(altMetaDataSet, 0x88 != stateObj.PeekByte());
                    }
                    goto Label_06D5;
                }
                case 0x79:
                {
                    int status = stateObj.ReadInt32();
                    if (cmdHandler != null)
                    {
                        cmdHandler.OnReturnStatus(status);
                    }
                    goto Label_06D5;
                }
                case 0x81:
                    if (tokenLength != 0xffff)
                    {
                        stateObj._cleanupMetaData = this.ProcessMetaData(tokenLength, stateObj);
                    }
                    else if (cmdHandler != null)
                    {
                        stateObj._cleanupMetaData = cmdHandler.MetaData;
                    }
                    if (dataStream != null)
                    {
                        byte num5 = stateObj.PeekByte();
                        dataStream.SetMetaData(stateObj._cleanupMetaData, (0xa4 == num5) || (0xa5 == num5));
                    }
                    else if (bulkCopyHandler != null)
                    {
                        bulkCopyHandler.SetMetaData(stateObj._cleanupMetaData);
                    }
                    goto Label_06D5;

                case 0xd1:
                    if (bulkCopyHandler == null)
                    {
                        if (RunBehavior.ReturnImmediately != (RunBehavior.ReturnImmediately & runBehavior))
                        {
                            this.SkipRow(stateObj._cleanupMetaData, stateObj);
                        }
                        break;
                    }
                    this.ProcessRow(stateObj._cleanupMetaData, bulkCopyHandler.CreateRowBuffer(), bulkCopyHandler.CreateIndexMap(), stateObj);
                    break;

                case 0xd3:
                    if (RunBehavior.ReturnImmediately != (RunBehavior.ReturnImmediately & runBehavior))
                    {
                        int id = stateObj.ReadUInt16();
                        this.SkipRow(stateObj._cleanupAltMetaDataSetArray.GetAltMetaData(id), stateObj);
                    }
                    flag = true;
                    goto Label_06D5;

                case 0xe3:
                {
                    SqlEnvChange[] changeArray = this.ProcessEnvChange(tokenLength, stateObj);
                    for (int i = 0; i < changeArray.Length; i++)
                    {
                        if ((changeArray[i] == null) || this.Connection.IgnoreEnvChange)
                        {
                            continue;
                        }
                        switch (changeArray[i].type)
                        {
                            case 8:
                            case 11:
                                this._currentTransaction = this._pendingTransaction;
                                this._pendingTransaction = null;
                                if (this._currentTransaction == null)
                                {
                                    break;
                                }
                                this._currentTransaction.TransactionId = changeArray[i].newLongValue;
                                goto Label_04B4;

                            case 9:
                            case 12:
                            case 0x11:
                                this._retainedTransactionId = 0L;
                                goto Label_04F7;

                            case 10:
                                goto Label_04F7;

                            default:
                                goto Label_0577;
                        }
                        TransactionType type = (8 == changeArray[i].type) ? TransactionType.LocalFromTSQL : TransactionType.Distributed;
                        this._currentTransaction = new SqlInternalTransaction(this._connHandler, type, null, changeArray[i].newLongValue);
                    Label_04B4:
                        if ((this._statistics != null) && !this._statisticsIsInTransaction)
                        {
                            this._statistics.SafeIncrement(ref this._statistics._transactions);
                        }
                        this._statisticsIsInTransaction = true;
                        this._retainedTransactionId = 0L;
                        continue;
                    Label_04F7:
                        if (this._currentTransaction != null)
                        {
                            if (9 == changeArray[i].type)
                            {
                                this._currentTransaction.Completed(TransactionState.Committed);
                            }
                            else if (10 == changeArray[i].type)
                            {
                                if (this._currentTransaction.IsDistributed && this._currentTransaction.IsActive)
                                {
                                    this._retainedTransactionId = changeArray[i].oldLongValue;
                                }
                                this._currentTransaction.Completed(TransactionState.Aborted);
                            }
                            else
                            {
                                this._currentTransaction.Completed(TransactionState.Unknown);
                            }
                            this._currentTransaction = null;
                        }
                        this._statisticsIsInTransaction = false;
                        continue;
                    Label_0577:
                        this._connHandler.OnEnvChange(changeArray[i]);
                    }
                    goto Label_06D5;
                }
                case 0xfd:
                case 0xfe:
                case 0xff:
                    this.ProcessDone(cmdHandler, dataStream, ref runBehavior, stateObj);
                    if ((token == 0xfe) && (cmdHandler != null))
                    {
                        cmdHandler.OnDoneProc();
                    }
                    goto Label_06D5;

                case 0xed:
                    this.ProcessSSPI(tokenLength);
                    goto Label_06D5;

                default:
                    goto Label_06D5;
            }
            if (this._statistics != null)
            {
                this._statistics.WaitForDoneAfterRow = true;
            }
            flag = true;
        Label_06D5:
            if ((stateObj._pendingData && (RunBehavior.ReturnImmediately != (RunBehavior.ReturnImmediately & runBehavior))) || ((!stateObj._pendingData && stateObj._attentionSent) && !stateObj._attentionReceived))
            {
                goto Label_0016;
            }
        Label_0705:
            if (!stateObj._pendingData && (this.CurrentTransaction != null))
            {
                this.CurrentTransaction.Activate();
            }
            if (stateObj._attentionSent && stateObj._attentionReceived)
            {
                stateObj._attentionSent = false;
                stateObj._attentionReceived = false;
                if ((RunBehavior.Clean != (RunBehavior.Clean & runBehavior)) && !stateObj._internalTimeout)
                {
                    this.Errors.Add(new SqlError(0, 0, 11, this._server, SQLMessage.OperationCancelled(), "", 0));
                }
            }
            if ((this._errors != null) || (this._warnings != null))
            {
                this.ThrowExceptionAndWarning();
            }
            return flag;
        }

        private void SendPreLoginHandshake(byte[] instanceName, bool encrypt)
        {
            this._physicalStateObj._outputMessageType = 0x12;
            int num2 = 0x1a;
            byte[] b = new byte[0x419];
            int index = 0;
            for (int i = 0; i < 5; i++)
            {
                int num3;
                int num4 = 0;
                this.WriteByte((byte) i, this._physicalStateObj);
                this.WriteByte((byte) (num2 & 0xff00), this._physicalStateObj);
                this.WriteByte((byte) (num2 & 0xff), this._physicalStateObj);
                switch (i)
                {
                    case 0:
                        b[index++] = 0x10;
                        b[index++] = 0;
                        b[index++] = 0;
                        b[index++] = 0;
                        b[index++] = 0;
                        b[index++] = 0;
                        num2 += 6;
                        num4 = 6;
                        goto Label_0196;

                    case 1:
                        if (this._encryptionOption != System.Data.SqlClient.EncryptionOptions.NOT_SUP)
                        {
                            break;
                        }
                        b[index] = 2;
                        goto Label_00EA;

                    case 2:
                        num3 = 0;
                        goto Label_010C;

                    case 3:
                    {
                        int currentThreadIdForTdsLoginOnly = TdsParserStaticMethods.GetCurrentThreadIdForTdsLoginOnly();
                        b[index++] = (byte) (0xff000000L & currentThreadIdForTdsLoginOnly);
                        b[index++] = (byte) (0xff0000 & currentThreadIdForTdsLoginOnly);
                        b[index++] = (byte) (0xff00 & currentThreadIdForTdsLoginOnly);
                        b[index++] = (byte) (0xff & currentThreadIdForTdsLoginOnly);
                        num2 += 4;
                        num4 = 4;
                        goto Label_0196;
                    }
                    case 4:
                        b[index++] = this._fMARS ? ((byte) 1) : ((byte) 0);
                        num2++;
                        num4++;
                        goto Label_0196;

                    default:
                        goto Label_0196;
                }
                if (encrypt)
                {
                    b[index] = 1;
                    this._encryptionOption = System.Data.SqlClient.EncryptionOptions.ON;
                }
                else
                {
                    b[index] = 0;
                    this._encryptionOption = System.Data.SqlClient.EncryptionOptions.OFF;
                }
            Label_00EA:
                index++;
                num2++;
                num4 = 1;
                goto Label_0196;
            Label_00FE:
                b[index] = instanceName[num3];
                index++;
                num3++;
            Label_010C:
                if (instanceName[num3] != 0)
                {
                    goto Label_00FE;
                }
                b[index] = 0;
                index++;
                num3++;
                num2 += num3;
                num4 = num3;
            Label_0196:
                this.WriteByte((byte) (num4 & 0xff00), this._physicalStateObj);
                this.WriteByte((byte) (num4 & 0xff), this._physicalStateObj);
            }
            this.WriteByte(0xff, this._physicalStateObj);
            this.WriteByteArray(b, index, 0, this._physicalStateObj);
            this._physicalStateObj.WritePacket(1);
        }

        public void SkipBytes(int num, TdsParserStateObject stateObj)
        {
            stateObj.ReadByteArray(null, 0, num);
        }

        internal void SkipLongBytes(ulong num, TdsParserStateObject stateObj)
        {
            int len = 0;
            while (num > 0L)
            {
                len = (num > 0x7fffffffL) ? 0x7fffffff : ((int) num);
                stateObj.ReadByteArray(null, 0, len);
                num -= len;
            }
        }

        internal ulong SkipPlpValue(ulong cb, TdsParserStateObject stateObj)
        {
            ulong num2 = 0L;
            if (stateObj._longlenleft == 0L)
            {
                stateObj.ReadPlpLength(false);
            }
            while ((num2 < cb) && (stateObj._longlenleft > 0L))
            {
                int num;
                if (stateObj._longlenleft > 0x7fffffffL)
                {
                    num = 0x7fffffff;
                }
                else
                {
                    num = (int) stateObj._longlenleft;
                }
                num = ((cb - num2) < num) ? ((int) (cb - num2)) : num;
                this.SkipBytes(num, stateObj);
                stateObj._longlenleft -= num;
                num2 += num;
                if (stateObj._longlenleft == 0L)
                {
                    stateObj.ReadPlpLength(false);
                }
            }
            return num2;
        }

        internal void SkipRow(_SqlMetaDataSet columns, TdsParserStateObject stateObj)
        {
            this.SkipRow(columns, 0, stateObj);
        }

        internal void SkipRow(_SqlMetaDataSet columns, int startCol, TdsParserStateObject stateObj)
        {
            for (int i = startCol; i < columns.Length; i++)
            {
                _SqlMetaData md = columns[i];
                if (md.metaType.IsLong && !md.metaType.IsPlp)
                {
                    byte num2 = stateObj.ReadByte();
                    if (num2 == 0)
                    {
                        continue;
                    }
                    this.SkipBytes(num2 + 8, stateObj);
                }
                this.SkipValue(md, stateObj);
            }
        }

        internal void SkipValue(SqlMetaDataPriv md, TdsParserStateObject stateObj)
        {
            if (md.metaType.IsPlp)
            {
                this.SkipPlpValue(ulong.MaxValue, stateObj);
            }
            else
            {
                int tokenLength = this.GetTokenLength(md.tdsType, stateObj);
                if (!this.IsNull(md.metaType, (ulong) tokenLength))
                {
                    this.SkipBytes(tokenLength, stateObj);
                }
            }
        }

        private void SNISSPIData(byte[] receivedBuff, uint receivedLength, byte[] sendBuff, ref uint sendLength)
        {
            if (receivedBuff == null)
            {
                receivedLength = 0;
            }
            if (SNINativeMethodWrapper.SNISecGenClientContext(this._physicalStateObj.Handle, receivedBuff, receivedLength, sendBuff, ref sendLength, this._sniSpnBuffer) != 0)
            {
                this.SSPIError(SQLMessage.SSPIGenerateError(), "GenClientContext");
            }
        }

        private void SSPIData(byte[] receivedBuff, uint receivedLength, byte[] sendBuff, ref uint sendLength)
        {
            this.SNISSPIData(receivedBuff, receivedLength, sendBuff, ref sendLength);
        }

        private void SSPIError(string error, string procedure)
        {
            this.Errors.Add(new SqlError(0, 0, 11, this._server, error, procedure, 0));
            this.ThrowExceptionAndWarning();
        }

        internal void TdsExecuteRPC(_SqlRPC[] rpcArray, int timeout, bool inSchema, SqlNotificationRequest notificationRequest, TdsParserStateObject stateObj, bool isCommandProc)
        {
            if ((TdsParserState.Broken != this.State) && (this.State != TdsParserState.Closed))
            {
                _SqlRPC lrpc = null;
                bool lockTaken = false;
                lock (this._connHandler)
                {
                    try
                    {
                        if (this._isYukon && !this.MARSOn)
                        {
                            Monitor.Enter(this._physicalStateObj, ref lockTaken);
                        }
                        this._connHandler.CheckEnlistedTransactionBinding();
                        stateObj.SetTimeoutSeconds(timeout);
                        if (!this._fMARS && this._physicalStateObj.HasOpenResult)
                        {
                            Bid.Trace("<sc.TdsParser.TdsExecuteRPC|ERR> Potential multi-threaded misuse of connection, non-MARs connection with an open result %d#\n", this.ObjectID);
                        }
                        stateObj.SniContext = SniContext.Snix_Execute;
                        if (this._isYukon)
                        {
                            this.WriteMarsHeader(stateObj, this.CurrentTransaction);
                            if (notificationRequest != null)
                            {
                                this.WriteQueryNotificationHeader(notificationRequest, stateObj);
                            }
                        }
                        stateObj._outputMessageType = 3;
                        for (int i = 0; i < rpcArray.Length; i++)
                        {
                            int length;
                            lrpc = rpcArray[i];
                            if ((lrpc.ProcID != 0) && this._isShiloh)
                            {
                                this.WriteShort(0xffff, stateObj);
                                this.WriteShort((short) lrpc.ProcID, stateObj);
                            }
                            else
                            {
                                length = lrpc.rpcName.Length;
                                this.WriteShort(length, stateObj);
                                this.WriteString(lrpc.rpcName, length, 0, stateObj);
                            }
                            this.WriteShort((short) lrpc.options, stateObj);
                            SqlParameter[] parameters = lrpc.parameters;
                            for (int j = 0; j < parameters.Length; j++)
                            {
                                SqlParameter param = parameters[j];
                                if (param == null)
                                {
                                    break;
                                }
                                param.Validate(j, isCommandProc);
                                MetaType internalMetaType = param.InternalMetaType;
                                if (internalMetaType.IsNewKatmaiType)
                                {
                                    this.WriteSmiParameter(param, j, 0 != (lrpc.paramoptions[j] & 2), stateObj);
                                }
                                else
                                {
                                    bool flag2;
                                    if (((!this._isShiloh && !internalMetaType.Is70Supported) || (!this._isYukon && !internalMetaType.Is80Supported)) || (!this._isKatmai && !internalMetaType.Is90Supported))
                                    {
                                        throw ADP.VersionDoesNotSupportDataType(internalMetaType.TypeName);
                                    }
                                    object coercedValue = null;
                                    if (param.Direction == ParameterDirection.Output)
                                    {
                                        bool paramaterIsSqlType = param.ParamaterIsSqlType;
                                        param.Value = null;
                                        coercedValue = null;
                                        param.ParamaterIsSqlType = paramaterIsSqlType;
                                    }
                                    else
                                    {
                                        coercedValue = param.GetCoercedValue();
                                    }
                                    bool isNull = ADP.IsNull(coercedValue, out flag2);
                                    string parameterNameFixed = param.ParameterNameFixed;
                                    this.WriteParameterName(parameterNameFixed, stateObj);
                                    this.WriteByte(lrpc.paramoptions[j], stateObj);
                                    this.WriteByte(internalMetaType.NullableType, stateObj);
                                    if (internalMetaType.TDSType == 0x62)
                                    {
                                        this.WriteSqlVariantValue(flag2 ? MetaType.GetComValueFromSqlVariant(coercedValue) : coercedValue, param.GetActualSize(), param.Offset, stateObj);
                                    }
                                    else
                                    {
                                        int actualSize;
                                        int num4 = internalMetaType.IsSizeInCharacters ? (param.GetParameterSize() * 2) : param.GetParameterSize();
                                        if (internalMetaType.TDSType != 240)
                                        {
                                            actualSize = param.GetActualSize();
                                        }
                                        else
                                        {
                                            actualSize = 0;
                                        }
                                        int size = 0;
                                        int num = 0;
                                        if (internalMetaType.IsAnsiType)
                                        {
                                            if (!isNull)
                                            {
                                                string str;
                                                if (flag2)
                                                {
                                                    if (coercedValue is SqlString)
                                                    {
                                                        SqlString str3 = (SqlString) coercedValue;
                                                        str = str3.Value;
                                                    }
                                                    else
                                                    {
                                                        str = new string(((SqlChars) coercedValue).Value);
                                                    }
                                                }
                                                else
                                                {
                                                    str = (string) coercedValue;
                                                }
                                                size = this.GetEncodingCharLength(str, actualSize, param.Offset, this._defaultEncoding);
                                            }
                                            if (internalMetaType.IsPlp)
                                            {
                                                this.WriteShort(0xffff, stateObj);
                                            }
                                            else
                                            {
                                                num = (num4 > size) ? num4 : size;
                                                if (num == 0)
                                                {
                                                    if (internalMetaType.IsNCharType)
                                                    {
                                                        num = 2;
                                                    }
                                                    else
                                                    {
                                                        num = 1;
                                                    }
                                                }
                                                this.WriteParameterVarLen(internalMetaType, num, false, stateObj);
                                            }
                                        }
                                        else if (internalMetaType.SqlDbType == SqlDbType.Timestamp)
                                        {
                                            this.WriteParameterVarLen(internalMetaType, 8, false, stateObj);
                                        }
                                        else
                                        {
                                            if (internalMetaType.SqlDbType == SqlDbType.Udt)
                                            {
                                                byte[] b = null;
                                                bool flag3 = ADP.IsNull(coercedValue);
                                                Format native = Format.Native;
                                                if (!flag3)
                                                {
                                                    b = this._connHandler.Connection.GetBytes(coercedValue, out native, out num);
                                                    num4 = b.Length;
                                                    if ((num4 < 0) || ((num4 >= 0xffff) && (num != -1)))
                                                    {
                                                        throw new IndexOutOfRangeException();
                                                    }
                                                }
                                                BitConverter.GetBytes((long) num4);
                                                if (ADP.IsEmpty(param.UdtTypeName))
                                                {
                                                    throw SQL.MustSetUdtTypeNameForUdtParams();
                                                }
                                                string[] strArray = SqlParameter.ParseTypeName(param.UdtTypeName, true);
                                                if (!ADP.IsEmpty(strArray[0]) && (0xff < strArray[0].Length))
                                                {
                                                    throw ADP.ArgumentOutOfRange("names");
                                                }
                                                if (!ADP.IsEmpty(strArray[1]) && (0xff < strArray[strArray.Length - 2].Length))
                                                {
                                                    throw ADP.ArgumentOutOfRange("names");
                                                }
                                                if (0xff < strArray[2].Length)
                                                {
                                                    throw ADP.ArgumentOutOfRange("names");
                                                }
                                                this.WriteUDTMetaData(coercedValue, strArray[0], strArray[1], strArray[2], stateObj);
                                                if (!flag3)
                                                {
                                                    this.WriteUnsignedLong((ulong) b.Length, stateObj);
                                                    if (b.Length > 0)
                                                    {
                                                        this.WriteInt(b.Length, stateObj);
                                                        this.WriteByteArray(b, b.Length, 0, stateObj);
                                                    }
                                                    this.WriteInt(0, stateObj);
                                                }
                                                else
                                                {
                                                    this.WriteUnsignedLong(ulong.MaxValue, stateObj);
                                                }
                                                goto Label_080A;
                                            }
                                            if (internalMetaType.IsPlp)
                                            {
                                                if (internalMetaType.SqlDbType != SqlDbType.Xml)
                                                {
                                                    this.WriteShort(0xffff, stateObj);
                                                }
                                            }
                                            else if (!internalMetaType.IsVarTime && (internalMetaType.SqlDbType != SqlDbType.Date))
                                            {
                                                num = (num4 > actualSize) ? num4 : actualSize;
                                                if ((num == 0) && this.IsYukonOrNewer)
                                                {
                                                    if (internalMetaType.IsNCharType)
                                                    {
                                                        num = 2;
                                                    }
                                                    else
                                                    {
                                                        num = 1;
                                                    }
                                                }
                                                this.WriteParameterVarLen(internalMetaType, num, false, stateObj);
                                            }
                                        }
                                        if (internalMetaType.SqlDbType == SqlDbType.Decimal)
                                        {
                                            byte actualPrecision = param.GetActualPrecision();
                                            byte actualScale = param.GetActualScale();
                                            if (actualPrecision > 0x26)
                                            {
                                                throw SQL.PrecisionValueOutOfRange(actualPrecision);
                                            }
                                            if (!isNull)
                                            {
                                                if (flag2)
                                                {
                                                    coercedValue = AdjustSqlDecimalScale((SqlDecimal) coercedValue, actualScale);
                                                    if (actualPrecision != 0)
                                                    {
                                                        SqlDecimal num11 = (SqlDecimal) coercedValue;
                                                        if (actualPrecision < num11.Precision)
                                                        {
                                                            throw ADP.ParameterValueOutOfRange((SqlDecimal) coercedValue);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    coercedValue = AdjustDecimalScale((decimal) coercedValue, actualScale);
                                                    SqlDecimal num10 = new SqlDecimal((decimal) coercedValue);
                                                    if ((actualPrecision != 0) && (actualPrecision < num10.Precision))
                                                    {
                                                        throw ADP.ParameterValueOutOfRange((decimal) coercedValue);
                                                    }
                                                }
                                            }
                                            if (actualPrecision == 0)
                                            {
                                                if (this._isShiloh)
                                                {
                                                    this.WriteByte(0x1d, stateObj);
                                                }
                                                else
                                                {
                                                    this.WriteByte(0x1c, stateObj);
                                                }
                                            }
                                            else
                                            {
                                                this.WriteByte(actualPrecision, stateObj);
                                            }
                                            this.WriteByte(actualScale, stateObj);
                                        }
                                        else if (internalMetaType.IsVarTime)
                                        {
                                            this.WriteByte(param.GetActualScale(), stateObj);
                                        }
                                        if (this._isYukon && (internalMetaType.SqlDbType == SqlDbType.Xml))
                                        {
                                            if ((((param.XmlSchemaCollectionDatabase != null) && (param.XmlSchemaCollectionDatabase != ADP.StrEmpty)) || ((param.XmlSchemaCollectionOwningSchema != null) && (param.XmlSchemaCollectionOwningSchema != ADP.StrEmpty))) || ((param.XmlSchemaCollectionName != null) && (param.XmlSchemaCollectionName != ADP.StrEmpty)))
                                            {
                                                this.WriteByte(1, stateObj);
                                                if ((param.XmlSchemaCollectionDatabase != null) && (param.XmlSchemaCollectionDatabase != ADP.StrEmpty))
                                                {
                                                    length = param.XmlSchemaCollectionDatabase.Length;
                                                    this.WriteByte((byte) length, stateObj);
                                                    this.WriteString(param.XmlSchemaCollectionDatabase, length, 0, stateObj);
                                                }
                                                else
                                                {
                                                    this.WriteByte(0, stateObj);
                                                }
                                                if ((param.XmlSchemaCollectionOwningSchema != null) && (param.XmlSchemaCollectionOwningSchema != ADP.StrEmpty))
                                                {
                                                    length = param.XmlSchemaCollectionOwningSchema.Length;
                                                    this.WriteByte((byte) length, stateObj);
                                                    this.WriteString(param.XmlSchemaCollectionOwningSchema, length, 0, stateObj);
                                                }
                                                else
                                                {
                                                    this.WriteByte(0, stateObj);
                                                }
                                                if ((param.XmlSchemaCollectionName != null) && (param.XmlSchemaCollectionName != ADP.StrEmpty))
                                                {
                                                    length = param.XmlSchemaCollectionName.Length;
                                                    this.WriteShort((short) length, stateObj);
                                                    this.WriteString(param.XmlSchemaCollectionName, length, 0, stateObj);
                                                }
                                                else
                                                {
                                                    this.WriteShort(0, stateObj);
                                                }
                                            }
                                            else
                                            {
                                                this.WriteByte(0, stateObj);
                                            }
                                        }
                                        else if (this._isShiloh && internalMetaType.IsCharType)
                                        {
                                            SqlCollation collation = (param.Collation != null) ? param.Collation : this._defaultCollation;
                                            this.WriteUnsignedInt(collation.info, stateObj);
                                            this.WriteByte(collation.sortId, stateObj);
                                        }
                                        if (size == 0)
                                        {
                                            this.WriteParameterVarLen(internalMetaType, actualSize, isNull, stateObj);
                                        }
                                        else
                                        {
                                            this.WriteParameterVarLen(internalMetaType, size, isNull, stateObj);
                                        }
                                        if (!isNull)
                                        {
                                            if (flag2)
                                            {
                                                this.WriteSqlValue(coercedValue, internalMetaType, actualSize, size, param.Offset, stateObj);
                                            }
                                            else
                                            {
                                                this.WriteValue(coercedValue, internalMetaType, param.GetActualScale(), actualSize, size, param.Offset, stateObj);
                                            }
                                        }
                                    Label_080A:;
                                    }
                                }
                            }
                            if (i < (rpcArray.Length - 1))
                            {
                                if (this._isYukon)
                                {
                                    this.WriteByte(0xff, stateObj);
                                }
                                else
                                {
                                    this.WriteByte(0x80, stateObj);
                                }
                            }
                        }
                        stateObj.ExecuteFlush();
                        stateObj.SniContext = SniContext.Snix_Read;
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        this.FailureCleanup(stateObj, exception);
                        throw;
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(this._physicalStateObj);
                        }
                    }
                }
            }
        }

        internal void TdsExecuteSQLBatch(string text, int timeout, SqlNotificationRequest notificationRequest, TdsParserStateObject stateObj)
        {
            if ((TdsParserState.Broken != this.State) && (this.State != TdsParserState.Closed))
            {
                if (stateObj.BcpLock)
                {
                    throw SQL.ConnectionLockedForBcpEvent();
                }
                bool lockTaken = false;
                lock (this._connHandler)
                {
                    try
                    {
                        if (this._isYukon && !this.MARSOn)
                        {
                            Monitor.Enter(this._physicalStateObj, ref lockTaken);
                        }
                        this._connHandler.CheckEnlistedTransactionBinding();
                        stateObj.SetTimeoutSeconds(timeout);
                        if (!this._fMARS && this._physicalStateObj.HasOpenResult)
                        {
                            Bid.Trace("<sc.TdsParser.TdsExecuteSQLBatch|ERR> Potential multi-threaded misuse of connection, non-MARs connection with an open result %d#\n", this.ObjectID);
                        }
                        stateObj.SniContext = SniContext.Snix_Execute;
                        if (this._isYukon)
                        {
                            this.WriteMarsHeader(stateObj, this.CurrentTransaction);
                            if (notificationRequest != null)
                            {
                                this.WriteQueryNotificationHeader(notificationRequest, stateObj);
                            }
                        }
                        stateObj._outputMessageType = 1;
                        this.WriteString(text, text.Length, 0, stateObj);
                        stateObj.ExecuteFlush();
                        stateObj.SniContext = SniContext.Snix_Read;
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        this.FailureCleanup(stateObj, exception);
                        throw;
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(this._physicalStateObj);
                        }
                    }
                }
            }
        }

        internal SqlDataReader TdsExecuteTransactionManagerRequest(byte[] buffer, TdsEnums.TransactionManagerRequestType request, string transactionName, TdsEnums.TransactionManagerIsolationLevel isoLevel, int timeout, SqlInternalTransaction transaction, TdsParserStateObject stateObj, bool isDelegateControlRequest)
        {
            SqlDataReader reader2;
            if ((TdsParserState.Broken == this.State) || (this.State == TdsParserState.Closed))
            {
                return null;
            }
            bool lockTaken = false;
            lock (this._connHandler)
            {
                try
                {
                    try
                    {
                        if (this._isYukon && !this.MARSOn)
                        {
                            Monitor.Enter(this._physicalStateObj, ref lockTaken);
                        }
                        if (!isDelegateControlRequest)
                        {
                            this._connHandler.CheckEnlistedTransactionBinding();
                        }
                        stateObj._outputMessageType = 14;
                        stateObj.SetTimeoutSeconds(timeout);
                        stateObj.SniContext = SniContext.Snix_Execute;
                        if (this._isYukon)
                        {
                            this.WriteMarsHeader(stateObj, this._currentTransaction);
                        }
                        this.WriteShort((short) request, stateObj);
                        bool flag = false;
                        switch (request)
                        {
                            case TdsEnums.TransactionManagerRequestType.GetDTCAddress:
                                this.WriteShort(0, stateObj);
                                flag = true;
                                goto Label_0193;

                            case TdsEnums.TransactionManagerRequestType.Propagate:
                                if (buffer == null)
                                {
                                    break;
                                }
                                this.WriteShort(buffer.Length, stateObj);
                                this.WriteByteArray(buffer, buffer.Length, 0, stateObj);
                                goto Label_0193;

                            case TdsEnums.TransactionManagerRequestType.Begin:
                                if (this._currentTransaction != transaction)
                                {
                                    this.PendingTransaction = transaction;
                                }
                                this.WriteByte((byte) isoLevel, stateObj);
                                this.WriteByte((byte) (transactionName.Length * 2), stateObj);
                                this.WriteString(transactionName, stateObj);
                                goto Label_0193;

                            case TdsEnums.TransactionManagerRequestType.Commit:
                                this.WriteByte(0, stateObj);
                                this.WriteByte(0, stateObj);
                                goto Label_0193;

                            case TdsEnums.TransactionManagerRequestType.Rollback:
                                this.WriteByte((byte) (transactionName.Length * 2), stateObj);
                                this.WriteString(transactionName, stateObj);
                                this.WriteByte(0, stateObj);
                                goto Label_0193;

                            case TdsEnums.TransactionManagerRequestType.Save:
                                this.WriteByte((byte) (transactionName.Length * 2), stateObj);
                                this.WriteString(transactionName, stateObj);
                                goto Label_0193;

                            default:
                                goto Label_0193;
                        }
                        this.WriteShort(0, stateObj);
                    Label_0193:
                        stateObj.WritePacket(1);
                        stateObj._pendingData = true;
                        SqlDataReader reader = null;
                        stateObj.SniContext = SniContext.Snix_Read;
                        if (flag)
                        {
                            reader = new SqlDataReader(null, CommandBehavior.Default);
                            reader.Bind(stateObj);
                            _SqlMetaDataSet metaData = reader.MetaData;
                        }
                        else
                        {
                            this.Run(RunBehavior.UntilDone, null, null, null, stateObj);
                        }
                        if ((request == TdsEnums.TransactionManagerRequestType.Begin) || (request == TdsEnums.TransactionManagerRequestType.Propagate))
                        {
                            if ((transaction != null) && (transaction.TransactionId == this._retainedTransactionId))
                            {
                                return reader;
                            }
                            this._retainedTransactionId = 0L;
                        }
                        return reader;
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        this.FailureCleanup(stateObj, exception);
                        throw;
                    }
                    return reader2;
                }
                finally
                {
                    this._pendingTransaction = null;
                    if (lockTaken)
                    {
                        Monitor.Exit(this._physicalStateObj);
                    }
                }
            }
            return reader2;
        }

        internal void TdsLogin(SqlLogin rec)
        {
            this._physicalStateObj.SetTimeoutSeconds(rec.timeout);
            byte[] b = null;
            b = TdsParserStaticMethods.EncryptPassword(rec.password);
            byte[] buffer2 = null;
            buffer2 = TdsParserStaticMethods.EncryptPassword(rec.newPassword);
            this._physicalStateObj._outputMessageType = 0x10;
            int v = 0x5e;
            string s = ".Net SqlClient Data Provider";
            v += ((((((rec.hostName.Length + rec.applicationName.Length) + rec.serverName.Length) + s.Length) + rec.language.Length) + rec.database.Length) + rec.attachDBFilename.Length) * 2;
            byte[] sendBuff = null;
            uint sendLength = 0;
            if (!rec.useSSPI)
            {
                v += ((rec.userName.Length * 2) + b.Length) + buffer2.Length;
            }
            else if (rec.useSSPI)
            {
                sendBuff = new byte[s_maxSSPILength];
                sendLength = s_maxSSPILength;
                this._physicalStateObj.SniContext = SniContext.Snix_LoginSspi;
                this.SSPIData(null, 0, sendBuff, ref sendLength);
                if (sendLength > 0x7fffffff)
                {
                    throw SQL.InvalidSSPIPacketSize();
                }
                this._physicalStateObj.SniContext = SniContext.Snix_Login;
                v += (int) sendLength;
            }
            try
            {
                this.WriteInt(v, this._physicalStateObj);
                this.WriteInt(0x730a0003, this._physicalStateObj);
                this.WriteInt(rec.packetSize, this._physicalStateObj);
                this.WriteInt(0x6000000, this._physicalStateObj);
                this.WriteInt(TdsParserStaticMethods.GetCurrentProcessIdForTdsLoginOnly(), this._physicalStateObj);
                this.WriteInt(0, this._physicalStateObj);
                int num2 = 0;
                num2 |= 0x20;
                num2 |= 0x40;
                num2 |= 0x80;
                num2 |= 0x100;
                num2 |= 0x200;
                if (rec.useReplication)
                {
                    num2 |= 0x3000;
                }
                if (rec.useSSPI)
                {
                    num2 |= 0x8000;
                }
                if (rec.readOnlyIntent)
                {
                    num2 |= 0x200000;
                }
                if (!ADP.IsEmpty(rec.newPassword))
                {
                    num2 |= 0x1000000;
                }
                if (rec.userInstance)
                {
                    num2 |= 0x4000000;
                }
                this.WriteInt(num2, this._physicalStateObj);
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.TdsParser.TdsLogin|ADV> %d#, TDS Login7 flags = %d:\n", this.ObjectID, num2);
                }
                this.WriteInt(0, this._physicalStateObj);
                this.WriteInt(0, this._physicalStateObj);
                int num = 0x5e;
                this.WriteShort(num, this._physicalStateObj);
                this.WriteShort(rec.hostName.Length, this._physicalStateObj);
                num += rec.hostName.Length * 2;
                if (!rec.useSSPI)
                {
                    this.WriteShort(num, this._physicalStateObj);
                    this.WriteShort(rec.userName.Length, this._physicalStateObj);
                    num += rec.userName.Length * 2;
                    this.WriteShort(num, this._physicalStateObj);
                    this.WriteShort(b.Length / 2, this._physicalStateObj);
                    num += b.Length;
                }
                else
                {
                    this.WriteShort(0, this._physicalStateObj);
                    this.WriteShort(0, this._physicalStateObj);
                    this.WriteShort(0, this._physicalStateObj);
                    this.WriteShort(0, this._physicalStateObj);
                }
                this.WriteShort(num, this._physicalStateObj);
                this.WriteShort(rec.applicationName.Length, this._physicalStateObj);
                num += rec.applicationName.Length * 2;
                this.WriteShort(num, this._physicalStateObj);
                this.WriteShort(rec.serverName.Length, this._physicalStateObj);
                num += rec.serverName.Length * 2;
                this.WriteShort(num, this._physicalStateObj);
                this.WriteShort(0, this._physicalStateObj);
                this.WriteShort(num, this._physicalStateObj);
                this.WriteShort(s.Length, this._physicalStateObj);
                num += s.Length * 2;
                this.WriteShort(num, this._physicalStateObj);
                this.WriteShort(rec.language.Length, this._physicalStateObj);
                num += rec.language.Length * 2;
                this.WriteShort(num, this._physicalStateObj);
                this.WriteShort(rec.database.Length, this._physicalStateObj);
                num += rec.database.Length * 2;
                if (s_nicAddress == null)
                {
                    s_nicAddress = TdsParserStaticMethods.GetNetworkPhysicalAddressForTdsLoginOnly();
                }
                this.WriteByteArray(s_nicAddress, s_nicAddress.Length, 0, this._physicalStateObj);
                this.WriteShort(num, this._physicalStateObj);
                if (rec.useSSPI)
                {
                    this.WriteShort((int) sendLength, this._physicalStateObj);
                    num += (int) sendLength;
                }
                else
                {
                    this.WriteShort(0, this._physicalStateObj);
                }
                this.WriteShort(num, this._physicalStateObj);
                this.WriteShort(rec.attachDBFilename.Length, this._physicalStateObj);
                num += rec.attachDBFilename.Length * 2;
                this.WriteShort(num, this._physicalStateObj);
                this.WriteShort(buffer2.Length / 2, this._physicalStateObj);
                this.WriteInt(0, this._physicalStateObj);
                this.WriteString(rec.hostName, this._physicalStateObj);
                if (!rec.useSSPI)
                {
                    this.WriteString(rec.userName, this._physicalStateObj);
                    this._physicalStateObj._tracePasswordOffset = this._physicalStateObj._outBytesUsed;
                    this._physicalStateObj._tracePasswordLength = b.Length;
                    this.WriteByteArray(b, b.Length, 0, this._physicalStateObj);
                }
                this.WriteString(rec.applicationName, this._physicalStateObj);
                this.WriteString(rec.serverName, this._physicalStateObj);
                this.WriteString(s, this._physicalStateObj);
                this.WriteString(rec.language, this._physicalStateObj);
                this.WriteString(rec.database, this._physicalStateObj);
                if (rec.useSSPI)
                {
                    this.WriteByteArray(sendBuff, (int) sendLength, 0, this._physicalStateObj);
                }
                this.WriteString(rec.attachDBFilename, this._physicalStateObj);
                if (!rec.useSSPI)
                {
                    this._physicalStateObj._traceChangePasswordOffset = this._physicalStateObj._outBytesUsed;
                    this._physicalStateObj._traceChangePasswordLength = buffer2.Length;
                    this.WriteByteArray(buffer2, buffer2.Length, 0, this._physicalStateObj);
                }
            }
            catch (Exception exception)
            {
                if (ADP.IsCatchableExceptionType(exception))
                {
                    this._physicalStateObj._outputPacketNumber = 1;
                    this._physicalStateObj.ResetBuffer();
                }
                throw;
            }
            this._physicalStateObj.WritePacket(1);
            this._physicalStateObj._pendingData = true;
        }

        internal void ThrowExceptionAndWarning()
        {
            lock (this._ErrorCollectionLock)
            {
                if ((this._errors == null) && (this._warnings == null))
                {
                    Bid.Trace("<sc.TdsParser.ThrowExceptionAndWarning|ERR> Potential multi-threaded misuse of connection, unexpectedly empty warnings/errors under lock %d#\n", this.ObjectID);
                }
                SqlErrorCollection temp = null;
                bool breakConnection = this.AddSqlErrorToCollection(ref temp, ref this._errors) | this.AddSqlErrorToCollection(ref temp, ref this._attentionErrors);
                breakConnection |= this.AddSqlErrorToCollection(ref temp, ref this._warnings);
                breakConnection |= this.AddSqlErrorToCollection(ref temp, ref this._attentionWarnings);
                if (breakConnection)
                {
                    this._state = TdsParserState.Broken;
                }
                if ((temp != null) && (temp.Count > 0))
                {
                    string serverVersion = null;
                    if (this._state == TdsParserState.OpenLoggedIn)
                    {
                        serverVersion = this._connHandler.ServerVersion;
                    }
                    SqlException exception = SqlException.CreateException(temp, serverVersion);
                    this._connHandler.OnError(exception, breakConnection);
                }
            }
        }

        internal void ThrowUnsupportedCollationEncountered(TdsParserStateObject stateObj)
        {
            this.Errors.Add(new SqlError(0, 0, 11, this._server, SQLMessage.CultureIdError(), "", 0));
            if (stateObj != null)
            {
                stateObj.CleanWire();
                stateObj._pendingData = false;
            }
            this.ThrowExceptionAndWarning();
        }

        private string TraceObjectClass(object instance)
        {
            if (instance == null)
            {
                return "(null)";
            }
            return instance.GetType().ToString();
        }

        internal string TraceString()
        {
            return string.Format(null, "\n\t         _physicalStateObj = {0}\n\t         _pMarsPhysicalConObj = {1}\n\t         _state = {2}\n\t         _server = {3}\n\t         _fResetConnection = {4}\n\t         _defaultCollation = {5}\n\t         _defaultCodePage = {6}\n\t         _defaultLCID = {7}\n\t         _defaultEncoding = {8}\n\t         _encryptionOption = {10}\n\t         _currentTransaction = {11}\n\t         _pendingTransaction = {12}\n\t         _retainedTransactionId = {13}\n\t         _nonTransactedOpenResultCount = {14}\n\t         _connHandler = {15}\n\t         _fAsync = {16}\n\t         _fMARS = {17}\n\t         _fAwaitingPreLogin = {18}\n\t         _fPreLoginErrorOccurred = {19}\n\t         _sessionPool = {20}\n\t         _isShiloh = {21}\n\t         _isShilohSP1 = {22}\n\t         _isYukon = {23}\n\t         _sniSpnBuffer = {24}\n\t         _errors = {25}\n\t         _warnings = {26}\n\t         _attentionErrors = {27}\n\t         _attentionWarnings = {28}\n\t         _statistics = {29}\n\t         _statisticsIsInTransaction = {30}\n\t         _fPreserveTransaction = {31}         _fParallel = {32}", new object[] { 
                null == this._physicalStateObj, null == this._pMarsPhysicalConObj, this._state, this._server, (bool) this._fResetConnection, (this._defaultCollation == null) ? "(null)" : this._defaultCollation.TraceString(), this._defaultCodePage, this._defaultLCID, this.TraceObjectClass(this._defaultEncoding), "", this._encryptionOption, (this._currentTransaction == null) ? "(null)" : this._currentTransaction.TraceString(), (this._pendingTransaction == null) ? "(null)" : this._pendingTransaction.TraceString(), this._retainedTransactionId, this._nonTransactedOpenResultCount, (this._connHandler == null) ? "(null)" : this._connHandler.ObjectID.ToString((IFormatProvider) null), 
                this._fAsync, this._fMARS, (bool) this._fAwaitingPreLogin, (bool) this._fPreLoginErrorOccurred, (this._sessionPool == null) ? "(null)" : this._sessionPool.TraceString(), this._isShiloh, this._isShilohSP1, this._isYukon, (this._sniSpnBuffer == null) ? "(null)" : this._sniSpnBuffer.Length.ToString((IFormatProvider) null), (this._errors == null) ? "(null)" : this._errors.Count.ToString((IFormatProvider) null), (this._warnings == null) ? "(null)" : this._warnings.Count.ToString((IFormatProvider) null), (this._attentionErrors == null) ? "(null)" : this._attentionErrors.Count.ToString((IFormatProvider) null), (this._attentionWarnings == null) ? "(null)" : this._attentionWarnings.Count.ToString((IFormatProvider) null), null == this._statistics, this._statisticsIsInTransaction, (bool) this._fPreserveTransaction, 
                (this._connHandler == null) ? "(null)" : this._connHandler.ConnectionOptions.MultiSubnetFailover.ToString(null)
             });
        }

        internal void WriteBulkCopyDone(TdsParserStateObject stateObj)
        {
            this.WriteByte(0xfd, stateObj);
            this.WriteShort(0, stateObj);
            this.WriteShort(0, stateObj);
            this.WriteInt(0, stateObj);
            stateObj.WritePacket(1);
            stateObj._pendingData = true;
        }

        internal void WriteBulkCopyMetaData(_SqlMetaDataSet metadataCollection, int count, TdsParserStateObject stateObj)
        {
            this.WriteByte(0x81, stateObj);
            this.WriteShort(count, stateObj);
            for (int i = 0; i < metadataCollection.Length; i++)
            {
                if (metadataCollection[i] == null)
                {
                    continue;
                }
                _SqlMetaData data = metadataCollection[i];
                if (this.IsYukonOrNewer)
                {
                    this.WriteInt(0, stateObj);
                }
                else
                {
                    this.WriteShort(0, stateObj);
                }
                ushort v = (ushort) (data.updatability << 2);
                v = (ushort) (v | (data.isNullable ? 1 : 0));
                v = (ushort) (v | (data.isIdentity ? 0x10 : 0));
                this.WriteShort(v, stateObj);
                switch (data.type)
                {
                    case SqlDbType.Xml:
                        this.WriteByteArray(s_xmlMetadataSubstituteSequence, s_xmlMetadataSubstituteSequence.Length, 0, stateObj);
                        break;

                    case SqlDbType.Udt:
                        this.WriteByte(0xa5, stateObj);
                        this.WriteTokenLength(0xa5, data.length, stateObj);
                        break;

                    case SqlDbType.Date:
                        this.WriteByte(data.tdsType, stateObj);
                        break;

                    case SqlDbType.Time:
                    case SqlDbType.DateTime2:
                    case SqlDbType.DateTimeOffset:
                        this.WriteByte(data.tdsType, stateObj);
                        this.WriteByte(data.scale, stateObj);
                        break;

                    case SqlDbType.Decimal:
                        this.WriteByte(data.tdsType, stateObj);
                        this.WriteTokenLength(data.tdsType, data.length, stateObj);
                        this.WriteByte(data.precision, stateObj);
                        this.WriteByte(data.scale, stateObj);
                        break;

                    default:
                        this.WriteByte(data.tdsType, stateObj);
                        this.WriteTokenLength(data.tdsType, data.length, stateObj);
                        if (data.metaType.IsCharType && this._isShiloh)
                        {
                            this.WriteUnsignedInt(data.collation.info, stateObj);
                            this.WriteByte(data.collation.sortId, stateObj);
                        }
                        break;
                }
                if (data.metaType.IsLong && !data.metaType.IsPlp)
                {
                    this.WriteShort(data.tableName.Length, stateObj);
                    this.WriteString(data.tableName, stateObj);
                }
                this.WriteByte((byte) data.column.Length, stateObj);
                this.WriteString(data.column, stateObj);
            }
        }

        internal void WriteBulkCopyValue(object value, SqlMetaDataPriv metadata, TdsParserStateObject stateObj)
        {
            MetaType metaType = metadata.metaType;
            ulong length = 0L;
            ulong byteCount = 0L;
            if (ADP.IsNull(value))
            {
                if (metaType.IsPlp && ((metaType.NullableType != 240) || metaType.IsLong))
                {
                    this.WriteLong(-1L, stateObj);
                }
                else if ((!metaType.IsFixed && !metaType.IsLong) && !metaType.IsVarTime)
                {
                    this.WriteShort(0xffff, stateObj);
                }
                else
                {
                    this.WriteByte(0, stateObj);
                }
            }
            else
            {
                switch (metaType.NullableType)
                {
                    case 0x22:
                    case 0xa5:
                    case 0xad:
                    case 240:
                        length = (value is byte[]) ? ((ulong) ((byte[]) value).Length) : ((ulong) ((SqlBinary) value).Length);
                        break;

                    case 0x23:
                    case 0xa7:
                    case 0xaf:
                        if (this._defaultEncoding == null)
                        {
                            this.ThrowUnsupportedCollationEncountered(null);
                        }
                        if (value is string)
                        {
                            length = (ulong) ((string) value).Length;
                            byteCount = (ulong) this._defaultEncoding.GetByteCount((string) value);
                        }
                        else
                        {
                            SqlString str4 = (SqlString) value;
                            length = (ulong) str4.Value.Length;
                            SqlString str3 = (SqlString) value;
                            byteCount = (ulong) this._defaultEncoding.GetByteCount(str3.Value);
                        }
                        break;

                    case 0x24:
                        length = 0x10L;
                        break;

                    case 0x63:
                    case 0xe7:
                    case 0xef:
                        length = (ulong) (((value is string) ? ((long) ((string) value).Length) : ((long) ((SqlString) value).Value.Length)) * 2L);
                        break;

                    case 0xf1:
                        if (value is XmlReader)
                        {
                            value = MetaType.GetStringFromXml((XmlReader) value);
                        }
                        length = (ulong) (((value is string) ? ((long) ((string) value).Length) : ((long) ((SqlString) value).Value.Length)) * 2L);
                        break;

                    default:
                        length = (ulong) metadata.length;
                        break;
                }
                if (metaType.IsLong)
                {
                    switch (metaType.SqlDbType)
                    {
                        case SqlDbType.NText:
                        case SqlDbType.Image:
                        case SqlDbType.Text:
                            this.WriteByteArray(s_longDataHeader, s_longDataHeader.Length, 0, stateObj);
                            this.WriteTokenLength(metadata.tdsType, (byteCount == 0L) ? ((int) length) : ((int) byteCount), stateObj);
                            break;

                        case SqlDbType.NVarChar:
                        case SqlDbType.VarBinary:
                        case SqlDbType.VarChar:
                        case SqlDbType.Xml:
                        case SqlDbType.Udt:
                            this.WriteUnsignedLong(18446744073709551614L, stateObj);
                            break;
                    }
                }
                else
                {
                    this.WriteTokenLength(metadata.tdsType, (byteCount == 0L) ? ((int) length) : ((int) byteCount), stateObj);
                }
                if (DataStorage.IsSqlType(value.GetType()))
                {
                    this.WriteSqlValue(value, metaType, (int) length, (int) byteCount, 0, stateObj);
                }
                else if ((metaType.SqlDbType != SqlDbType.Udt) || metaType.IsLong)
                {
                    this.WriteValue(value, metaType, metadata.scale, (int) length, (int) byteCount, 0, stateObj);
                }
                else
                {
                    this.WriteShort((int) length, stateObj);
                    this.WriteByteArray((byte[]) value, (int) length, 0, stateObj);
                }
            }
        }

        internal void WriteByte(byte b, TdsParserStateObject stateObj)
        {
            if (stateObj._outBytesUsed == stateObj._outBuff.Length)
            {
                stateObj.WritePacket(0);
            }
            stateObj._outBuff[stateObj._outBytesUsed++] = b;
        }

        internal void WriteByteArray(byte[] b, int len, int offsetBuffer, TdsParserStateObject stateObj)
        {
            int srcOffset = offsetBuffer;
            while (len > 0)
            {
                if ((stateObj._outBytesUsed + len) > stateObj._outBuff.Length)
                {
                    int count = stateObj._outBuff.Length - stateObj._outBytesUsed;
                    Buffer.BlockCopy(b, srcOffset, stateObj._outBuff, stateObj._outBytesUsed, count);
                    srcOffset += count;
                    stateObj._outBytesUsed += count;
                    if (stateObj._outBytesUsed == stateObj._outBuff.Length)
                    {
                        stateObj.WritePacket(0);
                    }
                    len -= count;
                }
                else
                {
                    Buffer.BlockCopy(b, srcOffset, stateObj._outBuff, stateObj._outBytesUsed, len);
                    stateObj._outBytesUsed += len;
                    return;
                }
            }
        }

        internal void WriteCharArray(char[] carr, int length, int offset, TdsParserStateObject stateObj)
        {
            int len = ADP.CharSize * length;
            if (len < (stateObj._outBuff.Length - stateObj._outBytesUsed))
            {
                CopyCharsToBytes(carr, offset, stateObj._outBuff, stateObj._outBytesUsed, length);
                stateObj._outBytesUsed += len;
            }
            else
            {
                if ((stateObj._bTmp == null) || (stateObj._bTmp.Length < len))
                {
                    stateObj._bTmp = new byte[len];
                }
                CopyCharsToBytes(carr, offset, stateObj._bTmp, 0, length);
                this.WriteByteArray(stateObj._bTmp, len, 0, stateObj);
            }
        }

        private void WriteCurrency(decimal value, int length, TdsParserStateObject stateObj)
        {
            SqlMoney money = new SqlMoney(value);
            int[] bits = decimal.GetBits(money.Value);
            bool flag = 0 != (bits[3] & -2147483648);
            long num = (long) ((((ulong) bits[1]) << 0x20) | ((ulong) bits[0]));
            if (flag)
            {
                num = -num;
            }
            if (length == 4)
            {
                if ((value < TdsEnums.SQL_SMALL_MONEY_MIN) || (value > TdsEnums.SQL_SMALL_MONEY_MAX))
                {
                    throw SQL.MoneyOverflow(value.ToString(CultureInfo.InvariantCulture));
                }
                this.WriteInt((int) num, stateObj);
            }
            else
            {
                this.WriteInt((int) (num >> 0x20), stateObj);
                this.WriteInt((int) num, stateObj);
            }
        }

        private void WriteDate(DateTime value, TdsParserStateObject stateObj)
        {
            int days = value.Subtract(DateTime.MinValue).Days;
            this.WriteByteArray(BitConverter.GetBytes(days), 3, 0, stateObj);
        }

        private void WriteDateTime2(DateTime value, byte scale, int length, TdsParserStateObject stateObj)
        {
            long num = value.TimeOfDay.Ticks / TdsEnums.TICKS_FROM_SCALE[scale];
            this.WriteByteArray(BitConverter.GetBytes(num), length - 3, 0, stateObj);
            this.WriteDate(value, stateObj);
        }

        private void WriteDateTimeOffset(DateTimeOffset value, byte scale, int length, TdsParserStateObject stateObj)
        {
            this.WriteDateTime2(value.UtcDateTime, scale, length - 2, stateObj);
            short totalMinutes = (short) value.Offset.TotalMinutes;
            this.WriteByte((byte) (totalMinutes & 0xff), stateObj);
            this.WriteByte((byte) ((totalMinutes >> 8) & 0xff), stateObj);
        }

        private void WriteDecimal(decimal value, TdsParserStateObject stateObj)
        {
            stateObj._decimalBits = decimal.GetBits(value);
            if (0x80000000L == (stateObj._decimalBits[3] & 0x80000000L))
            {
                this.WriteByte(0, stateObj);
            }
            else
            {
                this.WriteByte(1, stateObj);
            }
            this.WriteInt(stateObj._decimalBits[0], stateObj);
            this.WriteInt(stateObj._decimalBits[1], stateObj);
            this.WriteInt(stateObj._decimalBits[2], stateObj);
            this.WriteInt(0, stateObj);
        }

        internal void WriteDouble(double v, TdsParserStateObject stateObj)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            this.WriteByteArray(bytes, bytes.Length, 0, stateObj);
        }

        private void WriteEncodingChar(string s, Encoding encoding, TdsParserStateObject stateObj)
        {
            this.WriteEncodingChar(s, s.Length, 0, encoding, stateObj);
        }

        private void WriteEncodingChar(string s, int numChars, int offset, Encoding encoding, TdsParserStateObject stateObj)
        {
            if (encoding == null)
            {
                encoding = this._defaultEncoding;
            }
            char[] chars = s.ToCharArray(offset, numChars);
            byte[] b = encoding.GetBytes(chars, 0, numChars);
            this.WriteByteArray(b, b.Length, 0, stateObj);
        }

        internal void WriteFloat(float v, TdsParserStateObject stateObj)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            this.WriteByteArray(bytes, bytes.Length, 0, stateObj);
        }

        private void WriteIdentifier(string s, TdsParserStateObject stateObj)
        {
            if (s != null)
            {
                this.WriteByte((byte) s.Length, stateObj);
                this.WriteString(s, stateObj);
            }
            else
            {
                this.WriteByte(0, stateObj);
            }
        }

        private void WriteIdentifierWithShortLength(string s, TdsParserStateObject stateObj)
        {
            if (s != null)
            {
                this.WriteShort((short) s.Length, stateObj);
                this.WriteString(s, stateObj);
            }
            else
            {
                this.WriteShort(0, stateObj);
            }
        }

        internal void WriteInt(int v, TdsParserStateObject stateObj)
        {
            this.WriteByteArray(BitConverter.GetBytes(v), 4, 0, stateObj);
        }

        internal void WriteLong(long v, TdsParserStateObject stateObj)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            this.WriteByteArray(bytes, bytes.Length, 0, stateObj);
        }

        private void WriteMarsHeader(TdsParserStateObject stateObj, SqlInternalTransaction transaction)
        {
            this.WriteUnsignedInt(0x16, stateObj);
            this.WriteUnsignedInt(0x12, stateObj);
            this.WriteShort(2, stateObj);
            if ((transaction != null) && (0L != transaction.TransactionId))
            {
                this.WriteLong(transaction.TransactionId, stateObj);
                this.WriteInt(stateObj.IncrementAndObtainOpenResultCount(transaction), stateObj);
            }
            else
            {
                this.WriteLong(this._retainedTransactionId, stateObj);
                this.WriteInt(stateObj.IncrementAndObtainOpenResultCount(null), stateObj);
            }
        }

        private void WriteParameterName(string parameterName, TdsParserStateObject stateObj)
        {
            if (!ADP.IsEmpty(parameterName))
            {
                int length = parameterName.Length & 0xff;
                this.WriteByte((byte) length, stateObj);
                this.WriteString(parameterName, length, 0, stateObj);
            }
            else
            {
                this.WriteByte(0, stateObj);
            }
        }

        internal void WriteParameterVarLen(MetaType type, int size, bool isNull, TdsParserStateObject stateObj)
        {
            if (type.IsLong)
            {
                if (isNull)
                {
                    if (type.IsPlp)
                    {
                        this.WriteLong(-1L, stateObj);
                    }
                    else
                    {
                        this.WriteInt(-1, stateObj);
                    }
                }
                else if (type.NullableType == 0xf1)
                {
                    this.WriteUnsignedLong(18446744073709551614L, stateObj);
                }
                else if (type.IsPlp)
                {
                    this.WriteLong((long) size, stateObj);
                }
                else
                {
                    this.WriteInt(size, stateObj);
                }
            }
            else if (type.IsVarTime)
            {
                if (isNull)
                {
                    this.WriteByte(0, stateObj);
                }
                else
                {
                    this.WriteByte((byte) size, stateObj);
                }
            }
            else if (!type.IsFixed)
            {
                if (isNull)
                {
                    this.WriteShort(0xffff, stateObj);
                }
                else
                {
                    this.WriteShort(size, stateObj);
                }
            }
            else if (isNull)
            {
                this.WriteByte(0, stateObj);
            }
            else
            {
                this.WriteByte((byte) (type.FixedLength & 0xff), stateObj);
            }
        }

        private void WriteQueryNotificationHeader(SqlNotificationRequest notificationRequest, TdsParserStateObject stateObj)
        {
            if (notificationRequest != null)
            {
                string userData = notificationRequest.UserData;
                string options = notificationRequest.Options;
                int timeout = notificationRequest.Timeout;
                if (userData == null)
                {
                    throw ADP.ArgumentNull("CallbackId");
                }
                if (0xffff < userData.Length)
                {
                    throw ADP.ArgumentOutOfRange("CallbackId");
                }
                if (options == null)
                {
                    throw ADP.ArgumentNull("Service");
                }
                if (0xffff < options.Length)
                {
                    throw ADP.ArgumentOutOfRange("Service");
                }
                if (-1 > timeout)
                {
                    throw ADP.ArgumentOutOfRange("Timeout");
                }
                Bid.NotificationsTrace("<sc.TdsParser.WriteQueryNotificationHeader|DEP> NotificationRequest: userData: '%ls', options: '%ls', timeout: '%d'\n", notificationRequest.UserData, notificationRequest.Options, notificationRequest.Timeout);
                int v = ((8 + (userData.Length * 2)) + 2) + (options.Length * 2);
                if (timeout > 0)
                {
                    v += 4;
                }
                int num4 = (v + stateObj._outBytesUsed) - 8;
                int num3 = stateObj._outBytesUsed;
                stateObj._outBytesUsed = 8;
                this.WriteInt(num4, stateObj);
                stateObj._outBytesUsed = num3;
                this.WriteInt(v, stateObj);
                this.WriteShort(1, stateObj);
                this.WriteShort(userData.Length * 2, stateObj);
                this.WriteString(userData, stateObj);
                this.WriteShort(options.Length * 2, stateObj);
                this.WriteString(options, stateObj);
                if (timeout > 0)
                {
                    this.WriteInt(timeout, stateObj);
                }
            }
        }

        internal void WriteShort(int v, TdsParserStateObject stateObj)
        {
            if ((stateObj._outBytesUsed + 2) > stateObj._outBuff.Length)
            {
                this.WriteByte((byte) (v & 0xff), stateObj);
                this.WriteByte((byte) ((v >> 8) & 0xff), stateObj);
            }
            else
            {
                stateObj._outBuff[stateObj._outBytesUsed++] = (byte) (v & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed++] = (byte) ((v >> 8) & 0xff);
            }
        }

        private void WriteSmiParameter(SqlParameter param, int paramIndex, bool sendDefault, TdsParserStateObject stateObj)
        {
            object coercedValue;
            ExtendedClrTypeCode iEnumerableOfSqlDataRecord;
            ParameterPeekAheadValue value2;
            SmiParameterMetaData metaData = param.MetaDataForSmi(out value2);
            if (!this._isKatmai)
            {
                throw ADP.VersionDoesNotSupportDataType(MetaType.GetMetaTypeFromSqlDbType(metaData.SqlDbType, metaData.IsMultiValued).TypeName);
            }
            if (sendDefault)
            {
                if ((SqlDbType.Structured == metaData.SqlDbType) && metaData.IsMultiValued)
                {
                    coercedValue = __tvpEmptyValue;
                    iEnumerableOfSqlDataRecord = ExtendedClrTypeCode.IEnumerableOfSqlDataRecord;
                }
                else
                {
                    coercedValue = null;
                    iEnumerableOfSqlDataRecord = ExtendedClrTypeCode.DBNull;
                }
            }
            else if (param.Direction == ParameterDirection.Output)
            {
                bool paramaterIsSqlType = param.ParamaterIsSqlType;
                param.Value = null;
                coercedValue = null;
                iEnumerableOfSqlDataRecord = ExtendedClrTypeCode.DBNull;
                param.ParamaterIsSqlType = paramaterIsSqlType;
            }
            else
            {
                coercedValue = param.GetCoercedValue();
                iEnumerableOfSqlDataRecord = MetaDataUtilsSmi.DetermineExtendedTypeCodeForUseWithSqlDbType(metaData.SqlDbType, metaData.IsMultiValued, coercedValue, null, 210L);
            }
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.TdsParser.WriteSmiParameter|ADV> %d#, Sending parameter '%ls', default flag=%d, metadata:\n", this.ObjectID, param.ParameterName, sendDefault ? 1 : 0);
                Bid.PutStr(metaData.TraceString(3));
                Bid.Trace("\n");
            }
            this.WriteSmiParameterMetaData(metaData, sendDefault, stateObj);
            TdsParameterSetter setters = new TdsParameterSetter(stateObj, metaData);
            ValueUtilsSmi.SetCompatibleValueV200(new SmiEventSink_Default(), setters, 0, metaData, coercedValue, iEnumerableOfSqlDataRecord, param.Offset, (0 < param.Size) ? param.Size : -1, value2);
        }

        private void WriteSmiParameterMetaData(SmiParameterMetaData metaData, bool sendDefault, TdsParserStateObject stateObj)
        {
            byte b = 0;
            if ((ParameterDirection.Output == metaData.Direction) || (ParameterDirection.InputOutput == metaData.Direction))
            {
                b = (byte) (b | 1);
            }
            if (sendDefault)
            {
                b = (byte) (b | 2);
            }
            this.WriteParameterName(metaData.Name, stateObj);
            this.WriteByte(b, stateObj);
            this.WriteSmiTypeInfo(metaData, stateObj);
        }

        private void WriteSmiTypeInfo(SmiExtendedMetaData metaData, TdsParserStateObject stateObj)
        {
            switch (metaData.SqlDbType)
            {
                case SqlDbType.BigInt:
                    this.WriteByte(0x26, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.Binary:
                    this.WriteByte(0xad, stateObj);
                    this.WriteUnsignedShort((ushort) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.Bit:
                    this.WriteByte(0x68, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.Char:
                    this.WriteByte(0xaf, stateObj);
                    this.WriteUnsignedShort((ushort) metaData.MaxLength, stateObj);
                    this.WriteUnsignedInt(this._defaultCollation.info, stateObj);
                    this.WriteByte(this._defaultCollation.sortId, stateObj);
                    return;

                case SqlDbType.DateTime:
                    this.WriteByte(0x6f, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.Decimal:
                    this.WriteByte(0x6c, stateObj);
                    this.WriteByte((byte) MetaType.MetaDecimal.FixedLength, stateObj);
                    this.WriteByte((metaData.Precision == 0) ? ((byte) 1) : metaData.Precision, stateObj);
                    this.WriteByte(metaData.Scale, stateObj);
                    return;

                case SqlDbType.Float:
                    this.WriteByte(0x6d, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.Image:
                    this.WriteByte(0xa5, stateObj);
                    this.WriteUnsignedShort(0xffff, stateObj);
                    return;

                case SqlDbType.Int:
                    this.WriteByte(0x26, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.Money:
                    this.WriteByte(110, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.NChar:
                    this.WriteByte(0xef, stateObj);
                    this.WriteUnsignedShort((ushort) (metaData.MaxLength * 2L), stateObj);
                    this.WriteUnsignedInt(this._defaultCollation.info, stateObj);
                    this.WriteByte(this._defaultCollation.sortId, stateObj);
                    return;

                case SqlDbType.NText:
                    this.WriteByte(0xe7, stateObj);
                    this.WriteUnsignedShort(0xffff, stateObj);
                    this.WriteUnsignedInt(this._defaultCollation.info, stateObj);
                    this.WriteByte(this._defaultCollation.sortId, stateObj);
                    return;

                case SqlDbType.NVarChar:
                    this.WriteByte(0xe7, stateObj);
                    if (-1L != metaData.MaxLength)
                    {
                        this.WriteUnsignedShort((ushort) (metaData.MaxLength * 2L), stateObj);
                        break;
                    }
                    this.WriteUnsignedShort(0xffff, stateObj);
                    break;

                case SqlDbType.Real:
                    this.WriteByte(0x6d, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.UniqueIdentifier:
                    this.WriteByte(0x24, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.SmallDateTime:
                    this.WriteByte(0x6f, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.SmallInt:
                    this.WriteByte(0x26, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.SmallMoney:
                    this.WriteByte(110, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.Text:
                    this.WriteByte(0xa7, stateObj);
                    this.WriteUnsignedShort(0xffff, stateObj);
                    this.WriteUnsignedInt(this._defaultCollation.info, stateObj);
                    this.WriteByte(this._defaultCollation.sortId, stateObj);
                    return;

                case SqlDbType.Timestamp:
                    this.WriteByte(0xad, stateObj);
                    this.WriteShort((int) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.TinyInt:
                    this.WriteByte(0x26, stateObj);
                    this.WriteByte((byte) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.VarBinary:
                    this.WriteByte(0xa5, stateObj);
                    this.WriteUnsignedShort((ushort) metaData.MaxLength, stateObj);
                    return;

                case SqlDbType.VarChar:
                    this.WriteByte(0xa7, stateObj);
                    this.WriteUnsignedShort((ushort) metaData.MaxLength, stateObj);
                    this.WriteUnsignedInt(this._defaultCollation.info, stateObj);
                    this.WriteByte(this._defaultCollation.sortId, stateObj);
                    return;

                case SqlDbType.Variant:
                    this.WriteByte(0x62, stateObj);
                    this.WriteInt((int) metaData.MaxLength, stateObj);
                    return;

                case (SqlDbType.SmallInt | SqlDbType.Int):
                case (SqlDbType.Text | SqlDbType.Int):
                case (SqlDbType.Xml | SqlDbType.Bit):
                case (SqlDbType.TinyInt | SqlDbType.Int):
                    return;

                case SqlDbType.Xml:
                    this.WriteByte(0xf1, stateObj);
                    if ((!ADP.IsEmpty(metaData.TypeSpecificNamePart1) || !ADP.IsEmpty(metaData.TypeSpecificNamePart2)) || !ADP.IsEmpty(metaData.TypeSpecificNamePart3))
                    {
                        this.WriteByte(1, stateObj);
                        this.WriteIdentifier(metaData.TypeSpecificNamePart1, stateObj);
                        this.WriteIdentifier(metaData.TypeSpecificNamePart2, stateObj);
                        this.WriteIdentifierWithShortLength(metaData.TypeSpecificNamePart3, stateObj);
                        return;
                    }
                    this.WriteByte(0, stateObj);
                    return;

                case SqlDbType.Udt:
                    this.WriteByte(240, stateObj);
                    this.WriteIdentifier(metaData.TypeSpecificNamePart1, stateObj);
                    this.WriteIdentifier(metaData.TypeSpecificNamePart2, stateObj);
                    this.WriteIdentifier(metaData.TypeSpecificNamePart3, stateObj);
                    return;

                case SqlDbType.Structured:
                    if (metaData.IsMultiValued)
                    {
                        this.WriteTvpTypeInfo(metaData, stateObj);
                    }
                    return;

                case SqlDbType.Date:
                    this.WriteByte(40, stateObj);
                    return;

                case SqlDbType.Time:
                    this.WriteByte(0x29, stateObj);
                    this.WriteByte(metaData.Scale, stateObj);
                    return;

                case SqlDbType.DateTime2:
                    this.WriteByte(0x2a, stateObj);
                    this.WriteByte(metaData.Scale, stateObj);
                    return;

                case SqlDbType.DateTimeOffset:
                    this.WriteByte(0x2b, stateObj);
                    this.WriteByte(metaData.Scale, stateObj);
                    return;

                default:
                    return;
            }
            this.WriteUnsignedInt(this._defaultCollation.info, stateObj);
            this.WriteByte(this._defaultCollation.sortId, stateObj);
        }

        internal void WriteSqlDecimal(SqlDecimal d, TdsParserStateObject stateObj)
        {
            if (d.IsPositive)
            {
                this.WriteByte(1, stateObj);
            }
            else
            {
                this.WriteByte(0, stateObj);
            }
            int[] data = d.Data;
            this.WriteInt(data[0], stateObj);
            this.WriteInt(data[1], stateObj);
            this.WriteInt(data[2], stateObj);
            this.WriteInt(data[3], stateObj);
        }

        private void WriteSqlMoney(SqlMoney value, int length, TdsParserStateObject stateObj)
        {
            int[] bits = decimal.GetBits(value.Value);
            bool flag = 0 != (bits[3] & -2147483648);
            long num = (long) ((((ulong) bits[1]) << 0x20) | ((ulong) bits[0]));
            if (flag)
            {
                num = -num;
            }
            if (length == 4)
            {
                decimal num2 = value.Value;
                if ((num2 < TdsEnums.SQL_SMALL_MONEY_MIN) || (num2 > TdsEnums.SQL_SMALL_MONEY_MAX))
                {
                    throw SQL.MoneyOverflow(num2.ToString(CultureInfo.InvariantCulture));
                }
                this.WriteInt((int) num, stateObj);
            }
            else
            {
                this.WriteInt((int) (num >> 0x20), stateObj);
                this.WriteInt((int) num, stateObj);
            }
        }

        private void WriteSqlValue(object value, MetaType type, int actualLength, int codePageByteSize, int offset, TdsParserStateObject stateObj)
        {
            switch (type.NullableType)
            {
                case 0x22:
                case 0xa5:
                case 0xad:
                    if (type.IsPlp)
                    {
                        this.WriteInt(actualLength, stateObj);
                    }
                    if (value is SqlBinary)
                    {
                        SqlBinary binary = (SqlBinary) value;
                        this.WriteByteArray(binary.Value, actualLength, offset, stateObj);
                    }
                    else
                    {
                        this.WriteByteArray(((SqlBytes) value).Value, actualLength, offset, stateObj);
                    }
                    break;

                case 0x23:
                case 0xa7:
                case 0xaf:
                    if (type.IsPlp)
                    {
                        this.WriteInt(codePageByteSize, stateObj);
                    }
                    if (value is SqlChars)
                    {
                        string s = new string(((SqlChars) value).Value);
                        this.WriteEncodingChar(s, actualLength, offset, this._defaultEncoding, stateObj);
                    }
                    else
                    {
                        SqlString str3 = (SqlString) value;
                        this.WriteEncodingChar(str3.Value, actualLength, offset, this._defaultEncoding, stateObj);
                    }
                    break;

                case 0x24:
                {
                    byte[] b = ((SqlGuid) value).ToByteArray();
                    this.WriteByteArray(b, actualLength, 0, stateObj);
                    break;
                }
                case 0x26:
                {
                    if (type.FixedLength != 1)
                    {
                        if (type.FixedLength == 2)
                        {
                            SqlInt16 num4 = (SqlInt16) value;
                            this.WriteShort(num4.Value, stateObj);
                        }
                        else if (type.FixedLength == 4)
                        {
                            SqlInt32 num3 = (SqlInt32) value;
                            this.WriteInt(num3.Value, stateObj);
                        }
                        else
                        {
                            SqlInt64 num2 = (SqlInt64) value;
                            this.WriteLong(num2.Value, stateObj);
                        }
                        break;
                    }
                    SqlByte num5 = (SqlByte) value;
                    this.WriteByte(num5.Value, stateObj);
                    break;
                }
                case 0x63:
                case 0xef:
                case 0xf1:
                case 0xe7:
                    if (type.IsPlp)
                    {
                        if (this.IsBOMNeeded(type, value))
                        {
                            this.WriteInt(actualLength + 2, stateObj);
                            this.WriteShort(0xfeff, stateObj);
                        }
                        else
                        {
                            this.WriteInt(actualLength, stateObj);
                        }
                    }
                    if (actualLength != 0)
                    {
                        actualLength = actualLength >> 1;
                    }
                    if (value is SqlChars)
                    {
                        this.WriteCharArray(((SqlChars) value).Value, actualLength, offset, stateObj);
                    }
                    else
                    {
                        SqlString str2 = (SqlString) value;
                        this.WriteString(str2.Value, actualLength, offset, stateObj);
                    }
                    break;

                case 0x68:
                {
                    SqlBoolean flag = (SqlBoolean) value;
                    if (!flag.Value)
                    {
                        this.WriteByte(0, stateObj);
                        break;
                    }
                    this.WriteByte(1, stateObj);
                    break;
                }
                case 0x6c:
                    this.WriteSqlDecimal((SqlDecimal) value, stateObj);
                    break;

                case 0x6d:
                {
                    if (type.FixedLength != 4)
                    {
                        SqlDouble num6 = (SqlDouble) value;
                        this.WriteDouble(num6.Value, stateObj);
                        break;
                    }
                    SqlSingle num7 = (SqlSingle) value;
                    this.WriteFloat(num7.Value, stateObj);
                    break;
                }
                case 110:
                    this.WriteSqlMoney((SqlMoney) value, type.FixedLength, stateObj);
                    break;

                case 0x6f:
                {
                    SqlDateTime time = (SqlDateTime) value;
                    if (type.FixedLength == 4)
                    {
                        if ((0 > time.DayTicks) || (time.DayTicks > 0xffff))
                        {
                            throw SQL.SmallDateTimeOverflow(time.ToString());
                        }
                        this.WriteShort(time.DayTicks, stateObj);
                        this.WriteShort(time.TimeTicks / SqlDateTime.SQLTicksPerMinute, stateObj);
                    }
                    else
                    {
                        this.WriteInt(time.DayTicks, stateObj);
                        this.WriteInt(time.TimeTicks, stateObj);
                    }
                    break;
                }
                case 240:
                    throw SQL.UDTUnexpectedResult(value.GetType().AssemblyQualifiedName);
            }
            if (type.IsPlp && (actualLength > 0))
            {
                this.WriteInt(0, stateObj);
            }
        }

        internal void WriteSqlVariantDataRowValue(object value, TdsParserStateObject stateObj)
        {
            if ((value == null) || (DBNull.Value == value))
            {
                this.WriteInt(0, stateObj);
            }
            else
            {
                MetaType metaTypeFromValue = MetaType.GetMetaTypeFromValue(value);
                int numChars = 0;
                if (metaTypeFromValue.IsAnsiType)
                {
                    numChars = this.GetEncodingCharLength((string) value, numChars, 0, this._defaultEncoding);
                }
                byte tDSType = metaTypeFromValue.TDSType;
                if (tDSType <= 0x3e)
                {
                    switch (tDSType)
                    {
                        case 0x29:
                            this.WriteSqlVariantHeader(8, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteByte(metaTypeFromValue.Scale, stateObj);
                            this.WriteTime((TimeSpan) value, metaTypeFromValue.Scale, 5, stateObj);
                            return;

                        case 0x2a:
                        case 0x31:
                        case 0x33:
                        case 0x35:
                        case 0x36:
                        case 0x37:
                        case 0x39:
                        case 0x3a:
                            return;

                        case 0x2b:
                            this.WriteSqlVariantHeader(13, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteByte(metaTypeFromValue.Scale, stateObj);
                            this.WriteDateTimeOffset((DateTimeOffset) value, metaTypeFromValue.Scale, 10, stateObj);
                            return;

                        case 0x24:
                        {
                            byte[] b = ((Guid) value).ToByteArray();
                            numChars = b.Length;
                            this.WriteSqlVariantHeader(0x12, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteByteArray(b, numChars, 0, stateObj);
                            return;
                        }
                        case 0x30:
                            this.WriteSqlVariantHeader(3, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteByte((byte) value, stateObj);
                            return;

                        case 50:
                            this.WriteSqlVariantHeader(3, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            if (!((bool) value))
                            {
                                this.WriteByte(0, stateObj);
                                return;
                            }
                            this.WriteByte(1, stateObj);
                            return;

                        case 0x34:
                            this.WriteSqlVariantHeader(4, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteShort((short) value, stateObj);
                            return;

                        case 0x38:
                            this.WriteSqlVariantHeader(6, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteInt((int) value, stateObj);
                            return;

                        case 0x3b:
                            this.WriteSqlVariantHeader(6, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteFloat((float) value, stateObj);
                            return;

                        case 60:
                            this.WriteSqlVariantHeader(10, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteCurrency((decimal) value, 8, stateObj);
                            return;

                        case 0x3d:
                        {
                            TdsDateTime time = MetaType.FromDateTime((DateTime) value, 8);
                            this.WriteSqlVariantHeader(10, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteInt(time.days, stateObj);
                            this.WriteInt(time.time, stateObj);
                            return;
                        }
                        case 0x3e:
                            this.WriteSqlVariantHeader(10, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteDouble((double) value, stateObj);
                            return;
                    }
                }
                else
                {
                    if (tDSType <= 0x7f)
                    {
                        if (tDSType == 0x6c)
                        {
                            this.WriteSqlVariantHeader(0x15, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                            this.WriteByte(metaTypeFromValue.Precision, stateObj);
                            this.WriteByte((byte) ((decimal.GetBits((decimal) value)[3] & 0xff0000) >> 0x10), stateObj);
                            this.WriteDecimal((decimal) value, stateObj);
                            return;
                        }
                        if (tDSType != 0x7f)
                        {
                            return;
                        }
                    }
                    else
                    {
                        switch (tDSType)
                        {
                            case 0xa5:
                            {
                                byte[] buffer2 = (byte[]) value;
                                numChars = buffer2.Length;
                                this.WriteSqlVariantHeader(4 + numChars, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                                this.WriteShort(numChars, stateObj);
                                this.WriteByteArray(buffer2, numChars, 0, stateObj);
                                return;
                            }
                            case 0xa6:
                                return;

                            case 0xa7:
                            {
                                string s = (string) value;
                                numChars = s.Length;
                                this.WriteSqlVariantHeader(9 + numChars, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                                this.WriteUnsignedInt(this._defaultCollation.info, stateObj);
                                this.WriteByte(this._defaultCollation.sortId, stateObj);
                                this.WriteShort(numChars, stateObj);
                                this.WriteEncodingChar(s, this._defaultEncoding, stateObj);
                                return;
                            }
                            case 0xe7:
                            {
                                string str = (string) value;
                                numChars = str.Length * 2;
                                this.WriteSqlVariantHeader(9 + numChars, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                                this.WriteUnsignedInt(this._defaultCollation.info, stateObj);
                                this.WriteByte(this._defaultCollation.sortId, stateObj);
                                this.WriteShort(numChars, stateObj);
                                numChars = numChars >> 1;
                                this.WriteString(str, numChars, 0, stateObj);
                                return;
                            }
                        }
                        return;
                    }
                    this.WriteSqlVariantHeader(10, metaTypeFromValue.TDSType, metaTypeFromValue.PropBytes, stateObj);
                    this.WriteLong((long) value, stateObj);
                }
            }
        }

        internal void WriteSqlVariantHeader(int length, byte tdstype, byte propbytes, TdsParserStateObject stateObj)
        {
            this.WriteInt(length, stateObj);
            this.WriteByte(tdstype, stateObj);
            this.WriteByte(propbytes, stateObj);
        }

        internal void WriteSqlVariantValue(object value, int length, int offset, TdsParserStateObject stateObj)
        {
            if (ADP.IsNull(value))
            {
                this.WriteInt(0, stateObj);
                this.WriteInt(0, stateObj);
            }
            else
            {
                MetaType metaTypeFromValue = MetaType.GetMetaTypeFromValue(value);
                if ((0x6c == metaTypeFromValue.TDSType) && (8 == length))
                {
                    metaTypeFromValue = MetaType.GetMetaTypeFromValue(new SqlMoney((decimal) value));
                }
                if (metaTypeFromValue.IsAnsiType)
                {
                    length = this.GetEncodingCharLength((string) value, length, 0, this._defaultEncoding);
                }
                this.WriteInt((2 + metaTypeFromValue.PropBytes) + length, stateObj);
                this.WriteInt((2 + metaTypeFromValue.PropBytes) + length, stateObj);
                this.WriteByte(metaTypeFromValue.TDSType, stateObj);
                this.WriteByte(metaTypeFromValue.PropBytes, stateObj);
                byte tDSType = metaTypeFromValue.TDSType;
                if (tDSType <= 0x3e)
                {
                    switch (tDSType)
                    {
                        case 0x29:
                            this.WriteByte(metaTypeFromValue.Scale, stateObj);
                            this.WriteTime((TimeSpan) value, metaTypeFromValue.Scale, length, stateObj);
                            return;

                        case 0x2a:
                        case 0x31:
                        case 0x33:
                        case 0x35:
                        case 0x36:
                        case 0x37:
                        case 0x39:
                        case 0x3a:
                            return;

                        case 0x2b:
                            this.WriteByte(metaTypeFromValue.Scale, stateObj);
                            this.WriteDateTimeOffset((DateTimeOffset) value, metaTypeFromValue.Scale, length, stateObj);
                            return;

                        case 0x24:
                        {
                            byte[] b = ((Guid) value).ToByteArray();
                            this.WriteByteArray(b, length, 0, stateObj);
                            return;
                        }
                        case 0x30:
                            this.WriteByte((byte) value, stateObj);
                            return;

                        case 50:
                            if (!((bool) value))
                            {
                                this.WriteByte(0, stateObj);
                                return;
                            }
                            this.WriteByte(1, stateObj);
                            return;

                        case 0x34:
                            this.WriteShort((short) value, stateObj);
                            return;

                        case 0x38:
                            this.WriteInt((int) value, stateObj);
                            return;

                        case 0x3b:
                            this.WriteFloat((float) value, stateObj);
                            return;

                        case 60:
                            this.WriteCurrency((decimal) value, 8, stateObj);
                            return;

                        case 0x3d:
                        {
                            TdsDateTime time = MetaType.FromDateTime((DateTime) value, 8);
                            this.WriteInt(time.days, stateObj);
                            this.WriteInt(time.time, stateObj);
                            return;
                        }
                        case 0x3e:
                            this.WriteDouble((double) value, stateObj);
                            return;
                    }
                }
                else
                {
                    if (tDSType <= 0x7f)
                    {
                        if (tDSType == 0x6c)
                        {
                            this.WriteByte(metaTypeFromValue.Precision, stateObj);
                            this.WriteByte((byte) ((decimal.GetBits((decimal) value)[3] & 0xff0000) >> 0x10), stateObj);
                            this.WriteDecimal((decimal) value, stateObj);
                            return;
                        }
                        if (tDSType != 0x7f)
                        {
                            return;
                        }
                    }
                    else
                    {
                        switch (tDSType)
                        {
                            case 0xa5:
                            {
                                byte[] buffer2 = (byte[]) value;
                                this.WriteShort(length, stateObj);
                                this.WriteByteArray(buffer2, length, offset, stateObj);
                                return;
                            }
                            case 0xa6:
                                return;

                            case 0xa7:
                            {
                                string s = (string) value;
                                this.WriteUnsignedInt(this._defaultCollation.info, stateObj);
                                this.WriteByte(this._defaultCollation.sortId, stateObj);
                                this.WriteShort(length, stateObj);
                                this.WriteEncodingChar(s, this._defaultEncoding, stateObj);
                                return;
                            }
                            case 0xe7:
                            {
                                string str = (string) value;
                                this.WriteUnsignedInt(this._defaultCollation.info, stateObj);
                                this.WriteByte(this._defaultCollation.sortId, stateObj);
                                this.WriteShort(length, stateObj);
                                length = length >> 1;
                                this.WriteString(str, length, offset, stateObj);
                                return;
                            }
                        }
                        return;
                    }
                    this.WriteLong((long) value, stateObj);
                }
            }
        }

        private void WriteString(string s, TdsParserStateObject stateObj)
        {
            this.WriteString(s, s.Length, 0, stateObj);
        }

        internal void WriteString(string s, int length, int offset, TdsParserStateObject stateObj)
        {
            int len = ADP.CharSize * length;
            if (len < (stateObj._outBuff.Length - stateObj._outBytesUsed))
            {
                CopyStringToBytes(s, offset, stateObj._outBuff, stateObj._outBytesUsed, length);
                stateObj._outBytesUsed += len;
            }
            else
            {
                if ((stateObj._bTmp == null) || (stateObj._bTmp.Length < len))
                {
                    stateObj._bTmp = new byte[len];
                }
                CopyStringToBytes(s, offset, stateObj._bTmp, 0, length);
                this.WriteByteArray(stateObj._bTmp, len, 0, stateObj);
            }
        }

        private void WriteTime(TimeSpan value, byte scale, int length, TdsParserStateObject stateObj)
        {
            if ((0L > value.Ticks) || (value.Ticks >= 0xc92a69c000L))
            {
                throw SQL.TimeOverflow(value.ToString());
            }
            long num = value.Ticks / TdsEnums.TICKS_FROM_SCALE[scale];
            this.WriteByteArray(BitConverter.GetBytes(num), length, 0, stateObj);
        }

        private void WriteTokenLength(byte token, int length, TdsParserStateObject stateObj)
        {
            int num = 0;
            if (this._isYukon)
            {
                if (240 == token)
                {
                    num = 8;
                }
                else if (token == 0xf1)
                {
                    num = 8;
                }
            }
            if (num == 0)
            {
                switch ((token & 0x30))
                {
                    case 0x20:
                    case 0:
                        if ((token & 0x80) != 0)
                        {
                            num = 2;
                        }
                        else if ((token & 12) == 0)
                        {
                            num = 4;
                        }
                        else
                        {
                            num = 1;
                        }
                        break;

                    case 0x30:
                        num = 0;
                        break;

                    case 0x10:
                        num = 0;
                        break;
                }
                switch (num)
                {
                    case 1:
                        this.WriteByte((byte) length, stateObj);
                        return;

                    case 2:
                        this.WriteShort(length, stateObj);
                        return;

                    case 3:
                        break;

                    case 4:
                        this.WriteInt(length, stateObj);
                        return;

                    case 8:
                        this.WriteShort(0xffff, stateObj);
                        break;

                    default:
                        return;
                }
            }
        }

        private void WriteTvpColumnMetaData(SmiExtendedMetaData md, bool isDefault, TdsParserStateObject stateObj)
        {
            if (SqlDbType.Timestamp == md.SqlDbType)
            {
                this.WriteUnsignedInt(80, stateObj);
            }
            else
            {
                this.WriteUnsignedInt(0, stateObj);
            }
            ushort us = 1;
            if (isDefault)
            {
                us = (ushort) (us | 0x200);
            }
            this.WriteUnsignedShort(us, stateObj);
            this.WriteSmiTypeInfo(md, stateObj);
            this.WriteIdentifier(null, stateObj);
        }

        private void WriteTvpOrderUnique(SmiExtendedMetaData metaData, TdsParserStateObject stateObj)
        {
            SmiOrderProperty property2 = (SmiOrderProperty) metaData.ExtendedProperties[SmiPropertySelector.SortOrder];
            SmiUniqueKeyProperty property = (SmiUniqueKeyProperty) metaData.ExtendedProperties[SmiPropertySelector.UniqueKey];
            List<TdsOrderUnique> list = new List<TdsOrderUnique>(metaData.FieldMetaData.Count);
            for (int i = 0; i < metaData.FieldMetaData.Count; i++)
            {
                byte flags = 0;
                SmiOrderProperty.SmiColumnOrder order = property2[i];
                if (order.Order == SortOrder.Ascending)
                {
                    flags = 1;
                }
                else if (SortOrder.Descending == order.Order)
                {
                    flags = 2;
                }
                if (property[i])
                {
                    flags = (byte) (flags | 4);
                }
                if (flags != 0)
                {
                    list.Add(new TdsOrderUnique((short) (i + 1), flags));
                }
            }
            if (0 < list.Count)
            {
                this.WriteByte(0x10, stateObj);
                this.WriteShort(list.Count, stateObj);
                foreach (TdsOrderUnique unique in list)
                {
                    this.WriteShort(unique.ColumnOrdinal, stateObj);
                    this.WriteByte(unique.Flags, stateObj);
                }
            }
        }

        private void WriteTvpTypeInfo(SmiExtendedMetaData metaData, TdsParserStateObject stateObj)
        {
            this.WriteByte(0xf3, stateObj);
            this.WriteIdentifier(metaData.TypeSpecificNamePart1, stateObj);
            this.WriteIdentifier(metaData.TypeSpecificNamePart2, stateObj);
            this.WriteIdentifier(metaData.TypeSpecificNamePart3, stateObj);
            if (metaData.FieldMetaData.Count == 0)
            {
                this.WriteUnsignedShort(0xffff, stateObj);
            }
            else
            {
                this.WriteUnsignedShort((ushort) metaData.FieldMetaData.Count, stateObj);
                SmiDefaultFieldsProperty property = (SmiDefaultFieldsProperty) metaData.ExtendedProperties[SmiPropertySelector.DefaultFields];
                for (int i = 0; i < metaData.FieldMetaData.Count; i++)
                {
                    this.WriteTvpColumnMetaData(metaData.FieldMetaData[i], property[i], stateObj);
                }
                this.WriteTvpOrderUnique(metaData, stateObj);
            }
            this.WriteByte(0, stateObj);
        }

        private void WriteUDTMetaData(object value, string database, string schema, string type, TdsParserStateObject stateObj)
        {
            if (ADP.IsEmpty(database))
            {
                this.WriteByte(0, stateObj);
            }
            else
            {
                this.WriteByte((byte) database.Length, stateObj);
                this.WriteString(database, stateObj);
            }
            if (ADP.IsEmpty(schema))
            {
                this.WriteByte(0, stateObj);
            }
            else
            {
                this.WriteByte((byte) schema.Length, stateObj);
                this.WriteString(schema, stateObj);
            }
            if (ADP.IsEmpty(type))
            {
                this.WriteByte(0, stateObj);
            }
            else
            {
                this.WriteByte((byte) type.Length, stateObj);
                this.WriteString(type, stateObj);
            }
        }

        internal void WriteUnsignedInt(uint i, TdsParserStateObject stateObj)
        {
            this.WriteByteArray(BitConverter.GetBytes(i), 4, 0, stateObj);
        }

        internal void WriteUnsignedLong(ulong uv, TdsParserStateObject stateObj)
        {
            byte[] bytes = BitConverter.GetBytes(uv);
            this.WriteByteArray(bytes, bytes.Length, 0, stateObj);
        }

        internal void WriteUnsignedShort(ushort us, TdsParserStateObject stateObj)
        {
            this.WriteShort((short) us, stateObj);
        }

        private void WriteValue(object value, MetaType type, byte scale, int actualLength, int encodingByteSize, int offset, TdsParserStateObject stateObj)
        {
            switch (type.NullableType)
            {
                case 0x22:
                case 240:
                case 0xa5:
                case 0xad:
                {
                    byte[] b = (byte[]) value;
                    if (type.IsPlp)
                    {
                        this.WriteInt(actualLength, stateObj);
                    }
                    this.WriteByteArray(b, actualLength, offset, stateObj);
                    break;
                }
                case 0x23:
                case 0xa7:
                case 0xaf:
                    if (type.IsPlp)
                    {
                        this.WriteInt(encodingByteSize, stateObj);
                    }
                    if (value is byte[])
                    {
                        this.WriteByteArray((byte[]) value, actualLength, 0, stateObj);
                    }
                    else
                    {
                        this.WriteEncodingChar((string) value, actualLength, offset, this._defaultEncoding, stateObj);
                    }
                    break;

                case 0x24:
                {
                    byte[] buffer = ((Guid) value).ToByteArray();
                    this.WriteByteArray(buffer, actualLength, 0, stateObj);
                    break;
                }
                case 0x26:
                    if (type.FixedLength != 1)
                    {
                        if (type.FixedLength == 2)
                        {
                            this.WriteShort((short) value, stateObj);
                        }
                        else if (type.FixedLength == 4)
                        {
                            this.WriteInt((int) value, stateObj);
                        }
                        else
                        {
                            this.WriteLong((long) value, stateObj);
                        }
                        break;
                    }
                    this.WriteByte((byte) value, stateObj);
                    break;

                case 40:
                    this.WriteDate((DateTime) value, stateObj);
                    break;

                case 0x29:
                    if (scale > 7)
                    {
                        throw SQL.TimeScaleValueOutOfRange(scale);
                    }
                    this.WriteTime((TimeSpan) value, scale, actualLength, stateObj);
                    break;

                case 0x2a:
                    if (scale > 7)
                    {
                        throw SQL.TimeScaleValueOutOfRange(scale);
                    }
                    this.WriteDateTime2((DateTime) value, scale, actualLength, stateObj);
                    break;

                case 0x2b:
                    this.WriteDateTimeOffset((DateTimeOffset) value, scale, actualLength, stateObj);
                    break;

                case 0x63:
                case 0xef:
                case 0xf1:
                case 0xe7:
                    if (type.IsPlp)
                    {
                        if (this.IsBOMNeeded(type, value))
                        {
                            this.WriteInt(actualLength + 2, stateObj);
                            this.WriteShort(0xfeff, stateObj);
                        }
                        else
                        {
                            this.WriteInt(actualLength, stateObj);
                        }
                    }
                    if (value is byte[])
                    {
                        this.WriteByteArray((byte[]) value, actualLength, 0, stateObj);
                    }
                    else
                    {
                        actualLength = actualLength >> 1;
                        this.WriteString((string) value, actualLength, offset, stateObj);
                    }
                    break;

                case 0x68:
                    if (!((bool) value))
                    {
                        this.WriteByte(0, stateObj);
                        break;
                    }
                    this.WriteByte(1, stateObj);
                    break;

                case 0x6c:
                    this.WriteDecimal((decimal) value, stateObj);
                    break;

                case 0x6d:
                    if (type.FixedLength != 4)
                    {
                        this.WriteDouble((double) value, stateObj);
                        break;
                    }
                    this.WriteFloat((float) value, stateObj);
                    break;

                case 110:
                    this.WriteCurrency((decimal) value, type.FixedLength, stateObj);
                    break;

                case 0x6f:
                {
                    TdsDateTime time2 = MetaType.FromDateTime((DateTime) value, (byte) type.FixedLength);
                    if (type.FixedLength == 4)
                    {
                        if ((0 > time2.days) || (time2.days > 0xffff))
                        {
                            throw SQL.SmallDateTimeOverflow(MetaType.ToDateTime(time2.days, time2.time, 4).ToString(CultureInfo.InvariantCulture));
                        }
                        this.WriteShort(time2.days, stateObj);
                        this.WriteShort(time2.time, stateObj);
                    }
                    else
                    {
                        this.WriteInt(time2.days, stateObj);
                        this.WriteInt(time2.time, stateObj);
                    }
                    break;
                }
            }
            if (type.IsPlp && (actualLength > 0))
            {
                this.WriteInt(0, stateObj);
            }
        }

        internal bool AsyncOn
        {
            get
            {
                return this._fAsync;
            }
        }

        internal SqlInternalConnectionTds Connection
        {
            get
            {
                return this._connHandler;
            }
        }

        internal SqlInternalTransaction CurrentTransaction
        {
            get
            {
                return this._currentTransaction;
            }
            set
            {
                if (((this._currentTransaction == null) && (value != null)) || ((this._currentTransaction != null) && (value == null)))
                {
                    this._currentTransaction = value;
                }
            }
        }

        internal int DefaultLCID
        {
            get
            {
                return this._defaultLCID;
            }
        }

        internal System.Data.SqlClient.EncryptionOptions EncryptionOptions
        {
            get
            {
                return this._encryptionOption;
            }
            set
            {
                this._encryptionOption = value;
            }
        }

        internal SqlErrorCollection Errors
        {
            get
            {
                lock (this._ErrorCollectionLock)
                {
                    if (this._errors == null)
                    {
                        this._errors = new SqlErrorCollection();
                    }
                    return this._errors;
                }
            }
        }

        internal bool IsKatmaiOrNewer
        {
            get
            {
                return this._isKatmai;
            }
        }

        internal bool IsYukonOrNewer
        {
            get
            {
                return this._isYukon;
            }
        }

        internal bool MARSOn
        {
            get
            {
                return this._fMARS;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal SqlInternalTransaction PendingTransaction
        {
            get
            {
                return this._pendingTransaction;
            }
            set
            {
                this._pendingTransaction = value;
            }
        }

        internal string Server
        {
            get
            {
                return this._server;
            }
        }

        internal TdsParserState State
        {
            get
            {
                return this._state;
            }
            set
            {
                this._state = value;
            }
        }

        internal SqlStatistics Statistics
        {
            get
            {
                return this._statistics;
            }
            set
            {
                this._statistics = value;
            }
        }

        internal SqlErrorCollection Warnings
        {
            get
            {
                lock (this._ErrorCollectionLock)
                {
                    if (this._warnings == null)
                    {
                        this._warnings = new SqlErrorCollection();
                    }
                    return this._warnings;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        internal struct ReliabilitySection
        {
            [Conditional("DEBUG")]
            internal void Start()
            {
            }

            [Conditional("DEBUG")]
            internal void Stop()
            {
            }

            [Conditional("DEBUG")]
            internal static void Assert(string message)
            {
            }
        }

        private class TdsOrderUnique
        {
            internal short ColumnOrdinal;
            internal byte Flags;

            internal TdsOrderUnique(short ordinal, byte flags)
            {
                this.ColumnOrdinal = ordinal;
                this.Flags = flags;
            }
        }
    }
}

