namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Xml;
    using System.Xml.Serialization;

    public abstract class OperationMessage : NamedItem
    {
        private XmlQualifiedName message = XmlQualifiedName.Empty;
        private System.Web.Services.Description.Operation parent;

        protected OperationMessage()
        {
        }

        internal void SetParent(System.Web.Services.Description.Operation parent)
        {
            this.parent = parent;
        }

        [XmlAttribute("message")]
        public XmlQualifiedName Message
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.message;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.message = value;
            }
        }

        public System.Web.Services.Description.Operation Operation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
            }
        }
    }
}

