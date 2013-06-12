namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public enum XmlSchemaContentProcessing
    {
        [XmlEnum("lax")]
        Lax = 2,
        [XmlIgnore]
        None = 0,
        [XmlEnum("skip")]
        Skip = 1,
        [XmlEnum("strict")]
        Strict = 3
    }
}

