namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Xml;

    internal class XmlDataNode : DataNode<object>
    {
        private XmlDocument ownerDocument;
        private IList<System.Xml.XmlAttribute> xmlAttributes;
        private IList<System.Xml.XmlNode> xmlChildNodes;

        internal XmlDataNode()
        {
            base.dataType = Globals.TypeOfXmlDataNode;
        }

        public override void Clear()
        {
            base.Clear();
            this.xmlAttributes = null;
            this.xmlChildNodes = null;
            this.ownerDocument = null;
        }

        internal XmlDocument OwnerDocument
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ownerDocument;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.ownerDocument = value;
            }
        }

        internal IList<System.Xml.XmlAttribute> XmlAttributes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.xmlAttributes;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.xmlAttributes = value;
            }
        }

        internal IList<System.Xml.XmlNode> XmlChildNodes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.xmlChildNodes;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.xmlChildNodes = value;
            }
        }
    }
}

