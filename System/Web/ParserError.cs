namespace System.Web
{
    using System;

    [Serializable]
    public sealed class ParserError
    {
        private string _errorText;
        private System.Exception _exception;
        private int _line;
        private System.Web.VirtualPath _virtualPath;

        public ParserError()
        {
        }

        public ParserError(string errorText, string virtualPath, int line) : this(errorText, System.Web.VirtualPath.CreateAllowNull(virtualPath), line)
        {
        }

        internal ParserError(string errorText, System.Web.VirtualPath virtualPath, int line)
        {
            this._virtualPath = virtualPath;
            this._line = line;
            this._errorText = errorText;
        }

        public string ErrorText
        {
            get
            {
                return this._errorText;
            }
            set
            {
                this._errorText = value;
            }
        }

        internal System.Exception Exception
        {
            get
            {
                return this._exception;
            }
            set
            {
                this._exception = value;
            }
        }

        public int Line
        {
            get
            {
                return this._line;
            }
            set
            {
                this._line = value;
            }
        }

        public string VirtualPath
        {
            get
            {
                return System.Web.VirtualPath.GetVirtualPathString(this._virtualPath);
            }
            set
            {
                this._virtualPath = System.Web.VirtualPath.Create(value);
            }
        }
    }
}

