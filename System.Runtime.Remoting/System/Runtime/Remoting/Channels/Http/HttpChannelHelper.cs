namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;

    internal static class HttpChannelHelper
    {
        private const string _http = "http://";
        private const string _https = "https://";
        private static char[] s_semicolonSeparator = new char[] { ';' };

        internal static int CharacterHexDigitToDecimal(byte b)
        {
            switch (((char) b))
            {
                case 'A':
                    return 10;

                case 'B':
                    return 11;

                case 'C':
                    return 12;

                case 'D':
                    return 13;

                case 'E':
                    return 14;

                case 'F':
                    return 15;
            }
            return (b - 0x30);
        }

        internal static char DecimalToCharacterHexDigit(int i)
        {
            switch (i)
            {
                case 10:
                    return 'A';

                case 11:
                    return 'B';

                case 12:
                    return 'C';

                case 13:
                    return 'D';

                case 14:
                    return 'E';

                case 15:
                    return 'F';
            }
            return (char) (i + 0x30);
        }

        internal static void DecodeUriInPlace(byte[] uriBytes, out int length)
        {
            int num = 0;
            int num2 = uriBytes.Length;
            length = num2;
            int index = 0;
            while (index < num2)
            {
                if (uriBytes[index] == 0x25)
                {
                    int num4 = index - (num * 2);
                    uriBytes[num4] = (byte) ((0x10 * CharacterHexDigitToDecimal(uriBytes[index + 1])) + CharacterHexDigitToDecimal(uriBytes[index + 2]));
                    num++;
                    length -= 2;
                    index += 3;
                }
                else
                {
                    if (num != 0)
                    {
                        int num5 = index - (num * 2);
                        uriBytes[num5] = uriBytes[index];
                    }
                    index++;
                }
            }
        }

        internal static string GetObjectUriFromRequestUri(string uri)
        {
            int index;
            int startIndex = 0;
            int length = uri.Length;
            startIndex = StartsWithHttp(uri);
            if (startIndex != -1)
            {
                index = uri.IndexOf('/', startIndex);
                if (index != -1)
                {
                    startIndex = index + 1;
                }
                else
                {
                    startIndex = length;
                }
            }
            else
            {
                startIndex = 0;
                if (uri[startIndex] == '/')
                {
                    startIndex++;
                }
            }
            index = uri.IndexOf('?');
            if (index != -1)
            {
                length = index;
            }
            if (startIndex < length)
            {
                return CoreChannel.RemoveApplicationNameFromUri(uri.Substring(startIndex, length - startIndex));
            }
            return "";
        }

        internal static void ParseContentType(string contentType, out string value, out string charset)
        {
            charset = null;
            if (contentType == null)
            {
                value = null;
            }
            else
            {
                string[] strArray = contentType.Split(s_semicolonSeparator);
                value = strArray[0];
                if (strArray.Length > 0)
                {
                    foreach (string str in strArray)
                    {
                        int index = str.IndexOf('=');
                        if ((index != -1) && (string.Compare(str.Substring(0, index).Trim(), "charset", StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            if ((index + 1) < str.Length)
                            {
                                charset = str.Substring(index + 1);
                            }
                            else
                            {
                                charset = null;
                            }
                            break;
                        }
                    }
                }
            }
        }

        internal static string ParseURL(string url, out string objectURI)
        {
            objectURI = null;
            int startIndex = StartsWithHttp(url);
            if (startIndex == -1)
            {
                return null;
            }
            startIndex = url.IndexOf('/', startIndex);
            if (-1 == startIndex)
            {
                return url;
            }
            string str = url.Substring(0, startIndex);
            objectURI = url.Substring(startIndex);
            return str;
        }

        internal static string ReplaceChannelUriWithThisString(string url, string channelUri)
        {
            string str;
            ParseURL(url, out str);
            return (channelUri + str);
        }

        internal static string ReplaceMachineNameWithThisString(string url, string newMachineName)
        {
            string str;
            string str2 = ParseURL(url, out str);
            int startIndex = StartsWithHttp(url);
            if (startIndex == -1)
            {
                return url;
            }
            int index = str2.IndexOf(':', startIndex);
            if (index == -1)
            {
                index = str2.Length;
            }
            return (url.Substring(0, startIndex) + newMachineName + url.Substring(index));
        }

        internal static int StartsWithHttp(string url)
        {
            int length = url.Length;
            if (StringHelper.StartsWithAsciiIgnoreCasePrefixLower(url, "http://"))
            {
                return "http://".Length;
            }
            if (StringHelper.StartsWithAsciiIgnoreCasePrefixLower(url, "https://"))
            {
                return "https://".Length;
            }
            return -1;
        }
    }
}

