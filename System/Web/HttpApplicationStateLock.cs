namespace System.Web
{
    using System;
    using System.Web.Util;

    internal class HttpApplicationStateLock : ReadWriteObjectLock
    {
        private int _recursionCount;
        private int _threadId;

        internal HttpApplicationStateLock()
        {
        }

        internal override void AcquireRead()
        {
            int currentThreadId = SafeNativeMethods.GetCurrentThreadId();
            if (this._threadId != currentThreadId)
            {
                base.AcquireRead();
            }
        }

        internal override void AcquireWrite()
        {
            int currentThreadId = SafeNativeMethods.GetCurrentThreadId();
            if (this._threadId == currentThreadId)
            {
                this._recursionCount++;
            }
            else
            {
                base.AcquireWrite();
                this._threadId = currentThreadId;
                this._recursionCount = 1;
            }
        }

        internal void EnsureReleaseWrite()
        {
            int currentThreadId = SafeNativeMethods.GetCurrentThreadId();
            if (this._threadId == currentThreadId)
            {
                this._threadId = 0;
                this._recursionCount = 0;
                base.ReleaseWrite();
            }
        }

        internal override void ReleaseRead()
        {
            int currentThreadId = SafeNativeMethods.GetCurrentThreadId();
            if (this._threadId != currentThreadId)
            {
                base.ReleaseRead();
            }
        }

        internal override void ReleaseWrite()
        {
            int currentThreadId = SafeNativeMethods.GetCurrentThreadId();
            if ((this._threadId == currentThreadId) && (--this._recursionCount == 0))
            {
                this._threadId = 0;
                base.ReleaseWrite();
            }
        }
    }
}

