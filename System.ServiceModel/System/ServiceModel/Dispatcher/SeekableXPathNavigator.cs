namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    public abstract class SeekableXPathNavigator : XPathNavigator
    {
        protected SeekableXPathNavigator()
        {
        }

        public abstract XmlNodeOrder ComparePosition(long firstPosition, long secondPosition);
        public abstract string GetLocalName(long nodePosition);
        public abstract string GetName(long nodePosition);
        public abstract string GetNamespace(long nodePosition);
        public abstract XPathNodeType GetNodeType(long nodePosition);
        public abstract string GetValue(long nodePosition);

        public abstract long CurrentPosition { get; set; }
    }
}

