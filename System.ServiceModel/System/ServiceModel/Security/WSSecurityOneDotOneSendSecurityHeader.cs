namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Security.Cryptography;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    internal sealed class WSSecurityOneDotOneSendSecurityHeader : WSSecurityOneDotZeroSendSecurityHeader
    {
        public WSSecurityOneDotOneSendSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction) : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, direction)
        {
        }

        protected override ISignatureValueSecurityElement[] CreateSignatureConfirmationElements(SignatureConfirmations signatureConfirmations)
        {
            if ((signatureConfirmations == null) || (signatureConfirmations.Count == 0))
            {
                return null;
            }
            ISignatureValueSecurityElement[] elementArray = new ISignatureValueSecurityElement[signatureConfirmations.Count];
            for (int i = 0; i < signatureConfirmations.Count; i++)
            {
                byte[] buffer;
                bool flag;
                signatureConfirmations.GetConfirmation(i, out buffer, out flag);
                elementArray[i] = new SignatureConfirmationElement(base.GenerateId(), buffer, base.StandardsManager.SecurityVersion);
            }
            return elementArray;
        }

        protected override EncryptedHeader EncryptHeader(MessageHeader plainTextHeader, SymmetricAlgorithm algorithm, SecurityKeyIdentifier keyIdentifier, MessageVersion version, string id, MemoryStream stream)
        {
            EncryptedHeaderXml headerXml = new EncryptedHeaderXml(version) {
                SecurityTokenSerializer = base.StandardsManager.SecurityTokenSerializer,
                EncryptionMethod = base.EncryptionAlgorithm,
                EncryptionMethodDictionaryString = base.EncryptionAlgorithmDictionaryString,
                KeyIdentifier = keyIdentifier,
                Id = id,
                MustUnderstand = this.MustUnderstand,
                Relay = this.Relay,
                Actor = this.Actor
            };
            headerXml.SetUpEncryption(algorithm, stream);
            return new EncryptedHeader(plainTextHeader, headerXml, EncryptedHeaderXml.ElementName.Value, EncryptedHeaderXml.NamespaceUri.Value, version);
        }
    }
}

