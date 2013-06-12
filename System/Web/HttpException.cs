namespace System.Web
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    public class HttpException : ExternalException
    {
        private ErrorFormatter _errorFormatter;
        private int _httpCode;
        private int _webEventCode;
        private const int FACILITY_WIN32 = 7;

        public HttpException()
        {
        }

        public HttpException(string message) : base(message)
        {
        }

        public HttpException(int httpCode, string message) : base(message)
        {
            this._httpCode = httpCode;
        }

        protected HttpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._httpCode = info.GetInt32("_httpCode");
        }

        public HttpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public HttpException(string message, int hr) : base(message)
        {
            base.HResult = hr;
        }

        public HttpException(int httpCode, string message, Exception innerException) : base(message, innerException)
        {
            this._httpCode = httpCode;
        }

        public HttpException(int httpCode, string message, int hr) : base(message)
        {
            base.HResult = hr;
            this._httpCode = httpCode;
        }

        internal HttpException(string message, Exception innerException, int code) : base(message, innerException)
        {
            this._webEventCode = code;
        }

        public static HttpException CreateFromLastError(string message)
        {
            return new HttpException(message, HResultFromLastError(Marshal.GetLastWin32Error()));
        }

        internal static ErrorFormatter GetErrorFormatter(Exception e)
        {
            Exception innerException = e.InnerException;
            ErrorFormatter errorFormatter = null;
            if (innerException != null)
            {
                errorFormatter = GetErrorFormatter(innerException);
                if (errorFormatter != null)
                {
                    return errorFormatter;
                }
                if (innerException is ConfigurationException)
                {
                    ConfigurationException exception2 = innerException as ConfigurationException;
                    if ((exception2 != null) && (exception2.Filename != null))
                    {
                        errorFormatter = new ConfigErrorFormatter((ConfigurationException) innerException);
                    }
                }
                else if (innerException is SecurityException)
                {
                    errorFormatter = new SecurityErrorFormatter(innerException);
                }
            }
            if (errorFormatter != null)
            {
                return errorFormatter;
            }
            HttpException exception3 = e as HttpException;
            if (exception3 != null)
            {
                return exception3._errorFormatter;
            }
            return null;
        }

        public string GetHtmlErrorMessage()
        {
            ErrorFormatter errorFormatter = GetErrorFormatter(this);
            if (errorFormatter == null)
            {
                return null;
            }
            return errorFormatter.GetHtmlErrorMessage();
        }

        public int GetHttpCode()
        {
            return GetHttpCodeForException(this);
        }

        internal static int GetHttpCodeForException(Exception e)
        {
            if (e is HttpException)
            {
                HttpException exception = (HttpException) e;
                if (exception._httpCode > 0)
                {
                    return exception._httpCode;
                }
            }
            else
            {
                if (e is UnauthorizedAccessException)
                {
                    return 0x191;
                }
                if (e is PathTooLongException)
                {
                    return 0x19e;
                }
            }
            if (e.InnerException != null)
            {
                return GetHttpCodeForException(e.InnerException);
            }
            return 500;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_httpCode", this._httpCode);
        }

        internal static int HResultFromLastError(int lastError)
        {
            if (lastError < 0)
            {
                return lastError;
            }
            return (((lastError & 0xffff) | 0x70000) | -2147483648);
        }

        internal void SetFormatter(ErrorFormatter errorFormatter)
        {
            this._errorFormatter = errorFormatter;
        }

        public int WebEventCode
        {
            get
            {
                return this._webEventCode;
            }
            internal set
            {
                this._webEventCode = value;
            }
        }
    }
}

