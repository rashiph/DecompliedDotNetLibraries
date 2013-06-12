namespace System.Xml
{
    using System;
    using System.IO;
    using System.Security.Permissions;
    using System.Xml.Schema;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public sealed class XmlReaderSettings
    {
        private bool checkCharacters;
        private bool closeInput;
        private System.Xml.ConformanceLevel conformanceLevel;
        private System.Xml.DtdProcessing dtdProcessing;
        private bool ignoreComments;
        private bool ignorePIs;
        private bool ignoreWhitespace;
        private bool isReadOnly;
        private int lineNumberOffset;
        private int linePositionOffset;
        private long maxCharactersFromEntities;
        private long maxCharactersInDocument;
        private XmlNameTable nameTable;
        private XmlSchemaSet schemas;
        private System.Xml.Schema.ValidationEventHandler valEventHandler;
        private XmlSchemaValidationFlags validationFlags;
        private System.Xml.ValidationType validationType;
        private System.Xml.XmlResolver xmlResolver;

        public event System.Xml.Schema.ValidationEventHandler ValidationEventHandler
        {
            add
            {
                this.CheckReadOnly("ValidationEventHandler");
                this.valEventHandler = (System.Xml.Schema.ValidationEventHandler) Delegate.Combine(this.valEventHandler, value);
            }
            remove
            {
                this.CheckReadOnly("ValidationEventHandler");
                this.valEventHandler = (System.Xml.Schema.ValidationEventHandler) Delegate.Remove(this.valEventHandler, value);
            }
        }

        public XmlReaderSettings()
        {
            this.Initialize();
        }

        internal XmlReader AddConformanceWrapper(XmlReader baseReader)
        {
            XmlReaderSettings settings = baseReader.Settings;
            bool checkCharacters = false;
            bool ignoreWhitespace = false;
            bool ignoreComments = false;
            bool ignorePis = false;
            System.Xml.DtdProcessing dtdProcessing = ~System.Xml.DtdProcessing.Prohibit;
            bool flag5 = false;
            if (settings == null)
            {
                if ((this.conformanceLevel != System.Xml.ConformanceLevel.Auto) && (this.conformanceLevel != XmlReader.GetV1ConformanceLevel(baseReader)))
                {
                    throw new InvalidOperationException(Res.GetString("Xml_IncompatibleConformanceLevel", new object[] { this.conformanceLevel.ToString() }));
                }
                XmlTextReader reader = baseReader as XmlTextReader;
                if (reader == null)
                {
                    XmlValidatingReader reader2 = baseReader as XmlValidatingReader;
                    if (reader2 != null)
                    {
                        reader = (XmlTextReader) reader2.Reader;
                    }
                }
                if (this.ignoreWhitespace)
                {
                    WhitespaceHandling all = WhitespaceHandling.All;
                    if (reader != null)
                    {
                        all = reader.WhitespaceHandling;
                    }
                    if (all == WhitespaceHandling.All)
                    {
                        ignoreWhitespace = true;
                        flag5 = true;
                    }
                }
                if (this.ignoreComments)
                {
                    ignoreComments = true;
                    flag5 = true;
                }
                if (this.ignorePIs)
                {
                    ignorePis = true;
                    flag5 = true;
                }
                System.Xml.DtdProcessing parse = System.Xml.DtdProcessing.Parse;
                if (reader != null)
                {
                    parse = reader.DtdProcessing;
                }
                if (((this.dtdProcessing == System.Xml.DtdProcessing.Prohibit) && (parse != System.Xml.DtdProcessing.Prohibit)) || ((this.dtdProcessing == System.Xml.DtdProcessing.Ignore) && (parse == System.Xml.DtdProcessing.Parse)))
                {
                    dtdProcessing = this.dtdProcessing;
                    flag5 = true;
                }
            }
            else
            {
                if ((this.conformanceLevel != settings.ConformanceLevel) && (this.conformanceLevel != System.Xml.ConformanceLevel.Auto))
                {
                    throw new InvalidOperationException(Res.GetString("Xml_IncompatibleConformanceLevel", new object[] { this.conformanceLevel.ToString() }));
                }
                if (this.checkCharacters && !settings.CheckCharacters)
                {
                    checkCharacters = true;
                    flag5 = true;
                }
                if (this.ignoreWhitespace && !settings.IgnoreWhitespace)
                {
                    ignoreWhitespace = true;
                    flag5 = true;
                }
                if (this.ignoreComments && !settings.IgnoreComments)
                {
                    ignoreComments = true;
                    flag5 = true;
                }
                if (this.ignorePIs && !settings.IgnoreProcessingInstructions)
                {
                    ignorePis = true;
                    flag5 = true;
                }
                if (((this.dtdProcessing == System.Xml.DtdProcessing.Prohibit) && (settings.DtdProcessing != System.Xml.DtdProcessing.Prohibit)) || ((this.dtdProcessing == System.Xml.DtdProcessing.Ignore) && (settings.DtdProcessing == System.Xml.DtdProcessing.Parse)))
                {
                    dtdProcessing = this.dtdProcessing;
                    flag5 = true;
                }
            }
            if (!flag5)
            {
                return baseReader;
            }
            IXmlNamespaceResolver readerAsNSResolver = baseReader as IXmlNamespaceResolver;
            if (readerAsNSResolver != null)
            {
                return new XmlCharCheckingReaderWithNS(baseReader, readerAsNSResolver, checkCharacters, ignoreWhitespace, ignoreComments, ignorePis, dtdProcessing);
            }
            return new XmlCharCheckingReader(baseReader, checkCharacters, ignoreWhitespace, ignoreComments, ignorePis, dtdProcessing);
        }

        internal XmlReader AddValidation(XmlReader reader)
        {
            if (this.validationType == System.Xml.ValidationType.Schema)
            {
                reader = new XsdValidatingReader(reader, this.GetXmlResolver(), this);
                return reader;
            }
            if (this.validationType == System.Xml.ValidationType.DTD)
            {
                reader = this.CreateDtdValidatingReader(reader);
            }
            return reader;
        }

        private XmlReader AddValidationAndConformanceWrapper(XmlReader reader)
        {
            if (this.validationType == System.Xml.ValidationType.DTD)
            {
                reader = this.CreateDtdValidatingReader(reader);
            }
            reader = this.AddConformanceWrapper(reader);
            if (this.validationType == System.Xml.ValidationType.Schema)
            {
                reader = new XsdValidatingReader(reader, this.GetXmlResolver(), this);
            }
            return reader;
        }

        private void CheckReadOnly(string propertyName)
        {
            if (this.isReadOnly)
            {
                throw new XmlException("Xml_ReadOnlyProperty", base.GetType().Name + '.' + propertyName);
            }
        }

        public XmlReaderSettings Clone()
        {
            XmlReaderSettings settings = base.MemberwiseClone() as XmlReaderSettings;
            settings.ReadOnly = false;
            return settings;
        }

        private System.Xml.XmlResolver CreateDefaultResolver()
        {
            return new XmlUrlResolver();
        }

        private XmlValidatingReaderImpl CreateDtdValidatingReader(XmlReader baseReader)
        {
            return new XmlValidatingReaderImpl(baseReader, this.GetEventHandler(), (this.ValidationFlags & XmlSchemaValidationFlags.ProcessIdentityConstraints) != XmlSchemaValidationFlags.None);
        }

        internal XmlReader CreateReader(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            return this.AddValidationAndConformanceWrapper(reader);
        }

        internal XmlReader CreateReader(string inputUri, XmlParserContext inputContext)
        {
            XmlReader reader;
            if (inputUri == null)
            {
                throw new ArgumentNullException("inputUri");
            }
            if (inputUri.Length == 0)
            {
                throw new ArgumentException(Res.GetString("XmlConvert_BadUri"), "inputUri");
            }
            System.Xml.XmlResolver xmlResolver = this.GetXmlResolver();
            if (xmlResolver == null)
            {
                xmlResolver = this.CreateDefaultResolver();
            }
            Uri absoluteUri = xmlResolver.ResolveUri(null, inputUri);
            Stream input = (Stream) xmlResolver.GetEntity(absoluteUri, string.Empty, typeof(Stream));
            if (input == null)
            {
                throw new XmlException("Xml_CannotResolveUrl", inputUri);
            }
            XmlReaderSettings settings = this;
            if (!settings.CloseInput)
            {
                settings = settings.Clone();
                settings.CloseInput = true;
            }
            try
            {
                reader = settings.CreateReader(input, absoluteUri, null, inputContext);
            }
            catch
            {
                input.Close();
                throw;
            }
            return reader;
        }

        internal XmlReader CreateReader(TextReader input, string baseUriString, XmlParserContext inputContext)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (baseUriString == null)
            {
                baseUriString = string.Empty;
            }
            XmlReader reader = new XmlTextReaderImpl(input, this, baseUriString, inputContext);
            if (this.ValidationType != System.Xml.ValidationType.None)
            {
                reader = this.AddValidation(reader);
            }
            return reader;
        }

        internal XmlReader CreateReader(Stream input, Uri baseUri, string baseUriString, XmlParserContext inputContext)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (baseUriString == null)
            {
                if (baseUri == null)
                {
                    baseUriString = string.Empty;
                }
                else
                {
                    baseUriString = baseUri.ToString();
                }
            }
            XmlReader reader = new XmlTextReaderImpl(input, null, 0, this, baseUri, baseUriString, inputContext, this.closeInput);
            if (this.ValidationType != System.Xml.ValidationType.None)
            {
                reader = this.AddValidation(reader);
            }
            return reader;
        }

        internal System.Xml.Schema.ValidationEventHandler GetEventHandler()
        {
            return this.valEventHandler;
        }

        internal System.Xml.XmlResolver GetXmlResolver()
        {
            return this.xmlResolver;
        }

        private void Initialize()
        {
            this.nameTable = null;
            this.xmlResolver = this.CreateDefaultResolver();
            this.lineNumberOffset = 0;
            this.linePositionOffset = 0;
            this.checkCharacters = true;
            this.conformanceLevel = System.Xml.ConformanceLevel.Document;
            this.ignoreWhitespace = false;
            this.ignorePIs = false;
            this.ignoreComments = false;
            this.dtdProcessing = System.Xml.DtdProcessing.Prohibit;
            this.closeInput = false;
            this.maxCharactersFromEntities = 0L;
            this.maxCharactersInDocument = 0L;
            this.schemas = null;
            this.validationType = System.Xml.ValidationType.None;
            this.validationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints;
            this.validationFlags |= XmlSchemaValidationFlags.AllowXmlAttributes;
            this.isReadOnly = false;
        }

        public void Reset()
        {
            this.CheckReadOnly("Reset");
            this.Initialize();
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

        public bool CloseInput
        {
            get
            {
                return this.closeInput;
            }
            set
            {
                this.CheckReadOnly("CloseInput");
                this.closeInput = value;
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

        public System.Xml.DtdProcessing DtdProcessing
        {
            get
            {
                return this.dtdProcessing;
            }
            set
            {
                this.CheckReadOnly("DtdProcessing");
                if (value > System.Xml.DtdProcessing.Parse)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.dtdProcessing = value;
            }
        }

        public bool IgnoreComments
        {
            get
            {
                return this.ignoreComments;
            }
            set
            {
                this.CheckReadOnly("IgnoreComments");
                this.ignoreComments = value;
            }
        }

        public bool IgnoreProcessingInstructions
        {
            get
            {
                return this.ignorePIs;
            }
            set
            {
                this.CheckReadOnly("IgnoreProcessingInstructions");
                this.ignorePIs = value;
            }
        }

        public bool IgnoreWhitespace
        {
            get
            {
                return this.ignoreWhitespace;
            }
            set
            {
                this.CheckReadOnly("IgnoreWhitespace");
                this.ignoreWhitespace = value;
            }
        }

        public int LineNumberOffset
        {
            get
            {
                return this.lineNumberOffset;
            }
            set
            {
                this.CheckReadOnly("LineNumberOffset");
                this.lineNumberOffset = value;
            }
        }

        public int LinePositionOffset
        {
            get
            {
                return this.linePositionOffset;
            }
            set
            {
                this.CheckReadOnly("LinePositionOffset");
                this.linePositionOffset = value;
            }
        }

        public long MaxCharactersFromEntities
        {
            get
            {
                return this.maxCharactersFromEntities;
            }
            set
            {
                this.CheckReadOnly("MaxCharactersFromEntities");
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.maxCharactersFromEntities = value;
            }
        }

        public long MaxCharactersInDocument
        {
            get
            {
                return this.maxCharactersInDocument;
            }
            set
            {
                this.CheckReadOnly("MaxCharactersInDocument");
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.maxCharactersInDocument = value;
            }
        }

        public XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
            set
            {
                this.CheckReadOnly("NameTable");
                this.nameTable = value;
            }
        }

        [Obsolete("Use XmlReaderSettings.DtdProcessing property instead.")]
        public bool ProhibitDtd
        {
            get
            {
                return (this.dtdProcessing == System.Xml.DtdProcessing.Prohibit);
            }
            set
            {
                this.CheckReadOnly("ProhibitDtd");
                this.dtdProcessing = value ? System.Xml.DtdProcessing.Prohibit : System.Xml.DtdProcessing.Parse;
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

        public XmlSchemaSet Schemas
        {
            get
            {
                if (this.schemas == null)
                {
                    this.schemas = new XmlSchemaSet();
                }
                return this.schemas;
            }
            set
            {
                this.CheckReadOnly("Schemas");
                this.schemas = value;
            }
        }

        public XmlSchemaValidationFlags ValidationFlags
        {
            get
            {
                return this.validationFlags;
            }
            set
            {
                this.CheckReadOnly("ValidationFlags");
                if (value > (XmlSchemaValidationFlags.AllowXmlAttributes | XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ProcessInlineSchema))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.validationFlags = value;
            }
        }

        public System.Xml.ValidationType ValidationType
        {
            get
            {
                return this.validationType;
            }
            set
            {
                this.CheckReadOnly("ValidationType");
                if (value > System.Xml.ValidationType.Schema)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.validationType = value;
            }
        }

        public System.Xml.XmlResolver XmlResolver
        {
            set
            {
                this.CheckReadOnly("XmlResolver");
                this.xmlResolver = value;
            }
        }
    }
}

