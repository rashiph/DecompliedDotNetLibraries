namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Web;
    using System.Xml;

    internal class ProtocolsConfiguration
    {
        private Hashtable _protocolEntries = new Hashtable();

        internal ProtocolsConfiguration(XmlNode section)
        {
            System.Web.Configuration.HandlerBase.CheckForUnrecognizedAttributes(section);
            foreach (XmlNode node in section.ChildNodes)
            {
                if (!this.IsIgnorableAlsoCheckForNonElement(node))
                {
                    if (node.Name == "add")
                    {
                        string id = System.Web.Configuration.HandlerBase.RemoveRequiredAttribute(node, "id");
                        string processHandlerType = System.Web.Configuration.HandlerBase.RemoveRequiredAttribute(node, "processHandlerType");
                        string appDomainHandlerType = System.Web.Configuration.HandlerBase.RemoveRequiredAttribute(node, "appDomainHandlerType");
                        bool val = true;
                        System.Web.Configuration.HandlerBase.GetAndRemoveBooleanAttribute(node, "validate", ref val);
                        System.Web.Configuration.HandlerBase.CheckForUnrecognizedAttributes(node);
                        System.Web.Configuration.HandlerBase.CheckForNonCommentChildNodes(node);
                        try
                        {
                            this._protocolEntries[id] = new ProtocolsConfigurationEntry(id, processHandlerType, appDomainHandlerType, val, ConfigurationErrorsException.GetFilename(node), ConfigurationErrorsException.GetLineNumber(node));
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        System.Web.Configuration.HandlerBase.ThrowUnrecognizedElement(node);
                    }
                }
            }
        }

        private bool IsIgnorableAlsoCheckForNonElement(XmlNode node)
        {
            if ((node.NodeType == XmlNodeType.Comment) || (node.NodeType == XmlNodeType.Whitespace))
            {
                return true;
            }
            if (node.NodeType != XmlNodeType.Element)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_elements_only"), node);
            }
            return false;
        }
    }
}

