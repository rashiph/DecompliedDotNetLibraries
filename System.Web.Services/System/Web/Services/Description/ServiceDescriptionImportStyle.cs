namespace System.Web.Services.Description
{
    using System;
    using System.Xml.Serialization;

    public enum ServiceDescriptionImportStyle
    {
        [XmlEnum("client")]
        Client = 0,
        [XmlEnum("server")]
        Server = 1,
        [XmlEnum("serverInterface")]
        ServerInterface = 2
    }
}

