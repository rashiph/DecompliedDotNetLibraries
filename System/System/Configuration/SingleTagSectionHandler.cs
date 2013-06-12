namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Xml;

    public class SingleTagSectionHandler : IConfigurationSectionHandler
    {
        public virtual object Create(object parent, object context, XmlNode section)
        {
            Hashtable hashtable;
            if (parent == null)
            {
                hashtable = new Hashtable();
            }
            else
            {
                hashtable = new Hashtable((IDictionary) parent);
            }
            HandlerBase.CheckForChildNodes(section);
            foreach (XmlAttribute attribute in section.Attributes)
            {
                hashtable[attribute.Name] = attribute.Value;
            }
            return hashtable;
        }
    }
}

