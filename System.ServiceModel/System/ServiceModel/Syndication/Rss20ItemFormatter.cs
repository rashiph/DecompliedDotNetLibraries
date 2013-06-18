namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), XmlRoot(ElementName="item", Namespace="")]
    public class Rss20ItemFormatter : SyndicationItemFormatter, IXmlSerializable
    {
        private Rss20FeedFormatter feedSerializer;
        private Type itemType;
        private bool preserveAttributeExtensions;
        private bool preserveElementExtensions;
        private bool serializeExtensionsAsAtom;

        public Rss20ItemFormatter() : this(typeof(SyndicationItem))
        {
        }

        public Rss20ItemFormatter(SyndicationItem itemToWrite) : this(itemToWrite, true)
        {
        }

        public Rss20ItemFormatter(Type itemTypeToCreate)
        {
            if (itemTypeToCreate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("itemTypeToCreate");
            }
            if (!typeof(SyndicationItem).IsAssignableFrom(itemTypeToCreate))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("itemTypeToCreate", System.ServiceModel.SR.GetString("InvalidObjectTypePassed", new object[] { "itemTypeToCreate", "SyndicationItem" }));
            }
            this.feedSerializer = new Rss20FeedFormatter();
            this.feedSerializer.PreserveAttributeExtensions = this.preserveAttributeExtensions = true;
            this.feedSerializer.PreserveElementExtensions = this.preserveElementExtensions = true;
            this.feedSerializer.SerializeExtensionsAsAtom = this.serializeExtensionsAsAtom = true;
            this.itemType = itemTypeToCreate;
        }

        public Rss20ItemFormatter(SyndicationItem itemToWrite, bool serializeExtensionsAsAtom) : base(itemToWrite)
        {
            this.feedSerializer = new Rss20FeedFormatter();
            this.feedSerializer.PreserveAttributeExtensions = this.preserveAttributeExtensions = true;
            this.feedSerializer.PreserveElementExtensions = this.preserveElementExtensions = true;
            this.feedSerializer.SerializeExtensionsAsAtom = this.serializeExtensionsAsAtom = serializeExtensionsAsAtom;
            this.itemType = itemToWrite.GetType();
        }

        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return reader.IsStartElement("item", "");
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
            XmlDictionaryWriter writer2 = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            this.feedSerializer.WriteItemContents(writer2, base.Item);
        }

        public override void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            SyndicationFeedFormatter.TraceItemWriteBegin();
            writer.WriteStartElement("item", "");
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

        public bool SerializeExtensionsAsAtom
        {
            get
            {
                return this.serializeExtensionsAsAtom;
            }
            set
            {
                this.serializeExtensionsAsAtom = value;
                this.feedSerializer.SerializeExtensionsAsAtom = value;
            }
        }

        public override string Version
        {
            get
            {
                return "Rss20";
            }
        }
    }
}

