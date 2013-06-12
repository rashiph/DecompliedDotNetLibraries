namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false)]
    public class XmlNamespaceDeclarationsAttribute : Attribute
    {
    }
}

