namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public sealed class CollectionDataContractAttribute : Attribute
    {
        private bool isItemNameSetExplicit;
        private bool isKeyNameSetExplicit;
        private bool isNameSetExplicit;
        private bool isNamespaceSetExplicit;
        private bool isReference;
        private bool isReferenceSetExplicit;
        private bool isValueNameSetExplicit;
        private string itemName;
        private string keyName;
        private string name;
        private string ns;
        private string valueName;

        internal bool IsItemNameSetExplicit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isItemNameSetExplicit;
            }
        }

        internal bool IsKeyNameSetExplicit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isKeyNameSetExplicit;
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

        internal bool IsValueNameSetExplicit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isValueNameSetExplicit;
            }
        }

        public string ItemName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.itemName;
            }
            set
            {
                this.itemName = value;
                this.isItemNameSetExplicit = true;
            }
        }

        public string KeyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.keyName;
            }
            set
            {
                this.keyName = value;
                this.isKeyNameSetExplicit = true;
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

        public string ValueName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.valueName;
            }
            set
            {
                this.valueName = value;
                this.isValueNameSetExplicit = true;
            }
        }
    }
}

