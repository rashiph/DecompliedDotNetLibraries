namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ActiveDirectoryObjectExistsException : Exception
    {
        public ActiveDirectoryObjectExistsException()
        {
        }

        public ActiveDirectoryObjectExistsException(string message) : base(message)
        {
        }

        protected ActiveDirectoryObjectExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ActiveDirectoryObjectExistsException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

