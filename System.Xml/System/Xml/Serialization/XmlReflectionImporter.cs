namespace System.Xml.Serialization
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;

    public class XmlReflectionImporter
    {
        private Hashtable anonymous;
        private int arrayNestingLevel;
        private XmlAttributeOverrides attributeOverrides;
        private int choiceNum;
        private XmlAttributes defaultAttributes;
        private string defaultNs;
        private System.Xml.Serialization.NameTable elements;
        private ModelScope modelScope;
        private System.Xml.Serialization.NameTable nullables;
        private StructMapping root;
        private XmlArrayItemAttributes savedArrayItemAttributes;
        private string savedArrayNamespace;
        private System.Xml.Serialization.NameTable serializables;
        private Hashtable specials;
        private System.Xml.Serialization.NameTable types;
        private TypeScope typeScope;
        private System.Xml.Serialization.NameTable xsdAttributes;

        public XmlReflectionImporter() : this(null, null)
        {
        }

        public XmlReflectionImporter(string defaultNamespace) : this(null, defaultNamespace)
        {
        }

        public XmlReflectionImporter(XmlAttributeOverrides attributeOverrides) : this(attributeOverrides, null)
        {
        }

        public XmlReflectionImporter(XmlAttributeOverrides attributeOverrides, string defaultNamespace)
        {
            this.defaultAttributes = new XmlAttributes();
            this.types = new System.Xml.Serialization.NameTable();
            this.nullables = new System.Xml.Serialization.NameTable();
            this.elements = new System.Xml.Serialization.NameTable();
            this.anonymous = new Hashtable();
            this.choiceNum = 1;
            if (defaultNamespace == null)
            {
                defaultNamespace = string.Empty;
            }
            if (attributeOverrides == null)
            {
                attributeOverrides = new XmlAttributeOverrides();
            }
            this.attributeOverrides = attributeOverrides;
            this.defaultNs = defaultNamespace;
            this.typeScope = new TypeScope();
            this.modelScope = new ModelScope(this.typeScope);
        }

        private static void AddUniqueAccessor(INameScope scope, Accessor accessor)
        {
            Accessor accessor2 = (Accessor) scope[accessor.Name, accessor.Namespace];
            if (accessor2 != null)
            {
                if (accessor is ElementAccessor)
                {
                    throw new InvalidOperationException(Res.GetString("XmlDuplicateElementName", new object[] { accessor2.Name, accessor2.Namespace }));
                }
                throw new InvalidOperationException(Res.GetString("XmlDuplicateAttributeName", new object[] { accessor2.Name, accessor2.Namespace }));
            }
            scope[accessor.Name, accessor.Namespace] = accessor;
        }

        private static void AddUniqueAccessor(MemberMapping member, INameScope elements, INameScope attributes, bool isSequence)
        {
            if (member.Attribute != null)
            {
                AddUniqueAccessor(attributes, member.Attribute);
            }
            else if ((!isSequence && (member.Elements != null)) && (member.Elements.Length > 0))
            {
                for (int i = 0; i < member.Elements.Length; i++)
                {
                    AddUniqueAccessor(elements, member.Elements[i]);
                }
            }
        }

        private void CheckAmbiguousChoice(XmlAttributes a, Type accessorType, string accessorName)
        {
            Hashtable hashtable = new Hashtable();
            XmlElementAttributes xmlElements = a.XmlElements;
            if (((xmlElements != null) && (xmlElements.Count >= 2)) && (a.XmlChoiceIdentifier == null))
            {
                for (int i = 0; i < xmlElements.Count; i++)
                {
                    Type key = (xmlElements[i].Type == null) ? accessorType : xmlElements[i].Type;
                    if (hashtable.Contains(key))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlChoiceIdentiferMissing", new object[] { typeof(XmlChoiceIdentifierAttribute).Name, accessorName }));
                    }
                    hashtable.Add(key, false);
                }
            }
            if (hashtable.Contains(typeof(XmlElement)) && (a.XmlAnyElements.Count > 0))
            {
                throw new InvalidOperationException(Res.GetString("XmlChoiceIdentiferMissing", new object[] { typeof(XmlChoiceIdentifierAttribute).Name, accessorName }));
            }
            XmlArrayItemAttributes xmlArrayItems = a.XmlArrayItems;
            if ((xmlArrayItems != null) && (xmlArrayItems.Count >= 2))
            {
                System.Xml.Serialization.NameTable table = new System.Xml.Serialization.NameTable();
                for (int j = 0; j < xmlArrayItems.Count; j++)
                {
                    Type type2 = (xmlArrayItems[j].Type == null) ? accessorType : xmlArrayItems[j].Type;
                    string str = xmlArrayItems[j].NestingLevel.ToString(CultureInfo.InvariantCulture);
                    XmlArrayItemAttribute attribute = (XmlArrayItemAttribute) table[type2.FullName, str];
                    if (attribute != null)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlArrayItemAmbiguousTypes", new object[] { accessorName, attribute.ElementName, xmlArrayItems[j].ElementName, typeof(XmlElementAttribute).Name, typeof(XmlChoiceIdentifierAttribute).Name, accessorName }));
                    }
                    table[type2.FullName, str] = xmlArrayItems[j];
                }
            }
        }

        private void CheckChoiceIdentifierMapping(EnumMapping choiceMapping)
        {
            System.Xml.Serialization.NameTable table = new System.Xml.Serialization.NameTable();
            for (int i = 0; i < choiceMapping.Constants.Length; i++)
            {
                string xmlName = choiceMapping.Constants[i].XmlName;
                int length = xmlName.LastIndexOf(':');
                string name = (length < 0) ? xmlName : xmlName.Substring(length + 1);
                string ns = (length < 0) ? "" : xmlName.Substring(0, length);
                if (table[name, ns] != null)
                {
                    throw new InvalidOperationException(Res.GetString("XmlChoiceIdDuplicate", new object[] { choiceMapping.TypeName, xmlName }));
                }
                table.Add(name, ns, choiceMapping.Constants[i]);
            }
        }

        private Type CheckChoiceIdentifierType(Type type, bool isArrayLike, string identifierName, string memberName)
        {
            if (type.IsArray)
            {
                if (!isArrayLike)
                {
                    throw new InvalidOperationException(Res.GetString("XmlChoiceIdentifierType", new object[] { identifierName, memberName, type.GetElementType().FullName }));
                }
                type = type.GetElementType();
            }
            else if (isArrayLike)
            {
                throw new InvalidOperationException(Res.GetString("XmlChoiceIdentifierArrayType", new object[] { identifierName, memberName, type.FullName }));
            }
            if (!type.IsEnum)
            {
                throw new InvalidOperationException(Res.GetString("XmlChoiceIdentifierTypeEnum", new object[] { identifierName }));
            }
            return type;
        }

        private void CheckContext(TypeDesc typeDesc, ImportContext context)
        {
            switch (context)
            {
                case ImportContext.Text:
                    if ((!typeDesc.CanBeTextValue && !typeDesc.IsEnum) && !typeDesc.IsPrimitive)
                    {
                        break;
                    }
                    return;

                case ImportContext.Attribute:
                    if (!typeDesc.CanBeAttributeValue)
                    {
                        break;
                    }
                    return;

                case ImportContext.Element:
                    if (!typeDesc.CanBeElementValue)
                    {
                        break;
                    }
                    return;

                default:
                    throw new ArgumentException(Res.GetString("XmlInternalError"), "context");
            }
            throw UnsupportedException(typeDesc, context);
        }

        private static void CheckForm(XmlSchemaForm form, bool isQualified)
        {
            if (isQualified && (form == XmlSchemaForm.Unqualified))
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidFormUnqualified"));
            }
        }

        private static void CheckNullable(bool isNullable, TypeDesc typeDesc, TypeMapping mapping)
        {
            if ((!(mapping is NullableMapping) && !(mapping is SerializableMapping)) && (isNullable && !typeDesc.IsNullable))
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidIsNullable", new object[] { typeDesc.FullName }));
            }
        }

        private void CheckTopLevelAttributes(XmlAttributes a, string accessorName)
        {
            XmlAttributeFlags xmlFlags = a.XmlFlags;
            if ((xmlFlags & (XmlAttributeFlags.AnyAttribute | XmlAttributeFlags.Attribute)) != ((XmlAttributeFlags) 0))
            {
                throw new InvalidOperationException(Res.GetString("XmlRpcLitAttributeAttributes"));
            }
            if ((xmlFlags & (XmlAttributeFlags.ChoiceIdentifier | XmlAttributeFlags.AnyElements | XmlAttributeFlags.Text)) != ((XmlAttributeFlags) 0))
            {
                throw new InvalidOperationException(Res.GetString("XmlRpcLitAttributes"));
            }
            if ((a.XmlElements != null) && (a.XmlElements.Count > 0))
            {
                if (a.XmlElements.Count > 1)
                {
                    throw new InvalidOperationException(Res.GetString("XmlRpcLitElements"));
                }
                XmlElementAttribute attribute = a.XmlElements[0];
                if (attribute.Namespace != null)
                {
                    throw new InvalidOperationException(Res.GetString("XmlRpcLitElementNamespace", new object[] { "Namespace", attribute.Namespace }));
                }
                if (attribute.IsNullable)
                {
                    throw new InvalidOperationException(Res.GetString("XmlRpcLitElementNullable", new object[] { "IsNullable", "true" }));
                }
            }
            if ((a.XmlArray != null) && (a.XmlArray.Namespace != null))
            {
                throw new InvalidOperationException(Res.GetString("XmlRpcLitElementNamespace", new object[] { "Namespace", a.XmlArray.Namespace }));
            }
        }

        private static int CountAtLevel(XmlArrayItemAttributes attributes, int level)
        {
            int num = 0;
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].NestingLevel == level)
                {
                    num++;
                }
            }
            return num;
        }

        private static XmlArrayAttribute CreateArrayAttribute(TypeDesc typeDesc)
        {
            return new XmlArrayAttribute();
        }

        private void CreateArrayElementsFromAttributes(ArrayMapping arrayMapping, XmlArrayItemAttributes attributes, Type arrayElementType, string arrayElementNs, RecursionLimiter limiter)
        {
            System.Xml.Serialization.NameTable scope = new System.Xml.Serialization.NameTable();
            for (int i = 0; (attributes != null) && (i < attributes.Count); i++)
            {
                XmlArrayItemAttribute attribute = attributes[i];
                if (attribute.NestingLevel == this.arrayNestingLevel)
                {
                    ElementAccessor accessor;
                    Type type = (attribute.Type != null) ? attribute.Type : arrayElementType;
                    TypeDesc typeDesc = this.typeScope.GetTypeDesc(type);
                    accessor = new ElementAccessor {
                        Namespace = (attribute.Namespace == null) ? arrayElementNs : attribute.Namespace,
                        Mapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(type), accessor.Namespace, ImportContext.Element, attribute.DataType, null, limiter),
                        Name = (attribute.ElementName.Length == 0) ? accessor.Mapping.DefaultElementName : XmlConvert.EncodeLocalName(attribute.ElementName),
                        IsNullable = attribute.IsNullableSpecified ? attribute.IsNullable : (typeDesc.IsNullable || typeDesc.IsOptionalValue),
                        Form = (attribute.Form == XmlSchemaForm.None) ? XmlSchemaForm.Qualified : attribute.Form
                    };
                    CheckForm(accessor.Form, arrayElementNs != accessor.Namespace);
                    CheckNullable(accessor.IsNullable, typeDesc, accessor.Mapping);
                    AddUniqueAccessor(scope, accessor);
                }
            }
            arrayMapping.Elements = (ElementAccessor[]) scope.ToArray(typeof(ElementAccessor));
        }

        private static XmlArrayItemAttribute CreateArrayItemAttribute(TypeDesc typeDesc, int nestingLevel)
        {
            return new XmlArrayItemAttribute { NestingLevel = nestingLevel };
        }

        private static ElementAccessor CreateElementAccessor(TypeMapping mapping, string ns)
        {
            ElementAccessor accessor = new ElementAccessor();
            bool isAny = mapping.TypeDesc.Kind == TypeKind.Node;
            if (!isAny && (mapping is SerializableMapping))
            {
                isAny = ((SerializableMapping) mapping).IsAny;
            }
            if (isAny)
            {
                accessor.Any = true;
            }
            else
            {
                accessor.Name = mapping.DefaultElementName;
                accessor.Namespace = ns;
            }
            accessor.Mapping = mapping;
            return accessor;
        }

        private static XmlElementAttribute CreateElementAttribute(TypeDesc typeDesc)
        {
            return new XmlElementAttribute { IsNullable = typeDesc.IsOptionalValue };
        }

        private Exception CreateMemberReflectionException(FieldModel model, Exception e)
        {
            return new InvalidOperationException(Res.GetString(model.IsProperty ? "XmlPropertyReflectionError" : "XmlFieldReflectionError", new object[] { model.Name }), e);
        }

        private NullableMapping CreateNullableMapping(TypeMapping baseMapping, Type type)
        {
            TypeMapping mapping;
            NullableMapping mapping2;
            TypeDesc nullableTypeDesc = baseMapping.TypeDesc.GetNullableTypeDesc(type);
            if (!baseMapping.IsAnonymousType)
            {
                mapping = (TypeMapping) this.nullables[baseMapping.TypeName, baseMapping.Namespace];
            }
            else
            {
                mapping = (TypeMapping) this.anonymous[type];
            }
            if (mapping != null)
            {
                if (!(mapping is NullableMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlTypesDuplicate", new object[] { nullableTypeDesc.FullName, mapping.TypeDesc.FullName, nullableTypeDesc.Name, mapping.Namespace }));
                }
                mapping2 = (NullableMapping) mapping;
                if ((!(mapping2.BaseMapping is PrimitiveMapping) || !(baseMapping is PrimitiveMapping)) && (mapping2.BaseMapping != baseMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlTypesDuplicate", new object[] { nullableTypeDesc.FullName, mapping.TypeDesc.FullName, nullableTypeDesc.Name, mapping.Namespace }));
                }
                return mapping2;
            }
            mapping2 = new NullableMapping {
                BaseMapping = baseMapping,
                TypeDesc = nullableTypeDesc,
                TypeName = baseMapping.TypeName,
                Namespace = baseMapping.Namespace,
                IncludeInSchema = baseMapping.IncludeInSchema
            };
            if (!baseMapping.IsAnonymousType)
            {
                this.nullables.Add(baseMapping.TypeName, baseMapping.Namespace, mapping2);
            }
            else
            {
                this.anonymous[type] = mapping2;
            }
            this.typeScope.AddTypeMapping(mapping2);
            return mapping2;
        }

        private Exception CreateReflectionException(string context, Exception e)
        {
            return new InvalidOperationException(Res.GetString("XmlReflectionError", new object[] { context }), e);
        }

        private StructMapping CreateRootMapping()
        {
            TypeDesc typeDesc = this.typeScope.GetTypeDesc(typeof(object));
            return new StructMapping { TypeDesc = typeDesc, TypeName = "anyType", Namespace = "http://www.w3.org/2001/XMLSchema", Members = new MemberMapping[0], IncludeInSchema = false };
        }

        private Exception CreateTypeReflectionException(string context, Exception e)
        {
            return new InvalidOperationException(Res.GetString("XmlTypeReflectionError", new object[] { context }), e);
        }

        internal static XmlReflectionMember FindSpecifiedMember(string memberName, XmlReflectionMember[] reflectionMembers)
        {
            for (int i = 0; i < reflectionMembers.Length; i++)
            {
                if (string.Compare(reflectionMembers[i].MemberName, memberName + "Specified", StringComparison.Ordinal) == 0)
                {
                    return reflectionMembers[i];
                }
            }
            return null;
        }

        private XmlAttributes GetAttributes(MemberInfo memberInfo)
        {
            XmlAttributes attributes = this.attributeOverrides[memberInfo.DeclaringType, memberInfo.Name];
            if (attributes != null)
            {
                return attributes;
            }
            return new XmlAttributes(memberInfo);
        }

        private XmlAttributes GetAttributes(Type type, bool canBeSimpleType)
        {
            XmlAttributes attributes = this.attributeOverrides[type];
            if (attributes != null)
            {
                return attributes;
            }
            if (canBeSimpleType && TypeScope.IsKnownType(type))
            {
                return this.defaultAttributes;
            }
            return new XmlAttributes(type);
        }

        private Type GetChoiceIdentifierType(XmlChoiceIdentifierAttribute choice, XmlReflectionMember[] xmlReflectionMembers, bool isArrayLike, string accessorName)
        {
            for (int i = 0; i < xmlReflectionMembers.Length; i++)
            {
                if (choice.MemberName == xmlReflectionMembers[i].MemberName)
                {
                    return this.CheckChoiceIdentifierType(xmlReflectionMembers[i].MemberType, isArrayLike, choice.MemberName, accessorName);
                }
            }
            throw new InvalidOperationException(Res.GetString("XmlChoiceIdentiferMemberMissing", new object[] { choice.MemberName, accessorName }));
        }

        private Type GetChoiceIdentifierType(XmlChoiceIdentifierAttribute choice, StructModel structModel, bool isArrayLike, string accessorName)
        {
            MemberInfo[] member = structModel.Type.GetMember(choice.MemberName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if ((member == null) || (member.Length == 0))
            {
                PropertyInfo property = structModel.Type.GetProperty(choice.MemberName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property == null)
                {
                    throw new InvalidOperationException(Res.GetString("XmlChoiceIdentiferMemberMissing", new object[] { choice.MemberName, accessorName }));
                }
                member = new MemberInfo[] { property };
            }
            else if (member.Length > 1)
            {
                throw new InvalidOperationException(Res.GetString("XmlChoiceIdentiferAmbiguous", new object[] { choice.MemberName }));
            }
            FieldModel fieldModel = structModel.GetFieldModel(member[0]);
            if (fieldModel == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlChoiceIdentiferMemberMissing", new object[] { choice.MemberName, accessorName }));
            }
            Type fieldType = fieldModel.FieldType;
            return this.CheckChoiceIdentifierType(fieldType, isArrayLike, choice.MemberName, accessorName);
        }

        private static string GetContextName(ImportContext context)
        {
            switch (context)
            {
                case ImportContext.Text:
                    return "text";

                case ImportContext.Attribute:
                    return "attribute";

                case ImportContext.Element:
                    return "element";
            }
            throw new ArgumentException(Res.GetString("XmlInternalError"), "context");
        }

        private object GetDefaultValue(TypeDesc fieldTypeDesc, Type t, XmlAttributes a)
        {
            if ((a.XmlDefaultValue == null) || (a.XmlDefaultValue == DBNull.Value))
            {
                return null;
            }
            if ((fieldTypeDesc.Kind != TypeKind.Primitive) && (fieldTypeDesc.Kind != TypeKind.Enum))
            {
                a.XmlDefaultValue = null;
                return a.XmlDefaultValue;
            }
            if (fieldTypeDesc.Kind != TypeKind.Enum)
            {
                return a.XmlDefaultValue;
            }
            string str = Enum.Format(t, a.XmlDefaultValue, "G").Replace(",", " ");
            string str2 = Enum.Format(t, a.XmlDefaultValue, "D");
            if (str == str2)
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidDefaultValue", new object[] { str, a.XmlDefaultValue.GetType().FullName }));
            }
            return str;
        }

        private static string GetMappingName(Mapping mapping)
        {
            if (mapping is MembersMapping)
            {
                return "(method)";
            }
            if (!(mapping is TypeMapping))
            {
                throw new ArgumentException(Res.GetString("XmlInternalError"), "mapping");
            }
            return ((TypeMapping) mapping).TypeDesc.FullName;
        }

        internal static MethodInfo GetMethodFromSchemaProvider(XmlSchemaProviderAttribute provider, Type type)
        {
            MethodInfo info;
            if (provider.IsAny)
            {
                return null;
            }
            if (provider.MethodName == null)
            {
                throw new ArgumentNullException("MethodName");
            }
            if (!CodeGenerator.IsValidLanguageIndependentIdentifier(provider.MethodName))
            {
                throw new ArgumentException(Res.GetString("XmlGetSchemaMethodName", new object[] { provider.MethodName }), "MethodName");
            }
            info = info = type.GetMethod(provider.MethodName, BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(XmlSchemaSet) }, null);
            if (info == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlGetSchemaMethodMissing", new object[] { provider.MethodName, typeof(XmlSchemaSet).Name, type.FullName }));
            }
            if (!typeof(XmlQualifiedName).IsAssignableFrom(info.ReturnType) && !typeof(XmlSchemaType).IsAssignableFrom(info.ReturnType))
            {
                throw new InvalidOperationException(Res.GetString("XmlGetSchemaMethodReturnType", new object[] { type.Name, provider.MethodName, typeof(XmlSchemaProviderAttribute).Name, typeof(XmlQualifiedName).FullName, typeof(XmlSchemaType).FullName }));
            }
            return info;
        }

        private StructMapping GetRootMapping()
        {
            if (this.root == null)
            {
                this.root = this.CreateRootMapping();
                this.typeScope.AddTypeMapping(this.root);
            }
            return this.root;
        }

        internal static XmlTypeMapping GetTopLevelMapping(Type type, string defaultNamespace)
        {
            XmlAttributes attributes = new XmlAttributes(type);
            TypeDesc typeDesc = new TypeScope().GetTypeDesc(type);
            ElementAccessor accessor = new ElementAccessor();
            if (typeDesc.Kind == TypeKind.Node)
            {
                accessor.Any = true;
            }
            else
            {
                string str = (attributes.XmlRoot == null) ? defaultNamespace : attributes.XmlRoot.Namespace;
                string name = string.Empty;
                if (attributes.XmlType != null)
                {
                    name = attributes.XmlType.TypeName;
                }
                if (name.Length == 0)
                {
                    name = type.Name;
                }
                accessor.Name = XmlConvert.EncodeLocalName(name);
                accessor.Namespace = str;
            }
            XmlTypeMapping mapping = new XmlTypeMapping(null, accessor);
            mapping.SetKeyInternal(XmlMapping.GenerateKey(type, attributes.XmlRoot, defaultNamespace));
            return mapping;
        }

        private TypeMapping GetTypeMapping(string typeName, string ns, TypeDesc typeDesc, System.Xml.Serialization.NameTable typeLib, Type type)
        {
            TypeMapping mapping;
            if ((typeName == null) || (typeName.Length == 0))
            {
                mapping = (type == null) ? null : ((TypeMapping) this.anonymous[type]);
            }
            else
            {
                mapping = (TypeMapping) typeLib[typeName, ns];
            }
            if (mapping == null)
            {
                return null;
            }
            if (!mapping.IsAnonymousType && (mapping.TypeDesc != typeDesc))
            {
                throw new InvalidOperationException(Res.GetString("XmlTypesDuplicate", new object[] { typeDesc.FullName, mapping.TypeDesc.FullName, typeName, ns }));
            }
            return mapping;
        }

        private void ImportAccessorMapping(MemberMapping accessor, FieldModel model, XmlAttributes a, string ns, Type choiceIdentifierType, bool rpc, bool openModel, RecursionLimiter limiter)
        {
            XmlSchemaForm qualified = XmlSchemaForm.Qualified;
            int arrayNestingLevel = this.arrayNestingLevel;
            int order = -1;
            XmlArrayItemAttributes savedArrayItemAttributes = this.savedArrayItemAttributes;
            string savedArrayNamespace = this.savedArrayNamespace;
            this.arrayNestingLevel = 0;
            this.savedArrayItemAttributes = null;
            this.savedArrayNamespace = null;
            Type fieldType = model.FieldType;
            string name = model.Name;
            ArrayList list = new ArrayList();
            System.Xml.Serialization.NameTable scope = new System.Xml.Serialization.NameTable();
            accessor.TypeDesc = this.typeScope.GetTypeDesc(fieldType);
            XmlAttributeFlags xmlFlags = a.XmlFlags;
            accessor.Ignore = a.XmlIgnore;
            if (rpc)
            {
                this.CheckTopLevelAttributes(a, name);
            }
            else
            {
                this.CheckAmbiguousChoice(a, fieldType, name);
            }
            XmlAttributeFlags flags2 = XmlAttributeFlags.ChoiceIdentifier | XmlAttributeFlags.AnyElements | XmlAttributeFlags.Elements | XmlAttributeFlags.Text;
            XmlAttributeFlags flags3 = XmlAttributeFlags.AnyAttribute | XmlAttributeFlags.Attribute;
            XmlAttributeFlags flags4 = XmlAttributeFlags.ArrayItems | XmlAttributeFlags.Array;
            if (((xmlFlags & flags4) != ((XmlAttributeFlags) 0)) && (fieldType == typeof(byte[])))
            {
                accessor.TypeDesc = this.typeScope.GetArrayTypeDesc(fieldType);
            }
            if (a.XmlChoiceIdentifier != null)
            {
                accessor.ChoiceIdentifier = new ChoiceIdentifierAccessor();
                accessor.ChoiceIdentifier.MemberName = a.XmlChoiceIdentifier.MemberName;
                accessor.ChoiceIdentifier.Mapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(choiceIdentifierType), ns, ImportContext.Element, string.Empty, null, limiter);
                this.CheckChoiceIdentifierMapping((EnumMapping) accessor.ChoiceIdentifier.Mapping);
            }
            if (accessor.TypeDesc.IsArrayLike)
            {
                Type arrayElementType = TypeScope.GetArrayElementType(fieldType, model.FieldTypeDesc.FullName + "." + model.Name);
                if ((xmlFlags & flags3) != ((XmlAttributeFlags) 0))
                {
                    if ((xmlFlags & flags3) != xmlFlags)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlIllegalAttributesArrayAttribute"));
                    }
                    if (((a.XmlAttribute != null) && !accessor.TypeDesc.ArrayElementTypeDesc.IsPrimitive) && !accessor.TypeDesc.ArrayElementTypeDesc.IsEnum)
                    {
                        if (accessor.TypeDesc.ArrayElementTypeDesc.Kind == TypeKind.Serializable)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlIllegalAttrOrTextInterface", new object[] { name, accessor.TypeDesc.ArrayElementTypeDesc.FullName, typeof(IXmlSerializable).Name }));
                        }
                        throw new InvalidOperationException(Res.GetString("XmlIllegalAttrOrText", new object[] { name, accessor.TypeDesc.ArrayElementTypeDesc.FullName }));
                    }
                    bool repeats = (a.XmlAttribute != null) && (accessor.TypeDesc.ArrayElementTypeDesc.IsPrimitive || accessor.TypeDesc.ArrayElementTypeDesc.IsEnum);
                    if (a.XmlAnyAttribute != null)
                    {
                        a.XmlAttribute = new XmlAttributeAttribute();
                    }
                    AttributeAccessor accessor2 = new AttributeAccessor();
                    Type type = (a.XmlAttribute.Type == null) ? arrayElementType : a.XmlAttribute.Type;
                    this.typeScope.GetTypeDesc(type);
                    accessor2.Name = Accessor.EscapeQName((a.XmlAttribute.AttributeName.Length == 0) ? name : a.XmlAttribute.AttributeName);
                    accessor2.Namespace = (a.XmlAttribute.Namespace == null) ? ns : a.XmlAttribute.Namespace;
                    accessor2.Form = a.XmlAttribute.Form;
                    if ((accessor2.Form == XmlSchemaForm.None) && (ns != accessor2.Namespace))
                    {
                        accessor2.Form = XmlSchemaForm.Qualified;
                    }
                    accessor2.CheckSpecial();
                    CheckForm(accessor2.Form, ns != accessor2.Namespace);
                    accessor2.Mapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(type), ns, ImportContext.Attribute, a.XmlAttribute.DataType, null, repeats, false, limiter);
                    accessor2.IsList = repeats;
                    accessor2.Default = this.GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                    accessor2.Any = a.XmlAnyAttribute != null;
                    if ((accessor2.Form == XmlSchemaForm.Qualified) && (accessor2.Namespace != ns))
                    {
                        if (this.xsdAttributes == null)
                        {
                            this.xsdAttributes = new System.Xml.Serialization.NameTable();
                        }
                        accessor2 = (AttributeAccessor) this.ReconcileAccessor(accessor2, this.xsdAttributes);
                    }
                    accessor.Attribute = accessor2;
                }
                else if ((xmlFlags & flags2) != ((XmlAttributeFlags) 0))
                {
                    if ((xmlFlags & flags2) != xmlFlags)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlIllegalElementsArrayAttribute"));
                    }
                    if (a.XmlText != null)
                    {
                        TextAccessor accessor3 = new TextAccessor();
                        Type type4 = (a.XmlText.Type == null) ? arrayElementType : a.XmlText.Type;
                        TypeDesc typeDesc = this.typeScope.GetTypeDesc(type4);
                        accessor3.Name = name;
                        accessor3.Mapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(type4), ns, ImportContext.Text, a.XmlText.DataType, null, true, false, limiter);
                        if (!(accessor3.Mapping is SpecialMapping) && (typeDesc != this.typeScope.GetTypeDesc(typeof(string))))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlIllegalArrayTextAttribute", new object[] { name }));
                        }
                        accessor.Text = accessor3;
                    }
                    if (((a.XmlText == null) && (a.XmlElements.Count == 0)) && (a.XmlAnyElements.Count == 0))
                    {
                        a.XmlElements.Add(CreateElementAttribute(accessor.TypeDesc));
                    }
                    for (int i = 0; i < a.XmlElements.Count; i++)
                    {
                        ElementAccessor accessor4;
                        XmlElementAttribute attribute = a.XmlElements[i];
                        Type type5 = (attribute.Type == null) ? arrayElementType : attribute.Type;
                        TypeDesc desc2 = this.typeScope.GetTypeDesc(type5);
                        TypeModel typeModel = this.modelScope.GetTypeModel(type5);
                        accessor4 = new ElementAccessor {
                            Namespace = rpc ? null : ((attribute.Namespace == null) ? ns : attribute.Namespace),
                            Mapping = this.ImportTypeMapping(typeModel, rpc ? ns : accessor4.Namespace, ImportContext.Element, attribute.DataType, null, limiter)
                        };
                        if (a.XmlElements.Count == 1)
                        {
                            accessor4.Name = XmlConvert.EncodeLocalName((attribute.ElementName.Length == 0) ? name : attribute.ElementName);
                        }
                        else
                        {
                            accessor4.Name = (attribute.ElementName.Length == 0) ? accessor4.Mapping.DefaultElementName : XmlConvert.EncodeLocalName(attribute.ElementName);
                        }
                        accessor4.Default = this.GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                        if ((attribute.IsNullableSpecified && !attribute.IsNullable) && typeModel.TypeDesc.IsOptionalValue)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlInvalidNotNullable", new object[] { typeModel.TypeDesc.BaseTypeDesc.FullName, "XmlElement" }));
                        }
                        accessor4.IsNullable = attribute.IsNullableSpecified ? attribute.IsNullable : typeModel.TypeDesc.IsOptionalValue;
                        accessor4.Form = rpc ? XmlSchemaForm.Unqualified : ((attribute.Form == XmlSchemaForm.None) ? qualified : attribute.Form);
                        CheckNullable(accessor4.IsNullable, desc2, accessor4.Mapping);
                        if (!rpc)
                        {
                            CheckForm(accessor4.Form, ns != accessor4.Namespace);
                            accessor4 = this.ReconcileLocalAccessor(accessor4, ns);
                        }
                        if (attribute.Order != -1)
                        {
                            if ((attribute.Order != order) && (order != -1))
                            {
                                throw new InvalidOperationException(Res.GetString("XmlSequenceMatch", new object[] { "Order" }));
                            }
                            order = attribute.Order;
                        }
                        AddUniqueAccessor(scope, accessor4);
                        list.Add(accessor4);
                    }
                    System.Xml.Serialization.NameTable table2 = new System.Xml.Serialization.NameTable();
                    for (int j = 0; j < a.XmlAnyElements.Count; j++)
                    {
                        XmlAnyElementAttribute attribute2 = a.XmlAnyElements[j];
                        Type c = typeof(IXmlSerializable).IsAssignableFrom(arrayElementType) ? arrayElementType : (typeof(XmlNode).IsAssignableFrom(arrayElementType) ? arrayElementType : typeof(XmlElement));
                        if (!arrayElementType.IsAssignableFrom(c))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlIllegalAnyElement", new object[] { arrayElementType.FullName }));
                        }
                        string str3 = (attribute2.Name.Length == 0) ? attribute2.Name : XmlConvert.EncodeLocalName(attribute2.Name);
                        string str4 = attribute2.NamespaceSpecified ? attribute2.Namespace : null;
                        if (table2[str3, str4] == null)
                        {
                            table2[str3, str4] = attribute2;
                            if (scope[str3, (str4 == null) ? ns : str4] != null)
                            {
                                throw new InvalidOperationException(Res.GetString("XmlAnyElementDuplicate", new object[] { name, attribute2.Name, (attribute2.Namespace == null) ? "null" : attribute2.Namespace }));
                            }
                            ElementAccessor accessor5 = new ElementAccessor {
                                Name = str3,
                                Namespace = (str4 == null) ? ns : str4,
                                Any = true,
                                AnyNamespaces = str4
                            };
                            TypeDesc desc3 = this.typeScope.GetTypeDesc(c);
                            TypeModel model3 = this.modelScope.GetTypeModel(c);
                            if (accessor5.Name.Length > 0)
                            {
                                model3.TypeDesc.IsMixed = true;
                            }
                            accessor5.Mapping = this.ImportTypeMapping(model3, accessor5.Namespace, ImportContext.Element, string.Empty, null, limiter);
                            accessor5.Default = this.GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                            accessor5.IsNullable = false;
                            accessor5.Form = qualified;
                            CheckNullable(accessor5.IsNullable, desc3, accessor5.Mapping);
                            if (!rpc)
                            {
                                CheckForm(accessor5.Form, ns != accessor5.Namespace);
                                accessor5 = this.ReconcileLocalAccessor(accessor5, ns);
                            }
                            scope.Add(accessor5.Name, accessor5.Namespace, accessor5);
                            list.Add(accessor5);
                            if (attribute2.Order != -1)
                            {
                                if ((attribute2.Order != order) && (order != -1))
                                {
                                    throw new InvalidOperationException(Res.GetString("XmlSequenceMatch", new object[] { "Order" }));
                                }
                                order = attribute2.Order;
                            }
                        }
                    }
                }
                else
                {
                    if (((xmlFlags & flags4) != ((XmlAttributeFlags) 0)) && ((xmlFlags & flags4) != xmlFlags))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlIllegalArrayArrayAttribute"));
                    }
                    TypeDesc desc4 = this.typeScope.GetTypeDesc(arrayElementType);
                    if (a.XmlArray == null)
                    {
                        a.XmlArray = CreateArrayAttribute(accessor.TypeDesc);
                    }
                    if (CountAtLevel(a.XmlArrayItems, this.arrayNestingLevel) == 0)
                    {
                        a.XmlArrayItems.Add(CreateArrayItemAttribute(desc4, this.arrayNestingLevel));
                    }
                    ElementAccessor accessor6 = new ElementAccessor {
                        Name = XmlConvert.EncodeLocalName((a.XmlArray.ElementName.Length == 0) ? name : a.XmlArray.ElementName),
                        Namespace = rpc ? null : ((a.XmlArray.Namespace == null) ? ns : a.XmlArray.Namespace)
                    };
                    this.savedArrayItemAttributes = a.XmlArrayItems;
                    this.savedArrayNamespace = accessor6.Namespace;
                    ArrayMapping mapping = this.ImportArrayLikeMapping(this.modelScope.GetArrayModel(fieldType), ns, limiter);
                    accessor6.Mapping = mapping;
                    accessor6.IsNullable = a.XmlArray.IsNullable;
                    accessor6.Form = rpc ? XmlSchemaForm.Unqualified : ((a.XmlArray.Form == XmlSchemaForm.None) ? qualified : a.XmlArray.Form);
                    order = a.XmlArray.Order;
                    CheckNullable(accessor6.IsNullable, accessor.TypeDesc, accessor6.Mapping);
                    if (!rpc)
                    {
                        CheckForm(accessor6.Form, ns != accessor6.Namespace);
                        accessor6 = this.ReconcileLocalAccessor(accessor6, ns);
                    }
                    this.savedArrayItemAttributes = null;
                    this.savedArrayNamespace = null;
                    AddUniqueAccessor(scope, accessor6);
                    list.Add(accessor6);
                }
            }
            else if (!accessor.TypeDesc.IsVoid)
            {
                XmlAttributeFlags flags5 = XmlAttributeFlags.XmlnsDeclarations | XmlAttributeFlags.ChoiceIdentifier | XmlAttributeFlags.AnyElements | XmlAttributeFlags.Attribute | XmlAttributeFlags.Elements | XmlAttributeFlags.Text;
                if ((xmlFlags & flags5) != xmlFlags)
                {
                    throw new InvalidOperationException(Res.GetString("XmlIllegalAttribute"));
                }
                if (accessor.TypeDesc.IsPrimitive || accessor.TypeDesc.IsEnum)
                {
                    if (a.XmlAnyElements.Count > 0)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlIllegalAnyElement", new object[] { accessor.TypeDesc.FullName }));
                    }
                    if (a.XmlAttribute != null)
                    {
                        if (a.XmlElements.Count > 0)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlIllegalAttribute"));
                        }
                        if (a.XmlAttribute.Type != null)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlIllegalType", new object[] { "XmlAttribute" }));
                        }
                        AttributeAccessor accessor7 = new AttributeAccessor {
                            Name = Accessor.EscapeQName((a.XmlAttribute.AttributeName.Length == 0) ? name : a.XmlAttribute.AttributeName),
                            Namespace = (a.XmlAttribute.Namespace == null) ? ns : a.XmlAttribute.Namespace,
                            Form = a.XmlAttribute.Form
                        };
                        if ((accessor7.Form == XmlSchemaForm.None) && (ns != accessor7.Namespace))
                        {
                            accessor7.Form = XmlSchemaForm.Qualified;
                        }
                        accessor7.CheckSpecial();
                        CheckForm(accessor7.Form, ns != accessor7.Namespace);
                        accessor7.Mapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(fieldType), ns, ImportContext.Attribute, a.XmlAttribute.DataType, null, limiter);
                        accessor7.Default = this.GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                        accessor7.Any = a.XmlAnyAttribute != null;
                        if ((accessor7.Form == XmlSchemaForm.Qualified) && (accessor7.Namespace != ns))
                        {
                            if (this.xsdAttributes == null)
                            {
                                this.xsdAttributes = new System.Xml.Serialization.NameTable();
                            }
                            accessor7 = (AttributeAccessor) this.ReconcileAccessor(accessor7, this.xsdAttributes);
                        }
                        accessor.Attribute = accessor7;
                    }
                    else
                    {
                        if (a.XmlText != null)
                        {
                            if ((a.XmlText.Type != null) && (a.XmlText.Type != fieldType))
                            {
                                throw new InvalidOperationException(Res.GetString("XmlIllegalType", new object[] { "XmlText" }));
                            }
                            TextAccessor accessor8 = new TextAccessor {
                                Name = name,
                                Mapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(fieldType), ns, ImportContext.Text, a.XmlText.DataType, null, limiter)
                            };
                            accessor.Text = accessor8;
                        }
                        else if (a.XmlElements.Count == 0)
                        {
                            a.XmlElements.Add(CreateElementAttribute(accessor.TypeDesc));
                        }
                        for (int k = 0; k < a.XmlElements.Count; k++)
                        {
                            XmlElementAttribute attribute3 = a.XmlElements[k];
                            if ((attribute3.Type != null) && (this.typeScope.GetTypeDesc(attribute3.Type) != accessor.TypeDesc))
                            {
                                throw new InvalidOperationException(Res.GetString("XmlIllegalType", new object[] { "XmlElement" }));
                            }
                            ElementAccessor accessor9 = new ElementAccessor {
                                Name = XmlConvert.EncodeLocalName((attribute3.ElementName.Length == 0) ? name : attribute3.ElementName),
                                Namespace = rpc ? null : ((attribute3.Namespace == null) ? ns : attribute3.Namespace)
                            };
                            TypeModel model4 = this.modelScope.GetTypeModel(fieldType);
                            accessor9.Mapping = this.ImportTypeMapping(model4, rpc ? ns : accessor9.Namespace, ImportContext.Element, attribute3.DataType, null, limiter);
                            if (accessor9.Mapping.TypeDesc.Kind == TypeKind.Node)
                            {
                                accessor9.Any = true;
                            }
                            accessor9.Default = this.GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                            if ((attribute3.IsNullableSpecified && !attribute3.IsNullable) && model4.TypeDesc.IsOptionalValue)
                            {
                                throw new InvalidOperationException(Res.GetString("XmlInvalidNotNullable", new object[] { model4.TypeDesc.BaseTypeDesc.FullName, "XmlElement" }));
                            }
                            accessor9.IsNullable = attribute3.IsNullableSpecified ? attribute3.IsNullable : model4.TypeDesc.IsOptionalValue;
                            accessor9.Form = rpc ? XmlSchemaForm.Unqualified : ((attribute3.Form == XmlSchemaForm.None) ? qualified : attribute3.Form);
                            CheckNullable(accessor9.IsNullable, accessor.TypeDesc, accessor9.Mapping);
                            if (!rpc)
                            {
                                CheckForm(accessor9.Form, ns != accessor9.Namespace);
                                accessor9 = this.ReconcileLocalAccessor(accessor9, ns);
                            }
                            if (attribute3.Order != -1)
                            {
                                if ((attribute3.Order != order) && (order != -1))
                                {
                                    throw new InvalidOperationException(Res.GetString("XmlSequenceMatch", new object[] { "Order" }));
                                }
                                order = attribute3.Order;
                            }
                            AddUniqueAccessor(scope, accessor9);
                            list.Add(accessor9);
                        }
                    }
                }
                else if (a.Xmlns)
                {
                    if (xmlFlags != XmlAttributeFlags.XmlnsDeclarations)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlSoleXmlnsAttribute"));
                    }
                    if (fieldType != typeof(XmlSerializerNamespaces))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlXmlnsInvalidType", new object[] { name, fieldType.FullName, typeof(XmlSerializerNamespaces).FullName }));
                    }
                    accessor.Xmlns = new XmlnsAccessor();
                    accessor.Ignore = true;
                }
                else
                {
                    if ((a.XmlAttribute != null) || (a.XmlText != null))
                    {
                        if (accessor.TypeDesc.Kind == TypeKind.Serializable)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlIllegalAttrOrTextInterface", new object[] { name, accessor.TypeDesc.FullName, typeof(IXmlSerializable).Name }));
                        }
                        throw new InvalidOperationException(Res.GetString("XmlIllegalAttrOrText", new object[] { name, accessor.TypeDesc }));
                    }
                    if ((a.XmlElements.Count == 0) && (a.XmlAnyElements.Count == 0))
                    {
                        a.XmlElements.Add(CreateElementAttribute(accessor.TypeDesc));
                    }
                    for (int m = 0; m < a.XmlElements.Count; m++)
                    {
                        XmlElementAttribute attribute4 = a.XmlElements[m];
                        Type type7 = (attribute4.Type == null) ? fieldType : attribute4.Type;
                        TypeDesc desc5 = this.typeScope.GetTypeDesc(type7);
                        ElementAccessor accessor10 = new ElementAccessor();
                        TypeModel model5 = this.modelScope.GetTypeModel(type7);
                        accessor10.Namespace = rpc ? null : ((attribute4.Namespace == null) ? ns : attribute4.Namespace);
                        accessor10.Mapping = this.ImportTypeMapping(model5, rpc ? ns : accessor10.Namespace, ImportContext.Element, attribute4.DataType, null, false, openModel, limiter);
                        if (a.XmlElements.Count == 1)
                        {
                            accessor10.Name = XmlConvert.EncodeLocalName((attribute4.ElementName.Length == 0) ? name : attribute4.ElementName);
                        }
                        else
                        {
                            accessor10.Name = (attribute4.ElementName.Length == 0) ? accessor10.Mapping.DefaultElementName : XmlConvert.EncodeLocalName(attribute4.ElementName);
                        }
                        accessor10.Default = this.GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                        if ((attribute4.IsNullableSpecified && !attribute4.IsNullable) && model5.TypeDesc.IsOptionalValue)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlInvalidNotNullable", new object[] { model5.TypeDesc.BaseTypeDesc.FullName, "XmlElement" }));
                        }
                        accessor10.IsNullable = attribute4.IsNullableSpecified ? attribute4.IsNullable : model5.TypeDesc.IsOptionalValue;
                        accessor10.Form = rpc ? XmlSchemaForm.Unqualified : ((attribute4.Form == XmlSchemaForm.None) ? qualified : attribute4.Form);
                        CheckNullable(accessor10.IsNullable, desc5, accessor10.Mapping);
                        if (!rpc)
                        {
                            CheckForm(accessor10.Form, ns != accessor10.Namespace);
                            accessor10 = this.ReconcileLocalAccessor(accessor10, ns);
                        }
                        if (attribute4.Order != -1)
                        {
                            if ((attribute4.Order != order) && (order != -1))
                            {
                                throw new InvalidOperationException(Res.GetString("XmlSequenceMatch", new object[] { "Order" }));
                            }
                            order = attribute4.Order;
                        }
                        AddUniqueAccessor(scope, accessor10);
                        list.Add(accessor10);
                    }
                    System.Xml.Serialization.NameTable table3 = new System.Xml.Serialization.NameTable();
                    for (int n = 0; n < a.XmlAnyElements.Count; n++)
                    {
                        XmlAnyElementAttribute attribute5 = a.XmlAnyElements[n];
                        Type type8 = typeof(IXmlSerializable).IsAssignableFrom(fieldType) ? fieldType : (typeof(XmlNode).IsAssignableFrom(fieldType) ? fieldType : typeof(XmlElement));
                        if (!fieldType.IsAssignableFrom(type8))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlIllegalAnyElement", new object[] { fieldType.FullName }));
                        }
                        string str5 = (attribute5.Name.Length == 0) ? attribute5.Name : XmlConvert.EncodeLocalName(attribute5.Name);
                        string str6 = attribute5.NamespaceSpecified ? attribute5.Namespace : null;
                        if (table3[str5, str6] == null)
                        {
                            table3[str5, str6] = attribute5;
                            if (scope[str5, (str6 == null) ? ns : str6] != null)
                            {
                                throw new InvalidOperationException(Res.GetString("XmlAnyElementDuplicate", new object[] { name, attribute5.Name, (attribute5.Namespace == null) ? "null" : attribute5.Namespace }));
                            }
                            ElementAccessor accessor11 = new ElementAccessor {
                                Name = str5,
                                Namespace = (str6 == null) ? ns : str6,
                                Any = true,
                                AnyNamespaces = str6
                            };
                            TypeDesc desc6 = this.typeScope.GetTypeDesc(type8);
                            TypeModel model6 = this.modelScope.GetTypeModel(type8);
                            if (accessor11.Name.Length > 0)
                            {
                                model6.TypeDesc.IsMixed = true;
                            }
                            accessor11.Mapping = this.ImportTypeMapping(model6, accessor11.Namespace, ImportContext.Element, string.Empty, null, false, openModel, limiter);
                            accessor11.Default = this.GetDefaultValue(model.FieldTypeDesc, model.FieldType, a);
                            accessor11.IsNullable = false;
                            accessor11.Form = qualified;
                            CheckNullable(accessor11.IsNullable, desc6, accessor11.Mapping);
                            if (!rpc)
                            {
                                CheckForm(accessor11.Form, ns != accessor11.Namespace);
                                accessor11 = this.ReconcileLocalAccessor(accessor11, ns);
                            }
                            if (attribute5.Order != -1)
                            {
                                if ((attribute5.Order != order) && (order != -1))
                                {
                                    throw new InvalidOperationException(Res.GetString("XmlSequenceMatch", new object[] { "Order" }));
                                }
                                order = attribute5.Order;
                            }
                            scope.Add(accessor11.Name, accessor11.Namespace, accessor11);
                            list.Add(accessor11);
                        }
                    }
                }
            }
            accessor.Elements = (ElementAccessor[]) list.ToArray(typeof(ElementAccessor));
            accessor.SequenceId = order;
            if (rpc)
            {
                if ((accessor.TypeDesc.IsArrayLike && (accessor.Elements.Length > 0)) && !(accessor.Elements[0].Mapping is ArrayMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlRpcLitArrayElement", new object[] { accessor.Elements[0].Name }));
                }
                if (accessor.Xmlns != null)
                {
                    throw new InvalidOperationException(Res.GetString("XmlRpcLitXmlns", new object[] { accessor.Name }));
                }
            }
            if (accessor.ChoiceIdentifier != null)
            {
                accessor.ChoiceIdentifier.MemberIds = new string[accessor.Elements.Length];
                for (int num8 = 0; num8 < accessor.Elements.Length; num8++)
                {
                    bool flag2 = false;
                    ElementAccessor accessor12 = accessor.Elements[num8];
                    EnumMapping mapping2 = (EnumMapping) accessor.ChoiceIdentifier.Mapping;
                    for (int num9 = 0; num9 < mapping2.Constants.Length; num9++)
                    {
                        string xmlName = mapping2.Constants[num9].XmlName;
                        if (accessor12.Any && (accessor12.Name.Length == 0))
                        {
                            string str8 = (accessor12.AnyNamespaces == null) ? "##any" : accessor12.AnyNamespaces;
                            if (!(xmlName.Substring(0, xmlName.Length - 1) == str8))
                            {
                                continue;
                            }
                            accessor.ChoiceIdentifier.MemberIds[num8] = mapping2.Constants[num9].Name;
                            flag2 = true;
                            break;
                        }
                        int length = xmlName.LastIndexOf(':');
                        string str9 = (length < 0) ? mapping2.Namespace : xmlName.Substring(0, length);
                        string str10 = (length < 0) ? xmlName : xmlName.Substring(length + 1);
                        if ((accessor12.Name == str10) && (((accessor12.Form == XmlSchemaForm.Unqualified) && string.IsNullOrEmpty(str9)) || (accessor12.Namespace == str9)))
                        {
                            accessor.ChoiceIdentifier.MemberIds[num8] = mapping2.Constants[num9].Name;
                            flag2 = true;
                            break;
                        }
                    }
                    if (!flag2)
                    {
                        if (accessor12.Any && (accessor12.Name.Length == 0))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlChoiceMissingAnyValue", new object[] { accessor.ChoiceIdentifier.Mapping.TypeDesc.FullName }));
                        }
                        string str11 = ((accessor12.Namespace != null) && (accessor12.Namespace.Length > 0)) ? (accessor12.Namespace + ":" + accessor12.Name) : accessor12.Name;
                        throw new InvalidOperationException(Res.GetString("XmlChoiceMissingValue", new object[] { accessor.ChoiceIdentifier.Mapping.TypeDesc.FullName, str11, accessor12.Name, accessor12.Namespace }));
                    }
                }
            }
            this.arrayNestingLevel = arrayNestingLevel;
            this.savedArrayItemAttributes = savedArrayItemAttributes;
            this.savedArrayNamespace = savedArrayNamespace;
        }

        private ArrayMapping ImportArrayLikeMapping(ArrayModel model, string ns, RecursionLimiter limiter)
        {
            ArrayMapping arrayMapping = new ArrayMapping {
                TypeDesc = model.TypeDesc
            };
            if (this.savedArrayItemAttributes == null)
            {
                this.savedArrayItemAttributes = new XmlArrayItemAttributes();
            }
            if (CountAtLevel(this.savedArrayItemAttributes, this.arrayNestingLevel) == 0)
            {
                this.savedArrayItemAttributes.Add(CreateArrayItemAttribute(this.typeScope.GetTypeDesc(model.Element.Type), this.arrayNestingLevel));
            }
            this.CreateArrayElementsFromAttributes(arrayMapping, this.savedArrayItemAttributes, model.Element.Type, (this.savedArrayNamespace == null) ? ns : this.savedArrayNamespace, limiter);
            this.SetArrayMappingType(arrayMapping, ns, model.Type);
            for (int i = 0; i < arrayMapping.Elements.Length; i++)
            {
                arrayMapping.Elements[i] = this.ReconcileLocalAccessor(arrayMapping.Elements[i], arrayMapping.Namespace);
            }
            this.IncludeTypes(model.Type);
            ArrayMapping next = (ArrayMapping) this.types[arrayMapping.TypeName, arrayMapping.Namespace];
            if (next != null)
            {
                ArrayMapping mapping3 = next;
                while (next != null)
                {
                    if (next.TypeDesc == model.TypeDesc)
                    {
                        return next;
                    }
                    next = next.Next;
                }
                arrayMapping.Next = mapping3;
                if (!arrayMapping.IsAnonymousType)
                {
                    this.types[arrayMapping.TypeName, arrayMapping.Namespace] = arrayMapping;
                    return arrayMapping;
                }
                this.anonymous[model.Type] = arrayMapping;
                return arrayMapping;
            }
            this.typeScope.AddTypeMapping(arrayMapping);
            if (!arrayMapping.IsAnonymousType)
            {
                this.types.Add(arrayMapping.TypeName, arrayMapping.Namespace, arrayMapping);
                return arrayMapping;
            }
            this.anonymous[model.Type] = arrayMapping;
            return arrayMapping;
        }

        private ConstantMapping ImportConstantMapping(ConstantModel model)
        {
            XmlAttributes attributes = this.GetAttributes(model.FieldInfo);
            if (attributes.XmlIgnore)
            {
                return null;
            }
            if ((attributes.XmlFlags & ~XmlAttributeFlags.Enum) != ((XmlAttributeFlags) 0))
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidConstantAttribute"));
            }
            if (attributes.XmlEnum == null)
            {
                attributes.XmlEnum = new XmlEnumAttribute();
            }
            return new ConstantMapping { XmlName = (attributes.XmlEnum.Name == null) ? model.Name : attributes.XmlEnum.Name, Name = model.Name, Value = model.Value };
        }

        private ElementAccessor ImportElement(TypeModel model, XmlRootAttribute root, string defaultNamespace, RecursionLimiter limiter)
        {
            XmlAttributes a = this.GetAttributes(model.Type, true);
            if (root == null)
            {
                root = a.XmlRoot;
            }
            string ns = (root == null) ? null : root.Namespace;
            if (ns == null)
            {
                ns = defaultNamespace;
            }
            if (ns == null)
            {
                ns = this.defaultNs;
            }
            this.arrayNestingLevel = -1;
            this.savedArrayItemAttributes = null;
            this.savedArrayNamespace = null;
            ElementAccessor accessor = CreateElementAccessor(this.ImportTypeMapping(model, ns, ImportContext.Element, string.Empty, a, limiter), ns);
            if (root != null)
            {
                if (root.ElementName.Length > 0)
                {
                    accessor.Name = XmlConvert.EncodeLocalName(root.ElementName);
                }
                if ((root.IsNullableSpecified && !root.IsNullable) && model.TypeDesc.IsOptionalValue)
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidNotNullable", new object[] { model.TypeDesc.BaseTypeDesc.FullName, "XmlRoot" }));
                }
                accessor.IsNullable = root.IsNullableSpecified ? root.IsNullable : (model.TypeDesc.IsNullable || model.TypeDesc.IsOptionalValue);
                CheckNullable(accessor.IsNullable, model.TypeDesc, accessor.Mapping);
            }
            else
            {
                accessor.IsNullable = model.TypeDesc.IsNullable || model.TypeDesc.IsOptionalValue;
            }
            accessor.Form = XmlSchemaForm.Qualified;
            return (ElementAccessor) this.ReconcileAccessor(accessor, this.elements);
        }

        private EnumMapping ImportEnumMapping(EnumModel model, string ns, bool repeats)
        {
            XmlAttributes a = this.GetAttributes(model.Type, false);
            string str = ns;
            if ((a.XmlType != null) && (a.XmlType.Namespace != null))
            {
                str = a.XmlType.Namespace;
            }
            string name = IsAnonymousType(a, ns) ? null : this.XsdTypeName(model.Type, a, model.TypeDesc.Name);
            name = XmlConvert.EncodeLocalName(name);
            EnumMapping mapping = (EnumMapping) this.GetTypeMapping(name, str, model.TypeDesc, this.types, model.Type);
            if (mapping == null)
            {
                mapping = new EnumMapping {
                    TypeDesc = model.TypeDesc,
                    TypeName = name,
                    Namespace = str,
                    IsFlags = model.Type.IsDefined(typeof(FlagsAttribute), false)
                };
                if (mapping.IsFlags && repeats)
                {
                    throw new InvalidOperationException(Res.GetString("XmlIllegalAttributeFlagsArray", new object[] { model.TypeDesc.FullName }));
                }
                mapping.IsList = repeats;
                mapping.IncludeInSchema = (a.XmlType == null) || a.XmlType.IncludeInSchema;
                if (!mapping.IsAnonymousType)
                {
                    this.types.Add(name, str, mapping);
                }
                else
                {
                    this.anonymous[model.Type] = mapping;
                }
                ArrayList list = new ArrayList();
                for (int i = 0; i < model.Constants.Length; i++)
                {
                    ConstantMapping mapping2 = this.ImportConstantMapping(model.Constants[i]);
                    if (mapping2 != null)
                    {
                        list.Add(mapping2);
                    }
                }
                if (list.Count == 0)
                {
                    throw new InvalidOperationException(Res.GetString("XmlNoSerializableMembers", new object[] { model.TypeDesc.FullName }));
                }
                mapping.Constants = (ConstantMapping[]) list.ToArray(typeof(ConstantMapping));
                this.typeScope.AddTypeMapping(mapping);
            }
            return mapping;
        }

        private MemberMapping ImportFieldMapping(StructModel parent, FieldModel model, XmlAttributes a, string ns, RecursionLimiter limiter)
        {
            MemberMapping accessor = new MemberMapping {
                Name = model.Name,
                CheckShouldPersist = model.CheckShouldPersist,
                CheckSpecified = model.CheckSpecified,
                ReadOnly = model.ReadOnly
            };
            Type choiceIdentifierType = null;
            if (a.XmlChoiceIdentifier != null)
            {
                choiceIdentifierType = this.GetChoiceIdentifierType(a.XmlChoiceIdentifier, parent, model.FieldTypeDesc.IsArrayLike, model.Name);
            }
            this.ImportAccessorMapping(accessor, model, a, ns, choiceIdentifierType, false, false, limiter);
            return accessor;
        }

        private MemberMapping ImportMemberMapping(XmlReflectionMember xmlReflectionMember, string ns, XmlReflectionMember[] xmlReflectionMembers, bool rpc, bool openModel, RecursionLimiter limiter)
        {
            XmlSchemaForm form = rpc ? XmlSchemaForm.Unqualified : XmlSchemaForm.Qualified;
            XmlAttributes xmlAttributes = xmlReflectionMember.XmlAttributes;
            TypeDesc typeDesc = this.typeScope.GetTypeDesc(xmlReflectionMember.MemberType);
            if (xmlAttributes.XmlFlags == ((XmlAttributeFlags) 0))
            {
                if (typeDesc.IsArrayLike)
                {
                    XmlArrayAttribute attribute = CreateArrayAttribute(typeDesc);
                    attribute.ElementName = xmlReflectionMember.MemberName;
                    attribute.Namespace = rpc ? null : ns;
                    attribute.Form = form;
                    xmlAttributes.XmlArray = attribute;
                }
                else
                {
                    XmlElementAttribute attribute2 = CreateElementAttribute(typeDesc);
                    if (typeDesc.IsStructLike)
                    {
                        XmlAttributes attributes2 = new XmlAttributes(xmlReflectionMember.MemberType);
                        if (attributes2.XmlRoot != null)
                        {
                            if (attributes2.XmlRoot.ElementName.Length > 0)
                            {
                                attribute2.ElementName = attributes2.XmlRoot.ElementName;
                            }
                            if (rpc)
                            {
                                attribute2.Namespace = null;
                                if (attributes2.XmlRoot.IsNullableSpecified)
                                {
                                    attribute2.IsNullable = attributes2.XmlRoot.IsNullable;
                                }
                            }
                            else
                            {
                                attribute2.Namespace = attributes2.XmlRoot.Namespace;
                                attribute2.IsNullable = attributes2.XmlRoot.IsNullable;
                            }
                        }
                    }
                    if (attribute2.ElementName.Length == 0)
                    {
                        attribute2.ElementName = xmlReflectionMember.MemberName;
                    }
                    if ((attribute2.Namespace == null) && !rpc)
                    {
                        attribute2.Namespace = ns;
                    }
                    attribute2.Form = form;
                    xmlAttributes.XmlElements.Add(attribute2);
                }
            }
            else if (xmlAttributes.XmlRoot != null)
            {
                CheckNullable(xmlAttributes.XmlRoot.IsNullable, typeDesc, null);
            }
            MemberMapping accessor = new MemberMapping {
                Name = xmlReflectionMember.MemberName
            };
            bool checkSpecified = FindSpecifiedMember(xmlReflectionMember.MemberName, xmlReflectionMembers) != null;
            FieldModel model = new FieldModel(xmlReflectionMember.MemberName, xmlReflectionMember.MemberType, this.typeScope.GetTypeDesc(xmlReflectionMember.MemberType), checkSpecified, false);
            accessor.CheckShouldPersist = model.CheckShouldPersist;
            accessor.CheckSpecified = model.CheckSpecified;
            accessor.ReadOnly = model.ReadOnly;
            Type choiceIdentifierType = null;
            if (xmlAttributes.XmlChoiceIdentifier != null)
            {
                choiceIdentifierType = this.GetChoiceIdentifierType(xmlAttributes.XmlChoiceIdentifier, xmlReflectionMembers, typeDesc.IsArrayLike, model.Name);
            }
            this.ImportAccessorMapping(accessor, model, xmlAttributes, ns, choiceIdentifierType, rpc, openModel, limiter);
            if (xmlReflectionMember.OverrideIsNullable && (accessor.Elements.Length > 0))
            {
                accessor.Elements[0].IsNullable = false;
            }
            return accessor;
        }

        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement)
        {
            return this.ImportMembersMapping(elementName, ns, members, hasWrapperElement, false);
        }

        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc)
        {
            return this.ImportMembersMapping(elementName, ns, members, hasWrapperElement, rpc, false);
        }

        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc, bool openModel)
        {
            return this.ImportMembersMapping(elementName, ns, members, hasWrapperElement, rpc, openModel, XmlMappingAccess.Write | XmlMappingAccess.Read);
        }

        private MembersMapping ImportMembersMapping(XmlReflectionMember[] xmlReflectionMembers, string ns, bool hasWrapperElement, bool rpc, bool openModel, RecursionLimiter limiter)
        {
            MembersMapping mapping = new MembersMapping {
                TypeDesc = this.typeScope.GetTypeDesc(typeof(object[]))
            };
            MemberMapping[] mappingArray = new MemberMapping[xmlReflectionMembers.Length];
            System.Xml.Serialization.NameTable elements = new System.Xml.Serialization.NameTable();
            System.Xml.Serialization.NameTable attributes = new System.Xml.Serialization.NameTable();
            TextAccessor text = null;
            bool isSequence = false;
            for (int i = 0; i < mappingArray.Length; i++)
            {
                try
                {
                    MemberMapping member = this.ImportMemberMapping(xmlReflectionMembers[i], ns, xmlReflectionMembers, rpc, openModel, limiter);
                    if (!hasWrapperElement && (member.Attribute != null))
                    {
                        if (rpc)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlRpcLitAttributeAttributes"));
                        }
                        throw new InvalidOperationException(Res.GetString("XmlInvalidAttributeType", new object[] { "XmlAttribute" }));
                    }
                    if (rpc && xmlReflectionMembers[i].IsReturnValue)
                    {
                        if (i > 0)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlInvalidReturnPosition"));
                        }
                        member.IsReturnValue = true;
                    }
                    mappingArray[i] = member;
                    isSequence |= member.IsSequence;
                    if (!xmlReflectionMembers[i].XmlAttributes.XmlIgnore)
                    {
                        AddUniqueAccessor(member, elements, attributes, isSequence);
                    }
                    mappingArray[i] = member;
                    if (member.Text != null)
                    {
                        if (text != null)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlIllegalMultipleTextMembers"));
                        }
                        text = member.Text;
                    }
                    if (member.Xmlns != null)
                    {
                        if (mapping.XmlnsMember != null)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlMultipleXmlnsMembers"));
                        }
                        mapping.XmlnsMember = member;
                    }
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    throw this.CreateReflectionException(xmlReflectionMembers[i].MemberName, exception);
                }
            }
            if (isSequence)
            {
                throw new InvalidOperationException(Res.GetString("XmlSequenceMembers", new object[] { "Order" }));
            }
            mapping.Members = mappingArray;
            mapping.HasWrapperElement = hasWrapperElement;
            return mapping;
        }

        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc, bool openModel, XmlMappingAccess access)
        {
            ElementAccessor accessor = new ElementAccessor {
                Name = ((elementName == null) || (elementName.Length == 0)) ? elementName : XmlConvert.EncodeLocalName(elementName),
                Namespace = ns
            };
            MembersMapping mapping = this.ImportMembersMapping(members, ns, hasWrapperElement, rpc, openModel, new RecursionLimiter());
            accessor.Mapping = mapping;
            accessor.Form = XmlSchemaForm.Qualified;
            if (!rpc)
            {
                if (hasWrapperElement)
                {
                    accessor = (ElementAccessor) this.ReconcileAccessor(accessor, this.elements);
                }
                else
                {
                    foreach (MemberMapping mapping2 in mapping.Members)
                    {
                        if ((mapping2.Elements != null) && (mapping2.Elements.Length > 0))
                        {
                            mapping2.Elements[0] = (ElementAccessor) this.ReconcileAccessor(mapping2.Elements[0], this.elements);
                        }
                    }
                }
            }
            return new XmlMembersMapping(this.typeScope, accessor, access) { GenerateSerializer = true };
        }

        private PrimitiveMapping ImportPrimitiveMapping(PrimitiveModel model, ImportContext context, string dataType, bool repeats)
        {
            PrimitiveMapping mapping = new PrimitiveMapping();
            if (dataType.Length > 0)
            {
                mapping.TypeDesc = this.typeScope.GetTypeDesc(dataType, "http://www.w3.org/2001/XMLSchema");
                if (mapping.TypeDesc == null)
                {
                    mapping.TypeDesc = this.typeScope.GetTypeDesc(dataType, "http://microsoft.com/wsdl/types/");
                    if (mapping.TypeDesc == null)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlUdeclaredXsdType", new object[] { dataType }));
                    }
                }
            }
            else
            {
                mapping.TypeDesc = model.TypeDesc;
            }
            mapping.TypeName = mapping.TypeDesc.DataType.Name;
            mapping.Namespace = mapping.TypeDesc.IsXsdType ? "http://www.w3.org/2001/XMLSchema" : "http://microsoft.com/wsdl/types/";
            mapping.IsList = repeats;
            this.CheckContext(mapping.TypeDesc, context);
            return mapping;
        }

        private SpecialMapping ImportSpecialMapping(Type type, TypeDesc typeDesc, string ns, ImportContext context, RecursionLimiter limiter)
        {
            if (this.specials == null)
            {
                this.specials = new Hashtable();
            }
            SpecialMapping mapping = (SpecialMapping) this.specials[type];
            if (mapping != null)
            {
                this.CheckContext(mapping.TypeDesc, context);
                return mapping;
            }
            if (typeDesc.Kind == TypeKind.Serializable)
            {
                SerializableMapping mapping2 = null;
                object[] customAttributes = type.GetCustomAttributes(typeof(XmlSchemaProviderAttribute), false);
                if (customAttributes.Length > 0)
                {
                    XmlSchemaProviderAttribute provider = (XmlSchemaProviderAttribute) customAttributes[0];
                    mapping2 = new SerializableMapping(GetMethodFromSchemaProvider(provider, type), provider.IsAny, ns);
                    XmlQualifiedName xsiType = mapping2.XsiType;
                    if ((xsiType != null) && !xsiType.IsEmpty)
                    {
                        if (this.serializables == null)
                        {
                            this.serializables = new System.Xml.Serialization.NameTable();
                        }
                        SerializableMapping mapping3 = (SerializableMapping) this.serializables[xsiType];
                        if (mapping3 != null)
                        {
                            if (mapping3.Type == null)
                            {
                                mapping2 = mapping3;
                            }
                            else if (mapping3.Type != type)
                            {
                                SerializableMapping next = mapping3.Next;
                                mapping3.Next = mapping2;
                                mapping2.Next = next;
                            }
                        }
                        else
                        {
                            XmlSchemaType xsdType = mapping2.XsdType;
                            if (xsdType != null)
                            {
                                this.SetBase(mapping2, xsdType.DerivedFrom);
                            }
                            this.serializables[xsiType] = mapping2;
                        }
                        mapping2.TypeName = xsiType.Name;
                        mapping2.Namespace = xsiType.Namespace;
                    }
                    mapping2.TypeDesc = typeDesc;
                    mapping2.Type = type;
                    this.IncludeTypes(type);
                }
                else
                {
                    mapping2 = new SerializableMapping {
                        TypeDesc = typeDesc,
                        Type = type
                    };
                }
                mapping = mapping2;
            }
            else
            {
                mapping = new SpecialMapping {
                    TypeDesc = typeDesc
                };
            }
            this.CheckContext(typeDesc, context);
            this.specials.Add(type, mapping);
            this.typeScope.AddTypeMapping(mapping);
            return mapping;
        }

        private StructMapping ImportStructLikeMapping(StructModel model, string ns, bool openModel, XmlAttributes a, RecursionLimiter limiter)
        {
            if (model.TypeDesc.Kind == TypeKind.Root)
            {
                return this.GetRootMapping();
            }
            if (a == null)
            {
                a = this.GetAttributes(model.Type, false);
            }
            string str = ns;
            if ((a.XmlType != null) && (a.XmlType.Namespace != null))
            {
                str = a.XmlType.Namespace;
            }
            else if ((a.XmlRoot != null) && (a.XmlRoot.Namespace != null))
            {
                str = a.XmlRoot.Namespace;
            }
            string name = IsAnonymousType(a, ns) ? null : this.XsdTypeName(model.Type, a, model.TypeDesc.Name);
            name = XmlConvert.EncodeLocalName(name);
            StructMapping mapping = (StructMapping) this.GetTypeMapping(name, str, model.TypeDesc, this.types, model.Type);
            if (mapping == null)
            {
                mapping = new StructMapping {
                    TypeDesc = model.TypeDesc,
                    Namespace = str,
                    TypeName = name
                };
                if (!mapping.IsAnonymousType)
                {
                    this.types.Add(name, str, mapping);
                }
                else
                {
                    this.anonymous[model.Type] = mapping;
                }
                if (a.XmlType != null)
                {
                    mapping.IncludeInSchema = a.XmlType.IncludeInSchema;
                }
                if (limiter.IsExceededLimit)
                {
                    limiter.DeferredWorkItems.Add(new ImportStructWorkItem(model, mapping));
                    return mapping;
                }
                limiter.Depth++;
                this.InitializeStructMembers(mapping, model, openModel, name, limiter);
                while (limiter.DeferredWorkItems.Count > 0)
                {
                    int index = limiter.DeferredWorkItems.Count - 1;
                    ImportStructWorkItem item = limiter.DeferredWorkItems[index];
                    if (this.InitializeStructMembers(item.Mapping, item.Model, openModel, name, limiter))
                    {
                        limiter.DeferredWorkItems.RemoveAt(index);
                    }
                }
                limiter.Depth--;
            }
            return mapping;
        }

        public XmlTypeMapping ImportTypeMapping(Type type)
        {
            return this.ImportTypeMapping(type, null, null);
        }

        public XmlTypeMapping ImportTypeMapping(Type type, string defaultNamespace)
        {
            return this.ImportTypeMapping(type, null, defaultNamespace);
        }

        public XmlTypeMapping ImportTypeMapping(Type type, XmlRootAttribute root)
        {
            return this.ImportTypeMapping(type, root, null);
        }

        public XmlTypeMapping ImportTypeMapping(Type type, XmlRootAttribute root, string defaultNamespace)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            XmlTypeMapping mapping = new XmlTypeMapping(this.typeScope, this.ImportElement(this.modelScope.GetTypeModel(type), root, defaultNamespace, new RecursionLimiter()));
            mapping.SetKeyInternal(XmlMapping.GenerateKey(type, root, defaultNamespace));
            mapping.GenerateSerializer = true;
            return mapping;
        }

        private TypeMapping ImportTypeMapping(TypeModel model, string ns, ImportContext context, string dataType, XmlAttributes a, RecursionLimiter limiter)
        {
            return this.ImportTypeMapping(model, ns, context, dataType, a, false, false, limiter);
        }

        private TypeMapping ImportTypeMapping(TypeModel model, string ns, ImportContext context, string dataType, XmlAttributes a, bool repeats, bool openModel, RecursionLimiter limiter)
        {
            TypeMapping mapping3;
            try
            {
                if (dataType.Length > 0)
                {
                    TypeDesc desc = TypeScope.IsOptionalValue(model.Type) ? model.TypeDesc.BaseTypeDesc : model.TypeDesc;
                    if (!desc.IsPrimitive)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInvalidDataTypeUsage", new object[] { dataType, "XmlElementAttribute.DataType" }));
                    }
                    TypeDesc typeDesc = this.typeScope.GetTypeDesc(dataType, "http://www.w3.org/2001/XMLSchema");
                    if (typeDesc == null)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInvalidXsdDataType", new object[] { dataType, "XmlElementAttribute.DataType", new XmlQualifiedName(dataType, "http://www.w3.org/2001/XMLSchema").ToString() }));
                    }
                    if (desc.FullName != typeDesc.FullName)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlDataTypeMismatch", new object[] { dataType, "XmlElementAttribute.DataType", desc.FullName }));
                    }
                }
                if (a == null)
                {
                    a = this.GetAttributes(model.Type, false);
                }
                if ((a.XmlFlags & ~(XmlAttributeFlags.Type | XmlAttributeFlags.Root)) != ((XmlAttributeFlags) 0))
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidTypeAttributes", new object[] { model.Type.FullName }));
                }
                switch (model.TypeDesc.Kind)
                {
                    case TypeKind.Root:
                    case TypeKind.Struct:
                    case TypeKind.Class:
                        if (context != ImportContext.Element)
                        {
                            throw UnsupportedException(model.TypeDesc, context);
                        }
                        goto Label_0216;

                    case TypeKind.Primitive:
                        if (a.XmlFlags != ((XmlAttributeFlags) 0))
                        {
                            throw InvalidAttributeUseException(model.Type);
                        }
                        return this.ImportPrimitiveMapping((PrimitiveModel) model, context, dataType, repeats);

                    case TypeKind.Enum:
                        return this.ImportEnumMapping((EnumModel) model, ns, repeats);

                    case TypeKind.Array:
                    case TypeKind.Collection:
                    case TypeKind.Enumerable:
                        if (context != ImportContext.Element)
                        {
                            throw UnsupportedException(model.TypeDesc, context);
                        }
                        break;

                    default:
                        goto Label_02E5;
                }
                this.arrayNestingLevel++;
                ArrayMapping mapping = this.ImportArrayLikeMapping((ArrayModel) model, ns, limiter);
                this.arrayNestingLevel--;
                return mapping;
            Label_0216:
                if (model.TypeDesc.IsOptionalValue)
                {
                    TypeDesc desc3 = string.IsNullOrEmpty(dataType) ? model.TypeDesc.BaseTypeDesc : this.typeScope.GetTypeDesc(dataType, "http://www.w3.org/2001/XMLSchema");
                    string typeName = (desc3.DataType == null) ? desc3.Name : desc3.DataType.Name;
                    TypeMapping baseMapping = this.GetTypeMapping(typeName, ns, desc3, this.types, null);
                    if (baseMapping == null)
                    {
                        baseMapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(model.TypeDesc.BaseTypeDesc.Type), ns, context, dataType, null, repeats, openModel, limiter);
                    }
                    return this.CreateNullableMapping(baseMapping, model.TypeDesc.Type);
                }
                return this.ImportStructLikeMapping((StructModel) model, ns, openModel, a, limiter);
            Label_02E5:
                if (model.TypeDesc.Kind == TypeKind.Serializable)
                {
                    if ((a.XmlFlags & ~XmlAttributeFlags.Root) != ((XmlAttributeFlags) 0))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlSerializableAttributes", new object[] { model.TypeDesc.FullName, typeof(XmlSchemaProviderAttribute).Name }));
                    }
                }
                else if (a.XmlFlags != ((XmlAttributeFlags) 0))
                {
                    throw InvalidAttributeUseException(model.Type);
                }
                if (!model.TypeDesc.IsSpecial)
                {
                    throw UnsupportedException(model.TypeDesc, context);
                }
                mapping3 = this.ImportSpecialMapping(model.Type, model.TypeDesc, ns, context, limiter);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw this.CreateTypeReflectionException(model.TypeDesc.FullName, exception);
            }
            return mapping3;
        }

        public void IncludeType(Type type)
        {
            this.IncludeType(type, new RecursionLimiter());
        }

        private void IncludeType(Type type, RecursionLimiter limiter)
        {
            int arrayNestingLevel = this.arrayNestingLevel;
            XmlArrayItemAttributes savedArrayItemAttributes = this.savedArrayItemAttributes;
            string savedArrayNamespace = this.savedArrayNamespace;
            this.arrayNestingLevel = 0;
            this.savedArrayItemAttributes = null;
            this.savedArrayNamespace = null;
            TypeMapping mapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(type), this.defaultNs, ImportContext.Element, string.Empty, null, limiter);
            if (mapping.IsAnonymousType && !mapping.TypeDesc.IsSpecial)
            {
                throw new InvalidOperationException(Res.GetString("XmlAnonymousInclude", new object[] { type.FullName }));
            }
            this.arrayNestingLevel = arrayNestingLevel;
            this.savedArrayItemAttributes = savedArrayItemAttributes;
            this.savedArrayNamespace = savedArrayNamespace;
        }

        public void IncludeTypes(ICustomAttributeProvider provider)
        {
            this.IncludeTypes(provider, new RecursionLimiter());
        }

        private void IncludeTypes(ICustomAttributeProvider provider, RecursionLimiter limiter)
        {
            object[] customAttributes = provider.GetCustomAttributes(typeof(XmlIncludeAttribute), false);
            for (int i = 0; i < customAttributes.Length; i++)
            {
                Type type = ((XmlIncludeAttribute) customAttributes[i]).Type;
                this.IncludeType(type, limiter);
            }
        }

        private bool InitializeStructMembers(StructMapping mapping, StructModel model, bool openModel, string typeName, RecursionLimiter limiter)
        {
            if (!mapping.IsFullyInitialized)
            {
                if (model.TypeDesc.BaseTypeDesc != null)
                {
                    TypeModel typeModel = this.modelScope.GetTypeModel(model.Type.BaseType, false);
                    if (!(typeModel is StructModel))
                    {
                        throw new NotSupportedException(Res.GetString("XmlUnsupportedInheritance", new object[] { model.Type.BaseType.FullName }));
                    }
                    StructMapping mapping2 = this.ImportStructLikeMapping((StructModel) typeModel, mapping.Namespace, openModel, null, limiter);
                    int index = limiter.DeferredWorkItems.IndexOf(mapping2);
                    if (index >= 0)
                    {
                        if (!limiter.DeferredWorkItems.Contains(mapping))
                        {
                            limiter.DeferredWorkItems.Add(new ImportStructWorkItem(model, mapping));
                        }
                        int num2 = limiter.DeferredWorkItems.Count - 1;
                        if (index < num2)
                        {
                            ImportStructWorkItem item = limiter.DeferredWorkItems[index];
                            limiter.DeferredWorkItems[index] = limiter.DeferredWorkItems[num2];
                            limiter.DeferredWorkItems[num2] = item;
                        }
                        return false;
                    }
                    mapping.BaseMapping = mapping2;
                    foreach (AttributeAccessor accessor in mapping.BaseMapping.LocalAttributes.Values)
                    {
                        AddUniqueAccessor(mapping.LocalAttributes, accessor);
                    }
                    if (!mapping.BaseMapping.HasExplicitSequence())
                    {
                        foreach (ElementAccessor accessor2 in mapping.BaseMapping.LocalElements.Values)
                        {
                            AddUniqueAccessor(mapping.LocalElements, accessor2);
                        }
                    }
                }
                ArrayList list = new ArrayList();
                TextAccessor text = null;
                bool hasElements = false;
                bool isSequence = false;
                foreach (MemberInfo info in model.GetMemberInfos())
                {
                    if ((info.MemberType & (MemberTypes.Property | MemberTypes.Field)) != 0)
                    {
                        XmlAttributes a = this.GetAttributes(info);
                        if (!a.XmlIgnore)
                        {
                            FieldModel fieldModel = model.GetFieldModel(info);
                            if (fieldModel != null)
                            {
                                try
                                {
                                    MemberMapping member = this.ImportFieldMapping(model, fieldModel, a, mapping.Namespace, limiter);
                                    if ((member != null) && ((mapping.BaseMapping == null) || !mapping.BaseMapping.Declares(member, mapping.TypeName)))
                                    {
                                        isSequence |= member.IsSequence;
                                        AddUniqueAccessor(member, mapping.LocalElements, mapping.LocalAttributes, isSequence);
                                        if (member.Text != null)
                                        {
                                            if (!member.Text.Mapping.TypeDesc.CanBeTextValue && member.Text.Mapping.IsList)
                                            {
                                                throw new InvalidOperationException(Res.GetString("XmlIllegalTypedTextAttribute", new object[] { typeName, member.Text.Name, member.Text.Mapping.TypeDesc.FullName }));
                                            }
                                            if (text != null)
                                            {
                                                throw new InvalidOperationException(Res.GetString("XmlIllegalMultipleText", new object[] { model.Type.FullName }));
                                            }
                                            text = member.Text;
                                        }
                                        if (member.Xmlns != null)
                                        {
                                            if (mapping.XmlnsMember != null)
                                            {
                                                throw new InvalidOperationException(Res.GetString("XmlMultipleXmlns", new object[] { model.Type.FullName }));
                                            }
                                            mapping.XmlnsMember = member;
                                        }
                                        if ((member.Elements != null) && (member.Elements.Length != 0))
                                        {
                                            hasElements = true;
                                        }
                                        list.Add(member);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                                    {
                                        throw;
                                    }
                                    throw this.CreateMemberReflectionException(fieldModel, exception);
                                }
                            }
                        }
                    }
                }
                mapping.SetContentModel(text, hasElements);
                if (isSequence)
                {
                    Hashtable hashtable = new Hashtable();
                    for (int i = 0; i < list.Count; i++)
                    {
                        MemberMapping mapping4 = (MemberMapping) list[i];
                        if (mapping4.IsParticle)
                        {
                            if (!mapping4.IsSequence)
                            {
                                throw new InvalidOperationException(Res.GetString("XmlSequenceInconsistent", new object[] { "Order", mapping4.Name }));
                            }
                            if (hashtable[mapping4.SequenceId] != null)
                            {
                                throw new InvalidOperationException(Res.GetString("XmlSequenceUnique", new object[] { mapping4.SequenceId.ToString(CultureInfo.InvariantCulture), "Order", mapping4.Name }));
                            }
                            hashtable[mapping4.SequenceId] = mapping4;
                        }
                    }
                    list.Sort(new MemberMappingComparer());
                }
                mapping.Members = (MemberMapping[]) list.ToArray(typeof(MemberMapping));
                if (mapping.BaseMapping == null)
                {
                    mapping.BaseMapping = this.GetRootMapping();
                }
                if ((mapping.XmlnsMember != null) && mapping.BaseMapping.HasXmlnsMember)
                {
                    throw new InvalidOperationException(Res.GetString("XmlMultipleXmlns", new object[] { model.Type.FullName }));
                }
                this.IncludeTypes(model.Type, limiter);
                this.typeScope.AddTypeMapping(mapping);
                if (openModel)
                {
                    mapping.IsOpenModel = true;
                }
            }
            return true;
        }

        private static Exception InvalidAttributeUseException(Type type)
        {
            return new InvalidOperationException(Res.GetString("XmlInvalidAttributeUse", new object[] { type.FullName }));
        }

        private static bool IsAnonymousType(XmlAttributes a, string contextNs)
        {
            if ((a.XmlType == null) || !a.XmlType.AnonymousType)
            {
                return false;
            }
            string str = a.XmlType.Namespace;
            if (!string.IsNullOrEmpty(str))
            {
                return (str == contextNs);
            }
            return true;
        }

        private Accessor ReconcileAccessor(Accessor accessor, System.Xml.Serialization.NameTable accessors)
        {
            if (accessor.Any && (accessor.Name.Length == 0))
            {
                return accessor;
            }
            Accessor accessor2 = (Accessor) accessors[accessor.Name, accessor.Namespace];
            if (accessor2 == null)
            {
                accessor.IsTopLevelInSchema = true;
                accessors.Add(accessor.Name, accessor.Namespace, accessor);
                return accessor;
            }
            if (accessor2.Mapping == accessor.Mapping)
            {
                return accessor2;
            }
            if ((!(accessor.Mapping is MembersMapping) && !(accessor2.Mapping is MembersMapping)) && (((accessor.Mapping.TypeDesc == accessor2.Mapping.TypeDesc) || ((accessor2.Mapping is NullableMapping) && (accessor.Mapping.TypeDesc == ((NullableMapping) accessor2.Mapping).BaseMapping.TypeDesc))) || ((accessor.Mapping is NullableMapping) && (((NullableMapping) accessor.Mapping).BaseMapping.TypeDesc == accessor2.Mapping.TypeDesc))))
            {
                string str = Convert.ToString(accessor.Default, CultureInfo.InvariantCulture);
                string str2 = Convert.ToString(accessor2.Default, CultureInfo.InvariantCulture);
                if (str != str2)
                {
                    throw new InvalidOperationException(Res.GetString("XmlCannotReconcileAccessorDefault", new object[] { accessor.Name, accessor.Namespace, str, str2 }));
                }
                return accessor2;
            }
            if ((accessor.Mapping is MembersMapping) || (accessor2.Mapping is MembersMapping))
            {
                throw new InvalidOperationException(Res.GetString("XmlMethodTypeNameConflict", new object[] { accessor.Name, accessor.Namespace }));
            }
            if (accessor.Mapping is ArrayMapping)
            {
                if (!(accessor2.Mapping is ArrayMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlCannotReconcileAccessor", new object[] { accessor.Name, accessor.Namespace, GetMappingName(accessor2.Mapping), GetMappingName(accessor.Mapping) }));
                }
                ArrayMapping mapping = (ArrayMapping) accessor.Mapping;
                ArrayMapping next = mapping.IsAnonymousType ? null : ((ArrayMapping) this.types[accessor2.Mapping.TypeName, accessor2.Mapping.Namespace]);
                ArrayMapping mapping3 = next;
                while (next != null)
                {
                    if (next == accessor.Mapping)
                    {
                        return accessor2;
                    }
                    next = next.Next;
                }
                mapping.Next = mapping3;
                if (!mapping.IsAnonymousType)
                {
                    this.types[accessor2.Mapping.TypeName, accessor2.Mapping.Namespace] = mapping;
                }
                return accessor2;
            }
            if (accessor is AttributeAccessor)
            {
                throw new InvalidOperationException(Res.GetString("XmlCannotReconcileAttributeAccessor", new object[] { accessor.Name, accessor.Namespace, GetMappingName(accessor2.Mapping), GetMappingName(accessor.Mapping) }));
            }
            throw new InvalidOperationException(Res.GetString("XmlCannotReconcileAccessor", new object[] { accessor.Name, accessor.Namespace, GetMappingName(accessor2.Mapping), GetMappingName(accessor.Mapping) }));
        }

        private ElementAccessor ReconcileLocalAccessor(ElementAccessor accessor, string ns)
        {
            if (accessor.Namespace == ns)
            {
                return accessor;
            }
            return (ElementAccessor) this.ReconcileAccessor(accessor, this.elements);
        }

        private void SetArrayMappingType(ArrayMapping mapping, string defaultNs, Type type)
        {
            XmlAttributes a = this.GetAttributes(type, false);
            if (IsAnonymousType(a, defaultNs))
            {
                mapping.TypeName = null;
                mapping.Namespace = defaultNs;
            }
            else
            {
                string defaultElementName;
                string str2;
                TypeMapping mapping2;
                ElementAccessor accessor = null;
                if (mapping.Elements.Length == 1)
                {
                    accessor = mapping.Elements[0];
                    mapping2 = accessor.Mapping;
                }
                else
                {
                    mapping2 = null;
                }
                bool flag2 = true;
                if (a.XmlType != null)
                {
                    str2 = a.XmlType.Namespace;
                    defaultElementName = XmlConvert.EncodeLocalName(this.XsdTypeName(type, a, a.XmlType.TypeName));
                    flag2 = defaultElementName == null;
                }
                else if (mapping2 is EnumMapping)
                {
                    str2 = mapping2.Namespace;
                    defaultElementName = mapping2.DefaultElementName;
                }
                else if (mapping2 is PrimitiveMapping)
                {
                    str2 = defaultNs;
                    defaultElementName = mapping2.TypeDesc.DataType.Name;
                }
                else if ((mapping2 is StructMapping) && mapping2.TypeDesc.IsRoot)
                {
                    str2 = defaultNs;
                    defaultElementName = "anyType";
                }
                else if (mapping2 != null)
                {
                    str2 = (mapping2.Namespace == "http://www.w3.org/2001/XMLSchema") ? defaultNs : mapping2.Namespace;
                    defaultElementName = mapping2.DefaultElementName;
                }
                else
                {
                    str2 = defaultNs;
                    defaultElementName = "Choice" + this.choiceNum++;
                }
                if (defaultElementName == null)
                {
                    defaultElementName = "Any";
                }
                if (accessor != null)
                {
                    str2 = accessor.Namespace;
                }
                if (str2 == null)
                {
                    str2 = defaultNs;
                }
                string str3 = defaultElementName = flag2 ? ("ArrayOf" + CodeIdentifier.MakePascal(defaultElementName)) : defaultElementName;
                int num = 1;
                TypeMapping mapping3 = (TypeMapping) this.types[str3, str2];
                while (mapping3 != null)
                {
                    if (mapping3 is ArrayMapping)
                    {
                        ArrayMapping mapping4 = (ArrayMapping) mapping3;
                        if (AccessorMapping.ElementsMatch(mapping4.Elements, mapping.Elements))
                        {
                            break;
                        }
                    }
                    str3 = defaultElementName + num.ToString(CultureInfo.InvariantCulture);
                    mapping3 = (TypeMapping) this.types[str3, str2];
                    num++;
                }
                mapping.TypeName = str3;
                mapping.Namespace = str2;
            }
        }

        internal void SetBase(SerializableMapping mapping, XmlQualifiedName baseQname)
        {
            if (!baseQname.IsEmpty && (baseQname.Namespace != "http://www.w3.org/2001/XMLSchema"))
            {
                XmlSchemaSet schemas = mapping.Schemas;
                ArrayList list = (ArrayList) schemas.Schemas(baseQname.Namespace);
                if (list.Count == 0)
                {
                    throw new InvalidOperationException(Res.GetString("XmlMissingSchema", new object[] { baseQname.Namespace }));
                }
                if (list.Count > 1)
                {
                    throw new InvalidOperationException(Res.GetString("XmlGetSchemaInclude", new object[] { baseQname.Namespace, typeof(IXmlSerializable).Name, "GetSchema" }));
                }
                XmlSchema schema = (XmlSchema) list[0];
                XmlSchemaType type = (XmlSchemaType) schema.SchemaTypes[baseQname];
                type = (type.Redefined != null) ? type.Redefined : type;
                if (this.serializables[baseQname] == null)
                {
                    SerializableMapping mapping2 = new SerializableMapping(baseQname, schemas);
                    this.SetBase(mapping2, type.DerivedFrom);
                    this.serializables.Add(baseQname, mapping2);
                }
                mapping.SetBaseMapping((SerializableMapping) this.serializables[baseQname]);
            }
        }

        private static Exception UnsupportedException(TypeDesc typeDesc, ImportContext context)
        {
            return new InvalidOperationException(Res.GetString("XmlIllegalTypeContext", new object[] { typeDesc.FullName, GetContextName(context) }));
        }

        internal static void ValidationCallbackWithErrorCode(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                throw new InvalidOperationException(Res.GetString("XmlSerializableSchemaError", new object[] { typeof(IXmlSerializable).Name, args.Message }));
            }
        }

        internal string XsdTypeName(Type type)
        {
            if (type == typeof(object))
            {
                return "anyType";
            }
            TypeDesc typeDesc = this.typeScope.GetTypeDesc(type);
            if ((typeDesc.IsPrimitive && (typeDesc.DataType != null)) && ((typeDesc.DataType.Name != null) && (typeDesc.DataType.Name.Length > 0)))
            {
                return typeDesc.DataType.Name;
            }
            return this.XsdTypeName(type, this.GetAttributes(type, false), typeDesc.Name);
        }

        internal string XsdTypeName(Type type, XmlAttributes a, string name)
        {
            string typeName = name;
            if ((a.XmlType != null) && (a.XmlType.TypeName.Length > 0))
            {
                typeName = a.XmlType.TypeName;
            }
            if (type.IsGenericType && (typeName.IndexOf('{') >= 0))
            {
                Type[] genericArguments = type.GetGenericTypeDefinition().GetGenericArguments();
                Type[] typeArray2 = type.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    string str2 = "{" + genericArguments[i] + "}";
                    if (typeName.Contains(str2))
                    {
                        typeName = typeName.Replace(str2, this.XsdTypeName(typeArray2[i]));
                        if (typeName.IndexOf('{') < 0)
                        {
                            return typeName;
                        }
                    }
                }
            }
            return typeName;
        }

        private enum ImportContext
        {
            Text,
            Attribute,
            Element
        }
    }
}

