namespace System.Xml
{
    using System;

    public class XmlEntityReference : XmlLinkedNode
    {
        private XmlLinkedNode lastChild;
        private string name;

        protected internal XmlEntityReference(string name, XmlDocument doc) : base(doc)
        {
            if ((!doc.IsLoading && (name.Length > 0)) && (name[0] == '#'))
            {
                throw new ArgumentException(Res.GetString("Xdom_InvalidCharacter_EntityReference"));
            }
            this.name = doc.NameTable.Add(name);
            doc.fEntRefNodesPresent = true;
        }

        public override XmlNode CloneNode(bool deep)
        {
            return this.OwnerDocument.CreateEntityReference(this.name);
        }

        private string ConstructBaseURI(string baseURI, string systemId)
        {
            if (baseURI == null)
            {
                return systemId;
            }
            int length = baseURI.LastIndexOf('/') + 1;
            string str = baseURI;
            if ((length > 0) && (length < baseURI.Length))
            {
                str = baseURI.Substring(0, length);
            }
            else if (length == 0)
            {
                str = str + @"\";
            }
            return (str + systemId.Replace('\\', '/'));
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
            }
            return false;
        }

        internal override void SetParent(XmlNode node)
        {
            base.SetParent(node);
            if (((this.LastNode == null) && (node != null)) && (node != this.OwnerDocument))
            {
                new XmlLoader().ExpandEntityReference(this);
            }
        }

        internal override void SetParentForLoad(XmlNode node)
        {
            this.SetParent(node);
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
            w.WriteEntityRef(this.name);
        }

        public override string BaseURI
        {
            get
            {
                return this.OwnerDocument.BaseURI;
            }
        }

        internal string ChildBaseURI
        {
            get
            {
                XmlEntity entityNode = this.OwnerDocument.GetEntityNode(this.name);
                if (entityNode == null)
                {
                    return string.Empty;
                }
                if ((entityNode.SystemId != null) && (entityNode.SystemId.Length > 0))
                {
                    return this.ConstructBaseURI(entityNode.BaseURI, entityNode.SystemId);
                }
                return entityNode.BaseURI;
            }
        }

        internal override bool IsContainer
        {
            get
            {
                return true;
            }
        }

        public override bool IsReadOnly
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
                return this.name;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.EntityReference;
            }
        }

        public override string Value
        {
            get
            {
                return null;
            }
            set
            {
                throw new InvalidOperationException(Res.GetString("Xdom_EntRef_SetVal"));
            }
        }
    }
}

