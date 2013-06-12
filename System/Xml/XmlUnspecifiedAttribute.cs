namespace System.Xml
{
    using System;

    internal class XmlUnspecifiedAttribute : XmlAttribute
    {
        private bool fSpecified;

        protected internal XmlUnspecifiedAttribute(string prefix, string localName, string namespaceURI, XmlDocument doc) : base(prefix, localName, namespaceURI, doc)
        {
        }

        public override XmlNode AppendChild(XmlNode newChild)
        {
            XmlNode node = base.AppendChild(newChild);
            this.fSpecified = true;
            return node;
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlDocument ownerDocument = this.OwnerDocument;
            XmlUnspecifiedAttribute attribute = (XmlUnspecifiedAttribute) ownerDocument.CreateDefaultAttribute(this.Prefix, this.LocalName, this.NamespaceURI);
            attribute.CopyChildren(ownerDocument, this, true);
            attribute.fSpecified = true;
            return attribute;
        }

        public override XmlNode InsertAfter(XmlNode newChild, XmlNode refChild)
        {
            XmlNode node = base.InsertAfter(newChild, refChild);
            this.fSpecified = true;
            return node;
        }

        public override XmlNode InsertBefore(XmlNode newChild, XmlNode refChild)
        {
            XmlNode node = base.InsertBefore(newChild, refChild);
            this.fSpecified = true;
            return node;
        }

        public override XmlNode RemoveChild(XmlNode oldChild)
        {
            XmlNode node = base.RemoveChild(oldChild);
            this.fSpecified = true;
            return node;
        }

        public override XmlNode ReplaceChild(XmlNode newChild, XmlNode oldChild)
        {
            XmlNode node = base.ReplaceChild(newChild, oldChild);
            this.fSpecified = true;
            return node;
        }

        internal void SetSpecified(bool f)
        {
            this.fSpecified = f;
        }

        public override void WriteTo(XmlWriter w)
        {
            if (this.fSpecified)
            {
                base.WriteTo(w);
            }
        }

        public override string InnerText
        {
            set
            {
                base.InnerText = value;
                this.fSpecified = true;
            }
        }

        public override bool Specified
        {
            get
            {
                return this.fSpecified;
            }
        }
    }
}

