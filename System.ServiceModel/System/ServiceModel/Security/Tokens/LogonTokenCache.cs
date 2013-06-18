namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.ServiceModel.Security;

    internal class LogonTokenCache : TimeBoundedCache
    {
        private TimeSpan cachedLogonTokenLifetime;
        private const int lowWaterMarkFactor = 0x4b;
        private RNGCryptoServiceProvider random;
        private const int saltSize = 4;

        public LogonTokenCache(int maxCachedLogonTokens, TimeSpan cachedLogonTokenLifetime) : base((maxCachedLogonTokens * 0x4b) / 100, maxCachedLogonTokens, StringComparer.OrdinalIgnoreCase, PurgingMode.TimerBasedPurge, TimeSpan.FromTicks(cachedLogonTokenLifetime.Ticks >> 2), true)
        {
            this.cachedLogonTokenLifetime = cachedLogonTokenLifetime;
            this.random = new RNGCryptoServiceProvider();
        }

        public void Flush()
        {
            base.ClearItems();
        }

        protected override ArrayList OnQuotaReached(Hashtable cacheTable)
        {
            List<TimeBoundedCache.IExpirableItem> list = new List<TimeBoundedCache.IExpirableItem>(cacheTable.Count);
            foreach (TimeBoundedCache.IExpirableItem item in cacheTable.Values)
            {
                list.Add(item);
            }
            list.Sort(TimeBoundedCache.ExpirableItemComparer.Default);
            int capacity = (list.Count * 0x19) / 100;
            capacity = (capacity <= 0) ? list.Count : capacity;
            ArrayList list2 = new ArrayList(capacity);
            for (int i = 0; i < capacity; i++)
            {
                LogonToken token = (LogonToken) base.ExtractItem(list[i]);
                list2.Add(token.UserName);
                this.OnRemove(token);
            }
            return list2;
        }

        protected override void OnRemove(object item)
        {
            ((LogonToken) item).Dispose();
            base.OnRemove(item);
        }

        public bool TryAddTokenCache(string userName, string password, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            byte[] data = new byte[4];
            this.random.GetBytes(data);
            LogonToken item = new LogonToken(userName, password, data, authorizationPolicies);
            DateTime expirationTime = DateTime.UtcNow.Add(this.cachedLogonTokenLifetime);
            return base.TryAddItem(userName, item, expirationTime, true);
        }

        public bool TryGetTokenCache(string userName, out LogonToken token)
        {
            token = (LogonToken) base.GetItem(userName);
            return (token != null);
        }

        public bool TryRemoveTokenCache(string userName)
        {
            return base.TryRemoveItem(userName);
        }
    }
}

