namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Runtime.CompilerServices;

    public sealed class OracleBFile : Stream, ICloneable, INullable, IDisposable
    {
        private string _directoryAlias;
        private string _fileName;
        private OracleLob _lob;
        private const short MaxDirectoryAliasChars = 30;
        private const short MaxFileAliasChars = 0xff;
        public static readonly OracleBFile Null = new OracleBFile();

        internal OracleBFile()
        {
            this._lob = OracleLob.Null;
        }

        internal OracleBFile(OciLobLocator lobLocator)
        {
            this._lob = new OracleLob(lobLocator);
        }

        internal OracleBFile(OracleBFile bfile)
        {
            this._lob = (OracleLob) bfile._lob.Clone();
            this._fileName = bfile._fileName;
            this._directoryAlias = bfile._directoryAlias;
        }

        internal void AssertInternalLobIsValid()
        {
            if (this.IsDisposed)
            {
                throw System.Data.Common.ADP.ObjectDisposed("OracleBFile");
            }
        }

        public object Clone()
        {
            return new OracleBFile(this);
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
            this.AssertInternalLobIsValid();
            if (destination == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("destination");
            }
            if (destination.IsNull)
            {
                throw System.Data.Common.ADP.LobWriteInvalidOnNull();
            }
            if (this._lob.IsNull)
            {
                return 0L;
            }
            this._lob.AssertConnectionIsOpen();
            this._lob.AssertAmountIsValid(amount, "amount");
            this._lob.AssertAmountIsValid(sourceOffset, "sourceOffset");
            this._lob.AssertAmountIsValid(destinationOffset, "destinationOffset");
            this._lob.AssertTransactionExists();
            long num = Math.Min(this.Length - sourceOffset, amount);
            long num4 = destinationOffset + 1L;
            long num3 = sourceOffset + 1L;
            if (0L >= num)
            {
                return 0L;
            }
            int rc = TracedNativeMethods.OCILobLoadFromFile(this.ServiceContextHandle, this.ErrorHandle, destination.Descriptor, this.Descriptor, (uint) num, (uint) num4, (uint) num3);
            if (rc != 0)
            {
                this.Connection.CheckError(this.ErrorHandle, rc);
            }
            return num;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OracleLob lob = this._lob;
                if (lob != null)
                {
                    lob.Close();
                }
            }
            this._lob = null;
            this._fileName = null;
            this._directoryAlias = null;
            base.Dispose(disposing);
        }

        private void EnsureLobIsOpened()
        {
            this.LobLocator.Open(OracleLobOpenMode.ReadOnly);
        }

        public override void Flush()
        {
        }

        internal void GetNames()
        {
            this._lob.AssertConnectionIsOpen();
            short num5 = this.Connection.EnvironmentHandle.IsUnicode ? ((short) 2) : ((short) 1);
            ushort num = (ushort) (30 * num5);
            int offset = num;
            ushort num3 = (ushort) (0xff * num5);
            NativeBuffer scratchBuffer = this.Connection.GetScratchBuffer(num + num3);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                scratchBuffer.DangerousAddRef(ref success);
                int rc = TracedNativeMethods.OCILobFileGetName(this.Connection.EnvironmentHandle, this.ErrorHandle, this.Descriptor, scratchBuffer.DangerousGetDataPtr(), ref num, scratchBuffer.DangerousGetDataPtr(offset), ref num3);
                if (rc != 0)
                {
                    this.Connection.CheckError(this.ErrorHandle, rc);
                }
                this._directoryAlias = this.Connection.GetString(scratchBuffer.ReadBytes(0, num));
                this._fileName = this.Connection.GetString(scratchBuffer.ReadBytes(offset, num3));
            }
            finally
            {
                if (success)
                {
                    scratchBuffer.DangerousRelease();
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.AssertInternalLobIsValid();
            if (!this.IsNull)
            {
                this.EnsureLobIsOpened();
            }
            return this._lob.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.AssertInternalLobIsValid();
            return this._lob.Seek(offset, origin);
        }

        public void SetFileName(string directory, string file)
        {
            this.AssertInternalLobIsValid();
            if (!this.IsNull)
            {
                this._lob.AssertConnectionIsOpen();
                this._lob.AssertTransactionExists();
                OciFileDescriptor filep = (OciFileDescriptor) this.LobLocator.Descriptor;
                if (filep != null)
                {
                    this.LobLocator.ForceClose();
                    int rc = TracedNativeMethods.OCILobFileSetName(this.Connection.EnvironmentHandle, this.ErrorHandle, filep, directory, file);
                    if (rc != 0)
                    {
                        this.Connection.CheckError(this.ErrorHandle, rc);
                    }
                    this.LobLocator.ForceOpen();
                    this._fileName = null;
                    this._directoryAlias = null;
                    try
                    {
                        this._lob.Position = 0L;
                    }
                    catch (Exception exception)
                    {
                        if (!System.Data.Common.ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                    }
                }
            }
        }

        public override void SetLength(long value)
        {
            this.AssertInternalLobIsValid();
            throw System.Data.Common.ADP.ReadOnlyLob();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.AssertInternalLobIsValid();
            throw System.Data.Common.ADP.ReadOnlyLob();
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
                return false;
            }
        }

        public OracleConnection Connection
        {
            get
            {
                this.AssertInternalLobIsValid();
                return this._lob.Connection;
            }
        }

        internal OciHandle Descriptor
        {
            get
            {
                return this.LobLocator.Descriptor;
            }
        }

        public string DirectoryName
        {
            get
            {
                this.AssertInternalLobIsValid();
                if (this.IsNull)
                {
                    return string.Empty;
                }
                if (this._directoryAlias == null)
                {
                    this.GetNames();
                }
                return this._directoryAlias;
            }
        }

        internal OciErrorHandle ErrorHandle
        {
            get
            {
                return this._lob.ErrorHandle;
            }
        }

        public bool FileExists
        {
            get
            {
                int num2;
                this.AssertInternalLobIsValid();
                if (this.IsNull)
                {
                    return false;
                }
                this._lob.AssertConnectionIsOpen();
                int rc = TracedNativeMethods.OCILobFileExists(this.ServiceContextHandle, this.ErrorHandle, this.Descriptor, out num2);
                if (rc != 0)
                {
                    this.Connection.CheckError(this.ErrorHandle, rc);
                }
                return (num2 != 0);
            }
        }

        public string FileName
        {
            get
            {
                this.AssertInternalLobIsValid();
                if (this.IsNull)
                {
                    return string.Empty;
                }
                if (this._fileName == null)
                {
                    this.GetNames();
                }
                return this._fileName;
            }
        }

        private bool IsDisposed
        {
            get
            {
                return (null == this._lob);
            }
        }

        public bool IsNull
        {
            get
            {
                return (OracleLob.Null == this._lob);
            }
        }

        public override long Length
        {
            get
            {
                this.AssertInternalLobIsValid();
                if (this.IsNull)
                {
                    return 0L;
                }
                return this._lob.Length;
            }
        }

        internal OciLobLocator LobLocator
        {
            get
            {
                return this._lob.LobLocator;
            }
        }

        public override long Position
        {
            get
            {
                this.AssertInternalLobIsValid();
                if (this.IsNull)
                {
                    return 0L;
                }
                return this._lob.Position;
            }
            set
            {
                this.AssertInternalLobIsValid();
                if (!this.IsNull)
                {
                    this._lob.Position = value;
                }
            }
        }

        internal OciServiceContextHandle ServiceContextHandle
        {
            get
            {
                return this._lob.ServiceContextHandle;
            }
        }

        public object Value
        {
            get
            {
                this.AssertInternalLobIsValid();
                if (this.IsNull)
                {
                    return DBNull.Value;
                }
                this.EnsureLobIsOpened();
                return this._lob.Value;
            }
        }
    }
}

