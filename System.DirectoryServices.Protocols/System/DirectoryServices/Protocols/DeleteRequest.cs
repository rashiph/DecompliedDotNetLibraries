namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class DeleteRequest : DirectoryRequest
    {
        private string dn;

        public DeleteRequest()
        {
        }

        public DeleteRequest(string distinguishedName)
        {
            this.dn = distinguishedName;
        }

        protected override XmlElement ToXmlNode(XmlDocument doc)
        {
            return base.CreateRequestElement(doc, "delRequest", true, this.dn);
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
    }
}

