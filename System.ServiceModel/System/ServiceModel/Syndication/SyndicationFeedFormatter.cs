namespace System.ServiceModel.Syndication
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), DataContract]
    public abstract class SyndicationFeedFormatter
    {
        private SyndicationFeed feed;

        protected SyndicationFeedFormatter()
        {
            this.feed = null;
        }

        protected SyndicationFeedFormatter(SyndicationFeed feedToWrite)
        {
            if (feedToWrite == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feedToWrite");
            }
            this.feed = feedToWrite;
        }

        public abstract bool CanRead(XmlReader reader);
        internal static void CloseBuffer(XmlBuffer buffer, XmlDictionaryWriter extWriter)
        {
            if (buffer != null)
            {
                extWriter.WriteEndElement();
                buffer.CloseSection();
                buffer.Close();
            }
        }

        internal static void CreateBufferIfRequiredAndWriteNode(ref XmlBuffer buffer, ref XmlDictionaryWriter extWriter, XmlReader reader, int maxExtensionSize)
        {
            if (buffer == null)
            {
                buffer = new XmlBuffer(maxExtensionSize);
                extWriter = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
                extWriter.WriteStartElement("extensionWrapper");
            }
            extWriter.WriteNode(reader, false);
        }

        protected internal static SyndicationCategory CreateCategory(SyndicationFeed feed)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            return GetNonNullValue<SyndicationCategory>(feed.CreateCategory(), "FeedCreatedNullCategory");
        }

        protected internal static SyndicationCategory CreateCategory(SyndicationItem item)
        {
            if (item == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            return GetNonNullValue<SyndicationCategory>(item.CreateCategory(), "ItemCreatedNullCategory");
        }

        protected abstract SyndicationFeed CreateFeedInstance();
        internal static SyndicationFeed CreateFeedInstance(Type feedType)
        {
            if (feedType.Equals(typeof(SyndicationFeed)))
            {
                return new SyndicationFeed();
            }
            return (SyndicationFeed) Activator.CreateInstance(feedType);
        }

        protected internal static SyndicationItem CreateItem(SyndicationFeed feed)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            return GetNonNullValue<SyndicationItem>(feed.CreateItem(), "FeedCreatedNullItem");
        }

        protected internal static SyndicationLink CreateLink(SyndicationFeed feed)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            return GetNonNullValue<SyndicationLink>(feed.CreateLink(), "FeedCreatedNullPerson");
        }

        protected internal static SyndicationLink CreateLink(SyndicationItem item)
        {
            if (item == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            return GetNonNullValue<SyndicationLink>(item.CreateLink(), "ItemCreatedNullPerson");
        }

        protected internal static SyndicationPerson CreatePerson(SyndicationFeed feed)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            return GetNonNullValue<SyndicationPerson>(feed.CreatePerson(), "FeedCreatedNullPerson");
        }

        protected internal static SyndicationPerson CreatePerson(SyndicationItem item)
        {
            if (item == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            return GetNonNullValue<SyndicationPerson>(item.CreatePerson(), "ItemCreatedNullPerson");
        }

        private static T GetNonNullValue<T>(T value, string errorMsg)
        {
            if (value == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString(errorMsg)));
            }
            return value;
        }

        internal static void LoadElementExtensions(XmlBuffer buffer, XmlDictionaryWriter writer, SyndicationCategory category)
        {
            if (category == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("category");
            }
            CloseBuffer(buffer, writer);
            category.LoadElementExtensions(buffer);
        }

        internal static void LoadElementExtensions(XmlBuffer buffer, XmlDictionaryWriter writer, SyndicationFeed feed)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            CloseBuffer(buffer, writer);
            feed.LoadElementExtensions(buffer);
        }

        internal static void LoadElementExtensions(XmlBuffer buffer, XmlDictionaryWriter writer, SyndicationItem item)
        {
            if (item == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            CloseBuffer(buffer, writer);
            item.LoadElementExtensions(buffer);
        }

        internal static void LoadElementExtensions(XmlBuffer buffer, XmlDictionaryWriter writer, SyndicationLink link)
        {
            if (link == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("link");
            }
            CloseBuffer(buffer, writer);
            link.LoadElementExtensions(buffer);
        }

        internal static void LoadElementExtensions(XmlBuffer buffer, XmlDictionaryWriter writer, SyndicationPerson person)
        {
            if (person == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("person");
            }
            CloseBuffer(buffer, writer);
            person.LoadElementExtensions(buffer);
        }

        protected internal static void LoadElementExtensions(XmlReader reader, SyndicationCategory category, int maxExtensionSize)
        {
            if (category == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("category");
            }
            category.LoadElementExtensions(reader, maxExtensionSize);
        }

        protected internal static void LoadElementExtensions(XmlReader reader, SyndicationFeed feed, int maxExtensionSize)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            feed.LoadElementExtensions(reader, maxExtensionSize);
        }

        protected internal static void LoadElementExtensions(XmlReader reader, SyndicationItem item, int maxExtensionSize)
        {
            if (item == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            item.LoadElementExtensions(reader, maxExtensionSize);
        }

        protected internal static void LoadElementExtensions(XmlReader reader, SyndicationLink link, int maxExtensionSize)
        {
            if (link == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("link");
            }
            link.LoadElementExtensions(reader, maxExtensionSize);
        }

        protected internal static void LoadElementExtensions(XmlReader reader, SyndicationPerson person, int maxExtensionSize)
        {
            if (person == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("person");
            }
            person.LoadElementExtensions(reader, maxExtensionSize);
        }

        internal static void MoveToStartElement(XmlReader reader)
        {
            if (!reader.IsStartElement())
            {
                XmlExceptionHelper.ThrowStartElementExpected(XmlDictionaryReader.CreateDictionaryReader(reader));
            }
        }

        public abstract void ReadFrom(XmlReader reader);
        protected internal virtual void SetFeed(SyndicationFeed feed)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            this.feed = feed;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}, SyndicationVersion={1}", new object[] { base.GetType(), this.Version });
        }

        internal static void TraceFeedReadBegin()
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf0019, System.ServiceModel.SR.GetString("TraceCodeSyndicationFeedReadBegin"));
            }
        }

        internal static void TraceFeedReadEnd()
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf001a, System.ServiceModel.SR.GetString("TraceCodeSyndicationFeedReadEnd"));
            }
        }

        internal static void TraceFeedWriteBegin()
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf001d, System.ServiceModel.SR.GetString("TraceCodeSyndicationFeedWriteBegin"));
            }
        }

        internal static void TraceFeedWriteEnd()
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf001e, System.ServiceModel.SR.GetString("TraceCodeSyndicationFeedWriteEnd"));
            }
        }

        internal static void TraceItemReadBegin()
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf001b, System.ServiceModel.SR.GetString("TraceCodeSyndicationItemReadBegin"));
            }
        }

        internal static void TraceItemReadEnd()
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf001c, System.ServiceModel.SR.GetString("TraceCodeSyndicationItemReadEnd"));
            }
        }

        internal static void TraceItemWriteBegin()
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf001f, System.ServiceModel.SR.GetString("TraceCodeSyndicationItemWriteBegin"));
            }
        }

        internal static void TraceItemWriteEnd()
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf0020, System.ServiceModel.SR.GetString("TraceCodeSyndicationItemWriteEnd"));
            }
        }

        internal static void TraceSyndicationElementIgnoredOnRead(XmlReader reader)
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf0021, System.ServiceModel.SR.GetString("TraceCodeSyndicationProtocolElementIgnoredOnRead", new object[] { reader.NodeType, reader.LocalName, reader.NamespaceURI }));
            }
        }

        protected internal static bool TryParseAttribute(string name, string ns, string value, SyndicationCategory category, string version)
        {
            if (category == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("category");
            }
            return (FeedUtils.IsXmlns(name, ns) || category.TryParseAttribute(name, ns, value, version));
        }

        protected internal static bool TryParseAttribute(string name, string ns, string value, SyndicationFeed feed, string version)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            return (FeedUtils.IsXmlns(name, ns) || feed.TryParseAttribute(name, ns, value, version));
        }

        protected internal static bool TryParseAttribute(string name, string ns, string value, SyndicationItem item, string version)
        {
            if (item == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            return (FeedUtils.IsXmlns(name, ns) || item.TryParseAttribute(name, ns, value, version));
        }

        protected internal static bool TryParseAttribute(string name, string ns, string value, SyndicationLink link, string version)
        {
            if (link == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("link");
            }
            return (FeedUtils.IsXmlns(name, ns) || link.TryParseAttribute(name, ns, value, version));
        }

        protected internal static bool TryParseAttribute(string name, string ns, string value, SyndicationPerson person, string version)
        {
            if (person == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("person");
            }
            return (FeedUtils.IsXmlns(name, ns) || person.TryParseAttribute(name, ns, value, version));
        }

        protected internal static bool TryParseContent(XmlReader reader, SyndicationItem item, string contentType, string version, out SyndicationContent content)
        {
            return item.TryParseContent(reader, contentType, version, out content);
        }

        protected internal static bool TryParseElement(XmlReader reader, SyndicationCategory category, string version)
        {
            if (category == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("category");
            }
            return category.TryParseElement(reader, version);
        }

        protected internal static bool TryParseElement(XmlReader reader, SyndicationFeed feed, string version)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            return feed.TryParseElement(reader, version);
        }

        protected internal static bool TryParseElement(XmlReader reader, SyndicationItem item, string version)
        {
            if (item == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            return item.TryParseElement(reader, version);
        }

        protected internal static bool TryParseElement(XmlReader reader, SyndicationLink link, string version)
        {
            if (link == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("link");
            }
            return link.TryParseElement(reader, version);
        }

        protected internal static bool TryParseElement(XmlReader reader, SyndicationPerson person, string version)
        {
            if (person == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("person");
            }
            return person.TryParseElement(reader, version);
        }

        protected internal static void WriteAttributeExtensions(XmlWriter writer, SyndicationCategory category, string version)
        {
            if (category == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("category");
            }
            category.WriteAttributeExtensions(writer, version);
        }

        protected internal static void WriteAttributeExtensions(XmlWriter writer, SyndicationFeed feed, string version)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            feed.WriteAttributeExtensions(writer, version);
        }

        protected internal static void WriteAttributeExtensions(XmlWriter writer, SyndicationItem item, string version)
        {
            if (item == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            item.WriteAttributeExtensions(writer, version);
        }

        protected internal static void WriteAttributeExtensions(XmlWriter writer, SyndicationLink link, string version)
        {
            if (link == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("link");
            }
            link.WriteAttributeExtensions(writer, version);
        }

        protected internal static void WriteAttributeExtensions(XmlWriter writer, SyndicationPerson person, string version)
        {
            if (person == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("person");
            }
            person.WriteAttributeExtensions(writer, version);
        }

        protected internal static void WriteElementExtensions(XmlWriter writer, SyndicationCategory category, string version)
        {
            if (category == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("category");
            }
            category.WriteElementExtensions(writer, version);
        }

        protected internal static void WriteElementExtensions(XmlWriter writer, SyndicationFeed feed, string version)
        {
            if (feed == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            feed.WriteElementExtensions(writer, version);
        }

        protected internal static void WriteElementExtensions(XmlWriter writer, SyndicationItem item, string version)
        {
            if (item == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            item.WriteElementExtensions(writer, version);
        }

        protected internal static void WriteElementExtensions(XmlWriter writer, SyndicationLink link, string version)
        {
            if (link == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("link");
            }
            link.WriteElementExtensions(writer, version);
        }

        protected internal static void WriteElementExtensions(XmlWriter writer, SyndicationPerson person, string version)
        {
            if (person == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("person");
            }
            person.WriteElementExtensions(writer, version);
        }

        public abstract void WriteTo(XmlWriter writer);

        public SyndicationFeed Feed
        {
            get
            {
                return this.feed;
            }
        }

        public abstract string Version { get; }

        private static class XmlExceptionHelper
        {
            private static string GetName(string prefix, string localName)
            {
                if (prefix.Length == 0)
                {
                    return localName;
                }
                return (prefix + ":" + localName);
            }

            private static string GetWhatWasFound(XmlDictionaryReader reader)
            {
                if (reader.EOF)
                {
                    return System.ServiceModel.SR.GetString("XmlFoundEndOfFile");
                }
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        return System.ServiceModel.SR.GetString("XmlFoundElement", new object[] { GetName(reader.Prefix, reader.LocalName), reader.NamespaceURI });

                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        return System.ServiceModel.SR.GetString("XmlFoundText", new object[] { reader.Value });

                    case XmlNodeType.CDATA:
                        return System.ServiceModel.SR.GetString("XmlFoundCData", new object[] { reader.Value });

                    case XmlNodeType.Comment:
                        return System.ServiceModel.SR.GetString("XmlFoundComment", new object[] { reader.Value });

                    case XmlNodeType.EndElement:
                        return System.ServiceModel.SR.GetString("XmlFoundEndElement", new object[] { GetName(reader.Prefix, reader.LocalName), reader.NamespaceURI });
                }
                return System.ServiceModel.SR.GetString("XmlFoundNodeType", new object[] { reader.NodeType });
            }

            public static void ThrowStartElementExpected(XmlDictionaryReader reader)
            {
                ThrowXmlException(reader, "XmlStartElementExpected", GetWhatWasFound(reader));
            }

            private static void ThrowXmlException(XmlDictionaryReader reader, string res, string arg1)
            {
                string message = System.ServiceModel.SR.GetString(res, new object[] { arg1 });
                IXmlLineInfo info = reader as IXmlLineInfo;
                if ((info != null) && info.HasLineInfo())
                {
                    message = message + " " + System.ServiceModel.SR.GetString("XmlLineInfo", new object[] { info.LineNumber, info.LinePosition });
                }
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(message));
            }
        }
    }
}

