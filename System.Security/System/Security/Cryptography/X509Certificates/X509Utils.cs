namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;
    using System.Security.Permissions;
    using System.Text;

    internal class X509Utils
    {
        private static readonly char[] hexValues = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private X509Utils()
        {
        }

        [SecurityCritical]
        internal static unsafe int BuildChain(IntPtr hChainEngine, System.Security.Cryptography.SafeCertContextHandle pCertContext, X509Certificate2Collection extraStore, OidCollection applicationPolicy, OidCollection certificatePolicy, X509RevocationMode revocationMode, X509RevocationFlag revocationFlag, DateTime verificationTime, TimeSpan timeout, ref System.Security.Cryptography.SafeCertChainHandle ppChainContext)
        {
            System.Security.Cryptography.CAPI.CERT_CHAIN_PARA cert_chain_para;
            if ((pCertContext == null) || pCertContext.IsInvalid)
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_InvalidContextHandle"), "pCertContext");
            }
            System.Security.Cryptography.SafeCertStoreHandle invalidHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            if ((extraStore != null) && (extraStore.Count > 0))
            {
                invalidHandle = ExportToMemoryStore(extraStore);
            }
            cert_chain_para = new System.Security.Cryptography.CAPI.CERT_CHAIN_PARA {
                cbSize = (uint) Marshal.SizeOf(cert_chain_para)
            };
            System.Security.Cryptography.SafeLocalAllocHandle handle2 = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            if ((applicationPolicy != null) && (applicationPolicy.Count > 0))
            {
                cert_chain_para.RequestedUsage.dwType = 0;
                cert_chain_para.RequestedUsage.Usage.cUsageIdentifier = (uint) applicationPolicy.Count;
                handle2 = CopyOidsToUnmanagedMemory(applicationPolicy);
                cert_chain_para.RequestedUsage.Usage.rgpszUsageIdentifier = handle2.DangerousGetHandle();
            }
            System.Security.Cryptography.SafeLocalAllocHandle handle3 = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            if ((certificatePolicy != null) && (certificatePolicy.Count > 0))
            {
                cert_chain_para.RequestedIssuancePolicy.dwType = 0;
                cert_chain_para.RequestedIssuancePolicy.Usage.cUsageIdentifier = (uint) certificatePolicy.Count;
                handle3 = CopyOidsToUnmanagedMemory(certificatePolicy);
                cert_chain_para.RequestedIssuancePolicy.Usage.rgpszUsageIdentifier = handle3.DangerousGetHandle();
            }
            cert_chain_para.dwUrlRetrievalTimeout = (uint) timeout.Milliseconds;
            System.Runtime.InteropServices.ComTypes.FILETIME pTime = new System.Runtime.InteropServices.ComTypes.FILETIME();
            *((long*) &pTime) = verificationTime.ToFileTime();
            uint dwFlags = MapRevocationFlags(revocationMode, revocationFlag);
            if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateChain(hChainEngine, pCertContext, ref pTime, invalidHandle, ref cert_chain_para, dwFlags, IntPtr.Zero, ref ppChainContext))
            {
                return Marshal.GetHRForLastWin32Error();
            }
            handle2.Dispose();
            handle3.Dispose();
            return 0;
        }

        [SecurityCritical]
        internal static System.Security.Cryptography.SafeLocalAllocHandle CopyOidsToUnmanagedMemory(OidCollection oids)
        {
            System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            if ((oids != null) && (oids.Count != 0))
            {
                int num = oids.Count * Marshal.SizeOf(typeof(IntPtr));
                int num2 = 0;
                OidEnumerator enumerator = oids.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Oid current = enumerator.Current;
                    num2 += current.Value.Length + 1;
                }
                invalidHandle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) ((ulong) (num + num2))));
                IntPtr val = new IntPtr(((long) invalidHandle.DangerousGetHandle()) + num);
                for (int i = 0; i < oids.Count; i++)
                {
                    Marshal.WriteIntPtr(new IntPtr(((long) invalidHandle.DangerousGetHandle()) + (i * Marshal.SizeOf(typeof(IntPtr)))), val);
                    byte[] bytes = Encoding.ASCII.GetBytes(oids[i].Value);
                    Marshal.Copy(bytes, 0, val, bytes.Length);
                    val = new IntPtr((((long) val) + oids[i].Value.Length) + 1L);
                }
            }
            return invalidHandle;
        }

        internal static byte[] DecodeHexString(string s)
        {
            string str = System.Security.Cryptography.Xml.Utils.DiscardWhiteSpaces(s);
            uint num = (uint) (str.Length / 2);
            byte[] buffer = new byte[num];
            int num2 = 0;
            for (int i = 0; i < num; i++)
            {
                buffer[i] = (byte) ((HexToByte(str[num2]) << 4) | HexToByte(str[num2 + 1]));
                num2 += 2;
            }
            return buffer;
        }

        internal static string EncodeHexString(byte[] sArray)
        {
            return EncodeHexString(sArray, 0, (uint) sArray.Length);
        }

        internal static string EncodeHexString(byte[] sArray, uint start, uint end)
        {
            string str = null;
            if (sArray == null)
            {
                return str;
            }
            char[] chArray = new char[(end - start) * 2];
            uint index = start;
            uint num3 = 0;
            while (index < end)
            {
                uint num = (uint) ((sArray[index] & 240) >> 4);
                chArray[num3++] = hexValues[num];
                num = (uint) (sArray[index] & 15);
                chArray[num3++] = hexValues[num];
                index++;
            }
            return new string(chArray);
        }

        internal static string EncodeHexStringFromInt(byte[] sArray)
        {
            return EncodeHexStringFromInt(sArray, 0, (uint) sArray.Length);
        }

        internal static string EncodeHexStringFromInt(byte[] sArray, uint start, uint end)
        {
            string str = null;
            if (sArray == null)
            {
                return str;
            }
            char[] chArray = new char[(end - start) * 2];
            uint index = end;
            uint num3 = 0;
            while (index-- > start)
            {
                uint num2 = (uint) ((sArray[index] & 240) >> 4);
                chArray[num3++] = hexValues[num2];
                num2 = (uint) (sArray[index] & 15);
                chArray[num3++] = hexValues[num2];
            }
            return new string(chArray);
        }

        [SecurityCritical]
        internal static System.Security.Cryptography.SafeCertStoreHandle ExportToMemoryStore(X509Certificate2Collection collection)
        {
            new StorePermission(StorePermissionFlags.AllFlags).Assert();
            System.Security.Cryptography.SafeCertStoreHandle invalidHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            invalidHandle = System.Security.Cryptography.CAPI.CertOpenStore(new IntPtr(2L), 0x10001, IntPtr.Zero, 0x2200, null);
            if ((invalidHandle == null) || invalidHandle.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            X509Certificate2Enumerator enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Certificate2 current = enumerator.Current;
                if (!System.Security.Cryptography.CAPI.CertAddCertificateLinkToStore(invalidHandle, GetCertContext(current), 4, System.Security.Cryptography.SafeCertContextHandle.InvalidHandle))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            return invalidHandle;
        }

        [SecurityCritical]
        internal static System.Security.Cryptography.SafeCertContextHandle GetCertContext(X509Certificate2 certificate)
        {
            System.Security.Cryptography.SafeCertContextHandle handle = System.Security.Cryptography.CAPI.CertDuplicateCertificateContext(certificate.Handle);
            GC.KeepAlive(certificate);
            return handle;
        }

        [SecurityCritical]
        internal static X509Certificate2Collection GetCertificates(System.Security.Cryptography.SafeCertStoreHandle safeCertStoreHandle)
        {
            X509Certificate2Collection certificates = new X509Certificate2Collection();
            for (IntPtr ptr = System.Security.Cryptography.CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, IntPtr.Zero); ptr != IntPtr.Zero; ptr = System.Security.Cryptography.CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, ptr))
            {
                X509Certificate2 certificate = new X509Certificate2(ptr);
                certificates.Add(certificate);
            }
            return certificates;
        }

        [SecurityCritical]
        internal static bool GetPrivateKeyInfo(System.Security.Cryptography.SafeCertContextHandle safeCertContext, ref CspParameters parameters)
        {
            System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            uint pcbData = 0;
            if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateContextProperty(safeCertContext, 2, invalidHandle, ref pcbData))
            {
                if (Marshal.GetLastWin32Error() != -2146885628)
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                return false;
            }
            invalidHandle = System.Security.Cryptography.CAPI.LocalAlloc(0, new IntPtr((long) pcbData));
            if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateContextProperty(safeCertContext, 2, invalidHandle, ref pcbData))
            {
                if (Marshal.GetLastWin32Error() != -2146885628)
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                return false;
            }
            System.Security.Cryptography.CAPI.CRYPT_KEY_PROV_INFO crypt_key_prov_info = (System.Security.Cryptography.CAPI.CRYPT_KEY_PROV_INFO) Marshal.PtrToStructure(invalidHandle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CRYPT_KEY_PROV_INFO));
            parameters.ProviderName = crypt_key_prov_info.pwszProvName;
            parameters.KeyContainerName = crypt_key_prov_info.pwszContainerName;
            parameters.ProviderType = (int) crypt_key_prov_info.dwProvType;
            parameters.KeyNumber = (int) crypt_key_prov_info.dwKeySpec;
            parameters.Flags = ((crypt_key_prov_info.dwFlags & 0x20) == 0x20) ? CspProviderFlags.UseMachineKeyStore : CspProviderFlags.NoFlags;
            invalidHandle.Dispose();
            return true;
        }

        internal static byte HexToByte(char val)
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

        internal static bool IsSelfSigned(X509Chain chain)
        {
            X509ChainElementCollection chainElements = chain.ChainElements;
            if (chainElements.Count != 1)
            {
                return false;
            }
            X509Certificate2 certificate = chainElements[0].Certificate;
            return (string.Compare(certificate.SubjectName.Name, certificate.IssuerName.Name, StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal static uint MapRevocationFlags(X509RevocationMode revocationMode, X509RevocationFlag revocationFlag)
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

        [SecurityCritical]
        internal static unsafe bool MemEqual(byte* pbBuf1, uint cbBuf1, byte* pbBuf2, uint cbBuf2)
        {
            if (cbBuf1 == cbBuf2)
            {
                while (cbBuf1-- > 0)
                {
                    pbBuf1++;
                    pbBuf2++;
                    if (pbBuf1[0] != pbBuf2[0])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        [SecuritySafeCritical]
        internal static uint OidToAlgId(string value)
        {
            System.Security.Cryptography.SafeLocalAllocHandle pvKey = StringToAnsiPtr(value);
            return System.Security.Cryptography.CAPI.CryptFindOIDInfo(1, pvKey, 0).Algid;
        }

        [SecurityCritical]
        internal static System.Security.Cryptography.SafeLocalAllocHandle StringToAnsiPtr(string s)
        {
            byte[] bytes = new byte[s.Length + 1];
            Encoding.ASCII.GetBytes(s, 0, s.Length, bytes, 0);
            System.Security.Cryptography.SafeLocalAllocHandle handle = System.Security.Cryptography.CAPI.LocalAlloc(0, new IntPtr(bytes.Length));
            Marshal.Copy(bytes, 0, handle.DangerousGetHandle(), bytes.Length);
            return handle;
        }

        [SecurityCritical]
        internal static int VerifyCertificate(System.Security.Cryptography.SafeCertContextHandle pCertContext, OidCollection applicationPolicy, OidCollection certificatePolicy, X509RevocationMode revocationMode, X509RevocationFlag revocationFlag, DateTime verificationTime, TimeSpan timeout, X509Certificate2Collection extraStore, IntPtr pszPolicy, IntPtr pdwErrorStatus)
        {
            if ((pCertContext == null) || pCertContext.IsInvalid)
            {
                throw new ArgumentException("pCertContext");
            }
            System.Security.Cryptography.CAPI.CERT_CHAIN_POLICY_PARA pPolicyPara = new System.Security.Cryptography.CAPI.CERT_CHAIN_POLICY_PARA(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CERT_CHAIN_POLICY_PARA)));
            System.Security.Cryptography.CAPI.CERT_CHAIN_POLICY_STATUS pPolicyStatus = new System.Security.Cryptography.CAPI.CERT_CHAIN_POLICY_STATUS(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CERT_CHAIN_POLICY_STATUS)));
            System.Security.Cryptography.SafeCertChainHandle invalidHandle = System.Security.Cryptography.SafeCertChainHandle.InvalidHandle;
            int num = BuildChain(new IntPtr(0L), pCertContext, extraStore, applicationPolicy, certificatePolicy, revocationMode, revocationFlag, verificationTime, timeout, ref invalidHandle);
            if (num != 0)
            {
                return num;
            }
            if (!System.Security.Cryptography.CAPI.CAPISafe.CertVerifyCertificateChainPolicy(pszPolicy, invalidHandle, ref pPolicyPara, ref pPolicyStatus))
            {
                return Marshal.GetHRForLastWin32Error();
            }
            if (pdwErrorStatus != IntPtr.Zero)
            {
                pdwErrorStatus[0] = (IntPtr) pPolicyStatus.dwError;
            }
            if (pPolicyStatus.dwError == 0)
            {
                return 0;
            }
            return 1;
        }
    }
}

