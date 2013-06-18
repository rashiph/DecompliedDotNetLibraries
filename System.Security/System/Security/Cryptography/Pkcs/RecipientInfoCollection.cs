namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class RecipientInfoCollection : ICollection, IEnumerable
    {
        private ArrayList m_recipientInfos;
        [SecurityCritical]
        private System.Security.Cryptography.SafeCryptMsgHandle m_safeCryptMsgHandle;

        [SecuritySafeCritical]
        internal RecipientInfoCollection()
        {
            this.m_safeCryptMsgHandle = System.Security.Cryptography.SafeCryptMsgHandle.InvalidHandle;
            this.m_recipientInfos = new ArrayList();
        }

        [SecuritySafeCritical]
        internal RecipientInfoCollection(RecipientInfo recipientInfo)
        {
            this.m_safeCryptMsgHandle = System.Security.Cryptography.SafeCryptMsgHandle.InvalidHandle;
            this.m_recipientInfos = new ArrayList(1);
            this.m_recipientInfos.Add(recipientInfo);
        }

        [SecurityCritical]
        internal unsafe RecipientInfoCollection(System.Security.Cryptography.SafeCryptMsgHandle safeCryptMsgHandle)
        {
            bool flag = PkcsUtils.CmsSupported();
            uint num = 0;
            uint num2 = (uint) Marshal.SizeOf(typeof(uint));
            if (flag)
            {
                if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, 0x21, 0, new IntPtr((void*) &num), new IntPtr((void*) &num2)))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            else if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(safeCryptMsgHandle, 0x11, 0, new IntPtr((void*) &num), new IntPtr((void*) &num2)))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            this.m_recipientInfos = new ArrayList();
            for (uint i = 0; i < num; i++)
            {
                uint num4;
                System.Security.Cryptography.SafeLocalAllocHandle handle;
                System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO cmsg_key_agree_public_key_recipient_info;
                uint num7;
                System.Security.Cryptography.SafeLocalAllocHandle handle2;
                if (!flag)
                {
                    goto Label_020B;
                }
                PkcsUtils.GetParam(safeCryptMsgHandle, 0x24, i, out handle, out num4);
                System.Security.Cryptography.CAPI.CMSG_CMS_RECIPIENT_INFO cmsg_cms_recipient_info = (System.Security.Cryptography.CAPI.CMSG_CMS_RECIPIENT_INFO) Marshal.PtrToStructure(handle.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CMSG_CMS_RECIPIENT_INFO));
                switch (cmsg_cms_recipient_info.dwRecipientChoice)
                {
                    case 1:
                    {
                        System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO keyTrans = (System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO) Marshal.PtrToStructure(cmsg_cms_recipient_info.pRecipientInfo, typeof(System.Security.Cryptography.CAPI.CMSG_KEY_TRANS_RECIPIENT_INFO));
                        this.m_recipientInfos.Add(new KeyTransRecipientInfo(handle, keyTrans, i));
                        continue;
                    }
                    case 2:
                    {
                        System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_INFO cmsg_key_agree_recipient_info = (System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_INFO) Marshal.PtrToStructure(cmsg_cms_recipient_info.pRecipientInfo, typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_RECIPIENT_INFO));
                        switch (cmsg_key_agree_recipient_info.dwOriginatorChoice)
                        {
                            case 2:
                                goto Label_0192;
                        }
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Cms_Invalid_Originator_Identifier_Choice"), cmsg_key_agree_recipient_info.dwOriginatorChoice.ToString(CultureInfo.CurrentCulture));
                    }
                    default:
                        throw new CryptographicException(-2147483647);
                }
                System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO certIdRecipient = (System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO) Marshal.PtrToStructure(cmsg_cms_recipient_info.pRecipientInfo, typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO));
                for (uint j = 0; j < certIdRecipient.cRecipientEncryptedKeys; j++)
                {
                    this.m_recipientInfos.Add(new KeyAgreeRecipientInfo(handle, certIdRecipient, i, j));
                }
                continue;
            Label_0192:
                cmsg_key_agree_public_key_recipient_info = (System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO) Marshal.PtrToStructure(cmsg_cms_recipient_info.pRecipientInfo, typeof(System.Security.Cryptography.CAPI.CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO));
                for (uint k = 0; k < cmsg_key_agree_public_key_recipient_info.cRecipientEncryptedKeys; k++)
                {
                    this.m_recipientInfos.Add(new KeyAgreeRecipientInfo(handle, cmsg_key_agree_public_key_recipient_info, i, k));
                }
                continue;
            Label_020B:
                PkcsUtils.GetParam(safeCryptMsgHandle, 0x13, i, out handle2, out num7);
                System.Security.Cryptography.CAPI.CERT_INFO certInfo = (System.Security.Cryptography.CAPI.CERT_INFO) Marshal.PtrToStructure(handle2.DangerousGetHandle(), typeof(System.Security.Cryptography.CAPI.CERT_INFO));
                this.m_recipientInfos.Add(new KeyTransRecipientInfo(handle2, certInfo, i));
            }
            this.m_safeCryptMsgHandle = safeCryptMsgHandle;
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Arg_RankMultiDimNotSupported"));
            }
            if ((index < 0) || (index >= array.Length))
            {
                throw new ArgumentOutOfRangeException("index", SecurityResources.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((index + this.Count) > array.Length)
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Argument_InvalidOffLen"));
            }
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], index);
                index++;
            }
        }

        public void CopyTo(RecipientInfo[] array, int index)
        {
            this.CopyTo(array, index);
        }

        public RecipientInfoEnumerator GetEnumerator()
        {
            return new RecipientInfoEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new RecipientInfoEnumerator(this);
        }

        public int Count
        {
            get
            {
                return this.m_recipientInfos.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public RecipientInfo this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.m_recipientInfos.Count))
                {
                    throw new ArgumentOutOfRangeException("index", SecurityResources.GetResourceString("ArgumentOutOfRange_Index"));
                }
                return (RecipientInfo) this.m_recipientInfos[index];
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

