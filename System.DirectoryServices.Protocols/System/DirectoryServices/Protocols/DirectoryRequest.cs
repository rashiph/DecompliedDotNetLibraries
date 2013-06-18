namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public abstract class DirectoryRequest : DirectoryOperation
    {
        internal DirectoryControlCollection directoryControlCollection;

        internal DirectoryRequest()
        {
            Utility.CheckOSVersion();
            this.directoryControlCollection = new DirectoryControlCollection();
        }

        internal XmlElement CreateRequestElement(XmlDocument doc, string requestName, bool includeDistinguishedName, string distinguishedName)
        {
            XmlElement element = doc.CreateElement(requestName, "urn:oasis:names:tc:DSML:2:0:core");
            if (includeDistinguishedName)
            {
                XmlAttribute node = doc.CreateAttribute("dn", null);
                node.InnerText = distinguishedName;
                element.Attributes.Append(node);
            }
            if (base.directoryRequestID != null)
            {
                XmlAttribute attribute2 = doc.CreateAttribute("requestID", null);
                attribute2.InnerText = base.directoryRequestID;
                element.Attributes.Append(attribute2);
            }
            if (this.directoryControlCollection != null)
            {
                foreach (DirectoryControl control in this.directoryControlCollection)
                {
                    XmlElement newChild = control.ToXmlNode(doc);
                    element.AppendChild(newChild);
                }
            }
            return element;
        }

        protected abstract XmlElement ToXmlNode(XmlDocument doc);
        internal XmlElement ToXmlNodeHelper(XmlDocument doc)
        {
            return this.ToXmlNode(doc);
        }

        public DirectoryControlCollection Controls
        {
            get
            {
                return this.directoryControlCollection;
            }
        }

        public string RequestId
        {
            get
            {
                return base.directoryRequestID;
            }
            set
            {
                base.directoryRequestID = value;
            }
        }
    }
}

