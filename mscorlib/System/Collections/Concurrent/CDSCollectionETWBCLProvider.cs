namespace System.Collections.Concurrent
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Runtime.CompilerServices;

    [FriendAccessAllowed]
    internal sealed class CDSCollectionETWBCLProvider : EventProviderBase
    {
        public static CDSCollectionETWBCLProvider Log = new CDSCollectionETWBCLProvider();

        private CDSCollectionETWBCLProvider() : base(new Guid(0x35167f8e, 0x49b2, 0x4b96, 0xab, 0x86, 0x43, 0x5b, 0x59, 0x33, 0x6b, 0x5e))
        {
        }

        [Event(5, Level=EventLevel.Verbose)]
        public void ConcurrentBag_TryPeekSteals()
        {
            if (base.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                base.WriteEvent(5);
            }
        }

        [Event(4, Level=EventLevel.Verbose)]
        public void ConcurrentBag_TryTakeSteals()
        {
            if (base.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                base.WriteEvent(4);
            }
        }

        [Event(3, Level=EventLevel.LogAlways)]
        public void ConcurrentDictionary_AcquiringAllLocks(int numOfBuckets)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(3, numOfBuckets);
            }
        }

        [Event(2, Level=EventLevel.LogAlways)]
        public void ConcurrentStack_FastPopFailed(int spinCount)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(2, spinCount);
            }
        }

        [Event(1, Level=EventLevel.LogAlways)]
        public void ConcurrentStack_FastPushFailed(int spinCount)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(1, spinCount);
            }
        }
    }
}

