namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.DirectoryServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ActiveDirectoryServerDownException : Exception, ISerializable
    {
        private int errorCode;
        private string name;

        public ActiveDirectoryServerDownException()
        {
        }

        public ActiveDirectoryServerDownException(string message) : base(message)
        {
        }

        protected ActiveDirectoryServerDownException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ActiveDirectoryServerDownException(string message, Exception inner) : base(message, inner)
        {
        }

        public ActiveDirectoryServerDownException(string message, int errorCode, string name) : base(message)
        {
            this.errorCode = errorCode;
            this.name = name;
        }

        public ActiveDirectoryServerDownException(string message, Exception inner, int errorCode, string name) : base(message, inner)
        {
            this.errorCode = errorCode;
            this.name = name;
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

        public override string Message
        {
            get
            {
                string message = base.Message;
                if ((this.name != null) && (this.name.Length != 0))
                {
                    return (message + Environment.NewLine + Res.GetString("Name", new object[] { this.name }) + Environment.NewLine);
                }
                return message;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

