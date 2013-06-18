namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class KeyAgreeRecipientInfo : RecipientInfo
    {
        private DateTime m_date;
        private byte[] m_encryptedKey;
        private System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_INFO m_encryptedKeyInfo;
        private AlgorithmIdentifier m_encryptionAlgorithm;
        private uint m_originatorChoice;
        private SubjectIdentifierOrKey m_originatorIdentifier;
        private CryptographicAttributeObject m_otherKeyAttribute;
        private SubjectIdentifier m_recipientIdentifier;
        private uint m_subIndex;
        private byte[] m_userKeyMaterial;
        private int m_version;

        private KeyAgreeRecipientInfo()
        {
        }

        [SecurityCritical]
        internal KeyAgreeRecipientInfo(System.Security.Cryptography.SafeLocalAllocHandle pRecipientInfo, System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO certIdRecipient, uint index, uint subIndex) : base(RecipientInfoType.KeyAgreement, RecipientSubType.CertIdKeyAgreement, pRecipientInfo, certIdRecipient, index)
        {
            System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_INFO encryptedKeyInfo = (System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_INFO) Marshal.PtrToStructure(Marshal.ReadIntPtr(new IntPtr(((long) certIdRecipient.rgpRecipientEncryptedKeys) + (subIndex * Marshal.SizeOf(typeof(IntPtr))))), typeof(System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_INFO));
            this.Reset(1, certIdRecipient.dwVersion, encryptedKeyInfo, subIndex);
        }

        [SecurityCritical]
        internal KeyAgreeRecipientInfo(System.Security.Cryptography.SafeLocalAllocHandle pRecipientInfo, System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO publicKeyRecipient, uint index, uint subIndex) : base(RecipientInfoType.KeyAgreement, RecipientSubType.PublicKeyAgreement, pRecipientInfo, publicKeyRecipient, index)
        {
            System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_INFO encryptedKeyInfo = (System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_INFO) Marshal.PtrToStructure(Marshal.ReadIntPtr(new IntPtr(((long) publicKeyRecipient.rgpRecipientEncryptedKeys) + (subIndex * Marshal.SizeOf(typeof(IntPtr))))), typeof(System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_INFO));
            this.Reset(2, publicKeyRecipient.dwVersion, encryptedKeyInfo, subIndex);
        }

        private void Reset(uint originatorChoice, uint version, System.Security.Cryptography.CAPI.CMSG_RECIPIENT_ENCRYPTED_KEY_INFO encryptedKeyInfo, uint subIndex)
        {
            this.m_encryptedKeyInfo = encryptedKeyInfo;
            this.m_originatorChoice = originatorChoice;
            this.m_version = (int) version;
            this.m_originatorIdentifier = null;
            this.m_userKeyMaterial = new byte[0];
            this.m_encryptionAlgorithm = null;
            this.m_recipientIdentifier = null;
            this.m_encryptedKey = new byte[0];
            this.m_date = DateTime.MinValue;
            this.m_otherKeyAttribute = null;
            this.m_subIndex = subIndex;
        }

        public DateTime Date
        {
            get
            {
                if (this.m_date == DateTime.MinValue)
                {
                    if (this.RecipientIdentifier.Type != SubjectIdentifierType.SubjectKeyIdentifier)
                    {
                        throw new InvalidOperationException(SecurityResources.GetResourceString("Cryptography_Cms_Key_Agree_Date_Not_Available"));
                    }
                    long fileTime = (long) ((((ulong) this.m_encryptedKeyInfo.Date.dwHighDateTime) << 0x20) | ((ulong) this.m_encryptedKeyInfo.Date.dwLowDateTime));
                    this.m_date = DateTime.FromFileTimeUtc(fileTime);
                }
                return this.m_date;
            }
        }

        public override byte[] EncryptedKey
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_encryptedKey.Length == 0) && (this.m_encryptedKeyInfo.EncryptedKey.cbData > 0))
                {
                    this.m_encryptedKey = new byte[this.m_encryptedKeyInfo.EncryptedKey.cbData];
                    Marshal.Copy(this.m_encryptedKeyInfo.EncryptedKey.pbData, this.m_encryptedKey, 0, this.m_encryptedKey.Length);
                }
                return this.m_encryptedKey;
            }
        }

        public override AlgorithmIdentifier KeyEncryptionAlgorithm
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_encryptionAlgorithm == null)
                {
                    if (this.m_originatorChoice == 1)
                    {
                        System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO cmsgRecipientInfo = (System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO) base.CmsgRecipientInfo;
                        this.m_encryptionAlgorithm = new AlgorithmIdentifier(cmsgRecipientInfo.KeyEncryptionAlgorithm);
                    }
                    else
                    {
                        System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO cmsg_key_agree_public_key_recipient_info = (System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO) base.CmsgRecipientInfo;
                        this.m_encryptionAlgorithm = new AlgorithmIdentifier(cmsg_key_agree_public_key_recipient_info.KeyEncryptionAlgorithm);
                    }
                }
                return this.m_encryptionAlgorithm;
            }
        }

        public SubjectIdentifierOrKey OriginatorIdentifierOrKey
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_originatorIdentifier == null)
                {
                    if (this.m_originatorChoice == 1)
                    {
                        System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO cmsgRecipientInfo = (System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO) base.CmsgRecipientInfo;
                        this.m_originatorIdentifier = new SubjectIdentifierOrKey(cmsgRecipientInfo.OriginatorCertId);
                    }
                    else
                    {
                        System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO cmsg_key_agree_public_key_recipient_info = (System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO) base.CmsgRecipientInfo;
                        this.m_originatorIdentifier = new SubjectIdentifierOrKey(cmsg_key_agree_public_key_recipient_info.OriginatorPublicKeyInfo);
                    }
                }
                return this.m_originatorIdentifier;
            }
        }

        public CryptographicAttributeObject OtherKeyAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_otherKeyAttribute == null)
                {
                    if (this.RecipientIdentifier.Type != SubjectIdentifierType.SubjectKeyIdentifier)
                    {
                        throw new InvalidOperationException(SecurityResources.GetResourceString("Cryptography_Cms_Key_Agree_Other_Key_Attribute_Not_Available"));
                    }
                    if (this.m_encryptedKeyInfo.pOtherAttr != IntPtr.Zero)
                    {
                        System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE_TYPE_VALUE cryptAttribute = (System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE_TYPE_VALUE) Marshal.PtrToStructure(this.m_encryptedKeyInfo.pOtherAttr, typeof(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE_TYPE_VALUE));
                        this.m_otherKeyAttribute = new CryptographicAttributeObject(cryptAttribute);
                    }
                }
                return this.m_otherKeyAttribute;
            }
        }

        internal System.Security.Cryptography.CAPI.CERT_ID RecipientId
        {
            get
            {
                return this.m_encryptedKeyInfo.RecipientId;
            }
        }

        public override SubjectIdentifier RecipientIdentifier
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_recipientIdentifier == null)
                {
                    this.m_recipientIdentifier = new SubjectIdentifier(this.m_encryptedKeyInfo.RecipientId);
                }
                return this.m_recipientIdentifier;
            }
        }

        internal uint SubIndex
        {
            get
            {
                return this.m_subIndex;
            }
        }

        public override int Version
        {
            get
            {
                return this.m_version;
            }
        }
    }
}

