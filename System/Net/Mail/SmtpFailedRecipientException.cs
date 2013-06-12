namespace System.Net.Mail
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class SmtpFailedRecipientException : SmtpException, ISerializable
    {
        private string failedRecipient;
        internal bool fatal;

        public SmtpFailedRecipientException()
        {
        }

        public SmtpFailedRecipientException(string message) : base(message)
        {
        }

        public SmtpFailedRecipientException(SmtpStatusCode statusCode, string failedRecipient) : base(statusCode)
        {
            this.failedRecipient = failedRecipient;
        }

        protected SmtpFailedRecipientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.failedRecipient = info.GetString("failedRecipient");
        }

        public SmtpFailedRecipientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SmtpFailedRecipientException(SmtpStatusCode statusCode, string failedRecipient, string serverResponse) : base(statusCode, serverResponse, true)
        {
            this.failedRecipient = failedRecipient;
        }

        public SmtpFailedRecipientException(string message, string failedRecipient, Exception innerException) : base(message, innerException)
        {
            this.failedRecipient = failedRecipient;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
            serializationInfo.AddValue("failedRecipient", this.failedRecipient, typeof(string));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        public string FailedRecipient
        {
            get
            {
                return this.failedRecipient;
            }
        }
    }
}

