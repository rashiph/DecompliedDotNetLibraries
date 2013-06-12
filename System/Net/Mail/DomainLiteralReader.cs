namespace System.Net.Mail
{
    using System;
    using System.Net.Mime;

    internal static class DomainLiteralReader
    {
        internal static int ReadReverse(string data, int index)
        {
            index--;
            do
            {
                index = WhitespaceReader.ReadFwsReverse(data, index);
                if (index < 0)
                {
                    break;
                }
                int num = QuotedPairReader.CountQuotedChars(data, index, false);
                if (num > 0)
                {
                    index -= num;
                }
                else
                {
                    if (data[index] == MailBnfHelper.StartSquareBracket)
                    {
                        return (index - 1);
                    }
                    if ((data[index] > MailBnfHelper.Ascii7bitMaxValue) || !MailBnfHelper.Dtext[data[index]])
                    {
                        throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[index] }));
                    }
                    index--;
                }
            }
            while (index >= 0);
            throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { MailBnfHelper.EndSquareBracket }));
        }
    }
}

