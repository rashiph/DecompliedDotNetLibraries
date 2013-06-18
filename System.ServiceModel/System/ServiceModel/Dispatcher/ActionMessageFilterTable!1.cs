namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    [DataContract]
    internal class ActionMessageFilterTable<TFilterData> : IMessageFilterTable<TFilterData>, IDictionary<MessageFilter, TFilterData>, ICollection<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable
    {
        private Dictionary<string, List<MessageFilter>> actions;
        private List<MessageFilter> always;
        private Dictionary<MessageFilter, TFilterData> filters;

        public ActionMessageFilterTable()
        {
            this.Init();
        }

        public void Add(KeyValuePair<MessageFilter, TFilterData> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(ActionMessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            this.filters.Add(filter, data);
            if (filter.Actions.Count == 0)
            {
                this.always.Add(filter);
            }
            else
            {
                for (int i = 0; i < filter.Actions.Count; i++)
                {
                    List<MessageFilter> list;
                    if (!this.actions.TryGetValue(filter.Actions[i], out list))
                    {
                        list = new List<MessageFilter>();
                        this.actions.Add(filter.Actions[i], list);
                    }
                    list.Add(filter);
                }
            }
        }

        public void Add(MessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            this.Add((ActionMessageFilter) filter, data);
        }

        public void Clear()
        {
            this.filters.Clear();
            this.actions.Clear();
            this.always.Clear();
        }

        public bool Contains(KeyValuePair<MessageFilter, TFilterData> item)
        {
            return this.filters.Contains(item);
        }

        public bool ContainsKey(MessageFilter filter)
        {
            return this.filters.ContainsKey(filter);
        }

        public void CopyTo(KeyValuePair<MessageFilter, TFilterData>[] array, int arrayIndex)
        {
            this.filters.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<MessageFilter, TFilterData>> GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }

        public bool GetMatchingFilter(Message message, out MessageFilter filter)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            filter = this.InnerMatch(message);
            return (filter != null);
        }

        public bool GetMatchingFilter(MessageBuffer messageBuffer, out MessageFilter filter)
        {
            bool flag;
            if (messageBuffer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            Message message = messageBuffer.CreateMessage();
            try
            {
                filter = this.InnerMatch(message);
                flag = filter != null;
            }
            finally
            {
                message.Close();
            }
            return flag;
        }

        public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            this.InnerMatch(message, results);
            return (count != results.Count);
        }

        public bool GetMatchingFilters(MessageBuffer messageBuffer, ICollection<MessageFilter> results)
        {
            bool flag;
            if (messageBuffer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            Message message = messageBuffer.CreateMessage();
            try
            {
                int count = results.Count;
                this.InnerMatch(message, results);
                flag = count != results.Count;
            }
            finally
            {
                message.Close();
            }
            return flag;
        }

        public bool GetMatchingValue(Message message, out TFilterData data)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            MessageFilter filter = this.InnerMatch(message);
            if (filter == null)
            {
                data = default(TFilterData);
                return false;
            }
            data = this.filters[filter];
            return true;
        }

        public bool GetMatchingValue(MessageBuffer messageBuffer, out TFilterData data)
        {
            if (messageBuffer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            MessageFilter filter = null;
            Message message = messageBuffer.CreateMessage();
            try
            {
                filter = this.InnerMatch(message);
            }
            finally
            {
                message.Close();
            }
            if (filter == null)
            {
                data = default(TFilterData);
                return false;
            }
            data = this.filters[filter];
            return true;
        }

        public bool GetMatchingValues(Message message, ICollection<TFilterData> results)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            this.InnerMatchData(message, results);
            return (count != results.Count);
        }

        public bool GetMatchingValues(MessageBuffer messageBuffer, ICollection<TFilterData> results)
        {
            bool flag;
            if (messageBuffer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            Message message = messageBuffer.CreateMessage();
            try
            {
                int count = results.Count;
                this.InnerMatchData(message, results);
                flag = count != results.Count;
            }
            finally
            {
                message.Close();
            }
            return flag;
        }

        private void Init()
        {
            this.filters = new Dictionary<MessageFilter, TFilterData>();
            this.actions = new Dictionary<string, List<MessageFilter>>();
            this.always = new List<MessageFilter>();
        }

        private MessageFilter InnerMatch(Message message)
        {
            List<MessageFilter> list;
            string action = message.Headers.Action;
            if (action == null)
            {
                action = string.Empty;
            }
            if (this.actions.TryGetValue(action, out list))
            {
                if ((this.always.Count + list.Count) > 1)
                {
                    List<MessageFilter> list2 = new List<MessageFilter>(list);
                    list2.AddRange(this.always);
                    Collection<MessageFilter> filters = new Collection<MessageFilter>(list2);
                    throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, filters), message);
                }
                return list[0];
            }
            if (this.always.Count > 1)
            {
                Collection<MessageFilter> collection2 = new Collection<MessageFilter>(new List<MessageFilter>(this.always));
                throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, collection2), message);
            }
            if (this.always.Count == 1)
            {
                return this.always[0];
            }
            return null;
        }

        private void InnerMatch(Message message, ICollection<MessageFilter> results)
        {
            List<MessageFilter> list;
            for (int i = 0; i < this.always.Count; i++)
            {
                results.Add(this.always[i]);
            }
            string action = message.Headers.Action;
            if (action == null)
            {
                action = string.Empty;
            }
            if (this.actions.TryGetValue(action, out list))
            {
                for (int j = 0; j < list.Count; j++)
                {
                    results.Add(list[j]);
                }
            }
        }

        private void InnerMatchData(Message message, ICollection<TFilterData> results)
        {
            List<MessageFilter> list;
            for (int i = 0; i < this.always.Count; i++)
            {
                results.Add(this.filters[this.always[i]]);
            }
            string action = message.Headers.Action;
            if (action == null)
            {
                action = string.Empty;
            }
            if (this.actions.TryGetValue(action, out list))
            {
                for (int j = 0; j < list.Count; j++)
                {
                    results.Add(this.filters[list[j]]);
                }
            }
        }

        public bool Remove(KeyValuePair<MessageFilter, TFilterData> item)
        {
            return (this.filters.Contains(item) && this.Remove(item.Key));
        }

        public bool Remove(ActionMessageFilter filter)
        {
            if (filter == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            if (!this.filters.Remove(filter))
            {
                return false;
            }
            if (filter.Actions.Count == 0)
            {
                this.always.Remove(filter);
            }
            else
            {
                for (int i = 0; i < filter.Actions.Count; i++)
                {
                    List<MessageFilter> list = this.actions[filter.Actions[i]];
                    if (list.Count == 1)
                    {
                        this.actions.Remove(filter.Actions[i]);
                    }
                    else
                    {
                        list.Remove(filter);
                    }
                }
            }
            return true;
        }

        public bool Remove(MessageFilter filter)
        {
            if (filter == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            ActionMessageFilter filter2 = filter as ActionMessageFilter;
            return ((filter2 != null) && this.Remove(filter2));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(MessageFilter filter, out TFilterData data)
        {
            return this.filters.TryGetValue(filter, out data);
        }

        public int Count
        {
            get
            {
                return this.filters.Count;
            }
        }

        [DataMember(IsRequired=true)]
        private Entry<TFilterData>[] Entries
        {
            get
            {
                Entry<TFilterData>[] entryArray = new Entry<TFilterData>[this.Count];
                int num = 0;
                foreach (KeyValuePair<MessageFilter, TFilterData> pair in this.filters)
                {
                    entryArray[num++] = new Entry<TFilterData>(pair.Key, pair.Value);
                }
                return entryArray;
            }
            set
            {
                this.Init();
                for (int i = 0; i < value.Length; i++)
                {
                    this.Add(value[i].filter, value[i].data);
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public TFilterData this[MessageFilter filter]
        {
            get
            {
                return this.filters[filter];
            }
            set
            {
                if (this.filters.ContainsKey(filter))
                {
                    this.filters[filter] = value;
                }
                else
                {
                    this.Add(filter, value);
                }
            }
        }

        public ICollection<MessageFilter> Keys
        {
            get
            {
                return this.filters.Keys;
            }
        }

        public ICollection<TFilterData> Values
        {
            get
            {
                return (ICollection<TFilterData>) this.filters.Values;
            }
        }

        [DataContract]
        private class Entry
        {
            [DataMember(IsRequired=true)]
            internal TFilterData data;
            [DataMember(IsRequired=true)]
            internal MessageFilter filter;

            internal Entry(MessageFilter f, TFilterData d)
            {
                this.filter = f;
                this.data = d;
            }
        }
    }
}

