namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public enum XmlSchemaForm
    {
        [XmlIgnore]
        None = 0,
        [XmlEnum("qualified")]
        Qualified = 1,
        [XmlEnum("unqualified")]
        Unqualified = 2
    }
}

