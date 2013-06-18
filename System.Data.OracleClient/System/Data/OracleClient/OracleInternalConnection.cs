namespace System.Data.OracleClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Globalization;
    using System.Text;
    using System.Transactions;

    internal sealed class OracleInternalConnection : System.Data.ProviderBase.DbConnectionInternal
    {
        private bool _connectionIsOpen;
        private OracleConnectionString _connectionOptions;
        private List<OracleInfoMessageEventArgs> _deferredInfoMessageCollection;
        private Encoding _encodingDatabase;
        private Encoding _encodingNational;
        private OciEnlistContext _enlistContext;
        private OciEnvironmentHandle _environmentHandle;
        private OciErrorHandle _errorHandle;
        private NativeBuffer _scratchBuffer;
        private OciServerHandle _serverHandle;
        private TimeSpan _serverTimeZoneAdjustment = TimeSpan.MinValue;
        private long _serverVersion;
        private string _serverVersionString;
        private string _serverVersionStringNormalized;
        private OciServiceContextHandle _serviceContextHandle;
        private OciSessionHandle _sessionHandle;
        private WeakReference _transaction;
        private System.Data.OracleClient.TransactionState _transactionState;

        internal OracleInternalConnection(OracleConnectionString connectionOptions)
        {
            this._connectionOptions = connectionOptions;
            string userId = connectionOptions.UserId;
            string password = connectionOptions.Password;
            string dataSource = connectionOptions.DataSource;
            bool integratedSecurity = connectionOptions.IntegratedSecurity;
            bool unicode = connectionOptions.Unicode;
            bool omitOracleConnectionName = this._connectionOptions.OmitOracleConnectionName;
            this._connectionIsOpen = this.OpenOnLocalTransaction(userId, password, dataSource, integratedSecurity, unicode, omitOracleConnectionName);
            if (this.UnicodeEnabled)
            {
                this._encodingDatabase = Encoding.Unicode;
            }
            else if (this.ServerVersionAtLeastOracle8i)
            {
                this._encodingDatabase = new OracleEncoding(this);
            }
            else
            {
                this._encodingDatabase = Encoding.Default;
            }
            this._encodingNational = Encoding.Unicode;
            if (connectionOptions.Enlist && !connectionOptions.Pooling)
            {
                System.Transactions.Transaction currentTransaction = System.Data.Common.ADP.GetCurrentTransaction();
                if (null != currentTransaction)
                {
                    this.Enlist(userId, password, dataSource, currentTransaction, false);
                }
            }
        }

        protected override void Activate(System.Transactions.Transaction transaction)
        {
            bool flag = null != transaction;
            OracleConnectionString str = this._connectionOptions;
            if (flag && str.Enlist)
            {
                if (!transaction.Equals(base.EnlistedTransaction))
                {
                    this.Enlist(str.UserId, str.Password, str.DataSource, transaction, false);
                }
            }
            else if (!flag && (this._enlistContext != null))
            {
                this.UnEnlist();
            }
        }

        internal OracleTransaction BeginOracleTransaction(System.Data.IsolationLevel il)
        {
            OracleConnection.ExecutePermission.Demand();
            if (this.TransactionState != System.Data.OracleClient.TransactionState.AutoCommit)
            {
                throw System.Data.Common.ADP.NoParallelTransactions();
            }
            this.RollbackDeadTransaction();
            OracleTransaction transaction = new OracleTransaction(this.ProxyConnection(), il);
            this.Transaction = transaction;
            return transaction;
        }

        public override DbTransaction BeginTransaction(System.Data.IsolationLevel il)
        {
            return this.BeginOracleTransaction(il);
        }

        internal void Commit()
        {
            int rc = TracedNativeMethods.OCITransCommit(this.ServiceContextHandle, this.ErrorHandle, OCI.MODE.OCI_DEFAULT);
            if (rc != 0)
            {
                OracleException.Check(this.ErrorHandle, rc);
            }
            this.TransactionState = System.Data.OracleClient.TransactionState.AutoCommit;
            this.Transaction = null;
        }

        internal void ConnectionIsBroken()
        {
            base.DoomThisConnection();
            OracleConnection owner = (OracleConnection) base.Owner;
            if (owner != null)
            {
                owner.Close();
            }
            else
            {
                this.Dispose();
            }
        }

        private void CreateDeferredInfoMessage(OciErrorHandle errorHandle, int rc)
        {
            OracleInfoMessageEventArgs item = new OracleInfoMessageEventArgs(OracleException.CreateException(errorHandle, rc));
            List<OracleInfoMessageEventArgs> list = this._deferredInfoMessageCollection;
            if (list == null)
            {
                list = this._deferredInfoMessageCollection = new List<OracleInfoMessageEventArgs>();
            }
            list.Add(item);
        }

        protected override void Deactivate()
        {
            if ((!base.IsConnectionDoomed && (this.ErrorHandle != null)) && this.ErrorHandle.ConnectionIsBroken)
            {
                this.ConnectionIsBroken();
            }
            if (System.Data.OracleClient.TransactionState.LocalStarted == this.TransactionState)
            {
                try
                {
                    this.Rollback();
                }
                catch (Exception exception)
                {
                    if (!System.Data.Common.ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    System.Data.Common.ADP.TraceException(exception);
                    base.DoomThisConnection();
                }
            }
        }

        public override void Dispose()
        {
            this.Deactivate();
            OciEnlistContext.SafeDispose(ref this._enlistContext);
            OciHandle.SafeDispose(ref this._sessionHandle);
            OciHandle.SafeDispose(ref this._serviceContextHandle);
            OciHandle.SafeDispose(ref this._serverHandle);
            OciHandle.SafeDispose(ref this._errorHandle);
            OciHandle.SafeDispose(ref this._environmentHandle);
            if (this._scratchBuffer != null)
            {
                this._scratchBuffer.Dispose();
            }
            this._scratchBuffer = null;
            this._encodingDatabase = null;
            this._encodingNational = null;
            this._transaction = null;
            this._serverVersionString = null;
            base.Dispose();
        }

        private void Enlist(string userName, string password, string serverName, System.Transactions.Transaction transaction, bool manualEnlistment)
        {
            this.UnEnlist();
            if (!OCI.ClientVersionAtLeastOracle9i)
            {
                throw System.Data.Common.ADP.DistribTxRequiresOracle9i();
            }
            if (null != transaction)
            {
                if (this.HasTransaction)
                {
                    throw System.Data.Common.ADP.TransactionPresent();
                }
                byte[] buffer3 = this.StringToNullTerminatedBytes(password);
                byte[] buffer2 = this.StringToNullTerminatedBytes(userName);
                byte[] buffer = this.StringToNullTerminatedBytes(serverName);
                this._enlistContext = new OciEnlistContext(buffer2, buffer3, buffer, this.ServiceContextHandle, this.ErrorHandle);
                this._enlistContext.Join(this, transaction);
                this.TransactionState = System.Data.OracleClient.TransactionState.GlobalStarted;
            }
            else
            {
                this.TransactionState = System.Data.OracleClient.TransactionState.AutoCommit;
            }
            base.EnlistedTransaction = transaction;
        }

        public override void EnlistTransaction(System.Transactions.Transaction transaction)
        {
            OracleConnectionString str = this._connectionOptions;
            this.RollbackDeadTransaction();
            this.Enlist(str.UserId, str.Password, str.DataSource, transaction, true);
        }

        internal void FireDeferredInfoMessageEvents(OracleConnection outerConnection)
        {
            List<OracleInfoMessageEventArgs> list = this._deferredInfoMessageCollection;
            this._deferredInfoMessageCollection = null;
            if (list != null)
            {
                foreach (OracleInfoMessageEventArgs args in list)
                {
                    if (args != null)
                    {
                        outerConnection.OnInfoMessage(args);
                    }
                }
            }
        }

        internal byte[] GetBytes(string value, bool useNationalCharacterSet)
        {
            if (useNationalCharacterSet)
            {
                return this._encodingNational.GetBytes(value);
            }
            return this._encodingDatabase.GetBytes(value);
        }

        internal NativeBuffer GetScratchBuffer(int minSize)
        {
            NativeBuffer buffer = this._scratchBuffer;
            if ((buffer == null) || (buffer.Length < minSize))
            {
                if (buffer != null)
                {
                    buffer.Dispose();
                }
                buffer = new NativeBuffer_ScratchBuffer(minSize);
                this._scratchBuffer = buffer;
            }
            return buffer;
        }

        internal TimeSpan GetServerTimeZoneAdjustmentToUTC(OracleConnection connection)
        {
            TimeSpan zero = this._serverTimeZoneAdjustment;
            if (TimeSpan.MinValue == zero)
            {
                if (this.ServerVersionAtLeastOracle9i)
                {
                    OracleCommand command = new OracleCommand {
                        Connection = connection,
                        Transaction = this.Transaction,
                        CommandText = "select tz_offset(dbtimezone) from dual"
                    };
                    string str = (string) command.ExecuteScalar();
                    int hours = int.Parse(str.Substring(0, 3), CultureInfo.InvariantCulture);
                    int minutes = int.Parse(str.Substring(4, 2), CultureInfo.InvariantCulture);
                    zero = new TimeSpan(hours, minutes, 0);
                }
                else
                {
                    zero = TimeSpan.Zero;
                }
                this._serverTimeZoneAdjustment = zero;
            }
            return this._serverTimeZoneAdjustment;
        }

        internal string GetString(byte[] bytearray)
        {
            return this._encodingDatabase.GetString(bytearray);
        }

        internal string GetString(byte[] bytearray, bool useNationalCharacterSet)
        {
            if (useNationalCharacterSet)
            {
                return this._encodingNational.GetString(bytearray);
            }
            return this._encodingDatabase.GetString(bytearray);
        }

        private bool OpenOnLocalTransaction(string userName, string password, string serverName, bool integratedSecurity, bool unicode, bool omitOracleConnectionName)
        {
            int rc = 0;
            OCI.MODE environmentMode = OCI.MODE.OCI_DATA_AT_EXEC | OCI.MODE.OCI_BATCH_MODE;
            OCI.DetermineClientVersion();
            if (unicode)
            {
                if (OCI.ClientVersionAtLeastOracle9i)
                {
                    environmentMode |= OCI.MODE.OCI_UTF16;
                }
                else
                {
                    unicode = false;
                }
            }
            this._environmentHandle = new OciEnvironmentHandle(environmentMode, unicode);
            if (this._environmentHandle.IsInvalid)
            {
                throw System.Data.Common.ADP.CouldNotCreateEnvironment("OCIEnvCreate", rc);
            }
            this._errorHandle = new OciErrorHandle(this._environmentHandle);
            this._serverHandle = new OciServerHandle(this._errorHandle);
            this._sessionHandle = new OciSessionHandle(this._serverHandle);
            this._serviceContextHandle = new OciServiceContextHandle(this._sessionHandle);
            try
            {
                OCI.CRED cred;
                rc = TracedNativeMethods.OCIServerAttach(this._serverHandle, this._errorHandle, serverName, serverName.Length, OCI.MODE.OCI_DEFAULT);
                if (rc != 0)
                {
                    if (1 == rc)
                    {
                        this.CreateDeferredInfoMessage(this.ErrorHandle, rc);
                    }
                    else
                    {
                        OracleException.Check(this.ErrorHandle, rc);
                    }
                }
                this._serviceContextHandle.SetAttribute(OCI.ATTR.OCI_ATTR_SERVER, this._serverHandle, this._errorHandle);
                if (integratedSecurity)
                {
                    cred = OCI.CRED.OCI_CRED_EXT;
                }
                else
                {
                    cred = OCI.CRED.OCI_CRED_RDBMS;
                    this._sessionHandle.SetAttribute(OCI.ATTR.OCI_ATTR_USERNAME, userName, this._errorHandle);
                    if (password != null)
                    {
                        this._sessionHandle.SetAttribute(OCI.ATTR.OCI_ATTR_PASSWORD, password, this._errorHandle);
                    }
                }
                if (!omitOracleConnectionName)
                {
                    string dataSource = this._connectionOptions.DataSource;
                    if (dataSource.Length > 0x10)
                    {
                        dataSource = dataSource.Substring(0, 0x10);
                    }
                    this._serverHandle.SetAttribute(OCI.ATTR.OCI_ATTR_EXTERNAL_NAME, dataSource, this._errorHandle);
                    this._serverHandle.SetAttribute(OCI.ATTR.OCI_ATTR_INTERNAL_NAME, dataSource, this._errorHandle);
                }
                rc = TracedNativeMethods.OCISessionBegin(this._serviceContextHandle, this._errorHandle, this._sessionHandle, cred, OCI.MODE.OCI_DEFAULT);
                if (rc != 0)
                {
                    if (1 == rc)
                    {
                        this.CreateDeferredInfoMessage(this.ErrorHandle, rc);
                    }
                    else
                    {
                        OracleException.Check(this.ErrorHandle, rc);
                    }
                }
                this._serviceContextHandle.SetAttribute(OCI.ATTR.OCI_ATTR_SESSION, this._sessionHandle, this._errorHandle);
            }
            catch (OracleException)
            {
                OciHandle.SafeDispose(ref this._serviceContextHandle);
                OciHandle.SafeDispose(ref this._sessionHandle);
                OciHandle.SafeDispose(ref this._serverHandle);
                OciHandle.SafeDispose(ref this._errorHandle);
                OciHandle.SafeDispose(ref this._environmentHandle);
                throw;
            }
            return true;
        }

        internal static long ParseServerVersion(string versionString)
        {
            PARSERSTATE nOTHINGYET = PARSERSTATE.NOTHINGYET;
            int startIndex = 0;
            int num2 = 0;
            long num3 = 0L;
            versionString = versionString + "0.0.0.0.0 ";
            for (int i = 0; i < versionString.Length; i++)
            {
                switch (nOTHINGYET)
                {
                    case PARSERSTATE.NOTHINGYET:
                    {
                        if (char.IsDigit(versionString, i))
                        {
                            nOTHINGYET = PARSERSTATE.DIGIT;
                            startIndex = i;
                        }
                        continue;
                    }
                    case PARSERSTATE.PERIOD:
                    {
                        if (!char.IsDigit(versionString, i))
                        {
                            break;
                        }
                        nOTHINGYET = PARSERSTATE.DIGIT;
                        startIndex = i;
                        continue;
                    }
                    case PARSERSTATE.DIGIT:
                    {
                        if (!("." == versionString.Substring(i, 1)) && (4 != num2))
                        {
                            goto Label_00AB;
                        }
                        num2++;
                        nOTHINGYET = PARSERSTATE.PERIOD;
                        long num5 = int.Parse(versionString.Substring(startIndex, i - startIndex), CultureInfo.InvariantCulture);
                        num3 = (num3 << 8) + num5;
                        if (5 != num2)
                        {
                            continue;
                        }
                        return num3;
                    }
                    default:
                    {
                        continue;
                    }
                }
                nOTHINGYET = PARSERSTATE.NOTHINGYET;
                num2 = 0;
                num3 = 0L;
                continue;
            Label_00AB:
                if (!char.IsDigit(versionString, i))
                {
                    nOTHINGYET = PARSERSTATE.NOTHINGYET;
                    num2 = 0;
                    num3 = 0L;
                }
            }
            return 0L;
        }

        private OracleConnection ProxyConnection()
        {
            OracleConnection owner = (OracleConnection) base.Owner;
            if (owner == null)
            {
                throw System.Data.Common.ADP.InvalidOperation("internal connection without a proxy?");
            }
            return owner;
        }

        internal void Rollback()
        {
            if (System.Data.OracleClient.TransactionState.GlobalStarted != this._transactionState)
            {
                int rc = TracedNativeMethods.OCITransRollback(this.ServiceContextHandle, this.ErrorHandle, OCI.MODE.OCI_DEFAULT);
                if (rc != 0)
                {
                    OracleException.Check(this.ErrorHandle, rc);
                }
                this.TransactionState = System.Data.OracleClient.TransactionState.AutoCommit;
            }
            this.Transaction = null;
        }

        internal void RollbackDeadTransaction()
        {
            if ((this._transaction != null) && !this._transaction.IsAlive)
            {
                this.Rollback();
            }
        }

        private byte[] StringToNullTerminatedBytes(string str)
        {
            Encoding encoding = Encoding.Default;
            int byteCount = encoding.GetByteCount(str);
            byte[] bytes = new byte[byteCount + 1];
            encoding.GetBytes(str, 0, str.Length, bytes, 0);
            bytes[byteCount] = 0;
            return bytes;
        }

        private void UnEnlist()
        {
            if (this._enlistContext != null)
            {
                this.TransactionState = System.Data.OracleClient.TransactionState.AutoCommit;
                this._enlistContext.Join(this, null);
                OciEnlistContext.SafeDispose(ref this._enlistContext);
                this.Transaction = null;
            }
        }

        internal OciEnvironmentHandle EnvironmentHandle
        {
            get
            {
                return this._environmentHandle;
            }
        }

        internal OciErrorHandle ErrorHandle
        {
            get
            {
                return this._errorHandle;
            }
        }

        internal bool HasTransaction
        {
            get
            {
                System.Data.OracleClient.TransactionState transactionState = this.TransactionState;
                return ((System.Data.OracleClient.TransactionState.LocalStarted == transactionState) || (System.Data.OracleClient.TransactionState.GlobalStarted == transactionState));
            }
        }

        public override string ServerVersion
        {
            get
            {
                if (this._serverVersionString == null)
                {
                    string versionString = "no version available";
                    NativeBuffer bufp = null;
                    try
                    {
                        bufp = new NativeBuffer_ServerVersion(500);
                        int rc = TracedNativeMethods.OCIServerVersion(this.ServiceContextHandle, this.ErrorHandle, bufp);
                        if (rc != 0)
                        {
                            throw System.Data.Common.ADP.OracleError(this.ErrorHandle, rc);
                        }
                        if (rc == 0)
                        {
                            versionString = this.ServiceContextHandle.PtrToString(bufp);
                        }
                        this._serverVersion = ParseServerVersion(versionString);
                        this._serverVersionString = string.Format(null, "{0}.{1}.{2}.{3}.{4} {5}", new object[] { (this._serverVersion >> 0x20) & 0xffL, (this._serverVersion >> 0x18) & 0xffL, (this._serverVersion >> 0x10) & 0xffL, (this._serverVersion >> 8) & 0xffL, this._serverVersion & 0xffL, versionString });
                        this._serverVersionStringNormalized = string.Format(null, "{0:00}.{1:00}.{2:00}.{3:00}.{4:00} ", new object[] { (this._serverVersion >> 0x20) & 0xffL, (this._serverVersion >> 0x18) & 0xffL, (this._serverVersion >> 0x10) & 0xffL, (this._serverVersion >> 8) & 0xffL, this._serverVersion & 0xffL });
                    }
                    finally
                    {
                        if (bufp != null)
                        {
                            bufp.Dispose();
                            bufp = null;
                        }
                    }
                }
                return this._serverVersionString;
            }
        }

        internal bool ServerVersionAtLeastOracle8
        {
            get
            {
                return (this.ServerVersionNumber >= 0x800000000L);
            }
        }

        internal bool ServerVersionAtLeastOracle8i
        {
            get
            {
                return (this.ServerVersionNumber >= 0x801000000L);
            }
        }

        internal bool ServerVersionAtLeastOracle9i
        {
            get
            {
                return (this.ServerVersionNumber >= 0x900000000L);
            }
        }

        public override string ServerVersionNormalized
        {
            get
            {
                if (this._serverVersionStringNormalized == null)
                {
                    string serverVersion = this.ServerVersion;
                }
                return this._serverVersionStringNormalized;
            }
        }

        internal long ServerVersionNumber
        {
            get
            {
                if (0L == this._serverVersion)
                {
                    string serverVersion = this.ServerVersion;
                }
                return this._serverVersion;
            }
        }

        internal OciServiceContextHandle ServiceContextHandle
        {
            get
            {
                return this._serviceContextHandle;
            }
        }

        internal OciSessionHandle SessionHandle
        {
            get
            {
                return this._sessionHandle;
            }
        }

        internal OracleTransaction Transaction
        {
            get
            {
                if ((this._transaction != null) && this._transaction.IsAlive)
                {
                    if (((OracleTransaction) this._transaction.Target).Connection != null)
                    {
                        return (OracleTransaction) this._transaction.Target;
                    }
                    this._transaction.Target = null;
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    this._transaction = null;
                }
                else if (this._transaction != null)
                {
                    this._transaction.Target = value;
                }
                else
                {
                    this._transaction = new WeakReference(value);
                }
            }
        }

        internal System.Data.OracleClient.TransactionState TransactionState
        {
            get
            {
                return this._transactionState;
            }
            set
            {
                this._transactionState = value;
            }
        }

        internal bool UnicodeEnabled
        {
            get
            {
                if (!OCI.ClientVersionAtLeastOracle9i)
                {
                    return false;
                }
                if (this.EnvironmentHandle != null)
                {
                    return this.EnvironmentHandle.IsUnicode;
                }
                return true;
            }
        }

        internal enum PARSERSTATE
        {
            DIGIT = 3,
            NOTHINGYET = 1,
            PERIOD = 2
        }
    }
}

