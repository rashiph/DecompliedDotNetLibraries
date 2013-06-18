namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class CreateCoordinationContextMessage : CoordinationMessage
    {
        private CreateCoordinationContext create;

        public CreateCoordinationContextMessage(MessageVersion version, ref CreateCoordinationContext create) : base(CoordinationStrings.Version(create.ProtocolVersion).CreateCoordinationContextAction, version)
        {
            this.create = create;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            this.create.WriteTo(writer);
        }
    }
}

