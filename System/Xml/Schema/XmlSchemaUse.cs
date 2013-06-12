namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public enum XmlSchemaUse
    {
        [XmlIgnore]
        None = 0,
        [XmlEnum("optional")]
        Optional = 1,
        [XmlEnum("prohibited")]
        Prohibited = 2,
        [XmlEnum("required")]
        Required = 3
    }
}

