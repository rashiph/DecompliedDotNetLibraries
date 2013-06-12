namespace System.Xml
{
    using System;

    public class XmlNodeChangedEventArgs : EventArgs
    {
        private XmlNodeChangedAction action;
        private XmlNode newParent;
        private string newValue;
        private XmlNode node;
        private XmlNode oldParent;
        private string oldValue;

        public XmlNodeChangedEventArgs(XmlNode node, XmlNode oldParent, XmlNode newParent, string oldValue, string newValue, XmlNodeChangedAction action)
        {
            this.node = node;
            this.oldParent = oldParent;
            this.newParent = newParent;
            this.action = action;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public XmlNodeChangedAction Action
        {
            get
            {
                return this.action;
            }
        }

        public XmlNode NewParent
        {
            get
            {
                return this.newParent;
            }
        }

        public string NewValue
        {
            get
            {
                return this.newValue;
            }
        }

        public XmlNode Node
        {
            get
            {
                return this.node;
            }
        }

        public XmlNode OldParent
        {
            get
            {
                return this.oldParent;
            }
        }

        public string OldValue
        {
            get
            {
                return this.oldValue;
            }
        }
    }
}

