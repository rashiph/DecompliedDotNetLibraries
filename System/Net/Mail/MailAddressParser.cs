namespace System.Net.Mail
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mime;

    internal static class MailAddressParser
    {
        internal static MailAddress ParseAddress(string data)
        {
            int index = data.Length - 1;
            return ParseAddress(data, false, ref index);
        }

        private static MailAddress ParseAddress(string data, bool expectMultipleAddresses, ref int index)
        {
            string domain = null;
            string userName = null;
            string displayName = null;
            index = ReadCfwsAndThrowIfIncomplete(data, index);
            bool expectAngleBracket = false;
            if (data[index] == MailBnfHelper.EndAngleBracket)
            {
                expectAngleBracket = true;
                index--;
            }
            domain = ParseDomain(data, ref index);
            if (data[index] != MailBnfHelper.At)
            {
                throw new FormatException(SR.GetString("MailAddressInvalidFormat"));
            }
            index--;
            userName = ParseLocalPart(data, ref index, expectAngleBracket, expectMultipleAddresses);
            if (expectAngleBracket)
            {
                if ((index < 0) || (data[index] != MailBnfHelper.StartAngleBracket))
                {
                    throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { (index >= 0) ? data[index] : MailBnfHelper.EndAngleBracket }));
                }
                index--;
                index = WhitespaceReader.ReadFwsReverse(data, index);
            }
            if ((index >= 0) && (!expectMultipleAddresses || (data[index] != MailBnfHelper.Comma)))
            {
                displayName = ParseDisplayName(data, ref index, expectMultipleAddresses);
            }
            else
            {
                displayName = string.Empty;
            }
            return new MailAddress(displayName, userName, domain);
        }

        private static string ParseDisplayName(string data, ref int index, bool expectMultipleAddresses)
        {
            int num = WhitespaceReader.ReadCfwsReverse(data, index);
            if ((num >= 0) && (data[num] == MailBnfHelper.Quote))
            {
                index = QuotedStringFormatReader.ReadReverseQuoted(data, num, true);
                int startIndex = index + 2;
                string str = data.Substring(startIndex, num - startIndex);
                index = WhitespaceReader.ReadCfwsReverse(data, index);
                if ((index >= 0) && (!expectMultipleAddresses || (data[index] != MailBnfHelper.Comma)))
                {
                    throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[index] }));
                }
                return str;
            }
            int num3 = index;
            index = QuotedStringFormatReader.ReadReverseUnQuoted(data, index, true, expectMultipleAddresses);
            return data.Substring(index + 1, num3 - index).Trim();
        }

        private static string ParseDomain(string data, ref int index)
        {
            index = ReadCfwsAndThrowIfIncomplete(data, index);
            int num = index;
            if (data[index] == MailBnfHelper.EndSquareBracket)
            {
                index = DomainLiteralReader.ReadReverse(data, index);
            }
            else
            {
                index = DotAtomReader.ReadReverse(data, index);
            }
            string str = data.Substring(index + 1, num - index);
            index = ReadCfwsAndThrowIfIncomplete(data, index);
            return str;
        }

        private static string ParseLocalPart(string data, ref int index, bool expectAngleBracket, bool expectMultipleAddresses)
        {
            index = ReadCfwsAndThrowIfIncomplete(data, index);
            int num = index;
            if (data[index] == MailBnfHelper.Quote)
            {
                index = QuotedStringFormatReader.ReadReverseQuoted(data, index, false);
            }
            else
            {
                index = DotAtomReader.ReadReverse(data, index);
                if ((((((index >= 0) && !MailBnfHelper.Whitespace.Contains(data[index])) && (data[index] != MailBnfHelper.EndComment)) && (!expectAngleBracket || (data[index] != MailBnfHelper.StartAngleBracket))) && (!expectMultipleAddresses || (data[index] != MailBnfHelper.Comma))) && (data[index] != MailBnfHelper.Quote))
                {
                    throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[index] }));
                }
            }
            string str = data.Substring(index + 1, num - index);
            index = WhitespaceReader.ReadCfwsReverse(data, index);
            return str;
        }

        internal static IList<MailAddress> ParseMultipleAddresses(string data)
        {
            IList<MailAddress> list = new List<MailAddress>();
            for (int i = data.Length - 1; i >= 0; i--)
            {
                list.Insert(0, ParseAddress(data, true, ref i));
            }
            return list;
        }

        private static int ReadCfwsAndThrowIfIncomplete(string data, int index)
        {
            index = WhitespaceReader.ReadCfwsReverse(data, index);
            if (index < 0)
            {
                throw new FormatException(SR.GetString("MailAddressInvalidFormat"));
            }
            return index;
        }
    }
}

