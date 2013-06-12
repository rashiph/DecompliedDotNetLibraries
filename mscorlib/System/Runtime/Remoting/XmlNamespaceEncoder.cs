namespace System.Runtime.Remoting
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    internal static class XmlNamespaceEncoder
    {
        [SecurityCritical]
        internal static string GetTypeNameForSoapActionNamespace(string uri, out bool assemblyIncluded)
        {
            assemblyIncluded = false;
            string fullNS = SoapServices.fullNS;
            string namespaceNS = SoapServices.namespaceNS;
            if (uri.StartsWith(fullNS, StringComparison.Ordinal))
            {
                uri = uri.Substring(fullNS.Length);
                char[] separator = new char[] { '/' };
                string[] strArray = uri.Split(separator);
                if (strArray.Length != 2)
                {
                    return null;
                }
                assemblyIncluded = true;
                return (strArray[0] + ", " + strArray[1]);
            }
            if (uri.StartsWith(namespaceNS, StringComparison.Ordinal))
            {
                string simpleName = ((RuntimeAssembly) typeof(string).Module.Assembly).GetSimpleName();
                assemblyIncluded = true;
                return (uri.Substring(namespaceNS.Length) + ", " + simpleName);
            }
            return null;
        }

        [SecurityCritical]
        internal static string GetXmlNamespaceForType(RuntimeType type, string dynamicUrl)
        {
            string fullName = type.FullName;
            RuntimeAssembly runtimeAssembly = type.GetRuntimeAssembly();
            StringBuilder builder = new StringBuilder(0x100);
            Assembly assembly = typeof(string).Module.Assembly;
            if (runtimeAssembly == assembly)
            {
                builder.Append(SoapServices.namespaceNS);
                builder.Append(fullName);
            }
            else
            {
                builder.Append(SoapServices.fullNS);
                builder.Append(fullName);
                builder.Append('/');
                builder.Append(runtimeAssembly.GetSimpleName());
            }
            return builder.ToString();
        }

        [SecurityCritical]
        internal static string GetXmlNamespaceForTypeNamespace(RuntimeType type, string dynamicUrl)
        {
            string str = type.Namespace;
            RuntimeAssembly runtimeAssembly = type.GetRuntimeAssembly();
            StringBuilder builder = new StringBuilder(0x100);
            Assembly assembly = typeof(string).Module.Assembly;
            if (runtimeAssembly == assembly)
            {
                builder.Append(SoapServices.namespaceNS);
                builder.Append(str);
            }
            else
            {
                builder.Append(SoapServices.fullNS);
                builder.Append(str);
                builder.Append('/');
                builder.Append(runtimeAssembly.GetSimpleName());
            }
            return builder.ToString();
        }
    }
}

