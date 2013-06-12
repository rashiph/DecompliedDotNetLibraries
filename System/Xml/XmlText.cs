namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    public class XmlText : XmlCharacterData
    {
        internal XmlText(string strData) : this(strData, null)
        {
        }

        protected internal XmlText(string strData, XmlDocument doc) : base(strData, doc)
        {
        }

        public override XmlNode CloneNode(bool deep)
        {
            return this.OwnerDocument.CreateTextNode(this.Data);
        }

        public virtual XmlText SplitText(int offset)
        {
            XmlNode parentNode = this.ParentNode;
            int length = this.Length;
            if (offset > length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (parentNode == null)
            {
                throw new InvalidOperationException(Res.GetString("Xdom_TextNode_SplitText"));
            }
            int count = length - offset;
            string str = this.Substring(offset, count);
            this.DeleteData(offset, count);
            XmlText newChild = this.OwnerDocument.CreateTextNode(str);
            parentNode.InsertAfter(newChild, this);
            return newChild;
        }

        public override void WriteContentTo(XmlWriter w)
        {
        }

        public override void WriteTo(XmlWriter w)
        {
            w.WriteString(this.Data);
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
                return this.OwnerDocument.strTextName;
            }
        }

        public override string Name
        {
            get
            {
                return this.OwnerDocument.strTextName;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.Text;
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

        public override string Value
        {
            get
            {
                return this.Data;
            }
            set
            {
                this.Data = value;
                XmlNode parentNode = base.parentNode;
                if ((parentNode != null) && (parentNode.NodeType == XmlNodeType.Attribute))
                {
                    XmlUnspecifiedAttribute attribute = parentNode as XmlUnspecifiedAttribute;
                    if ((attribute != null) && !attribute.Specified)
                    {
                        attribute.SetSpecified(true);
                    }
                }
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

