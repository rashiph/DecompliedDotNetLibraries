namespace System.Security.Policy
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    internal sealed class PEFileEvidenceFactory : IRuntimeEvidenceFactory
    {
        private List<EvidenceBase> m_assemblyProvidedEvidence;
        private bool m_generatedLocationEvidence;
        [SecurityCritical]
        private SafePEFileHandle m_peFile;
        private Site m_siteEvidence;
        private Url m_urlEvidence;
        private Zone m_zoneEvidence;

        [SecurityCritical]
        private PEFileEvidenceFactory(SafePEFileHandle peFile)
        {
            this.m_peFile = peFile;
        }

        [SecurityCritical]
        private static Evidence CreateSecurityIdentity(SafePEFileHandle peFile, Evidence hostProvidedEvidence)
        {
            PEFileEvidenceFactory target = new PEFileEvidenceFactory(peFile);
            Evidence evidence = new Evidence(target);
            if (hostProvidedEvidence != null)
            {
                evidence.MergeWithNoDuplicates(hostProvidedEvidence);
            }
            return evidence;
        }

        [SecuritySafeCritical]
        internal void FireEvidenceGeneratedEvent(EvidenceTypeGenerated type)
        {
            FireEvidenceGeneratedEvent(this.m_peFile, type);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void FireEvidenceGeneratedEvent(SafePEFileHandle peFile, EvidenceTypeGenerated type);
        public EvidenceBase GenerateEvidence(Type evidenceType)
        {
            if (evidenceType == typeof(Site))
            {
                return this.GenerateSiteEvidence();
            }
            if (evidenceType == typeof(Url))
            {
                return this.GenerateUrlEvidence();
            }
            if (evidenceType == typeof(Zone))
            {
                return this.GenerateZoneEvidence();
            }
            if (evidenceType == typeof(Publisher))
            {
                return this.GeneratePublisherEvidence();
            }
            return null;
        }

        [SecuritySafeCritical]
        private void GenerateLocationEvidence()
        {
            if (!this.m_generatedLocationEvidence)
            {
                SecurityZone noZone = SecurityZone.NoZone;
                string s = null;
                GetLocationEvidence(this.m_peFile, out noZone, JitHelpers.GetStringHandleOnStack(ref s));
                if (noZone != SecurityZone.NoZone)
                {
                    this.m_zoneEvidence = new Zone(noZone);
                }
                if (!string.IsNullOrEmpty(s))
                {
                    this.m_urlEvidence = new Url(s, true);
                    if (!s.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                    {
                        this.m_siteEvidence = Site.CreateFromUrl(s);
                    }
                }
                this.m_generatedLocationEvidence = true;
            }
        }

        [SecuritySafeCritical]
        private Publisher GeneratePublisherEvidence()
        {
            byte[] o = null;
            GetPublisherCertificate(this.m_peFile, JitHelpers.GetObjectHandleOnStack<byte[]>(ref o));
            if (o == null)
            {
                return null;
            }
            return new Publisher(new X509Certificate(o));
        }

        private Site GenerateSiteEvidence()
        {
            if (this.m_siteEvidence == null)
            {
                this.GenerateLocationEvidence();
            }
            return this.m_siteEvidence;
        }

        private Url GenerateUrlEvidence()
        {
            if (this.m_urlEvidence == null)
            {
                this.GenerateLocationEvidence();
            }
            return this.m_urlEvidence;
        }

        private Zone GenerateZoneEvidence()
        {
            if (this.m_zoneEvidence == null)
            {
                this.GenerateLocationEvidence();
            }
            return this.m_zoneEvidence;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetAssemblySuppliedEvidence(SafePEFileHandle peFile, ObjectHandleOnStack retSerializedEvidence);
        [SecuritySafeCritical]
        public IEnumerable<EvidenceBase> GetFactorySuppliedEvidence()
        {
            if (this.m_assemblyProvidedEvidence == null)
            {
                byte[] o = null;
                GetAssemblySuppliedEvidence(this.m_peFile, JitHelpers.GetObjectHandleOnStack<byte[]>(ref o));
                this.m_assemblyProvidedEvidence = new List<EvidenceBase>();
                if (o != null)
                {
                    Evidence evidence = new Evidence();
                    new SecurityPermission(SecurityPermissionFlag.SerializationFormatter).Assert();
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        using (MemoryStream stream = new MemoryStream(o))
                        {
                            evidence = (Evidence) formatter.Deserialize(stream);
                        }
                    }
                    catch
                    {
                    }
                    CodeAccessPermission.RevertAssert();
                    if (evidence != null)
                    {
                        IEnumerator assemblyEnumerator = evidence.GetAssemblyEnumerator();
                        while (assemblyEnumerator.MoveNext())
                        {
                            if (assemblyEnumerator.Current != null)
                            {
                                EvidenceBase current = assemblyEnumerator.Current as EvidenceBase;
                                if (current == null)
                                {
                                    current = new LegacyEvidenceWrapper(assemblyEnumerator.Current);
                                }
                                this.m_assemblyProvidedEvidence.Add(current);
                            }
                        }
                    }
                }
            }
            return this.m_assemblyProvidedEvidence;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetLocationEvidence(SafePEFileHandle peFile, out SecurityZone zone, StringHandleOnStack retUrl);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetPublisherCertificate(SafePEFileHandle peFile, ObjectHandleOnStack retCertificate);

        internal SafePEFileHandle PEFile
        {
            [SecurityCritical]
            get
            {
                return this.m_peFile;
            }
        }

        public IEvidenceFactory Target
        {
            get
            {
                return null;
            }
        }
    }
}

