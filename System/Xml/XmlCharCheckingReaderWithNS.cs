namespace System.Xml
{
    using System;
    using System.Collections.Generic;

    internal class XmlCharCheckingReaderWithNS : XmlCharCheckingReader, IXmlNamespaceResolver
    {
        internal IXmlNamespaceResolver readerAsNSResolver;

        internal XmlCharCheckingReaderWithNS(XmlReader reader, IXmlNamespaceResolver readerAsNSResolver, bool checkCharacters, bool ignoreWhitespace, bool ignoreComments, bool ignorePis, DtdProcessing dtdProcessing) : base(reader, checkCharacters, ignoreWhitespace, ignoreComments, ignorePis, dtdProcessing)
        {
            this.readerAsNSResolver = readerAsNSResolver;
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return this.readerAsNSResolver.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix)
        {
            return this.readerAsNSResolver.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            return this.readerAsNSResolver.LookupPrefix(namespaceName);
        }
    }
}

