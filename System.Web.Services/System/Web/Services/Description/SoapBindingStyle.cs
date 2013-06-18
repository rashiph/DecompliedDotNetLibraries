namespace System.Web.Services.Description
{
    using System;
    using System.Xml.Serialization;

    public enum SoapBindingStyle
    {
        [XmlIgnore]
        Default = 0,
        [XmlEnum("document")]
        Document = 1,
        [XmlEnum("rpc")]
        Rpc = 2
    }
}

