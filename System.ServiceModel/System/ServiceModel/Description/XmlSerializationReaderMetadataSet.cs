namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.ServiceModel;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class XmlSerializationReaderMetadataSet : XmlSerializationReader
    {
        private string id1_Metadata;
        private string id10_definitions;
        private string id100_XmlSchemaFractionDigitsFacet;
        private string id1000_Item;
        private string id101_value;
        private string id102_XmlSchemaTotalDigitsFacet;
        private string id103_XmlSchemaWhiteSpaceFacet;
        private string id104_XmlSchemaLengthFacet;
        private string id105_XmlSchemaEnumerationFacet;
        private string id106_XmlSchemaPatternFacet;
        private string id107_XmlSchemaMaxLengthFacet;
        private string id108_XmlSchemaMinLengthFacet;
        private string id109_XmlSchemaMinExclusiveFacet;
        private string id11_Item;
        private string id110_XmlSchemaMaxInclusiveFacet;
        private string id111_XmlSchemaMinInclusiveFacet;
        private string id112_XmlSchemaMaxExclusiveFacet;
        private string id113_XmlSchemaSimpleTypeList;
        private string id114_itemType;
        private string id115_XmlSchemaSimpleTypeUnion;
        private string id116_memberTypes;
        private string id117_XmlSchemaAttributeGroupRef;
        private string id118_XmlSchemaAll;
        private string id119_XmlSchemaChoice;
        private string id12_MetadataReference;
        private string id120_any;
        private string id121_XmlSchemaGroupRef;
        private string id122_XmlSchemaSequence;
        private string id123_XmlSchemaAny;
        private string id124_XmlSchemaComplexContent;
        private string id125_extension;
        private string id126_Item;
        private string id127_Item;
        private string id128_XmlSchemaSimpleContent;
        private string id129_Item;
        private string id13_Location;
        private string id130_Item;
        private string id131_XmlSchemaAttributeGroup;
        private string id132_XmlSchemaGroup;
        private string id133_XmlSchemaNotation;
        private string id134_public;
        private string id135_system;
        private string id136_XmlSchemaImport;
        private string id137_schemaLocation;
        private string id138_XmlSchemaRedefine;
        private string id139_XmlSchemaInclude;
        private string id14_MetadataLocation;
        private string id15_EndpointReference;
        private string id16_Item;
        private string id17_XmlSchema;
        private string id18_attributeFormDefault;
        private string id19_blockDefault;
        private string id2_Item;
        private string id20_finalDefault;
        private string id21_elementFormDefault;
        private string id22_targetNamespace;
        private string id23_version;
        private string id24_id;
        private string id25_include;
        private string id26_redefine;
        private string id27_import;
        private string id28_notation;
        private string id29_group;
        private string id3_MetadataSet;
        private string id30_annotation;
        private string id31_attribute;
        private string id32_attributeGroup;
        private string id33_complexType;
        private string id34_simpleType;
        private string id35_element;
        private string id36_XmlSchemaElement;
        private string id37_minOccurs;
        private string id38_maxOccurs;
        private string id39_abstract;
        private string id4_MetadataSection;
        private string id40_block;
        private string id41_default;
        private string id42_final;
        private string id43_fixed;
        private string id44_form;
        private string id45_name;
        private string id46_nillable;
        private string id47_ref;
        private string id48_substitutionGroup;
        private string id49_type;
        private string id5_Dialect;
        private string id50_unique;
        private string id51_key;
        private string id52_keyref;
        private string id53_XmlSchemaKeyref;
        private string id54_refer;
        private string id55_selector;
        private string id56_field;
        private string id57_XmlSchemaXPath;
        private string id58_xpath;
        private string id59_XmlSchemaAnnotation;
        private string id6_Item;
        private string id60_documentation;
        private string id61_appinfo;
        private string id62_XmlSchemaAppInfo;
        private string id63_source;
        private string id64_XmlSchemaDocumentation;
        private string id65_lang;
        private string id66_Item;
        private string id67_XmlSchemaKey;
        private string id68_XmlSchemaUnique;
        private string id69_XmlSchemaComplexType;
        private string id7_Identifier;
        private string id70_mixed;
        private string id71_simpleContent;
        private string id72_complexContent;
        private string id73_sequence;
        private string id74_choice;
        private string id75_all;
        private string id76_anyAttribute;
        private string id77_XmlSchemaAnyAttribute;
        private string id78_namespace;
        private string id79_processContents;
        private string id8_schema;
        private string id80_XmlSchemaAttribute;
        private string id81_use;
        private string id82_XmlSchemaSimpleType;
        private string id83_union;
        private string id84_list;
        private string id85_restriction;
        private string id86_XmlSchemaSimpleTypeRestriction;
        private string id87_base;
        private string id88_maxExclusive;
        private string id89_minInclusive;
        private string id9_Item;
        private string id90_maxInclusive;
        private string id91_minExclusive;
        private string id92_minLength;
        private string id93_maxLength;
        private string id94_pattern;
        private string id95_enumeration;
        private string id96_length;
        private string id97_whiteSpace;
        private string id98_totalDigits;
        private string id99_fractionDigits;
        private bool processOuterElement = true;

        protected override void InitCallbacks()
        {
        }

        protected override void InitIDs()
        {
            this.id60_documentation = base.Reader.NameTable.Add("documentation");
            this.id22_targetNamespace = base.Reader.NameTable.Add("targetNamespace");
            this.id10_definitions = base.Reader.NameTable.Add("definitions");
            this.id65_lang = base.Reader.NameTable.Add("lang");
            this.id31_attribute = base.Reader.NameTable.Add("attribute");
            this.id47_ref = base.Reader.NameTable.Add("ref");
            this.id4_MetadataSection = base.Reader.NameTable.Add("MetadataSection");
            this.id54_refer = base.Reader.NameTable.Add("refer");
            this.id83_union = base.Reader.NameTable.Add("union");
            this.id127_Item = base.Reader.NameTable.Add("XmlSchemaComplexContentRestriction");
            this.id53_XmlSchemaKeyref = base.Reader.NameTable.Add("XmlSchemaKeyref");
            this.id27_import = base.Reader.NameTable.Add("import");
            this.id75_all = base.Reader.NameTable.Add("all");
            this.id128_XmlSchemaSimpleContent = base.Reader.NameTable.Add("XmlSchemaSimpleContent");
            this.id139_XmlSchemaInclude = base.Reader.NameTable.Add("XmlSchemaInclude");
            this.id78_namespace = base.Reader.NameTable.Add("namespace");
            this.id18_attributeFormDefault = base.Reader.NameTable.Add("attributeFormDefault");
            this.id100_XmlSchemaFractionDigitsFacet = base.Reader.NameTable.Add("XmlSchemaFractionDigitsFacet");
            this.id32_attributeGroup = base.Reader.NameTable.Add("attributeGroup");
            this.id64_XmlSchemaDocumentation = base.Reader.NameTable.Add("XmlSchemaDocumentation");
            this.id93_maxLength = base.Reader.NameTable.Add("maxLength");
            this.id49_type = base.Reader.NameTable.Add("type");
            this.id86_XmlSchemaSimpleTypeRestriction = base.Reader.NameTable.Add("XmlSchemaSimpleTypeRestriction");
            this.id96_length = base.Reader.NameTable.Add("length");
            this.id104_XmlSchemaLengthFacet = base.Reader.NameTable.Add("XmlSchemaLengthFacet");
            this.id17_XmlSchema = base.Reader.NameTable.Add("XmlSchema");
            this.id134_public = base.Reader.NameTable.Add("public");
            this.id77_XmlSchemaAnyAttribute = base.Reader.NameTable.Add("XmlSchemaAnyAttribute");
            this.id24_id = base.Reader.NameTable.Add("id");
            this.id71_simpleContent = base.Reader.NameTable.Add("simpleContent");
            this.id51_key = base.Reader.NameTable.Add("key");
            this.id67_XmlSchemaKey = base.Reader.NameTable.Add("XmlSchemaKey");
            this.id80_XmlSchemaAttribute = base.Reader.NameTable.Add("XmlSchemaAttribute");
            this.id126_Item = base.Reader.NameTable.Add("XmlSchemaComplexContentExtension");
            this.id23_version = base.Reader.NameTable.Add("version");
            this.id121_XmlSchemaGroupRef = base.Reader.NameTable.Add("XmlSchemaGroupRef");
            this.id90_maxInclusive = base.Reader.NameTable.Add("maxInclusive");
            this.id116_memberTypes = base.Reader.NameTable.Add("memberTypes");
            this.id20_finalDefault = base.Reader.NameTable.Add("finalDefault");
            this.id120_any = base.Reader.NameTable.Add("any");
            this.id112_XmlSchemaMaxExclusiveFacet = base.Reader.NameTable.Add("XmlSchemaMaxExclusiveFacet");
            this.id15_EndpointReference = base.Reader.NameTable.Add("EndpointReference");
            this.id45_name = base.Reader.NameTable.Add("name");
            this.id122_XmlSchemaSequence = base.Reader.NameTable.Add("XmlSchemaSequence");
            this.id73_sequence = base.Reader.NameTable.Add("sequence");
            this.id82_XmlSchemaSimpleType = base.Reader.NameTable.Add("XmlSchemaSimpleType");
            this.id48_substitutionGroup = base.Reader.NameTable.Add("substitutionGroup");
            this.id111_XmlSchemaMinInclusiveFacet = base.Reader.NameTable.Add("XmlSchemaMinInclusiveFacet");
            this.id7_Identifier = base.Reader.NameTable.Add("Identifier");
            this.id113_XmlSchemaSimpleTypeList = base.Reader.NameTable.Add("XmlSchemaSimpleTypeList");
            this.id41_default = base.Reader.NameTable.Add("default");
            this.id125_extension = base.Reader.NameTable.Add("extension");
            this.id16_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/ws/2004/08/addressing");
            this.id1000_Item = base.Reader.NameTable.Add("http://www.w3.org/2005/08/addressing");
            this.id124_XmlSchemaComplexContent = base.Reader.NameTable.Add("XmlSchemaComplexContent");
            this.id72_complexContent = base.Reader.NameTable.Add("complexContent");
            this.id11_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/wsdl/");
            this.id25_include = base.Reader.NameTable.Add("include");
            this.id34_simpleType = base.Reader.NameTable.Add("simpleType");
            this.id91_minExclusive = base.Reader.NameTable.Add("minExclusive");
            this.id94_pattern = base.Reader.NameTable.Add("pattern");
            this.id2_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/ws/2004/09/mex");
            this.id95_enumeration = base.Reader.NameTable.Add("enumeration");
            this.id114_itemType = base.Reader.NameTable.Add("itemType");
            this.id115_XmlSchemaSimpleTypeUnion = base.Reader.NameTable.Add("XmlSchemaSimpleTypeUnion");
            this.id59_XmlSchemaAnnotation = base.Reader.NameTable.Add("XmlSchemaAnnotation");
            this.id28_notation = base.Reader.NameTable.Add("notation");
            this.id84_list = base.Reader.NameTable.Add("list");
            this.id39_abstract = base.Reader.NameTable.Add("abstract");
            this.id103_XmlSchemaWhiteSpaceFacet = base.Reader.NameTable.Add("XmlSchemaWhiteSpaceFacet");
            this.id110_XmlSchemaMaxInclusiveFacet = base.Reader.NameTable.Add("XmlSchemaMaxInclusiveFacet");
            this.id55_selector = base.Reader.NameTable.Add("selector");
            this.id43_fixed = base.Reader.NameTable.Add("fixed");
            this.id57_XmlSchemaXPath = base.Reader.NameTable.Add("XmlSchemaXPath");
            this.id118_XmlSchemaAll = base.Reader.NameTable.Add("XmlSchemaAll");
            this.id56_field = base.Reader.NameTable.Add("field");
            this.id119_XmlSchemaChoice = base.Reader.NameTable.Add("XmlSchemaChoice");
            this.id123_XmlSchemaAny = base.Reader.NameTable.Add("XmlSchemaAny");
            this.id132_XmlSchemaGroup = base.Reader.NameTable.Add("XmlSchemaGroup");
            this.id35_element = base.Reader.NameTable.Add("element");
            this.id129_Item = base.Reader.NameTable.Add("XmlSchemaSimpleContentExtension");
            this.id30_annotation = base.Reader.NameTable.Add("annotation");
            this.id44_form = base.Reader.NameTable.Add("form");
            this.id21_elementFormDefault = base.Reader.NameTable.Add("elementFormDefault");
            this.id98_totalDigits = base.Reader.NameTable.Add("totalDigits");
            this.id88_maxExclusive = base.Reader.NameTable.Add("maxExclusive");
            this.id42_final = base.Reader.NameTable.Add("final");
            this.id46_nillable = base.Reader.NameTable.Add("nillable");
            this.id9_Item = base.Reader.NameTable.Add("http://www.w3.org/2001/XMLSchema");
            this.id61_appinfo = base.Reader.NameTable.Add("appinfo");
            this.id38_maxOccurs = base.Reader.NameTable.Add("maxOccurs");
            this.id70_mixed = base.Reader.NameTable.Add("mixed");
            this.id87_base = base.Reader.NameTable.Add("base");
            this.id13_Location = base.Reader.NameTable.Add("Location");
            this.id12_MetadataReference = base.Reader.NameTable.Add("MetadataReference");
            this.id97_whiteSpace = base.Reader.NameTable.Add("whiteSpace");
            this.id29_group = base.Reader.NameTable.Add("group");
            this.id92_minLength = base.Reader.NameTable.Add("minLength");
            this.id99_fractionDigits = base.Reader.NameTable.Add("fractionDigits");
            this.id137_schemaLocation = base.Reader.NameTable.Add("schemaLocation");
            this.id26_redefine = base.Reader.NameTable.Add("redefine");
            this.id101_value = base.Reader.NameTable.Add("value");
            this.id63_source = base.Reader.NameTable.Add("source");
            this.id89_minInclusive = base.Reader.NameTable.Add("minInclusive");
            this.id133_XmlSchemaNotation = base.Reader.NameTable.Add("XmlSchemaNotation");
            this.id52_keyref = base.Reader.NameTable.Add("keyref");
            this.id33_complexType = base.Reader.NameTable.Add("complexType");
            this.id135_system = base.Reader.NameTable.Add("system");
            this.id50_unique = base.Reader.NameTable.Add("unique");
            this.id74_choice = base.Reader.NameTable.Add("choice");
            this.id66_Item = base.Reader.NameTable.Add("http://www.w3.org/XML/1998/namespace");
            this.id105_XmlSchemaEnumerationFacet = base.Reader.NameTable.Add("XmlSchemaEnumerationFacet");
            this.id107_XmlSchemaMaxLengthFacet = base.Reader.NameTable.Add("XmlSchemaMaxLengthFacet");
            this.id36_XmlSchemaElement = base.Reader.NameTable.Add("XmlSchemaElement");
            this.id106_XmlSchemaPatternFacet = base.Reader.NameTable.Add("XmlSchemaPatternFacet");
            this.id37_minOccurs = base.Reader.NameTable.Add("minOccurs");
            this.id130_Item = base.Reader.NameTable.Add("XmlSchemaSimpleContentRestriction");
            this.id68_XmlSchemaUnique = base.Reader.NameTable.Add("XmlSchemaUnique");
            this.id131_XmlSchemaAttributeGroup = base.Reader.NameTable.Add("XmlSchemaAttributeGroup");
            this.id40_block = base.Reader.NameTable.Add("block");
            this.id81_use = base.Reader.NameTable.Add("use");
            this.id85_restriction = base.Reader.NameTable.Add("restriction");
            this.id1_Metadata = base.Reader.NameTable.Add("Metadata");
            this.id69_XmlSchemaComplexType = base.Reader.NameTable.Add("XmlSchemaComplexType");
            this.id117_XmlSchemaAttributeGroupRef = base.Reader.NameTable.Add("XmlSchemaAttributeGroupRef");
            this.id138_XmlSchemaRedefine = base.Reader.NameTable.Add("XmlSchemaRedefine");
            this.id6_Item = base.Reader.NameTable.Add("");
            this.id102_XmlSchemaTotalDigitsFacet = base.Reader.NameTable.Add("XmlSchemaTotalDigitsFacet");
            this.id58_xpath = base.Reader.NameTable.Add("xpath");
            this.id5_Dialect = base.Reader.NameTable.Add("Dialect");
            this.id14_MetadataLocation = base.Reader.NameTable.Add("MetadataLocation");
            this.id3_MetadataSet = base.Reader.NameTable.Add("MetadataSet");
            this.id79_processContents = base.Reader.NameTable.Add("processContents");
            this.id76_anyAttribute = base.Reader.NameTable.Add("anyAttribute");
            this.id19_blockDefault = base.Reader.NameTable.Add("blockDefault");
            this.id136_XmlSchemaImport = base.Reader.NameTable.Add("XmlSchemaImport");
            this.id109_XmlSchemaMinExclusiveFacet = base.Reader.NameTable.Add("XmlSchemaMinExclusiveFacet");
            this.id108_XmlSchemaMinLengthFacet = base.Reader.NameTable.Add("XmlSchemaMinLengthFacet");
            this.id8_schema = base.Reader.NameTable.Add("schema");
            this.id62_XmlSchemaAppInfo = base.Reader.NameTable.Add("XmlSchemaAppInfo");
        }

        private MetadataLocation Read65_MetadataLocation(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id14_MetadataLocation) || (type.Namespace != this.id2_Item)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateUnknownTypeException(type));
            }
            if (flag)
            {
                return null;
            }
            MetadataLocation o = new MetadataLocation();
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                string str = null;
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else if (((base.Reader.NodeType == XmlNodeType.Text) || (base.Reader.NodeType == XmlNodeType.CDATA)) || ((base.Reader.NodeType == XmlNodeType.Whitespace) || (base.Reader.NodeType == XmlNodeType.SignificantWhitespace)))
                {
                    str = base.ReadString(str, false);
                    o.Location = str;
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private MetadataSection Read66_MetadataSection(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_MetadataSection) || (type.Namespace != this.id2_Item)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateUnknownTypeException(type));
            }
            if (flag)
            {
                return null;
            }
            MetadataSection o = new MetadataSection();
            Collection<System.Xml.XmlAttribute> attributes = o.Attributes;
            bool[] flagArray = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id5_Dialect)) && (base.Reader.NamespaceURI == this.id6_Item))
                {
                    o.Dialect = base.Reader.Value;
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id7_Identifier)) && (base.Reader.NamespaceURI == this.id6_Item))
                    {
                        o.Identifier = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) base.Document.ReadNode(base.Reader);
                        base.ParseWsdlArrayType(attr);
                        attributes.Add(attr);
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id1_Metadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Metadata = this.Read67_MetadataSet(false, true);
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id8_schema)) && (base.Reader.NamespaceURI == this.id9_Item))
                    {
                        o.Metadata = System.Xml.Schema.XmlSchema.Read(base.Reader, null);
                        if (base.Reader.NodeType == XmlNodeType.EndElement)
                        {
                            base.ReadEndElement();
                        }
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id10_definitions)) && (base.Reader.NamespaceURI == this.id11_Item))
                    {
                        o.Metadata = System.Web.Services.Description.ServiceDescription.Read(base.Reader);
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id12_MetadataReference)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Metadata = (MetadataReference) base.ReadSerializable((IXmlSerializable) Activator.CreateInstance(typeof(MetadataReference), BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[0], null));
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id13_Location)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Metadata = this.Read65_MetadataLocation(false, true);
                        flagArray[3] = true;
                    }
                    else
                    {
                        o.Metadata = (XmlElement) base.ReadXmlNode(false);
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/ws/2004/09/mex:Metadata, http://www.w3.org/2001/XMLSchema:schema, http://schemas.xmlsoap.org/wsdl/:definitions, http://schemas.xmlsoap.org/ws/2004/09/mex:MetadataReference, http://schemas.xmlsoap.org/ws/2004/09/mex:Location");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private MetadataSet Read67_MetadataSet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (((checkType && this.processOuterElement) && (type != null)) && ((type.Name != this.id3_MetadataSet) || (type.Namespace != this.id2_Item)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateUnknownTypeException(type));
            }
            if (flag)
            {
                return null;
            }
            MetadataSet o = new MetadataSet();
            Collection<MetadataSection> metadataSections = o.MetadataSections;
            Collection<System.Xml.XmlAttribute> attributes = o.Attributes;
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    attributes.Add(attr);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((base.Reader.LocalName == this.id4_MetadataSection) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (metadataSections == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            metadataSections.Add(this.Read66_MetadataSection(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/ws/2004/09/mex:MetadataSection");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/ws/2004/09/mex:MetadataSection");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        public object Read68_Metadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if (this.processOuterElement && ((base.Reader.LocalName != this.id1_Metadata) || (base.Reader.NamespaceURI != this.id2_Item)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateUnknownNodeException());
                }
                return this.Read67_MetadataSet(true, true);
            }
            base.UnknownNode(null, "http://schemas.xmlsoap.org/ws/2004/09/mex:Metadata");
            return null;
        }

        public bool ProcessOuterElement
        {
            get
            {
                return this.processOuterElement;
            }
            set
            {
                this.processOuterElement = value;
            }
        }
    }
}

