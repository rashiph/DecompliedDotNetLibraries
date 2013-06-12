namespace System.Net
{
    using System;
    using System.IO;

    internal sealed class FileWebStream : FileStream, ICloseEx
    {
        private FileWebRequest m_request;

        public FileWebStream(FileWebRequest request, string path, FileMode mode, FileAccess access, FileShare sharing) : base(path, mode, access, sharing)
        {
            this.m_request = request;
        }

        public FileWebStream(FileWebRequest request, string path, FileMode mode, FileAccess access, FileShare sharing, int length, bool async) : base(path, mode, access, sharing, length, async)
        {
            this.m_request = request;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            this.CheckError();
            try
            {
                result = base.BeginRead(buffer, offset, size, callback, state);
            }
            catch
            {
                this.CheckError();
                throw;
            }
            return result;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            this.CheckError();
            try
            {
                result = base.BeginWrite(buffer, offset, size, callback, state);
            }
            catch
            {
                this.CheckError();
                throw;
            }
            return result;
        }

        private void CheckError()
        {
            if (this.m_request.Aborted)
            {
                throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this.m_request != null))
                {
                    this.m_request.UnblockReader();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult ar)
        {
            int num;
            try
            {
                num = base.EndRead(ar);
            }
            catch
            {
                this.CheckError();
                throw;
            }
            return num;
        }

        public override void EndWrite(IAsyncResult ar)
        {
            try
            {
                base.EndWrite(ar);
            }
            catch
            {
                this.CheckError();
                throw;
            }
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            int num;
            this.CheckError();
            try
            {
                num = base.Read(buffer, offset, size);
            }
            catch
            {
                this.CheckError();
                throw;
            }
            return num;
        }

        void ICloseEx.CloseEx(CloseExState closeState)
        {
            if ((closeState & CloseExState.Abort) != CloseExState.Normal)
            {
                this.SafeFileHandle.Close();
            }
            else
            {
                this.Close();
            }
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            this.CheckError();
            try
            {
                base.Write(buffer, offset, size);
            }
            catch
            {
                this.CheckError();
                throw;
            }
        }
    }
}

