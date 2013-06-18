namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    internal sealed class SoapWriter
    {
        private static byte[] _soapStart = Encoding.UTF8.GetBytes(_soapStartStr);
        private static byte[] _soapStart1999 = Encoding.UTF8.GetBytes(_soapStart1999Str);
        private static string _soapStart1999Str = "<SOAP-ENV:Envelope xmlns:xsi=\"http://www.w3.org/1999/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/1999/XMLSchema\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"";
        private static byte[] _soapStart2000 = Encoding.UTF8.GetBytes(_soapStart2000Str);
        private static string _soapStart2000Str = "<SOAP-ENV:Envelope xmlns:xsi=\"http://www.w3.org/2000/10/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2000/10/XMLSchema\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"";
        private static string _soapStartStr = "<SOAP-ENV:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:clr=\"http://schemas.microsoft.com/soap/encoding/clr/1.0\" SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"";
        private Hashtable assemblyInfos = new Hashtable(10);
        private Hashtable assemblyInfoUsed = new Hashtable(10);
        private AttributeList attrList = new AttributeList();
        private AttributeList attrValueList = new AttributeList();
        private int dottedAssemId = 1;
        private Hashtable dottedAssemToAssemIdTable;
        public static Dictionary<char, string> encodingTable = new Dictionary<char, string>();
        private int headerId;
        private int instanceIndent = 1;
        internal bool isUsedEnc;
        private int lineIndent = 4;
        private NameCache nameCache = new NameCache();
        private StringBuilder sb = new StringBuilder(120);
        private StringBuilder sb1 = new StringBuilder("ref-", 15);
        private StringBuilder sb2 = new StringBuilder("a-", 15);
        private StringBuilder sb3 = new StringBuilder("i-", 15);
        private StringBuilder sb4 = new StringBuilder("#ref-", 0x10);
        private StringBuilder sbOffset = new StringBuilder(10);
        private Stream stream;
        private StringBuilder stringBuffer = new StringBuilder(120);
        private const int StringBuilderSize = 0x400;
        private int topId;
        private StringBuilder traceBuffer;
        private Hashtable typeNameToDottedInfoTable;
        private StreamWriter writer;
        private XsdVersion xsdVersion = XsdVersion.V2001;

        static SoapWriter()
        {
            encodingTable.Add('&', "&#38;");
            encodingTable.Add('"', "&#34;");
            encodingTable.Add('\'', "&#39;");
            encodingTable.Add('<', "&#60;");
            encodingTable.Add('>', "&#62;");
            encodingTable.Add('\0', "&#0;");
            encodingTable.Add('\v', "&#xB;");
            encodingTable.Add('\f', "&#xC;");
            for (int i = 1; i < 9; i++)
            {
                encodingTable.Add(((IConvertible) i).ToChar(NumberFormatInfo.InvariantInfo), "&#x" + i.ToString("X", CultureInfo.InvariantCulture) + ";");
            }
            for (int j = 14; j < 0x20; j++)
            {
                encodingTable.Add(((IConvertible) j).ToChar(NumberFormatInfo.InvariantInfo), "&#x" + j.ToString("X", CultureInfo.InvariantCulture) + ";");
            }
            for (int k = 0x7f; k < 0x85; k++)
            {
                encodingTable.Add(((IConvertible) k).ToChar(NumberFormatInfo.InvariantInfo), "&#x" + k.ToString("X", CultureInfo.InvariantCulture) + ";");
            }
            for (int m = 0x86; m < 160; m++)
            {
                encodingTable.Add(((IConvertible) m).ToChar(NumberFormatInfo.InvariantInfo), "&#x" + m.ToString("X", CultureInfo.InvariantCulture) + ";");
            }
        }

        internal SoapWriter(Stream stream)
        {
            this.stream = stream;
            UTF8Encoding encoding = new UTF8Encoding(false, true);
            this.writer = new StreamWriter(stream, encoding, 0x400);
            this.typeNameToDottedInfoTable = new Hashtable(20);
            this.dottedAssemToAssemIdTable = new Hashtable(20);
        }

        private string AssemIdToString(int assemId)
        {
            this.sb2.Length = 1;
            this.sb2.Append(assemId);
            return this.sb2.ToString();
        }

        private string DottedDimensionName(string dottedName, string dimensionName)
        {
            int index = dottedName.IndexOf('[');
            int startIndex = dimensionName.IndexOf('[');
            return (dottedName.Substring(0, index) + dimensionName.Substring(startIndex));
        }

        [Conditional("_DEBUG")]
        private void EmitIndent(int count)
        {
            while (--count >= 0)
            {
                for (int i = 0; i < this.lineIndent; i++)
                {
                    this.writer.Write(' ');
                }
            }
        }

        private void EmitLine()
        {
            this.writer.Write("\r\n");
        }

        [Conditional("_DEBUG")]
        private void EmitLine(int indent, string value)
        {
            this.writer.Write(value);
            this.EmitLine();
        }

        private string Escape(string value)
        {
            this.stringBuffer.Length = 0;
            foreach (char ch in value)
            {
                if (encodingTable.ContainsKey(ch))
                {
                    this.stringBuffer.Append(encodingTable[ch]);
                }
                else
                {
                    this.stringBuffer.Append(ch);
                }
            }
            if (this.stringBuffer.Length > 0)
            {
                return this.stringBuffer.ToString();
            }
            return value;
        }

        private string IdToString(int objectId)
        {
            this.sb1.Length = 4;
            this.sb1.Append(objectId);
            return this.sb1.ToString();
        }

        internal void InternalWrite(string s)
        {
            this.writer.Write(s);
        }

        private string InteropAssemIdToString(int assemId)
        {
            this.sb3.Length = 1;
            this.sb3.Append(assemId);
            return this.sb3.ToString();
        }

        private string MemberElementName(NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
            string nIname = memberNameInfo.NIname;
            if (memberNameInfo.NIisHeader)
            {
                return (memberNameInfo.NIheaderPrefix + ":" + memberNameInfo.NIname);
            }
            if ((typeNameInfo != null) && (typeNameInfo.NItype == SoapUtil.typeofSoapFault))
            {
                return "SOAP-ENV:Fault";
            }
            if (memberNameInfo.NIisArray && !memberNameInfo.NIisNestedObject)
            {
                nIname = "SOAP-ENC:Array";
                this.isUsedEnc = true;
                return nIname;
            }
            if (memberNameInfo.NIisArrayItem)
            {
                return "item";
            }
            if ((!memberNameInfo.NIisNestedObject && (!memberNameInfo.NIisRemoteRecord || memberNameInfo.NIisTopLevelObject)) && (typeNameInfo != null))
            {
                nIname = this.NameTagResolver(typeNameInfo, true);
            }
            return nIname;
        }

        private string NameEscape(string name)
        {
            string cachedValue = (string) this.nameCache.GetCachedValue(name);
            if (cachedValue == null)
            {
                cachedValue = XmlConvert.EncodeName(name);
                this.nameCache.SetCachedValue(cachedValue);
            }
            return cachedValue;
        }

        private void NamespaceAttribute()
        {
            IDictionaryEnumerator enumerator = this.assemblyInfoUsed.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssemblyInfo key = (AssemblyInfo) enumerator.Key;
                this.attrList.Put("xmlns:" + key.prefix, key.name);
            }
            this.assemblyInfoUsed.Clear();
        }

        private string NameTagResolver(NameInfo typeNameInfo, bool isXsiAppended)
        {
            return this.NameTagResolver(typeNameInfo, isXsiAppended, null);
        }

        private string NameTagResolver(NameInfo typeNameInfo, bool isXsiAppended, string arrayItemName)
        {
            string nIname = typeNameInfo.NIname;
            switch (typeNameInfo.NInameSpaceEnum)
            {
                case InternalNameSpaceE.None:
                case InternalNameSpaceE.UserNameSpace:
                case InternalNameSpaceE.MemberName:
                    return nIname;

                case InternalNameSpaceE.Soap:
                    nIname = "SOAP-ENC:" + typeNameInfo.NIname;
                    this.isUsedEnc = true;
                    return nIname;

                case InternalNameSpaceE.XdrPrimitive:
                    if (isXsiAppended)
                    {
                        nIname = "xsd:" + typeNameInfo.NIname;
                    }
                    return nIname;

                case InternalNameSpaceE.XdrString:
                    if (isXsiAppended)
                    {
                        nIname = "xsd:" + typeNameInfo.NIname;
                    }
                    return nIname;

                case InternalNameSpaceE.UrtSystem:
                    if (!(typeNameInfo.NItype == SoapUtil.typeofObject))
                    {
                        DottedInfo info3;
                        if (arrayItemName == null)
                        {
                            DottedInfo info;
                            if (this.typeNameToDottedInfoTable.ContainsKey(typeNameInfo.NIname))
                            {
                                info = (DottedInfo) this.typeNameToDottedInfoTable[typeNameInfo.NIname];
                            }
                            else
                            {
                                info = this.ParseAssemblyName(typeNameInfo.NIname, null);
                            }
                            string str2 = this.AssemIdToString(info.assemId);
                            nIname = str2 + ":" + info.name;
                            AssemblyInfo info2 = (AssemblyInfo) this.assemblyInfos[str2];
                            info2.isUsed = true;
                            info2.prefix = str2;
                            this.assemblyInfoUsed[info2] = 1;
                            return nIname;
                        }
                        if (this.typeNameToDottedInfoTable.ContainsKey(arrayItemName))
                        {
                            info3 = (DottedInfo) this.typeNameToDottedInfoTable[arrayItemName];
                        }
                        else
                        {
                            info3 = this.ParseAssemblyName(arrayItemName, null);
                        }
                        string str3 = this.AssemIdToString(info3.assemId);
                        nIname = str3 + ":" + this.DottedDimensionName(info3.name, typeNameInfo.NIname);
                        AssemblyInfo info4 = (AssemblyInfo) this.assemblyInfos[str3];
                        info4.isUsed = true;
                        info4.prefix = str3;
                        this.assemblyInfoUsed[info4] = 1;
                        return nIname;
                    }
                    return "xsd:anyType";

                case InternalNameSpaceE.UrtUser:
                    if (typeNameInfo.NIassemId > 0L)
                    {
                        if (arrayItemName != null)
                        {
                            if (!this.typeNameToDottedInfoTable.ContainsKey(arrayItemName))
                            {
                                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Assembly"), new object[] { typeNameInfo.NIname }));
                            }
                            DottedInfo info7 = (DottedInfo) this.typeNameToDottedInfoTable[arrayItemName];
                            string str5 = this.AssemIdToString(info7.assemId);
                            nIname = str5 + ":" + this.DottedDimensionName(info7.name, typeNameInfo.NIname);
                            AssemblyInfo info8 = (AssemblyInfo) this.assemblyInfos[str5];
                            info8.isUsed = true;
                            info8.prefix = str5;
                            this.assemblyInfoUsed[info8] = 1;
                            return nIname;
                        }
                        if (!this.typeNameToDottedInfoTable.ContainsKey(typeNameInfo.NIname))
                        {
                            throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Assembly"), new object[] { typeNameInfo.NIname }));
                        }
                        DottedInfo info5 = (DottedInfo) this.typeNameToDottedInfoTable[typeNameInfo.NIname];
                        string str4 = this.AssemIdToString(info5.assemId);
                        nIname = str4 + ":" + info5.name;
                        AssemblyInfo info6 = (AssemblyInfo) this.assemblyInfos[str4];
                        info6.isUsed = true;
                        info6.prefix = str4;
                        this.assemblyInfoUsed[info6] = 1;
                    }
                    return nIname;

                case InternalNameSpaceE.Interop:
                    if ((typeNameInfo.NIattributeInfo != null) && (typeNameInfo.NIattributeInfo.AttributeElementName != null))
                    {
                        if (typeNameInfo.NIassemId <= 0L)
                        {
                            return typeNameInfo.NIattributeInfo.AttributeElementName;
                        }
                        string str7 = this.InteropAssemIdToString((int) typeNameInfo.NIassemId);
                        nIname = str7 + ":" + typeNameInfo.NIattributeInfo.AttributeElementName;
                        if (arrayItemName != null)
                        {
                            int index = typeNameInfo.NIname.IndexOf("[");
                            nIname = nIname + typeNameInfo.NIname.Substring(index);
                        }
                        AssemblyInfo info10 = (AssemblyInfo) this.assemblyInfos[str7];
                        info10.isUsed = true;
                        info10.prefix = str7;
                        this.assemblyInfoUsed[info10] = 1;
                    }
                    return nIname;

                case InternalNameSpaceE.CallElement:
                    if (typeNameInfo.NIassemId > 0L)
                    {
                        string str6 = this.InteropAssemIdToString((int) typeNameInfo.NIassemId);
                        AssemblyInfo info9 = (AssemblyInfo) this.assemblyInfos[str6];
                        if (info9 == null)
                        {
                            throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_NameSpaceEnum"), new object[] { typeNameInfo.NInameSpaceEnum }));
                        }
                        nIname = str6 + ":" + typeNameInfo.NIname;
                        info9.isUsed = true;
                        info9.prefix = str6;
                        this.assemblyInfoUsed[info9] = 1;
                        return nIname;
                    }
                    return nIname;
            }
            throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_NameSpaceEnum"), new object[] { typeNameInfo.NInameSpaceEnum }));
        }

        private DottedInfo ParseAssemblyName(string typeFullName, string assemName)
        {
            string typeNamespace = null;
            string str2 = null;
            string key = null;
            int num;
            if (this.typeNameToDottedInfoTable.ContainsKey(typeFullName))
            {
                return (DottedInfo) this.typeNameToDottedInfoTable[typeFullName];
            }
            int length = typeFullName.LastIndexOf('.');
            if (length > 0)
            {
                typeNamespace = typeFullName.Substring(0, length);
            }
            else
            {
                typeNamespace = "";
            }
            key = SoapServices.CodeXmlNamespaceForClrTypeNamespace(typeNamespace, assemName);
            str2 = typeFullName.Substring(length + 1);
            if (this.dottedAssemToAssemIdTable.ContainsKey(key))
            {
                num = (int) this.dottedAssemToAssemIdTable[key];
            }
            else
            {
                num = this.dottedAssemId++;
                this.assemblyInfos[this.AssemIdToString(num)] = new AssemblyInfo(num, key, false);
                this.dottedAssemToAssemIdTable[key] = num;
            }
            DottedInfo info = new DottedInfo {
                dottedAssemblyName = key,
                name = str2,
                nameSpace = typeNamespace,
                assemId = num
            };
            this.typeNameToDottedInfoTable[typeFullName] = info;
            return info;
        }

        private string RefToString(int objectId)
        {
            this.sb4.Length = 5;
            this.sb4.Append(objectId);
            return this.sb4.ToString();
        }

        internal void Reset()
        {
            this.writer = null;
            this.stringBuffer = null;
        }

        [Conditional("_LOGGING")]
        internal void TraceSoap(string s)
        {
            if (this.traceBuffer == null)
            {
                this.traceBuffer = new StringBuilder();
            }
            this.traceBuffer.Append(s);
        }

        private string TypeArrayNameTagResolver(NameInfo memberNameInfo, NameInfo typeNameInfo, bool isXsiAppended)
        {
            if (((typeNameInfo.NIassemId > 0L) && (typeNameInfo.NIattributeInfo != null)) && (typeNameInfo.NIattributeInfo.AttributeTypeName != null))
            {
                return (this.InteropAssemIdToString((int) typeNameInfo.NIassemId) + ":" + typeNameInfo.NIattributeInfo.AttributeTypeName);
            }
            return this.NameTagResolver(typeNameInfo, isXsiAppended, memberNameInfo.NIname);
        }

        private string TypeNameTagResolver(NameInfo typeNameInfo, bool isXsiAppended)
        {
            string str = null;
            if (((typeNameInfo.NIassemId > 0L) && (typeNameInfo.NIattributeInfo != null)) && (typeNameInfo.NIattributeInfo.AttributeTypeName != null))
            {
                string str2 = this.InteropAssemIdToString((int) typeNameInfo.NIassemId);
                str = str2 + ":" + typeNameInfo.NIattributeInfo.AttributeTypeName;
                AssemblyInfo info = (AssemblyInfo) this.assemblyInfos[str2];
                info.isUsed = true;
                info.prefix = str2;
                this.assemblyInfoUsed[info] = 1;
                return str;
            }
            return this.NameTagResolver(typeNameInfo, isXsiAppended);
        }

        internal void Write(InternalElementTypeE use, string name, AttributeList attrList, string value, bool isNameEscape, bool isValueEscape)
        {
            string s = name;
            if (isNameEscape)
            {
                s = this.NameEscape(name);
            }
            if (use == InternalElementTypeE.ObjectEnd)
            {
                this.instanceIndent--;
            }
            this.InternalWrite("<");
            if (use == InternalElementTypeE.ObjectEnd)
            {
                this.InternalWrite("/");
            }
            this.InternalWrite(s);
            this.WriteAttributeList(attrList);
            switch (use)
            {
                case InternalElementTypeE.ObjectBegin:
                    this.InternalWrite(">");
                    this.instanceIndent++;
                    break;

                case InternalElementTypeE.ObjectEnd:
                    this.InternalWrite(">");
                    break;

                case InternalElementTypeE.Member:
                    if (value != null)
                    {
                        this.InternalWrite(">");
                        if (isValueEscape)
                        {
                            this.InternalWrite(this.Escape(value));
                        }
                        else
                        {
                            this.InternalWrite(value);
                        }
                        this.InternalWrite("</");
                        this.InternalWrite(s);
                        this.InternalWrite(">");
                        break;
                    }
                    this.InternalWrite("/>");
                    break;

                default:
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_UseCode"), new object[] { use.ToString() }));
            }
            this.EmitLine();
        }

        internal void WriteAssembly(string typeFullName, Type type, string assemName, int assemId, bool isNew, bool isInteropType)
        {
            if (isNew && isInteropType)
            {
                this.assemblyInfos[this.InteropAssemIdToString(assemId)] = new AssemblyInfo(assemId, assemName, isInteropType);
            }
            if (!isInteropType)
            {
                this.ParseAssemblyName(typeFullName, assemName);
            }
        }

        private void WriteAttributeList(AttributeList attrList)
        {
            for (int i = 0; i < attrList.Count; i++)
            {
                string str;
                string str2;
                attrList.Get(i, out str, out str2);
                this.InternalWrite(" ");
                this.InternalWrite(str);
                this.InternalWrite("=");
                this.InternalWrite("\"");
                this.InternalWrite(str2);
                this.InternalWrite("\"");
            }
        }

        internal void WriteAttributeValue(NameInfo memberNameInfo, NameInfo typeNameInfo, object value)
        {
            string str = null;
            if (value is string)
            {
                str = (string) value;
            }
            else
            {
                str = Converter.SoapToString(value, typeNameInfo.NIprimitiveTypeEnum);
            }
            this.attrValueList.Put(this.MemberElementName(memberNameInfo, typeNameInfo), str);
        }

        internal void WriteBegin()
        {
        }

        internal void WriteEnd()
        {
            this.writer.Flush();
            this.Reset();
        }

        internal void WriteHeader(int objectId, int numMembers)
        {
            this.attrList.Clear();
            this.Write(InternalElementTypeE.ObjectBegin, "SOAP-ENV:Header", this.attrList, null, false, false);
        }

        internal void WriteHeaderArrayEnd()
        {
        }

        internal void WriteHeaderEntry(NameInfo nameInfo, NameInfo typeNameInfo, object value)
        {
            this.attrList.Clear();
            if (value == null)
            {
                this.attrList.Put("xsi:null", "1");
            }
            else
            {
                this.attrList.Put("xsi:type", this.TypeNameTagResolver(typeNameInfo, true));
            }
            if (nameInfo.NIisMustUnderstand)
            {
                this.attrList.Put("SOAP-ENV:mustUnderstand", "1");
                this.isUsedEnc = true;
            }
            this.attrList.Put("xmlns:" + nameInfo.NIheaderPrefix, nameInfo.NInamespace);
            this.attrList.Put("SOAP-ENC:root", "1");
            string str = null;
            if (value != null)
            {
                if ((typeNameInfo != null) && (typeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.QName))
                {
                    SoapQName name = (SoapQName) value;
                    if ((name.Key == null) || (name.Key.Length == 0))
                    {
                        this.attrList.Put("xmlns", "");
                    }
                    else
                    {
                        this.attrList.Put("xmlns:" + name.Key, name.Namespace);
                    }
                    str = name.ToString();
                }
                else
                {
                    str = Converter.SoapToString(value, typeNameInfo.NIprimitiveTypeEnum);
                }
            }
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.Member, nameInfo.NIheaderPrefix + ":" + nameInfo.NIname, this.attrList, str, true, true);
        }

        internal void WriteHeaderMethodSignature(NameInfo nameInfo, NameInfo[] typeNameInfos)
        {
            this.attrList.Clear();
            this.attrList.Put("xsi:type", "SOAP-ENC:methodSignature");
            this.isUsedEnc = true;
            if (nameInfo.NIisMustUnderstand)
            {
                this.attrList.Put("SOAP-ENV:mustUnderstand", "1");
            }
            this.attrList.Put("xmlns:" + nameInfo.NIheaderPrefix, nameInfo.NInamespace);
            this.attrList.Put("SOAP-ENC:root", "1");
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < typeNameInfos.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }
                builder.Append(this.NameTagResolver(typeNameInfos[i], true));
            }
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.Member, nameInfo.NIheaderPrefix + ":" + nameInfo.NIname, this.attrList, builder.ToString(), true, true);
        }

        internal void WriteHeaderObjectRef(NameInfo nameInfo)
        {
            this.attrList.Clear();
            this.attrList.Put("href", this.RefToString((int) nameInfo.NIobjectId));
            if (nameInfo.NIisMustUnderstand)
            {
                this.attrList.Put("SOAP-ENV:mustUnderstand", "1");
                this.isUsedEnc = true;
            }
            this.attrList.Put("xmlns:" + nameInfo.NIheaderPrefix, nameInfo.NInamespace);
            this.attrList.Put("SOAP-ENC:root", "1");
            this.Write(InternalElementTypeE.Member, nameInfo.NIheaderPrefix + ":" + nameInfo.NIname, this.attrList, null, true, true);
        }

        internal void WriteHeaderSectionEnd()
        {
            this.attrList.Clear();
            this.Write(InternalElementTypeE.ObjectEnd, "SOAP-ENV:Header", this.attrList, null, false, false);
        }

        internal void WriteHeaderString(NameInfo nameInfo, string value)
        {
            this.attrList.Clear();
            this.attrList.Put("xsi:type", "SOAP-ENC:string");
            this.isUsedEnc = true;
            if (nameInfo.NIisMustUnderstand)
            {
                this.attrList.Put("SOAP-ENV:mustUnderstand", "1");
            }
            this.attrList.Put("xmlns:" + nameInfo.NIheaderPrefix, nameInfo.NInamespace);
            this.attrList.Put("SOAP-ENC:root", "1");
            this.Write(InternalElementTypeE.Member, nameInfo.NIheaderPrefix + ":" + nameInfo.NIname, this.attrList, value, true, true);
        }

        internal void WriteItem(NameInfo itemNameInfo, NameInfo typeNameInfo, object value)
        {
            this.attrList.Clear();
            if (itemNameInfo.NItransmitTypeOnMember)
            {
                this.attrList.Put("xsi:type", this.TypeNameTagResolver(typeNameInfo, true));
            }
            string str = null;
            if (typeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.QName)
            {
                if (value != null)
                {
                    SoapQName name = (SoapQName) value;
                    if ((name.Key == null) || (name.Key.Length == 0))
                    {
                        this.attrList.Put("xmlns", "");
                    }
                    else
                    {
                        this.attrList.Put("xmlns:" + name.Key, name.Namespace);
                    }
                    str = name.ToString();
                }
            }
            else
            {
                str = Converter.SoapToString(value, typeNameInfo.NIprimitiveTypeEnum);
            }
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.Member, "item", this.attrList, str, false, typeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.Invalid);
        }

        internal void WriteItemObjectRef(NameInfo itemNameInfo, int arrayId)
        {
            this.attrList.Clear();
            this.attrList.Put("href", this.RefToString(arrayId));
            this.Write(InternalElementTypeE.Member, "item", this.attrList, null, false, false);
        }

        internal void WriteItemString(NameInfo itemNameInfo, NameInfo typeNameInfo, string value)
        {
            this.attrList.Clear();
            if (typeNameInfo.NIobjectId > 0L)
            {
                this.attrList.Put("id", this.IdToString((int) typeNameInfo.NIobjectId));
            }
            if (itemNameInfo.NItransmitTypeOnMember)
            {
                if (typeNameInfo.NItype == SoapUtil.typeofString)
                {
                    if (typeNameInfo.NIobjectId > 0L)
                    {
                        this.attrList.Put("xsi:type", "SOAP-ENC:string");
                        this.isUsedEnc = true;
                    }
                    else
                    {
                        this.attrList.Put("xsi:type", "xsd:string");
                    }
                }
                else
                {
                    this.attrList.Put("xsi:type", this.TypeNameTagResolver(typeNameInfo, true));
                }
            }
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.Member, "item", this.attrList, value, false, Converter.IsEscaped(typeNameInfo.NIprimitiveTypeEnum));
        }

        internal void WriteJaggedArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound)
        {
            this.attrList.Clear();
            if (memberNameInfo.NIobjectId == this.topId)
            {
                this.Write(InternalElementTypeE.ObjectBegin, "SOAP-ENV:Body", this.attrList, null, false, false);
            }
            if (arrayNameInfo.NIobjectId > 1L)
            {
                this.attrList.Put("id", this.IdToString((int) arrayNameInfo.NIobjectId));
            }
            arrayElemTypeNameInfo.NIitemName = "SOAP-ENC:Array";
            this.isUsedEnc = true;
            this.attrList.Put("SOAP-ENC:arrayType", this.TypeArrayNameTagResolver(memberNameInfo, arrayNameInfo, true));
            if (lowerBound != 0)
            {
                this.attrList.Put("SOAP-ENC:offset", "[" + lowerBound + "]");
            }
            string name = this.MemberElementName(memberNameInfo, null);
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.ObjectBegin, name, this.attrList, null, false, false);
        }

        internal void WriteMember(NameInfo memberNameInfo, NameInfo typeNameInfo, object value)
        {
            this.attrList.Clear();
            if ((typeNameInfo.NItype != null) && (memberNameInfo.NItransmitTypeOnMember || (memberNameInfo.NItransmitTypeOnObject && !memberNameInfo.NIisArrayItem)))
            {
                this.attrList.Put("xsi:type", this.TypeNameTagResolver(typeNameInfo, true));
            }
            string str = null;
            if (value != null)
            {
                if (typeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.QName)
                {
                    SoapQName name = (SoapQName) value;
                    if ((name.Key == null) || (name.Key.Length == 0))
                    {
                        this.attrList.Put("xmlns", "");
                    }
                    else
                    {
                        this.attrList.Put("xmlns:" + name.Key, name.Namespace);
                    }
                    str = name.ToString();
                }
                else if (value is string)
                {
                    str = (string) value;
                }
                else
                {
                    str = Converter.SoapToString(value, typeNameInfo.NIprimitiveTypeEnum);
                }
            }
            NameInfo info = null;
            if (typeNameInfo.NInameSpaceEnum == InternalNameSpaceE.Interop)
            {
                info = typeNameInfo;
            }
            string str2 = this.MemberElementName(memberNameInfo, info);
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.Member, str2, this.attrList, str, true, Converter.IsEscaped(typeNameInfo.NIprimitiveTypeEnum));
        }

        internal void WriteMemberNested(NameInfo memberNameInfo)
        {
        }

        internal void WriteMemberObjectRef(NameInfo memberNameInfo, NameInfo typeNameInfo, int idRef)
        {
            this.attrList.Clear();
            this.attrList.Put("href", this.RefToString(idRef));
            NameInfo info = null;
            if (typeNameInfo.NInameSpaceEnum == InternalNameSpaceE.Interop)
            {
                info = typeNameInfo;
            }
            string name = this.MemberElementName(memberNameInfo, info);
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.Member, name, this.attrList, null, true, false);
        }

        internal void WriteMemberString(NameInfo memberNameInfo, NameInfo typeNameInfo, string value)
        {
            int nIobjectId = (int) typeNameInfo.NIobjectId;
            this.attrList.Clear();
            if (nIobjectId > 0)
            {
                this.attrList.Put("id", this.IdToString((int) typeNameInfo.NIobjectId));
            }
            if ((typeNameInfo.NItype != null) && (memberNameInfo.NItransmitTypeOnMember || (memberNameInfo.NItransmitTypeOnObject && !memberNameInfo.NIisArrayItem)))
            {
                if (typeNameInfo.NIobjectId > 0L)
                {
                    this.attrList.Put("xsi:type", "SOAP-ENC:string");
                    this.isUsedEnc = true;
                }
                else
                {
                    this.attrList.Put("xsi:type", "xsd:string");
                }
            }
            NameInfo info = null;
            if (typeNameInfo.NInameSpaceEnum == InternalNameSpaceE.Interop)
            {
                info = typeNameInfo;
            }
            string name = this.MemberElementName(memberNameInfo, info);
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.Member, name, this.attrList, value, true, Converter.IsEscaped(typeNameInfo.NIprimitiveTypeEnum));
        }

        internal void WriteNullItem(NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
            string nIname = typeNameInfo.NIname;
            this.attrList.Clear();
            if (((typeNameInfo.NItransmitTypeOnMember && !nIname.Equals("System.Object")) && (!nIname.Equals("Object") && !nIname.Equals("System.Empty"))) && (!nIname.Equals("ur-type") && !nIname.Equals("anyType")))
            {
                this.attrList.Put("xsi:type", this.TypeNameTagResolver(typeNameInfo, true));
            }
            this.attrList.Put("xsi:null", "1");
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.Member, "item", this.attrList, null, false, false);
        }

        internal void WriteNullMember(NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
            this.attrList.Clear();
            if ((typeNameInfo.NItype != null) && (memberNameInfo.NItransmitTypeOnMember || (memberNameInfo.NItransmitTypeOnObject && !memberNameInfo.NIisArrayItem)))
            {
                this.attrList.Put("xsi:type", this.TypeNameTagResolver(typeNameInfo, true));
            }
            this.attrList.Put("xsi:null", "1");
            string name = this.MemberElementName(memberNameInfo, null);
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.Member, name, this.attrList, null, true, false);
        }

        internal void WriteObject(NameInfo nameInfo, NameInfo typeNameInfo, int numMembers, string[] memberNames, Type[] memberTypes, WriteObjectInfo[] objectInfos)
        {
            int nIobjectId = (int) nameInfo.NIobjectId;
            this.attrList.Clear();
            if (nIobjectId == this.topId)
            {
                this.Write(InternalElementTypeE.ObjectBegin, "SOAP-ENV:Body", this.attrList, null, false, false);
            }
            if (nIobjectId > 0)
            {
                this.attrList.Put("id", this.IdToString((int) nameInfo.NIobjectId));
            }
            if ((nameInfo.NItransmitTypeOnObject || nameInfo.NItransmitTypeOnMember) && (nameInfo.NIisNestedObject || nameInfo.NIisArrayItem))
            {
                this.attrList.Put("xsi:type", this.TypeNameTagResolver(typeNameInfo, true));
            }
            if (nameInfo.NIisMustUnderstand)
            {
                this.attrList.Put("SOAP-ENV:mustUnderstand", "1");
                this.isUsedEnc = true;
            }
            if (nameInfo.NIisHeader)
            {
                this.attrList.Put("xmlns:" + nameInfo.NIheaderPrefix, nameInfo.NInamespace);
                this.attrList.Put("SOAP-ENC:root", "1");
            }
            if (this.attrValueList.Count > 0)
            {
                for (int i = 0; i < this.attrValueList.Count; i++)
                {
                    string str;
                    string str2;
                    this.attrValueList.Get(i, out str, out str2);
                    this.attrList.Put(str, str2);
                }
                this.attrValueList.Clear();
            }
            string name = this.MemberElementName(nameInfo, typeNameInfo);
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.ObjectBegin, name, this.attrList, null, true, false);
        }

        internal void WriteObjectByteArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound, byte[] byteA)
        {
            string str = Convert.ToBase64String(byteA);
            this.attrList.Clear();
            if (memberNameInfo.NIobjectId == this.topId)
            {
                this.Write(InternalElementTypeE.ObjectBegin, "SOAP-ENV:Body", this.attrList, null, false, false);
            }
            if (arrayNameInfo.NIobjectId > 1L)
            {
                this.attrList.Put("id", this.IdToString((int) arrayNameInfo.NIobjectId));
            }
            this.attrList.Put("xsi:type", "SOAP-ENC:base64");
            this.isUsedEnc = true;
            string name = this.MemberElementName(memberNameInfo, null);
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.Member, name, this.attrList, str, true, false);
        }

        internal void WriteObjectEnd(NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
            this.attrList.Clear();
            this.Write(InternalElementTypeE.ObjectEnd, this.MemberElementName(memberNameInfo, typeNameInfo), this.attrList, null, true, false);
            this.assemblyInfoUsed.Clear();
        }

        internal void WriteObjectString(NameInfo nameInfo, string value)
        {
            this.attrList.Clear();
            if (nameInfo.NIobjectId == this.topId)
            {
                this.Write(InternalElementTypeE.ObjectBegin, "SOAP-ENV:Body", this.attrList, null, false, false);
            }
            if (nameInfo.NIobjectId > 0L)
            {
                this.attrList.Put("id", this.IdToString((int) nameInfo.NIobjectId));
            }
            string name = null;
            if (nameInfo.NIobjectId > 0L)
            {
                name = "SOAP-ENC:string";
                this.isUsedEnc = true;
            }
            else
            {
                name = "xsd:string";
            }
            this.Write(InternalElementTypeE.Member, name, this.attrList, value, false, Converter.IsEscaped(nameInfo.NIprimitiveTypeEnum));
        }

        internal void WriteRectangleArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int rank, int[] lengthA, int[] lowerBoundA)
        {
            this.sbOffset.Length = 0;
            this.sbOffset.Append("[");
            bool flag = true;
            for (int i = 0; i < rank; i++)
            {
                if (lowerBoundA[i] != 0)
                {
                    flag = false;
                }
                if (i > 0)
                {
                    this.sbOffset.Append(",");
                }
                this.sbOffset.Append(lowerBoundA[i]);
            }
            this.sbOffset.Append("]");
            this.attrList.Clear();
            if (memberNameInfo.NIobjectId == this.topId)
            {
                this.Write(InternalElementTypeE.ObjectBegin, "SOAP-ENV:Body", this.attrList, null, false, false);
            }
            if (arrayNameInfo.NIobjectId > 1L)
            {
                this.attrList.Put("id", this.IdToString((int) arrayNameInfo.NIobjectId));
            }
            arrayElemTypeNameInfo.NIitemName = this.NameTagResolver(arrayElemTypeNameInfo, true);
            this.attrList.Put("SOAP-ENC:arrayType", this.TypeArrayNameTagResolver(memberNameInfo, arrayNameInfo, true));
            this.isUsedEnc = true;
            if (!flag)
            {
                this.attrList.Put("SOAP-ENC:offset", this.sbOffset.ToString());
            }
            string name = this.MemberElementName(memberNameInfo, null);
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.ObjectBegin, name, this.attrList, null, false, false);
        }

        internal void WriteSerializationHeader(int topId, int headerId, int minorVersion, int majorVersion)
        {
            this.topId = topId;
            this.headerId = headerId;
            switch (this.xsdVersion)
            {
                case XsdVersion.V1999:
                    this.stream.Write(_soapStart1999, 0, _soapStart1999.Length);
                    break;

                case XsdVersion.V2000:
                    this.stream.Write(_soapStart1999, 0, _soapStart2000.Length);
                    break;

                case XsdVersion.V2001:
                    this.stream.Write(_soapStart, 0, _soapStart.Length);
                    break;
            }
            this.writer.Write(">\r\n");
        }

        internal void WriteSerializationHeaderEnd()
        {
            this.attrList.Clear();
            this.Write(InternalElementTypeE.ObjectEnd, "SOAP-ENV:Body", this.attrList, null, false, false);
            this.Write(InternalElementTypeE.ObjectEnd, "SOAP-ENV:Envelope", this.attrList, null, false, false);
            this.writer.Flush();
        }

        internal void WriteSingleArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound, Array array)
        {
            this.attrList.Clear();
            if (memberNameInfo.NIobjectId == this.topId)
            {
                this.Write(InternalElementTypeE.ObjectBegin, "SOAP-ENV:Body", this.attrList, null, false, false);
            }
            if (arrayNameInfo.NIobjectId > 1L)
            {
                this.attrList.Put("id", this.IdToString((int) arrayNameInfo.NIobjectId));
            }
            arrayElemTypeNameInfo.NIitemName = this.NameTagResolver(arrayElemTypeNameInfo, true);
            this.attrList.Put("SOAP-ENC:arrayType", this.NameTagResolver(arrayNameInfo, true, memberNameInfo.NIname));
            this.isUsedEnc = true;
            if (lowerBound != 0)
            {
                this.attrList.Put("SOAP-ENC:offset", "[" + lowerBound + "]");
            }
            string name = this.MemberElementName(memberNameInfo, null);
            this.NamespaceAttribute();
            this.Write(InternalElementTypeE.ObjectBegin, name, this.attrList, null, false, false);
        }

        internal void WriteTopPrimitive(NameInfo nameInfo, object value)
        {
            this.attrList.Clear();
            this.Write(InternalElementTypeE.ObjectBegin, "SOAP-ENV:Body", this.attrList, null, false, false);
            if (nameInfo.NIobjectId > 0L)
            {
                this.attrList.Put("id", this.IdToString((int) nameInfo.NIobjectId));
            }
            string str = null;
            if (value is string)
            {
                str = (string) value;
            }
            else
            {
                str = Converter.SoapToString(value, nameInfo.NIprimitiveTypeEnum);
            }
            this.Write(InternalElementTypeE.Member, "xsd:" + Converter.ToXmlDataType(nameInfo.NIprimitiveTypeEnum), this.attrList, str, true, false);
        }

        [Conditional("_LOGGING")]
        internal void WriteTraceSoap()
        {
            this.traceBuffer.Length = 0;
        }

        internal void WriteXsdVersion(XsdVersion xsdVersion)
        {
            this.xsdVersion = xsdVersion;
        }

        internal sealed class AssemblyInfo
        {
            internal int id;
            internal bool isInteropType;
            internal bool isUsed;
            internal string name;
            internal string prefix;

            internal AssemblyInfo(int id, string name, bool isInteropType)
            {
                this.id = id;
                this.name = name;
                this.isInteropType = isInteropType;
                this.isUsed = false;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DottedInfo
        {
            internal string dottedAssemblyName;
            internal string name;
            internal string nameSpace;
            internal int assemId;
        }
    }
}

