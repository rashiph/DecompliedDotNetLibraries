namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public sealed class DataContractAttribute : Attribute
    {
        private bool isNameSetExplicit;
        private bool isNamespaceSetExplicit;
        private bool isReference;
        private bool isReferenceSetExplicit;
        private string name;
        private string ns;

        internal bool IsNameSetExplicit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isNameSetExplicit;
            }
        }

        internal bool IsNamespaceSetExplicit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isNamespaceSetExplicit;
            }
        }

        public bool IsReference
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isReference;
            }
            set
            {
                this.isReference = value;
                this.isReferenceSetExplicit = true;
            }
        }

        internal bool IsReferenceSetExplicit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isReferenceSetExplicit;
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

        public string Namespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ns;
            }
            set
            {
                this.ns = value;
                this.isNamespaceSetExplicit = true;
            }
        }
    }
}

