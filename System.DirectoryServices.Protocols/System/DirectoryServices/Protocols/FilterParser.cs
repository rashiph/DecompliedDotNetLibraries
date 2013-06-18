namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class FilterParser
    {
        private const string mAnyRE = @"(\*\s*((?<anyvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\*\s*)*)";
        private const string mAttrRE = @"(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*";
        private const string mDNAttrRE = @"(?<dnattr>\:dn){0,1}\s*";
        private const string mExtenAttrRE = @"(?<extenattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*";
        private const string mExtenRE = @"(?<extensible>(((?<extenattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*(?<dnattr>\:dn){0,1}\s*(\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\.[0-9]+)+))){0,1}\s*)|((?<dnattr>\:dn){0,1}\s*(\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\.[0-9]+)+)))\s*))\:\=\s*(?<extenvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\s*)\s*";
        private const string mExtenValueRE = @"(?<extenvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\s*";
        private static Regex mFilter = new Regex(@"^\s*\(\s*(((?<filtercomp>\!|\&|\|)\s*(?<filterlist>.+)\s*)|((?<item>(?<simple>(?<simpleattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*(?<filtertype>\=|\~\=|\>\=|\<\=)\s*(?<simplevalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\s*)\s*|(?<present>(?<presentattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\=\*)\s*|(?<substr>(?<substrattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*\=\s*\s*(?<initialvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\s*(\*\s*((?<anyvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\*\s*)*)\s*(?<finalvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\s*)\s*|(?<extensible>(((?<extenattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*(?<dnattr>\:dn){0,1}\s*(\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\.[0-9]+)+))){0,1}\s*)|((?<dnattr>\:dn){0,1}\s*(\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\.[0-9]+)+)))\s*))\:\=\s*(?<extenvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\s*)\s*)\s*))\)\s*$", RegexOptions.ExplicitCapture);
        private const string mFiltercompRE = @"(?<filtercomp>\!|\&|\|)\s*";
        private const string mFilterlistRE = @"(?<filterlist>.+)\s*";
        private const string mFilterRE = @"^\s*\(\s*(((?<filtercomp>\!|\&|\|)\s*(?<filterlist>.+)\s*)|((?<item>(?<simple>(?<simpleattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*(?<filtertype>\=|\~\=|\>\=|\<\=)\s*(?<simplevalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\s*)\s*|(?<present>(?<presentattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\=\*)\s*|(?<substr>(?<substrattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*\=\s*\s*(?<initialvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\s*(\*\s*((?<anyvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\*\s*)*)\s*(?<finalvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\s*)\s*|(?<extensible>(((?<extenattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*(?<dnattr>\:dn){0,1}\s*(\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\.[0-9]+)+))){0,1}\s*)|((?<dnattr>\:dn){0,1}\s*(\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\.[0-9]+)+)))\s*))\:\=\s*(?<extenvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\s*)\s*)\s*))\)\s*$";
        private const string mFiltertypeRE = @"(?<filtertype>\=|\~\=|\>\=|\<\=)\s*";
        private const string mFinalRE = @"\s*(?<finalvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\s*";
        private const string mInitialRE = @"\s*(?<initialvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\s*";
        private const string mItemRE = @"(?<item>(?<simple>(?<simpleattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*(?<filtertype>\=|\~\=|\>\=|\<\=)\s*(?<simplevalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\s*)\s*|(?<present>(?<presentattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\=\*)\s*|(?<substr>(?<substrattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*\=\s*\s*(?<initialvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\s*(\*\s*((?<anyvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\*\s*)*)\s*(?<finalvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\s*)\s*|(?<extensible>(((?<extenattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*(?<dnattr>\:dn){0,1}\s*(\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\.[0-9]+)+))){0,1}\s*)|((?<dnattr>\:dn){0,1}\s*(\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\.[0-9]+)+)))\s*))\:\=\s*(?<extenvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\s*)\s*)\s*";
        private const string mMatchRuleOptionalRE = @"(\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\.[0-9]+)+))){0,1}\s*";
        private const string mMatchRuleRE = @"(\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\.[0-9]+)+)))\s*";
        private const string mPresentRE = @"(?<present>(?<presentattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\=\*)\s*";
        private const string mSimpleAttrRE = @"(?<simpleattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*";
        private const string mSimpleRE = @"(?<simple>(?<simpleattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*(?<filtertype>\=|\~\=|\>\=|\<\=)\s*(?<simplevalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\s*)\s*";
        private const string mSimpleValueRE = @"(?<simplevalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\s*";
        private const string mSubstrAttrRE = @"(?<substrattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*";
        private const string mSubstrRE = @"(?<substr>(?<substrattr>(([0-2](\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\s*\=\s*\s*(?<initialvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\s*(\*\s*((?<anyvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?)\*\s*)*)\s*(?<finalvalue>(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\s*)\s*";
        private const string mValueRE = @"(([^\*\(\)\\])|(\\[a-fA-F0-9][a-fA-F0-9]))+?";

        public static ADFilter ParseFilterString(string filter)
        {
            Match match = mFilter.Match(filter);
            if (match.Success)
            {
                ADFilter filter2 = new ADFilter();
                if (match.Groups["item"].ToString().Length == 0)
                {
                    int num;
                    ArrayList list = new ArrayList();
                    for (string str = match.Groups["filterlist"].ToString().Trim(); str.Length > 0; str = str.Substring(num).TrimStart(new char[0]))
                    {
                        if (str[0] != '(')
                        {
                            return null;
                        }
                        num = 1;
                        int num2 = 1;
                        bool flag = false;
                        while ((num < str.Length) && !flag)
                        {
                            if (str[num] == '(')
                            {
                                num2++;
                            }
                            if (str[num] == ')')
                            {
                                if (num2 < 1)
                                {
                                    return null;
                                }
                                if (num2 == 1)
                                {
                                    flag = true;
                                }
                                else
                                {
                                    num2--;
                                }
                            }
                            num++;
                        }
                        if (!flag)
                        {
                            return null;
                        }
                        list.Add(str.Substring(0, num));
                    }
                    ADFilter filter5 = null;
                    switch (match.Groups["filtercomp"].ToString())
                    {
                        case "|":
                            filter2.Type = ADFilter.FilterType.Or;
                            filter2.Filter.Or = new ArrayList();
                            foreach (string str2 in list)
                            {
                                filter5 = ParseFilterString(str2);
                                if (filter5 == null)
                                {
                                    return null;
                                }
                                filter2.Filter.Or.Add(filter5);
                            }
                            if (filter2.Filter.Or.Count >= 1)
                            {
                                return filter2;
                            }
                            return null;

                        case "&":
                            filter2.Type = ADFilter.FilterType.And;
                            filter2.Filter.And = new ArrayList();
                            foreach (string str3 in list)
                            {
                                filter5 = ParseFilterString(str3);
                                if (filter5 == null)
                                {
                                    return null;
                                }
                                filter2.Filter.And.Add(filter5);
                            }
                            if (filter2.Filter.And.Count >= 1)
                            {
                                return filter2;
                            }
                            return null;

                        case "!":
                            filter2.Type = ADFilter.FilterType.Not;
                            filter5 = ParseFilterString((string) list[0]);
                            if ((list.Count > 1) || (filter5 == null))
                            {
                                return null;
                            }
                            filter2.Filter.Not = filter5;
                            return filter2;
                    }
                    return null;
                }
                if (match.Groups["present"].ToString().Length != 0)
                {
                    filter2.Type = ADFilter.FilterType.Present;
                    filter2.Filter.Present = match.Groups["presentattr"].ToString();
                    return filter2;
                }
                if (match.Groups["simple"].ToString().Length == 0)
                {
                    if (match.Groups["substr"].ToString().Length != 0)
                    {
                        filter2.Type = ADFilter.FilterType.Substrings;
                        ADSubstringFilter filter3 = new ADSubstringFilter {
                            Initial = StringFilterValueToADValue(match.Groups["initialvalue"].ToString()),
                            Final = StringFilterValueToADValue(match.Groups["finalvalue"].ToString())
                        };
                        if (match.Groups["anyvalue"].ToString().Length != 0)
                        {
                            foreach (Capture capture in match.Groups["anyvalue"].Captures)
                            {
                                filter3.Any.Add(StringFilterValueToADValue(capture.ToString()));
                            }
                        }
                        filter3.Name = match.Groups["substrattr"].ToString();
                        filter2.Filter.Substrings = filter3;
                        return filter2;
                    }
                    if (match.Groups["extensible"].ToString().Length != 0)
                    {
                        filter2.Type = ADFilter.FilterType.ExtensibleMatch;
                        ADExtenMatchFilter filter4 = new ADExtenMatchFilter {
                            Value = StringFilterValueToADValue(match.Groups["extenvalue"].ToString()),
                            DNAttributes = match.Groups["dnattr"].ToString().Length != 0,
                            Name = match.Groups["extenattr"].ToString(),
                            MatchingRule = match.Groups["matchrule"].ToString()
                        };
                        filter2.Filter.ExtensibleMatch = filter4;
                        return filter2;
                    }
                    return null;
                }
                ADAttribute attribute = new ADAttribute();
                if (match.Groups["simplevalue"].ToString().Length != 0)
                {
                    ADValue value2 = StringFilterValueToADValue(match.Groups["simplevalue"].ToString());
                    attribute.Values.Add(value2);
                }
                attribute.Name = match.Groups["simpleattr"].ToString();
                switch (match.Groups["filtertype"].ToString())
                {
                    case "=":
                        filter2.Type = ADFilter.FilterType.EqualityMatch;
                        filter2.Filter.EqualityMatch = attribute;
                        return filter2;

                    case "~=":
                        filter2.Type = ADFilter.FilterType.ApproxMatch;
                        filter2.Filter.ApproxMatch = attribute;
                        return filter2;

                    case "<=":
                        filter2.Type = ADFilter.FilterType.LessOrEqual;
                        filter2.Filter.LessOrEqual = attribute;
                        return filter2;

                    case ">=":
                        filter2.Type = ADFilter.FilterType.GreaterOrEqual;
                        filter2.Filter.GreaterOrEqual = attribute;
                        return filter2;
                }
            }
            return null;
        }

        protected static ADValue StringFilterValueToADValue(string strVal)
        {
            if ((strVal == null) || (strVal.Length == 0))
            {
                return null;
            }
            ADValue value2 = new ADValue();
            string[] strArray = strVal.Split(new char[] { '\\' });
            if (strArray.Length == 1)
            {
                value2.IsBinary = false;
                value2.StringVal = strVal;
                value2.BinaryVal = null;
                return value2;
            }
            ArrayList list = new ArrayList(strArray.Length);
            UTF8Encoding encoding = new UTF8Encoding();
            value2.IsBinary = true;
            value2.StringVal = null;
            if (strArray[0].Length != 0)
            {
                list.Add(encoding.GetBytes(strArray[0]));
            }
            for (int i = 1; i < strArray.Length; i++)
            {
                string s = strArray[i].Substring(0, 2);
                list.Add(new byte[] { byte.Parse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) });
                if (strArray[i].Length > 2)
                {
                    list.Add(encoding.GetBytes(strArray[i].Substring(2)));
                }
            }
            int num2 = 0;
            foreach (byte[] buffer in list)
            {
                num2 += buffer.Length;
            }
            value2.BinaryVal = new byte[num2];
            int index = 0;
            foreach (byte[] buffer2 in list)
            {
                buffer2.CopyTo(value2.BinaryVal, index);
                index += buffer2.Length;
            }
            return value2;
        }
    }
}

