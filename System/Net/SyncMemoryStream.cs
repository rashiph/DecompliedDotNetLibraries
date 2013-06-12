namespace System.Net
{
    using System;
    using System.IO;

    internal sealed class SyncMemoryStream : MemoryStream, IRequestLifetimeTracker
    {
        private bool m_Disposed;
        private int m_ReadTimeout;
        private RequestLifetimeSetter m_RequestLifetimeSetter;
        private int m_WriteTimeout;

        internal SyncMemoryStream(byte[] bytes) : base(bytes, false)
        {
            this.m_ReadTimeout = this.m_WriteTimeout = -1;
        }

        internal SyncMemoryStream(int initialCapacity) : base(initialCapacity)
        {
            this.m_ReadTimeout = this.m_WriteTimeout = -1;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return new LazyAsyncResult(null, state, callback, this.Read(buffer, offset, count));
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.Write(buffer, offset, count);
            return new LazyAsyncResult(null, state, callback, null);
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.m_Disposed)
            {
                this.m_Disposed = true;
                if (disposing)
                {
                    RequestLifetimeSetter.Report(this.m_RequestLifetimeSetter);
                }
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            LazyAsyncResult result = (LazyAsyncResult) asyncResult;
            return (int) result.InternalWaitForCompletion();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            ((LazyAsyncResult) asyncResult).InternalWaitForCompletion();
        }

        public void TrackRequestLifetime(long requestStartTimestamp)
        {
            this.m_RequestLifetimeSetter = new RequestLifetimeSetter(requestStartTimestamp);
        }

        public override bool CanTimeout
        {
            get
            {
                return true;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return this.m_ReadTimeout;
            }
            set
            {
                this.m_ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this.m_WriteTimeout;
            }
            set
            {
                this.m_WriteTimeout = value;
            }
        }
    }
}

