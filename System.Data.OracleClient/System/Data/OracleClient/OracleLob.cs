namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public sealed class OracleLob : Stream, ICloneable, IDisposable, INullable
    {
        private OCI.CHARSETFORM _charsetForm;
        private long _currentPosition;
        private bool _isNull;
        private byte _isTemporaryState;
        private OciLobLocator _lobLocator;
        private OracleType _lobType;
        public static readonly OracleLob Null = new OracleLob();
        private const byte x_IsNotTemporary = 2;
        private const byte x_IsTemporary = 1;
        private const byte x_IsTemporaryUnknown = 0;

        internal OracleLob()
        {
            this._isNull = true;
            this._lobType = OracleType.Blob;
        }

        internal OracleLob(OciLobLocator lobLocator)
        {
            this._lobLocator = lobLocator.Clone();
            this._lobType = this._lobLocator.LobType;
            this._charsetForm = (OracleType.NClob == this._lobType) ? OCI.CHARSETFORM.SQLCS_NCHAR : OCI.CHARSETFORM.SQLCS_IMPLICIT;
        }

        internal OracleLob(OracleLob lob)
        {
            this._lobLocator = lob._lobLocator.Clone();
            this._lobType = lob._lobLocator.LobType;
            this._charsetForm = lob._charsetForm;
            this._currentPosition = lob._currentPosition;
            this._isTemporaryState = lob._isTemporaryState;
        }

        internal OracleLob(OracleConnection connection, OracleType oracleType)
        {
            this._lobLocator = new OciLobLocator(connection, oracleType);
            this._lobType = oracleType;
            this._charsetForm = (OracleType.NClob == this._lobType) ? OCI.CHARSETFORM.SQLCS_NCHAR : OCI.CHARSETFORM.SQLCS_IMPLICIT;
            this._isTemporaryState = 1;
            OCI.LOB_TYPE lobtype = (OracleType.Blob == oracleType) ? OCI.LOB_TYPE.OCI_TEMP_BLOB : OCI.LOB_TYPE.OCI_TEMP_CLOB;
            int rc = TracedNativeMethods.OCILobCreateTemporary(connection.ServiceContextHandle, connection.ErrorHandle, this._lobLocator.Descriptor, 0, this._charsetForm, lobtype, 0, OCI.DURATION.OCI_DURATION_BEGIN);
            if (rc != 0)
            {
                connection.CheckError(this.ErrorHandle, rc);
            }
        }

        internal int AdjustOffsetToOracle(int amount)
        {
            return (this.IsCharacterLob ? (amount / 2) : amount);
        }

        internal long AdjustOffsetToOracle(long amount)
        {
            return (this.IsCharacterLob ? (amount / 2L) : amount);
        }

        internal int AdjustOracleToOffset(int amount)
        {
            return (this.IsCharacterLob ? (amount * 2) : amount);
        }

        internal long AdjustOracleToOffset(long amount)
        {
            return (this.IsCharacterLob ? (amount * 2L) : amount);
        }

        public void Append(OracleLob source)
        {
            if (source == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("source");
            }
            this.AssertObjectNotDisposed();
            source.AssertObjectNotDisposed();
            if (this.IsNull)
            {
                throw System.Data.Common.ADP.LobWriteInvalidOnNull();
            }
            if (!source.IsNull)
            {
                this.AssertConnectionIsOpen();
                int rc = TracedNativeMethods.OCILobAppend(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor, source.Descriptor);
                if (rc != 0)
                {
                    this.Connection.CheckError(this.ErrorHandle, rc);
                }
            }
        }

        internal void AssertAmountIsEven(long amount, string argName)
        {
            if (this.IsCharacterLob && (1L == (amount & 1L)))
            {
                throw System.Data.Common.ADP.LobAmountMustBeEven(argName);
            }
        }

        internal void AssertAmountIsValid(long amount, string argName)
        {
            this.AssertAmountIsValidOddOK(amount, argName);
            this.AssertAmountIsEven(amount, argName);
        }

        internal void AssertAmountIsValidOddOK(long amount, string argName)
        {
            if ((amount < 0L) || (amount >= 0xffffffffL))
            {
                throw System.Data.Common.ADP.LobAmountExceeded(argName);
            }
        }

        internal void AssertConnectionIsOpen()
        {
            if (this.ConnectionIsClosed)
            {
                throw System.Data.Common.ADP.ClosedConnectionError();
            }
        }

        internal void AssertObjectNotDisposed()
        {
            if (this.IsDisposed)
            {
                throw System.Data.Common.ADP.ObjectDisposed("OracleLob");
            }
        }

        internal void AssertPositionIsValid()
        {
            if (this.IsCharacterLob && (1L == (this._currentPosition & 1L)))
            {
                throw System.Data.Common.ADP.LobPositionMustBeEven();
            }
        }

        internal void AssertTransactionExists()
        {
            if (!this.Connection.HasTransaction)
            {
                throw System.Data.Common.ADP.LobWriteRequiresTransaction();
            }
        }

        public void BeginBatch()
        {
            this.BeginBatch(OracleLobOpenMode.ReadOnly);
        }

        public void BeginBatch(OracleLobOpenMode mode)
        {
            this.AssertObjectNotDisposed();
            if (!this.IsNull)
            {
                this.AssertConnectionIsOpen();
                this.LobLocator.Open(mode);
            }
        }

        public object Clone()
        {
            this.AssertObjectNotDisposed();
            if (this.IsNull)
            {
                return Null;
            }
            this.AssertConnectionIsOpen();
            return new OracleLob(this);
        }

        public long CopyTo(OracleLob destination)
        {
            return this.CopyTo(0L, destination, 0L, this.Length);
        }

        public long CopyTo(OracleLob destination, long destinationOffset)
        {
            return this.CopyTo(0L, destination, destinationOffset, this.Length);
        }

        public long CopyTo(long sourceOffset, OracleLob destination, long destinationOffset, long amount)
        {
            if (destination == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("destination");
            }
            this.AssertObjectNotDisposed();
            destination.AssertObjectNotDisposed();
            this.AssertAmountIsValid(amount, "amount");
            this.AssertAmountIsValid(sourceOffset, "sourceOffset");
            this.AssertAmountIsValid(destinationOffset, "destinationOffset");
            if (destination.IsNull)
            {
                throw System.Data.Common.ADP.LobWriteInvalidOnNull();
            }
            if (this.IsNull)
            {
                return 0L;
            }
            this.AssertConnectionIsOpen();
            this.AssertTransactionExists();
            long num = this.AdjustOffsetToOracle(Math.Min(this.Length - sourceOffset, amount));
            long num5 = this.AdjustOffsetToOracle(destinationOffset) + 1L;
            long num4 = this.AdjustOffsetToOracle(sourceOffset) + 1L;
            if (0L >= num)
            {
                return 0L;
            }
            int rc = TracedNativeMethods.OCILobCopy(this.ServiceContextHandle, this.ErrorHandle, destination.Descriptor, this.Descriptor, (uint) num, (uint) num5, (uint) num4);
            if (rc != 0)
            {
                this.Connection.CheckError(this.ErrorHandle, rc);
            }
            return this.AdjustOracleToOffset(num);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((disposing && !this.IsNull) && !this.ConnectionIsClosed)
                {
                    this.Flush();
                    OciLobLocator.SafeDispose(ref this._lobLocator);
                    this._lobLocator = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public void EndBatch()
        {
            this.AssertObjectNotDisposed();
            if (!this.IsNull)
            {
                this.AssertConnectionIsOpen();
                this.LobLocator.ForceClose();
            }
        }

        public long Erase()
        {
            return this.Erase(0L, this.Length);
        }

        public long Erase(long offset, long amount)
        {
            this.AssertObjectNotDisposed();
            if (this.IsNull)
            {
                throw System.Data.Common.ADP.LobWriteInvalidOnNull();
            }
            this.AssertAmountIsValid(amount, "amount");
            this.AssertAmountIsEven(offset, "offset");
            this.AssertPositionIsValid();
            this.AssertConnectionIsOpen();
            this.AssertTransactionExists();
            if ((offset < 0L) || (offset >= 0xffffffffL))
            {
                return 0L;
            }
            uint num4 = (uint) this.AdjustOffsetToOracle(amount);
            uint num3 = ((uint) this.AdjustOffsetToOracle(offset)) + 1;
            int rc = TracedNativeMethods.OCILobErase(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor, ref num4, num3);
            if (rc != 0)
            {
                this.Connection.CheckError(this.ErrorHandle, rc);
            }
            return this.AdjustOracleToOffset((long) num4);
        }

        public override void Flush()
        {
        }

        internal void Free()
        {
            int rc = TracedNativeMethods.OCILobFreeTemporary(this._lobLocator.ServiceContextHandle, this._lobLocator.ErrorHandle, this._lobLocator.Descriptor);
            if (rc != 0)
            {
                this._lobLocator.Connection.CheckError(this.ErrorHandle, rc);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.AssertObjectNotDisposed();
            if (count < 0)
            {
                throw System.Data.Common.ADP.MustBePositive("count");
            }
            if (offset < 0)
            {
                throw System.Data.Common.ADP.MustBePositive("offset");
            }
            if (buffer == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("buffer");
            }
            if (buffer.Length < (offset + count))
            {
                throw System.Data.Common.ADP.BufferExceeded("count");
            }
            if (this.IsNull || (count == 0))
            {
                return 0;
            }
            this.AssertConnectionIsOpen();
            this.AssertAmountIsValidOddOK((long) offset, "offset");
            this.AssertAmountIsValidOddOK((long) count, "count");
            uint num5 = (uint) this._currentPosition;
            int srcOffset = 0;
            int num8 = 0;
            int num6 = 0;
            byte[] buffer2 = buffer;
            int num7 = offset;
            int amount = count;
            if (this.IsCharacterLob)
            {
                srcOffset = ((int) num5) & 1;
                num8 = offset & 1;
                num6 = count & 1;
                num5 /= 2;
                if (((1 == num8) || (1 == srcOffset)) || (1 == num6))
                {
                    num7 = 0;
                    amount = (count + num6) + (2 * srcOffset);
                    buffer2 = new byte[amount];
                }
            }
            ushort csid = this.IsCharacterLob ? ((ushort) 0x3e8) : ((ushort) 0);
            int rc = 0;
            int amtp = this.AdjustOffsetToOracle(amount);
            GCHandle handle = new GCHandle();
            try
            {
                handle = GCHandle.Alloc(buffer2, GCHandleType.Pinned);
                IntPtr bufp = new IntPtr(((long) handle.AddrOfPinnedObject()) + num7);
                rc = TracedNativeMethods.OCILobRead(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor, ref amtp, num5 + 1, bufp, (uint) amount, csid, this._charsetForm);
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            if (0x63 == rc)
            {
                rc = 0;
            }
            if (100 == rc)
            {
                return 0;
            }
            if (rc != 0)
            {
                this.Connection.CheckError(this.ErrorHandle, rc);
            }
            amtp = this.AdjustOracleToOffset(amtp);
            if (buffer2 != buffer)
            {
                if (amtp >= count)
                {
                    amtp = count;
                }
                else
                {
                    amtp -= srcOffset;
                }
                Buffer.BlockCopy(buffer2, srcOffset, buffer, offset, amtp);
                buffer2 = null;
            }
            this._currentPosition += amtp;
            return amtp;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.AssertObjectNotDisposed();
            if (this.IsNull)
            {
                return 0L;
            }
            long num = offset;
            long length = this.Length;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    num = offset;
                    break;

                case SeekOrigin.Current:
                    num = this._currentPosition + offset;
                    break;

                case SeekOrigin.End:
                    num = length + offset;
                    break;

                default:
                    throw System.Data.Common.ADP.InvalidSeekOrigin(origin);
            }
            if ((num < 0L) || (num > length))
            {
                throw System.Data.Common.ADP.SeekBeyondEnd("offset");
            }
            this._currentPosition = num;
            return this._currentPosition;
        }

        public override void SetLength(long value)
        {
            this.AssertObjectNotDisposed();
            if (this.IsNull)
            {
                throw System.Data.Common.ADP.LobWriteInvalidOnNull();
            }
            this.AssertConnectionIsOpen();
            this.AssertAmountIsValid(value, "value");
            this.AssertTransactionExists();
            uint newlen = (uint) this.AdjustOffsetToOracle(value);
            int rc = TracedNativeMethods.OCILobTrim(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor, newlen);
            if (rc != 0)
            {
                this.Connection.CheckError(this.ErrorHandle, rc);
            }
            this._currentPosition = Math.Min(this._currentPosition, value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.AssertObjectNotDisposed();
            this.AssertConnectionIsOpen();
            if (count < 0)
            {
                throw System.Data.Common.ADP.MustBePositive("count");
            }
            if (offset < 0)
            {
                throw System.Data.Common.ADP.MustBePositive("offset");
            }
            if (buffer == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("buffer");
            }
            if (buffer.Length < (offset + count))
            {
                throw System.Data.Common.ADP.BufferExceeded("count");
            }
            this.AssertTransactionExists();
            if (this.IsNull)
            {
                throw System.Data.Common.ADP.LobWriteInvalidOnNull();
            }
            this.AssertAmountIsValid((long) offset, "offset");
            this.AssertAmountIsValid((long) count, "count");
            this.AssertPositionIsValid();
            OCI.CHARSETFORM csfrm = this._charsetForm;
            ushort csid = this.IsCharacterLob ? ((ushort) 0x3e8) : ((ushort) 0);
            int amtp = this.AdjustOffsetToOracle(count);
            int rc = 0;
            if (amtp != 0)
            {
                GCHandle handle = new GCHandle();
                try
                {
                    handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    IntPtr bufp = new IntPtr(((long) handle.AddrOfPinnedObject()) + offset);
                    rc = TracedNativeMethods.OCILobWrite(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor, ref amtp, this.CurrentOraclePosition, bufp, (uint) count, 0, csid, csfrm);
                }
                finally
                {
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                }
                if (rc != 0)
                {
                    this.Connection.CheckError(this.ErrorHandle, rc);
                }
                amtp = this.AdjustOracleToOffset(amtp);
                this._currentPosition += amtp;
            }
        }

        public override void WriteByte(byte value)
        {
            if ((OracleType.Clob == this._lobType) || (OracleType.NClob == this._lobType))
            {
                throw System.Data.Common.ADP.WriteByteForBinaryLobsOnly();
            }
            base.WriteByte(value);
        }

        public override bool CanRead
        {
            get
            {
                return (this.IsNull || !this.IsDisposed);
            }
        }

        public override bool CanSeek
        {
            get
            {
                return (this.IsNull || !this.IsDisposed);
            }
        }

        public override bool CanWrite
        {
            get
            {
                bool flag = OracleType.BFile != this._lobType;
                if (!this.IsNull)
                {
                    flag = !this.IsDisposed;
                }
                return flag;
            }
        }

        public int ChunkSize
        {
            get
            {
                this.AssertObjectNotDisposed();
                if (this.IsNull)
                {
                    return 0;
                }
                this.AssertConnectionIsOpen();
                uint lenp = 0;
                int rc = TracedNativeMethods.OCILobGetChunkSize(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor, out lenp);
                if (rc != 0)
                {
                    this.Connection.CheckError(this.ErrorHandle, rc);
                }
                return (int) lenp;
            }
        }

        public OracleConnection Connection
        {
            get
            {
                this.AssertObjectNotDisposed();
                OciLobLocator lobLocator = this.LobLocator;
                if (lobLocator == null)
                {
                    return null;
                }
                return lobLocator.Connection;
            }
        }

        private bool ConnectionIsClosed
        {
            get
            {
                if (this.LobLocator != null)
                {
                    return this.LobLocator.ConnectionIsClosed;
                }
                return true;
            }
        }

        private uint CurrentOraclePosition
        {
            get
            {
                return (((uint) this.AdjustOffsetToOracle(this._currentPosition)) + 1);
            }
        }

        internal OciHandle Descriptor
        {
            get
            {
                return this.LobLocator.Descriptor;
            }
        }

        internal OciErrorHandle ErrorHandle
        {
            get
            {
                return this.LobLocator.ErrorHandle;
            }
        }

        public bool IsBatched
        {
            get
            {
                int num2;
                if ((this.IsNull || this.IsDisposed) || this.ConnectionIsClosed)
                {
                    return false;
                }
                int rc = TracedNativeMethods.OCILobIsOpen(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor, out num2);
                if (rc != 0)
                {
                    this.Connection.CheckError(this.ErrorHandle, rc);
                }
                return (num2 != 0);
            }
        }

        private bool IsCharacterLob
        {
            get
            {
                if (OracleType.Clob != this._lobType)
                {
                    return (OracleType.NClob == this._lobType);
                }
                return true;
            }
        }

        private bool IsDisposed
        {
            get
            {
                return (!this._isNull && (null == this.LobLocator));
            }
        }

        public bool IsNull
        {
            get
            {
                return this._isNull;
            }
        }

        public bool IsTemporary
        {
            get
            {
                this.AssertObjectNotDisposed();
                if (this.IsNull)
                {
                    return false;
                }
                this.AssertConnectionIsOpen();
                if (this._isTemporaryState == 0)
                {
                    int num2;
                    int rc = TracedNativeMethods.OCILobIsTemporary(this.Connection.EnvironmentHandle, this.ErrorHandle, this.Descriptor, out num2);
                    if (rc != 0)
                    {
                        this.Connection.CheckError(this.ErrorHandle, rc);
                    }
                    this._isTemporaryState = (num2 != 0) ? ((byte) 1) : ((byte) 2);
                }
                return (1 == this._isTemporaryState);
            }
        }

        public override long Length
        {
            get
            {
                uint num2;
                this.AssertObjectNotDisposed();
                if (this.IsNull)
                {
                    return 0L;
                }
                this.AssertConnectionIsOpen();
                int rc = TracedNativeMethods.OCILobGetLength(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor, out num2);
                if (rc != 0)
                {
                    this.Connection.CheckError(this.ErrorHandle, rc);
                }
                return this.AdjustOracleToOffset((long) num2);
            }
        }

        internal OciLobLocator LobLocator
        {
            get
            {
                return this._lobLocator;
            }
        }

        public OracleType LobType
        {
            get
            {
                return this._lobType;
            }
        }

        public override long Position
        {
            get
            {
                this.AssertObjectNotDisposed();
                if (this.IsNull)
                {
                    return 0L;
                }
                this.AssertConnectionIsOpen();
                return this._currentPosition;
            }
            set
            {
                if (!this.IsNull)
                {
                    this.Seek(value, SeekOrigin.Begin);
                }
            }
        }

        internal OciServiceContextHandle ServiceContextHandle
        {
            get
            {
                return this.LobLocator.ServiceContextHandle;
            }
        }

        public object Value
        {
            get
            {
                string str;
                this.AssertObjectNotDisposed();
                if (this.IsNull)
                {
                    return DBNull.Value;
                }
                long num2 = this._currentPosition;
                int length = (int) this.Length;
                bool flag = (OracleType.Blob == this._lobType) || (OracleType.BFile == this._lobType);
                if (length == 0)
                {
                    if (flag)
                    {
                        return new byte[0];
                    }
                    return string.Empty;
                }
                try
                {
                    this.Seek(0L, SeekOrigin.Begin);
                    if (flag)
                    {
                        byte[] buffer = new byte[length];
                        this.Read(buffer, 0, length);
                        return buffer;
                    }
                    try
                    {
                        str = new StreamReader(this, Encoding.Unicode).ReadToEnd();
                    }
                    finally
                    {
                    }
                }
                finally
                {
                    this._currentPosition = num2;
                }
                return str;
            }
        }
    }
}

