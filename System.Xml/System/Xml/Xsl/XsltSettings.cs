namespace System.Xml.Xsl
{
    using System;
    using System.CodeDom.Compiler;

    public sealed class XsltSettings
    {
        private bool checkOnly;
        private bool enableDocumentFunction;
        private bool enableScript;
        private bool includeDebugInformation;
        private TempFileCollection tempFiles;
        private bool treatWarningsAsErrors;
        private int warningLevel;

        public XsltSettings()
        {
            this.warningLevel = -1;
        }

        public XsltSettings(bool enableDocumentFunction, bool enableScript)
        {
            this.warningLevel = -1;
            this.enableDocumentFunction = enableDocumentFunction;
            this.enableScript = enableScript;
        }

        internal bool CheckOnly
        {
            get
            {
                return this.checkOnly;
            }
            set
            {
                this.checkOnly = value;
            }
        }

        public static XsltSettings Default
        {
            get
            {
                return new XsltSettings(false, false);
            }
        }

        public bool EnableDocumentFunction
        {
            get
            {
                return this.enableDocumentFunction;
            }
            set
            {
                this.enableDocumentFunction = value;
            }
        }

        public bool EnableScript
        {
            get
            {
                return this.enableScript;
            }
            set
            {
                this.enableScript = value;
            }
        }

        internal bool IncludeDebugInformation
        {
            get
            {
                return this.includeDebugInformation;
            }
            set
            {
                this.includeDebugInformation = value;
            }
        }

        internal TempFileCollection TempFiles
        {
            get
            {
                return this.tempFiles;
            }
            set
            {
                this.tempFiles = value;
            }
        }

        internal bool TreatWarningsAsErrors
        {
            get
            {
                return this.treatWarningsAsErrors;
            }
            set
            {
                this.treatWarningsAsErrors = value;
            }
        }

        public static XsltSettings TrustedXslt
        {
            get
            {
                return new XsltSettings(true, true);
            }
        }

        internal int WarningLevel
        {
            get
            {
                return this.warningLevel;
            }
            set
            {
                this.warningLevel = value;
            }
        }
    }
}

