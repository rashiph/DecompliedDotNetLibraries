namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml.Xsl.Runtime;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public sealed class XmlWriterSettings
    {
        private bool autoXmlDecl;
        private List<XmlQualifiedName> cdataSections;
        private bool checkCharacters;
        private bool closeOutput;
        private System.Xml.ConformanceLevel conformanceLevel;
        private string docTypePublic;
        private string docTypeSystem;
        private System.Text.Encoding encoding;
        private TriState indent;
        private string indentChars;
        private bool isReadOnly;
        private string mediaType;
        private bool mergeCDataSections;
        private System.Xml.NamespaceHandling namespaceHandling;
        private string newLineChars;
        private System.Xml.NewLineHandling newLineHandling;
        private bool newLineOnAttributes;
        private bool omitXmlDecl;
        private XmlOutputMethod outputMethod;
        private XmlStandalone standalone;

        public XmlWriterSettings()
        {
            this.cdataSections = new List<XmlQualifiedName>();
            this.Initialize();
        }

        internal XmlWriterSettings(XmlQueryDataReader reader)
        {
            this.cdataSections = new List<XmlQualifiedName>();
            this.Encoding = System.Text.Encoding.GetEncoding(reader.ReadInt32());
            this.OmitXmlDeclaration = reader.ReadBoolean();
            this.NewLineHandling = (System.Xml.NewLineHandling) reader.ReadSByte(0, 2);
            this.NewLineChars = reader.ReadStringQ();
            this.IndentInternal = (TriState) reader.ReadSByte(-1, 1);
            this.IndentChars = reader.ReadStringQ();
            this.NewLineOnAttributes = reader.ReadBoolean();
            this.CloseOutput = reader.ReadBoolean();
            this.ConformanceLevel = (System.Xml.ConformanceLevel) reader.ReadSByte(0, 2);
            this.CheckCharacters = reader.ReadBoolean();
            this.outputMethod = (XmlOutputMethod) reader.ReadSByte(0, 3);
            int capacity = reader.ReadInt32();
            this.cdataSections = new List<XmlQualifiedName>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                this.cdataSections.Add(new XmlQualifiedName(reader.ReadString(), reader.ReadString()));
            }
            this.mergeCDataSections = reader.ReadBoolean();
            this.mediaType = reader.ReadStringQ();
            this.docTypeSystem = reader.ReadStringQ();
            this.docTypePublic = reader.ReadStringQ();
            this.Standalone = (XmlStandalone) reader.ReadSByte(0, 2);
            this.autoXmlDecl = reader.ReadBoolean();
            this.ReadOnly = reader.ReadBoolean();
        }

        private XmlWriter AddConformanceWrapper(XmlWriter baseWriter)
        {
            System.Xml.ConformanceLevel auto = System.Xml.ConformanceLevel.Auto;
            XmlWriterSettings settings = baseWriter.Settings;
            bool checkValues = false;
            bool checkNames = false;
            bool replaceNewLines = false;
            bool flag4 = false;
            if (settings == null)
            {
                if (this.newLineHandling == System.Xml.NewLineHandling.Replace)
                {
                    replaceNewLines = true;
                    flag4 = true;
                }
                if (this.checkCharacters)
                {
                    checkValues = true;
                    flag4 = true;
                }
            }
            else
            {
                if (this.conformanceLevel != settings.ConformanceLevel)
                {
                    auto = this.ConformanceLevel;
                    flag4 = true;
                }
                if (this.checkCharacters && !settings.CheckCharacters)
                {
                    checkValues = true;
                    checkNames = auto == System.Xml.ConformanceLevel.Auto;
                    flag4 = true;
                }
                if ((this.newLineHandling == System.Xml.NewLineHandling.Replace) && (settings.NewLineHandling == System.Xml.NewLineHandling.None))
                {
                    replaceNewLines = true;
                    flag4 = true;
                }
            }
            XmlWriter writer = baseWriter;
            if (flag4)
            {
                if (auto != System.Xml.ConformanceLevel.Auto)
                {
                    writer = new XmlWellFormedWriter(writer, this);
                }
                if (checkValues || replaceNewLines)
                {
                    writer = new XmlCharCheckingWriter(writer, checkValues, checkNames, replaceNewLines, this.NewLineChars);
                }
            }
            if (!this.IsQuerySpecific || ((settings != null) && settings.IsQuerySpecific))
            {
                return writer;
            }
            return new QueryOutputWriterV1(writer, this);
        }

        private void CheckReadOnly(string propertyName)
        {
            if (this.isReadOnly)
            {
                throw new XmlException("Xml_ReadOnlyProperty", base.GetType().Name + '.' + propertyName);
            }
        }

        public XmlWriterSettings Clone()
        {
            XmlWriterSettings settings = base.MemberwiseClone() as XmlWriterSettings;
            settings.cdataSections = new List<XmlQualifiedName>(this.cdataSections);
            settings.isReadOnly = false;
            return settings;
        }

        internal XmlWriter CreateWriter(Stream output)
        {
            XmlWriter writer;
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            if (!(this.Encoding.WebName == "utf-8"))
            {
                switch (this.OutputMethod)
                {
                    case XmlOutputMethod.Xml:
                        if (!this.Indent)
                        {
                            writer = new XmlEncodedRawTextWriter(output, this);
                        }
                        else
                        {
                            writer = new XmlEncodedRawTextWriterIndent(output, this);
                        }
                        goto Label_010B;

                    case XmlOutputMethod.Html:
                        if (!this.Indent)
                        {
                            writer = new HtmlEncodedRawTextWriter(output, this);
                        }
                        else
                        {
                            writer = new HtmlEncodedRawTextWriterIndent(output, this);
                        }
                        goto Label_010B;

                    case XmlOutputMethod.Text:
                        writer = new TextEncodedRawTextWriter(output, this);
                        goto Label_010B;

                    case XmlOutputMethod.AutoDetect:
                        writer = new XmlAutoDetectWriter(output, this);
                        goto Label_010B;
                }
                return null;
            }
            switch (this.OutputMethod)
            {
                case XmlOutputMethod.Xml:
                    if (!this.Indent)
                    {
                        writer = new XmlUtf8RawTextWriter(output, this);
                        break;
                    }
                    writer = new XmlUtf8RawTextWriterIndent(output, this);
                    break;

                case XmlOutputMethod.Html:
                    if (!this.Indent)
                    {
                        writer = new HtmlUtf8RawTextWriter(output, this);
                        break;
                    }
                    writer = new HtmlUtf8RawTextWriterIndent(output, this);
                    break;

                case XmlOutputMethod.Text:
                    writer = new TextUtf8RawTextWriter(output, this);
                    break;

                case XmlOutputMethod.AutoDetect:
                    writer = new XmlAutoDetectWriter(output, this);
                    break;

                default:
                    return null;
            }
        Label_010B:
            if ((this.OutputMethod != XmlOutputMethod.AutoDetect) && this.IsQuerySpecific)
            {
                writer = new QueryOutputWriter((XmlRawWriter) writer, this);
            }
            return new XmlWellFormedWriter(writer, this);
        }

        internal XmlWriter CreateWriter(TextWriter output)
        {
            XmlWriter writer;
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            switch (this.OutputMethod)
            {
                case XmlOutputMethod.Xml:
                    if (!this.Indent)
                    {
                        writer = new XmlEncodedRawTextWriter(output, this);
                        break;
                    }
                    writer = new XmlEncodedRawTextWriterIndent(output, this);
                    break;

                case XmlOutputMethod.Html:
                    if (!this.Indent)
                    {
                        writer = new HtmlEncodedRawTextWriter(output, this);
                        break;
                    }
                    writer = new HtmlEncodedRawTextWriterIndent(output, this);
                    break;

                case XmlOutputMethod.Text:
                    writer = new TextEncodedRawTextWriter(output, this);
                    break;

                case XmlOutputMethod.AutoDetect:
                    writer = new XmlAutoDetectWriter(output, this);
                    break;

                default:
                    return null;
            }
            if ((this.OutputMethod != XmlOutputMethod.AutoDetect) && this.IsQuerySpecific)
            {
                writer = new QueryOutputWriter((XmlRawWriter) writer, this);
            }
            return new XmlWellFormedWriter(writer, this);
        }

        internal XmlWriter CreateWriter(string outputFileName)
        {
            XmlWriter writer;
            if (outputFileName == null)
            {
                throw new ArgumentNullException("outputFileName");
            }
            XmlWriterSettings settings = this;
            if (!settings.CloseOutput)
            {
                settings = settings.Clone();
                settings.CloseOutput = true;
            }
            FileStream output = null;
            try
            {
                output = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                writer = settings.CreateWriter(output);
            }
            catch
            {
                if (output != null)
                {
                    output.Close();
                }
                throw;
            }
            return writer;
        }

        internal XmlWriter CreateWriter(XmlWriter output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            return this.AddConformanceWrapper(output);
        }

        internal void GetObjectData(XmlQueryDataWriter writer)
        {
            writer.Write(this.Encoding.CodePage);
            writer.Write(this.OmitXmlDeclaration);
            writer.Write((sbyte) this.NewLineHandling);
            writer.WriteStringQ(this.NewLineChars);
            writer.Write((sbyte) this.IndentInternal);
            writer.WriteStringQ(this.IndentChars);
            writer.Write(this.NewLineOnAttributes);
            writer.Write(this.CloseOutput);
            writer.Write((sbyte) this.ConformanceLevel);
            writer.Write(this.CheckCharacters);
            writer.Write((sbyte) this.outputMethod);
            writer.Write(this.cdataSections.Count);
            foreach (XmlQualifiedName name in this.cdataSections)
            {
                writer.Write(name.Name);
                writer.Write(name.Namespace);
            }
            writer.Write(this.mergeCDataSections);
            writer.WriteStringQ(this.mediaType);
            writer.WriteStringQ(this.docTypeSystem);
            writer.WriteStringQ(this.docTypePublic);
            writer.Write((sbyte) this.standalone);
            writer.Write(this.autoXmlDecl);
            writer.Write(this.ReadOnly);
        }

        private void Initialize()
        {
            this.encoding = System.Text.Encoding.UTF8;
            this.omitXmlDecl = false;
            this.newLineHandling = System.Xml.NewLineHandling.Replace;
            this.newLineChars = Environment.NewLine;
            this.indent = TriState.Unknown;
            this.indentChars = "  ";
            this.newLineOnAttributes = false;
            this.closeOutput = false;
            this.namespaceHandling = System.Xml.NamespaceHandling.Default;
            this.conformanceLevel = System.Xml.ConformanceLevel.Document;
            this.checkCharacters = true;
            this.outputMethod = XmlOutputMethod.Xml;
            this.cdataSections.Clear();
            this.mergeCDataSections = false;
            this.mediaType = null;
            this.docTypeSystem = null;
            this.docTypePublic = null;
            this.standalone = XmlStandalone.Omit;
            this.isReadOnly = false;
        }

        public void Reset()
        {
            this.CheckReadOnly("Reset");
            this.Initialize();
        }

        internal bool AutoXmlDeclaration
        {
            get
            {
                return this.autoXmlDecl;
            }
            set
            {
                this.CheckReadOnly("AutoXmlDeclaration");
                this.autoXmlDecl = value;
            }
        }

        internal List<XmlQualifiedName> CDataSectionElements
        {
            get
            {
                return this.cdataSections;
            }
        }

        public bool CheckCharacters
        {
            get
            {
                return this.checkCharacters;
            }
            set
            {
                this.CheckReadOnly("CheckCharacters");
                this.checkCharacters = value;
            }
        }

        public bool CloseOutput
        {
            get
            {
                return this.closeOutput;
            }
            set
            {
                this.CheckReadOnly("CloseOutput");
                this.closeOutput = value;
            }
        }

        public System.Xml.ConformanceLevel ConformanceLevel
        {
            get
            {
                return this.conformanceLevel;
            }
            set
            {
                this.CheckReadOnly("ConformanceLevel");
                if (value > System.Xml.ConformanceLevel.Document)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.conformanceLevel = value;
            }
        }

        internal string DocTypePublic
        {
            get
            {
                return this.docTypePublic;
            }
            set
            {
                this.CheckReadOnly("DocTypePublic");
                this.docTypePublic = value;
            }
        }

        internal string DocTypeSystem
        {
            get
            {
                return this.docTypeSystem;
            }
            set
            {
                this.CheckReadOnly("DocTypeSystem");
                this.docTypeSystem = value;
            }
        }

        public System.Text.Encoding Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                this.CheckReadOnly("Encoding");
                this.encoding = value;
            }
        }

        public bool Indent
        {
            get
            {
                return (this.indent == TriState.True);
            }
            set
            {
                this.CheckReadOnly("Indent");
                this.indent = value ? TriState.True : TriState.False;
            }
        }

        public string IndentChars
        {
            get
            {
                return this.indentChars;
            }
            set
            {
                this.CheckReadOnly("IndentChars");
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.indentChars = value;
            }
        }

        internal TriState IndentInternal
        {
            get
            {
                return this.indent;
            }
            set
            {
                this.indent = value;
            }
        }

        internal bool IsQuerySpecific
        {
            get
            {
                if (((this.cdataSections.Count == 0) && (this.docTypePublic == null)) && (this.docTypeSystem == null))
                {
                    return (this.standalone == XmlStandalone.Yes);
                }
                return true;
            }
        }

        internal string MediaType
        {
            get
            {
                return this.mediaType;
            }
            set
            {
                this.CheckReadOnly("MediaType");
                this.mediaType = value;
            }
        }

        internal bool MergeCDataSections
        {
            get
            {
                return this.mergeCDataSections;
            }
            set
            {
                this.CheckReadOnly("MergeCDataSections");
                this.mergeCDataSections = value;
            }
        }

        public System.Xml.NamespaceHandling NamespaceHandling
        {
            get
            {
                return this.namespaceHandling;
            }
            set
            {
                this.CheckReadOnly("NamespaceHandling");
                if (value > System.Xml.NamespaceHandling.OmitDuplicates)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.namespaceHandling = value;
            }
        }

        public string NewLineChars
        {
            get
            {
                return this.newLineChars;
            }
            set
            {
                this.CheckReadOnly("NewLineChars");
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.newLineChars = value;
            }
        }

        public System.Xml.NewLineHandling NewLineHandling
        {
            get
            {
                return this.newLineHandling;
            }
            set
            {
                this.CheckReadOnly("NewLineHandling");
                if (value > System.Xml.NewLineHandling.None)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.newLineHandling = value;
            }
        }

        public bool NewLineOnAttributes
        {
            get
            {
                return this.newLineOnAttributes;
            }
            set
            {
                this.CheckReadOnly("NewLineOnAttributes");
                this.newLineOnAttributes = value;
            }
        }

        public bool OmitXmlDeclaration
        {
            get
            {
                return this.omitXmlDecl;
            }
            set
            {
                this.CheckReadOnly("OmitXmlDeclaration");
                this.omitXmlDecl = value;
            }
        }

        public XmlOutputMethod OutputMethod
        {
            get
            {
                return this.outputMethod;
            }
            internal set
            {
                this.outputMethod = value;
            }
        }

        internal bool ReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
            set
            {
                this.isReadOnly = value;
            }
        }

        internal XmlStandalone Standalone
        {
            get
            {
                return this.standalone;
            }
            set
            {
                this.CheckReadOnly("Standalone");
                this.standalone = value;
            }
        }
    }
}

