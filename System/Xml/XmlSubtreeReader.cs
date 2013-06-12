namespace System.Xml
{
    using System;
    using System.Collections.Generic;

    internal sealed class XmlSubtreeReader : XmlWrappingReader, IXmlLineInfo, IXmlNamespaceResolver
    {
        private const int AttributeActiveStates = 0x62;
        private IncrementalReadDecoder binDecoder;
        private NodeData curNode;
        private int curNsAttr;
        private int initialDepth;
        internal int InitialNamespaceAttributeCount;
        private const int NamespaceActiveStates = 0x7e2;
        private int nsAttrCount;
        private NodeData[] nsAttributes;
        private int nsIncReadOffset;
        private XmlNamespaceManager nsManager;
        private State state;
        private NodeData tmpNode;
        private bool useCurNode;
        private string xmlns;
        private string xmlnsUri;

        internal XmlSubtreeReader(XmlReader reader) : base(reader)
        {
            this.curNsAttr = -1;
            this.InitialNamespaceAttributeCount = 4;
            this.initialDepth = reader.Depth;
            this.state = State.Initial;
            this.nsManager = new XmlNamespaceManager(reader.NameTable);
            this.xmlns = reader.NameTable.Add("xmlns");
            this.xmlnsUri = reader.NameTable.Add("http://www.w3.org/2000/xmlns/");
            this.tmpNode = new NodeData();
            this.tmpNode.Set(XmlNodeType.None, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            this.SetCurrentNode(this.tmpNode);
        }

        private void AddNamespace(string prefix, string ns)
        {
            this.nsManager.AddNamespace(prefix, ns);
            int length = this.nsAttrCount++;
            if (this.nsAttributes == null)
            {
                this.nsAttributes = new NodeData[this.InitialNamespaceAttributeCount];
            }
            if (length == this.nsAttributes.Length)
            {
                NodeData[] destinationArray = new NodeData[this.nsAttributes.Length * 2];
                Array.Copy(this.nsAttributes, 0, destinationArray, 0, length);
                this.nsAttributes = destinationArray;
            }
            if (this.nsAttributes[length] == null)
            {
                this.nsAttributes[length] = new NodeData();
            }
            if (prefix.Length == 0)
            {
                this.nsAttributes[length].Set(XmlNodeType.Attribute, this.xmlns, string.Empty, this.xmlns, this.xmlnsUri, ns);
            }
            else
            {
                this.nsAttributes[length].Set(XmlNodeType.Attribute, prefix, this.xmlns, base.reader.NameTable.Add(this.xmlns + ":" + prefix), this.xmlnsUri, ns);
            }
            this.state = State.ClearNsAttributes;
            this.curNsAttr = -1;
        }

        private void CheckBuffer(Array buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
        }

        public override void Close()
        {
            if (this.state != State.Closed)
            {
                try
                {
                    if (this.state != State.EndOfFile)
                    {
                        base.reader.MoveToElement();
                        if (((base.reader.Depth == this.initialDepth) && (base.reader.NodeType == XmlNodeType.Element)) && !base.reader.IsEmptyElement)
                        {
                            base.reader.Read();
                        }
                        while ((base.reader.Depth > this.initialDepth) && base.reader.Read())
                        {
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    this.curNsAttr = -1;
                    this.useCurNode = false;
                    this.state = State.Closed;
                    this.SetEmptyNode();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.Close();
        }

        private bool FinishReadContentAsBinary()
        {
            byte[] buffer = new byte[0x100];
            if (this.state == State.ReadContentAsBase64)
            {
                while (base.reader.ReadContentAsBase64(buffer, 0, 0x100) > 0)
                {
                }
            }
            else
            {
                while (base.reader.ReadContentAsBinHex(buffer, 0, 0x100) > 0)
                {
                }
            }
            this.state = State.Interactive;
            this.ProcessNamespaces();
            if (base.reader.Depth == this.initialDepth)
            {
                this.state = State.EndOfFile;
                this.SetEmptyNode();
                return false;
            }
            return true;
        }

        private void FinishReadContentAsType()
        {
            switch (this.NodeType)
            {
                case XmlNodeType.Element:
                    this.ProcessNamespaces();
                    return;

                case XmlNodeType.Attribute:
                    break;

                case XmlNodeType.EndElement:
                    this.state = State.PopNamespaceScope;
                    break;

                default:
                    return;
            }
        }

        private bool FinishReadElementContentAsBinary()
        {
            byte[] buffer = new byte[0x100];
            if (this.state == State.ReadElementContentAsBase64)
            {
                while (base.reader.ReadContentAsBase64(buffer, 0, 0x100) > 0)
                {
                }
            }
            else
            {
                while (base.reader.ReadContentAsBinHex(buffer, 0, 0x100) > 0)
                {
                }
            }
            if (this.NodeType != XmlNodeType.EndElement)
            {
                throw new XmlException("Xml_InvalidNodeType", base.reader.NodeType.ToString(), base.reader as IXmlLineInfo);
            }
            this.state = State.Interactive;
            this.ProcessNamespaces();
            if (base.reader.Depth == this.initialDepth)
            {
                this.state = State.EndOfFile;
                this.SetEmptyNode();
                return false;
            }
            return this.Read();
        }

        public override string GetAttribute(int i)
        {
            if (!this.InAttributeActiveState)
            {
                throw new ArgumentOutOfRangeException("i");
            }
            int attributeCount = base.reader.AttributeCount;
            if (i < attributeCount)
            {
                return base.reader.GetAttribute(i);
            }
            if ((i - attributeCount) >= this.nsAttrCount)
            {
                throw new ArgumentOutOfRangeException("i");
            }
            return this.nsAttributes[i - attributeCount].value;
        }

        public override string GetAttribute(string name)
        {
            if (this.InAttributeActiveState)
            {
                string attribute = base.reader.GetAttribute(name);
                if (attribute != null)
                {
                    return attribute;
                }
                for (int i = 0; i < this.nsAttrCount; i++)
                {
                    if (name == this.nsAttributes[i].name)
                    {
                        return this.nsAttributes[i].value;
                    }
                }
            }
            return null;
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            if (this.InAttributeActiveState)
            {
                string attribute = base.reader.GetAttribute(name, namespaceURI);
                if (attribute != null)
                {
                    return attribute;
                }
                for (int i = 0; i < this.nsAttrCount; i++)
                {
                    if ((name == this.nsAttributes[i].localName) && (namespaceURI == this.xmlnsUri))
                    {
                        return this.nsAttributes[i].value;
                    }
                }
            }
            return null;
        }

        private void InitReadContentAsType(string methodName)
        {
            switch (this.state)
            {
                case State.Initial:
                case State.Error:
                case State.EndOfFile:
                case State.Closed:
                    throw new InvalidOperationException(Res.GetString("Xml_ClosedOrErrorReader"));

                case State.Interactive:
                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                    return;

                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    throw new InvalidOperationException(Res.GetString("Xml_MixingReadValueChunkWithBinary"));
            }
            throw base.CreateReadContentAsException(methodName);
        }

        private bool InitReadElementContentAsBinary(State binaryState)
        {
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.reader.CreateReadElementContentAsException("ReadElementContentAsBase64");
            }
            bool isEmptyElement = this.IsEmptyElement;
            if (!this.Read() || isEmptyElement)
            {
                return false;
            }
            switch (this.NodeType)
            {
                case XmlNodeType.Element:
                    throw new XmlException("Xml_InvalidNodeType", base.reader.NodeType.ToString(), base.reader as IXmlLineInfo);

                case XmlNodeType.EndElement:
                    this.ProcessNamespaces();
                    this.Read();
                    return false;
            }
            this.state = binaryState;
            return true;
        }

        public override string LookupNamespace(string prefix)
        {
            return ((IXmlNamespaceResolver) this).LookupNamespace(prefix);
        }

        public override void MoveToAttribute(int i)
        {
            if (!this.InAttributeActiveState)
            {
                throw new ArgumentOutOfRangeException("i");
            }
            int attributeCount = base.reader.AttributeCount;
            if (i < attributeCount)
            {
                base.reader.MoveToAttribute(i);
                this.curNsAttr = -1;
                this.useCurNode = false;
            }
            else
            {
                if ((i - attributeCount) >= this.nsAttrCount)
                {
                    throw new ArgumentOutOfRangeException("i");
                }
                this.MoveToNsAttribute(i - attributeCount);
            }
        }

        public override bool MoveToAttribute(string name)
        {
            if (this.InAttributeActiveState)
            {
                if (base.reader.MoveToAttribute(name))
                {
                    this.curNsAttr = -1;
                    this.useCurNode = false;
                    return true;
                }
                for (int i = 0; i < this.nsAttrCount; i++)
                {
                    if (name == this.nsAttributes[i].name)
                    {
                        this.MoveToNsAttribute(i);
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            if (this.InAttributeActiveState)
            {
                if (base.reader.MoveToAttribute(name, ns))
                {
                    this.curNsAttr = -1;
                    this.useCurNode = false;
                    return true;
                }
                for (int i = 0; i < this.nsAttrCount; i++)
                {
                    if ((name == this.nsAttributes[i].localName) && (ns == this.xmlnsUri))
                    {
                        this.MoveToNsAttribute(i);
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool MoveToElement()
        {
            if (!this.InAttributeActiveState)
            {
                return false;
            }
            this.curNsAttr = -1;
            this.useCurNode = false;
            return base.reader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.InAttributeActiveState)
            {
                if (base.reader.MoveToFirstAttribute())
                {
                    this.useCurNode = false;
                    return true;
                }
                if (this.nsAttrCount > 0)
                {
                    this.MoveToNsAttribute(0);
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            if (this.InAttributeActiveState)
            {
                if ((this.curNsAttr == -1) && base.reader.MoveToNextAttribute())
                {
                    return true;
                }
                if ((this.curNsAttr + 1) < this.nsAttrCount)
                {
                    this.MoveToNsAttribute(this.curNsAttr + 1);
                    return true;
                }
            }
            return false;
        }

        private void MoveToNsAttribute(int index)
        {
            base.reader.MoveToElement();
            this.curNsAttr = index;
            this.nsIncReadOffset = 0;
            this.SetCurrentNode(this.nsAttributes[index]);
        }

        private void ProcessNamespaces()
        {
            XmlNodeType nodeType = base.reader.NodeType;
            if (nodeType != XmlNodeType.Element)
            {
                if (nodeType != XmlNodeType.EndElement)
                {
                    return;
                }
            }
            else
            {
                this.nsManager.PushScope();
                string prefix = base.reader.Prefix;
                string namespaceURI = base.reader.NamespaceURI;
                if (this.nsManager.LookupNamespace(prefix) != namespaceURI)
                {
                    this.AddNamespace(prefix, namespaceURI);
                }
                if (base.reader.MoveToFirstAttribute())
                {
                    do
                    {
                        prefix = base.reader.Prefix;
                        namespaceURI = base.reader.NamespaceURI;
                        if (Ref.Equal(namespaceURI, this.xmlnsUri))
                        {
                            if (prefix.Length == 0)
                            {
                                this.nsManager.AddNamespace(string.Empty, base.reader.Value);
                                this.RemoveNamespace(string.Empty, this.xmlns);
                            }
                            else
                            {
                                prefix = base.reader.LocalName;
                                this.nsManager.AddNamespace(prefix, base.reader.Value);
                                this.RemoveNamespace(this.xmlns, prefix);
                            }
                        }
                        else if ((prefix.Length != 0) && (this.nsManager.LookupNamespace(prefix) != namespaceURI))
                        {
                            this.AddNamespace(prefix, namespaceURI);
                        }
                    }
                    while (base.reader.MoveToNextAttribute());
                    base.reader.MoveToElement();
                }
                if (base.reader.IsEmptyElement)
                {
                    this.state = State.PopNamespaceScope;
                }
                return;
            }
            this.state = State.PopNamespaceScope;
        }

        public override bool Read()
        {
            switch (this.state)
            {
                case State.Initial:
                    this.useCurNode = false;
                    this.state = State.Interactive;
                    this.ProcessNamespaces();
                    return true;

                case State.Interactive:
                    break;

                case State.Error:
                case State.EndOfFile:
                case State.Closed:
                    return false;

                case State.PopNamespaceScope:
                    this.nsManager.PopScope();
                    goto Label_00E5;

                case State.ClearNsAttributes:
                    goto Label_00E5;

                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                    if (!this.FinishReadElementContentAsBinary())
                    {
                        return false;
                    }
                    return this.Read();

                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    return (this.FinishReadContentAsBinary() && this.Read());

                default:
                    return false;
            }
        Label_0054:
            this.curNsAttr = -1;
            this.useCurNode = false;
            base.reader.MoveToElement();
            if ((base.reader.Depth == this.initialDepth) && ((base.reader.NodeType == XmlNodeType.EndElement) || ((base.reader.NodeType == XmlNodeType.Element) && base.reader.IsEmptyElement)))
            {
                this.state = State.EndOfFile;
                this.SetEmptyNode();
                return false;
            }
            if (base.reader.Read())
            {
                this.ProcessNamespaces();
                return true;
            }
            this.SetEmptyNode();
            return false;
        Label_00E5:
            this.nsAttrCount = 0;
            this.state = State.Interactive;
            goto Label_0054;
        }

        public override bool ReadAttributeValue()
        {
            if (!this.InAttributeActiveState)
            {
                return false;
            }
            if (this.curNsAttr == -1)
            {
                return base.reader.ReadAttributeValue();
            }
            if (this.curNode.type == XmlNodeType.Text)
            {
                return false;
            }
            this.tmpNode.type = XmlNodeType.Text;
            this.tmpNode.value = this.curNode.value;
            this.SetCurrentNode(this.tmpNode);
            return true;
        }

        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            object obj3;
            try
            {
                this.InitReadContentAsType("ReadContentAs");
                object obj2 = base.reader.ReadContentAs(returnType, namespaceResolver);
                this.FinishReadContentAsType();
                obj3 = obj2;
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
            return obj3;
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            int num;
            switch (this.state)
            {
                case State.Initial:
                case State.Error:
                case State.EndOfFile:
                case State.Closed:
                    return 0;

                case State.Interactive:
                    this.state = State.ReadContentAsBase64;
                    goto Label_015F;

                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                    switch (this.NodeType)
                    {
                        case XmlNodeType.Element:
                            throw base.CreateReadContentAsException("ReadContentAsBase64");

                        case XmlNodeType.Attribute:
                            if ((this.curNsAttr != -1) && base.reader.CanReadBinaryContent)
                            {
                                this.CheckBuffer(buffer, index, count);
                                if (count == 0)
                                {
                                    return 0;
                                }
                                if (this.nsIncReadOffset == 0)
                                {
                                    if ((this.binDecoder != null) && (this.binDecoder is Base64Decoder))
                                    {
                                        this.binDecoder.Reset();
                                    }
                                    else
                                    {
                                        this.binDecoder = new Base64Decoder();
                                    }
                                }
                                if (this.nsIncReadOffset == this.curNode.value.Length)
                                {
                                    return 0;
                                }
                                this.binDecoder.SetNextOutputBuffer(buffer, index, count);
                                this.nsIncReadOffset += this.binDecoder.Decode(this.curNode.value, this.nsIncReadOffset, this.curNode.value.Length - this.nsIncReadOffset);
                                return this.binDecoder.DecodedCount;
                            }
                            goto Label_0146;

                        case XmlNodeType.EndElement:
                            return 0;
                    }
                    return 0;

                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                case State.ReadContentAsBinHex:
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));

                case State.ReadContentAsBase64:
                    goto Label_015F;

                default:
                    return 0;
            }
        Label_0146:
            return base.reader.ReadContentAsBase64(buffer, index, count);
        Label_015F:
            num = base.reader.ReadContentAsBase64(buffer, index, count);
            if (num == 0)
            {
                this.state = State.Interactive;
                this.ProcessNamespaces();
            }
            return num;
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            int num;
            switch (this.state)
            {
                case State.Initial:
                case State.Error:
                case State.EndOfFile:
                case State.Closed:
                    return 0;

                case State.Interactive:
                    this.state = State.ReadContentAsBinHex;
                    goto Label_015F;

                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                    switch (this.NodeType)
                    {
                        case XmlNodeType.Element:
                            throw base.CreateReadContentAsException("ReadContentAsBinHex");

                        case XmlNodeType.Attribute:
                            if ((this.curNsAttr != -1) && base.reader.CanReadBinaryContent)
                            {
                                this.CheckBuffer(buffer, index, count);
                                if (count == 0)
                                {
                                    return 0;
                                }
                                if (this.nsIncReadOffset == 0)
                                {
                                    if ((this.binDecoder != null) && (this.binDecoder is BinHexDecoder))
                                    {
                                        this.binDecoder.Reset();
                                    }
                                    else
                                    {
                                        this.binDecoder = new BinHexDecoder();
                                    }
                                }
                                if (this.nsIncReadOffset == this.curNode.value.Length)
                                {
                                    return 0;
                                }
                                this.binDecoder.SetNextOutputBuffer(buffer, index, count);
                                this.nsIncReadOffset += this.binDecoder.Decode(this.curNode.value, this.nsIncReadOffset, this.curNode.value.Length - this.nsIncReadOffset);
                                return this.binDecoder.DecodedCount;
                            }
                            goto Label_0146;

                        case XmlNodeType.EndElement:
                            return 0;
                    }
                    return 0;

                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                case State.ReadContentAsBase64:
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));

                case State.ReadContentAsBinHex:
                    goto Label_015F;

                default:
                    return 0;
            }
        Label_0146:
            return base.reader.ReadContentAsBinHex(buffer, index, count);
        Label_015F:
            num = base.reader.ReadContentAsBinHex(buffer, index, count);
            if (num == 0)
            {
                this.state = State.Interactive;
                this.ProcessNamespaces();
            }
            return num;
        }

        public override bool ReadContentAsBoolean()
        {
            bool flag2;
            try
            {
                this.InitReadContentAsType("ReadContentAsBoolean");
                bool flag = base.reader.ReadContentAsBoolean();
                this.FinishReadContentAsType();
                flag2 = flag;
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
            return flag2;
        }

        public override DateTime ReadContentAsDateTime()
        {
            DateTime time2;
            try
            {
                this.InitReadContentAsType("ReadContentAsDateTime");
                DateTime time = base.reader.ReadContentAsDateTime();
                this.FinishReadContentAsType();
                time2 = time;
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
            return time2;
        }

        public override decimal ReadContentAsDecimal()
        {
            decimal num2;
            try
            {
                this.InitReadContentAsType("ReadContentAsDecimal");
                decimal num = base.reader.ReadContentAsDecimal();
                this.FinishReadContentAsType();
                num2 = num;
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
            return num2;
        }

        public override double ReadContentAsDouble()
        {
            double num2;
            try
            {
                this.InitReadContentAsType("ReadContentAsDouble");
                double num = base.reader.ReadContentAsDouble();
                this.FinishReadContentAsType();
                num2 = num;
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
            return num2;
        }

        public override float ReadContentAsFloat()
        {
            float num2;
            try
            {
                this.InitReadContentAsType("ReadContentAsFloat");
                float num = base.reader.ReadContentAsFloat();
                this.FinishReadContentAsType();
                num2 = num;
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
            return num2;
        }

        public override int ReadContentAsInt()
        {
            int num2;
            try
            {
                this.InitReadContentAsType("ReadContentAsInt");
                int num = base.reader.ReadContentAsInt();
                this.FinishReadContentAsType();
                num2 = num;
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
            return num2;
        }

        public override long ReadContentAsLong()
        {
            long num2;
            try
            {
                this.InitReadContentAsType("ReadContentAsLong");
                long num = base.reader.ReadContentAsLong();
                this.FinishReadContentAsType();
                num2 = num;
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
            return num2;
        }

        public override object ReadContentAsObject()
        {
            object obj3;
            try
            {
                this.InitReadContentAsType("ReadContentAsObject");
                object obj2 = base.reader.ReadContentAsObject();
                this.FinishReadContentAsType();
                obj3 = obj2;
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
            return obj3;
        }

        public override string ReadContentAsString()
        {
            string str2;
            try
            {
                this.InitReadContentAsType("ReadContentAsString");
                string str = base.reader.ReadContentAsString();
                this.FinishReadContentAsType();
                str2 = str;
            }
            catch
            {
                this.state = State.Error;
                throw;
            }
            return str2;
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            switch (this.state)
            {
                case State.Initial:
                case State.Error:
                case State.EndOfFile:
                case State.Closed:
                    return 0;

                case State.Interactive:
                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                    if (this.InitReadElementContentAsBinary(State.ReadElementContentAsBase64))
                    {
                        break;
                    }
                    return 0;

                case State.ReadElementContentAsBase64:
                    break;

                case State.ReadElementContentAsBinHex:
                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));

                default:
                    return 0;
            }
            int num = base.reader.ReadContentAsBase64(buffer, index, count);
            if ((num > 0) || (count == 0))
            {
                return num;
            }
            if (this.NodeType != XmlNodeType.EndElement)
            {
                throw new XmlException("Xml_InvalidNodeType", base.reader.NodeType.ToString(), base.reader as IXmlLineInfo);
            }
            this.state = State.Interactive;
            this.ProcessNamespaces();
            if (base.reader.Depth == this.initialDepth)
            {
                this.state = State.EndOfFile;
                this.SetEmptyNode();
            }
            else
            {
                this.Read();
            }
            return 0;
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            switch (this.state)
            {
                case State.Initial:
                case State.Error:
                case State.EndOfFile:
                case State.Closed:
                    return 0;

                case State.Interactive:
                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                    if (this.InitReadElementContentAsBinary(State.ReadElementContentAsBinHex))
                    {
                        break;
                    }
                    return 0;

                case State.ReadElementContentAsBase64:
                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));

                case State.ReadElementContentAsBinHex:
                    break;

                default:
                    return 0;
            }
            int num = base.reader.ReadContentAsBinHex(buffer, index, count);
            if ((num > 0) || (count == 0))
            {
                return num;
            }
            if (this.NodeType != XmlNodeType.EndElement)
            {
                throw new XmlException("Xml_InvalidNodeType", base.reader.NodeType.ToString(), base.reader as IXmlLineInfo);
            }
            this.state = State.Interactive;
            this.ProcessNamespaces();
            if (base.reader.Depth == this.initialDepth)
            {
                this.state = State.EndOfFile;
                this.SetEmptyNode();
            }
            else
            {
                this.Read();
            }
            return 0;
        }

        public override int ReadValueChunk(char[] buffer, int index, int count)
        {
            switch (this.state)
            {
                case State.Initial:
                case State.Error:
                case State.EndOfFile:
                case State.Closed:
                    return 0;

                case State.Interactive:
                    break;

                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                {
                    if ((this.curNsAttr == -1) || !base.reader.CanReadValueChunk)
                    {
                        break;
                    }
                    this.CheckBuffer(buffer, index, count);
                    int num = this.curNode.value.Length - this.nsIncReadOffset;
                    if (num > count)
                    {
                        num = count;
                    }
                    if (num > 0)
                    {
                        this.curNode.value.CopyTo(this.nsIncReadOffset, buffer, index, num);
                    }
                    this.nsIncReadOffset += num;
                    return num;
                }
                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    throw new InvalidOperationException(Res.GetString("Xml_MixingReadValueChunkWithBinary"));

                default:
                    return 0;
            }
            return base.reader.ReadValueChunk(buffer, index, count);
        }

        private void RemoveNamespace(string prefix, string localName)
        {
            for (int i = 0; i < this.nsAttrCount; i++)
            {
                if (Ref.Equal(prefix, this.nsAttributes[i].prefix) && Ref.Equal(localName, this.nsAttributes[i].localName))
                {
                    if (i < (this.nsAttrCount - 1))
                    {
                        NodeData data = this.nsAttributes[i];
                        this.nsAttributes[i] = this.nsAttributes[this.nsAttrCount - 1];
                        this.nsAttributes[this.nsAttrCount - 1] = data;
                    }
                    this.nsAttrCount--;
                    return;
                }
            }
        }

        private void SetCurrentNode(NodeData node)
        {
            this.curNode = node;
            this.useCurNode = true;
        }

        private void SetEmptyNode()
        {
            this.tmpNode.type = XmlNodeType.None;
            this.tmpNode.value = string.Empty;
            this.curNode = this.tmpNode;
            this.useCurNode = true;
        }

        public override void Skip()
        {
            switch (this.state)
            {
                case State.Initial:
                    this.Read();
                    return;

                case State.Interactive:
                    break;

                case State.Error:
                    return;

                case State.EndOfFile:
                case State.Closed:
                    return;

                case State.PopNamespaceScope:
                    this.nsManager.PopScope();
                    goto Label_0119;

                case State.ClearNsAttributes:
                    goto Label_0119;

                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                    if (this.FinishReadElementContentAsBinary())
                    {
                        this.Skip();
                    }
                    return;

                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    if (this.FinishReadContentAsBinary())
                    {
                        this.Skip();
                    }
                    return;

                default:
                    return;
            }
        Label_0042:
            this.curNsAttr = -1;
            this.useCurNode = false;
            base.reader.MoveToElement();
            if (base.reader.Depth == this.initialDepth)
            {
                if (((base.reader.NodeType == XmlNodeType.Element) && !base.reader.IsEmptyElement) && base.reader.Read())
                {
                    while ((base.reader.NodeType != XmlNodeType.EndElement) && (base.reader.Depth > this.initialDepth))
                    {
                        base.reader.Skip();
                    }
                }
                this.state = State.EndOfFile;
                this.SetEmptyNode();
                return;
            }
            if ((base.reader.NodeType == XmlNodeType.Element) && !base.reader.IsEmptyElement)
            {
                this.nsManager.PopScope();
            }
            base.reader.Skip();
            this.ProcessNamespaces();
            return;
        Label_0119:
            this.nsAttrCount = 0;
            this.state = State.Interactive;
            goto Label_0042;
        }

        bool IXmlLineInfo.HasLineInfo()
        {
            return (base.reader is IXmlLineInfo);
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            if (!this.InNamespaceActiveState)
            {
                return new Dictionary<string, string>();
            }
            return this.nsManager.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix)
        {
            if (!this.InNamespaceActiveState)
            {
                return null;
            }
            return this.nsManager.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            if (!this.InNamespaceActiveState)
            {
                return null;
            }
            return this.nsManager.LookupPrefix(namespaceName);
        }

        public override int AttributeCount
        {
            get
            {
                if (!this.InAttributeActiveState)
                {
                    return 0;
                }
                return (base.reader.AttributeCount + this.nsAttrCount);
            }
        }

        public override string BaseURI
        {
            get
            {
                return base.reader.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return base.reader.CanReadBinaryContent;
            }
        }

        public override bool CanReadValueChunk
        {
            get
            {
                return base.reader.CanReadValueChunk;
            }
        }

        public override int Depth
        {
            get
            {
                int num = base.reader.Depth - this.initialDepth;
                if (this.curNsAttr != -1)
                {
                    if (this.curNode.type == XmlNodeType.Text)
                    {
                        return (num + 2);
                    }
                    num++;
                }
                return num;
            }
        }

        public override bool EOF
        {
            get
            {
                if (this.state != State.EndOfFile)
                {
                    return (this.state == State.Closed);
                }
                return true;
            }
        }

        private bool InAttributeActiveState
        {
            get
            {
                return (0 != (0x62 & (((int) 1) << this.state)));
            }
        }

        private bool InNamespaceActiveState
        {
            get
            {
                return (0 != (0x7e2 & (((int) 1) << this.state)));
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return base.reader.IsEmptyElement;
            }
        }

        public override string LocalName
        {
            get
            {
                if (!this.useCurNode)
                {
                    return base.reader.LocalName;
                }
                return this.curNode.localName;
            }
        }

        public override string Name
        {
            get
            {
                if (!this.useCurNode)
                {
                    return base.reader.Name;
                }
                return this.curNode.name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                if (!this.useCurNode)
                {
                    return base.reader.NamespaceURI;
                }
                return this.curNode.namespaceUri;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return base.reader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                if (!this.useCurNode)
                {
                    return base.reader.NodeType;
                }
                return this.curNode.type;
            }
        }

        public override string Prefix
        {
            get
            {
                if (!this.useCurNode)
                {
                    return base.reader.Prefix;
                }
                return this.curNode.prefix;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                if (base.reader.ReadState == System.Xml.ReadState.Error)
                {
                    return System.Xml.ReadState.Error;
                }
                if (this.state <= State.Closed)
                {
                    return (System.Xml.ReadState) this.state;
                }
                return System.Xml.ReadState.Interactive;
            }
        }

        int IXmlLineInfo.LineNumber
        {
            get
            {
                if (!this.useCurNode)
                {
                    IXmlLineInfo reader = base.reader as IXmlLineInfo;
                    if (reader != null)
                    {
                        return reader.LineNumber;
                    }
                }
                return 0;
            }
        }

        int IXmlLineInfo.LinePosition
        {
            get
            {
                if (!this.useCurNode)
                {
                    IXmlLineInfo reader = base.reader as IXmlLineInfo;
                    if (reader != null)
                    {
                        return reader.LinePosition;
                    }
                }
                return 0;
            }
        }

        public override string Value
        {
            get
            {
                if (!this.useCurNode)
                {
                    return base.reader.Value;
                }
                return this.curNode.value;
            }
        }

        private class NodeData
        {
            internal string localName;
            internal string name;
            internal string namespaceUri;
            internal string prefix;
            internal XmlNodeType type;
            internal string value;

            internal NodeData()
            {
            }

            internal void Set(XmlNodeType nodeType, string localName, string prefix, string name, string namespaceUri, string value)
            {
                this.type = nodeType;
                this.localName = localName;
                this.prefix = prefix;
                this.name = name;
                this.namespaceUri = namespaceUri;
                this.value = value;
            }
        }

        private enum State
        {
            Initial,
            Interactive,
            Error,
            EndOfFile,
            Closed,
            PopNamespaceScope,
            ClearNsAttributes,
            ReadElementContentAsBase64,
            ReadElementContentAsBinHex,
            ReadContentAsBase64,
            ReadContentAsBinHex
        }
    }
}

