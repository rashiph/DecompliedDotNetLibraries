namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class LdapException : DirectoryException, ISerializable
    {
        private int errorCode;
        internal PartialResultsCollection results;
        private string serverErrorMessage;

        public LdapException()
        {
            this.results = new PartialResultsCollection();
        }

        public LdapException(int errorCode) : base(Res.GetString("DefaultLdapError"))
        {
            this.results = new PartialResultsCollection();
            this.errorCode = errorCode;
        }

        public LdapException(string message) : base(message)
        {
            this.results = new PartialResultsCollection();
        }

        public LdapException(int errorCode, string message) : base(message)
        {
            this.results = new PartialResultsCollection();
            this.errorCode = errorCode;
        }

        protected LdapException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.results = new PartialResultsCollection();
        }

        public LdapException(string message, Exception inner) : base(message, inner)
        {
            this.results = new PartialResultsCollection();
        }

        public LdapException(int errorCode, string message, Exception inner) : base(message, inner)
        {
            this.results = new PartialResultsCollection();
            this.errorCode = errorCode;
        }

        public LdapException(int errorCode, string message, string serverErrorMessage) : base(message)
        {
            this.results = new PartialResultsCollection();
            this.errorCode = errorCode;
            this.serverErrorMessage = serverErrorMessage;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }

        public PartialResultsCollection PartialResults
        {
            get
            {
                return this.results;
            }
        }

        public string ServerErrorMessage
        {
            get
            {
                return this.serverErrorMessage;
            }
        }
    }
}

