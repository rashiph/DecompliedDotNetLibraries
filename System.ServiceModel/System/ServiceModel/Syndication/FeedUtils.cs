namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.ServiceModel;
    using System.Xml;

    internal static class FeedUtils
    {
        public static string AddLineInfo(XmlReader reader, string error)
        {
            IXmlLineInfo info = reader as IXmlLineInfo;
            if ((info != null) && info.HasLineInfo())
            {
                error = string.Format(CultureInfo.InvariantCulture, "{0} {1}", new object[] { System.ServiceModel.SR.GetString("ErrorInLine", new object[] { info.LineNumber, info.LinePosition }), System.ServiceModel.SR.GetString(error) });
            }
            return error;
        }

        internal static Collection<SyndicationCategory> CloneCategories(Collection<SyndicationCategory> categories)
        {
            if (categories == null)
            {
                return null;
            }
            Collection<SyndicationCategory> collection = new NullNotAllowedCollection<SyndicationCategory>();
            for (int i = 0; i < categories.Count; i++)
            {
                collection.Add(categories[i].Clone());
            }
            return collection;
        }

        internal static Collection<SyndicationLink> CloneLinks(Collection<SyndicationLink> links)
        {
            if (links == null)
            {
                return null;
            }
            Collection<SyndicationLink> collection = new NullNotAllowedCollection<SyndicationLink>();
            for (int i = 0; i < links.Count; i++)
            {
                collection.Add(links[i].Clone());
            }
            return collection;
        }

        internal static Collection<SyndicationPerson> ClonePersons(Collection<SyndicationPerson> persons)
        {
            if (persons == null)
            {
                return null;
            }
            Collection<SyndicationPerson> collection = new NullNotAllowedCollection<SyndicationPerson>();
            for (int i = 0; i < persons.Count; i++)
            {
                collection.Add(persons[i].Clone());
            }
            return collection;
        }

        internal static TextSyndicationContent CloneTextContent(TextSyndicationContent content)
        {
            if (content == null)
            {
                return null;
            }
            return (TextSyndicationContent) content.Clone();
        }

        internal static Uri CombineXmlBase(Uri rootBase, string newBase)
        {
            if (string.IsNullOrEmpty(newBase))
            {
                return rootBase;
            }
            Uri uri = new Uri(newBase, UriKind.RelativeOrAbsolute);
            if ((rootBase != null) && !uri.IsAbsoluteUri)
            {
                return new Uri(rootBase, newBase);
            }
            return uri;
        }

        internal static Uri GetBaseUriToWrite(Uri rootBase, Uri currentBase)
        {
            if ((rootBase == currentBase) || (currentBase == null))
            {
                return null;
            }
            if ((rootBase != null) && ((rootBase.IsAbsoluteUri && currentBase.IsAbsoluteUri) && rootBase.IsBaseOf(currentBase)))
            {
                return rootBase.MakeRelativeUri(currentBase);
            }
            return currentBase;
        }

        internal static string GetUriString(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }
            if (uri.IsAbsoluteUri)
            {
                return uri.AbsoluteUri;
            }
            return uri.ToString();
        }

        internal static bool IsXmlns(string name, string ns)
        {
            if (!(name == "xmlns"))
            {
                return (ns == "http://www.w3.org/2000/xmlns/");
            }
            return true;
        }

        internal static bool IsXmlSchemaType(string name, string ns)
        {
            return ((name == "type") && (ns == "http://www.w3.org/2001/XMLSchema-instance"));
        }
    }
}

