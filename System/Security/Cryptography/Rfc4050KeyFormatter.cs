namespace System.Security.Cryptography
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;

    internal static class Rfc4050KeyFormatter
    {
        private const string DomainParametersRoot = "DomainParameters";
        private const string ECDHRoot = "ECDHKeyValue";
        private const string ECDsaRoot = "ECDSAKeyValue";
        private const string NamedCurveElement = "NamedCurve";
        private const string Namespace = "http://www.w3.org/2001/04/xmldsig-more#";
        private const string Prime256CurveUrn = "urn:oid:1.2.840.10045.3.1.7";
        private const string Prime384CurveUrn = "urn:oid:1.3.132.0.34";
        private const string Prime521CurveUrn = "urn:oid:1.3.132.0.35";
        private const string PublicKeyRoot = "PublicKey";
        private const string UrnAttribute = "URN";
        private const string ValueAttribute = "Value";
        private const string XElement = "X";
        private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        private const string XsiNamespacePrefix = "xsi";
        private const string XsiTypeAttribute = "type";
        private const string XsiTypeAttributeValue = "PrimeFieldElemType";
        private const string YElement = "Y";

        internal static CngKey FromXml(string xml)
        {
            CngKey key;
            using (TextReader reader = new StringReader(xml))
            {
                using (XmlTextReader reader2 = new XmlTextReader(reader))
                {
                    BigInteger integer;
                    BigInteger integer2;
                    XPathNavigator navigator = new XPathDocument(reader2).CreateNavigator();
                    if (!navigator.MoveToFirstChild())
                    {
                        throw new ArgumentException(System.SR.GetString("Cryptography_MissingDomainParameters"));
                    }
                    CngAlgorithm algorithm = ReadAlgorithm(navigator);
                    if (!navigator.MoveToNext(XPathNodeType.Element))
                    {
                        throw new ArgumentException(System.SR.GetString("Cryptography_MissingPublicKey"));
                    }
                    ReadPublicKey(navigator, out integer, out integer2);
                    key = CngKey.Import(NCryptNative.BuildEccPublicBlob(algorithm.Algorithm, integer, integer2), CngKeyBlobFormat.EccPublicBlob);
                }
            }
            return key;
        }

        private static string GetCurveUrn(CngAlgorithm algorithm)
        {
            if ((algorithm == CngAlgorithm.ECDsaP256) || (algorithm == CngAlgorithm.ECDiffieHellmanP256))
            {
                return "urn:oid:1.2.840.10045.3.1.7";
            }
            if ((algorithm == CngAlgorithm.ECDsaP384) || (algorithm == CngAlgorithm.ECDiffieHellmanP384))
            {
                return "urn:oid:1.3.132.0.34";
            }
            if (!(algorithm == CngAlgorithm.ECDsaP521) && !(algorithm == CngAlgorithm.ECDiffieHellmanP521))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_UnknownEllipticCurve"), "algorithm");
            }
            return "urn:oid:1.3.132.0.35";
        }

        private static int GetKeySize(string urn)
        {
            switch (urn)
            {
                case "urn:oid:1.2.840.10045.3.1.7":
                    return 0x100;

                case "urn:oid:1.3.132.0.34":
                    return 0x180;

                case "urn:oid:1.3.132.0.35":
                    return 0x209;
            }
            throw new ArgumentException(System.SR.GetString("Cryptography_UnknownEllipticCurve"), "algorithm");
        }

        private static CngAlgorithm ReadAlgorithm(XPathNavigator navigator)
        {
            if (navigator.NamespaceURI != "http://www.w3.org/2001/04/xmldsig-more#")
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_UnexpectedXmlNamespace", new object[] { navigator.NamespaceURI, "http://www.w3.org/2001/04/xmldsig-more#" }));
            }
            bool flag = navigator.Name == "ECDHKeyValue";
            bool flag2 = navigator.Name == "ECDSAKeyValue";
            if (!flag && !flag2)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_UnknownEllipticCurveAlgorithm"));
            }
            if (!navigator.MoveToFirstChild() || (navigator.Name != "DomainParameters"))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_MissingDomainParameters"));
            }
            if (!navigator.MoveToFirstChild() || (navigator.Name != "NamedCurve"))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_MissingDomainParameters"));
            }
            if ((!navigator.MoveToFirstAttribute() || (navigator.Name != "URN")) || string.IsNullOrEmpty(navigator.Value))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_MissingDomainParameters"));
            }
            int keySize = GetKeySize(navigator.Value);
            navigator.MoveToParent();
            navigator.MoveToParent();
            if (flag)
            {
                switch (keySize)
                {
                    case 0x100:
                        return CngAlgorithm.ECDiffieHellmanP256;

                    case 0x180:
                        return CngAlgorithm.ECDiffieHellmanP384;
                }
                return CngAlgorithm.ECDiffieHellmanP521;
            }
            switch (keySize)
            {
                case 0x100:
                    return CngAlgorithm.ECDsaP256;

                case 0x180:
                    return CngAlgorithm.ECDsaP384;
            }
            return CngAlgorithm.ECDsaP521;
        }

        private static void ReadPublicKey(XPathNavigator navigator, out BigInteger x, out BigInteger y)
        {
            if (navigator.NamespaceURI != "http://www.w3.org/2001/04/xmldsig-more#")
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_UnexpectedXmlNamespace", new object[] { navigator.NamespaceURI, "http://www.w3.org/2001/04/xmldsig-more#" }));
            }
            if (navigator.Name != "PublicKey")
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_MissingPublicKey"));
            }
            if (!navigator.MoveToFirstChild() || (navigator.Name != "X"))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_MissingPublicKey"));
            }
            if ((!navigator.MoveToFirstAttribute() || (navigator.Name != "Value")) || string.IsNullOrEmpty(navigator.Value))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_MissingPublicKey"));
            }
            x = BigInteger.Parse(navigator.Value, CultureInfo.InvariantCulture);
            navigator.MoveToParent();
            if (!navigator.MoveToNext(XPathNodeType.Element) || (navigator.Name != "Y"))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_MissingPublicKey"));
            }
            if ((!navigator.MoveToFirstAttribute() || (navigator.Name != "Value")) || string.IsNullOrEmpty(navigator.Value))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_MissingPublicKey"));
            }
            y = BigInteger.Parse(navigator.Value, CultureInfo.InvariantCulture);
        }

        internal static string ToXml(CngKey key)
        {
            StringBuilder output = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                IndentChars = "  ",
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(output, settings))
            {
                string localName = (key.AlgorithmGroup == CngAlgorithmGroup.ECDsa) ? "ECDSAKeyValue" : "ECDHKeyValue";
                writer.WriteStartElement(localName, "http://www.w3.org/2001/04/xmldsig-more#");
                WriteDomainParameters(writer, key);
                WritePublicKeyValue(writer, key);
                writer.WriteEndElement();
            }
            return output.ToString();
        }

        private static void WriteDomainParameters(XmlWriter writer, CngKey key)
        {
            writer.WriteStartElement("DomainParameters");
            writer.WriteStartElement("NamedCurve");
            writer.WriteAttributeString("URN", GetCurveUrn(key.Algorithm));
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void WritePublicKeyValue(XmlWriter writer, CngKey key)
        {
            BigInteger integer;
            BigInteger integer2;
            writer.WriteStartElement("PublicKey");
            NCryptNative.UnpackEccPublicBlob(key.Export(CngKeyBlobFormat.EccPublicBlob), out integer, out integer2);
            writer.WriteStartElement("X");
            writer.WriteAttributeString("Value", integer.ToString("R", CultureInfo.InvariantCulture));
            writer.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", "PrimeFieldElemType");
            writer.WriteEndElement();
            writer.WriteStartElement("Y");
            writer.WriteAttributeString("Value", integer2.ToString("R", CultureInfo.InvariantCulture));
            writer.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", "PrimeFieldElemType");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}

