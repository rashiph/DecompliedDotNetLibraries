namespace System.ServiceModel.Security
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.ServiceModel;

    internal sealed class NegotiationTokenAuthenticatorStateCache<T> : TimeBoundedCache where T: NegotiationTokenAuthenticatorState
    {
        private TimeSpan cachingSpan;
        private static int lowWaterMark;
        private static TimeSpan purgingInterval;

        static NegotiationTokenAuthenticatorStateCache()
        {
            NegotiationTokenAuthenticatorStateCache<T>.lowWaterMark = 50;
            NegotiationTokenAuthenticatorStateCache<T>.purgingInterval = TimeSpan.FromMinutes(10.0);
        }

        public NegotiationTokenAuthenticatorStateCache(TimeSpan cachingSpan, int maximumCachedState) : base(NegotiationTokenAuthenticatorStateCache<T>.lowWaterMark, maximumCachedState, null, PurgingMode.TimerBasedPurge, TimeSpan.FromTicks(cachingSpan.Ticks >> 2), true)
        {
            this.cachingSpan = cachingSpan;
        }

        public void AddState(string context, T state)
        {
            DateTime expirationTime = TimeoutHelper.Add(DateTime.UtcNow, this.cachingSpan);
            if (!base.TryAddItem(context, state, expirationTime, false))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NegotiationStateAlreadyPresent", new object[] { context })));
            }
        }

        public T GetState(string context)
        {
            return (base.GetItem(context) as T);
        }

        protected override ArrayList OnQuotaReached(Hashtable cacheTable)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QuotaExceededException(System.ServiceModel.SR.GetString("CachedNegotiationStateQuotaReached", new object[] { base.Capacity })));
        }

        protected override void OnRemove(object item)
        {
            ((IDisposable) item).Dispose();
            base.OnRemove(item);
        }

        public void RemoveState(string context)
        {
            base.TryRemoveItem(context);
        }
    }
}

