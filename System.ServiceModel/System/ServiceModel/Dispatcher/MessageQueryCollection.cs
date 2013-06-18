namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;

    public abstract class MessageQueryCollection : Collection<MessageQuery>
    {
        protected MessageQueryCollection()
        {
        }

        public abstract IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(Message message);
        public abstract IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(MessageBuffer buffer);
    }
}

