namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    internal class Lease : MarshalByRefObject, ILease
    {
        internal int id = nextId++;
        internal TimeSpan initialLeaseTime;
        internal LeaseManager leaseManager;
        internal DateTime leaseTime;
        internal MarshalByRefObject managedObject;
        internal static int nextId;
        internal TimeSpan renewOnCallTime;
        internal int sponsorCallThread;
        internal TimeSpan sponsorshipTimeout;
        internal Hashtable sponsorTable;
        internal LeaseState state;

        internal Lease(TimeSpan initialLeaseTime, TimeSpan renewOnCallTime, TimeSpan sponsorshipTimeout, MarshalByRefObject managedObject)
        {
            this.renewOnCallTime = renewOnCallTime;
            this.sponsorshipTimeout = sponsorshipTimeout;
            this.initialLeaseTime = initialLeaseTime;
            this.managedObject = managedObject;
            this.leaseManager = LeaseManager.GetLeaseManager();
            this.sponsorTable = new Hashtable(10);
            this.state = LeaseState.Initial;
        }

        internal void ActivateLease()
        {
            this.leaseTime = DateTime.UtcNow.Add(this.initialLeaseTime);
            this.state = LeaseState.Active;
            this.leaseManager.ActivateLease(this);
        }

        private void AddTime(TimeSpan renewalSpan)
        {
            if (this.state != LeaseState.Expired)
            {
                DateTime time2 = DateTime.UtcNow.Add(renewalSpan);
                if (this.leaseTime.CompareTo(time2) < 0)
                {
                    this.leaseManager.ChangedLeaseTime(this, time2);
                    this.leaseTime = time2;
                    this.state = LeaseState.Active;
                }
            }
        }

        [SecurityCritical]
        internal void Cancel()
        {
            lock (this)
            {
                if (this.state != LeaseState.Expired)
                {
                    this.Remove();
                    RemotingServices.Disconnect(this.managedObject, false);
                    RemotingServices.Disconnect(this);
                }
            }
        }

        [SecurityCritical]
        private ISponsor GetSponsorFromId(object sponsorId)
        {
            object transparentProxy = null;
            RealProxy proxy = sponsorId as RealProxy;
            if (proxy != null)
            {
                transparentProxy = proxy.GetTransparentProxy();
            }
            else
            {
                transparentProxy = sponsorId;
            }
            return (ISponsor) transparentProxy;
        }

        [SecurityCritical]
        private object GetSponsorId(ISponsor obj)
        {
            object obj2 = null;
            if (obj == null)
            {
                return obj2;
            }
            if (RemotingServices.IsTransparentProxy(obj))
            {
                return RemotingServices.GetRealProxy(obj);
            }
            return obj;
        }

        [SecurityCritical]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        [SecurityCritical]
        internal void LeaseExpired(DateTime now)
        {
            lock (this)
            {
                if ((this.state != LeaseState.Expired) && (this.leaseTime.CompareTo(now) < 0))
                {
                    this.ProcessNextSponsor();
                }
            }
        }

        [SecurityCritical]
        private void ProcessNextSponsor()
        {
            object sponsorId = null;
            TimeSpan zero = TimeSpan.Zero;
            lock (this.sponsorTable)
            {
                IDictionaryEnumerator enumerator = this.sponsorTable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    object key = enumerator.Key;
                    SponsorStateInfo info = (SponsorStateInfo) enumerator.Value;
                    if ((info.sponsorState == SponsorState.Initial) && (zero == TimeSpan.Zero))
                    {
                        zero = info.renewalTime;
                        sponsorId = key;
                    }
                    else if (info.renewalTime > zero)
                    {
                        zero = info.renewalTime;
                        sponsorId = key;
                    }
                }
            }
            if (sponsorId != null)
            {
                this.SponsorCall(this.GetSponsorFromId(sponsorId));
            }
            else
            {
                this.Cancel();
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public void Register(ISponsor obj)
        {
            this.Register(obj, TimeSpan.Zero);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public void Register(ISponsor obj, TimeSpan renewalTime)
        {
            lock (this)
            {
                if ((this.state != LeaseState.Expired) && (this.sponsorshipTimeout != TimeSpan.Zero))
                {
                    object sponsorId = this.GetSponsorId(obj);
                    lock (this.sponsorTable)
                    {
                        if (renewalTime > TimeSpan.Zero)
                        {
                            this.AddTime(renewalTime);
                        }
                        if (!this.sponsorTable.ContainsKey(sponsorId))
                        {
                            this.sponsorTable[sponsorId] = new SponsorStateInfo(renewalTime, SponsorState.Initial);
                        }
                    }
                }
            }
        }

        internal void Remove()
        {
            if (this.state != LeaseState.Expired)
            {
                this.state = LeaseState.Expired;
                this.leaseManager.DeleteLease(this);
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public TimeSpan Renew(TimeSpan renewalTime)
        {
            return this.RenewInternal(renewalTime);
        }

        internal TimeSpan RenewInternal(TimeSpan renewalTime)
        {
            lock (this)
            {
                if (this.state == LeaseState.Expired)
                {
                    return TimeSpan.Zero;
                }
                this.AddTime(renewalTime);
                return this.leaseTime.Subtract(DateTime.UtcNow);
            }
        }

        internal void RenewOnCall()
        {
            lock (this)
            {
                if ((this.state != LeaseState.Initial) && (this.state != LeaseState.Expired))
                {
                    this.AddTime(this.renewOnCallTime);
                }
            }
        }

        [SecurityCritical]
        internal void SponsorCall(ISponsor sponsor)
        {
            bool flag = false;
            if (this.state != LeaseState.Expired)
            {
                lock (this.sponsorTable)
                {
                    try
                    {
                        object sponsorId = this.GetSponsorId(sponsor);
                        this.sponsorCallThread = Thread.CurrentThread.GetHashCode();
                        AsyncRenewal renewal = new AsyncRenewal(sponsor.Renewal);
                        SponsorStateInfo info = (SponsorStateInfo) this.sponsorTable[sponsorId];
                        info.sponsorState = SponsorState.Waiting;
                        renewal.BeginInvoke(this, new AsyncCallback(this.SponsorCallback), null);
                        if ((info.sponsorState == SponsorState.Waiting) && (this.state != LeaseState.Expired))
                        {
                            this.leaseManager.RegisterSponsorCall(this, sponsorId, this.sponsorshipTimeout);
                        }
                        this.sponsorCallThread = 0;
                    }
                    catch (Exception)
                    {
                        flag = true;
                        this.sponsorCallThread = 0;
                    }
                }
                if (flag)
                {
                    this.Unregister(sponsor);
                    this.ProcessNextSponsor();
                }
            }
        }

        [SecurityCritical]
        internal void SponsorCallback(IAsyncResult iar)
        {
            if (this.state != LeaseState.Expired)
            {
                if (Thread.CurrentThread.GetHashCode() == this.sponsorCallThread)
                {
                    WaitCallback callBack = new WaitCallback(this.SponsorCallback);
                    ThreadPool.QueueUserWorkItem(callBack, iar);
                }
                else
                {
                    AsyncResult result = (AsyncResult) iar;
                    AsyncRenewal asyncDelegate = (AsyncRenewal) result.AsyncDelegate;
                    ISponsor target = (ISponsor) asyncDelegate.Target;
                    SponsorStateInfo info = null;
                    if (iar.IsCompleted)
                    {
                        bool flag = false;
                        TimeSpan zero = TimeSpan.Zero;
                        try
                        {
                            zero = asyncDelegate.EndInvoke(iar);
                        }
                        catch (Exception)
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            this.Unregister(target);
                            this.ProcessNextSponsor();
                        }
                        else
                        {
                            object sponsorId = this.GetSponsorId(target);
                            lock (this.sponsorTable)
                            {
                                if (this.sponsorTable.ContainsKey(sponsorId))
                                {
                                    info = (SponsorStateInfo) this.sponsorTable[sponsorId];
                                    info.sponsorState = SponsorState.Completed;
                                    info.renewalTime = zero;
                                }
                            }
                            if (info == null)
                            {
                                this.ProcessNextSponsor();
                            }
                            else if (info.renewalTime == TimeSpan.Zero)
                            {
                                this.Unregister(target);
                                this.ProcessNextSponsor();
                            }
                            else
                            {
                                this.RenewInternal(info.renewalTime);
                            }
                        }
                    }
                    else
                    {
                        this.Unregister(target);
                        this.ProcessNextSponsor();
                    }
                }
            }
        }

        [SecurityCritical]
        internal void SponsorCallback(object obj)
        {
            this.SponsorCallback((IAsyncResult) obj);
        }

        [SecurityCritical]
        internal void SponsorTimeout(object sponsorId)
        {
            lock (this)
            {
                if (this.sponsorTable.ContainsKey(sponsorId))
                {
                    lock (this.sponsorTable)
                    {
                        SponsorStateInfo info = (SponsorStateInfo) this.sponsorTable[sponsorId];
                        if (info.sponsorState == SponsorState.Waiting)
                        {
                            this.Unregister(this.GetSponsorFromId(sponsorId));
                            this.ProcessNextSponsor();
                        }
                    }
                }
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public void Unregister(ISponsor sponsor)
        {
            lock (this)
            {
                if (this.state != LeaseState.Expired)
                {
                    object sponsorId = this.GetSponsorId(sponsor);
                    lock (this.sponsorTable)
                    {
                        if (sponsorId != null)
                        {
                            this.leaseManager.DeleteSponsor(sponsorId);
                            SponsorStateInfo info1 = (SponsorStateInfo) this.sponsorTable[sponsorId];
                            this.sponsorTable.Remove(sponsorId);
                        }
                    }
                }
            }
        }

        public TimeSpan CurrentLeaseTime
        {
            [SecurityCritical]
            get
            {
                return this.leaseTime.Subtract(DateTime.UtcNow);
            }
        }

        public LeaseState CurrentState
        {
            [SecurityCritical]
            get
            {
                return this.state;
            }
        }

        public TimeSpan InitialLeaseTime
        {
            [SecurityCritical]
            get
            {
                return this.initialLeaseTime;
            }
            [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
            set
            {
                if (this.state != LeaseState.Initial)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Lifetime_InitialStateInitialLeaseTime", new object[] { this.state.ToString() }));
                }
                this.initialLeaseTime = value;
                if (TimeSpan.Zero.CompareTo(value) >= 0)
                {
                    this.state = LeaseState.Null;
                }
            }
        }

        public TimeSpan RenewOnCallTime
        {
            [SecurityCritical]
            get
            {
                return this.renewOnCallTime;
            }
            [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
            set
            {
                if (this.state != LeaseState.Initial)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Lifetime_InitialStateRenewOnCall", new object[] { this.state.ToString() }));
                }
                this.renewOnCallTime = value;
            }
        }

        public TimeSpan SponsorshipTimeout
        {
            [SecurityCritical]
            get
            {
                return this.sponsorshipTimeout;
            }
            [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
            set
            {
                if (this.state != LeaseState.Initial)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Lifetime_InitialStateSponsorshipTimeout", new object[] { this.state.ToString() }));
                }
                this.sponsorshipTimeout = value;
            }
        }

        internal delegate TimeSpan AsyncRenewal(ILease lease);

        [Serializable]
        internal enum SponsorState
        {
            Initial,
            Waiting,
            Completed
        }

        internal sealed class SponsorStateInfo
        {
            internal TimeSpan renewalTime;
            internal Lease.SponsorState sponsorState;

            internal SponsorStateInfo(TimeSpan renewalTime, Lease.SponsorState sponsorState)
            {
                this.renewalTime = renewalTime;
                this.sponsorState = sponsorState;
            }
        }
    }
}

