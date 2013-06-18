namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Web.Services.Description;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SoapRpcServiceAttribute : Attribute
    {
        private SoapServiceRoutingStyle routingStyle;
        private SoapBindingUse use = SoapBindingUse.Encoded;

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

        [ComVisible(false)]
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

