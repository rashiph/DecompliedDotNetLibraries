namespace System.Net
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Security.Permissions;

    internal class FtpDataStream : Stream, ICloseEx
    {
        private bool m_Closing;
        private bool m_IsFullyRead;
        private NetworkStream m_NetworkStream;
        private bool m_Readable = true;
        private FtpWebRequest m_Request;
        private bool m_Writeable = true;

        internal FtpDataStream(NetworkStream networkStream, FtpWebRequest request, TriState writeOnly)
        {
            if (writeOnly == TriState.True)
            {
                this.m_Readable = false;
            }
            else if (writeOnly == TriState.False)
            {
                this.m_Writeable = false;
            }
            this.m_NetworkStream = networkStream;
            this.m_Request = request;
        }

        private void AsyncReadCallback(IAsyncResult ar)
        {
            LazyAsyncResult asyncState = (LazyAsyncResult) ar.AsyncState;
            try
            {
                try
                {
                    int result = this.m_NetworkStream.EndRead(ar);
                    if (result == 0)
                    {
                        this.m_IsFullyRead = true;
                        this.Close();
                    }
                    asyncState.InvokeCallback(result);
                }
                catch (Exception exception)
                {
                    if (!asyncState.IsCompleted)
                    {
                        asyncState.InvokeCallback(exception);
                    }
                }
            }
            catch
            {
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            this.CheckError();
            LazyAsyncResult result = new LazyAsyncResult(this, state, callback);
            try
            {
                this.m_NetworkStream.BeginRead(buffer, offset, size, new AsyncCallback(this.AsyncReadCallback), result);
            }
            catch
            {
                this.CheckError();
                throw;
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            this.CheckError();
            try
            {
                result = this.m_NetworkStream.BeginWrite(buffer, offset, size, callback, state);
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
            if (this.m_Request.Aborted)
            {
                throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    ((ICloseEx) this).CloseEx(CloseExState.Normal);
                }
                else
                {
                    ((ICloseEx) this).CloseEx(CloseExState.Silent | CloseExState.Abort);
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
                object obj2 = ((LazyAsyncResult) ar).InternalWaitForCompletion();
                if (obj2 is Exception)
                {
                    throw ((Exception) obj2);
                }
                num = (int) obj2;
            }
            finally
            {
                this.CheckError();
            }
            return num;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            try
            {
                this.m_NetworkStream.EndWrite(asyncResult);
            }
            finally
            {
                this.CheckError();
            }
        }

        public override void Flush()
        {
            this.m_NetworkStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            int num;
            this.CheckError();
            try
            {
                num = this.m_NetworkStream.Read(buffer, offset, size);
            }
            catch
            {
                this.CheckError();
                throw;
            }
            if (num == 0)
            {
                this.m_IsFullyRead = true;
                this.Close();
            }
            return num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long num;
            this.CheckError();
            try
            {
                num = this.m_NetworkStream.Seek(offset, origin);
            }
            catch
            {
                this.CheckError();
                throw;
            }
            return num;
        }

        public override void SetLength(long value)
        {
            this.m_NetworkStream.SetLength(value);
        }

        internal void SetSocketTimeoutOption(SocketShutdown mode, int timeout, bool silent)
        {
            this.m_NetworkStream.SetSocketTimeoutOption(mode, timeout, silent);
        }

        void ICloseEx.CloseEx(CloseExState closeState)
        {
            lock (this)
            {
                if (this.m_Closing)
                {
                    return;
                }
                this.m_Closing = true;
                this.m_Writeable = false;
                this.m_Readable = false;
            }
            try
            {
                try
                {
                    if ((closeState & CloseExState.Abort) == CloseExState.Normal)
                    {
                        this.m_NetworkStream.Close(-1);
                    }
                    else
                    {
                        this.m_NetworkStream.Close(0);
                    }
                }
                finally
                {
                    this.m_Request.DataStreamClosed(closeState);
                }
            }
            catch (Exception exception)
            {
                bool flag = true;
                WebException exception2 = exception as WebException;
                if (exception2 != null)
                {
                    FtpWebResponse response = exception2.Response as FtpWebResponse;
                    if (((response != null) && !this.m_IsFullyRead) && (response.StatusCode == FtpStatusCode.ConnectionClosed))
                    {
                        flag = false;
                    }
                }
                if (flag && ((closeState & CloseExState.Silent) == CloseExState.Normal))
                {
                    throw;
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            this.CheckError();
            try
            {
                this.m_NetworkStream.Write(buffer, offset, size);
            }
            catch
            {
                this.CheckError();
                throw;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.m_Readable;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.m_NetworkStream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return this.m_NetworkStream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.m_Writeable;
            }
        }

        public override long Length
        {
            get
            {
                return this.m_NetworkStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this.m_NetworkStream.Position;
            }
            set
            {
                this.m_NetworkStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return this.m_NetworkStream.ReadTimeout;
            }
            set
            {
                this.m_NetworkStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this.m_NetworkStream.WriteTimeout;
            }
            set
            {
                this.m_NetworkStream.WriteTimeout = value;
            }
        }
    }
}

