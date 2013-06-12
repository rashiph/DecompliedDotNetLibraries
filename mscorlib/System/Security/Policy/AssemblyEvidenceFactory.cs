namespace System.Security.Policy
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class AssemblyEvidenceFactory : IRuntimeEvidenceFactory
    {
        private PEFileEvidenceFactory m_peFileFactory;
        private RuntimeAssembly m_targetAssembly;

        private AssemblyEvidenceFactory(RuntimeAssembly targetAssembly, PEFileEvidenceFactory peFileFactory)
        {
            this.m_targetAssembly = targetAssembly;
            this.m_peFileFactory = peFileFactory;
        }

        public EvidenceBase GenerateEvidence(Type evidenceType)
        {
            EvidenceBase base2 = this.m_peFileFactory.GenerateEvidence(evidenceType);
            if (base2 != null)
            {
                return base2;
            }
            if (evidenceType == typeof(GacInstalled))
            {
                return this.GenerateGacEvidence();
            }
            if (evidenceType == typeof(Hash))
            {
                return this.GenerateHashEvidence();
            }
            if (evidenceType == typeof(PermissionRequestEvidence))
            {
                return this.GeneratePermissionRequestEvidence();
            }
            if (evidenceType == typeof(StrongName))
            {
                return this.GenerateStrongNameEvidence();
            }
            return null;
        }

        private GacInstalled GenerateGacEvidence()
        {
            if (!this.m_targetAssembly.GlobalAssemblyCache)
            {
                return null;
            }
            this.m_peFileFactory.FireEvidenceGeneratedEvent(EvidenceTypeGenerated.Gac);
            return new GacInstalled();
        }

        private Hash GenerateHashEvidence()
        {
            if (this.m_targetAssembly.IsDynamic)
            {
                return null;
            }
            this.m_peFileFactory.FireEvidenceGeneratedEvent(EvidenceTypeGenerated.Hash);
            return new Hash(this.m_targetAssembly);
        }

        [SecuritySafeCritical]
        private PermissionRequestEvidence GeneratePermissionRequestEvidence()
        {
            PermissionSet o = null;
            PermissionSet set2 = null;
            PermissionSet set3 = null;
            GetAssemblyPermissionRequests(this.m_targetAssembly.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<PermissionSet>(ref o), JitHelpers.GetObjectHandleOnStack<PermissionSet>(ref set2), JitHelpers.GetObjectHandleOnStack<PermissionSet>(ref set3));
            if (((o == null) && (set2 == null)) && (set3 == null))
            {
                return null;
            }
            return new PermissionRequestEvidence(o, set2, set3);
        }

        [SecuritySafeCritical]
        private StrongName GenerateStrongNameEvidence()
        {
            byte[] o = null;
            string s = null;
            ushort majorVersion = 0;
            ushort minorVersion = 0;
            ushort build = 0;
            ushort revision = 0;
            GetStrongNameInformation(this.m_targetAssembly.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<byte[]>(ref o), JitHelpers.GetStringHandleOnStack(ref s), out majorVersion, out minorVersion, out build, out revision);
            if ((o != null) && (o.Length != 0))
            {
                return new StrongName(new StrongNamePublicKeyBlob(o), s, new Version(majorVersion, minorVersion, build, revision), this.m_targetAssembly);
            }
            return null;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetAssemblyPermissionRequests(RuntimeAssembly assembly, ObjectHandleOnStack retMinimumPermissions, ObjectHandleOnStack retOptionalPermissions, ObjectHandleOnStack retRefusedPermissions);
        public IEnumerable<EvidenceBase> GetFactorySuppliedEvidence()
        {
            return this.m_peFileFactory.GetFactorySuppliedEvidence();
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetStrongNameInformation(RuntimeAssembly assembly, ObjectHandleOnStack retPublicKeyBlob, StringHandleOnStack retSimpleName, out ushort majorVersion, out ushort minorVersion, out ushort build, out ushort revision);
        [SecurityCritical]
        private static Evidence UpgradeSecurityIdentity(Evidence peFileEvidence, RuntimeAssembly targetAssembly)
        {
            peFileEvidence.Target = new AssemblyEvidenceFactory(targetAssembly, peFileEvidence.Target as PEFileEvidenceFactory);
            HostSecurityManager hostSecurityManager = AppDomain.CurrentDomain.HostSecurityManager;
            if ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostAssemblyEvidence) == HostSecurityManagerOptions.HostAssemblyEvidence)
            {
                peFileEvidence = hostSecurityManager.ProvideAssemblyEvidence(targetAssembly, peFileEvidence);
                if (peFileEvidence == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Policy_NullHostEvidence", new object[] { hostSecurityManager.GetType().FullName, targetAssembly.FullName }));
                }
            }
            return peFileEvidence;
        }

        internal SafePEFileHandle PEFile
        {
            [SecurityCritical]
            get
            {
                return this.m_peFileFactory.PEFile;
            }
        }

        public IEvidenceFactory Target
        {
            get
            {
                return this.m_targetAssembly;
            }
        }
    }
}

