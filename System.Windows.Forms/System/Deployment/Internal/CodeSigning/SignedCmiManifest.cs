namespace System.Deployment.Internal.CodeSigning
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography.Xml;
    using System.Text;
    using System.Xml;

    internal class SignedCmiManifest
    {
        private const string AssemblyNamespaceUri = "urn:schemas-microsoft-com:asm.v1";
        private const string AssemblyV2NamespaceUri = "urn:schemas-microsoft-com:asm.v2";
        private const string AuthenticodeNamespaceUri = "http://schemas.microsoft.com/windows/pki/2005/Authenticode";
        private static readonly char[] hexValues = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        private const string LicenseNamespaceUri = "urn:mpeg:mpeg21:2003:01-REL-R-NS";
        private const string licenseTemplate = "<r:license xmlns:r=\"urn:mpeg:mpeg21:2003:01-REL-R-NS\" xmlns:as=\"http://schemas.microsoft.com/windows/pki/2005/Authenticode\"><r:grant><as:ManifestInformation><as:assemblyIdentity /></as:ManifestInformation><as:SignedBy/><as:AuthenticodePublisher><as:X509SubjectName>CN=dummy</as:X509SubjectName></as:AuthenticodePublisher></r:grant><r:issuer></r:issuer></r:license>";
        private System.Deployment.Internal.CodeSigning.CmiAuthenticodeSignerInfo m_authenticodeSignerInfo;
        private XmlDocument m_manifestDom;
        private System.Deployment.Internal.CodeSigning.CmiStrongNameSignerInfo m_strongNameSignerInfo;
        private const string MSRelNamespaceUri = "http://schemas.microsoft.com/windows/rel/2005/reldata";

        private SignedCmiManifest()
        {
        }

        internal SignedCmiManifest(XmlDocument manifestDom)
        {
            if (manifestDom == null)
            {
                throw new ArgumentNullException("manifestDom");
            }
            this.m_manifestDom = manifestDom;
        }

        private static void AuthenticodeSignLicenseDom(XmlDocument licenseDom, System.Deployment.Internal.CodeSigning.CmiManifestSigner signer, string timeStampUrl)
        {
            if (signer.Certificate.PublicKey.Key.GetType() != typeof(RSACryptoServiceProvider))
            {
                throw new NotSupportedException();
            }
            System.Deployment.Internal.CodeSigning.ManifestSignedXml xml = new System.Deployment.Internal.CodeSigning.ManifestSignedXml(licenseDom) {
                SigningKey = signer.Certificate.PrivateKey
            };
            xml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
            xml.KeyInfo.AddClause(new RSAKeyValue(signer.Certificate.PublicKey.Key as RSA));
            xml.KeyInfo.AddClause(new KeyInfoX509Data(signer.Certificate, signer.IncludeOption));
            Reference reference = new Reference {
                Uri = ""
            };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());
            xml.AddReference(reference);
            xml.ComputeSignature();
            XmlElement node = xml.GetXml();
            node.SetAttribute("Id", "AuthenticodeSignature");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(licenseDom.NameTable);
            nsmgr.AddNamespace("r", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
            (licenseDom.SelectSingleNode("r:license/r:issuer", nsmgr) as XmlElement).AppendChild(licenseDom.ImportNode(node, true));
            if ((timeStampUrl != null) && (timeStampUrl.Length != 0))
            {
                TimestampSignedLicenseDom(licenseDom, timeStampUrl);
            }
            licenseDom.DocumentElement.ParentNode.InnerXml = "<msrel:RelData xmlns:msrel=\"http://schemas.microsoft.com/windows/rel/2005/reldata\">" + licenseDom.OuterXml + "</msrel:RelData>";
        }

        private static string BytesToHexString(byte[] array, int start, int end)
        {
            string str = null;
            if (array == null)
            {
                return str;
            }
            char[] chArray = new char[(end - start) * 2];
            int index = end;
            int num3 = 0;
            while (index-- > start)
            {
                int num2 = (array[index] & 240) >> 4;
                chArray[num3++] = hexValues[num2];
                num2 = array[index] & 15;
                chArray[num3++] = hexValues[num2];
            }
            return new string(chArray);
        }

        private static byte[] ComputeHashFromManifest(XmlDocument manifestDom)
        {
            return ComputeHashFromManifest(manifestDom, false);
        }

        private static byte[] ComputeHashFromManifest(XmlDocument manifestDom, bool oldFormat)
        {
            if (oldFormat)
            {
                XmlDsigExcC14NTransform transform = new XmlDsigExcC14NTransform();
                transform.LoadInput(manifestDom);
                using (SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider())
                {
                    byte[] buffer = provider.ComputeHash(transform.GetOutput() as MemoryStream);
                    if (buffer == null)
                    {
                        throw new CryptographicException(-2146869232);
                    }
                    return buffer;
                }
            }
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            using (TextReader reader = new StringReader(manifestDom.OuterXml))
            {
                XmlReaderSettings settings = new XmlReaderSettings {
                    DtdProcessing = DtdProcessing.Parse
                };
                XmlReader reader2 = XmlReader.Create(reader, settings, manifestDom.BaseURI);
                document.Load(reader2);
            }
            XmlDsigExcC14NTransform transform2 = new XmlDsigExcC14NTransform();
            transform2.LoadInput(document);
            using (SHA1CryptoServiceProvider provider2 = new SHA1CryptoServiceProvider())
            {
                byte[] buffer2 = provider2.ComputeHash(transform2.GetOutput() as MemoryStream);
                if (buffer2 == null)
                {
                    throw new CryptographicException(-2146869232);
                }
                return buffer2;
            }
        }

        private static XmlDocument CreateLicenseDom(System.Deployment.Internal.CodeSigning.CmiManifestSigner signer, XmlElement principal, byte[] hash)
        {
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            document.LoadXml("<r:license xmlns:r=\"urn:mpeg:mpeg21:2003:01-REL-R-NS\" xmlns:as=\"http://schemas.microsoft.com/windows/pki/2005/Authenticode\"><r:grant><as:ManifestInformation><as:assemblyIdentity /></as:ManifestInformation><as:SignedBy/><as:AuthenticodePublisher><as:X509SubjectName>CN=dummy</as:X509SubjectName></as:AuthenticodePublisher></r:grant><r:issuer></r:issuer></r:license>");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
            nsmgr.AddNamespace("r", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
            nsmgr.AddNamespace("as", "http://schemas.microsoft.com/windows/pki/2005/Authenticode");
            XmlElement element = document.SelectSingleNode("r:license/r:grant/as:ManifestInformation/as:assemblyIdentity", nsmgr) as XmlElement;
            element.RemoveAllAttributes();
            foreach (XmlAttribute attribute in principal.Attributes)
            {
                element.SetAttribute(attribute.Name, attribute.Value);
            }
            XmlElement element2 = document.SelectSingleNode("r:license/r:grant/as:ManifestInformation", nsmgr) as XmlElement;
            element2.SetAttribute("Hash", (hash.Length == 0) ? "" : BytesToHexString(hash, 0, hash.Length));
            element2.SetAttribute("Description", (signer.Description == null) ? "" : signer.Description);
            element2.SetAttribute("Url", (signer.DescriptionUrl == null) ? "" : signer.DescriptionUrl);
            XmlElement element3 = document.SelectSingleNode("r:license/r:grant/as:AuthenticodePublisher/as:X509SubjectName", nsmgr) as XmlElement;
            element3.InnerText = signer.Certificate.SubjectName.Name;
            return document;
        }

        private XmlElement ExtractPrincipalFromManifest()
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(this.m_manifestDom.NameTable);
            nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
            XmlNode node = this.m_manifestDom.SelectSingleNode("asm:assembly/asm:assemblyIdentity", nsmgr);
            if (node == null)
            {
                throw new CryptographicException(-2146762749);
            }
            return (node as XmlElement);
        }

        private static string GetPublicKeyToken(XmlDocument manifestDom)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(manifestDom.NameTable);
            nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement element = manifestDom.SelectSingleNode("asm:assembly/asm:assemblyIdentity", nsmgr) as XmlElement;
            if ((element == null) || !element.HasAttribute("publicKeyToken"))
            {
                throw new CryptographicException(-2146762749);
            }
            return element.GetAttribute("publicKeyToken");
        }

        private static byte[] HexStringToBytes(string hexString)
        {
            uint num = (uint) (hexString.Length / 2);
            byte[] buffer = new byte[num];
            int num2 = hexString.Length - 2;
            for (int i = 0; i < num; i++)
            {
                buffer[i] = (byte) ((HexToByte(hexString[num2]) << 4) | HexToByte(hexString[num2 + 1]));
                num2 -= 2;
            }
            return buffer;
        }

        private static byte HexToByte(char val)
        {
            if ((val <= '9') && (val >= '0'))
            {
                return (byte) (val - '0');
            }
            if ((val >= 'a') && (val <= 'f'))
            {
                return (byte) ((val - 'a') + 10);
            }
            if ((val >= 'A') && (val <= 'F'))
            {
                return (byte) ((val - 'A') + 10);
            }
            return 0xff;
        }

        private static void InsertPublisherIdentity(XmlDocument manifestDom, X509Certificate2 signerCert)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(manifestDom.NameTable);
            nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
            nsmgr.AddNamespace("asm2", "urn:schemas-microsoft-com:asm.v2");
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement element = manifestDom.SelectSingleNode("asm:assembly", nsmgr) as XmlElement;
            if (!(manifestDom.SelectSingleNode("asm:assembly/asm:assemblyIdentity", nsmgr) is XmlElement))
            {
                throw new CryptographicException(-2146762749);
            }
            XmlElement newChild = manifestDom.SelectSingleNode("asm:assembly/asm2:publisherIdentity", nsmgr) as XmlElement;
            if (newChild == null)
            {
                newChild = manifestDom.CreateElement("publisherIdentity", "urn:schemas-microsoft-com:asm.v2");
            }
            IntPtr ppwszPublicKeyHash = new IntPtr();
            int hr = System.Deployment.Internal.CodeSigning.Win32._AxlGetIssuerPublicKeyHash(signerCert.Handle, ref ppwszPublicKeyHash);
            if (hr != 0)
            {
                throw new CryptographicException(hr);
            }
            string str = Marshal.PtrToStringUni(ppwszPublicKeyHash);
            System.Deployment.Internal.CodeSigning.Win32.HeapFree(System.Deployment.Internal.CodeSigning.Win32.GetProcessHeap(), 0, ppwszPublicKeyHash);
            newChild.SetAttribute("name", signerCert.SubjectName.Name);
            newChild.SetAttribute("issuerKeyHash", str);
            XmlElement refChild = manifestDom.SelectSingleNode("asm:assembly/ds:Signature", nsmgr) as XmlElement;
            if (refChild != null)
            {
                element.InsertBefore(newChild, refChild);
            }
            else
            {
                element.AppendChild(newChild);
            }
        }

        private static void RemoveExistingSignature(XmlDocument manifestDom)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(manifestDom.NameTable);
            nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            XmlNode oldChild = manifestDom.SelectSingleNode("asm:assembly/ds:Signature", nsmgr);
            if (oldChild != null)
            {
                oldChild.ParentNode.RemoveChild(oldChild);
            }
        }

        private static unsafe void ReplacePublicKeyToken(XmlDocument manifestDom, AsymmetricAlgorithm snKey)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(manifestDom.NameTable);
            nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
            XmlElement element = manifestDom.SelectSingleNode("asm:assembly/asm:assemblyIdentity", nsmgr) as XmlElement;
            if (element == null)
            {
                throw new CryptographicException(-2146762749);
            }
            if (!element.HasAttribute("publicKeyToken"))
            {
                throw new CryptographicException(-2146762749);
            }
            byte[] buffer = ((RSACryptoServiceProvider) snKey).ExportCspBlob(false);
            if ((buffer == null) || (buffer.Length == 0))
            {
                throw new CryptographicException(-2146893821);
            }
            fixed (byte* numRef = buffer)
            {
                System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB pCspPublicKeyBlob = new System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB {
                    cbData = (uint) buffer.Length,
                    pbData = new IntPtr((void*) numRef)
                };
                IntPtr ppwszPublicKeyToken = new IntPtr();
                int hr = System.Deployment.Internal.CodeSigning.Win32._AxlPublicKeyBlobToPublicKeyToken(ref pCspPublicKeyBlob, ref ppwszPublicKeyToken);
                if (hr != 0)
                {
                    throw new CryptographicException(hr);
                }
                string str = Marshal.PtrToStringUni(ppwszPublicKeyToken);
                System.Deployment.Internal.CodeSigning.Win32.HeapFree(System.Deployment.Internal.CodeSigning.Win32.GetProcessHeap(), 0, ppwszPublicKeyToken);
                element.SetAttribute("publicKeyToken", str);
            }
        }

        internal void Sign(System.Deployment.Internal.CodeSigning.CmiManifestSigner signer)
        {
            this.Sign(signer, null);
        }

        internal void Sign(System.Deployment.Internal.CodeSigning.CmiManifestSigner signer, string timeStampUrl)
        {
            this.m_strongNameSignerInfo = null;
            this.m_authenticodeSignerInfo = null;
            if ((signer == null) || (signer.StrongNameKey == null))
            {
                throw new ArgumentNullException("signer");
            }
            RemoveExistingSignature(this.m_manifestDom);
            if ((signer.Flag & System.Deployment.Internal.CodeSigning.CmiManifestSignerFlag.DontReplacePublicKeyToken) == System.Deployment.Internal.CodeSigning.CmiManifestSignerFlag.None)
            {
                ReplacePublicKeyToken(this.m_manifestDom, signer.StrongNameKey);
            }
            XmlDocument licenseDom = null;
            if (signer.Certificate != null)
            {
                InsertPublisherIdentity(this.m_manifestDom, signer.Certificate);
                licenseDom = CreateLicenseDom(signer, this.ExtractPrincipalFromManifest(), ComputeHashFromManifest(this.m_manifestDom));
                AuthenticodeSignLicenseDom(licenseDom, signer, timeStampUrl);
            }
            StrongNameSignManifestDom(this.m_manifestDom, licenseDom, signer);
        }

        private static void StrongNameSignManifestDom(XmlDocument manifestDom, XmlDocument licenseDom, System.Deployment.Internal.CodeSigning.CmiManifestSigner signer)
        {
            RSA strongNameKey = signer.StrongNameKey as RSA;
            if (strongNameKey == null)
            {
                throw new NotSupportedException();
            }
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(manifestDom.NameTable);
            nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
            XmlElement elem = manifestDom.SelectSingleNode("asm:assembly", nsmgr) as XmlElement;
            if (elem == null)
            {
                throw new CryptographicException(-2146762749);
            }
            System.Deployment.Internal.CodeSigning.ManifestSignedXml xml = new System.Deployment.Internal.CodeSigning.ManifestSignedXml(elem) {
                SigningKey = signer.StrongNameKey
            };
            xml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
            xml.KeyInfo.AddClause(new RSAKeyValue(strongNameKey));
            if (licenseDom != null)
            {
                xml.KeyInfo.AddClause(new KeyInfoNode(licenseDom.DocumentElement));
            }
            xml.KeyInfo.Id = "StrongNameKeyInfo";
            Reference reference = new Reference {
                Uri = ""
            };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());
            xml.AddReference(reference);
            xml.ComputeSignature();
            XmlElement newChild = xml.GetXml();
            newChild.SetAttribute("Id", "StrongNameSignature");
            elem.AppendChild(newChild);
        }

        private static unsafe void TimestampSignedLicenseDom(XmlDocument licenseDom, string timeStampUrl)
        {
            System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB pTimestampSignatureBlob = new System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(licenseDom.NameTable);
            nsmgr.AddNamespace("r", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            nsmgr.AddNamespace("as", "http://schemas.microsoft.com/windows/pki/2005/Authenticode");
            byte[] bytes = Encoding.UTF8.GetBytes(licenseDom.OuterXml);
            fixed (byte* numRef = bytes)
            {
                System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB pSignedLicenseBlob = new System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB();
                IntPtr ptr = new IntPtr((void*) numRef);
                pSignedLicenseBlob.cbData = (uint) bytes.Length;
                pSignedLicenseBlob.pbData = ptr;
                int hr = System.Deployment.Internal.CodeSigning.Win32.CertTimestampAuthenticodeLicense(ref pSignedLicenseBlob, timeStampUrl, ref pTimestampSignatureBlob);
                if (hr != 0)
                {
                    throw new CryptographicException(hr);
                }
            }
            byte[] destination = new byte[pTimestampSignatureBlob.cbData];
            Marshal.Copy(pTimestampSignatureBlob.pbData, destination, 0, destination.Length);
            System.Deployment.Internal.CodeSigning.Win32.HeapFree(System.Deployment.Internal.CodeSigning.Win32.GetProcessHeap(), 0, pTimestampSignatureBlob.pbData);
            XmlElement newChild = licenseDom.CreateElement("as", "Timestamp", "http://schemas.microsoft.com/windows/pki/2005/Authenticode");
            newChild.InnerText = Encoding.UTF8.GetString(destination);
            XmlElement element2 = licenseDom.CreateElement("Object", "http://www.w3.org/2000/09/xmldsig#");
            element2.AppendChild(newChild);
            (licenseDom.SelectSingleNode("r:license/r:issuer/ds:Signature", nsmgr) as XmlElement).AppendChild(element2);
        }

        internal void Verify(System.Deployment.Internal.CodeSigning.CmiManifestVerifyFlags verifyFlags)
        {
            this.m_strongNameSignerInfo = null;
            this.m_authenticodeSignerInfo = null;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(this.m_manifestDom.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement element = this.m_manifestDom.SelectSingleNode("//ds:Signature", nsmgr) as XmlElement;
            if (element == null)
            {
                throw new CryptographicException(-2146762496);
            }
            string name = "Id";
            if (!element.HasAttribute(name))
            {
                name = "id";
                if (!element.HasAttribute(name))
                {
                    name = "ID";
                    if (!element.HasAttribute(name))
                    {
                        throw new CryptographicException(-2146762749);
                    }
                }
            }
            string attribute = element.GetAttribute(name);
            if ((attribute == null) || (string.Compare(attribute, "StrongNameSignature", StringComparison.Ordinal) != 0))
            {
                throw new CryptographicException(-2146762749);
            }
            bool oldFormat = false;
            bool flag2 = false;
            foreach (XmlNode node in element.SelectNodes("ds:SignedInfo/ds:Reference", nsmgr))
            {
                XmlElement element2 = node as XmlElement;
                if ((element2 != null) && element2.HasAttribute("URI"))
                {
                    string strA = element2.GetAttribute("URI");
                    if (strA != null)
                    {
                        if (strA.Length == 0)
                        {
                            XmlNode node2 = element2.SelectSingleNode("ds:Transforms", nsmgr);
                            if (node2 == null)
                            {
                                throw new CryptographicException(-2146762749);
                            }
                            XmlNodeList list2 = node2.SelectNodes("ds:Transform", nsmgr);
                            if (list2.Count < 2)
                            {
                                throw new CryptographicException(-2146762749);
                            }
                            bool flag3 = false;
                            bool flag4 = false;
                            for (int i = 0; i < list2.Count; i++)
                            {
                                string str4 = (list2[i] as XmlElement).GetAttribute("Algorithm");
                                if (str4 == null)
                                {
                                    break;
                                }
                                if (string.Compare(str4, "http://www.w3.org/2001/10/xml-exc-c14n#", StringComparison.Ordinal) != 0)
                                {
                                    flag3 = true;
                                    if (!flag4)
                                    {
                                        continue;
                                    }
                                    flag2 = true;
                                    break;
                                }
                                if (string.Compare(str4, "http://www.w3.org/2000/09/xmldsig#enveloped-signature", StringComparison.Ordinal) != 0)
                                {
                                    flag4 = true;
                                    if (flag3)
                                    {
                                        flag2 = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (string.Compare(strA, "#StrongNameKeyInfo", StringComparison.Ordinal) == 0)
                        {
                            oldFormat = true;
                            XmlNode node3 = node.SelectSingleNode("ds:Transforms", nsmgr);
                            if (node3 == null)
                            {
                                throw new CryptographicException(-2146762749);
                            }
                            XmlNodeList list3 = node3.SelectNodes("ds:Transform", nsmgr);
                            if (list3.Count < 1)
                            {
                                throw new CryptographicException(-2146762749);
                            }
                            for (int j = 0; j < list3.Count; j++)
                            {
                                string str5 = (list3[j] as XmlElement).GetAttribute("Algorithm");
                                if (str5 == null)
                                {
                                    break;
                                }
                                if (string.Compare(str5, "http://www.w3.org/2001/10/xml-exc-c14n#", StringComparison.Ordinal) != 0)
                                {
                                    flag2 = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!flag2)
            {
                throw new CryptographicException(-2146762749);
            }
            string publicKeyToken = this.VerifyPublicKeyToken();
            this.m_strongNameSignerInfo = new System.Deployment.Internal.CodeSigning.CmiStrongNameSignerInfo(-2146762485, publicKeyToken);
            System.Deployment.Internal.CodeSigning.ManifestSignedXml xml = new System.Deployment.Internal.CodeSigning.ManifestSignedXml(this.m_manifestDom, true);
            xml.LoadXml(element);
            AsymmetricAlgorithm signingKey = null;
            bool flag5 = xml.CheckSignatureReturningKey(out signingKey);
            this.m_strongNameSignerInfo.PublicKey = signingKey;
            if (!flag5)
            {
                this.m_strongNameSignerInfo.ErrorCode = -2146869232;
                throw new CryptographicException(-2146869232);
            }
            if ((verifyFlags & System.Deployment.Internal.CodeSigning.CmiManifestVerifyFlags.StrongNameOnly) != System.Deployment.Internal.CodeSigning.CmiManifestVerifyFlags.StrongNameOnly)
            {
                this.VerifyLicense(verifyFlags, oldFormat);
            }
        }

        private void VerifyAssemblyIdentity(XmlNamespaceManager nsm)
        {
            XmlElement element = this.m_manifestDom.SelectSingleNode("asm:assembly/asm:assemblyIdentity", nsm) as XmlElement;
            XmlElement element2 = this.m_manifestDom.SelectSingleNode("asm:assembly/ds:Signature/ds:KeyInfo/msrel:RelData/r:license/r:grant/as:ManifestInformation/as:assemblyIdentity", nsm) as XmlElement;
            if (((element == null) || (element2 == null)) || (!element.HasAttributes || !element2.HasAttributes))
            {
                throw new CryptographicException(-2146762749);
            }
            XmlAttributeCollection attributes = element.Attributes;
            if ((attributes.Count == 0) || (attributes.Count != element2.Attributes.Count))
            {
                throw new CryptographicException(-2146762749);
            }
            foreach (XmlAttribute attribute in attributes)
            {
                if (!element2.HasAttribute(attribute.LocalName) || (attribute.Value != element2.GetAttribute(attribute.LocalName)))
                {
                    throw new CryptographicException(-2146762749);
                }
            }
            this.VerifyHash(nsm);
        }

        private void VerifyHash(XmlNamespaceManager nsm)
        {
            XmlDocument manifestDom = new XmlDocument {
                PreserveWhitespace = true
            };
            manifestDom = (XmlDocument) this.m_manifestDom.Clone();
            XmlElement element = manifestDom.SelectSingleNode("asm:assembly/ds:Signature/ds:KeyInfo/msrel:RelData/r:license/r:grant/as:ManifestInformation", nsm) as XmlElement;
            if (element == null)
            {
                throw new CryptographicException(-2146762749);
            }
            if (!element.HasAttribute("Hash"))
            {
                throw new CryptographicException(-2146762749);
            }
            string attribute = element.GetAttribute("Hash");
            if ((attribute == null) || (attribute.Length == 0))
            {
                throw new CryptographicException(-2146762749);
            }
            XmlElement oldChild = manifestDom.SelectSingleNode("asm:assembly/ds:Signature", nsm) as XmlElement;
            if (oldChild == null)
            {
                throw new CryptographicException(-2146762749);
            }
            oldChild.ParentNode.RemoveChild(oldChild);
            byte[] buffer = HexStringToBytes(element.GetAttribute("Hash"));
            byte[] buffer2 = ComputeHashFromManifest(manifestDom);
            if ((buffer.Length == 0) || (buffer.Length != buffer2.Length))
            {
                byte[] buffer3 = ComputeHashFromManifest(manifestDom, true);
                if ((buffer.Length == 0) || (buffer.Length != buffer3.Length))
                {
                    throw new CryptographicException(-2146869232);
                }
                for (int j = 0; j < buffer.Length; j++)
                {
                    if (buffer[j] != buffer3[j])
                    {
                        throw new CryptographicException(-2146869232);
                    }
                }
            }
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] != buffer2[i])
                {
                    byte[] buffer4 = ComputeHashFromManifest(manifestDom, true);
                    if ((buffer.Length == 0) || (buffer.Length != buffer4.Length))
                    {
                        throw new CryptographicException(-2146869232);
                    }
                    for (i = 0; i < buffer.Length; i++)
                    {
                        if (buffer[i] != buffer4[i])
                        {
                            throw new CryptographicException(-2146869232);
                        }
                    }
                }
            }
        }

        private unsafe void VerifyLicense(System.Deployment.Internal.CodeSigning.CmiManifestVerifyFlags verifyFlags, bool oldFormat)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(this.m_manifestDom.NameTable);
            nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
            nsmgr.AddNamespace("asm2", "urn:schemas-microsoft-com:asm.v2");
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            nsmgr.AddNamespace("msrel", "http://schemas.microsoft.com/windows/rel/2005/reldata");
            nsmgr.AddNamespace("r", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
            nsmgr.AddNamespace("as", "http://schemas.microsoft.com/windows/pki/2005/Authenticode");
            XmlElement element = this.m_manifestDom.SelectSingleNode("asm:assembly/ds:Signature/ds:KeyInfo/msrel:RelData/r:license", nsmgr) as XmlElement;
            if (element != null)
            {
                this.VerifyAssemblyIdentity(nsmgr);
                this.m_authenticodeSignerInfo = new System.Deployment.Internal.CodeSigning.CmiAuthenticodeSignerInfo(-2146762485);
                byte[] bytes = Encoding.UTF8.GetBytes(element.OuterXml);
                fixed (byte* numRef = bytes)
                {
                    System.Deployment.Internal.CodeSigning.Win32.AXL_SIGNER_INFO pSignerInfo = new System.Deployment.Internal.CodeSigning.Win32.AXL_SIGNER_INFO {
                        cbSize = (uint) Marshal.SizeOf(typeof(System.Deployment.Internal.CodeSigning.Win32.AXL_SIGNER_INFO))
                    };
                    System.Deployment.Internal.CodeSigning.Win32.AXL_TIMESTAMPER_INFO pTimestamperInfo = new System.Deployment.Internal.CodeSigning.Win32.AXL_TIMESTAMPER_INFO {
                        cbSize = (uint) Marshal.SizeOf(typeof(System.Deployment.Internal.CodeSigning.Win32.AXL_TIMESTAMPER_INFO))
                    };
                    System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB pLicenseBlob = new System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB();
                    IntPtr ptr = new IntPtr((void*) numRef);
                    pLicenseBlob.cbData = (uint) bytes.Length;
                    pLicenseBlob.pbData = ptr;
                    int hr = System.Deployment.Internal.CodeSigning.Win32.CertVerifyAuthenticodeLicense(ref pLicenseBlob, (uint) verifyFlags, ref pSignerInfo, ref pTimestamperInfo);
                    if (0x800b0100 != pSignerInfo.dwError)
                    {
                        this.m_authenticodeSignerInfo = new System.Deployment.Internal.CodeSigning.CmiAuthenticodeSignerInfo(pSignerInfo, pTimestamperInfo);
                    }
                    System.Deployment.Internal.CodeSigning.Win32.CertFreeAuthenticodeSignerInfo(ref pSignerInfo);
                    System.Deployment.Internal.CodeSigning.Win32.CertFreeAuthenticodeTimestamperInfo(ref pTimestamperInfo);
                    if (hr != 0)
                    {
                        throw new CryptographicException(hr);
                    }
                }
                if (!oldFormat)
                {
                    this.VerifyPublisherIdentity(nsmgr);
                }
            }
        }

        private unsafe string VerifyPublicKeyToken()
        {
            byte[] buffer4;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(this.m_manifestDom.NameTable);
            nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement element = this.m_manifestDom.SelectSingleNode("asm:assembly/ds:Signature/ds:KeyInfo/ds:KeyValue/ds:RSAKeyValue/ds:Modulus", nsmgr) as XmlElement;
            XmlElement element2 = this.m_manifestDom.SelectSingleNode("asm:assembly/ds:Signature/ds:KeyInfo/ds:KeyValue/ds:RSAKeyValue/ds:Exponent", nsmgr) as XmlElement;
            if ((element == null) || (element2 == null))
            {
                throw new CryptographicException(-2146762749);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(element.InnerXml);
            byte[] buffer2 = Encoding.UTF8.GetBytes(element2.InnerXml);
            string publicKeyToken = GetPublicKeyToken(this.m_manifestDom);
            byte[] buffer3 = HexStringToBytes(publicKeyToken);
            fixed (byte* numRef = bytes)
            {
                fixed (byte* numRef2 = buffer2)
                {
                    System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB pModulusBlob = new System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB();
                    System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB pExponentBlob = new System.Deployment.Internal.CodeSigning.Win32.CRYPT_DATA_BLOB();
                    IntPtr ppwszPublicKeyToken = new IntPtr();
                    pModulusBlob.cbData = (uint) bytes.Length;
                    pModulusBlob.pbData = new IntPtr((void*) numRef);
                    pExponentBlob.cbData = (uint) buffer2.Length;
                    pExponentBlob.pbData = new IntPtr((void*) numRef2);
                    int hr = System.Deployment.Internal.CodeSigning.Win32._AxlRSAKeyValueToPublicKeyToken(ref pModulusBlob, ref pExponentBlob, ref ppwszPublicKeyToken);
                    if (hr != 0)
                    {
                        throw new CryptographicException(hr);
                    }
                    buffer4 = HexStringToBytes(Marshal.PtrToStringUni(ppwszPublicKeyToken));
                    System.Deployment.Internal.CodeSigning.Win32.HeapFree(System.Deployment.Internal.CodeSigning.Win32.GetProcessHeap(), 0, ppwszPublicKeyToken);
                }
            }
            if ((buffer3.Length == 0) || (buffer3.Length != buffer4.Length))
            {
                throw new CryptographicException(-2146762485);
            }
            for (int i = 0; i < buffer3.Length; i++)
            {
                if (buffer3[i] != buffer4[i])
                {
                    throw new CryptographicException(-2146762485);
                }
            }
            return publicKeyToken;
        }

        private void VerifyPublisherIdentity(XmlNamespaceManager nsm)
        {
            if (this.m_authenticodeSignerInfo.ErrorCode != -2146762496)
            {
                X509Certificate2 certificate = this.m_authenticodeSignerInfo.SignerChain.ChainElements[0].Certificate;
                XmlElement element = this.m_manifestDom.SelectSingleNode("asm:assembly/asm2:publisherIdentity", nsm) as XmlElement;
                if ((element == null) || !element.HasAttributes)
                {
                    throw new CryptographicException(-2146762749);
                }
                if (!element.HasAttribute("name") || !element.HasAttribute("issuerKeyHash"))
                {
                    throw new CryptographicException(-2146762749);
                }
                string attribute = element.GetAttribute("name");
                string strA = element.GetAttribute("issuerKeyHash");
                IntPtr ppwszPublicKeyHash = new IntPtr();
                int hr = System.Deployment.Internal.CodeSigning.Win32._AxlGetIssuerPublicKeyHash(certificate.Handle, ref ppwszPublicKeyHash);
                if (hr != 0)
                {
                    throw new CryptographicException(hr);
                }
                string strB = Marshal.PtrToStringUni(ppwszPublicKeyHash);
                System.Deployment.Internal.CodeSigning.Win32.HeapFree(System.Deployment.Internal.CodeSigning.Win32.GetProcessHeap(), 0, ppwszPublicKeyHash);
                if ((string.Compare(attribute, certificate.SubjectName.Name, StringComparison.Ordinal) != 0) || (string.Compare(strA, strB, StringComparison.Ordinal) != 0))
                {
                    throw new CryptographicException(-2146762485);
                }
            }
        }

        internal System.Deployment.Internal.CodeSigning.CmiAuthenticodeSignerInfo AuthenticodeSignerInfo
        {
            get
            {
                return this.m_authenticodeSignerInfo;
            }
        }

        internal System.Deployment.Internal.CodeSigning.CmiStrongNameSignerInfo StrongNameSignerInfo
        {
            get
            {
                return this.m_strongNameSignerInfo;
            }
        }
    }
}

