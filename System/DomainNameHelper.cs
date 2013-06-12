namespace System
{
    using System.Globalization;

    internal class DomainNameHelper
    {
        private const char c_DummyChar = '￿';
        internal const string Localhost = "localhost";
        internal const string Loopback = "loopback";

        private DomainNameHelper()
        {
        }

        internal static unsafe string IdnEquivalent(char* hostname, int start, int end, ref bool allAscii, ref bool atLeastOneValidIdn)
        {
            string bidiStrippedHost = null;
            string str2 = IdnEquivalent(hostname, start, end, ref allAscii, ref bidiStrippedHost);
            if (str2 == null)
            {
                atLeastOneValidIdn = false;
                return str2;
            }
            string str3 = allAscii ? str2 : bidiStrippedHost;
            fixed (char* str4 = ((char*) str3))
            {
                char* input = str4;
                int length = str3.Length;
                int index = 0;
                int startIndex = 0;
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                do
                {
                    flag = false;
                    flag2 = false;
                    flag3 = false;
                    index = startIndex;
                    while (index < length)
                    {
                        char ch = input[index];
                        if (!flag2)
                        {
                            flag2 = true;
                            if (((index + 3) < length) && IsIdnAce(input, index))
                            {
                                index += 4;
                                flag = true;
                                continue;
                            }
                        }
                        if (((ch == '.') || (ch == '。')) || ((ch == 0xff0e) || (ch == 0xff61)))
                        {
                            flag3 = true;
                            break;
                        }
                        index++;
                    }
                    if (flag)
                    {
                        try
                        {
                            new IdnMapping().GetUnicode(new string(input, startIndex, index - startIndex));
                            atLeastOneValidIdn = true;
                            goto Label_00F2;
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                    startIndex = index + (flag3 ? 1 : 0);
                }
                while (startIndex < length);
            }
        Label_00F2:;
            return str2;
        }

        internal static unsafe string IdnEquivalent(char* hostname, int start, int end, ref bool allAscii, ref string bidiStrippedHost)
        {
            string str = null;
            string ascii;
            if (end <= start)
            {
                return str;
            }
            int index = start;
            allAscii = true;
            while (index < end)
            {
                if (hostname[index] > '\x007f')
                {
                    allAscii = false;
                    break;
                }
                index++;
            }
            if (allAscii)
            {
                string str2 = new string(hostname, start, end - start);
                if (str2 == null)
                {
                    return null;
                }
                return str2.ToLowerInvariant();
            }
            IdnMapping mapping = new IdnMapping();
            bidiStrippedHost = Uri.StripBidiControlCharacter(hostname, start, end - start);
            try
            {
                ascii = mapping.GetAscii(bidiStrippedHost);
            }
            catch (ArgumentException)
            {
                throw new UriFormatException(SR.GetString("net_uri_BadUnicodeHostForIdn"));
            }
            return ascii;
        }

        private static bool IsASCIILetter(char character, ref bool notCanonical)
        {
            if ((character < 'a') || (character > 'z'))
            {
                if ((character < 'A') || (character > 'Z'))
                {
                    return false;
                }
                if (!notCanonical)
                {
                    notCanonical = true;
                }
            }
            return true;
        }

        private static bool IsASCIILetterOrDigit(char character, ref bool notCanonical)
        {
            if (((character >= 'a') && (character <= 'z')) || ((character >= '0') && (character <= '9')))
            {
                return true;
            }
            if ((character >= 'A') && (character <= 'Z'))
            {
                notCanonical = true;
                return true;
            }
            return false;
        }

        private static bool IsIdnAce(string input, int index)
        {
            return (((input[index] == 'x') && (input[index + 1] == 'n')) && ((input[index + 2] == '-') && (input[index + 3] == '-')));
        }

        private static unsafe bool IsIdnAce(char* input, int index)
        {
            return (((input[index] == 'x') && (input[index + 1] == 'n')) && ((input[index + 2] == '-') && (input[index + 3] == '-')));
        }

        internal static unsafe bool IsValid(char* name, ushort pos, ref int returnedEnd, ref bool notCanonical, bool notImplicitFile)
        {
            char* chPtr = name + pos;
            char* chPtr2 = chPtr;
            char* chPtr3 = name + returnedEnd;
            while (chPtr2 < chPtr3)
            {
                char ch = chPtr2[0];
                if (ch > '\x007f')
                {
                    return false;
                }
                if (((ch == '/') || (ch == '\\')) || (notImplicitFile && (((ch == ':') || (ch == '?')) || (ch == '#'))))
                {
                    chPtr3 = chPtr2;
                    break;
                }
                chPtr2++;
            }
            if (chPtr3 == chPtr)
            {
                return false;
            }
        Label_004D:
            chPtr2 = chPtr;
            while (chPtr2 < chPtr3)
            {
                if (chPtr2[0] == '.')
                {
                    break;
                }
                chPtr2++;
            }
            if ((chPtr != chPtr2) && (((long) ((chPtr2 - chPtr) / 2)) <= 0x3fL))
            {
                chPtr++;
                if (IsASCIILetterOrDigit(chPtr[0], ref notCanonical))
                {
                    while (chPtr < chPtr2)
                    {
                        chPtr++;
                        if (!IsValidDomainLabelCharacter(chPtr[0], ref notCanonical))
                        {
                            return false;
                        }
                    }
                    chPtr++;
                    if (chPtr < chPtr3)
                    {
                        goto Label_004D;
                    }
                    returnedEnd = (ushort) ((long) ((chPtr3 - name) / 2));
                    return true;
                }
            }
            return false;
        }

        internal static unsafe bool IsValidByIri(char* name, ushort pos, ref int returnedEnd, ref bool notCanonical, bool notImplicitFile)
        {
            char* chPtr = name + pos;
            char* chPtr2 = chPtr;
            char* chPtr3 = name + returnedEnd;
            int num = 0;
            while (chPtr2 < chPtr3)
            {
                char ch = chPtr2[0];
                if (((ch == '/') || (ch == '\\')) || (notImplicitFile && (((ch == ':') || (ch == '?')) || (ch == '#'))))
                {
                    chPtr3 = chPtr2;
                    break;
                }
                chPtr2++;
            }
            if (chPtr3 == chPtr)
            {
                return false;
            }
        Label_004E:
            chPtr2 = chPtr;
            num = 0;
            bool flag = false;
            while (chPtr2 < chPtr3)
            {
                if (((chPtr2[0] == '.') || (chPtr2[0] == '。')) || ((chPtr2[0] == 0xff0e) || (chPtr2[0] == 0xff61)))
                {
                    break;
                }
                num++;
                if (chPtr2[0] > '\x00ff')
                {
                    num++;
                }
                if (chPtr2[0] >= '\x00a0')
                {
                    flag = true;
                }
                chPtr2++;
            }
            if ((chPtr != chPtr2) && ((flag ? (num + 4) : num) <= 0x3f))
            {
                chPtr++;
                if ((chPtr[0] >= '\x00a0') || IsASCIILetterOrDigit(*(chPtr - 1), ref notCanonical))
                {
                    while (chPtr < chPtr2)
                    {
                        chPtr++;
                        if ((chPtr[0] < '\x00a0') && !IsValidDomainLabelCharacter(*(chPtr - 1), ref notCanonical))
                        {
                            return false;
                        }
                    }
                    chPtr++;
                    if (chPtr < chPtr3)
                    {
                        goto Label_004E;
                    }
                    returnedEnd = (ushort) ((long) ((chPtr3 - name) / 2));
                    return true;
                }
            }
            return false;
        }

        private static bool IsValidDomainLabelCharacter(char character, ref bool notCanonical)
        {
            if ((((character >= 'a') && (character <= 'z')) || ((character >= '0') && (character <= '9'))) || ((character == '-') || (character == '_')))
            {
                return true;
            }
            if ((character >= 'A') && (character <= 'Z'))
            {
                notCanonical = true;
                return true;
            }
            return false;
        }

        internal static string ParseCanonicalName(string str, int start, int end, ref bool loopback)
        {
            string str2 = null;
            for (int i = end - 1; i >= start; i--)
            {
                if ((str[i] >= 'A') && (str[i] <= 'Z'))
                {
                    str2 = str.Substring(start, end - start).ToLower(CultureInfo.InvariantCulture);
                    break;
                }
                if (str[i] == ':')
                {
                    end = i;
                }
            }
            if (str2 == null)
            {
                str2 = str.Substring(start, end - start);
            }
            if (!(str2 == "localhost") && !(str2 == "loopback"))
            {
                return str2;
            }
            loopback = true;
            return "localhost";
        }

        internal static unsafe string UnicodeEquivalent(string idnHost, char* hostname, int start, int end)
        {
            IdnMapping mapping = new IdnMapping();
            try
            {
                return mapping.GetUnicode(idnHost);
            }
            catch (ArgumentException)
            {
            }
            bool allAscii = true;
            return UnicodeEquivalent(hostname, start, end, ref allAscii, ref allAscii);
        }

        internal static unsafe string UnicodeEquivalent(char* hostname, int start, int end, ref bool allAscii, ref bool atLeastOneValidIdn)
        {
            IdnMapping mapping = new IdnMapping();
            allAscii = true;
            atLeastOneValidIdn = false;
            string str = null;
            if (end <= start)
            {
                return str;
            }
            string input = Uri.StripBidiControlCharacter(hostname, start, end - start);
            string str3 = null;
            int startIndex = 0;
            int index = 0;
            int length = input.Length;
            bool flag = true;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            do
            {
                flag = true;
                flag2 = false;
                flag3 = false;
                flag4 = false;
                index = startIndex;
                while (index < length)
                {
                    char ch = input[index];
                    if (!flag3)
                    {
                        flag3 = true;
                        if ((((index + 3) < length) && (ch == 'x')) && IsIdnAce(input, index))
                        {
                            flag2 = true;
                        }
                    }
                    if (flag && (ch > '\x007f'))
                    {
                        flag = false;
                        allAscii = false;
                    }
                    if (((ch == '.') || (ch == '。')) || ((ch == 0xff0e) || (ch == 0xff61)))
                    {
                        flag4 = true;
                        break;
                    }
                    index++;
                }
                if (!flag)
                {
                    string unicode = input.Substring(startIndex, index - startIndex);
                    try
                    {
                        unicode = mapping.GetAscii(unicode);
                    }
                    catch (ArgumentException)
                    {
                        throw new UriFormatException(SR.GetString("net_uri_BadUnicodeHostForIdn"));
                    }
                    str3 = str3 + mapping.GetUnicode(unicode);
                    if (flag4)
                    {
                        str3 = str3 + ".";
                    }
                }
                else
                {
                    bool flag5 = false;
                    if (flag2)
                    {
                        try
                        {
                            str3 = str3 + mapping.GetUnicode(input.Substring(startIndex, index - startIndex));
                            if (flag4)
                            {
                                str3 = str3 + ".";
                            }
                            flag5 = true;
                            atLeastOneValidIdn = true;
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                    if (!flag5)
                    {
                        str3 = str3 + input.Substring(startIndex, index - startIndex).ToLowerInvariant();
                        if (flag4)
                        {
                            str3 = str3 + ".";
                        }
                    }
                }
                startIndex = index + (flag4 ? 1 : 0);
            }
            while (startIndex < length);
            return str3;
        }
    }
}

