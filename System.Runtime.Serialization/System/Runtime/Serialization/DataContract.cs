namespace System.Runtime.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Configuration;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Schema;

    internal abstract class DataContract
    {
        [SecurityCritical]
        private static DataContractSerializerSection configSection;
        [SecurityCritical]
        private DataContractCriticalHelper helper;
        [SecurityCritical]
        private XmlDictionaryString name;
        [SecurityCritical]
        private XmlDictionaryString ns;

        [SecuritySafeCritical]
        protected DataContract(DataContractCriticalHelper helper)
        {
            this.helper = helper;
            this.name = helper.Name;
            this.ns = helper.Namespace;
        }

        internal virtual DataContract BindGenericParameters(DataContract[] paramContracts, Dictionary<DataContract, DataContract> boundContracts)
        {
            return this;
        }

        [SecurityCritical, SecurityTreatAsSafe]
        internal static void CheckAndAdd(Type type, Dictionary<Type, Type> typesChecked, ref Dictionary<XmlQualifiedName, DataContract> nameToDataContractTable)
        {
            type = UnwrapNullableType(type);
            DataContract dataContract = GetDataContract(type);
            if (nameToDataContractTable == null)
            {
                nameToDataContractTable = new Dictionary<XmlQualifiedName, DataContract>();
            }
            else
            {
                DataContract contract2;
                if (nameToDataContractTable.TryGetValue(dataContract.StableName, out contract2))
                {
                    if (contract2.UnderlyingType != DataContractCriticalHelper.GetDataContractAdapterType(type))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("DupContractInKnownTypes", new object[] { type, contract2.UnderlyingType, dataContract.StableName.Namespace, dataContract.StableName.Name })));
                    }
                    return;
                }
            }
            nameToDataContractTable.Add(dataContract.StableName, dataContract);
            ImportKnownTypeAttributes(type, typesChecked, ref nameToDataContractTable);
        }

        private static void CheckExplicitDataContractNamespaceUri(string dataContractNs, Type type)
        {
            Uri uri;
            if (dataContractNs.Length > 0)
            {
                string str = dataContractNs.Trim();
                if ((str.Length == 0) || (str.IndexOf("##", StringComparison.Ordinal) != -1))
                {
                    ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("DataContractNamespaceIsNotValid", new object[] { dataContractNs }), type);
                }
                dataContractNs = str;
            }
            if (Uri.TryCreate(dataContractNs, UriKind.RelativeOrAbsolute, out uri))
            {
                if (uri.ToString() == "http://schemas.microsoft.com/2003/10/Serialization/")
                {
                    ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("DataContractNamespaceReserved", new object[] { "http://schemas.microsoft.com/2003/10/Serialization/" }), type);
                }
            }
            else
            {
                ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("DataContractNamespaceIsNotValid", new object[] { dataContractNs }), type);
            }
        }

        private static void CheckRootTypeInConfigIsGeneric(Type type, ref Type rootType, ref Type[] genArgs)
        {
            if (rootType.IsGenericType)
            {
                if (!rootType.ContainsGenericParameters)
                {
                    genArgs = rootType.GetGenericArguments();
                    rootType = rootType.GetGenericTypeDefinition();
                }
                else
                {
                    ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("TypeMustBeConcrete", new object[] { type }), type);
                }
            }
        }

        internal static bool ConstructorRequiresMemberAccess(ConstructorInfo ctor)
        {
            return (((ctor != null) && !ctor.IsPublic) && !IsMemberVisibleInSerializationModule(ctor));
        }

        internal static XmlQualifiedName CreateQualifiedName(string localName, string ns)
        {
            return new XmlQualifiedName(localName, GetNamespace(ns));
        }

        internal static string EncodeLocalName(string localName)
        {
            if (IsAsciiLocalName(localName))
            {
                return localName;
            }
            if (IsValidNCName(localName))
            {
                return localName;
            }
            return XmlConvert.EncodeLocalName(localName);
        }

        public sealed override bool Equals(object other)
        {
            return ((this == other) || this.Equals(other, new Dictionary<DataContractPairKey, object>()));
        }

        internal virtual bool Equals(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            DataContract contract = other as DataContract;
            if (contract == null)
            {
                return false;
            }
            return (((this.StableName.Name == contract.StableName.Name) && (this.StableName.Namespace == contract.StableName.Namespace)) && (this.IsReference == contract.IsReference));
        }

        internal static string ExpandGenericParameters(string format, IGenericNameProvider genericNameProvider)
        {
            string namespacesDigest = null;
            StringBuilder builder = new StringBuilder();
            IList<int> nestedParameterCounts = genericNameProvider.GetNestedParameterCounts();
            for (int i = 0; i < format.Length; i++)
            {
                char ch = format[i];
                if (ch == '{')
                {
                    i++;
                    int startIndex = i;
                    while (i < format.Length)
                    {
                        if (format[i] == '}')
                        {
                            break;
                        }
                        i++;
                    }
                    if (i == format.Length)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GenericNameBraceMismatch", new object[] { format, genericNameProvider.GetGenericTypeName() })));
                    }
                    if ((format[startIndex] == '#') && (i == (startIndex + 1)))
                    {
                        if ((nestedParameterCounts.Count > 1) || !genericNameProvider.ParametersFromBuiltInNamespaces)
                        {
                            if (namespacesDigest == null)
                            {
                                StringBuilder builder2 = new StringBuilder(genericNameProvider.GetNamespaces());
                                foreach (int num3 in nestedParameterCounts)
                                {
                                    builder2.Insert(0, num3).Insert(0, " ");
                                }
                                namespacesDigest = GetNamespacesDigest(builder2.ToString());
                            }
                            builder.Append(namespacesDigest);
                        }
                    }
                    else
                    {
                        int num4;
                        if ((!int.TryParse(format.Substring(startIndex, i - startIndex), out num4) || (num4 < 0)) || (num4 >= genericNameProvider.GetParameterCount()))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GenericParameterNotValid", new object[] { format.Substring(startIndex, i - startIndex), genericNameProvider.GetGenericTypeName(), genericNameProvider.GetParameterCount() - 1 })));
                        }
                        builder.Append(genericNameProvider.GetParameterName(num4));
                    }
                    continue;
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        private static string ExpandGenericParameters(string format, Type type)
        {
            GenericNameProvider genericNameProvider = new GenericNameProvider(type);
            return ExpandGenericParameters(format, genericNameProvider);
        }

        internal static bool FieldRequiresMemberAccess(FieldInfo field)
        {
            return (((field != null) && !field.IsPublic) && !IsMemberVisibleInSerializationModule(field));
        }

        private static string GetArrayPrefix(ref Type itemType)
        {
            string str = string.Empty;
            while (itemType.IsArray)
            {
                if (GetBuiltInDataContract(itemType) != null)
                {
                    return str;
                }
                str = str + "ArrayOf";
                itemType = itemType.GetElementType();
            }
            return str;
        }

        internal XmlQualifiedName GetArrayTypeName(bool isNullable)
        {
            XmlQualifiedName expandedStableName;
            if (this.IsValueType && isNullable)
            {
                System.Runtime.Serialization.GenericInfo info = new System.Runtime.Serialization.GenericInfo(GetStableName(Globals.TypeOfNullable), Globals.TypeOfNullable.FullName);
                info.Add(new System.Runtime.Serialization.GenericInfo(this.StableName, null));
                info.AddToLevel(0, 1);
                expandedStableName = info.GetExpandedStableName();
            }
            else
            {
                expandedStableName = this.StableName;
            }
            return new XmlQualifiedName("ArrayOf" + expandedStableName.Name, GetCollectionNamespace(expandedStableName.Namespace));
        }

        [SecuritySafeCritical]
        public static DataContract GetBuiltInDataContract(string typeName)
        {
            return DataContractCriticalHelper.GetBuiltInDataContract(typeName);
        }

        [SecuritySafeCritical]
        public static DataContract GetBuiltInDataContract(Type type)
        {
            return DataContractCriticalHelper.GetBuiltInDataContract(type);
        }

        [SecuritySafeCritical]
        public static DataContract GetBuiltInDataContract(string name, string ns)
        {
            return DataContractCriticalHelper.GetBuiltInDataContract(name, ns);
        }

        internal static string GetClrAssemblyName(Type type)
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(TypeForwardedFromAttribute), false);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                TypeForwardedFromAttribute attribute = (TypeForwardedFromAttribute) customAttributes[0];
                return attribute.AssemblyFullName;
            }
            return type.Assembly.FullName;
        }

        internal static void GetClrNameAndNamespace(string fullTypeName, out string localName, out string ns)
        {
            int length = fullTypeName.LastIndexOf('.');
            if (length < 0)
            {
                ns = string.Empty;
                localName = fullTypeName.Replace('+', '.');
            }
            else
            {
                ns = fullTypeName.Substring(0, length);
                localName = fullTypeName.Substring(length + 1).Replace('+', '.');
            }
            int index = localName.IndexOf('[');
            if (index >= 0)
            {
                localName = localName.Substring(0, index);
            }
        }

        internal static string GetClrTypeFullName(Type type)
        {
            if (type.IsGenericTypeDefinition || !type.ContainsGenericParameters)
            {
                return type.FullName;
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { type.Namespace, type.Name });
        }

        private static string GetClrTypeFullNameForArray(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { GetClrTypeFullNameUsingTypeForwardedFromAttribute(type.GetElementType()), "[", "]" });
        }

        private static string GetClrTypeFullNameForNonArrayTypes(Type type)
        {
            if (!type.IsGenericType)
            {
                return GetClrTypeFullName(type);
            }
            Type[] genericArguments = type.GetGenericArguments();
            StringBuilder builder = new StringBuilder(type.GetGenericTypeDefinition().FullName).Append("[");
            foreach (Type type2 in genericArguments)
            {
                builder.Append("[").Append(GetClrTypeFullNameUsingTypeForwardedFromAttribute(type2)).Append(",");
                builder.Append(" ").Append(GetClrAssemblyName(type2));
                builder.Append("]").Append(",");
            }
            return builder.Remove(builder.Length - 1, 1).Append("]").ToString();
        }

        internal static string GetClrTypeFullNameUsingTypeForwardedFromAttribute(Type type)
        {
            if (type.IsArray)
            {
                return GetClrTypeFullNameForArray(type);
            }
            return GetClrTypeFullNameForNonArrayTypes(type);
        }

        [SecuritySafeCritical]
        internal static XmlDictionaryString GetClrTypeString(string key)
        {
            return DataContractCriticalHelper.GetClrTypeString(key);
        }

        internal static string GetCollectionNamespace(string elementNs)
        {
            if (!IsBuiltInNamespace(elementNs))
            {
                return elementNs;
            }
            return "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
        }

        internal static XmlQualifiedName GetCollectionStableName(Type type, Type itemType, out CollectionDataContractAttribute collectionContractAttribute)
        {
            return GetCollectionStableName(type, itemType, new HashSet<Type>(), out collectionContractAttribute);
        }

        private static XmlQualifiedName GetCollectionStableName(Type type, Type itemType, HashSet<Type> previousCollectionTypes, out CollectionDataContractAttribute collectionContractAttribute)
        {
            string defaultStableLocalName;
            string defaultDataContractNamespace;
            object[] customAttributes = type.GetCustomAttributes(Globals.TypeOfCollectionDataContractAttribute, false);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                collectionContractAttribute = (CollectionDataContractAttribute) customAttributes[0];
                if (collectionContractAttribute.IsNameSetExplicit)
                {
                    defaultStableLocalName = collectionContractAttribute.Name;
                    if ((defaultStableLocalName == null) || (defaultStableLocalName.Length == 0))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidCollectionContractName", new object[] { GetClrTypeFullName(type) })));
                    }
                    if (type.IsGenericType && !type.IsGenericTypeDefinition)
                    {
                        defaultStableLocalName = ExpandGenericParameters(defaultStableLocalName, type);
                    }
                    defaultStableLocalName = EncodeLocalName(defaultStableLocalName);
                }
                else
                {
                    defaultStableLocalName = GetDefaultStableLocalName(type);
                }
                if (collectionContractAttribute.IsNamespaceSetExplicit)
                {
                    defaultDataContractNamespace = collectionContractAttribute.Namespace;
                    if (defaultDataContractNamespace == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidCollectionContractNamespace", new object[] { GetClrTypeFullName(type) })));
                    }
                    CheckExplicitDataContractNamespaceUri(defaultDataContractNamespace, type);
                }
                else
                {
                    defaultDataContractNamespace = GetDefaultDataContractNamespace(type);
                }
            }
            else
            {
                bool flag;
                collectionContractAttribute = null;
                string str3 = "ArrayOf" + GetArrayPrefix(ref itemType);
                XmlQualifiedName name = GetStableName(itemType, previousCollectionTypes, out flag);
                defaultStableLocalName = str3 + name.Name;
                defaultDataContractNamespace = GetCollectionNamespace(name.Namespace);
            }
            return CreateQualifiedName(defaultStableLocalName, defaultDataContractNamespace);
        }

        internal static DataContract GetDataContract(Type type)
        {
            return GetDataContract(type.TypeHandle, type, SerializationMode.SharedContract);
        }

        internal static DataContract GetDataContract(int id, RuntimeTypeHandle typeHandle, SerializationMode mode)
        {
            return GetDataContractSkipValidation(id, typeHandle, null).GetValidContract(mode);
        }

        internal static DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type, SerializationMode mode)
        {
            return GetDataContract(GetId(typeHandle), typeHandle, mode);
        }

        [SecuritySafeCritical]
        internal static DataContract GetDataContractForInitialization(int id)
        {
            return DataContractCriticalHelper.GetDataContractForInitialization(id);
        }

        internal static IList<int> GetDataContractNameForGenericName(string typeName, StringBuilder localName)
        {
            int num2;
            List<int> list = new List<int>();
            int startIndex = 0;
        Label_0008:
            num2 = typeName.IndexOf('`', startIndex);
            if (num2 < 0)
            {
                if (localName != null)
                {
                    localName.Append(typeName.Substring(startIndex));
                }
                list.Add(0);
            }
            else
            {
                if (localName != null)
                {
                    localName.Append(typeName.Substring(startIndex, num2 - startIndex));
                }
                while ((startIndex = typeName.IndexOf('.', startIndex + 1, (num2 - startIndex) - 1)) >= 0)
                {
                    list.Add(0);
                }
                startIndex = typeName.IndexOf('.', num2);
                if (startIndex < 0)
                {
                    list.Add(int.Parse(typeName.Substring(num2 + 1), CultureInfo.InvariantCulture));
                }
                else
                {
                    list.Add(int.Parse(typeName.Substring(num2 + 1, (startIndex - num2) - 1), CultureInfo.InvariantCulture));
                    goto Label_0008;
                }
            }
            if (localName != null)
            {
                localName.Append("Of");
            }
            return list;
        }

        internal static string GetDataContractNamespaceFromUri(string uriString)
        {
            if (!uriString.StartsWith("http://schemas.datacontract.org/2004/07/", StringComparison.Ordinal))
            {
                return uriString;
            }
            return uriString.Substring("http://schemas.datacontract.org/2004/07/".Length);
        }

        [SecuritySafeCritical]
        internal static DataContract GetDataContractSkipValidation(int id, RuntimeTypeHandle typeHandle, Type type)
        {
            return DataContractCriticalHelper.GetDataContractSkipValidation(id, typeHandle, type);
        }

        private static XmlQualifiedName GetDCTypeStableName(Type type, DataContractAttribute dataContractAttribute)
        {
            string format = null;
            string dataContractNs = null;
            if (dataContractAttribute.IsNameSetExplicit)
            {
                format = dataContractAttribute.Name;
                if ((format == null) || (format.Length == 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidDataContractName", new object[] { GetClrTypeFullName(type) })));
                }
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                {
                    format = ExpandGenericParameters(format, type);
                }
                format = EncodeLocalName(format);
            }
            else
            {
                format = GetDefaultStableLocalName(type);
            }
            if (dataContractAttribute.IsNamespaceSetExplicit)
            {
                dataContractNs = dataContractAttribute.Namespace;
                if (dataContractNs == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidDataContractNamespace", new object[] { GetClrTypeFullName(type) })));
                }
                CheckExplicitDataContractNamespaceUri(dataContractNs, type);
            }
            else
            {
                dataContractNs = GetDefaultDataContractNamespace(type);
            }
            return CreateQualifiedName(format, dataContractNs);
        }

        private static string GetDefaultDataContractNamespace(Type type)
        {
            string clrNs = type.Namespace;
            if (clrNs == null)
            {
                clrNs = string.Empty;
            }
            string globalDataContractNamespace = GetGlobalDataContractNamespace(clrNs, type.Module);
            if (globalDataContractNamespace == null)
            {
                globalDataContractNamespace = GetGlobalDataContractNamespace(clrNs, type.Assembly);
            }
            if (globalDataContractNamespace == null)
            {
                return GetDefaultStableNamespace(type);
            }
            CheckExplicitDataContractNamespaceUri(globalDataContractNamespace, type);
            return globalDataContractNamespace;
        }

        private static string GetDefaultStableLocalName(Type type)
        {
            string str;
            if (type.IsGenericParameter)
            {
                return ("{" + type.GenericParameterPosition + "}");
            }
            string arrayPrefix = null;
            if (type.IsArray)
            {
                arrayPrefix = GetArrayPrefix(ref type);
            }
            if (type.DeclaringType == null)
            {
                str = type.Name;
            }
            else
            {
                int startIndex = (type.Namespace == null) ? 0 : type.Namespace.Length;
                if (startIndex > 0)
                {
                    startIndex++;
                }
                str = GetClrTypeFullName(type).Substring(startIndex).Replace('+', '.');
            }
            if (arrayPrefix != null)
            {
                str = arrayPrefix + str;
            }
            if (type.IsGenericType)
            {
                StringBuilder localName = new StringBuilder();
                StringBuilder builder2 = new StringBuilder();
                bool flag = true;
                int index = str.IndexOf('[');
                if (index >= 0)
                {
                    str = str.Substring(0, index);
                }
                IList<int> dataContractNameForGenericName = GetDataContractNameForGenericName(str, localName);
                bool isGenericTypeDefinition = type.IsGenericTypeDefinition;
                Type[] genericArguments = type.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    Type type2 = genericArguments[i];
                    if (isGenericTypeDefinition)
                    {
                        localName.Append("{").Append(i).Append("}");
                    }
                    else
                    {
                        XmlQualifiedName stableName = GetStableName(type2);
                        localName.Append(stableName.Name);
                        builder2.Append(" ").Append(stableName.Namespace);
                        if (flag)
                        {
                            flag = IsBuiltInNamespace(stableName.Namespace);
                        }
                    }
                }
                if (isGenericTypeDefinition)
                {
                    localName.Append("{#}");
                }
                else if ((dataContractNameForGenericName.Count > 1) || !flag)
                {
                    foreach (int num4 in dataContractNameForGenericName)
                    {
                        builder2.Insert(0, num4).Insert(0, " ");
                    }
                    localName.Append(GetNamespacesDigest(builder2.ToString()));
                }
                str = localName.ToString();
            }
            return EncodeLocalName(str);
        }

        internal static XmlQualifiedName GetDefaultStableName(Type type)
        {
            return CreateQualifiedName(GetDefaultStableLocalName(type), GetDefaultStableNamespace(type));
        }

        private static void GetDefaultStableName(CodeTypeReference typeReference, out string localName, out string ns)
        {
            string baseType = typeReference.BaseType;
            DataContract builtInDataContract = GetBuiltInDataContract(baseType);
            if (builtInDataContract != null)
            {
                localName = builtInDataContract.StableName.Name;
                ns = builtInDataContract.StableName.Namespace;
            }
            else
            {
                GetClrNameAndNamespace(baseType, out localName, out ns);
                if (typeReference.TypeArguments.Count > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    StringBuilder builder2 = new StringBuilder();
                    bool flag = true;
                    IList<int> dataContractNameForGenericName = GetDataContractNameForGenericName(localName, builder);
                    foreach (CodeTypeReference reference in typeReference.TypeArguments)
                    {
                        string str2;
                        string str3;
                        GetDefaultStableName(reference, out str2, out str3);
                        builder.Append(str2);
                        builder2.Append(" ").Append(str3);
                        if (flag)
                        {
                            flag = IsBuiltInNamespace(str3);
                        }
                    }
                    if ((dataContractNameForGenericName.Count > 1) || !flag)
                    {
                        foreach (int num in dataContractNameForGenericName)
                        {
                            builder2.Insert(0, num).Insert(0, " ");
                        }
                        builder.Append(GetNamespacesDigest(builder2.ToString()));
                    }
                    localName = builder.ToString();
                }
                localName = EncodeLocalName(localName);
                ns = GetDefaultStableNamespace(ns);
            }
        }

        internal static void GetDefaultStableName(string fullTypeName, out string localName, out string ns)
        {
            CodeTypeReference typeReference = new CodeTypeReference(fullTypeName);
            GetDefaultStableName(typeReference, out localName, out ns);
        }

        internal static string GetDefaultStableNamespace(string clrNs)
        {
            if (clrNs == null)
            {
                clrNs = string.Empty;
            }
            return new Uri(Globals.DataContractXsdBaseNamespaceUri, clrNs).AbsoluteUri;
        }

        internal static string GetDefaultStableNamespace(Type type)
        {
            if (type.IsGenericParameter)
            {
                return "{ns}";
            }
            return GetDefaultStableNamespace(type.Namespace);
        }

        internal static DataContract GetGetOnlyCollectionDataContract(int id, RuntimeTypeHandle typeHandle, Type type, SerializationMode mode)
        {
            DataContract validContract = GetGetOnlyCollectionDataContractSkipValidation(id, typeHandle, type).GetValidContract(mode);
            if (validContract is ClassDataContract)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("ClassDataContractReturnedForGetOnlyCollection", new object[] { GetClrTypeFullName(validContract.UnderlyingType) })));
            }
            return validContract;
        }

        [SecuritySafeCritical]
        internal static DataContract GetGetOnlyCollectionDataContractSkipValidation(int id, RuntimeTypeHandle typeHandle, Type type)
        {
            return DataContractCriticalHelper.GetGetOnlyCollectionDataContractSkipValidation(id, typeHandle, type);
        }

        private static string GetGlobalDataContractNamespace(string clrNs, ICustomAttributeProvider customAttribuetProvider)
        {
            object[] customAttributes = customAttribuetProvider.GetCustomAttributes(typeof(ContractNamespaceAttribute), false);
            string contractNamespace = null;
            for (int i = 0; i < customAttributes.Length; i++)
            {
                ContractNamespaceAttribute attribute = (ContractNamespaceAttribute) customAttributes[i];
                string clrNamespace = attribute.ClrNamespace;
                if (clrNamespace == null)
                {
                    clrNamespace = string.Empty;
                }
                if (clrNamespace == clrNs)
                {
                    if (attribute.ContractNamespace == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidGlobalDataContractNamespace", new object[] { clrNs })));
                    }
                    if (contractNamespace != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("DataContractNamespaceAlreadySet", new object[] { contractNamespace, attribute.ContractNamespace, clrNs })));
                    }
                    contractNamespace = attribute.ContractNamespace;
                }
            }
            return contractNamespace;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SecuritySafeCritical]
        internal static int GetId(RuntimeTypeHandle typeHandle)
        {
            return DataContractCriticalHelper.GetId(typeHandle);
        }

        [SecuritySafeCritical]
        internal static int GetIdForInitialization(ClassDataContract classContract)
        {
            return DataContractCriticalHelper.GetIdForInitialization(classContract);
        }

        [SecuritySafeCritical]
        internal static string GetNamespace(string key)
        {
            return DataContractCriticalHelper.GetNamespace(key);
        }

        private static string GetNamespacesDigest(string namespaces)
        {
            byte[] inArray = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(namespaces));
            char[] outArray = new char[0x18];
            int num = Convert.ToBase64CharArray(inArray, 0, 6, outArray, 0);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < num; i++)
            {
                char ch = outArray[i];
                switch (ch)
                {
                    case '+':
                        builder.Append("_P");
                        break;

                    case '/':
                        builder.Append("_S");
                        break;

                    case '=':
                        break;

                    default:
                        builder.Append(ch);
                        break;
                }
            }
            return builder.ToString();
        }

        private static XmlQualifiedName GetNonDCTypeStableName(Type type, HashSet<Type> previousCollectionTypes)
        {
            string localName = null;
            string ns = null;
            Type type2;
            if (CollectionDataContract.IsCollection(type, out type2))
            {
                CollectionDataContractAttribute attribute;
                ValidatePreviousCollectionTypes(type, type2, previousCollectionTypes);
                return GetCollectionStableName(type, type2, previousCollectionTypes, out attribute);
            }
            localName = GetDefaultStableLocalName(type);
            if (ClassDataContract.IsNonAttributedTypeValidForSerialization(type))
            {
                ns = GetDefaultDataContractNamespace(type);
            }
            else
            {
                ns = GetDefaultStableNamespace(type);
            }
            return CreateQualifiedName(localName, ns);
        }

        internal static XmlQualifiedName GetStableName(Type type)
        {
            bool flag;
            return GetStableName(type, out flag);
        }

        internal static XmlQualifiedName GetStableName(Type type, out bool hasDataContract)
        {
            return GetStableName(type, new HashSet<Type>(), out hasDataContract);
        }

        private static XmlQualifiedName GetStableName(Type type, HashSet<Type> previousCollectionTypes, out bool hasDataContract)
        {
            XmlQualifiedName nonDCTypeStableName;
            DataContractAttribute attribute;
            type = UnwrapRedundantNullableType(type);
            if (TryGetBuiltInXmlAndArrayTypeStableName(type, previousCollectionTypes, out nonDCTypeStableName))
            {
                hasDataContract = false;
                return nonDCTypeStableName;
            }
            if (TryGetDCAttribute(type, out attribute))
            {
                nonDCTypeStableName = GetDCTypeStableName(type, attribute);
                hasDataContract = true;
                return nonDCTypeStableName;
            }
            nonDCTypeStableName = GetNonDCTypeStableName(type, previousCollectionTypes);
            hasDataContract = false;
            return nonDCTypeStableName;
        }

        internal virtual DataContract GetValidContract()
        {
            return this;
        }

        internal virtual DataContract GetValidContract(SerializationMode mode)
        {
            return this;
        }

        internal static Dictionary<XmlQualifiedName, DataContract> ImportKnownTypeAttributes(Type type)
        {
            Dictionary<XmlQualifiedName, DataContract> knownDataContracts = null;
            Dictionary<Type, Type> typesChecked = new Dictionary<Type, Type>();
            ImportKnownTypeAttributes(type, typesChecked, ref knownDataContracts);
            return knownDataContracts;
        }

        private static void ImportKnownTypeAttributes(Type type, Dictionary<Type, Type> typesChecked, ref Dictionary<XmlQualifiedName, DataContract> knownDataContracts)
        {
            while ((type != null) && IsTypeSerializable(type))
            {
                if (typesChecked.ContainsKey(type))
                {
                    return;
                }
                typesChecked.Add(type, type);
                object[] customAttributes = type.GetCustomAttributes(Globals.TypeOfKnownTypeAttribute, false);
                if (customAttributes != null)
                {
                    bool flag = false;
                    bool flag2 = false;
                    for (int i = 0; i < customAttributes.Length; i++)
                    {
                        KnownTypeAttribute attribute = (KnownTypeAttribute) customAttributes[i];
                        if (attribute.Type != null)
                        {
                            if (flag)
                            {
                                ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("KnownTypeAttributeOneScheme", new object[] { GetClrTypeFullName(type) }), type);
                            }
                            CheckAndAdd(attribute.Type, typesChecked, ref knownDataContracts);
                            flag2 = true;
                        }
                        else
                        {
                            if (flag || flag2)
                            {
                                ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("KnownTypeAttributeOneScheme", new object[] { GetClrTypeFullName(type) }), type);
                            }
                            string methodName = attribute.MethodName;
                            if (methodName == null)
                            {
                                ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("KnownTypeAttributeNoData", new object[] { GetClrTypeFullName(type) }), type);
                            }
                            if (methodName.Length == 0)
                            {
                                ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("KnownTypeAttributeEmptyString", new object[] { GetClrTypeFullName(type) }), type);
                            }
                            MethodInfo info = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, Globals.EmptyTypeArray, null);
                            if (info == null)
                            {
                                ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("KnownTypeAttributeUnknownMethod", new object[] { methodName, GetClrTypeFullName(type) }), type);
                            }
                            if (!Globals.TypeOfTypeEnumerable.IsAssignableFrom(info.ReturnType))
                            {
                                ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("KnownTypeAttributeReturnType", new object[] { GetClrTypeFullName(type), methodName }), type);
                            }
                            object obj2 = info.Invoke(null, Globals.EmptyObjectArray);
                            if (obj2 == null)
                            {
                                ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("KnownTypeAttributeMethodNull", new object[] { GetClrTypeFullName(type) }), type);
                            }
                            foreach (Type type2 in (IEnumerable<Type>) obj2)
                            {
                                if (type2 == null)
                                {
                                    ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("KnownTypeAttributeValidMethodTypes", new object[] { GetClrTypeFullName(type) }), type);
                                }
                                CheckAndAdd(type2, typesChecked, ref knownDataContracts);
                            }
                            flag = true;
                        }
                    }
                }
                LoadKnownTypesFromConfig(type, typesChecked, ref knownDataContracts);
                type = type.BaseType;
            }
        }

        private static bool IsAlpha(char ch)
        {
            return (((ch >= 'A') && (ch <= 'Z')) || ((ch >= 'a') && (ch <= 'z')));
        }

        private static bool IsAsciiLocalName(string localName)
        {
            if (localName.Length == 0)
            {
                return false;
            }
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

        internal static bool IsAssemblyFriendOfSerialization(Assembly assembly)
        {
            InternalsVisibleToAttribute[] customAttributes = (InternalsVisibleToAttribute[]) assembly.GetCustomAttributes(typeof(InternalsVisibleToAttribute), false);
            foreach (InternalsVisibleToAttribute attribute in customAttributes)
            {
                string assemblyName = attribute.AssemblyName;
                if (Regex.IsMatch(assemblyName, @"^[\s]*System\.Runtime\.Serialization[\s]*$") || Regex.IsMatch(assemblyName, @"^[\s]*System\.Runtime\.Serialization[\s]*,[\s]*PublicKey[\s]*=[\s]*(?i:00000000000000000400000000000000)[\s]*$"))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsBuiltInNamespace(string ns)
        {
            if (!(ns == "http://www.w3.org/2001/XMLSchema"))
            {
                return (ns == "http://schemas.microsoft.com/2003/10/Serialization/");
            }
            return true;
        }

        private static bool IsCollectionElementTypeEqualToRootType(string collectionElementTypeName, Type rootType)
        {
            if (collectionElementTypeName.StartsWith(GetClrTypeFullName(rootType), StringComparison.Ordinal))
            {
                Type t = Type.GetType(collectionElementTypeName, false);
                if (t != null)
                {
                    if (t.IsGenericType && !IsOpenGenericType(t))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("KnownTypeConfigClosedGenericDeclared", new object[] { collectionElementTypeName })));
                    }
                    if (rootType.Equals(t))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsDigit(char ch)
        {
            return ((ch >= '0') && (ch <= '9'));
        }

        private static bool IsElemTypeNullOrNotEqualToRootType(string elemTypeName, Type rootType)
        {
            Type o = Type.GetType(elemTypeName, false);
            if ((o != null) && rootType.Equals(o))
            {
                return false;
            }
            return true;
        }

        internal bool IsEqualOrChecked(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            if (this == other)
            {
                return true;
            }
            if (checkedContracts != null)
            {
                DataContractPairKey key = new DataContractPairKey(this, other);
                if (checkedContracts.ContainsKey(key))
                {
                    return true;
                }
                checkedContracts.Add(key, null);
            }
            return false;
        }

        private static bool IsMemberVisibleInSerializationModule(MemberInfo member)
        {
            if (!IsTypeVisibleInSerializationModule(member.DeclaringType))
            {
                return false;
            }
            if (member is MethodInfo)
            {
                MethodInfo info = (MethodInfo) member;
                if (!info.IsAssembly)
                {
                    return info.IsFamilyOrAssembly;
                }
                return true;
            }
            if (member is FieldInfo)
            {
                FieldInfo info2 = (FieldInfo) member;
                if (!info2.IsAssembly && !info2.IsFamilyOrAssembly)
                {
                    return false;
                }
                return IsTypeVisible(info2.FieldType);
            }
            if (!(member is ConstructorInfo))
            {
                return false;
            }
            ConstructorInfo info3 = (ConstructorInfo) member;
            if (!info3.IsAssembly)
            {
                return info3.IsFamilyOrAssembly;
            }
            return true;
        }

        private static bool IsOpenGenericType(Type t)
        {
            Type[] genericArguments = t.GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                if (!genericArguments[i].IsGenericParameter)
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsTypeNullable(Type type)
        {
            return (!type.IsValueType || (type.IsGenericType && (type.GetGenericTypeDefinition() == Globals.TypeOfNullable)));
        }

        internal static bool IsTypeSerializable(Type type)
        {
            return IsTypeSerializable(type, new HashSet<Type>());
        }

        private static bool IsTypeSerializable(Type type, HashSet<Type> previousCollectionTypes)
        {
            if ((!type.IsSerializable && !type.IsDefined(Globals.TypeOfDataContractAttribute, false)) && ((!type.IsInterface && !type.IsPointer) && !Globals.TypeOfIXmlSerializable.IsAssignableFrom(type)))
            {
                Type type2;
                if (CollectionDataContract.IsCollection(type, out type2))
                {
                    ValidatePreviousCollectionTypes(type, type2, previousCollectionTypes);
                    if (IsTypeSerializable(type2, previousCollectionTypes))
                    {
                        return true;
                    }
                }
                if (GetBuiltInDataContract(type) == null)
                {
                    return ClassDataContract.IsNonAttributedTypeValidForSerialization(type);
                }
            }
            return true;
        }

        internal static bool IsTypeVisible(Type t)
        {
            if (!t.IsVisible && !IsTypeVisibleInSerializationModule(t))
            {
                return false;
            }
            foreach (Type type in t.GetGenericArguments())
            {
                if (!type.IsGenericParameter && !IsTypeVisible(type))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsTypeVisibleInSerializationModule(Type type)
        {
            if (!type.Module.Equals(typeof(CodeGenerator).Module) && !IsAssemblyFriendOfSerialization(type.Assembly))
            {
                return false;
            }
            return !type.IsNestedPrivate;
        }

        internal virtual bool IsValidContract(SerializationMode mode)
        {
            return true;
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

        [SecuritySafeCritical]
        private static void LoadKnownTypesFromConfig(Type type, Dictionary<Type, Type> typesChecked, ref Dictionary<XmlQualifiedName, DataContract> knownDataContracts)
        {
            if (ConfigSection != null)
            {
                DeclaredTypeElementCollection declaredTypes = ConfigSection.DeclaredTypes;
                Type rootType = type;
                Type[] genArgs = null;
                CheckRootTypeInConfigIsGeneric(type, ref rootType, ref genArgs);
                DeclaredTypeElement element = declaredTypes[rootType.AssemblyQualifiedName];
                if ((element != null) && IsElemTypeNullOrNotEqualToRootType(element.Type, rootType))
                {
                    element = null;
                }
                if (element == null)
                {
                    for (int i = 0; i < declaredTypes.Count; i++)
                    {
                        if (IsCollectionElementTypeEqualToRootType(declaredTypes[i].Type, rootType))
                        {
                            element = declaredTypes[i];
                            break;
                        }
                    }
                }
                if (element != null)
                {
                    for (int j = 0; j < element.KnownTypes.Count; j++)
                    {
                        Type type3 = element.KnownTypes[j].GetType(element.Type, genArgs);
                        if (type3 != null)
                        {
                            CheckAndAdd(type3, typesChecked, ref knownDataContracts);
                        }
                    }
                }
            }
        }

        internal static bool MethodRequiresMemberAccess(MethodInfo method)
        {
            return (((method != null) && !method.IsPublic) && !IsMemberVisibleInSerializationModule(method));
        }

        public virtual object ReadXmlValue(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("UnexpectedContractType", new object[] { GetClrTypeFullName(base.GetType()), GetClrTypeFullName(this.UnderlyingType) })));
        }

        internal void ThrowInvalidDataContractException(string message)
        {
            ThrowInvalidDataContractException(message, this.UnderlyingType);
        }

        [SecuritySafeCritical]
        internal static void ThrowInvalidDataContractException(string message, Type type)
        {
            DataContractCriticalHelper.ThrowInvalidDataContractException(message, type);
        }

        public static void ThrowTypeNotSerializable(Type type)
        {
            ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("TypeNotSerializable", new object[] { type }), type);
        }

        private static bool TryGetBuiltInXmlAndArrayTypeStableName(Type type, HashSet<Type> previousCollectionTypes, out XmlQualifiedName stableName)
        {
            stableName = null;
            DataContract builtInDataContract = GetBuiltInDataContract(type);
            if (builtInDataContract != null)
            {
                stableName = builtInDataContract.StableName;
            }
            else if (Globals.TypeOfIXmlSerializable.IsAssignableFrom(type))
            {
                bool flag;
                XmlSchemaType type2;
                XmlQualifiedName name;
                SchemaExporter.GetXmlTypeInfo(type, out name, out type2, out flag);
                stableName = name;
            }
            else if (type.IsArray)
            {
                CollectionDataContractAttribute attribute;
                Type elementType = type.GetElementType();
                ValidatePreviousCollectionTypes(type, elementType, previousCollectionTypes);
                stableName = GetCollectionStableName(type, elementType, previousCollectionTypes, out attribute);
            }
            return (stableName != null);
        }

        [SecuritySafeCritical]
        internal static bool TryGetDCAttribute(Type type, out DataContractAttribute dataContractAttribute)
        {
            dataContractAttribute = null;
            object[] customAttributes = type.GetCustomAttributes(Globals.TypeOfDataContractAttribute, false);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                dataContractAttribute = (DataContractAttribute) customAttributes[0];
            }
            return (dataContractAttribute != null);
        }

        internal static Type UnwrapNullableType(Type type)
        {
            while (type.IsGenericType && (type.GetGenericTypeDefinition() == Globals.TypeOfNullable))
            {
                type = type.GetGenericArguments()[0];
            }
            return type;
        }

        internal static Type UnwrapRedundantNullableType(Type type)
        {
            Type type2 = type;
            while (type.IsGenericType && (type.GetGenericTypeDefinition() == Globals.TypeOfNullable))
            {
                type2 = type;
                type = type.GetGenericArguments()[0];
            }
            return type2;
        }

        private static void ValidatePreviousCollectionTypes(Type collectionType, Type itemType, HashSet<Type> previousCollectionTypes)
        {
            previousCollectionTypes.Add(collectionType);
            while (itemType.IsArray)
            {
                itemType = itemType.GetElementType();
            }
            if (previousCollectionTypes.Contains(itemType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("RecursiveCollectionType", new object[] { GetClrTypeFullName(itemType) })));
            }
        }

        internal virtual void WriteRootElement(XmlWriterDelegator writer, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (object.ReferenceEquals(ns, DictionaryGlobals.SerializationNamespace) && !this.IsPrimitive)
            {
                writer.WriteStartElement("z", name, ns);
            }
            else
            {
                writer.WriteStartElement(name, ns);
            }
        }

        public virtual void WriteXmlValue(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("UnexpectedContractType", new object[] { GetClrTypeFullName(base.GetType()), GetClrTypeFullName(this.UnderlyingType) })));
        }

        internal virtual bool CanContainReferences
        {
            get
            {
                return true;
            }
        }

        private static DataContractSerializerSection ConfigSection
        {
            [SecurityCritical]
            get
            {
                if (configSection == null)
                {
                    configSection = DataContractSerializerSection.UnsafeGetSection();
                }
                return configSection;
            }
        }

        internal System.Runtime.Serialization.GenericInfo GenericInfo
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.GenericInfo;
            }
            [SecurityCritical]
            set
            {
                this.helper.GenericInfo = value;
            }
        }

        internal virtual bool HasRoot
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        protected DataContractCriticalHelper Helper
        {
            [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.helper;
            }
        }

        internal virtual bool IsBuiltInDataContract
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsBuiltInDataContract;
            }
        }

        internal virtual bool IsISerializable
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsISerializable;
            }
            [SecurityCritical]
            set
            {
                this.helper.IsISerializable = value;
            }
        }

        internal virtual bool IsPrimitive
        {
            get
            {
                return false;
            }
        }

        internal bool IsReference
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsReference;
            }
            [SecurityCritical]
            set
            {
                this.helper.IsReference = value;
            }
        }

        internal bool IsValueType
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsValueType;
            }
            [SecurityCritical]
            set
            {
                this.helper.IsValueType = value;
            }
        }

        internal virtual Dictionary<XmlQualifiedName, DataContract> KnownDataContracts
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.KnownDataContracts;
            }
            [SecurityCritical]
            set
            {
                this.helper.KnownDataContracts = value;
            }
        }

        internal XmlDictionaryString Name
        {
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }

        public virtual XmlDictionaryString Namespace
        {
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ns;
            }
        }

        internal Type OriginalUnderlyingType
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.OriginalUnderlyingType;
            }
        }

        internal XmlQualifiedName StableName
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.StableName;
            }
            [SecurityCritical]
            set
            {
                this.helper.StableName = value;
            }
        }

        internal virtual XmlDictionaryString TopLevelElementName
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.TopLevelElementName;
            }
            [SecurityCritical]
            set
            {
                this.helper.TopLevelElementName = value;
            }
        }

        internal virtual XmlDictionaryString TopLevelElementNamespace
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.TopLevelElementNamespace;
            }
            [SecurityCritical]
            set
            {
                this.helper.TopLevelElementNamespace = value;
            }
        }

        internal Type TypeForInitialization
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.TypeForInitialization;
            }
        }

        internal Type UnderlyingType
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.UnderlyingType;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        protected class DataContractCriticalHelper
        {
            private static object cacheLock = new object();
            private static Dictionary<string, XmlDictionaryString> clrTypeStrings;
            private static XmlDictionary clrTypeStringsDictionary;
            private static object clrTypeStringsLock = new object();
            private static object createDataContractLock = new object();
            private static DataContract[] dataContractCache = new DataContract[0x20];
            private static int dataContractID = 0;
            private System.Runtime.Serialization.GenericInfo genericInfo;
            private static object initBuiltInContractsLock = new object();
            private bool isReference;
            private bool isValueType;
            private XmlDictionaryString name;
            private static Dictionary<string, string> namespaces;
            private static object namespacesLock = new object();
            private static Dictionary<XmlQualifiedName, DataContract> nameToBuiltInContract;
            private XmlDictionaryString ns;
            private Type originalUnderlyingType;
            private XmlQualifiedName stableName;
            private Type typeForInitialization;
            private static TypeHandleRef typeHandleRef = new TypeHandleRef();
            private static Dictionary<string, DataContract> typeNameToBuiltInContract;
            private static Dictionary<Type, DataContract> typeToBuiltInContract;
            private static Dictionary<TypeHandleRef, IntRef> typeToIDCache = new Dictionary<TypeHandleRef, IntRef>(new TypeHandleRefEqualityComparer());
            private readonly Type underlyingType;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal DataContractCriticalHelper()
            {
            }

            internal DataContractCriticalHelper(Type type)
            {
                this.underlyingType = type;
                this.SetTypeForInitialization(type);
                this.isValueType = type.IsValueType;
            }

            private static bool ContractMatches(DataContract contract, DataContract cachedContract)
            {
                return ((cachedContract != null) && (cachedContract.UnderlyingType == contract.UnderlyingType));
            }

            private static DataContract CreateDataContract(int id, RuntimeTypeHandle typeHandle, Type type)
            {
                lock (createDataContractLock)
                {
                    DataContract dataContract = dataContractCache[id];
                    if (dataContract == null)
                    {
                        if (type == null)
                        {
                            type = Type.GetTypeFromHandle(typeHandle);
                        }
                        type = DataContract.UnwrapNullableType(type);
                        type = GetDataContractAdapterType(type);
                        dataContract = GetBuiltInDataContract(type);
                        if (dataContract == null)
                        {
                            if (type.IsArray)
                            {
                                dataContract = new CollectionDataContract(type);
                            }
                            else if (type.IsEnum)
                            {
                                dataContract = new EnumDataContract(type);
                            }
                            else if (type.IsGenericParameter)
                            {
                                dataContract = new GenericParameterDataContract(type);
                            }
                            else if (Globals.TypeOfIXmlSerializable.IsAssignableFrom(type))
                            {
                                dataContract = new XmlDataContract(type);
                            }
                            else
                            {
                                if (type.IsPointer)
                                {
                                    type = Globals.TypeOfReflectionPointer;
                                }
                                if (!CollectionDataContract.TryCreate(type, out dataContract))
                                {
                                    if ((!type.IsSerializable && !type.IsDefined(Globals.TypeOfDataContractAttribute, false)) && !ClassDataContract.IsNonAttributedTypeValidForSerialization(type))
                                    {
                                        ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("TypeNotSerializable", new object[] { type }), type);
                                    }
                                    dataContract = new ClassDataContract(type);
                                }
                            }
                        }
                    }
                    dataContractCache[id] = dataContract;
                    return dataContract;
                }
            }

            private static DataContract CreateGetOnlyCollectionDataContract(int id, RuntimeTypeHandle typeHandle, Type type)
            {
                DataContract dataContract = null;
                lock (createDataContractLock)
                {
                    dataContract = dataContractCache[id];
                    if (dataContract != null)
                    {
                        return dataContract;
                    }
                    if (type == null)
                    {
                        type = Type.GetTypeFromHandle(typeHandle);
                    }
                    type = DataContract.UnwrapNullableType(type);
                    type = GetDataContractAdapterType(type);
                    CollectionDataContract.CreateGetOnlyCollectionDataContract(type, out dataContract);
                }
                return dataContract;
            }

            public static DataContract GetBuiltInDataContract(string typeName)
            {
                if (!typeName.StartsWith("System.", StringComparison.Ordinal))
                {
                    return null;
                }
                lock (initBuiltInContractsLock)
                {
                    if (typeNameToBuiltInContract == null)
                    {
                        typeNameToBuiltInContract = new Dictionary<string, DataContract>();
                    }
                    DataContract contract = null;
                    if (!typeNameToBuiltInContract.TryGetValue(typeName, out contract))
                    {
                        Type type = null;
                        switch (typeName.Substring(7))
                        {
                            case "Char":
                                type = typeof(char);
                                break;

                            case "Boolean":
                                type = typeof(bool);
                                break;

                            case "SByte":
                                type = typeof(sbyte);
                                break;

                            case "Byte":
                                type = typeof(byte);
                                break;

                            case "Int16":
                                type = typeof(short);
                                break;

                            case "UInt16":
                                type = typeof(ushort);
                                break;

                            case "Int32":
                                type = typeof(int);
                                break;

                            case "UInt32":
                                type = typeof(uint);
                                break;

                            case "Int64":
                                type = typeof(long);
                                break;

                            case "UInt64":
                                type = typeof(ulong);
                                break;

                            case "Single":
                                type = typeof(float);
                                break;

                            case "Double":
                                type = typeof(double);
                                break;

                            case "Decimal":
                                type = typeof(decimal);
                                break;

                            case "DateTime":
                                type = typeof(DateTime);
                                break;

                            case "String":
                                type = typeof(string);
                                break;

                            case "Byte[]":
                                type = typeof(byte[]);
                                break;

                            case "Object":
                                type = typeof(object);
                                break;

                            case "TimeSpan":
                                type = typeof(TimeSpan);
                                break;

                            case "Guid":
                                type = typeof(Guid);
                                break;

                            case "Uri":
                                type = typeof(Uri);
                                break;

                            case "Xml.XmlQualifiedName":
                                type = typeof(XmlQualifiedName);
                                break;

                            case "Enum":
                                type = typeof(Enum);
                                break;

                            case "ValueType":
                                type = typeof(ValueType);
                                break;

                            case "Array":
                                type = typeof(Array);
                                break;

                            case "Xml.XmlElement":
                                type = typeof(XmlElement);
                                break;

                            case "Xml.XmlNode[]":
                                type = typeof(System.Xml.XmlNode[]);
                                break;
                        }
                        if (type != null)
                        {
                            TryCreateBuiltInDataContract(type, out contract);
                        }
                        typeNameToBuiltInContract.Add(typeName, contract);
                    }
                    return contract;
                }
            }

            public static DataContract GetBuiltInDataContract(Type type)
            {
                if (type.IsInterface && !CollectionDataContract.IsCollectionInterface(type))
                {
                    type = Globals.TypeOfObject;
                }
                lock (initBuiltInContractsLock)
                {
                    if (typeToBuiltInContract == null)
                    {
                        typeToBuiltInContract = new Dictionary<Type, DataContract>();
                    }
                    DataContract contract = null;
                    if (!typeToBuiltInContract.TryGetValue(type, out contract))
                    {
                        TryCreateBuiltInDataContract(type, out contract);
                        typeToBuiltInContract.Add(type, contract);
                    }
                    return contract;
                }
            }

            public static DataContract GetBuiltInDataContract(string name, string ns)
            {
                lock (initBuiltInContractsLock)
                {
                    if (nameToBuiltInContract == null)
                    {
                        nameToBuiltInContract = new Dictionary<XmlQualifiedName, DataContract>();
                    }
                    DataContract contract = null;
                    XmlQualifiedName key = new XmlQualifiedName(name, ns);
                    if (!nameToBuiltInContract.TryGetValue(key, out contract))
                    {
                        TryCreateBuiltInDataContract(name, ns, out contract);
                        nameToBuiltInContract.Add(key, contract);
                    }
                    return contract;
                }
            }

            internal static XmlDictionaryString GetClrTypeString(string key)
            {
                lock (clrTypeStringsLock)
                {
                    XmlDictionaryString str;
                    if (clrTypeStrings == null)
                    {
                        clrTypeStringsDictionary = new XmlDictionary();
                        clrTypeStrings = new Dictionary<string, XmlDictionaryString>();
                        try
                        {
                            clrTypeStrings.Add(Globals.TypeOfInt.Assembly.FullName, clrTypeStringsDictionary.Add("0"));
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(exception.Message, exception);
                        }
                    }
                    if (!clrTypeStrings.TryGetValue(key, out str))
                    {
                        str = clrTypeStringsDictionary.Add(key);
                        try
                        {
                            clrTypeStrings.Add(key, str);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(exception2.Message, exception2);
                        }
                    }
                    return str;
                }
            }

            internal static Type GetDataContractAdapterType(Type type)
            {
                if (type == Globals.TypeOfDateTimeOffset)
                {
                    return Globals.TypeOfDateTimeOffsetAdapter;
                }
                return type;
            }

            private static RuntimeTypeHandle GetDataContractAdapterTypeHandle(RuntimeTypeHandle typeHandle)
            {
                if (Globals.TypeOfDateTimeOffset.TypeHandle.Equals(typeHandle))
                {
                    return Globals.TypeOfDateTimeOffsetAdapter.TypeHandle;
                }
                return typeHandle;
            }

            internal static DataContract GetDataContractForInitialization(int id)
            {
                DataContract contract = dataContractCache[id];
                if (contract == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("DataContractCacheOverflow")));
                }
                return contract;
            }

            internal static Type GetDataContractOriginalType(Type type)
            {
                if (type == Globals.TypeOfDateTimeOffsetAdapter)
                {
                    return Globals.TypeOfDateTimeOffset;
                }
                return type;
            }

            internal static DataContract GetDataContractSkipValidation(int id, RuntimeTypeHandle typeHandle, Type type)
            {
                DataContract contract = dataContractCache[id];
                if (contract == null)
                {
                    return CreateDataContract(id, typeHandle, type);
                }
                return contract.GetValidContract();
            }

            internal static DataContract GetGetOnlyCollectionDataContractSkipValidation(int id, RuntimeTypeHandle typeHandle, Type type)
            {
                DataContract contract = dataContractCache[id];
                if (contract == null)
                {
                    contract = CreateGetOnlyCollectionDataContract(id, typeHandle, type);
                    dataContractCache[id] = contract;
                }
                return contract;
            }

            internal static int GetId(RuntimeTypeHandle typeHandle)
            {
                lock (cacheLock)
                {
                    IntRef nextId;
                    typeHandle = GetDataContractAdapterTypeHandle(typeHandle);
                    typeHandleRef.Value = typeHandle;
                    if (!typeToIDCache.TryGetValue(typeHandleRef, out nextId))
                    {
                        nextId = GetNextId();
                        try
                        {
                            typeToIDCache.Add(new TypeHandleRef(typeHandle), nextId);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(exception.Message, exception);
                        }
                    }
                    return nextId.Value;
                }
            }

            internal static int GetIdForInitialization(ClassDataContract classContract)
            {
                int id = DataContract.GetId(classContract.TypeForInitialization.TypeHandle);
                if ((id < dataContractCache.Length) && ContractMatches(classContract, dataContractCache[id]))
                {
                    return id;
                }
                for (int i = 0; i < dataContractID; i++)
                {
                    if (ContractMatches(classContract, dataContractCache[i]))
                    {
                        return i;
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("DataContractCacheOverflow")));
            }

            internal static string GetNamespace(string key)
            {
                lock (namespacesLock)
                {
                    string str;
                    if (namespaces == null)
                    {
                        namespaces = new Dictionary<string, string>();
                    }
                    if (namespaces.TryGetValue(key, out str))
                    {
                        return str;
                    }
                    try
                    {
                        namespaces.Add(key, key);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(exception.Message, exception);
                    }
                    return key;
                }
            }

            private static IntRef GetNextId()
            {
                int num = dataContractID++;
                if (num >= dataContractCache.Length)
                {
                    int newSize = (num < 0x3fffffff) ? (num * 2) : 0x7fffffff;
                    if (newSize <= num)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("DataContractCacheOverflow")));
                    }
                    Array.Resize<DataContract>(ref dataContractCache, newSize);
                }
                return new IntRef(num);
            }

            internal void SetDataContractName(XmlQualifiedName stableName)
            {
                XmlDictionary dictionary = new XmlDictionary(2);
                this.Name = dictionary.Add(stableName.Name);
                this.Namespace = dictionary.Add(stableName.Namespace);
                this.StableName = stableName;
            }

            internal void SetDataContractName(XmlDictionaryString name, XmlDictionaryString ns)
            {
                this.Name = name;
                this.Namespace = ns;
                this.StableName = DataContract.CreateQualifiedName(name.Value, ns.Value);
            }

            [SecuritySafeCritical]
            private void SetTypeForInitialization(Type classType)
            {
                if (classType.IsSerializable || classType.IsDefined(Globals.TypeOfDataContractAttribute, false))
                {
                    this.typeForInitialization = classType;
                }
            }

            internal void ThrowInvalidDataContractException(string message)
            {
                ThrowInvalidDataContractException(message, this.UnderlyingType);
            }

            internal static void ThrowInvalidDataContractException(string message, Type type)
            {
                if (type != null)
                {
                    lock (cacheLock)
                    {
                        typeHandleRef.Value = GetDataContractAdapterTypeHandle(type.TypeHandle);
                        try
                        {
                            typeToIDCache.Remove(typeHandleRef);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(exception.Message, exception);
                        }
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(message));
            }

            public static bool TryCreateBuiltInDataContract(Type type, out DataContract dataContract)
            {
                if (type.IsEnum)
                {
                    dataContract = null;
                    return false;
                }
                dataContract = null;
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        dataContract = new BooleanDataContract();
                        break;

                    case TypeCode.Char:
                        dataContract = new CharDataContract();
                        break;

                    case TypeCode.SByte:
                        dataContract = new SignedByteDataContract();
                        break;

                    case TypeCode.Byte:
                        dataContract = new UnsignedByteDataContract();
                        break;

                    case TypeCode.Int16:
                        dataContract = new ShortDataContract();
                        break;

                    case TypeCode.UInt16:
                        dataContract = new UnsignedShortDataContract();
                        break;

                    case TypeCode.Int32:
                        dataContract = new IntDataContract();
                        break;

                    case TypeCode.UInt32:
                        dataContract = new UnsignedIntDataContract();
                        break;

                    case TypeCode.Int64:
                        dataContract = new LongDataContract();
                        break;

                    case TypeCode.UInt64:
                        dataContract = new UnsignedLongDataContract();
                        break;

                    case TypeCode.Single:
                        dataContract = new FloatDataContract();
                        break;

                    case TypeCode.Double:
                        dataContract = new DoubleDataContract();
                        break;

                    case TypeCode.Decimal:
                        dataContract = new DecimalDataContract();
                        break;

                    case TypeCode.DateTime:
                        dataContract = new DateTimeDataContract();
                        break;

                    case TypeCode.String:
                        dataContract = new StringDataContract();
                        break;

                    default:
                        if (type == typeof(byte[]))
                        {
                            dataContract = new ByteArrayDataContract();
                        }
                        else if (type == typeof(object))
                        {
                            dataContract = new ObjectDataContract();
                        }
                        else if (type == typeof(Uri))
                        {
                            dataContract = new UriDataContract();
                        }
                        else if (type == typeof(XmlQualifiedName))
                        {
                            dataContract = new QNameDataContract();
                        }
                        else if (type == typeof(TimeSpan))
                        {
                            dataContract = new TimeSpanDataContract();
                        }
                        else if (type == typeof(Guid))
                        {
                            dataContract = new GuidDataContract();
                        }
                        else if ((type == typeof(Enum)) || (type == typeof(ValueType)))
                        {
                            dataContract = new SpecialTypeDataContract(type, DictionaryGlobals.ObjectLocalName, DictionaryGlobals.SchemaNamespace);
                        }
                        else if (type == typeof(Array))
                        {
                            dataContract = new CollectionDataContract(type);
                        }
                        else if ((type == typeof(XmlElement)) || (type == typeof(System.Xml.XmlNode[])))
                        {
                            dataContract = new XmlDataContract(type);
                        }
                        break;
                }
                return (dataContract != null);
            }

            public static bool TryCreateBuiltInDataContract(string name, string ns, out DataContract dataContract)
            {
                dataContract = null;
                if (ns == DictionaryGlobals.SchemaNamespace.Value)
                {
                    if (DictionaryGlobals.BooleanLocalName.Value == name)
                    {
                        dataContract = new BooleanDataContract();
                    }
                    else if (DictionaryGlobals.SignedByteLocalName.Value == name)
                    {
                        dataContract = new SignedByteDataContract();
                    }
                    else if (DictionaryGlobals.UnsignedByteLocalName.Value == name)
                    {
                        dataContract = new UnsignedByteDataContract();
                    }
                    else if (DictionaryGlobals.ShortLocalName.Value == name)
                    {
                        dataContract = new ShortDataContract();
                    }
                    else if (DictionaryGlobals.UnsignedShortLocalName.Value == name)
                    {
                        dataContract = new UnsignedShortDataContract();
                    }
                    else if (DictionaryGlobals.IntLocalName.Value == name)
                    {
                        dataContract = new IntDataContract();
                    }
                    else if (DictionaryGlobals.UnsignedIntLocalName.Value == name)
                    {
                        dataContract = new UnsignedIntDataContract();
                    }
                    else if (DictionaryGlobals.LongLocalName.Value == name)
                    {
                        dataContract = new LongDataContract();
                    }
                    else if (DictionaryGlobals.integerLocalName.Value == name)
                    {
                        dataContract = new IntegerDataContract();
                    }
                    else if (DictionaryGlobals.positiveIntegerLocalName.Value == name)
                    {
                        dataContract = new PositiveIntegerDataContract();
                    }
                    else if (DictionaryGlobals.negativeIntegerLocalName.Value == name)
                    {
                        dataContract = new NegativeIntegerDataContract();
                    }
                    else if (DictionaryGlobals.nonPositiveIntegerLocalName.Value == name)
                    {
                        dataContract = new NonPositiveIntegerDataContract();
                    }
                    else if (DictionaryGlobals.nonNegativeIntegerLocalName.Value == name)
                    {
                        dataContract = new NonNegativeIntegerDataContract();
                    }
                    else if (DictionaryGlobals.UnsignedLongLocalName.Value == name)
                    {
                        dataContract = new UnsignedLongDataContract();
                    }
                    else if (DictionaryGlobals.FloatLocalName.Value == name)
                    {
                        dataContract = new FloatDataContract();
                    }
                    else if (DictionaryGlobals.DoubleLocalName.Value == name)
                    {
                        dataContract = new DoubleDataContract();
                    }
                    else if (DictionaryGlobals.DecimalLocalName.Value == name)
                    {
                        dataContract = new DecimalDataContract();
                    }
                    else if (DictionaryGlobals.DateTimeLocalName.Value == name)
                    {
                        dataContract = new DateTimeDataContract();
                    }
                    else if (DictionaryGlobals.StringLocalName.Value == name)
                    {
                        dataContract = new StringDataContract();
                    }
                    else if (DictionaryGlobals.timeLocalName.Value == name)
                    {
                        dataContract = new TimeDataContract();
                    }
                    else if (DictionaryGlobals.dateLocalName.Value == name)
                    {
                        dataContract = new DateDataContract();
                    }
                    else if (DictionaryGlobals.hexBinaryLocalName.Value == name)
                    {
                        dataContract = new HexBinaryDataContract();
                    }
                    else if (DictionaryGlobals.gYearMonthLocalName.Value == name)
                    {
                        dataContract = new GYearMonthDataContract();
                    }
                    else if (DictionaryGlobals.gYearLocalName.Value == name)
                    {
                        dataContract = new GYearDataContract();
                    }
                    else if (DictionaryGlobals.gMonthDayLocalName.Value == name)
                    {
                        dataContract = new GMonthDayDataContract();
                    }
                    else if (DictionaryGlobals.gDayLocalName.Value == name)
                    {
                        dataContract = new GDayDataContract();
                    }
                    else if (DictionaryGlobals.gMonthLocalName.Value == name)
                    {
                        dataContract = new GMonthDataContract();
                    }
                    else if (DictionaryGlobals.normalizedStringLocalName.Value == name)
                    {
                        dataContract = new NormalizedStringDataContract();
                    }
                    else if (DictionaryGlobals.tokenLocalName.Value == name)
                    {
                        dataContract = new TokenDataContract();
                    }
                    else if (DictionaryGlobals.languageLocalName.Value == name)
                    {
                        dataContract = new LanguageDataContract();
                    }
                    else if (DictionaryGlobals.NameLocalName.Value == name)
                    {
                        dataContract = new NameDataContract();
                    }
                    else if (DictionaryGlobals.NCNameLocalName.Value == name)
                    {
                        dataContract = new NCNameDataContract();
                    }
                    else if (DictionaryGlobals.XSDIDLocalName.Value == name)
                    {
                        dataContract = new IDDataContract();
                    }
                    else if (DictionaryGlobals.IDREFLocalName.Value == name)
                    {
                        dataContract = new IDREFDataContract();
                    }
                    else if (DictionaryGlobals.IDREFSLocalName.Value == name)
                    {
                        dataContract = new IDREFSDataContract();
                    }
                    else if (DictionaryGlobals.ENTITYLocalName.Value == name)
                    {
                        dataContract = new ENTITYDataContract();
                    }
                    else if (DictionaryGlobals.ENTITIESLocalName.Value == name)
                    {
                        dataContract = new ENTITIESDataContract();
                    }
                    else if (DictionaryGlobals.NMTOKENLocalName.Value == name)
                    {
                        dataContract = new NMTOKENDataContract();
                    }
                    else if (DictionaryGlobals.NMTOKENSLocalName.Value == name)
                    {
                        dataContract = new NMTOKENDataContract();
                    }
                    else if (DictionaryGlobals.ByteArrayLocalName.Value == name)
                    {
                        dataContract = new ByteArrayDataContract();
                    }
                    else if (DictionaryGlobals.ObjectLocalName.Value == name)
                    {
                        dataContract = new ObjectDataContract();
                    }
                    else if (DictionaryGlobals.TimeSpanLocalName.Value == name)
                    {
                        dataContract = new XsDurationDataContract();
                    }
                    else if (DictionaryGlobals.UriLocalName.Value == name)
                    {
                        dataContract = new UriDataContract();
                    }
                    else if (DictionaryGlobals.QNameLocalName.Value == name)
                    {
                        dataContract = new QNameDataContract();
                    }
                }
                else if (ns == DictionaryGlobals.SerializationNamespace.Value)
                {
                    if (DictionaryGlobals.TimeSpanLocalName.Value == name)
                    {
                        dataContract = new TimeSpanDataContract();
                    }
                    else if (DictionaryGlobals.GuidLocalName.Value == name)
                    {
                        dataContract = new GuidDataContract();
                    }
                    else if (DictionaryGlobals.CharLocalName.Value == name)
                    {
                        dataContract = new CharDataContract();
                    }
                    else if ("ArrayOfanyType" == name)
                    {
                        dataContract = new CollectionDataContract(typeof(Array));
                    }
                }
                else if (ns == DictionaryGlobals.AsmxTypesNamespace.Value)
                {
                    if (DictionaryGlobals.CharLocalName.Value == name)
                    {
                        dataContract = new AsmxCharDataContract();
                    }
                    else if (DictionaryGlobals.GuidLocalName.Value == name)
                    {
                        dataContract = new AsmxGuidDataContract();
                    }
                }
                else if (ns == "http://schemas.datacontract.org/2004/07/System.Xml")
                {
                    if (name == "XmlElement")
                    {
                        dataContract = new XmlDataContract(typeof(XmlElement));
                    }
                    else if (name == "ArrayOfXmlNode")
                    {
                        dataContract = new XmlDataContract(typeof(System.Xml.XmlNode[]));
                    }
                }
                return (dataContract != null);
            }

            internal virtual void WriteRootElement(XmlWriterDelegator writer, XmlDictionaryString name, XmlDictionaryString ns)
            {
                if (object.ReferenceEquals(ns, DictionaryGlobals.SerializationNamespace) && !this.IsPrimitive)
                {
                    writer.WriteStartElement("z", name, ns);
                }
                else
                {
                    writer.WriteStartElement(name, ns);
                }
            }

            internal virtual bool CanContainReferences
            {
                get
                {
                    return true;
                }
            }

            internal System.Runtime.Serialization.GenericInfo GenericInfo
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.genericInfo;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.genericInfo = value;
                }
            }

            internal virtual bool HasRoot
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }

            internal virtual bool IsBuiltInDataContract
            {
                get
                {
                    return false;
                }
            }

            internal virtual bool IsISerializable
            {
                get
                {
                    return false;
                }
                set
                {
                    this.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("RequiresClassDataContractToSetIsISerializable"));
                }
            }

            internal virtual bool IsPrimitive
            {
                get
                {
                    return false;
                }
            }

            internal bool IsReference
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.isReference;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.isReference = value;
                }
            }

            internal bool IsValueType
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.isValueType;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.isValueType = value;
                }
            }

            internal virtual Dictionary<XmlQualifiedName, DataContract> KnownDataContracts
            {
                get
                {
                    return null;
                }
                set
                {
                }
            }

            internal XmlDictionaryString Name
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.name;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.name = value;
                }
            }

            public XmlDictionaryString Namespace
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.ns;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.ns = value;
                }
            }

            internal Type OriginalUnderlyingType
            {
                get
                {
                    if (this.originalUnderlyingType == null)
                    {
                        this.originalUnderlyingType = GetDataContractOriginalType(this.underlyingType);
                    }
                    return this.originalUnderlyingType;
                }
            }

            internal XmlQualifiedName StableName
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.stableName;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.stableName = value;
                }
            }

            internal virtual XmlDictionaryString TopLevelElementName
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.name;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.name = value;
                }
            }

            internal virtual XmlDictionaryString TopLevelElementNamespace
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.ns;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.ns = value;
                }
            }

            internal Type TypeForInitialization
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.typeForInitialization;
                }
            }

            internal Type UnderlyingType
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.underlyingType;
                }
            }
        }
    }
}

