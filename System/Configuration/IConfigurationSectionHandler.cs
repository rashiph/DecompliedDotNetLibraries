namespace System.Configuration
{
    using System;
    using System.Xml;

    public interface IConfigurationSectionHandler
    {
        object Create(object parent, object configContext, XmlNode section);
    }
}

