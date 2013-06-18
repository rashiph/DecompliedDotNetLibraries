namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Diagnostics;
    using System.Security;
    using System.Xml;
    using System.Xml.Schema;

    internal class SchemaImporter
    {
        private DataContractSet dataContractSet;
        private ICollection<XmlSchemaElement> elements;
        private XmlQualifiedName[] elementTypeNames;
        private bool importXmlDataType;
        private bool needToImportKnownTypesForObject;
        private List<XmlSchemaRedefine> redefineList;
        private Dictionary<XmlQualifiedName, SchemaObjectInfo> schemaObjects;
        private XmlSchemaSet schemaSet;
        [SecurityCritical]
        private static Hashtable serializationSchemaElements;
        private ICollection<XmlQualifiedName> typeNames;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SchemaImporter(XmlSchemaSet schemas, ICollection<XmlQualifiedName> typeNames, ICollection<XmlSchemaElement> elements, XmlQualifiedName[] elementTypeNames, DataContractSet dataContractSet, bool importXmlDataType)
        {
            this.dataContractSet = dataContractSet;
            this.schemaSet = schemas;
            this.typeNames = typeNames;
            this.elements = elements;
            this.elementTypeNames = elementTypeNames;
            this.importXmlDataType = importXmlDataType;
        }

        private void AddDataContract(DataContract dataContract)
        {
            this.dataContractSet.Add(dataContract.StableName, dataContract);
        }

        private void CheckComplexType(XmlQualifiedName typeName, XmlSchemaComplexType type)
        {
            if (type.IsAbstract)
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("AbstractTypeNotSupported"));
            }
            if (type.IsMixed)
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("MixedContentNotSupported"));
            }
        }

        private bool CheckIfCollection(XmlSchemaSequence rootSequence)
        {
            if ((rootSequence.Items == null) || (rootSequence.Items.Count == 0))
            {
                return false;
            }
            this.RemoveOptionalUnknownSerializationElements(rootSequence.Items);
            if (rootSequence.Items.Count != 1)
            {
                return false;
            }
            XmlSchemaObject obj2 = rootSequence.Items[0];
            if (!(obj2 is XmlSchemaElement))
            {
                return false;
            }
            XmlSchemaElement element = (XmlSchemaElement) obj2;
            if (!(element.MaxOccursString == "unbounded"))
            {
                return (element.MaxOccurs > 1M);
            }
            return true;
        }

        private void CheckIfElementUsesUnsupportedConstructs(XmlQualifiedName typeName, XmlSchemaElement element)
        {
            if (element.IsAbstract)
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("AbstractElementNotSupported", new object[] { element.Name }));
            }
            if (element.DefaultValue != null)
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("DefaultOnElementNotSupported", new object[] { element.Name }));
            }
            if (element.FixedValue != null)
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("FixedOnElementNotSupported", new object[] { element.Name }));
            }
            if (!element.SubstitutionGroup.IsEmpty)
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("SubstitutionGroupOnElementNotSupported", new object[] { element.Name }));
            }
        }

        private bool CheckIfEnum(XmlSchemaSimpleTypeRestriction restriction)
        {
            foreach (XmlSchemaFacet facet in restriction.Facets)
            {
                if (!(facet is XmlSchemaEnumerationFacet))
                {
                    return false;
                }
            }
            XmlQualifiedName stringQualifiedName = SchemaExporter.StringQualifiedName;
            if (restriction.BaseTypeName != XmlQualifiedName.Empty)
            {
                return (((restriction.BaseTypeName == stringQualifiedName) && (restriction.Facets.Count > 0)) || (this.ImportType(restriction.BaseTypeName) is EnumDataContract));
            }
            if (restriction.BaseType == null)
            {
                return false;
            }
            DataContract contract = this.ImportType(restriction.BaseType);
            if (!(contract.StableName == stringQualifiedName))
            {
                return (contract is EnumDataContract);
            }
            return true;
        }

        private bool CheckIfISerializable(XmlSchemaSequence rootSequence, XmlSchemaObjectCollection attributes)
        {
            if ((rootSequence.Items == null) || (rootSequence.Items.Count == 0))
            {
                return false;
            }
            this.RemoveOptionalUnknownSerializationElements(rootSequence.Items);
            if ((attributes == null) || (attributes.Count == 0))
            {
                return false;
            }
            return ((rootSequence.Items.Count == 1) && (rootSequence.Items[0] is XmlSchemaAny));
        }

        private void CheckISerializableBase(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlSchemaObjectCollection attributes)
        {
            if (rootSequence == null)
            {
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ISerializableDoesNotContainAny"));
            }
            if ((rootSequence.Items == null) || (rootSequence.Items.Count < 1))
            {
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ISerializableDoesNotContainAny"));
            }
            else if (rootSequence.Items.Count > 1)
            {
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ISerializableContainsMoreThanOneItems"));
            }
            XmlSchemaObject obj2 = rootSequence.Items[0];
            if (!(obj2 is XmlSchemaAny))
            {
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ISerializableDoesNotContainAny"));
            }
            XmlSchemaAny any = (XmlSchemaAny) obj2;
            XmlSchemaAny iSerializableWildcardElement = SchemaExporter.ISerializableWildcardElement;
            if (any.MinOccurs != iSerializableWildcardElement.MinOccurs)
            {
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ISerializableWildcardMinOccursMustBe", new object[] { iSerializableWildcardElement.MinOccurs }));
            }
            if (any.MaxOccursString != iSerializableWildcardElement.MaxOccursString)
            {
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ISerializableWildcardMaxOccursMustBe", new object[] { iSerializableWildcardElement.MaxOccursString }));
            }
            if (any.Namespace != iSerializableWildcardElement.Namespace)
            {
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ISerializableWildcardNamespaceInvalid", new object[] { iSerializableWildcardElement.Namespace }));
            }
            if (any.ProcessContents != iSerializableWildcardElement.ProcessContents)
            {
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ISerializableWildcardProcessContentsInvalid", new object[] { iSerializableWildcardElement.ProcessContents }));
            }
            XmlQualifiedName refName = SchemaExporter.ISerializableFactoryTypeAttribute.RefName;
            bool flag = false;
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Count; i++)
                {
                    obj2 = attributes[i];
                    if ((obj2 is XmlSchemaAttribute) && (((XmlSchemaAttribute) obj2).RefName == refName))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ISerializableMustRefFactoryTypeAttribute", new object[] { refName.Name, refName.Namespace }));
            }
        }

        internal static void CompileSchemaSet(XmlSchemaSet schemaSet)
        {
            if (schemaSet.Contains("http://www.w3.org/2001/XMLSchema"))
            {
                schemaSet.Compile();
            }
            else
            {
                XmlSchema schema = new XmlSchema {
                    TargetNamespace = "http://www.w3.org/2001/XMLSchema"
                };
                XmlSchemaElement item = new XmlSchemaElement {
                    Name = "schema",
                    SchemaType = new XmlSchemaComplexType()
                };
                schema.Items.Add(item);
                schemaSet.Add(schema);
                schemaSet.Compile();
            }
        }

        private List<XmlSchemaRedefine> CreateRedefineList()
        {
            List<XmlSchemaRedefine> list = new List<XmlSchemaRedefine>();
            foreach (object obj2 in this.schemaSet.Schemas())
            {
                XmlSchema schema = obj2 as XmlSchema;
                if (schema != null)
                {
                    foreach (XmlSchemaExternal external in schema.Includes)
                    {
                        XmlSchemaRedefine item = external as XmlSchemaRedefine;
                        if (item != null)
                        {
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }

        internal Dictionary<XmlQualifiedName, SchemaObjectInfo> CreateSchemaObjects()
        {
            Dictionary<XmlQualifiedName, SchemaObjectInfo> dictionary = new Dictionary<XmlQualifiedName, SchemaObjectInfo>();
            ICollection is2 = this.schemaSet.Schemas();
            List<XmlSchemaType> knownTypes = new List<XmlSchemaType>();
            dictionary.Add(SchemaExporter.AnytypeQualifiedName, new SchemaObjectInfo(null, null, null, knownTypes));
            foreach (XmlSchema schema in is2)
            {
                if (schema.TargetNamespace != "http://schemas.microsoft.com/2003/10/Serialization/")
                {
                    foreach (XmlSchemaObject obj2 in schema.SchemaTypes.Values)
                    {
                        XmlSchemaType item = obj2 as XmlSchemaType;
                        if (item != null)
                        {
                            SchemaObjectInfo info;
                            knownTypes.Add(item);
                            XmlQualifiedName key = new XmlQualifiedName(item.Name, schema.TargetNamespace);
                            if (dictionary.TryGetValue(key, out info))
                            {
                                info.type = item;
                                info.schema = schema;
                            }
                            else
                            {
                                dictionary.Add(key, new SchemaObjectInfo(item, null, schema, null));
                            }
                            XmlQualifiedName baseTypeName = this.GetBaseTypeName(item);
                            if (baseTypeName != null)
                            {
                                SchemaObjectInfo info2;
                                if (dictionary.TryGetValue(baseTypeName, out info2))
                                {
                                    if (info2.knownTypes == null)
                                    {
                                        info2.knownTypes = new List<XmlSchemaType>();
                                    }
                                }
                                else
                                {
                                    info2 = new SchemaObjectInfo(null, null, null, new List<XmlSchemaType>());
                                    dictionary.Add(baseTypeName, info2);
                                }
                                info2.knownTypes.Add(item);
                            }
                        }
                    }
                    foreach (XmlSchemaObject obj3 in schema.Elements.Values)
                    {
                        XmlSchemaElement element = obj3 as XmlSchemaElement;
                        if (element != null)
                        {
                            SchemaObjectInfo info3;
                            XmlQualifiedName name3 = new XmlQualifiedName(element.Name, schema.TargetNamespace);
                            if (dictionary.TryGetValue(name3, out info3))
                            {
                                info3.element = element;
                                info3.schema = schema;
                            }
                            else
                            {
                                dictionary.Add(name3, new SchemaObjectInfo(null, element, schema, null));
                            }
                        }
                    }
                }
            }
            return dictionary;
        }

        private XmlQualifiedName GetBaseTypeName(XmlSchemaType type)
        {
            XmlQualifiedName baseTypeName = null;
            XmlSchemaComplexType type2 = type as XmlSchemaComplexType;
            if ((type2 != null) && (type2.ContentModel != null))
            {
                XmlSchemaComplexContent contentModel = type2.ContentModel as XmlSchemaComplexContent;
                if (contentModel != null)
                {
                    XmlSchemaComplexContentExtension content = contentModel.Content as XmlSchemaComplexContentExtension;
                    if (content != null)
                    {
                        baseTypeName = content.BaseTypeName;
                    }
                }
            }
            return baseTypeName;
        }

        private GenericInfo GetGenericInfoForDataMember(DataMember dataMember)
        {
            GenericInfo info = null;
            if (dataMember.MemberTypeContract.IsValueType && dataMember.IsNullable)
            {
                info = new GenericInfo(DataContract.GetStableName(Globals.TypeOfNullable), Globals.TypeOfNullable.FullName);
                info.Add(new GenericInfo(dataMember.MemberTypeContract.StableName, null));
                return info;
            }
            return new GenericInfo(dataMember.MemberTypeContract.StableName, null);
        }

        private string GetInnerText(XmlQualifiedName typeName, XmlElement xmlElement)
        {
            if (xmlElement == null)
            {
                return null;
            }
            for (System.Xml.XmlNode node = xmlElement.FirstChild; node != null; node = node.NextSibling)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("InvalidAnnotationExpectingText", new object[] { xmlElement.LocalName, xmlElement.NamespaceURI, node.LocalName, node.NamespaceURI }));
                }
            }
            return xmlElement.InnerText;
        }

        internal void Import()
        {
            if (!this.schemaSet.Contains("http://schemas.microsoft.com/2003/10/Serialization/"))
            {
                StringReader reader = new StringReader("<?xml version='1.0' encoding='utf-8'?>\r\n<xs:schema elementFormDefault='qualified' attributeFormDefault='qualified' xmlns:tns='http://schemas.microsoft.com/2003/10/Serialization/' targetNamespace='http://schemas.microsoft.com/2003/10/Serialization/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\r\n  <xs:element name='anyType' nillable='true' type='xs:anyType' />\r\n  <xs:element name='anyURI' nillable='true' type='xs:anyURI' />\r\n  <xs:element name='base64Binary' nillable='true' type='xs:base64Binary' />\r\n  <xs:element name='boolean' nillable='true' type='xs:boolean' />\r\n  <xs:element name='byte' nillable='true' type='xs:byte' />\r\n  <xs:element name='dateTime' nillable='true' type='xs:dateTime' />\r\n  <xs:element name='decimal' nillable='true' type='xs:decimal' />\r\n  <xs:element name='double' nillable='true' type='xs:double' />\r\n  <xs:element name='float' nillable='true' type='xs:float' />\r\n  <xs:element name='int' nillable='true' type='xs:int' />\r\n  <xs:element name='long' nillable='true' type='xs:long' />\r\n  <xs:element name='QName' nillable='true' type='xs:QName' />\r\n  <xs:element name='short' nillable='true' type='xs:short' />\r\n  <xs:element name='string' nillable='true' type='xs:string' />\r\n  <xs:element name='unsignedByte' nillable='true' type='xs:unsignedByte' />\r\n  <xs:element name='unsignedInt' nillable='true' type='xs:unsignedInt' />\r\n  <xs:element name='unsignedLong' nillable='true' type='xs:unsignedLong' />\r\n  <xs:element name='unsignedShort' nillable='true' type='xs:unsignedShort' />\r\n  <xs:element name='char' nillable='true' type='tns:char' />\r\n  <xs:simpleType name='char'>\r\n    <xs:restriction base='xs:int'/>\r\n  </xs:simpleType>  \r\n  <xs:element name='duration' nillable='true' type='tns:duration' />\r\n  <xs:simpleType name='duration'>\r\n    <xs:restriction base='xs:duration'>\r\n      <xs:pattern value='\\-?P(\\d*D)?(T(\\d*H)?(\\d*M)?(\\d*(\\.\\d*)?S)?)?' />\r\n      <xs:minInclusive value='-P10675199DT2H48M5.4775808S' />\r\n      <xs:maxInclusive value='P10675199DT2H48M5.4775807S' />\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:element name='guid' nillable='true' type='tns:guid' />\r\n  <xs:simpleType name='guid'>\r\n    <xs:restriction base='xs:string'>\r\n      <xs:pattern value='[\\da-fA-F]{8}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{12}' />\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:attribute name='FactoryType' type='xs:QName' />\r\n  <xs:attribute name='Id' type='xs:ID' />\r\n  <xs:attribute name='Ref' type='xs:IDREF' />\r\n</xs:schema>\r\n");
                XmlSchema schema = XmlSchema.Read(reader, null);
                if (schema == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("CouldNotReadSerializationSchema", new object[] { "http://schemas.microsoft.com/2003/10/Serialization/" })));
                }
                this.schemaSet.Add(schema);
            }
            try
            {
                CompileSchemaSet(this.schemaSet);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("CannotImportInvalidSchemas"), exception));
            }
            if (this.typeNames == null)
            {
                foreach (object obj2 in this.schemaSet.Schemas())
                {
                    if (obj2 == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("CannotImportNullSchema")));
                    }
                    XmlSchema schema2 = (XmlSchema) obj2;
                    if ((schema2.TargetNamespace != "http://schemas.microsoft.com/2003/10/Serialization/") && (schema2.TargetNamespace != "http://www.w3.org/2001/XMLSchema"))
                    {
                        foreach (XmlSchemaObject obj3 in schema2.SchemaTypes.Values)
                        {
                            this.ImportType((XmlSchemaType) obj3);
                        }
                        foreach (XmlSchemaElement element in schema2.Elements.Values)
                        {
                            if (element.SchemaType != null)
                            {
                                this.ImportAnonymousGlobalElement(element, element.QualifiedName, schema2.TargetNamespace);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (XmlQualifiedName name in this.typeNames)
                {
                    if (name == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("CannotImportNullDataContractName")));
                    }
                    this.ImportType(name);
                }
                if (this.elements != null)
                {
                    int num = 0;
                    foreach (XmlSchemaElement element2 in this.elements)
                    {
                        XmlQualifiedName schemaTypeName = element2.SchemaTypeName;
                        if ((schemaTypeName != null) && (schemaTypeName.Name.Length > 0))
                        {
                            this.elementTypeNames[num++] = this.ImportType(schemaTypeName).StableName;
                        }
                        else
                        {
                            XmlSchema schemaWithGlobalElementDeclaration = SchemaHelper.GetSchemaWithGlobalElementDeclaration(element2, this.schemaSet);
                            if (schemaWithGlobalElementDeclaration == null)
                            {
                                this.elementTypeNames[num++] = this.ImportAnonymousElement(element2, element2.QualifiedName).StableName;
                            }
                            else
                            {
                                this.elementTypeNames[num++] = this.ImportAnonymousGlobalElement(element2, element2.QualifiedName, schemaWithGlobalElementDeclaration.TargetNamespace).StableName;
                            }
                        }
                    }
                }
            }
            this.ImportKnownTypesForObject();
        }

        internal static XmlQualifiedName ImportActualType(XmlSchemaAnnotation annotation, XmlQualifiedName defaultTypeName, XmlQualifiedName typeName)
        {
            XmlElement element = ImportAnnotation(annotation, SchemaExporter.ActualTypeAnnotationName);
            if (element == null)
            {
                return defaultTypeName;
            }
            System.Xml.XmlNode namedItem = element.Attributes.GetNamedItem("Name");
            string name = (namedItem == null) ? null : namedItem.Value;
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("AnnotationAttributeNotFound", new object[] { SchemaExporter.ActualTypeAnnotationName.Name, typeName.Name, typeName.Namespace, "Name" })));
            }
            System.Xml.XmlNode node2 = element.Attributes.GetNamedItem("Namespace");
            string ns = (node2 == null) ? null : node2.Value;
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("AnnotationAttributeNotFound", new object[] { SchemaExporter.ActualTypeAnnotationName.Name, typeName.Name, typeName.Namespace, "Namespace" })));
            }
            return new XmlQualifiedName(name, ns);
        }

        private static XmlElement ImportAnnotation(XmlSchemaAnnotation annotation, XmlQualifiedName annotationQualifiedName)
        {
            if (((annotation != null) && (annotation.Items != null)) && ((annotation.Items.Count > 0) && (annotation.Items[0] is XmlSchemaAppInfo)))
            {
                XmlSchemaAppInfo info = (XmlSchemaAppInfo) annotation.Items[0];
                System.Xml.XmlNode[] markup = info.Markup;
                if (markup != null)
                {
                    for (int i = 0; i < markup.Length; i++)
                    {
                        XmlElement element = markup[i] as XmlElement;
                        if (((element != null) && (element.LocalName == annotationQualifiedName.Name)) && (element.NamespaceURI == annotationQualifiedName.Namespace))
                        {
                            return element;
                        }
                    }
                }
            }
            return null;
        }

        private DataContract ImportAnonymousElement(XmlSchemaElement element, XmlQualifiedName typeQName)
        {
            if (SchemaHelper.GetSchemaType(this.SchemaObjects, typeQName) != null)
            {
                int num = 1;
                while (true)
                {
                    typeQName = new XmlQualifiedName(typeQName.Name + num.ToString(NumberFormatInfo.InvariantInfo), typeQName.Namespace);
                    if (SchemaHelper.GetSchemaType(this.SchemaObjects, typeQName) == null)
                    {
                        break;
                    }
                    if (num == 0x7fffffff)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("CannotComputeUniqueName", new object[] { element.Name })));
                    }
                    num++;
                }
            }
            if (element.SchemaType == null)
            {
                return this.ImportType(SchemaExporter.AnytypeQualifiedName);
            }
            return this.ImportType(element.SchemaType, typeQName, true);
        }

        [SecuritySafeCritical]
        private DataContract ImportAnonymousGlobalElement(XmlSchemaElement element, XmlQualifiedName typeQName, string ns)
        {
            DataContract contract = this.ImportAnonymousElement(element, typeQName);
            XmlDataContract contract2 = contract as XmlDataContract;
            if (contract2 != null)
            {
                contract2.SetTopLevelElementName(new XmlQualifiedName(element.Name, ns));
                contract2.IsTopLevelElementNullable = element.IsNillable;
            }
            return contract;
        }

        private void ImportAttributes(XmlQualifiedName typeName, XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute anyAttribute, out bool isReference)
        {
            if (anyAttribute != null)
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("AnyAttributeNotSupported"));
            }
            isReference = false;
            if (attributes != null)
            {
                bool foundAttribute = false;
                bool flag2 = false;
                for (int i = 0; i < attributes.Count; i++)
                {
                    XmlSchemaObject obj2 = attributes[i];
                    if (obj2 is XmlSchemaAttribute)
                    {
                        XmlSchemaAttribute attribute = (XmlSchemaAttribute) obj2;
                        if ((((attribute.Use != XmlSchemaUse.Prohibited) && !this.TryCheckIfAttribute(typeName, attribute, Globals.IdQualifiedName, ref foundAttribute)) && !this.TryCheckIfAttribute(typeName, attribute, Globals.RefQualifiedName, ref flag2)) && ((attribute.RefName.IsEmpty || (attribute.RefName.Namespace != "http://schemas.microsoft.com/2003/10/Serialization/")) || (attribute.Use == XmlSchemaUse.Required)))
                        {
                            ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("TypeShouldNotContainAttributes", new object[] { "http://schemas.microsoft.com/2003/10/Serialization/" }));
                        }
                    }
                }
                isReference = foundAttribute && flag2;
            }
        }

        [SecuritySafeCritical]
        private void ImportBaseContract(XmlQualifiedName baseTypeName, ClassDataContract dataContract)
        {
            ClassDataContract contract = this.ImportType(baseTypeName) as ClassDataContract;
            if (contract == null)
            {
                ThrowTypeCannotBeImportedException(dataContract.StableName.Name, dataContract.StableName.Namespace, System.Runtime.Serialization.SR.GetString(dataContract.IsISerializable ? "InvalidISerializableDerivation" : "InvalidClassDerivation", new object[] { baseTypeName.Name, baseTypeName.Namespace }));
            }
            if (contract.IsValueType)
            {
                contract.IsValueType = false;
            }
            for (ClassDataContract contract2 = contract; contract2 != null; contract2 = contract2.BaseContract)
            {
                Dictionary<XmlQualifiedName, DataContract> knownDataContracts = contract2.KnownDataContracts;
                if (knownDataContracts == null)
                {
                    knownDataContracts = new Dictionary<XmlQualifiedName, DataContract>();
                    contract2.KnownDataContracts = knownDataContracts;
                }
                knownDataContracts.Add(dataContract.StableName, dataContract);
            }
            dataContract.BaseContract = contract;
        }

        [SecuritySafeCritical]
        private ClassDataContract ImportClass(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlQualifiedName baseTypeName, XmlSchemaAnnotation annotation, bool isReference)
        {
            ClassDataContract dataContract = new ClassDataContract {
                StableName = typeName
            };
            this.AddDataContract(dataContract);
            dataContract.IsValueType = this.IsValueType(typeName, annotation);
            dataContract.IsReference = isReference;
            if (baseTypeName != null)
            {
                this.ImportBaseContract(baseTypeName, dataContract);
                if (dataContract.BaseContract.IsISerializable)
                {
                    if (this.IsISerializableDerived(typeName, rootSequence))
                    {
                        dataContract.IsISerializable = true;
                    }
                    else
                    {
                        ThrowTypeCannotBeImportedException(dataContract.StableName.Name, dataContract.StableName.Namespace, System.Runtime.Serialization.SR.GetString("DerivedTypeNotISerializable", new object[] { baseTypeName.Name, baseTypeName.Namespace }));
                    }
                }
                if (dataContract.BaseContract.IsReference)
                {
                    dataContract.IsReference = true;
                }
            }
            if (!dataContract.IsISerializable)
            {
                dataContract.Members = new List<DataMember>();
                this.RemoveOptionalUnknownSerializationElements(rootSequence.Items);
                for (int i = 0; i < rootSequence.Items.Count; i++)
                {
                    XmlSchemaElement element = rootSequence.Items[i] as XmlSchemaElement;
                    if (element == null)
                    {
                        ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("MustContainOnlyLocalElements"));
                    }
                    this.ImportClassMember(element, dataContract);
                }
            }
            return dataContract;
        }

        private void ImportClassMember(XmlSchemaElement element, ClassDataContract dataContract)
        {
            bool flag3;
            XmlQualifiedName stableName = dataContract.StableName;
            if (element.MinOccurs > 1M)
            {
                ThrowTypeCannotBeImportedException(stableName.Name, stableName.Namespace, System.Runtime.Serialization.SR.GetString("ElementMinOccursMustBe", new object[] { element.Name }));
            }
            if (element.MaxOccurs != 1M)
            {
                ThrowTypeCannotBeImportedException(stableName.Name, stableName.Namespace, System.Runtime.Serialization.SR.GetString("ElementMaxOccursMustBe", new object[] { element.Name }));
            }
            DataContract contract = null;
            string name = element.Name;
            bool isRequired = element.MinOccurs > 0M;
            bool isNillable = element.IsNillable;
            int order = 0;
            XmlSchemaForm form = (element.Form == XmlSchemaForm.None) ? SchemaHelper.GetSchemaWithType(this.SchemaObjects, this.schemaSet, stableName).ElementFormDefault : element.Form;
            if (form != XmlSchemaForm.Qualified)
            {
                ThrowTypeCannotBeImportedException(stableName.Name, stableName.Namespace, System.Runtime.Serialization.SR.GetString("FormMustBeQualified", new object[] { element.Name }));
            }
            this.CheckIfElementUsesUnsupportedConstructs(stableName, element);
            if (element.SchemaTypeName.IsEmpty)
            {
                if (element.SchemaType != null)
                {
                    contract = this.ImportAnonymousElement(element, new XmlQualifiedName(string.Format(CultureInfo.InvariantCulture, "{0}.{1}Type", new object[] { stableName.Name, element.Name }), stableName.Namespace));
                }
                else if (!element.RefName.IsEmpty)
                {
                    ThrowTypeCannotBeImportedException(stableName.Name, stableName.Namespace, System.Runtime.Serialization.SR.GetString("ElementRefOnLocalElementNotSupported", new object[] { element.RefName.Name, element.RefName.Namespace }));
                }
                else
                {
                    contract = this.ImportType(SchemaExporter.AnytypeQualifiedName);
                }
            }
            else
            {
                XmlQualifiedName typeName = ImportActualType(element.Annotation, element.SchemaTypeName, stableName);
                contract = this.ImportType(typeName);
                if (IsObjectContract(contract))
                {
                    this.needToImportKnownTypesForObject = true;
                }
            }
            bool? nullable = this.ImportEmitDefaultValue(element.Annotation, stableName);
            if (!contract.IsValueType && !isNillable)
            {
                if (nullable.HasValue && nullable.Value)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidEmitDefaultAnnotation", new object[] { name, stableName.Name, stableName.Namespace })));
                }
                flag3 = false;
            }
            else
            {
                flag3 = nullable.HasValue ? nullable.Value : true;
            }
            int num2 = dataContract.Members.Count - 1;
            if (num2 >= 0)
            {
                DataMember x = dataContract.Members[num2];
                if (x.Order > 0)
                {
                    order = dataContract.Members.Count;
                }
                DataMember y = new DataMember(contract, name, isNillable, isRequired, flag3, order);
                int num3 = ClassDataContract.DataMemberComparer.Singleton.Compare(x, y);
                if (num3 == 0)
                {
                    ThrowTypeCannotBeImportedException(stableName.Name, stableName.Namespace, System.Runtime.Serialization.SR.GetString("CannotHaveDuplicateElementNames", new object[] { name }));
                }
                else if (num3 > 0)
                {
                    order = dataContract.Members.Count;
                }
            }
            DataMember key = new DataMember(contract, name, isNillable, isRequired, flag3, order);
            XmlQualifiedName surrogateDataAnnotationName = SchemaExporter.SurrogateDataAnnotationName;
            this.dataContractSet.SetSurrogateData(key, this.ImportSurrogateData(ImportAnnotation(element.Annotation, surrogateDataAnnotationName), surrogateDataAnnotationName.Name, surrogateDataAnnotationName.Namespace));
            dataContract.Members.Add(key);
        }

        [SecuritySafeCritical]
        private CollectionDataContract ImportCollection(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlSchemaObjectCollection attributes, XmlSchemaAnnotation annotation, bool isReference)
        {
            CollectionDataContract dataContract = new CollectionDataContract(CollectionKind.Array) {
                StableName = typeName
            };
            this.AddDataContract(dataContract);
            dataContract.IsReference = isReference;
            XmlSchemaElement element = (XmlSchemaElement) rootSequence.Items[0];
            dataContract.IsItemTypeNullable = element.IsNillable;
            dataContract.ItemName = element.Name;
            XmlSchemaForm form = (element.Form == XmlSchemaForm.None) ? SchemaHelper.GetSchemaWithType(this.SchemaObjects, this.schemaSet, typeName).ElementFormDefault : element.Form;
            if (form != XmlSchemaForm.Qualified)
            {
                ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ArrayItemFormMustBe", new object[] { element.Name }));
            }
            this.CheckIfElementUsesUnsupportedConstructs(typeName, element);
            if (element.SchemaTypeName.IsEmpty)
            {
                if (element.SchemaType != null)
                {
                    XmlQualifiedName typeQName = new XmlQualifiedName(element.Name, typeName.Namespace);
                    if (this.dataContractSet[typeQName] == null)
                    {
                        dataContract.ItemContract = this.ImportAnonymousElement(element, typeQName);
                    }
                    else
                    {
                        XmlQualifiedName name2 = new XmlQualifiedName(string.Format(CultureInfo.InvariantCulture, "{0}.{1}Type", new object[] { typeName.Name, element.Name }), typeName.Namespace);
                        dataContract.ItemContract = this.ImportAnonymousElement(element, name2);
                    }
                }
                else if (!element.RefName.IsEmpty)
                {
                    ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ElementRefOnLocalElementNotSupported", new object[] { element.RefName.Name, element.RefName.Namespace }));
                }
                else
                {
                    dataContract.ItemContract = this.ImportType(SchemaExporter.AnytypeQualifiedName);
                }
            }
            else
            {
                dataContract.ItemContract = this.ImportType(element.SchemaTypeName);
            }
            if (this.IsDictionary(typeName, annotation))
            {
                ClassDataContract itemContract = dataContract.ItemContract as ClassDataContract;
                DataMember dataMember = null;
                DataMember member2 = null;
                if (((itemContract == null) || (itemContract.Members == null)) || (((itemContract.Members.Count != 2) || !(dataMember = itemContract.Members[0]).IsRequired) || !(member2 = itemContract.Members[1]).IsRequired))
                {
                    ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("InvalidKeyValueType", new object[] { element.Name }));
                }
                if (itemContract.Namespace != dataContract.Namespace)
                {
                    ThrowArrayTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("InvalidKeyValueTypeNamespace", new object[] { element.Name, itemContract.Namespace }));
                }
                itemContract.IsValueType = true;
                dataContract.KeyName = dataMember.Name;
                dataContract.ValueName = member2.Name;
                if (element.SchemaType != null)
                {
                    this.dataContractSet.Remove(itemContract.StableName);
                    GenericInfo info = new GenericInfo(DataContract.GetStableName(Globals.TypeOfKeyValue), Globals.TypeOfKeyValue.FullName);
                    info.Add(this.GetGenericInfoForDataMember(dataMember));
                    info.Add(this.GetGenericInfoForDataMember(member2));
                    info.AddToLevel(0, 2);
                    dataContract.ItemContract.StableName = new XmlQualifiedName(info.GetExpandedStableName().Name, typeName.Namespace);
                }
            }
            return dataContract;
        }

        private void ImportDataContractExtension(XmlSchemaType type, DataContract dataContract)
        {
            if ((type.Annotation != null) && (type.Annotation.Items != null))
            {
                foreach (XmlSchemaAppInfo info in type.Annotation.Items)
                {
                    if ((info != null) && (info.Markup != null))
                    {
                        foreach (System.Xml.XmlNode node in info.Markup)
                        {
                            XmlElement typeElement = node as XmlElement;
                            XmlQualifiedName surrogateDataAnnotationName = SchemaExporter.SurrogateDataAnnotationName;
                            if (((typeElement != null) && (typeElement.NamespaceURI == surrogateDataAnnotationName.Namespace)) && (typeElement.LocalName == surrogateDataAnnotationName.Name))
                            {
                                object surrogateData = this.ImportSurrogateData(typeElement, surrogateDataAnnotationName.Name, surrogateDataAnnotationName.Namespace);
                                this.dataContractSet.SetSurrogateData(dataContract, surrogateData);
                            }
                        }
                    }
                }
            }
        }

        private bool? ImportEmitDefaultValue(XmlSchemaAnnotation annotation, XmlQualifiedName typeName)
        {
            XmlElement element = ImportAnnotation(annotation, SchemaExporter.DefaultValueAnnotation);
            if (element == null)
            {
                return null;
            }
            System.Xml.XmlNode namedItem = element.Attributes.GetNamedItem("EmitDefaultValue");
            string s = (namedItem == null) ? null : namedItem.Value;
            if (s == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("AnnotationAttributeNotFound", new object[] { SchemaExporter.DefaultValueAnnotation.Name, typeName.Name, typeName.Namespace, "EmitDefaultValue" })));
            }
            return new bool?(XmlConvert.ToBoolean(s));
        }

        [SecuritySafeCritical]
        private EnumDataContract ImportEnum(XmlQualifiedName typeName, XmlSchemaSimpleTypeRestriction restriction, bool isFlags, XmlSchemaAnnotation annotation)
        {
            EnumDataContract dataContract = new EnumDataContract {
                StableName = typeName,
                BaseContractName = ImportActualType(annotation, SchemaExporter.DefaultEnumBaseTypeName, typeName),
                IsFlags = isFlags
            };
            this.AddDataContract(dataContract);
            dataContract.Values = new List<long>();
            dataContract.Members = new List<DataMember>();
            foreach (XmlSchemaFacet facet in restriction.Facets)
            {
                XmlSchemaEnumerationFacet facet2 = facet as XmlSchemaEnumerationFacet;
                if (facet2 == null)
                {
                    ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("EnumOnlyEnumerationFacetsSupported"));
                }
                if (facet2.Value == null)
                {
                    ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("EnumEnumerationFacetsMustHaveValue"));
                }
                string innerText = this.GetInnerText(typeName, ImportAnnotation(facet2.Annotation, SchemaExporter.EnumerationValueAnnotationName));
                if (innerText == null)
                {
                    dataContract.Values.Add(SchemaExporter.GetDefaultEnumValue(isFlags, dataContract.Members.Count));
                }
                else
                {
                    dataContract.Values.Add(dataContract.GetEnumValueFromString(innerText));
                }
                DataMember item = new DataMember(facet2.Value);
                dataContract.Members.Add(item);
            }
            return dataContract;
        }

        private EnumDataContract ImportFlagsEnum(XmlQualifiedName typeName, XmlSchemaSimpleTypeList list, XmlSchemaAnnotation annotation)
        {
            XmlSchemaSimpleType itemType = list.ItemType;
            if (itemType == null)
            {
                ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("EnumListMustContainAnonymousType"));
            }
            XmlSchemaSimpleTypeContent content = itemType.Content;
            if (content is XmlSchemaSimpleTypeUnion)
            {
                ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("EnumUnionInAnonymousTypeNotSupported"));
            }
            else if (content is XmlSchemaSimpleTypeList)
            {
                ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("EnumListInAnonymousTypeNotSupported"));
            }
            else if (content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction) content;
                if (this.CheckIfEnum(restriction))
                {
                    return this.ImportEnum(typeName, restriction, true, annotation);
                }
                ThrowEnumTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("EnumRestrictionInvalid"));
            }
            return null;
        }

        [SecuritySafeCritical]
        private void ImportGenericInfo(XmlSchemaType type, DataContract dataContract)
        {
            if ((type.Annotation != null) && (type.Annotation.Items != null))
            {
                foreach (XmlSchemaAppInfo info in type.Annotation.Items)
                {
                    if ((info != null) && (info.Markup != null))
                    {
                        foreach (System.Xml.XmlNode node in info.Markup)
                        {
                            XmlElement typeElement = node as XmlElement;
                            if (((typeElement != null) && (typeElement.NamespaceURI == "http://schemas.microsoft.com/2003/10/Serialization/")) && (typeElement.LocalName == "GenericType"))
                            {
                                dataContract.GenericInfo = this.ImportGenericInfo(typeElement, type);
                            }
                        }
                    }
                }
            }
        }

        private GenericInfo ImportGenericInfo(XmlElement typeElement, XmlSchemaType type)
        {
            System.Xml.XmlNode namedItem = typeElement.Attributes.GetNamedItem("Name");
            string localName = (namedItem == null) ? null : namedItem.Value;
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GenericAnnotationAttributeNotFound", new object[] { type.Name, "Name" })));
            }
            System.Xml.XmlNode node2 = typeElement.Attributes.GetNamedItem("Namespace");
            string ns = (node2 == null) ? null : node2.Value;
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GenericAnnotationAttributeNotFound", new object[] { type.Name, "Namespace" })));
            }
            if (typeElement.ChildNodes.Count > 0)
            {
                localName = DataContract.EncodeLocalName(localName);
            }
            int num = 0;
            GenericInfo info = new GenericInfo(new XmlQualifiedName(localName, ns), type.Name);
            foreach (System.Xml.XmlNode node3 in typeElement.ChildNodes)
            {
                XmlElement element = node3 as XmlElement;
                if (element != null)
                {
                    if ((element.LocalName != "GenericParameter") || (element.NamespaceURI != "http://schemas.microsoft.com/2003/10/Serialization/"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GenericAnnotationHasInvalidElement", new object[] { element.LocalName, element.NamespaceURI, type.Name })));
                    }
                    System.Xml.XmlNode node4 = element.Attributes.GetNamedItem("NestedLevel");
                    int result = 0;
                    if ((node4 != null) && !int.TryParse(node4.Value, out result))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GenericAnnotationHasInvalidAttributeValue", new object[] { element.LocalName, element.NamespaceURI, type.Name, node4.Value, node4.LocalName, Globals.TypeOfInt.Name })));
                    }
                    if (result < num)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GenericAnnotationForNestedLevelMustBeIncreasing", new object[] { element.LocalName, element.NamespaceURI, type.Name })));
                    }
                    info.Add(this.ImportGenericInfo(element, type));
                    info.AddToLevel(result, 1);
                    num = result;
                }
            }
            System.Xml.XmlNode node5 = typeElement.Attributes.GetNamedItem("NestedLevel");
            if (node5 != null)
            {
                int num3 = 0;
                if (!int.TryParse(node5.Value, out num3))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GenericAnnotationHasInvalidAttributeValue", new object[] { typeElement.LocalName, typeElement.NamespaceURI, type.Name, node5.Value, node5.LocalName, Globals.TypeOfInt.Name })));
                }
                if ((num3 - 1) > num)
                {
                    info.AddToLevel(num3 - 1, 0);
                }
            }
            return info;
        }

        [SecuritySafeCritical]
        private ClassDataContract ImportISerializable(XmlQualifiedName typeName, XmlSchemaSequence rootSequence, XmlQualifiedName baseTypeName, XmlSchemaObjectCollection attributes, XmlSchemaAnnotation annotation)
        {
            ClassDataContract dataContract = new ClassDataContract {
                StableName = typeName,
                IsISerializable = true
            };
            this.AddDataContract(dataContract);
            dataContract.IsValueType = this.IsValueType(typeName, annotation);
            if (baseTypeName == null)
            {
                this.CheckISerializableBase(typeName, rootSequence, attributes);
                return dataContract;
            }
            this.ImportBaseContract(baseTypeName, dataContract);
            if (!dataContract.BaseContract.IsISerializable)
            {
                ThrowISerializableTypeCannotBeImportedException(dataContract.StableName.Name, dataContract.StableName.Namespace, System.Runtime.Serialization.SR.GetString("BaseTypeNotISerializable", new object[] { baseTypeName.Name, baseTypeName.Namespace }));
            }
            if (!this.IsISerializableDerived(typeName, rootSequence))
            {
                ThrowISerializableTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ISerializableDerivedContainsOneOrMoreItems"));
            }
            return dataContract;
        }

        private void ImportKnownTypes(XmlQualifiedName typeName)
        {
            SchemaObjectInfo info;
            if (this.SchemaObjects.TryGetValue(typeName, out info))
            {
                List<XmlSchemaType> knownTypes = info.knownTypes;
                if (knownTypes != null)
                {
                    foreach (XmlSchemaType type in knownTypes)
                    {
                        this.ImportType(type);
                    }
                }
            }
        }

        private void ImportKnownTypesForObject()
        {
            if (this.needToImportKnownTypesForObject)
            {
                SchemaObjectInfo info;
                this.needToImportKnownTypesForObject = false;
                if ((this.dataContractSet.KnownTypesForObject == null) && this.SchemaObjects.TryGetValue(SchemaExporter.AnytypeQualifiedName, out info))
                {
                    List<XmlSchemaType> knownTypes = info.knownTypes;
                    if (knownTypes != null)
                    {
                        Dictionary<XmlQualifiedName, DataContract> dictionary = new Dictionary<XmlQualifiedName, DataContract>();
                        foreach (XmlSchemaType type in knownTypes)
                        {
                            DataContract contract2;
                            DataContract contract = this.ImportType(type);
                            if (!dictionary.TryGetValue(contract.StableName, out contract2))
                            {
                                dictionary.Add(contract.StableName, contract);
                            }
                        }
                        this.dataContractSet.KnownTypesForObject = dictionary;
                    }
                }
            }
        }

        private DataContract ImportSimpleTypeRestriction(XmlQualifiedName typeName, XmlSchemaSimpleTypeRestriction restriction)
        {
            DataContract dataContract = null;
            if (!restriction.BaseTypeName.IsEmpty)
            {
                dataContract = this.ImportType(restriction.BaseTypeName);
            }
            else if (restriction.BaseType != null)
            {
                dataContract = this.ImportType(restriction.BaseType);
            }
            else
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("SimpleTypeRestrictionDoesNotSpecifyBase"));
            }
            if (dataContract.IsBuiltInDataContract)
            {
                this.dataContractSet.InternalAdd(typeName, dataContract);
            }
            return dataContract;
        }

        private XmlDataContract ImportSpecialXmlDataType(XmlSchemaType xsdType, bool isAnonymous)
        {
            if (isAnonymous)
            {
                XmlSchemaComplexType type = xsdType as XmlSchemaComplexType;
                if (type == null)
                {
                    return null;
                }
                if (this.IsXmlAnyElementType(type))
                {
                    Type type2;
                    XmlQualifiedName stableName = new XmlQualifiedName("XElement", "http://schemas.datacontract.org/2004/07/System.Xml.Linq");
                    if (this.dataContractSet.TryGetReferencedType(stableName, null, out type2) && Globals.TypeOfIXmlSerializable.IsAssignableFrom(type2))
                    {
                        XmlDataContract dataContract = new XmlDataContract(type2);
                        this.AddDataContract(dataContract);
                        return dataContract;
                    }
                    return (XmlDataContract) DataContract.GetBuiltInDataContract(Globals.TypeOfXmlElement);
                }
                if (this.IsXmlAnyType(type))
                {
                    return (XmlDataContract) DataContract.GetBuiltInDataContract(Globals.TypeOfXmlNodeArray);
                }
            }
            return null;
        }

        private object ImportSurrogateData(XmlElement typeElement, string name, string ns)
        {
            if ((this.dataContractSet.DataContractSurrogate != null) && (typeElement != null))
            {
                Collection<Type> customDataTypes = new Collection<Type>();
                DataContractSurrogateCaller.GetKnownCustomDataTypes(this.dataContractSet.DataContractSurrogate, customDataTypes);
                DataContractSerializer serializer = new DataContractSerializer(Globals.TypeOfObject, name, ns, customDataTypes, 0x7fffffff, false, true, null);
                return serializer.ReadObject(new XmlNodeReader(typeElement));
            }
            return null;
        }

        private void ImportTopLevelElement(XmlQualifiedName typeName)
        {
            XmlSchemaElement schemaElement = SchemaHelper.GetSchemaElement(this.SchemaObjects, typeName);
            if (schemaElement != null)
            {
                XmlQualifiedName schemaTypeName = schemaElement.SchemaTypeName;
                if (schemaTypeName.IsEmpty)
                {
                    if (schemaElement.SchemaType != null)
                    {
                        ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("AnonymousTypeNotSupported", new object[] { typeName.Name, typeName.Namespace }));
                    }
                    else
                    {
                        schemaTypeName = SchemaExporter.AnytypeQualifiedName;
                    }
                }
                if (schemaTypeName != typeName)
                {
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("TopLevelElementRepresentsDifferentType", new object[] { schemaElement.SchemaTypeName.Name, schemaElement.SchemaTypeName.Namespace }));
                }
                this.CheckIfElementUsesUnsupportedConstructs(typeName, schemaElement);
            }
        }

        private DataContract ImportType(XmlSchemaType type)
        {
            return this.ImportType(type, type.QualifiedName, false);
        }

        private DataContract ImportType(XmlQualifiedName typeName)
        {
            DataContract builtInDataContract = DataContract.GetBuiltInDataContract(typeName.Name, typeName.Namespace);
            if (builtInDataContract == null)
            {
                XmlSchemaType schemaType = SchemaHelper.GetSchemaType(this.SchemaObjects, typeName);
                if (schemaType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("SpecifiedTypeNotFoundInSchema", new object[] { typeName.Name, typeName.Namespace })));
                }
                builtInDataContract = this.ImportType(schemaType);
            }
            if (IsObjectContract(builtInDataContract))
            {
                this.needToImportKnownTypesForObject = true;
            }
            return builtInDataContract;
        }

        private DataContract ImportType(XmlSchemaType type, XmlQualifiedName typeName, bool isAnonymous)
        {
            InvalidDataContractException exception;
            Type type4;
            DataContract dataContract = this.dataContractSet[typeName];
            if (dataContract != null)
            {
                return dataContract;
            }
            try
            {
                foreach (XmlSchemaRedefine redefine in this.RedefineList)
                {
                    if (redefine.SchemaTypes[typeName] != null)
                    {
                        ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("RedefineNotSupported"));
                    }
                }
                if (type is XmlSchemaSimpleType)
                {
                    XmlSchemaSimpleType type2 = (XmlSchemaSimpleType) type;
                    XmlSchemaSimpleTypeContent content = type2.Content;
                    if (content is XmlSchemaSimpleTypeUnion)
                    {
                        ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("SimpleTypeUnionNotSupported"));
                    }
                    else if (content is XmlSchemaSimpleTypeList)
                    {
                        dataContract = this.ImportFlagsEnum(typeName, (XmlSchemaSimpleTypeList) content, type2.Annotation);
                    }
                    else if (content is XmlSchemaSimpleTypeRestriction)
                    {
                        XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction) content;
                        if (this.CheckIfEnum(restriction))
                        {
                            dataContract = this.ImportEnum(typeName, restriction, false, type2.Annotation);
                        }
                        else
                        {
                            dataContract = this.ImportSimpleTypeRestriction(typeName, restriction);
                        }
                    }
                }
                else if (type is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType type3 = (XmlSchemaComplexType) type;
                    if (type3.ContentModel == null)
                    {
                        this.CheckComplexType(typeName, type3);
                        dataContract = this.ImportType(typeName, type3.Particle, type3.Attributes, type3.AnyAttribute, null, type3.Annotation);
                    }
                    else
                    {
                        XmlSchemaContentModel contentModel = type3.ContentModel;
                        if (contentModel is XmlSchemaSimpleContent)
                        {
                            ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("SimpleContentNotSupported"));
                        }
                        else if (contentModel is XmlSchemaComplexContent)
                        {
                            XmlSchemaComplexContent content2 = (XmlSchemaComplexContent) contentModel;
                            if (content2.IsMixed)
                            {
                                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("MixedContentNotSupported"));
                            }
                            if (content2.Content is XmlSchemaComplexContentExtension)
                            {
                                XmlSchemaComplexContentExtension extension = (XmlSchemaComplexContentExtension) content2.Content;
                                dataContract = this.ImportType(typeName, extension.Particle, extension.Attributes, extension.AnyAttribute, extension.BaseTypeName, type3.Annotation);
                            }
                            else if (content2.Content is XmlSchemaComplexContentRestriction)
                            {
                                XmlSchemaComplexContentRestriction restriction2 = (XmlSchemaComplexContentRestriction) content2.Content;
                                if (restriction2.BaseTypeName == SchemaExporter.AnytypeQualifiedName)
                                {
                                    dataContract = this.ImportType(typeName, restriction2.Particle, restriction2.Attributes, restriction2.AnyAttribute, null, type3.Annotation);
                                }
                                else
                                {
                                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("ComplexTypeRestrictionNotSupported"));
                                }
                            }
                        }
                    }
                }
                if (dataContract == null)
                {
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, string.Empty);
                }
                if (type.QualifiedName != XmlQualifiedName.Empty)
                {
                    this.ImportTopLevelElement(typeName);
                }
                this.ImportDataContractExtension(type, dataContract);
                this.ImportGenericInfo(type, dataContract);
                this.ImportKnownTypes(typeName);
                return dataContract;
            }
            catch (InvalidDataContractException exception2)
            {
                exception = exception2;
            }
            if (this.importXmlDataType)
            {
                this.RemoveFailedContract(typeName);
                return this.ImportXmlDataType(typeName, type, isAnonymous);
            }
            if ((this.dataContractSet.TryGetReferencedType(typeName, dataContract, out type4) || (string.IsNullOrEmpty(type.Name) && this.dataContractSet.TryGetReferencedType(ImportActualType(type.Annotation, typeName, typeName), dataContract, out type4))) && Globals.TypeOfIXmlSerializable.IsAssignableFrom(type4))
            {
                this.RemoveFailedContract(typeName);
                return this.ImportXmlDataType(typeName, type, isAnonymous);
            }
            XmlDataContract contract2 = this.ImportSpecialXmlDataType(type, isAnonymous);
            if (contract2 == null)
            {
                throw exception;
            }
            this.dataContractSet.Remove(typeName);
            return contract2;
        }

        private DataContract ImportType(XmlQualifiedName typeName, XmlSchemaParticle rootParticle, XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute anyAttribute, XmlQualifiedName baseTypeName, XmlSchemaAnnotation annotation)
        {
            DataContract contract = null;
            bool flag2;
            bool flag = baseTypeName != null;
            this.ImportAttributes(typeName, attributes, anyAttribute, out flag2);
            if (rootParticle == null)
            {
                return this.ImportClass(typeName, new XmlSchemaSequence(), baseTypeName, annotation, flag2);
            }
            if (!(rootParticle is XmlSchemaSequence))
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("RootParticleMustBeSequence"));
                return contract;
            }
            XmlSchemaSequence rootSequence = (XmlSchemaSequence) rootParticle;
            if (rootSequence.MinOccurs != 1M)
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("RootSequenceMustBeRequired"));
            }
            if (rootSequence.MaxOccurs != 1M)
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("RootSequenceMaxOccursMustBe"));
            }
            if (!flag && this.CheckIfCollection(rootSequence))
            {
                return this.ImportCollection(typeName, rootSequence, attributes, annotation, flag2);
            }
            if (this.CheckIfISerializable(rootSequence, attributes))
            {
                return this.ImportISerializable(typeName, rootSequence, baseTypeName, attributes, annotation);
            }
            return this.ImportClass(typeName, rootSequence, baseTypeName, annotation, flag2);
        }

        [SecuritySafeCritical]
        private DataContract ImportXmlDataType(XmlQualifiedName typeName, XmlSchemaType xsdType, bool isAnonymous)
        {
            DataContract contract = this.dataContractSet[typeName];
            if (contract != null)
            {
                return contract;
            }
            XmlDataContract dataContract = this.ImportSpecialXmlDataType(xsdType, isAnonymous);
            if (dataContract == null)
            {
                dataContract = new XmlDataContract {
                    StableName = typeName,
                    IsValueType = false
                };
                this.AddDataContract(dataContract);
                if (xsdType != null)
                {
                    this.ImportDataContractExtension(xsdType, dataContract);
                    dataContract.IsValueType = this.IsValueType(typeName, xsdType.Annotation);
                    dataContract.IsTypeDefinedOnImport = true;
                    dataContract.XsdType = isAnonymous ? xsdType : null;
                    dataContract.HasRoot = !this.IsXmlAnyElementType(xsdType as XmlSchemaComplexType);
                }
                else
                {
                    dataContract.IsValueType = true;
                    dataContract.IsTypeDefinedOnImport = false;
                    dataContract.HasRoot = true;
                    if (DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.Trace(TraceEventType.Verbose, 0x3000f, System.Runtime.Serialization.SR.GetString("TraceCodeXsdImportAnnotationFailed"), new StringTraceRecord("Type", typeName.Namespace + ":" + typeName.Name));
                    }
                }
                if (!isAnonymous)
                {
                    bool flag;
                    dataContract.SetTopLevelElementName(SchemaHelper.GetGlobalElementDeclaration(this.schemaSet, typeName, out flag));
                    dataContract.IsTopLevelElementNullable = flag;
                }
            }
            return dataContract;
        }

        private bool IsDictionary(XmlQualifiedName typeName, XmlSchemaAnnotation annotation)
        {
            string innerText = this.GetInnerText(typeName, ImportAnnotation(annotation, SchemaExporter.IsDictionaryAnnotationName));
            if (innerText != null)
            {
                try
                {
                    return XmlConvert.ToBoolean(innerText);
                }
                catch (FormatException exception)
                {
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("IsDictionaryFormattedIncorrectly", new object[] { innerText, exception.Message }));
                }
            }
            return false;
        }

        private bool IsISerializableDerived(XmlQualifiedName typeName, XmlSchemaSequence rootSequence)
        {
            if ((rootSequence != null) && (rootSequence.Items != null))
            {
                return (rootSequence.Items.Count == 0);
            }
            return true;
        }

        internal static bool IsObjectContract(DataContract dataContract)
        {
            HashSet<Type> set = new HashSet<Type>();
            while ((dataContract is CollectionDataContract) && !set.Contains(dataContract.OriginalUnderlyingType))
            {
                set.Add(dataContract.OriginalUnderlyingType);
                dataContract = ((CollectionDataContract) dataContract).ItemContract;
            }
            return ((dataContract is PrimitiveDataContract) && (((PrimitiveDataContract) dataContract).UnderlyingType == Globals.TypeOfObject));
        }

        private bool IsValueType(XmlQualifiedName typeName, XmlSchemaAnnotation annotation)
        {
            string innerText = this.GetInnerText(typeName, ImportAnnotation(annotation, SchemaExporter.IsValueTypeName));
            if (innerText != null)
            {
                try
                {
                    return XmlConvert.ToBoolean(innerText);
                }
                catch (FormatException exception)
                {
                    ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("IsValueTypeFormattedIncorrectly", new object[] { innerText, exception.Message }));
                }
            }
            return false;
        }

        private bool IsXmlAnyElementType(XmlSchemaComplexType xsdType)
        {
            if (xsdType == null)
            {
                return false;
            }
            XmlSchemaSequence particle = xsdType.Particle as XmlSchemaSequence;
            if (particle == null)
            {
                return false;
            }
            if ((particle.Items == null) || (particle.Items.Count != 1))
            {
                return false;
            }
            XmlSchemaAny any = particle.Items[0] as XmlSchemaAny;
            if ((any == null) || (any.Namespace != null))
            {
                return false;
            }
            return ((xsdType.AnyAttribute == null) && ((xsdType.Attributes == null) || (xsdType.Attributes.Count <= 0)));
        }

        private bool IsXmlAnyType(XmlSchemaComplexType xsdType)
        {
            if (xsdType == null)
            {
                return false;
            }
            XmlSchemaSequence particle = xsdType.Particle as XmlSchemaSequence;
            if (particle == null)
            {
                return false;
            }
            if ((particle.Items == null) || (particle.Items.Count != 1))
            {
                return false;
            }
            XmlSchemaAny any = particle.Items[0] as XmlSchemaAny;
            if ((any == null) || (any.Namespace != null))
            {
                return false;
            }
            if (any.MaxOccurs != 79228162514264337593543950335M)
            {
                return false;
            }
            return ((xsdType.AnyAttribute != null) && (xsdType.Attributes.Count <= 0));
        }

        private void RemoveFailedContract(XmlQualifiedName typeName)
        {
            ClassDataContract contract = this.dataContractSet[typeName] as ClassDataContract;
            this.dataContractSet.Remove(typeName);
            if (contract != null)
            {
                for (ClassDataContract contract2 = contract.BaseContract; contract2 != null; contract2 = contract2.BaseContract)
                {
                    contract2.KnownDataContracts.Remove(typeName);
                }
                if (this.dataContractSet.KnownTypesForObject != null)
                {
                    this.dataContractSet.KnownTypesForObject.Remove(typeName);
                }
            }
        }

        [SecuritySafeCritical]
        private void RemoveOptionalUnknownSerializationElements(XmlSchemaObjectCollection items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                XmlSchemaElement element = items[i] as XmlSchemaElement;
                if (((element != null) && (element.RefName != null)) && ((element.RefName.Namespace == "http://schemas.microsoft.com/2003/10/Serialization/") && (element.MinOccurs == 0M)))
                {
                    if (serializationSchemaElements == null)
                    {
                        XmlSchema schema = XmlSchema.Read(XmlReader.Create(new StringReader("<?xml version='1.0' encoding='utf-8'?>\r\n<xs:schema elementFormDefault='qualified' attributeFormDefault='qualified' xmlns:tns='http://schemas.microsoft.com/2003/10/Serialization/' targetNamespace='http://schemas.microsoft.com/2003/10/Serialization/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\r\n  <xs:element name='anyType' nillable='true' type='xs:anyType' />\r\n  <xs:element name='anyURI' nillable='true' type='xs:anyURI' />\r\n  <xs:element name='base64Binary' nillable='true' type='xs:base64Binary' />\r\n  <xs:element name='boolean' nillable='true' type='xs:boolean' />\r\n  <xs:element name='byte' nillable='true' type='xs:byte' />\r\n  <xs:element name='dateTime' nillable='true' type='xs:dateTime' />\r\n  <xs:element name='decimal' nillable='true' type='xs:decimal' />\r\n  <xs:element name='double' nillable='true' type='xs:double' />\r\n  <xs:element name='float' nillable='true' type='xs:float' />\r\n  <xs:element name='int' nillable='true' type='xs:int' />\r\n  <xs:element name='long' nillable='true' type='xs:long' />\r\n  <xs:element name='QName' nillable='true' type='xs:QName' />\r\n  <xs:element name='short' nillable='true' type='xs:short' />\r\n  <xs:element name='string' nillable='true' type='xs:string' />\r\n  <xs:element name='unsignedByte' nillable='true' type='xs:unsignedByte' />\r\n  <xs:element name='unsignedInt' nillable='true' type='xs:unsignedInt' />\r\n  <xs:element name='unsignedLong' nillable='true' type='xs:unsignedLong' />\r\n  <xs:element name='unsignedShort' nillable='true' type='xs:unsignedShort' />\r\n  <xs:element name='char' nillable='true' type='tns:char' />\r\n  <xs:simpleType name='char'>\r\n    <xs:restriction base='xs:int'/>\r\n  </xs:simpleType>  \r\n  <xs:element name='duration' nillable='true' type='tns:duration' />\r\n  <xs:simpleType name='duration'>\r\n    <xs:restriction base='xs:duration'>\r\n      <xs:pattern value='\\-?P(\\d*D)?(T(\\d*H)?(\\d*M)?(\\d*(\\.\\d*)?S)?)?' />\r\n      <xs:minInclusive value='-P10675199DT2H48M5.4775808S' />\r\n      <xs:maxInclusive value='P10675199DT2H48M5.4775807S' />\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:element name='guid' nillable='true' type='tns:guid' />\r\n  <xs:simpleType name='guid'>\r\n    <xs:restriction base='xs:string'>\r\n      <xs:pattern value='[\\da-fA-F]{8}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{12}' />\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:attribute name='FactoryType' type='xs:QName' />\r\n  <xs:attribute name='Id' type='xs:ID' />\r\n  <xs:attribute name='Ref' type='xs:IDREF' />\r\n</xs:schema>\r\n")), null);
                        serializationSchemaElements = new Hashtable();
                        foreach (XmlSchemaElement element2 in schema.Items)
                        {
                            if (element2 != null)
                            {
                                serializationSchemaElements.Add(element2.Name, element2);
                            }
                        }
                    }
                    if (!serializationSchemaElements.ContainsKey(element.RefName.Name))
                    {
                        items.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private static void ThrowArrayTypeCannotBeImportedException(string name, string ns, string message)
        {
            ThrowTypeCannotBeImportedException(System.Runtime.Serialization.SR.GetString("ArrayTypeCannotBeImported", new object[] { name, ns, message }));
        }

        private static void ThrowEnumTypeCannotBeImportedException(string name, string ns, string message)
        {
            ThrowTypeCannotBeImportedException(System.Runtime.Serialization.SR.GetString("EnumTypeCannotBeImported", new object[] { name, ns, message }));
        }

        private static void ThrowISerializableTypeCannotBeImportedException(string name, string ns, string message)
        {
            ThrowTypeCannotBeImportedException(System.Runtime.Serialization.SR.GetString("ISerializableTypeCannotBeImported", new object[] { name, ns, message }));
        }

        private static void ThrowTypeCannotBeImportedException(string message)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("TypeCannotBeImportedHowToFix", new object[] { message })));
        }

        private static void ThrowTypeCannotBeImportedException(string name, string ns, string message)
        {
            ThrowTypeCannotBeImportedException(System.Runtime.Serialization.SR.GetString("TypeCannotBeImported", new object[] { name, ns, message }));
        }

        private bool TryCheckIfAttribute(XmlQualifiedName typeName, XmlSchemaAttribute attribute, XmlQualifiedName refName, ref bool foundAttribute)
        {
            if (attribute.RefName != refName)
            {
                return false;
            }
            if (foundAttribute)
            {
                ThrowTypeCannotBeImportedException(typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("CannotHaveDuplicateAttributeNames", new object[] { refName.Name }));
            }
            foundAttribute = true;
            return true;
        }

        private List<XmlSchemaRedefine> RedefineList
        {
            get
            {
                if (this.redefineList == null)
                {
                    this.redefineList = this.CreateRedefineList();
                }
                return this.redefineList;
            }
        }

        private Dictionary<XmlQualifiedName, SchemaObjectInfo> SchemaObjects
        {
            get
            {
                if (this.schemaObjects == null)
                {
                    this.schemaObjects = this.CreateSchemaObjects();
                }
                return this.schemaObjects;
            }
        }
    }
}

