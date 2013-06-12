namespace System.Xml.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct XmlDeserializationEvents
    {
        private XmlNodeEventHandler onUnknownNode;
        private XmlAttributeEventHandler onUnknownAttribute;
        private XmlElementEventHandler onUnknownElement;
        private UnreferencedObjectEventHandler onUnreferencedObject;
        internal object sender;
        public XmlNodeEventHandler OnUnknownNode
        {
            get
            {
                return this.onUnknownNode;
            }
            set
            {
                this.onUnknownNode = value;
            }
        }
        public XmlAttributeEventHandler OnUnknownAttribute
        {
            get
            {
                return this.onUnknownAttribute;
            }
            set
            {
                this.onUnknownAttribute = value;
            }
        }
        public XmlElementEventHandler OnUnknownElement
        {
            get
            {
                return this.onUnknownElement;
            }
            set
            {
                this.onUnknownElement = value;
            }
        }
        public UnreferencedObjectEventHandler OnUnreferencedObject
        {
            get
            {
                return this.onUnreferencedObject;
            }
            set
            {
                this.onUnreferencedObject = value;
            }
        }
    }
}

