namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal sealed class TdsParserStateObject
    {
        private int _activateCount;
        private volatile int _allowObjectID;
        internal DbAsyncResult _asyncAttentionResult;
        internal DbAsyncResult _asyncResult;
        internal bool _attentionReceived;
        internal bool _attentionSent;
        private bool _bcpLock;
        internal byte[] _bHeaderBuffer;
        internal byte[] _bTmp;
        internal bool _bulkCopyOpperationInProgress;
        internal DbAsyncResult _cachedAsyncResult;
        private bool _cancelled;
        internal _SqlMetaDataSetCollection _cleanupAltMetaDataSetArray;
        internal _SqlMetaDataSet _cleanupMetaData;
        internal int[] _decimalBits;
        internal SqlError _error;
        internal bool _errorTokenReceived;
        internal SqlInternalTransaction _executedUnderTransaction;
        internal volatile bool _fResetConnectionSent;
        internal volatile bool _fResetEventOwned;
        private GCHandle _gcHandle;
        internal bool _hasOpenResult;
        private byte[] _inBuff;
        internal int _inBytesPacket;
        internal int _inBytesRead;
        internal int _inBytesUsed;
        internal readonly int _inputHeaderLen;
        internal bool _internalTimeout;
        internal ulong _longlen;
        internal ulong _longlenleft;
        internal byte _messageStatus;
        private TdsParserStateObject _nextPooledObject;
        internal readonly int _objectID;
        private static int _objectTypeCount;
        internal byte[] _outBuff;
        internal int _outBytesUsed;
        internal readonly int _outputHeaderLen;
        internal byte _outputMessageType;
        internal byte _outputPacketNumber;
        private readonly WeakReference _owner;
        private readonly TdsParser _parser;
        private int _pendingCallbacks;
        internal bool _pendingData;
        internal bool _receivedColMetaData;
        private SNIHandle _sessionHandle;
        internal SNIPacket _sniAsyncAttnPacket;
        private System.Data.SqlClient.SniContext _sniContext;
        internal SNIPacket _sniPacket;
        private int _timeoutSeconds;
        private long _timeoutTime;
        internal int _traceChangePasswordLength;
        internal int _traceChangePasswordOffset;
        internal int _tracePasswordLength;
        internal int _tracePasswordOffset;

        internal TdsParserStateObject(TdsParser parser)
        {
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            this._owner = new WeakReference(null);
            this._inputHeaderLen = 8;
            this._outputHeaderLen = 8;
            this._outBytesUsed = 8;
            this._outputPacketNumber = 1;
            this._bTmp = new byte[12];
            this._parser = parser;
            this.SetPacketSize(0x1000);
            this.IncrementPendingCallbacks();
        }

        internal TdsParserStateObject(TdsParser parser, SNIHandle physicalConnection, bool async)
        {
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            this._owner = new WeakReference(null);
            this._inputHeaderLen = 8;
            this._outputHeaderLen = 8;
            this._outBytesUsed = 8;
            this._outputPacketNumber = 1;
            this._bTmp = new byte[12];
            this._parser = parser;
            this.SniContext = System.Data.SqlClient.SniContext.Snix_GetMarsSession;
            this.SetPacketSize(this._parser._physicalStateObj._outBuff.Length);
            SNINativeMethodWrapper.ConsumerInfo myInfo = this.CreateConsumerInfo(async);
            this._sessionHandle = new SNIHandle(myInfo, "session:", physicalConnection);
            if (this._sessionHandle.Status != 0)
            {
                parser.Errors.Add(parser.ProcessSNIError(this));
                parser.ThrowExceptionAndWarning();
            }
            this.IncrementPendingCallbacks();
        }

        internal void Activate(object owner)
        {
            this.Owner = owner;
            Interlocked.Increment(ref this._activateCount);
        }

        [Conditional("DEBUG")]
        private void AssertValidState()
        {
            string str = null;
            if ((this._inBytesUsed < 0) || (this._inBytesRead < 0))
            {
                str = string.Format(CultureInfo.InvariantCulture, "either _inBytesUsed or _inBytesRead is negative: {0}, {1}", new object[] { this._inBytesUsed, this._inBytesRead });
            }
            else if (this._inBytesUsed > this._inBytesRead)
            {
                str = string.Format(CultureInfo.InvariantCulture, "_inBytesUsed > _inBytesRead: {0} > {1}", new object[] { this._inBytesUsed, this._inBytesRead });
            }
        }

        internal void Cancel(int objectID)
        {
            lock (this)
            {
                if ((!this._cancelled && (objectID == this._allowObjectID)) && (objectID != -1))
                {
                    this._cancelled = true;
                    if (this._pendingData && !this._attentionSent)
                    {
                        this.SendAttention();
                    }
                }
            }
        }

        internal void CancelRequest()
        {
            this.ResetBuffer();
            this.SendAttention();
            this.Parser.ProcessPendingAck(this);
        }

        public void CheckSetResetConnectionState(uint error, CallbackType callbackType)
        {
            if (this._fResetEventOwned)
            {
                if ((callbackType == CallbackType.Read) && (error == 0))
                {
                    this._parser._fResetConnection = false;
                    this._fResetConnectionSent = false;
                    this._fResetEventOwned = !this._parser._resetConnectionEvent.Set();
                }
                if (error != 0)
                {
                    this._fResetConnectionSent = false;
                    this._fResetEventOwned = !this._parser._resetConnectionEvent.Set();
                }
            }
        }

        internal void CleanWire()
        {
            if ((TdsParserState.Broken != this.Parser.State) && (this.Parser.State != TdsParserState.Closed))
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.TdsParserStateObject.CleanWire|ADV> %d#\n", this.ObjectID);
                }
                while ((this._messageStatus != 1) || ((this._messageStatus == 1) && (this._inBytesPacket != 0)))
                {
                    int num = this._inBytesRead - this._inBytesUsed;
                    if (this._inBytesPacket >= num)
                    {
                        this._inBytesPacket -= num;
                        this._inBytesUsed = this._inBytesRead;
                        if ((this._messageStatus != 1) || (this._inBytesPacket > 0))
                        {
                            this.ReadBuffer();
                        }
                    }
                    else
                    {
                        this._inBytesUsed += this._inBytesPacket;
                        this._inBytesPacket = 0;
                        this.ProcessHeader();
                    }
                }
                this._inBytesUsed = this._inBytesPacket = this._inBytesRead = 0;
                this._pendingData = false;
            }
        }

        internal void CloseSession()
        {
            this.ResetCancelAndProcessAttention();
            this.Parser.PutSession(this);
        }

        private SNINativeMethodWrapper.ConsumerInfo CreateConsumerInfo(bool async)
        {
            SNINativeMethodWrapper.ConsumerInfo info = new SNINativeMethodWrapper.ConsumerInfo {
                defaultBufferSize = this._outBuff.Length
            };
            if (async)
            {
                info.readDelegate = SNILoadHandle.SingletonInstance.ReadAsyncCallbackDispatcher;
                info.writeDelegate = SNILoadHandle.SingletonInstance.WriteAsyncCallbackDispatcher;
                this._gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
                info.key = (IntPtr) this._gcHandle;
            }
            return info;
        }

        internal void CreatePhysicalSNIHandle(string serverName, bool ignoreSniOpenTimeout, long timerExpire, out byte[] instanceName, byte[] spnBuffer, bool flushCache, bool async, bool fParallel)
        {
            long num;
            SNINativeMethodWrapper.ConsumerInfo myInfo = this.CreateConsumerInfo(async);
            if (0x7fffffffffffffffL == timerExpire)
            {
                num = 0x7fffffffL;
            }
            else
            {
                num = ADP.TimerRemainingMilliseconds(timerExpire);
                if (num > 0x7fffffffL)
                {
                    num = 0x7fffffffL;
                }
                else if (0L > num)
                {
                    num = 0L;
                }
            }
            this._sessionHandle = new SNIHandle(myInfo, serverName, spnBuffer, ignoreSniOpenTimeout, (int) num, out instanceName, flushCache, !async, fParallel);
        }

        internal bool Deactivate()
        {
            bool flag = false;
            Interlocked.Decrement(ref this._activateCount);
            this.Owner = null;
            try
            {
                switch (this.Parser.State)
                {
                    case TdsParserState.Broken:
                    case TdsParserState.Closed:
                        return flag;
                }
                if (this._pendingData)
                {
                    this.CleanWire();
                }
                if (this.HasOpenResult)
                {
                    this.DecrementOpenResultCount();
                }
                this.ResetCancelAndProcessAttention();
                flag = true;
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                ADP.TraceExceptionWithoutRethrow(exception);
            }
            return flag;
        }

        internal void DecrementOpenResultCount()
        {
            if (this._executedUnderTransaction == null)
            {
                this._parser.DecrementNonTransactedOpenResultCount();
            }
            else
            {
                this._executedUnderTransaction.DecrementAndObtainOpenResultCount();
                this._executedUnderTransaction = null;
            }
            this._hasOpenResult = false;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void DecrementPendingCallbacks(bool release)
        {
            int num = Interlocked.Decrement(ref this._pendingCallbacks);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.TdsParserStateObject.DecrementPendingCallbacks|ADV> %d#, after decrementing _pendingCallbacks: %d\n", this.ObjectID, this._pendingCallbacks);
            }
            if (((num == 0) || release) && this._gcHandle.IsAllocated)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.TdsParserStateObject.DecrementPendingCallbacks|ADV> %d#, FREEING HANDLE!\n", this.ObjectID);
                }
                this._gcHandle.Free();
            }
        }

        internal void Dispose()
        {
            SafeHandle handle2 = this._sniPacket;
            SafeHandle handle = this._sessionHandle;
            SafeHandle handle3 = this._sniAsyncAttnPacket;
            this._sniPacket = null;
            this._sessionHandle = null;
            this._sniAsyncAttnPacket = null;
            if ((handle != null) || (handle2 != null))
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    if (handle2 != null)
                    {
                        handle2.Dispose();
                    }
                    if (handle3 != null)
                    {
                        handle3.Dispose();
                    }
                    if (handle != null)
                    {
                        handle.Dispose();
                        this.DecrementPendingCallbacks(true);
                    }
                }
            }
        }

        internal void ExecuteFlush()
        {
            lock (this)
            {
                if (this._cancelled && (1 == this._outputPacketNumber))
                {
                    this.ResetBuffer();
                    this._cancelled = false;
                    throw SQL.OperationCancelled();
                }
                this.WritePacket(1);
                this._pendingData = true;
            }
        }

        internal int IncrementAndObtainOpenResultCount(SqlInternalTransaction transaction)
        {
            this._hasOpenResult = true;
            if (transaction == null)
            {
                return this._parser.IncrementNonTransactedOpenResultCount();
            }
            this._executedUnderTransaction = transaction;
            return transaction.IncrementAndObtainOpenResultCount();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void IncrementPendingCallbacks()
        {
            Interlocked.Increment(ref this._pendingCallbacks);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.TdsParserStateObject.IncrementPendingCallbacks|ADV> %d#, after incrementing _pendingCallbacks: %d\n", this.ObjectID, this._pendingCallbacks);
            }
        }

        internal bool IsConnectionAlive(bool throwOnException)
        {
            bool flag = true;
            if (((this._parser == null) || (this._parser.State == TdsParserState.Broken)) || (this._parser.State == TdsParserState.Closed))
            {
                flag = false;
                if (throwOnException)
                {
                    throw SQL.ConnectionDoomed();
                }
                return flag;
            }
            if (!this._parser.AsyncOn || (this._pendingCallbacks <= 1))
            {
                if ((this._parser.Connection != null) && (this._parser.Connection.Owner != null))
                {
                    return flag;
                }
                IntPtr zero = IntPtr.Zero;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    uint num;
                    this.SniContext = System.Data.SqlClient.SniContext.Snix_Connect;
                    if (this._parser.AsyncOn)
                    {
                        num = SNINativeMethodWrapper.SNICheckConnection(this.Handle);
                    }
                    else
                    {
                        num = SNINativeMethodWrapper.SNIReadSync(this.Handle, ref zero, 0);
                    }
                    if ((num != 0) && (num != 0x102))
                    {
                        Bid.Trace("<sc.TdsParser.IsConnectionAlive|Info> received error %d on idle connection\n", (int) num);
                        flag = false;
                        if (throwOnException)
                        {
                            this._parser.Errors.Add(this._parser.ProcessSNIError(this));
                            this._parser.ThrowExceptionAndWarning();
                        }
                    }
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        SNINativeMethodWrapper.SNIPacketRelease(zero);
                    }
                }
            }
            return flag;
        }

        internal byte PeekByte()
        {
            byte num = this.ReadByte();
            this._inBytesPacket++;
            this._inBytesUsed--;
            return num;
        }

        internal void ProcessHeader()
        {
            if ((this._inBytesUsed + this._inputHeaderLen) > this._inBytesRead)
            {
                int count = this._inBytesRead - this._inBytesUsed;
                int num2 = this._inputHeaderLen - count;
                if (this._bHeaderBuffer == null)
                {
                    this._bHeaderBuffer = new byte[this._inputHeaderLen];
                }
                Buffer.BlockCopy(this._inBuff, this._inBytesUsed, this._bHeaderBuffer, 0, count);
                this._inBytesUsed = this._inBytesRead;
                int dstOffset = count;
                do
                {
                    if ((this._parser.State == TdsParserState.Broken) || (this._parser.State == TdsParserState.Closed))
                    {
                        this.ThrowExceptionAndWarning();
                        return;
                    }
                    this.ReadNetworkPacket();
                    if (this._internalTimeout)
                    {
                        this.ThrowExceptionAndWarning();
                        return;
                    }
                    int num = Math.Min(this._inBytesRead - this._inBytesUsed, num2);
                    Buffer.BlockCopy(this._inBuff, this._inBytesUsed, this._bHeaderBuffer, dstOffset, num);
                    dstOffset += num;
                    num2 -= num;
                    this._inBytesUsed += num;
                }
                while (num2 > 0);
                this._inBytesPacket = ((this._bHeaderBuffer[2] << 8) | this._bHeaderBuffer[3]) - this._inputHeaderLen;
                this._messageStatus = this._bHeaderBuffer[1];
            }
            else
            {
                this._messageStatus = this._inBuff[this._inBytesUsed + 1];
                this._inBytesPacket = ((this._inBuff[this._inBytesUsed + 2] << 8) | this._inBuff[(this._inBytesUsed + 2) + 1]) - this._inputHeaderLen;
                this._inBytesUsed += this._inputHeaderLen;
            }
        }

        public void ProcessSniPacket(IntPtr packet, uint error)
        {
            if (((this._parser.State != TdsParserState.Closed) && (this._parser.State != TdsParserState.Broken)) || (error == 0))
            {
                if (error != 0)
                {
                    if (this._parser._fAwaitingPreLogin && (error != 0x102))
                    {
                        this._parser._fPreLoginErrorOccurred = true;
                    }
                    else
                    {
                        this._error = this._parser.ProcessSNIError(this);
                    }
                }
                else
                {
                    SNINativeMethodWrapper.SNIPacketGetConnection(packet);
                    uint dataSize = 0;
                    IntPtr ptrZero = ADP.PtrZero;
                    SNINativeMethodWrapper.SNIPacketGetData(packet, ref ptrZero, ref dataSize);
                    if (this._inBuff.Length < dataSize)
                    {
                        throw SQL.InvalidInternalPacketSize(Res.GetString("SqlMisc_InvalidArraySizeMessage"));
                    }
                    Marshal.Copy(ptrZero, this._inBuff, 0, (int) dataSize);
                    this._inBytesRead = (int) dataSize;
                    this._inBytesUsed = 0;
                }
            }
        }

        public void ReadAsyncCallback(IntPtr key, IntPtr packet, uint error)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            bool flag = true;
            try
            {
                if (this._parser.MARSOn)
                {
                    this.CheckSetResetConnectionState(error, CallbackType.Read);
                }
                this.ProcessSniPacket(packet, error);
            }
            catch (Exception exception)
            {
                flag = ADP.IsCatchableExceptionType(exception);
                throw;
            }
            finally
            {
                this.DecrementPendingCallbacks(false);
                if (flag)
                {
                    this._asyncResult.SetCompleted();
                }
            }
        }

        internal void ReadBuffer()
        {
            if (this._inBytesPacket > 0)
            {
                this.ReadNetworkPacket();
            }
            else if (this._inBytesPacket == 0)
            {
                this.ReadNetworkPacket();
                this.ProcessHeader();
                if (this._inBytesUsed == this._inBytesRead)
                {
                    this.ReadNetworkPacket();
                }
            }
        }

        internal byte ReadByte()
        {
            if (this._inBytesUsed == this._inBytesRead)
            {
                this.ReadBuffer();
            }
            else if (this._inBytesPacket == 0)
            {
                this.ProcessHeader();
                if (this._inBytesUsed == this._inBytesRead)
                {
                    this.ReadBuffer();
                }
            }
            this._inBytesPacket--;
            return this._inBuff[this._inBytesUsed++];
        }

        public void ReadByteArray(byte[] buff, int offset, int len)
        {
            while (len > 0)
            {
                if ((len <= this._inBytesPacket) && ((this._inBytesUsed + len) <= this._inBytesRead))
                {
                    if (buff != null)
                    {
                        Buffer.BlockCopy(this._inBuff, this._inBytesUsed, buff, offset, len);
                    }
                    this._inBytesUsed += len;
                    this._inBytesPacket -= len;
                    return;
                }
                if (((len <= this._inBytesPacket) && ((this._inBytesUsed + len) > this._inBytesRead)) || ((len > this._inBytesPacket) && ((this._inBytesUsed + this._inBytesPacket) > this._inBytesRead)))
                {
                    int count = this._inBytesRead - this._inBytesUsed;
                    if (buff != null)
                    {
                        Buffer.BlockCopy(this._inBuff, this._inBytesUsed, buff, offset, count);
                    }
                    offset += count;
                    this._inBytesUsed += count;
                    this._inBytesPacket -= count;
                    len -= count;
                    this.ReadBuffer();
                }
                else if ((len > this._inBytesPacket) && ((this._inBytesUsed + this._inBytesPacket) <= this._inBytesRead))
                {
                    if (buff != null)
                    {
                        Buffer.BlockCopy(this._inBuff, this._inBytesUsed, buff, offset, this._inBytesPacket);
                    }
                    this._inBytesUsed += this._inBytesPacket;
                    offset += this._inBytesPacket;
                    len -= this._inBytesPacket;
                    this._inBytesPacket = 0;
                    if (this._inBytesUsed == this._inBytesRead)
                    {
                        this.ReadBuffer();
                        continue;
                    }
                    this.ProcessHeader();
                    if (this._inBytesUsed == this._inBytesRead)
                    {
                        this.ReadBuffer();
                    }
                }
            }
        }

        internal char ReadChar()
        {
            byte num2 = this.ReadByte();
            return (char) (((this.ReadByte() & 0xff) << 8) + (num2 & 0xff));
        }

        internal double ReadDouble()
        {
            if (((this._inBytesUsed + 8) > this._inBytesRead) || (this._inBytesPacket < 8))
            {
                this.ReadByteArray(this._bTmp, 0, 8);
                return BitConverter.ToDouble(this._bTmp, 0);
            }
            double num = BitConverter.ToDouble(this._inBuff, this._inBytesUsed);
            this._inBytesUsed += 8;
            this._inBytesPacket -= 8;
            return num;
        }

        internal short ReadInt16()
        {
            byte num2 = this.ReadByte();
            return (short) ((this.ReadByte() << 8) + num2);
        }

        internal int ReadInt32()
        {
            if (((this._inBytesUsed + 4) > this._inBytesRead) || (this._inBytesPacket < 4))
            {
                this.ReadByteArray(this._bTmp, 0, 4);
                return BitConverter.ToInt32(this._bTmp, 0);
            }
            int num = BitConverter.ToInt32(this._inBuff, this._inBytesUsed);
            this._inBytesUsed += 4;
            this._inBytesPacket -= 4;
            return num;
        }

        internal long ReadInt64()
        {
            if (((this._inBytesUsed + 8) > this._inBytesRead) || (this._inBytesPacket < 8))
            {
                this.ReadByteArray(this._bTmp, 0, 8);
                return BitConverter.ToInt64(this._bTmp, 0);
            }
            long num = BitConverter.ToInt64(this._inBuff, this._inBytesUsed);
            this._inBytesUsed += 8;
            this._inBytesPacket -= 8;
            return num;
        }

        internal void ReadNetworkPacket()
        {
            this._inBytesUsed = 0;
            if (this.Parser.AsyncOn && (this._cachedAsyncResult == null))
            {
                this._cachedAsyncResult = new DbAsyncResult(this, string.Empty, null, null, null);
            }
            this.ReadSni(this._cachedAsyncResult, this);
            if (this.Parser.AsyncOn)
            {
                this.ReadSniSyncOverAsync();
            }
            this.SniReadStatisticsAndTracing();
            if (Bid.AdvancedOn)
            {
                Bid.TraceBin("<sc.TdsParser.ReadNetworkPacket|INFO|ADV> Packet read", this._inBuff, (ushort) this._inBytesRead);
            }
        }

        internal int ReadPlpBytes(ref byte[] buff, int offst, int len)
        {
            int num = 0;
            int num3 = 0;
            if (this._longlen == 0L)
            {
                if (buff == null)
                {
                    buff = new byte[0];
                }
                return 0;
            }
            int num2 = len;
            if ((buff == null) && (this._longlen != 18446744073709551614L))
            {
                buff = new byte[Math.Min((int) this._longlen, len)];
            }
            if (this._longlenleft == 0L)
            {
                this.ReadPlpLength(false);
                if (this._longlenleft == 0L)
                {
                    return 0;
                }
            }
            if (buff == null)
            {
                buff = new byte[this._longlenleft];
            }
            while (num2 > 0)
            {
                num = (int) Math.Min(this._longlenleft, (ulong) num2);
                if (buff.Length < (offst + num))
                {
                    byte[] dst = new byte[offst + num];
                    Buffer.BlockCopy(buff, 0, dst, 0, offst);
                    buff = dst;
                }
                num = this.ReadPlpBytesChunk(buff, offst, num);
                num2 -= num;
                offst += num;
                num3 += num;
                if (this._longlenleft == 0L)
                {
                    this.ReadPlpLength(false);
                }
                if (this._longlenleft == 0L)
                {
                    return num3;
                }
            }
            return num3;
        }

        internal int ReadPlpBytesChunk(byte[] buff, int offset, int len)
        {
            int num = 0;
            if (this._longlenleft == 0L)
            {
                return 0;
            }
            num = len;
            if (this._longlenleft < len)
            {
                num = (int) this._longlenleft;
            }
            this.ReadByteArray(buff, offset, num);
            this._longlenleft -= num;
            return num;
        }

        internal ulong ReadPlpLength(bool returnPlpNullIfNull)
        {
            bool flag = false;
            if (this._longlen == 0L)
            {
                this._longlen = (ulong) this.ReadInt64();
            }
            if (this._longlen == ulong.MaxValue)
            {
                this._longlen = 0L;
                this._longlenleft = 0L;
                flag = true;
            }
            else
            {
                uint num = this.ReadUInt32();
                if (num == 0)
                {
                    this._longlenleft = 0L;
                    this._longlen = 0L;
                }
                else
                {
                    this._longlenleft = num;
                }
            }
            if (flag && returnPlpNullIfNull)
            {
                return ulong.MaxValue;
            }
            return this._longlenleft;
        }

        internal float ReadSingle()
        {
            if (((this._inBytesUsed + 4) > this._inBytesRead) || (this._inBytesPacket < 4))
            {
                this.ReadByteArray(this._bTmp, 0, 4);
                return BitConverter.ToSingle(this._bTmp, 0);
            }
            float num = BitConverter.ToSingle(this._inBuff, this._inBytesUsed);
            this._inBytesUsed += 4;
            this._inBytesPacket -= 4;
            return num;
        }

        internal void ReadSni(DbAsyncResult asyncResult, TdsParserStateObject stateObj)
        {
            if ((this._parser.State != TdsParserState.Broken) && (this._parser.State != TdsParserState.Closed))
            {
                IntPtr zero = IntPtr.Zero;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    uint num;
                    if (!this._parser.AsyncOn)
                    {
                        num = SNINativeMethodWrapper.SNIReadSync(stateObj.Handle, ref zero, TdsParserStaticMethods.GetTimeoutMilliseconds(stateObj.TimeoutTime));
                        if (num == 0)
                        {
                            stateObj.ProcessSniPacket(zero, 0);
                        }
                        else
                        {
                            this.ReadSniError(stateObj, num);
                        }
                    }
                    else
                    {
                        stateObj._asyncResult = asyncResult;
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                        }
                        finally
                        {
                            stateObj.IncrementPendingCallbacks();
                            num = SNINativeMethodWrapper.SNIReadAsync(stateObj.Handle, ref zero);
                            if ((num != 0) && (0x3e5 != num))
                            {
                                stateObj.DecrementPendingCallbacks(false);
                            }
                        }
                        if (num == 0)
                        {
                            stateObj._asyncResult.SetCompletedSynchronously();
                            stateObj.ReadAsyncCallback(ADP.PtrZero, zero, 0);
                        }
                        else if (0x3e5 != num)
                        {
                            this.ReadSniError(stateObj, num);
                        }
                    }
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        SNINativeMethodWrapper.SNIPacketRelease(zero);
                    }
                }
            }
        }

        private void ReadSniError(TdsParserStateObject stateObj, uint error)
        {
            if (this._parser._fAwaitingPreLogin && (error != 0x102))
            {
                this._parser._fPreLoginErrorOccurred = true;
                return;
            }
            if (0x102 != error)
            {
                this._parser.Errors.Add(this._parser.ProcessSNIError(stateObj));
                goto Label_011E;
            }
            bool flag = false;
            if (this._internalTimeout)
            {
                flag = true;
            }
            else
            {
                stateObj._internalTimeout = true;
                this._parser.Errors.Add(new SqlError(-2, 0, 11, this._parser.Server, SQLMessage.Timeout(), "", 0));
                if (!stateObj._attentionSent)
                {
                    if (stateObj.Parser.State == TdsParserState.OpenLoggedIn)
                    {
                        stateObj.SendAttention();
                        IntPtr zero = IntPtr.Zero;
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                            error = SNINativeMethodWrapper.SNIReadSync(stateObj.Handle, ref zero, TdsParserStaticMethods.GetTimeoutMilliseconds(stateObj.TimeoutTime));
                            if (error == 0)
                            {
                                stateObj.ProcessSniPacket(zero, 0);
                                return;
                            }
                            flag = true;
                            goto Label_00E1;
                        }
                        finally
                        {
                            if (zero != IntPtr.Zero)
                            {
                                SNINativeMethodWrapper.SNIPacketRelease(zero);
                            }
                        }
                    }
                    flag = true;
                }
            }
        Label_00E1:
            if (flag)
            {
                this._parser.State = TdsParserState.Broken;
                this._parser.Connection.BreakConnection();
            }
        Label_011E:
            this._parser.ThrowExceptionAndWarning();
        }

        internal void ReadSniSyncOverAsync()
        {
            if ((this._parser.State != TdsParserState.Broken) && (this._parser.State != TdsParserState.Closed))
            {
                try
                {
                    if (!((IAsyncResult) this._cachedAsyncResult).AsyncWaitHandle.WaitOne(TdsParserStaticMethods.GetTimeoutMilliseconds(this.TimeoutTime), false))
                    {
                        bool flag = false;
                        if (this._internalTimeout)
                        {
                            flag = true;
                        }
                        else
                        {
                            this._internalTimeout = true;
                            this._parser.Errors.Add(new SqlError(-2, 0, 11, this._parser.Server, SQLMessage.Timeout(), "", 0));
                            if (!this._attentionSent)
                            {
                                if (this._parser.State == TdsParserState.OpenLoggedIn)
                                {
                                    this.SendAttention();
                                }
                                else
                                {
                                    flag = true;
                                }
                                if (!((IAsyncResult) this._cachedAsyncResult).AsyncWaitHandle.WaitOne(TdsParserStaticMethods.GetTimeoutMilliseconds(this.TimeoutTime), false))
                                {
                                    flag = true;
                                }
                            }
                        }
                        if (flag)
                        {
                            this._parser.State = TdsParserState.Broken;
                            this._parser.Connection.BreakConnection();
                            this._parser.ThrowExceptionAndWarning();
                        }
                    }
                    if (this._error != null)
                    {
                        this.Parser.Errors.Add(this._error);
                        this._error = null;
                        this._parser.ThrowExceptionAndWarning();
                    }
                }
                finally
                {
                    if (this._cachedAsyncResult != null)
                    {
                        this._cachedAsyncResult.Reset();
                    }
                }
            }
        }

        internal string ReadString(int length)
        {
            byte[] buffer;
            int len = length << 1;
            int index = 0;
            if (((this._inBytesUsed + len) > this._inBytesRead) || (this._inBytesPacket < len))
            {
                if ((this._bTmp == null) || (this._bTmp.Length < len))
                {
                    this._bTmp = new byte[len];
                }
                this.ReadByteArray(this._bTmp, 0, len);
                buffer = this._bTmp;
            }
            else
            {
                buffer = this._inBuff;
                index = this._inBytesUsed;
                this._inBytesUsed += len;
                this._inBytesPacket -= len;
            }
            return Encoding.Unicode.GetString(buffer, index, len);
        }

        internal string ReadStringWithEncoding(int length, Encoding encoding, bool isPlp)
        {
            if (encoding == null)
            {
                this._parser.ThrowUnsupportedCollationEncountered(this);
            }
            byte[] buff = null;
            int index = 0;
            if (isPlp)
            {
                length = this.ReadPlpBytes(ref buff, 0, 0x7fffffff);
            }
            else if (((this._inBytesUsed + length) > this._inBytesRead) || (this._inBytesPacket < length))
            {
                if ((this._bTmp == null) || (this._bTmp.Length < length))
                {
                    this._bTmp = new byte[length];
                }
                this.ReadByteArray(this._bTmp, 0, length);
                buff = this._bTmp;
            }
            else
            {
                buff = this._inBuff;
                index = this._inBytesUsed;
                this._inBytesUsed += length;
                this._inBytesPacket -= length;
            }
            return encoding.GetString(buff, index, length);
        }

        internal ushort ReadUInt16()
        {
            byte num2 = this.ReadByte();
            return (ushort) ((this.ReadByte() << 8) + num2);
        }

        internal uint ReadUInt32()
        {
            if (((this._inBytesUsed + 4) > this._inBytesRead) || (this._inBytesPacket < 4))
            {
                this.ReadByteArray(this._bTmp, 0, 4);
                return BitConverter.ToUInt32(this._bTmp, 0);
            }
            uint num = BitConverter.ToUInt32(this._inBuff, this._inBytesUsed);
            this._inBytesUsed += 4;
            this._inBytesPacket -= 4;
            return num;
        }

        internal void ResetBuffer()
        {
            this._outBytesUsed = this._outputHeaderLen;
        }

        private void ResetCancelAndProcessAttention()
        {
            lock (this)
            {
                this._cancelled = false;
                this._allowObjectID = -1;
                if (this._attentionSent)
                {
                    this.Parser.ProcessPendingAck(this);
                }
                this._internalTimeout = false;
            }
        }

        internal void SendAttention()
        {
            if (!this._attentionSent && ((this._parser.State != TdsParserState.Closed) && (this._parser.State != TdsParserState.Broken)))
            {
                uint num;
                SNIPacket packet = new SNIPacket(this.Handle);
                if (this._parser.AsyncOn)
                {
                    this._sniAsyncAttnPacket = packet;
                    if (this._asyncAttentionResult == null)
                    {
                        this._asyncAttentionResult = new DbAsyncResult(this._parser, string.Empty, null, null, null);
                    }
                }
                else
                {
                    this._sniAsyncAttnPacket = null;
                }
                SNINativeMethodWrapper.SNIPacketSetData(packet, SQL.AttentionHeader, 8);
                if (this._parser.AsyncOn)
                {
                    num = this.SNIWriteAsync(this.Handle, packet, this._asyncAttentionResult);
                    Bid.Trace("<sc.TdsParser.SendAttention|Info> Send Attention ASync .\n");
                }
                else
                {
                    num = SNINativeMethodWrapper.SNIWriteSync(this.Handle, packet);
                    Bid.Trace("<sc.TdsParser.SendAttention|Info> Send Attention Sync.\n");
                    if (num != 0)
                    {
                        Bid.Trace("<sc.TdsParser.SendAttention|Info> SNIWriteSync returned error code %d\n", (int) num);
                        this._parser.Errors.Add(this._parser.ProcessSNIError(this));
                        this._parser.ThrowExceptionAndWarning();
                    }
                }
                this.SetTimeoutSeconds(5);
                this._attentionSent = true;
                if (Bid.AdvancedOn)
                {
                    Bid.TraceBin("<sc.TdsParser.WritePacket|INFO|ADV>  Packet sent", this._outBuff, (ushort) this._outBytesUsed);
                }
                Bid.Trace("<sc.TdsParser.SendAttention|Info> Attention sent to the server.\n");
            }
        }

        internal bool SetPacketSize(int size)
        {
            if (size > 0x8000)
            {
                throw SQL.InvalidPacketSize();
            }
            if ((this._inBuff != null) && (this._inBuff.Length == size))
            {
                return false;
            }
            if (this._inBuff == null)
            {
                this._inBuff = new byte[size];
                this._inBytesRead = 0;
                this._inBytesUsed = 0;
            }
            else if (size != this._inBuff.Length)
            {
                if (this._inBytesRead > this._inBytesUsed)
                {
                    byte[] src = this._inBuff;
                    this._inBuff = new byte[size];
                    int count = this._inBytesRead - this._inBytesUsed;
                    if ((src.Length < (this._inBytesUsed + count)) || (this._inBuff.Length < count))
                    {
                        throw SQL.InvalidInternalPacketSize(string.Concat(new object[] { Res.GetString("SQL_InvalidInternalPacketSize"), ' ', src.Length, ", ", this._inBytesUsed, ", ", count, ", ", this._inBuff.Length }));
                    }
                    Buffer.BlockCopy(src, this._inBytesUsed, this._inBuff, 0, count);
                    this._inBytesRead -= this._inBytesUsed;
                    this._inBytesUsed = 0;
                }
                else
                {
                    this._inBuff = new byte[size];
                    this._inBytesRead = 0;
                    this._inBytesUsed = 0;
                }
            }
            this._outBuff = new byte[size];
            this._outBytesUsed = this._outputHeaderLen;
            return true;
        }

        internal void SetTimeoutSeconds(int timeout)
        {
            this._timeoutSeconds = timeout;
            if (timeout == 0)
            {
                this._timeoutTime = 0x7fffffffffffffffL;
            }
        }

        private void SniReadStatisticsAndTracing()
        {
            SqlStatistics statistics = this.Parser.Statistics;
            if (statistics != null)
            {
                if (statistics.WaitForReply)
                {
                    statistics.SafeIncrement(ref statistics._serverRoundtrips);
                    statistics.ReleaseAndUpdateNetworkServerTimer();
                }
                statistics.SafeAdd(ref statistics._bytesReceived, (long) this._inBytesRead);
                statistics.SafeIncrement(ref statistics._buffersReceived);
            }
        }

        private uint SNIWriteAsync(SNIHandle handle, SNIPacket packet, DbAsyncResult asyncResult)
        {
            uint num;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this.IncrementPendingCallbacks();
                num = SNINativeMethodWrapper.SNIWriteAsync(handle, packet);
                if ((num == 0) || (num != 0x3e5))
                {
                    this.DecrementPendingCallbacks(false);
                }
            }
            if (num != 0)
            {
                if (num != 0x3e5)
                {
                    Bid.Trace("<sc.TdsParser.WritePacket|Info> write async returned error code %d\n", (int) num);
                    this._parser.Errors.Add(this._parser.ProcessSNIError(this));
                    this.ThrowExceptionAndWarning();
                    return num;
                }
                if (num != 0x3e5)
                {
                    return num;
                }
                try
                {
                    ((IAsyncResult) asyncResult).AsyncWaitHandle.WaitOne();
                    if (this._error != null)
                    {
                        this._parser.Errors.Add(this._error);
                        this._error = null;
                        Bid.Trace("<sc.TdsParser.WritePacket|Info> write async returned error code %d\n", (int) num);
                        this.ThrowExceptionAndWarning();
                    }
                }
                finally
                {
                    asyncResult.Reset();
                }
            }
            return num;
        }

        private void SniWriteStatisticsAndTracing()
        {
            SqlStatistics statistics = this._parser.Statistics;
            if (statistics != null)
            {
                statistics.SafeIncrement(ref statistics._buffersSent);
                statistics.SafeAdd(ref statistics._bytesSent, (long) this._outBytesUsed);
                statistics.RequestNetworkServerTimer();
            }
            if (Bid.AdvancedOn)
            {
                if (this._tracePasswordOffset != 0)
                {
                    for (int i = this._tracePasswordOffset; i < (this._tracePasswordOffset + this._tracePasswordLength); i++)
                    {
                        this._outBuff[i] = 0;
                    }
                    this._tracePasswordOffset = 0;
                    this._tracePasswordLength = 0;
                }
                if (this._traceChangePasswordOffset != 0)
                {
                    for (int j = this._traceChangePasswordOffset; j < (this._traceChangePasswordOffset + this._traceChangePasswordLength); j++)
                    {
                        this._outBuff[j] = 0;
                    }
                    this._traceChangePasswordOffset = 0;
                    this._traceChangePasswordLength = 0;
                }
                Bid.TraceBin("<sc.TdsParser.WritePacket|INFO|ADV>  Packet sent", this._outBuff, (ushort) this._outBytesUsed);
            }
        }

        internal void StartSession(int objectID)
        {
            this._allowObjectID = objectID;
        }

        private void ThrowExceptionAndWarning()
        {
            this.Parser.ThrowExceptionAndWarning();
        }

        public void WriteAsyncCallback(IntPtr key, IntPtr packet, uint error)
        {
            DbAsyncResult result = this._asyncResult;
            if ((this._sniAsyncAttnPacket != null) && (this._sniAsyncAttnPacket.DangerousGetHandle() == packet))
            {
                result = this._asyncAttentionResult;
            }
            bool flag = true;
            try
            {
                if (this._parser.MARSOn)
                {
                    this.CheckSetResetConnectionState(error, CallbackType.Read);
                }
                if (error != 0)
                {
                    this._error = this._parser.ProcessSNIError(this);
                }
            }
            catch (Exception exception)
            {
                flag = ADP.IsCatchableExceptionType(exception);
                throw;
            }
            finally
            {
                this.DecrementPendingCallbacks(false);
                if (flag)
                {
                    result.SetCompleted();
                }
            }
        }

        internal void WritePacket(byte flushMode)
        {
            if ((((this._parser.State != TdsParserState.Closed) && (this._parser.State != TdsParserState.Broken)) && ((!this._parser.IsYukonOrNewer || this._bulkCopyOpperationInProgress) || ((this._outBytesUsed != (this._outputHeaderLen + BitConverter.ToInt32(this._outBuff, this._outputHeaderLen))) || (this._outputPacketNumber != 1)))) && ((this._outBytesUsed != this._outputHeaderLen) || (this._outputPacketNumber != 1)))
            {
                byte num = 1;
                byte num2 = this._outputPacketNumber;
                if (1 == flushMode)
                {
                    num = 1;
                    this._outputPacketNumber = 1;
                }
                else if (flushMode == 0)
                {
                    num = 4;
                    this._outputPacketNumber = (byte) (this._outputPacketNumber + 1);
                }
                this._outBuff[0] = this._outputMessageType;
                this._outBuff[1] = num;
                this._outBuff[2] = (byte) (this._outBytesUsed >> 8);
                this._outBuff[3] = (byte) (this._outBytesUsed & 0xff);
                this._outBuff[4] = 0;
                this._outBuff[5] = 0;
                this._outBuff[6] = num2;
                this._outBuff[7] = 0;
                this._parser.CheckResetConnection(this);
                this.WriteSni();
            }
        }

        private void WriteSni()
        {
            uint num;
            if (this._sniPacket == null)
            {
                this._sniPacket = new SNIPacket(this.Handle);
            }
            else
            {
                SNINativeMethodWrapper.SNIPacketReset(this.Handle, SNINativeMethodWrapper.IOType.WRITE, this._sniPacket, SNINativeMethodWrapper.ConsumerNumber.SNI_Consumer_SNI);
            }
            SNINativeMethodWrapper.SNIPacketSetData(this._sniPacket, this._outBuff, this._outBytesUsed);
            if (this._parser.AsyncOn)
            {
                if (this._cachedAsyncResult == null)
                {
                    this._cachedAsyncResult = new DbAsyncResult(this._parser, string.Empty, null, null, null);
                }
                this._asyncResult = this._cachedAsyncResult;
                num = this.SNIWriteAsync(this.Handle, this._sniPacket, this._cachedAsyncResult);
            }
            else
            {
                num = SNINativeMethodWrapper.SNIWriteSync(this.Handle, this._sniPacket);
                if (num != 0)
                {
                    Bid.Trace("<sc.TdsParser.WritePacket|Info> write sync returned error code %d\n", (int) num);
                    this._parser.Errors.Add(this._parser.ProcessSNIError(this));
                    this.ThrowExceptionAndWarning();
                }
                if (this._bulkCopyOpperationInProgress && (TdsParserStaticMethods.GetTimeoutMilliseconds(this.TimeoutTime) == 0))
                {
                    this._parser.Errors.Add(new SqlError(-2, 0, 11, this._parser.Server, SQLMessage.Timeout(), "", 0));
                    this.SendAttention();
                    this._parser.ProcessPendingAck(this);
                    this._parser.ThrowExceptionAndWarning();
                }
            }
            if ((this._parser.State == TdsParserState.OpenNotLoggedIn) && (this._parser.EncryptionOptions == EncryptionOptions.LOGIN))
            {
                this._parser.RemoveEncryption();
                this._parser.EncryptionOptions = EncryptionOptions.OFF;
                this._sniPacket.Dispose();
                this._sniPacket = new SNIPacket(this.Handle);
            }
            this.SniWriteStatisticsAndTracing();
            this.ResetBuffer();
        }

        internal bool BcpLock
        {
            get
            {
                return this._bcpLock;
            }
            set
            {
                this._bcpLock = value;
            }
        }

        internal SNIHandle Handle
        {
            get
            {
                return this._sessionHandle;
            }
        }

        internal bool HasOpenResult
        {
            get
            {
                return this._hasOpenResult;
            }
        }

        internal bool IsOrphaned
        {
            get
            {
                return ((this._activateCount != 0) && !this._owner.IsAlive);
            }
        }

        internal TdsParserStateObject NextPooledObject
        {
            get
            {
                return this._nextPooledObject;
            }
            set
            {
                this._nextPooledObject = value;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal object Owner
        {
            set
            {
                this._owner.Target = value;
            }
        }

        internal TdsParser Parser
        {
            get
            {
                return this._parser;
            }
        }

        internal System.Data.SqlClient.SniContext SniContext
        {
            get
            {
                return this._sniContext;
            }
            set
            {
                this._sniContext = value;
            }
        }

        internal uint Status
        {
            get
            {
                if (this._sessionHandle != null)
                {
                    return this._sessionHandle.Status;
                }
                return uint.MaxValue;
            }
        }

        internal bool TimeoutHasExpired
        {
            get
            {
                return TdsParserStaticMethods.TimeoutHasExpired(this._timeoutTime);
            }
        }

        internal long TimeoutTime
        {
            get
            {
                if (this._timeoutSeconds != 0)
                {
                    this._timeoutTime = TdsParserStaticMethods.GetTimeoutSeconds(this._timeoutSeconds);
                    this._timeoutSeconds = 0;
                }
                return this._timeoutTime;
            }
            set
            {
                this._timeoutSeconds = 0;
                this._timeoutTime = value;
            }
        }
    }
}

