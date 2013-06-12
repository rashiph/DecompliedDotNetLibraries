namespace System.ComponentModel.Design
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class DesignerTransaction : IDisposable
    {
        private bool canceled;
        private bool committed;
        private string desc;
        private bool suppressedFinalization;

        protected DesignerTransaction() : this("")
        {
        }

        protected DesignerTransaction(string description)
        {
            this.desc = description;
        }

        public void Cancel()
        {
            if (!this.canceled && !this.committed)
            {
                this.canceled = true;
                GC.SuppressFinalize(this);
                this.suppressedFinalization = true;
                this.OnCancel();
            }
        }

        public void Commit()
        {
            if (!this.committed && !this.canceled)
            {
                this.committed = true;
                GC.SuppressFinalize(this);
                this.suppressedFinalization = true;
                this.OnCommit();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Cancel();
        }

        ~DesignerTransaction()
        {
            this.Dispose(false);
        }

        protected abstract void OnCancel();
        protected abstract void OnCommit();
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            if (!this.suppressedFinalization)
            {
                GC.SuppressFinalize(this);
            }
        }

        public bool Canceled
        {
            get
            {
                return this.canceled;
            }
        }

        public bool Committed
        {
            get
            {
                return this.committed;
            }
        }

        public string Description
        {
            get
            {
                return this.desc;
            }
        }
    }
}

