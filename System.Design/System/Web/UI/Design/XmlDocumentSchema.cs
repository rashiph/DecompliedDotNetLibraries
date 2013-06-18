namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.XPath;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public sealed class XmlDocumentSchema : IDataSourceSchema
    {
        private bool _includeSpecialSchema;
        private OrderedDictionary _rootSchema;
        private IDataSourceViewSchema[] _viewSchemas;

        public XmlDocumentSchema(XmlDocument xmlDocument, string xPath) : this(xmlDocument, xPath, false)
        {
        }

        internal XmlDocumentSchema(XmlDocument xmlDocument, string xPath, bool includeSpecialSchema)
        {
            if (xmlDocument == null)
            {
                throw new ArgumentNullException("xmlDocument");
            }
            this._includeSpecialSchema = includeSpecialSchema;
            this._rootSchema = new OrderedDictionary();
            XPathNavigator rootNav = xmlDocument.CreateNavigator();
            if (!string.IsNullOrEmpty(xPath))
            {
                XPathNodeIterator iterator = rootNav.Select(xPath);
                while (iterator.MoveNext())
                {
                    XPathNodeIterator iterator2 = iterator.Current.SelectDescendants(XPathNodeType.Element, true);
                    while (iterator2.MoveNext())
                    {
                        this.AddSchemaElement(iterator2.Current, iterator.Current);
                    }
                }
            }
            else
            {
                XPathNodeIterator iterator3 = rootNav.SelectDescendants(XPathNodeType.Element, true);
                while (iterator3.MoveNext())
                {
                    this.AddSchemaElement(iterator3.Current, rootNav);
                }
            }
        }

        private void AddAttributeList(XPathNavigator nav, ArrayList attrs)
        {
            if (nav.HasAttributes)
            {
                nav.MoveToFirstAttribute();
                do
                {
                    if (!attrs.Contains(nav.Name))
                    {
                        attrs.Add(nav.Name);
                    }
                }
                while (nav.MoveToNextAttribute());
                nav.MoveToParent();
            }
        }

        private void AddSchemaElement(XPathNavigator nav, XPathNavigator rootNav)
        {
            List<string> list = new List<string>();
            XPathNodeIterator iterator = nav.SelectAncestors(XPathNodeType.Element, true);
            while (iterator.MoveNext())
            {
                list.Add(iterator.Current.Name);
                if (iterator.Current.IsSamePosition(rootNav))
                {
                    break;
                }
            }
            list.Reverse();
            OrderedDictionary first = this._rootSchema;
            Pair pair = null;
            foreach (string str in list)
            {
                pair = first[str] as Pair;
                if (pair == null)
                {
                    pair = new Pair(new OrderedDictionary(), new ArrayList());
                    first.Add(str, pair);
                }
                first = (OrderedDictionary) pair.First;
            }
            this.AddAttributeList(nav, (ArrayList) pair.Second);
        }

        public IDataSourceViewSchema[] GetViews()
        {
            if (this._viewSchemas == null)
            {
                this._viewSchemas = new IDataSourceViewSchema[this._rootSchema.Count];
                int index = 0;
                foreach (DictionaryEntry entry in this._rootSchema)
                {
                    this._viewSchemas[index] = new XmlDocumentViewSchema((string) entry.Key, (Pair) entry.Value, this._includeSpecialSchema);
                    index++;
                }
            }
            return this._viewSchemas;
        }
    }
}

