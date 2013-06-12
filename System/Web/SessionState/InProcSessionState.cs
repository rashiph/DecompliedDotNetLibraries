namespace System.Web.SessionState
{
    using System;
    using System.Web;
    using System.Web.Util;

    internal sealed class InProcSessionState
    {
        internal int _flags;
        internal int _lockCookie;
        internal bool _locked;
        internal ISessionStateItemCollection _sessionItems;
        internal ReadWriteSpinLock _spinLock;
        internal HttpStaticObjectsCollection _staticObjects;
        internal int _timeout;
        internal DateTime _utcLockDate;

        internal InProcSessionState(ISessionStateItemCollection sessionItems, HttpStaticObjectsCollection staticObjects, int timeout, bool locked, DateTime utcLockDate, int lockCookie, int flags)
        {
            this.Copy(sessionItems, staticObjects, timeout, locked, utcLockDate, lockCookie, flags);
        }

        internal void Copy(ISessionStateItemCollection sessionItems, HttpStaticObjectsCollection staticObjects, int timeout, bool locked, DateTime utcLockDate, int lockCookie, int flags)
        {
            this._sessionItems = sessionItems;
            this._staticObjects = staticObjects;
            this._timeout = timeout;
            this._locked = locked;
            this._utcLockDate = utcLockDate;
            this._lockCookie = lockCookie;
            this._flags = flags;
        }
    }
}

