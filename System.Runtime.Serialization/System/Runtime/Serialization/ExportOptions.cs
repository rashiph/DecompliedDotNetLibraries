namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime;

    public class ExportOptions
    {
        private IDataContractSurrogate dataContractSurrogate;
        private Collection<Type> knownTypes;

        internal IDataContractSurrogate GetSurrogate()
        {
            return this.dataContractSurrogate;
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

        public Collection<Type> KnownTypes
        {
            get
            {
                if (this.knownTypes == null)
                {
                    this.knownTypes = new Collection<Type>();
                }
                return this.knownTypes;
            }
        }
    }
}

