namespace System.Xml
{
    using System;

    public class XmlEntity : XmlNode
    {
        private string baseURI;
        private bool childrenFoliating;
        private XmlLinkedNode lastChild;
        private string name;
        private string notationName;
        private string publicId;
        private string systemId;
        private string unparsedReplacementStr;

        internal XmlEntity(string name, string strdata, string publicId, string systemId, string notationName, XmlDocument doc) : base(doc)
        {
            this.name = doc.NameTable.Add(name);
            this.publicId = publicId;
            this.systemId = systemId;
            this.notationName = notationName;
            this.unparsedReplacementStr = strdata;
            this.childrenFoliating = false;
        }

        public override XmlNode CloneNode(bool deep)
        {
            throw new InvalidOperationException(Res.GetString("Xdom_Node_Cloning"));
        }

        internal override bool IsValidChildType(XmlNodeType type)
        {
            if ((((type != XmlNodeType.Text) && (type != XmlNodeType.Element)) && ((type != XmlNodeType.ProcessingInstruction) && (type != XmlNodeType.Comment))) && (((type != XmlNodeType.CDATA) && (type != XmlNodeType.Whitespace)) && (type != XmlNodeType.SignificantWhitespace)))
            {
                return (type == XmlNodeType.EntityReference);
            }
            return true;
        }

        internal void SetBaseURI(string inBaseURI)
        {
            this.baseURI = inBaseURI;
        }

        public override void WriteContentTo(XmlWriter w)
        {
        }

        public override void WriteTo(XmlWriter w)
        {
        }

        public override string BaseURI
        {
            get
            {
                return this.baseURI;
            }
        }

        public override string InnerText
        {
            get
            {
                return base.InnerText;
            }
            set
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Ent_Innertext"));
            }
        }

        public override string InnerXml
        {
            get
            {
                return string.Empty;
            }
            set
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Set_InnerXml"));
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
                if ((this.lastChild == null) && !this.childrenFoliating)
                {
                    this.childrenFoliating = true;
                    new XmlLoader().ExpandEntity(this);
                }
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
                return XmlNodeType.Entity;
            }
        }

        public string NotationName
        {
            get
            {
                return this.notationName;
            }
        }

        public override string OuterXml
        {
            get
            {
                return string.Empty;
            }
        }

        public string PublicId
        {
            get
            {
                return this.publicId;
            }
        }

        public string SystemId
        {
            get
            {
                return this.systemId;
            }
        }
    }
}

