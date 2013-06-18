namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Runtime.ConstrainedExecution;
    using System.Threading;

    internal sealed class OciLobLocator
    {
        private int _cloneCount;
        private OracleConnection _connection;
        private int _connectionCloseCount;
        private OciHandle _descriptor;
        private OracleType _lobType;
        private int _openMode;

        internal OciLobLocator(OracleConnection connection, OracleType lobType)
        {
            this._connection = connection;
            this._connectionCloseCount = connection.CloseCount;
            this._lobType = lobType;
            this._cloneCount = 1;
            switch (lobType)
            {
                case OracleType.BFile:
                    this._descriptor = new OciFileDescriptor(connection.ServiceContextHandle);
                    break;

                case OracleType.Blob:
                case OracleType.Clob:
                case OracleType.NClob:
                    this._descriptor = new OciLobDescriptor(connection.ServiceContextHandle);
                    return;

                case OracleType.Char:
                    break;

                default:
                    return;
            }
        }

        internal OciLobLocator Clone()
        {
            Interlocked.Increment(ref this._cloneCount);
            return this;
        }

        internal void Dispose()
        {
            if (Interlocked.Decrement(ref this._cloneCount) == 0)
            {
                if ((this._openMode != 0) && !this.ConnectionIsClosed)
                {
                    this.ForceClose();
                }
                OciHandle.SafeDispose(ref this._descriptor);
                GC.KeepAlive(this);
                this._connection = null;
            }
        }

        internal void ForceClose()
        {
            if (this._openMode != 0)
            {
                int rc = TracedNativeMethods.OCILobClose(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor);
                if (rc != 0)
                {
                    this.Connection.CheckError(this.ErrorHandle, rc);
                }
                this._openMode = 0;
            }
        }

        internal void ForceOpen()
        {
            if (this._openMode != 0)
            {
                int rc = TracedNativeMethods.OCILobOpen(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor, (byte) this._openMode);
                if (rc != 0)
                {
                    this._openMode = 0;
                    this.Connection.CheckError(this.ErrorHandle, rc);
                }
            }
        }

        internal void Open(OracleLobOpenMode mode)
        {
            OracleLobOpenMode current = (OracleLobOpenMode) Interlocked.CompareExchange(ref this._openMode, (int) mode, 0);
            if (current == ((OracleLobOpenMode) 0))
            {
                this.ForceOpen();
            }
            else if (mode != current)
            {
                throw System.Data.Common.ADP.CannotOpenLobWithDifferentMode(mode, current);
            }
        }

        internal static void SafeDispose(ref OciLobLocator locator)
        {
            if (locator != null)
            {
                locator.Dispose();
            }
            locator = null;
        }

        internal OracleConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        internal bool ConnectionIsClosed
        {
            get
            {
                if (this._connection != null)
                {
                    return (this._connectionCloseCount != this._connection.CloseCount);
                }
                return true;
            }
        }

        internal OciHandle Descriptor
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._descriptor;
            }
        }

        internal OciErrorHandle ErrorHandle
        {
            get
            {
                return this.Connection.ErrorHandle;
            }
        }

        public OracleType LobType
        {
            get
            {
                return this._lobType;
            }
        }

        internal OciServiceContextHandle ServiceContextHandle
        {
            get
            {
                return this.Connection.ServiceContextHandle;
            }
        }
    }
}

