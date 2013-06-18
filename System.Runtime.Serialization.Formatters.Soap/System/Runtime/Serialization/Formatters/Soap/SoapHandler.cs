namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    internal sealed class SoapHandler
    {
        private Hashtable assemKeyToAssemblyTable;
        private Hashtable assemKeyToInteropAssemblyTable;
        private Hashtable assemKeyToNameSpaceTable;
        private SerStack attributeValues = new SerStack("AttributePrefix");
        private InternalParseStateE currentState;
        private int headerArrayLength;
        private ArrayList headerList;
        private HeaderStateEnum headerState;
        private bool isBody;
        private bool isEnvelope;
        private bool isTopFound;
        internal Hashtable keyToNamespaceTable;
        private NameCache nameCache = new NameCache();
        private Hashtable nameSpaceToKey;
        private int nextPrefix;
        private ObjectReader objectReader;
        private SerStack prPool = new SerStack("prPool");
        private StringBuilder sburi = new StringBuilder(50);
        private string soapEnvKey = "SOAP-ENV";
        private string soapKey = "SOAP-ENC";
        private SoapParser soapParser;
        private SerStack stack = new SerStack("SoapParser Stack");
        private StringBuilder stringBuffer = new StringBuilder(120);
        private string textValue = "";
        private string urtKey = "urt";
        private ArrayList xmlAttributeList;
        private XmlTextReader xmlTextReader;
        private string xsdKey = "xsd";
        private string xsiKey = "xsi";

        internal SoapHandler(SoapParser soapParser)
        {
            this.soapParser = soapParser;
        }

        internal void Attribute(string prefix, string name, string urn, string value)
        {
            switch (this.currentState)
            {
                case InternalParseStateE.Object:
                case InternalParseStateE.Member:
                {
                    ParseRecord record1 = (ParseRecord) this.stack.Peek();
                    string key = name;
                    if (((urn != null) && (urn.Length != 0)) && ((prefix == null) || (prefix.Length == 0)))
                    {
                        if (this.nameSpaceToKey.ContainsKey(urn))
                        {
                            key = (string) this.nameSpaceToKey[urn];
                        }
                        else
                        {
                            key = this.NextPrefix();
                            this.nameSpaceToKey[urn] = key;
                        }
                    }
                    if (((prefix != null) && (key != null)) && ((value != null) && (urn != null)))
                    {
                        this.attributeValues.Push(new AttributeValueEntry(prefix, key, value, urn));
                    }
                    return;
                }
            }
            this.MarshalError("EndAttribute, Unknown State ", (ParseRecord) this.stack.Peek(), name, this.currentState);
        }

        internal void Comment(string body)
        {
        }

        internal void EndElement(string prefix, string name, string urn)
        {
            string str = this.NameFilter(name);
            ParseRecord objectPr = null;
            ParseRecord pr = null;
            switch (this.currentState)
            {
                case InternalParseStateE.Object:
                    pr = (ParseRecord) this.stack.Pop();
                    if (pr.PRparseTypeEnum != InternalParseTypeE.Envelope)
                    {
                        if (pr.PRparseTypeEnum == InternalParseTypeE.Body)
                        {
                            pr.PRparseTypeEnum = InternalParseTypeE.BodyEnd;
                        }
                        else if (pr.PRparseTypeEnum == InternalParseTypeE.Headers)
                        {
                            pr.PRparseTypeEnum = InternalParseTypeE.HeadersEnd;
                            this.headerState = HeaderStateEnum.HeaderRecord;
                        }
                        else if (pr.PRarrayTypeEnum != InternalArrayTypeE.Base64)
                        {
                            objectPr = (ParseRecord) this.stack.Peek();
                            if ((!this.isTopFound && (objectPr != null)) && (objectPr.PRparseTypeEnum == InternalParseTypeE.Body))
                            {
                                pr.PRobjectPositionEnum = InternalObjectPositionE.Top;
                                this.isTopFound = true;
                            }
                            if (!pr.PRisParsed)
                            {
                                if (!pr.PRisProcessAttributes && ((pr.PRobjectPositionEnum != InternalObjectPositionE.Top) || !this.objectReader.IsFakeTopObject))
                                {
                                    this.ProcessAttributes(pr, objectPr);
                                }
                                this.objectReader.Parse(pr);
                                pr.PRisParsed = true;
                            }
                            pr.PRparseTypeEnum = InternalParseTypeE.ObjectEnd;
                        }
                        break;
                    }
                    pr.PRparseTypeEnum = InternalParseTypeE.EnvelopeEnd;
                    break;

                case InternalParseStateE.Member:
                {
                    pr = (ParseRecord) this.stack.Peek();
                    objectPr = (ParseRecord) this.stack.PeekPeek();
                    this.ProcessAttributes(pr, objectPr);
                    ArrayList xmlAttributeList = this.xmlAttributeList;
                    if ((this.xmlAttributeList != null) && (this.xmlAttributeList.Count > 0))
                    {
                        for (int i = 0; i < this.xmlAttributeList.Count; i++)
                        {
                            this.objectReader.Parse((ParseRecord) this.xmlAttributeList[i]);
                        }
                        this.xmlAttributeList.Clear();
                    }
                    pr = (ParseRecord) this.stack.Pop();
                    if ((this.headerState == HeaderStateEnum.TopLevelObject) && (pr.PRarrayTypeEnum == InternalArrayTypeE.Base64))
                    {
                        this.objectReader.Parse(pr);
                        pr.PRisParsed = true;
                    }
                    else if (pr.PRmemberValueEnum != InternalMemberValueE.Nested)
                    {
                        if ((pr.PRobjectTypeEnum == InternalObjectTypeE.Array) && (pr.PRmemberValueEnum != InternalMemberValueE.Null))
                        {
                            pr.PRmemberValueEnum = InternalMemberValueE.Nested;
                            this.objectReader.Parse(pr);
                            pr.PRisParsed = true;
                            pr.PRparseTypeEnum = InternalParseTypeE.MemberEnd;
                        }
                        else if (pr.PRidRef > 0L)
                        {
                            pr.PRmemberValueEnum = InternalMemberValueE.Reference;
                        }
                        else if (pr.PRmemberValueEnum != InternalMemberValueE.Null)
                        {
                            pr.PRmemberValueEnum = InternalMemberValueE.InlineValue;
                        }
                        switch (this.headerState)
                        {
                            case HeaderStateEnum.None:
                            case HeaderStateEnum.TopLevelObject:
                                if (pr.PRparseTypeEnum == InternalParseTypeE.Object)
                                {
                                    if (!pr.PRisParsed)
                                    {
                                        this.objectReader.Parse(pr);
                                    }
                                    pr.PRparseTypeEnum = InternalParseTypeE.ObjectEnd;
                                }
                                this.objectReader.Parse(pr);
                                pr.PRisParsed = true;
                                break;

                            case HeaderStateEnum.HeaderRecord:
                            case HeaderStateEnum.NestedObject:
                                this.ProcessHeaderMember(pr);
                                break;
                        }
                    }
                    else
                    {
                        pr.PRparseTypeEnum = InternalParseTypeE.MemberEnd;
                        switch (this.headerState)
                        {
                            case HeaderStateEnum.None:
                            case HeaderStateEnum.TopLevelObject:
                                this.objectReader.Parse(pr);
                                pr.PRisParsed = true;
                                break;

                            case HeaderStateEnum.HeaderRecord:
                            case HeaderStateEnum.NestedObject:
                                this.ProcessHeaderMemberEnd(pr);
                                break;
                        }
                    }
                    this.PutPr(pr);
                    return;
                }
                case InternalParseStateE.MemberChild:
                    pr = (ParseRecord) this.stack.Peek();
                    if (pr.PRmemberValueEnum != InternalMemberValueE.Null)
                    {
                        this.MarshalError("EndElement", (ParseRecord) this.stack.Peek(), str, this.currentState);
                    }
                    return;

                default:
                    this.MarshalError("EndElement", (ParseRecord) this.stack.Peek(), str, this.currentState);
                    return;
            }
            switch (this.headerState)
            {
                case HeaderStateEnum.None:
                case HeaderStateEnum.TopLevelObject:
                    this.objectReader.Parse(pr);
                    break;

                case HeaderStateEnum.HeaderRecord:
                case HeaderStateEnum.NestedObject:
                    this.ProcessHeaderEnd(pr);
                    break;
            }
            if (pr.PRparseTypeEnum == InternalParseTypeE.EnvelopeEnd)
            {
                this.soapParser.Stop();
            }
            this.PutPr(pr);
        }

        internal void FinishChildren(string prefix, string name, string urn)
        {
            ParseRecord record = null;
            switch (this.currentState)
            {
                case InternalParseStateE.Object:
                    record = (ParseRecord) this.stack.Peek();
                    if (record.PRarrayTypeEnum != InternalArrayTypeE.Base64)
                    {
                        break;
                    }
                    record.PRvalue = this.textValue;
                    this.textValue = "";
                    return;

                case InternalParseStateE.Member:
                    record = (ParseRecord) this.stack.Peek();
                    this.currentState = record.PRparseStateEnum;
                    record.PRvalue = this.textValue;
                    this.textValue = "";
                    return;

                case InternalParseStateE.MemberChild:
                {
                    record = (ParseRecord) this.stack.Peek();
                    this.currentState = record.PRparseStateEnum;
                    ParseRecord record1 = (ParseRecord) this.stack.PeekPeek();
                    record.PRvalue = this.textValue;
                    this.textValue = "";
                    return;
                }
                default:
                    this.MarshalError("FinishChildren", (ParseRecord) this.stack.Peek(), name, this.currentState);
                    break;
            }
        }

        private Type GetInteropType(string value, string httpstring)
        {
            Type interopTypeFromXmlType = SoapServices.GetInteropTypeFromXmlType(value, httpstring);
            if (interopTypeFromXmlType == null)
            {
                int index = httpstring.IndexOf("%2C");
                if (index > 0)
                {
                    string xmlTypeNamespace = httpstring.Substring(0, index);
                    interopTypeFromXmlType = SoapServices.GetInteropTypeFromXmlType(value, xmlTypeNamespace);
                }
            }
            return interopTypeFromXmlType;
        }

        private ParseRecord GetPr()
        {
            ParseRecord record = null;
            if (!this.prPool.IsEmpty())
            {
                record = (ParseRecord) this.prPool.Pop();
                record.Init();
                return record;
            }
            return new ParseRecord();
        }

        internal void Init(ObjectReader objectReader)
        {
            this.objectReader = objectReader;
            objectReader.soapHandler = this;
            this.isEnvelope = false;
            this.isBody = false;
            this.isTopFound = false;
            this.attributeValues.Clear();
            this.assemKeyToAssemblyTable = new Hashtable(10);
            this.assemKeyToAssemblyTable[this.urtKey] = new SoapAssemblyInfo(SoapUtil.urtAssemblyString, SoapUtil.urtAssembly);
            this.assemKeyToNameSpaceTable = new Hashtable(10);
            this.assemKeyToInteropAssemblyTable = new Hashtable(10);
            this.nameSpaceToKey = new Hashtable(5);
            this.keyToNamespaceTable = new Hashtable(10);
        }

        private void MarshalError(string handler, ParseRecord pr, string value, InternalParseStateE currentState)
        {
            string str = SerTraceString(handler, pr, value, currentState, this.headerState);
            throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Syntax"), new object[] { str }));
        }

        private string NameFilter(string name)
        {
            string cachedValue = this.nameCache.GetCachedValue(name) as string;
            if (cachedValue == null)
            {
                cachedValue = XmlConvert.DecodeName(name);
                this.nameCache.SetCachedValue(cachedValue);
            }
            return cachedValue;
        }

        private string NextPrefix()
        {
            this.nextPrefix++;
            return ("_P" + this.nextPrefix);
        }

        private int[] ParseArrayDimensions(string dimString, out int rank, out string dimSignature, out InternalArrayTypeE arrayTypeEnum)
        {
            char[] chArray = dimString.ToCharArray();
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int[] numArray = new int[chArray.Length];
            StringBuilder builder = new StringBuilder(10);
            StringBuilder builder2 = new StringBuilder(10);
            for (int i = 0; i < chArray.Length; i++)
            {
                if (chArray[i] == '[')
                {
                    num++;
                    builder2.Append(chArray[i]);
                }
                else if (chArray[i] == ']')
                {
                    if (builder.Length > 0)
                    {
                        numArray[num3++] = int.Parse(builder.ToString(), CultureInfo.InvariantCulture);
                        builder.Length = 0;
                    }
                    builder2.Append(chArray[i]);
                }
                else if (chArray[i] == ',')
                {
                    num2++;
                    if (builder.Length > 0)
                    {
                        numArray[num3++] = int.Parse(builder.ToString(), CultureInfo.InvariantCulture);
                        builder.Length = 0;
                    }
                    builder2.Append(chArray[i]);
                }
                else
                {
                    if ((chArray[i] != '-') && !char.IsDigit(chArray[i]))
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_ArrayDimensions"), new object[] { dimString }));
                    }
                    builder.Append(chArray[i]);
                }
            }
            rank = num3;
            dimSignature = builder2.ToString();
            int[] numArray2 = new int[rank];
            for (int j = 0; j < rank; j++)
            {
                numArray2[j] = numArray[j];
            }
            InternalArrayTypeE empty = InternalArrayTypeE.Empty;
            if (num2 > 0)
            {
                empty = InternalArrayTypeE.Rectangular;
            }
            else
            {
                empty = InternalArrayTypeE.Single;
            }
            arrayTypeEnum = empty;
            return numArray2;
        }

        private void ProcessArray(ParseRecord pr, int firstIndex, bool IsInterop)
        {
            string pRtypeXmlKey = pr.PRtypeXmlKey;
            InternalPrimitiveTypeE invalid = InternalPrimitiveTypeE.Invalid;
            pr.PRobjectTypeEnum = InternalObjectTypeE.Array;
            pr.PRmemberTypeEnum = InternalMemberTypeE.Item;
            pr.PRprimitiveArrayTypeString = pr.PRkeyDt.Substring(0, firstIndex);
            pr.PRkeyDt.Substring(firstIndex);
            if (IsInterop)
            {
                string assemblyString = (string) this.assemKeyToInteropAssemblyTable[pr.PRtypeXmlKey];
                pr.PRarrayElementType = this.objectReader.Bind(assemblyString, pr.PRprimitiveArrayTypeString);
                if (pr.PRarrayElementType == null)
                {
                    pr.PRarrayElementType = SoapServices.GetInteropTypeFromXmlType(pr.PRprimitiveArrayTypeString, assemblyString);
                }
                if (pr.PRarrayElementType == null)
                {
                    pr.PRarrayElementType = SoapServices.GetInteropTypeFromXmlElement(pr.PRprimitiveArrayTypeString, assemblyString);
                }
                if (pr.PRarrayElementType == null)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_TypeElement"), new object[] { pr.PRname + " " + pr.PRkeyDt }));
                }
                pr.PRprimitiveArrayTypeString = pr.PRarrayElementType.FullName;
            }
            else
            {
                invalid = Converter.ToCode(pr.PRprimitiveArrayTypeString);
                if (invalid != InternalPrimitiveTypeE.Invalid)
                {
                    pr.PRprimitiveArrayTypeString = Converter.SoapToComType(invalid);
                    pRtypeXmlKey = this.urtKey;
                }
                else if (string.Compare(pr.PRprimitiveArrayTypeString, "string", StringComparison.Ordinal) == 0)
                {
                    pr.PRprimitiveArrayTypeString = "System.String";
                    pRtypeXmlKey = this.urtKey;
                }
                else if ((string.Compare(pr.PRprimitiveArrayTypeString, "anyType", StringComparison.Ordinal) == 0) || (string.Compare(pr.PRprimitiveArrayTypeString, "ur-type", StringComparison.Ordinal) == 0))
                {
                    pr.PRprimitiveArrayTypeString = "System.Object";
                    pRtypeXmlKey = this.urtKey;
                }
            }
            int startIndex = firstIndex;
            int index = pr.PRkeyDt.IndexOf(']', startIndex + 1);
            if (index < 1)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_ArrayDimensions"), new object[] { pr.PRkeyDt }));
            }
            int rank = 0;
            int[] numArray = null;
            string dimSignature = null;
            InternalArrayTypeE empty = InternalArrayTypeE.Empty;
            int num4 = 0;
            StringBuilder builder = new StringBuilder(10);
            while (true)
            {
                num4++;
                numArray = this.ParseArrayDimensions(pr.PRkeyDt.Substring(startIndex, (index - startIndex) + 1), out rank, out dimSignature, out empty);
                if ((index + 1) == pr.PRkeyDt.Length)
                {
                    break;
                }
                builder.Append(dimSignature);
                startIndex = index + 1;
                index = pr.PRkeyDt.IndexOf(']', startIndex);
            }
            pr.PRlengthA = numArray;
            pr.PRrank = rank;
            if (num4 == 1)
            {
                pr.PRarrayElementTypeCode = invalid;
                pr.PRarrayTypeEnum = empty;
                pr.PRarrayElementTypeString = pr.PRprimitiveArrayTypeString;
            }
            else
            {
                pr.PRarrayElementTypeCode = InternalPrimitiveTypeE.Invalid;
                pr.PRarrayTypeEnum = InternalArrayTypeE.Rectangular;
                pr.PRarrayElementTypeString = pr.PRprimitiveArrayTypeString + builder.ToString();
            }
            if (!IsInterop || (num4 > 1))
            {
                pr.PRarrayElementType = this.ProcessGetType(pr.PRarrayElementTypeString, pRtypeXmlKey, out pr.PRassemblyName);
                if (pr.PRarrayElementType == null)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_ArrayType"), new object[] { pr.PRarrayElementType }));
                }
                if (pr.PRarrayElementType == SoapUtil.typeofObject)
                {
                    pr.PRisArrayVariant = true;
                    pRtypeXmlKey = this.urtKey;
                }
            }
        }

        private void ProcessAttributes(ParseRecord pr, ParseRecord objectPr)
        {
            string dimString = null;
            string str2 = null;
            string str3 = null;
            pr.PRisProcessAttributes = true;
            string strB = "http://schemas.xmlsoap.org/soap/encoding/";
            int length = strB.Length;
            string str5 = "http://schemas.microsoft.com/clr/id";
            int num2 = str5.Length;
            string str6 = "http://schemas.xmlsoap.org/soap/envelope/";
            int num3 = str6.Length;
            string str7 = "http://www.w3.org/2001/XMLSchema-instance";
            int num4 = str7.Length;
            string str8 = "http://www.w3.org/2000/10/XMLSchema-instance";
            int num5 = str8.Length;
            string str9 = "http://www.w3.org/1999/XMLSchema-instance";
            int num6 = str9.Length;
            string str10 = "http://www.w3.org/1999/XMLSchema";
            int num7 = str10.Length;
            string str11 = "http://www.w3.org/2000/10/XMLSchema";
            int num8 = str11.Length;
            string str12 = "http://www.w3.org/2001/XMLSchema";
            int num9 = str12.Length;
            string str13 = "http://schemas.microsoft.com/soap/encoding/clr/1.0";
            int num10 = str13.Length;
            for (int i = 0; i < this.attributeValues.Count(); i++)
            {
                AttributeValueEntry item = (AttributeValueEntry) this.attributeValues.GetItem(i);
                string prefix = item.prefix;
                string key = item.key;
                if ((key == null) || (key.Length == 0))
                {
                    key = pr.PRnameXmlKey;
                }
                string keyId = item.value;
                bool flag = false;
                string urn = item.urn;
                int num12 = key.Length;
                int num13 = keyId.Length;
                if ((key == null) || (num12 == 0))
                {
                    this.keyToNamespaceTable[prefix] = keyId;
                }
                else
                {
                    this.keyToNamespaceTable[prefix + ":" + key] = keyId;
                }
                if ((num12 == 2) && (string.Compare(key, "id", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    pr.PRobjectId = this.objectReader.GetId(keyId);
                }
                else if ((num12 == 8) && (string.Compare(key, "position", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    dimString = keyId;
                }
                else if ((num12 == 6) && (string.Compare(key, "offset", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    str2 = keyId;
                }
                else if ((num12 == 14) && (string.Compare(key, "MustUnderstand", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    str3 = keyId;
                }
                else if ((num12 == 4) && (string.Compare(key, "null", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    pr.PRmemberValueEnum = InternalMemberValueE.Null;
                    pr.PRvalue = null;
                }
                else if ((num12 == 4) && (string.Compare(key, "root", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    if (keyId.Equals("1"))
                    {
                        pr.PRisHeaderRoot = true;
                    }
                }
                else if ((num12 == 4) && (string.Compare(key, "href", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    pr.PRidRef = this.objectReader.GetId(keyId);
                }
                else if ((num12 == 4) && (string.Compare(key, "type", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    string pRtypeXmlKey = pr.PRtypeXmlKey;
                    string pRkeyDt = pr.PRkeyDt;
                    Type pRdtType = pr.PRdtType;
                    string strA = keyId;
                    int index = keyId.IndexOf(":");
                    if (index > 0)
                    {
                        pr.PRtypeXmlKey = keyId.Substring(0, index);
                        strA = keyId.Substring(++index);
                    }
                    else
                    {
                        pr.PRtypeXmlKey = prefix;
                    }
                    if ((string.Compare(strA, "anyType", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, "ur-type", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        pr.PRkeyDt = "System.Object";
                        pr.PRdtType = SoapUtil.typeofObject;
                        pr.PRtypeXmlKey = this.urtKey;
                    }
                    if ((pr.PRtypeXmlKey == this.soapKey) && (strA == "Array"))
                    {
                        pr.PRtypeXmlKey = pRtypeXmlKey;
                        pr.PRkeyDt = pRkeyDt;
                        pr.PRdtType = pRdtType;
                    }
                    else
                    {
                        pr.PRkeyDt = strA;
                    }
                }
                else if ((num12 == 9) && (string.Compare(key, "arraytype", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    string str21 = keyId;
                    int num15 = keyId.IndexOf(":");
                    if (num15 > 0)
                    {
                        pr.PRtypeXmlKey = keyId.Substring(0, num15);
                        pr.PRkeyDt = str21 = keyId.Substring(++num15);
                    }
                    if (str21.StartsWith("ur_type[", StringComparison.Ordinal))
                    {
                        pr.PRkeyDt = "System.Object" + str21.Substring(6);
                        pr.PRtypeXmlKey = this.urtKey;
                    }
                }
                else if (SoapServices.IsClrTypeNamespace(keyId))
                {
                    if (!this.assemKeyToAssemblyTable.ContainsKey(key))
                    {
                        string typeNamespace = null;
                        string assemblyName = null;
                        SoapServices.DecodeXmlNamespaceForClrTypeNamespace(keyId, out typeNamespace, out assemblyName);
                        if (assemblyName == null)
                        {
                            this.assemKeyToAssemblyTable[key] = new SoapAssemblyInfo(SoapUtil.urtAssemblyString, SoapUtil.urtAssembly);
                            this.assemKeyToNameSpaceTable[key] = typeNamespace;
                        }
                        else
                        {
                            this.assemKeyToAssemblyTable[key] = new SoapAssemblyInfo(assemblyName);
                            if (typeNamespace != null)
                            {
                                this.assemKeyToNameSpaceTable[key] = typeNamespace;
                            }
                        }
                    }
                }
                else if (((flag = prefix.Equals("xmlns")) && (num13 == length)) && (string.Compare(keyId, strB, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    this.soapKey = key;
                }
                else if ((flag && (num13 == num2)) && (string.Compare(keyId, str5, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    this.urtKey = key;
                    this.assemKeyToAssemblyTable[this.urtKey] = new SoapAssemblyInfo(SoapUtil.urtAssemblyString, SoapUtil.urtAssembly);
                }
                else if ((flag && (num13 == num3)) && (string.Compare(keyId, str6, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    this.soapEnvKey = key;
                }
                else if (key != "encodingStyle")
                {
                    if (flag && ((((num13 == num4) && (string.Compare(keyId, str7, StringComparison.OrdinalIgnoreCase) == 0)) || ((num13 == num6) && (string.Compare(keyId, str9, StringComparison.OrdinalIgnoreCase) == 0))) || ((num13 == num5) && (string.Compare(keyId, str8, StringComparison.OrdinalIgnoreCase) == 0))))
                    {
                        this.xsiKey = key;
                    }
                    else if ((((flag && (num13 == num9)) && (string.Compare(keyId, str12, StringComparison.OrdinalIgnoreCase) == 0)) || ((num13 == num7) && (string.Compare(keyId, str10, StringComparison.OrdinalIgnoreCase) == 0))) || ((num13 == num8) && (string.Compare(keyId, str11, StringComparison.OrdinalIgnoreCase) == 0)))
                    {
                        this.xsdKey = key;
                    }
                    else if ((flag && (num13 == num10)) && (string.Compare(keyId, str13, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        this.objectReader.SetVersion(1, 0);
                    }
                    else if (flag)
                    {
                        this.assemKeyToInteropAssemblyTable[key] = keyId;
                    }
                    else if (((string.Compare(prefix, this.soapKey, StringComparison.OrdinalIgnoreCase) != 0) && this.assemKeyToInteropAssemblyTable.ContainsKey(prefix)) && ((string) this.assemKeyToInteropAssemblyTable[prefix]).Equals(urn))
                    {
                        this.ProcessXmlAttribute(prefix, key, keyId, objectPr);
                    }
                }
            }
            this.attributeValues.Clear();
            if (this.headerState != HeaderStateEnum.None)
            {
                if (objectPr.PRparseTypeEnum == InternalParseTypeE.Headers)
                {
                    if (pr.PRisHeaderRoot || (this.headerState == HeaderStateEnum.FirstHeaderRecord))
                    {
                        this.headerState = HeaderStateEnum.HeaderRecord;
                    }
                    else
                    {
                        this.headerState = HeaderStateEnum.TopLevelObject;
                        this.currentState = InternalParseStateE.Object;
                        pr.PRobjectTypeEnum = InternalObjectTypeE.Object;
                        pr.PRparseTypeEnum = InternalParseTypeE.Object;
                        pr.PRparseStateEnum = InternalParseStateE.Object;
                        pr.PRmemberTypeEnum = InternalMemberTypeE.Empty;
                        pr.PRmemberValueEnum = InternalMemberValueE.Empty;
                    }
                }
                else if (objectPr.PRisHeaderRoot)
                {
                    this.headerState = HeaderStateEnum.NestedObject;
                }
            }
            if ((!this.isTopFound && (objectPr != null)) && (objectPr.PRparseTypeEnum == InternalParseTypeE.Body))
            {
                pr.PRobjectPositionEnum = InternalObjectPositionE.Top;
                this.isTopFound = true;
            }
            else if (pr.PRobjectPositionEnum != InternalObjectPositionE.Top)
            {
                pr.PRobjectPositionEnum = InternalObjectPositionE.Child;
            }
            if ((((pr.PRparseTypeEnum != InternalParseTypeE.Envelope) && (pr.PRparseTypeEnum != InternalParseTypeE.Body)) && (pr.PRparseTypeEnum != InternalParseTypeE.Headers)) && (((pr.PRobjectPositionEnum != InternalObjectPositionE.Top) || !this.objectReader.IsFakeTopObject) || pr.PRnameXmlKey.Equals(this.soapEnvKey)))
            {
                this.ProcessType(pr, objectPr);
            }
            if (dimString != null)
            {
                int num16;
                string str24;
                InternalArrayTypeE ee;
                pr.PRpositionA = this.ParseArrayDimensions(dimString, out num16, out str24, out ee);
            }
            if (str2 != null)
            {
                int num17;
                string str25;
                InternalArrayTypeE ee2;
                pr.PRlowerBoundA = this.ParseArrayDimensions(str2, out num17, out str25, out ee2);
            }
            if (str3 != null)
            {
                if (!str3.Equals("1"))
                {
                    if (!str3.Equals("0"))
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_MustUnderstand"), new object[] { str3 }));
                    }
                    pr.PRisMustUnderstand = false;
                }
                else
                {
                    pr.PRisMustUnderstand = true;
                }
            }
            if (pr.PRparseTypeEnum == InternalParseTypeE.Member)
            {
                if (objectPr.PRparseTypeEnum == InternalParseTypeE.Headers)
                {
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Header;
                }
                else if (objectPr.PRobjectTypeEnum == InternalObjectTypeE.Array)
                {
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Item;
                }
                else
                {
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Field;
                }
            }
        }

        private Type ProcessGetType(string value, string xmlKey, out string assemblyString)
        {
            Type interopType = null;
            string typeString = null;
            assemblyString = null;
            string httpstring = (string) this.keyToNamespaceTable["xmlns:" + xmlKey];
            if (httpstring != null)
            {
                interopType = this.GetInteropType(value, httpstring);
                if (interopType != null)
                {
                    return interopType;
                }
            }
            if (((string.Compare(value, "anyType", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(value, "ur-type", StringComparison.OrdinalIgnoreCase) == 0)) && xmlKey.Equals(this.xsdKey))
            {
                interopType = SoapUtil.typeofObject;
            }
            else if (xmlKey.Equals(this.xsdKey) || xmlKey.Equals(this.soapKey))
            {
                if (string.Compare(value, "string", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    interopType = SoapUtil.typeofString;
                }
                else
                {
                    InternalPrimitiveTypeE code = Converter.ToCode(value);
                    if (code == InternalPrimitiveTypeE.Invalid)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Parser_xsd"), new object[] { value }));
                    }
                    interopType = Converter.SoapToType(code);
                }
            }
            else
            {
                if (xmlKey == null)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Parser_xml"), new object[] { value }));
                }
                string str3 = (string) this.assemKeyToNameSpaceTable[xmlKey];
                typeString = null;
                if ((str3 == null) || (str3.Length == 0))
                {
                    typeString = value;
                }
                else
                {
                    typeString = str3 + "." + value;
                }
                SoapAssemblyInfo info = (SoapAssemblyInfo) this.assemKeyToAssemblyTable[xmlKey];
                if (info == null)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Parser_xmlAssembly"), new object[] { xmlKey + " " + value }));
                }
                assemblyString = info.assemblyString;
                if (assemblyString != null)
                {
                    interopType = this.objectReader.Bind(assemblyString, typeString);
                    if (interopType == null)
                    {
                        interopType = this.objectReader.FastBindToType(assemblyString, typeString);
                    }
                }
                if (interopType == null)
                {
                    Assembly assem = null;
                    try
                    {
                        assem = info.GetAssembly(this.objectReader);
                    }
                    catch
                    {
                    }
                    if (assem == null)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Parser_xmlAssembly"), new object[] { xmlKey + ":" + httpstring + " " + value }));
                    }
                    interopType = FormatterServices.GetTypeFromAssembly(assem, typeString);
                }
            }
            if (interopType == null)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Parser_xmlType"), new object[] { xmlKey + " " + typeString + " " + assemblyString }));
            }
            return interopType;
        }

        private void ProcessHeader(ParseRecord pr)
        {
            if (this.headerList == null)
            {
                this.headerList = new ArrayList(10);
            }
            ParseRecord record = this.GetPr();
            record.PRparseTypeEnum = InternalParseTypeE.Object;
            record.PRobjectTypeEnum = InternalObjectTypeE.Array;
            record.PRobjectPositionEnum = InternalObjectPositionE.Headers;
            record.PRarrayTypeEnum = InternalArrayTypeE.Single;
            record.PRarrayElementType = typeof(Header);
            record.PRisArrayVariant = false;
            record.PRarrayElementTypeCode = InternalPrimitiveTypeE.Invalid;
            record.PRrank = 1;
            record.PRlengthA = new int[1];
            this.headerList.Add(record);
        }

        private void ProcessHeaderEnd(ParseRecord pr)
        {
            if (this.headerList != null)
            {
                ParseRecord record = this.GetPr();
                record.PRparseTypeEnum = InternalParseTypeE.ObjectEnd;
                record.PRobjectTypeEnum = InternalObjectTypeE.Array;
                this.headerList.Add(record);
                record = (ParseRecord) this.headerList[0];
                record = (ParseRecord) this.headerList[0];
                record.PRlengthA[0] = this.headerArrayLength;
                record.PRobjectPositionEnum = InternalObjectPositionE.Headers;
                for (int i = 0; i < this.headerList.Count; i++)
                {
                    this.objectReader.Parse((ParseRecord) this.headerList[i]);
                }
                for (int j = 0; j < this.headerList.Count; j++)
                {
                    this.PutPr((ParseRecord) this.headerList[j]);
                }
            }
        }

        private void ProcessHeaderMember(ParseRecord pr)
        {
            if (this.headerState == HeaderStateEnum.NestedObject)
            {
                ParseRecord record2 = pr.Copy();
                this.headerList.Add(record2);
            }
            else
            {
                ParseRecord record = this.GetPr();
                record.PRparseTypeEnum = InternalParseTypeE.Member;
                record.PRmemberTypeEnum = InternalMemberTypeE.Item;
                record.PRmemberValueEnum = InternalMemberValueE.Nested;
                record.PRisHeaderRoot = true;
                this.headerArrayLength++;
                this.headerList.Add(record);
                record = this.GetPr();
                record.PRparseTypeEnum = InternalParseTypeE.Member;
                record.PRmemberTypeEnum = InternalMemberTypeE.Field;
                record.PRmemberValueEnum = InternalMemberValueE.InlineValue;
                record.PRisHeaderRoot = true;
                record.PRname = "Name";
                record.PRvalue = pr.PRname;
                record.PRdtType = SoapUtil.typeofString;
                record.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
                this.headerList.Add(record);
                record = this.GetPr();
                record.PRparseTypeEnum = InternalParseTypeE.Member;
                record.PRmemberTypeEnum = InternalMemberTypeE.Field;
                record.PRmemberValueEnum = InternalMemberValueE.InlineValue;
                record.PRisHeaderRoot = true;
                record.PRname = "HeaderNamespace";
                record.PRvalue = pr.PRxmlNameSpace;
                record.PRdtType = SoapUtil.typeofString;
                record.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
                this.headerList.Add(record);
                record = this.GetPr();
                record.PRparseTypeEnum = InternalParseTypeE.Member;
                record.PRmemberTypeEnum = InternalMemberTypeE.Field;
                record.PRmemberValueEnum = InternalMemberValueE.InlineValue;
                record.PRisHeaderRoot = true;
                record.PRname = "MustUnderstand";
                if (pr.PRisMustUnderstand)
                {
                    record.PRvarValue = true;
                }
                else
                {
                    record.PRvarValue = false;
                }
                record.PRdtType = SoapUtil.typeofBoolean;
                record.PRdtTypeCode = InternalPrimitiveTypeE.Boolean;
                this.headerList.Add(record);
                record = this.GetPr();
                record.PRparseTypeEnum = InternalParseTypeE.Member;
                record.PRmemberTypeEnum = InternalMemberTypeE.Field;
                record.PRmemberValueEnum = pr.PRmemberValueEnum;
                record.PRisHeaderRoot = true;
                record.PRname = "Value";
                switch (pr.PRmemberValueEnum)
                {
                    case InternalMemberValueE.InlineValue:
                        record.PRvalue = pr.PRvalue;
                        record.PRvarValue = pr.PRvarValue;
                        record.PRdtType = pr.PRdtType;
                        record.PRdtTypeCode = pr.PRdtTypeCode;
                        record.PRkeyDt = pr.PRkeyDt;
                        this.headerList.Add(record);
                        this.ProcessHeaderMemberEnd(pr);
                        return;

                    case InternalMemberValueE.Nested:
                        record.PRdtType = pr.PRdtType;
                        record.PRdtTypeCode = pr.PRdtTypeCode;
                        record.PRkeyDt = pr.PRkeyDt;
                        this.headerList.Add(record);
                        return;

                    case InternalMemberValueE.Reference:
                        record.PRidRef = pr.PRidRef;
                        this.headerList.Add(record);
                        this.ProcessHeaderMemberEnd(pr);
                        return;

                    case InternalMemberValueE.Null:
                        this.headerList.Add(record);
                        this.ProcessHeaderMemberEnd(pr);
                        return;
                }
            }
        }

        private void ProcessHeaderMemberEnd(ParseRecord pr)
        {
            ParseRecord record = null;
            if (this.headerState == HeaderStateEnum.NestedObject)
            {
                ParseRecord record2 = pr.Copy();
                this.headerList.Add(record2);
            }
            else
            {
                record = this.GetPr();
                record.PRparseTypeEnum = InternalParseTypeE.MemberEnd;
                record.PRmemberTypeEnum = InternalMemberTypeE.Field;
                record.PRmemberValueEnum = pr.PRmemberValueEnum;
                record.PRisHeaderRoot = true;
                this.headerList.Add(record);
                record = this.GetPr();
                record.PRparseTypeEnum = InternalParseTypeE.MemberEnd;
                record.PRmemberTypeEnum = InternalMemberTypeE.Item;
                record.PRmemberValueEnum = InternalMemberValueE.Nested;
                record.PRisHeaderRoot = true;
                this.headerList.Add(record);
            }
        }

        private void ProcessType(ParseRecord pr, ParseRecord objectPr)
        {
            if (pr.PRdtType == null)
            {
                if (pr.PRnameXmlKey.Equals(this.soapEnvKey) && (string.Compare(pr.PRname, "Fault", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    pr.PRdtType = SoapUtil.typeofSoapFault;
                    pr.PRparseTypeEnum = InternalParseTypeE.Object;
                }
                else if (pr.PRname != null)
                {
                    string xmlNamespace = null;
                    if ((pr.PRnameXmlKey != null) && (pr.PRnameXmlKey.Length > 0))
                    {
                        xmlNamespace = (string) this.assemKeyToInteropAssemblyTable[pr.PRnameXmlKey];
                    }
                    Type type = null;
                    string name = null;
                    if (objectPr != null)
                    {
                        if (pr.PRisXmlAttribute)
                        {
                            SoapServices.GetInteropFieldTypeAndNameFromXmlAttribute(objectPr.PRdtType, pr.PRname, xmlNamespace, out type, out name);
                        }
                        else
                        {
                            SoapServices.GetInteropFieldTypeAndNameFromXmlElement(objectPr.PRdtType, pr.PRname, xmlNamespace, out type, out name);
                        }
                    }
                    if (type != null)
                    {
                        pr.PRdtType = type;
                        pr.PRname = name;
                        pr.PRdtTypeCode = Converter.SoapToCode(pr.PRdtType);
                    }
                    else
                    {
                        if (xmlNamespace != null)
                        {
                            pr.PRdtType = this.objectReader.Bind(xmlNamespace, pr.PRname);
                        }
                        if (pr.PRdtType == null)
                        {
                            pr.PRdtType = SoapServices.GetInteropTypeFromXmlElement(pr.PRname, xmlNamespace);
                        }
                        if ((((pr.PRkeyDt == null) && (pr.PRnameXmlKey != null)) && ((pr.PRnameXmlKey.Length > 0) && (objectPr.PRobjectTypeEnum == InternalObjectTypeE.Array))) && (objectPr.PRarrayElementType == Converter.typeofObject))
                        {
                            pr.PRdtType = this.ProcessGetType(pr.PRname, pr.PRnameXmlKey, out pr.PRassemblyName);
                            pr.PRdtTypeCode = Converter.SoapToCode(pr.PRdtType);
                        }
                    }
                }
                if (pr.PRdtType == null)
                {
                    if ((((pr.PRtypeXmlKey != null) && (pr.PRtypeXmlKey.Length > 0)) && ((pr.PRkeyDt != null) && (pr.PRkeyDt.Length > 0))) && this.assemKeyToInteropAssemblyTable.ContainsKey(pr.PRtypeXmlKey))
                    {
                        int index = pr.PRkeyDt.IndexOf("[");
                        if (index > 0)
                        {
                            this.ProcessArray(pr, index, true);
                        }
                        else
                        {
                            string assemblyString = (string) this.assemKeyToInteropAssemblyTable[pr.PRtypeXmlKey];
                            pr.PRdtType = this.objectReader.Bind(assemblyString, pr.PRkeyDt);
                            if (pr.PRdtType == null)
                            {
                                pr.PRdtType = SoapServices.GetInteropTypeFromXmlType(pr.PRkeyDt, assemblyString);
                                if (pr.PRdtType == null)
                                {
                                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_TypeElement"), new object[] { pr.PRname + " " + pr.PRkeyDt }));
                                }
                            }
                            if (pr.PRdtType == null)
                            {
                                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_TypeElement"), new object[] { pr.PRname + " " + pr.PRkeyDt + ", " + assemblyString }));
                            }
                        }
                    }
                    else if (pr.PRkeyDt != null)
                    {
                        if (string.Compare(pr.PRkeyDt, "Base64", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            pr.PRobjectTypeEnum = InternalObjectTypeE.Array;
                            pr.PRarrayTypeEnum = InternalArrayTypeE.Base64;
                        }
                        else if (string.Compare(pr.PRkeyDt, "String", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            pr.PRdtType = SoapUtil.typeofString;
                        }
                        else
                        {
                            if (string.Compare(pr.PRkeyDt, "methodSignature", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                try
                                {
                                    pr.PRdtType = typeof(Type[]);
                                    char[] separator = new char[] { ' ', ':' };
                                    string[] strArray = null;
                                    if (pr.PRvalue != null)
                                    {
                                        strArray = pr.PRvalue.Split(separator);
                                    }
                                    Type[] typeArray = null;
                                    if ((strArray == null) || ((strArray.Length == 1) && (strArray[0].Length == 0)))
                                    {
                                        typeArray = new Type[0];
                                    }
                                    else
                                    {
                                        typeArray = new Type[strArray.Length / 2];
                                        for (int i = 0; i < strArray.Length; i += 2)
                                        {
                                            string xmlKey = strArray[i];
                                            string str5 = strArray[i + 1];
                                            typeArray[i / 2] = this.ProcessGetType(str5, xmlKey, out pr.PRassemblyName);
                                        }
                                    }
                                    pr.PRvarValue = typeArray;
                                    return;
                                }
                                catch
                                {
                                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_MethodSignature"), new object[] { pr.PRvalue }));
                                }
                            }
                            pr.PRdtTypeCode = Converter.ToCode(pr.PRkeyDt);
                            if (pr.PRdtTypeCode != InternalPrimitiveTypeE.Invalid)
                            {
                                pr.PRdtType = Converter.SoapToType(pr.PRdtTypeCode);
                            }
                            else
                            {
                                int firstIndex = pr.PRkeyDt.IndexOf("[");
                                if (firstIndex > 0)
                                {
                                    this.ProcessArray(pr, firstIndex, false);
                                }
                                else
                                {
                                    pr.PRobjectTypeEnum = InternalObjectTypeE.Object;
                                    pr.PRdtType = this.ProcessGetType(pr.PRkeyDt, pr.PRtypeXmlKey, out pr.PRassemblyName);
                                    if ((pr.PRdtType == null) && (pr.PRobjectPositionEnum != InternalObjectPositionE.Top))
                                    {
                                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_TypeElement"), new object[] { pr.PRname + " " + pr.PRkeyDt }));
                                    }
                                }
                            }
                        }
                    }
                    else if ((pr.PRparseTypeEnum == InternalParseTypeE.Object) && (!this.objectReader.IsFakeTopObject || (pr.PRobjectPositionEnum != InternalObjectPositionE.Top)))
                    {
                        if (string.Compare(pr.PRname, "Array", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            pr.PRdtType = this.ProcessGetType(pr.PRkeyDt, pr.PRtypeXmlKey, out pr.PRassemblyName);
                        }
                        else
                        {
                            pr.PRdtType = this.ProcessGetType(pr.PRname, pr.PRnameXmlKey, out pr.PRassemblyName);
                        }
                    }
                }
            }
        }

        private void ProcessXmlAttribute(string prefix, string key, string value, ParseRecord objectPr)
        {
            if (this.xmlAttributeList == null)
            {
                this.xmlAttributeList = new ArrayList(10);
            }
            ParseRecord pr = this.GetPr();
            pr.PRparseTypeEnum = InternalParseTypeE.Member;
            pr.PRmemberTypeEnum = InternalMemberTypeE.Field;
            pr.PRmemberValueEnum = InternalMemberValueE.InlineValue;
            pr.PRname = key;
            pr.PRvalue = value;
            pr.PRnameXmlKey = prefix;
            pr.PRisXmlAttribute = true;
            this.ProcessType(pr, objectPr);
            this.xmlAttributeList.Add(pr);
        }

        private void PutPr(ParseRecord pr)
        {
            this.prPool.Push(pr);
        }

        private static string SerTraceString(string handler, ParseRecord pr, string value, InternalParseStateE currentState, HeaderStateEnum headerState)
        {
            string str = "";
            if (value != null)
            {
                str = value;
            }
            string str2 = "";
            if (pr != null)
            {
                str2 = pr.PRparseStateEnum.ToString();
            }
            return (handler + " - " + str + ", State " + currentState.ToString() + ", PushState " + str2);
        }

        internal void Start(XmlTextReader p)
        {
            this.currentState = InternalParseStateE.Object;
            this.xmlTextReader = p;
        }

        internal void StartChildren()
        {
            ParseRecord pr = null;
            switch (this.currentState)
            {
                case InternalParseStateE.Object:
                {
                    pr = (ParseRecord) this.stack.Peek();
                    ParseRecord objectPr = (ParseRecord) this.stack.PeekPeek();
                    this.ProcessAttributes(pr, objectPr);
                    if (pr.PRarrayTypeEnum != InternalArrayTypeE.Base64)
                    {
                        if ((pr.PRparseTypeEnum != InternalParseTypeE.Envelope) && (pr.PRparseTypeEnum != InternalParseTypeE.Body))
                        {
                            this.currentState = InternalParseStateE.Member;
                        }
                        switch (this.headerState)
                        {
                            case HeaderStateEnum.None:
                            case HeaderStateEnum.TopLevelObject:
                                if ((!this.isTopFound && (objectPr != null)) && (objectPr.PRparseTypeEnum == InternalParseTypeE.Body))
                                {
                                    pr.PRobjectPositionEnum = InternalObjectPositionE.Top;
                                    this.isTopFound = true;
                                }
                                this.objectReader.Parse(pr);
                                pr.PRisParsed = true;
                                return;

                            case HeaderStateEnum.FirstHeaderRecord:
                            case HeaderStateEnum.HeaderRecord:
                            case HeaderStateEnum.NestedObject:
                                this.ProcessHeader(pr);
                                return;
                        }
                    }
                    return;
                }
                case InternalParseStateE.Member:
                    pr = (ParseRecord) this.stack.Peek();
                    this.currentState = InternalParseStateE.MemberChild;
                    return;
            }
            this.MarshalError("StartChildren", (ParseRecord) this.stack.Peek(), null, this.currentState);
        }

        internal void StartElement(string prefix, string name, string urn)
        {
            ParseRecord record2;
            string str = this.NameFilter(name);
            string str2 = prefix;
            ParseRecord pr = null;
            if (((urn != null) && (urn.Length != 0)) && ((prefix == null) || (prefix.Length == 0)))
            {
                if (this.nameSpaceToKey.ContainsKey(urn))
                {
                    str2 = (string) this.nameSpaceToKey[urn];
                }
                else
                {
                    str2 = this.NextPrefix();
                    this.nameSpaceToKey[urn] = str2;
                }
            }
            switch (this.currentState)
            {
                case InternalParseStateE.Object:
                    pr = this.GetPr();
                    pr.PRname = str;
                    pr.PRnameXmlKey = str2;
                    pr.PRxmlNameSpace = urn;
                    pr.PRparseStateEnum = InternalParseStateE.Object;
                    if ((string.Compare(name, "Array", StringComparison.OrdinalIgnoreCase) != 0) || !str2.Equals(this.soapKey))
                    {
                        if (((string.Compare(name, "anyType", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(name, "ur-type", StringComparison.OrdinalIgnoreCase) == 0)) && str2.Equals(this.xsdKey))
                        {
                            pr.PRname = "System.Object";
                            pr.PRnameXmlKey = this.urtKey;
                            pr.PRxmlNameSpace = urn;
                            pr.PRparseTypeEnum = InternalParseTypeE.Object;
                        }
                        else if (string.Compare(urn, "http://schemas.xmlsoap.org/soap/envelope/", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (string.Compare(name, "Envelope", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (this.isEnvelope)
                                {
                                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Parser_Envelope"), new object[] { prefix + ":" + name }));
                                }
                                this.isEnvelope = true;
                                pr.PRparseTypeEnum = InternalParseTypeE.Envelope;
                            }
                            else if (string.Compare(name, "Body", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (!this.isEnvelope)
                                {
                                    throw new SerializationException(SoapUtil.GetResourceString("Serialization_Parser_BodyChild"));
                                }
                                if (this.isBody)
                                {
                                    throw new SerializationException(SoapUtil.GetResourceString("Serialization_Parser_BodyOnce"));
                                }
                                this.isBody = true;
                                this.headerState = HeaderStateEnum.None;
                                this.isTopFound = false;
                                pr.PRparseTypeEnum = InternalParseTypeE.Body;
                            }
                            else if (string.Compare(name, "Header", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (!this.isEnvelope)
                                {
                                    throw new SerializationException(SoapUtil.GetResourceString("Serialization_Parser_Header"));
                                }
                                pr.PRparseTypeEnum = InternalParseTypeE.Headers;
                                this.headerState = HeaderStateEnum.FirstHeaderRecord;
                            }
                            else
                            {
                                pr.PRparseTypeEnum = InternalParseTypeE.Object;
                            }
                        }
                        else
                        {
                            pr.PRparseTypeEnum = InternalParseTypeE.Object;
                        }
                        break;
                    }
                    pr.PRparseTypeEnum = InternalParseTypeE.Object;
                    break;

                case InternalParseStateE.Member:
                    pr = this.GetPr();
                    record2 = (ParseRecord) this.stack.Peek();
                    pr.PRname = str;
                    pr.PRnameXmlKey = str2;
                    pr.PRxmlNameSpace = urn;
                    pr.PRparseTypeEnum = InternalParseTypeE.Member;
                    pr.PRparseStateEnum = InternalParseStateE.Member;
                    this.stack.Push(pr);
                    return;

                case InternalParseStateE.MemberChild:
                {
                    record2 = (ParseRecord) this.stack.PeekPeek();
                    pr = (ParseRecord) this.stack.Peek();
                    pr.PRmemberValueEnum = InternalMemberValueE.Nested;
                    this.ProcessAttributes(pr, record2);
                    switch (this.headerState)
                    {
                        case HeaderStateEnum.None:
                        case HeaderStateEnum.TopLevelObject:
                            this.objectReader.Parse(pr);
                            pr.PRisParsed = true;
                            break;

                        case HeaderStateEnum.HeaderRecord:
                        case HeaderStateEnum.NestedObject:
                            this.ProcessHeaderMember(pr);
                            break;
                    }
                    ParseRecord record3 = this.GetPr();
                    record3.PRparseTypeEnum = InternalParseTypeE.Member;
                    record3.PRparseStateEnum = InternalParseStateE.Member;
                    record3.PRname = str;
                    record3.PRnameXmlKey = str2;
                    pr.PRxmlNameSpace = urn;
                    this.currentState = InternalParseStateE.Member;
                    this.stack.Push(record3);
                    return;
                }
                default:
                    this.MarshalError("StartElement", (ParseRecord) this.stack.Peek(), str, this.currentState);
                    return;
            }
            this.stack.Push(pr);
        }

        internal void Text(string text)
        {
            this.textValue = text;
        }

        internal class AttributeValueEntry
        {
            internal string key;
            internal string prefix;
            internal string urn;
            internal string value;

            internal AttributeValueEntry(string prefix, string key, string value, string urn)
            {
                this.prefix = prefix;
                this.key = key;
                this.value = value;
                this.urn = urn;
            }
        }

        [Serializable]
        private enum HeaderStateEnum
        {
            None,
            FirstHeaderRecord,
            HeaderRecord,
            NestedObject,
            TopLevelObject
        }
    }
}

