namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Schema;

    internal sealed class XmlSqlBinaryReader : XmlReader, IXmlNamespaceResolver
    {
        private int attrCount;
        private int[] attrHashTbl;
        private AttrInfo[] attributes;
        private int attrIndex;
        private string baseUri;
        private bool checkCharacters;
        private bool closeInput;
        private byte[] data;
        private int docState;
        private DtdProcessing dtdProcessing;
        private int elemDepth;
        private ElemInfo[] elementStack;
        private int end;
        private bool eof;
        private SecureStringHasher hasher;
        private bool hasTypedValue;
        private bool ignoreComments;
        private bool ignorePIs;
        private bool ignoreWhitespace;
        private Stream inStrm;
        private bool isEmpty;
        private int mark;
        private Dictionary<string, NamespaceDecl> namespaces;
        private XmlNodeType nodetype;
        private string nsxmlns;
        private long offset;
        private XmlNodeType parentNodeType;
        private int pos;
        private int posAfterAttrs;
        private NestedBinXml prevNameInfo;
        private QName qnameElement;
        private QName qnameOther;
        private static System.Xml.ReadState[] ScanState2ReadState;
        private bool sniffed;
        private ScanState state;
        private string stringValue;
        private SymbolTables symbolTables;
        private XmlReader textXmlReader;
        private int tokDataPos;
        private BinXmlToken token;
        private static Type[] TokenTypeMap = null;
        private int tokLen;
        internal static readonly Type TypeOfObject = typeof(object);
        internal static readonly Type TypeOfString = typeof(string);
        private Encoding unicode = Encoding.Unicode;
        private Type valueType;
        private byte version;
        private string xml;
        private XmlCharType xmlCharType = XmlCharType.Instance;
        private string xmlns;
        private bool xmlspacePreserve;
        private XmlNameTable xnt;
        private bool xntFromSettings;
        private static byte[] XsdKatmaiTimeScaleToValueLengthMap = new byte[] { 3, 3, 3, 4, 4, 5, 5, 5 };

        static XmlSqlBinaryReader()
        {
            System.Xml.ReadState[] stateArray = new System.Xml.ReadState[9];
            stateArray[0] = System.Xml.ReadState.Interactive;
            stateArray[1] = System.Xml.ReadState.Interactive;
            stateArray[2] = System.Xml.ReadState.Interactive;
            stateArray[3] = System.Xml.ReadState.Interactive;
            stateArray[4] = System.Xml.ReadState.Interactive;
            stateArray[6] = System.Xml.ReadState.Error;
            stateArray[7] = System.Xml.ReadState.EndOfFile;
            stateArray[8] = System.Xml.ReadState.Closed;
            ScanState2ReadState = stateArray;
        }

        public XmlSqlBinaryReader(Stream stream, byte[] data, int len, string baseUri, bool closeInput, XmlReaderSettings settings)
        {
            this.xnt = settings.NameTable;
            if (this.xnt == null)
            {
                this.xnt = new System.Xml.NameTable();
                this.xntFromSettings = false;
            }
            else
            {
                this.xntFromSettings = true;
            }
            this.xml = this.xnt.Add("xml");
            this.xmlns = this.xnt.Add("xmlns");
            this.nsxmlns = this.xnt.Add("http://www.w3.org/2000/xmlns/");
            this.baseUri = baseUri;
            this.state = ScanState.Init;
            this.nodetype = XmlNodeType.None;
            this.token = BinXmlToken.Error;
            this.elementStack = new ElemInfo[0x10];
            this.attributes = new AttrInfo[8];
            this.attrHashTbl = new int[8];
            this.symbolTables.Init();
            this.qnameOther.Clear();
            this.qnameElement.Clear();
            this.xmlspacePreserve = false;
            this.hasher = new SecureStringHasher();
            this.namespaces = new Dictionary<string, NamespaceDecl>(this.hasher);
            this.AddInitNamespace(string.Empty, string.Empty);
            this.AddInitNamespace(this.xml, this.xnt.Add("http://www.w3.org/XML/1998/namespace"));
            this.AddInitNamespace(this.xmlns, this.nsxmlns);
            this.valueType = TypeOfString;
            this.inStrm = stream;
            if (data != null)
            {
                this.data = data;
                this.end = len;
                this.pos = 2;
                this.sniffed = true;
            }
            else
            {
                this.data = new byte[0x1000];
                this.end = stream.Read(this.data, 0, 0x1000);
                this.pos = 0;
                this.sniffed = false;
            }
            this.mark = -1;
            this.eof = 0 == this.end;
            this.offset = 0L;
            this.closeInput = closeInput;
            switch (settings.ConformanceLevel)
            {
                case ConformanceLevel.Auto:
                    this.docState = 0;
                    break;

                case ConformanceLevel.Fragment:
                    this.docState = 9;
                    break;

                case ConformanceLevel.Document:
                    this.docState = 1;
                    break;
            }
            this.checkCharacters = settings.CheckCharacters;
            this.dtdProcessing = settings.DtdProcessing;
            this.ignoreWhitespace = settings.IgnoreWhitespace;
            this.ignorePIs = settings.IgnoreProcessingInstructions;
            this.ignoreComments = settings.IgnoreComments;
            if (TokenTypeMap == null)
            {
                this.GenerateTokenTypeMap();
            }
        }

        private void AddInitNamespace(string prefix, string uri)
        {
            NamespaceDecl decl = new NamespaceDecl(prefix, uri, this.elementStack[0].nsdecls, null, -1, true);
            this.elementStack[0].nsdecls = decl;
            this.namespaces.Add(prefix, decl);
        }

        private void AddName()
        {
            string array = this.ParseText();
            int length = this.symbolTables.symCount++;
            string[] symtable = this.symbolTables.symtable;
            if (length == symtable.Length)
            {
                string[] destinationArray = new string[length * 2];
                Array.Copy(symtable, 0, destinationArray, 0, length);
                this.symbolTables.symtable = symtable = destinationArray;
            }
            symtable[length] = this.xnt.Add(array);
        }

        private void AddQName()
        {
            string str2;
            string nsxmlns;
            int index = this.ReadNameRef();
            int num2 = this.ReadNameRef();
            int num3 = this.ReadNameRef();
            int length = this.symbolTables.qnameCount++;
            QName[] qnametable = this.symbolTables.qnametable;
            if (length == qnametable.Length)
            {
                QName[] destinationArray = new QName[length * 2];
                Array.Copy(qnametable, 0, destinationArray, 0, length);
                this.symbolTables.qnametable = qnametable = destinationArray;
            }
            string[] symtable = this.symbolTables.symtable;
            string prefix = symtable[num2];
            if (num3 == 0)
            {
                if ((num2 == 0) && (index == 0))
                {
                    return;
                }
                if (!prefix.StartsWith("xmlns", StringComparison.Ordinal))
                {
                    goto Label_0108;
                }
                if (5 < prefix.Length)
                {
                    if ((6 == prefix.Length) || (':' != prefix[5]))
                    {
                        goto Label_0108;
                    }
                    str2 = this.xnt.Add(prefix.Substring(6));
                    prefix = this.xmlns;
                }
                else
                {
                    str2 = prefix;
                    prefix = string.Empty;
                }
                nsxmlns = this.nsxmlns;
            }
            else
            {
                str2 = symtable[num3];
                nsxmlns = symtable[index];
            }
            qnametable[length].Set(prefix, str2, nsxmlns);
            return;
        Label_0108:
            throw new XmlException("Xml_BadNamespaceDecl", null);
        }

        private string CDATAValue()
        {
            string str = this.GetString(this.tokDataPos, this.tokLen);
            StringBuilder builder = null;
            while (this.PeekToken() == BinXmlToken.CData)
            {
                this.pos++;
                if (builder == null)
                {
                    builder = new StringBuilder(str.Length + (str.Length / 2));
                    builder.Append(str);
                }
                builder.Append(this.ParseText());
            }
            if (builder != null)
            {
                str = builder.ToString();
            }
            this.stringValue = str;
            return str;
        }

        private void CheckAllowContent()
        {
            switch (this.docState)
            {
                case 0:
                    this.docState = 9;
                    return;

                case 3:
                case 9:
                    return;
            }
            throw this.ThrowXmlException("Xml_InvalidRootData");
        }

        private unsafe XmlNodeType CheckText(bool attr)
        {
            byte[] buffer;
            XmlCharType xmlCharType = this.xmlCharType;
            if (((buffer = this.data) == null) || (buffer.Length == 0))
            {
                numRef = null;
                goto Label_0026;
            }
            fixed (byte* numRef = buffer)
            {
                int num;
                int num3;
                int num4;
            Label_0026:
                num = this.pos;
                int tokDataPos = this.tokDataPos;
                if (attr)
                {
                    goto Label_0076;
                }
            Label_0037:
                num3 = tokDataPos + 2;
                if (num3 > num)
                {
                    return (this.xmlspacePreserve ? XmlNodeType.SignificantWhitespace : XmlNodeType.Whitespace);
                }
                if ((numRef[tokDataPos + 1] == 0) && ((xmlCharType.charProperties[numRef[tokDataPos]] & 1) != 0))
                {
                    tokDataPos = num3;
                    goto Label_0037;
                }
            Label_0076:
                num4 = tokDataPos + 2;
                if (num4 > num)
                {
                    return XmlNodeType.Text;
                }
                char index = (char) (numRef[tokDataPos] | (numRef[tokDataPos + 1] << 8));
                if ((xmlCharType.charProperties[index] & 0x10) != 0)
                {
                    tokDataPos = num4;
                }
                else
                {
                    if (!XmlCharType.IsHighSurrogate(index))
                    {
                        throw XmlConvert.CreateInvalidCharException(index, '\0', ExceptionType.XmlException);
                    }
                    if ((tokDataPos + 4) > num)
                    {
                        throw this.ThrowXmlException("Xml_InvalidSurrogateMissingLowChar");
                    }
                    char ch = (char) (numRef[tokDataPos + 2] | (numRef[tokDataPos + 3] << 8));
                    if (!XmlCharType.IsLowSurrogate(ch))
                    {
                        throw XmlConvert.CreateInvalidSurrogatePairException(index, ch);
                    }
                    tokDataPos += 4;
                }
                goto Label_0076;
            }
        }

        private XmlNodeType CheckTextIsWS()
        {
            byte[] data = this.data;
            for (int i = this.tokDataPos; i < this.pos; i += 2)
            {
                if (data[i + 1] == 0)
                {
                    switch (data[i])
                    {
                        case 9:
                        case 10:
                        case 13:
                        case 0x20:
                        {
                            continue;
                        }
                    }
                }
                return XmlNodeType.Text;
            }
            if (this.xmlspacePreserve)
            {
                return XmlNodeType.SignificantWhitespace;
            }
            return XmlNodeType.Whitespace;
        }

        private void CheckValueTokenBounds()
        {
            if ((this.end - this.tokDataPos) < this.tokLen)
            {
                throw this.ThrowXmlException("Xml_UnexpectedEOF1");
            }
        }

        private void ClearAttributes()
        {
            if (this.attrCount != 0)
            {
                this.attrCount = 0;
            }
        }

        public override void Close()
        {
            this.state = ScanState.Closed;
            this.nodetype = XmlNodeType.None;
            this.token = BinXmlToken.Error;
            this.stringValue = null;
            if (this.textXmlReader != null)
            {
                this.textXmlReader.Close();
                this.textXmlReader = null;
            }
            if ((this.inStrm != null) && this.closeInput)
            {
                this.inStrm.Close();
            }
            this.inStrm = null;
            this.pos = this.end = 0;
        }

        private void Fill(int require)
        {
            if ((this.pos + require) >= this.end)
            {
                this.Fill_(require);
            }
        }

        private void Fill_(int require)
        {
            while (this.FillAllowEOF() && ((this.pos + require) >= this.end))
            {
            }
            if ((this.pos + require) >= this.end)
            {
                throw this.ThrowXmlException("Xml_UnexpectedEOF1");
            }
        }

        private bool FillAllowEOF()
        {
            if (this.eof)
            {
                return false;
            }
            byte[] data = this.data;
            int pos = this.pos;
            int mark = this.mark;
            int end = this.end;
            if (mark == -1)
            {
                mark = pos;
            }
            if ((mark >= 0) && (mark < end))
            {
                int length = end - mark;
                if (length > (7 * (data.Length / 8)))
                {
                    byte[] destinationArray = new byte[data.Length * 2];
                    Array.Copy(data, mark, destinationArray, 0, length);
                    this.data = data = destinationArray;
                }
                else
                {
                    Array.Copy(data, mark, data, 0, length);
                }
                pos -= mark;
                end -= mark;
                this.tokDataPos -= mark;
                for (int i = 0; i < this.attrCount; i++)
                {
                    this.attributes[i].AdjustPosition(-mark);
                }
                this.pos = pos;
                this.mark = 0;
                this.offset += mark;
            }
            else
            {
                this.pos -= end;
                this.mark -= end;
                this.offset += end;
                this.tokDataPos -= end;
                end = 0;
            }
            int count = data.Length - end;
            int num7 = this.inStrm.Read(data, end, count);
            this.end = end + num7;
            this.eof = num7 <= 0;
            return (num7 > 0);
        }

        private void FinishCDATA()
        {
        Label_0000:
            switch (this.PeekToken())
            {
                case BinXmlToken.EndCData:
                    this.pos++;
                    return;

                case BinXmlToken.CData:
                    int num;
                    this.pos++;
                    this.ScanText(out num);
                    goto Label_0000;
            }
            throw new XmlException("XmlBin_MissingEndCDATA");
        }

        private int FinishContentAsXXX(int origPos)
        {
            if (this.state != ScanState.Doc)
            {
                return origPos;
            }
            if ((this.NodeType == XmlNodeType.Element) || (this.NodeType == XmlNodeType.EndElement))
            {
                goto Label_004F;
            }
        Label_001B:
            if (this.Read())
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                        goto Label_001B;

                    case XmlNodeType.EndElement:
                    case XmlNodeType.Element:
                        goto Label_004F;
                }
                throw this.ThrowNotSupported("XmlBinary_ListsOfValuesNotSupported");
            }
        Label_004F:
            return this.pos;
        }

        private void FinishEndElement()
        {
            NamespaceDecl firstInScopeChain = this.elementStack[this.elemDepth].Clear();
            this.PopNamespaces(firstInScopeChain);
            this.elemDepth--;
        }

        private void GenerateImpliedXmlnsAttrs()
        {
            for (NamespaceDecl decl = this.elementStack[this.elemDepth].nsdecls; decl != null; decl = decl.scopeLink)
            {
                if (decl.implied)
                {
                    QName name;
                    if (this.attrCount == this.attributes.Length)
                    {
                        this.GrowAttributes();
                    }
                    if (decl.prefix.Length == 0)
                    {
                        name = new QName(string.Empty, this.xmlns, this.nsxmlns);
                    }
                    else
                    {
                        name = new QName(this.xmlns, this.xnt.Add(decl.prefix), this.nsxmlns);
                    }
                    this.attributes[this.attrCount].Set(name, decl.uri);
                    this.attrCount++;
                }
            }
        }

        private void GenerateTokenTypeMap()
        {
            Type[] typeArray = new Type[0x100];
            typeArray[0x86] = typeof(bool);
            typeArray[7] = typeof(byte);
            typeArray[0x88] = typeof(sbyte);
            typeArray[1] = typeof(short);
            typeArray[0x89] = typeof(ushort);
            typeArray[0x8a] = typeof(uint);
            typeArray[3] = typeof(float);
            typeArray[4] = typeof(double);
            typeArray[8] = typeof(long);
            typeArray[0x8b] = typeof(ulong);
            typeArray[140] = typeof(XmlQualifiedName);
            Type type = typeof(int);
            typeArray[6] = type;
            typeArray[2] = type;
            Type type2 = typeof(decimal);
            typeArray[20] = type2;
            typeArray[5] = type2;
            typeArray[10] = type2;
            typeArray[11] = type2;
            typeArray[0x87] = type2;
            Type type3 = typeof(DateTime);
            typeArray[0x13] = type3;
            typeArray[0x12] = type3;
            typeArray[0x81] = type3;
            typeArray[130] = type3;
            typeArray[0x83] = type3;
            typeArray[0x7f] = type3;
            typeArray[0x7e] = type3;
            typeArray[0x7d] = type3;
            Type type4 = typeof(DateTimeOffset);
            typeArray[0x7c] = type4;
            typeArray[0x7b] = type4;
            typeArray[0x7a] = type4;
            Type type5 = typeof(byte[]);
            typeArray[15] = type5;
            typeArray[12] = type5;
            typeArray[0x17] = type5;
            typeArray[0x1b] = type5;
            typeArray[0x84] = type5;
            typeArray[0x85] = type5;
            typeArray[13] = TypeOfString;
            typeArray[0x10] = TypeOfString;
            typeArray[0x16] = TypeOfString;
            typeArray[14] = TypeOfString;
            typeArray[0x11] = TypeOfString;
            typeArray[0x18] = TypeOfString;
            typeArray[9] = TypeOfString;
            if (TokenTypeMap == null)
            {
                TokenTypeMap = typeArray;
            }
        }

        public override string GetAttribute(int i)
        {
            if (ScanState.XmlText == this.state)
            {
                return this.textXmlReader.GetAttribute(i);
            }
            if ((i < 0) || (i >= this.attrCount))
            {
                throw new ArgumentOutOfRangeException("i");
            }
            return this.GetAttributeText(i);
        }

        public override string GetAttribute(string name)
        {
            if (ScanState.XmlText == this.state)
            {
                return this.textXmlReader.GetAttribute(name);
            }
            int i = this.LocateAttribute(name);
            if (-1 == i)
            {
                return null;
            }
            return this.GetAttribute(i);
        }

        public override string GetAttribute(string name, string ns)
        {
            if (ScanState.XmlText == this.state)
            {
                return this.textXmlReader.GetAttribute(name, ns);
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (ns == null)
            {
                ns = string.Empty;
            }
            int i = this.LocateAttribute(name, ns);
            if (-1 == i)
            {
                return null;
            }
            return this.GetAttribute(i);
        }

        private string GetAttributeText(int i)
        {
            string str2;
            string val = this.attributes[i].val;
            if (val != null)
            {
                return val;
            }
            int pos = this.pos;
            try
            {
                this.pos = this.attributes[i].contentPos;
                BinXmlToken token = this.RescanNextToken();
                if ((BinXmlToken.Attr == token) || (BinXmlToken.EndAttrs == token))
                {
                    return "";
                }
                this.token = token;
                this.ReScanOverValue(token);
                str2 = this.ValueAsString(token);
            }
            finally
            {
                this.pos = pos;
            }
            return str2;
        }

        private unsafe double GetDouble(int offset)
        {
            uint num = (uint) (((this.data[offset] | (this.data[offset + 1] << 8)) | (this.data[offset + 2] << 0x10)) | (this.data[offset + 3] << 0x18));
            uint num2 = (uint) (((this.data[offset + 4] | (this.data[offset + 5] << 8)) | (this.data[offset + 6] << 0x10)) | (this.data[offset + 7] << 0x18));
            ulong num3 = (num2 << 0x20) | num;
            return *(((double*) &num3));
        }

        private short GetInt16(int pos)
        {
            byte[] data = this.data;
            return (short) (data[pos] | (data[pos + 1] << 8));
        }

        private int GetInt32(int pos)
        {
            byte[] data = this.data;
            return (((data[pos] | (data[pos + 1] << 8)) | (data[pos + 2] << 0x10)) | (data[pos + 3] << 0x18));
        }

        private long GetInt64(int pos)
        {
            byte[] data = this.data;
            uint num = (uint) (((data[pos] | (data[pos + 1] << 8)) | (data[pos + 2] << 0x10)) | (data[pos + 3] << 0x18));
            uint num2 = (uint) (((data[pos + 4] | (data[pos + 5] << 8)) | (data[pos + 6] << 0x10)) | (data[pos + 7] << 0x18));
            return (long) ((num2 << 0x20) | num);
        }

        private unsafe float GetSingle(int offset)
        {
            byte[] data = this.data;
            uint num = (uint) (((data[offset] | (data[offset + 1] << 8)) | (data[offset + 2] << 0x10)) | (data[offset + 3] << 0x18));
            return *(((float*) &num));
        }

        private string GetString(int pos, int cch)
        {
            if ((pos + (cch * 2)) > this.end)
            {
                throw new XmlException("Xml_UnexpectedEOF1", null);
            }
            if (cch == 0)
            {
                return string.Empty;
            }
            if ((pos & 1) == 0)
            {
                return this.GetStringAligned(this.data, pos, cch);
            }
            return this.unicode.GetString(this.data, pos, cch * 2);
        }

        private unsafe string GetStringAligned(byte[] data, int offset, int cch)
        {
            fixed (byte* numRef = data)
            {
                return new string((char*) (numRef + offset), 0, cch);
            }
        }

        private ushort GetUInt16(int pos)
        {
            byte[] data = this.data;
            return (ushort) (data[pos] | (data[pos + 1] << 8));
        }

        private uint GetUInt32(int pos)
        {
            byte[] data = this.data;
            return (uint) (((data[pos] | (data[pos + 1] << 8)) | (data[pos + 2] << 0x10)) | (data[pos + 3] << 0x18));
        }

        private ulong GetUInt64(int pos)
        {
            byte[] data = this.data;
            uint num = (uint) (((data[pos] | (data[pos + 1] << 8)) | (data[pos + 2] << 0x10)) | (data[pos + 3] << 0x18));
            uint num2 = (uint) (((data[pos + 4] | (data[pos + 5] << 8)) | (data[pos + 6] << 0x10)) | (data[pos + 7] << 0x18));
            return ((num2 << 0x20) | num);
        }

        private XmlValueConverter GetValueConverter(XmlTypeCode typeCode)
        {
            return DatatypeImplementation.GetSimpleTypeFromTypeCode(typeCode).ValueConverter;
        }

        private Type GetValueType(BinXmlToken token)
        {
            Type type = TokenTypeMap[(int) token];
            if (type == null)
            {
                throw this.ThrowUnexpectedToken(token);
            }
            return type;
        }

        private int GetXsdKatmaiTokenLength(BinXmlToken token)
        {
            byte num;
            switch (token)
            {
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    this.Fill(0);
                    num = this.data[this.pos];
                    return (6 + this.XsdKatmaiTimeScaleToValueLength(num));

                case BinXmlToken.XSD_KATMAI_TIME:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                    this.Fill(0);
                    num = this.data[this.pos];
                    return (4 + this.XsdKatmaiTimeScaleToValueLength(num));

                case BinXmlToken.XSD_KATMAI_DATE:
                    return 3;
            }
            throw this.ThrowUnexpectedToken(this.token);
        }

        private void GrowAttributes()
        {
            int num = this.attributes.Length * 2;
            AttrInfo[] destinationArray = new AttrInfo[num];
            Array.Copy(this.attributes, 0, destinationArray, 0, this.attrCount);
            this.attributes = destinationArray;
        }

        private void GrowElements()
        {
            int num = this.elementStack.Length * 2;
            ElemInfo[] destinationArray = new ElemInfo[num];
            Array.Copy(this.elementStack, 0, destinationArray, 0, this.elementStack.Length);
            this.elementStack = destinationArray;
        }

        private void HashCheckForDuplicateAttributes()
        {
            int length = 0x100;
            while (length < this.attrCount)
            {
                length *= 2;
            }
            if (this.attrHashTbl.Length < length)
            {
                this.attrHashTbl = new int[length];
            }
            for (int i = 0; i < this.attrCount; i++)
            {
                string str;
                string str2;
                int hash = this.attributes[i].GetLocalnameAndNamespaceUriAndHash(this.hasher, out str, out str2);
                int index = hash & (length - 1);
                int prevHash = this.attrHashTbl[index];
                this.attrHashTbl[index] = i + 1;
                this.attributes[i].prevHash = prevHash;
                while (prevHash != 0)
                {
                    prevHash--;
                    if (this.attributes[prevHash].MatchHashNS(hash, str, str2))
                    {
                        throw new XmlException("Xml_DupAttributeName", this.attributes[i].name.ToString());
                    }
                    prevHash = this.attributes[prevHash].prevHash;
                }
            }
            Array.Clear(this.attrHashTbl, 0, length);
        }

        private void ImplReadCDATA()
        {
            this.CheckAllowContent();
            this.nodetype = XmlNodeType.CDATA;
            this.mark = this.pos;
            this.tokLen = this.ScanText(out this.tokDataPos);
        }

        private void ImplReadComment()
        {
            this.nodetype = XmlNodeType.Comment;
            this.mark = this.pos;
            this.tokLen = this.ScanText(out this.tokDataPos);
        }

        private void ImplReadData(BinXmlToken tokenType)
        {
            this.mark = this.pos;
            switch (tokenType)
            {
                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_TEXT:
                case BinXmlToken.SQL_NTEXT:
                    this.valueType = TypeOfString;
                    this.hasTypedValue = false;
                    break;

                default:
                    this.valueType = this.GetValueType(this.token);
                    this.hasTypedValue = true;
                    break;
            }
            this.nodetype = this.ScanOverValue(this.token, false, true);
            switch (this.PeekNextToken())
            {
                case BinXmlToken.SQL_SMALLINT:
                case BinXmlToken.SQL_INT:
                case BinXmlToken.SQL_REAL:
                case BinXmlToken.SQL_FLOAT:
                case BinXmlToken.SQL_MONEY:
                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT:
                case BinXmlToken.SQL_BIGINT:
                case BinXmlToken.SQL_UUID:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.SQL_BINARY:
                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_VARBINARY:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_DATETIME:
                case BinXmlToken.SQL_SMALLDATETIME:
                case BinXmlToken.SQL_SMALLMONEY:
                case BinXmlToken.SQL_TEXT:
                case BinXmlToken.SQL_IMAGE:
                case BinXmlToken.SQL_NTEXT:
                case BinXmlToken.SQL_UDT:
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                case BinXmlToken.XSD_KATMAI_TIME:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                case BinXmlToken.XSD_KATMAI_DATE:
                case BinXmlToken.XSD_TIME:
                case BinXmlToken.XSD_DATETIME:
                case BinXmlToken.XSD_DATE:
                case BinXmlToken.XSD_BINHEX:
                case BinXmlToken.XSD_BASE64:
                case BinXmlToken.XSD_BOOLEAN:
                case BinXmlToken.XSD_DECIMAL:
                case BinXmlToken.XSD_BYTE:
                case BinXmlToken.XSD_UNSIGNEDSHORT:
                case BinXmlToken.XSD_UNSIGNEDINT:
                case BinXmlToken.XSD_UNSIGNEDLONG:
                case BinXmlToken.XSD_QNAME:
                    throw this.ThrowNotSupported("XmlBinary_ListsOfValuesNotSupported");

                case (BinXmlToken.SQL_SMALLMONEY | BinXmlToken.SQL_SMALLINT):
                case (BinXmlToken.SQL_NTEXT | BinXmlToken.SQL_SMALLINT):
                case (BinXmlToken.SQL_NTEXT | BinXmlToken.SQL_INT):
                case ((BinXmlToken) 0x80):
                    return;
            }
        }

        private void ImplReadDoctype()
        {
            if (this.dtdProcessing == DtdProcessing.Prohibit)
            {
                throw this.ThrowXmlException("Xml_DtdIsProhibited");
            }
            switch (this.docState)
            {
                case 0:
                case 1:
                    this.docState = 2;
                    this.qnameOther.localname = this.ParseText();
                    if (BinXmlToken.System == this.PeekToken())
                    {
                        this.pos++;
                        this.attributes[this.attrCount++].Set(new QName(string.Empty, this.xnt.Add("SYSTEM"), string.Empty), this.ParseText());
                    }
                    if (BinXmlToken.Public == this.PeekToken())
                    {
                        this.pos++;
                        this.attributes[this.attrCount++].Set(new QName(string.Empty, this.xnt.Add("PUBLIC"), string.Empty), this.ParseText());
                    }
                    if (BinXmlToken.Subset == this.PeekToken())
                    {
                        this.pos++;
                        this.mark = this.pos;
                        this.tokLen = this.ScanText(out this.tokDataPos);
                    }
                    else
                    {
                        this.tokLen = this.tokDataPos = 0;
                    }
                    this.nodetype = XmlNodeType.DocumentType;
                    this.posAfterAttrs = this.pos;
                    return;

                case 9:
                    throw this.ThrowXmlException("Xml_DtdNotAllowedInFragment");
            }
            throw this.ThrowXmlException("Xml_BadDTDLocation");
        }

        private void ImplReadElement()
        {
            if ((3 != this.docState) || (9 != this.docState))
            {
                switch (this.docState)
                {
                    case -1:
                        throw this.ThrowUnexpectedToken(this.token);

                    case 0:
                        this.docState = 9;
                        break;

                    case 1:
                    case 2:
                        this.docState = 3;
                        break;
                }
            }
            this.elemDepth++;
            if (this.elemDepth == this.elementStack.Length)
            {
                this.GrowElements();
            }
            QName name = this.symbolTables.qnametable[this.ReadQNameRef()];
            this.qnameOther = this.qnameElement = name;
            this.elementStack[this.elemDepth].Set(name, this.xmlspacePreserve);
            this.PushNamespace(name.prefix, name.namespaceUri, true);
            BinXmlToken token = this.PeekNextToken();
            if (BinXmlToken.Attr == token)
            {
                this.ScanAttributes();
                token = this.PeekNextToken();
            }
            this.GenerateImpliedXmlnsAttrs();
            if (BinXmlToken.EndElem == token)
            {
                this.NextToken();
                this.isEmpty = true;
            }
            else if (BinXmlToken.SQL_NVARCHAR == token)
            {
                if (this.mark < 0)
                {
                    this.mark = this.pos;
                }
                this.pos++;
                if (this.ReadByte() == 0)
                {
                    if (0xf7 != this.ReadByte())
                    {
                        this.pos -= 3;
                    }
                    else
                    {
                        this.pos--;
                    }
                }
                else
                {
                    this.pos -= 2;
                }
            }
            this.nodetype = XmlNodeType.Element;
            this.valueType = TypeOfObject;
            this.posAfterAttrs = this.pos;
        }

        private void ImplReadEndElement()
        {
            if (this.elemDepth == 0)
            {
                throw this.ThrowXmlException("Xml_UnexpectedEndTag");
            }
            int elemDepth = this.elemDepth;
            if ((1 == elemDepth) && (3 == this.docState))
            {
                this.docState = -1;
            }
            this.qnameOther = this.elementStack[elemDepth].name;
            this.xmlspacePreserve = this.elementStack[elemDepth].xmlspacePreserve;
            this.nodetype = XmlNodeType.EndElement;
        }

        private void ImplReadEndNest()
        {
            NestedBinXml prevNameInfo = this.prevNameInfo;
            this.symbolTables = prevNameInfo.symbolTables;
            this.docState = prevNameInfo.docState;
            this.prevNameInfo = prevNameInfo.next;
        }

        private void ImplReadNest()
        {
            this.CheckAllowContent();
            this.prevNameInfo = new NestedBinXml(this.symbolTables, this.docState, this.prevNameInfo);
            this.symbolTables.Init();
            this.docState = 0;
        }

        private void ImplReadPI()
        {
            this.qnameOther.localname = this.symbolTables.symtable[this.ReadNameRef()];
            this.mark = this.pos;
            this.tokLen = this.ScanText(out this.tokDataPos);
            this.nodetype = XmlNodeType.ProcessingInstruction;
        }

        private void ImplReadXmlText()
        {
            this.CheckAllowContent();
            string xmlFragment = this.ParseText();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(this.xnt);
            foreach (NamespaceDecl decl in this.namespaces.Values)
            {
                if (decl.scope > 0)
                {
                    nsMgr.AddNamespace(decl.prefix, decl.uri);
                }
            }
            XmlReaderSettings settings = this.Settings;
            settings.ReadOnly = false;
            settings.NameTable = this.xnt;
            settings.DtdProcessing = DtdProcessing.Prohibit;
            if (this.elemDepth != 0)
            {
                settings.ConformanceLevel = ConformanceLevel.Fragment;
            }
            settings.ReadOnly = true;
            XmlParserContext context = new XmlParserContext(this.xnt, nsMgr, this.XmlLang, this.XmlSpace);
            this.textXmlReader = new XmlTextReaderImpl(xmlFragment, context, settings);
            if (!this.textXmlReader.Read() || ((this.textXmlReader.NodeType == XmlNodeType.XmlDeclaration) && !this.textXmlReader.Read()))
            {
                this.state = ScanState.Doc;
                this.ReadDoc();
            }
            else
            {
                this.state = ScanState.XmlText;
                this.UpdateFromTextReader();
            }
        }

        private int LocateAttribute(string name)
        {
            string str;
            string str2;
            ValidateNames.SplitQName(name, out str, out str2);
            for (int i = 0; i < this.attrCount; i++)
            {
                if (this.attributes[i].name.MatchPrefix(str, str2))
                {
                    return i;
                }
            }
            return -1;
        }

        private int LocateAttribute(string name, string ns)
        {
            for (int i = 0; i < this.attrCount; i++)
            {
                if (this.attributes[i].name.MatchNs(name, ns))
                {
                    return i;
                }
            }
            return -1;
        }

        public override string LookupNamespace(string prefix)
        {
            NamespaceDecl decl;
            if (ScanState.XmlText == this.state)
            {
                return this.textXmlReader.LookupNamespace(prefix);
            }
            if ((prefix != null) && this.namespaces.TryGetValue(prefix, out decl))
            {
                return decl.uri;
            }
            return null;
        }

        public override void MoveToAttribute(int i)
        {
            if (ScanState.XmlText == this.state)
            {
                this.textXmlReader.MoveToAttribute(i);
                this.UpdateFromTextReader(true);
            }
            else
            {
                if ((i < 0) || (i >= this.attrCount))
                {
                    throw new ArgumentOutOfRangeException("i");
                }
                this.PositionOnAttribute(i + 1);
            }
        }

        public override bool MoveToAttribute(string name)
        {
            if (ScanState.XmlText == this.state)
            {
                return this.UpdateFromTextReader(this.textXmlReader.MoveToAttribute(name));
            }
            int num = this.LocateAttribute(name);
            if ((-1 != num) && (this.state < ScanState.Init))
            {
                this.PositionOnAttribute(num + 1);
                return true;
            }
            return false;
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            if (ScanState.XmlText == this.state)
            {
                return this.UpdateFromTextReader(this.textXmlReader.MoveToAttribute(name, ns));
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (ns == null)
            {
                ns = string.Empty;
            }
            int num = this.LocateAttribute(name, ns);
            if ((-1 != num) && (this.state < ScanState.Init))
            {
                this.PositionOnAttribute(num + 1);
                return true;
            }
            return false;
        }

        public override bool MoveToElement()
        {
            switch (this.state)
            {
                case ScanState.XmlText:
                    return this.UpdateFromTextReader(this.textXmlReader.MoveToElement());

                case ScanState.Attr:
                case ScanState.AttrVal:
                case ScanState.AttrValPseudoValue:
                    this.attrIndex = 0;
                    this.qnameOther = this.qnameElement;
                    if (XmlNodeType.Element != this.parentNodeType)
                    {
                        if (XmlNodeType.XmlDeclaration == this.parentNodeType)
                        {
                            this.token = BinXmlToken.XmlDecl;
                        }
                        else if (XmlNodeType.DocumentType == this.parentNodeType)
                        {
                            this.token = BinXmlToken.DocType;
                        }
                        break;
                    }
                    this.token = BinXmlToken.Element;
                    break;

                default:
                    return false;
            }
            this.nodetype = this.parentNodeType;
            this.state = ScanState.Doc;
            this.pos = this.posAfterAttrs;
            this.stringValue = null;
            return true;
        }

        public override bool MoveToFirstAttribute()
        {
            if (ScanState.XmlText == this.state)
            {
                return this.UpdateFromTextReader(this.textXmlReader.MoveToFirstAttribute());
            }
            if (this.attrCount == 0)
            {
                return false;
            }
            this.PositionOnAttribute(1);
            return true;
        }

        public override bool MoveToNextAttribute()
        {
            switch (this.state)
            {
                case ScanState.Doc:
                case ScanState.Attr:
                case ScanState.AttrVal:
                case ScanState.AttrValPseudoValue:
                    if (this.attrIndex < this.attrCount)
                    {
                        this.PositionOnAttribute(++this.attrIndex);
                        return true;
                    }
                    return false;

                case ScanState.XmlText:
                    return this.UpdateFromTextReader(this.textXmlReader.MoveToNextAttribute());
            }
            return false;
        }

        private void NameFlush()
        {
            this.symbolTables.symCount = this.symbolTables.qnameCount = 1;
            Array.Clear(this.symbolTables.symtable, 1, this.symbolTables.symtable.Length - 1);
            Array.Clear(this.symbolTables.qnametable, 0, this.symbolTables.qnametable.Length);
        }

        private BinXmlToken NextToken()
        {
            int pos = this.pos;
            if (pos < this.end)
            {
                BinXmlToken token = (BinXmlToken) this.data[pos];
                if ((token < BinXmlToken.NmFlush) || (token > BinXmlToken.Name))
                {
                    this.pos = pos + 1;
                    return token;
                }
            }
            return this.NextToken1();
        }

        private BinXmlToken NextToken1()
        {
            BinXmlToken token;
            int pos = this.pos;
            if (pos >= this.end)
            {
                token = this.ReadToken();
            }
            else
            {
                token = (BinXmlToken) this.data[pos];
                this.pos = pos + 1;
            }
            if ((token >= BinXmlToken.NmFlush) && (token <= BinXmlToken.Name))
            {
                return this.NextToken2(token);
            }
            return token;
        }

        private BinXmlToken NextToken2(BinXmlToken token)
        {
            while (true)
            {
                switch (token)
                {
                    case BinXmlToken.NmFlush:
                        this.NameFlush();
                        break;

                    case BinXmlToken.Extn:
                        this.SkipExtn();
                        break;

                    case BinXmlToken.QName:
                        this.AddQName();
                        break;

                    case BinXmlToken.Name:
                        this.AddName();
                        break;

                    default:
                        return token;
                }
                token = this.ReadToken();
            }
        }

        private int ParseMB32()
        {
            byte b = this.ReadByte();
            if (b > 0x7f)
            {
                return this.ParseMB32_(b);
            }
            return b;
        }

        private int ParseMB32(int pos)
        {
            byte[] data = this.data;
            byte num3 = data[pos++];
            uint num = (uint) (num3 & 0x7f);
            if (num3 > 0x7f)
            {
                num3 = data[pos++];
                uint num2 = (uint) (num3 & 0x7f);
                num += num2 << 7;
                if (num3 > 0x7f)
                {
                    num3 = data[pos++];
                    num2 = (uint) (num3 & 0x7f);
                    num += num2 << 14;
                    if (num3 > 0x7f)
                    {
                        num3 = data[pos++];
                        num2 = (uint) (num3 & 0x7f);
                        num += num2 << 0x15;
                        if (num3 > 0x7f)
                        {
                            num3 = data[pos++];
                            num2 = (uint) (num3 & 7);
                            if (num3 > 7)
                            {
                                throw this.ThrowXmlException("XmlBinary_ValueTooBig");
                            }
                            num += num2 << 0x1c;
                        }
                    }
                }
            }
            return (int) num;
        }

        private int ParseMB32_(byte b)
        {
            uint num = (uint) (b & 0x7f);
            b = this.ReadByte();
            uint num2 = (uint) (b & 0x7f);
            num += num2 << 7;
            if (b > 0x7f)
            {
                b = this.ReadByte();
                num2 = (uint) (b & 0x7f);
                num += num2 << 14;
                if (b > 0x7f)
                {
                    b = this.ReadByte();
                    num2 = (uint) (b & 0x7f);
                    num += num2 << 0x15;
                    if (b > 0x7f)
                    {
                        b = this.ReadByte();
                        num2 = (uint) (b & 7);
                        if (b > 7)
                        {
                            throw this.ThrowXmlException("XmlBinary_ValueTooBig");
                        }
                        num += num2 << 0x1c;
                    }
                }
            }
            return (int) num;
        }

        private int ParseMB64()
        {
            byte b = this.ReadByte();
            if (b > 0x7f)
            {
                return this.ParseMB32_(b);
            }
            return b;
        }

        private string ParseText()
        {
            string str;
            int mark = this.mark;
            try
            {
                int num3;
                if (mark < 0)
                {
                    this.mark = this.pos;
                }
                int cch = this.ScanText(out num3);
                str = this.GetString(num3, cch);
            }
            finally
            {
                if (mark < 0)
                {
                    this.mark = -1;
                }
            }
            return str;
        }

        private BinXmlToken PeekNextToken()
        {
            BinXmlToken token = this.NextToken();
            if (BinXmlToken.EOF != token)
            {
                this.pos--;
            }
            return token;
        }

        private BinXmlToken PeekToken()
        {
            while ((this.pos >= this.end) && this.FillAllowEOF())
            {
            }
            if (this.pos >= this.end)
            {
                return BinXmlToken.EOF;
            }
            return (BinXmlToken) this.data[this.pos];
        }

        private void PopNamespaces(NamespaceDecl firstInScopeChain)
        {
            NamespaceDecl scopeLink;
            for (NamespaceDecl decl = firstInScopeChain; decl != null; decl = scopeLink)
            {
                if (decl.prevLink == null)
                {
                    this.namespaces.Remove(decl.prefix);
                }
                else
                {
                    this.namespaces[decl.prefix] = decl.prevLink;
                }
                scopeLink = decl.scopeLink;
                decl.prevLink = null;
                decl.scopeLink = null;
            }
        }

        private void PositionOnAttribute(int i)
        {
            this.attrIndex = i;
            this.qnameOther = this.attributes[i - 1].name;
            if (this.state == ScanState.Doc)
            {
                this.parentNodeType = this.nodetype;
            }
            this.token = BinXmlToken.Attr;
            this.nodetype = XmlNodeType.Attribute;
            this.state = ScanState.Attr;
            this.valueType = TypeOfObject;
            this.stringValue = null;
        }

        private void PushNamespace(string prefix, string ns, bool implied)
        {
            if (prefix != "xml")
            {
                NamespaceDecl decl;
                int elemDepth = this.elemDepth;
                this.namespaces.TryGetValue(prefix, out decl);
                if (decl != null)
                {
                    if (decl.uri == ns)
                    {
                        if ((!implied && decl.implied) && (decl.scope == elemDepth))
                        {
                            decl.implied = false;
                        }
                        return;
                    }
                    this.qnameElement.CheckPrefixNS(prefix, ns);
                    if (prefix.Length != 0)
                    {
                        for (int i = 0; i < this.attrCount; i++)
                        {
                            if (this.attributes[i].name.prefix.Length != 0)
                            {
                                this.attributes[i].name.CheckPrefixNS(prefix, ns);
                            }
                        }
                    }
                }
                NamespaceDecl decl2 = new NamespaceDecl(prefix, ns, this.elementStack[elemDepth].nsdecls, decl, elemDepth, implied);
                this.elementStack[elemDepth].nsdecls = decl2;
                this.namespaces[prefix] = decl2;
            }
        }

        public override bool Read()
        {
            bool flag;
            try
            {
                switch (this.state)
                {
                    case ScanState.Doc:
                        break;

                    case ScanState.XmlText:
                        if (this.textXmlReader.Read())
                        {
                            return this.UpdateFromTextReader(true);
                        }
                        this.state = ScanState.Doc;
                        this.nodetype = XmlNodeType.None;
                        this.isEmpty = false;
                        break;

                    case ScanState.Attr:
                    case ScanState.AttrVal:
                    case ScanState.AttrValPseudoValue:
                        this.MoveToElement();
                        break;

                    case ScanState.Init:
                        return this.ReadInit(false);

                    default:
                        goto Label_0071;
                }
                return this.ReadDoc();
            Label_0071:
                flag = false;
            }
            catch (OverflowException exception)
            {
                this.state = ScanState.Error;
                throw new XmlException(exception.Message, exception);
            }
            catch
            {
                this.state = ScanState.Error;
                throw;
            }
            return flag;
        }

        public override bool ReadAttributeValue()
        {
            this.stringValue = null;
            switch (this.state)
            {
                case ScanState.XmlText:
                    return this.UpdateFromTextReader(this.textXmlReader.ReadAttributeValue());

                case ScanState.Attr:
                {
                    if (this.attributes[this.attrIndex - 1].val != null)
                    {
                        this.token = BinXmlToken.Error;
                        this.valueType = TypeOfString;
                        this.state = ScanState.AttrValPseudoValue;
                        break;
                    }
                    this.pos = this.attributes[this.attrIndex - 1].contentPos;
                    BinXmlToken token = this.RescanNextToken();
                    if ((BinXmlToken.Attr != token) && (BinXmlToken.EndAttrs != token))
                    {
                        this.token = token;
                        this.ReScanOverValue(token);
                        this.valueType = this.GetValueType(token);
                        this.state = ScanState.AttrVal;
                        break;
                    }
                    return false;
                }
                case ScanState.AttrVal:
                    return false;

                default:
                    return false;
            }
            this.qnameOther.Clear();
            this.nodetype = XmlNodeType.Text;
            return true;
        }

        private byte ReadByte()
        {
            this.Fill(0);
            return this.data[this.pos++];
        }

        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            int pos = this.pos;
            try
            {
                if (this.SetupContentAsXXX("ReadContentAs"))
                {
                    object obj2;
                    try
                    {
                        if ((this.NodeType == XmlNodeType.Element) || (this.NodeType == XmlNodeType.EndElement))
                        {
                            obj2 = string.Empty;
                        }
                        else if ((returnType == this.ValueType) || (returnType == typeof(object)))
                        {
                            obj2 = this.ValueAsObject(this.token, false);
                        }
                        else
                        {
                            obj2 = this.ValueAs(this.token, returnType, namespaceResolver);
                        }
                    }
                    catch (InvalidCastException exception)
                    {
                        throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception, null);
                    }
                    catch (FormatException exception2)
                    {
                        throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception2, null);
                    }
                    catch (OverflowException exception3)
                    {
                        throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception3, null);
                    }
                    pos = this.FinishContentAsXXX(pos);
                    return obj2;
                }
            }
            finally
            {
                this.pos = pos;
            }
            return base.ReadContentAs(returnType, namespaceResolver);
        }

        public override bool ReadContentAsBoolean()
        {
            int pos = this.pos;
            bool flag = false;
            try
            {
                if (!this.SetupContentAsXXX("ReadContentAsBoolean"))
                {
                    goto Label_019C;
                }
                try
                {
                    switch (this.token)
                    {
                        case BinXmlToken.SQL_SMALLINT:
                        case BinXmlToken.SQL_INT:
                        case BinXmlToken.SQL_REAL:
                        case BinXmlToken.SQL_FLOAT:
                        case BinXmlToken.SQL_MONEY:
                        case BinXmlToken.SQL_BIT:
                        case BinXmlToken.SQL_TINYINT:
                        case BinXmlToken.SQL_BIGINT:
                        case BinXmlToken.SQL_UUID:
                        case BinXmlToken.SQL_DECIMAL:
                        case BinXmlToken.SQL_NUMERIC:
                        case BinXmlToken.SQL_BINARY:
                        case BinXmlToken.SQL_VARBINARY:
                        case BinXmlToken.SQL_DATETIME:
                        case BinXmlToken.SQL_SMALLDATETIME:
                        case BinXmlToken.SQL_SMALLMONEY:
                        case BinXmlToken.SQL_IMAGE:
                        case BinXmlToken.SQL_UDT:
                        case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                        case BinXmlToken.XSD_KATMAI_TIME:
                        case BinXmlToken.XSD_KATMAI_DATETIME:
                        case BinXmlToken.XSD_KATMAI_DATE:
                        case BinXmlToken.XSD_TIME:
                        case BinXmlToken.XSD_DATETIME:
                        case BinXmlToken.XSD_DATE:
                        case BinXmlToken.XSD_BINHEX:
                        case BinXmlToken.XSD_BASE64:
                        case BinXmlToken.XSD_DECIMAL:
                        case BinXmlToken.XSD_BYTE:
                        case BinXmlToken.XSD_UNSIGNEDSHORT:
                        case BinXmlToken.XSD_UNSIGNEDINT:
                        case BinXmlToken.XSD_UNSIGNEDLONG:
                        case BinXmlToken.XSD_QNAME:
                            throw new InvalidCastException(Res.GetString("XmlBinary_CastNotSupported", new object[] { this.token, "Boolean" }));

                        case BinXmlToken.XSD_BOOLEAN:
                            flag = 0 != this.data[this.tokDataPos];
                            goto Label_0185;

                        case BinXmlToken.EndElem:
                        case BinXmlToken.Element:
                            return XmlConvert.ToBoolean(string.Empty);
                    }
                    goto Label_019C;
                }
                catch (InvalidCastException exception)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Boolean", exception, null);
                }
                catch (FormatException exception2)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Boolean", exception2, null);
                }
            Label_0185:
                pos = this.FinishContentAsXXX(pos);
                return flag;
            }
            finally
            {
                this.pos = pos;
            }
        Label_019C:
            return base.ReadContentAsBoolean();
        }

        public override DateTime ReadContentAsDateTime()
        {
            int pos = this.pos;
            try
            {
                DateTime time;
                if (!this.SetupContentAsXXX("ReadContentAsDateTime"))
                {
                    goto Label_01A3;
                }
                try
                {
                    switch (this.token)
                    {
                        case BinXmlToken.SQL_SMALLINT:
                        case BinXmlToken.SQL_INT:
                        case BinXmlToken.SQL_REAL:
                        case BinXmlToken.SQL_FLOAT:
                        case BinXmlToken.SQL_MONEY:
                        case BinXmlToken.SQL_BIT:
                        case BinXmlToken.SQL_TINYINT:
                        case BinXmlToken.SQL_BIGINT:
                        case BinXmlToken.SQL_UUID:
                        case BinXmlToken.SQL_DECIMAL:
                        case BinXmlToken.SQL_NUMERIC:
                        case BinXmlToken.SQL_BINARY:
                        case BinXmlToken.SQL_VARBINARY:
                        case BinXmlToken.SQL_SMALLMONEY:
                        case BinXmlToken.SQL_IMAGE:
                        case BinXmlToken.SQL_UDT:
                        case BinXmlToken.XSD_BINHEX:
                        case BinXmlToken.XSD_BASE64:
                        case BinXmlToken.XSD_BOOLEAN:
                        case BinXmlToken.XSD_DECIMAL:
                        case BinXmlToken.XSD_BYTE:
                        case BinXmlToken.XSD_UNSIGNEDSHORT:
                        case BinXmlToken.XSD_UNSIGNEDINT:
                        case BinXmlToken.XSD_UNSIGNEDLONG:
                        case BinXmlToken.XSD_QNAME:
                            throw new InvalidCastException(Res.GetString("XmlBinary_CastNotSupported", new object[] { this.token, "DateTime" }));

                        case BinXmlToken.SQL_DATETIME:
                        case BinXmlToken.SQL_SMALLDATETIME:
                        case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                        case BinXmlToken.XSD_KATMAI_TIME:
                        case BinXmlToken.XSD_KATMAI_DATETIME:
                        case BinXmlToken.XSD_KATMAI_DATE:
                        case BinXmlToken.XSD_TIME:
                        case BinXmlToken.XSD_DATETIME:
                        case BinXmlToken.XSD_DATE:
                            time = this.ValueAsDateTime();
                            goto Label_018C;

                        case BinXmlToken.EndElem:
                        case BinXmlToken.Element:
                            return XmlConvert.ToDateTime(string.Empty, XmlDateTimeSerializationMode.RoundtripKind);
                    }
                    goto Label_01A3;
                }
                catch (InvalidCastException exception)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "DateTime", exception, null);
                }
                catch (FormatException exception2)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "DateTime", exception2, null);
                }
                catch (OverflowException exception3)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "DateTime", exception3, null);
                }
            Label_018C:
                pos = this.FinishContentAsXXX(pos);
                return time;
            }
            finally
            {
                this.pos = pos;
            }
        Label_01A3:
            return base.ReadContentAsDateTime();
        }

        public override decimal ReadContentAsDecimal()
        {
            int pos = this.pos;
            try
            {
                decimal num2;
                if (!this.SetupContentAsXXX("ReadContentAsDecimal"))
                {
                    goto Label_01A2;
                }
                try
                {
                    switch (this.token)
                    {
                        case BinXmlToken.SQL_SMALLINT:
                        case BinXmlToken.SQL_INT:
                        case BinXmlToken.SQL_MONEY:
                        case BinXmlToken.SQL_BIT:
                        case BinXmlToken.SQL_TINYINT:
                        case BinXmlToken.SQL_BIGINT:
                        case BinXmlToken.SQL_DECIMAL:
                        case BinXmlToken.SQL_NUMERIC:
                        case BinXmlToken.SQL_SMALLMONEY:
                        case BinXmlToken.XSD_DECIMAL:
                        case BinXmlToken.XSD_BYTE:
                        case BinXmlToken.XSD_UNSIGNEDSHORT:
                        case BinXmlToken.XSD_UNSIGNEDINT:
                        case BinXmlToken.XSD_UNSIGNEDLONG:
                            num2 = this.ValueAsDecimal();
                            goto Label_018B;

                        case BinXmlToken.SQL_REAL:
                        case BinXmlToken.SQL_FLOAT:
                        case BinXmlToken.SQL_UUID:
                        case BinXmlToken.SQL_BINARY:
                        case BinXmlToken.SQL_VARBINARY:
                        case BinXmlToken.SQL_DATETIME:
                        case BinXmlToken.SQL_SMALLDATETIME:
                        case BinXmlToken.SQL_IMAGE:
                        case BinXmlToken.SQL_UDT:
                        case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                        case BinXmlToken.XSD_KATMAI_TIME:
                        case BinXmlToken.XSD_KATMAI_DATETIME:
                        case BinXmlToken.XSD_KATMAI_DATE:
                        case BinXmlToken.XSD_TIME:
                        case BinXmlToken.XSD_DATETIME:
                        case BinXmlToken.XSD_DATE:
                        case BinXmlToken.XSD_BINHEX:
                        case BinXmlToken.XSD_BASE64:
                        case BinXmlToken.XSD_BOOLEAN:
                        case BinXmlToken.XSD_QNAME:
                            throw new InvalidCastException(Res.GetString("XmlBinary_CastNotSupported", new object[] { this.token, "Decimal" }));

                        case BinXmlToken.EndElem:
                        case BinXmlToken.Element:
                            return XmlConvert.ToDecimal(string.Empty);
                    }
                    goto Label_01A2;
                }
                catch (InvalidCastException exception)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Decimal", exception, null);
                }
                catch (FormatException exception2)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Decimal", exception2, null);
                }
                catch (OverflowException exception3)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Decimal", exception3, null);
                }
            Label_018B:
                pos = this.FinishContentAsXXX(pos);
                return num2;
            }
            finally
            {
                this.pos = pos;
            }
        Label_01A2:
            return base.ReadContentAsDecimal();
        }

        public override double ReadContentAsDouble()
        {
            int pos = this.pos;
            try
            {
                double num2;
                if (!this.SetupContentAsXXX("ReadContentAsDouble"))
                {
                    goto Label_01A2;
                }
                try
                {
                    switch (this.token)
                    {
                        case BinXmlToken.SQL_SMALLINT:
                        case BinXmlToken.SQL_INT:
                        case BinXmlToken.SQL_MONEY:
                        case BinXmlToken.SQL_BIT:
                        case BinXmlToken.SQL_TINYINT:
                        case BinXmlToken.SQL_BIGINT:
                        case BinXmlToken.SQL_UUID:
                        case BinXmlToken.SQL_DECIMAL:
                        case BinXmlToken.SQL_NUMERIC:
                        case BinXmlToken.SQL_BINARY:
                        case BinXmlToken.SQL_VARBINARY:
                        case BinXmlToken.SQL_DATETIME:
                        case BinXmlToken.SQL_SMALLDATETIME:
                        case BinXmlToken.SQL_SMALLMONEY:
                        case BinXmlToken.SQL_IMAGE:
                        case BinXmlToken.SQL_UDT:
                        case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                        case BinXmlToken.XSD_KATMAI_TIME:
                        case BinXmlToken.XSD_KATMAI_DATETIME:
                        case BinXmlToken.XSD_KATMAI_DATE:
                        case BinXmlToken.XSD_TIME:
                        case BinXmlToken.XSD_DATETIME:
                        case BinXmlToken.XSD_DATE:
                        case BinXmlToken.XSD_BINHEX:
                        case BinXmlToken.XSD_BASE64:
                        case BinXmlToken.XSD_BOOLEAN:
                        case BinXmlToken.XSD_DECIMAL:
                        case BinXmlToken.XSD_BYTE:
                        case BinXmlToken.XSD_UNSIGNEDSHORT:
                        case BinXmlToken.XSD_UNSIGNEDINT:
                        case BinXmlToken.XSD_UNSIGNEDLONG:
                        case BinXmlToken.XSD_QNAME:
                            throw new InvalidCastException(Res.GetString("XmlBinary_CastNotSupported", new object[] { this.token, "Double" }));

                        case BinXmlToken.SQL_REAL:
                        case BinXmlToken.SQL_FLOAT:
                            num2 = this.ValueAsDouble();
                            goto Label_018B;

                        case BinXmlToken.EndElem:
                        case BinXmlToken.Element:
                            return XmlConvert.ToDouble(string.Empty);
                    }
                    goto Label_01A2;
                }
                catch (InvalidCastException exception)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Double", exception, null);
                }
                catch (FormatException exception2)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Double", exception2, null);
                }
                catch (OverflowException exception3)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Double", exception3, null);
                }
            Label_018B:
                pos = this.FinishContentAsXXX(pos);
                return num2;
            }
            finally
            {
                this.pos = pos;
            }
        Label_01A2:
            return base.ReadContentAsDouble();
        }

        public override float ReadContentAsFloat()
        {
            int pos = this.pos;
            try
            {
                float num2;
                if (!this.SetupContentAsXXX("ReadContentAsFloat"))
                {
                    goto Label_01A3;
                }
                try
                {
                    switch (this.token)
                    {
                        case BinXmlToken.SQL_SMALLINT:
                        case BinXmlToken.SQL_INT:
                        case BinXmlToken.SQL_MONEY:
                        case BinXmlToken.SQL_BIT:
                        case BinXmlToken.SQL_TINYINT:
                        case BinXmlToken.SQL_BIGINT:
                        case BinXmlToken.SQL_UUID:
                        case BinXmlToken.SQL_DECIMAL:
                        case BinXmlToken.SQL_NUMERIC:
                        case BinXmlToken.SQL_BINARY:
                        case BinXmlToken.SQL_VARBINARY:
                        case BinXmlToken.SQL_DATETIME:
                        case BinXmlToken.SQL_SMALLDATETIME:
                        case BinXmlToken.SQL_SMALLMONEY:
                        case BinXmlToken.SQL_IMAGE:
                        case BinXmlToken.SQL_UDT:
                        case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                        case BinXmlToken.XSD_KATMAI_TIME:
                        case BinXmlToken.XSD_KATMAI_DATETIME:
                        case BinXmlToken.XSD_KATMAI_DATE:
                        case BinXmlToken.XSD_TIME:
                        case BinXmlToken.XSD_DATETIME:
                        case BinXmlToken.XSD_DATE:
                        case BinXmlToken.XSD_BINHEX:
                        case BinXmlToken.XSD_BASE64:
                        case BinXmlToken.XSD_BOOLEAN:
                        case BinXmlToken.XSD_DECIMAL:
                        case BinXmlToken.XSD_BYTE:
                        case BinXmlToken.XSD_UNSIGNEDSHORT:
                        case BinXmlToken.XSD_UNSIGNEDINT:
                        case BinXmlToken.XSD_UNSIGNEDLONG:
                        case BinXmlToken.XSD_QNAME:
                            throw new InvalidCastException(Res.GetString("XmlBinary_CastNotSupported", new object[] { this.token, "Float" }));

                        case BinXmlToken.SQL_REAL:
                        case BinXmlToken.SQL_FLOAT:
                            num2 = (float) this.ValueAsDouble();
                            goto Label_018C;

                        case BinXmlToken.EndElem:
                        case BinXmlToken.Element:
                            return XmlConvert.ToSingle(string.Empty);
                    }
                    goto Label_01A3;
                }
                catch (InvalidCastException exception)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Float", exception, null);
                }
                catch (FormatException exception2)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Float", exception2, null);
                }
                catch (OverflowException exception3)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Float", exception3, null);
                }
            Label_018C:
                pos = this.FinishContentAsXXX(pos);
                return num2;
            }
            finally
            {
                this.pos = pos;
            }
        Label_01A3:
            return base.ReadContentAsFloat();
        }

        public override int ReadContentAsInt()
        {
            int pos = this.pos;
            try
            {
                int num2;
                if (!this.SetupContentAsXXX("ReadContentAsInt"))
                {
                    goto Label_01A3;
                }
                try
                {
                    switch (this.token)
                    {
                        case BinXmlToken.SQL_SMALLINT:
                        case BinXmlToken.SQL_INT:
                        case BinXmlToken.SQL_MONEY:
                        case BinXmlToken.SQL_BIT:
                        case BinXmlToken.SQL_TINYINT:
                        case BinXmlToken.SQL_BIGINT:
                        case BinXmlToken.SQL_DECIMAL:
                        case BinXmlToken.SQL_NUMERIC:
                        case BinXmlToken.SQL_SMALLMONEY:
                        case BinXmlToken.XSD_DECIMAL:
                        case BinXmlToken.XSD_BYTE:
                        case BinXmlToken.XSD_UNSIGNEDSHORT:
                        case BinXmlToken.XSD_UNSIGNEDINT:
                        case BinXmlToken.XSD_UNSIGNEDLONG:
                            num2 = (int) this.ValueAsLong();
                            goto Label_018C;

                        case BinXmlToken.SQL_REAL:
                        case BinXmlToken.SQL_FLOAT:
                        case BinXmlToken.SQL_UUID:
                        case BinXmlToken.SQL_BINARY:
                        case BinXmlToken.SQL_VARBINARY:
                        case BinXmlToken.SQL_DATETIME:
                        case BinXmlToken.SQL_SMALLDATETIME:
                        case BinXmlToken.SQL_IMAGE:
                        case BinXmlToken.SQL_UDT:
                        case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                        case BinXmlToken.XSD_KATMAI_TIME:
                        case BinXmlToken.XSD_KATMAI_DATETIME:
                        case BinXmlToken.XSD_KATMAI_DATE:
                        case BinXmlToken.XSD_TIME:
                        case BinXmlToken.XSD_DATETIME:
                        case BinXmlToken.XSD_DATE:
                        case BinXmlToken.XSD_BINHEX:
                        case BinXmlToken.XSD_BASE64:
                        case BinXmlToken.XSD_BOOLEAN:
                        case BinXmlToken.XSD_QNAME:
                            throw new InvalidCastException(Res.GetString("XmlBinary_CastNotSupported", new object[] { this.token, "Int32" }));

                        case BinXmlToken.EndElem:
                        case BinXmlToken.Element:
                            return XmlConvert.ToInt32(string.Empty);
                    }
                    goto Label_01A3;
                }
                catch (InvalidCastException exception)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Int32", exception, null);
                }
                catch (FormatException exception2)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Int32", exception2, null);
                }
                catch (OverflowException exception3)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Int32", exception3, null);
                }
            Label_018C:
                pos = this.FinishContentAsXXX(pos);
                return num2;
            }
            finally
            {
                this.pos = pos;
            }
        Label_01A3:
            return base.ReadContentAsInt();
        }

        public override long ReadContentAsLong()
        {
            int pos = this.pos;
            try
            {
                long num2;
                if (!this.SetupContentAsXXX("ReadContentAsLong"))
                {
                    goto Label_01A2;
                }
                try
                {
                    switch (this.token)
                    {
                        case BinXmlToken.SQL_SMALLINT:
                        case BinXmlToken.SQL_INT:
                        case BinXmlToken.SQL_MONEY:
                        case BinXmlToken.SQL_BIT:
                        case BinXmlToken.SQL_TINYINT:
                        case BinXmlToken.SQL_BIGINT:
                        case BinXmlToken.SQL_DECIMAL:
                        case BinXmlToken.SQL_NUMERIC:
                        case BinXmlToken.SQL_SMALLMONEY:
                        case BinXmlToken.XSD_DECIMAL:
                        case BinXmlToken.XSD_BYTE:
                        case BinXmlToken.XSD_UNSIGNEDSHORT:
                        case BinXmlToken.XSD_UNSIGNEDINT:
                        case BinXmlToken.XSD_UNSIGNEDLONG:
                            num2 = this.ValueAsLong();
                            goto Label_018B;

                        case BinXmlToken.SQL_REAL:
                        case BinXmlToken.SQL_FLOAT:
                        case BinXmlToken.SQL_UUID:
                        case BinXmlToken.SQL_BINARY:
                        case BinXmlToken.SQL_VARBINARY:
                        case BinXmlToken.SQL_DATETIME:
                        case BinXmlToken.SQL_SMALLDATETIME:
                        case BinXmlToken.SQL_IMAGE:
                        case BinXmlToken.SQL_UDT:
                        case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                        case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                        case BinXmlToken.XSD_KATMAI_TIME:
                        case BinXmlToken.XSD_KATMAI_DATETIME:
                        case BinXmlToken.XSD_KATMAI_DATE:
                        case BinXmlToken.XSD_TIME:
                        case BinXmlToken.XSD_DATETIME:
                        case BinXmlToken.XSD_DATE:
                        case BinXmlToken.XSD_BINHEX:
                        case BinXmlToken.XSD_BASE64:
                        case BinXmlToken.XSD_BOOLEAN:
                        case BinXmlToken.XSD_QNAME:
                            throw new InvalidCastException(Res.GetString("XmlBinary_CastNotSupported", new object[] { this.token, "Int64" }));

                        case BinXmlToken.EndElem:
                        case BinXmlToken.Element:
                            return XmlConvert.ToInt64(string.Empty);
                    }
                    goto Label_01A2;
                }
                catch (InvalidCastException exception)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Int64", exception, null);
                }
                catch (FormatException exception2)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Int64", exception2, null);
                }
                catch (OverflowException exception3)
                {
                    throw new XmlException("Xml_ReadContentAsFormatException", "Int64", exception3, null);
                }
            Label_018B:
                pos = this.FinishContentAsXXX(pos);
                return num2;
            }
            finally
            {
                this.pos = pos;
            }
        Label_01A2:
            return base.ReadContentAsLong();
        }

        public override object ReadContentAsObject()
        {
            int pos = this.pos;
            try
            {
                if (this.SetupContentAsXXX("ReadContentAsObject"))
                {
                    object obj2;
                    try
                    {
                        if ((this.NodeType == XmlNodeType.Element) || (this.NodeType == XmlNodeType.EndElement))
                        {
                            obj2 = string.Empty;
                        }
                        else
                        {
                            obj2 = this.ValueAsObject(this.token, false);
                        }
                    }
                    catch (InvalidCastException exception)
                    {
                        throw new XmlException("Xml_ReadContentAsFormatException", "Object", exception, null);
                    }
                    catch (FormatException exception2)
                    {
                        throw new XmlException("Xml_ReadContentAsFormatException", "Object", exception2, null);
                    }
                    catch (OverflowException exception3)
                    {
                        throw new XmlException("Xml_ReadContentAsFormatException", "Object", exception3, null);
                    }
                    pos = this.FinishContentAsXXX(pos);
                    return obj2;
                }
            }
            finally
            {
                this.pos = pos;
            }
            return base.ReadContentAsObject();
        }

        private bool ReadDoc()
        {
            switch (this.nodetype)
            {
                case XmlNodeType.Element:
                    if (this.isEmpty)
                    {
                        this.FinishEndElement();
                        this.isEmpty = false;
                    }
                    break;

                case XmlNodeType.CDATA:
                    this.FinishCDATA();
                    break;

                case XmlNodeType.EndElement:
                    this.FinishEndElement();
                    break;
            }
        Label_003B:
            this.nodetype = XmlNodeType.None;
            this.mark = -1;
            if (this.qnameOther.localname.Length != 0)
            {
                this.qnameOther.Clear();
            }
            this.ClearAttributes();
            this.attrCount = 0;
            this.valueType = TypeOfString;
            this.stringValue = null;
            this.hasTypedValue = false;
            this.token = this.NextToken();
            switch (this.token)
            {
                case BinXmlToken.EOF:
                    if (this.elemDepth > 0)
                    {
                        throw new XmlException("Xml_UnexpectedEOF1", null);
                    }
                    this.state = ScanState.EOF;
                    return false;

                case BinXmlToken.SQL_SMALLINT:
                case BinXmlToken.SQL_INT:
                case BinXmlToken.SQL_REAL:
                case BinXmlToken.SQL_FLOAT:
                case BinXmlToken.SQL_MONEY:
                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT:
                case BinXmlToken.SQL_BIGINT:
                case BinXmlToken.SQL_UUID:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.SQL_BINARY:
                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_VARBINARY:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_DATETIME:
                case BinXmlToken.SQL_SMALLDATETIME:
                case BinXmlToken.SQL_SMALLMONEY:
                case BinXmlToken.SQL_TEXT:
                case BinXmlToken.SQL_IMAGE:
                case BinXmlToken.SQL_NTEXT:
                case BinXmlToken.SQL_UDT:
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                case BinXmlToken.XSD_KATMAI_TIME:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                case BinXmlToken.XSD_KATMAI_DATE:
                case BinXmlToken.XSD_TIME:
                case BinXmlToken.XSD_DATETIME:
                case BinXmlToken.XSD_DATE:
                case BinXmlToken.XSD_BINHEX:
                case BinXmlToken.XSD_BASE64:
                case BinXmlToken.XSD_BOOLEAN:
                case BinXmlToken.XSD_DECIMAL:
                case BinXmlToken.XSD_BYTE:
                case BinXmlToken.XSD_UNSIGNEDSHORT:
                case BinXmlToken.XSD_UNSIGNEDINT:
                case BinXmlToken.XSD_UNSIGNEDLONG:
                case BinXmlToken.XSD_QNAME:
                    this.ImplReadData(this.token);
                    if (XmlNodeType.Text == this.nodetype)
                    {
                        this.CheckAllowContent();
                    }
                    else if (this.ignoreWhitespace && !this.xmlspacePreserve)
                    {
                        goto Label_003B;
                    }
                    return true;

                case BinXmlToken.EndNest:
                    if (this.prevNameInfo == null)
                    {
                        break;
                    }
                    this.ImplReadEndNest();
                    return this.ReadDoc();

                case BinXmlToken.Nest:
                    this.ImplReadNest();
                    this.sniffed = false;
                    return this.ReadInit(true);

                case BinXmlToken.XmlText:
                    this.ImplReadXmlText();
                    goto Label_02C9;

                case BinXmlToken.CData:
                    this.ImplReadCDATA();
                    goto Label_02C9;

                case BinXmlToken.Comment:
                    this.ImplReadComment();
                    if (!this.ignoreComments)
                    {
                        goto Label_02C9;
                    }
                    goto Label_003B;

                case BinXmlToken.PI:
                    this.ImplReadPI();
                    if (!this.ignorePIs)
                    {
                        goto Label_02C9;
                    }
                    goto Label_003B;

                case BinXmlToken.EndElem:
                    this.ImplReadEndElement();
                    goto Label_02C9;

                case BinXmlToken.Element:
                    this.ImplReadElement();
                    goto Label_02C9;

                case BinXmlToken.DocType:
                    this.ImplReadDoctype();
                    if ((this.dtdProcessing != DtdProcessing.Ignore) && (this.prevNameInfo == null))
                    {
                        goto Label_02C9;
                    }
                    goto Label_003B;
            }
            throw this.ThrowUnexpectedToken(this.token);
        Label_02C9:
            return true;
        }

        private bool ReadInit(bool skipXmlDecl)
        {
            string res = null;
            if (!this.sniffed && (this.ReadUShort() != 0xffdf))
            {
                res = "XmlBinary_InvalidSignature";
            }
            else
            {
                this.version = this.ReadByte();
                if ((this.version != 1) && (this.version != 2))
                {
                    res = "XmlBinary_InvalidProtocolVersion";
                }
                else if (0x4b0 != this.ReadUShort())
                {
                    res = "XmlBinary_UnsupportedCodePage";
                }
                else
                {
                    this.state = ScanState.Doc;
                    if (BinXmlToken.XmlDecl != this.PeekToken())
                    {
                        goto Label_01EE;
                    }
                    this.pos++;
                    this.attributes[0].Set(new QName(string.Empty, this.xnt.Add("version"), string.Empty), this.ParseText());
                    this.attrCount = 1;
                    if (BinXmlToken.Encoding == this.PeekToken())
                    {
                        this.pos++;
                        this.attributes[1].Set(new QName(string.Empty, this.xnt.Add("encoding"), string.Empty), this.ParseText());
                        this.attrCount++;
                    }
                    byte num2 = this.ReadByte();
                    switch (num2)
                    {
                        case 0:
                            goto Label_01A2;

                        case 1:
                        case 2:
                            this.attributes[this.attrCount].Set(new QName(string.Empty, this.xnt.Add("standalone"), string.Empty), (num2 == 1) ? "yes" : "no");
                            this.attrCount++;
                            goto Label_01A2;
                    }
                    res = "XmlBinary_InvalidStandalone";
                }
            }
            this.state = ScanState.Error;
            throw new XmlException(res, null);
        Label_01A2:
            if (!skipXmlDecl)
            {
                QName name = new QName(string.Empty, this.xnt.Add("xml"), string.Empty);
                this.qnameOther = this.qnameElement = name;
                this.nodetype = XmlNodeType.XmlDeclaration;
                this.posAfterAttrs = this.pos;
                return true;
            }
        Label_01EE:
            return this.ReadDoc();
        }

        private int ReadNameRef()
        {
            int num = this.ParseMB32();
            if ((num < 0) || (num >= this.symbolTables.symCount))
            {
                throw new XmlException("XmlBin_InvalidQNameID", string.Empty);
            }
            return num;
        }

        private int ReadQNameRef()
        {
            int num = this.ParseMB32();
            if ((num < 0) || (num >= this.symbolTables.qnameCount))
            {
                throw new XmlException("XmlBin_InvalidQNameID", string.Empty);
            }
            return num;
        }

        private BinXmlToken ReadToken()
        {
            while ((this.pos >= this.end) && this.FillAllowEOF())
            {
            }
            if (this.pos >= this.end)
            {
                return BinXmlToken.EOF;
            }
            return (BinXmlToken) this.data[this.pos++];
        }

        private ushort ReadUShort()
        {
            this.Fill(1);
            int pos = this.pos;
            byte[] data = this.data;
            ushort num2 = (ushort) (data[pos] + (data[pos + 1] << 8));
            this.pos += 2;
            return num2;
        }

        private BinXmlToken RescanNextToken()
        {
            BinXmlToken token;
        Label_0000:
            token = this.ReadToken();
            switch (token)
            {
                case BinXmlToken.NmFlush:
                    goto Label_0000;

                case BinXmlToken.Extn:
                {
                    int num2 = this.ParseMB32();
                    this.pos += num2;
                    goto Label_0000;
                }
                case BinXmlToken.QName:
                    this.ParseMB32();
                    this.ParseMB32();
                    this.ParseMB32();
                    goto Label_0000;

                case BinXmlToken.Name:
                {
                    int num = this.ParseMB32();
                    this.pos += 2 * num;
                    goto Label_0000;
                }
            }
            return token;
        }

        private void ReScanOverValue(BinXmlToken token)
        {
            this.ScanOverValue(token, true, false);
        }

        public override void ResolveEntity()
        {
            throw new NotSupportedException();
        }

        private void ScanAttributes()
        {
            BinXmlToken token;
            int i = -1;
            int attrCount = -1;
            this.mark = this.pos;
            string prefix = null;
            bool flag = false;
            while (BinXmlToken.EndAttrs != (token = this.NextToken()))
            {
                if (BinXmlToken.Attr == token)
                {
                    if (prefix != null)
                    {
                        this.PushNamespace(prefix, string.Empty, false);
                        prefix = null;
                    }
                    if (this.attrCount == this.attributes.Length)
                    {
                        this.GrowAttributes();
                    }
                    QName n = this.symbolTables.qnametable[this.ReadQNameRef()];
                    this.attributes[this.attrCount].Set(n, this.pos);
                    if (n.prefix == "xml")
                    {
                        if (n.localname == "lang")
                        {
                            attrCount = this.attrCount;
                        }
                        else if (n.localname == "space")
                        {
                            i = this.attrCount;
                        }
                    }
                    else if (Ref.Equal(n.namespaceUri, this.nsxmlns))
                    {
                        prefix = n.localname;
                        if (prefix == "xmlns")
                        {
                            prefix = string.Empty;
                        }
                    }
                    else if (n.prefix.Length != 0)
                    {
                        if (n.namespaceUri.Length == 0)
                        {
                            throw new XmlException("Xml_PrefixForEmptyNs", string.Empty);
                        }
                        this.PushNamespace(n.prefix, n.namespaceUri, true);
                    }
                    else if (n.namespaceUri.Length != 0)
                    {
                        throw this.ThrowXmlException("XmlBinary_AttrWithNsNoPrefix", n.localname, n.namespaceUri);
                    }
                    this.attrCount++;
                    flag = false;
                }
                else
                {
                    this.ScanOverValue(token, true, true);
                    if (flag)
                    {
                        throw this.ThrowNotSupported("XmlBinary_ListsOfValuesNotSupported");
                    }
                    string stringValue = this.stringValue;
                    if (stringValue != null)
                    {
                        this.attributes[this.attrCount - 1].val = stringValue;
                        this.stringValue = null;
                    }
                    if (prefix != null)
                    {
                        string ns = this.xnt.Add(this.ValueAsString(token));
                        this.PushNamespace(prefix, ns, false);
                        prefix = null;
                    }
                    flag = true;
                }
            }
            if (i != -1)
            {
                string attributeText = this.GetAttributeText(i);
                System.Xml.XmlSpace none = System.Xml.XmlSpace.None;
                switch (attributeText)
                {
                    case "preserve":
                        none = System.Xml.XmlSpace.Preserve;
                        break;

                    case "default":
                        none = System.Xml.XmlSpace.Default;
                        break;
                }
                this.elementStack[this.elemDepth].xmlSpace = none;
                this.xmlspacePreserve = System.Xml.XmlSpace.Preserve == none;
            }
            if (attrCount != -1)
            {
                this.elementStack[this.elemDepth].xmlLang = this.GetAttributeText(attrCount);
            }
            if (this.attrCount < 200)
            {
                this.SimpleCheckForDuplicateAttributes();
            }
            else
            {
                this.HashCheckForDuplicateAttributes();
            }
        }

        private XmlNodeType ScanOverAnyValue(BinXmlToken token, bool attr, bool checkChars)
        {
            if (this.mark < 0)
            {
                this.mark = this.pos;
            }
            switch (token)
            {
                case BinXmlToken.SQL_SMALLINT:
                case BinXmlToken.XSD_UNSIGNEDSHORT:
                    this.tokDataPos = this.pos;
                    this.tokLen = 2;
                    this.pos += 2;
                    break;

                case BinXmlToken.SQL_INT:
                case BinXmlToken.SQL_REAL:
                case BinXmlToken.SQL_SMALLDATETIME:
                case BinXmlToken.SQL_SMALLMONEY:
                case BinXmlToken.XSD_UNSIGNEDINT:
                    this.tokDataPos = this.pos;
                    this.tokLen = 4;
                    this.pos += 4;
                    break;

                case BinXmlToken.SQL_FLOAT:
                case BinXmlToken.SQL_MONEY:
                case BinXmlToken.SQL_BIGINT:
                case BinXmlToken.SQL_DATETIME:
                case BinXmlToken.XSD_TIME:
                case BinXmlToken.XSD_DATETIME:
                case BinXmlToken.XSD_DATE:
                case BinXmlToken.XSD_UNSIGNEDLONG:
                    this.tokDataPos = this.pos;
                    this.tokLen = 8;
                    this.pos += 8;
                    break;

                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT:
                case BinXmlToken.XSD_BOOLEAN:
                case BinXmlToken.XSD_BYTE:
                    this.tokDataPos = this.pos;
                    this.tokLen = 1;
                    this.pos++;
                    break;

                case BinXmlToken.SQL_UUID:
                    this.tokDataPos = this.pos;
                    this.tokLen = 0x10;
                    this.pos += 0x10;
                    break;

                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.XSD_DECIMAL:
                    this.tokDataPos = this.pos;
                    this.tokLen = this.ParseMB64();
                    this.pos += this.tokLen;
                    break;

                case BinXmlToken.SQL_BINARY:
                case BinXmlToken.SQL_VARBINARY:
                case BinXmlToken.SQL_IMAGE:
                case BinXmlToken.SQL_UDT:
                case BinXmlToken.XSD_BINHEX:
                case BinXmlToken.XSD_BASE64:
                    this.tokLen = this.ParseMB64();
                    this.tokDataPos = this.pos;
                    this.pos += this.tokLen;
                    break;

                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_TEXT:
                    this.tokLen = this.ParseMB64();
                    this.tokDataPos = this.pos;
                    this.pos += this.tokLen;
                    if (checkChars && this.checkCharacters)
                    {
                        this.Fill(-1);
                        string data = this.ValueAsString(token);
                        XmlConvert.VerifyCharData(data, ExceptionType.ArgumentException, ExceptionType.XmlException);
                        this.stringValue = data;
                    }
                    break;

                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_NTEXT:
                    return this.ScanOverValue(BinXmlToken.SQL_NVARCHAR, attr, checkChars);

                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                case BinXmlToken.XSD_KATMAI_TIME:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                case BinXmlToken.XSD_KATMAI_DATE:
                    this.VerifyVersion(2, token);
                    this.tokDataPos = this.pos;
                    this.tokLen = this.GetXsdKatmaiTokenLength(token);
                    this.pos += this.tokLen;
                    break;

                case BinXmlToken.XSD_QNAME:
                    this.tokDataPos = this.pos;
                    this.ParseMB32();
                    break;

                default:
                    throw this.ThrowUnexpectedToken(token);
            }
            this.Fill(-1);
            return XmlNodeType.Text;
        }

        private XmlNodeType ScanOverValue(BinXmlToken token, bool attr, bool checkChars)
        {
            if (token != BinXmlToken.SQL_NVARCHAR)
            {
                return this.ScanOverAnyValue(token, attr, checkChars);
            }
            if (this.mark < 0)
            {
                this.mark = this.pos;
            }
            this.tokLen = this.ParseMB32();
            this.tokDataPos = this.pos;
            this.pos += this.tokLen * 2;
            this.Fill(-1);
            if (checkChars && this.checkCharacters)
            {
                return this.CheckText(attr);
            }
            if (!attr)
            {
                return this.CheckTextIsWS();
            }
            return XmlNodeType.Text;
        }

        private int ScanText(out int start)
        {
            int num = this.ParseMB32();
            int mark = this.mark;
            int pos = this.pos;
            this.pos += num * 2;
            if (this.pos > this.end)
            {
                this.Fill(-1);
            }
            start = pos - (mark - this.mark);
            return num;
        }

        private bool SetupContentAsXXX(string name)
        {
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException(name);
            }
            switch (this.state)
            {
                case ScanState.Doc:
                    if (this.NodeType != XmlNodeType.EndElement)
                    {
                        if ((this.NodeType == XmlNodeType.ProcessingInstruction) || (this.NodeType == XmlNodeType.Comment))
                        {
                            while (this.Read() && ((this.NodeType == XmlNodeType.ProcessingInstruction) || (this.NodeType == XmlNodeType.Comment)))
                            {
                            }
                            if (this.NodeType == XmlNodeType.EndElement)
                            {
                                return true;
                            }
                        }
                        if (this.hasTypedValue)
                        {
                            return true;
                        }
                        break;
                    }
                    return true;

                case ScanState.Attr:
                {
                    this.pos = this.attributes[this.attrIndex - 1].contentPos;
                    BinXmlToken token = this.RescanNextToken();
                    if ((BinXmlToken.Attr == token) || (BinXmlToken.EndAttrs == token))
                    {
                        break;
                    }
                    this.token = token;
                    this.ReScanOverValue(token);
                    return true;
                }
                case ScanState.AttrVal:
                    return true;
            }
            return false;
        }

        private void SimpleCheckForDuplicateAttributes()
        {
            for (int i = 0; i < this.attrCount; i++)
            {
                string str;
                string str2;
                this.attributes[i].GetLocalnameAndNamespaceUri(out str, out str2);
                for (int j = i + 1; j < this.attrCount; j++)
                {
                    if (this.attributes[j].MatchNS(str, str2))
                    {
                        throw new XmlException("Xml_DupAttributeName", this.attributes[i].name.ToString());
                    }
                }
            }
        }

        private void SkipExtn()
        {
            int num = this.ParseMB32();
            this.pos += num;
            this.Fill(-1);
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            if (ScanState.XmlText == this.state)
            {
                IXmlNamespaceResolver textXmlReader = (IXmlNamespaceResolver) this.textXmlReader;
                return textXmlReader.GetNamespacesInScope(scope);
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (XmlNamespaceScope.Local == scope)
            {
                if (this.elemDepth > 0)
                {
                    for (NamespaceDecl decl = this.elementStack[this.elemDepth].nsdecls; decl != null; decl = decl.scopeLink)
                    {
                        dictionary.Add(decl.prefix, decl.uri);
                    }
                }
                return dictionary;
            }
            foreach (NamespaceDecl decl2 in this.namespaces.Values)
            {
                if (((decl2.scope != -1) || ((scope == XmlNamespaceScope.All) && ("xml" == decl2.prefix))) && ((decl2.prefix.Length > 0) || (decl2.uri.Length > 0)))
                {
                    dictionary.Add(decl2.prefix, decl2.uri);
                }
            }
            return dictionary;
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            if (ScanState.XmlText == this.state)
            {
                IXmlNamespaceResolver textXmlReader = (IXmlNamespaceResolver) this.textXmlReader;
                return textXmlReader.LookupPrefix(namespaceName);
            }
            if (namespaceName != null)
            {
                namespaceName = this.xnt.Get(namespaceName);
                if (namespaceName == null)
                {
                    return null;
                }
                for (int i = this.elemDepth; i >= 0; i--)
                {
                    for (NamespaceDecl decl = this.elementStack[i].nsdecls; decl != null; decl = decl.scopeLink)
                    {
                        if (decl.uri == namespaceName)
                        {
                            return decl.prefix;
                        }
                    }
                }
            }
            return null;
        }

        private Exception ThrowNotSupported(string res)
        {
            this.state = ScanState.Error;
            return new NotSupportedException(Res.GetString(res));
        }

        private Exception ThrowUnexpectedToken(BinXmlToken token)
        {
            return this.ThrowXmlException("XmlBinary_UnexpectedToken");
        }

        private Exception ThrowXmlException(string res)
        {
            this.state = ScanState.Error;
            return new XmlException(res, null);
        }

        private Exception ThrowXmlException(string res, string arg1, string arg2)
        {
            this.state = ScanState.Error;
            return new XmlException(res, new string[] { arg1, arg2 });
        }

        private void UpdateFromTextReader()
        {
            XmlReader textXmlReader = this.textXmlReader;
            this.nodetype = textXmlReader.NodeType;
            this.qnameOther.prefix = textXmlReader.Prefix;
            this.qnameOther.localname = textXmlReader.LocalName;
            this.qnameOther.namespaceUri = textXmlReader.NamespaceURI;
            this.valueType = textXmlReader.ValueType;
            this.isEmpty = textXmlReader.IsEmptyElement;
        }

        private bool UpdateFromTextReader(bool needUpdate)
        {
            if (needUpdate)
            {
                this.UpdateFromTextReader();
            }
            return needUpdate;
        }

        private object ValueAs(BinXmlToken token, Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            this.CheckValueTokenBounds();
            switch (token)
            {
                case BinXmlToken.SQL_SMALLINT:
                {
                    int num = this.GetInt16(this.tokDataPos);
                    return this.GetValueConverter(XmlTypeCode.Short).ChangeType(num, returnType, namespaceResolver);
                }
                case BinXmlToken.SQL_INT:
                {
                    int num2 = this.GetInt32(this.tokDataPos);
                    return this.GetValueConverter(XmlTypeCode.Int).ChangeType(num2, returnType, namespaceResolver);
                }
                case BinXmlToken.SQL_REAL:
                {
                    float single = this.GetSingle(this.tokDataPos);
                    return this.GetValueConverter(XmlTypeCode.Float).ChangeType(single, returnType, namespaceResolver);
                }
                case BinXmlToken.SQL_FLOAT:
                {
                    double num8 = this.GetDouble(this.tokDataPos);
                    return this.GetValueConverter(XmlTypeCode.Double).ChangeType(num8, returnType, namespaceResolver);
                }
                case BinXmlToken.SQL_MONEY:
                {
                    BinXmlSqlMoney money2 = new BinXmlSqlMoney(this.GetInt64(this.tokDataPos));
                    return this.GetValueConverter(XmlTypeCode.Decimal).ChangeType(money2.ToDecimal(), returnType, namespaceResolver);
                }
                case BinXmlToken.SQL_BIT:
                    return this.GetValueConverter(XmlTypeCode.NonNegativeInteger).ChangeType((int) this.data[this.tokDataPos], returnType, namespaceResolver);

                case BinXmlToken.SQL_TINYINT:
                    return this.GetValueConverter(XmlTypeCode.UnsignedByte).ChangeType(this.data[this.tokDataPos], returnType, namespaceResolver);

                case BinXmlToken.SQL_BIGINT:
                {
                    long num3 = this.GetInt64(this.tokDataPos);
                    return this.GetValueConverter(XmlTypeCode.Long).ChangeType(num3, returnType, namespaceResolver);
                }
                case BinXmlToken.SQL_UUID:
                    return this.GetValueConverter(XmlTypeCode.String).ChangeType(this.ValueAsString(token), returnType, namespaceResolver);

                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.XSD_DECIMAL:
                {
                    BinXmlSqlDecimal num12 = new BinXmlSqlDecimal(this.data, this.tokDataPos, token == BinXmlToken.XSD_DECIMAL);
                    return this.GetValueConverter(XmlTypeCode.Decimal).ChangeType(num12.ToDecimal(), returnType, namespaceResolver);
                }
                case BinXmlToken.SQL_BINARY:
                case BinXmlToken.SQL_VARBINARY:
                case BinXmlToken.SQL_IMAGE:
                case BinXmlToken.SQL_UDT:
                case BinXmlToken.XSD_BINHEX:
                case BinXmlToken.XSD_BASE64:
                {
                    byte[] destinationArray = new byte[this.tokLen];
                    Array.Copy(this.data, this.tokDataPos, destinationArray, 0, this.tokLen);
                    return this.GetValueConverter((token == BinXmlToken.XSD_BINHEX) ? XmlTypeCode.HexBinary : XmlTypeCode.Base64Binary).ChangeType(destinationArray, returnType, namespaceResolver);
                }
                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_TEXT:
                {
                    int tokDataPos = this.tokDataPos;
                    Encoding encoding = Encoding.GetEncoding(this.GetInt32(tokDataPos));
                    return this.GetValueConverter(XmlTypeCode.UntypedAtomic).ChangeType(encoding.GetString(this.data, tokDataPos + 4, this.tokLen - 4), returnType, namespaceResolver);
                }
                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_NTEXT:
                    return this.GetValueConverter(XmlTypeCode.UntypedAtomic).ChangeType(this.GetString(this.tokDataPos, this.tokLen), returnType, namespaceResolver);

                case BinXmlToken.SQL_DATETIME:
                case BinXmlToken.SQL_SMALLDATETIME:
                case BinXmlToken.XSD_KATMAI_TIME:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                case BinXmlToken.XSD_KATMAI_DATE:
                case BinXmlToken.XSD_DATETIME:
                    return this.GetValueConverter(XmlTypeCode.DateTime).ChangeType(this.ValueAsDateTime(), returnType, namespaceResolver);

                case BinXmlToken.SQL_SMALLMONEY:
                {
                    BinXmlSqlMoney money = new BinXmlSqlMoney(this.GetInt32(this.tokDataPos));
                    return this.GetValueConverter(XmlTypeCode.Decimal).ChangeType(money.ToDecimal(), returnType, namespaceResolver);
                }
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    return this.GetValueConverter(XmlTypeCode.DateTime).ChangeType(this.ValueAsDateTimeOffset(), returnType, namespaceResolver);

                case BinXmlToken.XSD_TIME:
                    return this.GetValueConverter(XmlTypeCode.Time).ChangeType(this.ValueAsDateTime(), returnType, namespaceResolver);

                case BinXmlToken.XSD_DATE:
                    return this.GetValueConverter(XmlTypeCode.Date).ChangeType(this.ValueAsDateTime(), returnType, namespaceResolver);

                case BinXmlToken.XSD_BOOLEAN:
                    return this.GetValueConverter(XmlTypeCode.Boolean).ChangeType(0 != this.data[this.tokDataPos], returnType, namespaceResolver);

                case BinXmlToken.XSD_BYTE:
                    return this.GetValueConverter(XmlTypeCode.Byte).ChangeType((int) ((sbyte) this.data[this.tokDataPos]), returnType, namespaceResolver);

                case BinXmlToken.XSD_UNSIGNEDSHORT:
                {
                    int num4 = this.GetUInt16(this.tokDataPos);
                    return this.GetValueConverter(XmlTypeCode.UnsignedShort).ChangeType(num4, returnType, namespaceResolver);
                }
                case BinXmlToken.XSD_UNSIGNEDINT:
                {
                    long num5 = this.GetUInt32(this.tokDataPos);
                    return this.GetValueConverter(XmlTypeCode.UnsignedInt).ChangeType(num5, returnType, namespaceResolver);
                }
                case BinXmlToken.XSD_UNSIGNEDLONG:
                {
                    decimal num6 = this.GetUInt64(this.tokDataPos);
                    return this.GetValueConverter(XmlTypeCode.UnsignedLong).ChangeType(num6, returnType, namespaceResolver);
                }
                case BinXmlToken.XSD_QNAME:
                {
                    int index = this.ParseMB32(this.tokDataPos);
                    if ((index < 0) || (index >= this.symbolTables.qnameCount))
                    {
                        throw new XmlException("XmlBin_InvalidQNameID", string.Empty);
                    }
                    QName name = this.symbolTables.qnametable[index];
                    return this.GetValueConverter(XmlTypeCode.QName).ChangeType(new XmlQualifiedName(name.localname, name.namespaceUri), returnType, namespaceResolver);
                }
            }
            throw this.ThrowUnexpectedToken(this.token);
        }

        private DateTime ValueAsDateTime()
        {
            this.CheckValueTokenBounds();
            switch (this.token)
            {
                case BinXmlToken.SQL_DATETIME:
                {
                    int tokDataPos = this.tokDataPos;
                    int dateticks = this.GetInt32(tokDataPos);
                    uint timeticks = this.GetUInt32(tokDataPos + 4);
                    return BinXmlDateTime.SqlDateTimeToDateTime(dateticks, timeticks);
                }
                case BinXmlToken.SQL_SMALLDATETIME:
                {
                    int pos = this.tokDataPos;
                    short num5 = this.GetInt16(pos);
                    ushort num6 = this.GetUInt16(pos + 2);
                    return BinXmlDateTime.SqlSmallDateTimeToDateTime(num5, num6);
                }
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiTimeOffsetToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateTimeOffsetToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateOffsetToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_TIME:
                    return BinXmlDateTime.XsdKatmaiTimeToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATETIME:
                    return BinXmlDateTime.XsdKatmaiDateTimeToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATE:
                    return BinXmlDateTime.XsdKatmaiDateToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_TIME:
                    return BinXmlDateTime.XsdTimeToDateTime(this.GetInt64(this.tokDataPos));

                case BinXmlToken.XSD_DATETIME:
                    return BinXmlDateTime.XsdDateTimeToDateTime(this.GetInt64(this.tokDataPos));

                case BinXmlToken.XSD_DATE:
                    return BinXmlDateTime.XsdDateToDateTime(this.GetInt64(this.tokDataPos));
            }
            throw this.ThrowUnexpectedToken(this.token);
        }

        private DateTimeOffset ValueAsDateTimeOffset()
        {
            this.CheckValueTokenBounds();
            switch (this.token)
            {
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiTimeOffsetToDateTimeOffset(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateTimeOffsetToDateTimeOffset(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateOffsetToDateTimeOffset(this.data, this.tokDataPos);
            }
            throw this.ThrowUnexpectedToken(this.token);
        }

        private string ValueAsDateTimeString()
        {
            this.CheckValueTokenBounds();
            switch (this.token)
            {
                case BinXmlToken.SQL_DATETIME:
                {
                    int tokDataPos = this.tokDataPos;
                    int dateticks = this.GetInt32(tokDataPos);
                    uint timeticks = this.GetUInt32(tokDataPos + 4);
                    return BinXmlDateTime.SqlDateTimeToString(dateticks, timeticks);
                }
                case BinXmlToken.SQL_SMALLDATETIME:
                {
                    int pos = this.tokDataPos;
                    short num5 = this.GetInt16(pos);
                    ushort num6 = this.GetUInt16(pos + 2);
                    return BinXmlDateTime.SqlSmallDateTimeToString(num5, num6);
                }
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiTimeOffsetToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateTimeOffsetToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateOffsetToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_TIME:
                    return BinXmlDateTime.XsdKatmaiTimeToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATETIME:
                    return BinXmlDateTime.XsdKatmaiDateTimeToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATE:
                    return BinXmlDateTime.XsdKatmaiDateToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_TIME:
                    return BinXmlDateTime.XsdTimeToString(this.GetInt64(this.tokDataPos));

                case BinXmlToken.XSD_DATETIME:
                    return BinXmlDateTime.XsdDateTimeToString(this.GetInt64(this.tokDataPos));

                case BinXmlToken.XSD_DATE:
                    return BinXmlDateTime.XsdDateToString(this.GetInt64(this.tokDataPos));
            }
            throw this.ThrowUnexpectedToken(this.token);
        }

        private decimal ValueAsDecimal()
        {
            this.CheckValueTokenBounds();
            switch (this.token)
            {
                case BinXmlToken.SQL_SMALLINT:
                case BinXmlToken.SQL_INT:
                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT:
                case BinXmlToken.SQL_BIGINT:
                case BinXmlToken.XSD_BYTE:
                case BinXmlToken.XSD_UNSIGNEDSHORT:
                case BinXmlToken.XSD_UNSIGNEDINT:
                    return new decimal(this.ValueAsLong());

                case BinXmlToken.SQL_REAL:
                    return new decimal(this.GetSingle(this.tokDataPos));

                case BinXmlToken.SQL_FLOAT:
                    return new decimal(this.GetDouble(this.tokDataPos));

                case BinXmlToken.SQL_MONEY:
                {
                    BinXmlSqlMoney money2 = new BinXmlSqlMoney(this.GetInt64(this.tokDataPos));
                    return money2.ToDecimal();
                }
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.XSD_DECIMAL:
                {
                    BinXmlSqlDecimal num = new BinXmlSqlDecimal(this.data, this.tokDataPos, this.token == BinXmlToken.XSD_DECIMAL);
                    return num.ToDecimal();
                }
                case BinXmlToken.SQL_SMALLMONEY:
                {
                    BinXmlSqlMoney money = new BinXmlSqlMoney(this.GetInt32(this.tokDataPos));
                    return money.ToDecimal();
                }
                case BinXmlToken.XSD_UNSIGNEDLONG:
                    return new decimal(this.ValueAsULong());
            }
            throw this.ThrowUnexpectedToken(this.token);
        }

        private double ValueAsDouble()
        {
            this.CheckValueTokenBounds();
            switch (this.token)
            {
                case BinXmlToken.SQL_SMALLINT:
                case BinXmlToken.SQL_INT:
                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT:
                case BinXmlToken.SQL_BIGINT:
                case BinXmlToken.XSD_BYTE:
                case BinXmlToken.XSD_UNSIGNEDSHORT:
                case BinXmlToken.XSD_UNSIGNEDINT:
                    return (double) this.ValueAsLong();

                case BinXmlToken.SQL_REAL:
                    return (double) this.GetSingle(this.tokDataPos);

                case BinXmlToken.SQL_FLOAT:
                    return this.GetDouble(this.tokDataPos);

                case BinXmlToken.SQL_MONEY:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.SQL_SMALLMONEY:
                case BinXmlToken.XSD_DECIMAL:
                    return (double) this.ValueAsDecimal();

                case BinXmlToken.XSD_UNSIGNEDLONG:
                    return (double) this.ValueAsULong();
            }
            throw this.ThrowUnexpectedToken(this.token);
        }

        private long ValueAsLong()
        {
            this.CheckValueTokenBounds();
            switch (this.token)
            {
                case BinXmlToken.SQL_SMALLINT:
                    return (long) this.GetInt16(this.tokDataPos);

                case BinXmlToken.SQL_INT:
                    return (long) this.GetInt32(this.tokDataPos);

                case BinXmlToken.SQL_REAL:
                case BinXmlToken.SQL_FLOAT:
                    return (long) this.ValueAsDouble();

                case BinXmlToken.SQL_MONEY:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.SQL_SMALLMONEY:
                case BinXmlToken.XSD_DECIMAL:
                    return (long) this.ValueAsDecimal();

                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT:
                {
                    byte num = this.data[this.tokDataPos];
                    return (long) num;
                }
                case BinXmlToken.SQL_BIGINT:
                    return this.GetInt64(this.tokDataPos);

                case BinXmlToken.XSD_BYTE:
                {
                    sbyte num2 = (sbyte) this.data[this.tokDataPos];
                    return (long) num2;
                }
                case BinXmlToken.XSD_UNSIGNEDSHORT:
                    return (long) this.GetUInt16(this.tokDataPos);

                case BinXmlToken.XSD_UNSIGNEDINT:
                    return (long) this.GetUInt32(this.tokDataPos);

                case BinXmlToken.XSD_UNSIGNEDLONG:
                    return (long) this.GetUInt64(this.tokDataPos);
            }
            throw this.ThrowUnexpectedToken(this.token);
        }

        private object ValueAsObject(BinXmlToken token, bool returnInternalTypes)
        {
            this.CheckValueTokenBounds();
            switch (token)
            {
                case BinXmlToken.SQL_SMALLINT:
                    return this.GetInt16(this.tokDataPos);

                case BinXmlToken.SQL_INT:
                    return this.GetInt32(this.tokDataPos);

                case BinXmlToken.SQL_REAL:
                    return this.GetSingle(this.tokDataPos);

                case BinXmlToken.SQL_FLOAT:
                    return this.GetDouble(this.tokDataPos);

                case BinXmlToken.SQL_MONEY:
                {
                    BinXmlSqlMoney money2 = new BinXmlSqlMoney(this.GetInt64(this.tokDataPos));
                    if (!returnInternalTypes)
                    {
                        return money2.ToDecimal();
                    }
                    return money2;
                }
                case BinXmlToken.SQL_BIT:
                    return (int) this.data[this.tokDataPos];

                case BinXmlToken.SQL_TINYINT:
                    return this.data[this.tokDataPos];

                case BinXmlToken.SQL_BIGINT:
                    return this.GetInt64(this.tokDataPos);

                case BinXmlToken.SQL_UUID:
                {
                    int tokDataPos = this.tokDataPos;
                    int a = this.GetInt32(tokDataPos);
                    short b = this.GetInt16(tokDataPos + 4);
                    short c = this.GetInt16(tokDataPos + 6);
                    Guid guid = new Guid(a, b, c, this.data[tokDataPos + 8], this.data[tokDataPos + 9], this.data[tokDataPos + 10], this.data[tokDataPos + 11], this.data[tokDataPos + 12], this.data[tokDataPos + 13], this.data[tokDataPos + 14], this.data[tokDataPos + 15]);
                    return guid.ToString();
                }
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.XSD_DECIMAL:
                {
                    BinXmlSqlDecimal num6 = new BinXmlSqlDecimal(this.data, this.tokDataPos, token == BinXmlToken.XSD_DECIMAL);
                    if (returnInternalTypes)
                    {
                        return num6;
                    }
                    return num6.ToDecimal();
                }
                case BinXmlToken.SQL_BINARY:
                case BinXmlToken.SQL_VARBINARY:
                case BinXmlToken.SQL_IMAGE:
                case BinXmlToken.SQL_UDT:
                case BinXmlToken.XSD_BINHEX:
                case BinXmlToken.XSD_BASE64:
                {
                    byte[] destinationArray = new byte[this.tokLen];
                    Array.Copy(this.data, this.tokDataPos, destinationArray, 0, this.tokLen);
                    return destinationArray;
                }
                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_TEXT:
                {
                    int pos = this.tokDataPos;
                    return Encoding.GetEncoding(this.GetInt32(pos)).GetString(this.data, pos + 4, this.tokLen - 4);
                }
                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_NTEXT:
                    return this.GetString(this.tokDataPos, this.tokLen);

                case BinXmlToken.SQL_DATETIME:
                case BinXmlToken.SQL_SMALLDATETIME:
                case BinXmlToken.XSD_KATMAI_TIME:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                case BinXmlToken.XSD_KATMAI_DATE:
                case BinXmlToken.XSD_TIME:
                case BinXmlToken.XSD_DATETIME:
                case BinXmlToken.XSD_DATE:
                    return this.ValueAsDateTime();

                case BinXmlToken.SQL_SMALLMONEY:
                {
                    BinXmlSqlMoney money = new BinXmlSqlMoney(this.GetInt32(this.tokDataPos));
                    if (!returnInternalTypes)
                    {
                        return money.ToDecimal();
                    }
                    return money;
                }
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    return this.ValueAsDateTimeOffset();

                case BinXmlToken.XSD_BOOLEAN:
                    return (0 != this.data[this.tokDataPos]);

                case BinXmlToken.XSD_BYTE:
                    return (sbyte) this.data[this.tokDataPos];

                case BinXmlToken.XSD_UNSIGNEDSHORT:
                    return this.GetUInt16(this.tokDataPos);

                case BinXmlToken.XSD_UNSIGNEDINT:
                    return this.GetUInt32(this.tokDataPos);

                case BinXmlToken.XSD_UNSIGNEDLONG:
                    return this.GetUInt64(this.tokDataPos);

                case BinXmlToken.XSD_QNAME:
                {
                    int index = this.ParseMB32(this.tokDataPos);
                    if ((index < 0) || (index >= this.symbolTables.qnameCount))
                    {
                        throw new XmlException("XmlBin_InvalidQNameID", string.Empty);
                    }
                    QName name = this.symbolTables.qnametable[index];
                    return new XmlQualifiedName(name.localname, name.namespaceUri);
                }
            }
            throw this.ThrowUnexpectedToken(this.token);
        }

        private string ValueAsString(BinXmlToken token)
        {
            string str;
            try
            {
                int num8;
                this.CheckValueTokenBounds();
                switch (token)
                {
                    case BinXmlToken.SQL_SMALLINT:
                    case BinXmlToken.SQL_INT:
                    case BinXmlToken.SQL_BIT:
                    case BinXmlToken.SQL_TINYINT:
                    case BinXmlToken.SQL_BIGINT:
                    case BinXmlToken.XSD_BYTE:
                    case BinXmlToken.XSD_UNSIGNEDSHORT:
                    case BinXmlToken.XSD_UNSIGNEDINT:
                        return this.ValueAsLong().ToString(CultureInfo.InvariantCulture);

                    case BinXmlToken.SQL_REAL:
                        return XmlConvert.ToString(this.GetSingle(this.tokDataPos));

                    case BinXmlToken.SQL_FLOAT:
                        return XmlConvert.ToString(this.GetDouble(this.tokDataPos));

                    case BinXmlToken.SQL_MONEY:
                    {
                        BinXmlSqlMoney money2 = new BinXmlSqlMoney(this.GetInt64(this.tokDataPos));
                        return money2.ToString();
                    }
                    case BinXmlToken.SQL_UUID:
                    {
                        int tokDataPos = this.tokDataPos;
                        int a = this.GetInt32(tokDataPos);
                        short b = this.GetInt16(tokDataPos + 4);
                        short c = this.GetInt16(tokDataPos + 6);
                        Guid guid = new Guid(a, b, c, this.data[tokDataPos + 8], this.data[tokDataPos + 9], this.data[tokDataPos + 10], this.data[tokDataPos + 11], this.data[tokDataPos + 12], this.data[tokDataPos + 13], this.data[tokDataPos + 14], this.data[tokDataPos + 15]);
                        return guid.ToString();
                    }
                    case BinXmlToken.SQL_DECIMAL:
                    case BinXmlToken.SQL_NUMERIC:
                    case BinXmlToken.XSD_DECIMAL:
                    {
                        BinXmlSqlDecimal num5 = new BinXmlSqlDecimal(this.data, this.tokDataPos, token == BinXmlToken.XSD_DECIMAL);
                        return num5.ToString();
                    }
                    case BinXmlToken.SQL_BINARY:
                    case BinXmlToken.SQL_VARBINARY:
                    case BinXmlToken.SQL_IMAGE:
                    case BinXmlToken.SQL_UDT:
                    case BinXmlToken.XSD_BASE64:
                        return Convert.ToBase64String(this.data, this.tokDataPos, this.tokLen);

                    case BinXmlToken.SQL_CHAR:
                    case BinXmlToken.SQL_VARCHAR:
                    case BinXmlToken.SQL_TEXT:
                    {
                        int pos = this.tokDataPos;
                        return Encoding.GetEncoding(this.GetInt32(pos)).GetString(this.data, pos + 4, this.tokLen - 4);
                    }
                    case BinXmlToken.SQL_NCHAR:
                    case BinXmlToken.SQL_NVARCHAR:
                    case BinXmlToken.SQL_NTEXT:
                        return this.GetString(this.tokDataPos, this.tokLen);

                    case BinXmlToken.SQL_DATETIME:
                    case BinXmlToken.SQL_SMALLDATETIME:
                    case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                    case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                    case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    case BinXmlToken.XSD_KATMAI_TIME:
                    case BinXmlToken.XSD_KATMAI_DATETIME:
                    case BinXmlToken.XSD_KATMAI_DATE:
                    case BinXmlToken.XSD_TIME:
                    case BinXmlToken.XSD_DATETIME:
                    case BinXmlToken.XSD_DATE:
                        return this.ValueAsDateTimeString();

                    case BinXmlToken.SQL_SMALLMONEY:
                    {
                        BinXmlSqlMoney money = new BinXmlSqlMoney(this.GetInt32(this.tokDataPos));
                        return money.ToString();
                    }
                    case BinXmlToken.XSD_BINHEX:
                        return BinHexEncoder.Encode(this.data, this.tokDataPos, this.tokLen);

                    case BinXmlToken.XSD_BOOLEAN:
                        if (this.data[this.tokDataPos] != 0)
                        {
                            return "true";
                        }
                        return "false";

                    case BinXmlToken.XSD_UNSIGNEDLONG:
                        return this.ValueAsULong().ToString(CultureInfo.InvariantCulture);

                    case BinXmlToken.XSD_QNAME:
                        num8 = this.ParseMB32(this.tokDataPos);
                        if ((num8 < 0) || (num8 >= this.symbolTables.qnameCount))
                        {
                            throw new XmlException("XmlBin_InvalidQNameID", string.Empty);
                        }
                        break;

                    default:
                        throw this.ThrowUnexpectedToken(this.token);
                }
                QName name = this.symbolTables.qnametable[num8];
                if (name.prefix.Length == 0)
                {
                    return name.localname;
                }
                return (name.prefix + ":" + name.localname);
            }
            catch
            {
                this.state = ScanState.Error;
                throw;
            }
            return str;
        }

        private ulong ValueAsULong()
        {
            if (BinXmlToken.XSD_UNSIGNEDLONG != this.token)
            {
                throw this.ThrowUnexpectedToken(this.token);
            }
            this.CheckValueTokenBounds();
            return this.GetUInt64(this.tokDataPos);
        }

        private void VerifyVersion(int requiredVersion, BinXmlToken token)
        {
            if (this.version < requiredVersion)
            {
                throw this.ThrowUnexpectedToken(token);
            }
        }

        private string XmlDeclValue()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.attrCount; i++)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }
                builder.Append(this.attributes[i].name.localname);
                builder.Append("=\"");
                builder.Append(this.attributes[i].val);
                builder.Append('"');
            }
            return builder.ToString();
        }

        private int XsdKatmaiTimeScaleToValueLength(byte scale)
        {
            if (scale > 7)
            {
                throw new XmlException("SqlTypes_ArithOverflow", null);
            }
            return XsdKatmaiTimeScaleToValueLengthMap[scale];
        }

        public override int AttributeCount
        {
            get
            {
                switch (this.state)
                {
                    case ScanState.Doc:
                    case ScanState.Attr:
                    case ScanState.AttrVal:
                    case ScanState.AttrValPseudoValue:
                        return this.attrCount;

                    case ScanState.XmlText:
                        return this.textXmlReader.AttributeCount;
                }
                return 0;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.baseUri;
            }
        }

        public override int Depth
        {
            get
            {
                int depth = 0;
                switch (this.state)
                {
                    case ScanState.Doc:
                        if ((this.nodetype == XmlNodeType.Element) || (this.nodetype == XmlNodeType.EndElement))
                        {
                            depth = -1;
                        }
                        break;

                    case ScanState.XmlText:
                        depth = this.textXmlReader.Depth;
                        break;

                    case ScanState.Attr:
                        if (this.parentNodeType != XmlNodeType.Element)
                        {
                            depth = 1;
                        }
                        break;

                    case ScanState.AttrVal:
                    case ScanState.AttrValPseudoValue:
                        if (this.parentNodeType != XmlNodeType.Element)
                        {
                            depth = 1;
                        }
                        depth++;
                        break;

                    default:
                        return 0;
                }
                return (this.elemDepth + depth);
            }
        }

        public override bool EOF
        {
            get
            {
                return (this.state == ScanState.EOF);
            }
        }

        public override bool HasValue
        {
            get
            {
                if (ScanState.XmlText == this.state)
                {
                    return this.textXmlReader.HasValue;
                }
                return XmlReader.HasValueInternal(this.nodetype);
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                switch (this.state)
                {
                    case ScanState.Doc:
                    case ScanState.XmlText:
                        return this.isEmpty;
                }
                return false;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.qnameOther.localname;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.qnameOther.namespaceUri;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.xnt;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.nodetype;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.qnameOther.prefix;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return ScanState2ReadState[(int) this.state];
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                if (this.xntFromSettings)
                {
                    settings.NameTable = this.xnt;
                }
                switch (this.docState)
                {
                    case 0:
                        settings.ConformanceLevel = ConformanceLevel.Auto;
                        break;

                    case 9:
                        settings.ConformanceLevel = ConformanceLevel.Fragment;
                        break;

                    default:
                        settings.ConformanceLevel = ConformanceLevel.Document;
                        break;
                }
                settings.CheckCharacters = this.checkCharacters;
                settings.IgnoreWhitespace = this.ignoreWhitespace;
                settings.IgnoreProcessingInstructions = this.ignorePIs;
                settings.IgnoreComments = this.ignoreComments;
                settings.DtdProcessing = this.dtdProcessing;
                settings.CloseInput = this.closeInput;
                settings.ReadOnly = true;
                return settings;
            }
        }

        public override string Value
        {
            get
            {
                if (this.stringValue != null)
                {
                    return this.stringValue;
                }
                switch (this.state)
                {
                    case ScanState.Doc:
                        switch (this.nodetype)
                        {
                            case XmlNodeType.Text:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                return (this.stringValue = this.ValueAsString(this.token));

                            case XmlNodeType.CDATA:
                                return (this.stringValue = this.CDATAValue());

                            case XmlNodeType.ProcessingInstruction:
                            case XmlNodeType.Comment:
                            case XmlNodeType.DocumentType:
                                return (this.stringValue = this.GetString(this.tokDataPos, this.tokLen));

                            case XmlNodeType.XmlDeclaration:
                                return (this.stringValue = this.XmlDeclValue());
                        }
                        break;

                    case ScanState.XmlText:
                        return this.textXmlReader.Value;

                    case ScanState.Attr:
                    case ScanState.AttrValPseudoValue:
                        return (this.stringValue = this.GetAttributeText(this.attrIndex - 1));

                    case ScanState.AttrVal:
                        return (this.stringValue = this.ValueAsString(this.token));
                }
                return string.Empty;
            }
        }

        public override Type ValueType
        {
            get
            {
                return this.valueType;
            }
        }

        public override string XmlLang
        {
            get
            {
                if (ScanState.XmlText == this.state)
                {
                    return this.textXmlReader.XmlLang;
                }
                for (int i = this.elemDepth; i >= 0; i--)
                {
                    string xmlLang = this.elementStack[i].xmlLang;
                    if (xmlLang != null)
                    {
                        return xmlLang;
                    }
                }
                return string.Empty;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                if (ScanState.XmlText == this.state)
                {
                    return this.textXmlReader.XmlSpace;
                }
                for (int i = this.elemDepth; i >= 0; i--)
                {
                    System.Xml.XmlSpace xmlSpace = this.elementStack[i].xmlSpace;
                    if (xmlSpace != System.Xml.XmlSpace.None)
                    {
                        return xmlSpace;
                    }
                }
                return System.Xml.XmlSpace.None;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AttrInfo
        {
            public XmlSqlBinaryReader.QName name;
            public string val;
            public int contentPos;
            public int hashCode;
            public int prevHash;
            public void Set(XmlSqlBinaryReader.QName n, string v)
            {
                this.name = n;
                this.val = v;
                this.contentPos = 0;
                this.hashCode = 0;
                this.prevHash = 0;
            }

            public void Set(XmlSqlBinaryReader.QName n, int pos)
            {
                this.name = n;
                this.val = null;
                this.contentPos = pos;
                this.hashCode = 0;
                this.prevHash = 0;
            }

            public void GetLocalnameAndNamespaceUri(out string localname, out string namespaceUri)
            {
                localname = this.name.localname;
                namespaceUri = this.name.namespaceUri;
            }

            public int GetLocalnameAndNamespaceUriAndHash(SecureStringHasher hasher, out string localname, out string namespaceUri)
            {
                localname = this.name.localname;
                namespaceUri = this.name.namespaceUri;
                return (this.hashCode = this.name.GetNSHashCode(hasher));
            }

            public bool MatchNS(string localname, string namespaceUri)
            {
                return this.name.MatchNs(localname, namespaceUri);
            }

            public bool MatchHashNS(int hash, string localname, string namespaceUri)
            {
                return ((this.hashCode == hash) && this.name.MatchNs(localname, namespaceUri));
            }

            public void AdjustPosition(int adj)
            {
                if (this.contentPos != 0)
                {
                    this.contentPos += adj;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ElemInfo
        {
            public XmlSqlBinaryReader.QName name;
            public string xmlLang;
            public XmlSpace xmlSpace;
            public bool xmlspacePreserve;
            public XmlSqlBinaryReader.NamespaceDecl nsdecls;
            public void Set(XmlSqlBinaryReader.QName name, bool xmlspacePreserve)
            {
                this.name = name;
                this.xmlLang = null;
                this.xmlSpace = XmlSpace.None;
                this.xmlspacePreserve = xmlspacePreserve;
            }

            public XmlSqlBinaryReader.NamespaceDecl Clear()
            {
                XmlSqlBinaryReader.NamespaceDecl nsdecls = this.nsdecls;
                this.nsdecls = null;
                return nsdecls;
            }
        }

        private class NamespaceDecl
        {
            public bool implied;
            public string prefix;
            public XmlSqlBinaryReader.NamespaceDecl prevLink;
            public int scope;
            public XmlSqlBinaryReader.NamespaceDecl scopeLink;
            public string uri;

            public NamespaceDecl(string prefix, string nsuri, XmlSqlBinaryReader.NamespaceDecl nextInScope, XmlSqlBinaryReader.NamespaceDecl prevDecl, int scope, bool implied)
            {
                this.prefix = prefix;
                this.uri = nsuri;
                this.scopeLink = nextInScope;
                this.prevLink = prevDecl;
                this.scope = scope;
                this.implied = implied;
            }
        }

        private class NestedBinXml
        {
            public int docState;
            public XmlSqlBinaryReader.NestedBinXml next;
            public XmlSqlBinaryReader.SymbolTables symbolTables;

            public NestedBinXml(XmlSqlBinaryReader.SymbolTables symbolTables, int docState, XmlSqlBinaryReader.NestedBinXml next)
            {
                this.symbolTables = symbolTables;
                this.docState = docState;
                this.next = next;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct QName
        {
            public string prefix;
            public string localname;
            public string namespaceUri;
            public QName(string prefix, string lname, string nsUri)
            {
                this.prefix = prefix;
                this.localname = lname;
                this.namespaceUri = nsUri;
            }

            public void Set(string prefix, string lname, string nsUri)
            {
                this.prefix = prefix;
                this.localname = lname;
                this.namespaceUri = nsUri;
            }

            public void Clear()
            {
                this.prefix = this.localname = this.namespaceUri = string.Empty;
            }

            public bool MatchNs(string lname, string nsUri)
            {
                return ((lname == this.localname) && (nsUri == this.namespaceUri));
            }

            public bool MatchPrefix(string prefix, string lname)
            {
                return ((lname == this.localname) && (prefix == this.prefix));
            }

            public void CheckPrefixNS(string prefix, string namespaceUri)
            {
                if ((this.prefix == prefix) && (this.namespaceUri != namespaceUri))
                {
                    throw new XmlException("XmlBinary_NoRemapPrefix", new string[] { prefix, this.namespaceUri, namespaceUri });
                }
            }

            public override int GetHashCode()
            {
                return (this.prefix.GetHashCode() ^ this.localname.GetHashCode());
            }

            public int GetNSHashCode(SecureStringHasher hasher)
            {
                return (hasher.GetHashCode(this.namespaceUri) ^ hasher.GetHashCode(this.localname));
            }

            public override bool Equals(object other)
            {
                if (other is XmlSqlBinaryReader.QName)
                {
                    XmlSqlBinaryReader.QName name = (XmlSqlBinaryReader.QName) other;
                    return (this == name);
                }
                return false;
            }

            public override string ToString()
            {
                if (this.prefix.Length == 0)
                {
                    return this.localname;
                }
                return (this.prefix + ":" + this.localname);
            }

            public static bool operator ==(XmlSqlBinaryReader.QName a, XmlSqlBinaryReader.QName b)
            {
                return (((a.prefix == b.prefix) && (a.localname == b.localname)) && (a.namespaceUri == b.namespaceUri));
            }

            public static bool operator !=(XmlSqlBinaryReader.QName a, XmlSqlBinaryReader.QName b)
            {
                return !(a == b);
            }
        }

        private enum ScanState
        {
            Doc,
            XmlText,
            Attr,
            AttrVal,
            AttrValPseudoValue,
            Init,
            Error,
            EOF,
            Closed
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SymbolTables
        {
            public string[] symtable;
            public int symCount;
            public XmlSqlBinaryReader.QName[] qnametable;
            public int qnameCount;
            public void Init()
            {
                this.symtable = new string[0x40];
                this.qnametable = new XmlSqlBinaryReader.QName[0x10];
                this.symtable[0] = string.Empty;
                this.symCount = 1;
                this.qnameCount = 1;
            }
        }
    }
}

