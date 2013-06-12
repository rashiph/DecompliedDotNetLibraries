namespace System.Security.Cryptography.Xml
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Pkcs;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;

    internal sealed class ManifestSignedXml : SignedXml
    {
        private ManifestKinds m_manifest;
        private XmlDocument m_manifestXml;
        private XmlNamespaceManager m_namespaceManager;

        public ManifestSignedXml(XmlDocument manifestXml, ManifestKinds manifest) : base(manifestXml)
        {
            this.m_manifest = manifest;
            this.m_manifestXml = manifestXml;
            this.m_namespaceManager = new XmlNamespaceManager(manifestXml.NameTable);
            this.m_namespaceManager.AddNamespace("as", "http://schemas.microsoft.com/windows/pki/2005/Authenticode");
            this.m_namespaceManager.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
            this.m_namespaceManager.AddNamespace("asmv2", "urn:schemas-microsoft-com:asm.v2");
            this.m_namespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            this.m_namespaceManager.AddNamespace("msrel", "http://schemas.microsoft.com/windows/rel/2005/reldata");
            this.m_namespaceManager.AddNamespace("r", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
        }

        private static byte[] BackwardHexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex) || ((hex.Length % 2) != 0))
            {
                return null;
            }
            byte[] buffer = new byte[hex.Length / 2];
            int num = hex.Length - 2;
            for (int i = 0; i < buffer.Length; i++)
            {
                byte? nullable = HexToByte(hex[num]);
                byte? nullable2 = HexToByte(hex[num + 1]);
                if (!nullable.HasValue || !nullable2.HasValue)
                {
                    return null;
                }
                buffer[i] = (nullable.Value << 4) | nullable2.Value;
                num -= 2;
            }
            return buffer;
        }

        [SecurityCritical, StorePermission(SecurityAction.Assert, EnumerateCertificates=true, OpenStore=true)]
        private X509Chain BuildSignatureChain(X509Native.AXL_AUTHENTICODE_SIGNER_INFO signer, XmlElement licenseNode, X509RevocationFlag revocationFlag, X509RevocationMode revocationMode)
        {
            X509Chain chain = null;
            if (signer.dwError == -2146762487)
            {
                XmlElement element = licenseNode.SelectSingleNode("r:issuer/ds:Signature/ds:KeyInfo/ds:X509Data", this.m_namespaceManager) as XmlElement;
                if (element != null)
                {
                    X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(element.InnerText.Trim()));
                    chain = new X509Chain {
                        ChainPolicy = { RevocationFlag = revocationFlag, RevocationMode = revocationMode }
                    };
                    chain.Build(certificate);
                }
                return chain;
            }
            if (signer.pChainContext != IntPtr.Zero)
            {
                chain = new X509Chain(signer.pChainContext);
            }
            return chain;
        }

        private byte[] CalculateManifestPublicKeyToken()
        {
            XmlElement element = this.m_manifestXml.SelectSingleNode("//asm:assembly/asm:assemblyIdentity", this.m_namespaceManager) as XmlElement;
            if (element == null)
            {
                return null;
            }
            return HexStringToBytes(element.GetAttribute("publicKeyToken"));
        }

        [SecurityCritical]
        private static unsafe byte[] CalculateSignerPublicKeyToken(AsymmetricAlgorithm key)
        {
            SafeAxlBufferHandle handle;
            byte[] buffer2;
            ICspAsymmetricAlgorithm algorithm = key as ICspAsymmetricAlgorithm;
            if (algorithm == null)
            {
                return null;
            }
            byte[] buffer = algorithm.ExportCspBlob(false);
            fixed (byte* numRef = buffer)
            {
                CapiNative.CRYPTOAPI_BLOB pCspPublicKeyBlob = new CapiNative.CRYPTOAPI_BLOB {
                    cbData = buffer.Length,
                    pbData = new IntPtr((void*) numRef)
                };
                if ((CapiNative.UnsafeNativeMethods._AxlPublicKeyBlobToPublicKeyToken(ref pCspPublicKeyBlob, out handle) & -2147483648) != 0)
                {
                    return null;
                }
            }
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                handle.DangerousAddRef(ref success);
                buffer2 = HexStringToBytes(Marshal.PtrToStringUni(handle.DangerousGetHandle()));
            }
            finally
            {
                if (success)
                {
                    handle.DangerousRelease();
                }
            }
            return buffer2;
        }

        private static bool CompareBytes(byte[] lhs, byte[] rhs)
        {
            if ((lhs == null) || (rhs == null))
            {
                return false;
            }
            for (int i = 0; i < lhs.Length; i++)
            {
                if (lhs[i] != rhs[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            if ((base.KeyInfo != null) && (string.Compare(base.KeyInfo.Id, idValue, StringComparison.OrdinalIgnoreCase) == 0))
            {
                return base.KeyInfo.GetXml();
            }
            return null;
        }

        [SecurityCritical]
        private TimestampInformation GetTimestampInformation(X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO timestamper, XmlElement licenseNode)
        {
            TimestampInformation information = null;
            if (timestamper.dwError == 0)
            {
                return new TimestampInformation(timestamper);
            }
            if ((timestamper.dwError == -2146762748) || (timestamper.dwError == -2146762496))
            {
                XmlElement element = licenseNode.SelectSingleNode("r:issuer/ds:Signature/ds:Object/as:Timestamp", this.m_namespaceManager) as XmlElement;
                if (element == null)
                {
                    return information;
                }
                byte[] encodedMessage = Convert.FromBase64String(element.InnerText);
                try
                {
                    SignedCms cms = new SignedCms();
                    cms.Decode(encodedMessage);
                    cms.CheckSignature(true);
                    return null;
                }
                catch (CryptographicException exception)
                {
                    return new TimestampInformation((SignatureVerificationResult) Marshal.GetHRForException(exception));
                }
            }
            return null;
        }

        private static byte[] HexStringToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex) || ((hex.Length % 2) != 0))
            {
                return null;
            }
            byte[] buffer = new byte[hex.Length / 2];
            for (int i = 0; i < buffer.Length; i++)
            {
                byte? nullable = HexToByte(hex[i]);
                byte? nullable2 = HexToByte(hex[i + 1]);
                if (!nullable.HasValue || !nullable2.HasValue)
                {
                    return null;
                }
                buffer[i] = (nullable.Value << 4) | nullable2.Value;
            }
            return buffer;
        }

        private static byte? HexToByte(char hex)
        {
            if ((hex >= '0') && (hex <= '9'))
            {
                return new byte?((byte) (hex - '0'));
            }
            if ((hex >= 'a') && (hex <= 'f'))
            {
                return new byte?((byte) ((hex - 'a') + 10));
            }
            if ((hex >= 'A') && (hex <= 'F'))
            {
                return new byte?((byte) ((hex - 'A') + 10));
            }
            return null;
        }

        private static X509Native.AxlVerificationFlags MapRevocationFlags(X509RevocationFlag revocationFlag, X509RevocationMode revocationMode)
        {
            X509Native.AxlVerificationFlags none = X509Native.AxlVerificationFlags.None;
            switch (revocationFlag)
            {
                case X509RevocationFlag.EndCertificateOnly:
                    none |= X509Native.AxlVerificationFlags.RevocationCheckEndCertOnly;
                    break;

                case X509RevocationFlag.EntireChain:
                    none |= X509Native.AxlVerificationFlags.RevocationCheckEntireChain;
                    break;

                default:
                    break;
            }
            switch (revocationMode)
            {
                case X509RevocationMode.NoCheck:
                    return (none | X509Native.AxlVerificationFlags.NoRevocationCheck);

                case X509RevocationMode.Online:
                    return none;

                case X509RevocationMode.Offline:
                    return (none | X509Native.AxlVerificationFlags.UrlOnlyCacheRetrieval);
            }
            return none;
        }

        private SignatureVerificationResult VerifyAuthenticodeExpectedHash(XmlElement licenseNode)
        {
            XmlElement element = licenseNode.SelectSingleNode("r:grant/as:ManifestInformation", this.m_namespaceManager) as XmlElement;
            if (element == null)
            {
                return SignatureVerificationResult.BadSignatureFormat;
            }
            string attribute = element.GetAttribute("Hash");
            if (string.IsNullOrEmpty(attribute))
            {
                return SignatureVerificationResult.BadSignatureFormat;
            }
            byte[] lhs = BackwardHexToBytes(attribute);
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            XmlReaderSettings settings = new XmlReaderSettings {
                DtdProcessing = DtdProcessing.Parse
            };
            using (TextReader reader = new StringReader(this.m_manifestXml.OuterXml))
            {
                using (XmlReader reader2 = XmlReader.Create(reader, settings, this.m_manifestXml.BaseURI))
                {
                    document.Load(reader2);
                }
            }
            XmlElement oldChild = document.SelectSingleNode("//asm:assembly/ds:Signature", this.m_namespaceManager) as XmlElement;
            oldChild.ParentNode.RemoveChild(oldChild);
            XmlDsigExcC14NTransform transform = new XmlDsigExcC14NTransform();
            transform.LoadInput(document);
            byte[] rhs = null;
            using (SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider())
            {
                rhs = provider.ComputeHash(transform.GetOutput() as MemoryStream);
            }
            if (!CompareBytes(lhs, rhs))
            {
                return SignatureVerificationResult.BadDigest;
            }
            return SignatureVerificationResult.Valid;
        }

        [SecurityCritical]
        private SignatureVerificationResult VerifyAuthenticodePublisher(X509Certificate2 publisherCertificate)
        {
            XmlElement element = this.m_manifestXml.SelectSingleNode("//asm:assembly/asmv2:publisherIdentity", this.m_namespaceManager) as XmlElement;
            if (element == null)
            {
                return SignatureVerificationResult.BadSignatureFormat;
            }
            string attribute = element.GetAttribute("name");
            string str2 = element.GetAttribute("issuerKeyHash");
            if (string.IsNullOrEmpty(attribute) || string.IsNullOrEmpty(str2))
            {
                return SignatureVerificationResult.BadSignatureFormat;
            }
            SafeAxlBufferHandle ppwszPublicKeyHash = null;
            int num = X509Native.UnsafeNativeMethods._AxlGetIssuerPublicKeyHash(publisherCertificate.Handle, out ppwszPublicKeyHash);
            if (num != 0)
            {
                return (SignatureVerificationResult) num;
            }
            string strB = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                ppwszPublicKeyHash.DangerousAddRef(ref success);
                strB = Marshal.PtrToStringUni(ppwszPublicKeyHash.DangerousGetHandle());
            }
            finally
            {
                if (success)
                {
                    ppwszPublicKeyHash.DangerousRelease();
                }
            }
            if ((string.Compare(attribute, publisherCertificate.SubjectName.Name, StringComparison.Ordinal) == 0) && (string.Compare(str2, strB, StringComparison.Ordinal) == 0))
            {
                return SignatureVerificationResult.Valid;
            }
            return SignatureVerificationResult.PublisherMismatch;
        }

        [SecurityCritical]
        private unsafe AuthenticodeSignatureInformation VerifyAuthenticodeSignature(XmlElement signatureNode, X509RevocationFlag revocationFlag, X509RevocationMode revocationMode)
        {
            XmlElement licenseNode = signatureNode.SelectSingleNode("ds:KeyInfo/msrel:RelData/r:license", this.m_namespaceManager) as XmlElement;
            if (licenseNode == null)
            {
                return null;
            }
            SignatureVerificationResult error = this.VerifyAuthenticodeSignatureIdentity(licenseNode);
            if (error != SignatureVerificationResult.Valid)
            {
                return new AuthenticodeSignatureInformation(error);
            }
            SignatureVerificationResult result2 = this.VerifyAuthenticodeExpectedHash(licenseNode);
            if (result2 != SignatureVerificationResult.Valid)
            {
                return new AuthenticodeSignatureInformation(result2);
            }
            AuthenticodeSignatureInformation information = null;
            X509Native.AXL_AUTHENTICODE_SIGNER_INFO pSignerInfo = new X509Native.AXL_AUTHENTICODE_SIGNER_INFO {
                cbSize = Marshal.SizeOf(typeof(X509Native.AXL_AUTHENTICODE_SIGNER_INFO))
            };
            X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO pTimestamperInfo = new X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO {
                cbsize = Marshal.SizeOf(typeof(X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO))
            };
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(licenseNode.OuterXml);
                X509Native.AxlVerificationFlags dwFlags = MapRevocationFlags(revocationFlag, revocationMode);
                try
                {
                    fixed (byte* numRef = bytes)
                    {
                        CapiNative.CRYPTOAPI_BLOB pLicenseBlob = new CapiNative.CRYPTOAPI_BLOB {
                            cbData = bytes.Length,
                            pbData = new IntPtr((void*) numRef)
                        };
                        if (X509Native.UnsafeNativeMethods.CertVerifyAuthenticodeLicense(ref pLicenseBlob, dwFlags, ref pSignerInfo, ref pTimestamperInfo) == -2146762496)
                        {
                            return new AuthenticodeSignatureInformation(SignatureVerificationResult.MissingSignature);
                        }
                    }
                }
                finally
                {
                    numRef = null;
                }
                X509Chain signatureChain = this.BuildSignatureChain(pSignerInfo, licenseNode, revocationFlag, revocationMode);
                TimestampInformation timestampInformation = this.GetTimestampInformation(pTimestamperInfo, licenseNode);
                information = new AuthenticodeSignatureInformation(pSignerInfo, signatureChain, timestampInformation);
            }
            finally
            {
                X509Native.UnsafeNativeMethods.CertFreeAuthenticodeSignerInfo(ref pSignerInfo);
                X509Native.UnsafeNativeMethods.CertFreeAuthenticodeTimestamperInfo(ref pTimestamperInfo);
            }
            if (information.SigningCertificate == null)
            {
                return new AuthenticodeSignatureInformation(information.VerificationResult);
            }
            SignatureVerificationResult result3 = this.VerifyAuthenticodePublisher(information.SigningCertificate);
            if (result3 != SignatureVerificationResult.Valid)
            {
                return new AuthenticodeSignatureInformation(result3);
            }
            return information;
        }

        private SignatureVerificationResult VerifyAuthenticodeSignatureIdentity(XmlElement licenseNode)
        {
            XmlElement element = licenseNode.SelectSingleNode("r:grant/as:ManifestInformation/as:assemblyIdentity", this.m_namespaceManager) as XmlElement;
            XmlElement element2 = this.m_manifestXml.SelectSingleNode("//asm:assembly/asm:assemblyIdentity", this.m_namespaceManager) as XmlElement;
            bool flag = (element2 != null) && element2.HasAttributes;
            bool flag2 = (element != null) && element.HasAttributes;
            if ((!flag || !flag2) || (element2.Attributes.Count != element.Attributes.Count))
            {
                return SignatureVerificationResult.BadSignatureFormat;
            }
            foreach (XmlAttribute attribute in element2.Attributes)
            {
                string strB = element.GetAttribute(attribute.LocalName);
                if ((strB == null) || (string.Compare(attribute.Value, strB, StringComparison.Ordinal) != 0))
                {
                    return SignatureVerificationResult.AssemblyIdentityMismatch;
                }
            }
            return SignatureVerificationResult.Valid;
        }

        [SecurityCritical]
        public ManifestSignatureInformation VerifySignature(X509RevocationFlag revocationFlag, X509RevocationMode revocationMode)
        {
            XmlElement element = this.m_manifestXml.SelectSingleNode("//ds:Signature", this.m_namespaceManager) as XmlElement;
            if (element == null)
            {
                return new ManifestSignatureInformation(this.m_manifest, null, null);
            }
            base.LoadXml(element);
            StrongNameSignatureInformation strongNameSignature = this.VerifyStrongNameSignature(element);
            AuthenticodeSignatureInformation authenticodeSignature = null;
            if (strongNameSignature.VerificationResult != SignatureVerificationResult.BadDigest)
            {
                authenticodeSignature = this.VerifyAuthenticodeSignature(element, revocationFlag, revocationMode);
            }
            else
            {
                authenticodeSignature = new AuthenticodeSignatureInformation(SignatureVerificationResult.ContainingSignatureInvalid);
            }
            return new ManifestSignatureInformation(this.m_manifest, strongNameSignature, authenticodeSignature);
        }

        [SecurityCritical]
        private StrongNameSignatureInformation VerifyStrongNameSignature(XmlElement signatureNode)
        {
            AsymmetricAlgorithm algorithm;
            if (!base.CheckSignatureReturningKey(out algorithm))
            {
                return new StrongNameSignatureInformation(SignatureVerificationResult.BadDigest);
            }
            SignatureVerificationResult error = VerifyStrongNameSignatureId(signatureNode);
            if (error != SignatureVerificationResult.Valid)
            {
                return new StrongNameSignatureInformation(error);
            }
            SignatureVerificationResult result2 = VerifyStrongNameSignatureTransforms(base.Signature.SignedInfo);
            if (result2 != SignatureVerificationResult.Valid)
            {
                return new StrongNameSignatureInformation(result2);
            }
            if (!CompareBytes(this.CalculateManifestPublicKeyToken(), CalculateSignerPublicKeyToken(algorithm)))
            {
                return new StrongNameSignatureInformation(SignatureVerificationResult.PublicKeyTokenMismatch);
            }
            return new StrongNameSignatureInformation(algorithm);
        }

        private static SignatureVerificationResult VerifyStrongNameSignatureId(XmlElement signatureNode)
        {
            string str = null;
            for (int i = 0; (i < signatureNode.Attributes.Count) && (str == null); i++)
            {
                if (string.Compare(signatureNode.Attributes[i].LocalName, "id", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    str = signatureNode.Attributes[i].Value;
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                return SignatureVerificationResult.BadSignatureFormat;
            }
            if (string.Compare(str, "StrongNameSignature", StringComparison.Ordinal) != 0)
            {
                return SignatureVerificationResult.BadSignatureFormat;
            }
            return SignatureVerificationResult.Valid;
        }

        private static SignatureVerificationResult VerifyStrongNameSignatureTransforms(SignedInfo signedInfo)
        {
            int num = 0;
            foreach (Reference reference in signedInfo.References)
            {
                TransformChain transformChain = reference.TransformChain;
                bool flag = false;
                if (string.IsNullOrEmpty(reference.Uri))
                {
                    num++;
                    flag = (((transformChain != null) && (transformChain.Count == 2)) && (string.Compare(transformChain[0].Algorithm, "http://www.w3.org/2000/09/xmldsig#enveloped-signature", StringComparison.Ordinal) == 0)) && (string.Compare(transformChain[1].Algorithm, "http://www.w3.org/2001/10/xml-exc-c14n#", StringComparison.Ordinal) == 0);
                }
                else if (string.Compare(reference.Uri, "#StrongNameKeyInfo", StringComparison.Ordinal) == 0)
                {
                    num++;
                    flag = ((transformChain != null) && (transformChain.Count == 1)) && (string.Compare(transformChain[0].Algorithm, "http://www.w3.org/2001/10/xml-exc-c14n#", StringComparison.Ordinal) == 0);
                }
                else
                {
                    flag = true;
                }
                if (!flag)
                {
                    return SignatureVerificationResult.BadSignatureFormat;
                }
            }
            if (num == 0)
            {
                return SignatureVerificationResult.BadSignatureFormat;
            }
            return SignatureVerificationResult.Valid;
        }
    }
}

