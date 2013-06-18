namespace System.Web.Util
{
    using System;
    using System.Text.RegularExpressions;

    internal class WildcardUrl : WildcardPath
    {
        internal WildcardUrl(string pattern, bool caseInsensitive) : base(pattern, caseInsensitive)
        {
        }

        protected override Regex[][] DirsFromWildcard(string pattern)
        {
            string[] strArray = Wildcard.commaRegex.Split(pattern);
            Regex[][] regexArray = new Regex[strArray.Length][];
            for (int i = 0; i < strArray.Length; i++)
            {
                string[] strArray2 = Wildcard.slashRegex.Split(strArray[i]);
                Regex[] regexArray2 = new Regex[strArray2.Length];
                if ((strArray.Length == 1) && (strArray2.Length == 1))
                {
                    base.EnsureRegex();
                    regexArray2[0] = base._regex;
                }
                else
                {
                    for (int j = 0; j < strArray2.Length; j++)
                    {
                        regexArray2[j] = this.RegexFromWildcard(strArray2[j], base._caseInsensitive);
                    }
                }
                regexArray[i] = regexArray2;
            }
            return regexArray;
        }

        protected override Regex RegexFromWildcard(string pattern, bool caseInsensitive)
        {
            RegexOptions rightToLeft;
            if ((pattern.Length > 0) && (pattern[0] == '*'))
            {
                rightToLeft = RegexOptions.RightToLeft;
            }
            else
            {
                rightToLeft = RegexOptions.None;
            }
            if (caseInsensitive)
            {
                rightToLeft |= RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
            }
            pattern = Wildcard.metaRegex.Replace(pattern, @"\$0");
            pattern = Wildcard.questRegex.Replace(pattern, "[^/]");
            pattern = Wildcard.starRegex.Replace(pattern, "[^/]*");
            pattern = Wildcard.commaRegex.Replace(pattern, @"\z|\A");
            return new Regex(@"\A" + pattern + @"\z", rightToLeft);
        }

        protected override string[] SplitDirs(string input)
        {
            return Wildcard.slashRegex.Split(input);
        }

        protected override Regex SuffixFromWildcard(string pattern, bool caseInsensitive)
        {
            RegexOptions rightToLeft = RegexOptions.RightToLeft;
            if (caseInsensitive)
            {
                rightToLeft |= RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
            }
            pattern = Wildcard.metaRegex.Replace(pattern, @"\$0");
            pattern = Wildcard.questRegex.Replace(pattern, "[^/]");
            pattern = Wildcard.starRegex.Replace(pattern, "[^/]*");
            pattern = Wildcard.commaRegex.Replace(pattern, @"\z|(?:\A|(?<=/))");
            return new Regex(@"(?:\A|(?<=/))" + pattern + @"\z", rightToLeft);
        }
    }
}

