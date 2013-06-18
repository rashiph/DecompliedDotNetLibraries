namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class RegisterMessage : CoordinationMessage
    {
        private Register register;

        public RegisterMessage(MessageVersion version, ref Register registerBody) : base(CoordinationStrings.Version(registerBody.ProtocolVersion).RegisterAction, version)
        {
            this.register = registerBody;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            this.register.WriteTo(writer);
        }
    }
}

