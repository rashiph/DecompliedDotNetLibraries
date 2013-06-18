namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [Serializable]
    public class MessageFilterException : CommunicationException
    {
        [NonSerialized]
        private Collection<MessageFilter> filters;

        public MessageFilterException()
        {
        }

        public MessageFilterException(string message) : this(message, null, null)
        {
        }

        protected MessageFilterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.filters = null;
        }

        public MessageFilterException(string message, Exception innerException) : this(message, innerException, null)
        {
        }

        public MessageFilterException(string message, Collection<MessageFilter> filters) : this(message, null, filters)
        {
        }

        public MessageFilterException(string message, Exception innerException, Collection<MessageFilter> filters) : base(message, innerException)
        {
            this.filters = filters;
        }

        public Collection<MessageFilter> Filters
        {
            get
            {
                return this.filters;
            }
        }
    }
}

