namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Dispatcher;

    public class MessageQuerySet : Dictionary<string, MessageQuery>
    {
        public MessageQuerySet()
        {
        }

        public MessageQuerySet(MessageQueryTable<string> queryTable)
        {
            if (queryTable == null)
            {
                throw FxTrace.Exception.ArgumentNull("queryTable");
            }
            InvertDictionary<MessageQuery, string>(queryTable, this);
        }

        public MessageQueryTable<string> GetMessageQueryTable()
        {
            MessageQueryTable<string> destination = new MessageQueryTable<string>();
            InvertDictionary<string, MessageQuery>(this, destination);
            return destination;
        }

        private static void InvertDictionary<TKey, TValue>(IDictionary<TKey, TValue> source, IDictionary<TValue, TKey> destination)
        {
            foreach (KeyValuePair<TKey, TValue> pair in source)
            {
                destination.Add(pair.Value, pair.Key);
            }
        }

        [DefaultValue((string) null)]
        public string Name { get; set; }
    }
}

