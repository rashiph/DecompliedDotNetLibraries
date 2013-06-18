namespace System.Web.SessionState
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Runtime.InteropServices;
    using System.Web;

    public abstract class SessionStateStoreProviderBase : ProviderBase
    {
        protected SessionStateStoreProviderBase()
        {
        }

        public abstract SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout);
        public abstract void CreateUninitializedItem(HttpContext context, string id, int timeout);
        public abstract void Dispose();
        public abstract void EndRequest(HttpContext context);
        public abstract SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions);
        public abstract SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions);
        internal virtual void Initialize(string name, NameValueCollection config, IPartitionResolver partitionResolver)
        {
        }

        public abstract void InitializeRequest(HttpContext context);
        public abstract void ReleaseItemExclusive(HttpContext context, string id, object lockId);
        public abstract void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item);
        public abstract void ResetItemTimeout(HttpContext context, string id);
        public abstract void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem);
        public abstract bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback);
    }
}

