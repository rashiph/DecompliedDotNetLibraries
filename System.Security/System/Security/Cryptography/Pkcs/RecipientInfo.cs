namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class RecipientInfo
    {
        private object m_cmsgRecipientInfo;
        private uint m_index;
        [SecurityCritical]
        private System.Security.Cryptography.SafeLocalAllocHandle m_pCmsgRecipientInfo;
        private RecipientInfoType m_recipentInfoType;
        private RecipientSubType m_recipientSubType;

        internal RecipientInfo()
        {
        }

        [SecurityCritical]
        internal RecipientInfo(RecipientInfoType recipientInfoType, RecipientSubType recipientSubType, System.Security.Cryptography.SafeLocalAllocHandle pCmsgRecipientInfo, object cmsgRecipientInfo, uint index)
        {
            if ((recipientInfoType < RecipientInfoType.Unknown) || (recipientInfoType > RecipientInfoType.KeyAgreement))
            {
                recipientInfoType = RecipientInfoType.Unknown;
            }
            if ((recipientSubType < RecipientSubType.Unknown) || (recipientSubType > RecipientSubType.PublicKeyAgreement))
            {
                recipientSubType = RecipientSubType.Unknown;
            }
            this.m_recipentInfoType = recipientInfoType;
            this.m_recipientSubType = recipientSubType;
            this.m_pCmsgRecipientInfo = pCmsgRecipientInfo;
            this.m_cmsgRecipientInfo = cmsgRecipientInfo;
            this.m_index = index;
        }

        internal object CmsgRecipientInfo
        {
            get
            {
                return this.m_cmsgRecipientInfo;
            }
        }

        public abstract byte[] EncryptedKey { get; }

        internal uint Index
        {
            get
            {
                return this.m_index;
            }
        }

        public abstract AlgorithmIdentifier KeyEncryptionAlgorithm { get; }

        internal System.Security.Cryptography.SafeLocalAllocHandle pCmsgRecipientInfo
        {
            [SecurityCritical]
            get
            {
                return this.m_pCmsgRecipientInfo;
            }
        }

        public abstract SubjectIdentifier RecipientIdentifier { get; }

        internal RecipientSubType SubType
        {
            get
            {
                return this.m_recipientSubType;
            }
        }

        public RecipientInfoType Type
        {
            get
            {
                return this.m_recipentInfoType;
            }
        }

        public abstract int Version { get; }
    }
}

