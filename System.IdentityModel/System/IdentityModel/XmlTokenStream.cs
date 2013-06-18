namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class XmlTokenStream : ISecurityElement
    {
        private int count;
        private XmlTokenEntry[] entries;
        private string excludedElement;
        private int? excludedElementDepth;
        private string excludedElementNamespace;

        public XmlTokenStream(int initialSize)
        {
            if (initialSize < 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("initialSize", System.IdentityModel.SR.GetString("ValueMustBeGreaterThanZero")));
            }
            this.entries = new XmlTokenEntry[initialSize];
        }

        public void Add(XmlNodeType type, string value)
        {
            this.EnsureCapacityToAdd();
            this.entries[this.count++].Set(type, value);
        }

        public void AddAttribute(string prefix, string localName, string namespaceUri, string value)
        {
            this.EnsureCapacityToAdd();
            this.entries[this.count++].SetAttribute(prefix, localName, namespaceUri, value);
        }

        public void AddElement(string prefix, string localName, string namespaceUri, bool isEmptyElement)
        {
            this.EnsureCapacityToAdd();
            this.entries[this.count++].SetElement(prefix, localName, namespaceUri, isEmptyElement);
        }

        private void EnsureCapacityToAdd()
        {
            if (this.count == this.entries.Length)
            {
                XmlTokenEntry[] destinationArray = new XmlTokenEntry[this.entries.Length * 2];
                Array.Copy(this.entries, 0, destinationArray, 0, this.count);
                this.entries = destinationArray;
            }
        }

        public XmlTokenStreamWriter GetWriter()
        {
            return new XmlTokenStreamWriter(this.entries, this.count, this.excludedElement, this.excludedElementDepth, this.excludedElementNamespace);
        }

        public void SetElementExclusion(string excludedElement, string excludedElementNamespace)
        {
            this.SetElementExclusion(excludedElement, excludedElementNamespace, null);
        }

        public void SetElementExclusion(string excludedElement, string excludedElementNamespace, int? excludedElementDepth)
        {
            this.excludedElement = excludedElement;
            this.excludedElementDepth = excludedElementDepth;
            this.excludedElementNamespace = excludedElementNamespace;
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            this.GetWriter().WriteTo(writer, dictionaryManager);
        }

        bool ISecurityElement.HasId
        {
            get
            {
                return false;
            }
        }

        string ISecurityElement.Id
        {
            get
            {
                return null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XmlTokenEntry
        {
            internal XmlNodeType nodeType;
            internal string prefix;
            internal string localName;
            internal string namespaceUri;
            private string value;
            public bool IsEmptyElement
            {
                get
                {
                    return (this.value == null);
                }
                set
                {
                    this.value = value ? null : "";
                }
            }
            public string Value
            {
                get
                {
                    return this.value;
                }
            }
            public void Set(XmlNodeType nodeType, string value)
            {
                this.nodeType = nodeType;
                this.value = value;
            }

            public void SetAttribute(string prefix, string localName, string namespaceUri, string value)
            {
                this.nodeType = XmlNodeType.Attribute;
                this.prefix = prefix;
                this.localName = localName;
                this.namespaceUri = namespaceUri;
                this.value = value;
            }

            public void SetElement(string prefix, string localName, string namespaceUri, bool isEmptyElement)
            {
                this.nodeType = XmlNodeType.Element;
                this.prefix = prefix;
                this.localName = localName;
                this.namespaceUri = namespaceUri;
                this.IsEmptyElement = isEmptyElement;
            }
        }

        internal class XmlTokenStreamWriter : ISecurityElement
        {
            private int count;
            private XmlTokenStream.XmlTokenEntry[] entries;
            private string excludedElement;
            private int? excludedElementDepth;
            private string excludedElementNamespace;
            private int position;

            public XmlTokenStreamWriter(XmlTokenStream.XmlTokenEntry[] entries, int count, string excludedElement, int? excludedElementDepth, string excludedElementNamespace)
            {
                if (entries == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("entries");
                }
                this.entries = entries;
                this.count = count;
                this.excludedElement = excludedElement;
                this.excludedElementDepth = excludedElementDepth;
                this.excludedElementNamespace = excludedElementNamespace;
            }

            public bool MoveToFirst()
            {
                this.position = 0;
                return (this.count > 0);
            }

            public bool MoveToFirstAttribute()
            {
                if ((this.position < (this.Count - 1)) && (this.entries[this.position + 1].nodeType == XmlNodeType.Attribute))
                {
                    this.position++;
                    return true;
                }
                return false;
            }

            public bool MoveToNext()
            {
                if (this.position < (this.count - 1))
                {
                    this.position++;
                    return true;
                }
                return false;
            }

            public bool MoveToNextAttribute()
            {
                if ((this.position < (this.count - 1)) && (this.entries[this.position + 1].nodeType == XmlNodeType.Attribute))
                {
                    this.position++;
                    return true;
                }
                return false;
            }

            public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
            {
                bool isEmptyElement;
                if (writer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
                }
                if (!this.MoveToFirst())
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("XmlTokenBufferIsEmpty")));
                }
                int num = 0;
                int num2 = -1;
                bool flag = true;
            Label_0040:
                switch (this.NodeType)
                {
                    case XmlNodeType.Element:
                        isEmptyElement = this.IsEmptyElement;
                        num++;
                        if (flag)
                        {
                            if (this.excludedElementDepth.HasValue)
                            {
                                int? excludedElementDepth = this.excludedElementDepth;
                                int num3 = num - 1;
                                if (!((excludedElementDepth.GetValueOrDefault() == num3) && excludedElementDepth.HasValue))
                                {
                                    break;
                                }
                            }
                            if ((this.LocalName == this.excludedElement) && (this.NamespaceUri == this.excludedElementNamespace))
                            {
                                flag = false;
                                num2 = num;
                            }
                        }
                        break;

                    case XmlNodeType.Text:
                        if (flag)
                        {
                            writer.WriteString(this.Value);
                        }
                        goto Label_01AD;

                    case XmlNodeType.CDATA:
                        if (flag)
                        {
                            writer.WriteCData(this.Value);
                        }
                        goto Label_01AD;

                    case XmlNodeType.Comment:
                        if (flag)
                        {
                            writer.WriteComment(this.Value);
                        }
                        goto Label_01AD;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        if (flag)
                        {
                            writer.WriteWhitespace(this.Value);
                        }
                        goto Label_01AD;

                    case XmlNodeType.EndElement:
                        goto Label_0152;

                    default:
                        goto Label_01AD;
                }
                if (flag)
                {
                    writer.WriteStartElement(this.Prefix, this.LocalName, this.NamespaceUri);
                }
                if (this.MoveToFirstAttribute())
                {
                    do
                    {
                        if (flag)
                        {
                            writer.WriteAttributeString(this.Prefix, this.LocalName, this.NamespaceUri, this.Value);
                        }
                    }
                    while (this.MoveToNextAttribute());
                }
                if (!isEmptyElement)
                {
                    goto Label_01AD;
                }
            Label_0152:
                if (flag)
                {
                    writer.WriteEndElement();
                }
                else if (num2 == num)
                {
                    flag = true;
                    num2 = -1;
                }
                num--;
            Label_01AD:
                if (this.MoveToNext())
                {
                    goto Label_0040;
                }
            }

            public int Count
            {
                get
                {
                    return this.count;
                }
            }

            public string ExcludedElement
            {
                get
                {
                    return this.excludedElement;
                }
            }

            public string ExcludedElementNamespace
            {
                get
                {
                    return this.excludedElementNamespace;
                }
            }

            public bool IsEmptyElement
            {
                get
                {
                    return this.entries[this.position].IsEmptyElement;
                }
            }

            public string LocalName
            {
                get
                {
                    return this.entries[this.position].localName;
                }
            }

            public string NamespaceUri
            {
                get
                {
                    return this.entries[this.position].namespaceUri;
                }
            }

            public XmlNodeType NodeType
            {
                get
                {
                    return this.entries[this.position].nodeType;
                }
            }

            public int Position
            {
                get
                {
                    return this.position;
                }
            }

            public string Prefix
            {
                get
                {
                    return this.entries[this.position].prefix;
                }
            }

            bool ISecurityElement.HasId
            {
                get
                {
                    return false;
                }
            }

            string ISecurityElement.Id
            {
                get
                {
                    return null;
                }
            }

            public string Value
            {
                get
                {
                    return this.entries[this.position].Value;
                }
            }
        }
    }
}

