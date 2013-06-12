namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public class XmlTextWriter : XmlWriter
    {
        private XmlTextWriterBase64Encoder base64Encoder;
        private char curQuoteChar;
        private State currentState;
        private Encoding encoding;
        private bool flush;
        private System.Xml.Formatting formatting;
        private int indentation;
        private char indentChar;
        private bool indented;
        private Token lastToken;
        private const int MaxNamespacesWalkCount = 0x10;
        private bool namespaces;
        private const int NamespaceStackInitialSize = 8;
        private Dictionary<string, int> nsHashtable;
        private Namespace[] nsStack;
        private int nsTop;
        private string prefixForXmlNs;
        private char quoteChar;
        private SpecialAttr specialAttr;
        private TagInfo[] stack;
        private static string[] stateName = new string[] { "Start", "Prolog", "PostDTD", "Element", "Attribute", "Content", "AttrOnly", "Epilog", "Error", "Closed" };
        private State[] stateTable;
        private static readonly State[] stateTableDefault = new State[] { 
            State.Prolog, State.Prolog, State.PostDTD, State.Content, State.Content, State.Content, State.Error, State.Epilog, State.PostDTD, State.PostDTD, State.Error, State.Error, State.Error, State.Error, State.Error, State.Error, 
            State.Prolog, State.Prolog, State.PostDTD, State.Content, State.Content, State.Content, State.Error, State.Epilog, State.Content, State.Content, State.Error, State.Content, State.Content, State.Content, State.Error, State.Epilog, 
            State.Element, State.Element, State.Element, State.Element, State.Element, State.Element, State.Error, State.Element, State.Error, State.Error, State.Error, State.Content, State.Content, State.Content, State.Error, State.Error, 
            State.Error, State.Error, State.Error, State.Content, State.Content, State.Content, State.Error, State.Error, State.AttrOnly, State.Error, State.Error, State.Attribute, State.Attribute, State.Error, State.Error, State.Error, 
            State.Error, State.Error, State.Error, State.Error, State.Element, State.Error, State.Epilog, State.Error, State.Content, State.Content, State.Error, State.Content, State.Attribute, State.Content, State.Attribute, State.Epilog, 
            State.Content, State.Content, State.Error, State.Content, State.Attribute, State.Content, State.Attribute, State.Epilog, State.Prolog, State.Prolog, State.PostDTD, State.Content, State.Attribute, State.Content, State.Attribute, State.Epilog, 
            State.Prolog, State.Prolog, State.PostDTD, State.Content, State.Attribute, State.Content, State.Attribute, State.Epilog
         };
        private static readonly State[] stateTableDocument = new State[] { 
            State.Error, State.Prolog, State.PostDTD, State.Content, State.Content, State.Content, State.Error, State.Epilog, State.Error, State.PostDTD, State.Error, State.Error, State.Error, State.Error, State.Error, State.Error, 
            State.Error, State.Prolog, State.PostDTD, State.Content, State.Content, State.Content, State.Error, State.Epilog, State.Error, State.Error, State.Error, State.Content, State.Content, State.Content, State.Error, State.Error, 
            State.Error, State.Element, State.Element, State.Element, State.Element, State.Element, State.Error, State.Error, State.Error, State.Error, State.Error, State.Content, State.Content, State.Content, State.Error, State.Error, 
            State.Error, State.Error, State.Error, State.Content, State.Content, State.Content, State.Error, State.Error, State.Error, State.Error, State.Error, State.Attribute, State.Attribute, State.Error, State.Error, State.Error, 
            State.Error, State.Error, State.Error, State.Error, State.Element, State.Error, State.Error, State.Error, State.Error, State.Error, State.Error, State.Content, State.Attribute, State.Content, State.Error, State.Error, 
            State.Error, State.Error, State.Error, State.Content, State.Attribute, State.Content, State.Error, State.Error, State.Error, State.Prolog, State.PostDTD, State.Content, State.Attribute, State.Content, State.Error, State.Epilog, 
            State.Error, State.Prolog, State.PostDTD, State.Content, State.Attribute, State.Content, State.Error, State.Epilog
         };
        private TextWriter textWriter;
        private static string[] tokenName = new string[] { "PI", "Doctype", "Comment", "CData", "StartElement", "EndElement", "LongEndElement", "StartAttribute", "EndAttribute", "Content", "Base64", "RawData", "Whitespace", "Empty" };
        private int top;
        private bool useNsHashtable;
        private XmlCharType xmlCharType;
        private XmlTextEncoder xmlEncoder;

        internal XmlTextWriter()
        {
            this.xmlCharType = XmlCharType.Instance;
            this.namespaces = true;
            this.formatting = System.Xml.Formatting.None;
            this.indentation = 2;
            this.indentChar = ' ';
            this.nsStack = new Namespace[8];
            this.nsTop = -1;
            this.stack = new TagInfo[10];
            this.top = 0;
            this.stack[this.top].Init(-1);
            this.quoteChar = '"';
            this.stateTable = stateTableDefault;
            this.currentState = State.Start;
            this.lastToken = Token.Empty;
        }

        public XmlTextWriter(TextWriter w) : this()
        {
            this.textWriter = w;
            this.encoding = w.Encoding;
            this.xmlEncoder = new XmlTextEncoder(w);
            this.xmlEncoder.QuoteChar = this.quoteChar;
        }

        public XmlTextWriter(Stream w, Encoding encoding) : this()
        {
            this.encoding = encoding;
            if (encoding != null)
            {
                this.textWriter = new StreamWriter(w, encoding);
            }
            else
            {
                this.textWriter = new StreamWriter(w);
            }
            this.xmlEncoder = new XmlTextEncoder(this.textWriter);
            this.xmlEncoder.QuoteChar = this.quoteChar;
        }

        public XmlTextWriter(string filename, Encoding encoding) : this(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read), encoding)
        {
        }

        private void AddNamespace(string prefix, string ns, bool declared)
        {
            int length = ++this.nsTop;
            if (length == this.nsStack.Length)
            {
                Namespace[] destinationArray = new Namespace[length * 2];
                Array.Copy(this.nsStack, destinationArray, length);
                this.nsStack = destinationArray;
            }
            this.nsStack[length].Set(prefix, ns, declared);
            if (this.useNsHashtable)
            {
                this.AddToNamespaceHashtable(length);
            }
            else if (length == 0x10)
            {
                this.nsHashtable = new Dictionary<string, int>(new SecureStringHasher());
                for (int i = 0; i <= length; i++)
                {
                    this.AddToNamespaceHashtable(i);
                }
                this.useNsHashtable = true;
            }
        }

        private void AddToNamespaceHashtable(int namespaceIndex)
        {
            int num;
            string prefix = this.nsStack[namespaceIndex].prefix;
            if (this.nsHashtable.TryGetValue(prefix, out num))
            {
                this.nsStack[namespaceIndex].prevNsIndex = num;
            }
            this.nsHashtable[prefix] = namespaceIndex;
        }

        private void AutoComplete(Token token)
        {
            if (this.currentState == State.Closed)
            {
                throw new InvalidOperationException(Res.GetString("Xml_Closed"));
            }
            if (this.currentState == State.Error)
            {
                throw new InvalidOperationException(Res.GetString("Xml_WrongToken", new object[] { tokenName[(int) token], stateName[8] }));
            }
            State epilog = this.stateTable[(int) ((token * Token.EndAttribute) + ((Token) ((int) this.currentState)))];
            if (epilog == State.Error)
            {
                throw new InvalidOperationException(Res.GetString("Xml_WrongToken", new object[] { tokenName[(int) token], stateName[(int) this.currentState] }));
            }
            switch (token)
            {
                case Token.PI:
                case Token.Comment:
                case Token.CData:
                case Token.StartElement:
                    if (this.currentState != State.Attribute)
                    {
                        if (this.currentState == State.Element)
                        {
                            this.WriteEndStartTag(false);
                        }
                        break;
                    }
                    this.WriteEndAttributeQuote();
                    this.WriteEndStartTag(false);
                    break;

                case Token.Doctype:
                    if (this.indented && (this.currentState != State.Start))
                    {
                        this.Indent(false);
                    }
                    goto Label_0272;

                case Token.EndElement:
                case Token.LongEndElement:
                    if (this.flush)
                    {
                        this.FlushEncoders();
                    }
                    if (this.currentState == State.Attribute)
                    {
                        this.WriteEndAttributeQuote();
                    }
                    if (this.currentState == State.Content)
                    {
                        token = Token.LongEndElement;
                    }
                    else
                    {
                        this.WriteEndStartTag(token == Token.EndElement);
                    }
                    if ((stateTableDocument == this.stateTable) && (this.top == 1))
                    {
                        epilog = State.Epilog;
                    }
                    goto Label_0272;

                case Token.StartAttribute:
                    if (this.flush)
                    {
                        this.FlushEncoders();
                    }
                    if (this.currentState == State.Attribute)
                    {
                        this.WriteEndAttributeQuote();
                        this.textWriter.Write(' ');
                    }
                    else if (this.currentState == State.Element)
                    {
                        this.textWriter.Write(' ');
                    }
                    goto Label_0272;

                case Token.EndAttribute:
                    if (this.flush)
                    {
                        this.FlushEncoders();
                    }
                    this.WriteEndAttributeQuote();
                    goto Label_0272;

                case Token.Content:
                case Token.Base64:
                case Token.RawData:
                case Token.Whitespace:
                    if ((token != Token.Base64) && this.flush)
                    {
                        this.FlushEncoders();
                    }
                    if ((this.currentState == State.Element) && (this.lastToken != Token.Content))
                    {
                        this.WriteEndStartTag(false);
                    }
                    if (epilog == State.Content)
                    {
                        this.stack[this.top].mixed = true;
                    }
                    goto Label_0272;

                default:
                    throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
            }
            if (token == Token.CData)
            {
                this.stack[this.top].mixed = true;
            }
            else if (this.indented && (this.currentState != State.Start))
            {
                this.Indent(false);
            }
        Label_0272:
            this.currentState = epilog;
            this.lastToken = token;
        }

        private void AutoCompleteAll()
        {
            if (this.flush)
            {
                this.FlushEncoders();
            }
            while (this.top > 0)
            {
                this.WriteEndElement();
            }
        }

        public override void Close()
        {
            try
            {
                this.AutoCompleteAll();
            }
            catch
            {
            }
            finally
            {
                this.currentState = State.Closed;
                this.textWriter.Close();
            }
        }

        private string FindPrefix(string ns)
        {
            for (int i = this.nsTop; i >= 0; i--)
            {
                if ((this.nsStack[i].ns == ns) && (this.LookupNamespace(this.nsStack[i].prefix) == i))
                {
                    return this.nsStack[i].prefix;
                }
            }
            return null;
        }

        public override void Flush()
        {
            this.textWriter.Flush();
        }

        private void FlushEncoders()
        {
            if (this.base64Encoder != null)
            {
                this.base64Encoder.Flush();
            }
            this.flush = false;
        }

        private string GeneratePrefix()
        {
            int num = this.stack[this.top].prefixCount++ + 1;
            return ("d" + this.top.ToString("d", CultureInfo.InvariantCulture) + "p" + num.ToString("d", CultureInfo.InvariantCulture));
        }

        private void HandleSpecialAttribute()
        {
            string attributeValue = this.xmlEncoder.AttributeValue;
            switch (this.specialAttr)
            {
                case SpecialAttr.XmlSpace:
                    attributeValue = XmlConvert.TrimString(attributeValue);
                    if (!(attributeValue == "default"))
                    {
                        if (attributeValue != "preserve")
                        {
                            throw new ArgumentException(Res.GetString("Xml_InvalidXmlSpace", new object[] { attributeValue }));
                        }
                        this.stack[this.top].xmlSpace = System.Xml.XmlSpace.Preserve;
                        return;
                    }
                    this.stack[this.top].xmlSpace = System.Xml.XmlSpace.Default;
                    return;

                case SpecialAttr.XmlLang:
                    this.stack[this.top].xmlLang = attributeValue;
                    return;

                case SpecialAttr.XmlNs:
                    this.VerifyPrefixXml(this.prefixForXmlNs, attributeValue);
                    this.PushNamespace(this.prefixForXmlNs, attributeValue, true);
                    return;
            }
        }

        private void Indent(bool beforeEndElement)
        {
            if (this.top == 0)
            {
                this.textWriter.WriteLine();
            }
            else if (!this.stack[this.top].mixed)
            {
                this.textWriter.WriteLine();
                int num = beforeEndElement ? (this.top - 1) : this.top;
                for (num *= this.indentation; num > 0; num--)
                {
                    this.textWriter.Write(this.indentChar);
                }
            }
        }

        private void InternalWriteEndElement(bool longFormat)
        {
            try
            {
                if (this.top <= 0)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_NoStartTag"));
                }
                this.AutoComplete(longFormat ? Token.LongEndElement : Token.EndElement);
                if (this.lastToken == Token.LongEndElement)
                {
                    if (this.indented)
                    {
                        this.Indent(true);
                    }
                    this.textWriter.Write('<');
                    this.textWriter.Write('/');
                    if (this.namespaces && (this.stack[this.top].prefix != null))
                    {
                        this.textWriter.Write(this.stack[this.top].prefix);
                        this.textWriter.Write(':');
                    }
                    this.textWriter.Write(this.stack[this.top].name);
                    this.textWriter.Write('>');
                }
                int prevNsTop = this.stack[this.top].prevNsTop;
                if (this.useNsHashtable && (prevNsTop < this.nsTop))
                {
                    this.PopNamespaces(prevNsTop + 1, this.nsTop);
                }
                this.nsTop = prevNsTop;
                this.top--;
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        private void InternalWriteName(string name, bool isNCName)
        {
            this.ValidateName(name, isNCName);
            this.textWriter.Write(name);
        }

        private void InternalWriteProcessingInstruction(string name, string text)
        {
            this.textWriter.Write("<?");
            this.ValidateName(name, false);
            this.textWriter.Write(name);
            this.textWriter.Write(' ');
            if (text != null)
            {
                this.xmlEncoder.WriteRawWithSurrogateChecking(text);
            }
            this.textWriter.Write("?>");
        }

        private int LookupNamespace(string prefix)
        {
            if (this.useNsHashtable)
            {
                int num;
                if (this.nsHashtable.TryGetValue(prefix, out num))
                {
                    return num;
                }
            }
            else
            {
                for (int i = this.nsTop; i >= 0; i--)
                {
                    if (this.nsStack[i].prefix == prefix)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private int LookupNamespaceInCurrentScope(string prefix)
        {
            if (this.useNsHashtable)
            {
                int num;
                if (this.nsHashtable.TryGetValue(prefix, out num) && (num > this.stack[this.top].prevNsTop))
                {
                    return num;
                }
            }
            else
            {
                for (int i = this.nsTop; i > this.stack[this.top].prevNsTop; i--)
                {
                    if (this.nsStack[i].prefix == prefix)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public override string LookupPrefix(string ns)
        {
            if ((ns == null) || (ns.Length == 0))
            {
                throw new ArgumentException(Res.GetString("Xml_EmptyName"));
            }
            string str = this.FindPrefix(ns);
            if ((str == null) && (ns == this.stack[this.top].defaultNs))
            {
                str = string.Empty;
            }
            return str;
        }

        private void PopNamespaces(int indexFrom, int indexTo)
        {
            for (int i = indexTo; i >= indexFrom; i--)
            {
                if (this.nsStack[i].prevNsIndex == -1)
                {
                    this.nsHashtable.Remove(this.nsStack[i].prefix);
                }
                else
                {
                    this.nsHashtable[this.nsStack[i].prefix] = this.nsStack[i].prevNsIndex;
                }
            }
        }

        private void PushNamespace(string prefix, string ns, bool declared)
        {
            if ("http://www.w3.org/2000/xmlns/" == ns)
            {
                throw new ArgumentException(Res.GetString("Xml_CanNotBindToReservedNamespace"));
            }
            if (prefix != null)
            {
                if ((prefix.Length != 0) && (ns.Length == 0))
                {
                    throw new ArgumentException(Res.GetString("Xml_PrefixForEmptyNs"));
                }
                int index = this.LookupNamespace(prefix);
                if ((index != -1) && (this.nsStack[index].ns == ns))
                {
                    if (declared)
                    {
                        this.nsStack[index].declared = true;
                    }
                }
                else
                {
                    if ((declared && (index != -1)) && (index > this.stack[this.top].prevNsTop))
                    {
                        this.nsStack[index].declared = true;
                    }
                    this.AddNamespace(prefix, ns, declared);
                }
            }
            else
            {
                switch (this.stack[this.top].defaultNsState)
                {
                    case NamespaceState.Uninitialized:
                    case NamespaceState.NotDeclaredButInScope:
                        this.stack[this.top].defaultNs = ns;
                        break;

                    case NamespaceState.DeclaredButNotWrittenOut:
                        break;

                    default:
                        return;
                }
                this.stack[this.top].defaultNsState = declared ? NamespaceState.DeclaredAndWrittenOut : NamespaceState.DeclaredButNotWrittenOut;
            }
        }

        private void PushStack()
        {
            if (this.top == (this.stack.Length - 1))
            {
                TagInfo[] destinationArray = new TagInfo[this.stack.Length + 10];
                if (this.top > 0)
                {
                    Array.Copy(this.stack, destinationArray, (int) (this.top + 1));
                }
                this.stack = destinationArray;
            }
            this.top++;
            this.stack[this.top].Init(this.nsTop);
        }

        private void StartDocument(int standalone)
        {
            try
            {
                if (this.currentState != State.Start)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_NotTheFirst"));
                }
                this.stateTable = stateTableDocument;
                this.currentState = State.Prolog;
                StringBuilder builder = new StringBuilder(0x80);
                builder.Append(string.Concat(new object[] { "version=", this.quoteChar, "1.0", this.quoteChar }));
                if (this.encoding != null)
                {
                    builder.Append(" encoding=");
                    builder.Append(this.quoteChar);
                    builder.Append(this.encoding.WebName);
                    builder.Append(this.quoteChar);
                }
                if (standalone >= 0)
                {
                    builder.Append(" standalone=");
                    builder.Append(this.quoteChar);
                    builder.Append((standalone == 0) ? "no" : "yes");
                    builder.Append(this.quoteChar);
                }
                this.InternalWriteProcessingInstruction("xml", builder.ToString());
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        private void ValidateName(string name, bool isNCName)
        {
            if ((name == null) || (name.Length == 0))
            {
                throw new ArgumentException(Res.GetString("Xml_EmptyName"));
            }
            int length = name.Length;
            if (this.namespaces)
            {
                int num2 = -1;
                for (int i = ValidateNames.ParseNCName(name); i != length; i += ValidateNames.ParseNmtoken(name, i))
                {
                    if ((((name[i] != ':') || isNCName) || ((num2 != -1) || (i <= 0))) || ((i + 1) >= length))
                    {
                        goto Label_006F;
                    }
                    num2 = i;
                    i++;
                }
                return;
            }
            if (ValidateNames.IsNameNoNamespaces(name))
            {
                return;
            }
        Label_006F:;
            throw new ArgumentException(Res.GetString("Xml_InvalidNameChars", new object[] { name }));
        }

        private void VerifyPrefixXml(string prefix, string ns)
        {
            if (((((prefix != null) && (prefix.Length == 3)) && ((prefix[0] == 'x') || (prefix[0] == 'X'))) && ((prefix[1] == 'm') || (prefix[1] == 'M'))) && (((prefix[2] == 'l') || (prefix[2] == 'L')) && ("http://www.w3.org/XML/1998/namespace" != ns)))
            {
                throw new ArgumentException(Res.GetString("Xml_InvalidPrefix"));
            }
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            try
            {
                if (!this.flush)
                {
                    this.AutoComplete(Token.Base64);
                }
                this.flush = true;
                if (this.base64Encoder == null)
                {
                    this.base64Encoder = new XmlTextWriterBase64Encoder(this.xmlEncoder);
                }
                this.base64Encoder.Encode(buffer, index, count);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            try
            {
                this.AutoComplete(Token.Content);
                BinHexEncoder.Encode(buffer, index, count, this);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteCData(string text)
        {
            try
            {
                this.AutoComplete(Token.CData);
                if ((text != null) && (text.IndexOf("]]>", StringComparison.Ordinal) >= 0))
                {
                    throw new ArgumentException(Res.GetString("Xml_InvalidCDataChars"));
                }
                this.textWriter.Write("<![CDATA[");
                if (text != null)
                {
                    this.xmlEncoder.WriteRawWithSurrogateChecking(text);
                }
                this.textWriter.Write("]]>");
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteCharEntity(char ch)
        {
            try
            {
                this.AutoComplete(Token.Content);
                this.xmlEncoder.WriteCharEntity(ch);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            try
            {
                this.AutoComplete(Token.Content);
                this.xmlEncoder.Write(buffer, index, count);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteComment(string text)
        {
            try
            {
                if ((text != null) && ((text.IndexOf("--", StringComparison.Ordinal) >= 0) || ((text.Length != 0) && (text[text.Length - 1] == '-'))))
                {
                    throw new ArgumentException(Res.GetString("Xml_InvalidCommentChars"));
                }
                this.AutoComplete(Token.Comment);
                this.textWriter.Write("<!--");
                if (text != null)
                {
                    this.xmlEncoder.WriteRawWithSurrogateChecking(text);
                }
                this.textWriter.Write("-->");
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            try
            {
                this.ValidateName(name, false);
                this.AutoComplete(Token.Doctype);
                this.textWriter.Write("<!DOCTYPE ");
                this.textWriter.Write(name);
                if (pubid != null)
                {
                    this.textWriter.Write(" PUBLIC " + this.quoteChar);
                    this.textWriter.Write(pubid);
                    this.textWriter.Write(this.quoteChar + " " + this.quoteChar);
                    this.textWriter.Write(sysid);
                    this.textWriter.Write(this.quoteChar);
                }
                else if (sysid != null)
                {
                    this.textWriter.Write(" SYSTEM " + this.quoteChar);
                    this.textWriter.Write(sysid);
                    this.textWriter.Write(this.quoteChar);
                }
                if (subset != null)
                {
                    this.textWriter.Write("[");
                    this.textWriter.Write(subset);
                    this.textWriter.Write("]");
                }
                this.textWriter.Write('>');
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteEndAttribute()
        {
            try
            {
                this.AutoComplete(Token.EndAttribute);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        private void WriteEndAttributeQuote()
        {
            if (this.specialAttr != SpecialAttr.None)
            {
                this.HandleSpecialAttribute();
            }
            this.xmlEncoder.EndAttribute();
            this.textWriter.Write(this.curQuoteChar);
        }

        public override void WriteEndDocument()
        {
            try
            {
                this.AutoCompleteAll();
                if (this.currentState != State.Epilog)
                {
                    if (this.currentState == State.Closed)
                    {
                        throw new ArgumentException(Res.GetString("Xml_ClosedOrError"));
                    }
                    throw new ArgumentException(Res.GetString("Xml_NoRoot"));
                }
                this.stateTable = stateTableDefault;
                this.currentState = State.Start;
                this.lastToken = Token.Empty;
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteEndElement()
        {
            this.InternalWriteEndElement(false);
        }

        private void WriteEndStartTag(bool empty)
        {
            this.xmlEncoder.StartAttribute(false);
            for (int i = this.nsTop; i > this.stack[this.top].prevNsTop; i--)
            {
                if (!this.nsStack[i].declared)
                {
                    this.textWriter.Write(" xmlns");
                    this.textWriter.Write(':');
                    this.textWriter.Write(this.nsStack[i].prefix);
                    this.textWriter.Write('=');
                    this.textWriter.Write(this.quoteChar);
                    this.xmlEncoder.Write(this.nsStack[i].ns);
                    this.textWriter.Write(this.quoteChar);
                }
            }
            if ((this.stack[this.top].defaultNs != this.stack[this.top - 1].defaultNs) && (this.stack[this.top].defaultNsState == NamespaceState.DeclaredButNotWrittenOut))
            {
                this.textWriter.Write(" xmlns");
                this.textWriter.Write('=');
                this.textWriter.Write(this.quoteChar);
                this.xmlEncoder.Write(this.stack[this.top].defaultNs);
                this.textWriter.Write(this.quoteChar);
                this.stack[this.top].defaultNsState = NamespaceState.DeclaredAndWrittenOut;
            }
            this.xmlEncoder.EndAttribute();
            if (empty)
            {
                this.textWriter.Write(" /");
            }
            this.textWriter.Write('>');
        }

        public override void WriteEntityRef(string name)
        {
            try
            {
                this.ValidateName(name, false);
                this.AutoComplete(Token.Content);
                this.xmlEncoder.WriteEntityRef(name);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteFullEndElement()
        {
            this.InternalWriteEndElement(true);
        }

        public override void WriteName(string name)
        {
            try
            {
                this.AutoComplete(Token.Content);
                this.InternalWriteName(name, false);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteNmToken(string name)
        {
            try
            {
                this.AutoComplete(Token.Content);
                if ((name == null) || (name.Length == 0))
                {
                    throw new ArgumentException(Res.GetString("Xml_EmptyName"));
                }
                if (!ValidateNames.IsNmtokenNoNamespaces(name))
                {
                    throw new ArgumentException(Res.GetString("Xml_InvalidNameChars", new object[] { name }));
                }
                this.textWriter.Write(name);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            try
            {
                if ((text != null) && (text.IndexOf("?>", StringComparison.Ordinal) >= 0))
                {
                    throw new ArgumentException(Res.GetString("Xml_InvalidPiChars"));
                }
                if ((string.Compare(name, "xml", StringComparison.OrdinalIgnoreCase) == 0) && (this.stateTable == stateTableDocument))
                {
                    throw new ArgumentException(Res.GetString("Xml_DupXmlDecl"));
                }
                this.AutoComplete(Token.PI);
                this.InternalWriteProcessingInstruction(name, text);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            try
            {
                this.AutoComplete(Token.Content);
                if (this.namespaces)
                {
                    if (((ns != null) && (ns.Length != 0)) && (ns != this.stack[this.top].defaultNs))
                    {
                        string prefix = this.FindPrefix(ns);
                        if (prefix == null)
                        {
                            if (this.currentState != State.Attribute)
                            {
                                throw new ArgumentException(Res.GetString("Xml_UndefNamespace", new object[] { ns }));
                            }
                            prefix = this.GeneratePrefix();
                            this.PushNamespace(prefix, ns, false);
                        }
                        if (prefix.Length != 0)
                        {
                            this.InternalWriteName(prefix, true);
                            this.textWriter.Write(':');
                        }
                    }
                }
                else if ((ns != null) && (ns.Length != 0))
                {
                    throw new ArgumentException(Res.GetString("Xml_NoNamespaces"));
                }
                this.InternalWriteName(localName, true);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteRaw(string data)
        {
            try
            {
                this.AutoComplete(Token.RawData);
                this.xmlEncoder.WriteRawWithSurrogateChecking(data);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            try
            {
                this.AutoComplete(Token.RawData);
                this.xmlEncoder.WriteRaw(buffer, index, count);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            try
            {
                this.AutoComplete(Token.StartAttribute);
                this.specialAttr = SpecialAttr.None;
                if (this.namespaces)
                {
                    if ((prefix != null) && (prefix.Length == 0))
                    {
                        prefix = null;
                    }
                    if (((ns == "http://www.w3.org/2000/xmlns/") && (prefix == null)) && (localName != "xmlns"))
                    {
                        prefix = "xmlns";
                    }
                    if (prefix == "xml")
                    {
                        if (localName == "lang")
                        {
                            this.specialAttr = SpecialAttr.XmlLang;
                        }
                        else if (localName == "space")
                        {
                            this.specialAttr = SpecialAttr.XmlSpace;
                        }
                    }
                    else if (prefix == "xmlns")
                    {
                        if (("http://www.w3.org/2000/xmlns/" != ns) && (ns != null))
                        {
                            throw new ArgumentException(Res.GetString("Xml_XmlnsBelongsToReservedNs"));
                        }
                        if ((localName == null) || (localName.Length == 0))
                        {
                            localName = prefix;
                            prefix = null;
                            this.prefixForXmlNs = null;
                        }
                        else
                        {
                            this.prefixForXmlNs = localName;
                        }
                        this.specialAttr = SpecialAttr.XmlNs;
                    }
                    else if ((prefix == null) && (localName == "xmlns"))
                    {
                        if (("http://www.w3.org/2000/xmlns/" != ns) && (ns != null))
                        {
                            throw new ArgumentException(Res.GetString("Xml_XmlnsBelongsToReservedNs"));
                        }
                        this.specialAttr = SpecialAttr.XmlNs;
                        this.prefixForXmlNs = null;
                    }
                    else if (ns == null)
                    {
                        if ((prefix != null) && (this.LookupNamespace(prefix) == -1))
                        {
                            throw new ArgumentException(Res.GetString("Xml_UndefPrefix"));
                        }
                    }
                    else if (ns.Length == 0)
                    {
                        prefix = string.Empty;
                    }
                    else
                    {
                        this.VerifyPrefixXml(prefix, ns);
                        if ((prefix != null) && (this.LookupNamespaceInCurrentScope(prefix) != -1))
                        {
                            prefix = null;
                        }
                        string str = this.FindPrefix(ns);
                        if ((str != null) && ((prefix == null) || (prefix == str)))
                        {
                            prefix = str;
                        }
                        else
                        {
                            if (prefix == null)
                            {
                                prefix = this.GeneratePrefix();
                            }
                            this.PushNamespace(prefix, ns, false);
                        }
                    }
                    if ((prefix != null) && (prefix.Length != 0))
                    {
                        this.textWriter.Write(prefix);
                        this.textWriter.Write(':');
                    }
                }
                else
                {
                    if (((ns != null) && (ns.Length != 0)) || ((prefix != null) && (prefix.Length != 0)))
                    {
                        throw new ArgumentException(Res.GetString("Xml_NoNamespaces"));
                    }
                    if (localName == "xml:lang")
                    {
                        this.specialAttr = SpecialAttr.XmlLang;
                    }
                    else if (localName == "xml:space")
                    {
                        this.specialAttr = SpecialAttr.XmlSpace;
                    }
                }
                this.xmlEncoder.StartAttribute(this.specialAttr != SpecialAttr.None);
                this.textWriter.Write(localName);
                this.textWriter.Write('=');
                if (this.curQuoteChar != this.quoteChar)
                {
                    this.curQuoteChar = this.quoteChar;
                    this.xmlEncoder.QuoteChar = this.quoteChar;
                }
                this.textWriter.Write(this.curQuoteChar);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteStartDocument()
        {
            this.StartDocument(-1);
        }

        public override void WriteStartDocument(bool standalone)
        {
            this.StartDocument(standalone ? 1 : 0);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            try
            {
                this.AutoComplete(Token.StartElement);
                this.PushStack();
                this.textWriter.Write('<');
                if (this.namespaces)
                {
                    this.stack[this.top].defaultNs = this.stack[this.top - 1].defaultNs;
                    if (this.stack[this.top - 1].defaultNsState != NamespaceState.Uninitialized)
                    {
                        this.stack[this.top].defaultNsState = NamespaceState.NotDeclaredButInScope;
                    }
                    this.stack[this.top].mixed = this.stack[this.top - 1].mixed;
                    if (ns == null)
                    {
                        if (((prefix != null) && (prefix.Length != 0)) && (this.LookupNamespace(prefix) == -1))
                        {
                            throw new ArgumentException(Res.GetString("Xml_UndefPrefix"));
                        }
                    }
                    else if (prefix == null)
                    {
                        string str = this.FindPrefix(ns);
                        if (str != null)
                        {
                            prefix = str;
                        }
                        else
                        {
                            this.PushNamespace(null, ns, false);
                        }
                    }
                    else if (prefix.Length == 0)
                    {
                        this.PushNamespace(null, ns, false);
                    }
                    else
                    {
                        if (ns.Length == 0)
                        {
                            prefix = null;
                        }
                        this.VerifyPrefixXml(prefix, ns);
                        this.PushNamespace(prefix, ns, false);
                    }
                    this.stack[this.top].prefix = null;
                    if ((prefix != null) && (prefix.Length != 0))
                    {
                        this.stack[this.top].prefix = prefix;
                        this.textWriter.Write(prefix);
                        this.textWriter.Write(':');
                    }
                }
                else if (((ns != null) && (ns.Length != 0)) || ((prefix != null) && (prefix.Length != 0)))
                {
                    throw new ArgumentException(Res.GetString("Xml_NoNamespaces"));
                }
                this.stack[this.top].name = localName;
                this.textWriter.Write(localName);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteString(string text)
        {
            try
            {
                if ((text != null) && (text.Length != 0))
                {
                    this.AutoComplete(Token.Content);
                    this.xmlEncoder.Write(text);
                }
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            try
            {
                this.AutoComplete(Token.Content);
                this.xmlEncoder.WriteSurrogateCharEntity(lowChar, highChar);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public override void WriteWhitespace(string ws)
        {
            try
            {
                if (ws == null)
                {
                    ws = string.Empty;
                }
                if (!this.xmlCharType.IsOnlyWhitespace(ws))
                {
                    throw new ArgumentException(Res.GetString("Xml_NonWhitespace"));
                }
                this.AutoComplete(Token.Whitespace);
                this.xmlEncoder.Write(ws);
            }
            catch
            {
                this.currentState = State.Error;
                throw;
            }
        }

        public Stream BaseStream
        {
            get
            {
                StreamWriter textWriter = this.textWriter as StreamWriter;
                if (textWriter != null)
                {
                    return textWriter.BaseStream;
                }
                return null;
            }
        }

        public System.Xml.Formatting Formatting
        {
            get
            {
                return this.formatting;
            }
            set
            {
                this.formatting = value;
                this.indented = value == System.Xml.Formatting.Indented;
            }
        }

        public int Indentation
        {
            get
            {
                return this.indentation;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("Xml_InvalidIndentation"));
                }
                this.indentation = value;
            }
        }

        public char IndentChar
        {
            get
            {
                return this.indentChar;
            }
            set
            {
                this.indentChar = value;
            }
        }

        public bool Namespaces
        {
            get
            {
                return this.namespaces;
            }
            set
            {
                if (this.currentState != State.Start)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_NotInWriteState"));
                }
                this.namespaces = value;
            }
        }

        public char QuoteChar
        {
            get
            {
                return this.quoteChar;
            }
            set
            {
                if ((value != '"') && (value != '\''))
                {
                    throw new ArgumentException(Res.GetString("Xml_InvalidQuote"));
                }
                this.quoteChar = value;
                this.xmlEncoder.QuoteChar = value;
            }
        }

        public override System.Xml.WriteState WriteState
        {
            get
            {
                switch (this.currentState)
                {
                    case State.Start:
                        return System.Xml.WriteState.Start;

                    case State.Prolog:
                    case State.PostDTD:
                        return System.Xml.WriteState.Prolog;

                    case State.Element:
                        return System.Xml.WriteState.Element;

                    case State.Attribute:
                    case State.AttrOnly:
                        return System.Xml.WriteState.Attribute;

                    case State.Content:
                    case State.Epilog:
                        return System.Xml.WriteState.Content;

                    case State.Error:
                        return System.Xml.WriteState.Error;

                    case State.Closed:
                        return System.Xml.WriteState.Closed;
                }
                return System.Xml.WriteState.Error;
            }
        }

        public override string XmlLang
        {
            get
            {
                for (int i = this.top; i > 0; i--)
                {
                    string xmlLang = this.stack[i].xmlLang;
                    if (xmlLang != null)
                    {
                        return xmlLang;
                    }
                }
                return null;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                for (int i = this.top; i > 0; i--)
                {
                    System.Xml.XmlSpace xmlSpace = this.stack[i].xmlSpace;
                    if (xmlSpace != System.Xml.XmlSpace.None)
                    {
                        return xmlSpace;
                    }
                }
                return System.Xml.XmlSpace.None;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Namespace
        {
            internal string prefix;
            internal string ns;
            internal bool declared;
            internal int prevNsIndex;
            internal void Set(string prefix, string ns, bool declared)
            {
                this.prefix = prefix;
                this.ns = ns;
                this.declared = declared;
                this.prevNsIndex = -1;
            }
        }

        private enum NamespaceState
        {
            Uninitialized,
            NotDeclaredButInScope,
            DeclaredButNotWrittenOut,
            DeclaredAndWrittenOut
        }

        private enum SpecialAttr
        {
            None,
            XmlSpace,
            XmlLang,
            XmlNs
        }

        private enum State
        {
            Start,
            Prolog,
            PostDTD,
            Element,
            Attribute,
            Content,
            AttrOnly,
            Epilog,
            Error,
            Closed
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TagInfo
        {
            internal string name;
            internal string prefix;
            internal string defaultNs;
            internal XmlTextWriter.NamespaceState defaultNsState;
            internal XmlSpace xmlSpace;
            internal string xmlLang;
            internal int prevNsTop;
            internal int prefixCount;
            internal bool mixed;
            internal void Init(int nsTop)
            {
                this.name = null;
                this.defaultNs = string.Empty;
                this.defaultNsState = XmlTextWriter.NamespaceState.Uninitialized;
                this.xmlSpace = XmlSpace.None;
                this.xmlLang = null;
                this.prevNsTop = nsTop;
                this.prefixCount = 0;
                this.mixed = false;
            }
        }

        private enum Token
        {
            PI,
            Doctype,
            Comment,
            CData,
            StartElement,
            EndElement,
            LongEndElement,
            StartAttribute,
            EndAttribute,
            Content,
            Base64,
            RawData,
            Whitespace,
            Empty
        }
    }
}

