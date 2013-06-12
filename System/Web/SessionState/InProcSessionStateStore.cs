namespace System.Web.SessionState
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;

    internal sealed class InProcSessionStateStore : SessionStateStoreProviderBase
    {
        private CacheItemRemovedCallback _callback;
        private SessionStateItemExpireCallback _expireCallback;
        internal static readonly int CACHEKEYPREFIXLENGTH = "j".Length;
        internal static readonly int NewLockCookie = 1;

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return SessionStateUtility.CreateLegitStoreData(context, null, null, timeout);
        }

        private string CreateSessionStateCacheKey(string id)
        {
            return ("j" + id);
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            string key = this.CreateSessionStateCacheKey(id);
            SessionIDManager.CheckIdLength(id, true);
            InProcSessionState state = new InProcSessionState(null, null, timeout, false, DateTime.MinValue, NewLockCookie, 1);
            try
            {
            }
            finally
            {
                if (HttpRuntime.CacheInternal.UtcAdd(key, state, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, timeout, 0), CacheItemPriority.NotRemovable, this._callback) == null)
                {
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_TOTAL);
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_ACTIVE);
                }
            }
        }

        public override void Dispose()
        {
        }

        private SessionStateStoreData DoGet(HttpContext context, string id, bool exclusive, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            bool flag;
            string key = this.CreateSessionStateCacheKey(id);
            locked = false;
            lockId = null;
            lockAge = TimeSpan.Zero;
            actionFlags = SessionStateActions.None;
            SessionIDManager.CheckIdLength(id, true);
            InProcSessionState state = (InProcSessionState) HttpRuntime.CacheInternal.Get(key);
            if (state == null)
            {
                return null;
            }
            int comparand = state._flags;
            if (((comparand & 1) != 0) && (comparand == Interlocked.CompareExchange(ref state._flags, comparand & -2, comparand)))
            {
                actionFlags = SessionStateActions.InitializeItem;
            }
            if (exclusive)
            {
                flag = true;
                if (!state._locked)
                {
                    state._spinLock.AcquireWriterLock();
                    try
                    {
                        if (!state._locked)
                        {
                            flag = false;
                            state._locked = true;
                            state._utcLockDate = DateTime.UtcNow;
                            state._lockCookie++;
                        }
                        lockId = state._lockCookie;
                        goto Label_00FE;
                    }
                    finally
                    {
                        state._spinLock.ReleaseWriterLock();
                    }
                }
                lockId = state._lockCookie;
            }
            else
            {
                state._spinLock.AcquireReaderLock();
                try
                {
                    flag = state._locked;
                    lockId = state._lockCookie;
                }
                finally
                {
                    state._spinLock.ReleaseReaderLock();
                }
            }
        Label_00FE:
            if (flag)
            {
                locked = true;
                lockAge = (TimeSpan) (DateTime.UtcNow - state._utcLockDate);
                return null;
            }
            return SessionStateUtility.CreateLegitStoreData(context, state._sessionItems, state._staticObjects, state._timeout);
        }

        public override void EndRequest(HttpContext context)
        {
        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            return this.DoGet(context, id, false, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            return this.DoGet(context, id, true, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "InProc Session State Provider";
            }
            base.Initialize(name, config);
            this._callback = new CacheItemRemovedCallback(this.OnCacheItemRemoved);
        }

        public override void InitializeRequest(HttpContext context)
        {
        }

        public void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            PerfCounters.DecrementCounter(AppPerfCounter.SESSIONS_ACTIVE);
            InProcSessionState state = (InProcSessionState) value;
            if (((state._flags & 2) == 0) && ((state._flags & 1) == 0))
            {
                switch (reason)
                {
                    case CacheItemRemovedReason.Removed:
                        PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_ABANDONED);
                        break;

                    case CacheItemRemovedReason.Expired:
                        PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_TIMED_OUT);
                        break;
                }
                if (this._expireCallback != null)
                {
                    string id = key.Substring(CACHEKEYPREFIXLENGTH);
                    this._expireCallback(id, SessionStateUtility.CreateLegitStoreData(null, state._sessionItems, state._staticObjects, state._timeout));
                }
            }
        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            string key = this.CreateSessionStateCacheKey(id);
            int num = (int) lockId;
            SessionIDManager.CheckIdLength(id, true);
            InProcSessionState state = (InProcSessionState) HttpRuntime.CacheInternal.Get(key);
            if ((state != null) && state._locked)
            {
                state._spinLock.AcquireWriterLock();
                try
                {
                    if (state._locked && (num == state._lockCookie))
                    {
                        state._locked = false;
                    }
                }
                finally
                {
                    state._spinLock.ReleaseWriterLock();
                }
            }
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            string key = this.CreateSessionStateCacheKey(id);
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            int num = (int) lockId;
            SessionIDManager.CheckIdLength(id, true);
            InProcSessionState state = (InProcSessionState) cacheInternal.Get(key);
            if (state != null)
            {
                state._spinLock.AcquireWriterLock();
                try
                {
                    if (!state._locked || (state._lockCookie != num))
                    {
                        return;
                    }
                    state._lockCookie = 0;
                }
                finally
                {
                    state._spinLock.ReleaseWriterLock();
                }
                cacheInternal.Remove(key);
            }
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            string key = this.CreateSessionStateCacheKey(id);
            SessionIDManager.CheckIdLength(id, true);
            HttpRuntime.CacheInternal.Get(key);
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            string key = this.CreateSessionStateCacheKey(id);
            bool flag = true;
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            int newLockCookie = NewLockCookie;
            ISessionStateItemCollection sessionItems = null;
            HttpStaticObjectsCollection staticObjects = null;
            SessionIDManager.CheckIdLength(id, true);
            if (item.Items.Count > 0)
            {
                sessionItems = item.Items;
            }
            if (!item.StaticObjects.NeverAccessed)
            {
                staticObjects = item.StaticObjects;
            }
            if (!newItem)
            {
                InProcSessionState state = (InProcSessionState) cacheInternal.Get(key);
                int lockCookie = (int) lockId;
                if (state == null)
                {
                    return;
                }
                state._spinLock.AcquireWriterLock();
                try
                {
                    if (!state._locked || (state._lockCookie != lockCookie))
                    {
                        return;
                    }
                    if (state._timeout == item.Timeout)
                    {
                        state.Copy(sessionItems, staticObjects, item.Timeout, false, DateTime.MinValue, lockCookie, state._flags);
                        flag = false;
                    }
                    else
                    {
                        state._flags |= 2;
                        newLockCookie = lockCookie;
                        state._lockCookie = 0;
                    }
                }
                finally
                {
                    state._spinLock.ReleaseWriterLock();
                }
            }
            if (flag)
            {
                InProcSessionState state2 = new InProcSessionState(sessionItems, staticObjects, item.Timeout, false, DateTime.MinValue, newLockCookie, 0);
                try
                {
                }
                finally
                {
                    cacheInternal.UtcInsert(key, state2, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, state2._timeout, 0), CacheItemPriority.NotRemovable, this._callback);
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_TOTAL);
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_ACTIVE);
                }
            }
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            this._expireCallback = expireCallback;
            return true;
        }

        [Conditional("DBG")]
        internal static void TraceSessionStats()
        {
        }
    }
}

