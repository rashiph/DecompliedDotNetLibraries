namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    public class X509Chain
    {
        private X509ChainElementCollection m_chainElementCollection;
        private X509ChainPolicy m_chainPolicy;
        private X509ChainStatus[] m_chainStatus;
        private SafeCertChainHandle m_safeCertChainHandle;
        private uint m_status;
        private readonly object m_syncRoot;
        private bool m_useMachineContext;

        public X509Chain() : this(false)
        {
        }

        public X509Chain(bool useMachineContext)
        {
            this.m_syncRoot = new object();
            this.m_status = 0;
            this.m_chainPolicy = null;
            this.m_chainStatus = null;
            this.m_chainElementCollection = new X509ChainElementCollection();
            this.m_safeCertChainHandle = SafeCertChainHandle.InvalidHandle;
            this.m_useMachineContext = useMachineContext;
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public X509Chain(IntPtr chainContext)
        {
            this.m_syncRoot = new object();
            if (chainContext == IntPtr.Zero)
            {
                throw new ArgumentNullException("chainContext");
            }
            this.m_safeCertChainHandle = CAPISafe.CertDuplicateCertificateChain(chainContext);
            if ((this.m_safeCertChainHandle == null) || (this.m_safeCertChainHandle == SafeCertChainHandle.InvalidHandle))
            {
                throw new CryptographicException(SR.GetString("Cryptography_InvalidContextHandle"), "chainContext");
            }
            this.Init();
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true), PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
        public bool Build(X509Certificate2 certificate)
        {
            lock (this.m_syncRoot)
            {
                if ((certificate == null) || certificate.CertContext.IsInvalid)
                {
                    throw new ArgumentException(SR.GetString("Cryptography_InvalidContextHandle"), "certificate");
                }
                new StorePermission(StorePermissionFlags.EnumerateCertificates | StorePermissionFlags.OpenStore).Demand();
                X509ChainPolicy chainPolicy = this.ChainPolicy;
                if ((chainPolicy.RevocationMode == X509RevocationMode.Online) && ((certificate.Extensions["2.5.29.31"] != null) || (certificate.Extensions["1.3.6.1.5.5.7.1.1"] != null)))
                {
                    PermissionSet set = new PermissionSet(PermissionState.None);
                    set.AddPermission(new WebPermission(PermissionState.Unrestricted));
                    set.AddPermission(new StorePermission(StorePermissionFlags.AddToStore));
                    set.Demand();
                }
                this.Reset();
                if (BuildChain(this.m_useMachineContext ? new IntPtr(1L) : new IntPtr(0L), certificate.CertContext, chainPolicy.ExtraStore, chainPolicy.ApplicationPolicy, chainPolicy.CertificatePolicy, chainPolicy.RevocationMode, chainPolicy.RevocationFlag, chainPolicy.VerificationTime, chainPolicy.UrlRetrievalTimeout, ref this.m_safeCertChainHandle) != 0)
                {
                    return false;
                }
                this.Init();
                CAPIBase.CERT_CHAIN_POLICY_PARA pPolicyPara = new CAPIBase.CERT_CHAIN_POLICY_PARA(Marshal.SizeOf(typeof(CAPIBase.CERT_CHAIN_POLICY_PARA)));
                CAPIBase.CERT_CHAIN_POLICY_STATUS pPolicyStatus = new CAPIBase.CERT_CHAIN_POLICY_STATUS(Marshal.SizeOf(typeof(CAPIBase.CERT_CHAIN_POLICY_STATUS)));
                pPolicyPara.dwFlags = (uint) chainPolicy.VerificationFlags;
                if (!CAPISafe.CertVerifyCertificateChainPolicy(new IntPtr(1L), this.m_safeCertChainHandle, ref pPolicyPara, ref pPolicyStatus))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                CAPISafe.SetLastError(pPolicyStatus.dwError);
                return (pPolicyStatus.dwError == 0);
            }
        }

        internal static unsafe int BuildChain(IntPtr hChainEngine, System.Security.Cryptography.SafeCertContextHandle pCertContext, X509Certificate2Collection extraStore, OidCollection applicationPolicy, OidCollection certificatePolicy, X509RevocationMode revocationMode, X509RevocationFlag revocationFlag, DateTime verificationTime, TimeSpan timeout, ref SafeCertChainHandle ppChainContext)
        {
            CAPIBase.CERT_CHAIN_PARA cert_chain_para;
            if ((pCertContext == null) || pCertContext.IsInvalid)
            {
                throw new ArgumentException(SR.GetString("Cryptography_InvalidContextHandle"), "pCertContext");
            }
            System.Security.Cryptography.SafeCertStoreHandle invalidHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            if ((extraStore != null) && (extraStore.Count > 0))
            {
                invalidHandle = System.Security.Cryptography.X509Certificates.X509Utils.ExportToMemoryStore(extraStore);
            }
            cert_chain_para = new CAPIBase.CERT_CHAIN_PARA {
                cbSize = (uint) Marshal.SizeOf(cert_chain_para)
            };
            SafeLocalAllocHandle handle2 = SafeLocalAllocHandle.InvalidHandle;
            if ((applicationPolicy != null) && (applicationPolicy.Count > 0))
            {
                cert_chain_para.RequestedUsage.dwType = 0;
                cert_chain_para.RequestedUsage.Usage.cUsageIdentifier = (uint) applicationPolicy.Count;
                handle2 = System.Security.Cryptography.X509Certificates.X509Utils.CopyOidsToUnmanagedMemory(applicationPolicy);
                cert_chain_para.RequestedUsage.Usage.rgpszUsageIdentifier = handle2.DangerousGetHandle();
            }
            SafeLocalAllocHandle handle3 = SafeLocalAllocHandle.InvalidHandle;
            if ((certificatePolicy != null) && (certificatePolicy.Count > 0))
            {
                cert_chain_para.RequestedIssuancePolicy.dwType = 0;
                cert_chain_para.RequestedIssuancePolicy.Usage.cUsageIdentifier = (uint) certificatePolicy.Count;
                handle3 = System.Security.Cryptography.X509Certificates.X509Utils.CopyOidsToUnmanagedMemory(certificatePolicy);
                cert_chain_para.RequestedIssuancePolicy.Usage.rgpszUsageIdentifier = handle3.DangerousGetHandle();
            }
            cert_chain_para.dwUrlRetrievalTimeout = (uint) timeout.Milliseconds;
            System.Runtime.InteropServices.ComTypes.FILETIME pTime = new System.Runtime.InteropServices.ComTypes.FILETIME();
            *((long*) &pTime) = verificationTime.ToFileTime();
            uint dwFlags = System.Security.Cryptography.X509Certificates.X509Utils.MapRevocationFlags(revocationMode, revocationFlag);
            if (!CAPISafe.CertGetCertificateChain(hChainEngine, pCertContext, ref pTime, invalidHandle, ref cert_chain_para, dwFlags, IntPtr.Zero, ref ppChainContext))
            {
                return Marshal.GetHRForLastWin32Error();
            }
            handle2.Dispose();
            handle3.Dispose();
            return 0;
        }

        public static X509Chain Create()
        {
            return (X509Chain) CryptoConfig.CreateFromName("X509Chain");
        }

        internal static X509ChainStatus[] GetChainStatusInformation(uint dwStatus)
        {
            if (dwStatus == 0)
            {
                return new X509ChainStatus[0];
            }
            int num = 0;
            for (uint i = dwStatus; i != 0; i = i >> 1)
            {
                if ((i & 1) != 0)
                {
                    num++;
                }
            }
            X509ChainStatus[] statusArray = new X509ChainStatus[num];
            int index = 0;
            if ((dwStatus & 8) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146869244);
                statusArray[index].Status = X509ChainStatusFlags.NotSignatureValid;
                index++;
                dwStatus &= 0xfffffff7;
            }
            if ((dwStatus & 0x40000) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146869244);
                statusArray[index].Status = X509ChainStatusFlags.CtlNotSignatureValid;
                index++;
                dwStatus &= 0xfffbffff;
            }
            if ((dwStatus & 0x20) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762487);
                statusArray[index].Status = X509ChainStatusFlags.UntrustedRoot;
                index++;
                dwStatus &= 0xffffffdf;
            }
            if ((dwStatus & 0x10000) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762486);
                statusArray[index].Status = X509ChainStatusFlags.PartialChain;
                index++;
                dwStatus &= 0xfffeffff;
            }
            if ((dwStatus & 4) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146885616);
                statusArray[index].Status = X509ChainStatusFlags.Revoked;
                index++;
                dwStatus &= 0xfffffffb;
            }
            if ((dwStatus & 0x10) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762480);
                statusArray[index].Status = X509ChainStatusFlags.NotValidForUsage;
                index++;
                dwStatus &= 0xffffffef;
            }
            if ((dwStatus & 0x80000) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762480);
                statusArray[index].Status = X509ChainStatusFlags.CtlNotValidForUsage;
                index++;
                dwStatus &= 0xfff7ffff;
            }
            if ((dwStatus & 1) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762495);
                statusArray[index].Status = X509ChainStatusFlags.NotTimeValid;
                index++;
                dwStatus &= 0xfffffffe;
            }
            if ((dwStatus & 0x20000) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762495);
                statusArray[index].Status = X509ChainStatusFlags.CtlNotTimeValid;
                index++;
                dwStatus &= 0xfffdffff;
            }
            if ((dwStatus & 0x800) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762476);
                statusArray[index].Status = X509ChainStatusFlags.InvalidNameConstraints;
                index++;
                dwStatus &= 0xfffff7ff;
            }
            if ((dwStatus & 0x1000) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762476);
                statusArray[index].Status = X509ChainStatusFlags.HasNotSupportedNameConstraint;
                index++;
                dwStatus &= 0xffffefff;
            }
            if ((dwStatus & 0x2000) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762476);
                statusArray[index].Status = X509ChainStatusFlags.HasNotDefinedNameConstraint;
                index++;
                dwStatus &= 0xffffdfff;
            }
            if ((dwStatus & 0x4000) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762476);
                statusArray[index].Status = X509ChainStatusFlags.HasNotPermittedNameConstraint;
                index++;
                dwStatus &= 0xffffbfff;
            }
            if ((dwStatus & 0x8000) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762476);
                statusArray[index].Status = X509ChainStatusFlags.HasExcludedNameConstraint;
                index++;
                dwStatus &= 0xffff7fff;
            }
            if ((dwStatus & 0x200) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762477);
                statusArray[index].Status = X509ChainStatusFlags.InvalidPolicyConstraints;
                index++;
                dwStatus &= 0xfffffdff;
            }
            if ((dwStatus & 0x2000000) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762477);
                statusArray[index].Status = X509ChainStatusFlags.NoIssuanceChainPolicy;
                index++;
                dwStatus &= 0xfdffffff;
            }
            if ((dwStatus & 0x400) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146869223);
                statusArray[index].Status = X509ChainStatusFlags.InvalidBasicConstraints;
                index++;
                dwStatus &= 0xfffffbff;
            }
            if ((dwStatus & 2) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146762494);
                statusArray[index].Status = X509ChainStatusFlags.NotTimeNested;
                index++;
                dwStatus &= 0xfffffffd;
            }
            if ((dwStatus & 0x40) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146885614);
                statusArray[index].Status = X509ChainStatusFlags.RevocationStatusUnknown;
                index++;
                dwStatus &= 0xffffffbf;
            }
            if ((dwStatus & 0x1000000) != 0)
            {
                statusArray[index].StatusInformation = System.Security.Cryptography.X509Certificates.X509Utils.GetSystemErrorString(-2146885613);
                statusArray[index].Status = X509ChainStatusFlags.OfflineRevocation;
                index++;
                dwStatus &= 0xfeffffff;
            }
            int num4 = 0;
            for (uint j = dwStatus; j != 0; j = j >> 1)
            {
                if ((j & 1) != 0)
                {
                    statusArray[index].Status = (X509ChainStatusFlags) (((int) 1) << num4);
                    statusArray[index].StatusInformation = SR.GetString("Unknown_Error");
                    index++;
                }
                num4++;
            }
            return statusArray;
        }

        private unsafe void Init()
        {
            using (SafeCertChainHandle handle = CAPISafe.CertDuplicateCertificateChain(this.m_safeCertChainHandle))
            {
                CAPIBase.CERT_CHAIN_CONTEXT structure = new CAPIBase.CERT_CHAIN_CONTEXT(Marshal.SizeOf(typeof(CAPIBase.CERT_CHAIN_CONTEXT)));
                uint size = (uint) Marshal.ReadInt32(handle.DangerousGetHandle());
                if (size > Marshal.SizeOf(structure))
                {
                    size = (uint) Marshal.SizeOf(structure);
                }
                System.Security.Cryptography.X509Certificates.X509Utils.memcpy(this.m_safeCertChainHandle.DangerousGetHandle(), new IntPtr((void*) &structure), size);
                this.m_status = structure.dwErrorStatus;
                this.m_chainElementCollection = new X509ChainElementCollection(Marshal.ReadIntPtr(structure.rgpChain));
            }
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true), PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
        public void Reset()
        {
            this.m_status = 0;
            this.m_chainStatus = null;
            this.m_chainElementCollection = new X509ChainElementCollection();
            if (!this.m_safeCertChainHandle.IsInvalid)
            {
                this.m_safeCertChainHandle.Dispose();
                this.m_safeCertChainHandle = SafeCertChainHandle.InvalidHandle;
            }
        }

        public IntPtr ChainContext
        {
            [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return this.m_safeCertChainHandle.DangerousGetHandle();
            }
        }

        public X509ChainElementCollection ChainElements
        {
            get
            {
                return this.m_chainElementCollection;
            }
        }

        public X509ChainPolicy ChainPolicy
        {
            get
            {
                if (this.m_chainPolicy == null)
                {
                    this.m_chainPolicy = new X509ChainPolicy();
                }
                return this.m_chainPolicy;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_chainPolicy = value;
            }
        }

        public X509ChainStatus[] ChainStatus
        {
            get
            {
                if (this.m_chainStatus == null)
                {
                    if (this.m_status == 0)
                    {
                        this.m_chainStatus = new X509ChainStatus[0];
                    }
                    else
                    {
                        this.m_chainStatus = GetChainStatusInformation(this.m_status);
                    }
                }
                return this.m_chainStatus;
            }
        }
    }
}

