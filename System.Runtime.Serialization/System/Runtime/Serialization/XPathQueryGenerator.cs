namespace System.Runtime.Serialization
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    public static class XPathQueryGenerator
    {
        private const string NsSeparator = ":";
        private const string XPathSeparator = "/";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string CreateFromDataContractSerializer(Type type, MemberInfo[] pathToMember, out XmlNamespaceManager namespaces)
        {
            return CreateFromDataContractSerializer(type, pathToMember, null, out namespaces);
        }

        public static string CreateFromDataContractSerializer(Type type, MemberInfo[] pathToMember, StringBuilder rootElementXpath, out XmlNamespaceManager namespaces)
        {
            ExportContext context;
            if (type == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("type"));
            }
            if (pathToMember == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("pathToMember"));
            }
            DataContract dataContract = DataContract.GetDataContract(type);
            if (rootElementXpath == null)
            {
                context = new ExportContext(dataContract);
            }
            else
            {
                context = new ExportContext(rootElementXpath);
            }
            for (int i = 0; i < pathToMember.Length; i++)
            {
                dataContract = ProcessDataContract(dataContract, context, pathToMember[i]);
            }
            namespaces = context.Namespaces;
            return context.XPath;
        }

        private static DataContract ProcessClassDataContract(ClassDataContract contract, ExportContext context, MemberInfo memberNode)
        {
            string prefix = context.SetNamespace(contract.Namespace.Value);
            if (contract.Members != null)
            {
                foreach (DataMember member in contract.Members)
                {
                    if (member.MemberInfo == memberNode)
                    {
                        context.WriteChildToContext(member, prefix);
                        return member.MemberTypeContract;
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("QueryGeneratorPathToMemberNotFound")));
        }

        private static DataContract ProcessDataContract(DataContract contract, ExportContext context, MemberInfo memberNode)
        {
            if (!(contract is ClassDataContract))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("QueryGeneratorPathToMemberNotFound")));
            }
            return ProcessClassDataContract((ClassDataContract) contract, context, memberNode);
        }

        private class ExportContext
        {
            private XmlNamespaceManager namespaces;
            private int nextPrefix;
            private StringBuilder xPathBuilder;

            public ExportContext(DataContract rootContract)
            {
                this.namespaces = new XmlNamespaceManager(new NameTable());
                string str = this.SetNamespace(rootContract.TopLevelElementNamespace.Value);
                this.xPathBuilder = new StringBuilder("/" + str + ":" + rootContract.TopLevelElementName.Value);
            }

            public ExportContext(StringBuilder rootContractXPath)
            {
                this.namespaces = new XmlNamespaceManager(new NameTable());
                this.xPathBuilder = rootContractXPath;
            }

            public string SetNamespace(string ns)
            {
                string prefix = this.namespaces.LookupPrefix(ns);
                if ((prefix == null) || (prefix.Length == 0))
                {
                    prefix = "xg" + this.nextPrefix++.ToString(NumberFormatInfo.InvariantInfo);
                    this.Namespaces.AddNamespace(prefix, ns);
                }
                return prefix;
            }

            public void WriteChildToContext(DataMember contextMember, string prefix)
            {
                this.xPathBuilder.Append("/" + prefix + ":" + contextMember.Name);
            }

            public XmlNamespaceManager Namespaces
            {
                get
                {
                    return this.namespaces;
                }
            }

            public string XPath
            {
                get
                {
                    return this.xPathBuilder.ToString();
                }
            }
        }
    }
}

