namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class ExtendedRequest : DirectoryRequest
    {
        private string requestName;
        private byte[] requestValue;

        public ExtendedRequest()
        {
        }

        public ExtendedRequest(string requestName)
        {
            this.requestName = requestName;
        }

        public ExtendedRequest(string requestName, byte[] requestValue) : this(requestName)
        {
            this.requestValue = requestValue;
        }

        protected override XmlElement ToXmlNode(XmlDocument doc)
        {
            XmlElement element = base.CreateRequestElement(doc, "extendedRequest", false, null);
            XmlElement newChild = doc.CreateElement("requestName", "urn:oasis:names:tc:DSML:2:0:core");
            newChild.InnerText = this.requestName;
            element.AppendChild(newChild);
            if (this.requestValue != null)
            {
                XmlElement element3 = doc.CreateElement("requestValue", "urn:oasis:names:tc:DSML:2:0:core");
                element3.InnerText = Convert.ToBase64String(this.requestValue);
                XmlAttribute node = doc.CreateAttribute("xsi:type", "http://www.w3.org/2001/XMLSchema-instance");
                node.InnerText = "xsd:base64Binary";
                element3.Attributes.Append(node);
                element.AppendChild(element3);
            }
            return element;
        }

        public string RequestName
        {
            get
            {
                return this.requestName;
            }
            set
            {
                this.requestName = value;
            }
        }

        public byte[] RequestValue
        {
            get
            {
                if (this.requestValue == null)
                {
                    return new byte[0];
                }
                byte[] buffer = new byte[this.requestValue.Length];
                for (int i = 0; i < this.requestValue.Length; i++)
                {
                    buffer[i] = this.requestValue[i];
                }
                return buffer;
            }
            set
            {
                this.requestValue = value;
            }
        }
    }
}

