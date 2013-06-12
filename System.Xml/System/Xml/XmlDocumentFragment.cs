namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    public class XmlDocumentFragment : XmlNode
    {
        private XmlLinkedNode lastChild;

        protected internal XmlDocumentFragment(XmlDocument ownerDocument)
        {
            if (ownerDocument == null)
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Null_Doc"));
            }
            base.parentNode = ownerDocument;
        }

        internal override bool CanInsertAfter(XmlNode newChild, XmlNode refChild)
        {
            return ((newChild.NodeType != XmlNodeType.XmlDeclaration) || ((refChild == null) && (this.LastNode == null)));
        }

        internal override bool CanInsertBefore(XmlNode newChild, XmlNode refChild)
        {
            if ((newChild.NodeType == XmlNodeType.XmlDeclaration) && (refChild != null))
            {
                return (refChild == this.FirstChild);
            }
            return true;
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlDocument ownerDocument = this.OwnerDocument;
            XmlDocumentFragment fragment = ownerDocument.CreateDocumentFragment();
            if (deep)
            {
                fragment.CopyChildren(ownerDocument, this, deep);
            }
            return fragment;
        }

        internal override bool IsValidChildType(XmlNodeType type)
        {
            switch (type)
            {
                case XmlNodeType.Element:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.EntityReference:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;

                case XmlNodeType.XmlDeclaration:
                {
                    XmlNode firstChild = this.FirstChild;
                    if ((firstChild != null) && (firstChild.NodeType == XmlNodeType.XmlDeclaration))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public override void WriteContentTo(XmlWriter w)
        {
            foreach (XmlNode node in this)
            {
                node.WriteTo(w);
            }
        }

        public override void WriteTo(XmlWriter w)
        {
            this.WriteContentTo(w);
        }

        public override string InnerXml
        {
            get
            {
                return base.InnerXml;
            }
            set
            {
                this.RemoveAll();
                new XmlLoader().ParsePartialContent(this, value, XmlNodeType.Element);
            }
        }

        internal override bool IsContainer
        {
            get
            {
                return true;
            }
        }

        internal override XmlLinkedNode LastNode
        {
            get
            {
                return this.lastChild;
            }
            set
            {
                this.lastChild = value;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.OwnerDocument.strDocumentFragmentName;
            }
        }

        public override string Name
        {
            get
            {
                return this.OwnerDocument.strDocumentFragmentName;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.DocumentFragment;
            }
        }

        public override XmlDocument OwnerDocument
        {
            get
            {
                return (XmlDocument) base.parentNode;
            }
        }

        public override XmlNode ParentNode
        {
            get
            {
                return null;
            }
        }

        internal override XPathNodeType XPNodeType
        {
            get
            {
                return XPathNodeType.Root;
            }
        }
    }
}

