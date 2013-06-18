namespace Microsoft.Build.Shared
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal static class ResourceUtilities
    {
        internal static string ExtractMessageCode(bool msbuildCodeOnly, string message, out string code)
        {
            ErrorUtilities.VerifyThrowInternalNull(message, "message");
            code = null;
            int startIndex = 0;
            while ((startIndex < message.Length) && char.IsWhiteSpace(message[startIndex]))
            {
                startIndex++;
            }
            if (msbuildCodeOnly)
            {
                if (((((message.Length < (startIndex + 8)) || (message[startIndex] != 'M')) || ((message[startIndex + 1] != 'S') || (message[startIndex + 2] != 'B'))) || (((message[startIndex + 3] < '0') || (message[startIndex + 3] > '9')) || ((message[startIndex + 4] < '0') || (message[startIndex + 4] > '9')))) || (((message[startIndex + 5] < '0') || (message[startIndex + 5] > '9')) || (((message[startIndex + 6] < '0') || (message[startIndex + 6] > '9')) || (message[startIndex + 7] != ':'))))
                {
                    return message;
                }
                code = message.Substring(startIndex, 7);
                startIndex += 8;
            }
            else
            {
                int num2 = startIndex;
                while (num2 < message.Length)
                {
                    char ch = message[num2];
                    if (((ch < 'a') || (ch > 'z')) && ((ch < 'A') || (ch > 'Z')))
                    {
                        break;
                    }
                    num2++;
                }
                if (num2 == startIndex)
                {
                    return message;
                }
                int num3 = num2;
                while (num3 < message.Length)
                {
                    char ch2 = message[num3];
                    if ((ch2 < '0') || (ch2 > '9'))
                    {
                        break;
                    }
                    num3++;
                }
                if (num3 == num2)
                {
                    return message;
                }
                if ((num3 == message.Length) || (message[num3] != ':'))
                {
                    return message;
                }
                code = message.Substring(startIndex, num3 - startIndex);
                startIndex = num3 + 1;
            }
            while ((startIndex < message.Length) && char.IsWhiteSpace(message[startIndex]))
            {
                startIndex++;
            }
            if (startIndex < message.Length)
            {
                message = message.Substring(startIndex, message.Length - startIndex);
            }
            return message;
        }

        internal static string FormatResourceString(string resourceName, params object[] args)
        {
            string str;
            string str2;
            return FormatResourceString(out str, out str2, resourceName, args);
        }

        internal static string FormatResourceString(out string code, out string helpKeyword, string resourceName, params object[] args)
        {
            helpKeyword = GetHelpKeyword(resourceName);
            return ExtractMessageCode(true, FormatString(GetResourceString(resourceName), args), out code);
        }

        internal static string FormatString(string unformatted, params object[] args)
        {
            string str = unformatted;
            if ((args != null) && (args.Length > 0))
            {
                str = string.Format(CultureInfo.CurrentCulture, unformatted, args);
            }
            return str;
        }

        private static string GetHelpKeyword(string resourceName)
        {
            return ("MSBuild." + resourceName);
        }

        internal static string GetResourceString(string resourceName)
        {
            return AssemblyResources.GetString(resourceName);
        }

        internal static void VerifyResourceStringExists(string resourceName)
        {
        }
    }
}

