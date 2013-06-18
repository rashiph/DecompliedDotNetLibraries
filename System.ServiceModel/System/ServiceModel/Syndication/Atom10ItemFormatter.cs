namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlRoot(ElementName="entry", Namespace="http://www.w3.org/2005/Atom"), TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class Atom10ItemFormatter : SyndicationItemFormatter, IXmlSerializable
    {
        private Atom10FeedFormatter feedSerializer;
        private Type itemType;
        private bool preserveAttributeExtensions;
        private bool preserveElementExtensions;

        public Atom10ItemFormatter() : this(typeof(SyndicationItem))
        {
        }

        public Atom10ItemFormatter(SyndicationItem itemToWrite) : base(itemToWrite)
        {
            this.feedSerializer = new Atom10FeedFormatter();
            this.feedSerializer.PreserveAttributeExtensions = this.preserveAttributeExtensions = true;
            this.feedSerializer.PreserveElementExtensions = this.preserveElementExtensions = true;
            this.itemType = itemToWrite.GetType();
        }

        public Atom10ItemFormatter(Type itemTypeToCreate)
        {
            if (itemTypeToCreate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("itemTypeToCreate");
            }
            if (!typeof(SyndicationItem).IsAssignableFrom(itemTypeToCreate))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("itemTypeToCreate", System.ServiceModel.SR.GetString("InvalidObjectTypePassed", new object[] { "itemTypeToCreate", "SyndicationItem" }));
            }
            this.feedSerializer = new Atom10FeedFormatter();
            this.feedSerializer.PreserveAttributeExtensions = this.preserveAttributeExtensions = true;
            this.feedSerializer.PreserveElementExtensions = this.preserveElementExtensions = true;
            this.itemType = itemTypeToCreate;
        }

        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return reader.IsStartElement("entry", "http://www.w3.org/2005/Atom");
        }

        protected override SyndicationItem CreateItemInstance()
        {
            return SyndicationItemFormatter.CreateItemInstance(this.itemType);
        }

        public override void ReadFrom(XmlReader reader)
        {
            SyndicationFeedFormatter.TraceItemReadBegin();
            if (!this.CanRead(reader))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnknownItemXml", new object[] { reader.LocalName, reader.NamespaceURI })));
            }
            this.ReadItem(reader);
            SyndicationFeedFormatter.TraceItemReadEnd();
        }

        private void ReadItem(XmlReader reader)
        {
            this.SetItem(this.CreateItemInstance());
            this.feedSerializer.ReadItemFrom(XmlDictionaryReader.CreateDictionaryReader(reader), base.Item);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            SyndicationFeedFormatter.TraceItemReadBegin();
            this.ReadItem(reader);
            SyndicationFeedFormatter.TraceItemReadEnd();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            SyndicationFeedFormatter.TraceItemWriteBegin();
            this.WriteItem(writer);
            SyndicationFeedFormatter.TraceItemWriteEnd();
        }

        private void WriteItem(XmlWriter writer)
        {
            if (base.Item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemFormatterDoesNotHaveItem")));
            }
            XmlDictionaryWriter dictWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            this.feedSerializer.WriteItemContents(dictWriter, base.Item);
        }

        public override void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            SyndicationFeedFormatter.TraceItemWriteBegin();
            writer.WriteStartElement("entry", "http://www.w3.org/2005/Atom");
            this.WriteItem(writer);
            writer.WriteEndElement();
            SyndicationFeedFormatter.TraceItemWriteEnd();
        }

        protected Type ItemType
        {
            get
            {
                return this.itemType;
            }
        }

        public bool PreserveAttributeExtensions
        {
            get
            {
                return this.preserveAttributeExtensions;
            }
            set
            {
                this.preserveAttributeExtensions = value;
                this.feedSerializer.PreserveAttributeExtensions = value;
            }
        }

        public bool PreserveElementExtensions
        {
            get
            {
                return this.preserveElementExtensions;
            }
            set
            {
                this.preserveElementExtensions = value;
                this.feedSerializer.PreserveElementExtensions = value;
            }
        }

        public override string Version
        {
            get
            {
                return "Atom10";
            }
        }
    }
}

