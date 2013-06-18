namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Services;
    using System.Text;
    using System.Xml;

    internal class WsdlParser
    {
        private SchemaBlockType _blockDefault;
        internal bool _bWrappedProxy;
        private Stack _currentReaderStack = new Stack(5);
        private Stack _currentSchemaReaderStack = new Stack(5);
        private ArrayList _outCodeStreamList;
        private string _outputDir;
        private ReaderStream _parsingInput;
        private XmlNameTable _primedNametable;
        private string _proxyNamespace;
        private int _proxyNamespaceCount;
        private ReaderStream _readerStreamsWsdl;
        private ReaderStream _readerStreamsXsd;
        private ArrayList _URTNamespaces;
        private WriterStream _writerStreams;
        private XmlTextReader _XMLReader = null;
        private XsdVersion _xsdVersion;
        private static Hashtable cSharpKeywords;
        private static string s_addressesString;
        private static string s_addressString;
        private static string s_allString;
        private static string s_arrayString;
        private static string s_arrayTypeString;
        private static string s_attributesString;
        private static string s_attributeString;
        private static string s_baseString;
        private static string s_bindingString;
        private static string s_bodyString;
        private static string s_choiceString;
        private static string s_classString;
        private static string s_comObjectString;
        private static string s_complexContentString;
        private static string s_complexTypeString;
        private static string s_definitionsString;
        private static string s_delegateString;
        private static string s_elementString;
        private static string s_emptyString;
        private static string s_encodedString;
        private static string s_encodingString;
        private static string s_encodingStyleString;
        private static string s_enumerationString;
        private static string s_enumTypeString;
        private static string s_extendsString;
        private static string s_faultString;
        private static string s_headerString;
        private static string s_idString;
        private static string s_implementsString;
        private static string s_importString;
        private static string s_includeString;
        private static string s_inputString;
        private static string s_instanceNamespaceString;
        private static string s_instanceNamespaceString1999;
        private static string s_instanceNamespaceString2000;
        private static string s_interfaceString;
        private static string s_ISerializableString;
        private static string s_locationString;
        private static string s_marshalByRefString;
        private static string s_maxOccursString;
        private static string s_messageString;
        private static string s_methodString;
        private static string s_minOccursString;
        private static string s_namespaceString;
        private static string s_nameString;
        private static string s_nestedTypeString;
        private static string s_objectString;
        private static string s_oneString;
        private static string s_onewayString;
        private static string s_operationString;
        private static string s_outputString;
        private static string s_parameterOrderString;
        private static string s_partsString;
        private static string s_partString;
        private static string s_portString;
        private static string s_portTypeString;
        private static string s_referenceString;
        private static string s_refString;
        private static string s_refTypeString;
        private static string s_requestResponseString;
        private static string s_requestString;
        private static string s_responseString;
        private static string s_restrictionString;
        private static string s_rootTypeString;
        private static string s_schemaLocationString;
        private static string s_schemaNamespaceString;
        private static string s_schemaNamespaceString1999;
        private static string s_schemaNamespaceString2000;
        private static string s_schemaString;
        private static string s_sequenceString;
        private static string s_servicedComponentString;
        private static string s_serviceDescString;
        private static string s_serviceNamespaceString;
        private static string s_serviceString;
        private static string s_simpleTypeString;
        private static string s_soapActionString;
        private static string s_soapEncodingString;
        private static string s_soapNamespaceString;
        private static string s_soapString;
        private static string s_structString;
        private static string s_styleString;
        private static string s_sudsNamespaceString;
        private static string s_sudsString;
        private static string s_targetNamespaceString;
        private static string s_transportString;
        private static string s_typesString;
        private static string s_typeString;
        private static string s_unboundedString;
        private static string s_uriString;
        private static string s_urTypeString;
        private static string s_useString;
        private static string s_valueString;
        private static string s_wsdlNamespaceString;
        private static string s_wsdlSoapNamespaceString;
        private static string s_wsdlSudsNamespaceString;
        private static string s_zeroString;
        private static StringBuilder vsb = new StringBuilder();
        private ArrayList wsdlBindings = new ArrayList(10);
        private Hashtable wsdlMessages = new Hashtable(10);
        private Hashtable wsdlPortTypes = new Hashtable(10);
        private ArrayList wsdlServices = new ArrayList(10);

        internal WsdlParser(TextReader input, string outputDir, ArrayList outCodeStreamList, string locationURL, bool bWrappedProxy, string proxyNamespace)
        {
            this._readerStreamsWsdl = new ReaderStream(locationURL);
            this._readerStreamsWsdl.InputStream = input;
            this._writerStreams = null;
            this._outputDir = outputDir;
            this._outCodeStreamList = outCodeStreamList;
            this._bWrappedProxy = bWrappedProxy;
            if ((proxyNamespace == null) || (proxyNamespace.Length == 0))
            {
                this._proxyNamespace = "InteropNS";
            }
            else
            {
                this._proxyNamespace = proxyNamespace;
            }
            if (outputDir == null)
            {
                outputDir = ".";
            }
            int length = outputDir.Length;
            if (length > 0)
            {
                char ch = outputDir[length - 1];
                if ((ch != '\\') && (ch != '/'))
                {
                    this._outputDir = this._outputDir + '\\';
                }
            }
            this._URTNamespaces = new ArrayList();
            this._blockDefault = SchemaBlockType.ALL;
            this._primedNametable = CreatePrimedNametable();
        }

        internal void AddNamespace(URTNamespace xns)
        {
            this._URTNamespaces.Add(xns);
        }

        internal URTNamespace AddNewNamespace(string ns)
        {
            if (ns == null)
            {
                return null;
            }
            URTNamespace namespace2 = this.LookupNamespace(ns);
            if (namespace2 == null)
            {
                namespace2 = new URTNamespace(ns, this);
            }
            if (!namespace2.IsSystem)
            {
                namespace2.bReferenced = true;
            }
            return namespace2;
        }

        internal string Atomize(string str)
        {
            return this._XMLReader.NameTable.Add(str);
        }

        internal static void CheckValidIdentifier(string ident)
        {
            if (!IsValidLanguageIndependentIdentifier(ident))
            {
                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_WsdlInvalidStringSyntax"), new object[] { ident }));
            }
        }

        private static XmlNameTable CreatePrimedNametable()
        {
            NameTable table = new NameTable();
            s_emptyString = table.Add(string.Empty);
            s_complexTypeString = table.Add("complexType");
            s_simpleTypeString = table.Add("simpleType");
            s_elementString = table.Add("element");
            s_enumerationString = table.Add("enumeration");
            s_encodingString = table.Add("encoding");
            s_attributeString = table.Add("attribute");
            s_attributesString = table.Add("attributes");
            s_allString = table.Add("all");
            s_sequenceString = table.Add("sequence");
            s_choiceString = table.Add("choice");
            s_minOccursString = table.Add("minOccurs");
            s_maxOccursString = table.Add("maxOccurs");
            s_unboundedString = table.Add("unbounded");
            s_oneString = table.Add("1");
            s_zeroString = table.Add("0");
            s_nameString = table.Add("name");
            s_typeString = table.Add("type");
            s_baseString = table.Add("base");
            s_valueString = table.Add("value");
            s_interfaceString = table.Add("interface");
            s_serviceString = table.Add("service");
            s_extendsString = table.Add("extends");
            s_addressesString = table.Add("addresses");
            s_addressString = table.Add("address");
            s_uriString = table.Add("uri");
            s_implementsString = table.Add("implements");
            s_nestedTypeString = table.Add("nestedType");
            s_requestString = table.Add("request");
            s_responseString = table.Add("response");
            s_requestResponseString = table.Add("requestResponse");
            s_messageString = table.Add("message");
            s_locationString = table.Add("location");
            s_schemaLocationString = table.Add("schemaLocation");
            s_importString = table.Add("import");
            s_onewayString = table.Add("oneway");
            s_includeString = table.Add("include");
            s_refString = table.Add("ref");
            s_refTypeString = table.Add("refType");
            s_referenceString = table.Add("Reference");
            s_objectString = table.Add("Object");
            s_urTypeString = table.Add("anyType");
            s_arrayString = table.Add("Array");
            s_sudsString = table.Add("suds");
            s_methodString = table.Add("method");
            s_useString = table.Add("use");
            s_rootTypeString = table.Add("rootType");
            s_soapString = table.Add("soap");
            s_serviceDescString = table.Add("serviceDescription");
            s_schemaString = table.Add("schema");
            s_targetNamespaceString = table.Add("targetNamespace");
            s_namespaceString = table.Add("namespace");
            s_idString = table.Add("ID");
            s_soapActionString = table.Add("soapAction");
            s_schemaNamespaceString1999 = table.Add(SudsConverter.Xsd1999);
            s_instanceNamespaceString1999 = table.Add(SudsConverter.Xsi1999);
            s_schemaNamespaceString2000 = table.Add(SudsConverter.Xsd2000);
            s_instanceNamespaceString2000 = table.Add(SudsConverter.Xsi2000);
            s_schemaNamespaceString = table.Add(SudsConverter.Xsd2001);
            s_instanceNamespaceString = table.Add(SudsConverter.Xsi2001);
            s_soapNamespaceString = table.Add("urn:schemas-xmlsoap-org:soap.v1");
            s_sudsNamespaceString = table.Add("urn:schemas-xmlsoap-org:soap-sdl-2000-01-25");
            s_serviceNamespaceString = table.Add("urn:schemas-xmlsoap-org:sdl.2000-01-25");
            s_definitionsString = table.Add("definitions");
            s_wsdlNamespaceString = table.Add("http://schemas.xmlsoap.org/wsdl/");
            s_wsdlSoapNamespaceString = table.Add("http://schemas.xmlsoap.org/wsdl/soap/");
            s_wsdlSudsNamespaceString = table.Add("http://www.w3.org/2000/wsdl/suds");
            s_enumTypeString = table.Add("enumType");
            s_typesString = table.Add("types");
            s_partString = table.Add("part");
            s_portTypeString = table.Add("portType");
            s_operationString = table.Add("operation");
            s_inputString = table.Add("input");
            s_outputString = table.Add("output");
            s_bindingString = table.Add("binding");
            s_classString = table.Add("class");
            s_structString = table.Add("struct");
            s_ISerializableString = table.Add("ISerializable");
            s_marshalByRefString = table.Add("MarshalByRefObject");
            s_delegateString = table.Add("Delegate");
            s_servicedComponentString = table.Add("ServicedComponent");
            s_comObjectString = table.Add("__ComObject");
            s_portString = table.Add("port");
            s_styleString = table.Add("style");
            s_transportString = table.Add("transport");
            s_encodedString = table.Add("encoded");
            s_faultString = table.Add("fault");
            s_bodyString = table.Add("body");
            s_partsString = table.Add("parts");
            s_headerString = table.Add("header");
            s_encodingStyleString = table.Add("encodingStyle");
            s_restrictionString = table.Add("restriction");
            s_complexContentString = table.Add("complexContent");
            s_soapEncodingString = table.Add("http://schemas.xmlsoap.org/soap/encoding/");
            s_arrayTypeString = table.Add("arrayType");
            s_parameterOrderString = table.Add("parameterOrder");
            return table;
        }

        [Conditional("_LOGGING")]
        private void DumpWsdl()
        {
            foreach (DictionaryEntry entry in this.wsdlMessages)
            {
                ((IDump) entry.Value).Dump();
            }
            foreach (DictionaryEntry entry2 in this.wsdlPortTypes)
            {
                ((IDump) entry2.Value).Dump();
            }
            foreach (WsdlBinding binding in this.wsdlBindings)
            {
                binding.Dump();
            }
            foreach (WsdlService service in this.wsdlServices)
            {
                service.Dump();
            }
        }

        internal string GetTypeString(string curNS, bool bNS, URTNamespace urtNS, string typeName, string typeNS)
        {
            string str;
            URTComplexType complexType = urtNS.LookupComplexType(typeName);
            if ((complexType != null) && complexType.IsArray())
            {
                if (complexType.GetArray() == null)
                {
                    complexType.ResolveArray();
                }
                string array = complexType.GetArray();
                URTNamespace arrayNS = complexType.GetArrayNS();
                StringBuilder builder = new StringBuilder(50);
                if ((arrayNS.EncodedNS != null) && this.Qualify(urtNS.EncodedNS, arrayNS.EncodedNS))
                {
                    builder.Append(IsValidCSAttr(arrayNS.EncodedNS));
                    builder.Append('.');
                }
                builder.Append(IsValidCSAttr(array));
                str = builder.ToString();
            }
            else
            {
                string encodedNS = null;
                if (urtNS.UrtType == UrtType.Interop)
                {
                    encodedNS = urtNS.EncodedNS;
                }
                else
                {
                    encodedNS = typeNS;
                }
                if (bNS && this.Qualify(encodedNS, curNS))
                {
                    StringBuilder builder2 = new StringBuilder(50);
                    if (encodedNS != null)
                    {
                        builder2.Append(IsValidCSAttr(encodedNS));
                        builder2.Append('.');
                    }
                    builder2.Append(IsValidCSAttr(typeName));
                    str = builder2.ToString();
                }
                else
                {
                    str = typeName;
                }
            }
            int index = str.IndexOf('+');
            if (index <= 0)
            {
                return str;
            }
            if (bNS)
            {
                return str.Replace('+', '.');
            }
            return str.Substring(0, index);
        }

        private static void InitKeywords()
        {
            Hashtable hashtable = new Hashtable(0x4b);
            object obj2 = new object();
            hashtable["abstract"] = obj2;
            hashtable["base"] = obj2;
            hashtable["bool"] = obj2;
            hashtable["break"] = obj2;
            hashtable["byte"] = obj2;
            hashtable["case"] = obj2;
            hashtable["catch"] = obj2;
            hashtable["char"] = obj2;
            hashtable["checked"] = obj2;
            hashtable["class"] = obj2;
            hashtable["const"] = obj2;
            hashtable["continue"] = obj2;
            hashtable["decimal"] = obj2;
            hashtable["default"] = obj2;
            hashtable["delegate"] = obj2;
            hashtable["do"] = obj2;
            hashtable["double"] = obj2;
            hashtable["else"] = obj2;
            hashtable["enum"] = obj2;
            hashtable["event"] = obj2;
            hashtable["exdouble"] = obj2;
            hashtable["exfloat"] = obj2;
            hashtable["explicit"] = obj2;
            hashtable["extern"] = obj2;
            hashtable["false"] = obj2;
            hashtable["finally"] = obj2;
            hashtable["fixed"] = obj2;
            hashtable["float"] = obj2;
            hashtable["for"] = obj2;
            hashtable["foreach"] = obj2;
            hashtable["goto"] = obj2;
            hashtable["if"] = obj2;
            hashtable["implicit"] = obj2;
            hashtable["in"] = obj2;
            hashtable["int"] = obj2;
            hashtable["interface"] = obj2;
            hashtable["internal"] = obj2;
            hashtable["is"] = obj2;
            hashtable["lock"] = obj2;
            hashtable["long"] = obj2;
            hashtable["namespace"] = obj2;
            hashtable["new"] = obj2;
            hashtable["null"] = obj2;
            hashtable["object"] = obj2;
            hashtable["operator"] = obj2;
            hashtable["out"] = obj2;
            hashtable["override"] = obj2;
            hashtable["private"] = obj2;
            hashtable["protected"] = obj2;
            hashtable["public"] = obj2;
            hashtable["readonly"] = obj2;
            hashtable["ref"] = obj2;
            hashtable["return"] = obj2;
            hashtable["sbyte"] = obj2;
            hashtable["sealed"] = obj2;
            hashtable["short"] = obj2;
            hashtable["sizeof"] = obj2;
            hashtable["static"] = obj2;
            hashtable["string"] = obj2;
            hashtable["struct"] = obj2;
            hashtable["switch"] = obj2;
            hashtable["this"] = obj2;
            hashtable["throw"] = obj2;
            hashtable["true"] = obj2;
            hashtable["try"] = obj2;
            hashtable["typeof"] = obj2;
            hashtable["uint"] = obj2;
            hashtable["ulong"] = obj2;
            hashtable["unchecked"] = obj2;
            hashtable["unsafe"] = obj2;
            hashtable["ushort"] = obj2;
            hashtable["using"] = obj2;
            hashtable["virtual"] = obj2;
            hashtable["void"] = obj2;
            hashtable["while"] = obj2;
            cSharpKeywords = hashtable;
        }

        private static bool IsCSharpKeyword(string value)
        {
            if (cSharpKeywords == null)
            {
                InitKeywords();
            }
            return cSharpKeywords.ContainsKey(value);
        }

        private bool IsPrimitiveType(string typeNS, string typeName)
        {
            bool flag = false;
            if (this.MatchingSchemaStrings(typeNS) && !MatchingStrings(typeName, s_urTypeString))
            {
                flag = true;
            }
            return flag;
        }

        internal UrtType IsURTExportedType(string name, out string ns, out string assemName)
        {
            UrtType none = UrtType.None;
            ns = null;
            assemName = null;
            if (this.MatchingSchemaStrings(name))
            {
                return UrtType.Xsd;
            }
            if (SoapServices.IsClrTypeNamespace(name))
            {
                SoapServices.DecodeXmlNamespaceForClrTypeNamespace(name, out ns, out assemName);
                if (assemName == null)
                {
                    assemName = typeof(string).Assembly.GetName().Name;
                    none = UrtType.UrtSystem;
                }
                else
                {
                    none = UrtType.UrtUser;
                }
            }
            if (none == UrtType.None)
            {
                ns = name;
                assemName = ns;
                none = UrtType.Interop;
            }
            ns = this.Atomize(ns);
            assemName = this.Atomize(assemName);
            return none;
        }

        internal static string IsValidCS(string identifier)
        {
            if (((identifier == null) || (identifier.Length == 0)) || (identifier == " "))
            {
                return identifier;
            }
            string str = identifier;
            int index = identifier.IndexOf('[');
            string str2 = null;
            if (index > -1)
            {
                str2 = identifier.Substring(index);
                identifier = identifier.Substring(0, index);
                for (int j = 0; j < str2.Length; j++)
                {
                    switch (str2[j])
                    {
                        case '[':
                        case ']':
                        case ',':
                        case ' ':
                        {
                            continue;
                        }
                    }
                    throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_WsdlInvalidStringSyntax"), new object[] { identifier }));
                }
            }
            string[] strArray = identifier.Split(new char[] { '.' });
            bool flag = false;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(".");
                }
                if (IsCSharpKeyword(strArray[i]))
                {
                    builder.Append("@");
                    flag = true;
                }
                CheckValidIdentifier(strArray[i]);
                builder.Append(strArray[i]);
            }
            if (!flag)
            {
                return str;
            }
            if (str2 != null)
            {
                builder.Append(str2);
            }
            return builder.ToString();
        }

        internal static string IsValidCSAttr(string identifier)
        {
            string str = IsValidCS(identifier);
            if ((str.Length > 0) && (str[0] == '@'))
            {
                return str.Substring(1);
            }
            return str;
        }

        private static bool IsValidLanguageIndependentIdentifier(string ident)
        {
            for (int i = 0; i < ident.Length; i++)
            {
                char c = ident[i];
                switch (char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                        break;

                    case UnicodeCategory.EnclosingMark:
                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.OtherNumber:
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.LineSeparator:
                    case UnicodeCategory.ParagraphSeparator:
                    case UnicodeCategory.Control:
                    case UnicodeCategory.Format:
                    case UnicodeCategory.Surrogate:
                    case UnicodeCategory.PrivateUse:
                    case UnicodeCategory.DashPunctuation:
                    case UnicodeCategory.OpenPunctuation:
                    case UnicodeCategory.ClosePunctuation:
                    case UnicodeCategory.InitialQuotePunctuation:
                    case UnicodeCategory.FinalQuotePunctuation:
                    case UnicodeCategory.OtherPunctuation:
                    case UnicodeCategory.MathSymbol:
                    case UnicodeCategory.CurrencySymbol:
                    case UnicodeCategory.ModifierSymbol:
                    case UnicodeCategory.OtherSymbol:
                    case UnicodeCategory.OtherNotAssigned:
                        return false;

                    default:
                        return false;
                }
            }
            return true;
        }

        internal static string IsValidUrl(string value)
        {
            if (value == null)
            {
                return "\"\"";
            }
            vsb.Length = 0;
            vsb.Append("@\"");
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '"')
                {
                    vsb.Append("\"\"");
                }
                else
                {
                    vsb.Append(value[i]);
                }
            }
            vsb.Append("\"");
            return vsb.ToString();
        }

        private string LookupAttribute(string attrName, string attrNS, bool throwExp)
        {
            bool flag;
            string str = s_emptyString;
            if (attrNS != null)
            {
                flag = this._XMLReader.MoveToAttribute(attrName, attrNS);
            }
            else
            {
                flag = this._XMLReader.MoveToAttribute(attrName);
            }
            if (flag)
            {
                str = this.Atomize(this._XMLReader.Value.Trim());
            }
            this._XMLReader.MoveToElement();
            if (!flag && throwExp)
            {
                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_AttributeNotFound"), new object[] { attrName, this.XMLReader.LineNumber, this.XMLReader.LinePosition, this.XMLReader.Name }));
            }
            return str;
        }

        private URTNamespace LookupNamespace(string name)
        {
            for (int i = 0; i < this._URTNamespaces.Count; i++)
            {
                URTNamespace namespace2 = (URTNamespace) this._URTNamespaces[i];
                if (MatchingStrings(namespace2.Name, name))
                {
                    return namespace2;
                }
            }
            return null;
        }

        private string MapSchemaTypesToCSharpTypes(string xsdType)
        {
            string str = xsdType;
            int index = xsdType.IndexOf('[');
            if (index != -1)
            {
                str = xsdType.Substring(0, index);
            }
            string str2 = SudsConverter.MapXsdToClrTypes(str);
            if (str2 == null)
            {
                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveTypeInNS"), new object[] { xsdType, s_schemaNamespaceString }));
            }
            if (index != -1)
            {
                str2 = str2 + xsdType.Substring(index);
            }
            return str2;
        }

        private bool MatchingNamespace(string elmNS)
        {
            return MatchingStrings(this._XMLReader.NamespaceURI, elmNS);
        }

        private bool MatchingSchemaNamespace()
        {
            if (this.MatchingNamespace(s_schemaNamespaceString))
            {
                return true;
            }
            if (this.MatchingNamespace(s_schemaNamespaceString1999))
            {
                this._xsdVersion = XsdVersion.V1999;
                return true;
            }
            if (this.MatchingNamespace(s_schemaNamespaceString2000))
            {
                this._xsdVersion = XsdVersion.V2000;
                return true;
            }
            if (this.MatchingNamespace(s_schemaNamespaceString))
            {
                this._xsdVersion = XsdVersion.V2001;
                return true;
            }
            return false;
        }

        private bool MatchingSchemaStrings(string left)
        {
            if (MatchingStrings(left, s_schemaNamespaceString1999))
            {
                this._xsdVersion = XsdVersion.V1999;
                return true;
            }
            if (MatchingStrings(left, s_schemaNamespaceString2000))
            {
                this._xsdVersion = XsdVersion.V2000;
                return true;
            }
            if (MatchingStrings(left, s_schemaNamespaceString))
            {
                this._xsdVersion = XsdVersion.V2001;
                return true;
            }
            return false;
        }

        private static bool MatchingStrings(string left, string right)
        {
            return (left == right);
        }

        internal void Parse()
        {
            ReaderStream input = this._readerStreamsWsdl;
            do
            {
                this._XMLReader = new XmlTextReader(input.InputStream, this._primedNametable);
                this._XMLReader.WhitespaceHandling = WhitespaceHandling.None;
                this._XMLReader.XmlResolver = null;
                this.ParseInput(input);
                input = ReaderStream.GetNextReaderStream(input);
            }
            while (input != null);
            this.StartWsdlResolution();
            if (this._writerStreams != null)
            {
                WriterStream.Close(this._writerStreams);
            }
        }

        private void ParseAttributeField(URTNamespace parsingNamespace, URTComplexType parsingComplexType)
        {
            string nextAnonymousName;
            string fieldNamespace;
            bool flag2;
            bool primitiveField;
            string name = this.LookupAttribute(s_nameString, null, true);
            bool bOptional = false;
            if (MatchingStrings(this.LookupAttribute(s_minOccursString, null, false), s_zeroString))
            {
                bOptional = true;
            }
            if (this._XMLReader.IsEmptyElement)
            {
                nextAnonymousName = this.LookupAttribute(s_typeString, null, true);
                this.ResolveTypeAttribute(ref nextAnonymousName, out fieldNamespace, out flag2, out primitiveField);
                this.ReadNextXmlElement();
                if (MatchingStrings(nextAnonymousName, s_idString) && this.MatchingSchemaStrings(fieldNamespace))
                {
                    parsingComplexType.IsStruct = false;
                    return;
                }
            }
            else
            {
                fieldNamespace = parsingNamespace.Namespace;
                nextAnonymousName = parsingNamespace.GetNextAnonymousName();
                primitiveField = false;
                flag2 = true;
                int depth = this._XMLReader.Depth;
                this.ReadNextXmlElement();
                while (this._XMLReader.Depth > depth)
                {
                    if (MatchingStrings(this._XMLReader.LocalName, s_simpleTypeString))
                    {
                        URTSimpleType type = this.ParseSimpleType(parsingNamespace, nextAnonymousName);
                        if (type.IsEmittableFieldType)
                        {
                            fieldNamespace = type.FieldNamespace;
                            nextAnonymousName = type.FieldName;
                            primitiveField = type.PrimitiveField;
                            parsingNamespace.RemoveSimpleType(type);
                        }
                    }
                    else
                    {
                        this.SkipXmlElement();
                    }
                }
            }
            parsingComplexType.AddField(new URTField(name, nextAnonymousName, fieldNamespace, this, primitiveField, flag2, true, bOptional, false, null, parsingNamespace));
        }

        private URTComplexType ParseComplexType(URTNamespace parsingNamespace, string typeName)
        {
            if (typeName == null)
            {
                typeName = this.LookupAttribute(s_nameString, null, true);
            }
            URTNamespace returnNS = null;
            this.ParseQName(ref typeName, parsingNamespace, out returnNS);
            URTComplexType complexType = returnNS.LookupComplexType(typeName);
            if (complexType == null)
            {
                complexType = new URTComplexType(typeName, returnNS.Name, returnNS.Namespace, returnNS.EncodedNS, this._blockDefault, false, typeName != null, this, returnNS);
                returnNS.AddComplexType(complexType);
            }
            string left = this.LookupAttribute(s_baseString, null, false);
            if (!MatchingStrings(left, s_emptyString))
            {
                string baseTypeNS = this.ParseQName(ref left, parsingNamespace);
                complexType.Extends(left, baseTypeNS);
            }
            if (complexType.Fields.Count > 0)
            {
                this.SkipXmlElement();
                return complexType;
            }
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            int fieldNum = 0;
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (MatchingStrings(localName, s_elementString))
                {
                    this.ParseElementField(returnNS, complexType, fieldNum);
                    fieldNum++;
                }
                else
                {
                    if (MatchingStrings(localName, s_attributeString))
                    {
                        this.ParseAttributeField(returnNS, complexType);
                        continue;
                    }
                    if (MatchingStrings(localName, s_allString))
                    {
                        complexType.BlockType = SchemaBlockType.ALL;
                    }
                    else if (MatchingStrings(localName, s_sequenceString))
                    {
                        complexType.BlockType = SchemaBlockType.SEQUENCE;
                    }
                    else if (MatchingStrings(localName, s_choiceString))
                    {
                        complexType.BlockType = SchemaBlockType.CHOICE;
                    }
                    else if (MatchingStrings(localName, s_complexContentString))
                    {
                        complexType.BlockType = SchemaBlockType.ComplexContent;
                    }
                    else
                    {
                        if (MatchingStrings(localName, s_restrictionString))
                        {
                            this.ParseRestrictionField(returnNS, complexType);
                        }
                        else
                        {
                            this.SkipXmlElement();
                        }
                        continue;
                    }
                    this.ReadNextXmlElement();
                }
            }
            return complexType;
        }

        private void ParseElementDecl(URTNamespace parsingNamespace)
        {
            string str4;
            bool flag;
            bool flag2;
            string elmName = this.LookupAttribute(s_nameString, null, true);
            string name = parsingNamespace.Name;
            string typeName = this.LookupAttribute(s_typeString, null, false);
            if (this._XMLReader.IsEmptyElement)
            {
                this.ResolveTypeAttribute(ref typeName, out str4, out flag, out flag2);
                this.ReadNextXmlElement();
            }
            else
            {
                str4 = parsingNamespace.Name;
                typeName = parsingNamespace.GetNextAnonymousName();
                flag = true;
                flag2 = false;
                int depth = this._XMLReader.Depth;
                this.ReadNextXmlElement();
                while (this._XMLReader.Depth > depth)
                {
                    string localName = this._XMLReader.LocalName;
                    if (MatchingStrings(localName, s_complexTypeString))
                    {
                        this.ParseComplexType(parsingNamespace, typeName);
                    }
                    else
                    {
                        if (MatchingStrings(localName, s_simpleTypeString))
                        {
                            this.ParseSimpleType(parsingNamespace, typeName);
                            continue;
                        }
                        this.SkipXmlElement();
                    }
                }
            }
            parsingNamespace.AddElementDecl(new ElementDecl(elmName, name, typeName, str4, flag2));
        }

        private void ParseElementField(URTNamespace parsingNamespace, URTComplexType parsingComplexType, int fieldNum)
        {
            string nextAnonymousName;
            string fieldNamespace;
            bool flag3;
            bool primitiveField;
            string name = this.LookupAttribute(s_nameString, null, true);
            string left = this.LookupAttribute(s_minOccursString, null, false);
            string str5 = this.LookupAttribute(s_maxOccursString, null, false);
            bool bOptional = false;
            if (MatchingStrings(left, s_zeroString))
            {
                bOptional = true;
            }
            bool bArray = false;
            string arraySize = null;
            if (!MatchingStrings(str5, s_emptyString) && !MatchingStrings(str5, s_oneString))
            {
                if (MatchingStrings(str5, s_unboundedString))
                {
                    arraySize = string.Empty;
                }
                else
                {
                    arraySize = str5;
                }
                bArray = true;
            }
            if (this._XMLReader.IsEmptyElement)
            {
                nextAnonymousName = this.LookupAttribute(s_typeString, null, false);
                this.ResolveTypeAttribute(ref nextAnonymousName, out fieldNamespace, out flag3, out primitiveField);
                this.ReadNextXmlElement();
            }
            else
            {
                fieldNamespace = parsingNamespace.Namespace;
                nextAnonymousName = parsingNamespace.GetNextAnonymousName();
                primitiveField = false;
                flag3 = true;
                int depth = this._XMLReader.Depth;
                this.ReadNextXmlElement();
                while (this._XMLReader.Depth > depth)
                {
                    string localName = this._XMLReader.LocalName;
                    if (MatchingStrings(localName, s_complexTypeString))
                    {
                        URTComplexType type = this.ParseComplexType(parsingNamespace, nextAnonymousName);
                        if (type.IsEmittableFieldType)
                        {
                            fieldNamespace = type.FieldNamespace;
                            nextAnonymousName = type.FieldName;
                            primitiveField = type.PrimitiveField;
                            parsingNamespace.RemoveComplexType(type);
                        }
                    }
                    else
                    {
                        if (MatchingStrings(localName, s_simpleTypeString))
                        {
                            URTSimpleType type2 = this.ParseSimpleType(parsingNamespace, nextAnonymousName);
                            if (type2.IsEmittableFieldType)
                            {
                                fieldNamespace = type2.FieldNamespace;
                                nextAnonymousName = type2.FieldName;
                                primitiveField = type2.PrimitiveField;
                                parsingNamespace.RemoveSimpleType(type2);
                            }
                            continue;
                        }
                        this.SkipXmlElement();
                    }
                }
            }
            parsingComplexType.AddField(new URTField(name, nextAnonymousName, fieldNamespace, this, primitiveField, flag3, false, bOptional, bArray, arraySize, parsingNamespace));
        }

        private void ParseEnumeration(URTSimpleType parsingSimpleType, int enumFacetNum)
        {
            if (!this._XMLReader.IsEmptyElement)
            {
                throw new SUDSParserException(CoreChannel.GetResourceString("Remoting_Suds_EnumMustBeEmpty"));
            }
            string valueString = this.LookupAttribute(s_valueString, null, true);
            parsingSimpleType.IsEnum = true;
            parsingSimpleType.AddFacet(new EnumFacet(valueString, enumFacetNum));
        }

        private void ParseImport()
        {
            this.LookupAttribute(s_namespaceString, null, true);
            string location = null;
            location = this.LookupAttribute(s_locationString, null, false);
            if ((location != null) && (location.Length > 0))
            {
                ReaderStream reader = new ReaderStream(location);
                this.ParseReaderStreamLocation(reader, (ReaderStream) this._currentReaderStack.Peek());
                ReaderStream.GetReaderStream(this._readerStreamsWsdl, reader);
            }
            this.ReadNextXmlElement();
        }

        private void ParseImportedSchema(ReaderStream input)
        {
            try
            {
                string localName = this._XMLReader.LocalName;
                this._currentSchemaReaderStack.Push(input);
                this.ReadNextXmlElement();
                this.ParseSchema();
                this._currentSchemaReaderStack.Pop();
            }
            finally
            {
                WriterStream.Flush(this._writerStreams);
            }
        }

        internal void ParseImportedSchemaController()
        {
            CreatePrimedNametable();
            ReaderStream input = this._readerStreamsXsd;
            XmlTextReader reader = this._XMLReader;
            ReaderStream stream2 = this._parsingInput;
            do
            {
                this._XMLReader = new XmlTextReader(input.InputStream, this._primedNametable);
                this._XMLReader.WhitespaceHandling = WhitespaceHandling.None;
                this._XMLReader.XmlResolver = null;
                this._parsingInput = input;
                this.ParseImportedSchema(input);
                input = ReaderStream.GetNextReaderStream(input);
            }
            while (input != null);
            this._readerStreamsXsd = null;
            this._XMLReader = reader;
            this._parsingInput = stream2;
        }

        private void ParseInput(ReaderStream input)
        {
            this._parsingInput = input;
            try
            {
                this.ReadNextXmlElement();
                string localName = this._XMLReader.LocalName;
                if (!this.MatchingNamespace(s_wsdlNamespaceString) || !MatchingStrings(localName, s_definitionsString))
                {
                    if (!this.MatchingNamespace(s_wsdlNamespaceString) || !MatchingStrings(localName, s_typesString))
                    {
                        if (!this.MatchingSchemaNamespace() || !MatchingStrings(localName, s_schemaString))
                        {
                            throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_UnknownElementAtRootLevel"), new object[] { localName }));
                        }
                        this._currentReaderStack.Push(input);
                        this.ParseSchema();
                        this._currentReaderStack.Pop();
                    }
                    else
                    {
                        this._currentReaderStack.Push(input);
                        this.ParseWsdlTypes();
                        this._currentReaderStack.Pop();
                    }
                }
                else
                {
                    this._currentReaderStack.Push(input);
                    this.ParseWsdl();
                    this._currentReaderStack.Pop();
                }
            }
            finally
            {
                WriterStream.Flush(this._writerStreams);
            }
        }

        private URTNamespace ParseNamespace()
        {
            string left = this.LookupAttribute(s_targetNamespaceString, null, false);
            bool flag = false;
            if ((MatchingStrings(left, s_emptyString) && MatchingStrings(this._XMLReader.LocalName, s_sudsString)) && (this._parsingInput.UniqueNS == null))
            {
                left = this._parsingInput.TargetNS;
                flag = true;
            }
            URTNamespace namespace2 = this.LookupNamespace(left);
            if (namespace2 == null)
            {
                namespace2 = new URTNamespace(left, this);
            }
            if (flag)
            {
                this._parsingInput.UniqueNS = namespace2;
            }
            this.ReadNextXmlElement();
            return namespace2;
        }

        private string ParseQName(ref string qname)
        {
            return this.ParseQName(ref qname, null);
        }

        private string ParseQName(ref string qname, URTNamespace defaultNS)
        {
            URTNamespace returnNS = null;
            return this.ParseQName(ref qname, defaultNS, out returnNS);
        }

        private string ParseQName(ref string qname, URTNamespace defaultNS, out URTNamespace returnNS)
        {
            string name = null;
            returnNS = null;
            if ((qname == null) || (qname.Length == 0))
            {
                return null;
            }
            int index = qname.IndexOf(":");
            if (index == -1)
            {
                returnNS = defaultNS;
                if (defaultNS == null)
                {
                    name = this._XMLReader.LookupNamespace("");
                }
                else
                {
                    name = defaultNS.Name;
                }
            }
            else
            {
                string prefix = qname.Substring(0, index);
                qname = this.Atomize(qname.Substring(index + 1));
                name = this._XMLReader.LookupNamespace(prefix);
            }
            name = this.Atomize(name);
            URTNamespace namespace2 = this.LookupNamespace(name);
            if (namespace2 == null)
            {
                namespace2 = new URTNamespace(name, this);
            }
            returnNS = namespace2;
            return name;
        }

        private void ParseReaderStreamLocation(ReaderStream reader, ReaderStream currentReaderStream)
        {
            string location = reader.Location;
            int index = location.IndexOf(':');
            if (index == -1)
            {
                if ((currentReaderStream == null) || (currentReaderStream.Location == null))
                {
                    throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_Import"), new object[] { reader.Location }));
                }
                if (currentReaderStream.Uri == null)
                {
                    currentReaderStream.Uri = new Uri(currentReaderStream.Location);
                }
                Uri uri = new Uri(currentReaderStream.Uri, location);
                reader.Uri = uri;
                location = uri.ToString();
                index = location.IndexOf(':');
                if (index == -1)
                {
                    return;
                }
                reader.Location = location;
            }
            string str2 = location.Substring(0, index).ToLower(CultureInfo.InvariantCulture);
            string path = location.Substring(index + 1);
            if (str2 == "file")
            {
                reader.InputStream = new StreamReader(path);
            }
            else if (str2.StartsWith("http", StringComparison.Ordinal))
            {
                Stream responseStream = WebRequest.Create(location).GetResponse().GetResponseStream();
                reader.InputStream = new StreamReader(responseStream);
            }
        }

        private void ParseRestrictionField(URTNamespace parsingNamespace, BaseType parsingType)
        {
            string qname = this.LookupAttribute(s_baseString, null, true);
            this.ParseQName(ref qname, parsingNamespace);
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            int enumFacetNum = 0;
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (MatchingStrings(localName, s_attributeString))
                {
                    string str4 = this.LookupAttribute(s_refString, null, true);
                    if (MatchingStrings(this.ParseQName(ref str4, parsingNamespace), s_soapEncodingString) && MatchingStrings(str4, s_arrayTypeString))
                    {
                        URTComplexType type = (URTComplexType) parsingType;
                        string str3 = this.LookupAttribute(s_arrayTypeString, s_wsdlNamespaceString, true);
                        URTNamespace returnNS = null;
                        this.ParseQName(ref str3, null, out returnNS);
                        type.AddArray(str3, returnNS);
                        returnNS.AddComplexType(type);
                        type.IsPrint = false;
                    }
                }
                else if (MatchingStrings(localName, s_enumerationString))
                {
                    URTSimpleType parsingSimpleType = (URTSimpleType) parsingType;
                    this.ParseEnumeration(parsingSimpleType, enumFacetNum);
                    enumFacetNum++;
                }
                else
                {
                    this.SkipXmlElement();
                }
                this.ReadNextXmlElement();
            }
        }

        private void ParseSchema()
        {
            int depth = this._XMLReader.Depth;
            URTNamespace parsingNamespace = this.ParseNamespace();
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (MatchingStrings(localName, s_complexTypeString))
                {
                    this.ParseComplexType(parsingNamespace, null);
                }
                else
                {
                    if (MatchingStrings(localName, s_simpleTypeString))
                    {
                        this.ParseSimpleType(parsingNamespace, null);
                        continue;
                    }
                    if (MatchingStrings(localName, s_schemaString))
                    {
                        this.ParseSchema();
                        continue;
                    }
                    if (MatchingStrings(localName, s_elementString))
                    {
                        this.ParseElementDecl(parsingNamespace);
                        continue;
                    }
                    if (MatchingStrings(localName, s_importString))
                    {
                        this.ParseSchemaImportElement();
                        continue;
                    }
                    if (MatchingStrings(localName, s_includeString))
                    {
                        this.ParseSchemaIncludeElement();
                        continue;
                    }
                    this.SkipXmlElement();
                }
            }
        }

        private void ParseSchemaImportElement()
        {
            this.ParseSchemaImportElement(true);
        }

        private void ParseSchemaImportElement(bool bImport)
        {
            if (bImport)
            {
                this.LookupAttribute(s_namespaceString, null, true);
            }
            string location = null;
            location = this.LookupAttribute(s_schemaLocationString, null, false);
            if ((location != null) && (location.Length > 0))
            {
                if (this._readerStreamsXsd == null)
                {
                    this._readerStreamsXsd = new ReaderStream(location);
                    this.ParseReaderStreamLocation(this._readerStreamsXsd, (ReaderStream) this._currentSchemaReaderStack.Peek());
                }
                else
                {
                    ReaderStream reader = new ReaderStream(location);
                    this.ParseReaderStreamLocation(reader, (ReaderStream) this._currentSchemaReaderStack.Peek());
                    ReaderStream.GetReaderStream(this._readerStreamsWsdl, reader);
                }
            }
            this.ReadNextXmlElement();
        }

        private void ParseSchemaIncludeElement()
        {
            this.ParseSchemaImportElement(false);
        }

        private URTSimpleType ParseSimpleType(URTNamespace parsingNamespace, string typeName)
        {
            if (typeName == null)
            {
                typeName = this.LookupAttribute(s_nameString, null, true);
            }
            string str = this.LookupAttribute(s_enumTypeString, s_wsdlSudsNamespaceString, false);
            URTSimpleType simpleType = parsingNamespace.LookupSimpleType(typeName);
            if (simpleType == null)
            {
                simpleType = new URTSimpleType(typeName, parsingNamespace.Name, parsingNamespace.Namespace, parsingNamespace.EncodedNS, typeName != null, this);
                string left = this.LookupAttribute(s_baseString, null, false);
                if (!MatchingStrings(left, s_emptyString))
                {
                    string baseTypeNS = this.ParseQName(ref left, parsingNamespace);
                    simpleType.Extends(left, baseTypeNS);
                }
                parsingNamespace.AddSimpleType(simpleType);
                int depth = this._XMLReader.Depth;
                this.ReadNextXmlElement();
                while (this._XMLReader.Depth > depth)
                {
                    if (MatchingStrings(this._XMLReader.LocalName, s_restrictionString))
                    {
                        this.ParseRestrictionField(parsingNamespace, simpleType);
                    }
                    else
                    {
                        this.SkipXmlElement();
                    }
                }
            }
            else
            {
                this.SkipXmlElement();
            }
            if (str != null)
            {
                simpleType.EnumType = str;
            }
            return simpleType;
        }

        private void ParseWsdl()
        {
            int depth = this._XMLReader.Depth;
            this._parsingInput.Name = this.LookupAttribute(s_nameString, null, false);
            this._parsingInput.TargetNS = this.LookupAttribute(s_targetNamespaceString, null, false);
            URTNamespace inparsingNamespace = this.ParseNamespace();
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (this.MatchingNamespace(s_wsdlNamespaceString))
                {
                    if (MatchingStrings(localName, s_typesString))
                    {
                        this.ParseWsdlTypes();
                        continue;
                    }
                    if (MatchingStrings(localName, s_messageString))
                    {
                        this.ParseWsdlMessage();
                        continue;
                    }
                    if (MatchingStrings(localName, s_portTypeString))
                    {
                        this.ParseWsdlPortType();
                        continue;
                    }
                    if (MatchingStrings(localName, s_bindingString))
                    {
                        this.ParseWsdlBinding(inparsingNamespace);
                        continue;
                    }
                    if (MatchingStrings(localName, s_serviceString))
                    {
                        this.ParseWsdlService();
                        continue;
                    }
                    if (MatchingStrings(localName, s_importString))
                    {
                        this.ParseImport();
                        continue;
                    }
                }
                this.SkipXmlElement();
            }
        }

        private void ParseWsdlBinding(URTNamespace inparsingNamespace)
        {
            WsdlBinding binding;
            binding = new WsdlBinding {
                name = this.LookupAttribute(s_nameString, null, true),
                type = this.LookupAttribute(s_typeString, null, true),
                typeNs = this.ParseQName(ref binding.type)
            };
            URTNamespace namespace2 = this.LookupNamespace(binding.typeNs);
            if (namespace2 == null)
            {
                namespace2 = new URTNamespace(binding.typeNs, this);
            }
            binding.parsingNamespace = namespace2;
            bool flag = false;
            bool bRpcBinding = false;
            bool bSoapEncoded = false;
            bool flag4 = false;
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (this.MatchingNamespace(s_wsdlSoapNamespaceString) && MatchingStrings(localName, s_bindingString))
                {
                    flag = true;
                    WsdlBindingSoapBinding binding2 = new WsdlBindingSoapBinding {
                        style = this.LookupAttribute(s_styleString, null, true)
                    };
                    if (binding2.style == "rpc")
                    {
                        bRpcBinding = true;
                    }
                    binding2.transport = this.LookupAttribute(s_transportString, null, true);
                    binding.soapBinding = binding2;
                    this.ReadNextXmlElement();
                    continue;
                }
                if (this.MatchingNamespace(s_wsdlSudsNamespaceString))
                {
                    flag4 = true;
                    if (MatchingStrings(localName, s_classString) || MatchingStrings(localName, s_structString))
                    {
                        WsdlBindingSuds suds;
                        suds = new WsdlBindingSuds {
                            elementName = localName,
                            typeName = this.LookupAttribute(s_typeString, null, true),
                            ns = this.ParseQName(ref suds.typeName),
                            extendsTypeName = this.LookupAttribute(s_extendsString, null, false)
                        };
                        string use = this.LookupAttribute(s_rootTypeString, null, false);
                        suds.sudsUse = this.ProcessSudsUse(use, localName);
                        if (!MatchingStrings(suds.extendsTypeName, s_emptyString))
                        {
                            suds.extendsNs = this.ParseQName(ref suds.extendsTypeName);
                        }
                        this.ParseWsdlBindingSuds(suds);
                        binding.suds.Add(suds);
                    }
                    else
                    {
                        WsdlBindingSuds suds2;
                        if (!MatchingStrings(localName, s_interfaceString))
                        {
                            goto Label_02CC;
                        }
                        suds2 = new WsdlBindingSuds {
                            elementName = localName,
                            typeName = this.LookupAttribute(s_typeString, null, true),
                            ns = this.ParseQName(ref suds2.typeName)
                        };
                        string str3 = this.LookupAttribute(s_rootTypeString, null, false);
                        suds2.sudsUse = this.ProcessSudsUse(str3, localName);
                        this.ParseWsdlBindingSuds(suds2);
                        binding.suds.Add(suds2);
                    }
                    continue;
                }
                if (this.MatchingNamespace(s_wsdlNamespaceString) && MatchingStrings(localName, s_operationString))
                {
                    WsdlBindingOperation op = new WsdlBindingOperation {
                        name = this.LookupAttribute(s_nameString, null, true),
                        nameNs = this._parsingInput.TargetNS
                    };
                    this.ParseWsdlBindingOperation(op, ref bRpcBinding, ref bSoapEncoded);
                    binding.operations.Add(op);
                    continue;
                }
            Label_02CC:
                this.SkipXmlElement();
            }
            if (((flag && bRpcBinding) && bSoapEncoded) || flag4)
            {
                this.wsdlBindings.Add(binding);
            }
        }

        private void ParseWsdlBindingOperation(WsdlBindingOperation op, ref bool bRpcBinding, ref bool bSoapEncoded)
        {
            int depth = this._XMLReader.Depth;
            bool flag = false;
            bool flag2 = false;
            WsdlBindingOperationSection section = null;
            this.ReadNextXmlElement();
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (this.MatchingNamespace(s_wsdlSudsNamespaceString) && MatchingStrings(localName, s_methodString))
                {
                    op.methodAttributes = this.LookupAttribute(s_attributesString, null, true);
                    this.ReadNextXmlElement();
                }
                else
                {
                    if (this.MatchingNamespace(s_wsdlSoapNamespaceString) && MatchingStrings(localName, s_operationString))
                    {
                        WsdlBindingSoapOperation operation = new WsdlBindingSoapOperation {
                            soapAction = this.LookupAttribute(s_soapActionString, null, false),
                            style = this.LookupAttribute(s_styleString, null, false)
                        };
                        if (operation.style == "rpc")
                        {
                            bRpcBinding = true;
                        }
                        op.soapOperation = operation;
                        this.ReadNextXmlElement();
                        continue;
                    }
                    if (this.MatchingNamespace(s_wsdlNamespaceString))
                    {
                        if (MatchingStrings(localName, s_inputString))
                        {
                            flag = true;
                            section = this.ParseWsdlBindingOperationSection(op, localName, ref bSoapEncoded);
                            continue;
                        }
                        if (MatchingStrings(localName, s_outputString))
                        {
                            flag2 = true;
                            this.ParseWsdlBindingOperationSection(op, localName, ref bSoapEncoded);
                            continue;
                        }
                        if (MatchingStrings(localName, s_faultString))
                        {
                            this.ParseWsdlBindingOperationSection(op, localName, ref bSoapEncoded);
                            continue;
                        }
                    }
                    this.SkipXmlElement();
                }
            }
            if (((section != null) && flag) && !flag2)
            {
                section.name = op.name;
            }
        }

        private WsdlBindingOperationSection ParseWsdlBindingOperationSection(WsdlBindingOperation op, string inputElementName, ref bool bSoapEncoded)
        {
            bool flag = false;
            WsdlBindingOperationSection section = new WsdlBindingOperationSection();
            op.sections.Add(section);
            section.name = this.LookupAttribute(s_nameString, null, false);
            if (MatchingStrings(section.name, s_emptyString))
            {
                if (MatchingStrings(inputElementName, s_inputString))
                {
                    flag = true;
                    section.name = this.Atomize(op.name + "Request");
                }
                else if (MatchingStrings(inputElementName, s_outputString))
                {
                    section.name = this.Atomize(op.name + "Response");
                }
            }
            section.elementName = inputElementName;
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (this.MatchingNamespace(s_wsdlSoapNamespaceString))
                {
                    if (MatchingStrings(localName, s_bodyString))
                    {
                        WsdlBindingSoapBody body = new WsdlBindingSoapBody();
                        section.extensions.Add(body);
                        body.parts = this.LookupAttribute(s_partsString, null, false);
                        body.use = this.LookupAttribute(s_useString, null, true);
                        if (body.use == "encoded")
                        {
                            bSoapEncoded = true;
                        }
                        body.encodingStyle = this.LookupAttribute(s_encodingStyleString, null, false);
                        body.namespaceUri = this.LookupAttribute(s_namespaceString, null, false);
                        this.ReadNextXmlElement();
                        continue;
                    }
                    if (MatchingStrings(localName, s_headerString))
                    {
                        WsdlBindingSoapHeader header = new WsdlBindingSoapHeader();
                        section.extensions.Add(header);
                        header.message = this.LookupAttribute(s_messageString, null, true);
                        header.messageNs = this.ParseQName(ref header.message);
                        header.part = this.LookupAttribute(s_partString, null, true);
                        header.use = this.LookupAttribute(s_useString, null, true);
                        header.encodingStyle = this.LookupAttribute(s_encodingStyleString, null, false);
                        header.namespaceUri = this.LookupAttribute(s_namespaceString, null, false);
                        this.ReadNextXmlElement();
                        continue;
                    }
                    if (MatchingStrings(localName, s_faultString))
                    {
                        WsdlBindingSoapFault fault = new WsdlBindingSoapFault();
                        section.extensions.Add(fault);
                        fault.name = this.LookupAttribute(s_nameString, null, true);
                        fault.use = this.LookupAttribute(s_useString, null, true);
                        fault.encodingStyle = this.LookupAttribute(s_encodingStyleString, null, false);
                        fault.namespaceUri = this.LookupAttribute(s_namespaceString, null, false);
                        this.ReadNextXmlElement();
                        continue;
                    }
                }
                this.SkipXmlElement();
            }
            if (flag)
            {
                return section;
            }
            return null;
        }

        private void ParseWsdlBindingSuds(WsdlBindingSuds suds)
        {
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (MatchingStrings(localName, s_implementsString) || MatchingStrings(localName, s_extendsString))
                {
                    WsdlBindingSudsImplements implements;
                    implements = new WsdlBindingSudsImplements {
                        typeName = this.LookupAttribute(s_typeString, null, true),
                        ns = this.ParseQName(ref implements.typeName)
                    };
                    suds.implements.Add(implements);
                    this.ReadNextXmlElement();
                }
                else if (MatchingStrings(localName, s_nestedTypeString))
                {
                    WsdlBindingSudsNestedType type;
                    type = new WsdlBindingSudsNestedType {
                        name = this.LookupAttribute(s_nameString, null, true),
                        typeName = this.LookupAttribute(s_typeString, null, true),
                        ns = this.ParseQName(ref type.typeName)
                    };
                    suds.nestedTypes.Add(type);
                    this.ReadNextXmlElement();
                }
                else
                {
                    this.SkipXmlElement();
                }
            }
        }

        private void ParseWsdlMessage()
        {
            WsdlMessage message = new WsdlMessage {
                name = this.LookupAttribute(s_nameString, null, true),
                nameNs = this._parsingInput.TargetNS
            };
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            while (this._XMLReader.Depth > depth)
            {
                if (MatchingStrings(this._XMLReader.LocalName, s_partString))
                {
                    WsdlMessagePart part = new WsdlMessagePart {
                        name = this.LookupAttribute(s_nameString, null, true),
                        nameNs = this._parsingInput.TargetNS,
                        element = this.LookupAttribute(s_elementString, null, false),
                        typeName = this.LookupAttribute(s_typeString, null, false)
                    };
                    if (part.element != null)
                    {
                        part.elementNs = this.ParseQName(ref part.element);
                    }
                    if (part.typeName != null)
                    {
                        part.typeNameNs = this.ParseQName(ref part.typeName);
                    }
                    message.parts.Add(part);
                    this.ReadNextXmlElement();
                }
                else
                {
                    this.SkipXmlElement();
                }
            }
            this.wsdlMessages[message.name] = message;
        }

        private void ParseWsdlPortType()
        {
            WsdlPortType portType = new WsdlPortType {
                name = this.LookupAttribute(s_nameString, null, true)
            };
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            while (this._XMLReader.Depth > depth)
            {
                if (MatchingStrings(this._XMLReader.LocalName, s_operationString))
                {
                    WsdlPortTypeOperation operation;
                    operation = new WsdlPortTypeOperation {
                        name = this.LookupAttribute(s_nameString, null, true),
                        nameNs = this.ParseQName(ref operation.nameNs),
                        parameterOrder = this.LookupAttribute(s_parameterOrderString, null, false)
                    };
                    this.ParseWsdlPortTypeOperationContent(portType, operation);
                    portType.operations.Add(operation);
                }
                else
                {
                    this.SkipXmlElement();
                }
            }
            this.wsdlPortTypes[portType.name] = portType;
        }

        private void ParseWsdlPortTypeOperationContent(WsdlPortType portType, WsdlPortTypeOperation portTypeOperation)
        {
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (MatchingStrings(localName, s_inputString))
                {
                    WsdlPortTypeOperationContent content = new WsdlPortTypeOperationContent {
                        element = this.Atomize("input"),
                        name = this.LookupAttribute(s_nameString, null, false)
                    };
                    if (MatchingStrings(content.name, s_emptyString))
                    {
                        content.name = this.Atomize(portTypeOperation.name + "Request");
                        if (portType.sections.ContainsKey(content.name))
                        {
                            throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_DuplicatePortTypesOperationName"), new object[] { portTypeOperation.name }));
                        }
                        portType.sections[content.name] = portTypeOperation;
                        portType.sections[portTypeOperation.name] = portTypeOperation;
                    }
                    else
                    {
                        if (portType.sections.ContainsKey(content.name))
                        {
                            throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_DuplicatePortSectionName"), new object[] { content.name }));
                        }
                        portType.sections[content.name] = portTypeOperation;
                    }
                    content.message = this.LookupAttribute(s_messageString, null, true);
                    content.messageNs = this.ParseQName(ref content.message);
                    portTypeOperation.contents.Add(content);
                    this.ReadNextXmlElement();
                }
                else if (MatchingStrings(localName, s_outputString))
                {
                    WsdlPortTypeOperationContent content2;
                    content2 = new WsdlPortTypeOperationContent {
                        element = this.Atomize("output"),
                        name = this.LookupAttribute(s_nameString, null, false),
                        nameNs = this.ParseQName(ref content2.name)
                    };
                    if (MatchingStrings(content2.name, s_emptyString))
                    {
                        content2.name = this.Atomize(portTypeOperation.name + "Response");
                    }
                    if (!portType.sections.ContainsKey(content2.name))
                    {
                        portType.sections[content2.name] = portTypeOperation;
                    }
                    content2.message = this.LookupAttribute(s_messageString, null, true);
                    content2.messageNs = this.ParseQName(ref content2.message);
                    portTypeOperation.contents.Add(content2);
                    this.ReadNextXmlElement();
                }
                else
                {
                    this.SkipXmlElement();
                }
            }
        }

        private void ParseWsdlService()
        {
            WsdlService service = new WsdlService {
                name = this.LookupAttribute(s_nameString, null, true)
            };
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (this.MatchingNamespace(s_wsdlNamespaceString) && MatchingStrings(localName, s_portString))
                {
                    WsdlServicePort port;
                    port = new WsdlServicePort {
                        name = this.LookupAttribute(s_nameString, null, true),
                        nameNs = this.ParseQName(ref port.nameNs),
                        binding = this.LookupAttribute(s_bindingString, null, true),
                        bindingNs = this.ParseQName(ref port.binding)
                    };
                    this.ParseWsdlServicePort(port);
                    service.ports[port.binding] = port;
                }
                else
                {
                    this.SkipXmlElement();
                }
            }
            this.wsdlServices.Add(service);
        }

        private void ParseWsdlServicePort(WsdlServicePort port)
        {
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (this.MatchingNamespace(s_wsdlSoapNamespaceString) && MatchingStrings(localName, s_addressString))
                {
                    if (port.locations == null)
                    {
                        port.locations = new ArrayList(10);
                    }
                    port.locations.Add(this.LookupAttribute(s_locationString, null, true));
                    this.ReadNextXmlElement();
                }
                else
                {
                    this.SkipXmlElement();
                }
            }
        }

        private void ParseWsdlTypes()
        {
            int depth = this._XMLReader.Depth;
            this.ReadNextXmlElement();
            this._currentSchemaReaderStack.Push(this._currentReaderStack.Peek());
            while (this._XMLReader.Depth > depth)
            {
                string localName = this._XMLReader.LocalName;
                if (this.MatchingSchemaNamespace() && MatchingStrings(localName, s_schemaString))
                {
                    this.ParseSchema();
                    if (this._readerStreamsXsd != null)
                    {
                        this.ParseImportedSchemaController();
                    }
                }
                else
                {
                    this.SkipXmlElement();
                }
            }
            this._currentSchemaReaderStack.Pop();
        }

        private void PrintCSC()
        {
            int num = 0;
            for (int i = 0; i < this._URTNamespaces.Count; i++)
            {
                URTNamespace namespace2 = (URTNamespace) this._URTNamespaces[i];
                if (!namespace2.IsEmpty && (namespace2.UrtType == UrtType.Interop))
                {
                    if (num == 0)
                    {
                        namespace2.EncodedNS = this._proxyNamespace;
                    }
                    else
                    {
                        namespace2.EncodedNS = this._proxyNamespace + num;
                    }
                    num++;
                }
            }
            for (int j = 0; j < this._URTNamespaces.Count; j++)
            {
                URTNamespace namespace3 = (URTNamespace) this._URTNamespaces[j];
                if ((!namespace3.IsEmpty && (namespace3.UrtType != UrtType.UrtSystem)) && ((namespace3.UrtType != UrtType.Xsd) && (namespace3.UrtType != UrtType.None)))
                {
                    string fileName = namespace3.IsURTNamespace ? namespace3.AssemName : namespace3.EncodedNS;
                    int index = fileName.IndexOf(',');
                    if (index > -1)
                    {
                        fileName = fileName.Substring(0, index);
                    }
                    string completeFileName = "";
                    WriterStream writerStream = WriterStream.GetWriterStream(ref this._writerStreams, this._outputDir, fileName, ref completeFileName);
                    if (completeFileName.Length > 0)
                    {
                        this._outCodeStreamList.Add(completeFileName);
                    }
                    namespace3.PrintCSC(writerStream);
                }
            }
        }

        private SudsUse ProcessSudsUse(string use, string elementName)
        {
            SudsUse servicedComponent = SudsUse.Class;
            if ((use == null) || (use.Length == 0))
            {
                use = elementName;
            }
            if (MatchingStrings(use, s_interfaceString))
            {
                return SudsUse.Interface;
            }
            if (MatchingStrings(use, s_classString))
            {
                return SudsUse.Class;
            }
            if (MatchingStrings(use, s_structString))
            {
                return SudsUse.Struct;
            }
            if (MatchingStrings(use, s_ISerializableString))
            {
                return SudsUse.ISerializable;
            }
            if (MatchingStrings(use, s_marshalByRefString))
            {
                return SudsUse.MarshalByRef;
            }
            if (MatchingStrings(use, s_delegateString))
            {
                return SudsUse.Delegate;
            }
            if (MatchingStrings(use, s_servicedComponentString))
            {
                servicedComponent = SudsUse.ServicedComponent;
            }
            return servicedComponent;
        }

        private void PruneNamespaces()
        {
            ArrayList list = new ArrayList(10);
            for (int i = 0; i < this._URTNamespaces.Count; i++)
            {
                URTNamespace namespace2 = (URTNamespace) this._URTNamespaces[i];
                if (namespace2.bReferenced)
                {
                    list.Add(namespace2);
                }
            }
            this._URTNamespaces = list;
        }

        private bool Qualify(string typeNS, string curNS)
        {
            return ((!this.MatchingSchemaStrings(typeNS) && !MatchingStrings(typeNS, s_soapNamespaceString)) && ((!MatchingStrings(typeNS, s_wsdlSoapNamespaceString) && !MatchingStrings(typeNS, "System")) && !MatchingStrings(typeNS, curNS)));
        }

        private bool ReadNextXmlElement()
        {
            this._XMLReader.Read();
            XmlNodeType type = this._XMLReader.MoveToContent();
            while (type == XmlNodeType.EndElement)
            {
                this._XMLReader.Read();
                type = this._XMLReader.MoveToContent();
                if (type == XmlNodeType.None)
                {
                    break;
                }
            }
            return (type != XmlNodeType.None);
        }

        private void Resolve()
        {
            for (int i = 0; i < this._URTNamespaces.Count; i++)
            {
                ((URTNamespace) this._URTNamespaces[i]).ResolveElements(this);
            }
            for (int j = 0; j < this._URTNamespaces.Count; j++)
            {
                ((URTNamespace) this._URTNamespaces[j]).ResolveTypes(this);
            }
            for (int k = 0; k < this._URTNamespaces.Count; k++)
            {
                ((URTNamespace) this._URTNamespaces[k]).ResolveMethods();
            }
        }

        private void ResolveTypeAttribute(ref string typeName, out string typeNS, out bool bEmbedded, out bool bPrimitive)
        {
            if (MatchingStrings(typeName, s_emptyString))
            {
                typeName = s_objectString;
                typeNS = this.SchemaNamespaceString;
                bEmbedded = true;
                bPrimitive = false;
            }
            else
            {
                typeNS = this.ParseQName(ref typeName);
                this.ResolveTypeNames(ref typeNS, ref typeName, out bEmbedded, out bPrimitive);
            }
        }

        private void ResolveTypeNames(ref string typeNS, ref string typeName, out bool bEmbedded, out bool bPrimitive)
        {
            bEmbedded = true;
            bool flag = false;
            if (MatchingStrings(typeNS, s_wsdlSoapNamespaceString))
            {
                if (MatchingStrings(typeName, s_referenceString))
                {
                    bEmbedded = false;
                }
                else if (MatchingStrings(typeName, s_arrayString))
                {
                    flag = true;
                }
            }
            if (!bEmbedded || flag)
            {
                typeName = this.LookupAttribute(s_refTypeString, s_wsdlSudsNamespaceString, true);
                typeNS = this.ParseQName(ref typeName);
            }
            bPrimitive = this.IsPrimitiveType(typeNS, typeName);
            if (bPrimitive)
            {
                typeName = this.MapSchemaTypesToCSharpTypes(typeName);
                bEmbedded = false;
            }
            else if (MatchingStrings(typeName, s_urTypeString) && this.MatchingSchemaStrings(typeNS))
            {
                typeName = s_objectString;
            }
        }

        private void ResolveWsdl()
        {
            if (this.wsdlBindings.Count == 0)
            {
                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_RpcBindingsMissing"), new object[0]));
            }
            foreach (WsdlBinding binding in this.wsdlBindings)
            {
                if (binding.soapBinding != null)
                {
                    if ((binding.suds != null) && (binding.suds.Count > 0))
                    {
                        bool bFirstSuds = true;
                        foreach (WsdlBindingSuds suds in binding.suds)
                        {
                            if (MatchingStrings(suds.elementName, s_classString) || MatchingStrings(suds.elementName, s_structString))
                            {
                                this.ResolveWsdlClass(binding, suds, bFirstSuds);
                                bFirstSuds = false;
                            }
                            else
                            {
                                if (!MatchingStrings(suds.elementName, s_interfaceString))
                                {
                                    throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveElementInNS"), new object[] { suds.elementName, s_wsdlSudsNamespaceString }));
                                }
                                this.ResolveWsdlInterface(binding, suds);
                            }
                        }
                    }
                    else
                    {
                        this.ResolveWsdlClass(binding, null, true);
                    }
                }
            }
        }

        private ArrayList ResolveWsdlAddress(WsdlBinding binding)
        {
            ArrayList list = null;
            if (this._bWrappedProxy)
            {
                foreach (WsdlService service in this.wsdlServices)
                {
                    WsdlServicePort port = (WsdlServicePort) service.ports[binding.name];
                    if (port != null)
                    {
                        return port.locations;
                    }
                    if (list != null)
                    {
                        return list;
                    }
                }
            }
            return list;
        }

        private void ResolveWsdlClass(WsdlBinding binding, WsdlBindingSuds suds, bool bFirstSuds)
        {
            URTComplexType complexType;
            URTNamespace namespace2;
            if (suds != null)
            {
                namespace2 = this.AddNewNamespace(suds.ns);
                complexType = namespace2.LookupComplexType(suds.typeName);
                if (complexType == null)
                {
                    complexType = new URTComplexType(suds.typeName, namespace2.Name, namespace2.Namespace, namespace2.EncodedNS, this._blockDefault, false, false, this, namespace2);
                    namespace2.AddComplexType(complexType);
                }
                if (MatchingStrings(suds.elementName, s_structString))
                {
                    complexType.IsValueType = true;
                }
                complexType.SudsUse = suds.sudsUse;
                if ((suds.sudsUse == SudsUse.MarshalByRef) || (suds.sudsUse == SudsUse.ServicedComponent))
                {
                    complexType.IsSUDSType = true;
                    if (this._bWrappedProxy)
                    {
                        complexType.SUDSType = SUDSType.ClientProxy;
                    }
                    else
                    {
                        complexType.SUDSType = SUDSType.MarshalByRef;
                    }
                    if ((suds.extendsTypeName != null) && (suds.extendsTypeName.Length > 0))
                    {
                        URTNamespace xns = this.AddNewNamespace(suds.extendsNs);
                        URTComplexType type = xns.LookupComplexType(suds.extendsTypeName);
                        if (type == null)
                        {
                            type = new URTComplexType(suds.extendsTypeName, xns.Name, xns.Namespace, xns.EncodedNS, this._blockDefault, true, false, this, xns);
                            xns.AddComplexType(type);
                        }
                        else
                        {
                            type.IsSUDSType = true;
                        }
                        if (this._bWrappedProxy)
                        {
                            type.SUDSType = SUDSType.ClientProxy;
                        }
                        else
                        {
                            type.SUDSType = SUDSType.MarshalByRef;
                        }
                        type.SudsUse = suds.sudsUse;
                    }
                }
                foreach (WsdlBindingSudsNestedType type3 in suds.nestedTypes)
                {
                    this.ResolveWsdlNestedType(binding, suds, type3);
                }
            }
            else
            {
                namespace2 = this.AddNewNamespace(binding.typeNs);
                string name = binding.name;
                int index = binding.name.IndexOf("Binding");
                if (index > 0)
                {
                    name = binding.name.Substring(0, index);
                }
                complexType = namespace2.LookupComplexTypeEqual(name);
                if (complexType == null)
                {
                    complexType = new URTComplexType(name, namespace2.Name, namespace2.Namespace, namespace2.EncodedNS, this._blockDefault, true, false, this, namespace2);
                    namespace2.AddComplexType(complexType);
                }
                else
                {
                    complexType.IsSUDSType = true;
                }
                if (this._bWrappedProxy)
                {
                    complexType.SUDSType = SUDSType.ClientProxy;
                }
                else
                {
                    complexType.SUDSType = SUDSType.MarshalByRef;
                }
                complexType.SudsUse = SudsUse.MarshalByRef;
            }
            complexType.ConnectURLs = this.ResolveWsdlAddress(binding);
            if (suds != null)
            {
                if (!MatchingStrings(suds.extendsTypeName, s_emptyString))
                {
                    complexType.Extends(suds.extendsTypeName, suds.extendsNs);
                }
                foreach (WsdlBindingSudsImplements implements in suds.implements)
                {
                    complexType.Implements(implements.typeName, implements.ns, this);
                }
            }
            if (bFirstSuds && (((complexType.SudsUse == SudsUse.MarshalByRef) || (complexType.SudsUse == SudsUse.ServicedComponent)) || ((complexType.SudsUse == SudsUse.Delegate) || (complexType.SudsUse == SudsUse.Interface))))
            {
                foreach (WsdlMethodInfo info in this.ResolveWsdlMethodInfo(binding))
                {
                    if ((info.inputMethodName == null) || (info.outputMethodName == null))
                    {
                        if (info.inputMethodName == null)
                        {
                            throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_WsdlInvalidMessage"), new object[] { info.methodName }));
                        }
                        OnewayMethod method2 = new OnewayMethod(info, complexType);
                        complexType.AddMethod(method2);
                        method2.AddMessage(info.methodName, info.methodNameNs);
                    }
                    else
                    {
                        RRMethod method = new RRMethod(info, complexType);
                        method.AddRequest(info.methodName, info.methodNameNs);
                        method.AddResponse(info.methodName, info.methodNameNs);
                        complexType.AddMethod(method);
                    }
                }
            }
        }

        private void ResolveWsdlInterface(WsdlBinding binding, WsdlBindingSuds suds)
        {
            URTNamespace parsingNamespace = binding.parsingNamespace;
            URTNamespace namespace2 = this.AddNewNamespace(suds.ns);
            URTInterface iface = namespace2.LookupInterface(suds.typeName);
            if (iface == null)
            {
                iface = new URTInterface(suds.typeName, namespace2.Name, namespace2.Namespace, namespace2.EncodedNS, this);
                namespace2.AddInterface(iface);
            }
            if (suds.extendsTypeName != null)
            {
                iface.Extends(suds.extendsTypeName, suds.extendsNs, this);
            }
            foreach (WsdlBindingSudsImplements implements in suds.implements)
            {
                iface.Extends(implements.typeName, implements.ns, this);
            }
            foreach (WsdlMethodInfo info in this.ResolveWsdlMethodInfo(binding))
            {
                if ((info.inputMethodName == null) || (info.outputMethodName == null))
                {
                    if (info.inputMethodName == null)
                    {
                        throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_WsdlInvalidMessage"), new object[] { info.methodName }));
                    }
                    OnewayMethod method2 = new OnewayMethod(info.methodName, info.soapAction, null);
                    method2.AddMessage(info.methodName, info.methodNameNs);
                    iface.AddMethod(method2);
                }
                else
                {
                    RRMethod method = new RRMethod(info, null);
                    method.AddRequest(info.methodName, info.methodNameNs);
                    method.AddResponse(info.methodName, info.methodNameNs);
                    iface.AddMethod(method);
                }
            }
        }

        private ArrayList ResolveWsdlMethodInfo(WsdlBinding binding)
        {
            ArrayList list = new ArrayList(10);
            Hashtable hashtable = new Hashtable(3);
            for (int i = 0; i < binding.operations.Count; i++)
            {
                bool flag = false;
                bool flag2 = false;
                WsdlBindingOperation operation = (WsdlBindingOperation) binding.operations[i];
                if (operation.soapOperation == null)
                {
                    continue;
                }
                WsdlMethodInfo info = new WsdlMethodInfo {
                    methodName = operation.name,
                    methodNameNs = operation.nameNs,
                    methodAttributes = operation.methodAttributes
                };
                this.AddNewNamespace(operation.nameNs);
                WsdlBindingSoapOperation soapOperation = operation.soapOperation;
                if (info.methodName.StartsWith("get_", StringComparison.Ordinal) && (info.methodName.Length > 4))
                {
                    flag = true;
                }
                else if (info.methodName.StartsWith("set_", StringComparison.Ordinal) && (info.methodName.Length > 4))
                {
                    flag2 = true;
                }
                if (flag || flag2)
                {
                    bool flag3 = false;
                    string str = info.methodName.Substring(4);
                    WsdlMethodInfo info2 = (WsdlMethodInfo) hashtable[str];
                    if (info2 == null)
                    {
                        hashtable[str] = info;
                        list.Add(info);
                        info2 = info;
                        info.propertyName = str;
                        info.bProperty = true;
                        flag3 = true;
                    }
                    if (flag)
                    {
                        info2.bGet = true;
                        info2.soapActionGet = soapOperation.soapAction;
                    }
                    else
                    {
                        info2.bSet = true;
                        info2.soapActionSet = soapOperation.soapAction;
                    }
                    if (flag3)
                    {
                        goto Label_016C;
                    }
                    continue;
                }
                list.Add(info);
            Label_016C:
                info.soapAction = soapOperation.soapAction;
                WsdlPortType type = (WsdlPortType) this.wsdlPortTypes[binding.type];
                if ((type == null) || (type.operations.Count != binding.operations.Count))
                {
                    throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_WsdlInvalidPortType"), new object[] { binding.type }));
                }
                WsdlPortTypeOperation operation3 = null;
                foreach (WsdlBindingOperationSection section in operation.sections)
                {
                    if (MatchingStrings(section.elementName, s_inputString))
                    {
                        operation3 = (WsdlPortTypeOperation) type.sections[section.name];
                        if (operation3 == null)
                        {
                            int length = section.name.LastIndexOf("Request");
                            if (length > 0)
                            {
                                string str2 = section.name.Substring(0, length);
                                operation3 = (WsdlPortTypeOperation) type.sections[str2];
                            }
                        }
                        if (((operation3 != null) && (operation3.parameterOrder != null)) && (operation3.parameterOrder.Length > 0))
                        {
                            info.paramNamesOrder = operation3.parameterOrder.Split(new char[] { ' ' });
                        }
                        foreach (WsdlBindingSoapBody body in section.extensions)
                        {
                            if ((body.namespaceUri != null) || (body.namespaceUri.Length > 0))
                            {
                                info.inputMethodNameNs = body.namespaceUri;
                            }
                        }
                    }
                    else if (MatchingStrings(section.elementName, s_outputString))
                    {
                        foreach (WsdlBindingSoapBody body2 in section.extensions)
                        {
                            if ((body2.namespaceUri != null) || (body2.namespaceUri.Length > 0))
                            {
                                info.outputMethodNameNs = body2.namespaceUri;
                            }
                        }
                    }
                }
                if (operation3 != null)
                {
                    foreach (WsdlPortTypeOperationContent content in operation3.contents)
                    {
                        if (MatchingStrings(content.element, s_inputString))
                        {
                            info.inputMethodName = content.message;
                            if (info.inputMethodNameNs == null)
                            {
                                info.inputMethodNameNs = content.messageNs;
                            }
                            WsdlMessage message = (WsdlMessage) this.wsdlMessages[content.message];
                            if (message == null)
                            {
                                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_WsdlMissingMessage"), new object[] { content.message }));
                            }
                            if (message.parts != null)
                            {
                                info.inputNames = new string[message.parts.Count];
                                info.inputNamesNs = new string[message.parts.Count];
                                info.inputElements = new string[message.parts.Count];
                                info.inputElementsNs = new string[message.parts.Count];
                                info.inputTypes = new string[message.parts.Count];
                                info.inputTypesNs = new string[message.parts.Count];
                                for (int j = 0; j < message.parts.Count; j++)
                                {
                                    info.inputNames[j] = ((WsdlMessagePart) message.parts[j]).name;
                                    info.inputNamesNs[j] = ((WsdlMessagePart) message.parts[j]).nameNs;
                                    this.AddNewNamespace(info.inputNamesNs[j]);
                                    info.inputElements[j] = ((WsdlMessagePart) message.parts[j]).element;
                                    info.inputElementsNs[j] = ((WsdlMessagePart) message.parts[j]).elementNs;
                                    this.AddNewNamespace(info.inputElementsNs[j]);
                                    info.inputTypes[j] = ((WsdlMessagePart) message.parts[j]).typeName;
                                    info.inputTypesNs[j] = ((WsdlMessagePart) message.parts[j]).typeNameNs;
                                    this.AddNewNamespace(info.inputTypesNs[j]);
                                    if ((info.bProperty && (info.inputTypes[j] != null)) && (info.propertyType == null))
                                    {
                                        info.propertyType = info.inputTypes[j];
                                        info.propertyNs = info.inputTypesNs[j];
                                        this.AddNewNamespace(info.propertyNs);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!MatchingStrings(content.element, s_outputString))
                            {
                                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_WsdlInvalidPortType"), new object[] { content.element }));
                            }
                            info.outputMethodName = content.message;
                            if (info.outputMethodNameNs == null)
                            {
                                info.outputMethodNameNs = content.messageNs;
                            }
                            WsdlMessage message2 = (WsdlMessage) this.wsdlMessages[content.message];
                            if (message2 == null)
                            {
                                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_WsdlMissingMessage"), new object[] { content.message }));
                            }
                            if (message2.parts != null)
                            {
                                info.outputNames = new string[message2.parts.Count];
                                info.outputNamesNs = new string[message2.parts.Count];
                                info.outputElements = new string[message2.parts.Count];
                                info.outputElementsNs = new string[message2.parts.Count];
                                info.outputTypes = new string[message2.parts.Count];
                                info.outputTypesNs = new string[message2.parts.Count];
                                for (int k = 0; k < message2.parts.Count; k++)
                                {
                                    info.outputNames[k] = ((WsdlMessagePart) message2.parts[k]).name;
                                    info.outputNamesNs[k] = ((WsdlMessagePart) message2.parts[k]).nameNs;
                                    this.AddNewNamespace(info.outputNamesNs[k]);
                                    info.outputElements[k] = ((WsdlMessagePart) message2.parts[k]).element;
                                    info.outputElementsNs[k] = ((WsdlMessagePart) message2.parts[k]).elementNs;
                                    this.AddNewNamespace(info.outputElementsNs[k]);
                                    info.outputTypes[k] = ((WsdlMessagePart) message2.parts[k]).typeName;
                                    info.outputTypesNs[k] = ((WsdlMessagePart) message2.parts[k]).typeNameNs;
                                    this.AddNewNamespace(info.outputTypesNs[k]);
                                    if ((info.bProperty && (info.outputTypes[k] != null)) && (info.propertyType == null))
                                    {
                                        info.propertyType = info.outputTypes[k];
                                        info.propertyNs = info.outputTypesNs[k];
                                        this.AddNewNamespace(info.outputTypesNs[k]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }

        private void ResolveWsdlNestedType(WsdlBinding binding, WsdlBindingSuds suds, WsdlBindingSudsNestedType nested)
        {
            string typeName = suds.typeName;
            string ns = nested.ns;
            string name = nested.name;
            string text3 = nested.typeName;
            if (suds.ns != ns)
            {
                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveNestedTypeNS"), new object[] { suds.typeName, suds.ns }));
            }
            URTNamespace xns = this.AddNewNamespace(suds.ns);
            URTComplexType complexType = xns.LookupComplexType(suds.typeName);
            if (complexType == null)
            {
                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveNestedType"), new object[] { suds.typeName, suds.ns }));
            }
            BaseType type = xns.LookupType(nested.typeName);
            if (type == null)
            {
                type = xns.LookupComplexType(nested.typeName);
                if (type == null)
                {
                    type = new URTComplexType(nested.typeName, xns.Name, xns.Namespace, xns.EncodedNS, this._blockDefault, false, false, this, xns);
                    xns.AddComplexType((URTComplexType) type);
                }
            }
            type.bNestedType = true;
            type.NestedTypeName = nested.name;
            type.FullNestedTypeName = nested.typeName;
            type.OuterTypeName = suds.typeName;
            complexType.AddNestedType(type);
        }

        private bool SkipXmlElement()
        {
            this._XMLReader.Skip();
            XmlNodeType type = this._XMLReader.MoveToContent();
            while (type == XmlNodeType.EndElement)
            {
                this._XMLReader.Read();
                type = this._XMLReader.MoveToContent();
                if (type == XmlNodeType.None)
                {
                    break;
                }
            }
            return (type != XmlNodeType.None);
        }

        private void StartWsdlResolution()
        {
            this.ResolveWsdl();
            this.Resolve();
            this.PruneNamespaces();
            this.PrintCSC();
        }

        internal string ProxyNamespace
        {
            get
            {
                return this._proxyNamespace;
            }
        }

        internal int ProxyNamespaceCount
        {
            get
            {
                return this._proxyNamespaceCount;
            }
            set
            {
                this._proxyNamespaceCount = value;
            }
        }

        internal string SchemaNamespaceString
        {
            get
            {
                switch (this._xsdVersion)
                {
                    case XsdVersion.V1999:
                        return s_schemaNamespaceString1999;

                    case XsdVersion.V2000:
                        return s_schemaNamespaceString2000;

                    case XsdVersion.V2001:
                        return s_schemaNamespaceString;
                }
                return null;
            }
        }

        internal XmlTextReader XMLReader
        {
            get
            {
                return this._XMLReader;
            }
        }

        internal abstract class BaseInterface
        {
            private string _encodedNS;
            private string _name;
            private string _namespace;
            private WsdlParser _parser;
            private string _urlNS;

            internal BaseInterface(string name, string urlNS, string ns, string encodedNS, WsdlParser parser)
            {
                this._name = name;
                this._urlNS = urlNS;
                this._namespace = ns;
                this._encodedNS = encodedNS;
                this._parser = parser;
            }

            internal string GetName(string curNS)
            {
                if (this._parser.Qualify(this._namespace, curNS))
                {
                    StringBuilder builder = new StringBuilder(this._encodedNS, 50);
                    builder.Append('.');
                    builder.Append(WsdlParser.IsValidCS(this._name));
                    return builder.ToString();
                }
                return this._name;
            }

            internal abstract void PrintClassMethods(TextWriter textWriter, string indentation, string curNS, ArrayList printedIFaces, bool bProxy, StringBuilder sb);

            internal bool IsURTInterface
            {
                get
                {
                    return (this._namespace == this._encodedNS);
                }
            }

            internal string Name
            {
                get
                {
                    return this._name;
                }
            }

            internal string Namespace
            {
                get
                {
                    return this._namespace;
                }
            }

            internal string UrlNS
            {
                get
                {
                    return this._urlNS;
                }
            }
        }

        internal abstract class BaseType
        {
            internal bool _bNestedType;
            internal bool _bNestedTypePrint;
            private string _elementName;
            private string _elementNS;
            private string _encodedNS;
            internal string _fullNestedTypeName;
            private string _name;
            private string _namespace;
            internal string _nestedTypeName;
            internal ArrayList _nestedTypes;
            internal string _outerTypeName;
            private string _searchName;
            private string _urlNS;

            internal BaseType(string name, string urlNS, string ns, string encodedNS)
            {
                this._searchName = name;
                this._name = name;
                this._urlNS = urlNS;
                this._namespace = ns;
                this._elementName = this._name;
                this._elementNS = ns;
                this._encodedNS = encodedNS;
            }

            internal abstract WsdlParser.MethodFlags GetMethodFlags(WsdlParser.URTMethod method);
            internal virtual string GetName(string curNS)
            {
                if (WsdlParser.MatchingStrings(this._namespace, curNS))
                {
                    return this._name;
                }
                StringBuilder builder = new StringBuilder(this._encodedNS, 50);
                builder.Append('.');
                builder.Append(WsdlParser.IsValidCS(this._name));
                return builder.ToString();
            }

            internal bool bNestedType
            {
                get
                {
                    return this._bNestedType;
                }
                set
                {
                    this._bNestedType = value;
                }
            }

            internal bool bNestedTypePrint
            {
                get
                {
                    return this._bNestedTypePrint;
                }
                set
                {
                    this._bNestedTypePrint = value;
                }
            }

            internal string ElementName
            {
                set
                {
                    this._elementName = value;
                }
            }

            internal string ElementNS
            {
                set
                {
                    this._elementNS = value;
                }
            }

            internal abstract string FieldName { get; }

            internal abstract string FieldNamespace { get; }

            internal string FullNestedTypeName
            {
                set
                {
                    this._fullNestedTypeName = value;
                }
            }

            internal abstract bool IsEmittableFieldType { get; }

            internal bool IsURTType
            {
                get
                {
                    return (this._namespace == this._encodedNS);
                }
            }

            internal string Name
            {
                get
                {
                    return this._name;
                }
                set
                {
                    this._name = value;
                }
            }

            internal string Namespace
            {
                get
                {
                    return this._namespace;
                }
            }

            internal string NestedTypeName
            {
                get
                {
                    return this._nestedTypeName;
                }
                set
                {
                    this._nestedTypeName = value;
                }
            }

            internal string OuterTypeName
            {
                set
                {
                    this._outerTypeName = value;
                }
            }

            internal abstract bool PrimitiveField { get; }

            internal string SearchName
            {
                get
                {
                    return this._searchName;
                }
                set
                {
                    this._searchName = value;
                }
            }

            internal string UrlNS
            {
                get
                {
                    return this._urlNS;
                }
            }
        }

        internal class ElementDecl
        {
            private bool _bPrimitive;
            private string _elmName;
            private string _elmNS;
            private string _typeName;
            private string _typeNS;

            internal ElementDecl(string elmName, string elmNS, string typeName, string typeNS, bool bPrimitive)
            {
                this._elmName = elmName;
                this._elmNS = elmNS;
                this._typeName = typeName;
                this._typeNS = typeNS;
                this._bPrimitive = bPrimitive;
            }

            internal bool Resolve(WsdlParser parser)
            {
                if (!this._bPrimitive)
                {
                    WsdlParser.URTNamespace namespace2 = parser.LookupNamespace(this.TypeNS);
                    if (namespace2 == null)
                    {
                        throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveSchemaNS"), new object[] { this.TypeNS, this.TypeName }));
                    }
                    WsdlParser.BaseType type = namespace2.LookupType(this.TypeName);
                    if (type == null)
                    {
                        return false;
                    }
                    type.ElementName = this.Name;
                    type.ElementNS = this.Namespace;
                }
                return true;
            }

            internal string Name
            {
                get
                {
                    return this._elmName;
                }
            }

            internal string Namespace
            {
                get
                {
                    return this._elmNS;
                }
            }

            internal string TypeName
            {
                get
                {
                    return this._typeName;
                }
            }

            internal string TypeNS
            {
                get
                {
                    return this._typeNS;
                }
            }
        }

        internal class EnumFacet : WsdlParser.SchemaFacet
        {
            private int _value;
            private string _valueString;

            internal EnumFacet(string valueString, int value)
            {
                this._valueString = valueString;
                this._value = value;
            }

            internal override void PrintCSC(TextWriter textWriter, string newIndentation, string curNS, StringBuilder sb)
            {
                sb.Length = 0;
                sb.Append(newIndentation);
                sb.Append(WsdlParser.IsValidCS(this._valueString));
                sb.Append(" = ");
                sb.Append(this._value);
                sb.Append(',');
                textWriter.WriteLine(sb);
            }
        }

        internal interface IDump
        {
            void Dump();
        }

        internal interface INamespaces
        {
            void UsedNamespace(Hashtable namespaces);
        }

        [Flags]
        internal enum MethodFlags
        {
            Internal = 0x20,
            New = 8,
            None = 0,
            Override = 4,
            Protected = 2,
            Public = 1,
            Virtual = 0x10
        }

        [Flags]
        internal enum MethodPrintEnum
        {
            InterfaceInClass = 4,
            InterfaceMethods = 2,
            PrintBody = 1
        }

        internal class OnewayMethod : WsdlParser.URTMethod
        {
            private string _messageElementName;
            private string _messageElementNS;
            private WsdlParser.WsdlMethodInfo _wsdlMethodInfo;

            internal OnewayMethod(WsdlParser.WsdlMethodInfo wsdlMethodInfo, WsdlParser.URTComplexType complexType) : base(wsdlMethodInfo.methodName, wsdlMethodInfo.soapAction, wsdlMethodInfo.methodAttributes, complexType)
            {
                this._wsdlMethodInfo = wsdlMethodInfo;
                this._messageElementName = null;
                this._messageElementNS = null;
            }

            internal OnewayMethod(string name, string soapAction, WsdlParser.URTComplexType complexType) : base(name, soapAction, null, complexType)
            {
                this._messageElementName = null;
                this._messageElementNS = null;
            }

            internal void AddMessage(string name, string ns)
            {
                this._messageElementName = name;
                this._messageElementNS = ns;
            }

            internal override void PrintCSC(TextWriter textWriter, string indentation, string namePrefix, string curNS, WsdlParser.MethodPrintEnum methodPrintEnum, bool bURTType, string bodyPrefix, StringBuilder sb)
            {
                if (base.Name != "Finalize")
                {
                    bool flag = false;
                    if (base.SoapAction != null)
                    {
                        flag = true;
                    }
                    if (!flag && bURTType)
                    {
                        textWriter.Write(indentation);
                        textWriter.WriteLine("[OneWay]");
                    }
                    else
                    {
                        sb.Length = 0;
                        sb.Append(indentation);
                        sb.Append("[OneWay, SoapMethod(");
                        if (flag)
                        {
                            sb.Append("SoapAction=");
                            sb.Append(WsdlParser.IsValidUrl(base.SoapAction));
                        }
                        if (!bURTType)
                        {
                            if (flag)
                            {
                                sb.Append(",");
                            }
                            sb.Append("XmlNamespace=");
                            sb.Append(WsdlParser.IsValidUrl(this._wsdlMethodInfo.inputMethodNameNs));
                        }
                        sb.Append(")]");
                        textWriter.WriteLine(sb);
                    }
                    base.PrintCSC(textWriter, indentation, namePrefix, curNS, methodPrintEnum, bURTType, bodyPrefix, sb);
                }
            }

            internal override void ResolveTypes(WsdlParser parser)
            {
                base.ResolveWsdlParams(parser, this._messageElementNS, this._messageElementName, true, this._wsdlMethodInfo);
                if (base._paramNamesOrder != null)
                {
                    object[] c = new object[base._params.Count];
                    for (int i = 0; i < base._params.Count; i++)
                    {
                        c[(int) base._paramPosition[i]] = base._params[i];
                    }
                    base._params = new ArrayList(c);
                }
                base.ResolveMethodAttributes();
            }
        }

        internal class ReaderStream
        {
            private string _location;
            private string _name;
            private WsdlParser.ReaderStream _next;
            private TextReader _reader;
            private string _targetNS;
            private WsdlParser.URTNamespace _uniqueNS;
            private System.Uri _uri;

            internal ReaderStream(string location)
            {
                this._location = location;
                this._name = string.Empty;
                this._targetNS = string.Empty;
                this._uniqueNS = null;
                this._reader = null;
                this._next = null;
            }

            internal static WsdlParser.ReaderStream GetNextReaderStream(WsdlParser.ReaderStream input)
            {
                return input._next;
            }

            internal static void GetReaderStream(WsdlParser.ReaderStream inputStreams, WsdlParser.ReaderStream newStream)
            {
                WsdlParser.ReaderStream stream2;
                WsdlParser.ReaderStream stream = inputStreams;
                do
                {
                    if (stream._location == newStream.Location)
                    {
                        return;
                    }
                    stream2 = stream;
                    stream = stream._next;
                }
                while (stream != null);
                stream = newStream;
                stream2._next = stream;
            }

            internal TextReader InputStream
            {
                get
                {
                    return this._reader;
                }
                set
                {
                    this._reader = value;
                }
            }

            internal string Location
            {
                get
                {
                    return this._location;
                }
                set
                {
                    this._location = value;
                }
            }

            internal string Name
            {
                set
                {
                    this._name = value;
                }
            }

            internal string TargetNS
            {
                get
                {
                    return this._targetNS;
                }
                set
                {
                    this._targetNS = value;
                }
            }

            internal WsdlParser.URTNamespace UniqueNS
            {
                get
                {
                    return this._uniqueNS;
                }
                set
                {
                    this._uniqueNS = value;
                }
            }

            internal System.Uri Uri
            {
                get
                {
                    return this._uri;
                }
                set
                {
                    this._uri = value;
                }
            }
        }

        internal class RRMethod : WsdlParser.URTMethod
        {
            private string _requestElementName;
            private string _requestElementNS;
            private string _responseElementName;
            private string _responseElementNS;
            private WsdlParser.WsdlMethodInfo _wsdlMethodInfo;

            internal RRMethod(WsdlParser.WsdlMethodInfo wsdlMethodInfo, WsdlParser.URTComplexType complexType) : base(wsdlMethodInfo.methodName, wsdlMethodInfo.soapAction, wsdlMethodInfo.methodAttributes, complexType)
            {
                this._wsdlMethodInfo = wsdlMethodInfo;
                this._requestElementName = null;
                this._requestElementNS = null;
                this._responseElementName = null;
                this._responseElementNS = null;
            }

            internal void AddRequest(string name, string ns)
            {
                this._requestElementName = name;
                this._requestElementNS = ns;
            }

            internal void AddResponse(string name, string ns)
            {
                this._responseElementName = name;
                this._responseElementNS = ns;
            }

            internal override void PrintCSC(TextWriter textWriter, string indentation, string namePrefix, string curNS, WsdlParser.MethodPrintEnum methodPrintEnum, bool bURTType, string bodyPrefix, StringBuilder sb)
            {
                if (base.Name != "Finalize")
                {
                    bool flag = false;
                    if (base.SoapAction != null)
                    {
                        flag = true;
                    }
                    if ((flag || !bURTType) && !this._wsdlMethodInfo.bProperty)
                    {
                        sb.Length = 0;
                        sb.Append(indentation);
                        sb.Append("[SoapMethod(");
                        if (flag)
                        {
                            sb.Append("SoapAction=");
                            sb.Append(WsdlParser.IsValidUrl(base.SoapAction));
                        }
                        if (!bURTType)
                        {
                            if (flag)
                            {
                                sb.Append(",");
                            }
                            sb.Append("ResponseXmlElementName=");
                            sb.Append(WsdlParser.IsValidUrl(this._responseElementName));
                            if (base.MethodType != null)
                            {
                                sb.Append(", ReturnXmlElementName=");
                                sb.Append(WsdlParser.IsValidUrl(base.MethodType.Name));
                            }
                            sb.Append(", XmlNamespace=");
                            sb.Append(WsdlParser.IsValidUrl(this._wsdlMethodInfo.inputMethodNameNs));
                            sb.Append(", ResponseXmlNamespace=");
                            sb.Append(WsdlParser.IsValidUrl(this._wsdlMethodInfo.outputMethodNameNs));
                        }
                        sb.Append(")]");
                        textWriter.WriteLine(sb);
                    }
                    base.PrintCSC(textWriter, indentation, namePrefix, curNS, methodPrintEnum, bURTType, bodyPrefix, sb);
                }
            }

            internal override void ResolveTypes(WsdlParser parser)
            {
                base.ResolveWsdlParams(parser, this._requestElementNS, this._requestElementName, true, this._wsdlMethodInfo);
                base.ResolveWsdlParams(parser, this._responseElementNS, this._responseElementName, false, this._wsdlMethodInfo);
                if (base._paramNamesOrder != null)
                {
                    object[] c = new object[base._params.Count];
                    for (int i = 0; i < base._params.Count; i++)
                    {
                        c[(int) base._paramPosition[i]] = base._params[i];
                    }
                    base._params = new ArrayList(c);
                }
                base.ResolveMethodAttributes();
            }
        }

        internal abstract class SchemaFacet
        {
            protected SchemaFacet()
            {
            }

            internal abstract void PrintCSC(TextWriter textWriter, string newIndentation, string curNS, StringBuilder sb);
            internal virtual void ResolveTypes(WsdlParser parser)
            {
            }
        }

        internal enum SudsUse
        {
            Class,
            ISerializable,
            Struct,
            Interface,
            MarshalByRef,
            Delegate,
            ServicedComponent
        }

        internal class SystemInterface : WsdlParser.BaseInterface
        {
            private Type _type;

            internal SystemInterface(string name, string urlNS, string ns, WsdlParser parser, string assemName) : base(name, urlNS, ns, ns, parser)
            {
                string str = ns + '.' + name;
                Assembly assembly = null;
                if (assemName == null)
                {
                    assembly = typeof(string).Assembly;
                }
                else
                {
                    assembly = Assembly.LoadWithPartialName(assemName, null);
                }
                if (assembly == null)
                {
                    throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_AssemblyNotFound"), new object[] { assemName }));
                }
                this._type = assembly.GetType(str, true);
            }

            private static string CSharpTypeString(string typeName)
            {
                string identifier = typeName;
                if (typeName == "System.SByte")
                {
                    identifier = "sbyte";
                }
                else if (typeName == "System.byte")
                {
                    identifier = "byte";
                }
                else if (typeName == "System.Int16")
                {
                    identifier = "short";
                }
                else if (typeName == "System.UInt16")
                {
                    identifier = "ushort";
                }
                else if (typeName == "System.Int32")
                {
                    identifier = "int";
                }
                else if (typeName == "System.UInt32")
                {
                    identifier = "uint";
                }
                else if (typeName == "System.Int64")
                {
                    identifier = "long";
                }
                else if (typeName == "System.UInt64")
                {
                    identifier = "ulong";
                }
                else if (typeName == "System.Char")
                {
                    identifier = "char";
                }
                else if (typeName == "System.Single")
                {
                    identifier = "float";
                }
                else if (typeName == "System.Double")
                {
                    identifier = "double";
                }
                else if (typeName == "System.Boolean")
                {
                    identifier = "boolean";
                }
                else if (typeName == "System.Void")
                {
                    identifier = "void";
                }
                else if (typeName == "System.String")
                {
                    identifier = "String";
                }
                return WsdlParser.IsValidCSAttr(identifier);
            }

            internal override void PrintClassMethods(TextWriter textWriter, string indentation, string curNS, ArrayList printedIFaces, bool bProxy, StringBuilder sb)
            {
                int num;
                for (num = 0; num < printedIFaces.Count; num++)
                {
                    if (printedIFaces[num] is WsdlParser.SystemInterface)
                    {
                        WsdlParser.SystemInterface interface2 = (WsdlParser.SystemInterface) printedIFaces[num];
                        if (interface2._type == this._type)
                        {
                            return;
                        }
                    }
                }
                printedIFaces.Add(this);
                BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                ArrayList list = new ArrayList();
                sb.Length = 0;
                list.Add(this._type);
                num = 0;
                int num2 = 1;
                while (num < num2)
                {
                    Type type = (Type) list[num];
                    MethodInfo[] methods = type.GetMethods(bindingAttr);
                    Type[] interfaces = type.GetInterfaces();
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        for (int k = 0; k < num2; k++)
                        {
                            if (type == interfaces[i])
                            {
                                continue;
                            }
                        }
                        list.Add(interfaces[i]);
                        num2++;
                    }
                    for (int j = 0; j < methods.Length; j++)
                    {
                        MethodInfo info = methods[j];
                        sb.Length = 0;
                        sb.Append(indentation);
                        sb.Append(CSharpTypeString(info.ReturnType.FullName));
                        sb.Append(' ');
                        sb.Append(WsdlParser.IsValidCS(type.FullName));
                        sb.Append('.');
                        sb.Append(WsdlParser.IsValidCS(info.Name));
                        sb.Append('(');
                        ParameterInfo[] parameters = info.GetParameters();
                        for (int m = 0; m < parameters.Length; m++)
                        {
                            if (m != 0)
                            {
                                sb.Append(", ");
                            }
                            ParameterInfo info2 = parameters[m];
                            Type parameterType = info2.ParameterType;
                            if (info2.IsIn)
                            {
                                sb.Append("in ");
                            }
                            else if (info2.IsOut)
                            {
                                sb.Append("out ");
                            }
                            else if (parameterType.IsByRef)
                            {
                                sb.Append("ref ");
                                parameterType = parameterType.GetElementType();
                            }
                            sb.Append(CSharpTypeString(parameterType.FullName));
                            sb.Append(' ');
                            sb.Append(WsdlParser.IsValidCS(info2.Name));
                        }
                        sb.Append(')');
                        textWriter.WriteLine(sb);
                        textWriter.Write(indentation);
                        textWriter.WriteLine('{');
                        string str = indentation + "    ";
                        if (!bProxy)
                        {
                            for (int n = 0; n < parameters.Length; n++)
                            {
                                ParameterInfo info3 = parameters[n];
                                Type type1 = info3.ParameterType;
                                if (info3.IsOut)
                                {
                                    sb.Length = 0;
                                    sb.Append(str);
                                    sb.Append(WsdlParser.IsValidCS(info3.Name));
                                    sb.Append(WsdlParser.URTMethod.ValueString(CSharpTypeString(info3.ParameterType.FullName)));
                                    sb.Append(';');
                                    textWriter.WriteLine(sb);
                                }
                            }
                            sb.Length = 0;
                            sb.Append(str);
                            sb.Append("return");
                            string str2 = WsdlParser.URTMethod.ValueString(CSharpTypeString(info.ReturnType.FullName));
                            if (str2 != null)
                            {
                                sb.Append('(');
                                sb.Append(str2);
                                sb.Append(')');
                            }
                            sb.Append(';');
                        }
                        else
                        {
                            sb.Length = 0;
                            sb.Append(str);
                            sb.Append("return((");
                            sb.Append(WsdlParser.IsValidCS(type.FullName));
                            sb.Append(") _tp).");
                            sb.Append(WsdlParser.IsValidCS(info.Name));
                            sb.Append('(');
                            if (parameters.Length > 0)
                            {
                                int num8 = parameters.Length - 1;
                                for (int num9 = 0; num9 < parameters.Length; num9++)
                                {
                                    ParameterInfo info4 = parameters[0];
                                    Type type3 = info4.ParameterType;
                                    if (info4.IsIn)
                                    {
                                        sb.Append("in ");
                                    }
                                    else if (info4.IsOut)
                                    {
                                        sb.Append("out ");
                                    }
                                    else if (type3.IsByRef)
                                    {
                                        sb.Append("ref ");
                                    }
                                    sb.Append(WsdlParser.IsValidCS(info4.Name));
                                    if (num9 < num8)
                                    {
                                        sb.Append(", ");
                                    }
                                }
                            }
                            sb.Append(");");
                        }
                        textWriter.WriteLine(sb);
                        textWriter.Write(indentation);
                        textWriter.WriteLine('}');
                    }
                    num++;
                }
            }
        }

        internal class SystemType : WsdlParser.BaseType
        {
            private Type _type;

            internal SystemType(string name, string urlNS, string ns, string assemName) : base(name, urlNS, ns, ns)
            {
                string str = ns + '.' + name;
                Assembly assembly = null;
                if (assemName == null)
                {
                    assembly = typeof(string).Assembly;
                }
                else
                {
                    assembly = Assembly.LoadWithPartialName(assemName, null);
                }
                if (assembly == null)
                {
                    throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_AssemblyNotFound"), new object[] { assemName }));
                }
                this._type = assembly.GetType(str, true);
            }

            internal override WsdlParser.MethodFlags GetMethodFlags(WsdlParser.URTMethod method)
            {
                BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                for (Type type = this._type; type != null; type = type.BaseType)
                {
                    MethodInfo[] methods = type.GetMethods(bindingAttr);
                    for (int i = 0; i < methods.Length; i++)
                    {
                        WsdlParser.MethodFlags methodFlags = method.GetMethodFlags(methods[i]);
                        if (methodFlags != WsdlParser.MethodFlags.None)
                        {
                            return methodFlags;
                        }
                    }
                }
                return WsdlParser.MethodFlags.None;
            }

            internal override string FieldName
            {
                get
                {
                    return null;
                }
            }

            internal override string FieldNamespace
            {
                get
                {
                    return null;
                }
            }

            internal override bool IsEmittableFieldType
            {
                get
                {
                    return true;
                }
            }

            internal override bool PrimitiveField
            {
                get
                {
                    return false;
                }
            }
        }

        internal class URTComplexType : WsdlParser.BaseType
        {
            private WsdlParser.URTNamespace _arrayNS;
            private string _arrayType;
            private bool _bAnonymous;
            private WsdlParser.BaseType _baseType;
            private string _baseTypeName;
            private string _baseTypeXmlNS;
            private SchemaBlockType _blockType;
            private bool _bNameMethodConflict;
            private bool _bprint;
            private bool _bStruct;
            private bool _bSUDSType;
            private bool _bValueType;
            private string _clrarray;
            private ArrayList _connectURLs;
            private ArrayList _fields;
            private string _fieldString;
            private ArrayList _implIFaceNames;
            private ArrayList _implIFaces;
            private ArrayList _inherit;
            private ArrayList _methods;
            private WsdlParser _parser;
            private System.Runtime.Remoting.MetadataServices.SUDSType _sudsType;
            private System.Runtime.Remoting.MetadataServices.WsdlParser.SudsUse _sudsUse;
            private string _wireType;

            internal URTComplexType(string name, string urlNS, string ns, string encodedNS, SchemaBlockType blockDefault, bool bSUDSType, bool bAnonymous, WsdlParser parser, WsdlParser.URTNamespace xns) : base(name, urlNS, ns, encodedNS)
            {
                this._bprint = true;
                this._baseTypeName = null;
                this._baseTypeXmlNS = null;
                this._baseType = null;
                this._connectURLs = null;
                this._bStruct = !bSUDSType;
                this._blockType = blockDefault;
                this._bSUDSType = bSUDSType;
                this._bAnonymous = bAnonymous;
                this._fieldString = null;
                this._fields = new ArrayList();
                this._methods = new ArrayList();
                this._implIFaces = new ArrayList();
                this._implIFaceNames = new ArrayList();
                this._sudsType = System.Runtime.Remoting.MetadataServices.SUDSType.None;
                this._parser = parser;
                int index = name.IndexOf('+');
                if (index > 0)
                {
                    string typeName = parser.Atomize(name.Substring(0, index));
                    if (xns.LookupComplexType(typeName) == null)
                    {
                        WsdlParser.URTComplexType type = new WsdlParser.URTComplexType(typeName, urlNS, ns, encodedNS, blockDefault, bSUDSType, bAnonymous, parser, xns);
                        xns.AddComplexType(type);
                    }
                }
                if ((xns.UrtType == UrtType.Interop) && (name.LastIndexOf('.') > -1))
                {
                    this._wireType = name;
                    base.Name = name.Replace(".", "_");
                    base.SearchName = name;
                }
            }

            internal void AddArray(string arrayType, WsdlParser.URTNamespace arrayNS)
            {
                this._arrayType = arrayType;
                this._arrayNS = arrayNS;
            }

            internal void AddField(WsdlParser.URTField field)
            {
                this._fields.Add(field);
            }

            internal void AddMethod(WsdlParser.URTMethod method)
            {
                if (method.Name == base.Name)
                {
                    this._bNameMethodConflict = true;
                }
                this._methods.Add(method);
                if (method.Name.IndexOf('.') > 0)
                {
                    method.MethodFlags = WsdlParser.MethodFlags.None;
                }
                else
                {
                    method.MethodFlags = method.MethodFlags |= WsdlParser.MethodFlags.Public;
                }
            }

            internal void AddNestedType(WsdlParser.BaseType ct)
            {
                if (base._nestedTypes == null)
                {
                    base._nestedTypes = new ArrayList(10);
                }
                base._nestedTypes.Add(ct);
            }

            internal void Extends(string baseTypeName, string baseTypeNS)
            {
                this._baseTypeName = baseTypeName;
                this._baseTypeXmlNS = baseTypeNS;
            }

            private string FilterDimensions(string value)
            {
                char[] chArray = new char[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    if (char.IsDigit(value[i]))
                    {
                        chArray[i] = ' ';
                    }
                    else
                    {
                        chArray[i] = value[i];
                    }
                }
                return new string(chArray);
            }

            internal string GetArray()
            {
                return this._clrarray;
            }

            internal WsdlParser.URTNamespace GetArrayNS()
            {
                return this._arrayNS;
            }

            internal string GetClassName()
            {
                if (this._bNameMethodConflict)
                {
                    return ("C" + base.Name);
                }
                return base.Name;
            }

            private WsdlParser.URTMethod GetMethod(string name)
            {
                for (int i = 0; i < this._methods.Count; i++)
                {
                    WsdlParser.URTMethod method = (WsdlParser.URTMethod) this._methods[i];
                    if (method.Name == name)
                    {
                        return method;
                    }
                }
                return null;
            }

            internal override WsdlParser.MethodFlags GetMethodFlags(WsdlParser.URTMethod method)
            {
                return method.MethodFlags;
            }

            internal override string GetName(string curNS)
            {
                if ((this._fieldString != null) && (this._fieldString != string.Empty))
                {
                    return this._fieldString;
                }
                return base.GetName(curNS);
            }

            internal void Implements(string iFaceName, string iFaceNS, WsdlParser parser)
            {
                this._implIFaceNames.Add(iFaceName);
                this._implIFaceNames.Add(iFaceNS);
                WsdlParser.URTNamespace namespace2 = parser.AddNewNamespace(iFaceNS);
                if (namespace2.LookupInterface(iFaceName) == null)
                {
                    WsdlParser.URTInterface iface = new WsdlParser.URTInterface(iFaceName, namespace2.Name, namespace2.Namespace, namespace2.EncodedNS, this._parser);
                    namespace2.AddInterface(iface);
                }
            }

            internal bool IsArray()
            {
                return (this._arrayType != null);
            }

            private void PrintClientProxy(TextWriter textWriter, string indentation, string curNS, StringBuilder sb)
            {
                string str = indentation + "    ";
                string str2 = str + "    ";
                sb.Length = 0;
                sb.Append(str);
                sb.Append("// Constructor");
                textWriter.WriteLine(sb);
                sb.Length = 0;
                sb.Append(str);
                sb.Append("public ");
                sb.Append(WsdlParser.IsValidCS(this.GetClassName()));
                sb.Append("()");
                textWriter.WriteLine(sb);
                sb.Length = 0;
                sb.Append(str);
                sb.Append('{');
                textWriter.WriteLine(sb);
                if (this._connectURLs != null)
                {
                    for (int i = 0; i < this._connectURLs.Count; i++)
                    {
                        sb.Length = 0;
                        sb.Append(str2);
                        if (i == 0)
                        {
                            sb.Append("base.ConfigureProxy(this.GetType(), ");
                            sb.Append(WsdlParser.IsValidUrl((string) this._connectURLs[i]));
                            sb.Append(");");
                        }
                        else
                        {
                            sb.Append("//base.ConfigureProxy(this.GetType(), ");
                            sb.Append(WsdlParser.IsValidUrl((string) this._connectURLs[i]));
                            sb.Append(");");
                        }
                        textWriter.WriteLine(sb);
                    }
                }
                foreach (WsdlParser.URTNamespace namespace2 in this._parser._URTNamespaces)
                {
                    foreach (WsdlParser.URTComplexType type in namespace2._URTComplexTypes)
                    {
                        if ((type._sudsType != System.Runtime.Remoting.MetadataServices.SUDSType.ClientProxy) && !type.IsArray())
                        {
                            sb.Length = 0;
                            sb.Append(str2);
                            sb.Append("System.Runtime.Remoting.SoapServices.PreLoad(typeof(");
                            sb.Append(WsdlParser.IsValidCS(namespace2.EncodedNS));
                            if ((namespace2.EncodedNS != null) && (namespace2.EncodedNS.Length > 0))
                            {
                                sb.Append(".");
                            }
                            sb.Append(WsdlParser.IsValidCS(type.Name));
                            sb.Append("));");
                            textWriter.WriteLine(sb);
                        }
                    }
                }
                foreach (WsdlParser.URTNamespace namespace3 in this._parser._URTNamespaces)
                {
                    foreach (WsdlParser.URTSimpleType type2 in namespace3._URTSimpleTypes)
                    {
                        if (type2.IsEnum)
                        {
                            sb.Length = 0;
                            sb.Append(str2);
                            sb.Append("System.Runtime.Remoting.SoapServices.PreLoad(typeof(");
                            sb.Append(WsdlParser.IsValidCS(namespace3.EncodedNS));
                            if ((namespace3.EncodedNS != null) && (namespace3.EncodedNS.Length > 0))
                            {
                                sb.Append(".");
                            }
                            sb.Append(WsdlParser.IsValidCS(type2.Name));
                            sb.Append("));");
                            textWriter.WriteLine(sb);
                        }
                    }
                }
                sb.Length = 0;
                sb.Append(str);
                sb.Append('}');
                textWriter.WriteLine(sb);
                textWriter.WriteLine();
                sb.Length = 0;
                sb.Append(str);
                sb.Append("public Object RemotingReference");
                textWriter.WriteLine(sb);
                sb.Length = 0;
                sb.Append(str);
                sb.Append("{");
                textWriter.WriteLine(sb);
                sb.Length = 0;
                sb.Append(str2);
                sb.Append("get{return(_tp);}");
                textWriter.WriteLine(sb);
                sb.Length = 0;
                sb.Append(str);
                sb.Append("}");
                textWriter.WriteLine(sb);
                textWriter.WriteLine();
            }

            internal void PrintCSC(TextWriter textWriter, string indentation, string curNS, StringBuilder sb)
            {
                if (!this.IsEmittableFieldType && (!base.bNestedType || base.bNestedTypePrint))
                {
                    bool flag3;
                    sb.Length = 0;
                    sb.Append(indentation);
                    if (this._baseTypeName != null)
                    {
                        switch (this._baseType.GetName(curNS))
                        {
                            case "System.Delegate":
                            case "System.MulticastDelegate":
                            {
                                sb.Append("public delegate ");
                                WsdlParser.URTMethod method = this.GetMethod("Invoke");
                                if (method == null)
                                {
                                    throw new SUDSParserException(CoreChannel.GetResourceString("Remoting_Suds_DelegateWithoutInvoke"));
                                }
                                string typeString = method.GetTypeString(curNS, true);
                                sb.Append(WsdlParser.IsValidCSAttr(typeString));
                                sb.Append(' ');
                                string name = base.Name;
                                int index = name.IndexOf('.');
                                if (index > 0)
                                {
                                    name = name.Substring(index + 1);
                                }
                                sb.Append(WsdlParser.IsValidCS(name));
                                sb.Append('(');
                                method.PrintSignature(sb, curNS);
                                sb.Append(");");
                                textWriter.WriteLine(sb);
                                return;
                            }
                        }
                    }
                    bool isURTType = base.IsURTType;
                    sb.Length = 0;
                    sb.Append("\n");
                    sb.Append(indentation);
                    sb.Append("[");
                    if (this._sudsType != System.Runtime.Remoting.MetadataServices.SUDSType.ClientProxy)
                    {
                        sb.Append("Serializable, ");
                    }
                    sb.Append("SoapType(");
                    if (this._parser._xsdVersion == XsdVersion.V1999)
                    {
                        sb.Append("SoapOptions=SoapOption.Option1|SoapOption.AlwaysIncludeTypes|SoapOption.XsdString|SoapOption.EmbedAll,");
                    }
                    else if (this._parser._xsdVersion == XsdVersion.V2000)
                    {
                        sb.Append("SoapOptions=SoapOption.Option2|SoapOption.AlwaysIncludeTypes|SoapOption.XsdString|SoapOption.EmbedAll,");
                    }
                    if (!isURTType)
                    {
                        sb.Append("XmlElementName=");
                        sb.Append(WsdlParser.IsValidUrl(this.GetClassName()));
                        sb.Append(", XmlNamespace=");
                        sb.Append(WsdlParser.IsValidUrl(base.Namespace));
                        sb.Append(", XmlTypeName=");
                        if (this.WireType != null)
                        {
                            sb.Append(WsdlParser.IsValidUrl(this.WireType));
                        }
                        else
                        {
                            sb.Append(WsdlParser.IsValidUrl(this.GetClassName()));
                        }
                        sb.Append(", XmlTypeNamespace=");
                        sb.Append(WsdlParser.IsValidUrl(base.Namespace));
                    }
                    else
                    {
                        sb.Append("XmlNamespace=");
                        sb.Append(WsdlParser.IsValidUrl(base.UrlNS));
                        sb.Append(", XmlTypeNamespace=");
                        sb.Append(WsdlParser.IsValidUrl(base.UrlNS));
                        if (this.WireType != null)
                        {
                            sb.Append(", XmlTypeName=");
                            sb.Append(WsdlParser.IsValidUrl(this.WireType));
                        }
                    }
                    sb.Append(")]");
                    sb.Append("[ComVisible(true)]");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(indentation);
                    if (this._sudsUse == System.Runtime.Remoting.MetadataServices.WsdlParser.SudsUse.Struct)
                    {
                        sb.Append("public struct ");
                    }
                    else
                    {
                        sb.Append("public class ");
                    }
                    if (base._bNestedType)
                    {
                        sb.Append(WsdlParser.IsValidCS(base.NestedTypeName));
                    }
                    else
                    {
                        sb.Append(WsdlParser.IsValidCS(this.GetClassName()));
                    }
                    if (((this._baseTypeName != null) || (this._sudsUse == System.Runtime.Remoting.MetadataServices.WsdlParser.SudsUse.ISerializable)) || (this._implIFaces.Count > 0))
                    {
                        sb.Append(" : ");
                    }
                    string identifier = null;
                    bool flag2 = false;
                    if (this._baseTypeName == "RemotingClientProxy")
                    {
                        flag3 = true;
                    }
                    else
                    {
                        flag3 = false;
                    }
                    if (flag3)
                    {
                        sb.Append("System.Runtime.Remoting.Services.RemotingClientProxy");
                        flag2 = true;
                    }
                    else if (this._baseTypeName != null)
                    {
                        bool flag1 = this._baseType.IsURTType;
                        identifier = this._baseType.GetName(curNS);
                        if (identifier == "System.__ComObject")
                        {
                            sb.Append("System.MarshalByRefObject");
                            flag2 = true;
                        }
                        else
                        {
                            sb.Append(WsdlParser.IsValidCSAttr(identifier));
                            flag2 = true;
                        }
                    }
                    else if (this._sudsUse == System.Runtime.Remoting.MetadataServices.WsdlParser.SudsUse.ISerializable)
                    {
                        sb.Append("System.Runtime.Serialization.ISerializable");
                        flag2 = true;
                    }
                    if (this._implIFaces.Count > 0)
                    {
                        for (int i = 0; i < this._implIFaces.Count; i++)
                        {
                            if (flag2)
                            {
                                sb.Append(", ");
                            }
                            sb.Append(WsdlParser.IsValidCS(((WsdlParser.BaseInterface) this._implIFaces[i]).GetName(curNS)));
                            flag2 = true;
                        }
                    }
                    textWriter.WriteLine(sb);
                    textWriter.Write(indentation);
                    textWriter.WriteLine('{');
                    string str5 = indentation + "    ";
                    int length = str5.Length;
                    if (flag3)
                    {
                        this.PrintClientProxy(textWriter, indentation, curNS, sb);
                    }
                    if (this._methods.Count > 0)
                    {
                        string bodyPrefix = null;
                        if (this._parser._bWrappedProxy)
                        {
                            sb.Length = 0;
                            sb.Append("((");
                            sb.Append(WsdlParser.IsValidCS(this.GetClassName()));
                            sb.Append(") _tp).");
                            bodyPrefix = sb.ToString();
                        }
                        for (int j = 0; j < this._methods.Count; j++)
                        {
                            ((WsdlParser.URTMethod) this._methods[j]).PrintCSC(textWriter, str5, " ", curNS, WsdlParser.MethodPrintEnum.PrintBody, isURTType, bodyPrefix, sb);
                        }
                        textWriter.WriteLine();
                    }
                    if (this._fields.Count > 0)
                    {
                        textWriter.Write(str5);
                        textWriter.WriteLine("// Class Fields");
                        for (int k = 0; k < this._fields.Count; k++)
                        {
                            ((WsdlParser.URTField) this._fields[k]).PrintCSC(textWriter, str5, curNS, sb);
                        }
                    }
                    if ((base._nestedTypes != null) && (base._nestedTypes.Count > 0))
                    {
                        foreach (WsdlParser.BaseType type in base._nestedTypes)
                        {
                            type.bNestedTypePrint = true;
                            if (type is WsdlParser.URTSimpleType)
                            {
                                ((WsdlParser.URTSimpleType) type).PrintCSC(textWriter, str5, curNS, sb);
                            }
                            else
                            {
                                ((WsdlParser.URTComplexType) type).PrintCSC(textWriter, str5, curNS, sb);
                            }
                            type.bNestedTypePrint = false;
                        }
                    }
                    if (this._sudsUse == System.Runtime.Remoting.MetadataServices.WsdlParser.SudsUse.ISerializable)
                    {
                        this.PrintISerializable(textWriter, indentation, curNS, sb, identifier);
                    }
                    sb.Length = 0;
                    sb.Append(indentation);
                    sb.Append("}");
                    textWriter.WriteLine(sb);
                }
            }

            private void PrintISerializable(TextWriter textWriter, string indentation, string curNS, StringBuilder sb, string baseString)
            {
                string str = indentation + "    ";
                string str2 = str + "    ";
                if ((baseString == null) || baseString.StartsWith("System.", StringComparison.Ordinal))
                {
                    sb.Length = 0;
                    sb.Append(str);
                    sb.Append("public System.Runtime.Serialization.SerializationInfo info;");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str);
                    sb.Append("public System.Runtime.Serialization.StreamingContext context; \n");
                    textWriter.WriteLine(sb);
                }
                sb.Length = 0;
                sb.Append(str);
                if (this._baseTypeName == null)
                {
                    sb.Append("public ");
                }
                else
                {
                    sb.Append("protected ");
                }
                if (base._bNestedType)
                {
                    sb.Append(WsdlParser.IsValidCS(base.NestedTypeName));
                }
                else
                {
                    sb.Append(WsdlParser.IsValidCS(this.GetClassName()));
                }
                sb.Append("(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)");
                if (this._baseTypeName != null)
                {
                    sb.Append(" : base(info, context)");
                }
                textWriter.WriteLine(sb);
                sb.Length = 0;
                sb.Append(str);
                sb.Append("{");
                textWriter.WriteLine(sb);
                if ((baseString == null) || baseString.StartsWith("System.", StringComparison.Ordinal))
                {
                    sb.Length = 0;
                    sb.Append(str2);
                    sb.Append("this.info = info;");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str2);
                    sb.Append("this.context = context;");
                    textWriter.WriteLine(sb);
                }
                sb.Length = 0;
                sb.Append(str);
                sb.Append("}");
                textWriter.WriteLine(sb);
                if (this._baseTypeName == null)
                {
                    sb.Length = 0;
                    sb.Append(str);
                    sb.Append("public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str);
                    sb.Append("{");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str);
                    sb.Append("}");
                    textWriter.WriteLine(sb);
                }
            }

            internal void ResolveArray()
            {
                if (this._clrarray == null)
                {
                    string str = null;
                    string xsdType = this._arrayType;
                    int index = this._arrayType.IndexOf("[");
                    if (index < 0)
                    {
                        throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_WsdlInvalidArraySyntax"), new object[] { this._arrayType }));
                    }
                    xsdType = this._arrayType.Substring(0, index);
                    switch (this._arrayNS.UrtType)
                    {
                        case UrtType.Interop:
                            str = xsdType;
                            break;

                        case UrtType.UrtSystem:
                        case UrtType.UrtUser:
                            str = xsdType;
                            break;

                        case UrtType.Xsd:
                            str = this._parser.MapSchemaTypesToCSharpTypes(xsdType);
                            break;
                    }
                    this._clrarray = str + this.FilterDimensions(this._arrayType.Substring(index));
                }
            }

            internal void ResolveMethods()
            {
                for (int i = 0; i < this._methods.Count; i++)
                {
                    WsdlParser.URTMethod method1 = (WsdlParser.URTMethod) this._methods[i];
                }
            }

            internal void ResolveTypes(WsdlParser parser)
            {
                string ns = null;
                string assemName = null;
                if (this.IsArray())
                {
                    this.ResolveArray();
                }
                else
                {
                    if (this.IsSUDSType && (this._sudsType == System.Runtime.Remoting.MetadataServices.SUDSType.None))
                    {
                        if (this._parser._bWrappedProxy)
                        {
                            this._sudsType = System.Runtime.Remoting.MetadataServices.SUDSType.ClientProxy;
                        }
                        else
                        {
                            this._sudsType = System.Runtime.Remoting.MetadataServices.SUDSType.MarshalByRef;
                        }
                    }
                    if (this._baseTypeName != null)
                    {
                        if ((parser.IsURTExportedType(this._baseTypeXmlNS, out ns, out assemName) == UrtType.UrtSystem) || ns.StartsWith("System", StringComparison.Ordinal))
                        {
                            this._baseType = new WsdlParser.SystemType(this._baseTypeName, this._baseTypeXmlNS, ns, assemName);
                        }
                        else
                        {
                            WsdlParser.URTNamespace namespace2 = parser.LookupNamespace(this._baseTypeXmlNS);
                            if (namespace2 == null)
                            {
                                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveSchemaNS"), new object[] { this._baseTypeXmlNS, this._baseTypeName }));
                            }
                            this._baseType = namespace2.LookupComplexType(this._baseTypeName);
                            if (this._baseType == null)
                            {
                                this._baseType = new WsdlParser.SystemType(this._baseTypeName, this._baseTypeXmlNS, ns, assemName);
                            }
                        }
                    }
                    if (this.IsSUDSType)
                    {
                        if (this._parser._bWrappedProxy)
                        {
                            if ((this._baseTypeName == null) || (this._baseType is WsdlParser.SystemType))
                            {
                                this._baseTypeName = "RemotingClientProxy";
                                this._baseTypeXmlNS = SoapServices.CodeXmlNamespaceForClrTypeNamespace("System.Runtime.Remoting", "System.Runtime.Remoting");
                                ns = "System.Runtime.Remoting.Services";
                                assemName = "System.Runtime.Remoting";
                                this._baseType = new WsdlParser.SystemType(this._baseTypeName, this._baseTypeXmlNS, ns, assemName);
                            }
                        }
                        else if (this._baseTypeName == null)
                        {
                            this._baseTypeName = "MarshalByRefObject";
                            this._baseTypeXmlNS = SoapServices.CodeXmlNamespaceForClrTypeNamespace("System", null);
                            ns = "System";
                            assemName = null;
                            this._baseType = new WsdlParser.SystemType(this._baseTypeName, this._baseTypeXmlNS, ns, assemName);
                        }
                    }
                    else if (this._baseType == null)
                    {
                        this._baseType = new WsdlParser.SystemType("Object", SoapServices.CodeXmlNamespaceForClrTypeNamespace("System", null), "System", null);
                    }
                    for (int i = 0; i < this._implIFaceNames.Count; i += 2)
                    {
                        string str5;
                        string str6;
                        WsdlParser.BaseInterface interface2;
                        string name = (string) this._implIFaceNames[i];
                        string str4 = (string) this._implIFaceNames[i + 1];
                        if (parser.IsURTExportedType(str4, out str5, out str6) == UrtType.UrtSystem)
                        {
                            interface2 = new WsdlParser.SystemInterface(name, str4, str5, parser, str6);
                        }
                        else
                        {
                            WsdlParser.URTNamespace namespace3 = parser.LookupNamespace(str4);
                            if (namespace3 == null)
                            {
                                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveSchemaNS"), new object[] { str4, name }));
                            }
                            interface2 = namespace3.LookupInterface(name);
                            if (interface2 == null)
                            {
                                throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveTypeInNS"), new object[] { name, str4 }));
                            }
                        }
                        this._implIFaces.Add(interface2);
                    }
                    for (int j = 0; j < this._methods.Count; j++)
                    {
                        ((WsdlParser.URTMethod) this._methods[j]).ResolveTypes(parser);
                    }
                }
            }

            internal SchemaBlockType BlockType
            {
                set
                {
                    this._blockType = value;
                }
            }

            internal ArrayList ConnectURLs
            {
                set
                {
                    this._connectURLs = value;
                }
            }

            internal override string FieldName
            {
                get
                {
                    return this._fieldString;
                }
            }

            internal override string FieldNamespace
            {
                get
                {
                    return ((WsdlParser.URTField) this._fields[0]).TypeNS;
                }
            }

            internal ArrayList Fields
            {
                get
                {
                    return this._fields;
                }
            }

            internal ArrayList Inherit
            {
                get
                {
                    return this._inherit;
                }
                set
                {
                    this._inherit = value;
                }
            }

            internal override bool IsEmittableFieldType
            {
                get
                {
                    if (this._fieldString == null)
                    {
                        if (this._bAnonymous && (this._fields.Count == 1))
                        {
                            WsdlParser.URTField field = (WsdlParser.URTField) this._fields[0];
                            if (field.IsArray)
                            {
                                this._fieldString = field.TypeName;
                                return true;
                            }
                        }
                        this._fieldString = string.Empty;
                    }
                    return (this._fieldString != string.Empty);
                }
            }

            internal bool IsPrint
            {
                get
                {
                    return this._bprint;
                }
                set
                {
                    this._bprint = value;
                }
            }

            internal bool IsStruct
            {
                set
                {
                    this._bStruct = value;
                }
            }

            internal bool IsSUDSType
            {
                get
                {
                    return this._bSUDSType;
                }
                set
                {
                    this._bSUDSType = value;
                    this._bStruct = !value;
                }
            }

            internal bool IsValueType
            {
                set
                {
                    this._bValueType = value;
                }
            }

            internal override bool PrimitiveField
            {
                get
                {
                    return ((WsdlParser.URTField) this._fields[0]).IsPrimitive;
                }
            }

            internal System.Runtime.Remoting.MetadataServices.SUDSType SUDSType
            {
                get
                {
                    return this._sudsType;
                }
                set
                {
                    this._sudsType = value;
                }
            }

            internal System.Runtime.Remoting.MetadataServices.WsdlParser.SudsUse SudsUse
            {
                get
                {
                    return this._sudsUse;
                }
                set
                {
                    this._sudsUse = value;
                }
            }

            internal string WireType
            {
                get
                {
                    return this._wireType;
                }
            }
        }

        internal class URTField
        {
            private bool _arrayField;
            private string _arraySize;
            private bool _attributeField;
            private bool _embeddedField;
            private string _encodedNS;
            private string _name;
            private bool _optionalField;
            private WsdlParser _parser;
            private bool _primitiveField;
            private string _typeName;
            private string _typeNS;
            private WsdlParser.URTNamespace _urtNamespace;

            internal URTField(string name, string typeName, string xmlNS, WsdlParser parser, bool bPrimitive, bool bEmbedded, bool bAttribute, bool bOptional, bool bArray, string arraySize, WsdlParser.URTNamespace urtNamespace)
            {
                string str;
                this._name = name;
                this._typeName = typeName;
                this._parser = parser;
                if (parser.IsURTExportedType(xmlNS, out this._typeNS, out str) == UrtType.Interop)
                {
                    this._encodedNS = urtNamespace.EncodedNS;
                }
                else
                {
                    this._encodedNS = this._typeNS;
                }
                this._primitiveField = bPrimitive;
                this._embeddedField = bEmbedded;
                this._attributeField = bAttribute;
                this._optionalField = bOptional;
                this._arrayField = bArray;
                this._arraySize = arraySize;
                this._urtNamespace = urtNamespace;
            }

            internal string GetTypeString(string curNS, bool bNS)
            {
                return this._parser.GetTypeString(curNS, bNS, this._urtNamespace, this.TypeName, this._typeNS);
            }

            internal void PrintCSC(TextWriter textWriter, string indentation, string curNS, StringBuilder sb)
            {
                if (this._embeddedField)
                {
                    textWriter.Write(indentation);
                    textWriter.WriteLine("[SoapField(Embedded=true)]");
                }
                sb.Length = 0;
                sb.Append(indentation);
                sb.Append("public ");
                sb.Append(WsdlParser.IsValidCSAttr(this.GetTypeString(curNS, true)));
                sb.Append(' ');
                sb.Append(WsdlParser.IsValidCS(this._name));
                sb.Append(';');
                textWriter.WriteLine(sb);
            }

            internal bool IsArray
            {
                get
                {
                    return this._arrayField;
                }
            }

            internal bool IsPrimitive
            {
                get
                {
                    return this._primitiveField;
                }
            }

            internal string TypeName
            {
                get
                {
                    if (this._arrayField)
                    {
                        return (this._typeName + "[]");
                    }
                    return this._typeName;
                }
            }

            internal string TypeNS
            {
                get
                {
                    return this._typeNS;
                }
            }
        }

        internal class URTInterface : WsdlParser.BaseInterface
        {
            private ArrayList _baseIFaceNames;
            private ArrayList _baseIFaces;
            private ArrayList _extendsInterface;
            private ArrayList _methods;
            private WsdlParser _parser;

            internal URTInterface(string name, string urlNS, string ns, string encodedNS, WsdlParser parser) : base(name, urlNS, ns, encodedNS, parser)
            {
                this._baseIFaces = new ArrayList();
                this._baseIFaceNames = new ArrayList();
                this._extendsInterface = new ArrayList();
                this._methods = new ArrayList();
                this._parser = parser;
            }

            internal void AddMethod(WsdlParser.URTMethod method)
            {
                this._methods.Add(method);
                method.MethodFlags = WsdlParser.MethodFlags.None;
            }

            private void CheckIfNewNeeded(WsdlParser.URTMethod method)
            {
                foreach (WsdlParser.URTMethod method2 in this._methods)
                {
                    if (method2.Name == method.Name)
                    {
                        method.MethodFlags |= WsdlParser.MethodFlags.New;
                        break;
                    }
                }
                if (WsdlParser.URTMethod.MethodFlagsTest(method.MethodFlags, WsdlParser.MethodFlags.New))
                {
                    this.NewNeeded(method);
                }
            }

            internal void Extends(string baseName, string baseNS, WsdlParser parser)
            {
                this._baseIFaceNames.Add(baseName);
                this._baseIFaceNames.Add(baseNS);
                WsdlParser.URTNamespace namespace2 = parser.AddNewNamespace(baseNS);
                WsdlParser.URTInterface iface = namespace2.LookupInterface(baseName);
                if (iface == null)
                {
                    iface = new WsdlParser.URTInterface(baseName, namespace2.Name, namespace2.Namespace, namespace2.EncodedNS, parser);
                    namespace2.AddInterface(iface);
                }
                this._extendsInterface.Add(iface);
            }

            internal void NewNeeded(WsdlParser.URTMethod method)
            {
                foreach (WsdlParser.URTInterface interface2 in this._extendsInterface)
                {
                    interface2.CheckIfNewNeeded(method);
                    if (WsdlParser.URTMethod.MethodFlagsTest(method.MethodFlags, WsdlParser.MethodFlags.New))
                    {
                        break;
                    }
                }
            }

            internal override void PrintClassMethods(TextWriter textWriter, string indentation, string curNS, ArrayList printedIFaces, bool bProxy, StringBuilder sb)
            {
                for (int i = 0; i < printedIFaces.Count; i++)
                {
                    if (printedIFaces[i] == this)
                    {
                        return;
                    }
                }
                printedIFaces.Add(this);
                sb.Length = 0;
                sb.Append(indentation);
                if (this._methods.Count > 0)
                {
                    sb.Append("// ");
                    sb.Append(WsdlParser.IsValidCS(base.Name));
                    sb.Append(" interface Methods");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(' ');
                    string name = base.GetName(curNS);
                    sb.Append(WsdlParser.IsValidCS(name));
                    sb.Append('.');
                    string namePrefix = sb.ToString();
                    string bodyPrefix = null;
                    if (bProxy)
                    {
                        sb.Length = 0;
                        sb.Append("((");
                        sb.Append(WsdlParser.IsValidCS(name));
                        sb.Append(") _tp).");
                        bodyPrefix = sb.ToString();
                    }
                    WsdlParser.MethodPrintEnum methodPrintEnum = WsdlParser.MethodPrintEnum.InterfaceInClass | WsdlParser.MethodPrintEnum.PrintBody;
                    for (int k = 0; k < this._methods.Count; k++)
                    {
                        ((WsdlParser.URTMethod) this._methods[k]).PrintCSC(textWriter, indentation, namePrefix, curNS, methodPrintEnum, true, bodyPrefix, sb);
                    }
                }
                for (int j = 0; j < this._baseIFaces.Count; j++)
                {
                    ((WsdlParser.BaseInterface) this._baseIFaces[j]).PrintClassMethods(textWriter, indentation, curNS, printedIFaces, bProxy, sb);
                }
            }

            internal void PrintCSC(TextWriter textWriter, string indentation, string curNS, StringBuilder sb)
            {
                bool isURTInterface = base.IsURTInterface;
                sb.Length = 0;
                sb.Append("\n");
                sb.Append(indentation);
                sb.Append("[SoapType(");
                if (this._parser._xsdVersion == XsdVersion.V1999)
                {
                    sb.Append("SoapOptions=SoapOption.Option1|SoapOption.AlwaysIncludeTypes|SoapOption.XsdString|SoapOption.EmbedAll,");
                }
                else if (this._parser._xsdVersion == XsdVersion.V2000)
                {
                    sb.Append("SoapOptions=SoapOption.Option2|SoapOption.AlwaysIncludeTypes|SoapOption.XsdString|SoapOption.EmbedAll,");
                }
                if (!isURTInterface)
                {
                    sb.Append("XmlElementName=");
                    sb.Append(WsdlParser.IsValidUrl(base.Name));
                    sb.Append(", XmlNamespace=");
                    sb.Append(WsdlParser.IsValidUrl(base.Namespace));
                    sb.Append(", XmlTypeName=");
                    sb.Append(WsdlParser.IsValidUrl(base.Name));
                    sb.Append(", XmlTypeNamespace=");
                    sb.Append(WsdlParser.IsValidUrl(base.Namespace));
                }
                else
                {
                    sb.Append("XmlNamespace=");
                    sb.Append(WsdlParser.IsValidUrl(base.UrlNS));
                    sb.Append(", XmlTypeNamespace=");
                    sb.Append(WsdlParser.IsValidUrl(base.UrlNS));
                }
                sb.Append(")]");
                sb.Append("[ComVisible(true)]");
                textWriter.WriteLine(sb);
                sb.Length = 0;
                sb.Append(indentation);
                sb.Append("public interface ");
                sb.Append(WsdlParser.IsValidCS(base.Name));
                if (this._baseIFaces.Count > 0)
                {
                    sb.Append(" : ");
                }
                if (this._baseIFaces.Count > 0)
                {
                    sb.Append(WsdlParser.IsValidCSAttr(((WsdlParser.BaseInterface) this._baseIFaces[0]).GetName(curNS)));
                    for (int j = 1; j < this._baseIFaces.Count; j++)
                    {
                        sb.Append(", ");
                        sb.Append(WsdlParser.IsValidCSAttr(((WsdlParser.BaseInterface) this._baseIFaces[j]).GetName(curNS)));
                    }
                }
                textWriter.WriteLine(sb);
                textWriter.Write(indentation);
                textWriter.WriteLine('{');
                string str = indentation + "    ";
                string namePrefix = " ";
                for (int i = 0; i < this._methods.Count; i++)
                {
                    this.NewNeeded((WsdlParser.URTMethod) this._methods[i]);
                    ((WsdlParser.URTMethod) this._methods[i]).PrintCSC(textWriter, str, namePrefix, curNS, WsdlParser.MethodPrintEnum.InterfaceMethods, isURTInterface, null, sb);
                }
                textWriter.Write(indentation);
                textWriter.WriteLine('}');
            }

            internal void ResolveTypes(WsdlParser parser)
            {
                for (int i = 0; i < this._baseIFaceNames.Count; i += 2)
                {
                    string str3;
                    string str4;
                    WsdlParser.BaseInterface interface2;
                    string name = (string) this._baseIFaceNames[i];
                    string str2 = (string) this._baseIFaceNames[i + 1];
                    if ((parser.IsURTExportedType(str2, out str3, out str4) != UrtType.Interop) && str3.StartsWith("System", StringComparison.Ordinal))
                    {
                        interface2 = new WsdlParser.SystemInterface(name, str2, str3, this._parser, str4);
                    }
                    else
                    {
                        WsdlParser.URTNamespace namespace2 = parser.LookupNamespace(str2);
                        if (namespace2 == null)
                        {
                            throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveSchemaNS"), new object[] { str2, name }));
                        }
                        interface2 = namespace2.LookupInterface(name);
                        if (interface2 == null)
                        {
                            throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveTypeInNS"), new object[] { name, str2 }));
                        }
                    }
                    this._baseIFaces.Add(interface2);
                }
                for (int j = 0; j < this._methods.Count; j++)
                {
                    ((WsdlParser.URTMethod) this._methods[j]).ResolveTypes(parser);
                }
            }
        }

        internal abstract class URTMethod
        {
            internal WsdlParser.URTComplexType _complexType;
            private System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags _methodFlags;
            private string _methodName;
            private WsdlParser.URTParam _methodType;
            protected string[] _paramNamesOrder;
            protected ArrayList _paramPosition = new ArrayList();
            protected ArrayList _params = new ArrayList();
            private string _soapAction;
            private WsdlParser.WsdlMethodInfo _wsdlMethodInfo;

            internal URTMethod(string name, string soapAction, string methodAttributes, WsdlParser.URTComplexType complexType)
            {
                this._methodName = name;
                this._soapAction = soapAction;
                this._methodType = null;
                this._complexType = complexType;
                name.IndexOf('.');
                this._methodFlags = System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.None;
                if ((methodAttributes != null) && (methodAttributes.Length > 0))
                {
                    foreach (string str in methodAttributes.Split(new char[] { ' ' }))
                    {
                        switch (str)
                        {
                            case "virtual":
                                this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Virtual;
                                break;

                            case "new":
                                this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.New;
                                break;

                            case "override":
                                this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Override;
                                break;

                            case "public":
                                this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Public;
                                break;

                            case "protected":
                                this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Protected;
                                break;

                            case "internal":
                                this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Internal;
                                break;
                        }
                    }
                }
            }

            internal void AddParam(WsdlParser.URTParam newParam)
            {
                for (int i = 0; i < this._params.Count; i++)
                {
                    WsdlParser.URTParam param = (WsdlParser.URTParam) this._params[i];
                    if (WsdlParser.MatchingStrings(param.Name, newParam.Name))
                    {
                        if (((param.ParamType != WsdlParser.URTParamType.IN) || (newParam.ParamType != WsdlParser.URTParamType.OUT)) || (!WsdlParser.MatchingStrings(param.TypeName, newParam.TypeName) || !WsdlParser.MatchingStrings(param.TypeNS, newParam.TypeNS)))
                        {
                            throw new SUDSParserException(CoreChannel.GetResourceString("Remoting_Suds_DuplicateParameter"));
                        }
                        param.ParamType = WsdlParser.URTParamType.REF;
                        return;
                    }
                }
                int num2 = -1;
                if (this._paramNamesOrder == null)
                {
                    if ((this._methodType == null) && (newParam.ParamType == WsdlParser.URTParamType.OUT))
                    {
                        this._methodType = newParam;
                    }
                    else
                    {
                        this._params.Add(newParam);
                    }
                }
                else
                {
                    for (int j = 0; j < this._paramNamesOrder.Length; j++)
                    {
                        if (this._paramNamesOrder[j] == newParam.Name)
                        {
                            num2 = j;
                            break;
                        }
                    }
                    if (num2 == -1)
                    {
                        this._methodType = newParam;
                    }
                    else
                    {
                        this._params.Add(newParam);
                        this._paramPosition.Add(num2);
                    }
                }
            }

            public override bool Equals(object obj)
            {
                WsdlParser.URTMethod method = (WsdlParser.URTMethod) obj;
                if (!WsdlParser.MatchingStrings(this._methodName, method._methodName) || (this._params.Count != method._params.Count))
                {
                    return false;
                }
                for (int i = 0; i < this._params.Count; i++)
                {
                    if (!this._params[i].Equals(method._params[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            private void FindMethodAttributes()
            {
                ArrayList inherit;
                Type baseType;
                BindingFlags flags;
                if (this._complexType != null)
                {
                    inherit = this._complexType.Inherit;
                    baseType = null;
                    if (inherit != null)
                    {
                        goto Label_00A0;
                    }
                    inherit = new ArrayList();
                    if (this._complexType.SUDSType == SUDSType.ClientProxy)
                    {
                        baseType = typeof(RemotingClientProxy);
                    }
                    else if (this._complexType.SudsUse == WsdlParser.SudsUse.MarshalByRef)
                    {
                        baseType = typeof(MarshalByRefObject);
                    }
                    else if (this._complexType.SudsUse == WsdlParser.SudsUse.ServicedComponent)
                    {
                        baseType = typeof(MarshalByRefObject);
                    }
                    if (baseType != null)
                    {
                        while (baseType != null)
                        {
                            inherit.Add(baseType);
                            baseType = baseType.BaseType;
                        }
                        this._complexType.Inherit = inherit;
                        goto Label_00A0;
                    }
                }
                return;
            Label_00A0:
                flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                bool flag = MethodFlagsTest(this._methodFlags, System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Virtual);
                bool flag2 = false;
                for (int i = 0; i < inherit.Count; i++)
                {
                    baseType = (Type) inherit[i];
                    MethodInfo[] methods = null;
                    try
                    {
                        MethodInfo method = baseType.GetMethod(this.Name, flags);
                        if (method != null)
                        {
                            methods = new MethodInfo[] { method };
                        }
                    }
                    catch
                    {
                        methods = baseType.GetMethods(flags);
                    }
                    if (methods != null)
                    {
                        MethodInfo[] infoArray2 = methods;
                        for (int j = 0; j < infoArray2.Length; j++)
                        {
                            MethodBase baseInfo = infoArray2[j];
                            if ((((baseInfo != null) && (baseInfo.Name == this.Name)) && ((baseInfo.IsFamily || baseInfo.IsPublic) || baseInfo.IsAssembly)) && this.IsSignature(baseInfo))
                            {
                                flag2 = true;
                                if (!baseInfo.IsPublic)
                                {
                                    if (baseInfo.IsAssembly)
                                    {
                                        this._methodFlags &= ~System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Public;
                                        this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Internal;
                                    }
                                    else if (baseInfo.IsFamily)
                                    {
                                        this._methodFlags &= ~System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Public;
                                        this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Protected;
                                    }
                                }
                                if (baseInfo.IsFinal)
                                {
                                    this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.New;
                                }
                                else if (baseInfo.IsVirtual && flag)
                                {
                                    this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Override;
                                }
                                else
                                {
                                    this._methodFlags |= System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.New;
                                }
                                break;
                            }
                        }
                    }
                    if (flag2)
                    {
                        return;
                    }
                }
            }

            internal static bool FlagTest(WsdlParser.MethodPrintEnum flag, WsdlParser.MethodPrintEnum target)
            {
                return ((flag & target) == target);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            internal System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags GetMethodFlags(MethodInfo method)
            {
                return this._methodFlags;
            }

            internal string GetTypeString(string curNS, bool bNS)
            {
                if (this._methodType == null)
                {
                    return "void";
                }
                return this._methodType.GetTypeString(curNS, bNS);
            }

            private bool IsSignature(MethodBase baseInfo)
            {
                ParameterInfo[] parameters = baseInfo.GetParameters();
                if (this._params.Count != parameters.Length)
                {
                    return false;
                }
                for (int i = 0; i < parameters.Length; i++)
                {
                    WsdlParser.URTParam param = (WsdlParser.URTParam) this._params[i];
                    if (param.GetTypeString(null, true) != parameters[i].ParameterType.FullName)
                    {
                        return false;
                    }
                }
                return true;
            }

            internal static bool MethodFlagsTest(System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags flag, System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags target)
            {
                return ((flag & target) == target);
            }

            internal virtual void PrintCSC(TextWriter textWriter, string indentation, string namePrefix, string curNS, WsdlParser.MethodPrintEnum methodPrintEnum, bool bURTType, string bodyPrefix, StringBuilder sb)
            {
                sb.Length = 0;
                sb.Append(indentation);
                if (this.Name != "Finalize")
                {
                    if (FlagTest(methodPrintEnum, WsdlParser.MethodPrintEnum.InterfaceInClass))
                    {
                        sb.Append("public ");
                    }
                    else if (MethodFlagsTest(this._methodFlags, System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Public))
                    {
                        sb.Append("public ");
                    }
                    else if (MethodFlagsTest(this._methodFlags, System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Protected))
                    {
                        sb.Append("protected ");
                    }
                    else if (MethodFlagsTest(this._methodFlags, System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Internal))
                    {
                        sb.Append("internal ");
                    }
                    if (MethodFlagsTest(this._methodFlags, System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Override))
                    {
                        sb.Append("override ");
                    }
                    else if (MethodFlagsTest(this._methodFlags, System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Virtual))
                    {
                        sb.Append("virtual ");
                    }
                    if (MethodFlagsTest(this._methodFlags, System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.New))
                    {
                        sb.Append("new ");
                    }
                    sb.Append(WsdlParser.IsValidCSAttr(this.GetTypeString(curNS, true)));
                    if (FlagTest(methodPrintEnum, WsdlParser.MethodPrintEnum.InterfaceInClass))
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(WsdlParser.IsValidCSAttr(namePrefix));
                    }
                    if (this._wsdlMethodInfo.bProperty)
                    {
                        sb.Append(WsdlParser.IsValidCS(this._wsdlMethodInfo.propertyName));
                    }
                    else
                    {
                        sb.Append(WsdlParser.IsValidCS(this._methodName));
                        sb.Append('(');
                        if (this._params.Count > 0)
                        {
                            ((WsdlParser.URTParam) this._params[0]).PrintCSC(sb, curNS);
                            for (int i = 1; i < this._params.Count; i++)
                            {
                                sb.Append(", ");
                                ((WsdlParser.URTParam) this._params[i]).PrintCSC(sb, curNS);
                            }
                        }
                        sb.Append(')');
                    }
                    if (this._wsdlMethodInfo.bProperty && FlagTest(methodPrintEnum, WsdlParser.MethodPrintEnum.InterfaceMethods))
                    {
                        sb.Append("{");
                        if (this._wsdlMethodInfo.bGet)
                        {
                            sb.Append(" get; ");
                        }
                        if (this._wsdlMethodInfo.bSet)
                        {
                            sb.Append(" set; ");
                        }
                        sb.Append("}");
                    }
                    else if (!FlagTest(methodPrintEnum, WsdlParser.MethodPrintEnum.PrintBody))
                    {
                        sb.Append(';');
                    }
                    textWriter.WriteLine(sb);
                    if (this._wsdlMethodInfo.bProperty && FlagTest(methodPrintEnum, WsdlParser.MethodPrintEnum.PrintBody))
                    {
                        this.PrintPropertyBody(textWriter, indentation, sb, bodyPrefix);
                    }
                    else if (FlagTest(methodPrintEnum, WsdlParser.MethodPrintEnum.PrintBody))
                    {
                        sb.Length = 0;
                        sb.Append(indentation);
                        sb.Append('{');
                        textWriter.WriteLine(sb);
                        string str = indentation + "    ";
                        if (bodyPrefix == null)
                        {
                            for (int j = 0; j < this._params.Count; j++)
                            {
                                WsdlParser.URTParam param = (WsdlParser.URTParam) this._params[j];
                                if (param.ParamType == WsdlParser.URTParamType.OUT)
                                {
                                    sb.Length = 0;
                                    sb.Append(str);
                                    sb.Append(WsdlParser.IsValidCS(param.Name));
                                    sb.Append(" = ");
                                    sb.Append(ValueString(param.GetTypeString(curNS, true)));
                                    sb.Append(';');
                                    textWriter.WriteLine(sb);
                                }
                            }
                            sb.Length = 0;
                            sb.Append(str);
                            sb.Append("return");
                            string str2 = ValueString(this.GetTypeString(curNS, true));
                            if (str2 != null)
                            {
                                sb.Append('(');
                                sb.Append(str2);
                                sb.Append(')');
                            }
                            sb.Append(';');
                        }
                        else
                        {
                            sb.Length = 0;
                            sb.Append(str);
                            if (ValueString(this.GetTypeString(curNS, true)) != null)
                            {
                                sb.Append("return ");
                            }
                            this.PrintMethodName(sb, bodyPrefix, this._methodName);
                            sb.Append('(');
                            if (this._params.Count > 0)
                            {
                                ((WsdlParser.URTParam) this._params[0]).PrintCSC(sb);
                                for (int k = 1; k < this._params.Count; k++)
                                {
                                    sb.Append(", ");
                                    ((WsdlParser.URTParam) this._params[k]).PrintCSC(sb);
                                }
                            }
                            sb.Append(");");
                        }
                        textWriter.WriteLine(sb);
                        textWriter.Write(indentation);
                        textWriter.WriteLine('}');
                    }
                }
            }

            private void PrintMethodName(StringBuilder sb, string bodyPrefix, string name)
            {
                int length = name.LastIndexOf('.');
                if (length < 0)
                {
                    sb.Append(bodyPrefix);
                    sb.Append(WsdlParser.IsValidCS(name));
                }
                else
                {
                    string identifier = name.Substring(0, length);
                    string str2 = name.Substring(length + 1);
                    if (bodyPrefix == null)
                    {
                        sb.Append("(");
                        sb.Append(WsdlParser.IsValidCS(identifier));
                        sb.Append(")");
                        sb.Append(WsdlParser.IsValidCS(str2));
                    }
                    else
                    {
                        sb.Append("((");
                        sb.Append(WsdlParser.IsValidCS(identifier));
                        sb.Append(") _tp).");
                        sb.Append(WsdlParser.IsValidCS(str2));
                    }
                }
            }

            private void PrintPropertyBody(TextWriter textWriter, string indentation, StringBuilder sb, string bodyPrefix)
            {
                sb.Length = 0;
                sb.Append(indentation);
                sb.Append('{');
                textWriter.WriteLine(sb);
                string str = indentation + "    ";
                sb.Length = 0;
                sb.Append(str);
                if (this._wsdlMethodInfo.bGet)
                {
                    sb.Length = 0;
                    sb.Append(str);
                    this.PrintSoapAction(this._wsdlMethodInfo.soapActionGet, sb);
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str);
                    sb.Append("get{return ");
                    this.PrintMethodName(sb, bodyPrefix, this._wsdlMethodInfo.propertyName);
                    sb.Append(";}");
                    textWriter.WriteLine(sb);
                }
                if (this._wsdlMethodInfo.bSet)
                {
                    if (this._wsdlMethodInfo.bGet)
                    {
                        textWriter.WriteLine();
                    }
                    sb.Length = 0;
                    sb.Append(str);
                    this.PrintSoapAction(this._wsdlMethodInfo.soapActionSet, sb);
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str);
                    sb.Append("set{");
                    this.PrintMethodName(sb, bodyPrefix, this._wsdlMethodInfo.propertyName);
                    sb.Append("= value;}");
                    textWriter.WriteLine(sb);
                }
                sb.Length = 0;
                sb.Append(indentation);
                sb.Append('}');
                textWriter.WriteLine(sb);
            }

            internal void PrintSignature(StringBuilder sb, string curNS)
            {
                for (int i = 0; i < this._params.Count; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(", ");
                    }
                    ((WsdlParser.URTParam) this._params[i]).PrintCSC(sb, curNS);
                }
            }

            private void PrintSoapAction(string action, StringBuilder sb)
            {
                sb.Append("[SoapMethod(SoapAction=");
                sb.Append(WsdlParser.IsValidUrl(action));
                sb.Append(")]");
            }

            internal void ResolveMethodAttributes()
            {
                if (!MethodFlagsTest(this._methodFlags, System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.Override) && !MethodFlagsTest(this._methodFlags, System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags.New))
                {
                    this.FindMethodAttributes();
                }
            }

            internal abstract void ResolveTypes(WsdlParser parser);
            protected void ResolveWsdlParams(WsdlParser parser, string targetNS, string targetName, bool bRequest, WsdlParser.WsdlMethodInfo wsdlMethodInfo)
            {
                int length;
                this._wsdlMethodInfo = wsdlMethodInfo;
                this._paramNamesOrder = this._wsdlMethodInfo.paramNamesOrder;
                if (this._wsdlMethodInfo.bProperty)
                {
                    length = 1;
                }
                else if (bRequest)
                {
                    length = wsdlMethodInfo.inputNames.Length;
                }
                else
                {
                    length = wsdlMethodInfo.outputNames.Length;
                }
                for (int i = 0; i < length; i++)
                {
                    WsdlParser.URTParamType oUT;
                    string str6;
                    string str7;
                    string str = null;
                    string str2 = null;
                    string name = null;
                    string propertyType = null;
                    string propertyNs = null;
                    if (this._wsdlMethodInfo.bProperty)
                    {
                        propertyType = wsdlMethodInfo.propertyType;
                        propertyNs = wsdlMethodInfo.propertyNs;
                        oUT = WsdlParser.URTParamType.OUT;
                    }
                    else if (bRequest && !this._wsdlMethodInfo.bProperty)
                    {
                        str = wsdlMethodInfo.inputElements[i];
                        str2 = wsdlMethodInfo.inputElementsNs[i];
                        name = wsdlMethodInfo.inputNames[i];
                        string text1 = wsdlMethodInfo.inputNamesNs[i];
                        propertyType = wsdlMethodInfo.inputTypes[i];
                        propertyNs = wsdlMethodInfo.inputTypesNs[i];
                        oUT = WsdlParser.URTParamType.IN;
                    }
                    else
                    {
                        str = wsdlMethodInfo.outputElements[i];
                        str2 = wsdlMethodInfo.outputElementsNs[i];
                        name = wsdlMethodInfo.outputNames[i];
                        string text2 = wsdlMethodInfo.outputNamesNs[i];
                        propertyType = wsdlMethodInfo.outputTypes[i];
                        propertyNs = wsdlMethodInfo.outputTypesNs[i];
                        oUT = WsdlParser.URTParamType.OUT;
                    }
                    if ((str == null) || (str.Length == 0))
                    {
                        str6 = propertyType;
                        str7 = propertyNs;
                    }
                    else
                    {
                        str6 = str;
                        str7 = str2;
                    }
                    WsdlParser.URTNamespace urtNamespace = parser.LookupNamespace(str7);
                    if (urtNamespace == null)
                    {
                        throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_CantResolveSchemaNS"), new object[] { str7, str6 }));
                    }
                    WsdlParser.URTComplexType complexType = urtNamespace.LookupComplexType(str6);
                    if ((complexType != null) && complexType.IsArray())
                    {
                        if (complexType.GetArray() == null)
                        {
                            complexType.ResolveArray();
                        }
                        string array = complexType.GetArray();
                        WsdlParser.URTNamespace arrayNS = complexType.GetArrayNS();
                        this.AddParam(new WsdlParser.URTParam(name, array, arrayNS.Name, arrayNS.EncodedNS, oUT, true, parser, arrayNS));
                    }
                    else if (urtNamespace.UrtType == UrtType.Xsd)
                    {
                        string typeName = parser.MapSchemaTypesToCSharpTypes(str6);
                        this.AddParam(new WsdlParser.URTParam(name, typeName, urtNamespace.Namespace, urtNamespace.EncodedNS, oUT, true, parser, urtNamespace));
                    }
                    else
                    {
                        string str10 = null;
                        if (complexType != null)
                        {
                            str10 = complexType.Name;
                        }
                        else
                        {
                            WsdlParser.URTSimpleType simpleType = urtNamespace.LookupSimpleType(str6);
                            if (simpleType != null)
                            {
                                str10 = simpleType.Name;
                            }
                            else
                            {
                                str10 = str6;
                            }
                        }
                        this.AddParam(new WsdlParser.URTParam(name, str10, urtNamespace.Namespace, urtNamespace.EncodedNS, oUT, true, parser, urtNamespace));
                    }
                }
            }

            internal static string ValueString(string paramType)
            {
                if (paramType == "void")
                {
                    return null;
                }
                if (paramType == "bool")
                {
                    return "false";
                }
                if (paramType == "string")
                {
                    return "null";
                }
                if ((((paramType == "sbyte") || (paramType == "byte")) || ((paramType == "short") || (paramType == "ushort"))) || (((paramType == "int") || (paramType == "uint")) || ((paramType == "long") || (paramType == "ulong"))))
                {
                    return "1";
                }
                if ((paramType == "float") || (paramType == "exfloat"))
                {
                    return "(float)1.0";
                }
                if ((paramType == "double") || (paramType == "exdouble"))
                {
                    return "1.0";
                }
                StringBuilder builder = new StringBuilder(50);
                builder.Append('(');
                builder.Append(WsdlParser.IsValidCS(paramType));
                builder.Append(") (Object) null");
                return builder.ToString();
            }

            internal System.Runtime.Remoting.MetadataServices.WsdlParser.MethodFlags MethodFlags
            {
                get
                {
                    return this._methodFlags;
                }
                set
                {
                    this._methodFlags = value;
                }
            }

            protected WsdlParser.URTParam MethodType
            {
                get
                {
                    return this._methodType;
                }
            }

            internal string Name
            {
                get
                {
                    return this._methodName;
                }
            }

            internal string SoapAction
            {
                get
                {
                    return this._soapAction;
                }
            }
        }

        internal class URTNamespace
        {
            private int _anonymousSeqNum;
            private string _assemName;
            private bool _bReferenced;
            private ArrayList _elmDecls;
            private string _encodedNS;
            private string _name;
            private string _namespace;
            private System.Runtime.Remoting.MetadataServices.UrtType _nsType;
            private int _numURTComplexTypes;
            private int _numURTSimpleTypes;
            private WsdlParser _parser;
            internal ArrayList _URTComplexTypes;
            private ArrayList _URTInterfaces;
            internal ArrayList _URTSimpleTypes;

            internal URTNamespace(string name, WsdlParser parser)
            {
                this._name = name;
                this._parser = parser;
                this._nsType = parser.IsURTExportedType(name, out this._namespace, out this._assemName);
                if (this._nsType == System.Runtime.Remoting.MetadataServices.UrtType.Interop)
                {
                    this._encodedNS = EncodeInterop(this._namespace, parser);
                }
                else
                {
                    this._encodedNS = this._namespace;
                }
                this._elmDecls = new ArrayList();
                this._URTComplexTypes = new ArrayList();
                this._numURTComplexTypes = 0;
                this._URTSimpleTypes = new ArrayList();
                this._numURTSimpleTypes = 0;
                this._URTInterfaces = new ArrayList();
                this._anonymousSeqNum = 0;
                parser.AddNamespace(this);
            }

            internal void AddComplexType(WsdlParser.URTComplexType type)
            {
                this._URTComplexTypes.Add(type);
                this._numURTComplexTypes++;
            }

            internal void AddElementDecl(WsdlParser.ElementDecl elmDecl)
            {
                this._elmDecls.Add(elmDecl);
            }

            internal void AddInterface(WsdlParser.URTInterface iface)
            {
                this._URTInterfaces.Add(iface);
            }

            internal void AddSimpleType(WsdlParser.URTSimpleType type)
            {
                this._URTSimpleTypes.Add(type);
                this._numURTSimpleTypes++;
            }

            internal bool ComplexTypeOnlyArrayorEmpty()
            {
                for (int i = 0; i < this._URTComplexTypes.Count; i++)
                {
                    WsdlParser.URTComplexType type = (WsdlParser.URTComplexType) this._URTComplexTypes[i];
                    if ((type != null) && !type.IsArray())
                    {
                        return false;
                    }
                }
                return true;
            }

            internal static string EncodeInterop(string name, WsdlParser parser)
            {
                string str = name;
                if ((parser.ProxyNamespace != null) && (parser.ProxyNamespace.Length > 0))
                {
                    string str2 = "";
                    if (parser.ProxyNamespaceCount > 0)
                    {
                        str2 = parser.ProxyNamespaceCount.ToString(CultureInfo.InvariantCulture);
                    }
                    parser.ProxyNamespaceCount++;
                    return (parser.ProxyNamespace + str2);
                }
                int index = name.IndexOf(":");
                if (index > 0)
                {
                    str = str.Substring(index + 1);
                }
                if (str.StartsWith("//", StringComparison.Ordinal))
                {
                    str = str.Substring(2);
                }
                return str.Replace('/', '_');
            }

            internal string GetNextAnonymousName()
            {
                this._anonymousSeqNum++;
                return ("AnonymousType" + this._anonymousSeqNum);
            }

            internal WsdlParser.URTComplexType LookupComplexType(string typeName)
            {
                for (int i = 0; i < this._URTComplexTypes.Count; i++)
                {
                    WsdlParser.URTComplexType type2 = (WsdlParser.URTComplexType) this._URTComplexTypes[i];
                    if ((type2 != null) && WsdlParser.MatchingStrings(type2.SearchName, typeName))
                    {
                        return type2;
                    }
                }
                return null;
            }

            internal WsdlParser.URTComplexType LookupComplexTypeEqual(string typeName)
            {
                for (int i = 0; i < this._URTComplexTypes.Count; i++)
                {
                    WsdlParser.URTComplexType type2 = (WsdlParser.URTComplexType) this._URTComplexTypes[i];
                    if ((type2 != null) && (type2.SearchName == typeName))
                    {
                        return type2;
                    }
                }
                return null;
            }

            internal WsdlParser.URTInterface LookupInterface(string iFaceName)
            {
                for (int i = 0; i < this._URTInterfaces.Count; i++)
                {
                    WsdlParser.URTInterface interface2 = (WsdlParser.URTInterface) this._URTInterfaces[i];
                    if (WsdlParser.MatchingStrings(interface2.Name, iFaceName))
                    {
                        return interface2;
                    }
                }
                return null;
            }

            internal WsdlParser.URTSimpleType LookupSimpleType(string typeName)
            {
                for (int i = 0; i < this._URTSimpleTypes.Count; i++)
                {
                    WsdlParser.URTSimpleType type = (WsdlParser.URTSimpleType) this._URTSimpleTypes[i];
                    if ((type != null) && WsdlParser.MatchingStrings(type.Name, typeName))
                    {
                        return type;
                    }
                }
                return null;
            }

            internal WsdlParser.BaseType LookupType(string typeName)
            {
                WsdlParser.BaseType complexType = this.LookupComplexType(typeName);
                if (complexType == null)
                {
                    complexType = this.LookupSimpleType(typeName);
                }
                return complexType;
            }

            internal void PrintCSC(WsdlParser.WriterStream writerStream)
            {
                TextWriter outputStream = writerStream.OutputStream;
                bool flag = false;
                if (this._numURTComplexTypes > 0)
                {
                    for (int i = 0; i < this._URTComplexTypes.Count; i++)
                    {
                        WsdlParser.URTComplexType type = (WsdlParser.URTComplexType) this._URTComplexTypes[i];
                        if ((type != null) && type.IsPrint)
                        {
                            flag = true;
                        }
                    }
                }
                if (this._numURTSimpleTypes > 0)
                {
                    for (int j = 0; j < this._URTSimpleTypes.Count; j++)
                    {
                        if (((WsdlParser.URTSimpleType) this._URTSimpleTypes[j]) != null)
                        {
                            flag = true;
                        }
                    }
                }
                if (this._URTInterfaces.Count > 0)
                {
                    flag = true;
                }
                if (flag)
                {
                    string indentation = string.Empty;
                    Stream baseStream = ((StreamWriter) outputStream).BaseStream;
                    if (!writerStream.GetWrittenTo())
                    {
                        outputStream.WriteLine("using System;");
                        outputStream.WriteLine("using System.Runtime.Remoting.Messaging;");
                        outputStream.WriteLine("using System.Runtime.Remoting.Metadata;");
                        outputStream.WriteLine("using System.Runtime.Remoting.Metadata.W3cXsd2001;");
                        outputStream.WriteLine("using System.Runtime.InteropServices;");
                        writerStream.SetWrittenTo();
                    }
                    if ((this.Namespace != null) && (this.Namespace.Length != 0))
                    {
                        outputStream.Write("namespace ");
                        outputStream.Write(WsdlParser.IsValidCS(this.EncodedNS));
                        outputStream.WriteLine(" {");
                        indentation = "    ";
                    }
                    StringBuilder sb = new StringBuilder(0x100);
                    if (this._numURTComplexTypes > 0)
                    {
                        for (int m = 0; m < this._URTComplexTypes.Count; m++)
                        {
                            WsdlParser.URTComplexType type3 = (WsdlParser.URTComplexType) this._URTComplexTypes[m];
                            if ((type3 != null) && type3.IsPrint)
                            {
                                type3.PrintCSC(outputStream, indentation, this._encodedNS, sb);
                            }
                        }
                    }
                    if (this._numURTSimpleTypes > 0)
                    {
                        for (int n = 0; n < this._URTSimpleTypes.Count; n++)
                        {
                            WsdlParser.URTSimpleType type4 = (WsdlParser.URTSimpleType) this._URTSimpleTypes[n];
                            if (type4 != null)
                            {
                                type4.PrintCSC(outputStream, indentation, this._encodedNS, sb);
                            }
                        }
                    }
                    for (int k = 0; k < this._URTInterfaces.Count; k++)
                    {
                        ((WsdlParser.URTInterface) this._URTInterfaces[k]).PrintCSC(outputStream, indentation, this._encodedNS, sb);
                    }
                    if ((this.Namespace != null) && (this.Namespace.Length != 0))
                    {
                        outputStream.WriteLine('}');
                    }
                }
            }

            internal void RemoveComplexType(WsdlParser.URTComplexType type)
            {
                for (int i = 0; i < this._URTComplexTypes.Count; i++)
                {
                    if (this._URTComplexTypes[i] == type)
                    {
                        this._URTComplexTypes[i] = null;
                        this._numURTComplexTypes--;
                        return;
                    }
                }
                throw new SUDSParserException(CoreChannel.GetResourceString("Remoting_Suds_TriedToRemoveNonexistentType"));
            }

            internal void RemoveSimpleType(WsdlParser.URTSimpleType type)
            {
                for (int i = 0; i < this._URTSimpleTypes.Count; i++)
                {
                    if (this._URTSimpleTypes[i] == type)
                    {
                        this._URTSimpleTypes[i] = null;
                        this._numURTSimpleTypes--;
                        return;
                    }
                }
                throw new SUDSParserException(CoreChannel.GetResourceString("Remoting_Suds_TriedToRemoveNonexistentType"));
            }

            internal void ResolveElements(WsdlParser parser)
            {
                for (int i = 0; i < this._elmDecls.Count; i++)
                {
                    ((WsdlParser.ElementDecl) this._elmDecls[i]).Resolve(parser);
                }
            }

            internal void ResolveMethods()
            {
                for (int i = 0; i < this._URTComplexTypes.Count; i++)
                {
                    if (this._URTComplexTypes[i] != null)
                    {
                        ((WsdlParser.URTComplexType) this._URTComplexTypes[i]).ResolveMethods();
                    }
                }
            }

            internal void ResolveTypes(WsdlParser parser)
            {
                for (int i = 0; i < this._URTComplexTypes.Count; i++)
                {
                    if (this._URTComplexTypes[i] != null)
                    {
                        ((WsdlParser.URTComplexType) this._URTComplexTypes[i]).ResolveTypes(parser);
                    }
                }
                for (int j = 0; j < this._URTInterfaces.Count; j++)
                {
                    ((WsdlParser.URTInterface) this._URTInterfaces[j]).ResolveTypes(parser);
                }
            }

            internal string AssemName
            {
                get
                {
                    return this._assemName;
                }
            }

            internal bool bReferenced
            {
                get
                {
                    return this._bReferenced;
                }
                set
                {
                    this._bReferenced = value;
                }
            }

            internal string EncodedNS
            {
                get
                {
                    return this._encodedNS;
                }
                set
                {
                    this._encodedNS = value;
                }
            }

            internal bool IsEmpty
            {
                get
                {
                    return ((this.ComplexTypeOnlyArrayorEmpty() && (this._URTInterfaces.Count == 0)) && (this._numURTSimpleTypes == 0));
                }
            }

            internal bool IsSystem
            {
                get
                {
                    return ((this._namespace != null) && this._namespace.StartsWith("System", StringComparison.Ordinal));
                }
            }

            internal bool IsURTNamespace
            {
                get
                {
                    return (this._namespace == this._encodedNS);
                }
            }

            internal string Name
            {
                get
                {
                    return this._name;
                }
            }

            internal string Namespace
            {
                get
                {
                    return this._namespace;
                }
            }

            internal System.Runtime.Remoting.MetadataServices.UrtType UrtType
            {
                get
                {
                    return this._nsType;
                }
            }
        }

        internal class URTParam
        {
            private bool _embeddedParam;
            private string _encodedNS;
            private string _name;
            private WsdlParser _parser;
            private WsdlParser.URTParamType _pType;
            private string _typeName;
            private string _typeNS;
            private WsdlParser.URTNamespace _urtNamespace;
            private static string[] PTypeString = new string[] { "", "out ", "ref " };

            internal URTParam(string name, string typeName, string typeNS, string encodedNS, WsdlParser.URTParamType pType, bool bEmbedded, WsdlParser parser, WsdlParser.URTNamespace urtNamespace)
            {
                this._name = name;
                this._typeName = typeName;
                this._typeNS = typeNS;
                this._encodedNS = encodedNS;
                this._pType = pType;
                this._embeddedParam = bEmbedded;
                this._parser = parser;
                this._urtNamespace = urtNamespace;
            }

            public override bool Equals(object obj)
            {
                WsdlParser.URTParam param = (WsdlParser.URTParam) obj;
                return (((this._pType == param._pType) && WsdlParser.MatchingStrings(this._typeName, param._typeName)) && WsdlParser.MatchingStrings(this._typeNS, param._typeNS));
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            internal string GetTypeString(string curNS, bool bNS)
            {
                return this._parser.GetTypeString(curNS, bNS, this._urtNamespace, this._typeName, this._encodedNS);
            }

            internal void PrintCSC(StringBuilder sb)
            {
                sb.Append(PTypeString[(int) this._pType]);
                sb.Append(WsdlParser.IsValidCS(this._name));
            }

            internal void PrintCSC(StringBuilder sb, string curNS)
            {
                sb.Append(PTypeString[(int) this._pType]);
                sb.Append(this.GetTypeString(curNS, true));
                sb.Append(' ');
                sb.Append(WsdlParser.IsValidCS(this._name));
            }

            internal string Name
            {
                get
                {
                    return this._name;
                }
            }

            internal WsdlParser.URTParamType ParamType
            {
                get
                {
                    return this._pType;
                }
                set
                {
                    this._pType = value;
                }
            }

            internal string TypeName
            {
                get
                {
                    return this._typeName;
                }
            }

            internal string TypeNS
            {
                get
                {
                    return this._typeNS;
                }
            }
        }

        [Serializable]
        internal enum URTParamType
        {
            IN,
            OUT,
            REF
        }

        internal class URTSimpleType : WsdlParser.BaseType
        {
            private bool _bAnonymous;
            private WsdlParser.BaseType _baseType;
            private string _baseTypeName;
            private string _baseTypeXmlNS;
            private bool _bEnum;
            private string _encoding;
            private string _enumType;
            private ArrayList _facets;
            private string _fieldString;
            private WsdlParser _parser;

            internal URTSimpleType(string name, string urlNS, string ns, string encodedNS, bool bAnonymous, WsdlParser parser) : base(name, urlNS, ns, encodedNS)
            {
                this._baseTypeName = null;
                this._baseTypeXmlNS = null;
                this._baseType = null;
                this._fieldString = null;
                this._facets = new ArrayList();
                this._bEnum = false;
                this._bAnonymous = bAnonymous;
                this._encoding = null;
                this._parser = parser;
            }

            internal void AddFacet(WsdlParser.SchemaFacet facet)
            {
                this._facets.Add(facet);
            }

            internal void Extends(string baseTypeName, string baseTypeNS)
            {
                this._baseTypeName = baseTypeName;
                this._baseTypeXmlNS = baseTypeNS;
            }

            internal override WsdlParser.MethodFlags GetMethodFlags(WsdlParser.URTMethod method)
            {
                return WsdlParser.MethodFlags.None;
            }

            internal override string GetName(string curNS)
            {
                if ((this._fieldString != null) && (this._fieldString != string.Empty))
                {
                    return this._fieldString;
                }
                return base.GetName(curNS);
            }

            private string MapToEnumType(string type)
            {
                if (type == "Byte")
                {
                    return "byte";
                }
                if (type == "SByte")
                {
                    return "sbyte";
                }
                if (type == "Int16")
                {
                    return "short";
                }
                if (type == "UInt16")
                {
                    return "ushort";
                }
                if (type == "Int32")
                {
                    return "int";
                }
                if (type == "UInt32")
                {
                    return "uint";
                }
                if (type == "Int64")
                {
                    return "long";
                }
                if (type != "UInt64")
                {
                    throw new SUDSParserException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Suds_InvalidEnumType"), new object[] { type }));
                }
                return "ulong";
            }

            internal void PrintCSC(TextWriter textWriter, string indentation, string curNS, StringBuilder sb)
            {
                if (!this.IsEmittableFieldType && (!base.bNestedType || base.bNestedTypePrint))
                {
                    string text1 = this._encoding;
                    sb.Length = 0;
                    sb.Append("\n");
                    sb.Append(indentation);
                    sb.Append("[");
                    sb.Append("Serializable, ");
                    sb.Append("SoapType(");
                    if (this._parser._xsdVersion == XsdVersion.V1999)
                    {
                        sb.Append("SoapOptions=SoapOption.Option1|SoapOption.AlwaysIncludeTypes|SoapOption.XsdString|SoapOption.EmbedAll,");
                    }
                    else if (this._parser._xsdVersion == XsdVersion.V2000)
                    {
                        sb.Append("SoapOptions=SoapOption.Option2|SoapOption.AlwaysIncludeTypes|SoapOption.XsdString|SoapOption.EmbedAll,");
                    }
                    sb.Append("XmlNamespace=");
                    sb.Append(WsdlParser.IsValidUrl(base.UrlNS));
                    sb.Append(", XmlTypeNamespace=");
                    sb.Append(WsdlParser.IsValidUrl(base.UrlNS));
                    sb.Append(")]");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(indentation);
                    if (this.IsEnum)
                    {
                        sb.Append("public enum ");
                    }
                    else
                    {
                        sb.Append("public class ");
                    }
                    if (base._bNestedType)
                    {
                        sb.Append(WsdlParser.IsValidCS(base.NestedTypeName));
                    }
                    else
                    {
                        sb.Append(WsdlParser.IsValidCS(base.Name));
                    }
                    if (this._baseType != null)
                    {
                        sb.Append(" : ");
                        sb.Append(WsdlParser.IsValidCSAttr(this._baseType.GetName(curNS)));
                    }
                    else if ((this.IsEnum && (this._enumType != null)) && (this._enumType.Length > 0))
                    {
                        sb.Append(" : ");
                        sb.Append(WsdlParser.IsValidCSAttr(this._enumType));
                    }
                    textWriter.WriteLine(sb);
                    textWriter.Write(indentation);
                    textWriter.WriteLine('{');
                    string newIndentation = indentation + "    ";
                    for (int i = 0; i < this._facets.Count; i++)
                    {
                        ((WsdlParser.SchemaFacet) this._facets[i]).PrintCSC(textWriter, newIndentation, curNS, sb);
                    }
                    textWriter.Write(indentation);
                    textWriter.WriteLine('}');
                }
            }

            internal string EnumType
            {
                set
                {
                    string qname = value;
                    this._parser.ParseQName(ref qname);
                    if ((qname != null) && (qname.Length > 0))
                    {
                        this._enumType = this.MapToEnumType(this._parser.MapSchemaTypesToCSharpTypes(qname));
                    }
                }
            }

            internal override string FieldName
            {
                get
                {
                    return this._fieldString;
                }
            }

            internal override string FieldNamespace
            {
                get
                {
                    string str = null;
                    if (this._parser._xsdVersion == XsdVersion.V1999)
                    {
                        return WsdlParser.s_schemaNamespaceString1999;
                    }
                    if (this._parser._xsdVersion == XsdVersion.V2000)
                    {
                        return WsdlParser.s_schemaNamespaceString2000;
                    }
                    if (this._parser._xsdVersion == XsdVersion.V2001)
                    {
                        str = WsdlParser.s_schemaNamespaceString;
                    }
                    return str;
                }
            }

            internal override bool IsEmittableFieldType
            {
                get
                {
                    if (this._fieldString == null)
                    {
                        if (((this._bAnonymous && (this._facets.Count == 0)) && ((this._encoding != null) && (this._baseTypeName == "binary"))) && this._parser.MatchingSchemaStrings(this._baseTypeXmlNS))
                        {
                            this._fieldString = "byte[]";
                        }
                        else
                        {
                            this._fieldString = string.Empty;
                        }
                    }
                    return (this._fieldString != string.Empty);
                }
            }

            internal bool IsEnum
            {
                get
                {
                    return this._bEnum;
                }
                set
                {
                    this._bEnum = value;
                }
            }

            internal override bool PrimitiveField
            {
                get
                {
                    return true;
                }
            }
        }

        internal class WriterStream
        {
            private bool _bWrittenTo;
            private string _fileName;
            private WsdlParser.WriterStream _next;
            private TextWriter _writer;

            private WriterStream(string fileName, TextWriter writer)
            {
                this._fileName = fileName;
                this._writer = writer;
            }

            internal static void Close(WsdlParser.WriterStream outputStreams)
            {
                for (WsdlParser.WriterStream stream = outputStreams; stream != null; stream = stream._next)
                {
                    stream._writer.Close();
                }
            }

            internal static void Flush(WsdlParser.WriterStream writerStream)
            {
                while (writerStream != null)
                {
                    writerStream._writer.Flush();
                    writerStream = writerStream._next;
                }
            }

            internal static WsdlParser.WriterStream GetWriterStream(ref WsdlParser.WriterStream outputStreams, string outputDir, string fileName, ref string completeFileName)
            {
                WsdlParser.WriterStream stream;
                for (stream = outputStreams; stream != null; stream = stream._next)
                {
                    if (stream._fileName == fileName)
                    {
                        return stream;
                    }
                }
                string str = fileName;
                if (str.EndsWith(".exe", StringComparison.Ordinal) || str.EndsWith(".dll", StringComparison.Ordinal))
                {
                    str = str.Substring(0, str.Length - 4);
                }
                string path = outputDir + str + ".cs";
                completeFileName = path;
                TextWriter writer = new StreamWriter(path, false, new UTF8Encoding(false));
                stream = new WsdlParser.WriterStream(fileName, writer) {
                    _next = outputStreams
                };
                outputStreams = stream;
                return stream;
            }

            internal bool GetWrittenTo()
            {
                return this._bWrittenTo;
            }

            internal void SetWrittenTo()
            {
                this._bWrittenTo = true;
            }

            internal TextWriter OutputStream
            {
                get
                {
                    return this._writer;
                }
            }
        }

        internal class WsdlBinding : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string name;
            internal ArrayList operations = new ArrayList(10);
            internal WsdlParser.URTNamespace parsingNamespace;
            internal WsdlParser.WsdlBindingSoapBinding soapBinding;
            internal ArrayList suds = new ArrayList(10);
            internal string type;
            internal string typeNs;

            public void Dump()
            {
                if (this.soapBinding != null)
                {
                    this.soapBinding.Dump();
                }
                foreach (WsdlParser.IDump dump in this.suds)
                {
                    dump.Dump();
                }
                foreach (WsdlParser.IDump dump2 in this.operations)
                {
                    dump2.Dump();
                }
            }

            public void UsedNamespace(Hashtable namespaces)
            {
                if (this.soapBinding != null)
                {
                    this.soapBinding.UsedNamespace(namespaces);
                }
                foreach (WsdlParser.INamespaces namespaces2 in this.suds)
                {
                    namespaces2.UsedNamespace(namespaces);
                }
                foreach (WsdlParser.INamespaces namespaces3 in this.operations)
                {
                    namespaces3.UsedNamespace(namespaces);
                }
            }
        }

        internal class WsdlBindingOperation : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string methodAttributes;
            internal string name;
            internal string nameNs;
            internal ArrayList sections = new ArrayList(10);
            internal WsdlParser.WsdlBindingSoapOperation soapOperation;

            public void Dump()
            {
                this.soapOperation.Dump();
                foreach (WsdlParser.IDump dump in this.sections)
                {
                    dump.Dump();
                }
            }

            public void UsedNamespace(Hashtable namespaces)
            {
                this.soapOperation.UsedNamespace(namespaces);
                foreach (WsdlParser.INamespaces namespaces2 in this.sections)
                {
                    namespaces2.UsedNamespace(namespaces);
                }
            }
        }

        internal class WsdlBindingOperationSection : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string elementName;
            internal ArrayList extensions = new ArrayList(10);
            internal string name;

            public void Dump()
            {
                foreach (WsdlParser.IDump dump in this.extensions)
                {
                    dump.Dump();
                }
            }

            public void UsedNamespace(Hashtable namespaces)
            {
                foreach (WsdlParser.INamespaces namespaces2 in this.extensions)
                {
                    namespaces2.UsedNamespace(namespaces);
                }
            }
        }

        internal class WsdlBindingSoapBinding : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string style;
            internal string transport;

            public void Dump()
            {
            }

            public void UsedNamespace(Hashtable namespaces)
            {
            }
        }

        internal class WsdlBindingSoapBody : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string encodingStyle;
            internal string namespaceUri;
            internal string parts;
            internal string use;

            public void Dump()
            {
            }

            public void UsedNamespace(Hashtable namespaces)
            {
            }
        }

        internal class WsdlBindingSoapFault : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string encodingStyle;
            internal string name;
            internal string namespaceUri;
            internal string use;

            public void Dump()
            {
            }

            public void UsedNamespace(Hashtable namespaces)
            {
            }
        }

        internal class WsdlBindingSoapHeader : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string encodingStyle;
            internal string message;
            internal string messageNs;
            internal string namespaceUri;
            internal string part;
            internal string use;

            public void Dump()
            {
            }

            public void UsedNamespace(Hashtable namespaces)
            {
            }
        }

        internal class WsdlBindingSoapOperation : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string soapAction;
            internal string style;

            public void Dump()
            {
            }

            public void UsedNamespace(Hashtable namespaces)
            {
            }
        }

        internal class WsdlBindingSuds : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string elementName;
            internal string extendsNs;
            internal string extendsTypeName;
            internal ArrayList implements = new ArrayList(10);
            internal ArrayList nestedTypes = new ArrayList(10);
            internal string ns;
            internal WsdlParser.SudsUse sudsUse;
            internal string typeName;

            public void Dump()
            {
                foreach (WsdlParser.IDump dump in this.implements)
                {
                    dump.Dump();
                }
                foreach (WsdlParser.IDump dump2 in this.nestedTypes)
                {
                    dump2.Dump();
                }
            }

            public void UsedNamespace(Hashtable namespaces)
            {
                if (this.ns != null)
                {
                    namespaces[this.ns] = 1;
                }
                if (this.extendsNs != null)
                {
                    namespaces[this.extendsNs] = 1;
                }
                foreach (WsdlParser.INamespaces namespaces2 in this.implements)
                {
                    namespaces2.UsedNamespace(namespaces);
                }
            }
        }

        internal class WsdlBindingSudsImplements : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string ns;
            internal string typeName;

            public void Dump()
            {
            }

            public void UsedNamespace(Hashtable namespaces)
            {
                if (this.ns != null)
                {
                    namespaces[this.ns] = 1;
                }
            }
        }

        internal class WsdlBindingSudsNestedType : WsdlParser.IDump
        {
            internal string name;
            internal string ns;
            internal string typeName;

            public void Dump()
            {
            }
        }

        internal class WsdlMessage : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string name;
            internal string nameNs;
            internal ArrayList parts = new ArrayList(10);

            public void Dump()
            {
                for (int i = 0; i < this.parts.Count; i++)
                {
                    ((WsdlParser.IDump) this.parts[i]).Dump();
                }
            }

            public void UsedNamespace(Hashtable namespaces)
            {
                for (int i = 0; i < this.parts.Count; i++)
                {
                    ((WsdlParser.INamespaces) this.parts[i]).UsedNamespace(namespaces);
                }
            }
        }

        internal class WsdlMessagePart : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string element;
            internal string elementNs;
            internal string name;
            internal string nameNs;
            internal string typeName;
            internal string typeNameNs;

            public void Dump()
            {
            }

            public void UsedNamespace(Hashtable namespaces)
            {
                if (this.nameNs != null)
                {
                    namespaces[this.nameNs] = 1;
                }
                if (this.elementNs != null)
                {
                    namespaces[this.elementNs] = 1;
                }
            }
        }

        internal class WsdlMethodInfo : WsdlParser.IDump
        {
            internal bool bGet;
            internal bool bProperty;
            internal bool bSet;
            internal string[] inputElements;
            internal string[] inputElementsNs;
            internal string inputMethodName;
            internal string inputMethodNameNs;
            internal string[] inputNames;
            internal string[] inputNamesNs;
            internal string[] inputTypes;
            internal string[] inputTypesNs;
            internal string methodAttributes;
            internal string methodName;
            internal string methodNameNs;
            internal string[] outputElements;
            internal string[] outputElementsNs;
            internal string outputMethodName;
            internal string outputMethodNameNs;
            internal string[] outputNames;
            internal string[] outputNamesNs;
            internal string[] outputTypes;
            internal string[] outputTypesNs;
            internal string[] paramNamesOrder;
            internal string propertyName;
            internal string propertyNs;
            internal string propertyType;
            internal string soapAction;
            internal string soapActionGet;
            internal string soapActionSet;

            public void Dump()
            {
                if (this.paramNamesOrder != null)
                {
                    string[] paramNamesOrder = this.paramNamesOrder;
                    for (int i = 0; i < paramNamesOrder.Length; i++)
                    {
                        string text1 = paramNamesOrder[i];
                    }
                }
                if (this.inputNames != null)
                {
                    for (int j = 0; j < this.inputNames.Length; j++)
                    {
                    }
                }
                if (this.outputNames != null)
                {
                    for (int k = 0; k < this.outputNames.Length; k++)
                    {
                    }
                }
                bool bProperty = this.bProperty;
            }
        }

        internal class WsdlPortType : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string name;
            internal ArrayList operations = new ArrayList(10);
            internal Hashtable sections = new Hashtable(10);

            public void Dump()
            {
                using (IDictionaryEnumerator enumerator = this.sections.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        DictionaryEntry current = (DictionaryEntry) enumerator.Current;
                    }
                }
                foreach (WsdlParser.IDump dump in this.operations)
                {
                    dump.Dump();
                }
            }

            public void UsedNamespace(Hashtable namespaces)
            {
                foreach (WsdlParser.INamespaces namespaces2 in this.operations)
                {
                    namespaces2.UsedNamespace(namespaces);
                }
            }
        }

        internal class WsdlPortTypeOperation : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal ArrayList contents = new ArrayList(3);
            internal string name;
            internal string nameNs;
            internal string parameterOrder;

            public void Dump()
            {
                foreach (WsdlParser.IDump dump in this.contents)
                {
                    dump.Dump();
                }
            }

            public void UsedNamespace(Hashtable namespaces)
            {
                foreach (WsdlParser.INamespaces namespaces2 in this.contents)
                {
                    namespaces2.UsedNamespace(namespaces);
                }
            }
        }

        internal class WsdlPortTypeOperationContent : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string element;
            internal string message;
            internal string messageNs;
            internal string name;
            internal string nameNs;

            public void Dump()
            {
            }

            public void UsedNamespace(Hashtable namespaces)
            {
            }
        }

        internal class WsdlService : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string name;
            internal Hashtable ports = new Hashtable(10);

            public void Dump()
            {
                foreach (DictionaryEntry entry in this.ports)
                {
                    ((WsdlParser.IDump) entry.Value).Dump();
                }
            }

            public void UsedNamespace(Hashtable namespaces)
            {
                foreach (DictionaryEntry entry in this.ports)
                {
                    ((WsdlParser.INamespaces) entry.Value).UsedNamespace(namespaces);
                }
            }
        }

        internal class WsdlServicePort : WsdlParser.IDump, WsdlParser.INamespaces
        {
            internal string binding;
            internal string bindingNs;
            internal ArrayList locations;
            internal string name;
            internal string nameNs;

            public void Dump()
            {
                if (this.locations != null)
                {
                    using (IEnumerator enumerator = this.locations.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            string current = (string) enumerator.Current;
                        }
                    }
                }
            }

            public void UsedNamespace(Hashtable namespaces)
            {
            }
        }
    }
}

