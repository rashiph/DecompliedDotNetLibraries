namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class AddRequest : DirectoryRequest
    {
        private DirectoryAttributeCollection attributeList;
        private string dn;

        public AddRequest()
        {
            this.attributeList = new DirectoryAttributeCollection();
        }

        public AddRequest(string distinguishedName, params DirectoryAttribute[] attributes) : this()
        {
            this.dn = distinguishedName;
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    this.attributeList.Add(attributes[i]);
                }
            }
        }

        public AddRequest(string distinguishedName, string objectClass) : this()
        {
            if (objectClass == null)
            {
                throw new ArgumentNullException("objectClass");
            }
            this.dn = distinguishedName;
            DirectoryAttribute attribute = new DirectoryAttribute {
                Name = "objectClass"
            };
            attribute.Add(objectClass);
            this.attributeList.Add(attribute);
        }

        protected override XmlElement ToXmlNode(XmlDocument doc)
        {
            XmlElement element = base.CreateRequestElement(doc, "addRequest", true, this.dn);
            if (this.attributeList != null)
            {
                foreach (DirectoryAttribute attribute in this.attributeList)
                {
                    XmlElement newChild = attribute.ToXmlNode(doc, "attr");
                    element.AppendChild(newChild);
                }
            }
            return element;
        }

        public DirectoryAttributeCollection Attributes
        {
            get
            {
                return this.attributeList;
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

