namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlRoot(ElementName="rss", Namespace=""), TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class Rss20FeedFormatter : SyndicationFeedFormatter, IXmlSerializable
    {
        private Atom10FeedFormatter atomSerializer;
        private Type feedType;
        private int maxExtensionSize;
        private bool preserveAttributeExtensions;
        private bool preserveElementExtensions;
        private const string Rfc822OutputLocalDateTimeFormat = "ddd, dd MMM yyyy HH:mm:ss zzz";
        private const string Rfc822OutputUtcDateTimeFormat = "ddd, dd MMM yyyy HH:mm:ss Z";
        private static readonly XmlQualifiedName Rss20Domain = new XmlQualifiedName("domain", string.Empty);
        private static readonly XmlQualifiedName Rss20Length = new XmlQualifiedName("length", string.Empty);
        private static readonly XmlQualifiedName Rss20Type = new XmlQualifiedName("type", string.Empty);
        private static readonly XmlQualifiedName Rss20Url = new XmlQualifiedName("url", string.Empty);
        private bool serializeExtensionsAsAtom;

        public Rss20FeedFormatter() : this(typeof(SyndicationFeed))
        {
        }

        public Rss20FeedFormatter(SyndicationFeed feedToWrite) : this(feedToWrite, true)
        {
        }

        public Rss20FeedFormatter(Type feedTypeToCreate)
        {
            if (feedTypeToCreate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("feedTypeToCreate");
            }
            if (!typeof(SyndicationFeed).IsAssignableFrom(feedTypeToCreate))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("feedTypeToCreate", System.ServiceModel.SR.GetString("InvalidObjectTypePassed", new object[] { "feedTypeToCreate", "SyndicationFeed" }));
            }
            this.serializeExtensionsAsAtom = true;
            this.maxExtensionSize = 0x7fffffff;
            this.preserveElementExtensions = true;
            this.preserveAttributeExtensions = true;
            this.atomSerializer = new Atom10FeedFormatter(feedTypeToCreate);
            this.feedType = feedTypeToCreate;
        }

        public Rss20FeedFormatter(SyndicationFeed feedToWrite, bool serializeExtensionsAsAtom) : base(feedToWrite)
        {
            this.serializeExtensionsAsAtom = serializeExtensionsAsAtom;
            this.maxExtensionSize = 0x7fffffff;
            this.preserveElementExtensions = true;
            this.preserveAttributeExtensions = true;
            this.atomSerializer = new Atom10FeedFormatter(base.Feed);
            this.feedType = feedToWrite.GetType();
        }

        private string AsString(DateTimeOffset dateTime)
        {
            if (dateTime.Offset == Atom10FeedFormatter.zeroOffset)
            {
                return dateTime.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss Z", CultureInfo.InvariantCulture);
            }
            StringBuilder builder = new StringBuilder(dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture));
            builder.Remove(builder.Length - 3, 1);
            return builder.ToString();
        }

        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return reader.IsStartElement("rss", "");
        }

        protected override SyndicationFeed CreateFeedInstance()
        {
            return SyndicationFeedFormatter.CreateFeedInstance(this.feedType);
        }

        private static DateTimeOffset DateFromString(string dateTimeString, XmlReader reader)
        {
            int num;
            bool flag2;
            DateTimeOffset offset;
            string str3;
            StringBuilder stringBuilder = new StringBuilder(dateTimeString.Trim());
            if (stringBuilder.Length < 0x12)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingDateTime")));
            }
            if (stringBuilder[3] == ',')
            {
                stringBuilder.Remove(0, 4);
                RemoveExtraWhiteSpaceAtStart(stringBuilder);
            }
            ReplaceMultipleWhiteSpaceWithSingleWhiteSpace(stringBuilder);
            if (!char.IsDigit(stringBuilder[1]))
            {
                stringBuilder.Insert(0, '0');
            }
            if (stringBuilder.Length < 0x13)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingDateTime")));
            }
            bool flag = stringBuilder[0x11] == ':';
            if (flag)
            {
                num = 0x15;
            }
            else
            {
                num = 0x12;
            }
            string str = stringBuilder.ToString().Substring(num);
            stringBuilder.Remove(num, stringBuilder.Length - num);
            stringBuilder.Append(NormalizeTimeZone(str, out flag2));
            string input = stringBuilder.ToString();
            if (flag)
            {
                str3 = "dd MMM yyyy HH:mm:ss zzz";
            }
            else
            {
                str3 = "dd MMM yyyy HH:mm zzz";
            }
            if (!DateTimeOffset.TryParseExact(input, str3, CultureInfo.InvariantCulture.DateTimeFormat, flag2 ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None, out offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingDateTime")));
            }
            return offset;
        }

        private static string NormalizeTimeZone(string rfc822TimeZone, out bool isUtc)
        {
            isUtc = false;
            if ((rfc822TimeZone[0] == '+') || (rfc822TimeZone[0] == '-'))
            {
                StringBuilder builder = new StringBuilder(rfc822TimeZone);
                if (builder.Length == 4)
                {
                    builder.Insert(1, '0');
                }
                builder.Insert(3, ':');
                return builder.ToString();
            }
            switch (rfc822TimeZone)
            {
                case "UT":
                case "Z":
                    isUtc = true;
                    return "-00:00";

                case "GMT":
                    return "-00:00";

                case "A":
                    return "-01:00";

                case "B":
                    return "-02:00";

                case "C":
                    return "-03:00";

                case "D":
                case "EDT":
                    return "-04:00";

                case "E":
                case "EST":
                case "CDT":
                    return "-05:00";

                case "F":
                case "CST":
                case "MDT":
                    return "-06:00";

                case "G":
                case "MST":
                case "PDT":
                    return "-07:00";

                case "H":
                case "PST":
                    return "-08:00";

                case "I":
                    return "-09:00";

                case "K":
                    return "-10:00";

                case "L":
                    return "-11:00";

                case "M":
                    return "-12:00";

                case "N":
                    return "+01:00";

                case "O":
                    return "+02:00";

                case "P":
                    return "+03:00";

                case "Q":
                    return "+04:00";

                case "R":
                    return "+05:00";

                case "S":
                    return "+06:00";

                case "T":
                    return "+07:00";

                case "U":
                    return "+08:00";

                case "V":
                    return "+09:00";

                case "W":
                    return "+10:00";

                case "X":
                    return "+11:00";

                case "Y":
                    return "+12:00";
            }
            return "";
        }

        private SyndicationLink ReadAlternateLink(XmlReader reader, Uri baseUri)
        {
            SyndicationLink link = new SyndicationLink {
                BaseUri = baseUri,
                RelationshipType = "alternate"
            };
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if ((reader.LocalName == "base") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                    {
                        link.BaseUri = FeedUtils.CombineXmlBase(link.BaseUri, reader.Value);
                    }
                    else if (!FeedUtils.IsXmlns(reader.LocalName, reader.NamespaceURI))
                    {
                        if (this.PreserveAttributeExtensions)
                        {
                            link.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                            continue;
                        }
                        SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                    }
                }
            }
            reader.ReadStartElement();
            link.Uri = new Uri(reader.ReadString(), UriKind.RelativeOrAbsolute);
            reader.ReadEndElement();
            return link;
        }

        private void ReadCategory(XmlReader reader, SyndicationCategory category)
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
                        if ((localName == "domain") && (namespaceURI == ""))
                        {
                            category.Scheme = str3;
                        }
                        else if (!SyndicationFeedFormatter.TryParseAttribute(localName, namespaceURI, str3, category, this.Version))
                        {
                            if (this.preserveAttributeExtensions)
                            {
                                category.AttributeExtensions.Add(new XmlQualifiedName(localName, namespaceURI), str3);
                                continue;
                            }
                            SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                        }
                    }
                }
            }
            reader.ReadStartElement("category", "");
            if (!isEmptyElement)
            {
                category.Name = reader.ReadString();
                reader.ReadEndElement();
            }
        }

        private SyndicationCategory ReadCategory(XmlReader reader, SyndicationFeed feed)
        {
            SyndicationCategory category = SyndicationFeedFormatter.CreateCategory(feed);
            this.ReadCategory(reader, category);
            return category;
        }

        private SyndicationCategory ReadCategory(XmlReader reader, SyndicationItem item)
        {
            SyndicationCategory category = SyndicationFeedFormatter.CreateCategory(item);
            this.ReadCategory(reader, category);
            return category;
        }

        private void ReadFeed(XmlReader reader)
        {
            this.SetFeed(this.CreateFeedInstance());
            this.ReadXml(reader, base.Feed);
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
                reader.MoveToContent();
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
                                    result.AttributeExtensions.Add(new XmlQualifiedName(localName, namespaceURI), str3);
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
                    string uriString = null;
                    XmlDictionaryWriter extWriter = null;
                    bool flag2 = false;
                    try
                    {
                        XmlBuffer buffer = null;
                        while (reader.IsStartElement())
                        {
                            if (reader.IsStartElement("title", ""))
                            {
                                result.Title = new TextSyndicationContent(reader.ReadElementString());
                                continue;
                            }
                            if (reader.IsStartElement("link", ""))
                            {
                                result.Links.Add(this.ReadAlternateLink(reader, result.BaseUri));
                                flag2 = true;
                                continue;
                            }
                            if (reader.IsStartElement("description", ""))
                            {
                                result.Summary = new TextSyndicationContent(reader.ReadElementString());
                                continue;
                            }
                            if (reader.IsStartElement("author", ""))
                            {
                                result.Authors.Add(this.ReadPerson(reader, result));
                                continue;
                            }
                            if (reader.IsStartElement("category", ""))
                            {
                                result.Categories.Add(this.ReadCategory(reader, result));
                                continue;
                            }
                            if (reader.IsStartElement("enclosure", ""))
                            {
                                result.Links.Add(this.ReadMediaEnclosure(reader, result.BaseUri));
                                continue;
                            }
                            if (reader.IsStartElement("guid", ""))
                            {
                                bool flag3 = true;
                                string attribute = reader.GetAttribute("isPermaLink", "");
                                if ((attribute != null) && (attribute.ToUpperInvariant() == "FALSE"))
                                {
                                    flag3 = false;
                                }
                                result.Id = reader.ReadElementString();
                                if (flag3)
                                {
                                    uriString = result.Id;
                                }
                                continue;
                            }
                            if (reader.IsStartElement("pubDate", ""))
                            {
                                reader.ReadStartElement();
                                string dateTimeString = reader.ReadString();
                                result.PublishDate = DateFromString(dateTimeString, reader);
                                reader.ReadEndElement();
                                continue;
                            }
                            if (reader.IsStartElement("source", ""))
                            {
                                SyndicationFeed feed = new SyndicationFeed();
                                if (reader.HasAttributes)
                                {
                                    while (reader.MoveToNextAttribute())
                                    {
                                        string ns = reader.NamespaceURI;
                                        string name = reader.LocalName;
                                        if (!FeedUtils.IsXmlns(name, ns))
                                        {
                                            string str9 = reader.Value;
                                            if ((name == "url") && (ns == ""))
                                            {
                                                feed.Links.Add(SyndicationLink.CreateSelfLink(new Uri(str9, UriKind.RelativeOrAbsolute)));
                                            }
                                            else if (!FeedUtils.IsXmlns(name, ns))
                                            {
                                                if (this.preserveAttributeExtensions)
                                                {
                                                    feed.AttributeExtensions.Add(new XmlQualifiedName(name, ns), str9);
                                                    continue;
                                                }
                                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                            }
                                        }
                                    }
                                }
                                reader.ReadStartElement();
                                string text = reader.ReadString();
                                reader.ReadEndElement();
                                feed.Title = new TextSyndicationContent(text);
                                result.SourceFeed = feed;
                                continue;
                            }
                            bool flag4 = this.serializeExtensionsAsAtom && this.atomSerializer.TryParseItemElementFrom(reader, result);
                            if (!flag4)
                            {
                                flag4 = SyndicationFeedFormatter.TryParseElement(reader, result, this.Version);
                            }
                            if (!flag4)
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
                        SyndicationFeedFormatter.LoadElementExtensions(buffer, extWriter, result);
                    }
                    finally
                    {
                        if (extWriter != null)
                        {
                            extWriter.Dispose();
                        }
                    }
                    reader.ReadEndElement();
                    if (!flag2 && (uriString != null))
                    {
                        result.Links.Add(SyndicationLink.CreateAlternateLink(new Uri(uriString, UriKind.RelativeOrAbsolute)));
                        flag2 = true;
                    }
                    if ((result.Content == null) && !flag2)
                    {
                        result.Content = result.Summary;
                        result.Summary = null;
                    }
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
            while (reader.IsStartElement("item", ""))
            {
                alloweds.Add(this.ReadItem(reader, feed));
            }
            areAllItemsRead = true;
            return alloweds;
        }

        private SyndicationLink ReadMediaEnclosure(XmlReader reader, Uri baseUri)
        {
            SyndicationLink link = new SyndicationLink {
                BaseUri = baseUri,
                RelationshipType = "enclosure"
            };
            bool isEmptyElement = reader.IsEmptyElement;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    string namespaceURI = reader.NamespaceURI;
                    string localName = reader.LocalName;
                    if ((localName == "base") && (namespaceURI == "http://www.w3.org/XML/1998/namespace"))
                    {
                        link.BaseUri = FeedUtils.CombineXmlBase(link.BaseUri, reader.Value);
                    }
                    else if (!FeedUtils.IsXmlns(localName, namespaceURI))
                    {
                        string uriString = reader.Value;
                        if ((localName == "url") && (namespaceURI == ""))
                        {
                            link.Uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
                            continue;
                        }
                        if ((localName == "type") && (namespaceURI == ""))
                        {
                            link.MediaType = uriString;
                            continue;
                        }
                        if ((localName == "length") && (namespaceURI == ""))
                        {
                            link.Length = !string.IsNullOrEmpty(uriString) ? Convert.ToInt64(uriString, CultureInfo.InvariantCulture.NumberFormat) : 0L;
                            continue;
                        }
                        if (!FeedUtils.IsXmlns(localName, namespaceURI))
                        {
                            if (this.preserveAttributeExtensions)
                            {
                                link.AttributeExtensions.Add(new XmlQualifiedName(localName, namespaceURI), uriString);
                                continue;
                            }
                            SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                        }
                    }
                }
            }
            reader.ReadStartElement("enclosure", "");
            if (!isEmptyElement)
            {
                reader.ReadEndElement();
            }
            return link;
        }

        private SyndicationPerson ReadPerson(XmlReader reader, SyndicationFeed feed)
        {
            SyndicationPerson person = SyndicationFeedFormatter.CreatePerson(feed);
            this.ReadPerson(reader, person);
            return person;
        }

        private SyndicationPerson ReadPerson(XmlReader reader, SyndicationItem item)
        {
            SyndicationPerson person = SyndicationFeedFormatter.CreatePerson(item);
            this.ReadPerson(reader, person);
            return person;
        }

        private void ReadPerson(XmlReader reader, SyndicationPerson person)
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
                        if (!SyndicationFeedFormatter.TryParseAttribute(localName, namespaceURI, str3, person, this.Version))
                        {
                            if (this.preserveAttributeExtensions)
                            {
                                person.AttributeExtensions.Add(new XmlQualifiedName(localName, namespaceURI), str3);
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
                string str4 = reader.ReadString();
                reader.ReadEndElement();
                person.Email = str4;
            }
        }

        private void ReadXml(XmlReader reader, SyndicationFeed result)
        {
            try
            {
                string str = null;
                reader.MoveToContent();
                string attribute = reader.GetAttribute("version", "");
                if (attribute != "2.0")
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(FeedUtils.AddLineInfo(reader, System.ServiceModel.SR.GetString("UnsupportedRssVersion", new object[] { attribute }))));
                }
                if (reader.AttributeCount > 1)
                {
                    string str3 = reader.GetAttribute("base", "http://www.w3.org/XML/1998/namespace");
                    if (!string.IsNullOrEmpty(str3))
                    {
                        str = str3;
                    }
                }
                reader.ReadStartElement();
                reader.MoveToContent();
                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        string namespaceURI = reader.NamespaceURI;
                        string localName = reader.LocalName;
                        if ((localName == "base") && (namespaceURI == "http://www.w3.org/XML/1998/namespace"))
                        {
                            str = reader.Value;
                        }
                        else if (!FeedUtils.IsXmlns(localName, namespaceURI) && !FeedUtils.IsXmlSchemaType(localName, namespaceURI))
                        {
                            string str6 = reader.Value;
                            if (!SyndicationFeedFormatter.TryParseAttribute(localName, namespaceURI, str6, result, this.Version))
                            {
                                if (this.preserveAttributeExtensions)
                                {
                                    result.AttributeExtensions.Add(new XmlQualifiedName(localName, namespaceURI), str6);
                                    continue;
                                }
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(str))
                {
                    result.BaseUri = new Uri(str, UriKind.RelativeOrAbsolute);
                }
                bool areAllItemsRead = true;
                bool flag2 = false;
                reader.ReadStartElement("channel", "");
                XmlBuffer buffer = null;
                using (XmlDictionaryWriter writer = null)
                {
                    while (reader.IsStartElement())
                    {
                        if (reader.IsStartElement("title", ""))
                        {
                            result.Title = new TextSyndicationContent(reader.ReadElementString());
                        }
                        else
                        {
                            if (reader.IsStartElement("link", ""))
                            {
                                result.Links.Add(this.ReadAlternateLink(reader, result.BaseUri));
                                continue;
                            }
                            if (reader.IsStartElement("description", ""))
                            {
                                result.Description = new TextSyndicationContent(reader.ReadElementString());
                                continue;
                            }
                            if (reader.IsStartElement("language", ""))
                            {
                                result.Language = reader.ReadElementString();
                                continue;
                            }
                            if (reader.IsStartElement("copyright", ""))
                            {
                                result.Copyright = new TextSyndicationContent(reader.ReadElementString());
                                continue;
                            }
                            if (reader.IsStartElement("managingEditor", ""))
                            {
                                result.Authors.Add(this.ReadPerson(reader, result));
                                continue;
                            }
                            if (reader.IsStartElement("lastBuildDate", ""))
                            {
                                reader.ReadStartElement();
                                result.LastUpdatedTime = DateFromString(reader.ReadString(), reader);
                                reader.ReadEndElement();
                                continue;
                            }
                            if (reader.IsStartElement("category", ""))
                            {
                                result.Categories.Add(this.ReadCategory(reader, result));
                                continue;
                            }
                            if (reader.IsStartElement("generator", ""))
                            {
                                result.Generator = reader.ReadElementString();
                                continue;
                            }
                            if (reader.IsStartElement("image", ""))
                            {
                                reader.ReadStartElement();
                                while (reader.IsStartElement())
                                {
                                    if (reader.IsStartElement("url", ""))
                                    {
                                        result.ImageUrl = new Uri(reader.ReadElementString(), UriKind.RelativeOrAbsolute);
                                    }
                                    else
                                    {
                                        SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                        reader.Skip();
                                    }
                                }
                                reader.ReadEndElement();
                                continue;
                            }
                            if (reader.IsStartElement("item", ""))
                            {
                                if (flag2)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("FeedHasNonContiguousItems", new object[] { base.GetType().ToString() })));
                                }
                                result.Items = this.ReadItems(reader, result, out areAllItemsRead);
                                flag2 = true;
                                if (areAllItemsRead)
                                {
                                    continue;
                                }
                                break;
                            }
                            bool flag3 = this.serializeExtensionsAsAtom && this.atomSerializer.TryParseFeedElementFrom(reader, result);
                            if (!flag3)
                            {
                                flag3 = SyndicationFeedFormatter.TryParseElement(reader, result, this.Version);
                            }
                            if (!flag3)
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
                    }
                    SyndicationFeedFormatter.LoadElementExtensions(buffer, writer, result);
                }
                if (areAllItemsRead)
                {
                    reader.ReadEndElement();
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
        }

        private static void RemoveExtraWhiteSpaceAtStart(StringBuilder stringBuilder)
        {
            int length = 0;
            while (length < stringBuilder.Length)
            {
                if (!char.IsWhiteSpace(stringBuilder[length]))
                {
                    break;
                }
                length++;
            }
            if (length > 0)
            {
                stringBuilder.Remove(0, length);
            }
        }

        private static void ReplaceMultipleWhiteSpaceWithSingleWhiteSpace(StringBuilder builder)
        {
            int num = 0;
            int startIndex = -1;
            while (num < builder.Length)
            {
                if (char.IsWhiteSpace(builder[num]))
                {
                    if (startIndex < 0)
                    {
                        startIndex = num;
                        builder[num] = ' ';
                    }
                }
                else if (startIndex >= 0)
                {
                    if (num > (startIndex + 1))
                    {
                        builder.Remove(startIndex, (num - startIndex) - 1);
                        num = startIndex + 1;
                    }
                    startIndex = -1;
                }
                num++;
            }
        }

        protected internal override void SetFeed(SyndicationFeed feed)
        {
            base.SetFeed(feed);
            this.atomSerializer.SetFeed(base.Feed);
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

        internal static void TraceExtensionsIgnoredOnWrite(string message)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf0022, System.ServiceModel.SR.GetString(message));
            }
        }

        private void WriteAlternateLink(XmlWriter writer, SyndicationLink link, Uri baseUri)
        {
            writer.WriteStartElement("link", "");
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(baseUri, link.BaseUri);
            if (baseUriToWrite != null)
            {
                writer.WriteAttributeString("xml", "base", "http://www.w3.org/XML/1998/namespace", FeedUtils.GetUriString(baseUriToWrite));
            }
            link.WriteAttributeExtensions(writer, "Rss20");
            writer.WriteString(FeedUtils.GetUriString(link.Uri));
            writer.WriteEndElement();
        }

        private void WriteCategory(XmlWriter writer, SyndicationCategory category)
        {
            if (category != null)
            {
                writer.WriteStartElement("category", "");
                SyndicationFeedFormatter.WriteAttributeExtensions(writer, category, this.Version);
                if (!string.IsNullOrEmpty(category.Scheme) && !category.AttributeExtensions.ContainsKey(Rss20Domain))
                {
                    writer.WriteAttributeString("domain", "", category.Scheme);
                }
                writer.WriteString(category.Name);
                writer.WriteEndElement();
            }
        }

        private void WriteFeed(XmlWriter writer)
        {
            if (base.Feed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FeedFormatterDoesNotHaveFeed")));
            }
            if (this.serializeExtensionsAsAtom)
            {
                writer.WriteAttributeString("xmlns", "a10", null, "http://www.w3.org/2005/Atom");
            }
            writer.WriteAttributeString("version", "2.0");
            writer.WriteStartElement("channel", "");
            if (base.Feed.BaseUri != null)
            {
                writer.WriteAttributeString("xml", "base", "http://www.w3.org/XML/1998/namespace", FeedUtils.GetUriString(base.Feed.BaseUri));
            }
            SyndicationFeedFormatter.WriteAttributeExtensions(writer, base.Feed, this.Version);
            string str = (base.Feed.Title != null) ? base.Feed.Title.Text : string.Empty;
            writer.WriteElementString("title", "", str);
            SyndicationLink link = null;
            for (int i = 0; i < base.Feed.Links.Count; i++)
            {
                if (base.Feed.Links[i].RelationshipType == "alternate")
                {
                    link = base.Feed.Links[i];
                    this.WriteAlternateLink(writer, link, base.Feed.BaseUri);
                    break;
                }
            }
            string str2 = (base.Feed.Description != null) ? base.Feed.Description.Text : string.Empty;
            writer.WriteElementString("description", "", str2);
            if (base.Feed.Language != null)
            {
                writer.WriteElementString("language", base.Feed.Language);
            }
            if (base.Feed.Copyright != null)
            {
                writer.WriteElementString("copyright", "", base.Feed.Copyright.Text);
            }
            if ((base.Feed.Authors.Count == 1) && (base.Feed.Authors[0].Email != null))
            {
                this.WritePerson(writer, "managingEditor", base.Feed.Authors[0]);
            }
            else if (this.serializeExtensionsAsAtom)
            {
                this.atomSerializer.WriteFeedAuthorsTo(writer, base.Feed.Authors);
            }
            else
            {
                TraceExtensionsIgnoredOnWrite("FeedAuthorsIgnoredOnWrite");
            }
            if (base.Feed.LastUpdatedTime > DateTimeOffset.MinValue)
            {
                writer.WriteStartElement("lastBuildDate");
                writer.WriteString(this.AsString(base.Feed.LastUpdatedTime));
                writer.WriteEndElement();
            }
            for (int j = 0; j < base.Feed.Categories.Count; j++)
            {
                this.WriteCategory(writer, base.Feed.Categories[j]);
            }
            if (!string.IsNullOrEmpty(base.Feed.Generator))
            {
                writer.WriteElementString("generator", base.Feed.Generator);
            }
            if (base.Feed.Contributors.Count > 0)
            {
                if (this.serializeExtensionsAsAtom)
                {
                    this.atomSerializer.WriteFeedContributorsTo(writer, base.Feed.Contributors);
                }
                else
                {
                    TraceExtensionsIgnoredOnWrite("FeedContributorsIgnoredOnWrite");
                }
            }
            if (base.Feed.ImageUrl != null)
            {
                writer.WriteStartElement("image");
                writer.WriteElementString("url", FeedUtils.GetUriString(base.Feed.ImageUrl));
                writer.WriteElementString("title", "", str);
                string str3 = (link != null) ? FeedUtils.GetUriString(link.Uri) : string.Empty;
                writer.WriteElementString("link", "", str3);
                writer.WriteEndElement();
            }
            if (this.serializeExtensionsAsAtom)
            {
                this.atomSerializer.WriteElement(writer, "id", base.Feed.Id);
                bool flag = true;
                for (int k = 0; k < base.Feed.Links.Count; k++)
                {
                    if ((base.Feed.Links[k].RelationshipType == "alternate") && flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        this.atomSerializer.WriteLink(writer, base.Feed.Links[k], base.Feed.BaseUri);
                    }
                }
            }
            else
            {
                if (base.Feed.Id != null)
                {
                    TraceExtensionsIgnoredOnWrite("FeedIdIgnoredOnWrite");
                }
                if (base.Feed.Links.Count > 1)
                {
                    TraceExtensionsIgnoredOnWrite("FeedLinksIgnoredOnWrite");
                }
            }
            SyndicationFeedFormatter.WriteElementExtensions(writer, base.Feed, this.Version);
            this.WriteItems(writer, base.Feed.Items, base.Feed.BaseUri);
            writer.WriteEndElement();
        }

        protected virtual void WriteItem(XmlWriter writer, SyndicationItem item, Uri feedBaseUri)
        {
            SyndicationFeedFormatter.TraceItemWriteBegin();
            writer.WriteStartElement("item", "");
            this.WriteItemContents(writer, item, feedBaseUri);
            writer.WriteEndElement();
            SyndicationFeedFormatter.TraceItemWriteEnd();
        }

        internal void WriteItemContents(XmlWriter writer, SyndicationItem item)
        {
            this.WriteItemContents(writer, item, null);
        }

        private void WriteItemContents(XmlWriter writer, SyndicationItem item, Uri feedBaseUri)
        {
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(feedBaseUri, item.BaseUri);
            if (baseUriToWrite != null)
            {
                writer.WriteAttributeString("xml", "base", "http://www.w3.org/XML/1998/namespace", FeedUtils.GetUriString(baseUriToWrite));
            }
            SyndicationFeedFormatter.WriteAttributeExtensions(writer, item, this.Version);
            string str = item.Id ?? string.Empty;
            bool flag = false;
            SyndicationLink link = null;
            for (int i = 0; i < item.Links.Count; i++)
            {
                if (item.Links[i].RelationshipType == "alternate")
                {
                    if (link == null)
                    {
                        link = item.Links[i];
                    }
                    if (str == FeedUtils.GetUriString(item.Links[i].Uri))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(str))
            {
                writer.WriteStartElement("guid");
                if (flag)
                {
                    writer.WriteAttributeString("isPermaLink", "true");
                }
                else
                {
                    writer.WriteAttributeString("isPermaLink", "false");
                }
                writer.WriteString(str);
                writer.WriteEndElement();
            }
            if (link != null)
            {
                this.WriteAlternateLink(writer, link, (item.BaseUri != null) ? item.BaseUri : feedBaseUri);
            }
            if ((item.Authors.Count == 1) && !string.IsNullOrEmpty(item.Authors[0].Email))
            {
                this.WritePerson(writer, "author", item.Authors[0]);
            }
            else if (this.serializeExtensionsAsAtom)
            {
                this.atomSerializer.WriteItemAuthorsTo(writer, item.Authors);
            }
            else
            {
                TraceExtensionsIgnoredOnWrite("ItemAuthorsIgnoredOnWrite");
            }
            for (int j = 0; j < item.Categories.Count; j++)
            {
                this.WriteCategory(writer, item.Categories[j]);
            }
            bool flag2 = false;
            if (item.Title != null)
            {
                writer.WriteElementString("title", item.Title.Text);
                flag2 = true;
            }
            bool flag3 = false;
            TextSyndicationContent summary = item.Summary;
            if (summary == null)
            {
                summary = item.Content as TextSyndicationContent;
                flag3 = summary != null;
            }
            if (!flag2 && (summary == null))
            {
                summary = new TextSyndicationContent(string.Empty);
            }
            if (summary != null)
            {
                writer.WriteElementString("description", "", summary.Text);
            }
            if (item.SourceFeed != null)
            {
                writer.WriteStartElement("source", "");
                SyndicationFeedFormatter.WriteAttributeExtensions(writer, item.SourceFeed, this.Version);
                SyndicationLink link2 = null;
                for (int m = 0; m < item.SourceFeed.Links.Count; m++)
                {
                    if (item.SourceFeed.Links[m].RelationshipType == "self")
                    {
                        link2 = item.SourceFeed.Links[m];
                        break;
                    }
                }
                if ((link2 != null) && !item.SourceFeed.AttributeExtensions.ContainsKey(Rss20Url))
                {
                    writer.WriteAttributeString("url", "", FeedUtils.GetUriString(link2.Uri));
                }
                string text = (item.SourceFeed.Title != null) ? item.SourceFeed.Title.Text : string.Empty;
                writer.WriteString(text);
                writer.WriteEndElement();
            }
            if (item.PublishDate > DateTimeOffset.MinValue)
            {
                writer.WriteElementString("pubDate", "", this.AsString(item.PublishDate));
            }
            SyndicationLink link3 = null;
            bool flag4 = false;
            bool flag5 = false;
            for (int k = 0; k < item.Links.Count; k++)
            {
                if (item.Links[k].RelationshipType == "enclosure")
                {
                    if (link3 != null)
                    {
                        goto Label_03D8;
                    }
                    link3 = item.Links[k];
                    this.WriteMediaEnclosure(writer, item.Links[k], item.BaseUri);
                    continue;
                }
                if ((item.Links[k].RelationshipType == "alternate") && !flag4)
                {
                    flag4 = true;
                    continue;
                }
            Label_03D8:
                if (this.serializeExtensionsAsAtom)
                {
                    this.atomSerializer.WriteLink(writer, item.Links[k], item.BaseUri);
                }
                else
                {
                    flag5 = true;
                }
            }
            if (flag5)
            {
                TraceExtensionsIgnoredOnWrite("ItemLinksIgnoredOnWrite");
            }
            if (item.LastUpdatedTime > DateTimeOffset.MinValue)
            {
                if (this.serializeExtensionsAsAtom)
                {
                    this.atomSerializer.WriteItemLastUpdatedTimeTo(writer, item.LastUpdatedTime);
                }
                else
                {
                    TraceExtensionsIgnoredOnWrite("ItemLastUpdatedTimeIgnoredOnWrite");
                }
            }
            if (this.serializeExtensionsAsAtom)
            {
                this.atomSerializer.WriteContentTo(writer, "rights", item.Copyright);
            }
            else
            {
                TraceExtensionsIgnoredOnWrite("ItemCopyrightIgnoredOnWrite");
            }
            if (!flag3)
            {
                if (this.serializeExtensionsAsAtom)
                {
                    this.atomSerializer.WriteContentTo(writer, "content", item.Content);
                }
                else
                {
                    TraceExtensionsIgnoredOnWrite("ItemContentIgnoredOnWrite");
                }
            }
            if (item.Contributors.Count > 0)
            {
                if (this.serializeExtensionsAsAtom)
                {
                    this.atomSerializer.WriteItemContributorsTo(writer, item.Contributors);
                }
                else
                {
                    TraceExtensionsIgnoredOnWrite("ItemContributorsIgnoredOnWrite");
                }
            }
            SyndicationFeedFormatter.WriteElementExtensions(writer, item, this.Version);
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

        private void WriteMediaEnclosure(XmlWriter writer, SyndicationLink link, Uri baseUri)
        {
            writer.WriteStartElement("enclosure", "");
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(baseUri, link.BaseUri);
            if (baseUriToWrite != null)
            {
                writer.WriteAttributeString("xml", "base", "http://www.w3.org/XML/1998/namespace", FeedUtils.GetUriString(baseUriToWrite));
            }
            link.WriteAttributeExtensions(writer, "Rss20");
            if (!link.AttributeExtensions.ContainsKey(Rss20Url))
            {
                writer.WriteAttributeString("url", "", FeedUtils.GetUriString(link.Uri));
            }
            if ((link.MediaType != null) && !link.AttributeExtensions.ContainsKey(Rss20Type))
            {
                writer.WriteAttributeString("type", "", link.MediaType);
            }
            if ((link.Length != 0L) && !link.AttributeExtensions.ContainsKey(Rss20Length))
            {
                writer.WriteAttributeString("length", "", Convert.ToString(link.Length, CultureInfo.InvariantCulture));
            }
            writer.WriteEndElement();
        }

        private void WritePerson(XmlWriter writer, string elementTag, SyndicationPerson person)
        {
            writer.WriteStartElement(elementTag, "");
            SyndicationFeedFormatter.WriteAttributeExtensions(writer, person, this.Version);
            writer.WriteString(person.Email);
            writer.WriteEndElement();
        }

        public override void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            SyndicationFeedFormatter.TraceFeedWriteBegin();
            writer.WriteStartElement("rss", "");
            this.WriteFeed(writer);
            writer.WriteEndElement();
            SyndicationFeedFormatter.TraceFeedWriteEnd();
        }

        protected Type FeedType
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

        public bool SerializeExtensionsAsAtom
        {
            get
            {
                return this.serializeExtensionsAsAtom;
            }
            set
            {
                this.serializeExtensionsAsAtom = value;
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

