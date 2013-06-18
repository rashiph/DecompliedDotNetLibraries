namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.EnterpriseServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    public sealed class Clerk
    {
        private CrmLogControl _control;
        private CrmMonitorLogRecords _monitor;

        internal Clerk(CrmLogControl logControl)
        {
            this._control = logControl;
            this._monitor = this._control.GetMonitor();
        }

        public Clerk(string compensator, string description, CompensatorOptions flags)
        {
            SecurityPermission permission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            permission.Demand();
            permission.Assert();
            this.Init(compensator, description, flags);
        }

        public Clerk(Type compensator, string description, CompensatorOptions flags)
        {
            SecurityPermission permission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            permission.Demand();
            permission.Assert();
            this.ValidateCompensator(compensator);
            string str = "{" + Marshal.GenerateGuidForType(compensator) + "}";
            this.Init(str, description, flags);
        }

        ~Clerk()
        {
            if (this._monitor != null)
            {
                this._monitor.Dispose();
            }
            if (this._control != null)
            {
                this._control.Dispose();
            }
        }

        public void ForceLog()
        {
            this._control.ForceLog();
        }

        public void ForceTransactionToAbort()
        {
            this._control.ForceTransactionToAbort();
        }

        public void ForgetLogRecord()
        {
            this._control.ForgetLogRecord();
        }

        private void Init(string compensator, string description, CompensatorOptions flags)
        {
            this._control = new CrmLogControl();
            this._control.RegisterCompensator(compensator, description, (int modopt(IsLong)) flags);
            this._monitor = this._control.GetMonitor();
        }

        private void ValidateCompensator(Type compensator)
        {
            if (!compensator.IsSubclassOf(typeof(Compensator)))
            {
                throw new ArgumentException(Resource.FormatString("CRM_CompensatorDerive"));
            }
            if (!new RegistrationServices().TypeRequiresRegistration(compensator))
            {
                throw new ArgumentException(Resource.FormatString("CRM_CompensatorConstructor"));
            }
            ServicedComponent sc = (ServicedComponent) Activator.CreateInstance(compensator);
            if (sc == null)
            {
                throw new ArgumentException(Resource.FormatString("CRM_CompensatorActivate"));
            }
            ServicedComponent.DisposeObject(sc);
        }

        public void WriteLogRecord(object record)
        {
            byte[] b = Packager.Serialize(record);
            this._control.WriteLogRecord(b);
        }

        public int LogRecordCount
        {
            get
            {
                return this._monitor.GetCount();
            }
        }

        private System.EnterpriseServices.CompensatingResourceManager.TransactionState TransactionState
        {
            get
            {
                return (System.EnterpriseServices.CompensatingResourceManager.TransactionState) this._monitor.GetTransactionState();
            }
        }

        public string TransactionUOW
        {
            get
            {
                return this._control.GetTransactionUOW();
            }
        }
    }
}

