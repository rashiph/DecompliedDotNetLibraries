namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public sealed class X500DistinguishedName : AsnEncodedData
    {
        private string m_distinguishedName;

        internal X500DistinguishedName(CAPIBase.CRYPTOAPI_BLOB encodedDistinguishedNameBlob) : base(new Oid(), encodedDistinguishedNameBlob)
        {
        }

        public X500DistinguishedName(byte[] encodedDistinguishedName) : base(new Oid(), encodedDistinguishedName)
        {
        }

        public X500DistinguishedName(AsnEncodedData encodedDistinguishedName) : base(encodedDistinguishedName)
        {
        }

        public X500DistinguishedName(X500DistinguishedName distinguishedName) : base(distinguishedName)
        {
            this.m_distinguishedName = distinguishedName.Name;
        }

        public X500DistinguishedName(string distinguishedName) : this(distinguishedName, X500DistinguishedNameFlags.Reversed)
        {
        }

        public X500DistinguishedName(string distinguishedName, X500DistinguishedNameFlags flag) : base(new Oid(), Encode(distinguishedName, flag))
        {
            this.m_distinguishedName = distinguishedName;
        }

        public unsafe string Decode(X500DistinguishedNameFlags flag)
        {
            uint dwStrType = 3 | MapNameToStrFlag(flag);
            byte[] rawData = base.m_rawData;
            fixed (byte* numRef = rawData)
            {
                CAPIBase.CRYPTOAPI_BLOB cryptoapi_blob;
                IntPtr pName = new IntPtr((void*) &cryptoapi_blob);
                cryptoapi_blob.cbData = (uint) rawData.Length;
                cryptoapi_blob.pbData = new IntPtr((void*) numRef);
                uint csz = CAPISafe.CertNameToStrW(0x10001, pName, dwStrType, SafeLocalAllocHandle.InvalidHandle, 0);
                if (csz == 0)
                {
                    throw new CryptographicException(-2146762476);
                }
                using (SafeLocalAllocHandle handle = CAPI.LocalAlloc(0x40, new IntPtr((long) (2 * csz))))
                {
                    if (CAPISafe.CertNameToStrW(0x10001, pName, dwStrType, handle, csz) == 0)
                    {
                        throw new CryptographicException(-2146762476);
                    }
                    return Marshal.PtrToStringUni(handle.DangerousGetHandle());
                }
            }
        }

        private static unsafe byte[] Encode(string distinguishedName, X500DistinguishedNameFlags flag)
        {
            if (distinguishedName == null)
            {
                throw new ArgumentNullException("distinguishedName");
            }
            uint pcbEncoded = 0;
            uint dwStrType = 3 | MapNameToStrFlag(flag);
            if (!CAPISafe.CertStrToNameW(0x10001, distinguishedName, dwStrType, IntPtr.Zero, IntPtr.Zero, ref pcbEncoded, IntPtr.Zero))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            byte[] buffer = new byte[pcbEncoded];
            fixed (byte* numRef = buffer)
            {
                if (!CAPISafe.CertStrToNameW(0x10001, distinguishedName, dwStrType, IntPtr.Zero, new IntPtr((void*) numRef), ref pcbEncoded, IntPtr.Zero))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            return buffer;
        }

        public override string Format(bool multiLine)
        {
            if ((base.m_rawData == null) || (base.m_rawData.Length == 0))
            {
                return string.Empty;
            }
            return CAPI.CryptFormatObject(1, multiLine ? 1 : 0, new IntPtr(7L), base.m_rawData);
        }

        private static uint MapNameToStrFlag(X500DistinguishedNameFlags flag)
        {
            uint num = 0x71f1;
            uint num2 = (uint) flag;
            if ((num2 & ~num) != 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { "flag" }));
            }
            uint num3 = 0;
            if (num2 != 0)
            {
                if ((flag & X500DistinguishedNameFlags.Reversed) == X500DistinguishedNameFlags.Reversed)
                {
                    num3 |= 0x2000000;
                }
                if ((flag & X500DistinguishedNameFlags.UseSemicolons) == X500DistinguishedNameFlags.UseSemicolons)
                {
                    num3 |= 0x40000000;
                }
                else if ((flag & X500DistinguishedNameFlags.UseCommas) == X500DistinguishedNameFlags.UseCommas)
                {
                    num3 |= 0x4000000;
                }
                else if ((flag & X500DistinguishedNameFlags.UseNewLines) == X500DistinguishedNameFlags.UseNewLines)
                {
                    num3 |= 0x8000000;
                }
                if ((flag & X500DistinguishedNameFlags.DoNotUsePlusSign) == X500DistinguishedNameFlags.DoNotUsePlusSign)
                {
                    num3 |= 0x20000000;
                }
                if ((flag & X500DistinguishedNameFlags.DoNotUseQuotes) == X500DistinguishedNameFlags.DoNotUseQuotes)
                {
                    num3 |= 0x10000000;
                }
                if ((flag & X500DistinguishedNameFlags.ForceUTF8Encoding) == X500DistinguishedNameFlags.ForceUTF8Encoding)
                {
                    num3 |= 0x80000;
                }
                if ((flag & X500DistinguishedNameFlags.UseUTF8Encoding) == X500DistinguishedNameFlags.UseUTF8Encoding)
                {
                    return (num3 | 0x40000);
                }
                if ((flag & X500DistinguishedNameFlags.UseT61Encoding) == X500DistinguishedNameFlags.UseT61Encoding)
                {
                    num3 |= 0x20000;
                }
            }
            return num3;
        }

        public string Name
        {
            get
            {
                if (this.m_distinguishedName == null)
                {
                    this.m_distinguishedName = this.Decode(X500DistinguishedNameFlags.Reversed);
                }
                return this.m_distinguishedName;
            }
        }
    }
}

