namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.Xml;

    public class PolicyAssertionCollection : Collection<XmlElement>
    {
        public PolicyAssertionCollection()
        {
        }

        public PolicyAssertionCollection(IEnumerable<XmlElement> elements)
        {
            if (elements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");
            }
            this.AddRange(elements);
        }

        internal void AddRange(IEnumerable<XmlElement> elements)
        {
            foreach (XmlElement element in elements)
            {
                base.Add(element);
            }
        }

        public bool Contains(string localName, string namespaceUri)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            for (int i = 0; i < base.Count; i++)
            {
                XmlElement element = base[i];
                if ((element.LocalName == localName) && (element.NamespaceURI == namespaceUri))
                {
                    return true;
                }
            }
            return false;
        }

        public XmlElement Find(string localName, string namespaceUri)
        {
            return this.Find(localName, namespaceUri, false);
        }

        private XmlElement Find(string localName, string namespaceUri, bool remove)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            for (int i = 0; i < base.Count; i++)
            {
                XmlElement element = base[i];
                if ((element.LocalName == localName) && (element.NamespaceURI == namespaceUri))
                {
                    if (remove)
                    {
                        base.RemoveAt(i);
                    }
                    return element;
                }
            }
            return null;
        }

        public Collection<XmlElement> FindAll(string localName, string namespaceUri)
        {
            return this.FindAll(localName, namespaceUri, false);
        }

        private Collection<XmlElement> FindAll(string localName, string namespaceUri, bool remove)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            Collection<XmlElement> collection = new Collection<XmlElement>();
            for (int i = 0; i < base.Count; i++)
            {
                XmlElement item = base[i];
                if ((item.LocalName == localName) && (item.NamespaceURI == namespaceUri))
                {
                    if (remove)
                    {
                        base.RemoveAt(i);
                        i--;
                    }
                    collection.Add(item);
                }
            }
            return collection;
        }

        protected override void InsertItem(int index, XmlElement item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.InsertItem(index, item);
        }

        public XmlElement Remove(string localName, string namespaceUri)
        {
            return this.Find(localName, namespaceUri, true);
        }

        public Collection<XmlElement> RemoveAll(string localName, string namespaceUri)
        {
            return this.FindAll(localName, namespaceUri, true);
        }

        protected override void SetItem(int index, XmlElement item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.SetItem(index, item);
        }
    }
}

