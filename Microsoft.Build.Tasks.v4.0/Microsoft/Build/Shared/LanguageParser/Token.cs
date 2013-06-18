namespace Microsoft.Build.Shared.LanguageParser
{
    using System;

    internal abstract class Token
    {
        private string innerText;
        private int line;

        protected Token()
        {
        }

        internal bool EqualsIgnoreCase(string compareTo)
        {
            return (string.Compare(this.innerText, compareTo, StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal string InnerText
        {
            get
            {
                return this.innerText;
            }
            set
            {
                this.innerText = value;
            }
        }

        internal int Line
        {
            get
            {
                return this.line;
            }
            set
            {
                this.line = value;
            }
        }
    }
}

