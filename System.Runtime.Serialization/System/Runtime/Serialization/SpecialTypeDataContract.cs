namespace System.Runtime.Serialization
{
    using System;
    using System.Security;
    using System.Xml;

    internal sealed class SpecialTypeDataContract : DataContract
    {
        [SecurityCritical]
        private SpecialTypeDataContractCriticalHelper helper;

        [SecuritySafeCritical]
        public SpecialTypeDataContract(Type type) : base(new SpecialTypeDataContractCriticalHelper(type))
        {
            this.helper = base.Helper as SpecialTypeDataContractCriticalHelper;
        }

        [SecuritySafeCritical]
        public SpecialTypeDataContract(Type type, XmlDictionaryString name, XmlDictionaryString ns) : base(new SpecialTypeDataContractCriticalHelper(type, name, ns))
        {
            this.helper = base.Helper as SpecialTypeDataContractCriticalHelper;
        }

        internal override bool IsBuiltInDataContract
        {
            get
            {
                return true;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class SpecialTypeDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            internal SpecialTypeDataContractCriticalHelper(Type type) : base(type)
            {
            }

            internal SpecialTypeDataContractCriticalHelper(Type type, XmlDictionaryString name, XmlDictionaryString ns) : base(type)
            {
                base.SetDataContractName(name, ns);
            }
        }
    }
}

