namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;

    internal class GenericNameProvider : IGenericNameProvider
    {
        private object[] genericParams;
        private string genericTypeName;
        private IList<int> nestedParamCounts;

        internal GenericNameProvider(Type type) : this(DataContract.GetClrTypeFullName(type.GetGenericTypeDefinition()), type.GetGenericArguments())
        {
        }

        internal GenericNameProvider(string genericTypeName, object[] genericParams)
        {
            string str;
            string str2;
            this.genericTypeName = genericTypeName;
            this.genericParams = new object[genericParams.Length];
            genericParams.CopyTo(this.genericParams, 0);
            DataContract.GetClrNameAndNamespace(genericTypeName, out str, out str2);
            this.nestedParamCounts = DataContract.GetDataContractNameForGenericName(str, null);
        }

        public string GetGenericTypeName()
        {
            return this.genericTypeName;
        }

        public string GetNamespaces()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.GetParameterCount(); i++)
            {
                builder.Append(" ").Append(this.GetStableName(i).Namespace);
            }
            return builder.ToString();
        }

        public IList<int> GetNestedParameterCounts()
        {
            return this.nestedParamCounts;
        }

        public int GetParameterCount()
        {
            return this.genericParams.Length;
        }

        public string GetParameterName(int paramIndex)
        {
            return this.GetStableName(paramIndex).Name;
        }

        private XmlQualifiedName GetStableName(int i)
        {
            object obj2 = this.genericParams[i];
            XmlQualifiedName name = obj2 as XmlQualifiedName;
            if (name == null)
            {
                Type type = obj2 as Type;
                if (type != null)
                {
                    this.genericParams[i] = name = DataContract.GetStableName(type);
                    return name;
                }
                this.genericParams[i] = name = ((DataContract) obj2).StableName;
            }
            return name;
        }

        public bool ParametersFromBuiltInNamespaces
        {
            get
            {
                bool flag = true;
                for (int i = 0; i < this.GetParameterCount(); i++)
                {
                    if (!flag)
                    {
                        return flag;
                    }
                    flag = DataContract.IsBuiltInNamespace(this.GetStableName(i).Namespace);
                }
                return flag;
            }
        }
    }
}

