namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    internal class CapabilitiesPattern
    {
        internal int[] _rules;
        internal string[] _strings;
        internal static readonly CapabilitiesPattern Default = new CapabilitiesPattern();
        internal static readonly Regex errorPat = new Regex(".{0,8}");
        internal const int Literal = 0;
        internal const int Reference = 1;
        internal static readonly Regex refPat = new Regex(@"\G\$(?:(?<name>\d+)|\{(?<name>\w+)\})");
        internal static readonly Regex textPat = new Regex(@"\G[^$%\\]*(?:\.[^$%\\]*)*");
        internal const int Variable = 2;
        internal static readonly Regex varPat = new Regex(@"\G\%\{(?<name>\w+)\}");

        internal CapabilitiesPattern()
        {
            this._strings = new string[] { string.Empty };
            this._rules = new int[] { 2 };
        }

        internal CapabilitiesPattern(string text)
        {
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            int startat = 0;
            while (true)
            {
                Match match = null;
                if ((match = textPat.Match(text, startat)).Success && (match.Length > 0))
                {
                    list2.Add(0);
                    list.Add(Regex.Unescape(match.ToString()));
                    startat = match.Index + match.Length;
                }
                if (startat == text.Length)
                {
                    break;
                }
                if ((match = refPat.Match(text, startat)).Success)
                {
                    list2.Add(1);
                    list.Add(match.Groups["name"].Value);
                }
                else if ((match = varPat.Match(text, startat)).Success)
                {
                    list2.Add(2);
                    list.Add(match.Groups["name"].Value);
                }
                else
                {
                    match = errorPat.Match(text, startat);
                    throw new ArgumentException(System.Web.SR.GetString("Unrecognized_construct_in_pattern", new object[] { match.ToString(), text }));
                }
                startat = match.Index + match.Length;
            }
            this._strings = (string[]) list.ToArray(typeof(string));
            this._rules = new int[list2.Count];
            for (int i = 0; i < list2.Count; i++)
            {
                this._rules[i] = (int) list2[i];
            }
        }

        internal virtual string Expand(CapabilitiesState matchstate)
        {
            StringBuilder builder = null;
            string str = null;
            for (int i = 0; i < this._rules.Length; i++)
            {
                if ((builder == null) && (str != null))
                {
                    builder = new StringBuilder(str);
                }
                switch (this._rules[i])
                {
                    case 0:
                        str = this._strings[i];
                        break;

                    case 1:
                        str = matchstate.ResolveReference(this._strings[i]);
                        break;

                    case 2:
                        str = matchstate.ResolveVariable(this._strings[i]);
                        break;
                }
                if ((builder != null) && (str != null))
                {
                    builder.Append(str);
                }
            }
            if (builder != null)
            {
                return builder.ToString();
            }
            if (str != null)
            {
                return str;
            }
            return string.Empty;
        }
    }
}

