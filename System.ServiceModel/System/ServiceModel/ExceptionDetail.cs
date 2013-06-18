namespace System.ServiceModel
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
    public class ExceptionDetail
    {
        private string helpLink;
        private ExceptionDetail innerException;
        private string message;
        private string stackTrace;
        private string type;

        public ExceptionDetail(Exception exception)
        {
            if (exception == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exception");
            }
            this.helpLink = exception.HelpLink;
            this.message = exception.Message;
            this.stackTrace = exception.StackTrace;
            this.type = exception.GetType().ToString();
            if (exception.InnerException != null)
            {
                this.innerException = new ExceptionDetail(exception.InnerException);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\n{1}", new object[] { System.ServiceModel.SR.GetString("SFxExceptionDetailFormat"), this.ToStringHelper(false) });
        }

        private string ToStringHelper(bool isInner)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}: {1}", this.Type, this.Message);
            if (this.InnerException != null)
            {
                builder.AppendFormat(" ----> {0}", this.InnerException.ToStringHelper(true));
            }
            else
            {
                builder.Append("\n");
            }
            builder.Append(this.StackTrace);
            if (isInner)
            {
                builder.AppendFormat("\n   {0}\n", System.ServiceModel.SR.GetString("SFxExceptionDetailEndOfInner"));
            }
            return builder.ToString();
        }

        [DataMember]
        public string HelpLink
        {
            get
            {
                return this.helpLink;
            }
            private set
            {
                this.helpLink = value;
            }
        }

        [DataMember]
        public ExceptionDetail InnerException
        {
            get
            {
                return this.innerException;
            }
            private set
            {
                this.innerException = value;
            }
        }

        [DataMember]
        public string Message
        {
            get
            {
                return this.message;
            }
            private set
            {
                this.message = value;
            }
        }

        [DataMember]
        public string StackTrace
        {
            get
            {
                return this.stackTrace;
            }
            private set
            {
                this.stackTrace = value;
            }
        }

        [DataMember]
        public string Type
        {
            get
            {
                return this.type;
            }
            private set
            {
                this.type = value;
            }
        }
    }
}

