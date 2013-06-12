namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;

    internal class X509Utils
    {
        private static readonly char[] hexValues = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private X509Utils()
        {
        }

        internal static uint AlignedLength(uint length)
        {
            return ((length + 7) & 0xfffffff8);
        }

        internal static SafeLocalAllocHandle ByteToPtr(byte[] managed)
        {
            SafeLocalAllocHandle handle = CAPI.LocalAlloc(0, new IntPtr(managed.Length));
            Marshal.Copy(managed, 0, handle.DangerousGetHandle(), managed.Length);
            return handle;
        }

        internal static SafeLocalAllocHandle CopyOidsToUnmanagedMemory(OidCollection oids)
        {
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
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
                int num = list.Count * Marshal.SizeOf(typeof(IntPtr));
                int num2 = 0;
                foreach (string str in list)
                {
                    num2 += str.Length + 1;
                }
                invalidHandle = CAPI.LocalAlloc(0x40, new IntPtr((long) (((uint) num) + ((uint) num2))));
                zero = new IntPtr(((long) invalidHandle.DangerousGetHandle()) + num);
                for (int i = 0; i < list.Count; i++)
                {
                    Marshal.WriteIntPtr(new IntPtr(((long) invalidHandle.DangerousGetHandle()) + (i * Marshal.SizeOf(typeof(IntPtr)))), zero);
                    byte[] bytes = Encoding.ASCII.GetBytes(list[i]);
                    Marshal.Copy(bytes, 0, zero, bytes.Length);
                    zero = new IntPtr((((long) zero) + list[i].Length) + 1L);
                }
            }
            return invalidHandle;
        }

        internal static byte[] DecodeHexString(string s)
        {
            string str = DiscardWhiteSpaces(s);
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

        internal static string DiscardWhiteSpaces(string inputBuffer)
        {
            return DiscardWhiteSpaces(inputBuffer, 0, inputBuffer.Length);
        }

        internal static string DiscardWhiteSpaces(string inputBuffer, int inputOffset, int inputCount)
        {
            int num;
            int num2 = 0;
            for (num = 0; num < inputCount; num++)
            {
                if (char.IsWhiteSpace(inputBuffer[inputOffset + num]))
                {
                    num2++;
                }
            }
            char[] chArray = new char[inputCount - num2];
            num2 = 0;
            for (num = 0; num < inputCount; num++)
            {
                if (!char.IsWhiteSpace(inputBuffer[inputOffset + num]))
                {
                    chArray[num2++] = inputBuffer[inputOffset + num];
                }
            }
            return new string(chArray);
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

        internal static System.Security.Cryptography.SafeCertStoreHandle ExportToMemoryStore(X509Certificate2Collection collection)
        {
            new StorePermission(StorePermissionFlags.AllFlags).Assert();
            System.Security.Cryptography.SafeCertStoreHandle invalidHandle = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
            invalidHandle = CAPI.CertOpenStore(new IntPtr(2L), 0x10001, IntPtr.Zero, 0x2200, null);
            if ((invalidHandle == null) || invalidHandle.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            X509Certificate2Enumerator enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Certificate2 current = enumerator.Current;
                if (!CAPI.CertAddCertificateLinkToStore(invalidHandle, current.CertContext, 4, System.Security.Cryptography.SafeCertContextHandle.InvalidHandle))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            return invalidHandle;
        }

        internal static string FindOidInfo(uint keyType, string keyValue, System.Security.Cryptography.OidGroup oidGroup)
        {
            if (keyValue == null)
            {
                throw new ArgumentNullException("keyValue");
            }
            if (keyValue.Length == 0)
            {
                return null;
            }
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            switch (keyType)
            {
                case 1:
                    invalidHandle = StringToAnsiPtr(keyValue);
                    break;

                case 2:
                    invalidHandle = StringToUniPtr(keyValue);
                    break;
            }
            CAPIBase.CRYPT_OID_INFO crypt_oid_info = CAPI.CryptFindOIDInfo(keyType, invalidHandle, oidGroup);
            if ((crypt_oid_info.pszOID == null) && (oidGroup != System.Security.Cryptography.OidGroup.AllGroups))
            {
                crypt_oid_info = CAPI.CryptFindOIDInfo(keyType, invalidHandle, System.Security.Cryptography.OidGroup.AllGroups);
            }
            if (keyType == 1)
            {
                return crypt_oid_info.pwszName;
            }
            return crypt_oid_info.pszOID;
        }

        internal static X509Certificate2Collection GetCertificates(System.Security.Cryptography.SafeCertStoreHandle safeCertStoreHandle)
        {
            X509Certificate2Collection certificates = new X509Certificate2Collection();
            for (IntPtr ptr = CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, IntPtr.Zero); ptr != IntPtr.Zero; ptr = CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, ptr))
            {
                X509Certificate2 certificate = new X509Certificate2(ptr);
                certificates.Add(certificate);
            }
            return certificates;
        }

        internal static int GetHexArraySize(byte[] hex)
        {
            int length = hex.Length;
            while (length-- > 0)
            {
                if (hex[length] != 0)
                {
                    break;
                }
            }
            return (length + 1);
        }

        internal static string GetSystemErrorString(int hr)
        {
            StringBuilder lpBuffer = new StringBuilder(0x200);
            if (CAPISafe.FormatMessage(0x1200, IntPtr.Zero, (uint) hr, 0, lpBuffer, (uint) lpBuffer.Capacity, IntPtr.Zero) != 0)
            {
                return lpBuffer.ToString();
            }
            return SR.GetString("Unknown_Error");
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

        internal static bool IsCertRdnCharString(uint dwValueType)
        {
            return ((dwValueType & 0xff) >= 3);
        }

        internal static X509ContentType MapContentType(uint contentType)
        {
            switch (contentType)
            {
                case 1:
                    return X509ContentType.Cert;

                case 4:
                    return X509ContentType.SerializedStore;

                case 5:
                    return X509ContentType.SerializedCert;

                case 8:
                case 9:
                    return X509ContentType.Pkcs7;

                case 10:
                    return X509ContentType.Authenticode;

                case 12:
                    return X509ContentType.Pfx;
            }
            return X509ContentType.Unknown;
        }

        internal static uint MapKeyStorageFlags(X509KeyStorageFlags keyStorageFlags)
        {
            uint num = 0;
            if ((keyStorageFlags & X509KeyStorageFlags.UserKeySet) == X509KeyStorageFlags.UserKeySet)
            {
                num |= 0x1000;
            }
            else if ((keyStorageFlags & X509KeyStorageFlags.MachineKeySet) == X509KeyStorageFlags.MachineKeySet)
            {
                num |= 0x20;
            }
            if ((keyStorageFlags & X509KeyStorageFlags.Exportable) == X509KeyStorageFlags.Exportable)
            {
                num |= 1;
            }
            if ((keyStorageFlags & X509KeyStorageFlags.UserProtected) == X509KeyStorageFlags.UserProtected)
            {
                num |= 2;
            }
            return num;
        }

        internal static uint MapNameType(X509NameType nameType)
        {
            switch (nameType)
            {
                case X509NameType.SimpleName:
                    return 4;

                case X509NameType.EmailName:
                    return 1;

                case X509NameType.UpnName:
                    return 8;

                case X509NameType.DnsName:
                case X509NameType.DnsFromAlternativeName:
                    return 6;

                case X509NameType.UrlName:
                    return 7;
            }
            throw new ArgumentException(SR.GetString("Argument_InvalidNameType"));
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

        internal static uint MapX509StoreFlags(StoreLocation storeLocation, OpenFlags flags)
        {
            uint num = 0;
            switch (((uint) (flags & (OpenFlags.MaxAllowed | OpenFlags.ReadWrite))))
            {
                case 0:
                    num |= 0x8000;
                    break;

                case 2:
                    num |= 0x1000;
                    break;
            }
            if ((flags & OpenFlags.OpenExistingOnly) == OpenFlags.OpenExistingOnly)
            {
                num |= 0x4000;
            }
            if ((flags & OpenFlags.IncludeArchived) == OpenFlags.IncludeArchived)
            {
                num |= 0x200;
            }
            if (storeLocation == StoreLocation.LocalMachine)
            {
                return (num | 0x20000);
            }
            if (storeLocation == StoreLocation.CurrentUser)
            {
                num |= 0x10000;
            }
            return num;
        }

        internal static void memcpy(IntPtr source, IntPtr dest, uint size)
        {
            for (uint i = 0; i < size; i++)
            {
                ((long) dest)[i] = Marshal.ReadByte(new IntPtr(((long) source) + i));
            }
        }

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

        internal static uint OidToAlgId(string value)
        {
            SafeLocalAllocHandle pvKey = StringToAnsiPtr(value);
            return CAPI.CryptFindOIDInfo(1, pvKey, System.Security.Cryptography.OidGroup.AllGroups).Algid;
        }

        internal static byte[] PtrToByte(IntPtr unmanaged, uint size)
        {
            byte[] destination = new byte[size];
            Marshal.Copy(unmanaged, destination, 0, destination.Length);
            return destination;
        }

        internal static SafeLocalAllocHandle StringToAnsiPtr(string s)
        {
            byte[] bytes = new byte[s.Length + 1];
            Encoding.ASCII.GetBytes(s, 0, s.Length, bytes, 0);
            SafeLocalAllocHandle handle = CAPI.LocalAlloc(0, new IntPtr(bytes.Length));
            Marshal.Copy(bytes, 0, handle.DangerousGetHandle(), bytes.Length);
            return handle;
        }

        internal static SafeLocalAllocHandle StringToUniPtr(string s)
        {
            byte[] bytes = new byte[2 * (s.Length + 1)];
            Encoding.Unicode.GetBytes(s, 0, s.Length, bytes, 0);
            SafeLocalAllocHandle handle = CAPI.LocalAlloc(0, new IntPtr(bytes.Length));
            Marshal.Copy(bytes, 0, handle.DangerousGetHandle(), bytes.Length);
            return handle;
        }

        internal static void ValidateOidValue(string keyValue)
        {
            if (keyValue == null)
            {
                throw new ArgumentNullException("keyValue");
            }
            int length = keyValue.Length;
            if (length >= 2)
            {
                char ch = keyValue[0];
                if ((((ch == '0') || (ch == '1')) || (ch == '2')) && ((keyValue[1] == '.') && (keyValue[length - 1] != '.')))
                {
                    bool flag = false;
                    for (int i = 1; i < length; i++)
                    {
                        if (!char.IsDigit(keyValue[i]))
                        {
                            if ((keyValue[i] != '.') || (keyValue[i + 1] == '.'))
                            {
                                goto Label_0082;
                            }
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        return;
                    }
                }
            }
        Label_0082:
            throw new ArgumentException(SR.GetString("Argument_InvalidOidValue"));
        }

        internal static int VerifyCertificate(System.Security.Cryptography.SafeCertContextHandle pCertContext, OidCollection applicationPolicy, OidCollection certificatePolicy, X509RevocationMode revocationMode, X509RevocationFlag revocationFlag, DateTime verificationTime, TimeSpan timeout, X509Certificate2Collection extraStore, IntPtr pszPolicy, IntPtr pdwErrorStatus)
        {
            if ((pCertContext == null) || pCertContext.IsInvalid)
            {
                throw new ArgumentException("pCertContext");
            }
            CAPIBase.CERT_CHAIN_POLICY_PARA pPolicyPara = new CAPIBase.CERT_CHAIN_POLICY_PARA(Marshal.SizeOf(typeof(CAPIBase.CERT_CHAIN_POLICY_PARA)));
            CAPIBase.CERT_CHAIN_POLICY_STATUS pPolicyStatus = new CAPIBase.CERT_CHAIN_POLICY_STATUS(Marshal.SizeOf(typeof(CAPIBase.CERT_CHAIN_POLICY_STATUS)));
            SafeCertChainHandle invalidHandle = SafeCertChainHandle.InvalidHandle;
            int num = X509Chain.BuildChain(new IntPtr(0L), pCertContext, extraStore, applicationPolicy, certificatePolicy, revocationMode, revocationFlag, verificationTime, timeout, ref invalidHandle);
            if (num != 0)
            {
                return num;
            }
            if (!CAPISafe.CertVerifyCertificateChainPolicy(pszPolicy, invalidHandle, ref pPolicyPara, ref pPolicyStatus))
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

