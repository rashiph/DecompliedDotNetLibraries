namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Text;

    internal static class MonikerUtility
    {
        internal static string Getkeyword(string moniker, out MonikerHelper.MonikerAttribute keyword)
        {
            moniker = moniker.TrimStart(new char[0]);
            int index = moniker.IndexOf("=", StringComparison.Ordinal);
            if (index == -1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("NoEqualSignFound", new object[] { moniker })));
            }
            int num2 = moniker.IndexOf(",", StringComparison.Ordinal);
            if ((num2 != -1) && (num2 < index))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("NoEqualSignFound", new object[] { moniker })));
            }
            string str = moniker.Substring(0, index).Trim().ToLower(CultureInfo.InvariantCulture);
            foreach (MonikerHelper.KeywordInfo info in MonikerHelper.KeywordInfo.KeywordCollection)
            {
                if (str == info.Name)
                {
                    keyword = info.Attrib;
                    moniker = moniker.Substring(index + 1).TrimStart(new char[0]);
                    return moniker;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("UnknownMonikerKeyword", new object[] { str })));
        }

        internal static string GetValue(string moniker, out string val)
        {
            StringBuilder builder = new StringBuilder();
            int startIndex = 0;
            moniker = moniker.Trim();
            if (string.IsNullOrEmpty(moniker))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("KewordMissingValue")));
            }
            char ch2 = moniker[startIndex];
            if ((ch2 != '"') && (ch2 != '\''))
            {
                while ((startIndex < moniker.Length) && (moniker[startIndex] != ','))
                {
                    builder.Append(moniker[startIndex]);
                    startIndex++;
                }
                if (startIndex < moniker.Length)
                {
                    startIndex++;
                    if (startIndex < moniker.Length)
                    {
                        moniker = moniker.Substring(startIndex);
                        moniker = moniker.Trim();
                    }
                }
                else
                {
                    moniker = "";
                }
            }
            else
            {
                char ch = moniker[startIndex];
                startIndex++;
                while (startIndex < moniker.Length)
                {
                    if (moniker[startIndex] == ch)
                    {
                        if ((startIndex >= (moniker.Length - 1)) || (moniker[startIndex + 1] != ch))
                        {
                            break;
                        }
                        builder.Append(ch);
                        startIndex++;
                    }
                    else
                    {
                        builder.Append(moniker[startIndex]);
                    }
                    startIndex++;
                }
                if (startIndex >= moniker.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MissingQuote", new object[] { builder.ToString() })));
                }
                startIndex++;
                if (startIndex < moniker.Length)
                {
                    moniker = moniker.Substring(startIndex);
                    moniker = moniker.Trim();
                    if (!string.IsNullOrEmpty(moniker))
                    {
                        if (moniker[0] != ',')
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("BadlyTerminatedValue", new object[] { builder.ToString() })));
                        }
                        moniker = moniker.Substring(1);
                        moniker = moniker.Trim();
                    }
                }
                else
                {
                    moniker = "";
                }
            }
            val = builder.ToString().Trim();
            return moniker;
        }

        internal static void Parse(string displayName, ref Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            int index = displayName.IndexOf(":", StringComparison.Ordinal);
            if (index == -1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerMissingColon")));
            }
            string keyword = displayName.Substring(index + 1).Trim();
            while (!string.IsNullOrEmpty(keyword))
            {
                MonikerHelper.MonikerAttribute attribute;
                string str2;
                keyword = Getkeyword(keyword, out attribute);
                propertyTable.TryGetValue(attribute, out str2);
                if (!string.IsNullOrEmpty(str2))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("RepeatedKeyword")));
                }
                keyword = GetValue(keyword, out str2);
                propertyTable[attribute] = str2;
            }
        }
    }
}

