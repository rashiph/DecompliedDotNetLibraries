namespace System.Configuration
{
    using System;
    using System.Xml;

    public class IgnoreSectionHandler : IConfigurationSectionHandler
    {
        public virtual object Create(object parent, object configContext, XmlNode section)
        {
            return null;
        }
    }
}

