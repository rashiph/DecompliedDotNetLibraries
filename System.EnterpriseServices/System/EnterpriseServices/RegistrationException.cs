namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class RegistrationException : SystemException
    {
        private RegistrationErrorInfo[] _errorInfo;

        public RegistrationException()
        {
        }

        public RegistrationException(string msg) : base(msg)
        {
            this._errorInfo = null;
        }

        internal RegistrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            try
            {
                this._errorInfo = (RegistrationErrorInfo[]) info.GetValue("RegistrationException._errorInfo", typeof(RegistrationErrorInfo[]));
            }
            catch (SerializationException)
            {
                this._errorInfo = null;
            }
        }

        public RegistrationException(string msg, Exception inner) : base(msg, inner)
        {
            this._errorInfo = null;
        }

        internal RegistrationException(string msg, RegistrationErrorInfo[] errorInfo) : base(msg)
        {
            this._errorInfo = errorInfo;
        }

        internal RegistrationException(string msg, RegistrationErrorInfo[] errorInfo, Exception inner) : base(msg, inner)
        {
            this._errorInfo = errorInfo;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            if (info == null)
            {
                throw new ArgumentException(Resource.FormatString("Err_info"));
            }
            base.GetObjectData(info, ctx);
            if (this._errorInfo != null)
            {
                info.AddValue("RegistrationException._errorInfo", this._errorInfo, typeof(RegistrationErrorInfo[]));
            }
        }

        public RegistrationErrorInfo[] ErrorInfo
        {
            get
            {
                return this._errorInfo;
            }
        }
    }
}

