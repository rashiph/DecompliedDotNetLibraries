namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited=false, AllowMultiple=false)]
    public sealed class DataMemberAttribute : Attribute
    {
        private bool emitDefaultValue = true;
        private bool isNameSetExplicit;
        private bool isRequired;
        private string name;
        private int order = -1;

        public bool EmitDefaultValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.emitDefaultValue;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.emitDefaultValue = value;
            }
        }

        internal bool IsNameSetExplicit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isNameSetExplicit;
            }
        }

        public bool IsRequired
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isRequired;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.isRequired = value;
            }
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
                this.name = value;
                this.isNameSetExplicit = true;
            }
        }

        public int Order
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.order;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("OrderCannotBeNegative")));
                }
                this.order = value;
            }
        }
    }
}

