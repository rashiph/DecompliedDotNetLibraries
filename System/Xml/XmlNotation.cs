namespace System.Xml
{
    using System;

    public class XmlNotation : XmlNode
    {
        private string name;
        private string publicId;
        private string systemId;

        internal XmlNotation(string name, string publicId, string systemId, XmlDocument doc) : base(doc)
        {
            this.name = doc.NameTable.Add(name);
            this.publicId = publicId;
            this.systemId = systemId;
        }

        public override XmlNode CloneNode(bool deep)
        {
            throw new InvalidOperationException(Res.GetString("Xdom_Node_Cloning"));
        }

        public override void WriteContentTo(XmlWriter w)
        {
        }

        public override void WriteTo(XmlWriter w)
        {
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

        public override bool IsReadOnly
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
                return XmlNodeType.Notation;
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

