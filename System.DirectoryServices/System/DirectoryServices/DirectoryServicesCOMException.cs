namespace System.DirectoryServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class DirectoryServicesCOMException : COMException, ISerializable
    {
        private int extendederror;
        private string extendedmessage;

        public DirectoryServicesCOMException()
        {
            this.extendedmessage = "";
        }

        public DirectoryServicesCOMException(string message) : base(message)
        {
            this.extendedmessage = "";
        }

        protected DirectoryServicesCOMException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.extendedmessage = "";
        }

        public DirectoryServicesCOMException(string message, Exception inner) : base(message, inner)
        {
            this.extendedmessage = "";
        }

        internal DirectoryServicesCOMException(string extendedMessage, int extendedError, COMException e) : base(e.Message, e.ErrorCode)
        {
            this.extendedmessage = "";
            this.extendederror = extendedError;
            this.extendedmessage = extendedMessage;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public int ExtendedError
        {
            get
            {
                return this.extendederror;
            }
        }

        public string ExtendedErrorMessage
        {
            get
            {
                return this.extendedmessage;
            }
        }
    }
}

