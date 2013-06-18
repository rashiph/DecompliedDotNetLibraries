namespace System.Web.Services.Description
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Xml;
    using System.Xml.Serialization;

    public class SoapHeaderFaultBinding : ServiceDescriptionFormatExtension
    {
        private string encoding;
        private XmlQualifiedName message = XmlQualifiedName.Empty;
        private string ns;
        private string part;
        private SoapBindingUse use;

        [XmlAttribute("encodingStyle"), DefaultValue("")]
        public string Encoding
        {
            get
            {
                if (this.encoding != null)
                {
                    return this.encoding;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.encoding = value;
            }
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

        [XmlAttribute("namespace"), DefaultValue("")]
        public string Namespace
        {
            get
            {
                if (this.ns != null)
                {
                    return this.ns;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.ns = value;
            }
        }

        [XmlAttribute("part")]
        public string Part
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.part;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.part = value;
            }
        }

        [DefaultValue(0), XmlAttribute("use")]
        public SoapBindingUse Use
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.use;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.use = value;
            }
        }
    }
}

