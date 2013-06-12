namespace System.Security
{
    using System;

    internal interface ISecurityElementFactory
    {
        string Attribute(string attributeName);
        object Copy();
        SecurityElement CreateSecurityElement();
        string GetTag();
    }
}

