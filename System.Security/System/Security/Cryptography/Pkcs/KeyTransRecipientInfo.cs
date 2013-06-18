namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class KeyTransRecipientInfo : RecipientInfo
    {
        private byte[] m_encryptedKey;
        private AlgorithmIdentifier m_encryptionAlgorithm;
        private SubjectIdentifier m_recipientIdentifier;
        private int m_version;

        [SecurityCritical]
        internal unsafe KeyTransRecipientInfo(System.Security.Cryptography.SafeLocalAllocHandle pRecipientInfo, System.Security.Cryptography.CAPI.CERT_INFO certInfo, uint index) : base(RecipientInfoType.KeyTransport, RecipientSubType.Pkcs7KeyTransport, pRecipientInfo, certInfo, index)
        {
            int version = 2;
            byte* pbData = (byte*) certInfo.SerialNumber.pbData;
            for (int i = 0; i < certInfo.SerialNumber.cbData; i++)
            {
                pbData++;
                if (pbData[0] != 0)
                {
                    version = 0;
                    break;
                }
            }
            this.Reset(version);
        }

        [SecurityCritical]
        internal KeyTransRecipientInfo(System.Security.Cryptography.SafeLocalAllocHandle pRecipientInfo, System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO keyTrans, uint index) : base(RecipientInfoType.KeyTransport, RecipientSubType.CmsKeyTransport, pRecipientInfo, keyTrans, index)
        {
            this.Reset((int) keyTrans.dwVersion);
        }

        private void Reset(int version)
        {
            this.m_version = version;
            this.m_recipientIdentifier = null;
            this.m_encryptionAlgorithm = null;
            this.m_encryptedKey = new byte[0];
        }

        public override byte[] EncryptedKey
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_encryptedKey.Length == 0) && (base.SubType == RecipientSubType.CmsKeyTransport))
                {
                    System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO cmsgRecipientInfo = (System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO) base.CmsgRecipientInfo;
                    if (cmsgRecipientInfo.EncryptedKey.cbData > 0)
                    {
                        this.m_encryptedKey = new byte[cmsgRecipientInfo.EncryptedKey.cbData];
                        Marshal.Copy(cmsgRecipientInfo.EncryptedKey.pbData, this.m_encryptedKey, 0, this.m_encryptedKey.Length);
                    }
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
                    if (base.SubType == RecipientSubType.CmsKeyTransport)
                    {
                        System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO cmsgRecipientInfo = (System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO) base.CmsgRecipientInfo;
                        this.m_encryptionAlgorithm = new AlgorithmIdentifier(cmsgRecipientInfo.KeyEncryptionAlgorithm);
                    }
                    else
                    {
                        System.Security.Cryptography.CAPI.CERT_INFO cert_info = (System.Security.Cryptography.CAPI.CERT_INFO) base.CmsgRecipientInfo;
                        this.m_encryptionAlgorithm = new AlgorithmIdentifier(cert_info.SignatureAlgorithm);
                    }
                }
                return this.m_encryptionAlgorithm;
            }
        }

        public override SubjectIdentifier RecipientIdentifier
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_recipientIdentifier == null)
                {
                    if (base.SubType == RecipientSubType.CmsKeyTransport)
                    {
                        System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO cmsgRecipientInfo = (System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO) base.CmsgRecipientInfo;
                        this.m_recipientIdentifier = new SubjectIdentifier(cmsgRecipientInfo.RecipientId);
                    }
                    else
                    {
                        System.Security.Cryptography.CAPI.CERT_INFO certInfo = (System.Security.Cryptography.CAPI.CERT_INFO) base.CmsgRecipientInfo;
                        this.m_recipientIdentifier = new SubjectIdentifier(certInfo);
                    }
                }
                return this.m_recipientIdentifier;
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

