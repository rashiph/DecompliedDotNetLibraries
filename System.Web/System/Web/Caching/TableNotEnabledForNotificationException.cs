namespace System.Web.Caching
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class TableNotEnabledForNotificationException : SystemException
    {
        public TableNotEnabledForNotificationException()
        {
        }

        public TableNotEnabledForNotificationException(string message) : base(message)
        {
        }

        internal TableNotEnabledForNotificationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TableNotEnabledForNotificationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

