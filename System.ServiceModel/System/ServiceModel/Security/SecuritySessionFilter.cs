namespace System.ServiceModel.Security
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal sealed class SecuritySessionFilter : HeaderFilter
    {
        private string[] excludedActions;
        private bool isStrictMode;
        private UniqueId securityContextTokenId;
        private static readonly string SessionContextIdsProperty = string.Format(CultureInfo.InvariantCulture, "{0}/SecuritySessionContextIds", new object[] { "http://schemas.microsoft.com/ws/2006/05/security" });
        private SecurityStandardsManager standardsManager;

        public SecuritySessionFilter(UniqueId securityContextTokenId, SecurityStandardsManager standardsManager, bool isStrictMode, params string[] excludedActions)
        {
            if (securityContextTokenId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("securityContextTokenId"));
            }
            this.excludedActions = excludedActions;
            this.securityContextTokenId = securityContextTokenId;
            this.standardsManager = standardsManager;
            this.isStrictMode = isStrictMode;
        }

        internal static bool CanHandleException(Exception e)
        {
            return (((((e is XmlException) || (e is FormatException)) || ((e is SecurityTokenException) || (e is MessageSecurityException))) || ((e is ProtocolException) || (e is InvalidOperationException))) || (e is ArgumentException));
        }

        protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return new SecuritySessionFilterTable<FilterData>(this.standardsManager, this.isStrictMode, this.excludedActions);
        }

        public override bool Match(Message message)
        {
            if (!ShouldExcludeMessage(message, this.excludedActions))
            {
                List<UniqueId> list;
                object obj2;
                if (!message.Properties.TryGetValue(SessionContextIdsProperty, out obj2))
                {
                    list = new List<UniqueId>(1);
                    try
                    {
                        if (!this.standardsManager.TryGetSecurityContextIds(message, message.Version.Envelope.UltimateDestinationActorValues, this.isStrictMode, list))
                        {
                            return false;
                        }
                    }
                    catch (Exception exception)
                    {
                        if (!CanHandleException(exception))
                        {
                            throw;
                        }
                        return false;
                    }
                    message.Properties.Add(SessionContextIdsProperty, list);
                }
                else
                {
                    list = obj2 as List<UniqueId>;
                    if (list == null)
                    {
                        return false;
                    }
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == this.securityContextTokenId)
                    {
                        message.Properties.Remove(SessionContextIdsProperty);
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool Match(MessageBuffer buffer)
        {
            using (Message message = buffer.CreateMessage())
            {
                return this.Match(message);
            }
        }

        private static bool ShouldExcludeMessage(Message message, string[] excludedActions)
        {
            string action = message.Headers.Action;
            if ((excludedActions != null) && (action != null))
            {
                for (int i = 0; i < excludedActions.Length; i++)
                {
                    if (string.Equals(action, excludedActions[i], StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public UniqueId SecurityContextTokenId
        {
            get
            {
                return this.securityContextTokenId;
            }
        }

        private class SecuritySessionFilterTable<FilterData> : IMessageFilterTable<FilterData>, IDictionary<MessageFilter, FilterData>, ICollection<KeyValuePair<MessageFilter, FilterData>>, IEnumerable<KeyValuePair<MessageFilter, FilterData>>, IEnumerable
        {
            private Dictionary<UniqueId, KeyValuePair<MessageFilter, FilterData>> contextMappings;
            private string[] excludedActions;
            private Dictionary<MessageFilter, FilterData> filterMappings;
            private bool isStrictMode;
            private SecurityStandardsManager standardsManager;

            public SecuritySessionFilterTable(SecurityStandardsManager standardsManager, bool isStrictMode, string[] excludedActions)
            {
                if (standardsManager == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("standardsManager");
                }
                if (excludedActions == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("excludedActions");
                }
                this.standardsManager = standardsManager;
                this.excludedActions = new string[excludedActions.Length];
                excludedActions.CopyTo(this.excludedActions, 0);
                this.isStrictMode = isStrictMode;
                this.contextMappings = new Dictionary<UniqueId, KeyValuePair<MessageFilter, FilterData>>();
                this.filterMappings = new Dictionary<MessageFilter, FilterData>();
            }

            public void Add(KeyValuePair<MessageFilter, FilterData> item)
            {
                this.Add(item.Key, item.Value);
            }

            public void Add(MessageFilter filter, FilterData data)
            {
                SecuritySessionFilter filter2 = filter as SecuritySessionFilter;
                if (filter2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UnknownFilterType", new object[] { filter.GetType() })));
                }
                if (filter2.standardsManager != this.standardsManager)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("StandardsManagerDoesNotMatch")));
                }
                if (filter2.isStrictMode != this.isStrictMode)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("FilterStrictModeDifferent")));
                }
                if (this.contextMappings.ContainsKey(filter2.SecurityContextTokenId))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecuritySessionIdAlreadyPresentInFilterTable", new object[] { filter2.SecurityContextTokenId })));
                }
                this.filterMappings.Add(filter, data);
                this.contextMappings.Add(filter2.SecurityContextTokenId, new KeyValuePair<MessageFilter, FilterData>(filter, data));
            }

            public void Clear()
            {
                this.filterMappings.Clear();
                this.contextMappings.Clear();
            }

            public bool Contains(KeyValuePair<MessageFilter, FilterData> item)
            {
                return this.ContainsKey(item.Key);
            }

            public bool ContainsKey(MessageFilter filter)
            {
                return this.filterMappings.ContainsKey(filter);
            }

            public void CopyTo(KeyValuePair<MessageFilter, FilterData>[] array, int arrayIndex)
            {
                int index = arrayIndex;
                foreach (KeyValuePair<MessageFilter, FilterData> pair in this.contextMappings.Values)
                {
                    array[index] = pair;
                    index++;
                }
            }

            public IEnumerator<KeyValuePair<MessageFilter, FilterData>> GetEnumerator()
            {
                return this.contextMappings.Values.GetEnumerator();
            }

            public bool GetMatchingFilter(Message message, out MessageFilter filter)
            {
                KeyValuePair<MessageFilter, FilterData> pair;
                if (!this.TryMatchCore(message, out pair))
                {
                    filter = null;
                    return false;
                }
                filter = pair.Key;
                return true;
            }

            public bool GetMatchingFilter(MessageBuffer buffer, out MessageFilter filter)
            {
                using (Message message = buffer.CreateMessage())
                {
                    return this.GetMatchingFilter(message, out filter);
                }
            }

            public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
            {
                MessageFilter filter;
                if (this.GetMatchingFilter(message, out filter))
                {
                    results.Add(filter);
                    return true;
                }
                return false;
            }

            public bool GetMatchingFilters(MessageBuffer buffer, ICollection<MessageFilter> results)
            {
                using (Message message = buffer.CreateMessage())
                {
                    return this.GetMatchingFilters(message, results);
                }
            }

            public bool GetMatchingValue(Message message, out FilterData data)
            {
                KeyValuePair<MessageFilter, FilterData> pair;
                if (!this.TryMatchCore(message, out pair))
                {
                    data = default(FilterData);
                    return false;
                }
                data = pair.Value;
                return true;
            }

            public bool GetMatchingValue(MessageBuffer buffer, out FilterData data)
            {
                using (Message message = buffer.CreateMessage())
                {
                    return this.GetMatchingValue(message, out data);
                }
            }

            public bool GetMatchingValues(Message message, ICollection<FilterData> results)
            {
                FilterData local;
                if (!this.GetMatchingValue(message, out local))
                {
                    return false;
                }
                results.Add(local);
                return true;
            }

            public bool GetMatchingValues(MessageBuffer buffer, ICollection<FilterData> results)
            {
                using (Message message = buffer.CreateMessage())
                {
                    return this.GetMatchingValues(message, results);
                }
            }

            public bool Remove(KeyValuePair<MessageFilter, FilterData> item)
            {
                return this.Remove(item.Key);
            }

            public bool Remove(MessageFilter filter)
            {
                SecuritySessionFilter filter2 = filter as SecuritySessionFilter;
                if (filter2 == null)
                {
                    return false;
                }
                bool flag = this.filterMappings.Remove(filter);
                if (flag)
                {
                    this.contextMappings.Remove(filter2.SecurityContextTokenId);
                }
                return flag;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            private bool TryGetContextIds(Message message, out List<UniqueId> contextIds)
            {
                object obj2;
                if (!message.Properties.TryGetValue(SecuritySessionFilter.SessionContextIdsProperty, out obj2))
                {
                    contextIds = new List<UniqueId>(1);
                    return this.standardsManager.TryGetSecurityContextIds(message, message.Version.Envelope.UltimateDestinationActorValues, this.isStrictMode, contextIds);
                }
                contextIds = obj2 as List<UniqueId>;
                return (contextIds != null);
            }

            public bool TryGetValue(MessageFilter filter, out FilterData data)
            {
                return this.filterMappings.TryGetValue(filter, out data);
            }

            private bool TryMatchCore(Message message, out KeyValuePair<MessageFilter, FilterData> match)
            {
                match = new KeyValuePair<MessageFilter, FilterData>();
                if (!SecuritySessionFilter.ShouldExcludeMessage(message, this.excludedActions))
                {
                    List<UniqueId> list;
                    try
                    {
                        if (!this.TryGetContextIds(message, out list))
                        {
                            return false;
                        }
                    }
                    catch (Exception exception)
                    {
                        if (!SecuritySessionFilter.CanHandleException(exception))
                        {
                            throw;
                        }
                        return false;
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (this.contextMappings.TryGetValue(list[i], out match))
                        {
                            message.Properties.Remove(SecuritySessionFilter.SessionContextIdsProperty);
                            return true;
                        }
                    }
                }
                return false;
            }

            public int Count
            {
                get
                {
                    return this.filterMappings.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public FilterData this[MessageFilter filter]
            {
                get
                {
                    return this.filterMappings[filter];
                }
                set
                {
                    if (this.filterMappings.ContainsKey(filter))
                    {
                        this.Remove(filter);
                    }
                    this.Add(filter, value);
                }
            }

            public ICollection<MessageFilter> Keys
            {
                get
                {
                    return this.filterMappings.Keys;
                }
            }

            public ICollection<FilterData> Values
            {
                get
                {
                    return (ICollection<FilterData>) this.filterMappings.Values;
                }
            }
        }
    }
}

