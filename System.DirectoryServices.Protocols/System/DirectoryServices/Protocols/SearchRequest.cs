namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    public class SearchRequest : DirectoryRequest
    {
        private StringCollection directoryAttributes;
        private object directoryFilter;
        private DereferenceAlias directoryRefAlias;
        private SearchScope directoryScope;
        private int directorySizeLimit;
        private TimeSpan directoryTimeLimit;
        private bool directoryTypesOnly;
        private string dn;

        public SearchRequest()
        {
            this.directoryAttributes = new StringCollection();
            this.directoryScope = SearchScope.Subtree;
            this.directoryTimeLimit = new TimeSpan(0L);
            this.directoryAttributes = new StringCollection();
        }

        public SearchRequest(string distinguishedName, string ldapFilter, SearchScope searchScope, params string[] attributeList) : this()
        {
            this.dn = distinguishedName;
            if (attributeList != null)
            {
                for (int i = 0; i < attributeList.Length; i++)
                {
                    this.directoryAttributes.Add(attributeList[i]);
                }
            }
            this.Scope = searchScope;
            this.Filter = ldapFilter;
        }

        public SearchRequest(string distinguishedName, XmlDocument filter, SearchScope searchScope, params string[] attributeList) : this()
        {
            this.dn = distinguishedName;
            if (attributeList != null)
            {
                for (int i = 0; i < attributeList.Length; i++)
                {
                    this.directoryAttributes.Add(attributeList[i]);
                }
            }
            this.Scope = searchScope;
            this.Filter = filter;
        }

        private void CopyFilter(XmlNode node, XmlTextWriter writer)
        {
            for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
            {
                if (node2 != null)
                {
                    this.CopyXmlTree(node2, writer);
                }
            }
        }

        private void CopyXmlTree(XmlNode node, XmlTextWriter writer)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                writer.WriteStartElement(node.LocalName, "urn:oasis:names:tc:DSML:2:0:core");
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    writer.WriteAttributeString(attribute.LocalName, attribute.Value);
                }
                for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
                {
                    this.CopyXmlTree(node2, writer);
                }
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteRaw(node.OuterXml);
            }
        }

        protected override XmlElement ToXmlNode(XmlDocument doc)
        {
            XmlElement element = base.CreateRequestElement(doc, "searchRequest", true, this.dn);
            XmlAttribute node = doc.CreateAttribute("scope", null);
            switch (this.directoryScope)
            {
                case SearchScope.Base:
                    node.InnerText = "baseObject";
                    break;

                case SearchScope.OneLevel:
                    node.InnerText = "singleLevel";
                    break;

                case SearchScope.Subtree:
                    node.InnerText = "wholeSubtree";
                    break;
            }
            element.Attributes.Append(node);
            XmlAttribute attribute2 = doc.CreateAttribute("derefAliases", null);
            switch (this.directoryRefAlias)
            {
                case DereferenceAlias.Never:
                    attribute2.InnerText = "neverDerefAliases";
                    break;

                case DereferenceAlias.InSearching:
                    attribute2.InnerText = "derefInSearching";
                    break;

                case DereferenceAlias.FindingBaseObject:
                    attribute2.InnerText = "derefFindingBaseObj";
                    break;

                case DereferenceAlias.Always:
                    attribute2.InnerText = "derefAlways";
                    break;
            }
            element.Attributes.Append(attribute2);
            XmlAttribute attribute3 = doc.CreateAttribute("sizeLimit", null);
            attribute3.InnerText = this.directorySizeLimit.ToString(CultureInfo.InvariantCulture);
            element.Attributes.Append(attribute3);
            XmlAttribute attribute4 = doc.CreateAttribute("timeLimit", null);
            attribute4.InnerText = (this.directoryTimeLimit.Ticks / 0x989680L).ToString(CultureInfo.InvariantCulture);
            element.Attributes.Append(attribute4);
            XmlAttribute attribute5 = doc.CreateAttribute("typesOnly", null);
            attribute5.InnerText = this.directoryTypesOnly ? "true" : "false";
            element.Attributes.Append(attribute5);
            XmlElement newChild = doc.CreateElement("filter", "urn:oasis:names:tc:DSML:2:0:core");
            if (this.Filter != null)
            {
                StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
                XmlTextWriter writer = new XmlTextWriter(w);
                try
                {
                    if (this.Filter is XmlDocument)
                    {
                        if (((XmlDocument) this.Filter).NamespaceURI.Length == 0)
                        {
                            this.CopyFilter((XmlDocument) this.Filter, writer);
                            newChild.InnerXml = w.ToString();
                        }
                        else
                        {
                            newChild.InnerXml = ((XmlDocument) this.Filter).OuterXml;
                        }
                    }
                    else if (this.Filter is string)
                    {
                        string str = (string) this.Filter;
                        if (!str.StartsWith("(", StringComparison.Ordinal) && !str.EndsWith(")", StringComparison.Ordinal))
                        {
                            str = str.Insert(0, "(") + ")";
                        }
                        ADFilter filter = FilterParser.ParseFilterString(str);
                        if (filter == null)
                        {
                            throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("BadSearchLDAPFilter"));
                        }
                        new DSMLFilterWriter().WriteFilter(filter, false, writer, "urn:oasis:names:tc:DSML:2:0:core");
                        newChild.InnerXml = w.ToString();
                    }
                }
                finally
                {
                    writer.Close();
                }
            }
            else
            {
                newChild.InnerXml = "<present name='objectClass' xmlns=\"urn:oasis:names:tc:DSML:2:0:core\"/>";
            }
            element.AppendChild(newChild);
            if ((this.directoryAttributes != null) && (this.directoryAttributes.Count != 0))
            {
                XmlElement element3 = doc.CreateElement("attributes", "urn:oasis:names:tc:DSML:2:0:core");
                element.AppendChild(element3);
                foreach (string str2 in this.directoryAttributes)
                {
                    XmlElement element4 = new DirectoryAttribute { Name = str2 }.ToXmlNode(doc, "attribute");
                    element3.AppendChild(element4);
                }
            }
            return element;
        }

        public DereferenceAlias Aliases
        {
            get
            {
                return this.directoryRefAlias;
            }
            set
            {
                if ((value < DereferenceAlias.Never) || (value > DereferenceAlias.Always))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DereferenceAlias));
                }
                this.directoryRefAlias = value;
            }
        }

        public StringCollection Attributes
        {
            get
            {
                return this.directoryAttributes;
            }
        }

        public string DistinguishedName
        {
            get
            {
                return this.dn;
            }
            set
            {
                this.dn = value;
            }
        }

        public object Filter
        {
            get
            {
                return this.directoryFilter;
            }
            set
            {
                if ((!(value is string) && !(value is XmlDocument)) && (value != null))
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("ValidFilterType"), "value");
                }
                this.directoryFilter = value;
            }
        }

        public SearchScope Scope
        {
            get
            {
                return this.directoryScope;
            }
            set
            {
                if ((value < SearchScope.Base) || (value > SearchScope.Subtree))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(SearchScope));
                }
                this.directoryScope = value;
            }
        }

        public int SizeLimit
        {
            get
            {
                return this.directorySizeLimit;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NoNegativeSizeLimit"), "value");
                }
                this.directorySizeLimit = value;
            }
        }

        public TimeSpan TimeLimit
        {
            get
            {
                return this.directoryTimeLimit;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NoNegativeTime"), "value");
                }
                if (value.TotalSeconds > 2147483647.0)
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("TimespanExceedMax"), "value");
                }
                this.directoryTimeLimit = value;
            }
        }

        public bool TypesOnly
        {
            get
            {
                return this.directoryTypesOnly;
            }
            set
            {
                this.directoryTypesOnly = value;
            }
        }
    }
}

