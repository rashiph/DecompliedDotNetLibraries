namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;
    using System.Web.Services.Description;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SoapDocumentServiceAttribute : Attribute
    {
        private SoapParameterStyle paramStyle;
        private SoapServiceRoutingStyle routingStyle;
        private SoapBindingUse use;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SoapDocumentServiceAttribute()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SoapDocumentServiceAttribute(SoapBindingUse use)
        {
            this.use = use;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SoapDocumentServiceAttribute(SoapBindingUse use, SoapParameterStyle paramStyle)
        {
            this.use = use;
            this.paramStyle = paramStyle;
        }

        public SoapParameterStyle ParameterStyle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.paramStyle;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.paramStyle = value;
            }
        }

        public SoapServiceRoutingStyle RoutingStyle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.routingStyle;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.routingStyle = value;
            }
        }

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

