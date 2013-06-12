namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;

    public class SoapReflectionImporter
    {
        private SoapAttributeOverrides attributeOverrides;
        private string defaultNs;
        private ModelScope modelScope;
        private System.Xml.Serialization.NameTable nullables;
        private StructMapping root;
        private System.Xml.Serialization.NameTable types;
        private TypeScope typeScope;

        public SoapReflectionImporter() : this(null, null)
        {
        }

        public SoapReflectionImporter(string defaultNamespace) : this(null, defaultNamespace)
        {
        }

        public SoapReflectionImporter(SoapAttributeOverrides attributeOverrides) : this(attributeOverrides, null)
        {
        }

        public SoapReflectionImporter(SoapAttributeOverrides attributeOverrides, string defaultNamespace)
        {
            this.types = new System.Xml.Serialization.NameTable();
            this.nullables = new System.Xml.Serialization.NameTable();
            if (defaultNamespace == null)
            {
                defaultNamespace = string.Empty;
            }
            if (attributeOverrides == null)
            {
                attributeOverrides = new SoapAttributeOverrides();
            }
            this.attributeOverrides = attributeOverrides;
            this.defaultNs = defaultNamespace;
            this.typeScope = new TypeScope();
            this.modelScope = new ModelScope(this.typeScope);
        }

        private static ElementAccessor CreateElementAccessor(TypeMapping mapping, string ns)
        {
            return new ElementAccessor { IsSoap = true, Name = mapping.TypeName, Namespace = ns, Mapping = mapping };
        }

        private NullableMapping CreateNullableMapping(TypeMapping baseMapping, Type type)
        {
            NullableMapping mapping2;
            TypeDesc nullableTypeDesc = baseMapping.TypeDesc.GetNullableTypeDesc(type);
            TypeMapping mapping = (TypeMapping) this.nullables[baseMapping.TypeName, baseMapping.Namespace];
            if (mapping != null)
            {
                if (mapping is NullableMapping)
                {
                    mapping2 = (NullableMapping) mapping;
                    if ((!(mapping2.BaseMapping is PrimitiveMapping) || !(baseMapping is PrimitiveMapping)) && (mapping2.BaseMapping != baseMapping))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlTypesDuplicate", new object[] { nullableTypeDesc.FullName, mapping.TypeDesc.FullName, nullableTypeDesc.Name, mapping.Namespace }));
                    }
                    return mapping2;
                }
                if (!(baseMapping is PrimitiveMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlTypesDuplicate", new object[] { nullableTypeDesc.FullName, mapping.TypeDesc.FullName, nullableTypeDesc.Name, mapping.Namespace }));
                }
            }
            mapping2 = new NullableMapping {
                BaseMapping = baseMapping,
                TypeDesc = nullableTypeDesc,
                TypeName = baseMapping.TypeName,
                Namespace = baseMapping.Namespace,
                IncludeInSchema = false
            };
            this.nullables.Add(baseMapping.TypeName, mapping2.Namespace, mapping2);
            this.typeScope.AddTypeMapping(mapping2);
            return mapping2;
        }

        private StructMapping CreateRootMapping()
        {
            TypeDesc typeDesc = this.typeScope.GetTypeDesc(typeof(object));
            return new StructMapping { IsSoap = true, TypeDesc = typeDesc, Members = new MemberMapping[0], IncludeInSchema = false, TypeName = "anyType", Namespace = "http://www.w3.org/2001/XMLSchema" };
        }

        private SoapAttributes GetAttributes(MemberInfo memberInfo)
        {
            SoapAttributes attributes = this.attributeOverrides[memberInfo.DeclaringType, memberInfo.Name];
            if (attributes != null)
            {
                return attributes;
            }
            return new SoapAttributes(memberInfo);
        }

        private SoapAttributes GetAttributes(Type type)
        {
            SoapAttributes attributes = this.attributeOverrides[type];
            if (attributes != null)
            {
                return attributes;
            }
            return new SoapAttributes(type);
        }

        private object GetDefaultValue(TypeDesc fieldTypeDesc, SoapAttributes a)
        {
            if ((a.SoapDefaultValue == null) || (a.SoapDefaultValue == DBNull.Value))
            {
                return null;
            }
            if ((fieldTypeDesc.Kind != TypeKind.Primitive) && (fieldTypeDesc.Kind != TypeKind.Enum))
            {
                a.SoapDefaultValue = null;
                return a.SoapDefaultValue;
            }
            if (fieldTypeDesc.Kind != TypeKind.Enum)
            {
                return a.SoapDefaultValue;
            }
            if (fieldTypeDesc != this.typeScope.GetTypeDesc(a.SoapDefaultValue.GetType()))
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidDefaultEnumValue", new object[] { a.SoapDefaultValue.GetType().FullName, fieldTypeDesc.FullName }));
            }
            string str = Enum.Format(a.SoapDefaultValue.GetType(), a.SoapDefaultValue, "G").Replace(",", " ");
            string str2 = Enum.Format(a.SoapDefaultValue.GetType(), a.SoapDefaultValue, "D");
            if (str == str2)
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidDefaultValue", new object[] { str, a.SoapDefaultValue.GetType().FullName }));
            }
            return str;
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

        private TypeMapping GetTypeMapping(string typeName, string ns, TypeDesc typeDesc)
        {
            TypeMapping mapping = (TypeMapping) this.types[typeName, ns];
            if (mapping == null)
            {
                return null;
            }
            if (mapping.TypeDesc != typeDesc)
            {
                throw new InvalidOperationException(Res.GetString("XmlTypesDuplicate", new object[] { typeDesc.FullName, mapping.TypeDesc.FullName, typeName, ns }));
            }
            return mapping;
        }

        private void ImportAccessorMapping(MemberMapping accessor, FieldModel model, SoapAttributes a, string ns, XmlSchemaForm form, RecursionLimiter limiter)
        {
            Type fieldType = model.FieldType;
            string name = model.Name;
            accessor.TypeDesc = this.typeScope.GetTypeDesc(fieldType);
            if (accessor.TypeDesc.IsVoid)
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidVoid"));
            }
            SoapAttributeFlags soapFlags = a.SoapFlags;
            if ((soapFlags & SoapAttributeFlags.Attribute) == SoapAttributeFlags.Attribute)
            {
                if (!accessor.TypeDesc.IsPrimitive && !accessor.TypeDesc.IsEnum)
                {
                    throw new InvalidOperationException(Res.GetString("XmlIllegalSoapAttribute", new object[] { name, accessor.TypeDesc.FullName }));
                }
                if ((soapFlags & SoapAttributeFlags.Attribute) != soapFlags)
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidElementAttribute"));
                }
                AttributeAccessor accessor2 = new AttributeAccessor {
                    Name = Accessor.EscapeQName(((a.SoapAttribute == null) || (a.SoapAttribute.AttributeName.Length == 0)) ? name : a.SoapAttribute.AttributeName),
                    Namespace = ((a.SoapAttribute == null) || (a.SoapAttribute.Namespace == null)) ? ns : a.SoapAttribute.Namespace,
                    Form = XmlSchemaForm.Qualified,
                    Mapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(fieldType), (a.SoapAttribute == null) ? string.Empty : a.SoapAttribute.DataType, limiter),
                    Default = this.GetDefaultValue(model.FieldTypeDesc, a)
                };
                accessor.Attribute = accessor2;
                accessor.Elements = new ElementAccessor[0];
            }
            else
            {
                if ((soapFlags & SoapAttributeFlags.Element) != soapFlags)
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidElementAttribute"));
                }
                ElementAccessor accessor3 = new ElementAccessor {
                    IsSoap = true,
                    Name = XmlConvert.EncodeLocalName(((a.SoapElement == null) || (a.SoapElement.ElementName.Length == 0)) ? name : a.SoapElement.ElementName),
                    Namespace = ns,
                    Form = form,
                    Mapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(fieldType), (a.SoapElement == null) ? string.Empty : a.SoapElement.DataType, limiter)
                };
                if (a.SoapElement != null)
                {
                    accessor3.IsNullable = a.SoapElement.IsNullable;
                }
                accessor.Elements = new ElementAccessor[] { accessor3 };
            }
        }

        private ArrayMapping ImportArrayLikeMapping(ArrayModel model, RecursionLimiter limiter)
        {
            ArrayMapping mapping = new ArrayMapping {
                IsSoap = true
            };
            TypeMapping mapping2 = this.ImportTypeMapping(model.Element, limiter);
            if ((mapping2.TypeDesc.IsValueType && !mapping2.TypeDesc.IsPrimitive) && !mapping2.TypeDesc.IsEnum)
            {
                throw new NotSupportedException(Res.GetString("XmlRpcArrayOfValueTypes", new object[] { model.TypeDesc.FullName }));
            }
            mapping.TypeDesc = model.TypeDesc;
            mapping.Elements = new ElementAccessor[] { CreateElementAccessor(mapping2, mapping.Namespace) };
            this.SetArrayMappingType(mapping);
            ArrayMapping next = (ArrayMapping) this.types[mapping.TypeName, mapping.Namespace];
            if (next != null)
            {
                ArrayMapping mapping4 = next;
                while (next != null)
                {
                    if (next.TypeDesc == model.TypeDesc)
                    {
                        return next;
                    }
                    next = next.Next;
                }
                mapping.Next = mapping4;
                this.types[mapping.TypeName, mapping.Namespace] = mapping;
                return mapping;
            }
            this.typeScope.AddTypeMapping(mapping);
            this.types.Add(mapping.TypeName, mapping.Namespace, mapping);
            this.IncludeTypes(model.Type);
            return mapping;
        }

        private ConstantMapping ImportConstantMapping(ConstantModel model)
        {
            SoapAttributes attributes = this.GetAttributes(model.FieldInfo);
            if (attributes.SoapIgnore)
            {
                return null;
            }
            if ((attributes.SoapFlags & ~SoapAttributeFlags.Enum) != ((SoapAttributeFlags) 0))
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidEnumAttribute"));
            }
            if (attributes.SoapEnum == null)
            {
                attributes.SoapEnum = new SoapEnumAttribute();
            }
            return new ConstantMapping { XmlName = (attributes.SoapEnum.Name.Length == 0) ? model.Name : attributes.SoapEnum.Name, Name = model.Name, Value = model.Value };
        }

        private EnumMapping ImportEnumMapping(EnumModel model)
        {
            SoapAttributes a = this.GetAttributes(model.Type);
            string defaultNs = this.defaultNs;
            if ((a.SoapType != null) && (a.SoapType.Namespace != null))
            {
                defaultNs = a.SoapType.Namespace;
            }
            string typeName = XmlConvert.EncodeLocalName(this.XsdTypeName(model.Type, a, model.TypeDesc.Name));
            EnumMapping typeMapping = (EnumMapping) this.GetTypeMapping(typeName, defaultNs, model.TypeDesc);
            if (typeMapping == null)
            {
                typeMapping = new EnumMapping {
                    IsSoap = true,
                    TypeDesc = model.TypeDesc,
                    TypeName = typeName,
                    Namespace = defaultNs,
                    IsFlags = model.Type.IsDefined(typeof(FlagsAttribute), false)
                };
                this.typeScope.AddTypeMapping(typeMapping);
                this.types.Add(typeName, defaultNs, typeMapping);
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
                typeMapping.Constants = (ConstantMapping[]) list.ToArray(typeof(ConstantMapping));
            }
            return typeMapping;
        }

        private MemberMapping ImportFieldMapping(FieldModel model, SoapAttributes a, string ns, RecursionLimiter limiter)
        {
            if (a.SoapIgnore)
            {
                return null;
            }
            MemberMapping accessor = new MemberMapping {
                IsSoap = true,
                Name = model.Name,
                CheckShouldPersist = model.CheckShouldPersist,
                CheckSpecified = model.CheckSpecified,
                ReadOnly = model.ReadOnly
            };
            this.ImportAccessorMapping(accessor, model, a, ns, XmlSchemaForm.Unqualified, limiter);
            return accessor;
        }

        private MemberMapping ImportMemberMapping(XmlReflectionMember xmlReflectionMember, string ns, XmlReflectionMember[] xmlReflectionMembers, XmlSchemaForm form, RecursionLimiter limiter)
        {
            SoapAttributes soapAttributes = xmlReflectionMember.SoapAttributes;
            if (soapAttributes.SoapIgnore)
            {
                return null;
            }
            MemberMapping accessor = new MemberMapping {
                IsSoap = true,
                Name = xmlReflectionMember.MemberName
            };
            bool checkSpecified = XmlReflectionImporter.FindSpecifiedMember(xmlReflectionMember.MemberName, xmlReflectionMembers) != null;
            FieldModel model = new FieldModel(xmlReflectionMember.MemberName, xmlReflectionMember.MemberType, this.typeScope.GetTypeDesc(xmlReflectionMember.MemberType), checkSpecified, false);
            accessor.CheckShouldPersist = model.CheckShouldPersist;
            accessor.CheckSpecified = model.CheckSpecified;
            accessor.ReadOnly = model.ReadOnly;
            this.ImportAccessorMapping(accessor, model, soapAttributes, ns, form, limiter);
            if (xmlReflectionMember.OverrideIsNullable)
            {
                accessor.Elements[0].IsNullable = false;
            }
            return accessor;
        }

        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members)
        {
            return this.ImportMembersMapping(elementName, ns, members, true, true, false);
        }

        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool writeAccessors)
        {
            return this.ImportMembersMapping(elementName, ns, members, hasWrapperElement, writeAccessors, false);
        }

        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool writeAccessors, bool validate)
        {
            return this.ImportMembersMapping(elementName, ns, members, hasWrapperElement, writeAccessors, validate, XmlMappingAccess.Write | XmlMappingAccess.Read);
        }

        private MembersMapping ImportMembersMapping(XmlReflectionMember[] xmlReflectionMembers, string ns, bool hasWrapperElement, bool writeAccessors, bool validateWrapperElement, RecursionLimiter limiter)
        {
            MembersMapping mapping = new MembersMapping {
                TypeDesc = this.typeScope.GetTypeDesc(typeof(object[]))
            };
            MemberMapping[] mappingArray = new MemberMapping[xmlReflectionMembers.Length];
            for (int i = 0; i < mappingArray.Length; i++)
            {
                try
                {
                    XmlReflectionMember xmlReflectionMember = xmlReflectionMembers[i];
                    MemberMapping mapping2 = this.ImportMemberMapping(xmlReflectionMember, ns, xmlReflectionMembers, hasWrapperElement ? XmlSchemaForm.Unqualified : XmlSchemaForm.Qualified, limiter);
                    if (xmlReflectionMember.IsReturnValue && writeAccessors)
                    {
                        if (i > 0)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlInvalidReturnPosition"));
                        }
                        mapping2.IsReturnValue = true;
                    }
                    mappingArray[i] = mapping2;
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    throw this.ReflectionException(xmlReflectionMembers[i].MemberName, exception);
                }
            }
            mapping.Members = mappingArray;
            mapping.HasWrapperElement = hasWrapperElement;
            if (hasWrapperElement)
            {
                mapping.ValidateRpcWrapperElement = validateWrapperElement;
            }
            mapping.WriteAccessors = writeAccessors;
            mapping.IsSoap = true;
            if (hasWrapperElement && !writeAccessors)
            {
                mapping.Namespace = ns;
            }
            return mapping;
        }

        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool writeAccessors, bool validate, XmlMappingAccess access)
        {
            ElementAccessor accessor = new ElementAccessor {
                IsSoap = true,
                Name = ((elementName == null) || (elementName.Length == 0)) ? elementName : XmlConvert.EncodeLocalName(elementName),
                Mapping = this.ImportMembersMapping(members, ns, hasWrapperElement, writeAccessors, validate, new RecursionLimiter())
            };
            accessor.Mapping.TypeName = elementName;
            accessor.Namespace = (accessor.Mapping.Namespace == null) ? ns : accessor.Mapping.Namespace;
            accessor.Form = XmlSchemaForm.Qualified;
            return new XmlMembersMapping(this.typeScope, accessor, access) { IsSoap = true, GenerateSerializer = true };
        }

        private PrimitiveMapping ImportPrimitiveMapping(PrimitiveModel model, string dataType)
        {
            PrimitiveMapping mapping = new PrimitiveMapping {
                IsSoap = true
            };
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
            return mapping;
        }

        private StructMapping ImportStructLikeMapping(StructModel model, RecursionLimiter limiter)
        {
            if (model.TypeDesc.Kind == TypeKind.Root)
            {
                return this.GetRootMapping();
            }
            SoapAttributes a = this.GetAttributes(model.Type);
            string defaultNs = this.defaultNs;
            if ((a.SoapType != null) && (a.SoapType.Namespace != null))
            {
                defaultNs = a.SoapType.Namespace;
            }
            string typeName = XmlConvert.EncodeLocalName(this.XsdTypeName(model.Type, a, model.TypeDesc.Name));
            StructMapping typeMapping = (StructMapping) this.GetTypeMapping(typeName, defaultNs, model.TypeDesc);
            if (typeMapping == null)
            {
                typeMapping = new StructMapping {
                    IsSoap = true,
                    TypeDesc = model.TypeDesc,
                    Namespace = defaultNs,
                    TypeName = typeName
                };
                if (a.SoapType != null)
                {
                    typeMapping.IncludeInSchema = a.SoapType.IncludeInSchema;
                }
                this.typeScope.AddTypeMapping(typeMapping);
                this.types.Add(typeName, defaultNs, typeMapping);
                if (limiter.IsExceededLimit)
                {
                    limiter.DeferredWorkItems.Add(new ImportStructWorkItem(model, typeMapping));
                    return typeMapping;
                }
                limiter.Depth++;
                this.InitializeStructMembers(typeMapping, model, limiter);
                while (limiter.DeferredWorkItems.Count > 0)
                {
                    int index = limiter.DeferredWorkItems.Count - 1;
                    ImportStructWorkItem item = limiter.DeferredWorkItems[index];
                    if (this.InitializeStructMembers(item.Mapping, item.Model, limiter))
                    {
                        limiter.DeferredWorkItems.RemoveAt(index);
                    }
                }
                limiter.Depth--;
            }
            return typeMapping;
        }

        public XmlTypeMapping ImportTypeMapping(Type type)
        {
            return this.ImportTypeMapping(type, null);
        }

        public XmlTypeMapping ImportTypeMapping(Type type, string defaultNamespace)
        {
            ElementAccessor accessor;
            accessor = new ElementAccessor {
                IsSoap = true,
                Mapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(type), new RecursionLimiter()),
                Name = accessor.Mapping.DefaultElementName,
                Namespace = (accessor.Mapping.Namespace == null) ? defaultNamespace : accessor.Mapping.Namespace,
                Form = XmlSchemaForm.Qualified
            };
            XmlTypeMapping mapping = new XmlTypeMapping(this.typeScope, accessor);
            mapping.SetKeyInternal(XmlMapping.GenerateKey(type, null, defaultNamespace));
            mapping.IsSoap = true;
            mapping.GenerateSerializer = true;
            return mapping;
        }

        private TypeMapping ImportTypeMapping(TypeModel model, RecursionLimiter limiter)
        {
            return this.ImportTypeMapping(model, string.Empty, limiter);
        }

        private TypeMapping ImportTypeMapping(TypeModel model, string dataType, RecursionLimiter limiter)
        {
            if (dataType.Length > 0)
            {
                if (!model.TypeDesc.IsPrimitive)
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidDataTypeUsage", new object[] { dataType, "SoapElementAttribute.DataType" }));
                }
                TypeDesc typeDesc = this.typeScope.GetTypeDesc(dataType, "http://www.w3.org/2001/XMLSchema");
                if (typeDesc == null)
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidXsdDataType", new object[] { dataType, "SoapElementAttribute.DataType", new XmlQualifiedName(dataType, "http://www.w3.org/2001/XMLSchema").ToString() }));
                }
                if (model.TypeDesc.FullName != typeDesc.FullName)
                {
                    throw new InvalidOperationException(Res.GetString("XmlDataTypeMismatch", new object[] { dataType, "SoapElementAttribute.DataType", model.TypeDesc.FullName }));
                }
            }
            if ((this.GetAttributes(model.Type).SoapFlags & ~SoapAttributeFlags.Type) != ((SoapAttributeFlags) 0))
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidTypeAttributes", new object[] { model.Type.FullName }));
            }
            switch (model.TypeDesc.Kind)
            {
                case TypeKind.Root:
                case TypeKind.Struct:
                case TypeKind.Class:
                {
                    if (!model.TypeDesc.IsOptionalValue)
                    {
                        return this.ImportStructLikeMapping((StructModel) model, limiter);
                    }
                    TypeDesc baseTypeDesc = model.TypeDesc.BaseTypeDesc;
                    SoapAttributes attributes = this.GetAttributes(baseTypeDesc.Type);
                    string defaultNs = this.defaultNs;
                    if ((attributes.SoapType != null) && (attributes.SoapType.Namespace != null))
                    {
                        defaultNs = attributes.SoapType.Namespace;
                    }
                    TypeDesc desc3 = string.IsNullOrEmpty(dataType) ? model.TypeDesc.BaseTypeDesc : this.typeScope.GetTypeDesc(dataType, "http://www.w3.org/2001/XMLSchema");
                    string typeName = string.IsNullOrEmpty(dataType) ? model.TypeDesc.BaseTypeDesc.Name : dataType;
                    TypeMapping baseMapping = this.GetTypeMapping(typeName, defaultNs, desc3);
                    if (baseMapping == null)
                    {
                        baseMapping = this.ImportTypeMapping(this.modelScope.GetTypeModel(baseTypeDesc.Type), dataType, limiter);
                    }
                    return this.CreateNullableMapping(baseMapping, model.TypeDesc.Type);
                }
                case TypeKind.Primitive:
                    return this.ImportPrimitiveMapping((PrimitiveModel) model, dataType);

                case TypeKind.Enum:
                    return this.ImportEnumMapping((EnumModel) model);

                case TypeKind.Array:
                case TypeKind.Collection:
                case TypeKind.Enumerable:
                    return this.ImportArrayLikeMapping((ArrayModel) model, limiter);
            }
            throw new NotSupportedException(Res.GetString("XmlUnsupportedSoapTypeKind", new object[] { model.TypeDesc.FullName }));
        }

        public void IncludeType(Type type)
        {
            this.IncludeType(type, new RecursionLimiter());
        }

        private void IncludeType(Type type, RecursionLimiter limiter)
        {
            this.ImportTypeMapping(this.modelScope.GetTypeModel(type), limiter);
        }

        public void IncludeTypes(ICustomAttributeProvider provider)
        {
            this.IncludeTypes(provider, new RecursionLimiter());
        }

        private void IncludeTypes(ICustomAttributeProvider provider, RecursionLimiter limiter)
        {
            object[] customAttributes = provider.GetCustomAttributes(typeof(SoapIncludeAttribute), false);
            for (int i = 0; i < customAttributes.Length; i++)
            {
                this.IncludeType(((SoapIncludeAttribute) customAttributes[i]).Type, limiter);
            }
        }

        private bool InitializeStructMembers(StructMapping mapping, StructModel model, RecursionLimiter limiter)
        {
            if (!mapping.IsFullyInitialized)
            {
                if (model.TypeDesc.BaseTypeDesc != null)
                {
                    StructMapping mapping2 = this.ImportStructLikeMapping((StructModel) this.modelScope.GetTypeModel(model.Type.BaseType, false), limiter);
                    int index = limiter.DeferredWorkItems.IndexOf(mapping.BaseMapping);
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
                }
                ArrayList list = new ArrayList();
                foreach (MemberInfo info in model.GetMemberInfos())
                {
                    if ((info.MemberType & (MemberTypes.Property | MemberTypes.Field)) != 0)
                    {
                        SoapAttributes a = this.GetAttributes(info);
                        if (!a.SoapIgnore)
                        {
                            FieldModel fieldModel = model.GetFieldModel(info);
                            if (fieldModel != null)
                            {
                                MemberMapping member = this.ImportFieldMapping(fieldModel, a, mapping.Namespace, limiter);
                                if (member != null)
                                {
                                    if ((!member.TypeDesc.IsPrimitive && !member.TypeDesc.IsEnum) && !member.TypeDesc.IsOptionalValue)
                                    {
                                        if (model.TypeDesc.IsValueType)
                                        {
                                            throw new NotSupportedException(Res.GetString("XmlRpcRefsInValueType", new object[] { model.TypeDesc.FullName }));
                                        }
                                        if (member.TypeDesc.IsValueType)
                                        {
                                            throw new NotSupportedException(Res.GetString("XmlRpcNestedValueType", new object[] { member.TypeDesc.FullName }));
                                        }
                                    }
                                    if ((mapping.BaseMapping == null) || !mapping.BaseMapping.Declares(member, mapping.TypeName))
                                    {
                                        list.Add(member);
                                    }
                                }
                            }
                        }
                    }
                }
                mapping.Members = (MemberMapping[]) list.ToArray(typeof(MemberMapping));
                if (mapping.BaseMapping == null)
                {
                    mapping.BaseMapping = this.GetRootMapping();
                }
                this.IncludeTypes(model.Type, limiter);
            }
            return true;
        }

        private Exception ReflectionException(string context, Exception e)
        {
            return new InvalidOperationException(Res.GetString("XmlReflectionError", new object[] { context }), e);
        }

        private void SetArrayMappingType(ArrayMapping mapping)
        {
            string typeName;
            string str2;
            TypeMapping mapping2;
            bool flag = false;
            if (mapping.Elements.Length == 1)
            {
                mapping2 = mapping.Elements[0].Mapping;
            }
            else
            {
                mapping2 = null;
            }
            if (mapping2 is EnumMapping)
            {
                str2 = mapping2.Namespace;
                typeName = mapping2.TypeName;
            }
            else if (mapping2 is PrimitiveMapping)
            {
                str2 = mapping2.TypeDesc.IsXsdType ? "http://www.w3.org/2001/XMLSchema" : "http://microsoft.com/wsdl/types/";
                typeName = mapping2.TypeDesc.DataType.Name;
                flag = true;
            }
            else if (mapping2 is StructMapping)
            {
                if (mapping2.TypeDesc.IsRoot)
                {
                    str2 = "http://www.w3.org/2001/XMLSchema";
                    typeName = "anyType";
                    flag = true;
                }
                else
                {
                    str2 = mapping2.Namespace;
                    typeName = mapping2.TypeName;
                }
            }
            else
            {
                if (!(mapping2 is ArrayMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidSoapArray", new object[] { mapping.TypeDesc.FullName }));
                }
                str2 = mapping2.Namespace;
                typeName = mapping2.TypeName;
            }
            typeName = CodeIdentifier.MakePascal(typeName);
            string str3 = "ArrayOf" + typeName;
            string str4 = flag ? this.defaultNs : str2;
            int num = 1;
            TypeMapping mapping3 = (TypeMapping) this.types[str3, str4];
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
                str3 = typeName + num.ToString(CultureInfo.InvariantCulture);
                mapping3 = (TypeMapping) this.types[str3, str4];
                num++;
            }
            mapping.Namespace = str4;
            mapping.TypeName = str3;
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
            return this.XsdTypeName(type, this.GetAttributes(type), typeDesc.Name);
        }

        internal string XsdTypeName(Type type, SoapAttributes a, string name)
        {
            string typeName = name;
            if ((a.SoapType != null) && (a.SoapType.TypeName.Length > 0))
            {
                typeName = a.SoapType.TypeName;
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
    }
}

