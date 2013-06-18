namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [Serializable]
    public class FilterInvalidBodyAccessException : InvalidBodyAccessException
    {
        [NonSerialized]
        private Collection<MessageFilter> filters;

        public FilterInvalidBodyAccessException() : this(System.ServiceModel.SR.GetString("SeekableMessageNavBodyForbidden"))
        {
        }

        public FilterInvalidBodyAccessException(string message) : this(message, null, null)
        {
        }

        protected FilterInvalidBodyAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.filters = null;
        }

        public FilterInvalidBodyAccessException(string message, Exception innerException) : this(message, innerException, null)
        {
        }

        public FilterInvalidBodyAccessException(string message, Collection<MessageFilter> filters) : this(message, null, filters)
        {
        }

        public FilterInvalidBodyAccessException(string message, Exception innerException, Collection<MessageFilter> filters) : base(message, innerException)
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

