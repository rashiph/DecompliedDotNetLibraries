namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;

    [Serializable]
    internal enum SoapAttributeType
    {
        Embedded = 1,
        None = 0,
        XmlAttribute = 4,
        XmlElement = 2,
        XmlType = 8
    }
}

