namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;

    public sealed class ClerkInfo
    {
        private _IMonitorClerks _clerks;
        private object _index;
        private CrmMonitor _monitor;

        internal ClerkInfo(object index, CrmMonitor monitor, _IMonitorClerks clerks)
        {
            this._index = index;
            this._clerks = clerks;
            this._monitor = monitor;
            this._monitor.AddRef();
        }

        ~ClerkInfo()
        {
            this._monitor.Release();
        }

        public string ActivityId
        {
            get
            {
                return (string) this._clerks.ActivityId(this._index);
            }
        }

        public System.EnterpriseServices.CompensatingResourceManager.Clerk Clerk
        {
            get
            {
                return new System.EnterpriseServices.CompensatingResourceManager.Clerk(this._monitor.HoldClerk(this.InstanceId));
            }
        }

        public string Compensator
        {
            get
            {
                return (string) this._clerks.ProgIdCompensator(this._index);
            }
        }

        public string Description
        {
            get
            {
                return (string) this._clerks.Description(this._index);
            }
        }

        public string InstanceId
        {
            get
            {
                return (string) this._clerks.Item(this._index);
            }
        }

        public string TransactionUOW
        {
            get
            {
                return (string) this._clerks.TransactionUOW(this._index);
            }
        }
    }
}

