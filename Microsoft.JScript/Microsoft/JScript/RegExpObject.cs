namespace Microsoft.JScript
{
    using System;
    using System.Text.RegularExpressions;

    public sealed class RegExpObject : JSObject
    {
        internal bool globalInt;
        internal bool ignoreCaseInt;
        internal object lastIndexInt;
        internal bool multilineInt;
        internal Regex regex;
        internal RegExpConstructor regExpConst;
        private string sourceInt;

        internal RegExpObject(Regex regex) : base(null)
        {
            this.regExpConst = null;
            this.sourceInt = "";
            this.ignoreCaseInt = (regex.Options & RegexOptions.IgnoreCase) != RegexOptions.None;
            this.globalInt = false;
            this.multilineInt = (regex.Options & RegexOptions.Multiline) != RegexOptions.None;
            this.regex = regex;
            this.lastIndexInt = 0;
            base.noExpando = true;
        }

        internal RegExpObject(RegExpPrototype parent, string source, bool ignoreCase, bool global, bool multiline, RegExpConstructor regExpConst) : base(parent)
        {
            this.regExpConst = regExpConst;
            this.sourceInt = source;
            this.ignoreCaseInt = ignoreCase;
            this.globalInt = global;
            this.multilineInt = multiline;
            RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.ECMAScript;
            if (ignoreCase)
            {
                options |= RegexOptions.IgnoreCase;
            }
            if (multiline)
            {
                options |= RegexOptions.Multiline;
            }
            try
            {
                this.regex = new Regex(source, options);
            }
            catch (ArgumentException)
            {
                throw new JScriptException(JSError.RegExpSyntax);
            }
            this.lastIndexInt = 0;
            base.noExpando = false;
        }

        internal RegExpObject compile(string source, string flags)
        {
            this.sourceInt = source;
            this.ignoreCaseInt = this.globalInt = this.multilineInt = false;
            RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.ECMAScript;
            for (int i = 0; i < flags.Length; i++)
            {
                switch (flags[i])
                {
                    case 'g':
                        if (this.globalInt)
                        {
                            throw new JScriptException(JSError.RegExpSyntax);
                        }
                        goto Label_0087;

                    case 'i':
                        if (this.ignoreCaseInt)
                        {
                            throw new JScriptException(JSError.RegExpSyntax);
                        }
                        break;

                    case 'm':
                    {
                        if (this.multilineInt)
                        {
                            throw new JScriptException(JSError.RegExpSyntax);
                        }
                        this.multilineInt = true;
                        options |= RegexOptions.Multiline;
                        continue;
                    }
                    default:
                        throw new JScriptException(JSError.RegExpSyntax);
                }
                this.ignoreCaseInt = true;
                options |= RegexOptions.IgnoreCase;
                continue;
            Label_0087:
                this.globalInt = true;
            }
            try
            {
                this.regex = new Regex(source, options);
            }
            catch (ArgumentException)
            {
                throw new JScriptException(JSError.RegExpSyntax);
            }
            return this;
        }

        internal object exec(string input)
        {
            Match match = null;
            if (!this.globalInt)
            {
                match = this.regex.Match(input);
            }
            else
            {
                int startat = (int) Runtime.DoubleToInt64(Microsoft.JScript.Convert.ToInteger(this.lastIndexInt));
                if (startat <= 0)
                {
                    match = this.regex.Match(input);
                }
                else if (startat <= input.Length)
                {
                    match = this.regex.Match(input, startat);
                }
            }
            if ((match == null) || !match.Success)
            {
                this.lastIndexInt = 0;
                return DBNull.Value;
            }
            this.lastIndexInt = this.regExpConst.UpdateConstructor(this.regex, match, input);
            return new RegExpMatch(this.regExpConst.arrayPrototype, this.regex, match, input);
        }

        internal override string GetClassName()
        {
            return "RegExp";
        }

        internal bool test(string input)
        {
            Match match = null;
            if (!this.globalInt)
            {
                match = this.regex.Match(input);
            }
            else
            {
                int startat = (int) Runtime.DoubleToInt64(Microsoft.JScript.Convert.ToInteger(this.lastIndexInt));
                if (startat <= 0)
                {
                    match = this.regex.Match(input);
                }
                else if (startat <= input.Length)
                {
                    match = this.regex.Match(input, startat);
                }
            }
            if ((match == null) || !match.Success)
            {
                this.lastIndexInt = 0;
                return false;
            }
            this.lastIndexInt = this.regExpConst.UpdateConstructor(this.regex, match, input);
            return true;
        }

        public override string ToString()
        {
            return ("/" + this.sourceInt + "/" + (this.ignoreCaseInt ? "i" : "") + (this.globalInt ? "g" : "") + (this.multilineInt ? "m" : ""));
        }

        public bool global
        {
            get
            {
                return this.globalInt;
            }
        }

        public bool ignoreCase
        {
            get
            {
                return this.ignoreCaseInt;
            }
        }

        public object lastIndex
        {
            get
            {
                return this.lastIndexInt;
            }
            set
            {
                this.lastIndexInt = value;
            }
        }

        public bool multiline
        {
            get
            {
                return this.multilineInt;
            }
        }

        public string source
        {
            get
            {
                return this.sourceInt;
            }
        }
    }
}

