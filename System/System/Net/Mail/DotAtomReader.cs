namespace System.Net.Mail
{
    using System;
    using System.Net.Mime;

    internal static class DotAtomReader
    {
        internal static int ReadReverse(string data, int index)
        {
            int num = index;
            while (0 <= index)
            {
                if ((data[index] > MailBnfHelper.Ascii7bitMaxValue) || ((data[index] != MailBnfHelper.Dot) && !MailBnfHelper.Atext[data[index]]))
                {
                    break;
                }
                index--;
            }
            if (num == index)
            {
                throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { data[index] }));
            }
            if (data[index + 1] == MailBnfHelper.Dot)
            {
                throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { MailBnfHelper.Dot }));
            }
            return index;
        }
    }
}

