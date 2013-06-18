namespace System.Xml.Linq
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml;

    [Serializable, KnownType(typeof(NameSerializer))]
    public sealed class XName : IEquatable<XName>, ISerializable
    {
        private int hashCode;
        private string localName;
        private XNamespace ns;

        internal XName(XNamespace ns, string localName)
        {
            this.ns = ns;
            this.localName = XmlConvert.VerifyNCName(localName);
            this.hashCode = ns.GetHashCode() ^ localName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (this == obj);
        }

        public static XName Get(string expandedName)
        {
            if (expandedName == null)
            {
                throw new ArgumentNullException("expandedName");
            }
            if (expandedName.Length == 0)
            {
                throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_InvalidExpandedName", new object[] { expandedName }));
            }
            if (expandedName[0] != '{')
            {
                return XNamespace.None.GetName(expandedName);
            }
            int num = expandedName.LastIndexOf('}');
            if ((num <= 1) || (num == (expandedName.Length - 1)))
            {
                throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_InvalidExpandedName", new object[] { expandedName }));
            }
            return XNamespace.Get(expandedName, 1, num - 1).GetName(expandedName, num + 1, (expandedName.Length - num) - 1);
        }

        public static XName Get(string localName, string namespaceName)
        {
            return XNamespace.Get(namespaceName).GetName(localName);
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public static bool operator ==(XName left, XName right)
        {
            return (left == right);
        }

        [CLSCompliant(false)]
        public static implicit operator XName(string expandedName)
        {
            if (expandedName == null)
            {
                return null;
            }
            return Get(expandedName);
        }

        public static bool operator !=(XName left, XName right)
        {
            return !(left == right);
        }

        bool IEquatable<XName>.Equals(XName other)
        {
            return (this == other);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("name", this.ToString());
            info.SetType(typeof(NameSerializer));
        }

        public override string ToString()
        {
            if (this.ns.NamespaceName.Length == 0)
            {
                return this.localName;
            }
            return ("{" + this.ns.NamespaceName + "}" + this.localName);
        }

        public string LocalName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.localName;
            }
        }

        public XNamespace Namespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ns;
            }
        }

        public string NamespaceName
        {
            get
            {
                return this.ns.NamespaceName;
            }
        }
    }
}

