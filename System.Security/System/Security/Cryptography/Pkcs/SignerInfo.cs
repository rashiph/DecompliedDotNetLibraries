namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography.Xml;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class SignerInfo
    {
        private X509Certificate2 m_certificate;
        private System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO m_cmsgSignerInfo;
        private byte[] m_encodedSignerInfo;
        private SignerInfo m_parentSignerInfo;
        [SecurityCritical]
        private System.Security.Cryptography.SafeLocalAllocHandle m_pbCmsgSignerInfo;
        private CryptographicAttributeObjectCollection m_signedAttributes;
        private SignedCms m_signedCms;
        private SubjectIdentifier m_signerIdentifier;
        private CryptographicAttributeObjectCollection m_unsignedAttributes;

        private SignerInfo()
        {
        }

        [SecurityCritical]
        internal SignerInfo(SignedCms signedCms, System.Security.Cryptography.SafeLocalAllocHandle pbCmsgSignerInfo)
        {
            this.m_signedCms = signedCms;
            this.m_parentSignerInfo = null;
            this.m_encodedSignerInfo = null;
            this.m_pbCmsgSignerInfo = pbCmsgSignerInfo;
            this.m_cmsgSignerInfo = (System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO) Marshal.PtrToStructure(pbCmsgSignerInfo.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO));
        }

        [SecuritySafeCritical]
        internal unsafe SignerInfo(SignedCms signedCms, SignerInfo parentSignerInfo, byte[] encodedSignerInfo)
        {
            uint cbDecodedValue = 0;
            System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            fixed (byte* numRef = encodedSignerInfo)
            {
                if (!System.Security.Cryptography.CAPI.DecodeObject(new IntPtr(500L), new IntPtr((void*) numRef), (uint) encodedSignerInfo.Length, out invalidHandle, out cbDecodedValue))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            this.m_signedCms = signedCms;
            this.m_parentSignerInfo = parentSignerInfo;
            this.m_encodedSignerInfo = (byte[]) encodedSignerInfo.Clone();
            this.m_pbCmsgSignerInfo = invalidHandle;
            this.m_cmsgSignerInfo = (System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO) Marshal.PtrToStructure(invalidHandle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO));
        }

        [SecuritySafeCritical]
        public unsafe void CheckHash()
        {
            int size = Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_CTRL_VERIFY_SIGNATURE_EX_PARA));
            System.Security.Cryptography.CAPI.CMSG_CTRL_VERIFY_SIGNATURE_EX_PARA cmsg_ctrl_verify_signature_ex_para = new System.Security.Cryptography.CAPI.CMSG_CTRL_VERIFY_SIGNATURE_EX_PARA(size) {
                dwSignerType = 4,
                dwSignerIndex = (uint) PkcsUtils.GetSignerIndex(this.m_signedCms.GetCryptMsgHandle(), this, 0)
            };
            if (!System.Security.Cryptography.CAPI.CryptMsgControl(this.m_signedCms.GetCryptMsgHandle(), 0, 0x13, new IntPtr((void*) &cmsg_ctrl_verify_signature_ex_para)))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
        }

        public void CheckSignature(bool verifySignatureOnly)
        {
            this.CheckSignature(new X509Certificate2Collection(), verifySignatureOnly);
        }

        public void CheckSignature(X509Certificate2Collection extraStore, bool verifySignatureOnly)
        {
            if (extraStore == null)
            {
                throw new ArgumentNullException("extraStore");
            }
            X509Certificate2 certificate = this.Certificate;
            if (certificate == null)
            {
                certificate = PkcsUtils.FindCertificate(this.SignerIdentifier, extraStore);
                if (certificate == null)
                {
                    throw new CryptographicException(-2146889714);
                }
            }
            this.Verify(extraStore, certificate, verifySignatureOnly);
        }

        public void ComputeCounterSignature()
        {
            this.ComputeCounterSignature(new CmsSigner((this.m_signedCms.Version == 2) ? SubjectIdentifierType.SubjectKeyIdentifier : SubjectIdentifierType.IssuerAndSerialNumber));
        }

        public void ComputeCounterSignature(CmsSigner signer)
        {
            if (this.m_parentSignerInfo != null)
            {
                throw new CryptographicException(-2147483647);
            }
            if (signer == null)
            {
                throw new ArgumentNullException("signer");
            }
            if (signer.Certificate == null)
            {
                signer.Certificate = PkcsUtils.SelectSignerCertificate();
            }
            if (!signer.Certificate.HasPrivateKey)
            {
                throw new CryptographicException(-2146893811);
            }
            this.CounterSign(signer);
        }

        [SecuritySafeCritical]
        private void CounterSign(CmsSigner signer)
        {
            CspParameters parameters = new CspParameters();
            if (!System.Security.Cryptography.X509Certificates.X509Utils.GetPrivateKeyInfo(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(signer.Certificate), ref parameters))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
            KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Sign | KeyContainerPermissionFlags.Open);
            permission.AccessEntries.Add(accessEntry);
            permission.Demand();
            uint dwIndex = (uint) PkcsUtils.GetSignerIndex(this.m_signedCms.GetCryptMsgHandle(), this, 0);
            System.Security.Cryptography.SafeLocalAllocHandle handle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO))));
            System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO structure = PkcsUtils.CreateSignerEncodeInfo(signer);
            try
            {
                Marshal.StructureToPtr(structure, handle.DangerousGetHandle(), false);
                if (!System.Security.Cryptography.CAPI.CryptMsgCountersign(this.m_signedCms.GetCryptMsgHandle(), dwIndex, 1, handle.DangerousGetHandle()))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                this.m_signedCms.ReopenToDecode();
            }
            finally
            {
                Marshal.DestroyStructure(handle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO));
                handle.Dispose();
                structure.Dispose();
            }
            PkcsUtils.AddCertsToMessage(this.m_signedCms.GetCryptMsgHandle(), this.m_signedCms.Certificates, PkcsUtils.CreateBagOfCertificates(signer));
        }

        internal System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO GetCmsgSignerInfo()
        {
            return this.m_cmsgSignerInfo;
        }

        [SecuritySafeCritical]
        public void RemoveCounterSignature(int index)
        {
            if (this.m_parentSignerInfo != null)
            {
                throw new CryptographicException(-2147483647);
            }
            this.RemoveCounterSignature(PkcsUtils.GetSignerIndex(this.m_signedCms.GetCryptMsgHandle(), this, 0), index);
        }

        [SecuritySafeCritical]
        public void RemoveCounterSignature(SignerInfo counterSignerInfo)
        {
            if (this.m_parentSignerInfo != null)
            {
                throw new CryptographicException(-2147483647);
            }
            if (counterSignerInfo == null)
            {
                throw new ArgumentNullException("counterSignerInfo");
            }
            CryptographicAttributeObjectEnumerator enumerator = this.UnsignedAttributes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CryptographicAttributeObject current = enumerator.Current;
                if (string.Compare(current.Oid.Value, "1.2.840.113549.1.9.6", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    for (int i = 0; i < current.Values.Count; i++)
                    {
                        AsnEncodedData data = current.Values[i];
                        SignerInfo info = new SignerInfo(this.m_signedCms, this.m_parentSignerInfo, data.RawData);
                        if ((counterSignerInfo.SignerIdentifier.Type == SubjectIdentifierType.IssuerAndSerialNumber) && (info.SignerIdentifier.Type == SubjectIdentifierType.IssuerAndSerialNumber))
                        {
                            X509IssuerSerial serial = (X509IssuerSerial) counterSignerInfo.SignerIdentifier.Value;
                            X509IssuerSerial serial2 = (X509IssuerSerial) info.SignerIdentifier.Value;
                            if ((string.Compare(serial.IssuerName, serial2.IssuerName, StringComparison.OrdinalIgnoreCase) != 0) || (string.Compare(serial.SerialNumber, serial2.SerialNumber, StringComparison.OrdinalIgnoreCase) != 0))
                            {
                                continue;
                            }
                            this.RemoveCounterSignature(PkcsUtils.GetSignerIndex(this.m_signedCms.GetCryptMsgHandle(), this, 0), i);
                            return;
                        }
                        if ((counterSignerInfo.SignerIdentifier.Type == SubjectIdentifierType.SubjectKeyIdentifier) && (info.SignerIdentifier.Type == SubjectIdentifierType.SubjectKeyIdentifier))
                        {
                            string strA = counterSignerInfo.SignerIdentifier.Value as string;
                            string strB = info.SignerIdentifier.Value as string;
                            if (string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                this.RemoveCounterSignature(PkcsUtils.GetSignerIndex(this.m_signedCms.GetCryptMsgHandle(), this, 0), i);
                                return;
                            }
                        }
                    }
                }
            }
            throw new CryptographicException(-2146889714);
        }

        [SecuritySafeCritical]
        private unsafe void RemoveCounterSignature(int parentIndex, int childIndex)
        {
            if (parentIndex < 0)
            {
                throw new ArgumentOutOfRangeException("parentIndex");
            }
            if (childIndex < 0)
            {
                throw new ArgumentOutOfRangeException("childIndex");
            }
            uint cbData = 0;
            System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            uint num2 = 0;
            System.Security.Cryptography.SafeLocalAllocHandle pvData = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            uint num3 = 0;
            uint cAttr = 0;
            IntPtr zero = IntPtr.Zero;
            System.Security.Cryptography.SafeCryptMsgHandle cryptMsgHandle = this.m_signedCms.GetCryptMsgHandle();
            if (PkcsUtils.CmsSupported())
            {
                PkcsUtils.GetParam(cryptMsgHandle, 0x27, (uint) parentIndex, out invalidHandle, out cbData);
                System.Security.Cryptography.CAPI.CMSG_CMS_SIGNER_INFO cmsg_cms_signer_info = (System.Security.Cryptography.CAPI.CMSG_CMS_SIGNER_INFO) Marshal.PtrToStructure(invalidHandle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_CMS_SIGNER_INFO));
                cAttr = cmsg_cms_signer_info.UnauthAttrs.cAttr;
                zero = new IntPtr((long) cmsg_cms_signer_info.UnauthAttrs.rgAttr);
            }
            else
            {
                PkcsUtils.GetParam(cryptMsgHandle, 6, (uint) parentIndex, out pvData, out num2);
                System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO cmsg_signer_info = (System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO) Marshal.PtrToStructure(pvData.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_INFO));
                cAttr = cmsg_signer_info.UnauthAttrs.cAttr;
                zero = new IntPtr((long) cmsg_signer_info.UnauthAttrs.rgAttr);
            }
            for (num3 = 0; num3 < cAttr; num3++)
            {
                System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE crypt_attribute = (System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE) Marshal.PtrToStructure(zero, typeof(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE));
                if ((string.Compare(crypt_attribute.pszObjId, "1.2.840.113549.1.9.6", StringComparison.OrdinalIgnoreCase) == 0) && (crypt_attribute.cValue > 0))
                {
                    if (childIndex < crypt_attribute.cValue)
                    {
                        System.Security.Cryptography.CAPI.CMSG_CTRL_DEL_SIGNER_UNAUTH_ATTR_PARA cmsg_ctrl_del_signer_unauth_attr_para = new System.Security.Cryptography.CAPI.CMSG_CTRL_DEL_SIGNER_UNAUTH_ATTR_PARA(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_CTRL_DEL_SIGNER_UNAUTH_ATTR_PARA))) {
                            dwSignerIndex = (uint) parentIndex,
                            dwUnauthAttrIndex = num3
                        };
                        if (!System.Security.Cryptography.CAPI.CryptMsgControl(cryptMsgHandle, 0, 9, new IntPtr((void*) &cmsg_ctrl_del_signer_unauth_attr_para)))
                        {
                            throw new CryptographicException(Marshal.GetLastWin32Error());
                        }
                        if (crypt_attribute.cValue > 1)
                        {
                            try
                            {
                                byte[] buffer;
                                uint num5 = (uint) ((crypt_attribute.cValue - 1) * Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB)));
                                System.Security.Cryptography.SafeLocalAllocHandle handle4 = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) num5));
                                System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB* rgValue = (System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB*) crypt_attribute.rgValue;
                                System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB* handle = (System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB*) handle4.DangerousGetHandle();
                                int num6 = 0;
                                while (num6 < crypt_attribute.cValue)
                                {
                                    if (num6 != childIndex)
                                    {
                                        handle[0] = rgValue[0];
                                    }
                                    num6++;
                                    rgValue++;
                                    handle++;
                                }
                                System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE structure = new System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE {
                                    pszObjId = crypt_attribute.pszObjId,
                                    cValue = crypt_attribute.cValue - 1,
                                    rgValue = handle4.DangerousGetHandle()
                                };
                                System.Security.Cryptography.SafeLocalAllocHandle handle5 = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE))));
                                Marshal.StructureToPtr(structure, handle5.DangerousGetHandle(), false);
                                try
                                {
                                    if (!System.Security.Cryptography.CAPI.EncodeObject(new IntPtr(0x16L), handle5.DangerousGetHandle(), out buffer))
                                    {
                                        throw new CryptographicException(Marshal.GetLastWin32Error());
                                    }
                                }
                                finally
                                {
                                    Marshal.DestroyStructure(handle5.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE));
                                    handle5.Dispose();
                                }
                                fixed (byte* numRef = buffer)
                                {
                                    System.Security.Cryptography.CAPI.CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR_PARA cmsg_ctrl_add_signer_unauth_attr_para = new System.Security.Cryptography.CAPI.CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR_PARA(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR_PARA))) {
                                        dwSignerIndex = (uint) parentIndex
                                    };
                                    cmsg_ctrl_add_signer_unauth_attr_para.blob.cbData = (uint) buffer.Length;
                                    cmsg_ctrl_add_signer_unauth_attr_para.blob.pbData = new IntPtr((void*) numRef);
                                    if (!System.Security.Cryptography.CAPI.CryptMsgControl(cryptMsgHandle, 0, 8, new IntPtr((void*) &cmsg_ctrl_add_signer_unauth_attr_para)))
                                    {
                                        throw new CryptographicException(Marshal.GetLastWin32Error());
                                    }
                                }
                                handle4.Dispose();
                            }
                            catch (CryptographicException)
                            {
                                byte[] buffer2;
                                if (System.Security.Cryptography.CAPI.EncodeObject(new IntPtr(0x16L), zero, out buffer2))
                                {
                                    fixed (byte* numRef2 = buffer2)
                                    {
                                        System.Security.Cryptography.CAPI.CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR_PARA cmsg_ctrl_add_signer_unauth_attr_para2 = new System.Security.Cryptography.CAPI.CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR_PARA(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR_PARA))) {
                                            dwSignerIndex = (uint) parentIndex
                                        };
                                        cmsg_ctrl_add_signer_unauth_attr_para2.blob.cbData = (uint) buffer2.Length;
                                        cmsg_ctrl_add_signer_unauth_attr_para2.blob.pbData = new IntPtr((void*) numRef2);
                                        System.Security.Cryptography.CAPI.CryptMsgControl(cryptMsgHandle, 0, 8, new IntPtr((void*) &cmsg_ctrl_add_signer_unauth_attr_para2));
                                    }
                                }
                                throw;
                            }
                        }
                        return;
                    }
                    childIndex -= (int) crypt_attribute.cValue;
                }
                zero = new IntPtr(((long) zero) + Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE)));
            }
            if ((invalidHandle != null) && !invalidHandle.IsInvalid)
            {
                invalidHandle.Dispose();
            }
            if ((pvData != null) && !pvData.IsInvalid)
            {
                pvData.Dispose();
            }
            throw new CryptographicException(-2146885618);
        }

        [SecuritySafeCritical]
        private unsafe void Verify(X509Certificate2Collection extraStore, X509Certificate2 certificate, bool verifySignatureOnly)
        {
            System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            System.Security.Cryptography.CAPI.CERT_CONTEXT cert_context = (System.Security.Cryptography.CAPI.CERT_CONTEXT) Marshal.PtrToStructure(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate).DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CERT_CONTEXT));
            IntPtr ptr = new IntPtr(((long) cert_context.pCertInfo) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_INFO), "SubjectPublicKeyInfo")));
            IntPtr ptr2 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_PUBLIC_KEY_INFO), "Algorithm")));
            IntPtr ptr3 = new IntPtr(((long) ptr2) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER), "Parameters")));
            IntPtr pvKey = Marshal.ReadIntPtr(ptr2);
            if (System.Security.Cryptography.CAPI.CryptFindOIDInfo(1, pvKey, 3).Algid == 0x2200)
            {
                bool flag = false;
                IntPtr ptr5 = new IntPtr(((long) ptr3) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "cbData")));
                IntPtr ptr6 = new IntPtr(((long) ptr3) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "pbData")));
                if (Marshal.ReadInt32(ptr5) == 0)
                {
                    flag = true;
                }
                else if (Marshal.ReadIntPtr(ptr6) == IntPtr.Zero)
                {
                    flag = true;
                }
                else if (Marshal.ReadInt32(Marshal.ReadIntPtr(ptr6)) == 5)
                {
                    flag = true;
                }
                if (flag)
                {
                    System.Security.Cryptography.SafeCertChainHandle ppChainContext = System.Security.Cryptography.SafeCertChainHandle.InvalidHandle;
                    System.Security.Cryptography.X509Certificates.X509Utils.BuildChain(new IntPtr(0L), System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate), null, null, null, X509RevocationMode.NoCheck, X509RevocationFlag.ExcludeRoot, DateTime.Now, new TimeSpan(0, 0, 0), ref ppChainContext);
                    ppChainContext.Dispose();
                    uint pcbData = 0;
                    if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateContextProperty(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate), 0x16, invalidHandle, ref pcbData))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    if (pcbData > 0)
                    {
                        invalidHandle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) pcbData));
                        if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateContextProperty(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate), 0x16, invalidHandle, ref pcbData))
                        {
                            throw new CryptographicException(Marshal.GetLastWin32Error());
                        }
                        Marshal.WriteInt32(ptr5, (int) pcbData);
                        Marshal.WriteIntPtr(ptr6, invalidHandle.DangerousGetHandle());
                    }
                }
            }
            if (this.m_parentSignerInfo == null)
            {
                if (!System.Security.Cryptography.CAPI.CryptMsgControl(this.m_signedCms.GetCryptMsgHandle(), 0, 1, cert_context.pCertInfo))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                goto Label_02F4;
            }
            int num2 = -1;
            int hr = 0;
        Label_022F:
            try
            {
                num2 = PkcsUtils.GetSignerIndex(this.m_signedCms.GetCryptMsgHandle(), this.m_parentSignerInfo, num2 + 1);
            }
            catch (CryptographicException)
            {
                if (hr == 0)
                {
                    throw;
                }
                throw new CryptographicException(hr);
            }
            uint cbData = 0;
            System.Security.Cryptography.SafeLocalAllocHandle pvData = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            PkcsUtils.GetParam(this.m_signedCms.GetCryptMsgHandle(), 0x1c, (uint) num2, out pvData, out cbData);
            if (cbData == 0)
            {
                hr = -2146885618;
                goto Label_022F;
            }
            fixed (byte* numRef = this.m_encodedSignerInfo)
            {
                if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgVerifyCountersignatureEncoded(IntPtr.Zero, 0x10001, pvData.DangerousGetHandle(), cbData, new IntPtr((void*) numRef), (uint) this.m_encodedSignerInfo.Length, cert_context.pCertInfo))
                {
                    hr = Marshal.GetLastWin32Error();
                    goto Label_022F;
                }
            }
            pvData.Dispose();
        Label_02F4:
            if (!verifySignatureOnly)
            {
                int num5 = VerifyCertificate(certificate, extraStore);
                if (num5 != 0)
                {
                    throw new CryptographicException(num5);
                }
            }
            invalidHandle.Dispose();
        }

        [SecuritySafeCritical]
        private static unsafe int VerifyCertificate(X509Certificate2 certificate, X509Certificate2Collection extraStore)
        {
            int num;
            int num2 = System.Security.Cryptography.X509Certificates.X509Utils.VerifyCertificate(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate), null, null, X509RevocationMode.Online, X509RevocationFlag.ExcludeRoot, DateTime.Now, new TimeSpan(0, 0, 0), extraStore, new IntPtr(1L), new IntPtr((void*) &num));
            if (num2 != 0)
            {
                return num;
            }
            X509ExtensionEnumerator enumerator = certificate.Extensions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Extension current = enumerator.Current;
                if (string.Compare(current.Oid.Value, "2.5.29.15", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    X509KeyUsageExtension extension2 = new X509KeyUsageExtension();
                    extension2.CopyFrom(current);
                    if (((extension2.KeyUsages & X509KeyUsageFlags.DigitalSignature) == X509KeyUsageFlags.None) && ((extension2.KeyUsages & X509KeyUsageFlags.NonRepudiation) == X509KeyUsageFlags.None))
                    {
                        return -2146762480;
                    }
                }
            }
            return num2;
        }

        public X509Certificate2 Certificate
        {
            get
            {
                if (this.m_certificate == null)
                {
                    this.m_certificate = PkcsUtils.FindCertificate(this.SignerIdentifier, this.m_signedCms.Certificates);
                }
                return this.m_certificate;
            }
        }

        public SignerInfoCollection CounterSignerInfos
        {
            get
            {
                if (this.m_parentSignerInfo != null)
                {
                    return new SignerInfoCollection();
                }
                return new SignerInfoCollection(this.m_signedCms, this);
            }
        }

        public Oid DigestAlgorithm
        {
            get
            {
                return new Oid(this.m_cmsgSignerInfo.HashAlgorithm.pszObjId);
            }
        }

        public CryptographicAttributeObjectCollection SignedAttributes
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_signedAttributes == null)
                {
                    this.m_signedAttributes = new CryptographicAttributeObjectCollection(this.m_cmsgSignerInfo.AuthAttrs);
                }
                return this.m_signedAttributes;
            }
        }

        public SubjectIdentifier SignerIdentifier
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_signerIdentifier == null)
                {
                    this.m_signerIdentifier = new SubjectIdentifier(this.m_cmsgSignerInfo);
                }
                return this.m_signerIdentifier;
            }
        }

        public CryptographicAttributeObjectCollection UnsignedAttributes
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_unsignedAttributes == null)
                {
                    this.m_unsignedAttributes = new CryptographicAttributeObjectCollection(this.m_cmsgSignerInfo.UnauthAttrs);
                }
                return this.m_unsignedAttributes;
            }
        }

        public int Version
        {
            get
            {
                return (int) this.m_cmsgSignerInfo.dwVersion;
            }
        }
    }
}

