namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Xml;

    public class XmlSerializerNamespaces
    {
        private Hashtable namespaces;

        public XmlSerializerNamespaces()
        {
        }

        public XmlSerializerNamespaces(XmlSerializerNamespaces namespaces)
        {
            this.namespaces = (Hashtable) namespaces.Namespaces.Clone();
        }

        public XmlSerializerNamespaces(XmlQualifiedName[] namespaces)
        {
            for (int i = 0; i < namespaces.Length; i++)
            {
                XmlQualifiedName name = namespaces[i];
                this.Add(name.Name, name.Namespace);
            }
        }

        public void Add(string prefix, string ns)
        {
            if ((prefix != null) && (prefix.Length > 0))
            {
                XmlConvert.VerifyNCName(prefix);
            }
            if ((ns != null) && (ns.Length > 0))
            {
                XmlConvert.ToUri(ns);
            }
            this.AddInternal(prefix, ns);
        }

        internal void AddInternal(string prefix, string ns)
        {
            this.Namespaces[prefix] = ns;
        }

        internal string LookupPrefix(string ns)
        {
            if (!string.IsNullOrEmpty(ns))
            {
                if ((this.namespaces == null) || (this.namespaces.Count == 0))
                {
                    return null;
                }
                foreach (string str in this.namespaces.Keys)
                {
                    if (!string.IsNullOrEmpty(str) && (((string) this.namespaces[str]) == ns))
                    {
                        return str;
                    }
                }
            }
            return null;
        }

        public XmlQualifiedName[] ToArray()
        {
            if (this.NamespaceList == null)
            {
                return new XmlQualifiedName[0];
            }
            return (XmlQualifiedName[]) this.NamespaceList.ToArray(typeof(XmlQualifiedName));
        }

        public int Count
        {
            get
            {
                return this.Namespaces.Count;
            }
        }

        internal ArrayList NamespaceList
        {
            get
            {
                if ((this.namespaces == null) || (this.namespaces.Count == 0))
                {
                    return null;
                }
                ArrayList list = new ArrayList();
                foreach (string str in this.Namespaces.Keys)
                {
                    list.Add(new XmlQualifiedName(str, (string) this.Namespaces[str]));
                }
                return list;
            }
        }

        internal Hashtable Namespaces
        {
            get
            {
                if (this.namespaces == null)
                {
                    this.namespaces = new Hashtable();
                }
                return this.namespaces;
            }
            set
            {
                this.namespaces = value;
            }
        }
    }
}

