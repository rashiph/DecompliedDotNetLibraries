namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NodeQName
    {
        internal static NodeQName Empty;
        internal string name;
        internal string ns;
        internal NodeQName(string name) : this(name, string.Empty)
        {
        }

        internal NodeQName(string name, string ns)
        {
            this.name = (name == null) ? string.Empty : name;
            this.ns = (ns == null) ? string.Empty : ns;
        }

        internal bool IsEmpty
        {
            get
            {
                return ((this.name.Length == 0) && (this.ns.Length == 0));
            }
        }
        internal bool IsNameDefined
        {
            get
            {
                return (this.name.Length > 0);
            }
        }
        internal bool IsNameWildcard
        {
            get
            {
                return object.ReferenceEquals(this.name, QueryDataModel.Wildcard);
            }
        }
        internal bool IsNamespaceDefined
        {
            get
            {
                return (this.ns.Length > 0);
            }
        }
        internal bool IsNamespaceWildcard
        {
            get
            {
                return object.ReferenceEquals(this.ns, QueryDataModel.Wildcard);
            }
        }
        internal string Name
        {
            get
            {
                return this.name;
            }
        }
        internal string Namespace
        {
            get
            {
                return this.ns;
            }
        }
        internal bool EqualsName(string name)
        {
            return (name == this.name);
        }

        internal bool Equals(NodeQName qname)
        {
            if ((qname.name.Length != this.name.Length) || !(qname.name == this.name))
            {
                return false;
            }
            return ((qname.ns.Length == this.ns.Length) && (qname.ns == this.ns));
        }

        internal bool EqualsNamespace(string ns)
        {
            return (ns == this.ns);
        }

        internal NodeQNameType GetQNameType()
        {
            NodeQNameType empty = NodeQNameType.Empty;
            if (this.IsNameDefined)
            {
                if (this.IsNameWildcard)
                {
                    empty = (NodeQNameType) ((byte) (empty | NodeQNameType.NameWildcard));
                }
                else
                {
                    empty = (NodeQNameType) ((byte) (empty | NodeQNameType.Name));
                }
            }
            if (!this.IsNamespaceDefined)
            {
                return empty;
            }
            if (this.IsNamespaceWildcard)
            {
                return (NodeQNameType) ((byte) (empty | NodeQNameType.NamespaceWildcard));
            }
            return (NodeQNameType) ((byte) (empty | NodeQNameType.Namespace));
        }

        static NodeQName()
        {
            Empty = new NodeQName(string.Empty, string.Empty);
        }
    }
}

