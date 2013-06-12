namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;
    using System.Text;

    [ComVisible(true)]
    public abstract class DSA : AsymmetricAlgorithm
    {
        protected DSA()
        {
        }

        [SecuritySafeCritical]
        public static DSA Create()
        {
            return Create("System.Security.Cryptography.DSA");
        }

        [SecuritySafeCritical]
        public static DSA Create(string algName)
        {
            return (DSA) CryptoConfig.CreateFromName(algName);
        }

        public abstract byte[] CreateSignature(byte[] rgbHash);
        public abstract DSAParameters ExportParameters(bool includePrivateParameters);
        public override void FromXmlString(string xmlString)
        {
            if (xmlString == null)
            {
                throw new ArgumentNullException("xmlString");
            }
            DSAParameters parameters = new DSAParameters();
            SecurityElement topElement = new Parser(xmlString).GetTopElement();
            string inputBuffer = topElement.SearchForTextOfLocalName("P");
            if (inputBuffer == null)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString", new object[] { "DSA", "P" }));
            }
            parameters.P = Convert.FromBase64String(Utils.DiscardWhiteSpaces(inputBuffer));
            string str2 = topElement.SearchForTextOfLocalName("Q");
            if (str2 == null)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString", new object[] { "DSA", "Q" }));
            }
            parameters.Q = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str2));
            string str3 = topElement.SearchForTextOfLocalName("G");
            if (str3 == null)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString", new object[] { "DSA", "G" }));
            }
            parameters.G = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str3));
            string str4 = topElement.SearchForTextOfLocalName("Y");
            if (str4 == null)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString", new object[] { "DSA", "Y" }));
            }
            parameters.Y = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str4));
            string str5 = topElement.SearchForTextOfLocalName("J");
            if (str5 != null)
            {
                parameters.J = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str5));
            }
            string str6 = topElement.SearchForTextOfLocalName("X");
            if (str6 != null)
            {
                parameters.X = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str6));
            }
            string str7 = topElement.SearchForTextOfLocalName("Seed");
            string str8 = topElement.SearchForTextOfLocalName("PgenCounter");
            if ((str7 != null) && (str8 != null))
            {
                parameters.Seed = Convert.FromBase64String(Utils.DiscardWhiteSpaces(str7));
                parameters.Counter = Utils.ConvertByteArrayToInt(Convert.FromBase64String(Utils.DiscardWhiteSpaces(str8)));
            }
            else if ((str7 != null) || (str8 != null))
            {
                if (str7 == null)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString", new object[] { "DSA", "Seed" }));
                }
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString", new object[] { "DSA", "PgenCounter" }));
            }
            this.ImportParameters(parameters);
        }

        public abstract void ImportParameters(DSAParameters parameters);
        public override string ToXmlString(bool includePrivateParameters)
        {
            DSAParameters parameters = this.ExportParameters(includePrivateParameters);
            StringBuilder builder = new StringBuilder();
            builder.Append("<DSAKeyValue>");
            builder.Append("<P>" + Convert.ToBase64String(parameters.P) + "</P>");
            builder.Append("<Q>" + Convert.ToBase64String(parameters.Q) + "</Q>");
            builder.Append("<G>" + Convert.ToBase64String(parameters.G) + "</G>");
            builder.Append("<Y>" + Convert.ToBase64String(parameters.Y) + "</Y>");
            if (parameters.J != null)
            {
                builder.Append("<J>" + Convert.ToBase64String(parameters.J) + "</J>");
            }
            if (parameters.Seed != null)
            {
                builder.Append("<Seed>" + Convert.ToBase64String(parameters.Seed) + "</Seed>");
                builder.Append("<PgenCounter>" + Convert.ToBase64String(Utils.ConvertIntToByteArray(parameters.Counter)) + "</PgenCounter>");
            }
            if (includePrivateParameters)
            {
                builder.Append("<X>" + Convert.ToBase64String(parameters.X) + "</X>");
            }
            builder.Append("</DSAKeyValue>");
            return builder.ToString();
        }

        public abstract bool VerifySignature(byte[] rgbHash, byte[] rgbSignature);
    }
}

