namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [Serializable]
    public class NavigatorInvalidBodyAccessException : InvalidBodyAccessException
    {
        public NavigatorInvalidBodyAccessException() : this(System.ServiceModel.SR.GetString("SeekableMessageNavBodyForbidden"))
        {
        }

        public NavigatorInvalidBodyAccessException(string message) : this(message, null)
        {
        }

        protected NavigatorInvalidBodyAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NavigatorInvalidBodyAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal FilterInvalidBodyAccessException Process(Opcode op)
        {
            Collection<MessageFilter> filters = new Collection<MessageFilter>();
            op.CollectXPathFilters(filters);
            return new FilterInvalidBodyAccessException(this.Message, base.InnerException, filters);
        }
    }
}

