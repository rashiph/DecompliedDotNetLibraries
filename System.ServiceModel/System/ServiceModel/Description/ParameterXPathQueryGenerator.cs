namespace System.ServiceModel.Description
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    public static class ParameterXPathQueryGenerator
    {
        private const string NsSeparator = ":";
        private const string ServiceContractPrefix = "xgSc";
        private const string XPathSeparator = "/";

        public static string CreateFromDataContractSerializer(XName serviceContractName, string operationName, string parameterName, bool isReply, Type type, MemberInfo[] pathToMember, out XmlNamespaceManager namespaces)
        {
            if (type == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("type"));
            }
            if (pathToMember == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("pathToMember"));
            }
            if (operationName == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("operationName"));
            }
            if (serviceContractName == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceContractName"));
            }
            if (isReply)
            {
                operationName = operationName + "Response";
            }
            StringBuilder rootElementXpath = new StringBuilder("/xgSc:" + operationName);
            rootElementXpath.Append("/xgSc:" + parameterName);
            string str = XPathQueryGenerator.CreateFromDataContractSerializer(type, pathToMember, rootElementXpath, out namespaces);
            string namespaceName = serviceContractName.NamespaceName;
            if (string.IsNullOrEmpty(namespaceName))
            {
                namespaceName = "http://tempuri.org/";
            }
            namespaces.AddNamespace("xgSc", namespaceName);
            return str;
        }
    }
}

