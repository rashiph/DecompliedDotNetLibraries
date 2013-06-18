namespace System.Web.Security
{
    using System;
    using System.Web;
    using System.Web.Configuration;

    internal class CookieProtectionHelper
    {
        internal static byte[] Decode(CookieProtection cookieProtection, string data)
        {
            byte[] buf = HttpServerUtility.UrlTokenDecode(data);
            if ((buf == null) || (cookieProtection == CookieProtection.None))
            {
                return buf;
            }
            if ((cookieProtection == CookieProtection.All) || (cookieProtection == CookieProtection.Encryption))
            {
                buf = MachineKeySection.EncryptOrDecryptData(false, buf, null, 0, buf.Length);
                if (buf == null)
                {
                    return null;
                }
            }
            if ((cookieProtection != CookieProtection.All) && (cookieProtection != CookieProtection.Validation))
            {
                return buf;
            }
            return MachineKeySection.GetUnHashedData(buf);
        }

        internal static string Encode(CookieProtection cookieProtection, byte[] buf, int count)
        {
            if ((cookieProtection == CookieProtection.All) || (cookieProtection == CookieProtection.Validation))
            {
                byte[] src = MachineKeySection.HashData(buf, null, 0, count);
                if (src == null)
                {
                    return null;
                }
                if (buf.Length >= (count + src.Length))
                {
                    Buffer.BlockCopy(src, 0, buf, count, src.Length);
                }
                else
                {
                    byte[] buffer2 = buf;
                    buf = new byte[count + src.Length];
                    Buffer.BlockCopy(buffer2, 0, buf, 0, count);
                    Buffer.BlockCopy(src, 0, buf, count, src.Length);
                }
                count += src.Length;
            }
            if ((cookieProtection == CookieProtection.All) || (cookieProtection == CookieProtection.Encryption))
            {
                buf = MachineKeySection.EncryptOrDecryptData(true, buf, null, 0, count);
                count = buf.Length;
            }
            if (count < buf.Length)
            {
                byte[] buffer3 = buf;
                buf = new byte[count];
                Buffer.BlockCopy(buffer3, 0, buf, 0, count);
            }
            return HttpServerUtility.UrlTokenEncode(buf);
        }
    }
}

