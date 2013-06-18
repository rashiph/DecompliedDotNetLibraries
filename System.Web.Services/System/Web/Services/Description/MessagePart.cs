namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Web.Services.Configuration;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlFormatExtensionPoint("Extensions")]
    public sealed class MessagePart : NamedItem
    {
        private XmlQualifiedName element = XmlQualifiedName.Empty;
        private ServiceDescriptionFormatExtensionCollection extensions;
        private System.Web.Services.Description.Message parent;
        private XmlQualifiedName type = XmlQualifiedName.Empty;

        internal void SetParent(System.Web.Services.Description.Message parent)
        {
            this.parent = parent;
        }

        [XmlAttribute("element")]
        public XmlQualifiedName Element
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.element;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.element = value;
            }
        }

        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new ServiceDescriptionFormatExtensionCollection(this);
                }
                return this.extensions;
            }
        }

        public System.Web.Services.Description.Message Message
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
            }
        }

        [XmlAttribute("type")]
        public XmlQualifiedName Type
        {
            get
            {
                if (this.type == null)
                {
                    return XmlQualifiedName.Empty;
                }
                return this.type;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.type = value;
            }
        }
    }
}

