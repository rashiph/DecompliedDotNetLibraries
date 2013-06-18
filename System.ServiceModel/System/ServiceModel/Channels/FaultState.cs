namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FaultState
    {
        private Message faultMessage;
        private System.ServiceModel.Channels.RequestContext requestContext;
        public FaultState(System.ServiceModel.Channels.RequestContext requestContext, Message faultMessage)
        {
            this.requestContext = requestContext;
            this.faultMessage = faultMessage;
        }

        public Message FaultMessage
        {
            get
            {
                return this.faultMessage;
            }
        }
        public System.ServiceModel.Channels.RequestContext RequestContext
        {
            get
            {
                return this.requestContext;
            }
        }
    }
}

