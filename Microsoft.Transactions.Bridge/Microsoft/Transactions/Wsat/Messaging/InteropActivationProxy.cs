namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class InteropActivationProxy : ActivationProxy
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InteropActivationProxy(CoordinationService coordination, EndpointAddress to) : base(coordination, to)
        {
        }

        protected override IChannelFactory<IRequestReplyService> SelectChannelFactory(out MessageVersion messageVersion)
        {
            base.interoperating = true;
            messageVersion = base.coordinationService.InteropActivationBinding.MessageVersion;
            return base.coordinationService.InteropActivationChannelFactory;
        }
    }
}

