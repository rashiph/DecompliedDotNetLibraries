namespace System.IdentityModel
{
    using System;
    using System.Security.Cryptography;
    using System.Xml;

    internal sealed class EnvelopedSignatureTransform : Transform
    {
        private string prefix = "";

        public override object Process(object input, SignatureResourcePool resourcePool, DictionaryManager dictionaryManager)
        {
            XmlTokenStream stream = input as XmlTokenStream;
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedInputTypeForTransform", new object[] { input.GetType() })));
            }
            stream.SetElementExclusion("Signature", "http://www.w3.org/2000/09/xmldsig#");
            return stream;
        }

        public override byte[] ProcessAndDigest(object input, SignatureResourcePool resourcePool, string digestAlgorithm, DictionaryManager dictionaryManager)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedLastTransform")));
        }

        public override void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
        {
            reader.MoveToContent();
            if (XmlHelper.ReadEmptyElementAndRequiredAttribute(reader, dictionaryManager.XmlSignatureDictionary.Transform, dictionaryManager.XmlSignatureDictionary.Namespace, dictionaryManager.XmlSignatureDictionary.Algorithm, out this.prefix) != this.Algorithm)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("AlgorithmMismatchForTransform")));
            }
        }

        public override void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.Transform, dictionaryManager.XmlSignatureDictionary.Namespace);
            writer.WriteAttributeString(dictionaryManager.XmlSignatureDictionary.Algorithm, null, this.Algorithm);
            writer.WriteEndElement();
        }

        public override string Algorithm
        {
            get
            {
                return XD.XmlSignatureDictionary.EnvelopedSignature.Value;
            }
        }
    }
}

