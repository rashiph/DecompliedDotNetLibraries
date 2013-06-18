namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xaml;

    internal class NamespacePrefixLookup : INamespacePrefixLookup
    {
        private readonly List<NamespaceDeclaration> _newNamespaces;
        private readonly Func<string, string> _nsResolver;
        private int n;

        public NamespacePrefixLookup(out IEnumerable<NamespaceDeclaration> newNamespaces, Func<string, string> nsResolver)
        {
            newNamespaces = this._newNamespaces = new List<NamespaceDeclaration>();
            this._nsResolver = nsResolver;
        }

        public string LookupPrefix(string ns)
        {
            string str;
            do
            {
                str = "prefix" + this.n++;
            }
            while (this._nsResolver(str) != null);
            this._newNamespaces.Add(new NamespaceDeclaration(ns, str));
            return str;
        }
    }
}

