namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    [Serializable]
    internal enum SoapAttributeType
    {
        Embedded = 2,
        None = 0,
        SchemaType = 1,
        XmlAttribute = 8,
        XmlElement = 4
    }
}

