namespace System.Web.Configuration
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System.Security.Principal;

    [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
    internal class RemoteWebConfigurationHostStream : Stream
    {
        private string _Domain;
        private string _FileName;
        private WindowsIdentity _Identity;
        private bool _IsDirty;
        private MemoryStream _MemoryStream;
        private string _Password;
        private long _ReadTime;
        private string _Server;
        private bool _streamForWrite;
        private string _TemplateFileName;
        private string _Username;

        internal RemoteWebConfigurationHostStream(bool streamForWrite, string serverName, string streamName, string templateStreamName, string username, string domain, string password, WindowsIdentity identity)
        {
            this._Server = serverName;
            this._FileName = streamName;
            this._TemplateFileName = templateStreamName;
            this._Username = username;
            this._Domain = domain;
            this._Password = password;
            this._Identity = identity;
            this._streamForWrite = streamForWrite;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.Init();
            return this._MemoryStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this._IsDirty = true;
            this.Init();
            if ((offset + count) > this._MemoryStream.Length)
            {
                this._MemoryStream.SetLength((long) (offset + count));
            }
            return this._MemoryStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            throw new RemotingException();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this._MemoryStream != null))
                {
                    this.Flush();
                    this._MemoryStream.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            this.Init();
            return this._MemoryStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.Init();
            this._MemoryStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
        }

        internal void FlushForWriteCompleted()
        {
            if (this._IsDirty && (this._MemoryStream != null))
            {
                WindowsImpersonationContext context = null;
                try
                {
                    if (this._Identity != null)
                    {
                        context = this._Identity.Impersonate();
                    }
                    try
                    {
                        IRemoteWebConfigurationHostServer o = RemoteWebConfigurationHost.CreateRemoteObject(this._Server, this._Username, this._Domain, this._Password);
                        try
                        {
                            o.WriteData(this._FileName, this._TemplateFileName, this._MemoryStream.ToArray(), ref this._ReadTime);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            while (Marshal.ReleaseComObject(o) > 0)
                            {
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (context != null)
                        {
                            context.Undo();
                        }
                    }
                }
                catch
                {
                    throw;
                }
                this._MemoryStream.Flush();
                this._IsDirty = false;
            }
        }

        private void Init()
        {
            if (this._MemoryStream == null)
            {
                byte[] buffer = null;
                WindowsImpersonationContext context = null;
                try
                {
                    if (this._Identity != null)
                    {
                        context = this._Identity.Impersonate();
                    }
                    try
                    {
                        IRemoteWebConfigurationHostServer o = RemoteWebConfigurationHost.CreateRemoteObject(this._Server, this._Username, this._Domain, this._Password);
                        try
                        {
                            buffer = o.GetData(this._FileName, this._streamForWrite, out this._ReadTime);
                        }
                        finally
                        {
                            while (Marshal.ReleaseComObject(o) > 0)
                            {
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (context != null)
                        {
                            context.Undo();
                        }
                    }
                }
                catch
                {
                    throw;
                }
                if ((buffer == null) || (buffer.Length < 1))
                {
                    this._MemoryStream = new MemoryStream();
                }
                else
                {
                    this._MemoryStream = new MemoryStream(buffer.Length);
                    this._MemoryStream.Write(buffer, 0, buffer.Length);
                    this._MemoryStream.Position = 0L;
                }
            }
        }

        public override object InitializeLifetimeService()
        {
            this.Init();
            return this._MemoryStream.InitializeLifetimeService();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.Init();
            return this._MemoryStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            this.Init();
            return this._MemoryStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.Init();
            return this._MemoryStream.Seek(offset, origin);
        }

        public override void SetLength(long val)
        {
            this._IsDirty = true;
            this.Init();
            this._MemoryStream.SetLength(val);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._IsDirty = true;
            this.Init();
            if ((offset + count) > this._MemoryStream.Length)
            {
                this._MemoryStream.SetLength((long) (offset + count));
            }
            this._MemoryStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte val)
        {
            this._IsDirty = true;
            this.Init();
            this._MemoryStream.WriteByte(val);
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                this.Init();
                return this._MemoryStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                this.Init();
                return this._MemoryStream.Position;
            }
            set
            {
                this.Init();
                this._MemoryStream.Position = value;
            }
        }
    }
}

