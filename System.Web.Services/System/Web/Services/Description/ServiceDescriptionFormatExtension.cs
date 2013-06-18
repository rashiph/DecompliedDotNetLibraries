namespace System.Web.Services.Description
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Xml.Serialization;

    public abstract class ServiceDescriptionFormatExtension
    {
        private bool handled;
        private object parent;
        private bool required;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ServiceDescriptionFormatExtension()
        {
        }

        internal void SetParent(object parent)
        {
            this.parent = parent;
        }

        [XmlIgnore]
        public bool Handled
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.handled;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.handled = value;
            }
        }

        public object Parent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
            }
        }

        [XmlAttribute("required", Namespace="http://schemas.xmlsoap.org/wsdl/"), DefaultValue(false)]
        public bool Required
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.required;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.required = value;
            }
        }
    }
}

