namespace System.Xml.Linq
{
    using System;
    using System.Runtime;
    using System.Threading;

    public sealed class XNamespace
    {
        private int hashCode;
        private XHashtable<XName> names;
        private const int NamesCapacity = 8;
        private string namespaceName;
        private static XHashtable<WeakReference> namespaces;
        private const int NamespacesCapacity = 0x20;
        private static WeakReference refNone;
        private static WeakReference refXml;
        private static WeakReference refXmlns;
        internal const string xmlnsPrefixNamespace = "http://www.w3.org/2000/xmlns/";
        internal const string xmlPrefixNamespace = "http://www.w3.org/XML/1998/namespace";

        internal XNamespace(string namespaceName)
        {
            this.namespaceName = namespaceName;
            this.hashCode = namespaceName.GetHashCode();
            this.names = new XHashtable<XName>(new XHashtable<XName>.ExtractKeyDelegate(XNamespace.ExtractLocalName), 8);
        }

        private static XNamespace EnsureNamespace(ref WeakReference refNmsp, string namespaceName)
        {
            while (true)
            {
                WeakReference comparand = refNmsp;
                if (comparand != null)
                {
                    XNamespace target = (XNamespace) comparand.Target;
                    if (target != null)
                    {
                        return target;
                    }
                }
                Interlocked.CompareExchange<WeakReference>(ref refNmsp, new WeakReference(new XNamespace(namespaceName)), comparand);
            }
        }

        public override bool Equals(object obj)
        {
            return (this == obj);
        }

        private static string ExtractLocalName(XName n)
        {
            return n.LocalName;
        }

        private static string ExtractNamespace(WeakReference r)
        {
            XNamespace namespace2;
            if ((r != null) && ((namespace2 = (XNamespace) r.Target) != null))
            {
                return namespace2.NamespaceName;
            }
            return null;
        }

        public static XNamespace Get(string namespaceName)
        {
            if (namespaceName == null)
            {
                throw new ArgumentNullException("namespaceName");
            }
            return Get(namespaceName, 0, namespaceName.Length);
        }

        internal static XNamespace Get(string namespaceName, int index, int count)
        {
            XNamespace namespace2;
            if (count == 0)
            {
                return None;
            }
            if (namespaces == null)
            {
                Interlocked.CompareExchange<XHashtable<WeakReference>>(ref namespaces, new XHashtable<WeakReference>(new XHashtable<WeakReference>.ExtractKeyDelegate(XNamespace.ExtractNamespace), 0x20), null);
            }
            do
            {
                WeakReference reference;
                if (!namespaces.TryGetValue(namespaceName, index, count, out reference))
                {
                    if ((count == "http://www.w3.org/XML/1998/namespace".Length) && (string.CompareOrdinal(namespaceName, index, "http://www.w3.org/XML/1998/namespace", 0, count) == 0))
                    {
                        return Xml;
                    }
                    if ((count == "http://www.w3.org/2000/xmlns/".Length) && (string.CompareOrdinal(namespaceName, index, "http://www.w3.org/2000/xmlns/", 0, count) == 0))
                    {
                        return Xmlns;
                    }
                    reference = namespaces.Add(new WeakReference(new XNamespace(namespaceName.Substring(index, count))));
                }
                namespace2 = (reference != null) ? ((XNamespace) reference.Target) : null;
            }
            while (namespace2 == null);
            return namespace2;
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public XName GetName(string localName)
        {
            if (localName == null)
            {
                throw new ArgumentNullException("localName");
            }
            return this.GetName(localName, 0, localName.Length);
        }

        internal XName GetName(string localName, int index, int count)
        {
            XName name;
            if (this.names.TryGetValue(localName, index, count, out name))
            {
                return name;
            }
            return this.names.Add(new XName(this, localName.Substring(index, count)));
        }

        public static XName operator +(XNamespace ns, string localName)
        {
            if (ns == null)
            {
                throw new ArgumentNullException("ns");
            }
            return ns.GetName(localName);
        }

        public static bool operator ==(XNamespace left, XNamespace right)
        {
            return (left == right);
        }

        [CLSCompliant(false)]
        public static implicit operator XNamespace(string namespaceName)
        {
            if (namespaceName == null)
            {
                return null;
            }
            return Get(namespaceName);
        }

        public static bool operator !=(XNamespace left, XNamespace right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return this.namespaceName;
        }

        public string NamespaceName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.namespaceName;
            }
        }

        public static XNamespace None
        {
            get
            {
                return EnsureNamespace(ref refNone, string.Empty);
            }
        }

        public static XNamespace Xml
        {
            get
            {
                return EnsureNamespace(ref refXml, "http://www.w3.org/XML/1998/namespace");
            }
        }

        public static XNamespace Xmlns
        {
            get
            {
                return EnsureNamespace(ref refXmlns, "http://www.w3.org/2000/xmlns/");
            }
        }
    }
}

