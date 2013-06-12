namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class XmlNamespaceManager : IXmlNamespaceResolver, IEnumerable
    {
        private Dictionary<string, int> hashTable;
        private int lastDecl;
        private const int MinDeclsCountForHashtable = 0x10;
        private XmlNameTable nameTable;
        private NamespaceDeclaration[] nsdecls;
        private static IXmlNamespaceResolver s_EmptyResolver;
        private int scopeId;
        private bool useHashtable;
        private string xml;
        private string xmlNs;

        internal XmlNamespaceManager()
        {
        }

        public XmlNamespaceManager(XmlNameTable nameTable)
        {
            this.nameTable = nameTable;
            this.xml = nameTable.Add("xml");
            this.xmlNs = nameTable.Add("xmlns");
            this.nsdecls = new NamespaceDeclaration[8];
            string prefix = nameTable.Add(string.Empty);
            this.nsdecls[0].Set(prefix, prefix, -1, -1);
            this.nsdecls[1].Set(this.xmlNs, nameTable.Add("http://www.w3.org/2000/xmlns/"), -1, -1);
            this.nsdecls[2].Set(this.xml, nameTable.Add("http://www.w3.org/XML/1998/namespace"), 0, -1);
            this.lastDecl = 2;
            this.scopeId = 1;
        }

        public virtual void AddNamespace(string prefix, string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            prefix = this.nameTable.Add(prefix);
            uri = this.nameTable.Add(uri);
            if (Ref.Equal(this.xml, prefix) && !uri.Equals("http://www.w3.org/XML/1998/namespace"))
            {
                throw new ArgumentException(Res.GetString("Xml_XmlPrefix"));
            }
            if (Ref.Equal(this.xmlNs, prefix))
            {
                throw new ArgumentException(Res.GetString("Xml_XmlnsPrefix"));
            }
            int namespaceDecl = this.LookupNamespaceDecl(prefix);
            int previousNsIndex = -1;
            if (namespaceDecl != -1)
            {
                if (this.nsdecls[namespaceDecl].scopeId == this.scopeId)
                {
                    this.nsdecls[namespaceDecl].uri = uri;
                    return;
                }
                previousNsIndex = namespaceDecl;
            }
            if (this.lastDecl == (this.nsdecls.Length - 1))
            {
                NamespaceDeclaration[] destinationArray = new NamespaceDeclaration[this.nsdecls.Length * 2];
                Array.Copy(this.nsdecls, 0, destinationArray, 0, this.nsdecls.Length);
                this.nsdecls = destinationArray;
            }
            this.nsdecls[++this.lastDecl].Set(prefix, uri, this.scopeId, previousNsIndex);
            if (this.useHashtable)
            {
                this.hashTable[prefix] = this.lastDecl;
            }
            else if (this.lastDecl >= 0x10)
            {
                this.hashTable = new Dictionary<string, int>(this.lastDecl);
                for (int i = 0; i <= this.lastDecl; i++)
                {
                    this.hashTable[this.nsdecls[i].prefix] = i;
                }
                this.useHashtable = true;
            }
        }

        public virtual IEnumerator GetEnumerator()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(this.lastDecl + 1);
            for (int i = 0; i <= this.lastDecl; i++)
            {
                if (this.nsdecls[i].uri != null)
                {
                    dictionary[this.nsdecls[i].prefix] = this.nsdecls[i].prefix;
                }
            }
            return dictionary.Keys.GetEnumerator();
        }

        internal bool GetNamespaceDeclaration(int idx, out string prefix, out string uri)
        {
            idx = this.lastDecl - idx;
            if (idx < 0)
            {
                prefix = (string) (uri = null);
                return false;
            }
            prefix = this.nsdecls[idx].prefix;
            uri = this.nsdecls[idx].uri;
            return true;
        }

        public virtual IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            int index = 0;
            switch (scope)
            {
                case XmlNamespaceScope.All:
                    index = 2;
                    break;

                case XmlNamespaceScope.ExcludeXml:
                    index = 3;
                    break;

                case XmlNamespaceScope.Local:
                    index = this.lastDecl;
                    while (this.nsdecls[index].scopeId == this.scopeId)
                    {
                        index--;
                    }
                    index++;
                    break;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>((this.lastDecl - index) + 1);
            while (index <= this.lastDecl)
            {
                string prefix = this.nsdecls[index].prefix;
                string uri = this.nsdecls[index].uri;
                if (uri != null)
                {
                    if (((uri.Length > 0) || (prefix.Length > 0)) || (scope == XmlNamespaceScope.Local))
                    {
                        dictionary[prefix] = uri;
                    }
                    else
                    {
                        dictionary.Remove(prefix);
                    }
                }
                index++;
            }
            return dictionary;
        }

        public virtual bool HasNamespace(string prefix)
        {
            for (int i = this.lastDecl; this.nsdecls[i].scopeId == this.scopeId; i--)
            {
                if (string.Equals(this.nsdecls[i].prefix, prefix) && (this.nsdecls[i].uri != null))
                {
                    if ((prefix.Length <= 0) && (this.nsdecls[i].uri.Length <= 0))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public virtual string LookupNamespace(string prefix)
        {
            int namespaceDecl = this.LookupNamespaceDecl(prefix);
            if (namespaceDecl != -1)
            {
                return this.nsdecls[namespaceDecl].uri;
            }
            return null;
        }

        private int LookupNamespaceDecl(string prefix)
        {
            if (this.useHashtable)
            {
                int previousNsIndex;
                if (!this.hashTable.TryGetValue(prefix, out previousNsIndex))
                {
                    return -1;
                }
                while ((previousNsIndex != -1) && (this.nsdecls[previousNsIndex].uri == null))
                {
                    previousNsIndex = this.nsdecls[previousNsIndex].previousNsIndex;
                }
                return previousNsIndex;
            }
            for (int i = this.lastDecl; i >= 0; i--)
            {
                if ((this.nsdecls[i].prefix == prefix) && (this.nsdecls[i].uri != null))
                {
                    return i;
                }
            }
            for (int j = this.lastDecl; j >= 0; j--)
            {
                if (string.Equals(this.nsdecls[j].prefix, prefix) && (this.nsdecls[j].uri != null))
                {
                    return j;
                }
            }
            return -1;
        }

        public virtual string LookupPrefix(string uri)
        {
            for (int i = this.lastDecl; i >= 0; i--)
            {
                if (string.Equals(this.nsdecls[i].uri, uri))
                {
                    string prefix = this.nsdecls[i].prefix;
                    if (string.Equals(this.LookupNamespace(prefix), uri))
                    {
                        return prefix;
                    }
                }
            }
            return null;
        }

        public virtual bool PopScope()
        {
            int lastDecl = this.lastDecl;
            if (this.scopeId != 1)
            {
                while (this.nsdecls[lastDecl].scopeId == this.scopeId)
                {
                    if (this.useHashtable)
                    {
                        this.hashTable[this.nsdecls[lastDecl].prefix] = this.nsdecls[lastDecl].previousNsIndex;
                    }
                    lastDecl--;
                }
                this.lastDecl = lastDecl;
                this.scopeId--;
                return true;
            }
            return false;
        }

        public virtual void PushScope()
        {
            this.scopeId++;
        }

        public virtual void RemoveNamespace(string prefix, string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            for (int i = this.LookupNamespaceDecl(prefix); i != -1; i = this.nsdecls[i].previousNsIndex)
            {
                if (string.Equals(this.nsdecls[i].uri, uri) && (this.nsdecls[i].scopeId == this.scopeId))
                {
                    this.nsdecls[i].uri = null;
                }
            }
        }

        public virtual string DefaultNamespace
        {
            get
            {
                string str = this.LookupNamespace(string.Empty);
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
        }

        internal static IXmlNamespaceResolver EmptyResolver
        {
            get
            {
                if (s_EmptyResolver == null)
                {
                    s_EmptyResolver = new XmlNamespaceManager(new System.Xml.NameTable());
                }
                return s_EmptyResolver;
            }
        }

        public virtual XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NamespaceDeclaration
        {
            public string prefix;
            public string uri;
            public int scopeId;
            public int previousNsIndex;
            public void Set(string prefix, string uri, int scopeId, int previousNsIndex)
            {
                this.prefix = prefix;
                this.uri = uri;
                this.scopeId = scopeId;
                this.previousNsIndex = previousNsIndex;
            }
        }
    }
}

