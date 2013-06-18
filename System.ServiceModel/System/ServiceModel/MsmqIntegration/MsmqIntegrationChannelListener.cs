namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml.Serialization;

    internal sealed class MsmqIntegrationChannelListener : MsmqInputChannelListenerBase
    {
        private XmlSerializer[] xmlSerializerList;

        internal MsmqIntegrationChannelListener(MsmqBindingElementBase bindingElement, BindingContext context, MsmqReceiveParameters receiveParameters) : base(bindingElement, context, receiveParameters, null)
        {
            base.SetSecurityTokenAuthenticator(MsmqUri.FormatNameAddressTranslator.Scheme, context);
            MsmqIntegrationReceiveParameters parameters = receiveParameters as MsmqIntegrationReceiveParameters;
            this.xmlSerializerList = XmlSerializer.FromTypes(parameters.TargetSerializationTypes);
        }

        protected override IInputChannel CreateInputChannel(MsmqInputChannelListenerBase listener)
        {
            return new MsmqIntegrationInputChannel(this);
        }

        public override string Scheme
        {
            get
            {
                return "msmq.formatname";
            }
        }

        internal XmlSerializer[] XmlSerializerList
        {
            get
            {
                return this.xmlSerializerList;
            }
        }
    }
}

