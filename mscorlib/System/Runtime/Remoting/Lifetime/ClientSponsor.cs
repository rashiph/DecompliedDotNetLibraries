namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true), SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class ClientSponsor : MarshalByRefObject, ISponsor
    {
        private TimeSpan m_renewalTime;
        private Hashtable sponsorTable;

        public ClientSponsor()
        {
            this.sponsorTable = new Hashtable(10);
            this.m_renewalTime = TimeSpan.FromMinutes(2.0);
        }

        public ClientSponsor(TimeSpan renewalTime)
        {
            this.sponsorTable = new Hashtable(10);
            this.m_renewalTime = TimeSpan.FromMinutes(2.0);
            this.m_renewalTime = renewalTime;
        }

        [SecurityCritical]
        public void Close()
        {
            lock (this.sponsorTable)
            {
                IDictionaryEnumerator enumerator = this.sponsorTable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    ((ILease) enumerator.Value).Unregister(this);
                }
                this.sponsorTable.Clear();
            }
        }

        [SecuritySafeCritical]
        ~ClientSponsor()
        {
        }

        [SecurityCritical]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        [SecurityCritical]
        public bool Register(MarshalByRefObject obj)
        {
            ILease lifetimeService = (ILease) obj.GetLifetimeService();
            if (lifetimeService == null)
            {
                return false;
            }
            lifetimeService.Register(this);
            lock (this.sponsorTable)
            {
                this.sponsorTable[obj] = lifetimeService;
            }
            return true;
        }

        [SecurityCritical]
        public TimeSpan Renewal(ILease lease)
        {
            return this.m_renewalTime;
        }

        [SecurityCritical]
        public void Unregister(MarshalByRefObject obj)
        {
            ILease lease = null;
            lock (this.sponsorTable)
            {
                lease = (ILease) this.sponsorTable[obj];
            }
            if (lease != null)
            {
                lease.Unregister(this);
            }
        }

        public TimeSpan RenewalTime
        {
            get
            {
                return this.m_renewalTime;
            }
            set
            {
                this.m_renewalTime = value;
            }
        }
    }
}

