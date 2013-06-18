namespace System.Xaml
{
    using System;
    using System.Collections.Generic;

    public interface IXamlNamespaceResolver
    {
        string GetNamespace(string prefix);
        IEnumerable<NamespaceDeclaration> GetNamespacePrefixes();
    }
}

