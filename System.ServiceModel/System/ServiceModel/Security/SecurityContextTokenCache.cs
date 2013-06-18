namespace System.ServiceModel.Security
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal sealed class SecurityContextTokenCache : TimeBoundedCache
    {
        private TimeSpan clockSkew;
        private static int lowWaterMark = 50;
        private static double pruningFactor = 0.2;
        private static TimeSpan purgingInterval = TimeSpan.FromMinutes(10.0);
        private bool replaceOldestEntries;
        private static SctEffectiveTimeComparer sctEffectiveTimeComparer = new SctEffectiveTimeComparer();

        public SecurityContextTokenCache(int capacity, bool replaceOldestEntries) : this(capacity, replaceOldestEntries, SecurityProtocolFactory.defaultMaxClockSkew)
        {
        }

        public SecurityContextTokenCache(int capacity, bool replaceOldestEntries, TimeSpan clockSkew) : base(lowWaterMark, capacity, null, PurgingMode.TimerBasedPurge, purgingInterval, true)
        {
            this.replaceOldestEntries = true;
            this.replaceOldestEntries = replaceOldestEntries;
            this.clockSkew = clockSkew;
        }

        public void AddContext(SecurityContextSecurityToken token)
        {
            this.TryAddContext(token, true);
        }

        public void ClearContexts()
        {
            base.ClearItems();
        }

        public Collection<SecurityContextSecurityToken> GetAllContexts(UniqueId contextId)
        {
            ArrayList matchingKeys = this.GetMatchingKeys(contextId);
            Collection<SecurityContextSecurityToken> collection = new Collection<SecurityContextSecurityToken>();
            for (int i = 0; i < matchingKeys.Count; i++)
            {
                SecurityContextSecurityToken item = base.GetItem(matchingKeys[i]) as SecurityContextSecurityToken;
                if (item != null)
                {
                    collection.Add(item);
                }
            }
            return collection;
        }

        public SecurityContextSecurityToken GetContext(UniqueId contextId, UniqueId generation)
        {
            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }
            object hashKey = this.GetHashKey(contextId, generation);
            SecurityContextSecurityToken item = (SecurityContextSecurityToken) base.GetItem(hashKey);
            if (item == null)
            {
                return null;
            }
            return item.Clone();
        }

        private object GetHashKey(UniqueId contextId, UniqueId generation)
        {
            if (generation == null)
            {
                return contextId;
            }
            return new ContextAndGenerationKey(contextId, generation);
        }

        private ArrayList GetMatchingKeys(UniqueId contextId)
        {
            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }
            ArrayList list = new ArrayList(2);
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    base.CacheLock.AcquireReaderLock(-1);
                    flag = true;
                }
                foreach (object obj2 in base.Entries.Keys)
                {
                    bool flag2 = false;
                    if (obj2 is UniqueId)
                    {
                        flag2 = ((UniqueId) obj2) == contextId;
                    }
                    else
                    {
                        ContextAndGenerationKey key = (ContextAndGenerationKey) obj2;
                        flag2 = key.ContextId == contextId;
                    }
                    if (flag2)
                    {
                        list.Add(obj2);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    base.CacheLock.ReleaseReaderLock();
                }
            }
            return list;
        }

        protected override ArrayList OnQuotaReached(Hashtable cacheTable)
        {
            if (!this.replaceOldestEntries)
            {
                SecurityTraceRecordHelper.TraceSecurityContextTokenCacheFull(base.Capacity, 0);
                return base.OnQuotaReached(cacheTable);
            }
            List<SecurityContextSecurityToken> list = new List<SecurityContextSecurityToken>(cacheTable.Count);
            foreach (TimeBoundedCache.IExpirableItem item in cacheTable.Values)
            {
                SecurityContextSecurityToken token = (SecurityContextSecurityToken) base.ExtractItem(item);
                list.Add(token);
            }
            list.Sort(sctEffectiveTimeComparer);
            int capacity = (int) (base.Capacity * pruningFactor);
            capacity = (capacity <= 0) ? base.Capacity : capacity;
            ArrayList list2 = new ArrayList(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list2.Add(this.GetHashKey(list[i].ContextId, list[i].KeyGeneration));
                this.OnRemove(list[i]);
            }
            SecurityTraceRecordHelper.TraceSecurityContextTokenCacheFull(base.Capacity, capacity);
            return list2;
        }

        protected override void OnRemove(object item)
        {
            ((IDisposable) item).Dispose();
            base.OnRemove(item);
        }

        public void RemoveAllContexts(UniqueId contextId)
        {
            ArrayList matchingKeys = this.GetMatchingKeys(contextId);
            for (int i = 0; i < matchingKeys.Count; i++)
            {
                base.TryRemoveItem(matchingKeys[i]);
            }
        }

        public void RemoveContext(UniqueId contextId, UniqueId generation, bool throwIfNotPresent)
        {
            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }
            object hashKey = this.GetHashKey(contextId, generation);
            if (!base.TryRemoveItem(hashKey) && throwIfNotPresent)
            {
                if (generation == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ContextNotPresentNoKeyGeneration", new object[] { contextId })));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ContextNotPresent", new object[] { contextId, generation.ToString() })));
            }
        }

        public bool TryAddContext(SecurityContextSecurityToken token)
        {
            return this.TryAddContext(token, false);
        }

        private bool TryAddContext(SecurityContextSecurityToken token, bool throwOnFailure)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (!System.ServiceModel.Security.SecurityUtils.IsCurrentlyTimeEffective(token.ValidFrom, token.ValidTo, this.clockSkew))
            {
                if (token.KeyGeneration == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SecurityContextExpiredNoKeyGeneration", new object[] { token.ContextId }));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SecurityContextExpired", new object[] { token.ContextId, token.KeyGeneration.ToString() }));
            }
            if (!System.ServiceModel.Security.SecurityUtils.IsCurrentlyTimeEffective(token.KeyEffectiveTime, token.KeyExpirationTime, this.clockSkew))
            {
                if (token.KeyGeneration == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SecurityContextKeyExpiredNoKeyGeneration", new object[] { token.ContextId }));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SecurityContextKeyExpired", new object[] { token.ContextId, token.KeyGeneration.ToString() }));
            }
            object hashKey = this.GetHashKey(token.ContextId, token.KeyGeneration);
            bool flag = base.TryAddItem(hashKey, token.Clone(), false);
            if (flag || !throwOnFailure)
            {
                return flag;
            }
            if (token.KeyGeneration == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ContextAlreadyRegisteredNoKeyGeneration", new object[] { token.ContextId })));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ContextAlreadyRegistered", new object[] { token.ContextId, token.KeyGeneration.ToString() })));
        }

        public void UpdateContextCachingTime(SecurityContextSecurityToken token, DateTime expirationTime)
        {
            if (token.ValidTo > expirationTime.ToUniversalTime())
            {
                base.TryReplaceItem(this.GetHashKey(token.ContextId, token.KeyGeneration), token, expirationTime);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ContextAndGenerationKey
        {
            private UniqueId contextId;
            private UniqueId generation;
            public ContextAndGenerationKey(UniqueId contextId, UniqueId generation)
            {
                this.contextId = contextId;
                this.generation = generation;
            }

            public UniqueId ContextId
            {
                get
                {
                    return this.contextId;
                }
            }
            public UniqueId Generation
            {
                get
                {
                    return this.generation;
                }
            }
            public override int GetHashCode()
            {
                return (this.contextId.GetHashCode() ^ this.generation.GetHashCode());
            }

            public override bool Equals(object obj)
            {
                if (!(obj is SecurityContextTokenCache.ContextAndGenerationKey))
                {
                    return false;
                }
                SecurityContextTokenCache.ContextAndGenerationKey key = (SecurityContextTokenCache.ContextAndGenerationKey) obj;
                return ((key.ContextId == this.contextId) && (key.Generation == this.generation));
            }

            public static bool operator ==(SecurityContextTokenCache.ContextAndGenerationKey a, SecurityContextTokenCache.ContextAndGenerationKey b)
            {
                if (object.ReferenceEquals(a, null))
                {
                    return object.ReferenceEquals(b, null);
                }
                return a.Equals(b);
            }

            public static bool operator !=(SecurityContextTokenCache.ContextAndGenerationKey a, SecurityContextTokenCache.ContextAndGenerationKey b)
            {
                return !(a == b);
            }
        }

        private sealed class SctEffectiveTimeComparer : IComparer<SecurityContextSecurityToken>
        {
            public int Compare(SecurityContextSecurityToken sct1, SecurityContextSecurityToken sct2)
            {
                if (sct1 != sct2)
                {
                    if (sct1.ValidFrom.ToUniversalTime() < sct2.ValidFrom.ToUniversalTime())
                    {
                        return -1;
                    }
                    if (sct1.ValidFrom.ToUniversalTime() > sct2.ValidFrom.ToUniversalTime())
                    {
                        return 1;
                    }
                    if (sct1.KeyEffectiveTime.ToUniversalTime() < sct2.KeyEffectiveTime.ToUniversalTime())
                    {
                        return -1;
                    }
                    if (sct1.KeyEffectiveTime.ToUniversalTime() > sct2.KeyEffectiveTime.ToUniversalTime())
                    {
                        return 1;
                    }
                }
                return 0;
            }
        }
    }
}

