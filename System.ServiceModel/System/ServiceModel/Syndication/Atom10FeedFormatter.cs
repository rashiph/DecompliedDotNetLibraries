namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlRoot(ElementName="feed", Namespace="http://www.w3.org/2005/Atom"), TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class Atom10FeedFormatter : SyndicationFeedFormatter, IXmlSerializable
    {
        private static readonly XmlQualifiedName Atom10Href = new XmlQualifiedName("href", string.Empty);
        private static readonly XmlQualifiedName Atom10Label = new XmlQualifiedName("label", string.Empty);
        private static readonly XmlQualifiedName Atom10Length = new XmlQualifiedName("length", string.Empty);
        private static readonly XmlQualifiedName Atom10Relative = new XmlQualifiedName("rel", string.Empty);
        private static readonly XmlQualifiedName Atom10Scheme = new XmlQualifiedName("scheme", string.Empty);
        private static readonly XmlQualifiedName Atom10Term = new XmlQualifiedName("term", string.Empty);
        private static readonly XmlQualifiedName Atom10Title = new XmlQualifiedName("title", string.Empty);
        private static readonly XmlQualifiedName Atom10Type = new XmlQualifiedName("type", string.Empty);
        private System.Type feedType;
        private static readonly UriGenerator idGenerator = new UriGenerator();
        private int maxExtensionSize;
        private bool preserveAttributeExtensions;
        private bool preserveElementExtensions;
        private const string Rfc3339LocalDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
        private const string Rfc3339UTCDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        internal const string XmlNs = "http://www.w3.org/XML/1998/namespace";
        internal const string XmlNsNs = "http://www.w3.org/2000/xmlns/";
        internal static readonly TimeSpan zeroOffset = new TimeSpan(0, 0, 0);

        public Atom10FeedFormatter() : this(typeof(SyndicationFeed))
        {
        }

        public Atom10FeedFormatter(SyndicationFeed feedToWrite) : base(feedToWrite)
        {
            this.maxExtensionSize = 0x7fffffff;
            this.preserveAttributeExtensions = this.preserveElementExtensions = true;
            this.feedType = feedToWrite.GetType();
        }

        public Atom10FeedFormatter(System.Type feedTypeToCreate)
        {
            if (feedTypeToCreate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feedTypeToCreate");
            }
            if (!typeof(SyndicationFeed).IsAssignableFrom(feedTypeToCreate))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("feedTypeToCreate", System.ServiceModel.SR.GetString("InvalidObjectTypePassed", new object[] { "feedTypeToCreate", "SyndicationFeed" }));
            }
            this.maxExtensionSize = 0x7fffffff;
            this.preserveAttributeExtensions = this.preserveElementExtensions = true;
            this.feedType = feedTypeToCreate;
        }

        private string AsString(DateTimeOffset dateTime)
        {
            if (dateTime.Offset == zeroOffset)
            {
                return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }
            return dateTime.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        }

        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return reader.IsStartElement("feed", "http://www.w3.org/2005/Atom");
        }

        protected override SyndicationFeed CreateFeedInstance()
        {
            return SyndicationFeedFormatter.CreateFeedInstance(this.feedType);
        }

        private DateTimeOffset DateFromString(string dateTimeString, XmlReader reader)
        {
            DateTimeOffset offset;
            DateTimeOffset offset2;
            dateTimeString = dateTimeString.Trim();
            if (dateTimeString.Length < 20)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingDateTime")));
            }
            if (dateTimeString[0x13] == '.')
            {
                int startIndex = 20;
                while ((dateTimeString.Length > startIndex) && char.IsDigit(dateTimeString[startIndex]))
                {
                    startIndex++;
                }
                dateTimeString = dateTimeString.Substring(0, 0x13) + dateTimeString.Substring(startIndex);
            }
            if (DateTimeOffset.TryParseExact(dateTimeString, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out offset))
            {
                return offset;
            }
            if (!DateTimeOffset.TryParseExact(dateTimeString, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out offset2))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingDateTime")));
            }
            return offset2;
        }

        private void ReadCategory(XmlReader reader, SyndicationCategory category)
        {
            ReadCategory(reader, category, this.Version, this.PreserveAttributeExtensions, this.PreserveElementExtensions, this.maxExtensionSize);
        }

        internal static void ReadCategory(XmlReader reader, SyndicationCategory category, string version, bool preserveAttributeExtensions, bool preserveElementExtensions, int maxExtensionSize)
        {
            SyndicationFeedFormatter.MoveToStartElement(reader);
            bool isEmptyElement = reader.IsEmptyElement;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if ((reader.LocalName == "term") && (reader.NamespaceURI == string.Empty))
                    {
                        category.Name = reader.Value;
                    }
                    else
                    {
                        if ((reader.LocalName == "scheme") && (reader.NamespaceURI == string.Empty))
                        {
                            category.Scheme = reader.Value;
                            continue;
                        }
                        if ((reader.LocalName == "label") && (reader.NamespaceURI == string.Empty))
                        {
                            category.Label = reader.Value;
                            continue;
                        }
                        string namespaceURI = reader.NamespaceURI;
                        string localName = reader.LocalName;
                        if (!FeedUtils.IsXmlns(localName, namespaceURI))
                        {
                            string str3 = reader.Value;
                            if (!SyndicationFeedFormatter.TryParseAttribute(localName, namespaceURI, str3, category, version))
                            {
                                if (preserveAttributeExtensions)
                                {
                                    category.AttributeExtensions.Add(new XmlQualifiedName(localName, namespaceURI), str3);
                                    continue;
                                }
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            }
                        }
                    }
                }
            }
            if (!isEmptyElement)
            {
                reader.ReadStartElement();
                XmlBuffer buffer = null;
                using (XmlDictionaryWriter writer = null)
                {
                    while (reader.IsStartElement())
                    {
                        if (!SyndicationFeedFormatter.TryParseElement(reader, category, version))
                        {
                            if (!preserveElementExtensions)
                            {
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                reader.Skip();
                            }
                            else
                            {
                                SyndicationFeedFormatter.CreateBufferIfRequiredAndWriteNode(ref buffer, ref writer, reader, maxExtensionSize);
                            }
                        }
                    }
                    SyndicationFeedFormatter.LoadElementExtensions(buffer, writer, category);
                }
                reader.ReadEndElement();
            }
            else
            {
                reader.ReadStartElement();
            }
        }

        private SyndicationCategory ReadCategoryFrom(XmlReader reader, SyndicationFeed feed)
        {
            SyndicationCategory category = SyndicationFeedFormatter.CreateCategory(feed);
            this.ReadCategory(reader, category);
            return category;
        }

        private SyndicationCategory ReadCategoryFrom(XmlReader reader, SyndicationItem item)
        {
            SyndicationCategory category = SyndicationFeedFormatter.CreateCategory(item);
            this.ReadCategory(reader, category);
            return category;
        }

        private SyndicationContent ReadContentFrom(XmlReader reader, SyndicationItem item)
        {
            SyndicationContent content;
            SyndicationFeedFormatter.MoveToStartElement(reader);
            string attribute = reader.GetAttribute("type", string.Empty);
            if (!SyndicationFeedFormatter.TryParseContent(reader, item, attribute, this.Version, out content))
            {
                if (string.IsNullOrEmpty(attribute))
                {
                    attribute = "text";
                }
                string str2 = reader.GetAttribute("src", string.Empty);
                if ((string.IsNullOrEmpty(str2) && (attribute != "text")) && ((attribute != "html") && (attribute != "xhtml")))
                {
                    return new XmlSyndicationContent(reader);
                }
                if (string.IsNullOrEmpty(str2))
                {
                    return ReadTextContentFromHelper(reader, attribute, "//atom:feed/atom:entry/atom:content[@type]", this.preserveAttributeExtensions);
                }
                content = new UrlSyndicationContent(new Uri(str2, UriKind.RelativeOrAbsolute), attribute);
                bool isEmptyElement = reader.IsEmptyElement;
                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if ((((reader.LocalName != "type") || (reader.NamespaceURI != string.Empty)) && ((reader.LocalName != "src") || (reader.NamespaceURI != string.Empty))) && !FeedUtils.IsXmlns(reader.LocalName, reader.NamespaceURI))
                        {
                            if (this.preserveAttributeExtensions)
                            {
                                content.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                            }
                            else
                            {
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            }
                        }
                    }
                }
                reader.ReadStartElement();
                if (!isEmptyElement)
                {
                    reader.ReadEndElement();
                }
            }
            return content;
        }

        private void ReadFeed(XmlReader reader)
        {
            this.SetFeed(this.CreateFeedInstance());
            this.ReadFeedFrom(reader, base.Feed, false);
        }

        private SyndicationFeed ReadFeedFrom(XmlReader reader, SyndicationFeed result, bool isSourceFeed)
        {
            reader.MoveToContent();
            try
            {
                bool isEmptyElement = false;
                if (!isSourceFeed)
                {
                    SyndicationFeedFormatter.MoveToStartElement(reader);
                    isEmptyElement = reader.IsEmptyElement;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if ((reader.LocalName == "lang") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                            {
                                result.Language = reader.Value;
                            }
                            else
                            {
                                if ((reader.LocalName == "base") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                                {
                                    result.BaseUri = FeedUtils.CombineXmlBase(result.BaseUri, reader.Value);
                                    continue;
                                }
                                string namespaceURI = reader.NamespaceURI;
                                string localName = reader.LocalName;
                                if (!FeedUtils.IsXmlns(localName, namespaceURI) && !FeedUtils.IsXmlSchemaType(localName, namespaceURI))
                                {
                                    string str3 = reader.Value;
                                    if (!SyndicationFeedFormatter.TryParseAttribute(localName, namespaceURI, str3, result, this.Version))
                                    {
                                        if (this.preserveAttributeExtensions)
                                        {
                                            result.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                                            continue;
                                        }
                                        SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                    }
                                }
                            }
                        }
                    }
                    reader.ReadStartElement();
                }
                XmlBuffer buffer = null;
                XmlDictionaryWriter extWriter = null;
                bool areAllItemsRead = true;
                bool flag3 = false;
                if (!isEmptyElement)
                {
                    try
                    {
                        while (reader.IsStartElement())
                        {
                            if (!this.TryParseFeedElementFrom(reader, result))
                            {
                                if (reader.IsStartElement("entry", "http://www.w3.org/2005/Atom") && !isSourceFeed)
                                {
                                    if (flag3)
                                    {
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("FeedHasNonContiguousItems", new object[] { base.GetType().ToString() })));
                                    }
                                    result.Items = this.ReadItems(reader, result, out areAllItemsRead);
                                    flag3 = true;
                                    if (areAllItemsRead)
                                    {
                                        continue;
                                    }
                                    break;
                                }
                                if (!SyndicationFeedFormatter.TryParseElement(reader, result, this.Version))
                                {
                                    if (this.preserveElementExtensions)
                                    {
                                        SyndicationFeedFormatter.CreateBufferIfRequiredAndWriteNode(ref buffer, ref extWriter, reader, this.maxExtensionSize);
                                    }
                                    else
                                    {
                                        SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                        reader.Skip();
                                    }
                                }
                            }
                        }
                        SyndicationFeedFormatter.LoadElementExtensions(buffer, extWriter, result);
                    }
                    finally
                    {
                        if (extWriter != null)
                        {
                            extWriter.Dispose();
                        }
                    }
                }
                if (!isSourceFeed && areAllItemsRead)
                {
                    reader.ReadEndElement();
                }
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingFeed"), exception));
            }
            catch (ArgumentException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingFeed"), exception2));
            }
            return result;
        }

        public override void ReadFrom(XmlReader reader)
        {
            SyndicationFeedFormatter.TraceFeedReadBegin();
            if (!this.CanRead(reader))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnknownFeedXml", new object[] { reader.LocalName, reader.NamespaceURI })));
            }
            this.ReadFeed(reader);
            SyndicationFeedFormatter.TraceFeedReadEnd();
        }

        protected virtual SyndicationItem ReadItem(XmlReader reader, SyndicationFeed feed)
        {
            if (feed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            SyndicationItem result = SyndicationFeedFormatter.CreateItem(feed);
            SyndicationFeedFormatter.TraceItemReadBegin();
            this.ReadItemFrom(reader, result, feed.BaseUri);
            SyndicationFeedFormatter.TraceItemReadEnd();
            return result;
        }

        internal void ReadItemFrom(XmlReader reader, SyndicationItem result)
        {
            this.ReadItemFrom(reader, result, null);
        }

        private void ReadItemFrom(XmlReader reader, SyndicationItem result, Uri feedBaseUri)
        {
            try
            {
                result.BaseUri = feedBaseUri;
                SyndicationFeedFormatter.MoveToStartElement(reader);
                bool isEmptyElement = reader.IsEmptyElement;
                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        string namespaceURI = reader.NamespaceURI;
                        string localName = reader.LocalName;
                        if ((localName == "base") && (namespaceURI == "http://www.w3.org/XML/1998/namespace"))
                        {
                            result.BaseUri = FeedUtils.CombineXmlBase(result.BaseUri, reader.Value);
                        }
                        else if (!FeedUtils.IsXmlns(localName, namespaceURI) && !FeedUtils.IsXmlSchemaType(localName, namespaceURI))
                        {
                            string str3 = reader.Value;
                            if (!SyndicationFeedFormatter.TryParseAttribute(localName, namespaceURI, str3, result, this.Version))
                            {
                                if (this.preserveAttributeExtensions)
                                {
                                    result.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                                    continue;
                                }
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            }
                        }
                    }
                }
                reader.ReadStartElement();
                if (!isEmptyElement)
                {
                    XmlBuffer buffer = null;
                    using (XmlDictionaryWriter writer = null)
                    {
                        while (reader.IsStartElement())
                        {
                            if (!this.TryParseItemElementFrom(reader, result) && !SyndicationFeedFormatter.TryParseElement(reader, result, this.Version))
                            {
                                if (this.preserveElementExtensions)
                                {
                                    SyndicationFeedFormatter.CreateBufferIfRequiredAndWriteNode(ref buffer, ref writer, reader, this.maxExtensionSize);
                                }
                                else
                                {
                                    SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                    reader.Skip();
                                }
                            }
                        }
                        SyndicationFeedFormatter.LoadElementExtensions(buffer, writer, result);
                    }
                    reader.ReadEndElement();
                }
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingItem"), exception));
            }
            catch (ArgumentException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingItem"), exception2));
            }
        }

        protected virtual IEnumerable<SyndicationItem> ReadItems(XmlReader reader, SyndicationFeed feed, out bool areAllItemsRead)
        {
            if (feed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feed");
            }
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            NullNotAllowedCollection<SyndicationItem> alloweds = new NullNotAllowedCollection<SyndicationItem>();
            while (reader.IsStartElement("entry", "http://www.w3.org/2005/Atom"))
            {
                alloweds.Add(this.ReadItem(reader, feed));
            }
            areAllItemsRead = true;
            return alloweds;
        }

        private void ReadLink(XmlReader reader, SyndicationLink link, Uri baseUri)
        {
            bool isEmptyElement = reader.IsEmptyElement;
            string str = null;
            string str2 = null;
            string str3 = null;
            string str4 = null;
            string uriString = null;
            link.BaseUri = baseUri;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if ((reader.LocalName == "base") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                    {
                        link.BaseUri = FeedUtils.CombineXmlBase(link.BaseUri, reader.Value);
                    }
                    else
                    {
                        if ((reader.LocalName == "type") && (reader.NamespaceURI == string.Empty))
                        {
                            str = reader.Value;
                            continue;
                        }
                        if ((reader.LocalName == "rel") && (reader.NamespaceURI == string.Empty))
                        {
                            str2 = reader.Value;
                            continue;
                        }
                        if ((reader.LocalName == "title") && (reader.NamespaceURI == string.Empty))
                        {
                            str3 = reader.Value;
                            continue;
                        }
                        if ((reader.LocalName == "length") && (reader.NamespaceURI == string.Empty))
                        {
                            str4 = reader.Value;
                            continue;
                        }
                        if ((reader.LocalName == "href") && (reader.NamespaceURI == string.Empty))
                        {
                            uriString = reader.Value;
                            continue;
                        }
                        if (!FeedUtils.IsXmlns(reader.LocalName, reader.NamespaceURI))
                        {
                            if (this.preserveAttributeExtensions)
                            {
                                link.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                                continue;
                            }
                            SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                        }
                    }
                }
            }
            long num = 0L;
            if (!string.IsNullOrEmpty(str4))
            {
                num = Convert.ToInt64(str4, CultureInfo.InvariantCulture.NumberFormat);
            }
            reader.ReadStartElement();
            if (!isEmptyElement)
            {
                XmlBuffer buffer = null;
                using (XmlDictionaryWriter writer = null)
                {
                    while (reader.IsStartElement())
                    {
                        if (!SyndicationFeedFormatter.TryParseElement(reader, link, this.Version))
                        {
                            if (!this.preserveElementExtensions)
                            {
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                reader.Skip();
                            }
                            else
                            {
                                SyndicationFeedFormatter.CreateBufferIfRequiredAndWriteNode(ref buffer, ref writer, reader, this.maxExtensionSize);
                            }
                        }
                    }
                    SyndicationFeedFormatter.LoadElementExtensions(buffer, writer, link);
                }
                reader.ReadEndElement();
            }
            link.Length = num;
            link.MediaType = str;
            link.RelationshipType = str2;
            link.Title = str3;
            link.Uri = (uriString != null) ? new Uri(uriString, UriKind.RelativeOrAbsolute) : null;
        }

        private SyndicationLink ReadLinkFrom(XmlReader reader, SyndicationFeed feed)
        {
            SyndicationLink link = SyndicationFeedFormatter.CreateLink(feed);
            this.ReadLink(reader, link, feed.BaseUri);
            return link;
        }

        private SyndicationLink ReadLinkFrom(XmlReader reader, SyndicationItem item)
        {
            SyndicationLink link = SyndicationFeedFormatter.CreateLink(item);
            this.ReadLink(reader, link, item.BaseUri);
            return link;
        }

        private SyndicationPerson ReadPersonFrom(XmlReader reader, SyndicationFeed feed)
        {
            SyndicationPerson result = SyndicationFeedFormatter.CreatePerson(feed);
            this.ReadPersonFrom(reader, result);
            return result;
        }

        private SyndicationPerson ReadPersonFrom(XmlReader reader, SyndicationItem item)
        {
            SyndicationPerson result = SyndicationFeedFormatter.CreatePerson(item);
            this.ReadPersonFrom(reader, result);
            return result;
        }

        private void ReadPersonFrom(XmlReader reader, SyndicationPerson result)
        {
            bool isEmptyElement = reader.IsEmptyElement;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    string namespaceURI = reader.NamespaceURI;
                    string localName = reader.LocalName;
                    if (!FeedUtils.IsXmlns(localName, namespaceURI))
                    {
                        string str3 = reader.Value;
                        if (!SyndicationFeedFormatter.TryParseAttribute(localName, namespaceURI, str3, result, this.Version))
                        {
                            if (this.preserveAttributeExtensions)
                            {
                                result.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                            }
                            else
                            {
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            }
                        }
                    }
                }
            }
            reader.ReadStartElement();
            if (!isEmptyElement)
            {
                XmlBuffer buffer = null;
                using (XmlDictionaryWriter writer = null)
                {
                    while (reader.IsStartElement())
                    {
                        if (reader.IsStartElement("name", "http://www.w3.org/2005/Atom"))
                        {
                            result.Name = reader.ReadElementString();
                        }
                        else
                        {
                            if (reader.IsStartElement("uri", "http://www.w3.org/2005/Atom"))
                            {
                                result.Uri = reader.ReadElementString();
                                continue;
                            }
                            if (reader.IsStartElement("email", "http://www.w3.org/2005/Atom"))
                            {
                                result.Email = reader.ReadElementString();
                                continue;
                            }
                            if (!SyndicationFeedFormatter.TryParseElement(reader, result, this.Version))
                            {
                                if (this.preserveElementExtensions)
                                {
                                    SyndicationFeedFormatter.CreateBufferIfRequiredAndWriteNode(ref buffer, ref writer, reader, this.maxExtensionSize);
                                    continue;
                                }
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                reader.Skip();
                            }
                        }
                    }
                    SyndicationFeedFormatter.LoadElementExtensions(buffer, writer, result);
                }
                reader.ReadEndElement();
            }
        }

        private TextSyndicationContent ReadTextContentFrom(XmlReader reader, string context)
        {
            return ReadTextContentFrom(reader, context, this.PreserveAttributeExtensions);
        }

        internal static TextSyndicationContent ReadTextContentFrom(XmlReader reader, string context, bool preserveAttributeExtensions)
        {
            string attribute = reader.GetAttribute("type");
            return ReadTextContentFromHelper(reader, attribute, context, preserveAttributeExtensions);
        }

        private static TextSyndicationContent ReadTextContentFromHelper(XmlReader reader, string type, string context, bool preserveAttributeExtensions)
        {
            TextSyndicationContentKind html;
            Dictionary<XmlQualifiedName, string> dictionary;
            if (string.IsNullOrEmpty(type))
            {
                type = "text";
            }
            string str5 = type;
            if (str5 != null)
            {
                if (!(str5 == "text"))
                {
                    if (str5 == "html")
                    {
                        html = TextSyndicationContentKind.Html;
                        goto Label_0081;
                    }
                    if (str5 == "xhtml")
                    {
                        html = TextSyndicationContentKind.XHtml;
                        goto Label_0081;
                    }
                }
                else
                {
                    html = TextSyndicationContentKind.Plaintext;
                    goto Label_0081;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, System.ServiceModel.SR.GetString("Atom10SpecRequiresTextConstruct", new object[] { context, type }))));
        Label_0081:
            dictionary = null;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if ((reader.LocalName != "type") || (reader.NamespaceURI != string.Empty))
                    {
                        string namespaceURI = reader.NamespaceURI;
                        string localName = reader.LocalName;
                        if (!FeedUtils.IsXmlns(localName, namespaceURI))
                        {
                            if (preserveAttributeExtensions)
                            {
                                string str3 = reader.Value;
                                if (dictionary == null)
                                {
                                    dictionary = new Dictionary<XmlQualifiedName, string>();
                                }
                                dictionary.Add(new XmlQualifiedName(localName, namespaceURI), str3);
                            }
                            else
                            {
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            }
                        }
                    }
                }
            }
            reader.MoveToElement();
            string text = (html == TextSyndicationContentKind.XHtml) ? reader.ReadInnerXml() : reader.ReadElementString();
            TextSyndicationContent content = new TextSyndicationContent(text, html);
            if (dictionary != null)
            {
                foreach (XmlQualifiedName name in dictionary.Keys)
                {
                    if (!FeedUtils.IsXmlns(name.Name, name.Namespace))
                    {
                        content.AttributeExtensions.Add(name, dictionary[name]);
                    }
                }
            }
            return content;
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
            SyndicationFeedFormatter.TraceFeedReadBegin();
            this.ReadFeed(reader);
            SyndicationFeedFormatter.TraceFeedReadEnd();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            SyndicationFeedFormatter.TraceFeedWriteBegin();
            this.WriteFeed(writer);
            SyndicationFeedFormatter.TraceFeedWriteEnd();
        }

        internal bool TryParseFeedElementFrom(XmlReader reader, SyndicationFeed result)
        {
            if (reader.IsStartElement("author", "http://www.w3.org/2005/Atom"))
            {
                result.Authors.Add(this.ReadPersonFrom(reader, result));
            }
            else if (reader.IsStartElement("category", "http://www.w3.org/2005/Atom"))
            {
                result.Categories.Add(this.ReadCategoryFrom(reader, result));
            }
            else if (reader.IsStartElement("contributor", "http://www.w3.org/2005/Atom"))
            {
                result.Contributors.Add(this.ReadPersonFrom(reader, result));
            }
            else if (reader.IsStartElement("generator", "http://www.w3.org/2005/Atom"))
            {
                result.Generator = reader.ReadElementString();
            }
            else if (reader.IsStartElement("id", "http://www.w3.org/2005/Atom"))
            {
                result.Id = reader.ReadElementString();
            }
            else if (reader.IsStartElement("link", "http://www.w3.org/2005/Atom"))
            {
                result.Links.Add(this.ReadLinkFrom(reader, result));
            }
            else if (reader.IsStartElement("logo", "http://www.w3.org/2005/Atom"))
            {
                result.ImageUrl = new Uri(reader.ReadElementString(), UriKind.RelativeOrAbsolute);
            }
            else if (reader.IsStartElement("rights", "http://www.w3.org/2005/Atom"))
            {
                result.Copyright = this.ReadTextContentFrom(reader, "//atom:feed/atom:rights[@type]");
            }
            else if (reader.IsStartElement("subtitle", "http://www.w3.org/2005/Atom"))
            {
                result.Description = this.ReadTextContentFrom(reader, "//atom:feed/atom:subtitle[@type]");
            }
            else if (reader.IsStartElement("title", "http://www.w3.org/2005/Atom"))
            {
                result.Title = this.ReadTextContentFrom(reader, "//atom:feed/atom:title[@type]");
            }
            else if (reader.IsStartElement("updated", "http://www.w3.org/2005/Atom"))
            {
                reader.ReadStartElement();
                result.LastUpdatedTime = this.DateFromString(reader.ReadString(), reader);
                reader.ReadEndElement();
            }
            else
            {
                return false;
            }
            return true;
        }

        internal bool TryParseItemElementFrom(XmlReader reader, SyndicationItem result)
        {
            if (reader.IsStartElement("author", "http://www.w3.org/2005/Atom"))
            {
                result.Authors.Add(this.ReadPersonFrom(reader, result));
            }
            else if (reader.IsStartElement("category", "http://www.w3.org/2005/Atom"))
            {
                result.Categories.Add(this.ReadCategoryFrom(reader, result));
            }
            else if (reader.IsStartElement("content", "http://www.w3.org/2005/Atom"))
            {
                result.Content = this.ReadContentFrom(reader, result);
            }
            else if (reader.IsStartElement("contributor", "http://www.w3.org/2005/Atom"))
            {
                result.Contributors.Add(this.ReadPersonFrom(reader, result));
            }
            else if (reader.IsStartElement("id", "http://www.w3.org/2005/Atom"))
            {
                result.Id = reader.ReadElementString();
            }
            else if (reader.IsStartElement("link", "http://www.w3.org/2005/Atom"))
            {
                result.Links.Add(this.ReadLinkFrom(reader, result));
            }
            else if (reader.IsStartElement("published", "http://www.w3.org/2005/Atom"))
            {
                reader.ReadStartElement();
                result.PublishDate = this.DateFromString(reader.ReadString(), reader);
                reader.ReadEndElement();
            }
            else if (reader.IsStartElement("rights", "http://www.w3.org/2005/Atom"))
            {
                result.Copyright = this.ReadTextContentFrom(reader, "//atom:feed/atom:entry/atom:rights[@type]");
            }
            else if (reader.IsStartElement("source", "http://www.w3.org/2005/Atom"))
            {
                reader.ReadStartElement();
                result.SourceFeed = this.ReadFeedFrom(reader, new SyndicationFeed(), true);
                reader.ReadEndElement();
            }
            else if (reader.IsStartElement("summary", "http://www.w3.org/2005/Atom"))
            {
                result.Summary = this.ReadTextContentFrom(reader, "//atom:feed/atom:entry/atom:summary[@type]");
            }
            else if (reader.IsStartElement("title", "http://www.w3.org/2005/Atom"))
            {
                result.Title = this.ReadTextContentFrom(reader, "//atom:feed/atom:entry/atom:title[@type]");
            }
            else if (reader.IsStartElement("updated", "http://www.w3.org/2005/Atom"))
            {
                reader.ReadStartElement();
                result.LastUpdatedTime = this.DateFromString(reader.ReadString(), reader);
                reader.ReadEndElement();
            }
            else
            {
                return false;
            }
            return true;
        }

        private void WriteCategoriesTo(XmlWriter writer, Collection<SyndicationCategory> categories)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                WriteCategory(writer, categories[i], this.Version);
            }
        }

        internal static void WriteCategory(XmlWriter writer, SyndicationCategory category, string version)
        {
            writer.WriteStartElement("category", "http://www.w3.org/2005/Atom");
            SyndicationFeedFormatter.WriteAttributeExtensions(writer, category, version);
            string str = category.Name ?? string.Empty;
            if (!category.AttributeExtensions.ContainsKey(Atom10Term))
            {
                writer.WriteAttributeString("term", str);
            }
            if (!string.IsNullOrEmpty(category.Label) && !category.AttributeExtensions.ContainsKey(Atom10Label))
            {
                writer.WriteAttributeString("label", category.Label);
            }
            if (!string.IsNullOrEmpty(category.Scheme) && !category.AttributeExtensions.ContainsKey(Atom10Scheme))
            {
                writer.WriteAttributeString("scheme", category.Scheme);
            }
            SyndicationFeedFormatter.WriteElementExtensions(writer, category, version);
            writer.WriteEndElement();
        }

        internal void WriteContentTo(XmlWriter writer, string elementName, SyndicationContent content)
        {
            if (content != null)
            {
                content.WriteTo(writer, elementName, "http://www.w3.org/2005/Atom");
            }
        }

        internal void WriteElement(XmlWriter writer, string elementName, string value)
        {
            if (value != null)
            {
                writer.WriteElementString(elementName, "http://www.w3.org/2005/Atom", value);
            }
        }

        private void WriteFeed(XmlWriter writer)
        {
            if (base.Feed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FeedFormatterDoesNotHaveFeed")));
            }
            this.WriteFeedTo(writer, base.Feed, false);
        }

        internal void WriteFeedAuthorsTo(XmlWriter writer, Collection<SyndicationPerson> authors)
        {
            for (int i = 0; i < authors.Count; i++)
            {
                SyndicationPerson p = authors[i];
                this.WritePersonTo(writer, p, "author");
            }
        }

        internal void WriteFeedContributorsTo(XmlWriter writer, Collection<SyndicationPerson> contributors)
        {
            for (int i = 0; i < contributors.Count; i++)
            {
                SyndicationPerson p = contributors[i];
                this.WritePersonTo(writer, p, "contributor");
            }
        }

        internal void WriteFeedLastUpdatedTimeTo(XmlWriter writer, DateTimeOffset lastUpdatedTime, bool isRequired)
        {
            if ((lastUpdatedTime == DateTimeOffset.MinValue) && isRequired)
            {
                lastUpdatedTime = DateTimeOffset.UtcNow;
            }
            if (lastUpdatedTime != DateTimeOffset.MinValue)
            {
                this.WriteElement(writer, "updated", this.AsString(lastUpdatedTime));
            }
        }

        private void WriteFeedTo(XmlWriter writer, SyndicationFeed feed, bool isSourceFeed)
        {
            if (!isSourceFeed)
            {
                if (!string.IsNullOrEmpty(feed.Language))
                {
                    writer.WriteAttributeString("xml", "lang", "http://www.w3.org/XML/1998/namespace", feed.Language);
                }
                if (feed.BaseUri != null)
                {
                    writer.WriteAttributeString("xml", "base", "http://www.w3.org/XML/1998/namespace", FeedUtils.GetUriString(feed.BaseUri));
                }
                SyndicationFeedFormatter.WriteAttributeExtensions(writer, feed, this.Version);
            }
            bool isRequired = !isSourceFeed;
            TextSyndicationContent title = feed.Title;
            if (isRequired)
            {
                title = title ?? new TextSyndicationContent(string.Empty);
            }
            this.WriteContentTo(writer, "title", title);
            this.WriteContentTo(writer, "subtitle", feed.Description);
            string id = feed.Id;
            if (isRequired)
            {
                id = id ?? idGenerator.Next();
            }
            this.WriteElement(writer, "id", id);
            this.WriteContentTo(writer, "rights", feed.Copyright);
            this.WriteFeedLastUpdatedTimeTo(writer, feed.LastUpdatedTime, isRequired);
            this.WriteCategoriesTo(writer, feed.Categories);
            if (feed.ImageUrl != null)
            {
                this.WriteElement(writer, "logo", feed.ImageUrl.ToString());
            }
            this.WriteFeedAuthorsTo(writer, feed.Authors);
            this.WriteFeedContributorsTo(writer, feed.Contributors);
            this.WriteElement(writer, "generator", feed.Generator);
            for (int i = 0; i < feed.Links.Count; i++)
            {
                this.WriteLink(writer, feed.Links[i], feed.BaseUri);
            }
            SyndicationFeedFormatter.WriteElementExtensions(writer, feed, this.Version);
            if (!isSourceFeed)
            {
                this.WriteItems(writer, feed.Items, feed.BaseUri);
            }
        }

        protected virtual void WriteItem(XmlWriter writer, SyndicationItem item, Uri feedBaseUri)
        {
            SyndicationFeedFormatter.TraceItemWriteBegin();
            writer.WriteStartElement("entry", "http://www.w3.org/2005/Atom");
            this.WriteItemContents(writer, item, feedBaseUri);
            writer.WriteEndElement();
            SyndicationFeedFormatter.TraceItemWriteEnd();
        }

        internal void WriteItemAuthorsTo(XmlWriter writer, Collection<SyndicationPerson> authors)
        {
            for (int i = 0; i < authors.Count; i++)
            {
                SyndicationPerson p = authors[i];
                this.WritePersonTo(writer, p, "author");
            }
        }

        internal void WriteItemContents(XmlWriter dictWriter, SyndicationItem item)
        {
            this.WriteItemContents(dictWriter, item, null);
        }

        private void WriteItemContents(XmlWriter dictWriter, SyndicationItem item, Uri feedBaseUri)
        {
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(feedBaseUri, item.BaseUri);
            if (baseUriToWrite != null)
            {
                dictWriter.WriteAttributeString("xml", "base", "http://www.w3.org/XML/1998/namespace", FeedUtils.GetUriString(baseUriToWrite));
            }
            SyndicationFeedFormatter.WriteAttributeExtensions(dictWriter, item, this.Version);
            string str = item.Id ?? idGenerator.Next();
            this.WriteElement(dictWriter, "id", str);
            TextSyndicationContent content = item.Title ?? new TextSyndicationContent(string.Empty);
            this.WriteContentTo(dictWriter, "title", content);
            this.WriteContentTo(dictWriter, "summary", item.Summary);
            if (item.PublishDate != DateTimeOffset.MinValue)
            {
                dictWriter.WriteElementString("published", "http://www.w3.org/2005/Atom", this.AsString(item.PublishDate));
            }
            this.WriteItemLastUpdatedTimeTo(dictWriter, item.LastUpdatedTime);
            this.WriteItemAuthorsTo(dictWriter, item.Authors);
            this.WriteItemContributorsTo(dictWriter, item.Contributors);
            for (int i = 0; i < item.Links.Count; i++)
            {
                this.WriteLink(dictWriter, item.Links[i], item.BaseUri);
            }
            this.WriteCategoriesTo(dictWriter, item.Categories);
            this.WriteContentTo(dictWriter, "content", item.Content);
            this.WriteContentTo(dictWriter, "rights", item.Copyright);
            if (item.SourceFeed != null)
            {
                dictWriter.WriteStartElement("source", "http://www.w3.org/2005/Atom");
                this.WriteFeedTo(dictWriter, item.SourceFeed, true);
                dictWriter.WriteEndElement();
            }
            SyndicationFeedFormatter.WriteElementExtensions(dictWriter, item, this.Version);
        }

        internal void WriteItemContributorsTo(XmlWriter writer, Collection<SyndicationPerson> contributors)
        {
            for (int i = 0; i < contributors.Count; i++)
            {
                SyndicationPerson p = contributors[i];
                this.WritePersonTo(writer, p, "contributor");
            }
        }

        internal void WriteItemLastUpdatedTimeTo(XmlWriter writer, DateTimeOffset lastUpdatedTime)
        {
            if (lastUpdatedTime == DateTimeOffset.MinValue)
            {
                lastUpdatedTime = DateTimeOffset.UtcNow;
            }
            writer.WriteElementString("updated", "http://www.w3.org/2005/Atom", this.AsString(lastUpdatedTime));
        }

        protected virtual void WriteItems(XmlWriter writer, IEnumerable<SyndicationItem> items, Uri feedBaseUri)
        {
            if (items != null)
            {
                foreach (SyndicationItem item in items)
                {
                    this.WriteItem(writer, item, feedBaseUri);
                }
            }
        }

        internal void WriteLink(XmlWriter writer, SyndicationLink link, Uri baseUri)
        {
            writer.WriteStartElement("link", "http://www.w3.org/2005/Atom");
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(baseUri, link.BaseUri);
            if (baseUriToWrite != null)
            {
                writer.WriteAttributeString("xml", "base", "http://www.w3.org/XML/1998/namespace", FeedUtils.GetUriString(baseUriToWrite));
            }
            link.WriteAttributeExtensions(writer, "Atom10");
            if (!string.IsNullOrEmpty(link.RelationshipType) && !link.AttributeExtensions.ContainsKey(Atom10Relative))
            {
                writer.WriteAttributeString("rel", link.RelationshipType);
            }
            if (!string.IsNullOrEmpty(link.MediaType) && !link.AttributeExtensions.ContainsKey(Atom10Type))
            {
                writer.WriteAttributeString("type", link.MediaType);
            }
            if (!string.IsNullOrEmpty(link.Title) && !link.AttributeExtensions.ContainsKey(Atom10Title))
            {
                writer.WriteAttributeString("title", link.Title);
            }
            if ((link.Length != 0L) && !link.AttributeExtensions.ContainsKey(Atom10Length))
            {
                writer.WriteAttributeString("length", Convert.ToString(link.Length, CultureInfo.InvariantCulture));
            }
            if (!link.AttributeExtensions.ContainsKey(Atom10Href))
            {
                writer.WriteAttributeString("href", FeedUtils.GetUriString(link.Uri));
            }
            link.WriteElementExtensions(writer, "Atom10");
            writer.WriteEndElement();
        }

        private void WritePersonTo(XmlWriter writer, SyndicationPerson p, string elementName)
        {
            writer.WriteStartElement(elementName, "http://www.w3.org/2005/Atom");
            SyndicationFeedFormatter.WriteAttributeExtensions(writer, p, this.Version);
            this.WriteElement(writer, "name", p.Name);
            if (!string.IsNullOrEmpty(p.Uri))
            {
                writer.WriteElementString("uri", "http://www.w3.org/2005/Atom", p.Uri);
            }
            if (!string.IsNullOrEmpty(p.Email))
            {
                writer.WriteElementString("email", "http://www.w3.org/2005/Atom", p.Email);
            }
            SyndicationFeedFormatter.WriteElementExtensions(writer, p, this.Version);
            writer.WriteEndElement();
        }

        public override void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            SyndicationFeedFormatter.TraceFeedWriteBegin();
            writer.WriteStartElement("feed", "http://www.w3.org/2005/Atom");
            this.WriteFeed(writer);
            writer.WriteEndElement();
            SyndicationFeedFormatter.TraceFeedWriteEnd();
        }

        protected System.Type FeedType
        {
            get
            {
                return this.feedType;
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

