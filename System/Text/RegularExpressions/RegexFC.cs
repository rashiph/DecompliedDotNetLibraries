namespace System.Text.RegularExpressions
{
    using System;
    using System.Globalization;

    internal sealed class RegexFC
    {
        internal bool _caseInsensitive;
        internal RegexCharClass _cc;
        internal bool _nullable;

        internal RegexFC(bool nullable)
        {
            this._cc = new RegexCharClass();
            this._nullable = nullable;
        }

        internal RegexFC(string charClass, bool nullable, bool caseInsensitive)
        {
            this._cc = RegexCharClass.Parse(charClass);
            this._nullable = nullable;
            this._caseInsensitive = caseInsensitive;
        }

        internal RegexFC(char ch, bool not, bool nullable, bool caseInsensitive)
        {
            this._cc = new RegexCharClass();
            if (not)
            {
                if (ch > '\0')
                {
                    this._cc.AddRange('\0', (char) (ch - '\x0001'));
                }
                if (ch < 0xffff)
                {
                    this._cc.AddRange((char) (ch + '\x0001'), 0xffff);
                }
            }
            else
            {
                this._cc.AddRange(ch, ch);
            }
            this._caseInsensitive = caseInsensitive;
            this._nullable = nullable;
        }

        internal bool AddFC(RegexFC fc, bool concatenate)
        {
            if (!this._cc.CanMerge || !fc._cc.CanMerge)
            {
                return false;
            }
            if (concatenate)
            {
                if (!this._nullable)
                {
                    return true;
                }
                if (!fc._nullable)
                {
                    this._nullable = false;
                }
            }
            else if (fc._nullable)
            {
                this._nullable = true;
            }
            this._caseInsensitive |= fc._caseInsensitive;
            this._cc.AddCharClass(fc._cc);
            return true;
        }

        internal string GetFirstChars(CultureInfo culture)
        {
            if (this._caseInsensitive)
            {
                this._cc.AddLowercase(culture);
            }
            return this._cc.ToStringClass();
        }

        internal bool IsCaseInsensitive()
        {
            return this._caseInsensitive;
        }
    }
}

