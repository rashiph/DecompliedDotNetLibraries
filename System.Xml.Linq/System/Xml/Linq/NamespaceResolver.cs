namespace System.Xml.Linq
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NamespaceResolver
    {
        private int scope;
        private NamespaceDeclaration declaration;
        private NamespaceDeclaration rover;
        public void PushScope()
        {
            this.scope++;
        }

        public void PopScope()
        {
            NamespaceDeclaration prev = this.declaration;
            if (prev != null)
            {
                do
                {
                    prev = prev.prev;
                    if (prev.scope != this.scope)
                    {
                        break;
                    }
                    if (prev == this.declaration)
                    {
                        this.declaration = null;
                    }
                    else
                    {
                        this.declaration.prev = prev.prev;
                    }
                    this.rover = null;
                }
                while ((prev != this.declaration) && (this.declaration != null));
            }
            this.scope--;
        }

        public void Add(string prefix, XNamespace ns)
        {
            NamespaceDeclaration declaration = new NamespaceDeclaration {
                prefix = prefix,
                ns = ns,
                scope = this.scope
            };
            if (this.declaration == null)
            {
                this.declaration = declaration;
            }
            else
            {
                declaration.prev = this.declaration.prev;
            }
            this.declaration.prev = declaration;
            this.rover = null;
        }

        public void AddFirst(string prefix, XNamespace ns)
        {
            NamespaceDeclaration declaration = new NamespaceDeclaration {
                prefix = prefix,
                ns = ns,
                scope = this.scope
            };
            if (this.declaration == null)
            {
                declaration.prev = declaration;
            }
            else
            {
                declaration.prev = this.declaration.prev;
                this.declaration.prev = declaration;
            }
            this.declaration = declaration;
            this.rover = null;
        }

        public string GetPrefixOfNamespace(XNamespace ns, bool allowDefaultNamespace)
        {
            if (((this.rover != null) && (this.rover.ns == ns)) && (allowDefaultNamespace || (this.rover.prefix.Length > 0)))
            {
                return this.rover.prefix;
            }
            NamespaceDeclaration prev = this.declaration;
            if (prev != null)
            {
                do
                {
                    prev = prev.prev;
                    if (prev.ns == ns)
                    {
                        NamespaceDeclaration declaration2 = this.declaration.prev;
                        while ((declaration2 != prev) && (declaration2.prefix != prev.prefix))
                        {
                            declaration2 = declaration2.prev;
                        }
                        if (declaration2 == prev)
                        {
                            if (allowDefaultNamespace)
                            {
                                this.rover = prev;
                                return prev.prefix;
                            }
                            if (prev.prefix.Length > 0)
                            {
                                return prev.prefix;
                            }
                        }
                    }
                }
                while (prev != this.declaration);
            }
            return null;
        }
        private class NamespaceDeclaration
        {
            public XNamespace ns;
            public string prefix;
            public NamespaceResolver.NamespaceDeclaration prev;
            public int scope;
        }
    }
}

