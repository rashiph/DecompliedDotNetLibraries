namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Text;

    internal static class MailBnfHelper
    {
        private static bool[] s_boundary = new bool[0x80];
        private static bool[] s_digits = new bool[0x80];
        private static bool[] s_fqtext = new bool[0x80];
        private static bool[] s_ttext = new bool[0x80];

        static MailBnfHelper()
        {
            for (int i = 1; i <= 9; i++)
            {
                s_fqtext[i] = true;
            }
            s_fqtext[11] = true;
            s_fqtext[12] = true;
            for (int j = 14; j <= 0x21; j++)
            {
                s_fqtext[j] = true;
            }
            for (int k = 0x23; k <= 0x5b; k++)
            {
                s_fqtext[k] = true;
            }
            for (int m = 0x5d; m <= 0x7f; m++)
            {
                s_fqtext[m] = true;
            }
            for (int n = 0x21; n <= 0x7e; n++)
            {
                s_ttext[n] = true;
            }
            s_ttext[40] = false;
            s_ttext[0x29] = false;
            s_ttext[60] = false;
            s_ttext[0x3e] = false;
            s_ttext[0x40] = false;
            s_ttext[0x2c] = false;
            s_ttext[0x3b] = false;
            s_ttext[0x3a] = false;
            s_ttext[0x5c] = false;
            s_ttext[0x22] = false;
            s_ttext[0x2f] = false;
            s_ttext[0x5b] = false;
            s_ttext[0x5d] = false;
            s_ttext[0x3f] = false;
            s_ttext[0x3d] = false;
            for (int num6 = 0x30; num6 <= 0x39; num6++)
            {
                s_digits[num6] = true;
            }
            for (int num7 = 0x30; num7 <= 0x39; num7++)
            {
                s_boundary[num7] = true;
            }
            for (int num8 = 0x41; num8 <= 90; num8++)
            {
                s_boundary[num8] = true;
            }
            for (int num9 = 0x61; num9 <= 0x7a; num9++)
            {
                s_boundary[num9] = true;
            }
            s_boundary[0x27] = true;
            s_boundary[40] = true;
            s_boundary[0x29] = true;
            s_boundary[0x2b] = true;
            s_boundary[0x5f] = true;
            s_boundary[0x2c] = true;
            s_boundary[0x2d] = true;
            s_boundary[0x2e] = true;
            s_boundary[0x2f] = true;
            s_boundary[0x3a] = true;
            s_boundary[0x3d] = true;
            s_boundary[0x3f] = true;
            s_boundary[0x20] = true;
        }

        public static bool IsValidMimeBoundary(string data)
        {
            int num = (data == null) ? 0 : data.Length;
            if (((num == 0) || (num > 70)) || (data[num - 1] == ' '))
            {
                return false;
            }
            for (int i = 0; i < num; i++)
            {
                if ((data[i] >= s_boundary.Length) || !s_boundary[data[i]])
                {
                    return false;
                }
            }
            return true;
        }

        public static string ReadDigits(string data, ref int offset, StringBuilder builder)
        {
            int startIndex = offset;
            StringBuilder builder2 = (builder != null) ? builder : new StringBuilder();
            while (((offset < data.Length) && (data[offset] < s_digits.Length)) && s_digits[data[offset]])
            {
                offset++;
            }
            builder2.Append(data, startIndex, offset - startIndex);
            if (builder == null)
            {
                return builder2.ToString();
            }
            return null;
        }

        public static string ReadParameterAttribute(string data, ref int offset, StringBuilder builder)
        {
            if (!SkipCFWS(data, ref offset))
            {
                return null;
            }
            return ReadToken(data, ref offset, null);
        }

        public static string ReadParameterValue(string data, ref int offset, StringBuilder builder)
        {
            if (!SkipCFWS(data, ref offset))
            {
                return string.Empty;
            }
            if ((offset < data.Length) && (data[offset] == '"'))
            {
                return ReadQuotedString(data, ref offset, builder);
            }
            return ReadToken(data, ref offset, builder);
        }

        public static string ReadQuotedString(string data, ref int offset, StringBuilder builder)
        {
            int startIndex = ++offset;
            StringBuilder builder2 = (builder != null) ? builder : new StringBuilder();
            while (offset < data.Length)
            {
                if (data[offset] == '\\')
                {
                    builder2.Append(data, startIndex, offset - startIndex);
                    startIndex = ++offset;
                }
                else
                {
                    if (data[offset] == '"')
                    {
                        builder2.Append(data, startIndex, offset - startIndex);
                        offset++;
                        if (builder == null)
                        {
                            return builder2.ToString();
                        }
                        return null;
                    }
                    if ((data[offset] >= s_fqtext.Length) || !s_fqtext[data[offset]])
                    {
                        object[] args = new object[] { data[offset], ((int) data[offset]).ToString("X", CultureInfo.InvariantCulture) };
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeHeaderInvalidCharacter", args)));
                    }
                }
                offset++;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeReaderMalformedHeader")));
        }

        public static string ReadToken(string data, ref int offset, StringBuilder builder)
        {
            int startIndex = offset;
            while (offset < data.Length)
            {
                if (data[offset] > s_ttext.Length)
                {
                    object[] args = new object[] { data[offset], ((int) data[offset]).ToString("X", CultureInfo.InvariantCulture) };
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeHeaderInvalidCharacter", args)));
                }
                if (!s_ttext[data[offset]])
                {
                    break;
                }
                offset++;
            }
            return data.Substring(startIndex, offset - startIndex);
        }

        public static bool SkipCFWS(string data, ref int offset)
        {
            int num = 0;
            while (offset < data.Length)
            {
                if (data[offset] > '\x007f')
                {
                    object[] args = new object[] { data[offset], ((int) data[offset]).ToString("X", CultureInfo.InvariantCulture) };
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeHeaderInvalidCharacter", args)));
                }
                if ((data[offset] == '\\') && (num > 0))
                {
                    offset += 2;
                }
                else if (data[offset] == '(')
                {
                    num++;
                }
                else if (data[offset] == ')')
                {
                    num--;
                }
                else if (((data[offset] != ' ') && (data[offset] != '\t')) && (num == 0))
                {
                    return true;
                }
                offset++;
            }
            return false;
        }
    }
}

