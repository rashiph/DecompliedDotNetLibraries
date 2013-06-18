namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class CompareRequest : DirectoryRequest
    {
        private DirectoryAttribute attribute;
        private string dn;

        public CompareRequest()
        {
            this.attribute = new DirectoryAttribute();
        }

        public CompareRequest(string distinguishedName, DirectoryAttribute assertion)
        {
            this.attribute = new DirectoryAttribute();
            if (assertion == null)
            {
                throw new ArgumentNullException("assertion");
            }
            if (assertion.Count != 1)
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("WrongNumValuesCompare"));
            }
            this.CompareRequestHelper(distinguishedName, assertion.Name, assertion[0]);
        }

        public CompareRequest(string distinguishedName, string attributeName, string value)
        {
            this.attribute = new DirectoryAttribute();
            this.CompareRequestHelper(distinguishedName, attributeName, value);
        }

        public CompareRequest(string distinguishedName, string attributeName, byte[] value)
        {
            this.attribute = new DirectoryAttribute();
            this.CompareRequestHelper(distinguishedName, attributeName, value);
        }

        public CompareRequest(string distinguishedName, string attributeName, Uri value)
        {
            this.attribute = new DirectoryAttribute();
            this.CompareRequestHelper(distinguishedName, attributeName, value);
        }

        private void CompareRequestHelper(string distinguishedName, string attributeName, object value)
        {
            if (attributeName == null)
            {
                throw new ArgumentNullException("attributeName");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.dn = distinguishedName;
            this.attribute.Name = attributeName;
            this.attribute.Add(value);
        }

        protected override XmlElement ToXmlNode(XmlDocument doc)
        {
            XmlElement element = base.CreateRequestElement(doc, "compareRequest", true, this.dn);
            if (this.attribute.Count != 1)
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("WrongNumValuesCompare"));
            }
            XmlElement newChild = this.attribute.ToXmlNode(doc, "assertion");
            element.AppendChild(newChild);
            return element;
        }

        public DirectoryAttribute Assertion
        {
            get
            {
                return this.attribute;
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
    }
}

