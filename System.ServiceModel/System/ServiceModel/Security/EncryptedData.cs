namespace System.ServiceModel.Security
{
    using System;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.Xml;

    internal class EncryptedData : EncryptedType
    {
        private SymmetricAlgorithm algorithm;
        private ArraySegment<byte> buffer;
        private byte[] cipherText;
        internal static readonly string ContentType = "http://www.w3.org/2001/04/xmlenc#Content";
        private byte[] decryptedBuffer;
        internal static readonly XmlDictionaryString ElementName = XD.XmlEncryptionDictionary.EncryptedData;
        internal static readonly string ElementType = "http://www.w3.org/2001/04/xmlenc#Element";
        private byte[] iv;

        private void EnsureDecryptionSet()
        {
            if (base.State == EncryptedType.EncryptionState.DecryptionSetup)
            {
                this.SetPlainText();
            }
            else if (base.State != EncryptedType.EncryptionState.Decrypted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("BadEncryptionState")));
            }
        }

        protected override void ForceEncryption()
        {
            CryptoHelper.GenerateIVAndEncrypt(this.algorithm, this.buffer, out this.iv, out this.cipherText);
            base.State = EncryptedType.EncryptionState.Encrypted;
            this.buffer = new ArraySegment<byte>(CryptoHelper.EmptyBuffer);
        }

        public byte[] GetDecryptedBuffer()
        {
            this.EnsureDecryptionSet();
            return this.decryptedBuffer;
        }

        protected override void ReadCipherData(XmlDictionaryReader reader)
        {
            this.cipherText = reader.ReadContentAsBase64();
        }

        protected override void ReadCipherData(XmlDictionaryReader reader, long maxBufferSize)
        {
            this.cipherText = System.ServiceModel.Security.SecurityUtils.ReadContentAsBase64(reader, maxBufferSize);
        }

        private void SetPlainText()
        {
            this.decryptedBuffer = CryptoHelper.ExtractIVAndDecrypt(this.algorithm, this.cipherText, 0, this.cipherText.Length);
            base.State = EncryptedType.EncryptionState.Decrypted;
        }

        public void SetUpDecryption(SymmetricAlgorithm algorithm)
        {
            if (base.State != EncryptedType.EncryptionState.Read)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("BadEncryptionState")));
            }
            if (algorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("algorithm");
            }
            this.algorithm = algorithm;
            base.State = EncryptedType.EncryptionState.DecryptionSetup;
        }

        public void SetUpEncryption(SymmetricAlgorithm algorithm, ArraySegment<byte> buffer)
        {
            if (base.State != EncryptedType.EncryptionState.New)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("BadEncryptionState")));
            }
            if (algorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("algorithm");
            }
            this.algorithm = algorithm;
            this.buffer = buffer;
            base.State = EncryptedType.EncryptionState.EncryptionSetup;
        }

        protected override void WriteCipherData(XmlDictionaryWriter writer)
        {
            writer.WriteBase64(this.iv, 0, this.iv.Length);
            writer.WriteBase64(this.cipherText, 0, this.cipherText.Length);
        }

        protected override XmlDictionaryString OpeningElementName
        {
            get
            {
                return ElementName;
            }
        }
    }
}

