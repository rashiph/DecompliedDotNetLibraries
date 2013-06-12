namespace System.Web.Caching
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class DatabaseNotEnabledForNotificationException : SystemException
    {
        public DatabaseNotEnabledForNotificationException()
        {
        }

        public DatabaseNotEnabledForNotificationException(string message) : base(message)
        {
        }

        internal DatabaseNotEnabledForNotificationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DatabaseNotEnabledForNotificationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

