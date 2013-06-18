namespace System.Messaging
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    public class Trustee
    {
        private string name;
        private string systemName;
        private System.Messaging.TrusteeType trusteeType;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Trustee()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Trustee(string name) : this(name, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Trustee(string name, string systemName) : this(name, systemName, System.Messaging.TrusteeType.Unknown)
        {
        }

        public Trustee(string name, string systemName, System.Messaging.TrusteeType trusteeType)
        {
            this.Name = name;
            this.SystemName = systemName;
            this.TrusteeType = trusteeType;
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.name = value;
            }
        }

        public string SystemName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.systemName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.systemName = value;
            }
        }

        public System.Messaging.TrusteeType TrusteeType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.trusteeType;
            }
            set
            {
                if (!ValidationUtility.ValidateTrusteeType(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Messaging.TrusteeType));
                }
                this.trusteeType = value;
            }
        }
    }
}

