namespace System.Xml.Serialization
{
    using System;
    using System.Xml;

    public class XmlAttributeEventArgs : EventArgs
    {
        private XmlAttribute attr;
        private int lineNumber;
        private int linePosition;
        private object o;
        private string qnames;

        internal XmlAttributeEventArgs(XmlAttribute attr, int lineNumber, int linePosition, object o, string qnames)
        {
            this.attr = attr;
            this.o = o;
            this.qnames = qnames;
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;
        }

        public XmlAttribute Attr
        {
            get
            {
                return this.attr;
            }
        }

        public string ExpectedAttributes
        {
            get
            {
                if (this.qnames != null)
                {
                    return this.qnames;
                }
                return string.Empty;
            }
        }

        public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                return this.linePosition;
            }
        }

        public object ObjectBeingDeserialized
        {
            get
            {
                return this.o;
            }
        }
    }
}

