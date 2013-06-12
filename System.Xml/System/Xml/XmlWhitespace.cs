namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    public class XmlWhitespace : XmlCharacterData
    {
        protected internal XmlWhitespace(string strData, XmlDocument doc) : base(strData, doc)
        {
            if (!doc.IsLoading && !base.CheckOnData(strData))
            {
                throw new ArgumentException(Res.GetString("Xdom_WS_Char"));
            }
        }

        public override XmlNode CloneNode(bool deep)
        {
            return this.OwnerDocument.CreateWhitespace(this.Data);
        }

        public override void WriteContentTo(XmlWriter w)
        {
        }

        public override void WriteTo(XmlWriter w)
        {
            w.WriteWhitespace(this.Data);
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
                return this.OwnerDocument.strNonSignificantWhitespaceName;
            }
        }

        public override string Name
        {
            get
            {
                return this.OwnerDocument.strNonSignificantWhitespaceName;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.Whitespace;
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
                        return base.ParentNode;
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

        public override string Value
        {
            get
            {
                return this.Data;
            }
            set
            {
                if (!base.CheckOnData(value))
                {
                    throw new ArgumentException(Res.GetString("Xdom_WS_Char"));
                }
                this.Data = value;
            }
        }

        internal override XPathNodeType XPNodeType
        {
            get
            {
                XPathNodeType whitespace = XPathNodeType.Whitespace;
                base.DecideXPNodeTypeForTextNodes(this, ref whitespace);
                return whitespace;
            }
        }
    }
}

