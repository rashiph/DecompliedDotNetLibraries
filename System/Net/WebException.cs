namespace System.Net
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class WebException : InvalidOperationException, ISerializable
    {
        [NonSerialized]
        private WebExceptionInternalStatus m_InternalStatus;
        private WebResponse m_Response;
        private WebExceptionStatus m_Status;

        public WebException()
        {
            this.m_Status = WebExceptionStatus.UnknownError;
        }

        public WebException(string message) : this(message, (Exception) null)
        {
        }

        protected WebException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this.m_Status = WebExceptionStatus.UnknownError;
        }

        public WebException(string message, Exception innerException) : base(message, innerException)
        {
            this.m_Status = WebExceptionStatus.UnknownError;
        }

        public WebException(string message, WebExceptionStatus status) : this(message, null, status, null)
        {
        }

        public WebException(string message, Exception innerException, WebExceptionStatus status, WebResponse response) : this(message, null, innerException, status, response)
        {
        }

        internal WebException(string message, WebExceptionStatus status, WebExceptionInternalStatus internalStatus, Exception innerException) : this(message, innerException, status, null, internalStatus)
        {
        }

        internal WebException(string message, Exception innerException, WebExceptionStatus status, WebResponse response, WebExceptionInternalStatus internalStatus) : this(message, null, innerException, status, response, internalStatus)
        {
        }

        internal WebException(string message, string data, Exception innerException, WebExceptionStatus status, WebResponse response) : base(message + ((data != null) ? (": '" + data + "'") : ""), innerException)
        {
            this.m_Status = WebExceptionStatus.UnknownError;
            this.m_Status = status;
            this.m_Response = response;
        }

        internal WebException(string message, string data, Exception innerException, WebExceptionStatus status, WebResponse response, WebExceptionInternalStatus internalStatus) : base(message + ((data != null) ? (": '" + data + "'") : ""), innerException)
        {
            this.m_Status = WebExceptionStatus.UnknownError;
            this.m_Status = status;
            this.m_Response = response;
            this.m_InternalStatus = internalStatus;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        internal WebExceptionInternalStatus InternalStatus
        {
            get
            {
                return this.m_InternalStatus;
            }
        }

        public WebResponse Response
        {
            get
            {
                return this.m_Response;
            }
        }

        public WebExceptionStatus Status
        {
            get
            {
                return this.m_Status;
            }
        }
    }
}

