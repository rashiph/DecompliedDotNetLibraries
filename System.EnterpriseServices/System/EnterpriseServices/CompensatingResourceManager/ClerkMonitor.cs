namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class ClerkMonitor : IEnumerable
    {
        internal _IMonitorClerks _clerks;
        internal CrmMonitor _monitor;
        internal int _version;

        public ClerkMonitor()
        {
            SecurityPermission permission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            permission.Demand();
            permission.Assert();
            this._monitor = new CrmMonitor();
            this._version = 0;
        }

        ~ClerkMonitor()
        {
            this._monitor.Release();
        }

        public IEnumerator GetEnumerator()
        {
            return new ClerkMonitorEnumerator(this);
        }

        public void Populate()
        {
            this._clerks = (_IMonitorClerks) this._monitor.GetClerks();
            this._version++;
        }

        public int Count
        {
            get
            {
                if (this._clerks == null)
                {
                    return 0;
                }
                return this._clerks.Count();
            }
        }

        public ClerkInfo this[int index]
        {
            get
            {
                if (this._clerks == null)
                {
                    return null;
                }
                return new ClerkInfo(index, this._monitor, this._clerks);
            }
        }

        public ClerkInfo this[string index]
        {
            get
            {
                if (this._clerks == null)
                {
                    return null;
                }
                return new ClerkInfo(index, this._monitor, this._clerks);
            }
        }
    }
}

