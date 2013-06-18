namespace System.ServiceModel
{
    using System;
    using System.IdentityModel.Claims;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;

    public class RsaEndpointIdentity : EndpointIdentity
    {
        public RsaEndpointIdentity(Claim identity)
        {
            if (identity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");
            }
            if (!identity.ClaimType.Equals(ClaimTypes.Rsa))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("UnrecognizedClaimTypeForIdentity", new object[] { identity.ClaimType, ClaimTypes.Rsa }));
            }
            base.Initialize(identity);
        }

        public RsaEndpointIdentity(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            RSA key = certificate.PublicKey.Key as RSA;
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("PublicKeyNotRSA")));
            }
            base.Initialize(Claim.CreateRsaClaim(key));
        }

        public RsaEndpointIdentity(string publicKey)
        {
            if (publicKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("publicKey");
            }
            base.Initialize(Claim.CreateRsaClaim(ToRsa(publicKey)));
        }

        internal RsaEndpointIdentity(XmlDictionaryReader reader)
        {
            reader.ReadStartElement(XD.XmlSignatureDictionary.RsaKeyValue, XD.XmlSignatureDictionary.Namespace);
            byte[] buffer = Convert.FromBase64String(reader.ReadElementString(XD.XmlSignatureDictionary.Modulus.Value, XD.XmlSignatureDictionary.Namespace.Value));
            byte[] buffer2 = Convert.FromBase64String(reader.ReadElementString(XD.XmlSignatureDictionary.Exponent.Value, XD.XmlSignatureDictionary.Namespace.Value));
            reader.ReadEndElement();
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSAParameters parameters = new RSAParameters {
                Exponent = buffer2,
                Modulus = buffer
            };
            rsa.ImportParameters(parameters);
            base.Initialize(Claim.CreateRsaClaim(rsa));
        }

        private static RSA ToRsa(string keyString)
        {
            if (keyString == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyString");
            }
            RSA rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(keyString);
            return rsa;
        }

        internal override void WriteContentsTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.KeyInfo, XD.XmlSignatureDictionary.Namespace);
            writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.RsaKeyValue, XD.XmlSignatureDictionary.Namespace);
            RSAParameters parameters = ((RSA) base.IdentityClaim.Resource).ExportParameters(false);
            writer.WriteElementString(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.Modulus, XD.XmlSignatureDictionary.Namespace, Convert.ToBase64String(parameters.Modulus));
            writer.WriteElementString(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.Exponent, XD.XmlSignatureDictionary.Namespace, Convert.ToBase64String(parameters.Exponent));
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}

