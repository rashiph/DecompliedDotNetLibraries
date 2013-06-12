namespace System.Net.Mail
{
    using System;
    using System.Net.Mime;

    internal static class QuotedStringFormatReader
    {
        private static bool IsValidQtext(bool allowUnicode, char ch)
        {
            if (ch > MailBnfHelper.Ascii7bitMaxValue)
            {
                return allowUnicode;
            }
            return MailBnfHelper.Qtext[ch];
        }

        internal static int ReadReverseQuoted(string data, int index, bool permitUnicode)
        {
            index--;
            do
            {
                index = WhitespaceReader.ReadFwsReverse(data, index);
                if (index < 0)
                {
                    break;
                }
                int num = QuotedPairReader.CountQuotedChars(data, index, permitUnicode);
                if (num > 0)
                {
                    index -= num;
                }
                else
                {
                    if (data[index] == MailBnfHelper.Quote)
                    {
                        return (index - 1);
                    }
                    if (!IsValidQtext(permitUnicode, data[index]))
                    {
                        throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[index] }));
                    }
                    index--;
                }
            }
            while (index >= 0);
            throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { MailBnfHelper.Quote }));
        }

        internal static int ReadReverseUnQuoted(string data, int index, bool permitUnicode, bool expectCommaDelimiter)
        {
            do
            {
                index = WhitespaceReader.ReadFwsReverse(data, index);
                if (index < 0)
                {
                    return index;
                }
                int num = QuotedPairReader.CountQuotedChars(data, index, permitUnicode);
                if (num > 0)
                {
                    index -= num;
                }
                else
                {
                    if (expectCommaDelimiter && (data[index] == MailBnfHelper.Comma))
                    {
                        return index;
                    }
                    if (!IsValidQtext(permitUnicode, data[index]))
                    {
                        throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[index] }));
                    }
                    index--;
                }
            }
            while (index >= 0);
            return index;
        }
    }
}

