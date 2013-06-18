namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;

    public interface IMessageFilterTable<TFilterData> : IDictionary<MessageFilter, TFilterData>, ICollection<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable
    {
        bool GetMatchingFilter(Message message, out MessageFilter filter);
        bool GetMatchingFilter(MessageBuffer messageBuffer, out MessageFilter filter);
        bool GetMatchingFilters(Message message, ICollection<MessageFilter> results);
        bool GetMatchingFilters(MessageBuffer messageBuffer, ICollection<MessageFilter> results);
        bool GetMatchingValue(Message message, out TFilterData value);
        bool GetMatchingValue(MessageBuffer messageBuffer, out TFilterData value);
        bool GetMatchingValues(Message message, ICollection<TFilterData> results);
        bool GetMatchingValues(MessageBuffer messageBuffer, ICollection<TFilterData> results);
    }
}

