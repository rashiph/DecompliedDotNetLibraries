namespace System.Xml
{
    using System;
    using System.Collections.Generic;

    internal sealed class DocumentXmlWriter : XmlRawWriter, IXmlNamespaceResolver
    {
        private static State[] changeState;
        private XmlDocument document;
        private XmlNode end;
        private List<XmlNode> fragment;
        private XmlNamespaceManager namespaceManager;
        private DocumentXPathNavigator navigator;
        private XmlWriterSettings settings;
        private XmlNode start;
        private State state;
        private DocumentXmlWriterType type;
        private XmlNode write;

        static DocumentXmlWriter()
        {
            State[] stateArray = new State[0x55];
            stateArray[2] = State.Prolog;
            stateArray[3] = State.Content;
            stateArray[0x11] = State.Prolog;
            stateArray[0x16] = State.Content;
            stateArray[0x17] = State.Content;
            stateArray[0x18] = State.Content;
            stateArray[0x1d] = State.Content;
            stateArray[0x22] = State.Content;
            stateArray[0x24] = State.Content;
            stateArray[0x27] = State.Content;
            stateArray[0x2c] = State.Content;
            stateArray[0x2e] = State.Content;
            stateArray[0x31] = State.Content;
            stateArray[0x36] = State.Content;
            stateArray[0x3a] = State.Content;
            stateArray[0x3b] = State.Content;
            stateArray[0x3e] = State.Prolog;
            stateArray[0x3f] = State.Content;
            stateArray[0x40] = State.Content;
            stateArray[0x43] = State.Prolog;
            stateArray[0x44] = State.Content;
            stateArray[0x45] = State.Content;
            stateArray[0x49] = State.Content;
            stateArray[0x4a] = State.Content;
            stateArray[0x4d] = State.Prolog;
            stateArray[0x4e] = State.Content;
            stateArray[0x4f] = State.Content;
            stateArray[0x53] = State.Content;
            stateArray[0x54] = State.Content;
            changeState = stateArray;
        }

        public DocumentXmlWriter(DocumentXmlWriterType type, XmlNode start, XmlDocument document)
        {
            this.type = type;
            this.start = start;
            this.document = document;
            this.state = this.StartState();
            this.fragment = new List<XmlNode>();
            this.settings = new XmlWriterSettings();
            this.settings.ReadOnly = false;
            this.settings.CheckCharacters = false;
            this.settings.CloseOutput = false;
            this.settings.ConformanceLevel = (this.state == State.Prolog) ? ConformanceLevel.Document : ConformanceLevel.Fragment;
            this.settings.ReadOnly = true;
        }

        private void AddAttribute(XmlAttribute attr, XmlNode parent)
        {
            if (parent == null)
            {
                this.fragment.Add(attr);
            }
            else
            {
                XmlElement element = parent as XmlElement;
                if (element == null)
                {
                    throw new InvalidOperationException();
                }
                element.Attributes.Append(attr);
            }
        }

        private void AddChild(XmlNode node, XmlNode parent)
        {
            if (parent == null)
            {
                this.fragment.Add(node);
            }
            else
            {
                parent.AppendChild(node);
            }
        }

        public override void Close()
        {
        }

        internal override void Close(WriteState currentState)
        {
            if (currentState != WriteState.Error)
            {
                try
                {
                    XmlNode parentNode;
                    int num2;
                    int num3;
                    int num4;
                    switch (this.type)
                    {
                        case DocumentXmlWriterType.InsertSiblingAfter:
                            parentNode = this.start.ParentNode;
                            if (parentNode == null)
                            {
                                throw new InvalidOperationException(Res.GetString("Xpn_MissingParent"));
                            }
                            break;

                        case DocumentXmlWriterType.InsertSiblingBefore:
                            parentNode = this.start.ParentNode;
                            if (parentNode == null)
                            {
                                throw new InvalidOperationException(Res.GetString("Xpn_MissingParent"));
                            }
                            goto Label_00A5;

                        case DocumentXmlWriterType.PrependChild:
                            num3 = this.fragment.Count - 1;
                            goto Label_0105;

                        case DocumentXmlWriterType.AppendChild:
                            num4 = 0;
                            goto Label_012F;

                        case DocumentXmlWriterType.AppendAttribute:
                            this.CloseWithAppendAttribute();
                            return;

                        case DocumentXmlWriterType.ReplaceToFollowingSibling:
                            if (this.fragment.Count == 0)
                            {
                                throw new InvalidOperationException(Res.GetString("Xpn_NoContent"));
                            }
                            goto Label_0165;

                        default:
                            return;
                    }
                    for (int i = this.fragment.Count - 1; i >= 0; i--)
                    {
                        parentNode.InsertAfter(this.fragment[i], this.start);
                    }
                    return;
                Label_00A5:
                    num2 = 0;
                    while (num2 < this.fragment.Count)
                    {
                        parentNode.InsertBefore(this.fragment[num2], this.start);
                        num2++;
                    }
                    return;
                Label_00E9:
                    this.start.PrependChild(this.fragment[num3]);
                    num3--;
                Label_0105:
                    if (num3 >= 0)
                    {
                        goto Label_00E9;
                    }
                    return;
                Label_0110:
                    this.start.AppendChild(this.fragment[num4]);
                    num4++;
                Label_012F:
                    if (num4 < this.fragment.Count)
                    {
                        goto Label_0110;
                    }
                    return;
                Label_0165:
                    this.CloseWithReplaceToFollowingSibling();
                }
                finally
                {
                    this.fragment.Clear();
                }
            }
        }

        private void CloseWithAppendAttribute()
        {
            XmlElement start = this.start as XmlElement;
            XmlAttributeCollection attributes = start.Attributes;
            for (int i = 0; i < this.fragment.Count; i++)
            {
                XmlAttribute node = this.fragment[i] as XmlAttribute;
                int num2 = attributes.FindNodeOffsetNS(node);
                if ((num2 != -1) && ((XmlAttribute) attributes.Nodes[num2]).Specified)
                {
                    throw new XmlException("Xml_DupAttributeName", (node.Prefix.Length == 0) ? node.LocalName : (node.Prefix + ":" + node.LocalName));
                }
            }
            for (int j = 0; j < this.fragment.Count; j++)
            {
                XmlAttribute attribute2 = this.fragment[j] as XmlAttribute;
                attributes.Append(attribute2);
            }
        }

        private void CloseWithReplaceToFollowingSibling()
        {
            XmlNode parentNode = this.start.ParentNode;
            if (parentNode == null)
            {
                throw new InvalidOperationException(Res.GetString("Xpn_MissingParent"));
            }
            if (this.start != this.end)
            {
                if (!DocumentXPathNavigator.IsFollowingSibling(this.start, this.end))
                {
                    throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
                }
                if (this.start.IsReadOnly)
                {
                    throw new InvalidOperationException(Res.GetString("Xdom_Node_Modify_ReadOnly"));
                }
                DocumentXPathNavigator.DeleteToFollowingSibling(this.start.NextSibling, this.end);
            }
            XmlNode newChild = this.fragment[0];
            parentNode.ReplaceChild(newChild, this.start);
            for (int i = this.fragment.Count - 1; i >= 1; i--)
            {
                parentNode.InsertAfter(this.fragment[i], newChild);
            }
            this.navigator.ResetPosition(newChild);
        }

        public override void Flush()
        {
        }

        internal void SetSettings(XmlWriterSettings value)
        {
            this.settings = value;
        }

        internal override void StartElementContent()
        {
        }

        private State StartState()
        {
            XmlNodeType none = XmlNodeType.None;
            switch (this.type)
            {
                case DocumentXmlWriterType.InsertSiblingAfter:
                case DocumentXmlWriterType.InsertSiblingBefore:
                {
                    XmlNode parentNode = this.start.ParentNode;
                    if (parentNode != null)
                    {
                        none = parentNode.NodeType;
                    }
                    if (none == XmlNodeType.Document)
                    {
                        return State.Prolog;
                    }
                    if (none != XmlNodeType.DocumentFragment)
                    {
                        break;
                    }
                    return State.Fragment;
                }
                case DocumentXmlWriterType.PrependChild:
                case DocumentXmlWriterType.AppendChild:
                    none = this.start.NodeType;
                    if (none != XmlNodeType.Document)
                    {
                        if (none == XmlNodeType.DocumentFragment)
                        {
                            return State.Fragment;
                        }
                        break;
                    }
                    return State.Prolog;

                case DocumentXmlWriterType.AppendAttribute:
                    return State.Attribute;
            }
            return State.Content;
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return this.namespaceManager.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix)
        {
            return this.namespaceManager.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            return this.namespaceManager.LookupPrefix(namespaceName);
        }

        private void VerifyState(Method method)
        {
            this.state = changeState[(int) ((method * Method.WriteEndElement) + ((Method) ((int) this.state)))];
            if (this.state == State.Error)
            {
                throw new InvalidOperationException(Res.GetString("Xml_ClosedOrError"));
            }
        }

        public override void WriteCData(string text)
        {
            this.VerifyState(Method.WriteCData);
            XmlConvert.VerifyCharData(text, ExceptionType.ArgumentException);
            XmlNode node = this.document.CreateCDataSection(text);
            this.AddChild(node, this.write);
        }

        public override void WriteCharEntity(char ch)
        {
            this.WriteString(new string(ch, 1));
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.WriteString(new string(buffer, index, count));
        }

        public override void WriteComment(string text)
        {
            this.VerifyState(Method.WriteComment);
            XmlConvert.VerifyCharData(text, ExceptionType.ArgumentException);
            XmlNode node = this.document.CreateComment(text);
            this.AddChild(node, this.write);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this.VerifyState(Method.WriteDocType);
            XmlNode node = this.document.CreateDocumentType(name, pubid, sysid, subset);
            this.AddChild(node, this.write);
        }

        public override void WriteEndAttribute()
        {
            this.VerifyState(Method.WriteEndAttribute);
            XmlAttribute write = this.write as XmlAttribute;
            if (write == null)
            {
                throw new InvalidOperationException();
            }
            if (!write.HasChildNodes)
            {
                XmlNode node = this.document.CreateTextNode(string.Empty);
                this.AddChild(node, write);
            }
            this.write = write.OwnerElement;
        }

        public override void WriteEndDocument()
        {
            this.VerifyState(Method.WriteEndDocument);
        }

        public override void WriteEndElement()
        {
            this.VerifyState(Method.WriteEndElement);
            if (this.write == null)
            {
                throw new InvalidOperationException();
            }
            this.write = this.write.ParentNode;
        }

        internal override void WriteEndElement(string prefix, string localName, string ns)
        {
            this.WriteEndElement();
        }

        internal override void WriteEndNamespaceDeclaration()
        {
            this.VerifyState(Method.WriteEndNamespaceDeclaration);
            XmlAttribute write = this.write as XmlAttribute;
            if (write == null)
            {
                throw new InvalidOperationException();
            }
            if (!write.HasChildNodes)
            {
                XmlNode node = this.document.CreateTextNode(string.Empty);
                this.AddChild(node, write);
            }
            this.write = write.OwnerElement;
        }

        public override void WriteEntityRef(string name)
        {
            this.VerifyState(Method.WriteEntityRef);
            XmlNode node = this.document.CreateEntityReference(name);
            this.AddChild(node, this.write);
        }

        public override void WriteFullEndElement()
        {
            this.VerifyState(Method.WriteFullEndElement);
            XmlElement write = this.write as XmlElement;
            if (write == null)
            {
                throw new InvalidOperationException();
            }
            write.IsEmpty = false;
            this.write = write.ParentNode;
        }

        internal override void WriteFullEndElement(string prefix, string localName, string ns)
        {
            this.WriteFullEndElement();
        }

        internal override void WriteNamespaceDeclaration(string prefix, string ns)
        {
            this.WriteStartNamespaceDeclaration(prefix);
            this.WriteString(ns);
            this.WriteEndNamespaceDeclaration();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.VerifyState(Method.WriteProcessingInstruction);
            XmlConvert.VerifyCharData(text, ExceptionType.ArgumentException);
            XmlNode node = this.document.CreateProcessingInstruction(name, text);
            this.AddChild(node, this.write);
        }

        public override void WriteRaw(string data)
        {
            this.WriteString(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.WriteString(new string(buffer, index, count));
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.VerifyState(Method.WriteStartAttribute);
            XmlAttribute attr = this.document.CreateAttribute(prefix, localName, ns);
            this.AddAttribute(attr, this.write);
            this.write = attr;
        }

        public override void WriteStartDocument()
        {
            this.VerifyState(Method.WriteStartDocument);
        }

        public override void WriteStartDocument(bool standalone)
        {
            this.VerifyState(Method.WriteStartDocument);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.VerifyState(Method.WriteStartElement);
            XmlNode node = this.document.CreateElement(prefix, localName, ns);
            this.AddChild(node, this.write);
            this.write = node;
        }

        internal override void WriteStartNamespaceDeclaration(string prefix)
        {
            XmlAttribute attribute;
            this.VerifyState(Method.WriteStartNamespaceDeclaration);
            if (prefix.Length == 0)
            {
                attribute = this.document.CreateAttribute(prefix, this.document.strXmlns, this.document.strReservedXmlns);
            }
            else
            {
                attribute = this.document.CreateAttribute(this.document.strXmlns, prefix, this.document.strReservedXmlns);
            }
            this.AddAttribute(attribute, this.write);
            this.write = attribute;
        }

        public override void WriteString(string text)
        {
            this.VerifyState(Method.WriteString);
            XmlConvert.VerifyCharData(text, ExceptionType.ArgumentException);
            XmlNode node = this.document.CreateTextNode(text);
            this.AddChild(node, this.write);
        }

        public override void WriteSurrogateCharEntity(char lowCh, char highCh)
        {
            this.WriteString(new string(new char[] { highCh, lowCh }));
        }

        public override void WriteWhitespace(string text)
        {
            this.VerifyState(Method.WriteWhitespace);
            XmlConvert.VerifyCharData(text, ExceptionType.ArgumentException);
            if (this.document.PreserveWhitespace)
            {
                XmlNode node = this.document.CreateWhitespace(text);
                this.AddChild(node, this.write);
            }
        }

        internal override void WriteXmlDeclaration(string xmldecl)
        {
            string str;
            string str2;
            string str3;
            this.VerifyState(Method.WriteXmlDeclaration);
            XmlLoader.ParseXmlDeclarationValue(xmldecl, out str, out str2, out str3);
            XmlNode node = this.document.CreateXmlDeclaration(str, str2, str3);
            this.AddChild(node, this.write);
        }

        internal override void WriteXmlDeclaration(XmlStandalone standalone)
        {
            this.VerifyState(Method.WriteXmlDeclaration);
            if (standalone != XmlStandalone.Omit)
            {
                XmlNode node = this.document.CreateXmlDeclaration("1.0", string.Empty, (standalone == XmlStandalone.Yes) ? "yes" : "no");
                this.AddChild(node, this.write);
            }
        }

        public XmlNode EndNode
        {
            set
            {
                this.end = value;
            }
        }

        public XmlNamespaceManager NamespaceManager
        {
            set
            {
                this.namespaceManager = value;
            }
        }

        public DocumentXPathNavigator Navigator
        {
            set
            {
                this.navigator = value;
            }
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                return this.settings;
            }
        }

        internal override bool SupportsNamespaceDeclarationInChunks
        {
            get
            {
                return true;
            }
        }

        private enum Method
        {
            WriteXmlDeclaration,
            WriteStartDocument,
            WriteEndDocument,
            WriteDocType,
            WriteStartElement,
            WriteEndElement,
            WriteFullEndElement,
            WriteStartAttribute,
            WriteEndAttribute,
            WriteStartNamespaceDeclaration,
            WriteEndNamespaceDeclaration,
            WriteCData,
            WriteComment,
            WriteProcessingInstruction,
            WriteEntityRef,
            WriteWhitespace,
            WriteString
        }

        private enum State
        {
            Error,
            Attribute,
            Prolog,
            Fragment,
            Content,
            Last
        }
    }
}

