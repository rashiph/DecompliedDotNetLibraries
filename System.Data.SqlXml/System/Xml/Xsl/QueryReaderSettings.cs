namespace System.Xml.Xsl
{
    using System;
    using System.IO;
    using System.Xml;

    internal class QueryReaderSettings
    {
        private EntityHandling entityHandling;
        private bool namespaces;
        private bool normalization;
        private bool prohibitDtd;
        private bool validatingReader;
        private WhitespaceHandling whitespaceHandling;
        private XmlNameTable xmlNameTable;
        private XmlReaderSettings xmlReaderSettings;
        private XmlResolver xmlResolver;

        public QueryReaderSettings(XmlNameTable xmlNameTable)
        {
            this.xmlReaderSettings = new XmlReaderSettings();
            this.xmlReaderSettings.NameTable = xmlNameTable;
            this.xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            this.xmlReaderSettings.XmlResolver = null;
            this.xmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
            this.xmlReaderSettings.CloseInput = true;
        }

        public QueryReaderSettings(XmlReader reader)
        {
            XmlValidatingReader reader2 = reader as XmlValidatingReader;
            if (reader2 != null)
            {
                this.validatingReader = true;
                reader = reader2.Impl.Reader;
            }
            this.xmlReaderSettings = reader.Settings;
            if (this.xmlReaderSettings != null)
            {
                this.xmlReaderSettings = this.xmlReaderSettings.Clone();
                this.xmlReaderSettings.NameTable = reader.NameTable;
                this.xmlReaderSettings.CloseInput = true;
                this.xmlReaderSettings.LineNumberOffset = 0;
                this.xmlReaderSettings.LinePositionOffset = 0;
                XmlTextReaderImpl impl = reader as XmlTextReaderImpl;
                if (impl != null)
                {
                    this.xmlReaderSettings.XmlResolver = impl.GetResolver();
                }
            }
            else
            {
                this.xmlNameTable = reader.NameTable;
                XmlTextReader reader3 = reader as XmlTextReader;
                if (reader3 != null)
                {
                    XmlTextReaderImpl impl2 = reader3.Impl;
                    this.entityHandling = impl2.EntityHandling;
                    this.namespaces = impl2.Namespaces;
                    this.normalization = impl2.Normalization;
                    this.prohibitDtd = impl2.DtdProcessing == DtdProcessing.Prohibit;
                    this.whitespaceHandling = impl2.WhitespaceHandling;
                    this.xmlResolver = impl2.GetResolver();
                }
                else
                {
                    this.entityHandling = EntityHandling.ExpandEntities;
                    this.namespaces = true;
                    this.normalization = true;
                    this.prohibitDtd = true;
                    this.whitespaceHandling = WhitespaceHandling.All;
                    this.xmlResolver = null;
                }
            }
        }

        public XmlReader CreateReader(Stream stream, string baseUri)
        {
            XmlReader reader;
            if (this.xmlReaderSettings != null)
            {
                reader = XmlReader.Create(stream, this.xmlReaderSettings, baseUri);
            }
            else
            {
                XmlTextReaderImpl impl = new XmlTextReaderImpl(baseUri, stream, this.xmlNameTable) {
                    EntityHandling = this.entityHandling,
                    Namespaces = this.namespaces,
                    Normalization = this.normalization,
                    DtdProcessing = this.prohibitDtd ? DtdProcessing.Prohibit : DtdProcessing.Parse,
                    WhitespaceHandling = this.whitespaceHandling,
                    XmlResolver = this.xmlResolver
                };
                reader = impl;
            }
            if (this.validatingReader)
            {
                reader = new XmlValidatingReader(reader);
            }
            return reader;
        }

        public XmlNameTable NameTable
        {
            get
            {
                if (this.xmlReaderSettings == null)
                {
                    return this.xmlNameTable;
                }
                return this.xmlReaderSettings.NameTable;
            }
        }
    }
}

