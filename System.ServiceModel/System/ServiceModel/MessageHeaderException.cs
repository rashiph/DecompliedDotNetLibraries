namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;

    [Serializable]
    public class MessageHeaderException : ProtocolException
    {
        [NonSerialized]
        private string headerName;
        [NonSerialized]
        private string headerNamespace;
        [NonSerialized]
        private bool isDuplicate;

        public MessageHeaderException()
        {
        }

        public MessageHeaderException(string message) : this(message, null, null)
        {
        }

        protected MessageHeaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MessageHeaderException(string message, bool isDuplicate) : this(message, null, null)
        {
        }

        public MessageHeaderException(string message, Exception innerException) : this(message, null, null, innerException)
        {
        }

        public MessageHeaderException(string message, string headerName, string ns) : this(message, headerName, ns, (Exception) null)
        {
        }

        public MessageHeaderException(string message, string headerName, string ns, bool isDuplicate) : this(message, headerName, ns, isDuplicate, null)
        {
        }

        public MessageHeaderException(string message, string headerName, string ns, Exception innerException) : this(message, headerName, ns, false, innerException)
        {
        }

        public MessageHeaderException(string message, string headerName, string ns, bool isDuplicate, Exception innerException) : base(message, innerException)
        {
            this.headerName = headerName;
            this.headerNamespace = ns;
            this.isDuplicate = isDuplicate;
        }

        internal Message ProvideFault(MessageVersion messageVersion)
        {
            WSAddressing10ProblemHeaderQNameFault fault = new WSAddressing10ProblemHeaderQNameFault(this);
            Message message = Message.CreateMessage(messageVersion, fault, AddressingVersion.WSAddressing10.FaultAction);
            fault.AddHeaders(message.Headers);
            return message;
        }

        public string HeaderName
        {
            get
            {
                return this.headerName;
            }
        }

        public string HeaderNamespace
        {
            get
            {
                return this.headerNamespace;
            }
        }

        public bool IsDuplicate
        {
            get
            {
                return this.isDuplicate;
            }
        }
    }
}

