namespace System.Xml.Serialization
{
    using System;
    using System.Xml;

    public class XmlElementEventArgs : EventArgs
    {
        private XmlElement elem;
        private int lineNumber;
        private int linePosition;
        private object o;
        private string qnames;

        internal XmlElementEventArgs(XmlElement elem, int lineNumber, int linePosition, object o, string qnames)
        {
            this.elem = elem;
            this.o = o;
            this.qnames = qnames;
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;
        }

        public XmlElement Element
        {
            get
            {
                return this.elem;
            }
        }

        public string ExpectedElements
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

