namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Text;
    using System.Xml;

    internal class GenericInfo : IGenericNameProvider
    {
        private string genericTypeName;
        private List<int> nestedParamCounts;
        private List<GenericInfo> paramGenericInfos;
        private XmlQualifiedName stableName;

        internal GenericInfo(XmlQualifiedName stableName, string genericTypeName)
        {
            this.stableName = stableName;
            this.genericTypeName = genericTypeName;
            this.nestedParamCounts = new List<int>();
            this.nestedParamCounts.Add(0);
        }

        internal void Add(GenericInfo actualParamInfo)
        {
            if (this.paramGenericInfos == null)
            {
                this.paramGenericInfos = new List<GenericInfo>();
            }
            this.paramGenericInfos.Add(actualParamInfo);
        }

        internal void AddToLevel(int level, int count)
        {
            if (level >= this.nestedParamCounts.Count)
            {
                do
                {
                    this.nestedParamCounts.Add((level == this.nestedParamCounts.Count) ? count : 0);
                }
                while (level >= this.nestedParamCounts.Count);
            }
            else
            {
                this.nestedParamCounts[level] += count;
            }
        }

        internal XmlQualifiedName GetExpandedStableName()
        {
            if (this.paramGenericInfos == null)
            {
                return this.stableName;
            }
            return new XmlQualifiedName(DataContract.EncodeLocalName(DataContract.ExpandGenericParameters(XmlConvert.DecodeName(this.stableName.Name), this)), this.stableName.Namespace);
        }

        public string GetGenericTypeName()
        {
            return this.genericTypeName;
        }

        public string GetNamespaces()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.paramGenericInfos.Count; i++)
            {
                builder.Append(" ").Append(this.paramGenericInfos[i].GetStableNamespace());
            }
            return builder.ToString();
        }

        public IList<int> GetNestedParameterCounts()
        {
            return this.nestedParamCounts;
        }

        public int GetParameterCount()
        {
            return this.paramGenericInfos.Count;
        }

        public string GetParameterName(int paramIndex)
        {
            return this.paramGenericInfos[paramIndex].GetExpandedStableName().Name;
        }

        internal string GetStableNamespace()
        {
            return this.stableName.Namespace;
        }

        internal IList<GenericInfo> Parameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.paramGenericInfos;
            }
        }

        public bool ParametersFromBuiltInNamespaces
        {
            get
            {
                bool flag = true;
                for (int i = 0; i < this.paramGenericInfos.Count; i++)
                {
                    if (!flag)
                    {
                        return flag;
                    }
                    flag = DataContract.IsBuiltInNamespace(this.paramGenericInfos[i].GetStableNamespace());
                }
                return flag;
            }
        }

        internal XmlQualifiedName StableName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.stableName;
            }
        }
    }
}

