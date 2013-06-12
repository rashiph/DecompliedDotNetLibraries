namespace System.Net.Mime
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class MailBnfHelper
    {
        internal static readonly int Ascii7bitMaxValue = 0x7f;
        internal static readonly char At = '@';
        internal static bool[] Atext = new bool[0x80];
        internal static readonly char Backslash = '\\';
        internal static readonly char Comma = ',';
        internal static readonly char CR = '\r';
        internal static bool[] Ctext = new bool[0x80];
        internal static readonly char Dot = '.';
        internal static bool[] Dtext = new bool[0x80];
        internal static readonly char EndAngleBracket = '>';
        internal static readonly char EndComment = ')';
        internal static readonly char EndSquareBracket = ']';
        internal static bool[] Ftext = new bool[0x80];
        internal static readonly char LF = '\n';
        internal static bool[] Qtext = new bool[0x80];
        internal static readonly char Quote = '"';
        private static string[] s_months;
        internal static readonly char Space = ' ';
        internal static readonly char StartAngleBracket = '<';
        internal static readonly char StartComment = '(';
        internal static readonly char StartSquareBracket = '[';
        internal static readonly char Tab = '\t';
        internal static bool[] Ttext = new bool[0x80];
        internal static readonly IList<char> Whitespace;

        static MailBnfHelper()
        {
            string[] strArray = new string[13];
            strArray[1] = "Jan";
            strArray[2] = "Feb";
            strArray[3] = "Mar";
            strArray[4] = "Apr";
            strArray[5] = "May";
            strArray[6] = "Jun";
            strArray[7] = "Jul";
            strArray[8] = "Aug";
            strArray[9] = "Sep";
            strArray[10] = "Oct";
            strArray[11] = "Nov";
            strArray[12] = "Dec";
            s_months = strArray;
            Whitespace = new List<char>();
            Whitespace.Add(Tab);
            Whitespace.Add(Space);
            Whitespace.Add(CR);
            Whitespace.Add(LF);
            for (int i = 0x30; i <= 0x39; i++)
            {
                Atext[i] = true;
            }
            for (int j = 0x41; j <= 90; j++)
            {
                Atext[j] = true;
            }
            for (int k = 0x61; k <= 0x7a; k++)
            {
                Atext[k] = true;
            }
            Atext[0x21] = true;
            Atext[0x23] = true;
            Atext[0x24] = true;
            Atext[0x25] = true;
            Atext[0x26] = true;
            Atext[0x27] = true;
            Atext[0x2a] = true;
            Atext[0x2b] = true;
            Atext[0x2d] = true;
            Atext[0x2f] = true;
            Atext[0x3d] = true;
            Atext[0x3f] = true;
            Atext[0x5e] = true;
            Atext[0x5f] = true;
            Atext[0x60] = true;
            Atext[0x7b] = true;
            Atext[0x7c] = true;
            Atext[0x7d] = true;
            Atext[0x7e] = true;
            for (int m = 1; m <= 9; m++)
            {
                Qtext[m] = true;
            }
            Qtext[11] = true;
            Qtext[12] = true;
            for (int n = 14; n <= 0x21; n++)
            {
                Qtext[n] = true;
            }
            for (int num6 = 0x23; num6 <= 0x5b; num6++)
            {
                Qtext[num6] = true;
            }
            for (int num7 = 0x5d; num7 <= 0x7f; num7++)
            {
                Qtext[num7] = true;
            }
            for (int num8 = 1; num8 <= 8; num8++)
            {
                Dtext[num8] = true;
            }
            Dtext[11] = true;
            Dtext[12] = true;
            for (int num9 = 14; num9 <= 0x1f; num9++)
            {
                Dtext[num9] = true;
            }
            for (int num10 = 0x21; num10 <= 90; num10++)
            {
                Dtext[num10] = true;
            }
            for (int num11 = 0x5e; num11 <= 0x7f; num11++)
            {
                Dtext[num11] = true;
            }
            for (int num12 = 0x21; num12 <= 0x39; num12++)
            {
                Ftext[num12] = true;
            }
            for (int num13 = 0x3b; num13 <= 0x7e; num13++)
            {
                Ftext[num13] = true;
            }
            for (int num14 = 0x21; num14 <= 0x7e; num14++)
            {
                Ttext[num14] = true;
            }
            Ttext[40] = false;
            Ttext[0x29] = false;
            Ttext[60] = false;
            Ttext[0x3e] = false;
            Ttext[0x40] = false;
            Ttext[0x2c] = false;
            Ttext[0x3b] = false;
            Ttext[0x3a] = false;
            Ttext[0x5c] = false;
            Ttext[0x22] = false;
            Ttext[0x2f] = false;
            Ttext[0x5b] = false;
            Ttext[0x5d] = false;
            Ttext[0x3f] = false;
            Ttext[0x3d] = false;
            for (int num15 = 1; num15 <= 8; num15++)
            {
                Ctext[num15] = true;
            }
            Ctext[11] = true;
            Ctext[12] = true;
            for (int num16 = 14; num16 <= 0x1f; num16++)
            {
                Ctext[num16] = true;
            }
            for (int num17 = 0x21; num17 <= 0x27; num17++)
            {
                Ctext[num17] = true;
            }
            for (int num18 = 0x2a; num18 <= 0x5b; num18++)
            {
                Ctext[num18] = true;
            }
            for (int num19 = 0x5d; num19 <= 0x7f; num19++)
            {
                Ctext[num19] = true;
            }
        }

        internal static string GetDateTimeString(DateTime value, StringBuilder builder)
        {
            StringBuilder builder2 = (builder != null) ? builder : new StringBuilder();
            builder2.Append(value.Day);
            builder2.Append(' ');
            builder2.Append(s_months[value.Month]);
            builder2.Append(' ');
            builder2.Append(value.Year);
            builder2.Append(' ');
            if (value.Hour <= 9)
            {
                builder2.Append('0');
            }
            builder2.Append(value.Hour);
            builder2.Append(':');
            if (value.Minute <= 9)
            {
                builder2.Append('0');
            }
            builder2.Append(value.Minute);
            builder2.Append(':');
            if (value.Second <= 9)
            {
                builder2.Append('0');
            }
            builder2.Append(value.Second);
            string str = TimeZone.CurrentTimeZone.GetUtcOffset(value).ToString();
            if (str[0] != '-')
            {
                builder2.Append(" +");
            }
            else
            {
                builder2.Append(" ");
            }
            string[] strArray = str.Split(new char[] { ':' });
            builder2.Append(strArray[0]);
            builder2.Append(strArray[1]);
            if (builder == null)
            {
                return builder2.ToString();
            }
            return null;
        }

        internal static string GetTokenOrQuotedString(string data, StringBuilder builder)
        {
            int num = 0;
            int startIndex = 0;
            while (num < data.Length)
            {
                if (data[num] > Ascii7bitMaxValue)
                {
                    throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[num] }));
                }
                if (!Ttext[data[num]] || (data[num] == ' '))
                {
                    StringBuilder builder2 = (builder != null) ? builder : new StringBuilder();
                    builder.Append('"');
                    while (num < data.Length)
                    {
                        if (data[num] > Ascii7bitMaxValue)
                        {
                            throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[num] }));
                        }
                        if (!Qtext[data[num]])
                        {
                            builder.Append(data, startIndex, num - startIndex);
                            builder.Append('\\');
                            startIndex = num;
                        }
                        num++;
                    }
                    builder.Append(data, startIndex, num - startIndex);
                    builder.Append('"');
                    if (builder == null)
                    {
                        return builder2.ToString();
                    }
                    return null;
                }
                num++;
            }
            if (data.Length == 0)
            {
                if (builder == null)
                {
                    return "\"\"";
                }
                builder.Append("\"\"");
            }
            if (builder != null)
            {
                builder.Append(data);
                return null;
            }
            return data;
        }

        internal static bool HasCROrLF(string data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if ((data[i] == '\r') || (data[i] == '\n'))
                {
                    return true;
                }
            }
            return false;
        }

        internal static string ReadParameterAttribute(string data, ref int offset, StringBuilder builder)
        {
            if (!SkipCFWS(data, ref offset))
            {
                return null;
            }
            return ReadToken(data, ref offset, null);
        }

        internal static string ReadQuotedString(string data, ref int offset, StringBuilder builder)
        {
            return ReadQuotedString(data, ref offset, builder, false, false);
        }

        internal static string ReadQuotedString(string data, ref int offset, StringBuilder builder, bool doesntRequireQuotes, bool permitUnicodeInDisplayName)
        {
            if (!doesntRequireQuotes)
            {
                offset++;
            }
            int startIndex = offset;
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
                    if ((((data[offset] == '=') && (data.Length > (offset + 3))) && ((data[offset + 1] == '\r') && (data[offset + 2] == '\n'))) && ((data[offset + 3] == ' ') || (data[offset + 3] == '\t')))
                    {
                        offset += 3;
                    }
                    else if (permitUnicodeInDisplayName)
                    {
                        if ((data[offset] <= Ascii7bitMaxValue) && !Qtext[data[offset]])
                        {
                            throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[offset] }));
                        }
                    }
                    else if ((data[offset] > Ascii7bitMaxValue) || !Qtext[data[offset]])
                    {
                        throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[offset] }));
                    }
                }
                offset++;
            }
            if (!doesntRequireQuotes)
            {
                throw new FormatException(SR.GetString("MailHeaderFieldMalformedHeader"));
            }
            builder2.Append(data, startIndex, offset - startIndex);
            if (builder == null)
            {
                return builder2.ToString();
            }
            return null;
        }

        internal static string ReadToken(string data, ref int offset, StringBuilder builder)
        {
            int startIndex = offset;
            while (offset < data.Length)
            {
                if (data[offset] > Ascii7bitMaxValue)
                {
                    throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[offset] }));
                }
                if (!Ttext[data[offset]])
                {
                    break;
                }
                offset++;
            }
            if (startIndex == offset)
            {
                throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[offset] }));
            }
            return data.Substring(startIndex, offset - startIndex);
        }

        internal static bool SkipCFWS(string data, ref int offset)
        {
            int num = 0;
            while (offset < data.Length)
            {
                if (data[offset] > '\x007f')
                {
                    throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[offset] }));
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
                if (num < 0)
                {
                    throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[offset] }));
                }
                offset++;
            }
            return false;
        }

        internal static void ValidateHeaderName(string data)
        {
            int num = 0;
            while (num < data.Length)
            {
                if ((data[num] > Ftext.Length) || !Ftext[data[num]])
                {
                    throw new FormatException(SR.GetString("InvalidHeaderName"));
                }
                num++;
            }
            if (num == 0)
            {
                throw new FormatException(SR.GetString("InvalidHeaderName"));
            }
        }
    }
}

