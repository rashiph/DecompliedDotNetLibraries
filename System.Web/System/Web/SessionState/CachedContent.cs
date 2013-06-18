namespace System.Web.SessionState
{
    using System;
    using System.Web.Util;

    internal sealed class CachedContent
    {
        internal byte[] _content;
        internal int _extraFlags;
        internal int _lockCookie;
        internal bool _locked;
        internal ReadWriteSpinLock _spinLock;
        internal IntPtr _stateItem;
        internal DateTime _utcLockDate;

        internal CachedContent(byte[] content, IntPtr stateItem, bool locked, DateTime utcLockDate, int lockCookie, int extraFlags)
        {
            this._content = content;
            this._stateItem = stateItem;
            this._locked = locked;
            this._utcLockDate = utcLockDate;
            this._lockCookie = lockCookie;
            this._extraFlags = extraFlags;
        }
    }
}

