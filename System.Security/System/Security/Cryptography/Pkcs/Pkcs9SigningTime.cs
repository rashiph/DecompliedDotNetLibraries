namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class Pkcs9SigningTime : Pkcs9AttributeObject
    {
        private bool m_decoded;
        private DateTime m_signingTime;

        public Pkcs9SigningTime() : this(DateTime.Now)
        {
        }

        public Pkcs9SigningTime(DateTime signingTime) : base("1.2.840.113549.1.9.5", Encode(signingTime))
        {
            this.m_signingTime = signingTime;
            this.m_decoded = true;
        }

        public Pkcs9SigningTime(byte[] encodedSigningTime) : base("1.2.840.113549.1.9.5", encodedSigningTime)
        {
        }

        public override void CopyFrom(AsnEncodedData asnEncodedData)
        {
            base.CopyFrom(asnEncodedData);
            this.m_decoded = false;
        }

        [SecuritySafeCritical]
        private void Decode()
        {
            uint cbDecodedValue = 0;
            System.Security.Cryptography.SafeLocalAllocHandle decodedValue = null;
            if (!System.Security.Cryptography.CAPI.DecodeObject(new IntPtr(0x11L), base.RawData, out decodedValue, out cbDecodedValue))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            long fileTime = Marshal.ReadInt64(decodedValue.DangerousGetHandle());
            decodedValue.Dispose();
            this.m_signingTime = DateTime.FromFileTimeUtc(fileTime);
            this.m_decoded = true;
        }

        [SecuritySafeCritical]
        private static byte[] Encode(DateTime signingTime)
        {
            long val = signingTime.ToFileTimeUtc();
            System.Security.Cryptography.SafeLocalAllocHandle handle = System.Security.Cryptography.CAPI.LocalAlloc(0x40, new IntPtr(Marshal.SizeOf(typeof(long))));
            Marshal.WriteInt64(handle.DangerousGetHandle(), val);
            byte[] encodedData = new byte[0];
            if (!System.Security.Cryptography.CAPI.EncodeObject("1.2.840.113549.1.9.5", handle.DangerousGetHandle(), out encodedData))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            handle.Dispose();
            return encodedData;
        }

        public DateTime SigningTime
        {
            get
            {
                if (!this.m_decoded && (base.RawData != null))
                {
                    this.Decode();
                }
                return this.m_signingTime;
            }
        }
    }
}

