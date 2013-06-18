namespace Microsoft.Transactions.Bridge
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Transactions;
    using System.Transactions;

    internal class EnlistmentOptions
    {
        private string description;
        private TimeSpan expires = new TimeSpan(0, 5, 0);
        private System.ServiceModel.Transactions.IsolationFlags isoFlags;
        private IsolationLevel isoLevel = IsolationLevel.Unspecified;

        public string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.description;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.description = value;
            }
        }

        public TimeSpan Expires
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.expires;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.expires = value;
            }
        }

        public System.ServiceModel.Transactions.IsolationFlags IsolationFlags
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isoFlags;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.isoFlags = value;
            }
        }

        public ulong IsolationFlagsLong
        {
            get
            {
                return (ulong) ((long) this.isoFlags);
            }
            set
            {
                this.isoFlags = (System.ServiceModel.Transactions.IsolationFlags) ((int) value);
            }
        }

        public IsolationLevel IsoLevel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isoLevel;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.isoLevel = value;
            }
        }
    }
}

