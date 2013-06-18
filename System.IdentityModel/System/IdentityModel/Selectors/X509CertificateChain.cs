namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    internal class X509CertificateChain
    {
        private X509ChainPolicy chainPolicy;
        private uint chainPolicyOID;
        public const uint DefaultChainPolicyOID = 1;
        private bool useMachineContext;

        public X509CertificateChain() : this(false)
        {
        }

        public X509CertificateChain(bool useMachineContext)
        {
            this.chainPolicyOID = 1;
            this.useMachineContext = useMachineContext;
        }

        public X509CertificateChain(bool useMachineContext, uint chainPolicyOID)
        {
            this.chainPolicyOID = 1;
            this.useMachineContext = useMachineContext;
            this.chainPolicyOID = chainPolicyOID;
        }

        public bool Build(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            if (certificate.Handle == IntPtr.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("certificate", System.IdentityModel.SR.GetString("ArgumentInvalidCertificate"));
            }
            System.IdentityModel.SafeCertChainHandle invalidHandle = System.IdentityModel.SafeCertChainHandle.InvalidHandle;
            X509ChainPolicy chainPolicy = this.ChainPolicy;
            chainPolicy.VerificationTime = DateTime.Now;
            BuildChain(this.useMachineContext ? new IntPtr(1L) : new IntPtr(0L), certificate.Handle, chainPolicy.ExtraStore, chainPolicy.ApplicationPolicy, chainPolicy.CertificatePolicy, chainPolicy.RevocationMode, chainPolicy.RevocationFlag, chainPolicy.VerificationTime, chainPolicy.UrlRetrievalTimeout, out invalidHandle);
            System.IdentityModel.CAPI.CERT_CHAIN_POLICY_PARA pPolicyPara = new System.IdentityModel.CAPI.CERT_CHAIN_POLICY_PARA(Marshal.SizeOf(typeof(System.IdentityModel.CAPI.CERT_CHAIN_POLICY_PARA)));
            System.IdentityModel.CAPI.CERT_CHAIN_POLICY_STATUS pPolicyStatus = new System.IdentityModel.CAPI.CERT_CHAIN_POLICY_STATUS(Marshal.SizeOf(typeof(System.IdentityModel.CAPI.CERT_CHAIN_POLICY_STATUS)));
            pPolicyPara.dwFlags = (uint) (chainPolicy.VerificationFlags | 0x1000);
            if (!System.IdentityModel.CAPI.CertVerifyCertificateChainPolicy(new IntPtr((long) this.chainPolicyOID), invalidHandle, ref pPolicyPara, ref pPolicyStatus))
            {
                int hr = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(hr));
            }
            if (pPolicyStatus.dwError != 0)
            {
                int dwError = (int) pPolicyStatus.dwError;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(System.IdentityModel.SR.GetString("X509ChainBuildFail", new object[] { System.IdentityModel.SecurityUtils.GetCertificateId(certificate), new CryptographicException(dwError).Message })));
            }
            return true;
        }

        private static unsafe void BuildChain(IntPtr hChainEngine, IntPtr pCertContext, X509Certificate2Collection extraStore, OidCollection applicationPolicy, OidCollection certificatePolicy, X509RevocationMode revocationMode, X509RevocationFlag revocationFlag, DateTime verificationTime, TimeSpan timeout, out System.IdentityModel.SafeCertChainHandle ppChainContext)
        {
            System.IdentityModel.SafeCertStoreHandle hAdditionalStore = ExportToMemoryStore(extraStore, pCertContext);
            System.IdentityModel.CAPI.CERT_CHAIN_PARA pChainPara = new System.IdentityModel.CAPI.CERT_CHAIN_PARA {
                cbSize = (uint) Marshal.SizeOf(typeof(System.IdentityModel.CAPI.CERT_CHAIN_PARA))
            };
            SafeHGlobalHandle invalidHandle = SafeHGlobalHandle.InvalidHandle;
            SafeHGlobalHandle handle3 = SafeHGlobalHandle.InvalidHandle;
            try
            {
                if ((applicationPolicy != null) && (applicationPolicy.Count > 0))
                {
                    pChainPara.RequestedUsage.dwType = 0;
                    pChainPara.RequestedUsage.Usage.cUsageIdentifier = (uint) applicationPolicy.Count;
                    invalidHandle = CopyOidsToUnmanagedMemory(applicationPolicy);
                    pChainPara.RequestedUsage.Usage.rgpszUsageIdentifier = invalidHandle.DangerousGetHandle();
                }
                if ((certificatePolicy != null) && (certificatePolicy.Count > 0))
                {
                    pChainPara.RequestedIssuancePolicy.dwType = 0;
                    pChainPara.RequestedIssuancePolicy.Usage.cUsageIdentifier = (uint) certificatePolicy.Count;
                    handle3 = CopyOidsToUnmanagedMemory(certificatePolicy);
                    pChainPara.RequestedIssuancePolicy.Usage.rgpszUsageIdentifier = handle3.DangerousGetHandle();
                }
                pChainPara.dwUrlRetrievalTimeout = (uint) timeout.Milliseconds;
                System.Runtime.InteropServices.ComTypes.FILETIME pTime = new System.Runtime.InteropServices.ComTypes.FILETIME();
                *((long*) &pTime) = verificationTime.ToFileTime();
                uint dwFlags = MapRevocationFlags(revocationMode, revocationFlag);
                if (!System.IdentityModel.CAPI.CertGetCertificateChain(hChainEngine, pCertContext, ref pTime, hAdditionalStore, ref pChainPara, dwFlags, IntPtr.Zero, out ppChainContext))
                {
                    int hr = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(hr));
                }
            }
            finally
            {
                if (invalidHandle != null)
                {
                    invalidHandle.Dispose();
                }
                if (handle3 != null)
                {
                    handle3.Dispose();
                }
                hAdditionalStore.Close();
            }
        }

        private static SafeHGlobalHandle CopyOidsToUnmanagedMemory(OidCollection oids)
        {
            SafeHGlobalHandle invalidHandle = SafeHGlobalHandle.InvalidHandle;
            if ((oids != null) && (oids.Count != 0))
            {
                List<string> list = new List<string>();
                OidEnumerator enumerator = oids.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Oid current = enumerator.Current;
                    list.Add(current.Value);
                }
                IntPtr zero = IntPtr.Zero;
                IntPtr ptr = IntPtr.Zero;
                int num = list.Count * Marshal.SizeOf(typeof(IntPtr));
                int num2 = 0;
                foreach (string str in list)
                {
                    num2 += str.Length + 1;
                }
                invalidHandle = SafeHGlobalHandle.AllocHGlobal((int) (num + num2));
                zero = new IntPtr(((long) invalidHandle.DangerousGetHandle()) + num);
                for (int i = 0; i < list.Count; i++)
                {
                    Marshal.WriteIntPtr(new IntPtr(((long) invalidHandle.DangerousGetHandle()) + (i * Marshal.SizeOf(typeof(IntPtr)))), zero);
                    byte[] bytes = Encoding.ASCII.GetBytes(list[i]);
                    if (bytes.Length != list[i].Length)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                    }
                    Marshal.Copy(bytes, 0, zero, bytes.Length);
                    ptr = new IntPtr(((long) zero) + bytes.Length);
                    Marshal.WriteByte(ptr, 0);
                    zero = new IntPtr((((long) zero) + list[i].Length) + 1L);
                }
            }
            return invalidHandle;
        }

        private static System.IdentityModel.SafeCertStoreHandle ExportToMemoryStore(X509Certificate2Collection collection, IntPtr pCertContext)
        {
            System.IdentityModel.CAPI.CERT_CONTEXT cert_context = (System.IdentityModel.CAPI.CERT_CONTEXT) Marshal.PtrToStructure(pCertContext, typeof(System.IdentityModel.CAPI.CERT_CONTEXT));
            if (((collection == null) || (collection.Count <= 0)) && (cert_context.hCertStore == IntPtr.Zero))
            {
                return System.IdentityModel.SafeCertStoreHandle.InvalidHandle;
            }
            System.IdentityModel.SafeCertStoreHandle hCertStore = System.IdentityModel.CAPI.CertOpenStore(new IntPtr(2L), 0x10001, IntPtr.Zero, 0x2200, null);
            if ((hCertStore == null) || hCertStore.IsInvalid)
            {
                int hr = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(hr));
            }
            if ((collection != null) && (collection.Count > 0))
            {
                X509Certificate2Enumerator enumerator = collection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Certificate2 current = enumerator.Current;
                    if (!System.IdentityModel.CAPI.CertAddCertificateLinkToStore(hCertStore, current.Handle, 4, System.IdentityModel.SafeCertContextHandle.InvalidHandle))
                    {
                        int num2 = Marshal.GetLastWin32Error();
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(num2));
                    }
                }
            }
            if (cert_context.hCertStore != IntPtr.Zero)
            {
                X509Store store = new X509Store(cert_context.hCertStore);
                X509Certificate2Collection certificates = null;
                try
                {
                    certificates = store.Certificates;
                    X509Certificate2Enumerator enumerator2 = certificates.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        X509Certificate2 certificate2 = enumerator2.Current;
                        if (!System.IdentityModel.CAPI.CertAddCertificateLinkToStore(hCertStore, certificate2.Handle, 4, System.IdentityModel.SafeCertContextHandle.InvalidHandle))
                        {
                            int num3 = Marshal.GetLastWin32Error();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(num3));
                        }
                    }
                }
                finally
                {
                    System.IdentityModel.SecurityUtils.ResetAllCertificates(certificates);
                    store.Close();
                }
            }
            return hCertStore;
        }

        private static uint MapRevocationFlags(X509RevocationMode revocationMode, X509RevocationFlag revocationFlag)
        {
            uint num = 0;
            if (revocationMode == X509RevocationMode.NoCheck)
            {
                return num;
            }
            if (revocationMode == X509RevocationMode.Offline)
            {
                num |= 0x80000000;
            }
            if (revocationFlag == X509RevocationFlag.EndCertificateOnly)
            {
                return (num | 0x10000000);
            }
            if (revocationFlag == X509RevocationFlag.EntireChain)
            {
                return (num | 0x20000000);
            }
            return (num | 0x40000000);
        }

        public X509ChainPolicy ChainPolicy
        {
            get
            {
                if (this.chainPolicy == null)
                {
                    this.chainPolicy = new X509ChainPolicy();
                }
                return this.chainPolicy;
            }
            set
            {
                this.chainPolicy = value;
            }
        }

        public X509ChainStatus[] ChainStatus
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }
    }
}

