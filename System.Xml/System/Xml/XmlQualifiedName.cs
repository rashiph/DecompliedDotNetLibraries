namespace System.Xml
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable]
    public class XmlQualifiedName
    {
        public static readonly XmlQualifiedName Empty = new XmlQualifiedName(string.Empty);
        [NonSerialized]
        private int hash;
        private string name;
        private string ns;

        public XmlQualifiedName() : this(string.Empty, string.Empty)
        {
        }

        public XmlQualifiedName(string name) : this(name, string.Empty)
        {
        }

        public XmlQualifiedName(string name, string ns)
        {
            this.ns = (ns == null) ? string.Empty : ns;
            this.name = (name == null) ? string.Empty : name;
        }

        internal void Atomize(XmlNameTable nameTable)
        {
            this.name = nameTable.Add(this.name);
            this.ns = nameTable.Add(this.ns);
        }

        internal XmlQualifiedName Clone()
        {
            return (XmlQualifiedName) base.MemberwiseClone();
        }

        internal static int Compare(XmlQualifiedName a, XmlQualifiedName b)
        {
            if (null == a)
            {
                if (null != b)
                {
                    return -1;
                }
                return 0;
            }
            if (null == b)
            {
                return 1;
            }
            int num = string.CompareOrdinal(a.Namespace, b.Namespace);
            if (num == 0)
            {
                num = string.CompareOrdinal(a.Name, b.Name);
            }
            return num;
        }

        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            XmlQualifiedName name = other as XmlQualifiedName;
            if (name == null)
            {
                return false;
            }
            return ((this.Name == name.Name) && (this.Namespace == name.Namespace));
        }

        public override int GetHashCode()
        {
            if (this.hash == 0)
            {
                this.hash = this.Name.GetHashCode();
            }
            return this.hash;
        }

        internal void Init(string name, string ns)
        {
            this.name = name;
            this.ns = ns;
            this.hash = 0;
        }

        public static bool operator ==(XmlQualifiedName a, XmlQualifiedName b)
        {
            if (a == b)
            {
                return true;
            }
            if ((a == null) || (b == null))
            {
                return false;
            }
            return ((a.Name == b.Name) && (a.Namespace == b.Namespace));
        }

        public static bool operator !=(XmlQualifiedName a, XmlQualifiedName b)
        {
            return !(a == b);
        }

        internal static XmlQualifiedName Parse(string s, IXmlNamespaceResolver nsmgr, out string prefix)
        {
            string str;
            ValidateNames.ParseQNameThrow(s, out prefix, out str);
            string ns = nsmgr.LookupNamespace(prefix);
            if (ns == null)
            {
                if (prefix.Length != 0)
                {
                    throw new XmlException("Xml_UnknownNs", prefix);
                }
                ns = string.Empty;
            }
            return new XmlQualifiedName(str, ns);
        }

        internal void SetNamespace(string ns)
        {
            this.ns = ns;
        }

        public override string ToString()
        {
            if (this.Namespace.Length != 0)
            {
                return (this.Namespace + ":" + this.Name);
            }
            return this.Name;
        }

        public static string ToString(string name, string ns)
        {
            if ((ns != null) && (ns.Length != 0))
            {
                return (ns + ":" + name);
            }
            return name;
        }

        internal void Verify()
        {
            XmlConvert.VerifyNCName(this.name);
            if (this.ns.Length != 0)
            {
                XmlConvert.ToUri(this.ns);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return ((this.Name.Length == 0) && (this.Namespace.Length == 0));
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string Namespace
        {
            get
            {
                return this.ns;
            }
        }
    }
}

