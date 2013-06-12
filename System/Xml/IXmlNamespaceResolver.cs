namespace System.Xml
{
    using System;
    using System.Collections.Generic;

    public interface IXmlNamespaceResolver
    {
        IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope);
        string LookupNamespace(string prefix);
        string LookupPrefix(string namespaceName);
    }
}

