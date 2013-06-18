namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    public class Basic192SecurityAlgorithmSuite : SecurityAlgorithmSuite
    {
        public override bool IsAsymmetricKeyLengthSupported(int length)
        {
            return ((length >= 0x400) && (length <= 0x1000));
        }

        public override bool IsSymmetricKeyLengthSupported(int length)
        {
            return ((length >= 0xc0) && (length <= 0x100));
        }

        public override string ToString()
        {
            return "Basic192";
        }

        public override string DefaultAsymmetricKeyWrapAlgorithm
        {
            get
            {
                return this.DefaultAsymmetricKeyWrapAlgorithmDictionaryString.Value;
            }
        }

        internal override XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.RsaOaepKeyWrap;
            }
        }

        public override string DefaultAsymmetricSignatureAlgorithm
        {
            get
            {
                return this.DefaultAsymmetricSignatureAlgorithmDictionaryString.Value;
            }
        }

        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.RsaSha1Signature;
            }
        }

        public override string DefaultCanonicalizationAlgorithm
        {
            get
            {
                return this.DefaultCanonicalizationAlgorithmDictionaryString.Value;
            }
        }

        internal override XmlDictionaryString DefaultCanonicalizationAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.ExclusiveC14n;
            }
        }

        public override string DefaultDigestAlgorithm
        {
            get
            {
                return this.DefaultDigestAlgorithmDictionaryString.Value;
            }
        }

        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.Sha1Digest;
            }
        }

        public override string DefaultEncryptionAlgorithm
        {
            get
            {
                return this.DefaultEncryptionAlgorithmDictionaryString.Value;
            }
        }

        internal override XmlDictionaryString DefaultEncryptionAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.Aes192Encryption;
            }
        }

        public override int DefaultEncryptionKeyDerivationLength
        {
            get
            {
                return 0xc0;
            }
        }

        public override int DefaultSignatureKeyDerivationLength
        {
            get
            {
                return 0xc0;
            }
        }

        public override int DefaultSymmetricKeyLength
        {
            get
            {
                return 0xc0;
            }
        }

        public override string DefaultSymmetricKeyWrapAlgorithm
        {
            get
            {
                return this.DefaultSymmetricKeyWrapAlgorithmDictionaryString.Value;
            }
        }

        internal override XmlDictionaryString DefaultSymmetricKeyWrapAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.Aes192KeyWrap;
            }
        }

        public override string DefaultSymmetricSignatureAlgorithm
        {
            get
            {
                return this.DefaultSymmetricSignatureAlgorithmDictionaryString.Value;
            }
        }

        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.HmacSha1Signature;
            }
        }
    }
}

