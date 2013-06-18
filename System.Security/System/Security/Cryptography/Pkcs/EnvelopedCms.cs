namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EnvelopedCms
    {
        private X509Certificate2Collection m_certificates;
        private System.Security.Cryptography.Pkcs.ContentInfo m_contentInfo;
        private AlgorithmIdentifier m_encryptionAlgorithm;
        private SubjectIdentifierType m_recipientIdentifierType;
        [SecurityCritical]
        private System.Security.Cryptography.SafeCryptMsgHandle m_safeCryptMsgHandle;
        private CryptographicAttributeObjectCollection m_unprotectedAttributes;
        private int m_version;

        public EnvelopedCms() : this(SubjectIdentifierType.IssuerAndSerialNumber, new System.Security.Cryptography.Pkcs.ContentInfo("1.2.840.113549.1.7.1", new byte[0]), new AlgorithmIdentifier("1.2.840.113549.3.7"))
        {
        }

        public EnvelopedCms(System.Security.Cryptography.Pkcs.ContentInfo contentInfo) : this(SubjectIdentifierType.IssuerAndSerialNumber, contentInfo, new AlgorithmIdentifier("1.2.840.113549.3.7"))
        {
        }

        public EnvelopedCms(System.Security.Cryptography.Pkcs.ContentInfo contentInfo, AlgorithmIdentifier encryptionAlgorithm) : this(SubjectIdentifierType.IssuerAndSerialNumber, contentInfo, encryptionAlgorithm)
        {
        }

        public EnvelopedCms(SubjectIdentifierType recipientIdentifierType, System.Security.Cryptography.Pkcs.ContentInfo contentInfo) : this(recipientIdentifierType, contentInfo, new AlgorithmIdentifier("1.2.840.113549.3.7"))
        {
        }

        [SecuritySafeCritical]
        public EnvelopedCms(SubjectIdentifierType recipientIdentifierType, System.Security.Cryptography.Pkcs.ContentInfo contentInfo, AlgorithmIdentifier encryptionAlgorithm)
        {
            if (contentInfo == null)
            {
                throw new ArgumentNullException("contentInfo");
            }
            if (contentInfo.Content == null)
            {
                throw new ArgumentNullException("contentInfo.Content");
            }
            if (encryptionAlgorithm == null)
            {
                throw new ArgumentNullException("encryptionAlgorithm");
            }
            this.m_safeCryptMsgHandle = System.Security.Cryptography.SafeCryptMsgHandle.InvalidHandle;
            this.m_version = (recipientIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier) ? 2 : 0;
            this.m_recipientIdentifierType = recipientIdentifierType;
            this.m_contentInfo = contentInfo;
            this.m_encryptionAlgorithm = encryptionAlgorithm;
            this.m_encryptionAlgorithm.Parameters = new byte[0];
            this.m_certificates = new X509Certificate2Collection();
            this.m_unprotectedAttributes = new CryptographicAttributeObjectCollection();
        }

        [SecurityCritical]
        private static System.Security.Cryptography.SafeCertStoreHandle BuildDecryptorStore(X509Certificate2Collection extraStore)
        {
            X509Certificate2Collection collection = new X509Certificate2Collection();
            try
            {
                X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
                store.Open(OpenFlags.IncludeArchived | OpenFlags.OpenExistingOnly);
                collection.AddRange(store.Certificates);
            }
            catch (SecurityException)
            {
            }
            try
            {
                X509Store store2 = new X509Store("MY", StoreLocation.LocalMachine);
                store2.Open(OpenFlags.IncludeArchived | OpenFlags.OpenExistingOnly);
                collection.AddRange(store2.Certificates);
            }
            catch (SecurityException)
            {
            }
            if (extraStore != null)
            {
                collection.AddRange(extraStore);
            }
            if (collection.Count == 0)
            {
                throw new CryptographicException(-2146889717);
            }
            return System.Security.Cryptography.X509Certificates.X509Utils.ExportToMemoryStore(collection);
        }

        [SecurityCritical]
        private static System.Security.Cryptography.SafeCertStoreHandle BuildOriginatorStore(X509Certificate2Collection bagOfCerts, X509Certificate2Collection extraStore)
        {
            X509Certificate2Collection collection = new X509Certificate2Collection();
            try
            {
                X509Store store = new X509Store("AddressBook", StoreLocation.CurrentUser);
                store.Open(OpenFlags.IncludeArchived | OpenFlags.OpenExistingOnly);
                collection.AddRange(store.Certificates);
            }
            catch (SecurityException)
            {
            }
            try
            {
                X509Store store2 = new X509Store("AddressBook", StoreLocation.LocalMachine);
                store2.Open(OpenFlags.IncludeArchived | OpenFlags.OpenExistingOnly);
                collection.AddRange(store2.Certificates);
            }
            catch (SecurityException)
            {
            }
            if (bagOfCerts != null)
            {
                collection.AddRange(bagOfCerts);
            }
            if (extraStore != null)
            {
                collection.AddRange(extraStore);
            }
            if (collection.Count == 0)
            {
                throw new CryptographicException(-2146885628);
            }
            return System.Security.Cryptography.X509Certificates.X509Utils.ExportToMemoryStore(collection);
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
            this.m_safeCryptMsgHandle = OpenToDecode(encodedMessage);
            this.m_version = (int) PkcsUtils.GetVersion(this.m_safeCryptMsgHandle);
            Oid contentType = PkcsUtils.GetContentType(this.m_safeCryptMsgHandle);
            byte[] content = PkcsUtils.GetContent(this.m_safeCryptMsgHandle);
            this.m_contentInfo = new System.Security.Cryptography.Pkcs.ContentInfo(contentType, content);
            this.m_encryptionAlgorithm = PkcsUtils.GetAlgorithmIdentifier(this.m_safeCryptMsgHandle);
            this.m_certificates = PkcsUtils.GetCertificates(this.m_safeCryptMsgHandle);
            this.m_unprotectedAttributes = PkcsUtils.GetUnprotectedAttributes(this.m_safeCryptMsgHandle);
        }

        public void Decrypt()
        {
            this.DecryptContent(this.RecipientInfos, null);
        }

        public void Decrypt(RecipientInfo recipientInfo)
        {
            if (recipientInfo == null)
            {
                throw new ArgumentNullException("recipientInfo");
            }
            this.DecryptContent(new RecipientInfoCollection(recipientInfo), null);
        }

        public void Decrypt(X509Certificate2Collection extraStore)
        {
            if (extraStore == null)
            {
                throw new ArgumentNullException("extraStore");
            }
            this.DecryptContent(this.RecipientInfos, extraStore);
        }

        public void Decrypt(RecipientInfo recipientInfo, X509Certificate2Collection extraStore)
        {
            if (recipientInfo == null)
            {
                throw new ArgumentNullException("recipientInfo");
            }
            if (extraStore == null)
            {
                throw new ArgumentNullException("extraStore");
            }
            this.DecryptContent(new RecipientInfoCollection(recipientInfo), extraStore);
        }

        [SecuritySafeCritical]
        private unsafe void DecryptContent(RecipientInfoCollection recipientInfos, X509Certificate2Collection extraStore)
        {
            int hr = -2146889717;
            if ((this.m_safeCryptMsgHandle == null) || this.m_safeCryptMsgHandle.IsInvalid)
            {
                throw new InvalidOperationException(SecurityResources.GetResourceString("Cryptography_Cms_NoEncryptedMessageToEncode"));
            }
            for (int i = 0; i < recipientInfos.Count; i++)
            {
                System.Security.Cryptography.SafeCertContextHandle invalidHandle;
                KeyAgreeRecipientInfo info2;
                System.Security.Cryptography.CAPI.CMSG_CTRL_KEY_AGREE_DECRYPT_PARA cmsg_ctrl_key_agree_decrypt_para;
                System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO cmsg_key_agree_public_key_recipient_info;
                RecipientInfo recipientInfo = recipientInfos[i];
                CMSG_DECRYPT_PARAM cmsgDecryptParam = new CMSG_DECRYPT_PARAM();
                int num3 = GetCspParams(recipientInfo, extraStore, ref cmsgDecryptParam);
                if (num3 != 0)
                {
                    goto Label_02F1;
                }
                CspParameters parameters = new CspParameters();
                if (!System.Security.Cryptography.X509Certificates.X509Utils.GetPrivateKeyInfo(cmsgDecryptParam.safeCertContextHandle, ref parameters))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Decrypt | KeyContainerPermissionFlags.Open);
                permission.AccessEntries.Add(accessEntry);
                permission.Demand();
                switch (recipientInfo.Type)
                {
                    case RecipientInfoType.KeyTransport:
                    {
                        System.Security.Cryptography.CAPI.CMSG_CTRL_DECRYPT_PARA cmsg_ctrl_decrypt_para = new System.Security.Cryptography.CAPI.CMSG_CTRL_DECRYPT_PARA(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_CTRL_DECRYPT_PARA))) {
                            hCryptProv = cmsgDecryptParam.safeCryptProvHandle.DangerousGetHandle(),
                            dwKeySpec = cmsgDecryptParam.keySpec,
                            dwRecipientIndex = recipientInfo.Index
                        };
                        if (!System.Security.Cryptography.CAPI.CryptMsgControl(this.m_safeCryptMsgHandle, 0, 2, new IntPtr((void*) &cmsg_ctrl_decrypt_para)))
                        {
                            num3 = Marshal.GetHRForLastWin32Error();
                        }
                        GC.KeepAlive(cmsg_ctrl_decrypt_para);
                        goto Label_02E6;
                    }
                    case RecipientInfoType.KeyAgreement:
                    {
                        invalidHandle = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
                        info2 = (KeyAgreeRecipientInfo) recipientInfo;
                        System.Security.Cryptography.CAPI.CMSG_CMS_RECIPIENT_INFO cmsg_cms_recipient_info = (System.Security.Cryptography.CAPI.CMSG_CMS_RECIPIENT_INFO) Marshal.PtrToStructure(info2.pCmsgRecipientInfo.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_CMS_RECIPIENT_INFO));
                        cmsg_ctrl_key_agree_decrypt_para = new System.Security.Cryptography.CAPI.CMSG_CTRL_KEY_AGREE_DECRYPT_PARA(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_CTRL_KEY_AGREE_DECRYPT_PARA))) {
                            hCryptProv = cmsgDecryptParam.safeCryptProvHandle.DangerousGetHandle(),
                            dwKeySpec = cmsgDecryptParam.keySpec,
                            pKeyAgree = cmsg_cms_recipient_info.pRecipientInfo,
                            dwRecipientIndex = info2.Index,
                            dwRecipientEncryptedKeyIndex = info2.SubIndex
                        };
                        if (info2.SubType != RecipientSubType.CertIdKeyAgreement)
                        {
                            goto Label_0286;
                        }
                        System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO cmsgRecipientInfo = (System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO) info2.CmsgRecipientInfo;
                        invalidHandle = System.Security.Cryptography.CAPI.CertFindCertificateInStore(BuildOriginatorStore(this.Certificates, extraStore), 0x10001, 0, 0x100000, new IntPtr((void*) &cmsgRecipientInfo.OriginatorCertId), System.Security.Cryptography.SafeCertContextHandle.InvalidHandle);
                        if ((invalidHandle != null) && !invalidHandle.IsInvalid)
                        {
                            break;
                        }
                        num3 = -2146885628;
                        goto Label_02E6;
                    }
                    default:
                        throw new CryptographicException(-2147483647);
                }
                System.Security.Cryptography.CAPI.CERT_CONTEXT cert_context = (System.Security.Cryptography.CAPI.CERT_CONTEXT) Marshal.PtrToStructure(invalidHandle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CERT_CONTEXT));
                System.Security.Cryptography.CAPI.CERT_INFO cert_info = (System.Security.Cryptography.CAPI.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(System.Security.Cryptography.CAPI.CERT_INFO));
                cmsg_ctrl_key_agree_decrypt_para.OriginatorPublicKey = cert_info.SubjectPublicKeyInfo.PublicKey;
                goto Label_02A7;
            Label_0286:
                cmsg_key_agree_public_key_recipient_info = (System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO) info2.CmsgRecipientInfo;
                cmsg_ctrl_key_agree_decrypt_para.OriginatorPublicKey = cmsg_key_agree_public_key_recipient_info.OriginatorPublicKeyInfo.PublicKey;
            Label_02A7:
                if (!System.Security.Cryptography.CAPI.CryptMsgControl(this.m_safeCryptMsgHandle, 0, 0x11, new IntPtr((void*) &cmsg_ctrl_key_agree_decrypt_para)))
                {
                    num3 = Marshal.GetHRForLastWin32Error();
                }
                GC.KeepAlive(cmsg_ctrl_key_agree_decrypt_para);
                GC.KeepAlive(invalidHandle);
            Label_02E6:
                GC.KeepAlive(cmsgDecryptParam);
            Label_02F1:
                if (num3 == 0)
                {
                    uint cbData = 0;
                    System.Security.Cryptography.SafeLocalAllocHandle pvData = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
                    PkcsUtils.GetParam(this.m_safeCryptMsgHandle, 2, 0, out pvData, out cbData);
                    if (cbData > 0)
                    {
                        Oid contentType = PkcsUtils.GetContentType(this.m_safeCryptMsgHandle);
                        byte[] destination = new byte[cbData];
                        Marshal.Copy(pvData.DangerousGetHandle(), destination, 0, (int) cbData);
                        this.m_contentInfo = new System.Security.Cryptography.Pkcs.ContentInfo(contentType, destination);
                    }
                    pvData.Dispose();
                    hr = 0;
                    break;
                }
                hr = num3;
            }
            if (hr != 0)
            {
                throw new CryptographicException(hr);
            }
        }

        [SecuritySafeCritical]
        public byte[] Encode()
        {
            if ((this.m_safeCryptMsgHandle == null) || this.m_safeCryptMsgHandle.IsInvalid)
            {
                throw new InvalidOperationException(SecurityResources.GetResourceString("Cryptography_Cms_MessageNotEncrypted"));
            }
            return PkcsUtils.GetContent(this.m_safeCryptMsgHandle);
        }

        public void Encrypt()
        {
            this.Encrypt(new CmsRecipientCollection());
        }

        public void Encrypt(CmsRecipient recipient)
        {
            if (recipient == null)
            {
                throw new ArgumentNullException("recipient");
            }
            this.Encrypt(new CmsRecipientCollection(recipient));
        }

        public void Encrypt(CmsRecipientCollection recipients)
        {
            if (recipients == null)
            {
                throw new ArgumentNullException("recipients");
            }
            if (this.ContentInfo.Content.Length == 0)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Envelope_Empty_Content"));
            }
            if (recipients.Count == 0)
            {
                recipients = PkcsUtils.SelectRecipients(this.m_recipientIdentifierType);
            }
            this.EncryptContent(recipients);
        }

        [SecuritySafeCritical]
        private unsafe void EncryptContent(CmsRecipientCollection recipients)
        {
            CMSG_ENCRYPT_PARAM encryptParam = new CMSG_ENCRYPT_PARAM();
            if (recipients.Count < 1)
            {
                throw new CryptographicException(-2146889717);
            }
            CmsRecipientEnumerator enumerator = recipients.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CmsRecipient current = enumerator.Current;
                if (current.Certificate == null)
                {
                    throw new ArgumentNullException(SecurityResources.GetResourceString("Cryptography_Cms_RecipientCertificateNotFound"));
                }
                if ((PkcsUtils.GetRecipientInfoType(current.Certificate) == RecipientInfoType.KeyAgreement) || (current.RecipientIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier))
                {
                    encryptParam.useCms = true;
                }
            }
            if (!encryptParam.useCms && ((this.Certificates.Count > 0) || (this.UnprotectedAttributes.Count > 0)))
            {
                encryptParam.useCms = true;
            }
            if (encryptParam.useCms && !PkcsUtils.CmsSupported())
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Not_Supported"));
            }
            System.Security.Cryptography.CAPI.CMSG_ENVELOPED_ENCODE_INFO structure = new System.Security.Cryptography.CAPI.CMSG_ENVELOPED_ENCODE_INFO(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_ENVELOPED_ENCODE_INFO)));
            System.Security.Cryptography.SafeLocalAllocHandle handle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_ENVELOPED_ENCODE_INFO))));
            SetCspParams(this.ContentEncryptionAlgorithm, ref encryptParam);
            structure.ContentEncryptionAlgorithm.pszObjId = this.ContentEncryptionAlgorithm.Oid.Value;
            if ((encryptParam.pvEncryptionAuxInfo != null) && !encryptParam.pvEncryptionAuxInfo.IsInvalid)
            {
                structure.pvEncryptionAuxInfo = encryptParam.pvEncryptionAuxInfo.DangerousGetHandle();
            }
            structure.cRecipients = (uint) recipients.Count;
            List<System.Security.Cryptography.SafeCertContextHandle> certContexts = null;
            if (encryptParam.useCms)
            {
                SetCmsRecipientParams(recipients, this.Certificates, this.UnprotectedAttributes, this.ContentEncryptionAlgorithm, ref encryptParam);
                structure.rgCmsRecipients = encryptParam.rgpRecipients.DangerousGetHandle();
                if ((encryptParam.rgCertEncoded != null) && !encryptParam.rgCertEncoded.IsInvalid)
                {
                    structure.cCertEncoded = (uint) this.Certificates.Count;
                    structure.rgCertEncoded = encryptParam.rgCertEncoded.DangerousGetHandle();
                }
                if ((encryptParam.rgUnprotectedAttr != null) && !encryptParam.rgUnprotectedAttr.IsInvalid)
                {
                    structure.cUnprotectedAttr = (uint) this.UnprotectedAttributes.Count;
                    structure.rgUnprotectedAttr = encryptParam.rgUnprotectedAttr.DangerousGetHandle();
                }
            }
            else
            {
                SetPkcs7RecipientParams(recipients, ref encryptParam, out certContexts);
                structure.rgpRecipients = encryptParam.rgpRecipients.DangerousGetHandle();
            }
            Marshal.StructureToPtr(structure, handle.DangerousGetHandle(), false);
            try
            {
                System.Security.Cryptography.SafeCryptMsgHandle handle2 = System.Security.Cryptography.CAPI.CryptMsgOpenToEncode(0x10001, 0, 3, handle.DangerousGetHandle(), this.ContentInfo.ContentType.Value, IntPtr.Zero);
                if ((handle2 == null) || handle2.IsInvalid)
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                if ((this.m_safeCryptMsgHandle != null) && !this.m_safeCryptMsgHandle.IsInvalid)
                {
                    this.m_safeCryptMsgHandle.Dispose();
                }
                this.m_safeCryptMsgHandle = handle2;
            }
            finally
            {
                Marshal.DestroyStructure(handle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_ENVELOPED_ENCODE_INFO));
                handle.Dispose();
            }
            byte[] encodedData = new byte[0];
            if (string.Compare(this.ContentInfo.ContentType.Value, "1.2.840.113549.1.7.1", StringComparison.OrdinalIgnoreCase) == 0)
            {
                byte[] content = this.ContentInfo.Content;
                fixed (byte* numRef = content)
                {
                    System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB cryptoapi_blob = new System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB {
                        cbData = (uint) content.Length,
                        pbData = new IntPtr((void*) numRef)
                    };
                    if (!System.Security.Cryptography.CAPI.EncodeObject(new IntPtr(0x19L), new IntPtr((void*) &cryptoapi_blob), out encodedData))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                }
            }
            else
            {
                encodedData = this.ContentInfo.Content;
            }
            if ((encodedData.Length > 0) && !System.Security.Cryptography.CAPI.CAPISafe.CryptMsgUpdate(this.m_safeCryptMsgHandle, encodedData, (uint) encodedData.Length, true))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            GC.KeepAlive(encryptParam);
            GC.KeepAlive(recipients);
            GC.KeepAlive(certContexts);
        }

        [SecurityCritical]
        private static unsafe int GetCspParams(RecipientInfo recipientInfo, X509Certificate2Collection extraStore, ref CMSG_DECRYPT_PARAM cmsgDecryptParam)
        {
            int num = -2146889717;
            System.Security.Cryptography.SafeCertContextHandle invalidHandle = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
            System.Security.Cryptography.SafeCertStoreHandle hCertStore = BuildDecryptorStore(extraStore);
            switch (recipientInfo.Type)
            {
                case RecipientInfoType.KeyTransport:
                    if (recipientInfo.SubType != RecipientSubType.Pkcs7KeyTransport)
                    {
                        System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO cmsgRecipientInfo = (System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO) recipientInfo.CmsgRecipientInfo;
                        invalidHandle = System.Security.Cryptography.CAPI.CertFindCertificateInStore(hCertStore, 0x10001, 0, 0x100000, new IntPtr((void*) &cmsgRecipientInfo.RecipientId), System.Security.Cryptography.SafeCertContextHandle.InvalidHandle);
                        break;
                    }
                    invalidHandle = System.Security.Cryptography.CAPI.CertFindCertificateInStore(hCertStore, 0x10001, 0, 0xb0000, recipientInfo.pCmsgRecipientInfo.DangerousGetHandle(), System.Security.Cryptography.SafeCertContextHandle.InvalidHandle);
                    break;

                case RecipientInfoType.KeyAgreement:
                {
                    KeyAgreeRecipientInfo info = (KeyAgreeRecipientInfo) recipientInfo;
                    System.Security.Cryptography.CAPI.CERT_ID recipientId = info.RecipientId;
                    invalidHandle = System.Security.Cryptography.CAPI.CertFindCertificateInStore(hCertStore, 0x10001, 0, 0x100000, new IntPtr((void*) &recipientId), System.Security.Cryptography.SafeCertContextHandle.InvalidHandle);
                    break;
                }
                default:
                    num = -2147483647;
                    break;
            }
            if ((invalidHandle == null) || invalidHandle.IsInvalid)
            {
                return num;
            }
            System.Security.Cryptography.SafeCryptProvHandle hCryptProv = System.Security.Cryptography.SafeCryptProvHandle.InvalidHandle;
            uint pdwKeySpec = 0;
            bool pfCallerFreeProv = false;
            CspParameters parameters = new CspParameters();
            if (!System.Security.Cryptography.X509Certificates.X509Utils.GetPrivateKeyInfo(invalidHandle, ref parameters))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if ((string.Compare(parameters.ProviderName, "Microsoft Base Cryptographic Provider v1.0", StringComparison.OrdinalIgnoreCase) == 0) && (System.Security.Cryptography.CAPI.CryptAcquireContext(ref hCryptProv, parameters.KeyContainerName, "Microsoft Enhanced Cryptographic Provider v1.0", 1, 0) || System.Security.Cryptography.CAPI.CryptAcquireContext(ref hCryptProv, parameters.KeyContainerName, "Microsoft Strong Cryptographic Provider", 1, 0)))
            {
                cmsgDecryptParam.safeCryptProvHandle = hCryptProv;
            }
            cmsgDecryptParam.safeCertContextHandle = invalidHandle;
            cmsgDecryptParam.keySpec = (uint) parameters.KeyNumber;
            num = 0;
            if ((hCryptProv != null) && !hCryptProv.IsInvalid)
            {
                return num;
            }
            if (System.Security.Cryptography.CAPI.CAPISafe.CryptAcquireCertificatePrivateKey(invalidHandle, 6, IntPtr.Zero, ref hCryptProv, ref pdwKeySpec, ref pfCallerFreeProv))
            {
                if (!pfCallerFreeProv)
                {
                    GC.SuppressFinalize(hCryptProv);
                }
                cmsgDecryptParam.safeCryptProvHandle = hCryptProv;
                return num;
            }
            return Marshal.GetHRForLastWin32Error();
        }

        [SecuritySafeCritical]
        private static System.Security.Cryptography.SafeCryptMsgHandle OpenToDecode(byte[] encodedMessage)
        {
            System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg = null;
            hCryptMsg = System.Security.Cryptography.CAPI.CAPISafe.CryptMsgOpenToDecode(0x10001, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if ((hCryptMsg == null) || hCryptMsg.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgUpdate(hCryptMsg, encodedMessage, (uint) encodedMessage.Length, true))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (3 != PkcsUtils.GetMessageType(hCryptMsg))
            {
                throw new CryptographicException(-2146889724);
            }
            return hCryptMsg;
        }

        [SecurityCritical]
        private static unsafe void SetCmsRecipientParams(CmsRecipientCollection recipients, X509Certificate2Collection certificates, CryptographicAttributeObjectCollection unprotectedAttributes, AlgorithmIdentifier contentEncryptionAlgorithm, ref CMSG_ENCRYPT_PARAM encryptParam)
        {
            int index = 0;
            uint[] numArray = new uint[recipients.Count];
            int num2 = 0;
            int num3 = recipients.Count * Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCODE_INFO));
            int num4 = num3;
            for (index = 0; index < recipients.Count; index++)
            {
                numArray[index] = (uint) PkcsUtils.GetRecipientInfoType(recipients[index].Certificate);
                if (numArray[index] == 1)
                {
                    num4 += Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_ENCODE_INFO));
                }
                else
                {
                    if (numArray[index] != 2)
                    {
                        throw new CryptographicException(-2146889726);
                    }
                    num2++;
                    num4 += Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO));
                }
            }
            encryptParam.rgpRecipients = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(num4));
            encryptParam.rgCertEncoded = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            encryptParam.rgUnprotectedAttr = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            encryptParam.rgSubjectKeyIdentifier = new System.Security.Cryptography.SafeLocalAllocHandle[recipients.Count];
            encryptParam.rgszObjId = new System.Security.Cryptography.SafeLocalAllocHandle[recipients.Count];
            if (num2 > 0)
            {
                encryptParam.rgszKeyWrapObjId = new System.Security.Cryptography.SafeLocalAllocHandle[num2];
                encryptParam.rgKeyWrapAuxInfo = new System.Security.Cryptography.SafeLocalAllocHandle[num2];
                encryptParam.rgEphemeralIdentifier = new System.Security.Cryptography.SafeLocalAllocHandle[num2];
                encryptParam.rgszEphemeralObjId = new System.Security.Cryptography.SafeLocalAllocHandle[num2];
                encryptParam.rgUserKeyingMaterial = new System.Security.Cryptography.SafeLocalAllocHandle[num2];
                encryptParam.prgpEncryptedKey = new System.Security.Cryptography.SafeLocalAllocHandle[num2];
                encryptParam.rgpEncryptedKey = new System.Security.Cryptography.SafeLocalAllocHandle[num2];
            }
            if (certificates.Count > 0)
            {
                encryptParam.rgCertEncoded = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(certificates.Count * Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB))));
                for (index = 0; index < certificates.Count; index++)
                {
                    System.Security.Cryptography.CAPI.CERT_CONTEXT cert_context = (System.Security.Cryptography.CAPI.CERT_CONTEXT) Marshal.PtrToStructure(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificates[index]).DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CERT_CONTEXT));
                    System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB* cryptoapi_blobPtr = (System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB*) new IntPtr(((long) encryptParam.rgCertEncoded.DangerousGetHandle()) + (index * Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB))));
                    cryptoapi_blobPtr->cbData = cert_context.cbCertEncoded;
                    cryptoapi_blobPtr->pbData = cert_context.pbCertEncoded;
                }
            }
            if (unprotectedAttributes.Count > 0)
            {
                encryptParam.rgUnprotectedAttr = new System.Security.Cryptography.SafeLocalAllocHandle(PkcsUtils.CreateCryptAttributes(unprotectedAttributes));
            }
            num2 = 0;
            IntPtr ptr = new IntPtr(((long) encryptParam.rgpRecipients.DangerousGetHandle()) + num3);
            for (index = 0; index < recipients.Count; index++)
            {
                CmsRecipient recipient = recipients[index];
                X509Certificate2 certificate = recipient.Certificate;
                System.Security.Cryptography.CAPI.CERT_CONTEXT cert_context2 = (System.Security.Cryptography.CAPI.CERT_CONTEXT) Marshal.PtrToStructure(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate).DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CERT_CONTEXT));
                System.Security.Cryptography.CAPI.CERT_INFO cert_info = (System.Security.Cryptography.CAPI.CERT_INFO) Marshal.PtrToStructure(cert_context2.pCertInfo, typeof(System.Security.Cryptography.CAPI.CERT_INFO));
                System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCODE_INFO* cmsg_recipient_encode_infoPtr = (System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCODE_INFO*) new IntPtr(((long) encryptParam.rgpRecipients.DangerousGetHandle()) + (index * Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCODE_INFO))));
                cmsg_recipient_encode_infoPtr->dwRecipientChoice = numArray[index];
                cmsg_recipient_encode_infoPtr->pRecipientInfo = ptr;
                if (numArray[index] == 1)
                {
                    IntPtr ptr2 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_ENCODE_INFO), "cbSize")));
                    Marshal.WriteInt32(ptr2, Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_ENCODE_INFO)));
                    IntPtr ptr3 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_ENCODE_INFO), "KeyEncryptionAlgorithm")));
                    byte[] bytes = Encoding.ASCII.GetBytes(cert_info.SubjectPublicKeyInfo.Algorithm.pszObjId);
                    encryptParam.rgszObjId[index] = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(bytes.Length + 1));
                    Marshal.Copy(bytes, 0, encryptParam.rgszObjId[index].DangerousGetHandle(), bytes.Length);
                    IntPtr ptr4 = new IntPtr(((long) ptr3) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER), "pszObjId")));
                    Marshal.WriteIntPtr(ptr4, encryptParam.rgszObjId[index].DangerousGetHandle());
                    IntPtr ptr5 = new IntPtr(((long) ptr3) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER), "Parameters")));
                    IntPtr ptr6 = new IntPtr(((long) ptr5) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "cbData")));
                    Marshal.WriteInt32(ptr6, (int) cert_info.SubjectPublicKeyInfo.Algorithm.Parameters.cbData);
                    IntPtr ptr7 = new IntPtr(((long) ptr5) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "pbData")));
                    Marshal.WriteIntPtr(ptr7, cert_info.SubjectPublicKeyInfo.Algorithm.Parameters.pbData);
                    IntPtr ptr8 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_ENCODE_INFO), "RecipientPublicKey")));
                    ptr6 = new IntPtr(((long) ptr8) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_BIT_BLOB), "cbData")));
                    Marshal.WriteInt32(ptr6, (int) cert_info.SubjectPublicKeyInfo.PublicKey.cbData);
                    ptr7 = new IntPtr(((long) ptr8) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_BIT_BLOB), "pbData")));
                    Marshal.WriteIntPtr(ptr7, cert_info.SubjectPublicKeyInfo.PublicKey.pbData);
                    IntPtr ptr9 = new IntPtr(((long) ptr8) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_BIT_BLOB), "cUnusedBits")));
                    Marshal.WriteInt32(ptr9, (int) cert_info.SubjectPublicKeyInfo.PublicKey.cUnusedBits);
                    IntPtr ptr10 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_ENCODE_INFO), "RecipientId")));
                    if (recipient.RecipientIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier)
                    {
                        uint pcbData = 0;
                        System.Security.Cryptography.SafeLocalAllocHandle invalidHandle = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
                        if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateContextProperty(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate), 20, invalidHandle, ref pcbData))
                        {
                            throw new CryptographicException(Marshal.GetLastWin32Error());
                        }
                        invalidHandle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) pcbData));
                        if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateContextProperty(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate), 20, invalidHandle, ref pcbData))
                        {
                            throw new CryptographicException(Marshal.GetLastWin32Error());
                        }
                        encryptParam.rgSubjectKeyIdentifier[index] = invalidHandle;
                        IntPtr ptr11 = new IntPtr(((long) ptr10) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ID), "dwIdChoice")));
                        Marshal.WriteInt32(ptr11, 2);
                        IntPtr ptr12 = new IntPtr(((long) ptr10) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ID), "Value")));
                        ptr6 = new IntPtr(((long) ptr12) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "cbData")));
                        Marshal.WriteInt32(ptr6, (int) pcbData);
                        ptr7 = new IntPtr(((long) ptr12) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "pbData")));
                        Marshal.WriteIntPtr(ptr7, invalidHandle.DangerousGetHandle());
                    }
                    else
                    {
                        IntPtr ptr13 = new IntPtr(((long) ptr10) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ID), "dwIdChoice")));
                        Marshal.WriteInt32(ptr13, 1);
                        IntPtr ptr14 = new IntPtr(((long) ptr10) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ID), "Value")));
                        IntPtr ptr15 = new IntPtr(((long) ptr14) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ISSUER_SERIAL_NUMBER), "Issuer")));
                        ptr6 = new IntPtr(((long) ptr15) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "cbData")));
                        Marshal.WriteInt32(ptr6, (int) cert_info.Issuer.cbData);
                        ptr7 = new IntPtr(((long) ptr15) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "pbData")));
                        Marshal.WriteIntPtr(ptr7, cert_info.Issuer.pbData);
                        IntPtr ptr16 = new IntPtr(((long) ptr14) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ISSUER_SERIAL_NUMBER), "SerialNumber")));
                        ptr6 = new IntPtr(((long) ptr16) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "cbData")));
                        Marshal.WriteInt32(ptr6, (int) cert_info.SerialNumber.cbData);
                        ptr7 = new IntPtr(((long) ptr16) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "pbData")));
                        Marshal.WriteIntPtr(ptr7, cert_info.SerialNumber.pbData);
                    }
                    ptr = new IntPtr(((long) ptr) + Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_ENCODE_INFO)));
                }
                else if (numArray[index] == 2)
                {
                    IntPtr ptr17 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO), "cbSize")));
                    Marshal.WriteInt32(ptr17, Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO)));
                    IntPtr ptr18 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO), "KeyEncryptionAlgorithm")));
                    byte[] source = Encoding.ASCII.GetBytes("1.2.840.113549.1.9.16.3.5");
                    encryptParam.rgszObjId[index] = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(source.Length + 1));
                    Marshal.Copy(source, 0, encryptParam.rgszObjId[index].DangerousGetHandle(), source.Length);
                    IntPtr ptr19 = new IntPtr(((long) ptr18) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER), "pszObjId")));
                    Marshal.WriteIntPtr(ptr19, encryptParam.rgszObjId[index].DangerousGetHandle());
                    IntPtr ptr20 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO), "KeyWrapAlgorithm")));
                    uint num6 = System.Security.Cryptography.X509Certificates.X509Utils.OidToAlgId(contentEncryptionAlgorithm.Oid.Value);
                    if (num6 == 0x6602)
                    {
                        source = Encoding.ASCII.GetBytes("1.2.840.113549.1.9.16.3.7");
                    }
                    else
                    {
                        source = Encoding.ASCII.GetBytes("1.2.840.113549.1.9.16.3.6");
                    }
                    encryptParam.rgszKeyWrapObjId[num2] = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(source.Length + 1));
                    Marshal.Copy(source, 0, encryptParam.rgszKeyWrapObjId[num2].DangerousGetHandle(), source.Length);
                    ptr19 = new IntPtr(((long) ptr20) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER), "pszObjId")));
                    Marshal.WriteIntPtr(ptr19, encryptParam.rgszKeyWrapObjId[num2].DangerousGetHandle());
                    if (num6 == 0x6602)
                    {
                        IntPtr ptr21 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO), "pvKeyWrapAuxInfo")));
                        Marshal.WriteIntPtr(ptr21, encryptParam.pvEncryptionAuxInfo.DangerousGetHandle());
                    }
                    IntPtr ptr22 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO), "dwKeyChoice")));
                    Marshal.WriteInt32(ptr22, 1);
                    IntPtr ptr23 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO), "pEphemeralAlgorithmOrSenderId")));
                    encryptParam.rgEphemeralIdentifier[num2] = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER))));
                    Marshal.WriteIntPtr(ptr23, encryptParam.rgEphemeralIdentifier[num2].DangerousGetHandle());
                    source = Encoding.ASCII.GetBytes(cert_info.SubjectPublicKeyInfo.Algorithm.pszObjId);
                    encryptParam.rgszEphemeralObjId[num2] = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(source.Length + 1));
                    Marshal.Copy(source, 0, encryptParam.rgszEphemeralObjId[num2].DangerousGetHandle(), source.Length);
                    ptr19 = new IntPtr(((long) encryptParam.rgEphemeralIdentifier[num2].DangerousGetHandle()) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER), "pszObjId")));
                    Marshal.WriteIntPtr(ptr19, encryptParam.rgszEphemeralObjId[num2].DangerousGetHandle());
                    IntPtr ptr24 = new IntPtr(((long) encryptParam.rgEphemeralIdentifier[num2].DangerousGetHandle()) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ALGORITHM_IDENTIFIER), "Parameters")));
                    IntPtr ptr25 = new IntPtr(((long) ptr24) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "cbData")));
                    Marshal.WriteInt32(ptr25, (int) cert_info.SubjectPublicKeyInfo.Algorithm.Parameters.cbData);
                    IntPtr ptr26 = new IntPtr(((long) ptr24) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "pbData")));
                    Marshal.WriteIntPtr(ptr26, cert_info.SubjectPublicKeyInfo.Algorithm.Parameters.pbData);
                    IntPtr ptr27 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO), "cRecipientEncryptedKeys")));
                    Marshal.WriteInt32(ptr27, 1);
                    encryptParam.prgpEncryptedKey[num2] = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(IntPtr))));
                    IntPtr ptr28 = new IntPtr(((long) ptr) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO), "rgpRecipientEncryptedKeys")));
                    Marshal.WriteIntPtr(ptr28, encryptParam.prgpEncryptedKey[num2].DangerousGetHandle());
                    encryptParam.rgpEncryptedKey[num2] = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_ENCODE_INFO))));
                    Marshal.WriteIntPtr(encryptParam.prgpEncryptedKey[num2].DangerousGetHandle(), encryptParam.rgpEncryptedKey[num2].DangerousGetHandle());
                    ptr17 = new IntPtr(((long) encryptParam.rgpEncryptedKey[num2].DangerousGetHandle()) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_ENCODE_INFO), "cbSize")));
                    Marshal.WriteInt32(ptr17, Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_ENCODE_INFO)));
                    IntPtr ptr29 = new IntPtr(((long) encryptParam.rgpEncryptedKey[num2].DangerousGetHandle()) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_ENCODE_INFO), "RecipientPublicKey")));
                    ptr25 = new IntPtr(((long) ptr29) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_BIT_BLOB), "cbData")));
                    Marshal.WriteInt32(ptr25, (int) cert_info.SubjectPublicKeyInfo.PublicKey.cbData);
                    ptr26 = new IntPtr(((long) ptr29) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_BIT_BLOB), "pbData")));
                    Marshal.WriteIntPtr(ptr26, cert_info.SubjectPublicKeyInfo.PublicKey.pbData);
                    IntPtr ptr30 = new IntPtr(((long) ptr29) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPT_BIT_BLOB), "cUnusedBits")));
                    Marshal.WriteInt32(ptr30, (int) cert_info.SubjectPublicKeyInfo.PublicKey.cUnusedBits);
                    IntPtr ptr31 = new IntPtr(((long) encryptParam.rgpEncryptedKey[num2].DangerousGetHandle()) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_ENCODE_INFO), "RecipientId")));
                    IntPtr ptr32 = new IntPtr(((long) ptr31) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ID), "dwIdChoice")));
                    if (recipient.RecipientIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier)
                    {
                        Marshal.WriteInt32(ptr32, 2);
                        IntPtr ptr33 = new IntPtr(((long) ptr31) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ID), "Value")));
                        uint num7 = 0;
                        System.Security.Cryptography.SafeLocalAllocHandle pvData = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
                        if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateContextProperty(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate), 20, pvData, ref num7))
                        {
                            throw new CryptographicException(Marshal.GetLastWin32Error());
                        }
                        pvData = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) num7));
                        if (!System.Security.Cryptography.CAPI.CAPISafe.CertGetCertificateContextProperty(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate), 20, pvData, ref num7))
                        {
                            throw new CryptographicException(Marshal.GetLastWin32Error());
                        }
                        encryptParam.rgSubjectKeyIdentifier[num2] = pvData;
                        ptr25 = new IntPtr(((long) ptr33) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "cbData")));
                        Marshal.WriteInt32(ptr25, (int) num7);
                        ptr26 = new IntPtr(((long) ptr33) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "pbData")));
                        Marshal.WriteIntPtr(ptr26, pvData.DangerousGetHandle());
                    }
                    else
                    {
                        Marshal.WriteInt32(ptr32, 1);
                        IntPtr ptr34 = new IntPtr(((long) ptr31) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ID), "Value")));
                        IntPtr ptr35 = new IntPtr(((long) ptr34) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ISSUER_SERIAL_NUMBER), "Issuer")));
                        ptr25 = new IntPtr(((long) ptr35) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "cbData")));
                        Marshal.WriteInt32(ptr25, (int) cert_info.Issuer.cbData);
                        ptr26 = new IntPtr(((long) ptr35) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "pbData")));
                        Marshal.WriteIntPtr(ptr26, cert_info.Issuer.pbData);
                        IntPtr ptr36 = new IntPtr(((long) ptr34) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CERT_ISSUER_SERIAL_NUMBER), "SerialNumber")));
                        ptr25 = new IntPtr(((long) ptr36) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "cbData")));
                        Marshal.WriteInt32(ptr25, (int) cert_info.SerialNumber.cbData);
                        ptr26 = new IntPtr(((long) ptr36) + ((long) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTOAPI_BLOB), "pbData")));
                        Marshal.WriteIntPtr(ptr26, cert_info.SerialNumber.pbData);
                    }
                    num2++;
                    ptr = new IntPtr(((long) ptr) + Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO)));
                }
            }
        }

        [SecurityCritical]
        private static void SetCspParams(AlgorithmIdentifier contentEncryptionAlgorithm, ref CMSG_ENCRYPT_PARAM encryptParam)
        {
            encryptParam.safeCryptProvHandle = System.Security.Cryptography.SafeCryptProvHandle.InvalidHandle;
            encryptParam.pvEncryptionAuxInfo = System.Security.Cryptography.SafeLocalAllocHandle.InvalidHandle;
            System.Security.Cryptography.SafeCryptProvHandle invalidHandle = System.Security.Cryptography.SafeCryptProvHandle.InvalidHandle;
            if (!System.Security.Cryptography.CAPI.CryptAcquireContext(ref invalidHandle, IntPtr.Zero, IntPtr.Zero, 1, 0xf0000000))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            uint algId = System.Security.Cryptography.X509Certificates.X509Utils.OidToAlgId(contentEncryptionAlgorithm.Oid.Value);
            switch (algId)
            {
                case 0x6602:
                case 0x6801:
                {
                    System.Security.Cryptography.CAPI.CMSG_RC2_AUX_INFO structure = new System.Security.Cryptography.CAPI.CMSG_RC2_AUX_INFO(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_RC2_AUX_INFO)));
                    uint keyLength = (uint) contentEncryptionAlgorithm.KeyLength;
                    if (keyLength == 0)
                    {
                        keyLength = (uint) PkcsUtils.GetMaxKeyLength(invalidHandle, algId);
                    }
                    structure.dwBitLen = keyLength;
                    System.Security.Cryptography.SafeLocalAllocHandle handle2 = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CMSG_RC2_AUX_INFO))));
                    Marshal.StructureToPtr(structure, handle2.DangerousGetHandle(), false);
                    encryptParam.pvEncryptionAuxInfo = handle2;
                    break;
                }
            }
            encryptParam.safeCryptProvHandle = invalidHandle;
        }

        [SecurityCritical]
        private static void SetPkcs7RecipientParams(CmsRecipientCollection recipients, ref CMSG_ENCRYPT_PARAM encryptParam, out List<System.Security.Cryptography.SafeCertContextHandle> certContexts)
        {
            int num = 0;
            int count = recipients.Count;
            certContexts = new List<System.Security.Cryptography.SafeCertContextHandle>();
            uint num3 = (uint) (count * Marshal.SizeOf(typeof(IntPtr)));
            encryptParam.rgpRecipients = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr((long) num3));
            IntPtr ptr = encryptParam.rgpRecipients.DangerousGetHandle();
            for (num = 0; num < count; num++)
            {
                System.Security.Cryptography.SafeCertContextHandle certContext = System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(recipients[num].Certificate);
                certContexts.Add(certContext);
                System.Security.Cryptography.CAPI.CERT_CONTEXT cert_context = (System.Security.Cryptography.CAPI.CERT_CONTEXT) Marshal.PtrToStructure(certContext.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CERT_CONTEXT));
                Marshal.WriteIntPtr(ptr, cert_context.pCertInfo);
                ptr = new IntPtr(((long) ptr) + Marshal.SizeOf(typeof(IntPtr)));
            }
        }

        public X509Certificate2Collection Certificates
        {
            get
            {
                return this.m_certificates;
            }
        }

        public AlgorithmIdentifier ContentEncryptionAlgorithm
        {
            get
            {
                return this.m_encryptionAlgorithm;
            }
        }

        public System.Security.Cryptography.Pkcs.ContentInfo ContentInfo
        {
            get
            {
                return this.m_contentInfo;
            }
        }

        public RecipientInfoCollection RecipientInfos
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_safeCryptMsgHandle != null) && !this.m_safeCryptMsgHandle.IsInvalid)
                {
                    return new RecipientInfoCollection(this.m_safeCryptMsgHandle);
                }
                return new RecipientInfoCollection();
            }
        }

        public CryptographicAttributeObjectCollection UnprotectedAttributes
        {
            get
            {
                return this.m_unprotectedAttributes;
            }
        }

        public int Version
        {
            get
            {
                return this.m_version;
            }
        }

        [StructLayout(LayoutKind.Sequential), SecurityCritical]
        private struct CMSG_DECRYPT_PARAM
        {
            internal System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle;
            internal System.Security.Cryptography.SafeCryptProvHandle safeCryptProvHandle;
            internal uint keySpec;
        }

        [StructLayout(LayoutKind.Sequential), SecurityCritical]
        private struct CMSG_ENCRYPT_PARAM
        {
            internal bool useCms;
            internal System.Security.Cryptography.SafeCryptProvHandle safeCryptProvHandle;
            internal System.Security.Cryptography.SafeLocalAllocHandle pvEncryptionAuxInfo;
            internal System.Security.Cryptography.SafeLocalAllocHandle rgpRecipients;
            internal System.Security.Cryptography.SafeLocalAllocHandle rgCertEncoded;
            internal System.Security.Cryptography.SafeLocalAllocHandle rgUnprotectedAttr;
            internal System.Security.Cryptography.SafeLocalAllocHandle[] rgSubjectKeyIdentifier;
            internal System.Security.Cryptography.SafeLocalAllocHandle[] rgszObjId;
            internal System.Security.Cryptography.SafeLocalAllocHandle[] rgszKeyWrapObjId;
            internal System.Security.Cryptography.SafeLocalAllocHandle[] rgKeyWrapAuxInfo;
            internal System.Security.Cryptography.SafeLocalAllocHandle[] rgEphemeralIdentifier;
            internal System.Security.Cryptography.SafeLocalAllocHandle[] rgszEphemeralObjId;
            internal System.Security.Cryptography.SafeLocalAllocHandle[] rgUserKeyingMaterial;
            internal System.Security.Cryptography.SafeLocalAllocHandle[] prgpEncryptedKey;
            internal System.Security.Cryptography.SafeLocalAllocHandle[] rgpEncryptedKey;
        }
    }
}

