namespace System.Runtime.Remoting
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Metadata;
    using System.Security;
    using System.Text;

    [SecurityCritical, ComVisible(true)]
    public class SoapServices
    {
        private static Hashtable _interopTypeToXmlElement = Hashtable.Synchronized(new Hashtable());
        private static Hashtable _interopTypeToXmlType = Hashtable.Synchronized(new Hashtable());
        private static Hashtable _interopXmlElementToType = Hashtable.Synchronized(new Hashtable());
        private static Hashtable _interopXmlTypeToType = Hashtable.Synchronized(new Hashtable());
        private static Hashtable _methodBaseToSoapAction = Hashtable.Synchronized(new Hashtable());
        private static Hashtable _soapActionToMethodBase = Hashtable.Synchronized(new Hashtable());
        private static Hashtable _xmlToFieldTypeMap = Hashtable.Synchronized(new Hashtable());
        internal static string assemblyNS = "http://schemas.microsoft.com/clr/assem/";
        internal static string fullNS = "http://schemas.microsoft.com/clr/nsassem/";
        internal static string namespaceNS = "http://schemas.microsoft.com/clr/ns/";
        internal static string startNS = "http://schemas.microsoft.com/clr/";

        private SoapServices()
        {
        }

        [SecurityCritical]
        public static string CodeXmlNamespaceForClrTypeNamespace(string typeNamespace, string assemblyName)
        {
            StringBuilder sb = new StringBuilder(0x100);
            if (IsNameNull(typeNamespace))
            {
                if (IsNameNull(assemblyName))
                {
                    throw new ArgumentNullException("typeNamespace,assemblyName");
                }
                sb.Append(assemblyNS);
                UriEncode(assemblyName, sb);
            }
            else if (IsNameNull(assemblyName))
            {
                sb.Append(namespaceNS);
                sb.Append(typeNamespace);
            }
            else
            {
                sb.Append(fullNS);
                if (typeNamespace[0] == '.')
                {
                    sb.Append(typeNamespace.Substring(1));
                }
                else
                {
                    sb.Append(typeNamespace);
                }
                sb.Append('/');
                UriEncode(assemblyName, sb);
            }
            return sb.ToString();
        }

        private static string CreateKey(string elementName, string elementNamespace)
        {
            if (elementNamespace == null)
            {
                return elementName;
            }
            return (elementName + " " + elementNamespace);
        }

        [SecurityCritical]
        public static bool DecodeXmlNamespaceForClrTypeNamespace(string inNamespace, out string typeNamespace, out string assemblyName)
        {
            if (IsNameNull(inNamespace))
            {
                throw new ArgumentNullException("inNamespace");
            }
            assemblyName = null;
            typeNamespace = "";
            if (inNamespace.StartsWith(assemblyNS, StringComparison.Ordinal))
            {
                assemblyName = UriDecode(inNamespace.Substring(assemblyNS.Length));
            }
            else if (inNamespace.StartsWith(namespaceNS, StringComparison.Ordinal))
            {
                typeNamespace = inNamespace.Substring(namespaceNS.Length);
            }
            else if (inNamespace.StartsWith(fullNS, StringComparison.Ordinal))
            {
                int index = inNamespace.IndexOf("/", fullNS.Length);
                typeNamespace = inNamespace.Substring(fullNS.Length, index - fullNS.Length);
                assemblyName = UriDecode(inNamespace.Substring(index + 1));
            }
            else
            {
                return false;
            }
            return true;
        }

        public static void GetInteropFieldTypeAndNameFromXmlAttribute(Type containingType, string xmlAttribute, string xmlNamespace, out Type type, out string name)
        {
            if (containingType == null)
            {
                type = null;
                name = null;
            }
            else
            {
                XmlToFieldTypeMap map = (XmlToFieldTypeMap) _xmlToFieldTypeMap[containingType];
                if (map != null)
                {
                    map.GetFieldTypeAndNameFromXmlAttribute(xmlAttribute, xmlNamespace, out type, out name);
                }
                else
                {
                    type = null;
                    name = null;
                }
            }
        }

        public static void GetInteropFieldTypeAndNameFromXmlElement(Type containingType, string xmlElement, string xmlNamespace, out Type type, out string name)
        {
            if (containingType == null)
            {
                type = null;
                name = null;
            }
            else
            {
                XmlToFieldTypeMap map = (XmlToFieldTypeMap) _xmlToFieldTypeMap[containingType];
                if (map != null)
                {
                    map.GetFieldTypeAndNameFromXmlElement(xmlElement, xmlNamespace, out type, out name);
                }
                else
                {
                    type = null;
                    name = null;
                }
            }
        }

        [SecurityCritical]
        public static Type GetInteropTypeFromXmlElement(string xmlElement, string xmlNamespace)
        {
            return (Type) _interopXmlElementToType[CreateKey(xmlElement, xmlNamespace)];
        }

        [SecurityCritical]
        public static Type GetInteropTypeFromXmlType(string xmlType, string xmlTypeNamespace)
        {
            return (Type) _interopXmlTypeToType[CreateKey(xmlType, xmlTypeNamespace)];
        }

        [SecurityCritical]
        public static string GetSoapActionFromMethodBase(MethodBase mb)
        {
            string soapAction = (string) _methodBaseToSoapAction[mb];
            if (soapAction == null)
            {
                SoapMethodAttribute cachedSoapAttribute = (SoapMethodAttribute) InternalRemotingServices.GetCachedSoapAttribute(mb);
                soapAction = cachedSoapAttribute.SoapAction;
            }
            return soapAction;
        }

        public static bool GetTypeAndMethodNameFromSoapAction(string soapAction, out string typeName, out string methodName)
        {
            if ((soapAction[0] == '"') && (soapAction[soapAction.Length - 1] == '"'))
            {
                soapAction = soapAction.Substring(1, soapAction.Length - 2);
            }
            ArrayList list = (ArrayList) _soapActionToMethodBase[soapAction];
            if (list != null)
            {
                if (list.Count > 1)
                {
                    typeName = null;
                    methodName = null;
                    return false;
                }
                MethodBase base2 = (MethodBase) list[0];
                if (base2 != null)
                {
                    RuntimeModule runtimeModule;
                    RuntimeMethodInfo info = base2 as RuntimeMethodInfo;
                    RuntimeConstructorInfo info2 = base2 as RuntimeConstructorInfo;
                    if (info != null)
                    {
                        runtimeModule = info.GetRuntimeModule();
                    }
                    else
                    {
                        if (info2 == null)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeReflectionObject"));
                        }
                        runtimeModule = info2.GetRuntimeModule();
                    }
                    typeName = base2.DeclaringType.FullName + ", " + runtimeModule.GetRuntimeAssembly().GetSimpleName();
                    methodName = base2.Name;
                    return true;
                }
            }
            string[] strArray = soapAction.Split(new char[] { '#' });
            if (strArray.Length == 2)
            {
                bool flag;
                typeName = XmlNamespaceEncoder.GetTypeNameForSoapActionNamespace(strArray[0], out flag);
                if (typeName == null)
                {
                    methodName = null;
                    return false;
                }
                methodName = strArray[1];
                return true;
            }
            typeName = null;
            methodName = null;
            return false;
        }

        [SecurityCritical]
        public static bool GetXmlElementForInteropType(Type type, out string xmlElement, out string xmlNamespace)
        {
            XmlEntry entry = (XmlEntry) _interopTypeToXmlElement[type];
            if (entry != null)
            {
                xmlElement = entry.Name;
                xmlNamespace = entry.Namespace;
                return true;
            }
            SoapTypeAttribute cachedSoapAttribute = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute(type);
            if (cachedSoapAttribute.IsInteropXmlElement())
            {
                xmlElement = cachedSoapAttribute.XmlElementName;
                xmlNamespace = cachedSoapAttribute.XmlNamespace;
                return true;
            }
            xmlElement = null;
            xmlNamespace = null;
            return false;
        }

        [SecurityCritical]
        public static string GetXmlNamespaceForMethodCall(MethodBase mb)
        {
            SoapMethodAttribute cachedSoapAttribute = (SoapMethodAttribute) InternalRemotingServices.GetCachedSoapAttribute(mb);
            return cachedSoapAttribute.XmlNamespace;
        }

        [SecurityCritical]
        public static string GetXmlNamespaceForMethodResponse(MethodBase mb)
        {
            SoapMethodAttribute cachedSoapAttribute = (SoapMethodAttribute) InternalRemotingServices.GetCachedSoapAttribute(mb);
            return cachedSoapAttribute.ResponseXmlNamespace;
        }

        [SecurityCritical]
        public static bool GetXmlTypeForInteropType(Type type, out string xmlType, out string xmlTypeNamespace)
        {
            XmlEntry entry = (XmlEntry) _interopTypeToXmlType[type];
            if (entry != null)
            {
                xmlType = entry.Name;
                xmlTypeNamespace = entry.Namespace;
                return true;
            }
            SoapTypeAttribute cachedSoapAttribute = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute(type);
            if (cachedSoapAttribute.IsInteropXmlType())
            {
                xmlType = cachedSoapAttribute.XmlTypeName;
                xmlTypeNamespace = cachedSoapAttribute.XmlTypeNamespace;
                return true;
            }
            xmlType = null;
            xmlTypeNamespace = null;
            return false;
        }

        public static bool IsClrTypeNamespace(string namespaceString)
        {
            return namespaceString.StartsWith(startNS, StringComparison.Ordinal);
        }

        private static bool IsNameNull(string name)
        {
            if ((name != null) && (name.Length != 0))
            {
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool IsSoapActionValidForMethodBase(string soapAction, MethodBase mb)
        {
            bool flag;
            RuntimeModule runtimeModule;
            if (mb == null)
            {
                throw new ArgumentNullException("mb");
            }
            if ((soapAction[0] == '"') && (soapAction[soapAction.Length - 1] == '"'))
            {
                soapAction = soapAction.Substring(1, soapAction.Length - 2);
            }
            SoapMethodAttribute cachedSoapAttribute = (SoapMethodAttribute) InternalRemotingServices.GetCachedSoapAttribute(mb);
            if (string.CompareOrdinal(cachedSoapAttribute.SoapAction, soapAction) == 0)
            {
                return true;
            }
            string strA = (string) _methodBaseToSoapAction[mb];
            if ((strA != null) && (string.CompareOrdinal(strA, soapAction) == 0))
            {
                return true;
            }
            string[] strArray = soapAction.Split(new char[] { '#' });
            if (strArray.Length != 2)
            {
                return false;
            }
            string typeNameForSoapActionNamespace = XmlNamespaceEncoder.GetTypeNameForSoapActionNamespace(strArray[0], out flag);
            if (typeNameForSoapActionNamespace == null)
            {
                return false;
            }
            string str3 = strArray[1];
            RuntimeMethodInfo info = mb as RuntimeMethodInfo;
            RuntimeConstructorInfo info2 = mb as RuntimeConstructorInfo;
            if (info != null)
            {
                runtimeModule = info.GetRuntimeModule();
            }
            else
            {
                if (info2 == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeReflectionObject"));
                }
                runtimeModule = info2.GetRuntimeModule();
            }
            string fullName = mb.DeclaringType.FullName;
            if (flag)
            {
                fullName = fullName + ", " + runtimeModule.GetRuntimeAssembly().GetSimpleName();
            }
            return (fullName.Equals(typeNameForSoapActionNamespace) && mb.Name.Equals(str3));
        }

        [SecurityCritical]
        public static void PreLoad(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (!(assembly is RuntimeAssembly))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"), "assembly");
            }
            foreach (Type type in assembly.GetTypes())
            {
                PreLoad(type);
            }
        }

        [SecurityCritical]
        public static void PreLoad(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!(type is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            foreach (MethodInfo info in type.GetMethods())
            {
                RegisterSoapActionForMethodBase(info);
            }
            SoapTypeAttribute cachedSoapAttribute = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute(type);
            if (cachedSoapAttribute.IsInteropXmlElement())
            {
                RegisterInteropXmlElement(cachedSoapAttribute.XmlElementName, cachedSoapAttribute.XmlNamespace, type);
            }
            if (cachedSoapAttribute.IsInteropXmlType())
            {
                RegisterInteropXmlType(cachedSoapAttribute.XmlTypeName, cachedSoapAttribute.XmlTypeNamespace, type);
            }
            int num = 0;
            XmlToFieldTypeMap map = new XmlToFieldTypeMap();
            foreach (FieldInfo info2 in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                SoapFieldAttribute attribute2 = (SoapFieldAttribute) InternalRemotingServices.GetCachedSoapAttribute(info2);
                if (attribute2.IsInteropXmlElement())
                {
                    string xmlElementName = attribute2.XmlElementName;
                    string xmlNamespace = attribute2.XmlNamespace;
                    if (attribute2.UseAttribute)
                    {
                        map.AddXmlAttribute(info2.FieldType, info2.Name, xmlElementName, xmlNamespace);
                    }
                    else
                    {
                        map.AddXmlElement(info2.FieldType, info2.Name, xmlElementName, xmlNamespace);
                    }
                    num++;
                }
            }
            if (num > 0)
            {
                _xmlToFieldTypeMap[type] = map;
            }
        }

        [SecurityCritical]
        public static void RegisterInteropXmlElement(string xmlElement, string xmlNamespace, Type type)
        {
            _interopXmlElementToType[CreateKey(xmlElement, xmlNamespace)] = type;
            _interopTypeToXmlElement[type] = new XmlEntry(xmlElement, xmlNamespace);
        }

        [SecurityCritical]
        public static void RegisterInteropXmlType(string xmlType, string xmlTypeNamespace, Type type)
        {
            _interopXmlTypeToType[CreateKey(xmlType, xmlTypeNamespace)] = type;
            _interopTypeToXmlType[type] = new XmlEntry(xmlType, xmlTypeNamespace);
        }

        [SecurityCritical]
        public static void RegisterSoapActionForMethodBase(MethodBase mb)
        {
            SoapMethodAttribute cachedSoapAttribute = (SoapMethodAttribute) InternalRemotingServices.GetCachedSoapAttribute(mb);
            if (cachedSoapAttribute.SoapActionExplicitySet)
            {
                RegisterSoapActionForMethodBase(mb, cachedSoapAttribute.SoapAction);
            }
        }

        public static void RegisterSoapActionForMethodBase(MethodBase mb, string soapAction)
        {
            if (soapAction != null)
            {
                _methodBaseToSoapAction[mb] = soapAction;
                ArrayList list = (ArrayList) _soapActionToMethodBase[soapAction];
                if (list == null)
                {
                    lock (_soapActionToMethodBase)
                    {
                        list = ArrayList.Synchronized(new ArrayList());
                        _soapActionToMethodBase[soapAction] = list;
                    }
                }
                list.Add(mb);
            }
        }

        internal static string UriDecode(string value)
        {
            if ((value == null) || (value.Length == 0))
            {
                return value;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if ((value[i] == '%') && ((value.Length - i) >= 3))
                {
                    if ((value[i + 1] == '2') && (value[i + 2] == '0'))
                    {
                        builder.Append(' ');
                        i += 2;
                    }
                    else if ((value[i + 1] == '3') && (value[i + 2] == 'D'))
                    {
                        builder.Append('=');
                        i += 2;
                    }
                    else if ((value[i + 1] == '2') && (value[i + 2] == 'C'))
                    {
                        builder.Append(',');
                        i += 2;
                    }
                    else
                    {
                        builder.Append(value[i]);
                    }
                }
                else
                {
                    builder.Append(value[i]);
                }
            }
            return builder.ToString();
        }

        internal static void UriEncode(string value, StringBuilder sb)
        {
            if ((value != null) && (value.Length != 0))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == ' ')
                    {
                        sb.Append("%20");
                    }
                    else if (value[i] == '=')
                    {
                        sb.Append("%3D");
                    }
                    else if (value[i] == ',')
                    {
                        sb.Append("%2C");
                    }
                    else
                    {
                        sb.Append(value[i]);
                    }
                }
            }
        }

        public static string XmlNsForClrType
        {
            get
            {
                return startNS;
            }
        }

        public static string XmlNsForClrTypeWithAssembly
        {
            get
            {
                return assemblyNS;
            }
        }

        public static string XmlNsForClrTypeWithNs
        {
            get
            {
                return namespaceNS;
            }
        }

        public static string XmlNsForClrTypeWithNsAndAssembly
        {
            get
            {
                return fullNS;
            }
        }

        private class XmlEntry
        {
            public string Name;
            public string Namespace;

            public XmlEntry(string name, string xmlNamespace)
            {
                this.Name = name;
                this.Namespace = xmlNamespace;
            }
        }

        private class XmlToFieldTypeMap
        {
            private Hashtable _attributes = new Hashtable();
            private Hashtable _elements = new Hashtable();

            [SecurityCritical]
            public void AddXmlAttribute(Type fieldType, string fieldName, string xmlAttribute, string xmlNamespace)
            {
                this._attributes[SoapServices.CreateKey(xmlAttribute, xmlNamespace)] = new FieldEntry(fieldType, fieldName);
            }

            [SecurityCritical]
            public void AddXmlElement(Type fieldType, string fieldName, string xmlElement, string xmlNamespace)
            {
                this._elements[SoapServices.CreateKey(xmlElement, xmlNamespace)] = new FieldEntry(fieldType, fieldName);
            }

            [SecurityCritical]
            public void GetFieldTypeAndNameFromXmlAttribute(string xmlAttribute, string xmlNamespace, out Type type, out string name)
            {
                FieldEntry entry = (FieldEntry) this._attributes[SoapServices.CreateKey(xmlAttribute, xmlNamespace)];
                if (entry != null)
                {
                    type = entry.Type;
                    name = entry.Name;
                }
                else
                {
                    type = null;
                    name = null;
                }
            }

            [SecurityCritical]
            public void GetFieldTypeAndNameFromXmlElement(string xmlElement, string xmlNamespace, out Type type, out string name)
            {
                FieldEntry entry = (FieldEntry) this._elements[SoapServices.CreateKey(xmlElement, xmlNamespace)];
                if (entry != null)
                {
                    type = entry.Type;
                    name = entry.Name;
                }
                else
                {
                    type = null;
                    name = null;
                }
            }

            private class FieldEntry
            {
                public string Name;
                public System.Type Type;

                public FieldEntry(System.Type type, string name)
                {
                    this.Type = type;
                    this.Name = name;
                }
            }
        }
    }
}

