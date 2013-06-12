namespace System.Net.Mail
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class SmtpException : Exception, ISerializable
    {
        private SmtpStatusCode statusCode;

        public SmtpException() : this(SmtpStatusCode.GeneralFailure)
        {
        }

        public SmtpException(SmtpStatusCode statusCode) : base(GetMessageForStatus(statusCode))
        {
            this.statusCode = SmtpStatusCode.GeneralFailure;
            this.statusCode = statusCode;
        }

        public SmtpException(string message) : base(message)
        {
            this.statusCode = SmtpStatusCode.GeneralFailure;
        }

        public SmtpException(SmtpStatusCode statusCode, string message) : base(message)
        {
            this.statusCode = SmtpStatusCode.GeneralFailure;
            this.statusCode = statusCode;
        }

        protected SmtpException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this.statusCode = SmtpStatusCode.GeneralFailure;
            this.statusCode = (SmtpStatusCode) serializationInfo.GetInt32("Status");
        }

        public SmtpException(string message, Exception innerException) : base(message, innerException)
        {
            this.statusCode = SmtpStatusCode.GeneralFailure;
        }

        internal SmtpException(string message, string serverResponse) : base(message + " " + SR.GetString("MailServerResponse", new object[] { serverResponse }))
        {
            this.statusCode = SmtpStatusCode.GeneralFailure;
        }

        internal SmtpException(SmtpStatusCode statusCode, string serverMessage, bool serverResponse) : base(GetMessageForStatus(statusCode, serverMessage))
        {
            this.statusCode = SmtpStatusCode.GeneralFailure;
            this.statusCode = statusCode;
        }

        private static string GetMessageForStatus(SmtpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.SystemStatus:
                    return SR.GetString("SmtpSystemStatus");

                case SmtpStatusCode.HelpMessage:
                    return SR.GetString("SmtpHelpMessage");

                case SmtpStatusCode.ServiceReady:
                    return SR.GetString("SmtpServiceReady");

                case SmtpStatusCode.ServiceClosingTransmissionChannel:
                    return SR.GetString("SmtpServiceClosingTransmissionChannel");

                case SmtpStatusCode.Ok:
                    return SR.GetString("SmtpOK");

                case SmtpStatusCode.UserNotLocalWillForward:
                    return SR.GetString("SmtpUserNotLocalWillForward");

                case SmtpStatusCode.StartMailInput:
                    return SR.GetString("SmtpStartMailInput");

                case SmtpStatusCode.MailboxBusy:
                    return SR.GetString("SmtpMailboxBusy");

                case SmtpStatusCode.LocalErrorInProcessing:
                    return SR.GetString("SmtpLocalErrorInProcessing");

                case SmtpStatusCode.InsufficientStorage:
                    return SR.GetString("SmtpInsufficientStorage");

                case SmtpStatusCode.ClientNotPermitted:
                    return SR.GetString("SmtpClientNotPermitted");

                case SmtpStatusCode.ServiceNotAvailable:
                    return SR.GetString("SmtpServiceNotAvailable");

                case SmtpStatusCode.SyntaxError:
                    return SR.GetString("SmtpSyntaxError");

                case SmtpStatusCode.CommandNotImplemented:
                    return SR.GetString("SmtpCommandNotImplemented");

                case SmtpStatusCode.BadCommandSequence:
                    return SR.GetString("SmtpBadCommandSequence");

                case SmtpStatusCode.CommandParameterNotImplemented:
                    return SR.GetString("SmtpCommandParameterNotImplemented");

                case SmtpStatusCode.MustIssueStartTlsFirst:
                    return SR.GetString("SmtpMustIssueStartTlsFirst");

                case SmtpStatusCode.MailboxUnavailable:
                    return SR.GetString("SmtpMailboxUnavailable");

                case SmtpStatusCode.UserNotLocalTryAlternatePath:
                    return SR.GetString("SmtpUserNotLocalTryAlternatePath");

                case SmtpStatusCode.ExceededStorageAllocation:
                    return SR.GetString("SmtpExceededStorageAllocation");

                case SmtpStatusCode.MailboxNameNotAllowed:
                    return SR.GetString("SmtpMailboxNameNotAllowed");

                case SmtpStatusCode.TransactionFailed:
                    return SR.GetString("SmtpTransactionFailed");
            }
            return SR.GetString("SmtpCommandUnrecognized");
        }

        private static string GetMessageForStatus(SmtpStatusCode statusCode, string serverResponse)
        {
            return (GetMessageForStatus(statusCode) + " " + SR.GetString("MailServerResponse", new object[] { serverResponse }));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
            serializationInfo.AddValue("Status", (int) this.statusCode, typeof(int));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        public SmtpStatusCode StatusCode
        {
            get
            {
                return this.statusCode;
            }
            set
            {
                this.statusCode = value;
            }
        }
    }
}

