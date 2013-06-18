namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Module | AttributeTargets.Assembly, Inherited=false, AllowMultiple=true)]
    public sealed class ContractNamespaceAttribute : Attribute
    {
        private string clrNamespace;
        private string contractNamespace;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ContractNamespaceAttribute(string contractNamespace)
        {
            this.contractNamespace = contractNamespace;
        }

        public string ClrNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.clrNamespace;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.clrNamespace = value;
            }
        }

        public string ContractNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.contractNamespace;
            }
        }
    }
}

