namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class ServiceDescriptionSerializationReader : XmlSerializationReader
    {
        private Hashtable _XmlSchemaDerivationMethodValues;
        private string id1_definitions;
        private string id10_message;
        private string id100_elementFormDefault;
        private string id101_version;
        private string id102_id;
        private string id103_include;
        private string id104_redefine;
        private string id105_simpleType;
        private string id106_complexType;
        private string id107_annotation;
        private string id108_notation;
        private string id109_attribute;
        private string id11_portType;
        private string id110_attributeGroup;
        private string id111_XmlSchemaAttributeGroup;
        private string id112_anyAttribute;
        private string id113_XmlSchemaAnyAttribute;
        private string id114_processContents;
        private string id115_XmlSchemaAnnotation;
        private string id116_appinfo;
        private string id117_XmlSchemaAppInfo;
        private string id118_source;
        private string id119_XmlSchemaDocumentation;
        private string id12_binding;
        private string id120_lang;
        private string id121_Item;
        private string id122_XmlSchemaAttributeGroupRef;
        private string id123_ref;
        private string id124_XmlSchemaAttribute;
        private string id125_default;
        private string id126_fixed;
        private string id127_form;
        private string id128_XmlSchemaSimpleType;
        private string id129_final;
        private string id13_service;
        private string id130_list;
        private string id131_restriction;
        private string id132_union;
        private string id133_XmlSchemaSimpleTypeUnion;
        private string id134_memberTypes;
        private string id135_XmlSchemaSimpleTypeRestriction;
        private string id136_base;
        private string id137_fractionDigits;
        private string id138_minInclusive;
        private string id139_maxLength;
        private string id14_Service;
        private string id140_length;
        private string id141_totalDigits;
        private string id142_enumeration;
        private string id143_maxInclusive;
        private string id144_maxExclusive;
        private string id145_whiteSpace;
        private string id146_minExclusive;
        private string id147_minLength;
        private string id148_XmlSchemaMinLengthFacet;
        private string id149_value;
        private string id15_port;
        private string id150_XmlSchemaMinExclusiveFacet;
        private string id151_XmlSchemaWhiteSpaceFacet;
        private string id152_XmlSchemaMaxExclusiveFacet;
        private string id153_XmlSchemaMaxInclusiveFacet;
        private string id154_XmlSchemaEnumerationFacet;
        private string id155_XmlSchemaPatternFacet;
        private string id156_XmlSchemaTotalDigitsFacet;
        private string id157_XmlSchemaLengthFacet;
        private string id158_XmlSchemaMaxLengthFacet;
        private string id159_XmlSchemaMinInclusiveFacet;
        private string id16_Port;
        private string id160_XmlSchemaFractionDigitsFacet;
        private string id161_XmlSchemaSimpleTypeList;
        private string id162_itemType;
        private string id163_XmlSchemaElement;
        private string id164_minOccurs;
        private string id165_maxOccurs;
        private string id166_abstract;
        private string id167_block;
        private string id168_nillable;
        private string id169_substitutionGroup;
        private string id17_address;
        private string id170_key;
        private string id171_unique;
        private string id172_keyref;
        private string id173_XmlSchemaKeyref;
        private string id174_refer;
        private string id175_selector;
        private string id176_field;
        private string id177_XmlSchemaXPath;
        private string id178_xpath;
        private string id179_XmlSchemaUnique;
        private string id18_Item;
        private string id180_XmlSchemaKey;
        private string id181_XmlSchemaComplexType;
        private string id182_mixed;
        private string id183_complexContent;
        private string id184_simpleContent;
        private string id185_sequence;
        private string id186_choice;
        private string id187_all;
        private string id188_XmlSchemaAll;
        private string id189_XmlSchemaChoice;
        private string id19_Item;
        private string id190_any;
        private string id191_XmlSchemaGroupRef;
        private string id192_XmlSchemaSequence;
        private string id193_XmlSchemaAny;
        private string id194_XmlSchemaSimpleContent;
        private string id195_extension;
        private string id196_Item;
        private string id197_Item;
        private string id198_XmlSchemaComplexContent;
        private string id199_Item;
        private string id2_Item;
        private string id20_Item;
        private string id200_Item;
        private string id201_XmlSchemaGroup;
        private string id202_XmlSchemaNotation;
        private string id203_public;
        private string id204_system;
        private string id205_XmlSchemaRedefine;
        private string id206_schemaLocation;
        private string id207_XmlSchemaImport;
        private string id208_XmlSchemaInclude;
        private string id209_Import;
        private string id21_Soap12AddressBinding;
        private string id22_required;
        private string id23_location;
        private string id24_SoapAddressBinding;
        private string id25_HttpAddressBinding;
        private string id26_Binding;
        private string id27_type;
        private string id28_operation;
        private string id29_OperationBinding;
        private string id3_ServiceDescription;
        private string id30_input;
        private string id31_output;
        private string id32_fault;
        private string id33_FaultBinding;
        private string id34_Soap12FaultBinding;
        private string id35_use;
        private string id36_namespace;
        private string id37_encodingStyle;
        private string id38_SoapFaultBinding;
        private string id39_OutputBinding;
        private string id4_name;
        private string id40_content;
        private string id41_Item;
        private string id42_mimeXml;
        private string id43_multipartRelated;
        private string id44_text;
        private string id45_Item;
        private string id46_body;
        private string id47_header;
        private string id48_Soap12HeaderBinding;
        private string id49_part;
        private string id5_Item;
        private string id50_headerfault;
        private string id51_SoapHeaderFaultBinding;
        private string id52_Soap12BodyBinding;
        private string id53_parts;
        private string id54_SoapHeaderBinding;
        private string id55_SoapBodyBinding;
        private string id56_MimeTextBinding;
        private string id57_match;
        private string id58_MimeTextMatch;
        private string id59_group;
        private string id6_targetNamespace;
        private string id60_capture;
        private string id61_repeats;
        private string id62_pattern;
        private string id63_ignoreCase;
        private string id64_MimeMultipartRelatedBinding;
        private string id65_MimePart;
        private string id66_MimeXmlBinding;
        private string id67_MimeContentBinding;
        private string id68_InputBinding;
        private string id69_urlEncoded;
        private string id7_documentation;
        private string id70_urlReplacement;
        private string id71_HttpUrlReplacementBinding;
        private string id72_HttpUrlEncodedBinding;
        private string id73_Soap12OperationBinding;
        private string id74_soapAction;
        private string id75_style;
        private string id76_soapActionRequired;
        private string id77_SoapOperationBinding;
        private string id78_HttpOperationBinding;
        private string id79_Soap12Binding;
        private string id8_import;
        private string id80_transport;
        private string id81_SoapBinding;
        private string id82_HttpBinding;
        private string id83_verb;
        private string id84_PortType;
        private string id85_Operation;
        private string id86_parameterOrder;
        private string id87_OperationFault;
        private string id88_OperationOutput;
        private string id89_OperationInput;
        private string id9_types;
        private string id90_Message;
        private string id91_MessagePart;
        private string id92_element;
        private string id93_Types;
        private string id94_schema;
        private string id95_Item;
        private string id96_XmlSchema;
        private string id97_attributeFormDefault;
        private string id98_blockDefault;
        private string id99_finalDefault;

        protected override void InitCallbacks()
        {
        }

        protected override void InitIDs()
        {
            this.id133_XmlSchemaSimpleTypeUnion = base.Reader.NameTable.Add("XmlSchemaSimpleTypeUnion");
            this.id143_maxInclusive = base.Reader.NameTable.Add("maxInclusive");
            this.id46_body = base.Reader.NameTable.Add("body");
            this.id190_any = base.Reader.NameTable.Add("any");
            this.id88_OperationOutput = base.Reader.NameTable.Add("OperationOutput");
            this.id6_targetNamespace = base.Reader.NameTable.Add("targetNamespace");
            this.id158_XmlSchemaMaxLengthFacet = base.Reader.NameTable.Add("XmlSchemaMaxLengthFacet");
            this.id11_portType = base.Reader.NameTable.Add("portType");
            this.id182_mixed = base.Reader.NameTable.Add("mixed");
            this.id172_keyref = base.Reader.NameTable.Add("keyref");
            this.id187_all = base.Reader.NameTable.Add("all");
            this.id162_itemType = base.Reader.NameTable.Add("itemType");
            this.id68_InputBinding = base.Reader.NameTable.Add("InputBinding");
            this.id25_HttpAddressBinding = base.Reader.NameTable.Add("HttpAddressBinding");
            this.id82_HttpBinding = base.Reader.NameTable.Add("HttpBinding");
            this.id17_address = base.Reader.NameTable.Add("address");
            this.id3_ServiceDescription = base.Reader.NameTable.Add("ServiceDescription");
            this.id38_SoapFaultBinding = base.Reader.NameTable.Add("SoapFaultBinding");
            this.id123_ref = base.Reader.NameTable.Add("ref");
            this.id198_XmlSchemaComplexContent = base.Reader.NameTable.Add("XmlSchemaComplexContent");
            this.id53_parts = base.Reader.NameTable.Add("parts");
            this.id35_use = base.Reader.NameTable.Add("use");
            this.id157_XmlSchemaLengthFacet = base.Reader.NameTable.Add("XmlSchemaLengthFacet");
            this.id207_XmlSchemaImport = base.Reader.NameTable.Add("XmlSchemaImport");
            this.id44_text = base.Reader.NameTable.Add("text");
            this.id117_XmlSchemaAppInfo = base.Reader.NameTable.Add("XmlSchemaAppInfo");
            this.id203_public = base.Reader.NameTable.Add("public");
            this.id69_urlEncoded = base.Reader.NameTable.Add("urlEncoded");
            this.id7_documentation = base.Reader.NameTable.Add("documentation");
            this.id19_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/wsdl/soap/");
            this.id129_final = base.Reader.NameTable.Add("final");
            this.id163_XmlSchemaElement = base.Reader.NameTable.Add("XmlSchemaElement");
            this.id60_capture = base.Reader.NameTable.Add("capture");
            this.id37_encodingStyle = base.Reader.NameTable.Add("encodingStyle");
            this.id185_sequence = base.Reader.NameTable.Add("sequence");
            this.id166_abstract = base.Reader.NameTable.Add("abstract");
            this.id23_location = base.Reader.NameTable.Add("location");
            this.id111_XmlSchemaAttributeGroup = base.Reader.NameTable.Add("XmlSchemaAttributeGroup");
            this.id192_XmlSchemaSequence = base.Reader.NameTable.Add("XmlSchemaSequence");
            this.id33_FaultBinding = base.Reader.NameTable.Add("FaultBinding");
            this.id153_XmlSchemaMaxInclusiveFacet = base.Reader.NameTable.Add("XmlSchemaMaxInclusiveFacet");
            this.id201_XmlSchemaGroup = base.Reader.NameTable.Add("XmlSchemaGroup");
            this.id43_multipartRelated = base.Reader.NameTable.Add("multipartRelated");
            this.id168_nillable = base.Reader.NameTable.Add("nillable");
            this.id149_value = base.Reader.NameTable.Add("value");
            this.id64_MimeMultipartRelatedBinding = base.Reader.NameTable.Add("MimeMultipartRelatedBinding");
            this.id193_XmlSchemaAny = base.Reader.NameTable.Add("XmlSchemaAny");
            this.id191_XmlSchemaGroupRef = base.Reader.NameTable.Add("XmlSchemaGroupRef");
            this.id74_soapAction = base.Reader.NameTable.Add("soapAction");
            this.id63_ignoreCase = base.Reader.NameTable.Add("ignoreCase");
            this.id101_version = base.Reader.NameTable.Add("version");
            this.id47_header = base.Reader.NameTable.Add("header");
            this.id195_extension = base.Reader.NameTable.Add("extension");
            this.id48_Soap12HeaderBinding = base.Reader.NameTable.Add("Soap12HeaderBinding");
            this.id134_memberTypes = base.Reader.NameTable.Add("memberTypes");
            this.id121_Item = base.Reader.NameTable.Add("http://www.w3.org/XML/1998/namespace");
            this.id146_minExclusive = base.Reader.NameTable.Add("minExclusive");
            this.id84_PortType = base.Reader.NameTable.Add("PortType");
            this.id42_mimeXml = base.Reader.NameTable.Add("mimeXml");
            this.id138_minInclusive = base.Reader.NameTable.Add("minInclusive");
            this.id118_source = base.Reader.NameTable.Add("source");
            this.id73_Soap12OperationBinding = base.Reader.NameTable.Add("Soap12OperationBinding");
            this.id131_restriction = base.Reader.NameTable.Add("restriction");
            this.id152_XmlSchemaMaxExclusiveFacet = base.Reader.NameTable.Add("XmlSchemaMaxExclusiveFacet");
            this.id135_XmlSchemaSimpleTypeRestriction = base.Reader.NameTable.Add("XmlSchemaSimpleTypeRestriction");
            this.id188_XmlSchemaAll = base.Reader.NameTable.Add("XmlSchemaAll");
            this.id116_appinfo = base.Reader.NameTable.Add("appinfo");
            this.id86_parameterOrder = base.Reader.NameTable.Add("parameterOrder");
            this.id147_minLength = base.Reader.NameTable.Add("minLength");
            this.id78_HttpOperationBinding = base.Reader.NameTable.Add("HttpOperationBinding");
            this.id161_XmlSchemaSimpleTypeList = base.Reader.NameTable.Add("XmlSchemaSimpleTypeList");
            this.id205_XmlSchemaRedefine = base.Reader.NameTable.Add("XmlSchemaRedefine");
            this.id194_XmlSchemaSimpleContent = base.Reader.NameTable.Add("XmlSchemaSimpleContent");
            this.id91_MessagePart = base.Reader.NameTable.Add("MessagePart");
            this.id92_element = base.Reader.NameTable.Add("element");
            this.id114_processContents = base.Reader.NameTable.Add("processContents");
            this.id18_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/wsdl/http/");
            this.id50_headerfault = base.Reader.NameTable.Add("headerfault");
            this.id154_XmlSchemaEnumerationFacet = base.Reader.NameTable.Add("XmlSchemaEnumerationFacet");
            this.id96_XmlSchema = base.Reader.NameTable.Add("XmlSchema");
            this.id127_form = base.Reader.NameTable.Add("form");
            this.id176_field = base.Reader.NameTable.Add("field");
            this.id49_part = base.Reader.NameTable.Add("part");
            this.id5_Item = base.Reader.NameTable.Add("");
            this.id57_match = base.Reader.NameTable.Add("match");
            this.id52_Soap12BodyBinding = base.Reader.NameTable.Add("Soap12BodyBinding");
            this.id104_redefine = base.Reader.NameTable.Add("redefine");
            this.id20_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/wsdl/soap12/");
            this.id21_Soap12AddressBinding = base.Reader.NameTable.Add("Soap12AddressBinding");
            this.id142_enumeration = base.Reader.NameTable.Add("enumeration");
            this.id24_SoapAddressBinding = base.Reader.NameTable.Add("SoapAddressBinding");
            this.id103_include = base.Reader.NameTable.Add("include");
            this.id139_maxLength = base.Reader.NameTable.Add("maxLength");
            this.id165_maxOccurs = base.Reader.NameTable.Add("maxOccurs");
            this.id65_MimePart = base.Reader.NameTable.Add("MimePart");
            this.id102_id = base.Reader.NameTable.Add("id");
            this.id196_Item = base.Reader.NameTable.Add("XmlSchemaSimpleContentExtension");
            this.id140_length = base.Reader.NameTable.Add("length");
            this.id27_type = base.Reader.NameTable.Add("type");
            this.id106_complexType = base.Reader.NameTable.Add("complexType");
            this.id31_output = base.Reader.NameTable.Add("output");
            this.id1_definitions = base.Reader.NameTable.Add("definitions");
            this.id4_name = base.Reader.NameTable.Add("name");
            this.id132_union = base.Reader.NameTable.Add("union");
            this.id29_OperationBinding = base.Reader.NameTable.Add("OperationBinding");
            this.id170_key = base.Reader.NameTable.Add("key");
            this.id45_Item = base.Reader.NameTable.Add("http://microsoft.com/wsdl/mime/textMatching/");
            this.id95_Item = base.Reader.NameTable.Add("http://www.w3.org/2001/XMLSchema");
            this.id169_substitutionGroup = base.Reader.NameTable.Add("substitutionGroup");
            this.id178_xpath = base.Reader.NameTable.Add("xpath");
            this.id9_types = base.Reader.NameTable.Add("types");
            this.id97_attributeFormDefault = base.Reader.NameTable.Add("attributeFormDefault");
            this.id62_pattern = base.Reader.NameTable.Add("pattern");
            this.id58_MimeTextMatch = base.Reader.NameTable.Add("MimeTextMatch");
            this.id180_XmlSchemaKey = base.Reader.NameTable.Add("XmlSchemaKey");
            this.id10_message = base.Reader.NameTable.Add("message");
            this.id8_import = base.Reader.NameTable.Add("import");
            this.id148_XmlSchemaMinLengthFacet = base.Reader.NameTable.Add("XmlSchemaMinLengthFacet");
            this.id105_simpleType = base.Reader.NameTable.Add("simpleType");
            this.id181_XmlSchemaComplexType = base.Reader.NameTable.Add("XmlSchemaComplexType");
            this.id164_minOccurs = base.Reader.NameTable.Add("minOccurs");
            this.id144_maxExclusive = base.Reader.NameTable.Add("maxExclusive");
            this.id160_XmlSchemaFractionDigitsFacet = base.Reader.NameTable.Add("XmlSchemaFractionDigitsFacet");
            this.id124_XmlSchemaAttribute = base.Reader.NameTable.Add("XmlSchemaAttribute");
            this.id209_Import = base.Reader.NameTable.Add("Import");
            this.id206_schemaLocation = base.Reader.NameTable.Add("schemaLocation");
            this.id179_XmlSchemaUnique = base.Reader.NameTable.Add("XmlSchemaUnique");
            this.id75_style = base.Reader.NameTable.Add("style");
            this.id119_XmlSchemaDocumentation = base.Reader.NameTable.Add("XmlSchemaDocumentation");
            this.id136_base = base.Reader.NameTable.Add("base");
            this.id66_MimeXmlBinding = base.Reader.NameTable.Add("MimeXmlBinding");
            this.id30_input = base.Reader.NameTable.Add("input");
            this.id40_content = base.Reader.NameTable.Add("content");
            this.id93_Types = base.Reader.NameTable.Add("Types");
            this.id94_schema = base.Reader.NameTable.Add("schema");
            this.id200_Item = base.Reader.NameTable.Add("XmlSchemaComplexContentExtension");
            this.id67_MimeContentBinding = base.Reader.NameTable.Add("MimeContentBinding");
            this.id59_group = base.Reader.NameTable.Add("group");
            this.id32_fault = base.Reader.NameTable.Add("fault");
            this.id80_transport = base.Reader.NameTable.Add("transport");
            this.id98_blockDefault = base.Reader.NameTable.Add("blockDefault");
            this.id13_service = base.Reader.NameTable.Add("service");
            this.id54_SoapHeaderBinding = base.Reader.NameTable.Add("SoapHeaderBinding");
            this.id204_system = base.Reader.NameTable.Add("system");
            this.id16_Port = base.Reader.NameTable.Add("Port");
            this.id108_notation = base.Reader.NameTable.Add("notation");
            this.id186_choice = base.Reader.NameTable.Add("choice");
            this.id110_attributeGroup = base.Reader.NameTable.Add("attributeGroup");
            this.id79_Soap12Binding = base.Reader.NameTable.Add("Soap12Binding");
            this.id77_SoapOperationBinding = base.Reader.NameTable.Add("SoapOperationBinding");
            this.id115_XmlSchemaAnnotation = base.Reader.NameTable.Add("XmlSchemaAnnotation");
            this.id83_verb = base.Reader.NameTable.Add("verb");
            this.id72_HttpUrlEncodedBinding = base.Reader.NameTable.Add("HttpUrlEncodedBinding");
            this.id39_OutputBinding = base.Reader.NameTable.Add("OutputBinding");
            this.id183_complexContent = base.Reader.NameTable.Add("complexContent");
            this.id202_XmlSchemaNotation = base.Reader.NameTable.Add("XmlSchemaNotation");
            this.id81_SoapBinding = base.Reader.NameTable.Add("SoapBinding");
            this.id199_Item = base.Reader.NameTable.Add("XmlSchemaComplexContentRestriction");
            this.id28_operation = base.Reader.NameTable.Add("operation");
            this.id122_XmlSchemaAttributeGroupRef = base.Reader.NameTable.Add("XmlSchemaAttributeGroupRef");
            this.id155_XmlSchemaPatternFacet = base.Reader.NameTable.Add("XmlSchemaPatternFacet");
            this.id76_soapActionRequired = base.Reader.NameTable.Add("soapActionRequired");
            this.id90_Message = base.Reader.NameTable.Add("Message");
            this.id159_XmlSchemaMinInclusiveFacet = base.Reader.NameTable.Add("XmlSchemaMinInclusiveFacet");
            this.id208_XmlSchemaInclude = base.Reader.NameTable.Add("XmlSchemaInclude");
            this.id85_Operation = base.Reader.NameTable.Add("Operation");
            this.id130_list = base.Reader.NameTable.Add("list");
            this.id14_Service = base.Reader.NameTable.Add("Service");
            this.id22_required = base.Reader.NameTable.Add("required");
            this.id174_refer = base.Reader.NameTable.Add("refer");
            this.id71_HttpUrlReplacementBinding = base.Reader.NameTable.Add("HttpUrlReplacementBinding");
            this.id56_MimeTextBinding = base.Reader.NameTable.Add("MimeTextBinding");
            this.id87_OperationFault = base.Reader.NameTable.Add("OperationFault");
            this.id125_default = base.Reader.NameTable.Add("default");
            this.id15_port = base.Reader.NameTable.Add("port");
            this.id51_SoapHeaderFaultBinding = base.Reader.NameTable.Add("SoapHeaderFaultBinding");
            this.id128_XmlSchemaSimpleType = base.Reader.NameTable.Add("XmlSchemaSimpleType");
            this.id36_namespace = base.Reader.NameTable.Add("namespace");
            this.id175_selector = base.Reader.NameTable.Add("selector");
            this.id150_XmlSchemaMinExclusiveFacet = base.Reader.NameTable.Add("XmlSchemaMinExclusiveFacet");
            this.id100_elementFormDefault = base.Reader.NameTable.Add("elementFormDefault");
            this.id26_Binding = base.Reader.NameTable.Add("Binding");
            this.id197_Item = base.Reader.NameTable.Add("XmlSchemaSimpleContentRestriction");
            this.id126_fixed = base.Reader.NameTable.Add("fixed");
            this.id107_annotation = base.Reader.NameTable.Add("annotation");
            this.id99_finalDefault = base.Reader.NameTable.Add("finalDefault");
            this.id137_fractionDigits = base.Reader.NameTable.Add("fractionDigits");
            this.id70_urlReplacement = base.Reader.NameTable.Add("urlReplacement");
            this.id189_XmlSchemaChoice = base.Reader.NameTable.Add("XmlSchemaChoice");
            this.id2_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/wsdl/");
            this.id112_anyAttribute = base.Reader.NameTable.Add("anyAttribute");
            this.id89_OperationInput = base.Reader.NameTable.Add("OperationInput");
            this.id141_totalDigits = base.Reader.NameTable.Add("totalDigits");
            this.id61_repeats = base.Reader.NameTable.Add("repeats");
            this.id184_simpleContent = base.Reader.NameTable.Add("simpleContent");
            this.id55_SoapBodyBinding = base.Reader.NameTable.Add("SoapBodyBinding");
            this.id145_whiteSpace = base.Reader.NameTable.Add("whiteSpace");
            this.id167_block = base.Reader.NameTable.Add("block");
            this.id151_XmlSchemaWhiteSpaceFacet = base.Reader.NameTable.Add("XmlSchemaWhiteSpaceFacet");
            this.id12_binding = base.Reader.NameTable.Add("binding");
            this.id109_attribute = base.Reader.NameTable.Add("attribute");
            this.id171_unique = base.Reader.NameTable.Add("unique");
            this.id120_lang = base.Reader.NameTable.Add("lang");
            this.id173_XmlSchemaKeyref = base.Reader.NameTable.Add("XmlSchemaKeyref");
            this.id177_XmlSchemaXPath = base.Reader.NameTable.Add("XmlSchemaXPath");
            this.id34_Soap12FaultBinding = base.Reader.NameTable.Add("Soap12FaultBinding");
            this.id41_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/wsdl/mime/");
            this.id156_XmlSchemaTotalDigitsFacet = base.Reader.NameTable.Add("XmlSchemaTotalDigitsFacet");
            this.id113_XmlSchemaAnyAttribute = base.Reader.NameTable.Add("XmlSchemaAnyAttribute");
        }

        private XmlSchemaAppInfo Read10_XmlSchemaAppInfo(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id117_XmlSchemaAppInfo) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaAppInfo o = new XmlSchemaAppInfo();
            XmlNode[] a = null;
            int length = 0;
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id118_source)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Source = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    base.UnknownNode(o, ":source");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Markup = (XmlNode[]) base.ShrinkArray(a, length, typeof(XmlNode), true);
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
                    a = (XmlNode[]) base.EnsureArrayIndex(a, length, typeof(XmlNode));
                    a[length++] = base.ReadXmlNode(false);
                }
                else if (((base.Reader.NodeType == XmlNodeType.Text) || (base.Reader.NodeType == XmlNodeType.CDATA)) || ((base.Reader.NodeType == XmlNodeType.Whitespace) || (base.Reader.NodeType == XmlNodeType.SignificantWhitespace)))
                {
                    a = (XmlNode[]) base.EnsureArrayIndex(a, length, typeof(XmlNode));
                    a[length++] = base.Document.CreateTextNode(base.Reader.ReadString());
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Markup = (XmlNode[]) base.ShrinkArray(a, length, typeof(XmlNode), true);
            base.ReadEndElement();
            return o;
        }

        private SoapBindingUse Read100_SoapBindingUse(string s)
        {
            switch (s)
            {
                case "encoded":
                    return SoapBindingUse.Encoded;

                case "literal":
                    return SoapBindingUse.Literal;
            }
            throw base.CreateUnknownConstantException(s, typeof(SoapBindingUse));
        }

        private Soap12BodyBinding Read102_Soap12BodyBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id52_Soap12BodyBinding) || (type.Namespace != this.id20_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Soap12BodyBinding o = new Soap12BodyBinding();
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id35_use)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Use = this.Read100_SoapBindingUse(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id37_encodingStyle)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Encoding = base.Reader.Value;
                        flagArray[3] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id53_parts)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.PartsString = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :use, :namespace, :encodingStyle, :parts");
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
                    base.UnknownNode(o, "");
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

        private MimePart Read103_MimePart(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id65_MimePart) || (type.Namespace != this.id41_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            MimePart o = new MimePart();
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required");
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
                    if ((base.Reader.LocalName == this.id40_content) && (base.Reader.NamespaceURI == this.id41_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read93_MimeContentBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id42_mimeXml) && (base.Reader.NamespaceURI == this.id41_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read94_MimeXmlBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id44_text) && (base.Reader.NamespaceURI == this.id45_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read97_MimeTextBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id46_body) && (base.Reader.NamespaceURI == this.id19_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read99_SoapBodyBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id46_body) && (base.Reader.NamespaceURI == this.id20_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read102_Soap12BodyBinding(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/mime/:content, http://schemas.xmlsoap.org/wsdl/mime/:mimeXml, http://microsoft.com/wsdl/mime/textMatching/:text, http://schemas.xmlsoap.org/wsdl/soap/:body, http://schemas.xmlsoap.org/wsdl/soap12/:body");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private MimeMultipartRelatedBinding Read104_MimeMultipartRelatedBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id64_MimeMultipartRelatedBinding) || (type.Namespace != this.id41_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            MimeMultipartRelatedBinding o = new MimeMultipartRelatedBinding();
            MimePartCollection parts = o.Parts;
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required");
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
                    if ((base.Reader.LocalName == this.id49_part) && (base.Reader.NamespaceURI == this.id41_Item))
                    {
                        if (parts == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            parts.Add(this.Read103_MimePart(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/mime/:part");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/mime/:part");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private SoapHeaderFaultBinding Read105_SoapHeaderFaultBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id51_SoapHeaderFaultBinding) || (type.Namespace != this.id19_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            SoapHeaderFaultBinding o = new SoapHeaderFaultBinding();
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id10_message)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Message = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id49_part)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Part = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id35_use)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Use = this.Read98_SoapBindingUse(base.Reader.Value);
                        flagArray[3] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id37_encodingStyle)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Encoding = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :message, :part, :use, :encodingStyle, :namespace");
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
                    base.UnknownNode(o, "");
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

        private SoapHeaderBinding Read106_SoapHeaderBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id54_SoapHeaderBinding) || (type.Namespace != this.id19_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            SoapHeaderBinding o = new SoapHeaderBinding();
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id10_message)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Message = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id49_part)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Part = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id35_use)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Use = this.Read98_SoapBindingUse(base.Reader.Value);
                        flagArray[3] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id37_encodingStyle)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Encoding = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :message, :part, :use, :encodingStyle, :namespace");
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
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id50_headerfault)) && (base.Reader.NamespaceURI == this.id19_Item))
                    {
                        o.Fault = this.Read105_SoapHeaderFaultBinding(false, true);
                        flagArray[6] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/soap/:headerfault");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/soap/:headerfault");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private SoapHeaderFaultBinding Read107_SoapHeaderFaultBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id51_SoapHeaderFaultBinding) || (type.Namespace != this.id20_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            SoapHeaderFaultBinding o = new SoapHeaderFaultBinding();
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id10_message)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Message = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id49_part)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Part = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id35_use)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Use = this.Read100_SoapBindingUse(base.Reader.Value);
                        flagArray[3] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id37_encodingStyle)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Encoding = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :message, :part, :use, :encodingStyle, :namespace");
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
                    base.UnknownNode(o, "");
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

        private Soap12HeaderBinding Read109_Soap12HeaderBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id48_Soap12HeaderBinding) || (type.Namespace != this.id20_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Soap12HeaderBinding o = new Soap12HeaderBinding();
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id10_message)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Message = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id49_part)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Part = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id35_use)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Use = this.Read100_SoapBindingUse(base.Reader.Value);
                        flagArray[3] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id37_encodingStyle)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Encoding = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :message, :part, :use, :encodingStyle, :namespace");
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
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id50_headerfault)) && (base.Reader.NamespaceURI == this.id20_Item))
                    {
                        o.Fault = this.Read107_SoapHeaderFaultBinding(false, true);
                        flagArray[6] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/soap12/:headerfault");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/soap12/:headerfault");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaAnnotation Read11_XmlSchemaAnnotation(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id115_XmlSchemaAnnotation) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaAnnotation o = new XmlSchemaAnnotation();
            XmlSchemaObjectCollection items = o.Items;
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((base.Reader.LocalName == this.id7_documentation) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read9_XmlSchemaDocumentation(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id116_appinfo) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read10_XmlSchemaAppInfo(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:documentation, http://www.w3.org/2001/XMLSchema:appinfo");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:documentation, http://www.w3.org/2001/XMLSchema:appinfo");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private InputBinding Read110_InputBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id68_InputBinding) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            InputBinding o = new InputBinding();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id69_urlEncoded) && (base.Reader.NamespaceURI == this.id18_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read90_HttpUrlEncodedBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id70_urlReplacement) && (base.Reader.NamespaceURI == this.id18_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read91_HttpUrlReplacementBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id40_content) && (base.Reader.NamespaceURI == this.id41_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read93_MimeContentBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id42_mimeXml) && (base.Reader.NamespaceURI == this.id41_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read94_MimeXmlBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id43_multipartRelated) && (base.Reader.NamespaceURI == this.id41_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read104_MimeMultipartRelatedBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id44_text) && (base.Reader.NamespaceURI == this.id45_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read97_MimeTextBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id46_body) && (base.Reader.NamespaceURI == this.id19_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read99_SoapBodyBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id47_header) && (base.Reader.NamespaceURI == this.id19_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read106_SoapHeaderBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id46_body) && (base.Reader.NamespaceURI == this.id20_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read102_Soap12BodyBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id47_header) && (base.Reader.NamespaceURI == this.id20_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read109_Soap12HeaderBinding(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/http/:urlEncoded, http://schemas.xmlsoap.org/wsdl/http/:urlReplacement, http://schemas.xmlsoap.org/wsdl/mime/:content, http://schemas.xmlsoap.org/wsdl/mime/:mimeXml, http://schemas.xmlsoap.org/wsdl/mime/:multipartRelated, http://microsoft.com/wsdl/mime/textMatching/:text, http://schemas.xmlsoap.org/wsdl/soap/:body, http://schemas.xmlsoap.org/wsdl/soap/:header, http://schemas.xmlsoap.org/wsdl/soap12/:body, http://schemas.xmlsoap.org/wsdl/soap12/:header");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private OutputBinding Read111_OutputBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id39_OutputBinding) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            OutputBinding o = new OutputBinding();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id40_content) && (base.Reader.NamespaceURI == this.id41_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read93_MimeContentBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id42_mimeXml) && (base.Reader.NamespaceURI == this.id41_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read94_MimeXmlBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id43_multipartRelated) && (base.Reader.NamespaceURI == this.id41_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read104_MimeMultipartRelatedBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id44_text) && (base.Reader.NamespaceURI == this.id45_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read97_MimeTextBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id46_body) && (base.Reader.NamespaceURI == this.id19_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read99_SoapBodyBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id47_header) && (base.Reader.NamespaceURI == this.id19_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read106_SoapHeaderBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id46_body) && (base.Reader.NamespaceURI == this.id20_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read102_Soap12BodyBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id47_header) && (base.Reader.NamespaceURI == this.id20_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read109_Soap12HeaderBinding(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/mime/:content, http://schemas.xmlsoap.org/wsdl/mime/:mimeXml, http://schemas.xmlsoap.org/wsdl/mime/:multipartRelated, http://microsoft.com/wsdl/mime/textMatching/:text, http://schemas.xmlsoap.org/wsdl/soap/:body, http://schemas.xmlsoap.org/wsdl/soap/:header, http://schemas.xmlsoap.org/wsdl/soap12/:body, http://schemas.xmlsoap.org/wsdl/soap12/:header");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private SoapFaultBinding Read112_SoapFaultBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id38_SoapFaultBinding) || (type.Namespace != this.id19_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            SoapFaultBinding o = new SoapFaultBinding();
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id35_use)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Use = this.Read98_SoapBindingUse(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.Reader.Value;
                        flagArray[3] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id37_encodingStyle)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Encoding = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :use, :name, :namespace, :encodingStyle");
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
                    base.UnknownNode(o, "");
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

        private Soap12FaultBinding Read114_Soap12FaultBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id34_Soap12FaultBinding) || (type.Namespace != this.id20_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Soap12FaultBinding o = new Soap12FaultBinding();
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id35_use)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Use = this.Read100_SoapBindingUse(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.Reader.Value;
                        flagArray[3] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id37_encodingStyle)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Encoding = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :use, :name, :namespace, :encodingStyle");
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
                    base.UnknownNode(o, "");
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

        private FaultBinding Read115_FaultBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id33_FaultBinding) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            FaultBinding o = new FaultBinding();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id32_fault) && (base.Reader.NamespaceURI == this.id19_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read112_SoapFaultBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id32_fault) && (base.Reader.NamespaceURI == this.id20_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read114_Soap12FaultBinding(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/soap/:fault, http://schemas.xmlsoap.org/wsdl/soap12/:fault");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private OperationBinding Read116_OperationBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id29_OperationBinding) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            OperationBinding o = new OperationBinding();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            FaultBindingCollection faults = o.Faults;
            bool[] flagArray = new bool[8];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id28_operation) && (base.Reader.NamespaceURI == this.id18_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read85_HttpOperationBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id28_operation) && (base.Reader.NamespaceURI == this.id19_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read86_SoapOperationBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id28_operation) && (base.Reader.NamespaceURI == this.id20_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read88_Soap12OperationBinding(false, true));
                        }
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id30_input)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Input = this.Read110_InputBinding(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id31_output)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Output = this.Read111_OutputBinding(false, true);
                        flagArray[6] = true;
                    }
                    else if ((base.Reader.LocalName == this.id32_fault) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (faults == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            faults.Add(this.Read115_FaultBinding(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/http/:operation, http://schemas.xmlsoap.org/wsdl/soap/:operation, http://schemas.xmlsoap.org/wsdl/soap12/:operation, http://schemas.xmlsoap.org/wsdl/:input, http://schemas.xmlsoap.org/wsdl/:output, http://schemas.xmlsoap.org/wsdl/:fault");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private Binding Read117_Binding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id26_Binding) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Binding o = new Binding();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            OperationBindingCollection operations = o.Operations;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id27_type)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Type = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[6] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id12_binding) && (base.Reader.NamespaceURI == this.id18_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read77_HttpBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id12_binding) && (base.Reader.NamespaceURI == this.id19_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read80_SoapBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id12_binding) && (base.Reader.NamespaceURI == this.id20_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read84_Soap12Binding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id28_operation) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (operations == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            operations.Add(this.Read116_OperationBinding(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/http/:binding, http://schemas.xmlsoap.org/wsdl/soap/:binding, http://schemas.xmlsoap.org/wsdl/soap12/:binding, http://schemas.xmlsoap.org/wsdl/:operation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private HttpAddressBinding Read118_HttpAddressBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id25_HttpAddressBinding) || (type.Namespace != this.id18_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            HttpAddressBinding o = new HttpAddressBinding();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id23_location)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Location = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :location");
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
                    base.UnknownNode(o, "");
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

        private SoapAddressBinding Read119_SoapAddressBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id24_SoapAddressBinding) || (type.Namespace != this.id19_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            SoapAddressBinding o = new SoapAddressBinding();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id23_location)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Location = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :location");
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
                    base.UnknownNode(o, "");
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

        private XmlSchemaInclude Read12_XmlSchemaInclude(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id208_XmlSchemaInclude) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaInclude o = new XmlSchemaInclude();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id206_schemaLocation)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.SchemaLocation = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Id = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[2] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[4] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private Soap12AddressBinding Read121_Soap12AddressBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id21_Soap12AddressBinding) || (type.Namespace != this.id20_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Soap12AddressBinding o = new Soap12AddressBinding();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id23_location)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Location = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :location");
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
                    base.UnknownNode(o, "");
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

        private Port Read122_Port(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id16_Port) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Port o = new Port();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id12_binding)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Binding = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id17_address) && (base.Reader.NamespaceURI == this.id18_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read118_HttpAddressBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id17_address) && (base.Reader.NamespaceURI == this.id19_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read119_SoapAddressBinding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id17_address) && (base.Reader.NamespaceURI == this.id20_Item))
                    {
                        if (extensions == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            extensions.Add(this.Read121_Soap12AddressBinding(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/http/:address, http://schemas.xmlsoap.org/wsdl/soap/:address, http://schemas.xmlsoap.org/wsdl/soap12/:address");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private Service Read123_Service(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id14_Service) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Service o = new Service();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            PortCollection ports = o.Ports;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id15_port) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (ports == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            ports.Add(this.Read122_Port(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/:port");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private ServiceDescription Read124_ServiceDescription(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id3_ServiceDescription) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            ServiceDescription o = new ServiceDescription();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            ImportCollection imports = o.Imports;
            MessageCollection messages = o.Messages;
            PortTypeCollection portTypes = o.PortTypes;
            BindingCollection bindings = o.Bindings;
            ServiceCollection services = o.Services;
            bool[] flagArray = new bool[12];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if ((!flagArray[11] && (base.Reader.LocalName == this.id6_targetNamespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.TargetNamespace = base.Reader.Value;
                        flagArray[11] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id8_import) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (imports == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            imports.Add(this.Read4_Import(false, true));
                        }
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id9_types)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Types = this.Read67_Types(false, true);
                        flagArray[6] = true;
                    }
                    else if ((base.Reader.LocalName == this.id10_message) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (messages == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            messages.Add(this.Read69_Message(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id11_portType) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (portTypes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            portTypes.Add(this.Read75_PortType(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id12_binding) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (bindings == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            bindings.Add(this.Read117_Binding(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id13_service) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (services == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            services.Add(this.Read123_Service(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/:import, http://schemas.xmlsoap.org/wsdl/:types, http://schemas.xmlsoap.org/wsdl/:message, http://schemas.xmlsoap.org/wsdl/:portType, http://schemas.xmlsoap.org/wsdl/:binding, http://schemas.xmlsoap.org/wsdl/:service");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        public object Read125_definitions()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id1_definitions) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read124_ServiceDescription(true, true);
            }
            base.UnknownNode(null, "http://schemas.xmlsoap.org/wsdl/:definitions");
            return null;
        }

        private XmlSchemaImport Read13_XmlSchemaImport(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id207_XmlSchemaImport) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaImport o = new XmlSchemaImport();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id206_schemaLocation)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.SchemaLocation = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Id = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[5] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaSimpleTypeList Read17_XmlSchemaSimpleTypeList(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id161_XmlSchemaSimpleTypeList) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaSimpleTypeList o = new XmlSchemaSimpleTypeList();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id162_itemType)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.ItemTypeName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id105_simpleType)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.ItemType = this.Read34_XmlSchemaSimpleType(false, true);
                        flagArray[5] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaFractionDigitsFacet Read20_XmlSchemaFractionDigitsFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id160_XmlSchemaFractionDigitsFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaFractionDigitsFacet o = new XmlSchemaFractionDigitsFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaMinInclusiveFacet Read21_XmlSchemaMinInclusiveFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id159_XmlSchemaMinInclusiveFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaMinInclusiveFacet o = new XmlSchemaMinInclusiveFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaMaxLengthFacet Read22_XmlSchemaMaxLengthFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id158_XmlSchemaMaxLengthFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaMaxLengthFacet o = new XmlSchemaMaxLengthFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaLengthFacet Read23_XmlSchemaLengthFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id157_XmlSchemaLengthFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaLengthFacet o = new XmlSchemaLengthFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaTotalDigitsFacet Read24_XmlSchemaTotalDigitsFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id156_XmlSchemaTotalDigitsFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaTotalDigitsFacet o = new XmlSchemaTotalDigitsFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaPatternFacet Read25_XmlSchemaPatternFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id155_XmlSchemaPatternFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaPatternFacet o = new XmlSchemaPatternFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaEnumerationFacet Read26_XmlSchemaEnumerationFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id154_XmlSchemaEnumerationFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaEnumerationFacet o = new XmlSchemaEnumerationFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaMaxInclusiveFacet Read27_XmlSchemaMaxInclusiveFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id153_XmlSchemaMaxInclusiveFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaMaxInclusiveFacet o = new XmlSchemaMaxInclusiveFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaMaxExclusiveFacet Read28_XmlSchemaMaxExclusiveFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id152_XmlSchemaMaxExclusiveFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaMaxExclusiveFacet o = new XmlSchemaMaxExclusiveFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaWhiteSpaceFacet Read29_XmlSchemaWhiteSpaceFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id151_XmlSchemaWhiteSpaceFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaWhiteSpaceFacet o = new XmlSchemaWhiteSpaceFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaMinExclusiveFacet Read30_XmlSchemaMinExclusiveFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id150_XmlSchemaMinExclusiveFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaMinExclusiveFacet o = new XmlSchemaMinExclusiveFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaMinLengthFacet Read31_XmlSchemaMinLengthFacet(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id148_XmlSchemaMinLengthFacet) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaMinLengthFacet o = new XmlSchemaMinLengthFacet();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id149_value)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Value = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsFixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaSimpleTypeRestriction Read32_XmlSchemaSimpleTypeRestriction(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id135_XmlSchemaSimpleTypeRestriction) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaSimpleTypeRestriction o = new XmlSchemaSimpleTypeRestriction();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection facets = o.Facets;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id136_base)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.BaseTypeName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id105_simpleType)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.BaseType = this.Read34_XmlSchemaSimpleType(false, true);
                        flagArray[5] = true;
                    }
                    else if ((base.Reader.LocalName == this.id137_fractionDigits) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read20_XmlSchemaFractionDigitsFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id138_minInclusive) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read21_XmlSchemaMinInclusiveFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id139_maxLength) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read22_XmlSchemaMaxLengthFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id140_length) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read23_XmlSchemaLengthFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id141_totalDigits) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read24_XmlSchemaTotalDigitsFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id62_pattern) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read25_XmlSchemaPatternFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id142_enumeration) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read26_XmlSchemaEnumerationFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id143_maxInclusive) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read27_XmlSchemaMaxInclusiveFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id144_maxExclusive) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read28_XmlSchemaMaxExclusiveFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id145_whiteSpace) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read29_XmlSchemaWhiteSpaceFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id146_minExclusive) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read30_XmlSchemaMinExclusiveFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id147_minLength) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read31_XmlSchemaMinLengthFacet(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:fractionDigits, http://www.w3.org/2001/XMLSchema:minInclusive, http://www.w3.org/2001/XMLSchema:maxLength, http://www.w3.org/2001/XMLSchema:length, http://www.w3.org/2001/XMLSchema:totalDigits, http://www.w3.org/2001/XMLSchema:pattern, http://www.w3.org/2001/XMLSchema:enumeration, http://www.w3.org/2001/XMLSchema:maxInclusive, http://www.w3.org/2001/XMLSchema:maxExclusive, http://www.w3.org/2001/XMLSchema:whiteSpace, http://www.w3.org/2001/XMLSchema:minExclusive, http://www.w3.org/2001/XMLSchema:minLength");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:fractionDigits, http://www.w3.org/2001/XMLSchema:minInclusive, http://www.w3.org/2001/XMLSchema:maxLength, http://www.w3.org/2001/XMLSchema:length, http://www.w3.org/2001/XMLSchema:totalDigits, http://www.w3.org/2001/XMLSchema:pattern, http://www.w3.org/2001/XMLSchema:enumeration, http://www.w3.org/2001/XMLSchema:maxInclusive, http://www.w3.org/2001/XMLSchema:maxExclusive, http://www.w3.org/2001/XMLSchema:whiteSpace, http://www.w3.org/2001/XMLSchema:minExclusive, http://www.w3.org/2001/XMLSchema:minLength");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaSimpleTypeUnion Read33_XmlSchemaSimpleTypeUnion(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id133_XmlSchemaSimpleTypeUnion) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaSimpleTypeUnion o = new XmlSchemaSimpleTypeUnion();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection baseTypes = o.BaseTypes;
            XmlQualifiedName[] nameArray = null;
            int num2 = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((base.Reader.LocalName == this.id134_memberTypes) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        string[] strArray = base.Reader.Value.Split(null);
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            nameArray = (XmlQualifiedName[]) base.EnsureArrayIndex(nameArray, num2, typeof(XmlQualifiedName));
                            nameArray[num2++] = base.ToXmlQualifiedName(strArray[i]);
                        }
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            o.MemberTypes = (XmlQualifiedName[]) base.ShrinkArray(nameArray, num2, typeof(XmlQualifiedName), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
                o.MemberTypes = (XmlQualifiedName[]) base.ShrinkArray(nameArray, num2, typeof(XmlQualifiedName), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((base.Reader.LocalName == this.id105_simpleType) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (baseTypes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            baseTypes.Add(this.Read34_XmlSchemaSimpleType(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            o.MemberTypes = (XmlQualifiedName[]) base.ShrinkArray(nameArray, num2, typeof(XmlQualifiedName), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaSimpleType Read34_XmlSchemaSimpleType(bool isNullable, bool checkType)
        {
            XmlQualifiedName name = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (name != null)) && ((name.Name != this.id128_XmlSchemaSimpleType) || (name.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(name);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaSimpleType o = new XmlSchemaSimpleType();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id129_final)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Final = this.Read7_XmlSchemaDerivationMethod(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id130_list)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Content = this.Read17_XmlSchemaSimpleTypeList(false, true);
                        flagArray[6] = true;
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id131_restriction)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Content = this.Read32_XmlSchemaSimpleTypeRestriction(false, true);
                        flagArray[6] = true;
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id132_union)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Content = this.Read33_XmlSchemaSimpleTypeUnion(false, true);
                        flagArray[6] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:list, http://www.w3.org/2001/XMLSchema:restriction, http://www.w3.org/2001/XMLSchema:union");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:list, http://www.w3.org/2001/XMLSchema:restriction, http://www.w3.org/2001/XMLSchema:union");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaUse Read35_XmlSchemaUse(string s)
        {
            switch (s)
            {
                case "optional":
                    return XmlSchemaUse.Optional;

                case "prohibited":
                    return XmlSchemaUse.Prohibited;

                case "required":
                    return XmlSchemaUse.Required;
            }
            throw base.CreateUnknownConstantException(s, typeof(XmlSchemaUse));
        }

        private XmlSchemaAttribute Read36_XmlSchemaAttribute(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id124_XmlSchemaAttribute) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaAttribute o = new XmlSchemaAttribute();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[12];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id125_default)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.DefaultValue = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.FixedValue = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id127_form)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Form = this.Read6_XmlSchemaForm(base.Reader.Value);
                        flagArray[6] = true;
                        continue;
                    }
                    if ((!flagArray[7] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[7] = true;
                        continue;
                    }
                    if ((!flagArray[8] && (base.Reader.LocalName == this.id123_ref)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.RefName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[8] = true;
                        continue;
                    }
                    if ((!flagArray[9] && (base.Reader.LocalName == this.id27_type)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.SchemaTypeName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[9] = true;
                        continue;
                    }
                    if ((!flagArray[11] && (base.Reader.LocalName == this.id35_use)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Use = this.Read35_XmlSchemaUse(base.Reader.Value);
                        flagArray[11] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[10] && (base.Reader.LocalName == this.id105_simpleType)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.SchemaType = this.Read34_XmlSchemaSimpleType(false, true);
                        flagArray[10] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaAttributeGroupRef Read37_XmlSchemaAttributeGroupRef(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id122_XmlSchemaAttributeGroupRef) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaAttributeGroupRef o = new XmlSchemaAttributeGroupRef();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id123_ref)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.RefName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaContentProcessing Read38_XmlSchemaContentProcessing(string s)
        {
            switch (s)
            {
                case "skip":
                    return XmlSchemaContentProcessing.Skip;

                case "lax":
                    return XmlSchemaContentProcessing.Lax;

                case "strict":
                    return XmlSchemaContentProcessing.Strict;
            }
            throw base.CreateUnknownConstantException(s, typeof(XmlSchemaContentProcessing));
        }

        private XmlSchemaAnyAttribute Read39_XmlSchemaAnyAttribute(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id113_XmlSchemaAnyAttribute) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaAnyAttribute o = new XmlSchemaAnyAttribute();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id114_processContents)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.ProcessContents = this.Read38_XmlSchemaContentProcessing(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private Import Read4_Import(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id209_Import) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Import o = new Import();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[4] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Namespace = base.Reader.Value;
                    flagArray[4] = true;
                }
                else
                {
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id23_location)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Location = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaAttributeGroup Read40_XmlSchemaAttributeGroup(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id111_XmlSchemaAttributeGroup) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaAttributeGroup o = new XmlSchemaAttributeGroup();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection attributes = o.Attributes;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((base.Reader.LocalName == this.id109_attribute) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read36_XmlSchemaAttribute(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id110_attributeGroup) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read37_XmlSchemaAttributeGroupRef(false, true));
                        }
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id112_anyAttribute)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.AnyAttribute = this.Read39_XmlSchemaAnyAttribute(false, true);
                        flagArray[6] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaGroupRef Read44_XmlSchemaGroupRef(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id191_XmlSchemaGroupRef) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaGroupRef o = new XmlSchemaGroupRef();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id164_minOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MinOccursString = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id165_maxOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MaxOccursString = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id123_ref)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.RefName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[6] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaAny Read46_XmlSchemaAny(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id193_XmlSchemaAny) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaAny o = new XmlSchemaAny();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[8];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id164_minOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MinOccursString = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id165_maxOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MaxOccursString = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.Reader.Value;
                        flagArray[6] = true;
                        continue;
                    }
                    if ((!flagArray[7] && (base.Reader.LocalName == this.id114_processContents)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.ProcessContents = this.Read38_XmlSchemaContentProcessing(base.Reader.Value);
                        flagArray[7] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaXPath Read47_XmlSchemaXPath(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id177_XmlSchemaXPath) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaXPath o = new XmlSchemaXPath();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id178_xpath)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.XPath = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaKey Read49_XmlSchemaKey(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id180_XmlSchemaKey) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaKey o = new XmlSchemaKey();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection fields = o.Fields;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id175_selector)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Selector = this.Read47_XmlSchemaXPath(false, true);
                        flagArray[5] = true;
                    }
                    else if ((base.Reader.LocalName == this.id176_field) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (fields == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            fields.Add(this.Read47_XmlSchemaXPath(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaUnique Read50_XmlSchemaUnique(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id179_XmlSchemaUnique) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaUnique o = new XmlSchemaUnique();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection fields = o.Fields;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id175_selector)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Selector = this.Read47_XmlSchemaXPath(false, true);
                        flagArray[5] = true;
                    }
                    else if ((base.Reader.LocalName == this.id176_field) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (fields == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            fields.Add(this.Read47_XmlSchemaXPath(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaKeyref Read51_XmlSchemaKeyref(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id173_XmlSchemaKeyref) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaKeyref o = new XmlSchemaKeyref();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection fields = o.Fields;
            bool[] flagArray = new bool[8];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[7] && (base.Reader.LocalName == this.id174_refer)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Refer = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[7] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id175_selector)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Selector = this.Read47_XmlSchemaXPath(false, true);
                        flagArray[5] = true;
                    }
                    else if ((base.Reader.LocalName == this.id176_field) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (fields == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            fields.Add(this.Read47_XmlSchemaXPath(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:selector, http://www.w3.org/2001/XMLSchema:field");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaElement Read52_XmlSchemaElement(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id163_XmlSchemaElement) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaElement o = new XmlSchemaElement();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection constraints = o.Constraints;
            bool[] flagArray = new bool[0x13];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id164_minOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MinOccursString = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id165_maxOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MaxOccursString = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id166_abstract)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsAbstract = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[6] = true;
                        continue;
                    }
                    if ((!flagArray[7] && (base.Reader.LocalName == this.id167_block)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Block = this.Read7_XmlSchemaDerivationMethod(base.Reader.Value);
                        flagArray[7] = true;
                        continue;
                    }
                    if ((!flagArray[8] && (base.Reader.LocalName == this.id125_default)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.DefaultValue = base.Reader.Value;
                        flagArray[8] = true;
                        continue;
                    }
                    if ((!flagArray[9] && (base.Reader.LocalName == this.id129_final)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Final = this.Read7_XmlSchemaDerivationMethod(base.Reader.Value);
                        flagArray[9] = true;
                        continue;
                    }
                    if ((!flagArray[10] && (base.Reader.LocalName == this.id126_fixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.FixedValue = base.Reader.Value;
                        flagArray[10] = true;
                        continue;
                    }
                    if ((!flagArray[11] && (base.Reader.LocalName == this.id127_form)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Form = this.Read6_XmlSchemaForm(base.Reader.Value);
                        flagArray[11] = true;
                        continue;
                    }
                    if ((!flagArray[12] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[12] = true;
                        continue;
                    }
                    if ((!flagArray[13] && (base.Reader.LocalName == this.id168_nillable)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsNillable = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[13] = true;
                        continue;
                    }
                    if ((!flagArray[14] && (base.Reader.LocalName == this.id123_ref)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.RefName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[14] = true;
                        continue;
                    }
                    if ((!flagArray[15] && (base.Reader.LocalName == this.id169_substitutionGroup)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.SubstitutionGroup = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[15] = true;
                        continue;
                    }
                    if ((!flagArray[0x10] && (base.Reader.LocalName == this.id27_type)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.SchemaTypeName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[0x10] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[0x11] && (base.Reader.LocalName == this.id105_simpleType)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.SchemaType = this.Read34_XmlSchemaSimpleType(false, true);
                        flagArray[0x11] = true;
                    }
                    else if ((!flagArray[0x11] && (base.Reader.LocalName == this.id106_complexType)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.SchemaType = this.Read62_XmlSchemaComplexType(false, true);
                        flagArray[0x11] = true;
                    }
                    else if ((base.Reader.LocalName == this.id170_key) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (constraints == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            constraints.Add(this.Read49_XmlSchemaKey(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id171_unique) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (constraints == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            constraints.Add(this.Read50_XmlSchemaUnique(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id172_keyref) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (constraints == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            constraints.Add(this.Read51_XmlSchemaKeyref(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:key, http://www.w3.org/2001/XMLSchema:unique, http://www.w3.org/2001/XMLSchema:keyref");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:key, http://www.w3.org/2001/XMLSchema:unique, http://www.w3.org/2001/XMLSchema:keyref");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaSequence Read53_XmlSchemaSequence(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id192_XmlSchemaSequence) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaSequence o = new XmlSchemaSequence();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection items = o.Items;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id164_minOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MinOccursString = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id165_maxOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MaxOccursString = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((base.Reader.LocalName == this.id92_element) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read52_XmlSchemaElement(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id185_sequence) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read53_XmlSchemaSequence(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id190_any) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read46_XmlSchemaAny(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id186_choice) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read54_XmlSchemaChoice(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id59_group) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read44_XmlSchemaGroupRef(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:any, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:group");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:any, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:group");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaChoice Read54_XmlSchemaChoice(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id189_XmlSchemaChoice) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaChoice o = new XmlSchemaChoice();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection items = o.Items;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id164_minOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MinOccursString = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id165_maxOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MaxOccursString = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((base.Reader.LocalName == this.id190_any) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read46_XmlSchemaAny(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id186_choice) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read54_XmlSchemaChoice(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id185_sequence) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read53_XmlSchemaSequence(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id92_element) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read52_XmlSchemaElement(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id59_group) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read44_XmlSchemaGroupRef(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:any, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:group");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:any, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:group");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaAll Read55_XmlSchemaAll(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id188_XmlSchemaAll) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaAll o = new XmlSchemaAll();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection items = o.Items;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id164_minOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MinOccursString = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id165_maxOccurs)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.MaxOccursString = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((base.Reader.LocalName == this.id92_element) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read52_XmlSchemaElement(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:element");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:element");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaComplexContentExtension Read56_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id200_Item) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaComplexContentExtension o = new XmlSchemaComplexContentExtension();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection attributes = o.Attributes;
            bool[] flagArray = new bool[8];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id136_base)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.BaseTypeName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id59_group)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read44_XmlSchemaGroupRef(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id186_choice)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read54_XmlSchemaChoice(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id187_all)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read55_XmlSchemaAll(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id185_sequence)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read53_XmlSchemaSequence(false, true);
                        flagArray[5] = true;
                    }
                    else if ((base.Reader.LocalName == this.id110_attributeGroup) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read37_XmlSchemaAttributeGroupRef(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id109_attribute) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read36_XmlSchemaAttribute(false, true));
                        }
                    }
                    else if ((!flagArray[7] && (base.Reader.LocalName == this.id112_anyAttribute)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.AnyAttribute = this.Read39_XmlSchemaAnyAttribute(false, true);
                        flagArray[7] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaComplexContentRestriction Read57_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id199_Item) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaComplexContentRestriction o = new XmlSchemaComplexContentRestriction();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection attributes = o.Attributes;
            bool[] flagArray = new bool[8];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id136_base)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.BaseTypeName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id186_choice)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read54_XmlSchemaChoice(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id59_group)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read44_XmlSchemaGroupRef(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id187_all)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read55_XmlSchemaAll(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id185_sequence)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read53_XmlSchemaSequence(false, true);
                        flagArray[5] = true;
                    }
                    else if ((base.Reader.LocalName == this.id110_attributeGroup) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read37_XmlSchemaAttributeGroupRef(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id109_attribute) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read36_XmlSchemaAttribute(false, true));
                        }
                    }
                    else if ((!flagArray[7] && (base.Reader.LocalName == this.id112_anyAttribute)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.AnyAttribute = this.Read39_XmlSchemaAnyAttribute(false, true);
                        flagArray[7] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaComplexContent Read58_XmlSchemaComplexContent(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id198_XmlSchemaComplexContent) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaComplexContent o = new XmlSchemaComplexContent();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id182_mixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsMixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id195_extension)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Content = this.Read56_Item(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id131_restriction)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Content = this.Read57_Item(false, true);
                        flagArray[5] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:extension, http://www.w3.org/2001/XMLSchema:restriction");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:extension, http://www.w3.org/2001/XMLSchema:restriction");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaSimpleContentRestriction Read59_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id197_Item) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaSimpleContentRestriction o = new XmlSchemaSimpleContentRestriction();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection facets = o.Facets;
            XmlSchemaObjectCollection attributes = o.Attributes;
            bool[] flagArray = new bool[9];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id136_base)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.BaseTypeName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id105_simpleType)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.BaseType = this.Read34_XmlSchemaSimpleType(false, true);
                        flagArray[5] = true;
                    }
                    else if ((base.Reader.LocalName == this.id138_minInclusive) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read21_XmlSchemaMinInclusiveFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id144_maxExclusive) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read28_XmlSchemaMaxExclusiveFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id145_whiteSpace) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read29_XmlSchemaWhiteSpaceFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id147_minLength) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read31_XmlSchemaMinLengthFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id62_pattern) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read25_XmlSchemaPatternFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id142_enumeration) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read26_XmlSchemaEnumerationFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id143_maxInclusive) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read27_XmlSchemaMaxInclusiveFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id140_length) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read23_XmlSchemaLengthFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id139_maxLength) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read22_XmlSchemaMaxLengthFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id146_minExclusive) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read30_XmlSchemaMinExclusiveFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id141_totalDigits) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read24_XmlSchemaTotalDigitsFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id137_fractionDigits) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (facets == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            facets.Add(this.Read20_XmlSchemaFractionDigitsFacet(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id110_attributeGroup) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read37_XmlSchemaAttributeGroupRef(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id109_attribute) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read36_XmlSchemaAttribute(false, true));
                        }
                    }
                    else if ((!flagArray[8] && (base.Reader.LocalName == this.id112_anyAttribute)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.AnyAttribute = this.Read39_XmlSchemaAnyAttribute(false, true);
                        flagArray[8] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:minInclusive, http://www.w3.org/2001/XMLSchema:maxExclusive, http://www.w3.org/2001/XMLSchema:whiteSpace, http://www.w3.org/2001/XMLSchema:minLength, http://www.w3.org/2001/XMLSchema:pattern, http://www.w3.org/2001/XMLSchema:enumeration, http://www.w3.org/2001/XMLSchema:maxInclusive, http://www.w3.org/2001/XMLSchema:length, http://www.w3.org/2001/XMLSchema:maxLength, http://www.w3.org/2001/XMLSchema:minExclusive, http://www.w3.org/2001/XMLSchema:totalDigits, http://www.w3.org/2001/XMLSchema:fractionDigits, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:minInclusive, http://www.w3.org/2001/XMLSchema:maxExclusive, http://www.w3.org/2001/XMLSchema:whiteSpace, http://www.w3.org/2001/XMLSchema:minLength, http://www.w3.org/2001/XMLSchema:pattern, http://www.w3.org/2001/XMLSchema:enumeration, http://www.w3.org/2001/XMLSchema:maxInclusive, http://www.w3.org/2001/XMLSchema:length, http://www.w3.org/2001/XMLSchema:maxLength, http://www.w3.org/2001/XMLSchema:minExclusive, http://www.w3.org/2001/XMLSchema:totalDigits, http://www.w3.org/2001/XMLSchema:fractionDigits, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaForm Read6_XmlSchemaForm(string s)
        {
            switch (s)
            {
                case "qualified":
                    return XmlSchemaForm.Qualified;

                case "unqualified":
                    return XmlSchemaForm.Unqualified;
            }
            throw base.CreateUnknownConstantException(s, typeof(XmlSchemaForm));
        }

        private XmlSchemaSimpleContentExtension Read60_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id196_Item) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaSimpleContentExtension o = new XmlSchemaSimpleContentExtension();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection attributes = o.Attributes;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id136_base)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.BaseTypeName = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((base.Reader.LocalName == this.id110_attributeGroup) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read37_XmlSchemaAttributeGroupRef(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id109_attribute) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read36_XmlSchemaAttribute(false, true));
                        }
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id112_anyAttribute)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.AnyAttribute = this.Read39_XmlSchemaAnyAttribute(false, true);
                        flagArray[6] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaSimpleContent Read61_XmlSchemaSimpleContent(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id194_XmlSchemaSimpleContent) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaSimpleContent o = new XmlSchemaSimpleContent();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[4] && (base.Reader.LocalName == this.id131_restriction)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Content = this.Read59_Item(false, true);
                        flagArray[4] = true;
                    }
                    else if ((!flagArray[4] && (base.Reader.LocalName == this.id195_extension)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Content = this.Read60_Item(false, true);
                        flagArray[4] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:restriction, http://www.w3.org/2001/XMLSchema:extension");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:restriction, http://www.w3.org/2001/XMLSchema:extension");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaComplexType Read62_XmlSchemaComplexType(bool isNullable, bool checkType)
        {
            XmlQualifiedName name = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (name != null)) && ((name.Name != this.id181_XmlSchemaComplexType) || (name.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(name);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaComplexType o = new XmlSchemaComplexType();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection attributes = o.Attributes;
            bool[] flagArray = new bool[13];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id129_final)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Final = this.Read7_XmlSchemaDerivationMethod(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id166_abstract)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsAbstract = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[6] = true;
                        continue;
                    }
                    if ((!flagArray[7] && (base.Reader.LocalName == this.id167_block)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Block = this.Read7_XmlSchemaDerivationMethod(base.Reader.Value);
                        flagArray[7] = true;
                        continue;
                    }
                    if ((!flagArray[8] && (base.Reader.LocalName == this.id182_mixed)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IsMixed = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[8] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[9] && (base.Reader.LocalName == this.id183_complexContent)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.ContentModel = this.Read58_XmlSchemaComplexContent(false, true);
                        flagArray[9] = true;
                    }
                    else if ((!flagArray[9] && (base.Reader.LocalName == this.id184_simpleContent)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.ContentModel = this.Read61_XmlSchemaSimpleContent(false, true);
                        flagArray[9] = true;
                    }
                    else if ((!flagArray[10] && (base.Reader.LocalName == this.id59_group)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read44_XmlSchemaGroupRef(false, true);
                        flagArray[10] = true;
                    }
                    else if ((!flagArray[10] && (base.Reader.LocalName == this.id185_sequence)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read53_XmlSchemaSequence(false, true);
                        flagArray[10] = true;
                    }
                    else if ((!flagArray[10] && (base.Reader.LocalName == this.id186_choice)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read54_XmlSchemaChoice(false, true);
                        flagArray[10] = true;
                    }
                    else if ((!flagArray[10] && (base.Reader.LocalName == this.id187_all)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read55_XmlSchemaAll(false, true);
                        flagArray[10] = true;
                    }
                    else if ((base.Reader.LocalName == this.id109_attribute) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read36_XmlSchemaAttribute(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id110_attributeGroup) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (attributes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            attributes.Add(this.Read37_XmlSchemaAttributeGroupRef(false, true));
                        }
                    }
                    else if ((!flagArray[12] && (base.Reader.LocalName == this.id112_anyAttribute)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.AnyAttribute = this.Read39_XmlSchemaAnyAttribute(false, true);
                        flagArray[12] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:complexContent, http://www.w3.org/2001/XMLSchema:simpleContent, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:anyAttribute");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:complexContent, http://www.w3.org/2001/XMLSchema:simpleContent, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:anyAttribute");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaGroup Read63_XmlSchemaGroup(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id201_XmlSchemaGroup) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaGroup o = new XmlSchemaGroup();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id185_sequence)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read53_XmlSchemaSequence(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id186_choice)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read54_XmlSchemaChoice(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id187_all)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Particle = this.Read55_XmlSchemaAll(false, true);
                        flagArray[5] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:sequence, http://www.w3.org/2001/XMLSchema:choice, http://www.w3.org/2001/XMLSchema:all");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaRedefine Read64_XmlSchemaRedefine(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id205_XmlSchemaRedefine) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaRedefine o = new XmlSchemaRedefine();
            XmlAttribute[] a = null;
            int index = 0;
            XmlSchemaObjectCollection items = o.Items;
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id206_schemaLocation)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.SchemaLocation = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Id = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[2] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((base.Reader.LocalName == this.id110_attributeGroup) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read40_XmlSchemaAttributeGroup(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id106_complexType) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read62_XmlSchemaComplexType(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id105_simpleType) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read34_XmlSchemaSimpleType(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id107_annotation) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read11_XmlSchemaAnnotation(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id59_group) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read63_XmlSchemaGroup(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:group");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:attributeGroup, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:group");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaNotation Read65_XmlSchemaNotation(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id202_XmlSchemaNotation) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaNotation o = new XmlSchemaNotation();
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Id = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Name = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id203_public)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Public = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id204_system)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.System = base.Reader.Value;
                        flagArray[6] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id107_annotation)) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        o.Annotation = this.Read11_XmlSchemaAnnotation(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:annotation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchema Read66_XmlSchema(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id96_XmlSchema) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchema o = new XmlSchema();
            XmlSchemaObjectCollection includes = o.Includes;
            XmlSchemaObjectCollection items = o.Items;
            XmlAttribute[] a = null;
            int index = 0;
            bool[] flagArray = new bool[11];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id97_attributeFormDefault)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.AttributeFormDefault = this.Read6_XmlSchemaForm(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id98_blockDefault)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.BlockDefault = this.Read7_XmlSchemaDerivationMethod(base.Reader.Value);
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id99_finalDefault)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.FinalDefault = this.Read7_XmlSchemaDerivationMethod(base.Reader.Value);
                        flagArray[3] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id100_elementFormDefault)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.ElementFormDefault = this.Read6_XmlSchemaForm(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id6_targetNamespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.TargetNamespace = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id101_version)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Version = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[6] = true;
                        continue;
                    }
                    if ((!flagArray[9] && (base.Reader.LocalName == this.id102_id)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Id = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[9] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((base.Reader.LocalName == this.id103_include) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (includes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            includes.Add(this.Read12_XmlSchemaInclude(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id8_import) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (includes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            includes.Add(this.Read13_XmlSchemaImport(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id104_redefine) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (includes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            includes.Add(this.Read64_XmlSchemaRedefine(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id105_simpleType) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read34_XmlSchemaSimpleType(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id106_complexType) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read62_XmlSchemaComplexType(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id107_annotation) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read11_XmlSchemaAnnotation(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id108_notation) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read65_XmlSchemaNotation(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id59_group) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read63_XmlSchemaGroup(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id92_element) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read52_XmlSchemaElement(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id109_attribute) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read36_XmlSchemaAttribute(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id110_attributeGroup) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (items == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            items.Add(this.Read40_XmlSchemaAttributeGroup(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:include, http://www.w3.org/2001/XMLSchema:import, http://www.w3.org/2001/XMLSchema:redefine, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:notation, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://www.w3.org/2001/XMLSchema:include, http://www.w3.org/2001/XMLSchema:import, http://www.w3.org/2001/XMLSchema:redefine, http://www.w3.org/2001/XMLSchema:simpleType, http://www.w3.org/2001/XMLSchema:complexType, http://www.w3.org/2001/XMLSchema:annotation, http://www.w3.org/2001/XMLSchema:notation, http://www.w3.org/2001/XMLSchema:group, http://www.w3.org/2001/XMLSchema:element, http://www.w3.org/2001/XMLSchema:attribute, http://www.w3.org/2001/XMLSchema:attributeGroup");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.UnhandledAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private Types Read67_Types(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id93_Types) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Types o = new Types();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            XmlSchemas schemas = o.Schemas;
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if (base.IsXmlnsAttribute(base.Reader.Name))
                {
                    if (o.Namespaces == null)
                    {
                        o.Namespaces = new XmlSerializerNamespaces();
                    }
                    o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                }
                else
                {
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id94_schema) && (base.Reader.NamespaceURI == this.id95_Item))
                    {
                        if (schemas == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            schemas.Add(this.Read66_XmlSchema(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://www.w3.org/2001/XMLSchema:schema");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private MessagePart Read68_MessagePart(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id91_MessagePart) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            MessagePart o = new MessagePart();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            bool[] flagArray = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id92_element)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Element = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[5] = true;
                        continue;
                    }
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id27_type)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Type = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[6] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private Message Read69_Message(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id90_Message) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Message o = new Message();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            MessagePartCollection parts = o.Parts;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id49_part) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (parts == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            parts.Add(this.Read68_MessagePart(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/:part");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private XmlSchemaDerivationMethod Read7_XmlSchemaDerivationMethod(string s)
        {
            return (XmlSchemaDerivationMethod) ((int) XmlSerializationReader.ToEnum(s, this.XmlSchemaDerivationMethodValues, "global::System.Xml.Schema.XmlSchemaDerivationMethod"));
        }

        private OperationInput Read71_OperationInput(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id89_OperationInput) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            OperationInput o = new OperationInput();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id10_message)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Message = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private OperationOutput Read72_OperationOutput(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id88_OperationOutput) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            OperationOutput o = new OperationOutput();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id10_message)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Message = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private OperationFault Read73_OperationFault(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id87_OperationFault) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            OperationFault o = new OperationFault();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id10_message)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Message = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[4] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private Operation Read74_Operation(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id85_Operation) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Operation o = new Operation();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            OperationMessageCollection messages = o.Messages;
            OperationFaultCollection faults = o.Faults;
            bool[] flagArray = new bool[8];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id86_parameterOrder)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.ParameterOrderString = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id30_input) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (messages == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            messages.Add(this.Read71_OperationInput(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id31_output) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (messages == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            messages.Add(this.Read72_OperationOutput(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id32_fault) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (faults == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            faults.Add(this.Read73_OperationFault(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/:input, http://schemas.xmlsoap.org/wsdl/:output, http://schemas.xmlsoap.org/wsdl/:fault");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private PortType Read75_PortType(bool isNullable, bool checkType)
        {
            XmlQualifiedName name = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (name != null)) && ((name.Name != this.id84_PortType) || (name.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(name);
            }
            if (flag)
            {
                return null;
            }
            PortType o = new PortType();
            XmlAttribute[] a = null;
            int index = 0;
            ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
            OperationCollection operations = o.Operations;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[3] = true;
                }
                else
                {
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    XmlAttribute attr = (XmlAttribute) base.Document.ReadNode(base.Reader);
                    base.ParseWsdlArrayType(attr);
                    a = (XmlAttribute[]) base.EnsureArrayIndex(a, index, typeof(XmlAttribute));
                    a[index++] = attr;
                }
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
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
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id7_documentation)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DocumentationElement = (XmlElement) base.ReadXmlNode(false);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id28_operation) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (operations == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            operations.Add(this.Read74_Operation(false, true));
                        }
                    }
                    else
                    {
                        extensions.Add((XmlElement) base.ReadXmlNode(false));
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:documentation, http://schemas.xmlsoap.org/wsdl/:operation");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.ExtensibleAttributes = (XmlAttribute[]) base.ShrinkArray(a, index, typeof(XmlAttribute), true);
            base.ReadEndElement();
            return o;
        }

        private HttpBinding Read77_HttpBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id82_HttpBinding) || (type.Namespace != this.id18_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            HttpBinding o = new HttpBinding();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id83_verb)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Verb = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :verb");
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
                    base.UnknownNode(o, "");
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

        private SoapBindingStyle Read79_SoapBindingStyle(string s)
        {
            switch (s)
            {
                case "document":
                    return SoapBindingStyle.Document;

                case "rpc":
                    return SoapBindingStyle.Rpc;
            }
            throw base.CreateUnknownConstantException(s, typeof(SoapBindingStyle));
        }

        private SoapBinding Read80_SoapBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id81_SoapBinding) || (type.Namespace != this.id19_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            SoapBinding o = new SoapBinding();
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id80_transport)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Transport = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id75_style)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Style = this.Read79_SoapBindingStyle(base.Reader.Value);
                        flagArray[2] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :transport, :style");
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
                    base.UnknownNode(o, "");
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

        private SoapBindingStyle Read82_SoapBindingStyle(string s)
        {
            switch (s)
            {
                case "document":
                    return SoapBindingStyle.Document;

                case "rpc":
                    return SoapBindingStyle.Rpc;
            }
            throw base.CreateUnknownConstantException(s, typeof(SoapBindingStyle));
        }

        private Soap12Binding Read84_Soap12Binding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id79_Soap12Binding) || (type.Namespace != this.id20_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Soap12Binding o = new Soap12Binding();
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id80_transport)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Transport = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id75_style)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Style = this.Read82_SoapBindingStyle(base.Reader.Value);
                        flagArray[2] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :transport, :style");
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
                    base.UnknownNode(o, "");
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

        private HttpOperationBinding Read85_HttpOperationBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id78_HttpOperationBinding) || (type.Namespace != this.id18_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            HttpOperationBinding o = new HttpOperationBinding();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id23_location)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Location = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :location");
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
                    base.UnknownNode(o, "");
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

        private SoapOperationBinding Read86_SoapOperationBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id77_SoapOperationBinding) || (type.Namespace != this.id19_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            SoapOperationBinding o = new SoapOperationBinding();
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id74_soapAction)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.SoapAction = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id75_style)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Style = this.Read79_SoapBindingStyle(base.Reader.Value);
                        flagArray[2] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :soapAction, :style");
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
                    base.UnknownNode(o, "");
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

        private Soap12OperationBinding Read88_Soap12OperationBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id73_Soap12OperationBinding) || (type.Namespace != this.id20_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Soap12OperationBinding o = new Soap12OperationBinding();
            bool[] flagArray = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id74_soapAction)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.SoapAction = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id75_style)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Style = this.Read82_SoapBindingStyle(base.Reader.Value);
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id76_soapActionRequired)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.SoapActionRequired = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[3] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :soapAction, :style, :soapActionRequired");
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
                    base.UnknownNode(o, "");
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

        private XmlSchemaDocumentation Read9_XmlSchemaDocumentation(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id119_XmlSchemaDocumentation) || (type.Namespace != this.id95_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            base.DecodeName = false;
            XmlSchemaDocumentation o = new XmlSchemaDocumentation();
            XmlNode[] a = null;
            int length = 0;
            bool[] flagArray = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id118_source)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Source = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id120_lang)) && (base.Reader.NamespaceURI == this.id121_Item))
                    {
                        o.Language = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if (base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        if (o.Namespaces == null)
                        {
                            o.Namespaces = new XmlSerializerNamespaces();
                        }
                        o.Namespaces.Add((base.Reader.Name.Length == 5) ? "" : base.Reader.LocalName, base.Reader.Value);
                        continue;
                    }
                    base.UnknownNode(o, ":source, http://www.w3.org/XML/1998/namespace");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Markup = (XmlNode[]) base.ShrinkArray(a, length, typeof(XmlNode), true);
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
                    a = (XmlNode[]) base.EnsureArrayIndex(a, length, typeof(XmlNode));
                    a[length++] = base.ReadXmlNode(false);
                }
                else if (((base.Reader.NodeType == XmlNodeType.Text) || (base.Reader.NodeType == XmlNodeType.CDATA)) || ((base.Reader.NodeType == XmlNodeType.Whitespace) || (base.Reader.NodeType == XmlNodeType.SignificantWhitespace)))
                {
                    a = (XmlNode[]) base.EnsureArrayIndex(a, length, typeof(XmlNode));
                    a[length++] = base.Document.CreateTextNode(base.Reader.ReadString());
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Markup = (XmlNode[]) base.ShrinkArray(a, length, typeof(XmlNode), true);
            base.ReadEndElement();
            return o;
        }

        private HttpUrlEncodedBinding Read90_HttpUrlEncodedBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id72_HttpUrlEncodedBinding) || (type.Namespace != this.id18_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            HttpUrlEncodedBinding o = new HttpUrlEncodedBinding();
            bool[] flagArray = new bool[1];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required");
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
                    base.UnknownNode(o, "");
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

        private HttpUrlReplacementBinding Read91_HttpUrlReplacementBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id71_HttpUrlReplacementBinding) || (type.Namespace != this.id18_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            HttpUrlReplacementBinding o = new HttpUrlReplacementBinding();
            bool[] flagArray = new bool[1];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required");
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
                    base.UnknownNode(o, "");
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

        private MimeContentBinding Read93_MimeContentBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id67_MimeContentBinding) || (type.Namespace != this.id41_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            MimeContentBinding o = new MimeContentBinding();
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id49_part)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Part = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id27_type)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Type = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :part, :type");
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
                    base.UnknownNode(o, "");
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

        private MimeXmlBinding Read94_MimeXmlBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id66_MimeXmlBinding) || (type.Namespace != this.id41_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            MimeXmlBinding o = new MimeXmlBinding();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id49_part)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Part = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :part");
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
                    base.UnknownNode(o, "");
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

        private MimeTextMatch Read96_MimeTextMatch(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id58_MimeTextMatch) || (type.Namespace != this.id45_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            MimeTextMatch o = new MimeTextMatch();
            MimeTextMatchCollection matches = o.Matches;
            bool[] flagArray = new bool[8];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id4_name)) && (base.Reader.NamespaceURI == this.id5_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id27_type)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Type = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id59_group)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Group = XmlConvert.ToInt32(base.Reader.Value);
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id60_capture)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Capture = XmlConvert.ToInt32(base.Reader.Value);
                        flagArray[3] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id61_repeats)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.RepeatsString = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if ((!flagArray[5] && (base.Reader.LocalName == this.id62_pattern)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Pattern = base.Reader.Value;
                        flagArray[5] = true;
                        continue;
                    }
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id63_ignoreCase)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.IgnoreCase = XmlConvert.ToBoolean(base.Reader.Value);
                        flagArray[6] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":name, :type, :group, :capture, :repeats, :pattern, :ignoreCase");
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
                    if ((base.Reader.LocalName == this.id57_match) && (base.Reader.NamespaceURI == this.id45_Item))
                    {
                        if (matches == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            matches.Add(this.Read96_MimeTextMatch(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://microsoft.com/wsdl/mime/textMatching/:match");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://microsoft.com/wsdl/mime/textMatching/:match");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private MimeTextBinding Read97_MimeTextBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id56_MimeTextBinding) || (type.Namespace != this.id45_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            MimeTextBinding o = new MimeTextBinding();
            MimeTextMatchCollection matches = o.Matches;
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required");
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
                    if ((base.Reader.LocalName == this.id57_match) && (base.Reader.NamespaceURI == this.id45_Item))
                    {
                        if (matches == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            matches.Add(this.Read96_MimeTextMatch(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://microsoft.com/wsdl/mime/textMatching/:match");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://microsoft.com/wsdl/mime/textMatching/:match");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private SoapBindingUse Read98_SoapBindingUse(string s)
        {
            switch (s)
            {
                case "encoded":
                    return SoapBindingUse.Encoded;

                case "literal":
                    return SoapBindingUse.Literal;
            }
            throw base.CreateUnknownConstantException(s, typeof(SoapBindingUse));
        }

        private SoapBodyBinding Read99_SoapBodyBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id55_SoapBodyBinding) || (type.Namespace != this.id19_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            SoapBodyBinding o = new SoapBodyBinding();
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id22_required)) && (base.Reader.NamespaceURI == this.id2_Item))
                {
                    o.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id35_use)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Use = this.Read98_SoapBindingUse(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id36_namespace)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Namespace = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id37_encodingStyle)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.Encoding = base.Reader.Value;
                        flagArray[3] = true;
                        continue;
                    }
                    if ((!flagArray[4] && (base.Reader.LocalName == this.id53_parts)) && (base.Reader.NamespaceURI == this.id5_Item))
                    {
                        o.PartsString = base.Reader.Value;
                        flagArray[4] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/wsdl/:required, :use, :namespace, :encodingStyle, :parts");
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
                    base.UnknownNode(o, "");
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

        internal Hashtable XmlSchemaDerivationMethodValues
        {
            get
            {
                if (this._XmlSchemaDerivationMethodValues == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable.Add("", 0L);
                    hashtable.Add("substitution", 1L);
                    hashtable.Add("extension", 2L);
                    hashtable.Add("restriction", 4L);
                    hashtable.Add("list", 8L);
                    hashtable.Add("union", 0x10L);
                    hashtable.Add("#all", 0xffL);
                    this._XmlSchemaDerivationMethodValues = hashtable;
                }
                return this._XmlSchemaDerivationMethodValues;
            }
        }
    }
}

