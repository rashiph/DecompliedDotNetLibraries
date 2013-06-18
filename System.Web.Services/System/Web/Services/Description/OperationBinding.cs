namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;

    [XmlFormatExtensionPoint("Extensions")]
    public sealed class OperationBinding : NamedItem
    {
        private ServiceDescriptionFormatExtensionCollection extensions;
        private FaultBindingCollection faults;
        private InputBinding input;
        private OutputBinding output;
        private System.Web.Services.Description.Binding parent;

        internal void SetParent(System.Web.Services.Description.Binding parent)
        {
            this.parent = parent;
        }

        public System.Web.Services.Description.Binding Binding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
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

        [XmlElement("fault")]
        public FaultBindingCollection Faults
        {
            get
            {
                if (this.faults == null)
                {
                    this.faults = new FaultBindingCollection(this);
                }
                return this.faults;
            }
        }

        [XmlElement("input")]
        public InputBinding Input
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.input;
            }
            set
            {
                if (this.input != null)
                {
                    this.input.SetParent(null);
                }
                this.input = value;
                if (this.input != null)
                {
                    this.input.SetParent(this);
                }
            }
        }

        [XmlElement("output")]
        public OutputBinding Output
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.output;
            }
            set
            {
                if (this.output != null)
                {
                    this.output.SetParent(null);
                }
                this.output = value;
                if (this.output != null)
                {
                    this.output.SetParent(this);
                }
            }
        }
    }
}

