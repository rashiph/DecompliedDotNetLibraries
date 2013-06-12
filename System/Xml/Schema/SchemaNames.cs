namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal sealed class SchemaNames
    {
        private XmlNameTable nameTable;
        public string NsDataType;
        public string NsDataTypeAlias;
        public string NsDataTypeOld;
        public string NsXdr;
        public string NsXdrAlias;
        public string NsXml;
        public string NsXmlNs;
        public string NsXs;
        public string NsXsi;
        public XmlQualifiedName QnAbstract;
        public XmlQualifiedName QnAttributeFormDefault;
        public XmlQualifiedName QnBase;
        public XmlQualifiedName QnBlock;
        public XmlQualifiedName QnBlockDefault;
        public XmlQualifiedName QnClosed;
        public XmlQualifiedName QnContent;
        public XmlQualifiedName QnDefault;
        public XmlQualifiedName QnDerivedBy;
        public XmlQualifiedName QnDtDt;
        public XmlQualifiedName QnDtMax;
        public XmlQualifiedName QnDtMaxExclusive;
        public XmlQualifiedName QnDtMaxLength;
        public XmlQualifiedName QnDtMin;
        public XmlQualifiedName QnDtMinExclusive;
        public XmlQualifiedName QnDtMinLength;
        public XmlQualifiedName QnDtType;
        public XmlQualifiedName QnDtValues;
        public XmlQualifiedName QnElementFormDefault;
        public XmlQualifiedName QnEltOnly;
        public XmlQualifiedName QnEmpty;
        public XmlQualifiedName QnEntities;
        public XmlQualifiedName QnEntity;
        public XmlQualifiedName QnEnumeration;
        public XmlQualifiedName QnFinal;
        public XmlQualifiedName QnFinalDefault;
        public XmlQualifiedName QnFixed;
        public XmlQualifiedName QnForm;
        public XmlQualifiedName QnID;
        public XmlQualifiedName QnIDRef;
        public XmlQualifiedName QnIDRefs;
        public XmlQualifiedName QnInfinite;
        public XmlQualifiedName QnItemType;
        public XmlQualifiedName QnMany;
        public XmlQualifiedName QnMaxOccurs;
        public XmlQualifiedName QnMemberTypes;
        public XmlQualifiedName QnMinOccurs;
        public XmlQualifiedName QnMixed;
        public XmlQualifiedName QnModel;
        public XmlQualifiedName QnName;
        public XmlQualifiedName QnNamespace;
        public XmlQualifiedName QnNillable;
        public XmlQualifiedName QnNmToken;
        public XmlQualifiedName QnNmTokens;
        public XmlQualifiedName QnNo;
        public XmlQualifiedName QnOne;
        public XmlQualifiedName QnOpen;
        public XmlQualifiedName QnOrder;
        public XmlQualifiedName QnPCData;
        public XmlQualifiedName QnProcessContents;
        public XmlQualifiedName QnPublic;
        public XmlQualifiedName QnRef;
        public XmlQualifiedName QnRefer;
        public XmlQualifiedName QnRequired;
        public XmlQualifiedName QnSchemaLocation;
        public XmlQualifiedName QnSeq;
        public XmlQualifiedName QnSource;
        public XmlQualifiedName QnString;
        public XmlQualifiedName QnSubstitutionGroup;
        public XmlQualifiedName QnSystem;
        public XmlQualifiedName QnTargetNamespace;
        public XmlQualifiedName QnTextOnly;
        public XmlQualifiedName QnType;
        public XmlQualifiedName QnUse;
        public XmlQualifiedName QnValue;
        public XmlQualifiedName QnVersion;
        public XmlQualifiedName QnXdrAliasSchema;
        public XmlQualifiedName QnXdrAttribute;
        public XmlQualifiedName QnXdrAttributeType;
        public XmlQualifiedName QnXdrDataType;
        public XmlQualifiedName QnXdrDescription;
        public XmlQualifiedName QnXdrElement;
        public XmlQualifiedName QnXdrElementType;
        public XmlQualifiedName QnXdrExtends;
        public XmlQualifiedName QnXdrGroup;
        public XmlQualifiedName QnXdrSchema;
        public XmlQualifiedName QnXml;
        public XmlQualifiedName QnXmlLang;
        public XmlQualifiedName QnXmlNs;
        public XmlQualifiedName QnXPath;
        public XmlQualifiedName QnXsdAll;
        public XmlQualifiedName QnXsdAnnotation;
        public XmlQualifiedName QnXsdAny;
        public XmlQualifiedName QnXsdAnyAttribute;
        public XmlQualifiedName QnXsdAnyType;
        public XmlQualifiedName QnXsdAppinfo;
        public XmlQualifiedName QnXsdAttribute;
        public XmlQualifiedName QnXsdAttributeGroup;
        public XmlQualifiedName QnXsdChoice;
        public XmlQualifiedName QnXsdComplexContent;
        public XmlQualifiedName QnXsdComplexType;
        public XmlQualifiedName QnXsdDocumentation;
        public XmlQualifiedName QnXsdElement;
        public XmlQualifiedName QnXsdEnumeration;
        public XmlQualifiedName QnXsdExtension;
        public XmlQualifiedName QnXsdField;
        public XmlQualifiedName QnXsdFractionDigits;
        public XmlQualifiedName QnXsdGroup;
        public XmlQualifiedName QnXsdImport;
        public XmlQualifiedName QnXsdInclude;
        public XmlQualifiedName QnXsdKey;
        public XmlQualifiedName QnXsdKeyRef;
        public XmlQualifiedName QnXsdLength;
        public XmlQualifiedName QnXsdList;
        public XmlQualifiedName QnXsdMaxExclusive;
        public XmlQualifiedName QnXsdMaxInclusive;
        public XmlQualifiedName QnXsdMaxLength;
        public XmlQualifiedName QnXsdMinExclusive;
        public XmlQualifiedName QnXsdMinInclusive;
        public XmlQualifiedName QnXsdMinLength;
        public XmlQualifiedName QnXsdNotation;
        public XmlQualifiedName QnXsdPattern;
        public XmlQualifiedName QnXsdRedefine;
        public XmlQualifiedName QnXsdRestriction;
        public XmlQualifiedName QnXsdSchema;
        public XmlQualifiedName QnXsdSelector;
        public XmlQualifiedName QnXsdSequence;
        public XmlQualifiedName QnXsdSimpleContent;
        public XmlQualifiedName QnXsdSimpleType;
        public XmlQualifiedName QnXsdTotalDigits;
        public XmlQualifiedName QnXsdUnion;
        public XmlQualifiedName QnXsdUnique;
        public XmlQualifiedName QnXsdWhiteSpace;
        public XmlQualifiedName QnYes;
        internal XmlQualifiedName[] TokenToQName = new XmlQualifiedName[0x7b];
        public string XdrSchema;
        public string XsdSchema;
        public string XsiNil;
        public string XsiNoNamespaceSchemaLocation;
        public string XsiSchemaLocation;
        public string XsiType;

        public SchemaNames(XmlNameTable nameTable)
        {
            this.nameTable = nameTable;
            this.NsDataType = nameTable.Add("urn:schemas-microsoft-com:datatypes");
            this.NsDataTypeAlias = nameTable.Add("uuid:C2F41010-65B3-11D1-A29F-00AA00C14882");
            this.NsDataTypeOld = nameTable.Add("urn:uuid:C2F41010-65B3-11D1-A29F-00AA00C14882/");
            this.NsXml = nameTable.Add("http://www.w3.org/XML/1998/namespace");
            this.NsXmlNs = nameTable.Add("http://www.w3.org/2000/xmlns/");
            this.NsXdr = nameTable.Add("urn:schemas-microsoft-com:xml-data");
            this.NsXdrAlias = nameTable.Add("uuid:BDC6E3F0-6DA3-11D1-A2A3-00AA00C14882");
            this.NsXs = nameTable.Add("http://www.w3.org/2001/XMLSchema");
            this.NsXsi = nameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
            this.XsiType = nameTable.Add("type");
            this.XsiNil = nameTable.Add("nil");
            this.XsiSchemaLocation = nameTable.Add("schemaLocation");
            this.XsiNoNamespaceSchemaLocation = nameTable.Add("noNamespaceSchemaLocation");
            this.XsdSchema = nameTable.Add("schema");
            this.XdrSchema = nameTable.Add("Schema");
            this.QnPCData = new XmlQualifiedName(nameTable.Add("#PCDATA"));
            this.QnXml = new XmlQualifiedName(nameTable.Add("xml"));
            this.QnXmlNs = new XmlQualifiedName(nameTable.Add("xmlns"), this.NsXmlNs);
            this.QnDtDt = new XmlQualifiedName(nameTable.Add("dt"), this.NsDataType);
            this.QnXmlLang = new XmlQualifiedName(nameTable.Add("lang"), this.NsXml);
            this.QnName = new XmlQualifiedName(nameTable.Add("name"));
            this.QnType = new XmlQualifiedName(nameTable.Add("type"));
            this.QnMaxOccurs = new XmlQualifiedName(nameTable.Add("maxOccurs"));
            this.QnMinOccurs = new XmlQualifiedName(nameTable.Add("minOccurs"));
            this.QnInfinite = new XmlQualifiedName(nameTable.Add("*"));
            this.QnModel = new XmlQualifiedName(nameTable.Add("model"));
            this.QnOpen = new XmlQualifiedName(nameTable.Add("open"));
            this.QnClosed = new XmlQualifiedName(nameTable.Add("closed"));
            this.QnContent = new XmlQualifiedName(nameTable.Add("content"));
            this.QnMixed = new XmlQualifiedName(nameTable.Add("mixed"));
            this.QnEmpty = new XmlQualifiedName(nameTable.Add("empty"));
            this.QnEltOnly = new XmlQualifiedName(nameTable.Add("eltOnly"));
            this.QnTextOnly = new XmlQualifiedName(nameTable.Add("textOnly"));
            this.QnOrder = new XmlQualifiedName(nameTable.Add("order"));
            this.QnSeq = new XmlQualifiedName(nameTable.Add("seq"));
            this.QnOne = new XmlQualifiedName(nameTable.Add("one"));
            this.QnMany = new XmlQualifiedName(nameTable.Add("many"));
            this.QnRequired = new XmlQualifiedName(nameTable.Add("required"));
            this.QnYes = new XmlQualifiedName(nameTable.Add("yes"));
            this.QnNo = new XmlQualifiedName(nameTable.Add("no"));
            this.QnString = new XmlQualifiedName(nameTable.Add("string"));
            this.QnID = new XmlQualifiedName(nameTable.Add("id"));
            this.QnIDRef = new XmlQualifiedName(nameTable.Add("idref"));
            this.QnIDRefs = new XmlQualifiedName(nameTable.Add("idrefs"));
            this.QnEntity = new XmlQualifiedName(nameTable.Add("entity"));
            this.QnEntities = new XmlQualifiedName(nameTable.Add("entities"));
            this.QnNmToken = new XmlQualifiedName(nameTable.Add("nmtoken"));
            this.QnNmTokens = new XmlQualifiedName(nameTable.Add("nmtokens"));
            this.QnEnumeration = new XmlQualifiedName(nameTable.Add("enumeration"));
            this.QnDefault = new XmlQualifiedName(nameTable.Add("default"));
            this.QnTargetNamespace = new XmlQualifiedName(nameTable.Add("targetNamespace"));
            this.QnVersion = new XmlQualifiedName(nameTable.Add("version"));
            this.QnFinalDefault = new XmlQualifiedName(nameTable.Add("finalDefault"));
            this.QnBlockDefault = new XmlQualifiedName(nameTable.Add("blockDefault"));
            this.QnFixed = new XmlQualifiedName(nameTable.Add("fixed"));
            this.QnAbstract = new XmlQualifiedName(nameTable.Add("abstract"));
            this.QnBlock = new XmlQualifiedName(nameTable.Add("block"));
            this.QnSubstitutionGroup = new XmlQualifiedName(nameTable.Add("substitutionGroup"));
            this.QnFinal = new XmlQualifiedName(nameTable.Add("final"));
            this.QnNillable = new XmlQualifiedName(nameTable.Add("nillable"));
            this.QnRef = new XmlQualifiedName(nameTable.Add("ref"));
            this.QnBase = new XmlQualifiedName(nameTable.Add("base"));
            this.QnDerivedBy = new XmlQualifiedName(nameTable.Add("derivedBy"));
            this.QnNamespace = new XmlQualifiedName(nameTable.Add("namespace"));
            this.QnProcessContents = new XmlQualifiedName(nameTable.Add("processContents"));
            this.QnRefer = new XmlQualifiedName(nameTable.Add("refer"));
            this.QnPublic = new XmlQualifiedName(nameTable.Add("public"));
            this.QnSystem = new XmlQualifiedName(nameTable.Add("system"));
            this.QnSchemaLocation = new XmlQualifiedName(nameTable.Add("schemaLocation"));
            this.QnValue = new XmlQualifiedName(nameTable.Add("value"));
            this.QnUse = new XmlQualifiedName(nameTable.Add("use"));
            this.QnForm = new XmlQualifiedName(nameTable.Add("form"));
            this.QnAttributeFormDefault = new XmlQualifiedName(nameTable.Add("attributeFormDefault"));
            this.QnElementFormDefault = new XmlQualifiedName(nameTable.Add("elementFormDefault"));
            this.QnSource = new XmlQualifiedName(nameTable.Add("source"));
            this.QnMemberTypes = new XmlQualifiedName(nameTable.Add("memberTypes"));
            this.QnItemType = new XmlQualifiedName(nameTable.Add("itemType"));
            this.QnXPath = new XmlQualifiedName(nameTable.Add("xpath"));
            this.QnXdrSchema = new XmlQualifiedName(this.XdrSchema, this.NsXdr);
            this.QnXdrElementType = new XmlQualifiedName(nameTable.Add("ElementType"), this.NsXdr);
            this.QnXdrElement = new XmlQualifiedName(nameTable.Add("element"), this.NsXdr);
            this.QnXdrGroup = new XmlQualifiedName(nameTable.Add("group"), this.NsXdr);
            this.QnXdrAttributeType = new XmlQualifiedName(nameTable.Add("AttributeType"), this.NsXdr);
            this.QnXdrAttribute = new XmlQualifiedName(nameTable.Add("attribute"), this.NsXdr);
            this.QnXdrDataType = new XmlQualifiedName(nameTable.Add("datatype"), this.NsXdr);
            this.QnXdrDescription = new XmlQualifiedName(nameTable.Add("description"), this.NsXdr);
            this.QnXdrExtends = new XmlQualifiedName(nameTable.Add("extends"), this.NsXdr);
            this.QnXdrAliasSchema = new XmlQualifiedName(nameTable.Add("Schema"), this.NsDataTypeAlias);
            this.QnDtType = new XmlQualifiedName(nameTable.Add("type"), this.NsDataType);
            this.QnDtValues = new XmlQualifiedName(nameTable.Add("values"), this.NsDataType);
            this.QnDtMaxLength = new XmlQualifiedName(nameTable.Add("maxLength"), this.NsDataType);
            this.QnDtMinLength = new XmlQualifiedName(nameTable.Add("minLength"), this.NsDataType);
            this.QnDtMax = new XmlQualifiedName(nameTable.Add("max"), this.NsDataType);
            this.QnDtMin = new XmlQualifiedName(nameTable.Add("min"), this.NsDataType);
            this.QnDtMinExclusive = new XmlQualifiedName(nameTable.Add("minExclusive"), this.NsDataType);
            this.QnDtMaxExclusive = new XmlQualifiedName(nameTable.Add("maxExclusive"), this.NsDataType);
            this.QnXsdSchema = new XmlQualifiedName(this.XsdSchema, this.NsXs);
            this.QnXsdAnnotation = new XmlQualifiedName(nameTable.Add("annotation"), this.NsXs);
            this.QnXsdInclude = new XmlQualifiedName(nameTable.Add("include"), this.NsXs);
            this.QnXsdImport = new XmlQualifiedName(nameTable.Add("import"), this.NsXs);
            this.QnXsdElement = new XmlQualifiedName(nameTable.Add("element"), this.NsXs);
            this.QnXsdAttribute = new XmlQualifiedName(nameTable.Add("attribute"), this.NsXs);
            this.QnXsdAttributeGroup = new XmlQualifiedName(nameTable.Add("attributeGroup"), this.NsXs);
            this.QnXsdAnyAttribute = new XmlQualifiedName(nameTable.Add("anyAttribute"), this.NsXs);
            this.QnXsdGroup = new XmlQualifiedName(nameTable.Add("group"), this.NsXs);
            this.QnXsdAll = new XmlQualifiedName(nameTable.Add("all"), this.NsXs);
            this.QnXsdChoice = new XmlQualifiedName(nameTable.Add("choice"), this.NsXs);
            this.QnXsdSequence = new XmlQualifiedName(nameTable.Add("sequence"), this.NsXs);
            this.QnXsdAny = new XmlQualifiedName(nameTable.Add("any"), this.NsXs);
            this.QnXsdNotation = new XmlQualifiedName(nameTable.Add("notation"), this.NsXs);
            this.QnXsdSimpleType = new XmlQualifiedName(nameTable.Add("simpleType"), this.NsXs);
            this.QnXsdComplexType = new XmlQualifiedName(nameTable.Add("complexType"), this.NsXs);
            this.QnXsdUnique = new XmlQualifiedName(nameTable.Add("unique"), this.NsXs);
            this.QnXsdKey = new XmlQualifiedName(nameTable.Add("key"), this.NsXs);
            this.QnXsdKeyRef = new XmlQualifiedName(nameTable.Add("keyref"), this.NsXs);
            this.QnXsdSelector = new XmlQualifiedName(nameTable.Add("selector"), this.NsXs);
            this.QnXsdField = new XmlQualifiedName(nameTable.Add("field"), this.NsXs);
            this.QnXsdMinExclusive = new XmlQualifiedName(nameTable.Add("minExclusive"), this.NsXs);
            this.QnXsdMinInclusive = new XmlQualifiedName(nameTable.Add("minInclusive"), this.NsXs);
            this.QnXsdMaxInclusive = new XmlQualifiedName(nameTable.Add("maxInclusive"), this.NsXs);
            this.QnXsdMaxExclusive = new XmlQualifiedName(nameTable.Add("maxExclusive"), this.NsXs);
            this.QnXsdTotalDigits = new XmlQualifiedName(nameTable.Add("totalDigits"), this.NsXs);
            this.QnXsdFractionDigits = new XmlQualifiedName(nameTable.Add("fractionDigits"), this.NsXs);
            this.QnXsdLength = new XmlQualifiedName(nameTable.Add("length"), this.NsXs);
            this.QnXsdMinLength = new XmlQualifiedName(nameTable.Add("minLength"), this.NsXs);
            this.QnXsdMaxLength = new XmlQualifiedName(nameTable.Add("maxLength"), this.NsXs);
            this.QnXsdEnumeration = new XmlQualifiedName(nameTable.Add("enumeration"), this.NsXs);
            this.QnXsdPattern = new XmlQualifiedName(nameTable.Add("pattern"), this.NsXs);
            this.QnXsdDocumentation = new XmlQualifiedName(nameTable.Add("documentation"), this.NsXs);
            this.QnXsdAppinfo = new XmlQualifiedName(nameTable.Add("appinfo"), this.NsXs);
            this.QnXsdComplexContent = new XmlQualifiedName(nameTable.Add("complexContent"), this.NsXs);
            this.QnXsdSimpleContent = new XmlQualifiedName(nameTable.Add("simpleContent"), this.NsXs);
            this.QnXsdRestriction = new XmlQualifiedName(nameTable.Add("restriction"), this.NsXs);
            this.QnXsdExtension = new XmlQualifiedName(nameTable.Add("extension"), this.NsXs);
            this.QnXsdUnion = new XmlQualifiedName(nameTable.Add("union"), this.NsXs);
            this.QnXsdList = new XmlQualifiedName(nameTable.Add("list"), this.NsXs);
            this.QnXsdWhiteSpace = new XmlQualifiedName(nameTable.Add("whiteSpace"), this.NsXs);
            this.QnXsdRedefine = new XmlQualifiedName(nameTable.Add("redefine"), this.NsXs);
            this.QnXsdAnyType = new XmlQualifiedName(nameTable.Add("anyType"), this.NsXs);
            this.CreateTokenToQNameTable();
        }

        public void CreateTokenToQNameTable()
        {
            this.TokenToQName[1] = this.QnName;
            this.TokenToQName[2] = this.QnType;
            this.TokenToQName[3] = this.QnMaxOccurs;
            this.TokenToQName[4] = this.QnMinOccurs;
            this.TokenToQName[5] = this.QnInfinite;
            this.TokenToQName[6] = this.QnModel;
            this.TokenToQName[7] = this.QnOpen;
            this.TokenToQName[8] = this.QnClosed;
            this.TokenToQName[9] = this.QnContent;
            this.TokenToQName[10] = this.QnMixed;
            this.TokenToQName[11] = this.QnEmpty;
            this.TokenToQName[12] = this.QnEltOnly;
            this.TokenToQName[13] = this.QnTextOnly;
            this.TokenToQName[14] = this.QnOrder;
            this.TokenToQName[15] = this.QnSeq;
            this.TokenToQName[0x10] = this.QnOne;
            this.TokenToQName[0x11] = this.QnMany;
            this.TokenToQName[0x12] = this.QnRequired;
            this.TokenToQName[0x13] = this.QnYes;
            this.TokenToQName[20] = this.QnNo;
            this.TokenToQName[0x15] = this.QnString;
            this.TokenToQName[0x16] = this.QnID;
            this.TokenToQName[0x17] = this.QnIDRef;
            this.TokenToQName[0x18] = this.QnIDRefs;
            this.TokenToQName[0x19] = this.QnEntity;
            this.TokenToQName[0x1a] = this.QnEntities;
            this.TokenToQName[0x1b] = this.QnNmToken;
            this.TokenToQName[0x1c] = this.QnNmTokens;
            this.TokenToQName[0x1d] = this.QnEnumeration;
            this.TokenToQName[30] = this.QnDefault;
            this.TokenToQName[0x1f] = this.QnXdrSchema;
            this.TokenToQName[0x20] = this.QnXdrElementType;
            this.TokenToQName[0x21] = this.QnXdrElement;
            this.TokenToQName[0x22] = this.QnXdrGroup;
            this.TokenToQName[0x23] = this.QnXdrAttributeType;
            this.TokenToQName[0x24] = this.QnXdrAttribute;
            this.TokenToQName[0x25] = this.QnXdrDataType;
            this.TokenToQName[0x26] = this.QnXdrDescription;
            this.TokenToQName[0x27] = this.QnXdrExtends;
            this.TokenToQName[40] = this.QnXdrAliasSchema;
            this.TokenToQName[0x29] = this.QnDtType;
            this.TokenToQName[0x2a] = this.QnDtValues;
            this.TokenToQName[0x2b] = this.QnDtMaxLength;
            this.TokenToQName[0x2c] = this.QnDtMinLength;
            this.TokenToQName[0x2d] = this.QnDtMax;
            this.TokenToQName[0x2e] = this.QnDtMin;
            this.TokenToQName[0x2f] = this.QnDtMinExclusive;
            this.TokenToQName[0x30] = this.QnDtMaxExclusive;
            this.TokenToQName[0x31] = this.QnTargetNamespace;
            this.TokenToQName[50] = this.QnVersion;
            this.TokenToQName[0x33] = this.QnFinalDefault;
            this.TokenToQName[0x34] = this.QnBlockDefault;
            this.TokenToQName[0x35] = this.QnFixed;
            this.TokenToQName[0x36] = this.QnAbstract;
            this.TokenToQName[0x37] = this.QnBlock;
            this.TokenToQName[0x38] = this.QnSubstitutionGroup;
            this.TokenToQName[0x39] = this.QnFinal;
            this.TokenToQName[0x3a] = this.QnNillable;
            this.TokenToQName[0x3b] = this.QnRef;
            this.TokenToQName[60] = this.QnBase;
            this.TokenToQName[0x3d] = this.QnDerivedBy;
            this.TokenToQName[0x3e] = this.QnNamespace;
            this.TokenToQName[0x3f] = this.QnProcessContents;
            this.TokenToQName[0x40] = this.QnRefer;
            this.TokenToQName[0x41] = this.QnPublic;
            this.TokenToQName[0x42] = this.QnSystem;
            this.TokenToQName[0x43] = this.QnSchemaLocation;
            this.TokenToQName[0x44] = this.QnValue;
            this.TokenToQName[0x77] = this.QnItemType;
            this.TokenToQName[120] = this.QnMemberTypes;
            this.TokenToQName[0x79] = this.QnXPath;
            this.TokenToQName[0x4a] = this.QnXsdSchema;
            this.TokenToQName[0x4b] = this.QnXsdAnnotation;
            this.TokenToQName[0x4c] = this.QnXsdInclude;
            this.TokenToQName[0x4d] = this.QnXsdImport;
            this.TokenToQName[0x4e] = this.QnXsdElement;
            this.TokenToQName[0x4f] = this.QnXsdAttribute;
            this.TokenToQName[80] = this.QnXsdAttributeGroup;
            this.TokenToQName[0x51] = this.QnXsdAnyAttribute;
            this.TokenToQName[0x52] = this.QnXsdGroup;
            this.TokenToQName[0x53] = this.QnXsdAll;
            this.TokenToQName[0x54] = this.QnXsdChoice;
            this.TokenToQName[0x55] = this.QnXsdSequence;
            this.TokenToQName[0x56] = this.QnXsdAny;
            this.TokenToQName[0x57] = this.QnXsdNotation;
            this.TokenToQName[0x58] = this.QnXsdSimpleType;
            this.TokenToQName[0x59] = this.QnXsdComplexType;
            this.TokenToQName[90] = this.QnXsdUnique;
            this.TokenToQName[0x5b] = this.QnXsdKey;
            this.TokenToQName[0x5c] = this.QnXsdKeyRef;
            this.TokenToQName[0x5d] = this.QnXsdSelector;
            this.TokenToQName[0x5e] = this.QnXsdField;
            this.TokenToQName[0x5f] = this.QnXsdMinExclusive;
            this.TokenToQName[0x60] = this.QnXsdMinInclusive;
            this.TokenToQName[0x61] = this.QnXsdMaxExclusive;
            this.TokenToQName[0x62] = this.QnXsdMaxInclusive;
            this.TokenToQName[0x63] = this.QnXsdTotalDigits;
            this.TokenToQName[100] = this.QnXsdFractionDigits;
            this.TokenToQName[0x65] = this.QnXsdLength;
            this.TokenToQName[0x66] = this.QnXsdMinLength;
            this.TokenToQName[0x67] = this.QnXsdMaxLength;
            this.TokenToQName[0x68] = this.QnXsdEnumeration;
            this.TokenToQName[0x69] = this.QnXsdPattern;
            this.TokenToQName[0x75] = this.QnXsdWhiteSpace;
            this.TokenToQName[0x6a] = this.QnXsdDocumentation;
            this.TokenToQName[0x6b] = this.QnXsdAppinfo;
            this.TokenToQName[0x6c] = this.QnXsdComplexContent;
            this.TokenToQName[110] = this.QnXsdRestriction;
            this.TokenToQName[0x71] = this.QnXsdRestriction;
            this.TokenToQName[0x73] = this.QnXsdRestriction;
            this.TokenToQName[0x6d] = this.QnXsdExtension;
            this.TokenToQName[0x70] = this.QnXsdExtension;
            this.TokenToQName[0x6f] = this.QnXsdSimpleContent;
            this.TokenToQName[0x74] = this.QnXsdUnion;
            this.TokenToQName[0x72] = this.QnXsdList;
            this.TokenToQName[0x76] = this.QnXsdRedefine;
            this.TokenToQName[0x45] = this.QnSource;
            this.TokenToQName[0x48] = this.QnUse;
            this.TokenToQName[0x49] = this.QnForm;
            this.TokenToQName[0x47] = this.QnElementFormDefault;
            this.TokenToQName[70] = this.QnAttributeFormDefault;
            this.TokenToQName[0x7a] = this.QnXmlLang;
            this.TokenToQName[0] = XmlQualifiedName.Empty;
        }

        public XmlQualifiedName GetName(Token token)
        {
            return this.TokenToQName[(int) token];
        }

        public bool IsXDRRoot(string localName, string ns)
        {
            return (Ref.Equal(ns, this.NsXdr) && Ref.Equal(localName, this.XdrSchema));
        }

        public bool IsXSDRoot(string localName, string ns)
        {
            return (Ref.Equal(ns, this.NsXs) && Ref.Equal(localName, this.XsdSchema));
        }

        public SchemaType SchemaTypeFromRoot(string localName, string ns)
        {
            if (this.IsXSDRoot(localName, ns))
            {
                return SchemaType.XSD;
            }
            if (this.IsXDRRoot(localName, XmlSchemaDatatype.XdrCanonizeUri(ns, this.nameTable, this)))
            {
                return SchemaType.XDR;
            }
            return SchemaType.None;
        }

        public XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        public enum Token
        {
            Empty,
            SchemaName,
            SchemaType,
            SchemaMaxOccurs,
            SchemaMinOccurs,
            SchemaInfinite,
            SchemaModel,
            SchemaOpen,
            SchemaClosed,
            SchemaContent,
            SchemaMixed,
            SchemaEmpty,
            SchemaElementOnly,
            SchemaTextOnly,
            SchemaOrder,
            SchemaSeq,
            SchemaOne,
            SchemaMany,
            SchemaRequired,
            SchemaYes,
            SchemaNo,
            SchemaString,
            SchemaId,
            SchemaIdref,
            SchemaIdrefs,
            SchemaEntity,
            SchemaEntities,
            SchemaNmtoken,
            SchemaNmtokens,
            SchemaEnumeration,
            SchemaDefault,
            XdrRoot,
            XdrElementType,
            XdrElement,
            XdrGroup,
            XdrAttributeType,
            XdrAttribute,
            XdrDatatype,
            XdrDescription,
            XdrExtends,
            SchemaXdrRootAlias,
            SchemaDtType,
            SchemaDtValues,
            SchemaDtMaxLength,
            SchemaDtMinLength,
            SchemaDtMax,
            SchemaDtMin,
            SchemaDtMinExclusive,
            SchemaDtMaxExclusive,
            SchemaTargetNamespace,
            SchemaVersion,
            SchemaFinalDefault,
            SchemaBlockDefault,
            SchemaFixed,
            SchemaAbstract,
            SchemaBlock,
            SchemaSubstitutionGroup,
            SchemaFinal,
            SchemaNillable,
            SchemaRef,
            SchemaBase,
            SchemaDerivedBy,
            SchemaNamespace,
            SchemaProcessContents,
            SchemaRefer,
            SchemaPublic,
            SchemaSystem,
            SchemaSchemaLocation,
            SchemaValue,
            SchemaSource,
            SchemaAttributeFormDefault,
            SchemaElementFormDefault,
            SchemaUse,
            SchemaForm,
            XsdSchema,
            XsdAnnotation,
            XsdInclude,
            XsdImport,
            XsdElement,
            XsdAttribute,
            xsdAttributeGroup,
            XsdAnyAttribute,
            XsdGroup,
            XsdAll,
            XsdChoice,
            XsdSequence,
            XsdAny,
            XsdNotation,
            XsdSimpleType,
            XsdComplexType,
            XsdUnique,
            XsdKey,
            XsdKeyref,
            XsdSelector,
            XsdField,
            XsdMinExclusive,
            XsdMinInclusive,
            XsdMaxExclusive,
            XsdMaxInclusive,
            XsdTotalDigits,
            XsdFractionDigits,
            XsdLength,
            XsdMinLength,
            XsdMaxLength,
            XsdEnumeration,
            XsdPattern,
            XsdDocumentation,
            XsdAppInfo,
            XsdComplexContent,
            XsdComplexContentExtension,
            XsdComplexContentRestriction,
            XsdSimpleContent,
            XsdSimpleContentExtension,
            XsdSimpleContentRestriction,
            XsdSimpleTypeList,
            XsdSimpleTypeRestriction,
            XsdSimpleTypeUnion,
            XsdWhitespace,
            XsdRedefine,
            SchemaItemType,
            SchemaMemberTypes,
            SchemaXPath,
            XmlLang
        }
    }
}

