namespace System.Xml.Serialization
{
    using System;
    using System.Xml;

    public interface IXmlTextParser
    {
        bool Normalized { get; set; }

        System.Xml.WhitespaceHandling WhitespaceHandling { get; set; }
    }
}

