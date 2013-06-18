namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class TripleDesSha256SecurityAlgorithmSuite : TripleDesSecurityAlgorithmSuite
    {
        public override string ToString()
        {
            return "TripleDesSha256";
        }

        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.RsaSha256Signature;
            }
        }

        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.Sha256Digest;
            }
        }

        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString
        {
            get
            {
                return XD.SecurityAlgorithmDictionary.HmacSha256Signature;
            }
        }
    }
}

