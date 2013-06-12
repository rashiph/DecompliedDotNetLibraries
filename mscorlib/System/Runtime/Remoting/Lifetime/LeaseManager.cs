namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Threading;

    internal class LeaseManager
    {
        private TimerCallback leaseTimeAnalyzerDelegate;
        private volatile Timer leaseTimer;
        private Hashtable leaseToTimeTable = new Hashtable();
        private TimeSpan pollTime;
        private Hashtable sponsorTable = new Hashtable();
        private ArrayList tempObjects = new ArrayList(10);
        private AutoResetEvent waitHandle;

        [SecurityCritical]
        private LeaseManager(TimeSpan pollTime)
        {
            this.pollTime = pollTime;
            this.leaseTimeAnalyzerDelegate = new TimerCallback(this.LeaseTimeAnalyzer);
            this.waitHandle = new AutoResetEvent(false);
            this.leaseTimer = new Timer(this.leaseTimeAnalyzerDelegate, null, -1, -1);
            this.leaseTimer.Change((int) pollTime.TotalMilliseconds, -1);
        }

        internal void ActivateLease(Lease lease)
        {
            lock (this.leaseToTimeTable)
            {
                this.leaseToTimeTable[lease] = lease.leaseTime;
            }
        }

        internal void ChangedLeaseTime(Lease lease, DateTime newTime)
        {
            lock (this.leaseToTimeTable)
            {
                this.leaseToTimeTable[lease] = newTime;
            }
        }

        internal void ChangePollTime(TimeSpan pollTime)
        {
            this.pollTime = pollTime;
        }

        internal void DeleteLease(Lease lease)
        {
            lock (this.leaseToTimeTable)
            {
                this.leaseToTimeTable.Remove(lease);
            }
        }

        internal void DeleteSponsor(object sponsorId)
        {
            lock (this.sponsorTable)
            {
                this.sponsorTable.Remove(sponsorId);
            }
        }

        [Conditional("_LOGGING")]
        internal void DumpLeases(Lease[] leases)
        {
            for (int i = 0; i < leases.Length; i++)
            {
            }
        }

        internal ILease GetLease(MarshalByRefObject obj)
        {
            bool fServer = true;
            Identity identity = MarshalByRefObject.GetIdentity(obj, out fServer);
            if (identity == null)
            {
                return null;
            }
            return identity.Lease;
        }

        internal static LeaseManager GetLeaseManager()
        {
            return Thread.GetDomain().RemotingData.LeaseManager;
        }

        [SecurityCritical]
        internal static LeaseManager GetLeaseManager(TimeSpan pollTime)
        {
            DomainSpecificRemotingData remotingData = Thread.GetDomain().RemotingData;
            LeaseManager leaseManager = remotingData.LeaseManager;
            if (leaseManager != null)
            {
                return leaseManager;
            }
            lock (remotingData)
            {
                if (remotingData.LeaseManager == null)
                {
                    remotingData.LeaseManager = new LeaseManager(pollTime);
                }
                return remotingData.LeaseManager;
            }
        }

        internal static bool IsInitialized()
        {
            return (Thread.GetDomain().RemotingData.LeaseManager != null);
        }

        [SecurityCritical]
        private void LeaseTimeAnalyzer(object state)
        {
            DateTime utcNow = DateTime.UtcNow;
            lock (this.leaseToTimeTable)
            {
                IDictionaryEnumerator enumerator = this.leaseToTimeTable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DateTime time2 = (DateTime) enumerator.Value;
                    Lease key = (Lease) enumerator.Key;
                    if (time2.CompareTo(utcNow) < 0)
                    {
                        this.tempObjects.Add(key);
                    }
                }
                for (int k = 0; k < this.tempObjects.Count; k++)
                {
                    Lease lease2 = (Lease) this.tempObjects[k];
                    this.leaseToTimeTable.Remove(lease2);
                }
            }
            for (int i = 0; i < this.tempObjects.Count; i++)
            {
                Lease lease3 = (Lease) this.tempObjects[i];
                if (lease3 != null)
                {
                    lease3.LeaseExpired(utcNow);
                }
            }
            this.tempObjects.Clear();
            lock (this.sponsorTable)
            {
                IDictionaryEnumerator enumerator2 = this.sponsorTable.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    object obj1 = enumerator2.Key;
                    SponsorInfo info = (SponsorInfo) enumerator2.Value;
                    if (info.sponsorWaitTime.CompareTo(utcNow) < 0)
                    {
                        this.tempObjects.Add(info);
                    }
                }
                for (int m = 0; m < this.tempObjects.Count; m++)
                {
                    SponsorInfo info2 = (SponsorInfo) this.tempObjects[m];
                    this.sponsorTable.Remove(info2.sponsorId);
                }
            }
            for (int j = 0; j < this.tempObjects.Count; j++)
            {
                SponsorInfo info3 = (SponsorInfo) this.tempObjects[j];
                if ((info3 != null) && (info3.lease != null))
                {
                    info3.lease.SponsorTimeout(info3.sponsorId);
                    this.tempObjects[j] = null;
                }
            }
            this.tempObjects.Clear();
            this.leaseTimer.Change((int) this.pollTime.TotalMilliseconds, -1);
        }

        internal void RegisterSponsorCall(Lease lease, object sponsorId, TimeSpan sponsorshipTimeOut)
        {
            lock (this.sponsorTable)
            {
                DateTime sponsorWaitTime = DateTime.UtcNow.Add(sponsorshipTimeOut);
                this.sponsorTable[sponsorId] = new SponsorInfo(lease, sponsorId, sponsorWaitTime);
            }
        }

        internal class SponsorInfo
        {
            internal Lease lease;
            internal object sponsorId;
            internal DateTime sponsorWaitTime;

            internal SponsorInfo(Lease lease, object sponsorId, DateTime sponsorWaitTime)
            {
                this.lease = lease;
                this.sponsorId = sponsorId;
                this.sponsorWaitTime = sponsorWaitTime;
            }
        }
    }
}

