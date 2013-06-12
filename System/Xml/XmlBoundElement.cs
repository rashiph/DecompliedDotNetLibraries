namespace System.Xml
{
    using System;
    using System.Data;
    using System.Threading;

    internal sealed class XmlBoundElement : XmlElement
    {
        private DataRow row;
        private System.Xml.ElementState state;

        internal XmlBoundElement(string prefix, string localName, string namespaceURI, XmlDocument doc) : base(prefix, localName, namespaceURI, doc)
        {
            this.state = System.Xml.ElementState.None;
        }

        public override XmlNode AppendChild(XmlNode newChild)
        {
            this.AutoFoliate();
            return base.AppendChild(newChild);
        }

        private void AutoFoliate()
        {
            XmlDataDocument ownerDocument = (XmlDataDocument) this.OwnerDocument;
            if (ownerDocument != null)
            {
                ownerDocument.Foliate(this, ownerDocument.AutoFoliationState);
            }
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlElement element;
            XmlDataDocument ownerDocument = (XmlDataDocument) this.OwnerDocument;
            System.Xml.ElementState autoFoliationState = ownerDocument.AutoFoliationState;
            ownerDocument.AutoFoliationState = System.Xml.ElementState.WeakFoliation;
            try
            {
                this.Foliate(System.Xml.ElementState.WeakFoliation);
                element = (XmlElement) base.CloneNode(deep);
            }
            finally
            {
                ownerDocument.AutoFoliationState = autoFoliationState;
            }
            return element;
        }

        internal void Foliate(System.Xml.ElementState newState)
        {
            XmlDataDocument ownerDocument = (XmlDataDocument) this.OwnerDocument;
            if (ownerDocument != null)
            {
                ownerDocument.Foliate(this, newState);
            }
        }

        public override XmlNodeList GetElementsByTagName(string name)
        {
            XmlNodeList elementsByTagName = base.GetElementsByTagName(name);
            int count = elementsByTagName.Count;
            return elementsByTagName;
        }

        public override XmlNode InsertAfter(XmlNode newChild, XmlNode refChild)
        {
            this.AutoFoliate();
            return base.InsertAfter(newChild, refChild);
        }

        public override XmlNode InsertBefore(XmlNode newChild, XmlNode refChild)
        {
            this.AutoFoliate();
            return base.InsertBefore(newChild, refChild);
        }

        internal void RemoveAllChildren()
        {
            XmlNode firstChild = this.FirstChild;
            XmlNode nextSibling = null;
            while (firstChild != null)
            {
                nextSibling = firstChild.NextSibling;
                this.RemoveChild(firstChild);
                firstChild = nextSibling;
            }
        }

        public override XmlNode ReplaceChild(XmlNode newChild, XmlNode oldChild)
        {
            this.AutoFoliate();
            return base.ReplaceChild(newChild, oldChild);
        }

        private static void WriteBoundElementContentTo(DataPointer dp, XmlWriter w)
        {
            if (!dp.IsEmptyElement && dp.MoveToFirstChild())
            {
                do
                {
                    WriteTo(dp, w);
                }
                while (dp.MoveToNextSibling());
                dp.MoveToParent();
            }
        }

        private static void WriteBoundElementTo(DataPointer dp, XmlWriter w)
        {
            w.WriteStartElement(dp.Prefix, dp.LocalName, dp.NamespaceURI);
            int attributeCount = dp.AttributeCount;
            if (attributeCount > 0)
            {
                for (int i = 0; i < attributeCount; i++)
                {
                    dp.MoveToAttribute(i);
                    WriteTo(dp, w);
                    dp.MoveToOwnerElement();
                }
            }
            WriteBoundElementContentTo(dp, w);
            if (dp.IsEmptyElement)
            {
                w.WriteEndElement();
            }
            else
            {
                w.WriteFullEndElement();
            }
        }

        public override void WriteContentTo(XmlWriter w)
        {
            DataPointer dp = new DataPointer((XmlDataDocument) this.OwnerDocument, this);
            try
            {
                dp.AddPointer();
                WriteBoundElementContentTo(dp, w);
            }
            finally
            {
                dp.SetNoLongerUse();
            }
        }

        private void WriteRootBoundElementTo(DataPointer dp, XmlWriter w)
        {
            XmlDataDocument ownerDocument = (XmlDataDocument) this.OwnerDocument;
            w.WriteStartElement(dp.Prefix, dp.LocalName, dp.NamespaceURI);
            int attributeCount = dp.AttributeCount;
            bool flag = false;
            if (attributeCount > 0)
            {
                for (int i = 0; i < attributeCount; i++)
                {
                    dp.MoveToAttribute(i);
                    if ((dp.Prefix == "xmlns") && (dp.LocalName == "xsi"))
                    {
                        flag = true;
                    }
                    WriteTo(dp, w);
                    dp.MoveToOwnerElement();
                }
            }
            if ((!flag && ownerDocument.bLoadFromDataSet) && ownerDocument.bHasXSINIL)
            {
                w.WriteAttributeString("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "http://www.w3.org/2001/XMLSchema-instance");
            }
            WriteBoundElementContentTo(dp, w);
            if (dp.IsEmptyElement)
            {
                w.WriteEndElement();
            }
            else
            {
                w.WriteFullEndElement();
            }
        }

        public override void WriteTo(XmlWriter w)
        {
            DataPointer dp = new DataPointer((XmlDataDocument) this.OwnerDocument, this);
            try
            {
                dp.AddPointer();
                this.WriteRootBoundElementTo(dp, w);
            }
            finally
            {
                dp.SetNoLongerUse();
            }
        }

        private static void WriteTo(DataPointer dp, XmlWriter w)
        {
            switch (dp.NodeType)
            {
                case XmlNodeType.Element:
                    WriteBoundElementTo(dp, w);
                    return;

                case XmlNodeType.Attribute:
                    if (dp.IsDefault)
                    {
                        break;
                    }
                    w.WriteStartAttribute(dp.Prefix, dp.LocalName, dp.NamespaceURI);
                    if (dp.MoveToFirstChild())
                    {
                        do
                        {
                            WriteTo(dp, w);
                        }
                        while (dp.MoveToNextSibling());
                        dp.MoveToParent();
                    }
                    w.WriteEndAttribute();
                    return;

                case XmlNodeType.Text:
                    w.WriteString(dp.Value);
                    return;

                default:
                    if (dp.GetNode() != null)
                    {
                        dp.GetNode().WriteTo(w);
                    }
                    break;
            }
        }

        public override XmlAttributeCollection Attributes
        {
            get
            {
                this.AutoFoliate();
                return base.Attributes;
            }
        }

        internal System.Xml.ElementState ElementState
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }

        public override XmlNode FirstChild
        {
            get
            {
                this.AutoFoliate();
                return base.FirstChild;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return (this.Attributes.Count > 0);
            }
        }

        public override bool HasChildNodes
        {
            get
            {
                this.AutoFoliate();
                return base.HasChildNodes;
            }
        }

        public override string InnerXml
        {
            get
            {
                return base.InnerXml;
            }
            set
            {
                this.RemoveAllChildren();
                XmlDataDocument ownerDocument = (XmlDataDocument) this.OwnerDocument;
                bool ignoreXmlEvents = ownerDocument.IgnoreXmlEvents;
                bool ignoreDataSetEvents = ownerDocument.IgnoreDataSetEvents;
                ownerDocument.IgnoreXmlEvents = true;
                ownerDocument.IgnoreDataSetEvents = true;
                base.InnerXml = value;
                ownerDocument.SyncTree(this);
                ownerDocument.IgnoreDataSetEvents = ignoreDataSetEvents;
                ownerDocument.IgnoreXmlEvents = ignoreXmlEvents;
            }
        }

        internal bool IsFoliated
        {
            get
            {
                while ((this.state == System.Xml.ElementState.Foliating) || (this.state == System.Xml.ElementState.Defoliating))
                {
                    Thread.Sleep(0);
                }
                return (this.state != System.Xml.ElementState.Defoliated);
            }
        }

        public override XmlNode LastChild
        {
            get
            {
                this.AutoFoliate();
                return base.LastChild;
            }
        }

        public override XmlNode NextSibling
        {
            get
            {
                XmlNode nextSibling = base.NextSibling;
                if (nextSibling == null)
                {
                    XmlBoundElement parentNode = this.ParentNode as XmlBoundElement;
                    if (parentNode != null)
                    {
                        parentNode.AutoFoliate();
                        return base.NextSibling;
                    }
                }
                return nextSibling;
            }
        }

        public override XmlNode PreviousSibling
        {
            get
            {
                XmlNode previousSibling = base.PreviousSibling;
                if (previousSibling == null)
                {
                    XmlBoundElement parentNode = this.ParentNode as XmlBoundElement;
                    if (parentNode != null)
                    {
                        parentNode.AutoFoliate();
                        return base.PreviousSibling;
                    }
                }
                return previousSibling;
            }
        }

        internal DataRow Row
        {
            get
            {
                return this.row;
            }
            set
            {
                this.row = value;
            }
        }

        internal XmlNode SafeFirstChild
        {
            get
            {
                return base.FirstChild;
            }
        }

        internal XmlNode SafeNextSibling
        {
            get
            {
                return base.NextSibling;
            }
        }

        internal XmlNode SafePreviousSibling
        {
            get
            {
                return base.PreviousSibling;
            }
        }
    }
}

