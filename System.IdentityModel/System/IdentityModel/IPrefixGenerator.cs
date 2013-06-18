namespace System.IdentityModel
{
    using System;

    internal interface IPrefixGenerator
    {
        string GetPrefix(string namespaceUri, int depth, bool isForAttribute);
    }
}

