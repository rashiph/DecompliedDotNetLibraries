namespace System.Security.Cryptography
{
    using System;
    using System.Collections.Generic;
    using System.Deployment.Internal;
    using System.IO;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography.Xml;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class ManifestSignatureInformation
    {
        private AuthenticodeSignatureInformation m_authenticodeSignature;
        private ManifestKinds m_manifest;
        private StrongNameSignatureInformation m_strongNameSignature;

        internal ManifestSignatureInformation(ManifestKinds manifest, StrongNameSignatureInformation strongNameSignature, AuthenticodeSignatureInformation authenticodeSignature)
        {
            this.m_manifest = manifest;
            this.m_strongNameSignature = strongNameSignature;
            this.m_authenticodeSignature = authenticodeSignature;
        }

        [SecurityCritical]
        private static unsafe XmlDocument GetManifestXml(ActivationContext application, ManifestKinds manifest)
        {
            IStream applicationComponentManifest = null;
            if (manifest == ManifestKinds.Application)
            {
                applicationComponentManifest = InternalActivationContextHelper.GetApplicationComponentManifest(application) as IStream;
            }
            else if (manifest == ManifestKinds.Deployment)
            {
                applicationComponentManifest = InternalActivationContextHelper.GetDeploymentComponentManifest(application) as IStream;
            }
            using (MemoryStream stream2 = new MemoryStream())
            {
                byte[] pv = new byte[0x1000];
                int count = 0;
                do
                {
                    applicationComponentManifest.Read(pv, pv.Length, new IntPtr((void*) &count));
                    stream2.Write(pv, 0, count);
                }
                while (count == pv.Length);
                stream2.Position = 0L;
                XmlDocument document = new XmlDocument {
                    PreserveWhitespace = true
                };
                document.Load(stream2);
                return document;
            }
        }

        public static ManifestSignatureInformationCollection VerifySignature(ActivationContext application)
        {
            return VerifySignature(application, ManifestKinds.ApplicationAndDeployment);
        }

        public static ManifestSignatureInformationCollection VerifySignature(ActivationContext application, ManifestKinds manifests)
        {
            return VerifySignature(application, manifests, X509RevocationFlag.ExcludeRoot, X509RevocationMode.Online);
        }

        [SecurityCritical]
        public static ManifestSignatureInformationCollection VerifySignature(ActivationContext application, ManifestKinds manifests, X509RevocationFlag revocationFlag, X509RevocationMode revocationMode)
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }
            if ((revocationFlag < X509RevocationFlag.EndCertificateOnly) || (X509RevocationFlag.ExcludeRoot < revocationFlag))
            {
                throw new ArgumentOutOfRangeException("revocationFlag");
            }
            if ((revocationMode < X509RevocationMode.NoCheck) || (X509RevocationMode.Offline < revocationMode))
            {
                throw new ArgumentOutOfRangeException("revocationMode");
            }
            List<ManifestSignatureInformation> signatureInformation = new List<ManifestSignatureInformation>();
            if ((manifests & ManifestKinds.Deployment) == ManifestKinds.Deployment)
            {
                ManifestSignedXml xml = new ManifestSignedXml(GetManifestXml(application, ManifestKinds.Deployment), ManifestKinds.Deployment);
                signatureInformation.Add(xml.VerifySignature(revocationFlag, revocationMode));
            }
            if ((manifests & ManifestKinds.Application) == ManifestKinds.Application)
            {
                ManifestSignedXml xml2 = new ManifestSignedXml(GetManifestXml(application, ManifestKinds.Application), ManifestKinds.Application);
                signatureInformation.Add(xml2.VerifySignature(revocationFlag, revocationMode));
            }
            return new ManifestSignatureInformationCollection(signatureInformation);
        }

        public AuthenticodeSignatureInformation AuthenticodeSignature
        {
            get
            {
                return this.m_authenticodeSignature;
            }
        }

        public ManifestKinds Manifest
        {
            get
            {
                return this.m_manifest;
            }
        }

        public StrongNameSignatureInformation StrongNameSignature
        {
            get
            {
                return this.m_strongNameSignature;
            }
        }
    }
}

