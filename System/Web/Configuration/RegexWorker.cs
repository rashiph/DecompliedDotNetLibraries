namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    public class RegexWorker
    {
        private HttpBrowserCapabilities _browserCaps;
        private Hashtable _groups;
        internal static readonly Regex RefPat = new BrowserCapsRefRegex();

        public RegexWorker(HttpBrowserCapabilities browserCaps)
        {
            this._browserCaps = browserCaps;
        }

        private string Lookup(string from)
        {
            MatchCollection matchs = RefPat.Matches(from);
            if (matchs.Count == 0)
            {
                return from;
            }
            StringBuilder builder = new StringBuilder();
            int startIndex = 0;
            foreach (Match match in matchs)
            {
                int length = match.Index - startIndex;
                builder.Append(from.Substring(startIndex, length));
                startIndex = match.Index + match.Length;
                string str = match.Groups["name"].Value;
                string str2 = null;
                if (this._groups != null)
                {
                    str2 = (string) this._groups[str];
                }
                if (str2 == null)
                {
                    str2 = this._browserCaps[str];
                }
                builder.Append(str2);
            }
            builder.Append(from, startIndex, from.Length - startIndex);
            string str3 = builder.ToString();
            if (str3.Length == 0)
            {
                return null;
            }
            return str3;
        }

        public bool ProcessRegex(string target, string regexExpression)
        {
            if (target == null)
            {
                target = string.Empty;
            }
            Regex regex = new Regex(regexExpression, RegexOptions.ExplicitCapture);
            Match match = regex.Match(target);
            if (!match.Success)
            {
                return false;
            }
            string[] groupNames = regex.GetGroupNames();
            if (groupNames.Length > 0)
            {
                if (this._groups == null)
                {
                    this._groups = new Hashtable();
                }
                for (int i = 0; i < groupNames.Length; i++)
                {
                    this._groups[groupNames[i]] = match.Groups[i].Value;
                }
            }
            return true;
        }

        public string this[string key]
        {
            get
            {
                return this.Lookup(key);
            }
        }
    }
}

