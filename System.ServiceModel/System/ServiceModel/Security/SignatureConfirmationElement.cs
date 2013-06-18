namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.ServiceModel;
    using System.Xml;

    internal class SignatureConfirmationElement : ISignatureValueSecurityElement, ISecurityElement
    {
        private string id;
        private byte[] signatureValue;
        private SecurityVersion version;

        public SignatureConfirmationElement(string id, byte[] signatureValue, SecurityVersion version)
        {
            if (id == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            }
            if (signatureValue == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signatureValue");
            }
            this.id = id;
            this.signatureValue = signatureValue;
            this.version = version;
        }

        public byte[] GetSignatureValue()
        {
            return this.signatureValue;
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            this.version.WriteSignatureConfirmation(writer, this.id, this.signatureValue);
        }

        public bool HasId
        {
            get
            {
                return true;
            }
        }

        public string Id
        {
            get
            {
                return this.id;
            }
        }
    }
}

