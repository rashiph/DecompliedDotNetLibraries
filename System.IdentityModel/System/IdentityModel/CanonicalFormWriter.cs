namespace System.IdentityModel
{
    using System;
    using System.IO;
    using System.Text;

    internal abstract class CanonicalFormWriter
    {
        internal static readonly UTF8Encoding Utf8WithoutPreamble = new UTF8Encoding(false);

        protected CanonicalFormWriter()
        {
        }

        protected static void Base64EncodeAndWrite(Stream stream, byte[] workBuffer, char[] base64WorkBuffer, byte[] data)
        {
            if ((((data.Length / 3) * 4) + 4) > base64WorkBuffer.Length)
            {
                EncodeAndWrite(stream, Convert.ToBase64String(data));
            }
            else
            {
                int count = Convert.ToBase64CharArray(data, 0, data.Length, base64WorkBuffer, 0, Base64FormattingOptions.None);
                EncodeAndWrite(stream, workBuffer, base64WorkBuffer, count);
            }
        }

        private static void EncodeAndWrite(Stream stream, string s)
        {
            byte[] bytes = Utf8WithoutPreamble.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void EncodeAndWrite(Stream stream, byte[] workBuffer, string s)
        {
            if (s.Length > workBuffer.Length)
            {
                EncodeAndWrite(stream, s);
            }
            else
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char ch = s[i];
                    if (ch < '\x007f')
                    {
                        workBuffer[i] = (byte) ch;
                    }
                    else
                    {
                        EncodeAndWrite(stream, s);
                        return;
                    }
                }
                stream.Write(workBuffer, 0, s.Length);
            }
        }

        protected static void EncodeAndWrite(Stream stream, byte[] workBuffer, char[] chars)
        {
            EncodeAndWrite(stream, workBuffer, chars, chars.Length);
        }

        private static void EncodeAndWrite(Stream stream, char[] chars, int count)
        {
            byte[] buffer = Utf8WithoutPreamble.GetBytes(chars, 0, count);
            stream.Write(buffer, 0, buffer.Length);
        }

        protected static void EncodeAndWrite(Stream stream, byte[] workBuffer, char[] chars, int count)
        {
            if (count > workBuffer.Length)
            {
                EncodeAndWrite(stream, chars, count);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    char ch = chars[i];
                    if (ch < '\x007f')
                    {
                        workBuffer[i] = (byte) ch;
                    }
                    else
                    {
                        EncodeAndWrite(stream, chars, count);
                        return;
                    }
                }
                stream.Write(workBuffer, 0, count);
            }
        }
    }
}

