namespace System.Web.Configuration
{
    using System;
    using System.Xml;

    internal class GatewayDefinition : BrowserDefinition
    {
        internal GatewayDefinition(XmlNode node) : base(node)
        {
        }
    }
}

