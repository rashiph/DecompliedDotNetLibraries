namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Diagnostics;
    using System.Security;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class SchemaExporter
    {
        [SecurityCritical]
        private static XmlQualifiedName actualTypeAnnotationName;
        [SecurityCritical]
        private static XmlQualifiedName anytypeQualifiedName;
        private DataContractSet dataContractSet;
        [SecurityCritical]
        private static XmlQualifiedName defaultEnumBaseTypeName;
        [SecurityCritical]
        private static XmlQualifiedName defaultValueAnnotation;
        [SecurityCritical]
        private static XmlQualifiedName enumerationValueAnnotationName;
        [SecurityCritical]
        private static XmlQualifiedName isDictionaryAnnotationName;
        [SecurityCritical]
        private static XmlQualifiedName isValueTypeName;
        private XmlSchemaSet schemas;
        [SecurityCritical]
        private static XmlQualifiedName stringQualifiedName;
        [SecurityCritical]
        private static XmlQualifiedName surrogateDataAnnotationName;
        private XmlDocument xmlDoc;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SchemaExporter(XmlSchemaSet schemas, DataContractSet dataContractSet)
        {
            this.schemas = schemas;
            this.dataContractSet = dataContractSet;
        }

        private static void AddDefaultDatasetType(XmlSchemaSet schemas, string localName, string ns)
        {
            XmlSchemaComplexType item = new XmlSchemaComplexType {
                Name = localName,
                Particle = new XmlSchemaSequence()
            };
            XmlSchemaElement element = new XmlSchemaElement {
                RefName = new XmlQualifiedName("schema", "http://www.w3.org/2001/XMLSchema")
            };
            ((XmlSchemaSequence) item.Particle).Items.Add(element);
            XmlSchemaAny any = new XmlSchemaAny();
            ((XmlSchemaSequence) item.Particle).Items.Add(any);
            XmlSchema schema = SchemaHelper.GetSchema(ns, schemas);
            schema.Items.Add(item);
            schemas.Reprocess(schema);
        }

        private static void AddDefaultTypedDatasetType(XmlSchemaSet schemas, XmlSchema datasetSchema, string localName, string ns)
        {
            XmlSchemaComplexType item = new XmlSchemaComplexType {
                Name = localName,
                Particle = new XmlSchemaSequence()
            };
            XmlSchemaAny any = new XmlSchemaAny {
                Namespace = (datasetSchema.TargetNamespace == null) ? string.Empty : datasetSchema.TargetNamespace
            };
            ((XmlSchemaSequence) item.Particle).Items.Add(any);
            schemas.Add(datasetSchema);
            XmlSchema schema = SchemaHelper.GetSchema(ns, schemas);
            schema.Items.Add(item);
            schemas.Reprocess(datasetSchema);
            schemas.Reprocess(schema);
        }

        internal static void AddDefaultXmlType(XmlSchemaSet schemas, string localName, string ns)
        {
            XmlSchemaComplexType item = CreateAnyType();
            item.Name = localName;
            XmlSchema schema = SchemaHelper.GetSchema(ns, schemas);
            schema.Items.Add(item);
            schemas.Reprocess(schema);
        }

        private void AddReferenceAttributes(XmlSchemaObjectCollection attributes, XmlSchema schema)
        {
            SchemaHelper.AddSchemaImport("http://schemas.microsoft.com/2003/10/Serialization/", schema);
            schema.Namespaces.Add("ser", "http://schemas.microsoft.com/2003/10/Serialization/");
            attributes.Add(IdAttribute);
            attributes.Add(RefAttribute);
        }

        private bool CheckIfMemberHasConflict(DataMember dataMember)
        {
            if (dataMember.HasConflictingNameAndType)
            {
                return true;
            }
            for (DataMember member = dataMember.ConflictingMember; member != null; member = member.ConflictingMember)
            {
                if (member.HasConflictingNameAndType)
                {
                    return true;
                }
            }
            return false;
        }

        private static XmlSchemaComplexType CreateAnyElementType()
        {
            XmlSchemaComplexType type = new XmlSchemaComplexType {
                IsMixed = false,
                Particle = new XmlSchemaSequence()
            };
            XmlSchemaAny item = new XmlSchemaAny {
                MinOccurs = 0M,
                ProcessContents = XmlSchemaContentProcessing.Lax
            };
            ((XmlSchemaSequence) type.Particle).Items.Add(item);
            return type;
        }

        private static XmlSchemaComplexType CreateAnyType()
        {
            XmlSchemaComplexType type = new XmlSchemaComplexType {
                IsMixed = true,
                Particle = new XmlSchemaSequence()
            };
            XmlSchemaAny item = new XmlSchemaAny {
                MinOccurs = 0M,
                MaxOccurs = 79228162514264337593543950335M,
                ProcessContents = XmlSchemaContentProcessing.Lax
            };
            ((XmlSchemaSequence) type.Particle).Items.Add(item);
            type.AnyAttribute = new XmlSchemaAnyAttribute();
            return type;
        }

        private XmlSchemaComplexContentExtension CreateTypeContent(XmlSchemaComplexType type, XmlQualifiedName baseTypeName, XmlSchema schema)
        {
            SchemaHelper.AddSchemaImport(baseTypeName.Namespace, schema);
            XmlSchemaComplexContentExtension extension = new XmlSchemaComplexContentExtension {
                BaseTypeName = baseTypeName
            };
            type.ContentModel = new XmlSchemaComplexContent();
            type.ContentModel.Content = extension;
            return extension;
        }

        internal void Export()
        {
            try
            {
                this.ExportSerializationSchema();
                foreach (KeyValuePair<XmlQualifiedName, DataContract> pair in this.dataContractSet)
                {
                    DataContract dataContract = pair.Value;
                    if (!this.dataContractSet.IsContractProcessed(dataContract))
                    {
                        this.ExportDataContract(dataContract);
                        this.dataContractSet.SetContractProcessed(dataContract);
                    }
                }
            }
            finally
            {
                this.xmlDoc = null;
                this.dataContractSet = null;
            }
        }

        private XmlElement ExportActualType(XmlQualifiedName typeName)
        {
            return ExportActualType(typeName, this.XmlDoc);
        }

        private static XmlElement ExportActualType(XmlQualifiedName typeName, XmlDocument xmlDoc)
        {
            XmlElement element = xmlDoc.CreateElement(ActualTypeAnnotationName.Name, ActualTypeAnnotationName.Namespace);
            System.Xml.XmlAttribute node = xmlDoc.CreateAttribute("Name");
            node.Value = typeName.Name;
            element.Attributes.Append(node);
            System.Xml.XmlAttribute attribute2 = xmlDoc.CreateAttribute("Namespace");
            attribute2.Value = typeName.Namespace;
            element.Attributes.Append(attribute2);
            return element;
        }

        private void ExportClassDataContract(ClassDataContract classDataContract, XmlSchema schema)
        {
            XmlSchemaComplexType item = new XmlSchemaComplexType {
                Name = classDataContract.StableName.Name
            };
            schema.Items.Add(item);
            XmlElement element = null;
            if (classDataContract.UnderlyingType.IsGenericType)
            {
                element = this.ExportGenericInfo(classDataContract.UnderlyingType, "GenericType", "http://schemas.microsoft.com/2003/10/Serialization/");
            }
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            for (int i = 0; i < classDataContract.Members.Count; i++)
            {
                DataMember dataMember = classDataContract.Members[i];
                XmlSchemaElement element2 = new XmlSchemaElement {
                    Name = dataMember.Name
                };
                XmlElement element3 = null;
                DataContract memberTypeDataContract = this.dataContractSet.GetMemberTypeDataContract(dataMember);
                if (this.CheckIfMemberHasConflict(dataMember))
                {
                    element2.SchemaTypeName = AnytypeQualifiedName;
                    element3 = this.ExportActualType(memberTypeDataContract.StableName);
                    SchemaHelper.AddSchemaImport(memberTypeDataContract.StableName.Namespace, schema);
                }
                else
                {
                    this.SetElementType(element2, memberTypeDataContract, schema);
                }
                SchemaHelper.AddElementForm(element2, schema);
                if (dataMember.IsNullable)
                {
                    element2.IsNillable = true;
                }
                if (!dataMember.IsRequired)
                {
                    element2.MinOccurs = 0M;
                }
                element2.Annotation = GetSchemaAnnotation(new System.Xml.XmlNode[] { element3, this.ExportSurrogateData(dataMember), this.ExportEmitDefaultValue(dataMember) });
                sequence.Items.Add(element2);
            }
            XmlElement element4 = null;
            if (classDataContract.BaseContract != null)
            {
                XmlSchemaComplexContentExtension extension = this.CreateTypeContent(item, classDataContract.BaseContract.StableName, schema);
                extension.Particle = sequence;
                if (classDataContract.IsReference && !classDataContract.BaseContract.IsReference)
                {
                    this.AddReferenceAttributes(extension.Attributes, schema);
                }
            }
            else
            {
                item.Particle = sequence;
                if (classDataContract.IsValueType)
                {
                    element4 = this.GetAnnotationMarkup(IsValueTypeName, XmlConvert.ToString(classDataContract.IsValueType), schema);
                }
                if (classDataContract.IsReference)
                {
                    this.AddReferenceAttributes(item.Attributes, schema);
                }
            }
            item.Annotation = GetSchemaAnnotation(new System.Xml.XmlNode[] { element, this.ExportSurrogateData(classDataContract), element4 });
        }

        private void ExportCollectionDataContract(CollectionDataContract collectionDataContract, XmlSchema schema)
        {
            XmlSchemaComplexType item = new XmlSchemaComplexType {
                Name = collectionDataContract.StableName.Name
            };
            schema.Items.Add(item);
            XmlElement element = null;
            XmlElement element2 = null;
            if (collectionDataContract.UnderlyingType.IsGenericType && CollectionDataContract.IsCollectionDataContract(collectionDataContract.UnderlyingType))
            {
                element = this.ExportGenericInfo(collectionDataContract.UnderlyingType, "GenericType", "http://schemas.microsoft.com/2003/10/Serialization/");
            }
            if (collectionDataContract.IsDictionary)
            {
                element2 = this.ExportIsDictionary();
            }
            item.Annotation = GetSchemaAnnotation(new System.Xml.XmlNode[] { element2, element, this.ExportSurrogateData(collectionDataContract) });
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            XmlSchemaElement element3 = new XmlSchemaElement {
                Name = collectionDataContract.ItemName,
                MinOccurs = 0M,
                MaxOccursString = "unbounded"
            };
            if (collectionDataContract.IsDictionary)
            {
                ClassDataContract itemContract = collectionDataContract.ItemContract as ClassDataContract;
                XmlSchemaComplexType type2 = new XmlSchemaComplexType();
                XmlSchemaSequence sequence2 = new XmlSchemaSequence();
                foreach (DataMember member in itemContract.Members)
                {
                    XmlSchemaElement element4 = new XmlSchemaElement {
                        Name = member.Name
                    };
                    this.SetElementType(element4, this.dataContractSet.GetMemberTypeDataContract(member), schema);
                    SchemaHelper.AddElementForm(element4, schema);
                    if (member.IsNullable)
                    {
                        element4.IsNillable = true;
                    }
                    element4.Annotation = GetSchemaAnnotation(new System.Xml.XmlNode[] { this.ExportSurrogateData(member) });
                    sequence2.Items.Add(element4);
                }
                type2.Particle = sequence2;
                element3.SchemaType = type2;
            }
            else
            {
                if (collectionDataContract.IsItemTypeNullable)
                {
                    element3.IsNillable = true;
                }
                DataContract itemTypeDataContract = this.dataContractSet.GetItemTypeDataContract(collectionDataContract);
                this.SetElementType(element3, itemTypeDataContract, schema);
            }
            SchemaHelper.AddElementForm(element3, schema);
            sequence.Items.Add(element3);
            item.Particle = sequence;
            if (collectionDataContract.IsReference)
            {
                this.AddReferenceAttributes(item.Attributes, schema);
            }
        }

        private void ExportDataContract(DataContract dataContract)
        {
            if (!dataContract.IsBuiltInDataContract)
            {
                if (dataContract is XmlDataContract)
                {
                    this.ExportXmlDataContract((XmlDataContract) dataContract);
                }
                else
                {
                    XmlSchema schema = this.GetSchema(dataContract.StableName.Namespace);
                    if (dataContract is ClassDataContract)
                    {
                        ClassDataContract contract = (ClassDataContract) dataContract;
                        if (contract.IsISerializable)
                        {
                            this.ExportISerializableDataContract(contract, schema);
                        }
                        else
                        {
                            this.ExportClassDataContract(contract, schema);
                        }
                    }
                    else if (dataContract is CollectionDataContract)
                    {
                        this.ExportCollectionDataContract((CollectionDataContract) dataContract, schema);
                    }
                    else if (dataContract is EnumDataContract)
                    {
                        this.ExportEnumDataContract((EnumDataContract) dataContract, schema);
                    }
                    this.ExportTopLevelElement(dataContract, schema);
                    this.Schemas.Reprocess(schema);
                }
            }
        }

        private XmlElement ExportEmitDefaultValue(DataMember dataMember)
        {
            if (dataMember.EmitDefaultValue)
            {
                return null;
            }
            XmlElement element = this.XmlDoc.CreateElement(DefaultValueAnnotation.Name, DefaultValueAnnotation.Namespace);
            System.Xml.XmlAttribute node = this.XmlDoc.CreateAttribute("EmitDefaultValue");
            node.Value = "false";
            element.Attributes.Append(node);
            return element;
        }

        private void ExportEnumDataContract(EnumDataContract enumDataContract, XmlSchema schema)
        {
            XmlSchemaSimpleType item = new XmlSchemaSimpleType {
                Name = enumDataContract.StableName.Name
            };
            XmlElement element = (enumDataContract.BaseContractName == DefaultEnumBaseTypeName) ? null : this.ExportActualType(enumDataContract.BaseContractName);
            item.Annotation = GetSchemaAnnotation(new System.Xml.XmlNode[] { element, this.ExportSurrogateData(enumDataContract) });
            schema.Items.Add(item);
            XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction {
                BaseTypeName = StringQualifiedName
            };
            SchemaHelper.AddSchemaImport(enumDataContract.BaseContractName.Namespace, schema);
            if (enumDataContract.Values != null)
            {
                for (int i = 0; i < enumDataContract.Values.Count; i++)
                {
                    XmlSchemaEnumerationFacet facet = new XmlSchemaEnumerationFacet {
                        Value = enumDataContract.Members[i].Name
                    };
                    if (enumDataContract.Values[i] != GetDefaultEnumValue(enumDataContract.IsFlags, i))
                    {
                        facet.Annotation = this.GetSchemaAnnotation(EnumerationValueAnnotationName, enumDataContract.GetStringFromEnumValue(enumDataContract.Values[i]), schema);
                    }
                    restriction.Facets.Add(facet);
                }
            }
            if (enumDataContract.IsFlags)
            {
                XmlSchemaSimpleTypeList list = new XmlSchemaSimpleTypeList();
                XmlSchemaSimpleType type2 = new XmlSchemaSimpleType {
                    Content = restriction
                };
                list.ItemType = type2;
                item.Content = list;
            }
            else
            {
                item.Content = restriction;
            }
        }

        private XmlElement ExportGenericInfo(Type clrType, string elementName, string elementNs)
        {
            Type type;
            int num = 0;
            while (CollectionDataContract.IsCollection(clrType, out type))
            {
                if ((DataContract.GetBuiltInDataContract(clrType) != null) || CollectionDataContract.IsCollectionDataContract(clrType))
                {
                    break;
                }
                clrType = type;
                num++;
            }
            Type[] genericArguments = null;
            IList<int> dataContractNameForGenericName = null;
            if (clrType.IsGenericType)
            {
                string name;
                genericArguments = clrType.GetGenericArguments();
                if (clrType.DeclaringType == null)
                {
                    name = clrType.Name;
                }
                else
                {
                    int startIndex = (clrType.Namespace == null) ? 0 : clrType.Namespace.Length;
                    if (startIndex > 0)
                    {
                        startIndex++;
                    }
                    name = DataContract.GetClrTypeFullName(clrType).Substring(startIndex).Replace('+', '.');
                }
                int index = name.IndexOf('[');
                if (index >= 0)
                {
                    name = name.Substring(0, index);
                }
                dataContractNameForGenericName = DataContract.GetDataContractNameForGenericName(name, null);
                clrType = clrType.GetGenericTypeDefinition();
            }
            XmlQualifiedName stableName = DataContract.GetStableName(clrType);
            if (num > 0)
            {
                string str2 = stableName.Name;
                for (int i = 0; i < num; i++)
                {
                    str2 = "ArrayOf" + str2;
                }
                stableName = new XmlQualifiedName(str2, DataContract.GetCollectionNamespace(stableName.Namespace));
            }
            XmlElement element = this.XmlDoc.CreateElement(elementName, elementNs);
            System.Xml.XmlAttribute node = this.XmlDoc.CreateAttribute("Name");
            node.Value = (genericArguments != null) ? XmlConvert.DecodeName(stableName.Name) : stableName.Name;
            element.Attributes.Append(node);
            System.Xml.XmlAttribute attribute2 = this.XmlDoc.CreateAttribute("Namespace");
            attribute2.Value = stableName.Namespace;
            element.Attributes.Append(attribute2);
            if (genericArguments != null)
            {
                int num5 = 0;
                int num6 = 0;
                foreach (int num7 in dataContractNameForGenericName)
                {
                    int num8 = 0;
                    while (num8 < num7)
                    {
                        XmlElement newChild = this.ExportGenericInfo(genericArguments[num5], "GenericParameter", "http://schemas.microsoft.com/2003/10/Serialization/");
                        if (num6 > 0)
                        {
                            System.Xml.XmlAttribute attribute3 = this.XmlDoc.CreateAttribute("NestedLevel");
                            attribute3.Value = num6.ToString(CultureInfo.InvariantCulture);
                            newChild.Attributes.Append(attribute3);
                        }
                        element.AppendChild(newChild);
                        num8++;
                        num5++;
                    }
                    num6++;
                }
                if (dataContractNameForGenericName[num6 - 1] == 0)
                {
                    System.Xml.XmlAttribute attribute4 = this.XmlDoc.CreateAttribute("NestedLevel");
                    attribute4.Value = dataContractNameForGenericName.Count.ToString(CultureInfo.InvariantCulture);
                    element.Attributes.Append(attribute4);
                }
            }
            return element;
        }

        private XmlElement ExportIsDictionary()
        {
            XmlElement element = this.XmlDoc.CreateElement(IsDictionaryAnnotationName.Name, IsDictionaryAnnotationName.Namespace);
            element.InnerText = "true";
            return element;
        }

        private void ExportISerializableDataContract(ClassDataContract dataContract, XmlSchema schema)
        {
            XmlSchemaComplexType item = new XmlSchemaComplexType {
                Name = dataContract.StableName.Name
            };
            schema.Items.Add(item);
            XmlElement element = null;
            if (dataContract.UnderlyingType.IsGenericType)
            {
                element = this.ExportGenericInfo(dataContract.UnderlyingType, "GenericType", "http://schemas.microsoft.com/2003/10/Serialization/");
            }
            XmlElement element2 = null;
            if (dataContract.BaseContract != null)
            {
                this.CreateTypeContent(item, dataContract.BaseContract.StableName, schema);
            }
            else
            {
                schema.Namespaces.Add("ser", "http://schemas.microsoft.com/2003/10/Serialization/");
                item.Particle = ISerializableSequence;
                XmlSchemaAttribute iSerializableFactoryTypeAttribute = ISerializableFactoryTypeAttribute;
                item.Attributes.Add(iSerializableFactoryTypeAttribute);
                SchemaHelper.AddSchemaImport(ISerializableFactoryTypeAttribute.RefName.Namespace, schema);
                if (dataContract.IsValueType)
                {
                    element2 = this.GetAnnotationMarkup(IsValueTypeName, XmlConvert.ToString(dataContract.IsValueType), schema);
                }
            }
            item.Annotation = GetSchemaAnnotation(new System.Xml.XmlNode[] { element, this.ExportSurrogateData(dataContract), element2 });
        }

        private void ExportSerializationSchema()
        {
            if (!this.Schemas.Contains("http://schemas.microsoft.com/2003/10/Serialization/"))
            {
                StringReader reader = new StringReader("<?xml version='1.0' encoding='utf-8'?>\r\n<xs:schema elementFormDefault='qualified' attributeFormDefault='qualified' xmlns:tns='http://schemas.microsoft.com/2003/10/Serialization/' targetNamespace='http://schemas.microsoft.com/2003/10/Serialization/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\r\n  <xs:element name='anyType' nillable='true' type='xs:anyType' />\r\n  <xs:element name='anyURI' nillable='true' type='xs:anyURI' />\r\n  <xs:element name='base64Binary' nillable='true' type='xs:base64Binary' />\r\n  <xs:element name='boolean' nillable='true' type='xs:boolean' />\r\n  <xs:element name='byte' nillable='true' type='xs:byte' />\r\n  <xs:element name='dateTime' nillable='true' type='xs:dateTime' />\r\n  <xs:element name='decimal' nillable='true' type='xs:decimal' />\r\n  <xs:element name='double' nillable='true' type='xs:double' />\r\n  <xs:element name='float' nillable='true' type='xs:float' />\r\n  <xs:element name='int' nillable='true' type='xs:int' />\r\n  <xs:element name='long' nillable='true' type='xs:long' />\r\n  <xs:element name='QName' nillable='true' type='xs:QName' />\r\n  <xs:element name='short' nillable='true' type='xs:short' />\r\n  <xs:element name='string' nillable='true' type='xs:string' />\r\n  <xs:element name='unsignedByte' nillable='true' type='xs:unsignedByte' />\r\n  <xs:element name='unsignedInt' nillable='true' type='xs:unsignedInt' />\r\n  <xs:element name='unsignedLong' nillable='true' type='xs:unsignedLong' />\r\n  <xs:element name='unsignedShort' nillable='true' type='xs:unsignedShort' />\r\n  <xs:element name='char' nillable='true' type='tns:char' />\r\n  <xs:simpleType name='char'>\r\n    <xs:restriction base='xs:int'/>\r\n  </xs:simpleType>  \r\n  <xs:element name='duration' nillable='true' type='tns:duration' />\r\n  <xs:simpleType name='duration'>\r\n    <xs:restriction base='xs:duration'>\r\n      <xs:pattern value='\\-?P(\\d*D)?(T(\\d*H)?(\\d*M)?(\\d*(\\.\\d*)?S)?)?' />\r\n      <xs:minInclusive value='-P10675199DT2H48M5.4775808S' />\r\n      <xs:maxInclusive value='P10675199DT2H48M5.4775807S' />\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:element name='guid' nillable='true' type='tns:guid' />\r\n  <xs:simpleType name='guid'>\r\n    <xs:restriction base='xs:string'>\r\n      <xs:pattern value='[\\da-fA-F]{8}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{12}' />\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:attribute name='FactoryType' type='xs:QName' />\r\n  <xs:attribute name='Id' type='xs:ID' />\r\n  <xs:attribute name='Ref' type='xs:IDREF' />\r\n</xs:schema>\r\n");
                XmlSchema schema = XmlSchema.Read(reader, null);
                if (schema == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("CouldNotReadSerializationSchema", new object[] { "http://schemas.microsoft.com/2003/10/Serialization/" })));
                }
                this.Schemas.Add(schema);
            }
        }

        private XmlElement ExportSurrogateData(object key)
        {
            object surrogateData = this.dataContractSet.GetSurrogateData(key);
            if (surrogateData == null)
            {
                return null;
            }
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            XmlWriterSettings settings = new XmlWriterSettings {
                OmitXmlDeclaration = true
            };
            XmlWriter writer = XmlWriter.Create(output, settings);
            Collection<Type> customDataTypes = new Collection<Type>();
            DataContractSurrogateCaller.GetKnownCustomDataTypes(this.dataContractSet.DataContractSurrogate, customDataTypes);
            new DataContractSerializer(Globals.TypeOfObject, SurrogateDataAnnotationName.Name, SurrogateDataAnnotationName.Namespace, customDataTypes, 0x7fffffff, false, true, null).WriteObject(writer, surrogateData);
            writer.Flush();
            return (XmlElement) this.XmlDoc.ReadNode(XmlReader.Create(new StringReader(output.ToString())));
        }

        private XmlSchemaElement ExportTopLevelElement(DataContract dataContract, XmlSchema schema)
        {
            if ((schema == null) || (dataContract.StableName.Namespace != dataContract.TopLevelElementNamespace.Value))
            {
                schema = this.GetSchema(dataContract.TopLevelElementNamespace.Value);
            }
            XmlSchemaElement element = new XmlSchemaElement {
                Name = dataContract.TopLevelElementName.Value
            };
            this.SetElementType(element, dataContract, schema);
            element.IsNillable = true;
            schema.Items.Add(element);
            return element;
        }

        private void ExportXmlDataContract(XmlDataContract dataContract)
        {
            XmlQualifiedName name;
            bool flag;
            XmlSchemaType type;
            Type underlyingType = dataContract.UnderlyingType;
            if (!IsSpecialXmlType(underlyingType, out name, out type, out flag) && !InvokeSchemaProviderMethod(underlyingType, this.schemas, out name, out type, out flag))
            {
                InvokeGetSchemaMethod(underlyingType, this.schemas, name);
            }
            if (flag)
            {
                XmlSchema schema;
                name.Equals(dataContract.StableName);
                if (SchemaHelper.GetSchemaElement(this.Schemas, new XmlQualifiedName(dataContract.TopLevelElementName.Value, dataContract.TopLevelElementNamespace.Value), out schema) == null)
                {
                    this.ExportTopLevelElement(dataContract, schema).IsNillable = dataContract.IsTopLevelElementNullable;
                    ReprocessAll(this.schemas);
                }
                XmlSchemaType type3 = type;
                type = SchemaHelper.GetSchemaType(this.schemas, name, out schema);
                if (((type3 == null) && (type == null)) && (name.Namespace != "http://www.w3.org/2001/XMLSchema"))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("MissingSchemaType", new object[] { name, DataContract.GetClrTypeFullName(underlyingType) })));
                }
                if (type != null)
                {
                    type.Annotation = GetSchemaAnnotation(new System.Xml.XmlNode[] { this.ExportSurrogateData(dataContract), dataContract.IsValueType ? this.GetAnnotationMarkup(IsValueTypeName, XmlConvert.ToString(dataContract.IsValueType), schema) : null });
                }
                else if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.Trace(TraceEventType.Verbose, 0x3000e, System.Runtime.Serialization.SR.GetString("TraceCodeXsdExportAnnotationFailed"), new StringTraceRecord("Type", name.Namespace + ":" + name.Name));
                }
            }
        }

        private XmlElement GetAnnotationMarkup(XmlQualifiedName annotationQualifiedName, string innerText, XmlSchema schema)
        {
            XmlElement element = this.XmlDoc.CreateElement(annotationQualifiedName.Name, annotationQualifiedName.Namespace);
            SchemaHelper.AddSchemaImport(annotationQualifiedName.Namespace, schema);
            element.InnerText = innerText;
            return element;
        }

        internal static long GetDefaultEnumValue(bool isFlags, int index)
        {
            if (!isFlags)
            {
                return (long) index;
            }
            return (long) Math.Pow(2.0, (double) index);
        }

        private XmlSchema GetSchema(string ns)
        {
            return SchemaHelper.GetSchema(ns, this.Schemas);
        }

        private static XmlSchemaAnnotation GetSchemaAnnotation(params System.Xml.XmlNode[] nodes)
        {
            if ((nodes == null) || (nodes.Length == 0))
            {
                return null;
            }
            bool flag = false;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return null;
            }
            XmlSchemaAnnotation annotation = new XmlSchemaAnnotation();
            XmlSchemaAppInfo item = new XmlSchemaAppInfo();
            annotation.Items.Add(item);
            item.Markup = nodes;
            return annotation;
        }

        private XmlSchemaAnnotation GetSchemaAnnotation(XmlQualifiedName annotationQualifiedName, string innerText, XmlSchema schema)
        {
            XmlSchemaAnnotation annotation = new XmlSchemaAnnotation();
            XmlSchemaAppInfo item = new XmlSchemaAppInfo();
            XmlElement element = this.GetAnnotationMarkup(annotationQualifiedName, innerText, schema);
            item.Markup = new System.Xml.XmlNode[] { element };
            annotation.Items.Add(item);
            return annotation;
        }

        internal static void GetXmlTypeInfo(Type type, out XmlQualifiedName stableName, out XmlSchemaType xsdType, out bool hasRoot)
        {
            if (!IsSpecialXmlType(type, out stableName, out xsdType, out hasRoot))
            {
                XmlSchemaSet schemas = new XmlSchemaSet {
                    XmlResolver = null
                };
                InvokeSchemaProviderMethod(type, schemas, out stableName, out xsdType, out hasRoot);
                if ((stableName.Name == null) || (stableName.Name.Length == 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidXmlDataContractName", new object[] { DataContract.GetClrTypeFullName(type) })));
                }
            }
        }

        private static void InvokeGetSchemaMethod(Type clrType, XmlSchemaSet schemas, XmlQualifiedName stableName)
        {
            XmlSchema datasetSchema = ((IXmlSerializable) Activator.CreateInstance(clrType)).GetSchema();
            if (datasetSchema == null)
            {
                AddDefaultDatasetType(schemas, stableName.Name, stableName.Namespace);
            }
            else
            {
                if ((datasetSchema.Id == null) || (datasetSchema.Id.Length == 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidReturnSchemaOnGetSchemaMethod", new object[] { DataContract.GetClrTypeFullName(clrType) })));
                }
                AddDefaultTypedDatasetType(schemas, datasetSchema, stableName.Name, stableName.Namespace);
            }
        }

        private static bool InvokeSchemaProviderMethod(Type clrType, XmlSchemaSet schemas, out XmlQualifiedName stableName, out XmlSchemaType xsdType, out bool hasRoot)
        {
            xsdType = null;
            hasRoot = true;
            object[] customAttributes = clrType.GetCustomAttributes(Globals.TypeOfXmlSchemaProviderAttribute, false);
            if ((customAttributes == null) || (customAttributes.Length == 0))
            {
                stableName = DataContract.GetDefaultStableName(clrType);
                return false;
            }
            XmlSchemaProviderAttribute attribute = (XmlSchemaProviderAttribute) customAttributes[0];
            if (attribute.IsAny)
            {
                xsdType = CreateAnyElementType();
                hasRoot = false;
            }
            string methodName = attribute.MethodName;
            if ((methodName == null) || (methodName.Length == 0))
            {
                if (!attribute.IsAny)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidGetSchemaMethod", new object[] { DataContract.GetClrTypeFullName(clrType) })));
                }
                stableName = DataContract.GetDefaultStableName(clrType);
            }
            else
            {
                MethodInfo info = clrType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(XmlSchemaSet) }, null);
                if (info == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("MissingGetSchemaMethod", new object[] { DataContract.GetClrTypeFullName(clrType), methodName })));
                }
                if (!Globals.TypeOfXmlQualifiedName.IsAssignableFrom(info.ReturnType) && !Globals.TypeOfXmlSchemaType.IsAssignableFrom(info.ReturnType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidReturnTypeOnGetSchemaMethod", new object[] { DataContract.GetClrTypeFullName(clrType), methodName, DataContract.GetClrTypeFullName(info.ReturnType), DataContract.GetClrTypeFullName(Globals.TypeOfXmlQualifiedName), typeof(XmlSchemaType) })));
                }
                object obj2 = info.Invoke(null, new object[] { schemas });
                if (attribute.IsAny)
                {
                    if (obj2 != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidNonNullReturnValueByIsAny", new object[] { DataContract.GetClrTypeFullName(clrType), methodName })));
                    }
                    stableName = DataContract.GetDefaultStableName(clrType);
                }
                else if (obj2 == null)
                {
                    xsdType = CreateAnyElementType();
                    hasRoot = false;
                    stableName = DataContract.GetDefaultStableName(clrType);
                }
                else
                {
                    XmlSchemaType type = obj2 as XmlSchemaType;
                    if (type == null)
                    {
                        stableName = (XmlQualifiedName) obj2;
                    }
                    else
                    {
                        string name = type.Name;
                        string ns = null;
                        if ((name == null) || (name.Length == 0))
                        {
                            DataContract.GetDefaultStableName(DataContract.GetClrTypeFullName(clrType), out name, out ns);
                            stableName = new XmlQualifiedName(name, ns);
                            type.Annotation = GetSchemaAnnotation(new System.Xml.XmlNode[] { ExportActualType(stableName, new XmlDocument()) });
                            xsdType = type;
                        }
                        else
                        {
                            foreach (XmlSchema schema in schemas.Schemas())
                            {
                                using (XmlSchemaObjectEnumerator enumerator2 = schema.Items.GetEnumerator())
                                {
                                    while (enumerator2.MoveNext())
                                    {
                                        if (enumerator2.Current == type)
                                        {
                                            ns = schema.TargetNamespace;
                                            if (ns == null)
                                            {
                                                ns = string.Empty;
                                            }
                                            goto Label_02DD;
                                        }
                                    }
                                }
                            Label_02DD:
                                if (ns != null)
                                {
                                    break;
                                }
                            }
                            if (ns == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("MissingSchemaType", new object[] { name, DataContract.GetClrTypeFullName(clrType) })));
                            }
                            stableName = new XmlQualifiedName(name, ns);
                        }
                    }
                }
            }
            return true;
        }

        internal static bool IsSpecialXmlType(Type type, out XmlQualifiedName typeName, out XmlSchemaType xsdType, out bool hasRoot)
        {
            xsdType = null;
            hasRoot = true;
            if ((type == Globals.TypeOfXmlElement) || (type == Globals.TypeOfXmlNodeArray))
            {
                string name = null;
                if (type == Globals.TypeOfXmlElement)
                {
                    xsdType = CreateAnyElementType();
                    name = "XmlElement";
                    hasRoot = false;
                }
                else
                {
                    xsdType = CreateAnyType();
                    name = "ArrayOfXmlNode";
                    hasRoot = true;
                }
                typeName = new XmlQualifiedName(name, DataContract.GetDefaultStableNamespace(type));
                return true;
            }
            typeName = null;
            return false;
        }

        private static void ReprocessAll(XmlSchemaSet schemas)
        {
            Hashtable hashtable = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            XmlSchema[] schemaArray = new XmlSchema[schemas.Count];
            schemas.CopyTo(schemaArray, 0);
            for (int i = 0; i < schemaArray.Length; i++)
            {
                XmlSchema schema = schemaArray[i];
                XmlSchemaObject[] array = new XmlSchemaObject[schema.Items.Count];
                schema.Items.CopyTo(array, 0);
                for (int j = 0; j < array.Length; j++)
                {
                    Hashtable hashtable3;
                    XmlQualifiedName name;
                    XmlSchemaObject item = array[j];
                    if (item is XmlSchemaElement)
                    {
                        hashtable3 = hashtable;
                        name = new XmlQualifiedName(((XmlSchemaElement) item).Name, schema.TargetNamespace);
                    }
                    else
                    {
                        if (!(item is XmlSchemaType))
                        {
                            continue;
                        }
                        hashtable3 = hashtable2;
                        name = new XmlQualifiedName(((XmlSchemaType) item).Name, schema.TargetNamespace);
                    }
                    if (hashtable3[name] != null)
                    {
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            Dictionary<string, string> dictionary2 = new Dictionary<string, string>(2);
                            dictionary2.Add("ItemType", item.ToString());
                            dictionary2.Add("Name", name.Namespace + ":" + name.Name);
                            Dictionary<string, string> dictionary = dictionary2;
                            TraceUtility.Trace(TraceEventType.Warning, 0x30010, System.Runtime.Serialization.SR.GetString("TraceCodeXsdExportDupItems"), new DictionaryTraceRecord(dictionary));
                        }
                        schema.Items.Remove(item);
                    }
                    else
                    {
                        hashtable3.Add(name, item);
                    }
                }
                schemas.Reprocess(schema);
            }
        }

        private void SetElementType(XmlSchemaElement element, DataContract dataContract, XmlSchema schema)
        {
            XmlDataContract contract = dataContract as XmlDataContract;
            if ((contract != null) && contract.IsAnonymous)
            {
                element.SchemaType = contract.XsdType;
            }
            else
            {
                element.SchemaTypeName = dataContract.StableName;
                if (element.SchemaTypeName.Namespace.Equals("http://schemas.microsoft.com/2003/10/Serialization/"))
                {
                    schema.Namespaces.Add("ser", "http://schemas.microsoft.com/2003/10/Serialization/");
                }
                SchemaHelper.AddSchemaImport(dataContract.StableName.Namespace, schema);
            }
        }

        internal static XmlQualifiedName ActualTypeAnnotationName
        {
            [SecuritySafeCritical]
            get
            {
                if (actualTypeAnnotationName == null)
                {
                    actualTypeAnnotationName = new XmlQualifiedName("ActualType", "http://schemas.microsoft.com/2003/10/Serialization/");
                }
                return actualTypeAnnotationName;
            }
        }

        internal static XmlQualifiedName AnytypeQualifiedName
        {
            [SecuritySafeCritical]
            get
            {
                if (anytypeQualifiedName == null)
                {
                    anytypeQualifiedName = new XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema");
                }
                return anytypeQualifiedName;
            }
        }

        internal static XmlQualifiedName DefaultEnumBaseTypeName
        {
            [SecuritySafeCritical]
            get
            {
                if (defaultEnumBaseTypeName == null)
                {
                    defaultEnumBaseTypeName = new XmlQualifiedName("int", "http://www.w3.org/2001/XMLSchema");
                }
                return defaultEnumBaseTypeName;
            }
        }

        internal static XmlQualifiedName DefaultValueAnnotation
        {
            [SecuritySafeCritical]
            get
            {
                if (defaultValueAnnotation == null)
                {
                    defaultValueAnnotation = new XmlQualifiedName("DefaultValue", "http://schemas.microsoft.com/2003/10/Serialization/");
                }
                return defaultValueAnnotation;
            }
        }

        internal static XmlQualifiedName EnumerationValueAnnotationName
        {
            [SecuritySafeCritical]
            get
            {
                if (enumerationValueAnnotationName == null)
                {
                    enumerationValueAnnotationName = new XmlQualifiedName("EnumerationValue", "http://schemas.microsoft.com/2003/10/Serialization/");
                }
                return enumerationValueAnnotationName;
            }
        }

        internal static XmlSchemaAttribute IdAttribute
        {
            get
            {
                return new XmlSchemaAttribute { RefName = Globals.IdQualifiedName };
            }
        }

        internal static XmlQualifiedName IsDictionaryAnnotationName
        {
            [SecuritySafeCritical]
            get
            {
                if (isDictionaryAnnotationName == null)
                {
                    isDictionaryAnnotationName = new XmlQualifiedName("IsDictionary", "http://schemas.microsoft.com/2003/10/Serialization/");
                }
                return isDictionaryAnnotationName;
            }
        }

        internal static XmlSchemaAttribute ISerializableFactoryTypeAttribute
        {
            get
            {
                return new XmlSchemaAttribute { RefName = new XmlQualifiedName("FactoryType", "http://schemas.microsoft.com/2003/10/Serialization/") };
            }
        }

        internal static XmlSchemaSequence ISerializableSequence
        {
            get
            {
                XmlSchemaSequence sequence = new XmlSchemaSequence();
                sequence.Items.Add(ISerializableWildcardElement);
                return sequence;
            }
        }

        internal static XmlSchemaAny ISerializableWildcardElement
        {
            get
            {
                return new XmlSchemaAny { MinOccurs = 0M, MaxOccursString = "unbounded", Namespace = "##local", ProcessContents = XmlSchemaContentProcessing.Skip };
            }
        }

        internal static XmlQualifiedName IsValueTypeName
        {
            [SecuritySafeCritical]
            get
            {
                if (isValueTypeName == null)
                {
                    isValueTypeName = new XmlQualifiedName("IsValueType", "http://schemas.microsoft.com/2003/10/Serialization/");
                }
                return isValueTypeName;
            }
        }

        internal static XmlSchemaAttribute RefAttribute
        {
            get
            {
                return new XmlSchemaAttribute { RefName = Globals.RefQualifiedName };
            }
        }

        private XmlSchemaSet Schemas
        {
            get
            {
                return this.schemas;
            }
        }

        internal static XmlQualifiedName StringQualifiedName
        {
            [SecuritySafeCritical]
            get
            {
                if (stringQualifiedName == null)
                {
                    stringQualifiedName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
                }
                return stringQualifiedName;
            }
        }

        internal static XmlQualifiedName SurrogateDataAnnotationName
        {
            [SecuritySafeCritical]
            get
            {
                if (surrogateDataAnnotationName == null)
                {
                    surrogateDataAnnotationName = new XmlQualifiedName("Surrogate", "http://schemas.microsoft.com/2003/10/Serialization/");
                }
                return surrogateDataAnnotationName;
            }
        }

        private XmlDocument XmlDoc
        {
            get
            {
                if (this.xmlDoc == null)
                {
                    this.xmlDoc = new XmlDocument();
                }
                return this.xmlDoc;
            }
        }
    }
}

