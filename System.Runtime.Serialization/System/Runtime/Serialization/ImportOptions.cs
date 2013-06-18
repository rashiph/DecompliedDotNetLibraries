namespace System.Runtime.Serialization
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Runtime;

    public class ImportOptions
    {
        private CodeDomProvider codeProvider;
        private IDataContractSurrogate dataContractSurrogate;
        private bool enableDataBinding;
        private bool generateInternal;
        private bool generateSerializable;
        private bool importXmlType;
        private IDictionary<string, string> namespaces;
        private ICollection<Type> referencedCollectionTypes;
        private ICollection<Type> referencedTypes;

        public CodeDomProvider CodeProvider
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.codeProvider;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.codeProvider = value;
            }
        }

        public IDataContractSurrogate DataContractSurrogate
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dataContractSurrogate;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.dataContractSurrogate = value;
            }
        }

        public bool EnableDataBinding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enableDataBinding;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.enableDataBinding = value;
            }
        }

        public bool GenerateInternal
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.generateInternal;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.generateInternal = value;
            }
        }

        public bool GenerateSerializable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.generateSerializable;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.generateSerializable = value;
            }
        }

        public bool ImportXmlType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.importXmlType;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.importXmlType = value;
            }
        }

        public IDictionary<string, string> Namespaces
        {
            get
            {
                if (this.namespaces == null)
                {
                    this.namespaces = new Dictionary<string, string>();
                }
                return this.namespaces;
            }
        }

        public ICollection<Type> ReferencedCollectionTypes
        {
            get
            {
                if (this.referencedCollectionTypes == null)
                {
                    this.referencedCollectionTypes = new List<Type>();
                }
                return this.referencedCollectionTypes;
            }
        }

        public ICollection<Type> ReferencedTypes
        {
            get
            {
                if (this.referencedTypes == null)
                {
                    this.referencedTypes = new List<Type>();
                }
                return this.referencedTypes;
            }
        }
    }
}

