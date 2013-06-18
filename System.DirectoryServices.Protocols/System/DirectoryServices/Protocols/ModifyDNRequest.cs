namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class ModifyDNRequest : DirectoryRequest
    {
        private bool deleteOldRDN;
        private string dn;
        private string newRDN;
        private string newSuperior;

        public ModifyDNRequest()
        {
            this.deleteOldRDN = true;
        }

        public ModifyDNRequest(string distinguishedName, string newParentDistinguishedName, string newName)
        {
            this.deleteOldRDN = true;
            this.dn = distinguishedName;
            this.newSuperior = newParentDistinguishedName;
            this.newRDN = newName;
        }

        protected override XmlElement ToXmlNode(XmlDocument doc)
        {
            XmlElement element = base.CreateRequestElement(doc, "modDNRequest", true, this.dn);
            XmlAttribute node = doc.CreateAttribute("newrdn", null);
            node.InnerText = this.newRDN;
            element.Attributes.Append(node);
            XmlAttribute attribute2 = doc.CreateAttribute("deleteoldrdn", null);
            attribute2.InnerText = this.deleteOldRDN ? "true" : "false";
            element.Attributes.Append(attribute2);
            if (this.newSuperior != null)
            {
                XmlAttribute attribute3 = doc.CreateAttribute("newSuperior", null);
                attribute3.InnerText = this.newSuperior;
                element.Attributes.Append(attribute3);
            }
            return element;
        }

        public bool DeleteOldRdn
        {
            get
            {
                return this.deleteOldRDN;
            }
            set
            {
                this.deleteOldRDN = value;
            }
        }

        public string DistinguishedName
        {
            get
            {
                return this.dn;
            }
            set
            {
                this.dn = value;
            }
        }

        public string NewName
        {
            get
            {
                return this.newRDN;
            }
            set
            {
                this.newRDN = value;
            }
        }

        public string NewParentDistinguishedName
        {
            get
            {
                return this.newSuperior;
            }
            set
            {
                this.newSuperior = value;
            }
        }
    }
}

