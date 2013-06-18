namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class CreateCoordinationContextResponseMessage : CoordinationMessage
    {
        private CreateCoordinationContextResponse response;

        public CreateCoordinationContextResponseMessage(MessageVersion version, ref CreateCoordinationContextResponse response) : base(CoordinationStrings.Version(response.ProtocolVersion).CreateCoordinationContextResponseAction, version)
        {
            this.response = response;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            this.response.WriteTo(writer);
        }
    }
}

