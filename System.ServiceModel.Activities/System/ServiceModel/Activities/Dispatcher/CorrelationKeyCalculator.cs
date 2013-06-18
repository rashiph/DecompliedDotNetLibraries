namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using System.Xml.Linq;

    internal class CorrelationKeyCalculator
    {
        private MessageBufferCalculator bufferCalculator;
        private CorrelationKeyCache keyCache;
        private MessageCalculator messageCalculator;
        private XName scopeName;
        private MessageFilterTable<SelectRuntime> whereRuntime = new MessageFilterTable<SelectRuntime>();

        public CorrelationKeyCalculator(XName scopeName)
        {
            this.scopeName = scopeName;
            this.keyCache = new CorrelationKeyCache();
        }

        public void AddQuery(MessageFilter where, MessageQueryTable<string> select, IDictionary<string, MessageQueryTable<string>> selectAdditional, bool isContextQuery)
        {
            SelectRuntime data = new SelectRuntime {
                Select = select,
                SelectAdditional = selectAdditional,
                IsContextQuery = isContextQuery
            };
            this.whereRuntime.Add(where, data);
        }

        public bool CalculateKeys(Message message, out InstanceKey instanceKey, out ICollection<InstanceKey> additionalKeys)
        {
            MessageCalculator messageCalculator = this.messageCalculator;
            if (messageCalculator == null)
            {
                messageCalculator = this.messageCalculator = new MessageCalculator(this);
            }
            return messageCalculator.CalculateKeys(message, null, out instanceKey, out additionalKeys);
        }

        public bool CalculateKeys(MessageBuffer buffer, Message messageToReadHeaders, out InstanceKey instanceKey, out ICollection<InstanceKey> additionalKeys)
        {
            MessageBufferCalculator bufferCalculator = this.bufferCalculator;
            if (bufferCalculator == null)
            {
                bufferCalculator = this.bufferCalculator = new MessageBufferCalculator(this);
            }
            return bufferCalculator.CalculateKeys(buffer, messageToReadHeaders, out instanceKey, out additionalKeys);
        }

        private abstract class Calculator<T>
        {
            private CorrelationKeyCalculator parent;

            public Calculator(CorrelationKeyCalculator parent)
            {
                this.parent = parent;
            }

            public bool CalculateKeys(T target, Message messageToReadHeaders, out InstanceKey instanceKey, out ICollection<InstanceKey> additionalKeys)
            {
                CorrelationKeyCalculator.SelectRuntime runtime;
                instanceKey = InstanceKey.InvalidKey;
                additionalKeys = null;
                if (!this.ExecuteWhere(target, messageToReadHeaders, this.parent.whereRuntime, out runtime))
                {
                    return false;
                }
                Dictionary<string, string> values = new Dictionary<string, string>();
                if (runtime.Select.Count > 0)
                {
                    bool flag = true;
                    foreach (KeyValuePair<MessageQuery, string> pair in this.ExecuteSelect(target, messageToReadHeaders, runtime.Select, runtime.IsContextQuery))
                    {
                        if (!(pair.Key is OptionalMessageQuery))
                        {
                            flag = false;
                        }
                        if (!string.IsNullOrEmpty(pair.Value))
                        {
                            values.Add(runtime.Select[pair.Key], pair.Value);
                        }
                    }
                    if (values.Count == 0)
                    {
                        if (!flag)
                        {
                            throw FxTrace.Exception.AsError(new ProtocolException(System.ServiceModel.Activities.SR.EmptyCorrelationQueryResults));
                        }
                    }
                    else
                    {
                        instanceKey = this.GetInstanceKey(values);
                        if (TD.TraceCorrelationKeysIsEnabled())
                        {
                            this.TraceCorrelationKeys(instanceKey, values);
                        }
                    }
                }
                foreach (KeyValuePair<string, MessageQueryTable<string>> pair2 in runtime.SelectAdditional)
                {
                    if (additionalKeys == null)
                    {
                        additionalKeys = new List<InstanceKey>();
                    }
                    values.Clear();
                    InstanceKey invalidKey = InstanceKey.InvalidKey;
                    bool flag2 = true;
                    foreach (KeyValuePair<MessageQuery, string> pair3 in this.ExecuteSelect(target, messageToReadHeaders, pair2.Value, runtime.IsContextQuery))
                    {
                        if (!(pair3.Key is OptionalMessageQuery))
                        {
                            flag2 = false;
                        }
                        if (!string.IsNullOrEmpty(pair3.Value))
                        {
                            values.Add(pair2.Value[pair3.Key], pair3.Value);
                        }
                    }
                    if (values.Count == 0)
                    {
                        if (!flag2)
                        {
                            throw FxTrace.Exception.AsError(new ProtocolException(System.ServiceModel.Activities.SR.EmptyCorrelationQueryResults));
                        }
                    }
                    else
                    {
                        CorrelationKey key2 = new CorrelationKey(values, this.parent.scopeName.ToString(), null) {
                            Name = pair2.Key
                        };
                        invalidKey = key2;
                        if (TD.TraceCorrelationKeysIsEnabled())
                        {
                            this.TraceCorrelationKeys(invalidKey, values);
                        }
                    }
                    additionalKeys.Add(invalidKey);
                }
                return true;
            }

            protected abstract IEnumerable<KeyValuePair<MessageQuery, string>> ExecuteSelect(T target, Message messageToReadHeaders, MessageQueryTable<string> select, bool IsContextQuery);
            protected abstract bool ExecuteWhere(T target, Message messageToReadHeaders, MessageFilterTable<CorrelationKeyCalculator.SelectRuntime> whereRuntime, out CorrelationKeyCalculator.SelectRuntime select);
            private CorrelationKey GetInstanceKey(Dictionary<string, string> values)
            {
                CorrelationKey key;
                if (values.Count > 3)
                {
                    return new CorrelationKey(values, this.parent.scopeName.ToString(), null);
                }
                CorrelationKeyCalculator.CorrelationCacheKey key2 = CorrelationKeyCalculator.CorrelationCacheKey.CreateKey(values);
                if (!this.parent.keyCache.TryGetValue(key2, out key))
                {
                    key = new CorrelationKey(values, this.parent.scopeName.ToString(), null);
                    this.parent.keyCache.Add(key2, key);
                }
                return key;
            }

            private void TraceCorrelationKeys(InstanceKey instanceKey, Dictionary<string, string> values)
            {
                StringBuilder builder = new StringBuilder();
                foreach (KeyValuePair<string, string> pair in values)
                {
                    StringBuilder introduced3 = builder.Append(pair.Key).Append(":");
                    introduced3.Append(pair.Value).Append(',');
                }
                TD.TraceCorrelationKeys(instanceKey.Value, builder.ToString(), this.parent.scopeName.ToString());
            }
        }

        private abstract class CorrelationCacheKey
        {
            protected CorrelationCacheKey()
            {
            }

            private static int CombineHashCodes(int h1, int h2)
            {
                return (((h1 << 5) + h1) ^ h2);
            }

            internal static CorrelationKeyCalculator.CorrelationCacheKey CreateKey(Dictionary<string, string> keys)
            {
                if (keys.Count == 1)
                {
                    return new SingleCacheKey(keys);
                }
                return new MultipleCacheKey(keys);
            }

            private class MultipleCacheKey : CorrelationKeyCalculator.CorrelationCacheKey
            {
                private int hashCode;
                private Dictionary<string, string> keyValues;

                public MultipleCacheKey(Dictionary<string, string> keys)
                {
                    this.keyValues = keys;
                    foreach (KeyValuePair<string, string> pair in this.keyValues)
                    {
                        int num = CorrelationKeyCalculator.CorrelationCacheKey.CombineHashCodes(this.hashCode, pair.Key.GetHashCode());
                        this.hashCode = CorrelationKeyCalculator.CorrelationCacheKey.CombineHashCodes(num, pair.Value.GetHashCode());
                    }
                }

                public override bool Equals(object obj)
                {
                    CorrelationKeyCalculator.CorrelationCacheKey.MultipleCacheKey key = obj as CorrelationKeyCalculator.CorrelationCacheKey.MultipleCacheKey;
                    if (((key == null) || (this.hashCode != key.hashCode)) || (this.keyValues.Count != key.keyValues.Count))
                    {
                        return false;
                    }
                    foreach (KeyValuePair<string, string> pair in key.keyValues)
                    {
                        string str;
                        if (!this.keyValues.TryGetValue(pair.Key, out str) || (str != pair.Value))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                public override int GetHashCode()
                {
                    return this.hashCode;
                }
            }

            private class SingleCacheKey : CorrelationKeyCalculator.CorrelationCacheKey
            {
                private int hashCode;
                private string key;
                private string value;

                public SingleCacheKey(Dictionary<string, string> keys)
                {
                    foreach (KeyValuePair<string, string> pair in keys)
                    {
                        this.key = pair.Key;
                        this.value = pair.Value;
                        this.hashCode = CorrelationKeyCalculator.CorrelationCacheKey.CombineHashCodes(this.key.GetHashCode(), this.value.GetHashCode());
                        break;
                    }
                }

                public override bool Equals(object obj)
                {
                    CorrelationKeyCalculator.CorrelationCacheKey.SingleCacheKey key = obj as CorrelationKeyCalculator.CorrelationCacheKey.SingleCacheKey;
                    if ((key == null) || (this.hashCode != key.hashCode))
                    {
                        return false;
                    }
                    return ((this.key == key.key) && (this.value == key.value));
                }

                public override int GetHashCode()
                {
                    return this.hashCode;
                }
            }
        }

        private class CorrelationKeyCache
        {
            private HopperCache cache = new HopperCache(0x80, false);
            private object cacheLock = new object();

            internal CorrelationKeyCache()
            {
            }

            internal void Add(CorrelationKeyCalculator.CorrelationCacheKey key, CorrelationKey value)
            {
                lock (this.cacheLock)
                {
                    this.cache.Add(key, value);
                }
            }

            internal bool TryGetValue(CorrelationKeyCalculator.CorrelationCacheKey key, out CorrelationKey value)
            {
                value = (CorrelationKey) this.cache.GetValue(this.cacheLock, key);
                return (value != null);
            }
        }

        private class MessageBufferCalculator : CorrelationKeyCalculator.Calculator<MessageBuffer>
        {
            public MessageBufferCalculator(CorrelationKeyCalculator parent) : base(parent)
            {
            }

            protected override IEnumerable<KeyValuePair<MessageQuery, string>> ExecuteSelect(MessageBuffer target, Message messageToReadHeaders, MessageQueryTable<string> select, bool isContextQuery)
            {
                if (isContextQuery && (messageToReadHeaders != null))
                {
                    return select.Evaluate<string>(messageToReadHeaders);
                }
                return select.Evaluate<string>(target);
            }

            protected override bool ExecuteWhere(MessageBuffer target, Message messageToReadHeaders, MessageFilterTable<CorrelationKeyCalculator.SelectRuntime> whereRuntime, out CorrelationKeyCalculator.SelectRuntime select)
            {
                return whereRuntime.GetMatchingValue(target, messageToReadHeaders, out select);
            }
        }

        private class MessageCalculator : CorrelationKeyCalculator.Calculator<Message>
        {
            public MessageCalculator(CorrelationKeyCalculator parent) : base(parent)
            {
            }

            protected override IEnumerable<KeyValuePair<MessageQuery, string>> ExecuteSelect(Message target, Message messageToReadHeaders, MessageQueryTable<string> select, bool isContextQuery)
            {
                return select.Evaluate<string>(target);
            }

            protected override bool ExecuteWhere(Message target, Message messageToReadHeaders, MessageFilterTable<CorrelationKeyCalculator.SelectRuntime> whereRuntime, out CorrelationKeyCalculator.SelectRuntime select)
            {
                return whereRuntime.GetMatchingValue(target, out select);
            }
        }

        private class SelectRuntime
        {
            internal bool IsContextQuery { get; set; }

            public MessageQueryTable<string> Select { get; set; }

            public IDictionary<string, MessageQueryTable<string>> SelectAdditional { get; set; }
        }
    }
}

