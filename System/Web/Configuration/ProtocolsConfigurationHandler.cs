namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Xml;

    public sealed class ProtocolsConfigurationHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContextObj, XmlNode section)
        {
            return new ProtocolsConfiguration(section);
        }
    }
}

