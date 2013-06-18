namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class MsmqChannelListenerBase : TransportChannelListener
    {
        private MsmqReceiveParameters receiveParameters;

        protected MsmqChannelListenerBase(MsmqBindingElementBase bindingElement, BindingContext context, MsmqReceiveParameters receiveParameters, MessageEncoderFactory messageEncoderFactory) : base(bindingElement, context, messageEncoderFactory)
        {
            this.receiveParameters = receiveParameters;
        }

        internal void FaultListener()
        {
            base.Fault();
        }

        internal Exception NormalizePoisonException(long lookupId, Exception innerException)
        {
            if (this.ReceiveParameters.ExactlyOnce)
            {
                return DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqPoisonMessageException(lookupId, innerException));
            }
            if (innerException == null)
            {
                throw Fx.AssertAndThrow("System.ServiceModel.Channels.MsmqChannelListenerBase.NormalizePoisonException(): (innerException == null)");
            }
            return DiagnosticUtility.ExceptionUtility.ThrowHelperError(innerException);
        }

        internal MsmqReceiveParameters ReceiveParameters
        {
            get
            {
                return this.receiveParameters;
            }
        }
    }
}

