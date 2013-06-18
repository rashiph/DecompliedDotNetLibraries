namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Diagnostics;

    internal sealed class SelfSignedCertificate : IDisposable
    {
        private CertificateHandle cert;
        private const int CERT_KEY_PROV_INFO_PROP_ID = 2;
        private const int CERT_KEY_SPEC_PROP_ID = 1;
        private const int CERT_STORE_PROV_MEMORY = 2;
        private const int DefaultLifeSpanInYears = 2;
        private byte[] exportedBytes;
        private KeyHandle key;
        private KeyContainerHandle keyContainer;
        private string keyContainerName;
        private string password;
        private X509Certificate2 x509Cert;

        private SelfSignedCertificate(string password, string containerName)
        {
            this.password = password;
            this.keyContainerName = containerName;
        }

        [DllImport("Crypt32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
        private static extern bool CertAddCertificateContextToStore(CertificateStoreHandle hCertStore, CertificateHandle pCertContext, AddDisposition dwAddDisposition, out StoreCertificateHandle ppStoreContext);
        [DllImport("Crypt32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
        private static extern CertificateHandle CertCreateSelfSignCertificate(KeyContainerHandle hProv, CryptoApiBlob.InteropHelper pSubjectIssuerBlob, SelfSignFlags dwFlags, IntPtr pKeyProvInfo, IntPtr pSignatureAlgorithm, [In] ref SystemTime pStartTime, [In] ref SystemTime pEndTime, IntPtr pExtensions);
        [DllImport("Crypt32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
        private static extern CertificateStoreHandle CertOpenStore(IntPtr lpszStoreProvider, int dwMsgAndCertEncodingType, IntPtr hCryptProv, int dwFlags, IntPtr pvPara);
        [DllImport("Crypt32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
        private static extern bool CertSetCertificateContextProperty(CertificateHandle context, int propId, int flags, KeyHandle pv);
        public static SelfSignedCertificate Create(string name, string password)
        {
            return Create(name, password, DateTime.UtcNow, DateTime.UtcNow.AddYears(2), Guid.NewGuid().ToString());
        }

        public static SelfSignedCertificate Create(string name, string password, DateTime start, DateTime expire, string containerName)
        {
            SelfSignedCertificate certificate = new SelfSignedCertificate(password, containerName);
            certificate.GenerateKeys();
            certificate.CreateCertContext(name, start, expire);
            certificate.GetX509Certificate();
            return certificate;
        }

        private void CreateCertContext(string name, DateTime start, DateTime expire)
        {
            CriticalAllocHandle providerInfo = this.GetProviderInfo();
            CriticalAllocHandle handle2 = GetSha1AlgorithmId();
            SystemTime pStartTime = new SystemTime(start);
            SystemTime pEndTime = new SystemTime(expire);
            CertificateName name2 = new CertificateName(name);
            using (CryptoApiBlob blob = name2.GetCryptoApiBlob())
            {
                using (providerInfo)
                {
                    using (handle2)
                    {
                        this.cert = CertCreateSelfSignCertificate(this.keyContainer, blob.GetMemoryForPinning(), SelfSignFlags.None, (IntPtr) providerInfo, (IntPtr) handle2, ref pStartTime, ref pEndTime, IntPtr.Zero);
                        if (this.cert.IsInvalid)
                        {
                            PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());
                        }
                        if (!CertSetCertificateContextProperty(this.cert, 1, 0, this.key))
                        {
                            PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());
                        }
                    }
                }
            }
        }

        [DllImport("Advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
        private static extern bool CryptAcquireContext(out KeyContainerHandle phProv, string pszContainer, string pszProvider, ProviderType dwProvType, ContextFlags dwFlags);
        [DllImport("Advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
        private static extern bool CryptGenKey(KeyContainerHandle hProv, AlgorithmType algId, KeyFlags dwFlags, out KeyHandle phKey);
        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.cert != null)
                {
                    this.cert.Dispose();
                }
                if (this.key != null)
                {
                    this.key.Dispose();
                }
                if (this.keyContainer != null)
                {
                    this.keyContainer.Dispose();
                }
                if (this.keyContainerName != null)
                {
                    CryptAcquireContext(out this.keyContainer, this.keyContainerName, null, ProviderType.RsaSecureChannel, ContextFlags.DeleteKeySet);
                    Utility.CloseInvalidOutSafeHandle(this.keyContainer);
                }
                GC.SuppressFinalize(this);
            }
        }

        private void Export()
        {
            using (CertificateStoreHandle handle = CertOpenStore(new IntPtr(2), 0, IntPtr.Zero, 0, IntPtr.Zero))
            {
                StoreCertificateHandle handle2;
                if (!CertAddCertificateContextToStore(handle, this.cert, AddDisposition.ReplaceExisting, out handle2))
                {
                    int error = Marshal.GetLastWin32Error();
                    Utility.CloseInvalidOutSafeHandle(handle2);
                    PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(new Win32Exception(error));
                }
                using (handle2)
                {
                    CryptoApiBlob blob = new CryptoApiBlob();
                    CryptoApiBlob.InteropHelper memoryForPinning = blob.GetMemoryForPinning();
                    GCHandle handle3 = GCHandle.Alloc(memoryForPinning, GCHandleType.Pinned);
                    try
                    {
                        if (!PFXExportCertStoreEx(handle, handle3.AddrOfPinnedObject(), this.password, IntPtr.Zero, PfxExportFlags.ExportPrivateKeys | PfxExportFlags.ReportNotAbleToExportPrivateKey | PfxExportFlags.ReportNoPrivateKey))
                        {
                            PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());
                        }
                        int size = memoryForPinning.size;
                        handle3.Free();
                        blob.AllocateBlob(size);
                        handle3 = GCHandle.Alloc(blob.GetMemoryForPinning(), GCHandleType.Pinned);
                        if (!PFXExportCertStoreEx(handle, handle3.AddrOfPinnedObject(), this.password, IntPtr.Zero, PfxExportFlags.ExportPrivateKeys | PfxExportFlags.ReportNotAbleToExportPrivateKey | PfxExportFlags.ReportNoPrivateKey))
                        {
                            PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());
                        }
                        this.exportedBytes = blob.GetBytes();
                    }
                    finally
                    {
                        handle3.Free();
                        if (blob != null)
                        {
                            blob.Dispose();
                        }
                    }
                }
            }
        }

        private void GenerateKeys()
        {
            if (!CryptAcquireContext(out this.keyContainer, this.keyContainerName, null, ProviderType.RsaSecureChannel, ContextFlags.NewKeySet | ContextFlags.Silent))
            {
                int error = Marshal.GetLastWin32Error();
                Utility.CloseInvalidOutSafeHandle(this.keyContainer);
                this.keyContainer = null;
                PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(new Win32Exception(error));
            }
            if (!CryptGenKey(this.keyContainer, AlgorithmType.KeyExchange, KeyFlags.Exportable2k, out this.key))
            {
                int num2 = Marshal.GetLastWin32Error();
                Utility.CloseInvalidOutSafeHandle(this.key);
                this.key = null;
                PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(new Win32Exception(num2));
            }
        }

        private CriticalAllocHandle GetProviderInfo()
        {
            CRYPT_KEY_PROV_INFO id = new CRYPT_KEY_PROV_INFO {
                container = this.keyContainerName,
                providerType = 12,
                paramsCount = 0,
                keySpec = 1
            };
            return CriticalAllocHandleBlob.FromBlob<CRYPT_KEY_PROV_INFO>(id);
        }

        private static CriticalAllocHandle GetSha1AlgorithmId()
        {
            Sha1AlgorithmId id = new Sha1AlgorithmId();
            return CriticalAllocHandleBlob.FromBlob<CRYPT_ALGORITHM_IDENTIFIER>(id);
        }

        public X509Certificate2 GetX509Certificate()
        {
            if (this.x509Cert == null)
            {
                this.Export();
                this.x509Cert = new X509Certificate2(this.exportedBytes, this.password);
            }
            return this.x509Cert;
        }

        [DllImport("Crypt32.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true)]
        private static extern bool PFXExportCertStoreEx(CertificateStoreHandle hStore, IntPtr pPFX, string password, IntPtr pvReserved, PfxExportFlags dwFlags);

        private enum AddDisposition
        {
            Always = 4,
            New = 1,
            ReplaceExisting = 3,
            ReplaceExistingInheritProperties = 5,
            UseExisting = 2
        }

        private enum AlgorithmType
        {
            KeyExchange = 1,
            Signature = 2
        }

        [Flags]
        private enum ContextFlags : uint
        {
            DeleteKeySet = 0x10,
            MachineKeySet = 0x20,
            NewKeySet = 8,
            Silent = 0x40,
            VerifyContext = 0xf0000000
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public class CRYPT_ALGORITHM_IDENTIFIER
        {
            public string pszObjId;
            public SelfSignedCertificate.CRYPT_OBJID_BLOB Parameters;
            public CRYPT_ALGORITHM_IDENTIFIER(string id)
            {
                this.pszObjId = id;
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public class CRYPT_KEY_PROV_INFO
        {
            public string container;
            public string provName;
            public int providerType;
            public int flags;
            public int paramsCount;
            public IntPtr param;
            public int keySpec;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_OBJID_BLOB
        {
            public int count;
            public IntPtr parameters;
        }

        private enum KeyFlags
        {
            Archivable = 0x4000,
            CreateIv = 0x200,
            CreateSalt = 4,
            DataKey = 0x800,
            Exportable = 1,
            Exportable2k = 0x8000001,
            KeyExchangeKey = 0x400,
            NoSalt = 0x10,
            Online = 0x80,
            PreGenerate = 0x40,
            Sf = 0x100,
            SgcKey = 0x2000,
            UpdateKey = 8,
            UserProtected = 2,
            Volatile = 0x1000
        }

        [Flags]
        private enum PfxExportFlags
        {
            ExportPrivateKeys = 4,
            ReportNoPrivateKey = 1,
            ReportNotAbleToExportPrivateKey = 2
        }

        private enum ProviderType
        {
            DiffieHellmanSecureChannel = 0x12,
            Dss = 3,
            DssDiffieHellman = 13,
            EcDsaFull = 0x10,
            EcDsaSignature = 14,
            EcNraFull = 0x11,
            EcNraSignature = 15,
            Fortezza = 4,
            IntelSec = 0x16,
            MsExchange = 5,
            RandomNumberGenerator = 0x15,
            ReplaceOwf = 0x17,
            RsaAes = 0x18,
            RsaFull = 1,
            RsaSecureChannel = 12,
            RsaSignature = 2,
            SpyrusLynks = 20,
            Ssl = 6
        }

        [Flags]
        private enum SelfSignFlags
        {
            None,
            NoSign,
            NoKeyInfo
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public class Sha1AlgorithmId : SelfSignedCertificate.CRYPT_ALGORITHM_IDENTIFIER
        {
            private const string AlgId = "1.2.840.113549.1.1.5";
            public Sha1AlgorithmId() : base("1.2.840.113549.1.1.5")
            {
            }
        }
    }
}

