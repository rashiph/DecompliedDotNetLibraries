namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [Serializable]
    public class MultipleFilterMatchesException : SystemException
    {
        [NonSerialized]
        private Collection<MessageFilter> filters;

        public MultipleFilterMatchesException() : this(System.ServiceModel.SR.GetString("FilterMultipleMatches"))
        {
        }

        public MultipleFilterMatchesException(string message) : this(message, null, null)
        {
        }

        protected MultipleFilterMatchesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.filters = null;
        }

        public MultipleFilterMatchesException(string message, Exception innerException) : this(message, innerException, null)
        {
        }

        public MultipleFilterMatchesException(string message, Collection<MessageFilter> filters) : this(message, null, filters)
        {
        }

        public MultipleFilterMatchesException(string message, Exception innerException, Collection<MessageFilter> filters) : base(message, innerException)
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

