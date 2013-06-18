namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class WSSecurityOneDotOneReceiveSecurityHeader : WSSecurityOneDotZeroReceiveSecurityHeader
    {
        public WSSecurityOneDotOneReceiveSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, int headerIndex, MessageDirection direction) : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, headerIndex, direction)
        {
        }

        protected override DecryptedHeader DecryptHeader(XmlDictionaryReader reader, WrappedKeySecurityToken wrappedKeyToken)
        {
            SecurityToken token;
            EncryptedHeaderXml xml = new EncryptedHeaderXml(base.Version) {
                SecurityTokenSerializer = base.StandardsManager.SecurityTokenSerializer
            };
            xml.ReadFrom(reader, base.MaxReceivedMessageSize);
            if (xml.MustUnderstand != this.MustUnderstand)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("EncryptedHeaderAttributeMismatch", new object[] { XD.MessageDictionary.MustUnderstand.Value, xml.MustUnderstand, this.MustUnderstand })));
            }
            if (xml.Relay != this.Relay)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("EncryptedHeaderAttributeMismatch", new object[] { XD.Message12Dictionary.Relay.Value, xml.Relay, this.Relay })));
            }
            if (xml.Actor != this.Actor)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("EncryptedHeaderAttributeMismatch", new object[] { base.Version.Envelope.DictionaryActor, xml.Actor, this.Actor })));
            }
            if (wrappedKeyToken == null)
            {
                token = WSSecurityOneDotZeroReceiveSecurityHeader.ResolveKeyIdentifier(xml.KeyIdentifier, base.CombinedPrimaryTokenResolver, false);
            }
            else
            {
                token = wrappedKeyToken;
            }
            base.RecordEncryptionToken(token);
            using (SymmetricAlgorithm algorithm = WSSecurityOneDotZeroReceiveSecurityHeader.CreateDecryptionAlgorithm(token, xml.EncryptionMethod, base.AlgorithmSuite))
            {
                xml.SetUpDecryption(algorithm);
                return new DecryptedHeader(xml.GetDecryptedBuffer(), base.SecurityVerifiedMessage.GetEnvelopeAttributes(), base.SecurityVerifiedMessage.GetHeaderAttributes(), base.Version, base.StandardsManager.IdManager, base.ReaderQuotas);
            }
        }
    }
}

