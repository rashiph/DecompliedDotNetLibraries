namespace System.ServiceModel.Description
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;

    internal static class NamingHelper
    {
        internal const string DefaultNamespace = "http://tempuri.org/";
        internal const string DefaultServiceName = "service";
        internal const string MSNamespace = "http://schemas.microsoft.com/2005/07/ServiceModel";

        internal static void CheckUriParameter(string ns, string paramName)
        {
            Uri uri;
            if (!Uri.TryCreate(ns, UriKind.RelativeOrAbsolute, out uri))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(paramName, System.ServiceModel.SR.GetString("SFXUnvalidNamespaceParam", new object[] { ns }));
            }
        }

        internal static void CheckUriProperty(string ns, string propName)
        {
            Uri uri;
            if (!Uri.TryCreate(ns, UriKind.RelativeOrAbsolute, out uri))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFXUnvalidNamespaceValue", new object[] { ns, propName }));
            }
        }

        internal static string CodeName(string name)
        {
            return XmlConvert.DecodeName(name);
        }

        internal static string CombineUriStrings(string baseUri, string path)
        {
            if (Uri.IsWellFormedUriString(path, UriKind.Absolute) || (path == string.Empty))
            {
                return path;
            }
            if (baseUri.EndsWith("/", StringComparison.Ordinal))
            {
                return (baseUri + (path.StartsWith("/", StringComparison.Ordinal) ? path.Substring(1) : path));
            }
            return (baseUri + (path.StartsWith("/", StringComparison.Ordinal) ? path : ("/" + path)));
        }

        internal static XmlQualifiedName GetContractName(Type contractType, string name, string ns)
        {
            System.ServiceModel.Description.XmlName name2 = new System.ServiceModel.Description.XmlName(name ?? TypeName(contractType));
            if (ns == null)
            {
                ns = "http://tempuri.org/";
            }
            return new XmlQualifiedName(name2.EncodedName, ns);
        }

        internal static string GetMessageAction(OperationDescription operation, bool isResponse)
        {
            ContractDescription declaringContract = operation.DeclaringContract;
            XmlQualifiedName contractName = new XmlQualifiedName(declaringContract.Name, declaringContract.Namespace);
            return GetMessageAction(contractName, operation.CodeName, null, isResponse);
        }

        internal static string GetMessageAction(XmlQualifiedName contractName, string opname, string action, bool isResponse)
        {
            if (action != null)
            {
                return action;
            }
            StringBuilder builder = new StringBuilder(0x40);
            if (string.IsNullOrEmpty(contractName.Namespace))
            {
                builder.Append("urn:");
            }
            else
            {
                builder.Append(contractName.Namespace);
                if (!contractName.Namespace.EndsWith("/", StringComparison.Ordinal))
                {
                    builder.Append('/');
                }
            }
            builder.Append(contractName.Name);
            builder.Append('/');
            action = isResponse ? (opname + "Response") : opname;
            return CombineUriStrings(builder.ToString(), action);
        }

        internal static System.ServiceModel.Description.XmlName GetOperationName(string logicalMethodName, string name)
        {
            return new System.ServiceModel.Description.XmlName(string.IsNullOrEmpty(name) ? logicalMethodName : name);
        }

        internal static string GetUniqueName(string baseName, DoesNameExist doesNameExist, object nameCollection)
        {
            for (int i = 0; i < 0x7fffffff; i++)
            {
                string name = (i > 0) ? (baseName + i) : baseName;
                if (!doesNameExist(name, nameCollection))
                {
                    return name;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot generate unique name for name {0}", new object[] { baseName })));
        }

        private static bool IsAlpha(char ch)
        {
            return (((ch >= 'A') && (ch <= 'Z')) || ((ch >= 'a') && (ch <= 'z')));
        }

        private static bool IsAsciiLocalName(string localName)
        {
            if (!IsAlpha(localName[0]))
            {
                return false;
            }
            for (int i = 1; i < localName.Length; i++)
            {
                char ch = localName[i];
                if (!IsAlpha(ch) && !IsDigit(ch))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsDigit(char ch)
        {
            return ((ch >= '0') && (ch <= '9'));
        }

        internal static bool IsValidNCName(string name)
        {
            try
            {
                XmlConvert.VerifyNCName(name);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        internal static string TypeName(Type t)
        {
            if (t.IsGenericType || t.ContainsGenericParameters)
            {
                Type[] genericArguments = t.GetGenericArguments();
                int index = t.Name.IndexOf('`');
                string str = (index > 0) ? t.Name.Substring(0, index) : t.Name;
                str = str + "Of";
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    str = str + "_" + TypeName(genericArguments[i]);
                }
                return str;
            }
            if (t.IsArray)
            {
                return ("ArrayOf" + TypeName(t.GetElementType()));
            }
            return t.Name;
        }

        internal static string XmlName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            if (IsAsciiLocalName(name))
            {
                return name;
            }
            if (IsValidNCName(name))
            {
                return name;
            }
            return XmlConvert.EncodeLocalName(name);
        }

        internal delegate bool DoesNameExist(string name, object nameCollection);
    }
}

