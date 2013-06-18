namespace System.Xml.Linq
{
    using System;
    using System.Runtime;
    using System.Xml;

    public class XComment : XNode
    {
        internal string value;

        public XComment(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.value = value;
        }

        public XComment(XComment other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            this.value = other.value;
        }

        internal XComment(XmlReader r)
        {
            this.value = r.Value;
            r.Read();
        }

        internal override XNode CloneNode()
        {
            return new XComment(this);
        }

        internal override bool DeepEquals(XNode node)
        {
            XComment comment = node as XComment;
            return ((comment != null) && (this.value == comment.value));
        }

        internal override int GetDeepHashCode()
        {
            return this.value.GetHashCode();
        }

        public override void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteComment(this.value);
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.Comment;
            }
        }

        public string Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                bool flag = base.NotifyChanging(this, XObjectChangeEventArgs.Value);
                this.value = value;
                if (flag)
                {
                    base.NotifyChanged(this, XObjectChangeEventArgs.Value);
                }
            }
        }
    }
}

