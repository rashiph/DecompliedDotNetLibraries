namespace MS.Internal.Xaml.Context
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xaml;

    internal abstract class XamlCommonFrame : XamlFrame
    {
        internal Dictionary<string, string> _namespaces;

        public XamlCommonFrame()
        {
        }

        public XamlCommonFrame(XamlCommonFrame source) : base(source)
        {
            this.XamlType = source.XamlType;
            this.Member = source.Member;
            if (source._namespaces != null)
            {
                this.SetNamespaces(source._namespaces);
            }
        }

        public void AddNamespace(string prefix, string xamlNs)
        {
            this.Namespaces.Add(prefix, xamlNs);
        }

        public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
        {
            List<NamespaceDeclaration> list = new List<NamespaceDeclaration>();
            foreach (KeyValuePair<string, string> pair in this._namespaces)
            {
                list.Add(new NamespaceDeclaration(pair.Value, pair.Key));
            }
            return list;
        }

        public override void Reset()
        {
            this.XamlType = null;
            this.Member = null;
            if (this._namespaces != null)
            {
                this._namespaces.Clear();
            }
        }

        public void SetNamespaces(Dictionary<string, string> namespaces)
        {
            if (this._namespaces != null)
            {
                this._namespaces.Clear();
            }
            if (namespaces != null)
            {
                foreach (KeyValuePair<string, string> pair in namespaces)
                {
                    this.Namespaces.Add(pair.Key, pair.Value);
                }
            }
        }

        public bool TryGetNamespaceByPrefix(string prefix, out string xamlNs)
        {
            if ((this._namespaces != null) && this._namespaces.TryGetValue(prefix, out xamlNs))
            {
                return true;
            }
            xamlNs = null;
            return false;
        }

        public XamlMember Member { get; set; }

        public Dictionary<string, string> Namespaces
        {
            get
            {
                if (this._namespaces == null)
                {
                    this._namespaces = new Dictionary<string, string>();
                }
                return this._namespaces;
            }
        }

        public System.Xaml.XamlType XamlType { get; set; }
    }
}

