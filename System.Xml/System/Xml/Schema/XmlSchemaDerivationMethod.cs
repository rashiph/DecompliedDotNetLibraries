namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    [Flags]
    public enum XmlSchemaDerivationMethod
    {
        [XmlEnum("#all")]
        All = 0xff,
        [XmlEnum("")]
        Empty = 0,
        [XmlEnum("extension")]
        Extension = 2,
        [XmlEnum("list")]
        List = 8,
        [XmlIgnore]
        None = 0x100,
        [XmlEnum("restriction")]
        Restriction = 4,
        [XmlEnum("substitution")]
        Substitution = 1,
        [XmlEnum("union")]
        Union = 0x10
    }
}

