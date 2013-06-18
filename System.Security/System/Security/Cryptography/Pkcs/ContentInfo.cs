namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class ContentInfo
    {
        private byte[] m_content;
        private Oid m_contentType;
        private GCHandle m_gcHandle;
        private IntPtr m_pContent;

        private ContentInfo() : this(new Oid("1.2.840.113549.1.7.1"), new byte[0])
        {
        }

        public ContentInfo(byte[] content) : this(new Oid("1.2.840.113549.1.7.1"), content)
        {
        }

        public ContentInfo(Oid contentType, byte[] content)
        {
            this.m_pContent = IntPtr.Zero;
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            this.m_contentType = contentType;
            this.m_content = content;
        }

        internal ContentInfo(string contentType, byte[] content) : this(new Oid(contentType), content)
        {
        }

        [SecuritySafeCritical]
        ~ContentInfo()
        {
            if (this.m_gcHandle.IsAllocated)
            {
                this.m_gcHandle.Free();
            }
        }

        [SecuritySafeCritical]
        public static Oid GetContentType(byte[] encodedMessage)
        {
            Oid oid;
            if (encodedMessage == null)
            {
                throw new ArgumentNullException("encodedMessage");
            }
            System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg = System.Security.Cryptography.CAPI.CAPISafe.CryptMsgOpenToDecode(0x10001, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if ((hCryptMsg == null) || hCryptMsg.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgUpdate(hCryptMsg, encodedMessage, (uint) encodedMessage.Length, true))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            switch (PkcsUtils.GetMessageType(hCryptMsg))
            {
                case 1:
                    oid = new Oid("1.2.840.113549.1.7.1");
                    break;

                case 2:
                    oid = new Oid("1.2.840.113549.1.7.2");
                    break;

                case 3:
                    oid = new Oid("1.2.840.113549.1.7.3");
                    break;

                case 4:
                    oid = new Oid("1.2.840.113549.1.7.4");
                    break;

                case 5:
                    oid = new Oid("1.2.840.113549.1.7.5");
                    break;

                case 6:
                    oid = new Oid("1.2.840.113549.1.7.6");
                    break;

                default:
                    throw new CryptographicException(-2146889724);
            }
            hCryptMsg.Dispose();
            return oid;
        }

        public byte[] Content
        {
            get
            {
                return this.m_content;
            }
        }

        public Oid ContentType
        {
            get
            {
                return this.m_contentType;
            }
        }

        internal IntPtr pContent
        {
            [SecurityCritical]
            get
            {
                if (((IntPtr.Zero == this.m_pContent) && (this.m_content != null)) && (this.m_content.Length != 0))
                {
                    this.m_gcHandle = GCHandle.Alloc(this.m_content, GCHandleType.Pinned);
                    this.m_pContent = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_content, 0);
                }
                return this.m_pContent;
            }
        }
    }
}

