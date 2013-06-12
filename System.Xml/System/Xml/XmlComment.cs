namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    public class XmlComment : XmlCharacterData
    {
        protected internal XmlComment(string comment, XmlDocument doc) : base(comment, doc)
        {
        }

        public override XmlNode CloneNode(bool deep)
        {
            return this.OwnerDocument.CreateComment(this.Data);
        }

        public override void WriteContentTo(XmlWriter w)
        {
        }

        public override void WriteTo(XmlWriter w)
        {
            w.WriteComment(this.Data);
        }

        public override string LocalName
        {
            get
            {
                return this.OwnerDocument.strCommentName;
            }
        }

        public override string Name
        {
            get
            {
                return this.OwnerDocument.strCommentName;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.Comment;
            }
        }

        internal override XPathNodeType XPNodeType
        {
            get
            {
                return XPathNodeType.Comment;
            }
        }
    }
}

