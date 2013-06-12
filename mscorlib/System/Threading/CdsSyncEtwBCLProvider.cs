namespace System.Threading
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Runtime.CompilerServices;

    [FriendAccessAllowed]
    internal sealed class CdsSyncEtwBCLProvider : EventProviderBase
    {
        public static CdsSyncEtwBCLProvider Log = new CdsSyncEtwBCLProvider();

        private CdsSyncEtwBCLProvider() : base(new Guid(0xec631d38, 0x466b, 0x4290, 0x93, 6, 0x83, 0x49, 0x71, 0xba, 2, 0x17))
        {
        }

        [Event(3, Level=EventLevel.Verbose)]
        public void Barrier_PhaseFinished(bool currentSense, long phaseNum)
        {
            if (base.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                base.WriteEvent(3, new object[] { currentSense, phaseNum });
            }
        }

        [Event(1, Level=EventLevel.LogAlways)]
        public void SpinLock_FastPathFailed(int ownerID)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(1, ownerID);
            }
        }

        [Event(2, Level=EventLevel.LogAlways)]
        public void SpinWait_NextSpinWillYield()
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(2);
            }
        }
    }
}

