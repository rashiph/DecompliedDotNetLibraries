namespace System.Web.Services.Description
{
    using System;
    using System.Xml.Serialization;

    public enum SoapBindingUse
    {
        [XmlIgnore]
        Default = 0,
        [XmlEnum("encoded")]
        Encoded = 1,
        [XmlEnum("literal")]
        Literal = 2
    }
}

