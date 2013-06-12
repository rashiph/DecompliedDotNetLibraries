namespace System.Web
{
    using System;

    public sealed class TraceContextRecord
    {
        private string _category;
        private Exception _errorInfo;
        private bool _isWarning;
        private string _message;

        public TraceContextRecord(string category, string msg, bool isWarning, Exception errorInfo)
        {
            this._category = category;
            this._message = msg;
            this._isWarning = isWarning;
            this._errorInfo = errorInfo;
        }

        public string Category
        {
            get
            {
                return this._category;
            }
        }

        public Exception ErrorInfo
        {
            get
            {
                return this._errorInfo;
            }
        }

        public bool IsWarning
        {
            get
            {
                return this._isWarning;
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }
    }
}

