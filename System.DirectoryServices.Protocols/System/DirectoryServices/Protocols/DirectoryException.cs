namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DirectoryException : Exception
    {
        public DirectoryException()
        {
            Utility.CheckOSVersion();
        }

        public DirectoryException(string message) : base(message)
        {
            Utility.CheckOSVersion();
        }

        protected DirectoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DirectoryException(string message, Exception inner) : base(message, inner)
        {
            Utility.CheckOSVersion();
        }
    }
}

