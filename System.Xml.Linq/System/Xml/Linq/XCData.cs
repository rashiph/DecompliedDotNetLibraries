namespace System.Xml.Linq
{
    using System;
    using System.Runtime;
    using System.Xml;

    public class XCData : XText
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XCData(string value) : base(value)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XCData(XCData other) : base(other)
        {
        }

        internal XCData(XmlReader r) : base(r)
        {
        }

        internal override XNode CloneNode()
        {
            return new XCData(this);
        }

        public override void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteCData(base.text);
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.CDATA;
            }
        }
    }
}

