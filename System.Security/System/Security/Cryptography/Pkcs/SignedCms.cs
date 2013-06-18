namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class SignedCms
    {
        private System.Security.Cryptography.Pkcs.ContentInfo m_contentInfo;
        private bool m_detached;
        [SecurityCritical]
        private System.Security.Cryptography.SafeCryptMsgHandle m_safeCryptMsgHandle;
        private SubjectIdentifierType m_signerIdentifierType;
        private int m_version;

        public SignedCms() : this(SubjectIdentifierType.IssuerAndSerialNumber, new System.Security.Cryptography.Pkcs.ContentInfo(new Oid("1.2.840.113549.1.7.1"), new byte[0]), false)
        {
        }

        public SignedCms(System.Security.Cryptography.Pkcs.ContentInfo contentInfo) : this(SubjectIdentifierType.IssuerAndSerialNumber, contentInfo, false)
        {
        }

        public SignedCms(SubjectIdentifierType signerIdentifierType) : this(signerIdentifierType, new System.Security.Cryptography.Pkcs.ContentInfo(new Oid("1.2.840.113549.1.7.1"), new byte[0]), false)
        {
        }

        public SignedCms(System.Security.Cryptography.Pkcs.ContentInfo contentInfo, bool detached) : this(SubjectIdentifierType.IssuerAndSerialNumber, contentInfo, detached)
        {
        }

        public SignedCms(SubjectIdentifierType signerIdentifierType, System.Security.Cryptography.Pkcs.ContentInfo contentInfo) : this(signerIdentifierType, contentInfo, false)
        {
        }

        [SecuritySafeCritical]
        public SignedCms(SubjectIdentifierType signerIdentifierType, System.Security.Cryptography.Pkcs.ContentInfo contentInfo, bool detached)
        {
            if (contentInfo == null)
            {
                throw new ArgumentNullException("contentInfo");
            }
            if (contentInfo.Content == null)
            {
                throw new ArgumentNullException("contentInfo.Content");
            }
            if (((signerIdentifierType != SubjectIdentifierType.SubjectKeyIdentifier) && (signerIdentifierType != SubjectIdentifierType.IssuerAndSerialNumber)) && (signerIdentifierType != SubjectIdentifierType.NoSignature))
            {
                signerIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
            }
            this.m_safeCryptMsgHandle = System.Security.Cryptography.SafeCryptMsgHandle.InvalidHandle;
            this.m_signerIdentifierType = signerIdentifierType;
            this.m_version = 0;
            this.m_contentInfo = contentInfo;
            this.m_detached = detached;
        }

        [SecuritySafeCritical]
        public void CheckHash()
        {
            if ((this.m_safeCryptMsgHandle == null) || this.m_safeCryptMsgHandle.IsInvalid)
            {
                throw new InvalidOperationException(SecurityResources.GetResourceString("Cryptography_Cms_MessageNotSigned"));
            }
            CheckHashes(this.SignerInfos);
        }

        private static void CheckHashes(SignerInfoCollection signers)
        {
            if ((signers == null) || (signers.Count < 1))
            {
                throw new CryptographicException(-2146885618);
            }
            SignerInfoEnumerator enumerator = signers.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SignerInfo current = enumerator.Current;
                if (current.SignerIdentifier.Type == SubjectIdentifierType.NoSignature)
                {
                    current.CheckHash();
                }
            }
        }

        public void CheckSignature(bool verifySignatureOnly)
        {
            this.CheckSignature(new X509Certificate2Collection(), verifySignatureOnly);
        }

        [SecuritySafeCritical]
        public void CheckSignature(X509Certificate2Collection extraStore, bool verifySignatureOnly)
        {
            if ((this.m_safeCryptMsgHandle == null) || this.m_safeCryptMsgHandle.IsInvalid)
            {
                throw new InvalidOperationException(SecurityResources.GetResourceString("Cryptography_Cms_MessageNotSigned"));
            }
            if (extraStore == null)
            {
                throw new ArgumentNullException("extraStore");
            }
            CheckSignatures(this.SignerInfos, extraStore, verifySignatureOnly);
        }

        private static void CheckSignatures(SignerInfoCollection signers, X509Certificate2Collection extraStore, bool verifySignatureOnly)
        {
            if ((signers == null) || (signers.Count < 1))
            {
                throw new CryptographicException(-2146885618);
            }
            SignerInfoEnumerator enumerator = signers.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SignerInfo current = enumerator.Current;
                current.CheckSignature(extraStore, verifySignatureOnly);
                if (current.CounterSignerInfos.Count > 0)
                {
                    CheckSignatures(current.CounterSignerInfos, extraStore, verifySignatureOnly);
                }
            }
        }

        public void ComputeSignature()
        {
            this.ComputeSignature(new CmsSigner(this.m_signerIdentifierType), true);
        }

        public void ComputeSignature(CmsSigner signer)
        {
            this.ComputeSignature(signer, true);
        }

        [SecuritySafeCritical]
        public void ComputeSignature(CmsSigner signer, bool silent)
        {
            if (signer == null)
            {
                throw new ArgumentNullException("signer");
            }
            if (this.ContentInfo.Content.Length == 0)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Sign_Empty_Content"));
            }
            if (SubjectIdentifierType.NoSignature == signer.SignerIdentifierType)
            {
                if ((this.m_safeCryptMsgHandle != null) && !this.m_safeCryptMsgHandle.IsInvalid)
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Sign_No_Signature_First_Signer"));
                }
                this.Sign(signer, silent);
            }
            else
            {
                if (signer.Certificate == null)
                {
                    if (silent)
                    {
                        throw new InvalidOperationException(SecurityResources.GetResourceString("Cryptography_Cms_RecipientCertificateNotFound"));
                    }
                    signer.Certificate = PkcsUtils.SelectSignerCertificate();
                }
                if (!signer.Certificate.HasPrivateKey)
                {
                    throw new CryptographicException(-2146893811);
                }
                CspParameters parameters = new CspParameters();
                if (!System.Security.Cryptography.X509Certificates.X509Utils.GetPrivateKeyInfo(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(signer.Certificate), ref parameters))
                {
                    throw new CryptographicException(SafeGetLastWin32Error());
                }
                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Sign | KeyContainerPermissionFlags.Open);
                permission.AccessEntries.Add(accessEntry);
                permission.Demand();
                if ((this.m_safeCryptMsgHandle == null) || this.m_safeCryptMsgHandle.IsInvalid)
                {
                    this.Sign(signer, silent);
                }
                else
                {
                    this.CoSign(signer, silent);
                }
            }
        }

        [SecuritySafeCritical]
        private void CoSign(CmsSigner signer, bool silent)
        {
            using (System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO cmsg_signer_encode_info = PkcsUtils.CreateSignerEncodeInfo(signer, silent))
            {
                System.Security.Cryptography.SafeLocalAllocHandle handle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO))));
                try
                {
                    Marshal.StructureToPtr(cmsg_signer_encode_info, handle.DangerousGetHandle(), false);
                    if (!System.Security.Cryptography.CAPI.CryptMsgControl(this.m_safeCryptMsgHandle, 0, 6, handle.DangerousGetHandle()))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                }
                finally
                {
                    Marshal.DestroyStructure(handle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO));
                    handle.Dispose();
                }
            }
            PkcsUtils.AddCertsToMessage(this.m_safeCryptMsgHandle, this.Certificates, PkcsUtils.CreateBagOfCertificates(signer));
        }

        [SecuritySafeCritical]
        public void Decode(byte[] encodedMessage)
        {
            if (encodedMessage == null)
            {
                throw new ArgumentNullException("encodedMessage");
            }
            if ((this.m_safeCryptMsgHandle != null) && !this.m_safeCryptMsgHandle.IsInvalid)
            {
                this.m_safeCryptMsgHandle.Dispose();
            }
            this.m_safeCryptMsgHandle = OpenToDecode(encodedMessage, this.ContentInfo, this.Detached);
            if (!this.Detached)
            {
                Oid contentType = PkcsUtils.GetContentType(this.m_safeCryptMsgHandle);
                byte[] content = PkcsUtils.GetContent(this.m_safeCryptMsgHandle);
                this.m_contentInfo = new System.Security.Cryptography.Pkcs.ContentInfo(contentType, content);
            }
        }

        [SecuritySafeCritical]
        public byte[] Encode()
        {
            if ((this.m_safeCryptMsgHandle == null) || this.m_safeCryptMsgHandle.IsInvalid)
            {
                throw new InvalidOperationException(SecurityResources.GetResourceString("Cryptography_Cms_MessageNotSigned"));
            }
            return PkcsUtils.GetMessage(this.m_safeCryptMsgHandle);
        }

        [SecurityCritical]
        internal System.Security.Cryptography.SafeCryptMsgHandle GetCryptMsgHandle()
        {
            return this.m_safeCryptMsgHandle;
        }

        [SecuritySafeCritical]
        private static System.Security.Cryptography.SafeCryptMsgHandle OpenToDecode(byte[] encodedMessage, System.Security.Cryptography.Pkcs.ContentInfo contentInfo, bool detached)
        {
            System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg = System.Security.Cryptography.CAPI.CAPISafe.CryptMsgOpenToDecode(0x10001, detached ? 4 : 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if ((hCryptMsg == null) || hCryptMsg.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgUpdate(hCryptMsg, encodedMessage, (uint) encodedMessage.Length, true))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (2 != PkcsUtils.GetMessageType(hCryptMsg))
            {
                throw new CryptographicException(-2146889724);
            }
            if (detached)
            {
                byte[] content = contentInfo.Content;
                if (((content != null) && (content.Length > 0)) && !System.Security.Cryptography.CAPI.CAPISafe.CryptMsgUpdate(hCryptMsg, content, (uint) content.Length, true))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            return hCryptMsg;
        }

        [SecuritySafeCritical]
        public unsafe void RemoveSignature(int index)
        {
            if ((this.m_safeCryptMsgHandle == null) || this.m_safeCryptMsgHandle.IsInvalid)
            {
                throw new InvalidOperationException(SecurityResources.GetResourceString("Cryptography_Cms_MessageNotSigned"));
            }
            uint num = 0;
            uint num2 = (uint) Marshal.SizeOf(typeof(uint));
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(this.m_safeCryptMsgHandle, 5, 0, new IntPtr((void*) &num), new IntPtr((void*) &num2)))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if ((index < 0) || (index >= num))
            {
                throw new ArgumentOutOfRangeException("index", SecurityResources.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (!System.Security.Cryptography.CAPI.CryptMsgControl(this.m_safeCryptMsgHandle, 0, 7, new IntPtr((void*) &index)))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
        }

        [SecuritySafeCritical]
        public void RemoveSignature(SignerInfo signerInfo)
        {
            if (signerInfo == null)
            {
                throw new ArgumentNullException("signerInfo");
            }
            this.RemoveSignature(PkcsUtils.GetSignerIndex(this.m_safeCryptMsgHandle, signerInfo, 0));
        }

        [SecuritySafeCritical]
        internal void ReopenToDecode()
        {
            byte[] message = PkcsUtils.GetMessage(this.m_safeCryptMsgHandle);
            if ((this.m_safeCryptMsgHandle != null) && !this.m_safeCryptMsgHandle.IsInvalid)
            {
                this.m_safeCryptMsgHandle.Dispose();
            }
            this.m_safeCryptMsgHandle = OpenToDecode(message, this.ContentInfo, this.Detached);
        }

        [SecuritySafeCritical]
        private static int SafeGetLastWin32Error()
        {
            return Marshal.GetLastWin32Error();
        }

        [SecuritySafeCritical]
        private unsafe void Sign(CmsSigner signer, bool silent)
        {
            System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg = null;
            System.Security.Cryptography.CAPI.CMSG_SIGNED_ENCODE_INFO cmsg_signed_encode_info = new System.Security.Cryptography.CAPI.CMSG_SIGNED_ENCODE_INFO(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_SIGNED_ENCODE_INFO)));
            System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO structure = PkcsUtils.CreateSignerEncodeInfo(signer, silent);
            byte[] encodedMessage = null;
            try
            {
                System.Security.Cryptography.SafeLocalAllocHandle handle2 = System.Security.Cryptography.CAPI.LocalAlloc(0, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO))));
                try
                {
                    Marshal.StructureToPtr(structure, handle2.DangerousGetHandle(), false);
                    X509Certificate2Collection certificates = PkcsUtils.CreateBagOfCertificates(signer);
                    System.Security.Cryptography.SafeLocalAllocHandle handle3 = PkcsUtils.CreateEncodedCertBlob(certificates);
                    cmsg_signed_encode_info.cSigners = 1;
                    cmsg_signed_encode_info.rgSigners = handle2.DangerousGetHandle();
                    cmsg_signed_encode_info.cCertEncoded = (uint) certificates.Count;
                    if (certificates.Count > 0)
                    {
                        cmsg_signed_encode_info.rgCertEncoded = handle3.DangerousGetHandle();
                    }
                    if (string.Compare(this.ContentInfo.ContentType.Value, "1.2.840.113549.1.7.1", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        hCryptMsg = System.Security.Cryptography.CAPI.CryptMsgOpenToEncode(0x10001, this.Detached ? 4 : 0, 2, new IntPtr((void*) &cmsg_signed_encode_info), IntPtr.Zero, IntPtr.Zero);
                    }
                    else
                    {
                        hCryptMsg = System.Security.Cryptography.CAPI.CryptMsgOpenToEncode(0x10001, this.Detached ? 4 : 0, 2, new IntPtr((void*) &cmsg_signed_encode_info), this.ContentInfo.ContentType.Value, IntPtr.Zero);
                    }
                    if ((hCryptMsg == null) || hCryptMsg.IsInvalid)
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    if ((this.ContentInfo.Content.Length > 0) && !System.Security.Cryptography.CAPI.CAPISafe.CryptMsgUpdate(hCryptMsg, this.ContentInfo.pContent, (uint) this.ContentInfo.Content.Length, true))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    encodedMessage = PkcsUtils.GetContent(hCryptMsg);
                    hCryptMsg.Dispose();
                    handle3.Dispose();
                }
                finally
                {
                    Marshal.DestroyStructure(handle2.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_SIGNER_ENCODE_INFO));
                    handle2.Dispose();
                }
            }
            finally
            {
                structure.Dispose();
            }
            hCryptMsg = OpenToDecode(encodedMessage, this.ContentInfo, this.Detached);
            if ((this.m_safeCryptMsgHandle != null) && !this.m_safeCryptMsgHandle.IsInvalid)
            {
                this.m_safeCryptMsgHandle.Dispose();
            }
            this.m_safeCryptMsgHandle = hCryptMsg;
            GC.KeepAlive(signer);
        }

        public X509Certificate2Collection Certificates
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_safeCryptMsgHandle != null) && !this.m_safeCryptMsgHandle.IsInvalid)
                {
                    return PkcsUtils.GetCertificates(this.m_safeCryptMsgHandle);
                }
                return new X509Certificate2Collection();
            }
        }

        public System.Security.Cryptography.Pkcs.ContentInfo ContentInfo
        {
            get
            {
                return this.m_contentInfo;
            }
        }

        public bool Detached
        {
            get
            {
                return this.m_detached;
            }
        }

        public SignerInfoCollection SignerInfos
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_safeCryptMsgHandle != null) && !this.m_safeCryptMsgHandle.IsInvalid)
                {
                    return new SignerInfoCollection(this);
                }
                return new SignerInfoCollection();
            }
        }

        public int Version
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_safeCryptMsgHandle != null) && !this.m_safeCryptMsgHandle.IsInvalid)
                {
                    return (int) PkcsUtils.GetVersion(this.m_safeCryptMsgHandle);
                }
                return this.m_version;
            }
        }
    }
}

