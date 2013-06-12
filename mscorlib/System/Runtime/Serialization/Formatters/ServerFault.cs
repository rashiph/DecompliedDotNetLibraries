namespace System.Runtime.Serialization.Formatters
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Metadata;

    [Serializable, ComVisible(true), SoapType(Embedded=true)]
    public sealed class ServerFault
    {
        private System.Exception exception;
        private string exceptionType;
        private string message;
        private string stackTrace;

        internal ServerFault(System.Exception exception)
        {
            this.exception = exception;
        }

        public ServerFault(string exceptionType, string message, string stackTrace)
        {
            this.exceptionType = exceptionType;
            this.message = message;
            this.stackTrace = stackTrace;
        }

        internal System.Exception Exception
        {
            get
            {
                return this.exception;
            }
        }

        public string ExceptionMessage
        {
            get
            {
                return this.message;
            }
            set
            {
                this.message = value;
            }
        }

        public string ExceptionType
        {
            get
            {
                return this.exceptionType;
            }
            set
            {
                this.exceptionType = value;
            }
        }

        public string StackTrace
        {
            get
            {
                return this.stackTrace;
            }
            set
            {
                this.stackTrace = value;
            }
        }
    }
}

