namespace System.Configuration
{
    using System;

    internal sealed class SchemeSettingInternal
    {
        private string name;
        private GenericUriParserOptions options;

        public SchemeSettingInternal(string name, GenericUriParserOptions options)
        {
            this.name = name.ToLowerInvariant();
            this.options = options;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public GenericUriParserOptions Options
        {
            get
            {
                return this.options;
            }
        }
    }
}

