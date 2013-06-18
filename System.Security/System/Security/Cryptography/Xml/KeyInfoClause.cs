namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class KeyInfoClause
    {
        protected KeyInfoClause()
        {
        }

        public abstract XmlElement GetXml();
        internal virtual XmlElement GetXml(XmlDocument xmlDocument)
        {
            XmlElement xml = this.GetXml();
            return (XmlElement) xmlDocument.ImportNode(xml, true);
        }

        public abstract void LoadXml(XmlElement element);
    }
}

