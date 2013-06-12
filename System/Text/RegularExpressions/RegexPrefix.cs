namespace System.Text.RegularExpressions
{
    using System;

    internal sealed class RegexPrefix
    {
        internal bool _caseInsensitive;
        internal static RegexPrefix _empty = new RegexPrefix(string.Empty, false);
        internal string _prefix;

        internal RegexPrefix(string prefix, bool ci)
        {
            this._prefix = prefix;
            this._caseInsensitive = ci;
        }

        internal bool CaseInsensitive
        {
            get
            {
                return this._caseInsensitive;
            }
        }

        internal static RegexPrefix Empty
        {
            get
            {
                return _empty;
            }
        }

        internal string Prefix
        {
            get
            {
                return this._prefix;
            }
        }
    }
}

