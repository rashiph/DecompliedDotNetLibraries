namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class SmtpFailedRecipientsException : SmtpFailedRecipientException, ISerializable
    {
        private SmtpFailedRecipientException[] innerExceptions;

        public SmtpFailedRecipientsException()
        {
            this.innerExceptions = new SmtpFailedRecipientException[0];
        }

        public SmtpFailedRecipientsException(string message) : base(message)
        {
            this.innerExceptions = new SmtpFailedRecipientException[0];
        }

        internal SmtpFailedRecipientsException(ArrayList innerExceptions, bool allFailed) : base(allFailed ? SR.GetString("SmtpAllRecipientsFailed") : SR.GetString("SmtpRecipientFailed"), ((innerExceptions != null) && (innerExceptions.Count > 0)) ? ((SmtpFailedRecipientException) innerExceptions[0]).FailedRecipient : null, ((innerExceptions != null) && (innerExceptions.Count > 0)) ? ((SmtpFailedRecipientException) innerExceptions[0]) : null)
        {
            if (innerExceptions == null)
            {
                throw new ArgumentNullException("innerExceptions");
            }
            this.innerExceptions = new SmtpFailedRecipientException[innerExceptions.Count];
            int num = 0;
            foreach (SmtpFailedRecipientException exception in innerExceptions)
            {
                this.innerExceptions[num++] = exception;
            }
        }

        protected SmtpFailedRecipientsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.innerExceptions = (SmtpFailedRecipientException[]) info.GetValue("innerExceptions", typeof(SmtpFailedRecipientException[]));
        }

        public SmtpFailedRecipientsException(string message, Exception innerException) : base(message, innerException)
        {
            SmtpFailedRecipientException exception = innerException as SmtpFailedRecipientException;
            this.innerExceptions = (exception == null) ? new SmtpFailedRecipientException[0] : new SmtpFailedRecipientException[] { exception };
        }

        public SmtpFailedRecipientsException(string message, SmtpFailedRecipientException[] innerExceptions) : base(message, ((innerExceptions != null) && (innerExceptions.Length > 0)) ? innerExceptions[0].FailedRecipient : null, ((innerExceptions != null) && (innerExceptions.Length > 0)) ? innerExceptions[0] : null)
        {
            if (innerExceptions == null)
            {
                throw new ArgumentNullException("innerExceptions");
            }
            this.innerExceptions = (innerExceptions == null) ? new SmtpFailedRecipientException[0] : innerExceptions;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
            serializationInfo.AddValue("innerExceptions", this.innerExceptions, typeof(SmtpFailedRecipientException[]));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        public SmtpFailedRecipientException[] InnerExceptions
        {
            get
            {
                return this.innerExceptions;
            }
        }
    }
}

