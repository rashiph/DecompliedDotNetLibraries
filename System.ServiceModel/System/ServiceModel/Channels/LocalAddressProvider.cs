namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;

    internal class LocalAddressProvider
    {
        private MessageFilter filter;
        private EndpointAddress localAddress;
        private int priority;

        public LocalAddressProvider(EndpointAddress localAddress, MessageFilter filter)
        {
            if (localAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localAddress");
            }
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            this.localAddress = localAddress;
            this.filter = filter;
            if (localAddress.Headers.FindHeader(XD.UtilityDictionary.UniqueEndpointHeaderName.Value, XD.UtilityDictionary.UniqueEndpointHeaderNamespace.Value) == null)
            {
                this.priority = 0x7ffffffe;
            }
            else
            {
                this.priority = 0x7fffffff;
            }
        }

        public MessageFilter Filter
        {
            get
            {
                return this.filter;
            }
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return this.localAddress;
            }
        }

        public int Priority
        {
            get
            {
                return this.priority;
            }
        }
    }
}

