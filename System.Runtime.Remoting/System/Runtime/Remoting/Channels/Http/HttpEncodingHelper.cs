namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Text;

    internal static class HttpEncodingHelper
    {
        internal static string DecodeUri(string uri)
        {
            int num;
            byte[] bytes = Encoding.UTF8.GetBytes(uri);
            HttpChannelHelper.DecodeUriInPlace(bytes, out num);
            return Encoding.UTF8.GetString(bytes, 0, num);
        }

        internal static string EncodeUriAsXLinkHref(string uri)
        {
            if (uri == null)
            {
                return null;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(uri);
            StringBuilder builder = new StringBuilder(uri.Length);
            foreach (byte num in bytes)
            {
                if (!EscapeInXLinkHref(num))
                {
                    builder.Append((char) num);
                }
                else
                {
                    builder.Append('%');
                    builder.Append(HttpChannelHelper.DecimalToCharacterHexDigit(num >> 4));
                    builder.Append(HttpChannelHelper.DecimalToCharacterHexDigit(num & 15));
                }
            }
            return builder.ToString();
        }

        internal static bool EscapeInXLinkHref(byte ch)
        {
            if (((ch > 0x20) && (ch < 0x80)) && (((ch != 60) && (ch != 0x3e)) && (ch != 0x22)))
            {
                return false;
            }
            return true;
        }
    }
}

