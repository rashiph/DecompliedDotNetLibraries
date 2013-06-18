namespace System.DirectoryServices.Protocols
{
    using System;
    using System.ComponentModel;
    using System.Xml;

    public class DirectoryAttributeModification : DirectoryAttribute
    {
        private DirectoryAttributeOperation attributeOperation = DirectoryAttributeOperation.Replace;

        internal XmlElement ToXmlNode(XmlDocument doc)
        {
            XmlElement elemBase = doc.CreateElement("modification", "urn:oasis:names:tc:DSML:2:0:core");
            base.ToXmlNodeCommon(elemBase);
            XmlAttribute node = doc.CreateAttribute("operation", null);
            switch (this.Operation)
            {
                case DirectoryAttributeOperation.Add:
                    node.InnerText = "add";
                    break;

                case DirectoryAttributeOperation.Delete:
                    node.InnerText = "delete";
                    break;

                case DirectoryAttributeOperation.Replace:
                    node.InnerText = "replace";
                    break;

                default:
                    throw new InvalidEnumArgumentException("Operation", (int) this.Operation, typeof(DirectoryAttributeOperation));
            }
            elemBase.Attributes.Append(node);
            return elemBase;
        }

        public DirectoryAttributeOperation Operation
        {
            get
            {
                return this.attributeOperation;
            }
            set
            {
                if ((value < DirectoryAttributeOperation.Add) || (value > DirectoryAttributeOperation.Replace))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DirectoryAttributeOperation));
                }
                this.attributeOperation = value;
            }
        }
    }
}

