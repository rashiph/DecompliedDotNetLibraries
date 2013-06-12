namespace System.Xml.Serialization
{
    using System;

    [Flags]
    public enum CodeGenerationOptions
    {
        [XmlEnum("enableDataBinding")]
        EnableDataBinding = 0x10,
        [XmlEnum("newAsync")]
        GenerateNewAsync = 2,
        [XmlEnum("oldAsync")]
        GenerateOldAsync = 4,
        [XmlEnum("order")]
        GenerateOrder = 8,
        [XmlEnum("properties")]
        GenerateProperties = 1,
        [XmlIgnore]
        None = 0
    }
}

