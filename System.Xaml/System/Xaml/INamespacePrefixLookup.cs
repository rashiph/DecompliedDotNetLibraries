namespace System.Xaml
{
    using System;

    public interface INamespacePrefixLookup
    {
        string LookupPrefix(string ns);
    }
}

