namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Security;

    [Serializable]
    internal class EUCJPEncoding : DBCSCodePageEncoding
    {
        [SecurityCritical]
        public EUCJPEncoding() : base(0xcadc, 0x3a4)
        {
            base.m_bUseMlangTypeForSerialization = true;
        }

        protected override bool CleanUpBytes(ref int bytes)
        {
            if (bytes >= 0x100)
            {
                if ((bytes >= 0xfa40) && (bytes <= 0xfc4b))
                {
                    if ((bytes >= 0xfa40) && (bytes <= 0xfa5b))
                    {
                        if (bytes <= 0xfa49)
                        {
                            bytes -= 0xb51;
                        }
                        else if ((bytes >= 0xfa4a) && (bytes <= 0xfa53))
                        {
                            bytes -= 0x72f6;
                        }
                        else if ((bytes >= 0xfa54) && (bytes <= 0xfa57))
                        {
                            bytes -= 0xb5b;
                        }
                        else if (bytes == 0xfa58)
                        {
                            bytes = 0x878a;
                        }
                        else if (bytes == 0xfa59)
                        {
                            bytes = 0x8782;
                        }
                        else if (bytes == 0xfa5a)
                        {
                            bytes = 0x8784;
                        }
                        else if (bytes == 0xfa5b)
                        {
                            bytes = 0x879a;
                        }
                    }
                    else if ((bytes >= 0xfa5c) && (bytes <= 0xfc4b))
                    {
                        byte num = (byte) bytes;
                        if (num < 0x5c)
                        {
                            bytes -= 0xd5f;
                        }
                        else if ((num >= 0x80) && (num <= 0x9b))
                        {
                            bytes -= 0xd1d;
                        }
                        else
                        {
                            bytes -= 0xd1c;
                        }
                    }
                }
                byte num2 = (byte) (bytes >> 8);
                byte num3 = (byte) bytes;
                num2 = (byte) (num2 - ((num2 > 0x9f) ? 0xb1 : 0x71));
                num2 = (byte) ((num2 << 1) + 1);
                if (num3 > 0x9e)
                {
                    num3 = (byte) (num3 - 0x7e);
                    num2 = (byte) (num2 + 1);
                }
                else
                {
                    if (num3 > 0x7e)
                    {
                        num3 = (byte) (num3 - 1);
                    }
                    num3 = (byte) (num3 - 0x1f);
                }
                bytes = ((num2 << 8) | num3) | 0x8080;
                if ((((bytes & 0xff00) < 0xa100) || ((bytes & 0xff00) > 0xfe00)) || (((bytes & 0xff) < 0xa1) || ((bytes & 0xff) > 0xfe)))
                {
                    return false;
                }
            }
            else
            {
                if ((bytes >= 0xa1) && (bytes <= 0xdf))
                {
                    bytes |= 0x8e00;
                    return true;
                }
                if (((bytes >= 0x81) && (bytes != 160)) && (bytes != 0xff))
                {
                    return false;
                }
            }
            return true;
        }

        [SecurityCritical]
        protected override unsafe void CleanUpEndBytes(char* chars)
        {
            for (int i = 0xa1; i <= 0xfe; i++)
            {
                chars[i] = 0xfffe;
            }
            chars[0x8e] = 0xfffe;
        }

        [SecurityCritical]
        protected override unsafe string GetMemorySectionName()
        {
            int num = base.bFlagDataTable ? base.dataTableCodePage : this.CodePage;
            return string.Format(CultureInfo.InvariantCulture, "CodePage_{0}_{1}_{2}_{3}_{4}_EUCJP", new object[] { num, base.pCodePage.VersionMajor, base.pCodePage.VersionMinor, base.pCodePage.VersionRevision, base.pCodePage.VersionBuild });
        }
    }
}

