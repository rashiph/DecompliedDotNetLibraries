namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Text;

    internal static class TextEncoderDefaults
    {
        internal static readonly CharSetEncoding[] CharSetEncodings = new CharSetEncoding[] { new CharSetEncoding("utf-8", System.Text.Encoding.UTF8), new CharSetEncoding("utf-16LE", System.Text.Encoding.Unicode), new CharSetEncoding("utf-16BE", System.Text.Encoding.BigEndianUnicode), new CharSetEncoding("utf-16", null), new CharSetEncoding(null, null) };
        internal static readonly System.Text.Encoding Encoding = System.Text.Encoding.GetEncoding("utf-8", new EncoderExceptionFallback(), new DecoderExceptionFallback());
        internal const string EncodingString = "utf-8";
        internal const string MessageVersionString = "Soap12WSAddressing10";
        internal static readonly System.Text.Encoding[] SupportedEncodings = new System.Text.Encoding[] { System.Text.Encoding.UTF8, System.Text.Encoding.Unicode, System.Text.Encoding.BigEndianUnicode };

        internal static string EncodingToCharSet(System.Text.Encoding encoding)
        {
            string webName = encoding.WebName;
            CharSetEncoding[] charSetEncodings = CharSetEncodings;
            for (int i = 0; i < charSetEncodings.Length; i++)
            {
                System.Text.Encoding encoding2 = charSetEncodings[i].Encoding;
                if ((encoding2 != null) && (encoding2.WebName == webName))
                {
                    return charSetEncodings[i].CharSet;
                }
            }
            return null;
        }

        internal static bool TryGetEncoding(string charSet, out System.Text.Encoding encoding)
        {
            CharSetEncoding[] charSetEncodings = CharSetEncodings;
            for (int i = 0; i < charSetEncodings.Length; i++)
            {
                if (charSetEncodings[i].CharSet == charSet)
                {
                    encoding = charSetEncodings[i].Encoding;
                    return true;
                }
            }
            for (int j = 0; j < charSetEncodings.Length; j++)
            {
                string str = charSetEncodings[j].CharSet;
                if ((str != null) && str.Equals(charSet, StringComparison.OrdinalIgnoreCase))
                {
                    encoding = charSetEncodings[j].Encoding;
                    return true;
                }
            }
            encoding = null;
            return false;
        }

        internal static void ValidateEncoding(System.Text.Encoding encoding)
        {
            string webName = encoding.WebName;
            System.Text.Encoding[] supportedEncodings = SupportedEncodings;
            for (int i = 0; i < supportedEncodings.Length; i++)
            {
                if (webName == supportedEncodings[i].WebName)
                {
                    return;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MessageTextEncodingNotSupported", new object[] { webName }), "encoding"));
        }

        internal class CharSetEncoding
        {
            internal string CharSet;
            internal System.Text.Encoding Encoding;

            internal CharSetEncoding(string charSet, System.Text.Encoding enc)
            {
                this.CharSet = charSet;
                this.Encoding = enc;
            }
        }
    }
}

