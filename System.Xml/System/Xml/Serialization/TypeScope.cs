namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    internal class TypeScope
    {
        private Hashtable arrayTypeDescs = new Hashtable();
        private static Hashtable primitiveDataTypes = new Hashtable();
        private static System.Xml.Serialization.NameTable primitiveNames = new System.Xml.Serialization.NameTable();
        private static Hashtable primitiveTypes = new Hashtable();
        private Hashtable typeDescs = new Hashtable();
        private ArrayList typeMappings = new ArrayList();
        private static string[] unsupportedTypes = new string[] { 
            "anyURI", "duration", "ENTITY", "ENTITIES", "gDay", "gMonth", "gMonthDay", "gYear", "gYearMonth", "ID", "IDREF", "IDREFS", "integer", "language", "negativeInteger", "nonNegativeInteger", 
            "nonPositiveInteger", "NOTATION", "positiveInteger", "token"
         };

        static TypeScope()
        {
            AddPrimitive(typeof(string), "string", "String", TypeFlags.HasDefaultConstructor | TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddPrimitive(typeof(int), "int", "Int32", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(bool), "boolean", "Boolean", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(short), "short", "Int16", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(long), "long", "Int64", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(float), "float", "Single", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(double), "double", "Double", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(decimal), "decimal", "Decimal", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(DateTime), "dateTime", "DateTime", TypeFlags.XmlEncodingNotRequired | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(XmlQualifiedName), "QName", "XmlQualifiedName", TypeFlags.XmlEncodingNotRequired | TypeFlags.HasIsEmpty | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddPrimitive(typeof(byte), "unsignedByte", "Byte", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(sbyte), "byte", "SByte", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(ushort), "unsignedShort", "UInt16", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(uint), "unsignedInt", "UInt32", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(ulong), "unsignedLong", "UInt64", TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(DateTime), "date", "Date", TypeFlags.XmlEncodingNotRequired | TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(DateTime), "time", "Time", TypeFlags.XmlEncodingNotRequired | TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddPrimitive(typeof(string), "Name", "XmlName", TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddPrimitive(typeof(string), "NCName", "XmlNCName", TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddPrimitive(typeof(string), "NMTOKEN", "XmlNmToken", TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddPrimitive(typeof(string), "NMTOKENS", "XmlNmTokens", TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddPrimitive(typeof(byte[]), "base64Binary", "ByteArrayBase64", TypeFlags.XmlEncodingNotRequired | TypeFlags.HasDefaultConstructor | TypeFlags.IgnoreDefault | TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddPrimitive(typeof(byte[]), "hexBinary", "ByteArrayHex", TypeFlags.XmlEncodingNotRequired | TypeFlags.HasDefaultConstructor | TypeFlags.IgnoreDefault | TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            XmlSchemaPatternFacet facet = new XmlSchemaPatternFacet {
                Value = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"
            };
            AddNonXsdPrimitive(typeof(Guid), "guid", "http://microsoft.com/wsdl/types/", "Guid", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), new XmlSchemaFacet[] { facet }, TypeFlags.XmlEncodingNotRequired | TypeFlags.IgnoreDefault | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddNonXsdPrimitive(typeof(char), "char", "http://microsoft.com/wsdl/types/", "Char", new XmlQualifiedName("unsignedShort", "http://www.w3.org/2001/XMLSchema"), new XmlSchemaFacet[0], TypeFlags.IgnoreDefault | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedTypes("http://schemas.xmlsoap.org/soap/encoding/");
            AddPrimitive(typeof(string), "normalizedString", "String", TypeFlags.HasDefaultConstructor | TypeFlags.AmbiguousDataType | TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            for (int i = 0; i < unsupportedTypes.Length; i++)
            {
                AddPrimitive(typeof(string), unsupportedTypes[i], "String", TypeFlags.CollapseWhitespace | TypeFlags.AmbiguousDataType | TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            }
        }

        private static void AddNonXsdPrimitive(Type type, string dataTypeName, string ns, string formatterName, XmlQualifiedName baseTypeName, XmlSchemaFacet[] facets, TypeFlags flags)
        {
            XmlSchemaSimpleType dataType = new XmlSchemaSimpleType {
                Name = dataTypeName
            };
            XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction {
                BaseTypeName = baseTypeName
            };
            foreach (XmlSchemaFacet facet in facets)
            {
                restriction.Facets.Add(facet);
            }
            dataType.Content = restriction;
            TypeDesc desc = new TypeDesc(type, false, dataType, formatterName, flags);
            if (primitiveTypes[type] == null)
            {
                primitiveTypes.Add(type, desc);
            }
            primitiveDataTypes.Add(dataType, desc);
            primitiveNames.Add(dataTypeName, ns, desc);
        }

        private static void AddPrimitive(Type type, string dataTypeName, string formatterName, TypeFlags flags)
        {
            XmlSchemaSimpleType dataType = new XmlSchemaSimpleType {
                Name = dataTypeName
            };
            TypeDesc desc = new TypeDesc(type, true, dataType, formatterName, flags);
            if (primitiveTypes[type] == null)
            {
                primitiveTypes.Add(type, desc);
            }
            primitiveDataTypes.Add(dataType, desc);
            primitiveNames.Add(dataTypeName, "http://www.w3.org/2001/XMLSchema", desc);
        }

        private static void AddSoapEncodedPrimitive(Type type, string dataTypeName, string ns, string formatterName, XmlQualifiedName baseTypeName, TypeFlags flags)
        {
            AddNonXsdPrimitive(type, dataTypeName, ns, formatterName, baseTypeName, new XmlSchemaFacet[0], flags);
        }

        private static void AddSoapEncodedTypes(string ns)
        {
            AddSoapEncodedPrimitive(typeof(string), "normalizedString", ns, "String", new XmlQualifiedName("normalizedString", "http://www.w3.org/2001/XMLSchema"), TypeFlags.HasDefaultConstructor | TypeFlags.AmbiguousDataType | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            for (int i = 0; i < unsupportedTypes.Length; i++)
            {
                AddSoapEncodedPrimitive(typeof(string), unsupportedTypes[i], ns, "String", new XmlQualifiedName(unsupportedTypes[i], "http://www.w3.org/2001/XMLSchema"), TypeFlags.CollapseWhitespace | TypeFlags.AmbiguousDataType | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            }
            AddSoapEncodedPrimitive(typeof(string), "string", ns, "String", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(int), "int", ns, "Int32", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(bool), "boolean", ns, "Boolean", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(short), "short", ns, "Int16", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(long), "long", ns, "Int64", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(float), "float", ns, "Single", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(double), "double", ns, "Double", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(decimal), "decimal", ns, "Decimal", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(DateTime), "dateTime", ns, "DateTime", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(XmlQualifiedName), "QName", ns, "XmlQualifiedName", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.HasIsEmpty | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(byte), "unsignedByte", ns, "Byte", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(sbyte), "byte", ns, "SByte", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(ushort), "unsignedShort", ns, "UInt16", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(uint), "unsignedInt", ns, "UInt32", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(ulong), "unsignedLong", ns, "UInt64", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(DateTime), "date", ns, "Date", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(DateTime), "time", ns, "Time", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(string), "Name", ns, "XmlName", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(string), "NCName", ns, "XmlNCName", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(string), "NMTOKEN", ns, "XmlNmToken", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(string), "NMTOKENS", ns, "XmlNmTokens", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(byte[]), "base64Binary", ns, "ByteArrayBase64", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.IgnoreDefault | TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(byte[]), "hexBinary", ns, "ByteArrayHex", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.XmlEncodingNotRequired | TypeFlags.IgnoreDefault | TypeFlags.AmbiguousDataType | TypeFlags.HasCustomFormatter | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(string), "arrayCoordinate", ns, "String", new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"), TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            AddSoapEncodedPrimitive(typeof(byte[]), "base64", ns, "ByteArrayBase64", new XmlQualifiedName("base64Binary", "http://www.w3.org/2001/XMLSchema"), TypeFlags.IgnoreDefault | TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue | TypeFlags.Reference);
        }

        internal void AddTypeMapping(TypeMapping typeMapping)
        {
            this.typeMappings.Add(typeMapping);
        }

        internal static MemberMapping[] GetAllMembers(StructMapping mapping)
        {
            if (mapping.BaseMapping == null)
            {
                return mapping.Members;
            }
            ArrayList list = new ArrayList();
            GetAllMembers(mapping, list);
            return (MemberMapping[]) list.ToArray(typeof(MemberMapping));
        }

        internal static void GetAllMembers(StructMapping mapping, ArrayList list)
        {
            if (mapping.BaseMapping != null)
            {
                GetAllMembers(mapping.BaseMapping, list);
            }
            for (int i = 0; i < mapping.Members.Length; i++)
            {
                list.Add(mapping.Members[i]);
            }
        }

        internal static Type GetArrayElementType(Type type, string memberInfo)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            if (typeof(ICollection).IsAssignableFrom(type))
            {
                return GetCollectionElementType(type, memberInfo);
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                TypeFlags none = TypeFlags.None;
                return GetEnumeratorElementType(type, ref none);
            }
            return null;
        }

        internal TypeDesc GetArrayTypeDesc(Type type)
        {
            TypeDesc typeDesc = (TypeDesc) this.arrayTypeDescs[type];
            if (typeDesc == null)
            {
                typeDesc = this.GetTypeDesc(type);
                if (!typeDesc.IsArrayLike)
                {
                    typeDesc = this.ImportTypeDesc(type, null, false);
                }
                typeDesc.CheckSupported();
                this.arrayTypeDescs.Add(type, typeDesc);
            }
            return typeDesc;
        }

        private static Type GetCollectionElementType(Type type, string memberInfo)
        {
            return GetDefaultIndexer(type, memberInfo).PropertyType;
        }

        private static TypeFlags GetConstructorFlags(Type type, ref Exception exception)
        {
            ConstructorInfo info = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            if (info == null)
            {
                return TypeFlags.None;
            }
            TypeFlags hasDefaultConstructor = TypeFlags.HasDefaultConstructor;
            if (!info.IsPublic)
            {
                return (hasDefaultConstructor | TypeFlags.CtorInaccessible);
            }
            object[] customAttributes = info.GetCustomAttributes(typeof(ObsoleteAttribute), false);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                ObsoleteAttribute attribute = (ObsoleteAttribute) customAttributes[0];
                if (attribute.IsError)
                {
                    hasDefaultConstructor |= TypeFlags.CtorInaccessible;
                }
            }
            return hasDefaultConstructor;
        }

        internal static PropertyInfo GetDefaultIndexer(Type type, string memberInfo)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                if (memberInfo == null)
                {
                    throw new NotSupportedException(Res.GetString("XmlUnsupportedIDictionary", new object[] { type.FullName }));
                }
                throw new NotSupportedException(Res.GetString("XmlUnsupportedIDictionaryDetails", new object[] { memberInfo, type.FullName }));
            }
            MemberInfo[] defaultMembers = type.GetDefaultMembers();
            PropertyInfo info = null;
            if ((defaultMembers != null) && (defaultMembers.Length > 0))
            {
                for (Type type2 = type; type2 != null; type2 = type2.BaseType)
                {
                    for (int i = 0; i < defaultMembers.Length; i++)
                    {
                        if (defaultMembers[i] is PropertyInfo)
                        {
                            PropertyInfo info2 = (PropertyInfo) defaultMembers[i];
                            if (!(info2.DeclaringType != type2) && info2.CanRead)
                            {
                                ParameterInfo[] parameters = info2.GetGetMethod().GetParameters();
                                if ((parameters.Length == 1) && (parameters[0].ParameterType == typeof(int)))
                                {
                                    info = info2;
                                    break;
                                }
                            }
                        }
                    }
                    if (info != null)
                    {
                        break;
                    }
                }
            }
            if (info == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlNoDefaultAccessors", new object[] { type.FullName }));
            }
            if (type.GetMethod("Add", new Type[] { info.PropertyType }) == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlNoAddMethod", new object[] { type.FullName, info.PropertyType, "ICollection" }));
            }
            return info;
        }

        private static Type GetEnumeratorElementType(Type type, ref TypeFlags flags)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(type))
            {
                return null;
            }
            MethodInfo method = type.GetMethod("GetEnumerator", new Type[0]);
            if ((method == null) || !typeof(IEnumerator).IsAssignableFrom(method.ReturnType))
            {
                method = null;
                foreach (MemberInfo info2 in type.GetMember("System.Collections.Generic.IEnumerable<*", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    method = info2 as MethodInfo;
                    if ((method != null) && typeof(IEnumerator).IsAssignableFrom(method.ReturnType))
                    {
                        flags |= TypeFlags.GenericInterface;
                        break;
                    }
                    method = null;
                }
                if (method == null)
                {
                    flags |= TypeFlags.UsePrivateImplementation;
                    method = type.GetMethod("System.Collections.IEnumerable.GetEnumerator", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
                }
            }
            if ((method == null) || !typeof(IEnumerator).IsAssignableFrom(method.ReturnType))
            {
                return null;
            }
            XmlAttributes attributes = new XmlAttributes(method);
            if (attributes.XmlIgnore)
            {
                return null;
            }
            PropertyInfo property = method.ReturnType.GetProperty("Current");
            Type type2 = (property == null) ? typeof(object) : property.PropertyType;
            MethodInfo info4 = type.GetMethod("Add", new Type[] { type2 });
            if ((info4 == null) && (type2 != typeof(object)))
            {
                type2 = typeof(object);
                info4 = type.GetMethod("Add", new Type[] { type2 });
            }
            if (info4 == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlNoAddMethod", new object[] { type.FullName, type2, "IEnumerable" }));
            }
            return type2;
        }

        internal TypeDesc GetTypeDesc(Type type)
        {
            return this.GetTypeDesc(type, null, true, true);
        }

        internal TypeDesc GetTypeDesc(XmlSchemaSimpleType dataType)
        {
            return (TypeDesc) primitiveDataTypes[dataType];
        }

        internal TypeDesc GetTypeDesc(string name, string ns)
        {
            return this.GetTypeDesc(name, ns, TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.CanBeAttributeValue);
        }

        internal TypeDesc GetTypeDesc(Type type, MemberInfo source)
        {
            return this.GetTypeDesc(type, source, true, true);
        }

        internal TypeDesc GetTypeDesc(string name, string ns, TypeFlags flags)
        {
            TypeDesc desc = (TypeDesc) primitiveNames[name, ns];
            if ((desc != null) && ((desc.Flags & flags) != TypeFlags.None))
            {
                return desc;
            }
            return null;
        }

        internal TypeDesc GetTypeDesc(Type type, MemberInfo source, bool directReference)
        {
            return this.GetTypeDesc(type, source, directReference, true);
        }

        internal TypeDesc GetTypeDesc(Type type, MemberInfo source, bool directReference, bool throwOnError)
        {
            if (type.ContainsGenericParameters)
            {
                throw new InvalidOperationException(Res.GetString("XmlUnsupportedOpenGenericType", new object[] { type.ToString() }));
            }
            TypeDesc desc = (TypeDesc) primitiveTypes[type];
            if (desc == null)
            {
                desc = (TypeDesc) this.typeDescs[type];
                if (desc == null)
                {
                    desc = this.ImportTypeDesc(type, source, directReference);
                }
            }
            if (throwOnError)
            {
                desc.CheckSupported();
            }
            return desc;
        }

        internal Type GetTypeFromTypeDesc(TypeDesc typeDesc)
        {
            if (typeDesc.Type != null)
            {
                return typeDesc.Type;
            }
            foreach (DictionaryEntry entry in this.typeDescs)
            {
                if (entry.Value == typeDesc)
                {
                    return (entry.Key as Type);
                }
            }
            return null;
        }

        internal TypeMapping GetTypeMappingFromTypeDesc(TypeDesc typeDesc)
        {
            foreach (TypeMapping mapping in this.TypeMappings)
            {
                if (mapping.TypeDesc == typeDesc)
                {
                    return mapping;
                }
            }
            return null;
        }

        private TypeDesc ImportTypeDesc(Type type, MemberInfo memberInfo, bool directReference)
        {
            TypeDesc desc = null;
            TypeKind root;
            Type elementType = null;
            Type baseType = null;
            TypeFlags none = TypeFlags.None;
            Exception exception = null;
            if (!type.IsPublic && !type.IsNestedPublic)
            {
                none |= TypeFlags.Unsupported;
                exception = new InvalidOperationException(Res.GetString("XmlTypeInaccessible", new object[] { type.FullName }));
            }
            else if (type.IsAbstract && type.IsSealed)
            {
                none |= TypeFlags.Unsupported;
                exception = new InvalidOperationException(Res.GetString("XmlTypeStatic", new object[] { type.FullName }));
            }
            if (DynamicAssemblies.IsTypeDynamic(type))
            {
                none |= TypeFlags.UseReflection;
            }
            if (!type.IsValueType)
            {
                none |= TypeFlags.Reference;
            }
            if (type == typeof(object))
            {
                root = TypeKind.Root;
                none |= TypeFlags.HasDefaultConstructor;
            }
            else if (type == typeof(ValueType))
            {
                root = TypeKind.Enum;
                none |= TypeFlags.Unsupported;
                if (exception == null)
                {
                    exception = new NotSupportedException(Res.GetString("XmlSerializerUnsupportedType", new object[] { type.FullName }));
                }
            }
            else if (type == typeof(void))
            {
                root = TypeKind.Void;
            }
            else if (typeof(IXmlSerializable).IsAssignableFrom(type))
            {
                root = TypeKind.Serializable;
                none |= TypeFlags.CanBeElementValue | TypeFlags.Special;
                none |= GetConstructorFlags(type, ref exception);
            }
            else if (type.IsArray)
            {
                root = TypeKind.Array;
                if (type.GetArrayRank() > 1)
                {
                    none |= TypeFlags.Unsupported;
                    if (exception == null)
                    {
                        exception = new NotSupportedException(Res.GetString("XmlUnsupportedRank", new object[] { type.FullName }));
                    }
                }
                elementType = type.GetElementType();
                none |= TypeFlags.HasDefaultConstructor;
            }
            else if (typeof(ICollection).IsAssignableFrom(type))
            {
                root = TypeKind.Collection;
                elementType = GetCollectionElementType(type, (memberInfo == null) ? null : (memberInfo.DeclaringType.FullName + "." + memberInfo.Name));
                none |= GetConstructorFlags(type, ref exception);
            }
            else if (type == typeof(XmlQualifiedName))
            {
                root = TypeKind.Primitive;
            }
            else if (type.IsPrimitive)
            {
                root = TypeKind.Primitive;
                none |= TypeFlags.Unsupported;
                if (exception == null)
                {
                    exception = new NotSupportedException(Res.GetString("XmlSerializerUnsupportedType", new object[] { type.FullName }));
                }
            }
            else if (type.IsEnum)
            {
                root = TypeKind.Enum;
            }
            else if (type.IsValueType)
            {
                root = TypeKind.Struct;
                if (IsOptionalValue(type))
                {
                    baseType = type.GetGenericArguments()[0];
                    none |= TypeFlags.OptionalValue;
                }
                else
                {
                    baseType = type.BaseType;
                }
                if (type.IsAbstract)
                {
                    none |= TypeFlags.Abstract;
                }
            }
            else if (type.IsClass)
            {
                if (type == typeof(XmlAttribute))
                {
                    root = TypeKind.Attribute;
                    none |= TypeFlags.CanBeAttributeValue | TypeFlags.Special;
                }
                else if (typeof(XmlNode).IsAssignableFrom(type))
                {
                    root = TypeKind.Node;
                    baseType = type.BaseType;
                    none |= TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.Special;
                    if (typeof(XmlText).IsAssignableFrom(type))
                    {
                        none &= ~TypeFlags.CanBeElementValue;
                    }
                    else if (typeof(XmlElement).IsAssignableFrom(type))
                    {
                        none &= ~TypeFlags.CanBeTextValue;
                    }
                    else if (type.IsAssignableFrom(typeof(XmlAttribute)))
                    {
                        none |= TypeFlags.CanBeAttributeValue;
                    }
                }
                else
                {
                    root = TypeKind.Class;
                    baseType = type.BaseType;
                    if (type.IsAbstract)
                    {
                        none |= TypeFlags.Abstract;
                    }
                }
            }
            else if (type.IsInterface)
            {
                root = TypeKind.Void;
                none |= TypeFlags.Unsupported;
                if (exception == null)
                {
                    if (memberInfo == null)
                    {
                        exception = new NotSupportedException(Res.GetString("XmlUnsupportedInterface", new object[] { type.FullName }));
                    }
                    else
                    {
                        exception = new NotSupportedException(Res.GetString("XmlUnsupportedInterfaceDetails", new object[] { memberInfo.DeclaringType.FullName + "." + memberInfo.Name, type.FullName }));
                    }
                }
            }
            else
            {
                root = TypeKind.Void;
                none |= TypeFlags.Unsupported;
                if (exception == null)
                {
                    exception = new NotSupportedException(Res.GetString("XmlSerializerUnsupportedType", new object[] { type.FullName }));
                }
            }
            if ((root == TypeKind.Class) && !type.IsAbstract)
            {
                none |= GetConstructorFlags(type, ref exception);
            }
            if (((root == TypeKind.Struct) || (root == TypeKind.Class)) && typeof(IEnumerable).IsAssignableFrom(type))
            {
                elementType = GetEnumeratorElementType(type, ref none);
                root = TypeKind.Enumerable;
                none |= GetConstructorFlags(type, ref exception);
            }
            desc = new TypeDesc(type, CodeIdentifier.MakeValid(TypeName(type)), type.ToString(), root, null, none, null) {
                Exception = exception
            };
            if (directReference && (desc.IsClass || (root == TypeKind.Serializable)))
            {
                desc.CheckNeedConstructor();
            }
            if (!desc.IsUnsupported)
            {
                this.typeDescs.Add(type, desc);
                if (elementType != null)
                {
                    TypeDesc desc2 = this.GetTypeDesc(elementType, memberInfo, true, false);
                    if ((directReference && (desc2.IsCollection || desc2.IsEnumerable)) && !desc2.IsPrimitive)
                    {
                        desc2.CheckNeedConstructor();
                    }
                    desc.ArrayElementTypeDesc = desc2;
                }
                if (((baseType != null) && (baseType != typeof(object))) && (baseType != typeof(ValueType)))
                {
                    desc.BaseTypeDesc = this.GetTypeDesc(baseType, memberInfo, false, false);
                }
                if (type.IsNestedPublic)
                {
                    for (Type type4 = type.DeclaringType; (type4 != null) && !type4.ContainsGenericParameters; type4 = type4.DeclaringType)
                    {
                        this.GetTypeDesc(type4, null, false);
                    }
                }
            }
            return desc;
        }

        internal static bool IsKnownType(Type type)
        {
            if (type == typeof(object))
            {
                return true;
            }
            if (type.IsEnum)
            {
                return false;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return true;

                case TypeCode.Char:
                    return true;

                case TypeCode.SByte:
                    return true;

                case TypeCode.Byte:
                    return true;

                case TypeCode.Int16:
                    return true;

                case TypeCode.UInt16:
                    return true;

                case TypeCode.Int32:
                    return true;

                case TypeCode.UInt32:
                    return true;

                case TypeCode.Int64:
                    return true;

                case TypeCode.UInt64:
                    return true;

                case TypeCode.Single:
                    return true;

                case TypeCode.Double:
                    return true;

                case TypeCode.Decimal:
                    return true;

                case TypeCode.DateTime:
                    return true;

                case TypeCode.String:
                    return true;
            }
            return ((type == typeof(XmlQualifiedName)) || ((type == typeof(byte[])) || ((type == typeof(Guid)) || (type == typeof(XmlNode[])))));
        }

        internal static bool IsOptionalValue(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition()));
        }

        internal static XmlQualifiedName ParseWsdlArrayType(string type, out string dims, XmlSchemaObject parent)
        {
            string str;
            int length = type.LastIndexOf(':');
            if (length <= 0)
            {
                str = "";
            }
            else
            {
                str = type.Substring(0, length);
            }
            int index = type.IndexOf('[', length + 1);
            if (index <= length)
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidArrayTypeSyntax", new object[] { type }));
            }
            string name = type.Substring(length + 1, (index - length) - 1);
            dims = type.Substring(index);
            while (parent != null)
            {
                if (parent.Namespaces != null)
                {
                    string str3 = (string) parent.Namespaces.Namespaces[str];
                    if (str3 != null)
                    {
                        str = str3;
                        break;
                    }
                }
                parent = parent.Parent;
            }
            return new XmlQualifiedName(name, str);
        }

        internal static string TypeName(Type t)
        {
            if (t.IsArray)
            {
                return ("ArrayOf" + TypeName(t.GetElementType()));
            }
            if (!t.IsGenericType)
            {
                return t.Name;
            }
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            string name = t.Name;
            int index = name.IndexOf("`", StringComparison.Ordinal);
            if (index >= 0)
            {
                name = name.Substring(0, index);
            }
            builder.Append(name);
            builder.Append("Of");
            Type[] genericArguments = t.GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                builder.Append(TypeName(genericArguments[i]));
                builder2.Append(genericArguments[i].Namespace);
            }
            return builder.ToString();
        }

        internal static Hashtable PrimtiveTypes
        {
            get
            {
                return primitiveTypes;
            }
        }

        internal ICollection TypeMappings
        {
            get
            {
                return this.typeMappings;
            }
        }

        internal ICollection Types
        {
            get
            {
                return this.typeDescs.Keys;
            }
        }
    }
}

