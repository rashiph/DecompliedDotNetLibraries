namespace System.Xml.Serialization
{
    using System;
    using System.Xml;

    public class XmlNodeEventArgs : EventArgs
    {
        private int lineNumber;
        private int linePosition;
        private object o;
        private XmlNode xmlNode;

        internal XmlNodeEventArgs(XmlNode xmlNode, int lineNumber, int linePosition, object o)
        {
            this.o = o;
            this.xmlNode = xmlNode;
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;
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

        public string LocalName
        {
            get
            {
                return this.xmlNode.LocalName;
            }
        }

        public string Name
        {
            get
            {
                return this.xmlNode.Name;
            }
        }

        public string NamespaceURI
        {
            get
            {
                return this.xmlNode.NamespaceURI;
            }
        }

        public XmlNodeType NodeType
        {
            get
            {
                return this.xmlNode.NodeType;
            }
        }

        public object ObjectBeingDeserialized
        {
            get
            {
                return this.o;
            }
        }

        public string Text
        {
            get
            {
                return this.xmlNode.Value;
            }
        }
    }
}

