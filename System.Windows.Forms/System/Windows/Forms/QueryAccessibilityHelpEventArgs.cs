namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class QueryAccessibilityHelpEventArgs : EventArgs
    {
        private string helpKeyword;
        private string helpNamespace;
        private string helpString;

        public QueryAccessibilityHelpEventArgs()
        {
        }

        public QueryAccessibilityHelpEventArgs(string helpNamespace, string helpString, string helpKeyword)
        {
            this.helpNamespace = helpNamespace;
            this.helpString = helpString;
            this.helpKeyword = helpKeyword;
        }

        public string HelpKeyword
        {
            get
            {
                return this.helpKeyword;
            }
            set
            {
                this.helpKeyword = value;
            }
        }

        public string HelpNamespace
        {
            get
            {
                return this.helpNamespace;
            }
            set
            {
                this.helpNamespace = value;
            }
        }

        public string HelpString
        {
            get
            {
                return this.helpString;
            }
            set
            {
                this.helpString = value;
            }
        }
    }
}

