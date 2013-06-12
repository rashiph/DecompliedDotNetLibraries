namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;
    using System.Text;

    [ComVisible(true)]
    public abstract class RSA : AsymmetricAlgorithm
    {
        protected RSA()
        {
        }

        [SecuritySafeCritical]
        public static RSA Create()
        {
            return Create("System.Security.Cryptography.RSA");
        }

        [SecuritySafeCritical]
        public static RSA Create(string algName)
        {
            return (RSA) CryptoConfig.CreateFromName(algName);
        }

        public abstract byte[] DecryptValue(byte[] rgb);
        public abstract byte[] EncryptValue(byte[] rgb);
        public abstract RSAParameters ExportParameters(bool includePrivateParameters);
        public override void FromXmlString(string xmlString)
        {
            if (xmlString == null)
            {
                throw new ArgumentNullException("xmlString");
            }
            RSAParameters parameters = new RSAParameters();
            SecurityElement topElement = new Parser(xmlString).GetTopElement();
            string inputBuffer = topElement.SearchForTextOfLocalName("Modulus");
            if (inputBuffer == null)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString", new object[] { "RSA", "Modulus" }));
            }
            parameters.Modulus = Convert.FromBase64String(Utils.DiscardWhiteSpaces(inputBuffer));
            string str2 = topElement.SearchForTextOfLocalName("Exponent");
            if (str2 == null)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString", new object[] { "RSA", "Exponent" }));
            }
            parameters.Exponent = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str2));
            string str3 = topElement.SearchForTextOfLocalName("P");
            if (str3 != null)
            {
                parameters.P = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str3));
            }
            string str4 = topElement.SearchForTextOfLocalName("Q");
            if (str4 != null)
            {
                parameters.Q = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str4));
            }
            string str5 = topElement.SearchForTextOfLocalName("DP");
            if (str5 != null)
            {
                parameters.DP = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str5));
            }
            string str6 = topElement.SearchForTextOfLocalName("DQ");
            if (str6 != null)
            {
                parameters.DQ = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str6));
            }
            string str7 = topElement.SearchForTextOfLocalName("InverseQ");
            if (str7 != null)
            {
                parameters.InverseQ = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str7));
            }
            string str8 = topElement.SearchForTextOfLocalName("D");
            if (str8 != null)
            {
                parameters.D = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str8));
            }
            this.ImportParameters(parameters);
        }

        public abstract void ImportParameters(RSAParameters parameters);
        public override string ToXmlString(bool includePrivateParameters)
        {
            RSAParameters parameters = this.ExportParameters(includePrivateParameters);
            StringBuilder builder = new StringBuilder();
            builder.Append("<RSAKeyValue>");
            builder.Append("<Modulus>" + Convert.ToBase64String(parameters.Modulus) + "</Modulus>");
            builder.Append("<Exponent>" + Convert.ToBase64String(parameters.Exponent) + "</Exponent>");
            if (includePrivateParameters)
            {
                builder.Append("<P>" + Convert.ToBase64String(parameters.P) + "</P>");
                builder.Append("<Q>" + Convert.ToBase64String(parameters.Q) + "</Q>");
                builder.Append("<DP>" + Convert.ToBase64String(parameters.DP) + "</DP>");
                builder.Append("<DQ>" + Convert.ToBase64String(parameters.DQ) + "</DQ>");
                builder.Append("<InverseQ>" + Convert.ToBase64String(parameters.InverseQ) + "</InverseQ>");
                builder.Append("<D>" + Convert.ToBase64String(parameters.D) + "</D>");
            }
            builder.Append("</RSAKeyValue>");
            return builder.ToString();
        }
    }
}

