namespace System.EnterpriseServices
{
    using System;

    [Serializable]
    public sealed class RegistrationErrorInfo
    {
        private int _errorCode;
        private string _errorString;
        private string _majorRef;
        private string _minorRef;
        private string _name;

        internal RegistrationErrorInfo(string majorRef, string minorRef, string name, int errorCode)
        {
            this._majorRef = majorRef;
            this._minorRef = minorRef;
            this._name = name;
            this._errorCode = errorCode;
            if (this._majorRef == null)
            {
                this._majorRef = "";
            }
            if (this._minorRef == null)
            {
                this._minorRef = "<invalid>";
            }
            this._errorString = Util.GetErrorString(this._errorCode);
            if (this._errorString == null)
            {
                this._errorString = Resource.FormatString("Err_UnknownHR", this._errorCode);
            }
        }

        public int ErrorCode
        {
            get
            {
                return this._errorCode;
            }
        }

        public string ErrorString
        {
            get
            {
                return this._errorString;
            }
        }

        public string MajorRef
        {
            get
            {
                return this._majorRef;
            }
        }

        public string MinorRef
        {
            get
            {
                return this._minorRef;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

