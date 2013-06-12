namespace System.Text.RegularExpressions
{
    using System;

    [Serializable]
    public class RegexCompilationInfo
    {
        private bool isPublic;
        private string name;
        private string nspace;
        private RegexOptions options;
        private string pattern;

        public RegexCompilationInfo(string pattern, RegexOptions options, string name, string fullnamespace, bool ispublic)
        {
            this.Pattern = pattern;
            this.Name = name;
            this.Namespace = fullnamespace;
            this.options = options;
            this.isPublic = ispublic;
        }

        public bool IsPublic
        {
            get
            {
                return this.isPublic;
            }
            set
            {
                this.isPublic = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", new object[] { "value" }), "value");
                }
                this.name = value;
            }
        }

        public string Namespace
        {
            get
            {
                return this.nspace;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.nspace = value;
            }
        }

        public RegexOptions Options
        {
            get
            {
                return this.options;
            }
            set
            {
                this.options = value;
            }
        }

        public string Pattern
        {
            get
            {
                return this.pattern;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.pattern = value;
            }
        }
    }
}

