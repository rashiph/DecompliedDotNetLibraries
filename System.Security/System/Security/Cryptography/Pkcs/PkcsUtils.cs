namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography.Xml;
    using System.Text;

    internal static class PkcsUtils
    {
        private static int m_cmsSupported = -1;

        [SecuritySafeCritical]
        internal static unsafe uint AddCertsToMessage(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle, X509Certificate2Collection bagOfCerts, X509Certificate2Collection chainOfCerts)
        {
            uint num = 0;
            X509Certificate2Enumerator enumerator = chainOfCerts.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Certificate2 current = enumerator.Current;
                if (bagOfCerts.Find(X509FindType.FindByThumbprint, current.Thumbprint, false).Count == 0)
                {
                    System.Security.Cryptography.CAPI.CERT_CONTEXT cert_context = *((System.Security.Cryptography.CAPI.CERT_CONTEXT*) System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(current).DangerousGetHandle());
                    System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob = new System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB {
                        cbData = cert_context.cbCertEncoded,
                        pbData = cert_context.pbCertEncoded
                    };
                    if (!System.Security.Cryptography.CAPI.CryptMsgControl(safeCryptMsgHandle, 0, 10, new IntPtr((long) ((ulong) ((IntPtr) &cryptoapi_blob)))))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    num++;
                }
            }
            return num;
        }

        internal static uint AlignedLength(uint length)
        {
            return ((length + 7) & 0xfffffff8);
        }

        private static void checkErr(int err)
        {
            if (-2146889724 != err)
            {
                throw new CryptographicException(err);
            }
        }

        [SecuritySafeCritical]
        internal static bool CmsSupported()
        {
            if (m_cmsSupported == -1)
            {
                using (System.Security.Cryptography.SafeLibraryHandle handle = System.Security.Cryptography.CAPI.CAPISafe.LoadLibrary("Crypt32.dll"))
                {
                    if (!handle.IsInvalid)
                    {
                        m_cmsSupported = (System.Security.Cryptography.CAPI.CAPISafe.GetProcAddress(handle, "CryptMsgVerifyCountersignatureEncodedEx") == IntPtr.Zero) ? 0 : 1;
                    }
                }
            }
            return (m_cmsSupported != 0);
        }

        [SecuritySafeCritical]
        internal static X509Certificate2Collection CreateBagOfCertificates(CmsSigner signer)
        {
            X509Certificate2Collection certificates = new X509Certificate2Collection();
            certificates.AddRange(signer.Certificates);
            if (signer.IncludeOption != X509IncludeOption.None)
            {
                if (signer.IncludeOption == X509IncludeOption.EndCertOnly)
                {
                    certificates.Add(signer.Certificate);
                    return certificates;
                }
                int count = 1;
                X509Chain chain = new X509Chain();
                chain.Build(signer.Certificate);
                if ((chain.ChainStatus.Length > 0) && ((chain.ChainStatus[0].Status & X509ChainStatusFlags.PartialChain) == X509ChainStatusFlags.PartialChain))
                {
                    throw new CryptographicException(-2146762486);
                }
                if (signer.IncludeOption == X509IncludeOption.WholeChain)
                {
                    count = chain.ChainElements.Count;
                }
                else if (chain.ChainElements.Count > 1)
                {
                    count = chain.ChainElements.Count - 1;
                }
                for (int i = 0; i < count; i++)
                {
                    certificates.Add(chain.ChainElements[i].Certificate);
                }
            }
            return certificates;
        }

        [SecurityCritical]
        internal static unsafe IntPtr CreateCryptAttributes(CryptographicAttributeObjectCollection attributes)
        {
            if (attributes.Count == 0)
            {
                return IntPtr.Zero;
            }
            uint num = 0;
            uint num2 = AlignedLength((uint) Marshal.SizeOf(typeof(I_CRYPT_ATTRIBUTE)));
            uint num3 = AlignedLength((uint) Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB)));
            CryptographicAttributeObjectEnumerator enumerator = attributes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CryptographicAttributeObject current = enumerator.Current;
                num += num2;
                num += AlignedLength((uint) (current.Oid.Value.Length + 1));
                AsnEncodedDataEnumerator enumerator2 = current.Values.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    AsnEncodedData data = enumerator2.Current;
                    num += num3;
                    num += AlignedLength((uint) data.RawData.Length);
                }
            }
            System.Security.Cryptography.SafeLocalAllocHandle handle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) num));
            I_CRYPT_ATTRIBUTE* i_crypt_attributePtr = (I_CRYPT_ATTRIBUTE*) handle.DangerousGetHandle();
            IntPtr ptr = new IntPtr(((long) handle.DangerousGetHandle()) + (num2 * attributes.Count));
            CryptographicAttributeObjectEnumerator enumerator3 = attributes.GetEnumerator();
            while (enumerator3.MoveNext())
            {
                CryptographicAttributeObject obj3 = enumerator3.Current;
                byte* numPtr = (byte*) ptr;
                byte[] bytes = new byte[obj3.Oid.Value.Length + 1];
                System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB* cryptoapi_blobPtr = (System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB*) (numPtr + AlignedLength((uint) bytes.Length));
                i_crypt_attributePtr->pszObjId = (IntPtr) numPtr;
                i_crypt_attributePtr->cValue = (uint) obj3.Values.Count;
                i_crypt_attributePtr->rgValue = (IntPtr) cryptoapi_blobPtr;
                Encoding.ASCII.GetBytes(obj3.Oid.Value, 0, obj3.Oid.Value.Length, bytes, 0);
                Marshal.Copy(bytes, 0, i_crypt_attributePtr->pszObjId, bytes.Length);
                IntPtr destination = new IntPtr(((long) ((ulong) cryptoapi_blobPtr)) + (obj3.Values.Count * num3));
                AsnEncodedDataEnumerator enumerator4 = obj3.Values.GetEnumerator();
                while (enumerator4.MoveNext())
                {
                    byte[] rawData = enumerator4.Current.RawData;
                    if (rawData.Length > 0)
                    {
                        cryptoapi_blobPtr->cbData = (uint) rawData.Length;
                        cryptoapi_blobPtr->pbData = destination;
                        Marshal.Copy(rawData, 0, destination, rawData.Length);
                        destination = new IntPtr(((long) destination) + AlignedLength((uint) rawData.Length));
                    }
                    cryptoapi_blobPtr++;
                }
                i_crypt_attributePtr++;
                ptr = destination;
            }
            GC.SuppressFinalize(handle);
            return handle.DangerousGetHandle();
        }

        [SecuritySafeCritical]
        internal static unsafe X509Certificate2 CreateDummyCertificate(CspParameters parameters)
        {
            System.Security.Cryptography.SafeCertContextHandle invalidHandle = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            System.Security.Cryptography.SafeCryptProvHandle hCryptProv = System.Security.Cryptography.SafeCryptProvHandle.InvalidHandle;
            uint dwFlags = 0;
            if ((parameters.Flags & CspProviderFlags.UseMachineKeyStore) != CspProviderFlags.NoFlags)
            {
                dwFlags |= 0x20;
            }
            if ((parameters.Flags & CspProviderFlags.UseDefaultKeyContainer) != CspProviderFlags.NoFlags)
            {
                dwFlags |= 0xf0000000;
            }
            if ((parameters.Flags & CspProviderFlags.NoPrompt) != CspProviderFlags.NoFlags)
            {
                dwFlags |= 0x40;
            }
            if (!System.Security.Cryptography.CAPI.CryptAcquireContext(ref hCryptProv, parameters.KeyContainerName, parameters.ProviderName, (uint) parameters.ProviderType, dwFlags))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            System.Security.Cryptography.CAPI.CRYPT_KEY_PROV_INFO structure = new System.Security.Cryptography.CAPI.CRYPT_KEY_PROV_INFO {
                pwszProvName = parameters.ProviderName,
                pwszContainerName = parameters.KeyContainerName,
                dwProvType = (uint) parameters.ProviderType,
                dwKeySpec = (uint) parameters.KeyNumber,
                dwFlags = ((parameters.Flags & CspProviderFlags.UseMachineKeyStore) == CspProviderFlags.UseMachineKeyStore) ? 0x20 : 0
            };
            System.Security.Cryptography.SafeLocalAllocHandle handle3 = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPT_KEY_PROV_INFO))));
            Marshal.StructureToPtr(structure, handle3.DangerousGetHandle(), false);
            System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER crypt_algorithm_identifier = new System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER {
                pszObjId = "1.3.14.3.2.29"
            };
            System.Security.Cryptography.SafeLocalAllocHandle handle4 = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER))));
            Marshal.StructureToPtr(crypt_algorithm_identifier, handle4.DangerousGetHandle(), false);
            X500DistinguishedName name = new X500DistinguishedName("cn=CMS Signer Dummy Certificate");
            fixed (byte* numRef = name.RawData)
            {
                System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob = new System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB {
                    cbData = (uint) name.RawData.Length,
                    pbData = new IntPtr((void*) numRef)
                };
                invalidHandle = System.Security.Cryptography.CAPI.CAPIUnsafe.CertCreateSelfSignCertificate(hCryptProv, new IntPtr((void*) &cryptoapi_blob), 1, handle3.DangerousGetHandle(), handle4.DangerousGetHandle(), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }
            Marshal.DestroyStructure(handle3.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CRYPT_KEY_PROV_INFO));
            handle3.Dispose();
            Marshal.DestroyStructure(handle4.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER));
            handle4.Dispose();
            if ((invalidHandle == null) || invalidHandle.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            X509Certificate2 certificate = new X509Certificate2(invalidHandle.DangerousGetHandle());
            invalidHandle.Dispose();
            return certificate;
        }

        [SecurityCritical]
        internal static unsafe System.Security.Cryptography.SafeLocalAllocHandle CreateEncodedCertBlob(X509Certificate2Collection certificates)
        {
            System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            if (certificates.Count > 0)
            {
                invalidHandle = System.Security.Cryptography.CAPI.LocalAlloc(0, new IntPtr(certificates.Count * Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB))));
                System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB* handle = (System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB*) invalidHandle.DangerousGetHandle();
                X509Certificate2Enumerator enumerator = certificates.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    System.Security.Cryptography.CAPI.CERT_CONTEXT cert_context = *((System.Security.Cryptography.CAPI.CERT_CONTEXT*) System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(enumerator.Current).DangerousGetHandle());
                    handle->cbData = cert_context.cbCertEncoded;
                    handle->pbData = cert_context.pbCertEncoded;
                    handle++;
                }
            }
            return invalidHandle;
        }

        internal static System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO CreateSignerEncodeInfo(CmsSigner signer)
        {
            return CreateSignerEncodeInfo(signer, false);
        }

        [SecuritySafeCritical]
        internal static unsafe System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO CreateSignerEncodeInfo(CmsSigner signer, bool silent)
        {
            System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO cmsg_signer_encode_info = new System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO)));
            System.Security.Cryptography.SafeCryptProvHandle invalidHandle = System.Security.Cryptography.SafeCryptProvHandle.InvalidHandle;
            uint pdwKeySpec = 0;
            bool pfCallerFreeProv = false;
            cmsg_signer_encode_info.HashAlgorithm.pszObjId = signer.DigestAlgorithm.Value;
            if (string.Compare(signer.Certificate.PublicKey.Oid.Value, "1.2.840.10040.4.1", StringComparison.Ordinal) == 0)
            {
                cmsg_signer_encode_info.HashEncryptionAlgorithm.pszObjId = "1.2.840.10040.4.3";
            }
            cmsg_signer_encode_info.cAuthAttr = (uint) signer.SignedAttributes.Count;
            cmsg_signer_encode_info.rgAuthAttr = CreateCryptAttributes(signer.SignedAttributes);
            cmsg_signer_encode_info.cUnauthAttr = (uint) signer.UnsignedAttributes.Count;
            cmsg_signer_encode_info.rgUnauthAttr = CreateCryptAttributes(signer.UnsignedAttributes);
            if (signer.SignerIdentifierType == SubjectIdentifierType.NoSignature)
            {
                cmsg_signer_encode_info.HashEncryptionAlgorithm.pszObjId = "1.3.6.1.5.5.7.6.2";
                cmsg_signer_encode_info.pCertInfo = IntPtr.Zero;
                cmsg_signer_encode_info.dwKeySpec = pdwKeySpec;
                if (!System.Security.Cryptography.CAPI.CryptAcquireContext(ref invalidHandle, (string) null, (string) null, 1, 0xf0000000))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                cmsg_signer_encode_info.hCryptProv = invalidHandle.DangerousGetHandle();
                GC.SuppressFinalize(invalidHandle);
                cmsg_signer_encode_info.SignerId.dwIdChoice = 1;
                X500DistinguishedName name = new X500DistinguishedName("CN=Dummy Signer") {
                    Oid = new Oid("1.3.6.1.4.1.311.21.9")
                };
                cmsg_signer_encode_info.SignerId.Value.IssuerSerialNumber.Issuer.cbData = (uint) name.RawData.Length;
                System.Security.Cryptography.SafeLocalAllocHandle handle2 = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) cmsg_signer_encode_info.SignerId.Value.IssuerSerialNumber.Issuer.cbData));
                Marshal.Copy(name.RawData, 0, handle2.DangerousGetHandle(), name.RawData.Length);
                cmsg_signer_encode_info.SignerId.Value.IssuerSerialNumber.Issuer.pbData = handle2.DangerousGetHandle();
                GC.SuppressFinalize(handle2);
                cmsg_signer_encode_info.SignerId.Value.IssuerSerialNumber.SerialNumber.cbData = 1;
                System.Security.Cryptography.SafeLocalAllocHandle handle3 = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) cmsg_signer_encode_info.SignerId.Value.IssuerSerialNumber.SerialNumber.cbData));
                byte* handle = (byte*) handle3.DangerousGetHandle();
                handle[0] = 0;
                cmsg_signer_encode_info.SignerId.Value.IssuerSerialNumber.SerialNumber.pbData = handle3.DangerousGetHandle();
                GC.SuppressFinalize(handle3);
                return cmsg_signer_encode_info;
            }
            System.Security.Cryptography.SafeCertContextHandle certContext = System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(signer.Certificate);
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptAcquireCertificatePrivateKey(certContext, silent ? 70 : 6, IntPtr.Zero, ref invalidHandle, ref pdwKeySpec, ref pfCallerFreeProv))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            cmsg_signer_encode_info.dwKeySpec = pdwKeySpec;
            cmsg_signer_encode_info.hCryptProv = invalidHandle.DangerousGetHandle();
            GC.SuppressFinalize(invalidHandle);
            System.Security.Cryptography.CAPI.CERT_CONTEXT cert_context = *((System.Security.Cryptography.CAPI.CERT_CONTEXT*) certContext.DangerousGetHandle());
            cmsg_signer_encode_info.pCertInfo = cert_context.pCertInfo;
            if (signer.SignerIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier)
            {
                uint pcbData = 0;
                System.Security.Cryptography.SafeLocalAllocHandle pvData = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
                if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateContextProperty(certContext, 20, pvData, ref pcbData))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                if (pcbData <= 0)
                {
                    return cmsg_signer_encode_info;
                }
                pvData = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) pcbData));
                if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateContextProperty(certContext, 20, pvData, ref pcbData))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                cmsg_signer_encode_info.SignerId.dwIdChoice = 2;
                cmsg_signer_encode_info.SignerId.Value.KeyId.cbData = pcbData;
                cmsg_signer_encode_info.SignerId.Value.KeyId.pbData = pvData.DangerousGetHandle();
                GC.SuppressFinalize(pvData);
            }
            return cmsg_signer_encode_info;
        }

        [SecurityCritical]
        internal static unsafe X509IssuerSerial DecodeIssuerSerial(System.Security.Cryptography.CAPI.CERT_ISSUER_SERIAL_NUMBER pIssuerAndSerial)
        {
            System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            uint csz = System.Security.Cryptography.CAPI.CAPISafe.CertNameToStrW(0x10001, new IntPtr((void*) &pIssuerAndSerial.Issuer), 0x2000003, invalidHandle, 0);
            if (csz <= 1)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            invalidHandle = System.Security.Cryptography.CAPI.LocalAlloc(0, new IntPtr((long) (2 * csz)));
            if (System.Security.Cryptography.CAPI.CAPISafe.CertNameToStrW(0x10001, new IntPtr((void*) &pIssuerAndSerial.Issuer), 0x2000003, invalidHandle, csz) <= 1)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            X509IssuerSerial serial = new X509IssuerSerial {
                IssuerName = Marshal.PtrToStringUni(invalidHandle.DangerousGetHandle())
            };
            byte[] destination = new byte[pIssuerAndSerial.SerialNumber.cbData];
            Marshal.Copy(pIssuerAndSerial.SerialNumber.pbData, destination, 0, destination.Length);
            serial.SerialNumber = System.Security.Cryptography.X509Certificates.X509Utils.EncodeHexStringFromInt(destination);
            invalidHandle.Dispose();
            return serial;
        }

        internal static string DecodeObjectIdentifier(byte[] encodedObjId, int offset)
        {
            StringBuilder builder = new StringBuilder("");
            if (0 < (encodedObjId.Length - offset))
            {
                byte num = encodedObjId[offset];
                byte num2 = (byte) (num / 40);
                builder.Append(num2.ToString(null, null));
                builder.Append(".");
                num2 = (byte) (num % 40);
                builder.Append(num2.ToString(null, null));
                ulong num3 = 0L;
                for (int i = offset + 1; i < encodedObjId.Length; i++)
                {
                    num2 = encodedObjId[i];
                    num3 = (num3 << 7) + (num2 & 0x7f);
                    if ((num2 & 0x80) == 0)
                    {
                        builder.Append(".");
                        builder.Append(num3.ToString(null, null));
                        num3 = 0L;
                    }
                }
                if (0L != num3)
                {
                    throw new CryptographicException(-2146885630);
                }
            }
            return builder.ToString();
        }

        [SecuritySafeCritical]
        internal static byte[] DecodeOctetBytes(byte[] encodedOctetString)
        {
            uint cbDecodedValue = 0;
            System.Security.Cryptography.SafeLocalAllocHandle decodedValue = null;
            if (!System.Security.Cryptography.CAPI.DecodeObject(new IntPtr(0x19L), encodedOctetString, out decodedValue, out cbDecodedValue))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (cbDecodedValue == 0)
            {
                return new byte[0];
            }
            using (decodedValue)
            {
                return System.Security.Cryptography.CAPI.BlobToByteArray(decodedValue.DangerousGetHandle());
            }
        }

        [SecuritySafeCritical]
        internal static string DecodeOctetString(byte[] encodedOctetString)
        {
            uint cbDecodedValue = 0;
            System.Security.Cryptography.SafeLocalAllocHandle decodedValue = null;
            if (!System.Security.Cryptography.CAPI.DecodeObject(new IntPtr(0x19L), encodedOctetString, out decodedValue, out cbDecodedValue))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (cbDecodedValue == 0)
            {
                return string.Empty;
            }
            System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob = (System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB) Marshal.PtrToStructure(decodedValue.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB));
            if (cryptoapi_blob.cbData == 0)
            {
                return string.Empty;
            }
            string str = Marshal.PtrToStringUni(cryptoapi_blob.pbData);
            decodedValue.Dispose();
            return str;
        }

        internal static byte[] EncodeOctetString(string octetString)
        {
            byte[] bytes = new byte[2 * (octetString.Length + 1)];
            Encoding.Unicode.GetBytes(octetString, 0, octetString.Length, bytes, 0);
            return EncodeOctetString(bytes);
        }

        [SecuritySafeCritical]
        internal static unsafe byte[] EncodeOctetString(byte[] octets)
        {
            fixed (byte* numRef = octets)
            {
                System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob = new System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB {
                    cbData = (uint) octets.Length,
                    pbData = new IntPtr((void*) numRef)
                };
                byte[] encodedData = new byte[0];
                if (!System.Security.Cryptography.CAPI.EncodeObject(new IntPtr(0x19L), new IntPtr((long) ((ulong) ((IntPtr) &cryptoapi_blob))), out encodedData))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                return encodedData;
            }
        }

        internal static X509Certificate2 FindCertificate(SubjectIdentifier identifier, X509Certificate2Collection certificates)
        {
            X509Certificate2 certificate = null;
            if ((certificates != null) && (certificates.Count > 0))
            {
                X509Certificate2Collection certificates2;
                switch (identifier.Type)
                {
                    case SubjectIdentifierType.IssuerAndSerialNumber:
                    {
                        X509IssuerSerial serial = (X509IssuerSerial) identifier.Value;
                        certificates2 = certificates.Find(X509FindType.FindByIssuerDistinguishedName, serial.IssuerName, false);
                        if (certificates2.Count > 0)
                        {
                            X509IssuerSerial serial2 = (X509IssuerSerial) identifier.Value;
                            certificates2 = certificates2.Find(X509FindType.FindBySerialNumber, serial2.SerialNumber, false);
                            if (certificates2.Count > 0)
                            {
                                certificate = certificates2[0];
                            }
                        }
                        return certificate;
                    }
                    case SubjectIdentifierType.SubjectKeyIdentifier:
                        certificates2 = certificates.Find(X509FindType.FindBySubjectKeyIdentifier, identifier.Value, false);
                        if (certificates2.Count > 0)
                        {
                            certificate = certificates2[0];
                        }
                        return certificate;
                }
            }
            return certificate;
        }

        [SecurityCritical]
        internal static unsafe AlgorithmIdentifier GetAlgorithmIdentifier(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle)
        {
            AlgorithmIdentifier identifier = new AlgorithmIdentifier();
            uint num = 0;
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, 15, 0, IntPtr.Zero, new IntPtr((void*) &num)))
            {
                checkErr(Marshal.GetLastWin32Error());
            }
            if (num > 0)
            {
                System.Security.Cryptography.SafeLocalAllocHandle pvData = System.Security.Cryptography.CAPI.LocalAlloc(0, new IntPtr((long) num));
                if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, 15, 0, pvData, new IntPtr((void*) &num)))
                {
                    checkErr(Marshal.GetLastWin32Error());
                }
                System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER algorithmIdentifier = (System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER) Marshal.PtrToStructure(pvData.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER));
                identifier = new AlgorithmIdentifier(algorithmIdentifier);
                pvData.Dispose();
            }
            return identifier;
        }

        [SecuritySafeCritical]
        internal static AsnEncodedDataCollection GetAsnEncodedDataCollection(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE cryptAttribute)
        {
            AsnEncodedDataCollection datas = new AsnEncodedDataCollection();
            Oid oid = new Oid(cryptAttribute.pszObjId);
            string name = oid.Value;
            for (uint i = 0; i < cryptAttribute.cValue; i++)
            {
                IntPtr pBlob = new IntPtr(((long) cryptAttribute.rgValue) + (i * Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB))));
                Pkcs9AttributeObject asnEncodedData = new Pkcs9AttributeObject(oid, System.Security.Cryptography.CAPI.BlobToByteArray(pBlob));
                Pkcs9AttributeObject obj3 = CryptoConfig.CreateFromName(name) as Pkcs9AttributeObject;
                if (obj3 != null)
                {
                    obj3.CopyFrom(asnEncodedData);
                    asnEncodedData = obj3;
                }
                datas.Add(asnEncodedData);
            }
            return datas;
        }

        [SecurityCritical]
        internal static AsnEncodedDataCollection GetAsnEncodedDataCollection(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE_TYPE_VALUE cryptAttribute)
        {
            AsnEncodedDataCollection datas = new AsnEncodedDataCollection();
            datas.Add(new Pkcs9AttributeObject(new Oid(cryptAttribute.pszObjId), System.Security.Cryptography.CAPI.BlobToByteArray(cryptAttribute.Value)));
            return datas;
        }

        [SecurityCritical]
        internal static unsafe X509Certificate2Collection GetCertificates(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle)
        {
            uint num = 0;
            uint num2 = (uint) Marshal.SizeOf(typeof(uint));
            X509Certificate2Collection certificates = new X509Certificate2Collection();
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, 11, 0, new IntPtr((void*) &num), new IntPtr((void*) &num2)))
            {
                checkErr(Marshal.GetLastWin32Error());
            }
            for (uint i = 0; i < num; i++)
            {
                uint cbData = 0;
                System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
                GetParam(safeCryptMsgHandle, 12, i, out invalidHandle, out cbData);
                if (cbData > 0)
                {
                    System.Security.Cryptography.SafeCertContextHandle handle2 = System.Security.Cryptography.CAPI.CAPISafe.CertCreateCertificateContext(0x10001, invalidHandle, cbData);
                    if ((handle2 == null) || handle2.IsInvalid)
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    certificates.Add(new X509Certificate2(handle2.DangerousGetHandle()));
                    handle2.Dispose();
                }
            }
            return certificates;
        }

        [SecurityCritical]
        internal static byte[] GetContent(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle)
        {
            uint cbData = 0;
            byte[] pvData = new byte[0];
            GetParam(safeCryptMsgHandle, 2, 0, out pvData, out cbData);
            return pvData;
        }

        [SecurityCritical]
        internal static Oid GetContentType(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle)
        {
            uint cbData = 0;
            byte[] pvData = new byte[0];
            GetParam(safeCryptMsgHandle, 4, 0, out pvData, out cbData);
            if ((pvData.Length > 0) && (pvData[pvData.Length - 1] == 0))
            {
                byte[] destinationArray = new byte[pvData.Length - 1];
                Array.Copy(pvData, 0, destinationArray, 0, destinationArray.Length);
                pvData = destinationArray;
            }
            return new Oid(Encoding.ASCII.GetString(pvData));
        }

        [SecurityCritical]
        internal static unsafe int GetMaxKeyLength(System.Security.Cryptography.SafeCryptProvHandle safeCryptProvHandle, uint algId)
        {
            uint dwFlags = 1;
            uint num2 = (uint) Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.PROV_ENUMALGS_EX));
            System.Security.Cryptography.SafeLocalAllocHandle handle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.PROV_ENUMALGS_EX))));
            using (handle)
            {
                while (System.Security.Cryptography.CAPI.CAPISafe.CryptGetProvParam(safeCryptProvHandle, 0x16, handle.DangerousGetHandle(), new IntPtr((void*) &num2), dwFlags))
                {
                    System.Security.Cryptography.CAPI.PROV_ENUMALGS_EX prov_enumalgs_ex = (System.Security.Cryptography.CAPI.PROV_ENUMALGS_EX) Marshal.PtrToStructure(handle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.PROV_ENUMALGS_EX));
                    if (prov_enumalgs_ex.aiAlgid == algId)
                    {
                        return (int) prov_enumalgs_ex.dwMaxLen;
                    }
                    dwFlags = 0;
                }
            }
            throw new CryptographicException(-2146889726);
        }

        [SecurityCritical]
        internal static byte[] GetMessage(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle)
        {
            uint cbData = 0;
            byte[] pvData = new byte[0];
            GetParam(safeCryptMsgHandle, 0x1d, 0, out pvData, out cbData);
            return pvData;
        }

        [SecurityCritical]
        internal static unsafe uint GetMessageType(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle)
        {
            uint num = 0;
            uint num2 = (uint) Marshal.SizeOf(typeof(uint));
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, 1, 0, new IntPtr((void*) &num), new IntPtr((void*) &num2)))
            {
                checkErr(Marshal.GetLastWin32Error());
            }
            return num;
        }

        [SecurityCritical]
        internal static unsafe void GetParam(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle, uint paramType, uint index, out System.Security.Cryptography.SafeLocalAllocHandle pvData, out uint cbData)
        {
            cbData = 0;
            pvData = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            fixed (uint* numRef = ((uint*) cbData))
            {
                if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, paramType, index, pvData, new IntPtr((void*) numRef)))
                {
                    checkErr(Marshal.GetLastWin32Error());
                }
                if (cbData > 0)
                {
                    pvData = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) ((ulong) cbData)));
                    if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, paramType, index, pvData, new IntPtr((void*) numRef)))
                    {
                        checkErr(Marshal.GetLastWin32Error());
                    }
                }
            }
        }

        [SecurityCritical]
        internal static unsafe void GetParam(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle, uint paramType, uint index, out byte[] pvData, out uint cbData)
        {
            cbData = 0;
            pvData = new byte[0];
            fixed (uint* numRef = ((uint*) cbData))
            {
                if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, paramType, index, IntPtr.Zero, new IntPtr((void*) numRef)))
                {
                    checkErr(Marshal.GetLastWin32Error());
                }
                if (cbData > 0)
                {
                    pvData = new byte[cbData];
                    fixed (byte* numRef2 = pvData)
                    {
                        if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, paramType, index, new IntPtr((void*) numRef2), new IntPtr((void*) numRef)))
                        {
                            checkErr(Marshal.GetLastWin32Error());
                        }
                    }
                }
            }
        }

        [SecuritySafeCritical]
        internal static RecipientInfoType GetRecipientInfoType(X509Certificate2 certificate)
        {
            RecipientInfoType unknown = RecipientInfoType.Unknown;
            if (certificate == null)
            {
                return unknown;
            }
            System.Security.Cryptography.CAPI.CERT_CONTEXT cert_context = (System.Security.Cryptography.CAPI.CERT_CONTEXT) Marshal.PtrToStructure(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate).DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CERT_CONTEXT));
            System.Security.Cryptography.CAPI.CERT_INFO cert_info = (System.Security.Cryptography.CAPI.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(System.Security.Cryptography.CAPI.CERT_INFO));
            switch (System.Security.Cryptography.X509Certificates.X509Utils.OidToAlgId(cert_info.SubjectPublicKeyInfo.Algorithm.pszObjId))
            {
                case 0xa400:
                    return RecipientInfoType.KeyTransport;

                case 0xaa01:
                case 0xaa02:
                    return RecipientInfoType.KeyAgreement;
            }
            return RecipientInfoType.Unknown;
        }

        [SecurityCritical]
        internal static unsafe int GetSignerIndex(System.Security.Cryptography.SafeCryptMsgHandle safeCrytpMsgHandle, SignerInfo signerInfo, int startIndex)
        {
            uint num = 0;
            uint num2 = (uint) Marshal.SizeOf(typeof(uint));
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCrytpMsgHandle, 5, 0, new IntPtr((void*) &num), new IntPtr((void*) &num2)))
            {
                checkErr(Marshal.GetLastWin32Error());
            }
            for (int i = startIndex; i < num; i++)
            {
                uint num4 = 0;
                if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCrytpMsgHandle, 6, (uint) i, IntPtr.Zero, new IntPtr((void*) &num4)))
                {
                    checkErr(Marshal.GetLastWin32Error());
                }
                if (num4 > 0)
                {
                    System.Security.Cryptography.SafeLocalAllocHandle pvData = System.Security.Cryptography.CAPI.LocalAlloc(0, new IntPtr((long) num4));
                    if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCrytpMsgHandle, 6, (uint) i, pvData, new IntPtr((void*) &num4)))
                    {
                        checkErr(Marshal.GetLastWin32Error());
                    }
                    System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO cmsgSignerInfo = signerInfo.GetCmsgSignerInfo();
                    System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO cmsg_signer_info2 = (System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO) Marshal.PtrToStructure(pvData.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO));
                    if (System.Security.Cryptography.X509Certificates.X509Utils.MemEqual((byte*) cmsgSignerInfo.Issuer.pbData, cmsgSignerInfo.Issuer.cbData, (byte*) cmsg_signer_info2.Issuer.pbData, cmsg_signer_info2.Issuer.cbData) && System.Security.Cryptography.X509Certificates.X509Utils.MemEqual((byte*) cmsgSignerInfo.SerialNumber.pbData, cmsgSignerInfo.SerialNumber.cbData, (byte*) cmsg_signer_info2.SerialNumber.pbData, cmsg_signer_info2.SerialNumber.cbData))
                    {
                        return i;
                    }
                    pvData.Dispose();
                }
            }
            throw new CryptographicException(-2146889714);
        }

        [SecurityCritical]
        internal static unsafe CryptographicAttributeObjectCollection GetUnprotectedAttributes(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle)
        {
            uint num = 0;
            CryptographicAttributeObjectCollection objects = new CryptographicAttributeObjectCollection();
            System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, 0x25, 0, invalidHandle, new IntPtr((void*) &num)) && (Marshal.GetLastWin32Error() != -2146889713))
            {
                checkErr(Marshal.GetLastWin32Error());
            }
            if (num <= 0)
            {
                return objects;
            }
            using (invalidHandle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) num)))
            {
                if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, 0x25, 0, invalidHandle, new IntPtr((void*) &num)))
                {
                    checkErr(Marshal.GetLastWin32Error());
                }
                return new CryptographicAttributeObjectCollection(invalidHandle);
            }
        }

        [SecurityCritical]
        internal static unsafe uint GetVersion(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle)
        {
            uint num = 0;
            uint num2 = (uint) Marshal.SizeOf(typeof(uint));
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, 30, 0, new IntPtr((void*) &num), new IntPtr((void*) &num2)))
            {
                checkErr(Marshal.GetLastWin32Error());
            }
            return num;
        }

        internal static CmsRecipientCollection SelectRecipients(SubjectIdentifierType recipientIdentifierType)
        {
            X509Store store = new X509Store("AddressBook");
            store.Open(OpenFlags.OpenExistingOnly);
            X509Certificate2Collection certificates = new X509Certificate2Collection(store.Certificates);
            X509Certificate2Enumerator enumerator = store.Certificates.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Certificate2 current = enumerator.Current;
                if ((current.NotBefore <= DateTime.Now) && (current.NotAfter >= DateTime.Now))
                {
                    bool flag = true;
                    X509ExtensionEnumerator enumerator2 = current.Extensions.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        X509Extension asnEncodedData = enumerator2.Current;
                        if (string.Compare(asnEncodedData.Oid.Value, "2.5.29.15", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            X509KeyUsageExtension extension2 = new X509KeyUsageExtension();
                            extension2.CopyFrom(asnEncodedData);
                            if (((extension2.KeyUsages & X509KeyUsageFlags.KeyEncipherment) == X509KeyUsageFlags.None) && ((extension2.KeyUsages & X509KeyUsageFlags.KeyAgreement) == X509KeyUsageFlags.None))
                            {
                                flag = false;
                            }
                            break;
                        }
                    }
                    if (flag)
                    {
                        certificates.Add(current);
                    }
                }
            }
            if (certificates.Count < 1)
            {
                throw new CryptographicException(-2146889717);
            }
            X509Certificate2Collection certificates2 = X509Certificate2UI.SelectFromCollection(certificates, null, null, X509SelectionFlag.MultiSelection);
            if (certificates2.Count < 1)
            {
                throw new CryptographicException(0x4c7);
            }
            return new CmsRecipientCollection(recipientIdentifierType, certificates2);
        }

        internal static X509Certificate2 SelectSignerCertificate()
        {
            X509Store store = new X509Store();
            store.Open(OpenFlags.IncludeArchived | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection certificates = new X509Certificate2Collection();
            X509Certificate2Enumerator enumerator = store.Certificates.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Certificate2 current = enumerator.Current;
                if ((current.HasPrivateKey && (current.NotBefore <= DateTime.Now)) && (current.NotAfter >= DateTime.Now))
                {
                    bool flag = true;
                    X509ExtensionEnumerator enumerator2 = current.Extensions.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        X509Extension asnEncodedData = enumerator2.Current;
                        if (string.Compare(asnEncodedData.Oid.Value, "2.5.29.15", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            X509KeyUsageExtension extension2 = new X509KeyUsageExtension();
                            extension2.CopyFrom(asnEncodedData);
                            if (((extension2.KeyUsages & X509KeyUsageFlags.DigitalSignature) == X509KeyUsageFlags.None) && ((extension2.KeyUsages & X509KeyUsageFlags.NonRepudiation) == X509KeyUsageFlags.None))
                            {
                                flag = false;
                            }
                            break;
                        }
                    }
                    if (flag)
                    {
                        certificates.Add(current);
                    }
                }
            }
            if (certificates.Count < 1)
            {
                throw new CryptographicException(-2146889714);
            }
            certificates = X509Certificate2UI.SelectFromCollection(certificates, null, null, X509SelectionFlag.SingleSelection);
            if (certificates.Count < 1)
            {
                throw new CryptographicException(0x4c7);
            }
            return certificates[0];
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct I_CRYPT_ATTRIBUTE
        {
            internal IntPtr pszObjId;
            internal uint cValue;
            internal IntPtr rgValue;
        }
    }
}

