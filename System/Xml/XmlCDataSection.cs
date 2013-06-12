namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    public class XmlCDataSection : XmlCharacterData
    {
        protected internal XmlCDataSection(string data, XmlDocument doc) : base(data, doc)
        {
        }

        public override XmlNode CloneNode(bool deep)
        {
            return this.OwnerDocument.CreateCDataSection(this.Data);
        }

        public override void WriteContentTo(XmlWriter w)
        {
        }

        public override void WriteTo(XmlWriter w)
        {
            w.WriteCData(this.Data);
        }

        internal override bool IsText
        {
            get
            {
                return true;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.OwnerDocument.strCDataSectionName;
            }
        }

        public override string Name
        {
            get
            {
                return this.OwnerDocument.strCDataSectionName;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.CDATA;
            }
        }

        public override XmlNode ParentNode
        {
            get
            {
                switch (base.parentNode.NodeType)
                {
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    {
                        XmlNode parentNode = base.parentNode.parentNode;
                        while (parentNode.IsText)
                        {
                            parentNode = parentNode.parentNode;
                        }
                        return parentNode;
                    }
                    case XmlNodeType.Document:
                        return null;
                }
                return base.parentNode;
            }
        }

        internal override XmlNode PreviousText
        {
            get
            {
                if (base.parentNode.IsText)
                {
                    return base.parentNode;
                }
                return null;
            }
        }

        internal override XPathNodeType XPNodeType
        {
            get
            {
                return XPathNodeType.Text;
            }
        }
    }
}

