namespace System.Xml.Linq
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NamespaceCache
    {
        private XNamespace ns;
        private string namespaceName;
        public XNamespace Get(string namespaceName)
        {
            if (namespaceName != this.namespaceName)
            {
                this.namespaceName = namespaceName;
                this.ns = XNamespace.Get(namespaceName);
            }
            return this.ns;
        }
    }
}

