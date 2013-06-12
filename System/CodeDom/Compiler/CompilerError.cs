namespace System.CodeDom.Compiler
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [Serializable, PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class CompilerError
    {
        private int column;
        private string errorNumber;
        private string errorText;
        private string fileName;
        private int line;
        private bool warning;

        public CompilerError()
        {
            this.line = 0;
            this.column = 0;
            this.errorNumber = string.Empty;
            this.errorText = string.Empty;
            this.fileName = string.Empty;
        }

        public CompilerError(string fileName, int line, int column, string errorNumber, string errorText)
        {
            this.line = line;
            this.column = column;
            this.errorNumber = errorNumber;
            this.errorText = errorText;
            this.fileName = fileName;
        }

        public override string ToString()
        {
            if (this.FileName.Length > 0)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}({1},{2}) : {3} {4}: {5}", new object[] { this.FileName, this.Line, this.Column, this.IsWarning ? "warning" : "error", this.ErrorNumber, this.ErrorText });
            }
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}: {2}", new object[] { this.IsWarning ? "warning" : "error", this.ErrorNumber, this.ErrorText });
        }

        public int Column
        {
            get
            {
                return this.column;
            }
            set
            {
                this.column = value;
            }
        }

        public string ErrorNumber
        {
            get
            {
                return this.errorNumber;
            }
            set
            {
                this.errorNumber = value;
            }
        }

        public string ErrorText
        {
            get
            {
                return this.errorText;
            }
            set
            {
                this.errorText = value;
            }
        }

        public string FileName
        {
            get
            {
                return this.fileName;
            }
            set
            {
                this.fileName = value;
            }
        }

        public bool IsWarning
        {
            get
            {
                return this.warning;
            }
            set
            {
                this.warning = value;
            }
        }

        public int Line
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

