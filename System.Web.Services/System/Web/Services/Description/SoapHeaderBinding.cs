namespace System.Web.Services.Description
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Web.Services.Configuration;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlFormatExtension("header", "http://schemas.xmlsoap.org/wsdl/soap/", typeof(InputBinding), typeof(OutputBinding))]
    public class SoapHeaderBinding : ServiceDescriptionFormatExtension
    {
        private string encoding;
        private SoapHeaderFaultBinding fault;
        private bool mapToProperty;
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

        [XmlElement("headerfault")]
        public SoapHeaderFaultBinding Fault
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fault;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.fault = value;
            }
        }

        [XmlIgnore]
        public bool MapToProperty
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.mapToProperty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.mapToProperty = value;
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

        [DefaultValue(""), XmlAttribute("namespace")]
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

        [XmlAttribute("use"), DefaultValue(0)]
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

