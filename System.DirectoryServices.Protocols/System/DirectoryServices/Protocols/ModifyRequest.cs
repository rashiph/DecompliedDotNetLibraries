namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class ModifyRequest : DirectoryRequest
    {
        private DirectoryAttributeModificationCollection attributeModificationList;
        private string dn;

        public ModifyRequest()
        {
            this.attributeModificationList = new DirectoryAttributeModificationCollection();
        }

        public ModifyRequest(string distinguishedName, params DirectoryAttributeModification[] modifications) : this()
        {
            this.dn = distinguishedName;
            this.attributeModificationList.AddRange(modifications);
        }

        public ModifyRequest(string distinguishedName, DirectoryAttributeOperation operation, string attributeName, params object[] values) : this()
        {
            this.dn = distinguishedName;
            if (attributeName == null)
            {
                throw new ArgumentNullException("attributeName");
            }
            DirectoryAttributeModification attribute = new DirectoryAttributeModification {
                Operation = operation,
                Name = attributeName
            };
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    attribute.Add(values[i]);
                }
            }
            this.attributeModificationList.Add(attribute);
        }

        protected override XmlElement ToXmlNode(XmlDocument doc)
        {
            XmlElement element = base.CreateRequestElement(doc, "modifyRequest", true, this.dn);
            if (this.attributeModificationList != null)
            {
                foreach (DirectoryAttributeModification modification in this.attributeModificationList)
                {
                    XmlElement newChild = modification.ToXmlNode(doc);
                    element.AppendChild(newChild);
                }
            }
            return element;
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

        public DirectoryAttributeModificationCollection Modifications
        {
            get
            {
                return this.attributeModificationList;
            }
        }
    }
}

