namespace System.Xml.Linq
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Text;
    using System.Xml;

    public class XDocument : XContainer
    {
        private XDeclaration declaration;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XDocument()
        {
        }

        public XDocument(params object[] content) : this()
        {
            base.AddContentSkipNotify(content);
        }

        public XDocument(XDocument other) : base(other)
        {
            if (other.declaration != null)
            {
                this.declaration = new XDeclaration(other.declaration);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XDocument(XDeclaration declaration, params object[] content) : this(content)
        {
            this.declaration = declaration;
        }

        internal override void AddAttribute(XAttribute a)
        {
            throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_AddAttribute"));
        }

        internal override void AddAttributeSkipNotify(XAttribute a)
        {
            throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_AddAttribute"));
        }

        internal override XNode CloneNode()
        {
            return new XDocument(this);
        }

        internal override bool DeepEquals(XNode node)
        {
            XDocument e = node as XDocument;
            return ((e != null) && base.ContentsEqual(e));
        }

        internal override int GetDeepHashCode()
        {
            return base.ContentsHashCode();
        }

        private T GetFirstNode<T>() where T: XNode
        {
            XNode content = base.content as XNode;
            if (content != null)
            {
                do
                {
                    content = content.next;
                    T local = content as T;
                    if (local != null)
                    {
                        return local;
                    }
                }
                while (content != base.content);
            }
            return default(T);
        }

        internal static bool IsWhitespace(string s)
        {
            foreach (char ch in s)
            {
                if (((ch != ' ') && (ch != '\t')) && ((ch != '\r') && (ch != '\n')))
                {
                    return false;
                }
            }
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XDocument Load(Stream stream)
        {
            return Load(stream, LoadOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XDocument Load(TextReader textReader)
        {
            return Load(textReader, LoadOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XDocument Load(string uri)
        {
            return Load(uri, LoadOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XDocument Load(XmlReader reader)
        {
            return Load(reader, LoadOptions.None);
        }

        public static XDocument Load(Stream stream, LoadOptions options)
        {
            XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
            using (XmlReader reader = XmlReader.Create(stream, xmlReaderSettings))
            {
                return Load(reader, options);
            }
        }

        public static XDocument Load(TextReader textReader, LoadOptions options)
        {
            XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
            using (XmlReader reader = XmlReader.Create(textReader, xmlReaderSettings))
            {
                return Load(reader, options);
            }
        }

        public static XDocument Load(string uri, LoadOptions options)
        {
            XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
            using (XmlReader reader = XmlReader.Create(uri, xmlReaderSettings))
            {
                return Load(reader, options);
            }
        }

        public static XDocument Load(XmlReader reader, LoadOptions options)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.ReadState == System.Xml.ReadState.Initial)
            {
                reader.Read();
            }
            XDocument document = new XDocument();
            if ((options & LoadOptions.SetBaseUri) != LoadOptions.None)
            {
                string baseURI = reader.BaseURI;
                if ((baseURI != null) && (baseURI.Length != 0))
                {
                    document.SetBaseUri(baseURI);
                }
            }
            if ((options & LoadOptions.SetLineInfo) != LoadOptions.None)
            {
                IXmlLineInfo info = reader as IXmlLineInfo;
                if ((info != null) && info.HasLineInfo())
                {
                    document.SetLineInfo(info.LineNumber, info.LinePosition);
                }
            }
            if (reader.NodeType == XmlNodeType.XmlDeclaration)
            {
                document.Declaration = new XDeclaration(reader);
            }
            document.ReadContentFrom(reader, options);
            if (!reader.EOF)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExpectedEndOfFile"));
            }
            if (document.Root == null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_MissingRoot"));
            }
            return document;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XDocument Parse(string text)
        {
            return Parse(text, LoadOptions.None);
        }

        public static XDocument Parse(string text, LoadOptions options)
        {
            XDocument document;
            using (StringReader reader = new StringReader(text))
            {
                XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
                using (XmlReader reader2 = XmlReader.Create(reader, xmlReaderSettings))
                {
                    document = Load(reader2, options);
                }
            }
            return document;
        }

        public void Save(Stream stream)
        {
            this.Save(stream, base.GetSaveOptionsFromAnnotations());
        }

        public void Save(TextWriter textWriter)
        {
            this.Save(textWriter, base.GetSaveOptionsFromAnnotations());
        }

        public void Save(string fileName)
        {
            this.Save(fileName, base.GetSaveOptionsFromAnnotations());
        }

        public void Save(XmlWriter writer)
        {
            this.WriteTo(writer);
        }

        public void Save(Stream stream, SaveOptions options)
        {
            XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
            if ((this.declaration != null) && !string.IsNullOrEmpty(this.declaration.Encoding))
            {
                try
                {
                    xmlWriterSettings.Encoding = Encoding.GetEncoding(this.declaration.Encoding);
                }
                catch (ArgumentException)
                {
                }
            }
            using (XmlWriter writer = XmlWriter.Create(stream, xmlWriterSettings))
            {
                this.Save(writer);
            }
        }

        public void Save(TextWriter textWriter, SaveOptions options)
        {
            XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
            using (XmlWriter writer = XmlWriter.Create(textWriter, xmlWriterSettings))
            {
                this.Save(writer);
            }
        }

        public void Save(string fileName, SaveOptions options)
        {
            XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
            if ((this.declaration != null) && !string.IsNullOrEmpty(this.declaration.Encoding))
            {
                try
                {
                    xmlWriterSettings.Encoding = Encoding.GetEncoding(this.declaration.Encoding);
                }
                catch (ArgumentException)
                {
                }
            }
            using (XmlWriter writer = XmlWriter.Create(fileName, xmlWriterSettings))
            {
                this.Save(writer);
            }
        }

        private void ValidateDocument(XNode previous, XmlNodeType allowBefore, XmlNodeType allowAfter)
        {
            XNode content = base.content as XNode;
            if (content != null)
            {
                if (previous == null)
                {
                    allowBefore = allowAfter;
                }
                do
                {
                    content = content.next;
                    XmlNodeType nodeType = content.NodeType;
                    switch (nodeType)
                    {
                        case XmlNodeType.Element:
                        case XmlNodeType.DocumentType:
                            if (nodeType != allowBefore)
                            {
                                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_DocumentStructure"));
                            }
                            allowBefore = XmlNodeType.None;
                            break;
                    }
                    if (content == previous)
                    {
                        allowBefore = allowAfter;
                    }
                }
                while (content != base.content);
            }
        }

        internal override void ValidateNode(XNode node, XNode previous)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    this.ValidateDocument(previous, XmlNodeType.DocumentType, XmlNodeType.None);
                    return;

                case XmlNodeType.Attribute:
                    return;

                case XmlNodeType.Text:
                    this.ValidateString(((XText) node).Value);
                    return;

                case XmlNodeType.CDATA:
                    throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_AddNode", new object[] { XmlNodeType.CDATA }));

                case XmlNodeType.Document:
                    throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_AddNode", new object[] { XmlNodeType.Document }));

                case XmlNodeType.DocumentType:
                    this.ValidateDocument(previous, XmlNodeType.None, XmlNodeType.Element);
                    return;
            }
        }

        internal override void ValidateString(string s)
        {
            if (!IsWhitespace(s))
            {
                throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_AddNonWhitespace"));
            }
        }

        public override void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if ((this.declaration != null) && (this.declaration.Standalone == "yes"))
            {
                writer.WriteStartDocument(true);
            }
            else if ((this.declaration != null) && (this.declaration.Standalone == "no"))
            {
                writer.WriteStartDocument(false);
            }
            else
            {
                writer.WriteStartDocument();
            }
            base.WriteContentTo(writer);
            writer.WriteEndDocument();
        }

        public XDeclaration Declaration
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.declaration;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.declaration = value;
            }
        }

        public XDocumentType DocumentType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetFirstNode<XDocumentType>();
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.Document;
            }
        }

        public XElement Root
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetFirstNode<XElement>();
            }
        }
    }
}

