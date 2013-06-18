namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ScopedKnownTypes
    {
        internal Dictionary<XmlQualifiedName, DataContract>[] dataContractDictionaries;
        private int count;
        internal void Push(Dictionary<XmlQualifiedName, DataContract> dataContractDictionary)
        {
            if (this.dataContractDictionaries == null)
            {
                this.dataContractDictionaries = new Dictionary<XmlQualifiedName, DataContract>[4];
            }
            else if (this.count == this.dataContractDictionaries.Length)
            {
                Array.Resize<Dictionary<XmlQualifiedName, DataContract>>(ref this.dataContractDictionaries, this.dataContractDictionaries.Length * 2);
            }
            this.dataContractDictionaries[this.count++] = dataContractDictionary;
        }

        internal void Pop()
        {
            this.count--;
        }

        internal DataContract GetDataContract(XmlQualifiedName qname)
        {
            for (int i = this.count - 1; i >= 0; i--)
            {
                DataContract contract;
                Dictionary<XmlQualifiedName, DataContract> dictionary = this.dataContractDictionaries[i];
                if (dictionary.TryGetValue(qname, out contract))
                {
                    return contract;
                }
            }
            return null;
        }
    }
}

