namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml.Linq;

    internal class CorrelationQueryBehavior : IEndpointBehavior, IChannelInitializer, IExtension<IContextChannel>
    {
        private const string contextCorrelationName = "wsc-instanceId";
        private const string cookieCorrelationName = "http-cookie";
        private CorrelationKeyCalculator correlationKeyCalculator;
        private const string defaultQueryFormat = "sm:correlation-data('{0}')";
        private ICollection<CorrelationQuery> queries;
        private ReadOnlyCollection<string> receiveNames;
        private ReadOnlyCollection<string> sendNames;
        private bool shouldPreserveMessage;
        private static string xPathForCookie = string.Format(CultureInfo.InvariantCulture, "sm:correlation-data('{0}')", new object[] { "http-cookie" });

        public CorrelationQueryBehavior(ICollection<CorrelationQuery> queries)
        {
            Fx.AssertAndThrow(queries != null, "queries must not be null");
            foreach (CorrelationQuery query in queries)
            {
                Fx.AssertAndThrow(query.Where != null, "CorrelationQuery.Where must not be null");
            }
            this.queries = queries;
            this.shouldPreserveMessage = true;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            ICorrelationDataSource property = endpoint.Binding.GetProperty<ICorrelationDataSource>(new BindingParameterCollection());
            if (property != null)
            {
                this.ConfigureBindingDataNames(property);
                this.ConfigureBindingDefaultQueries(endpoint, property, false);
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            ICorrelationDataSource property = endpoint.Binding.GetProperty<ICorrelationDataSource>(new BindingParameterCollection());
            if (property != null)
            {
                this.ConfigureBindingDataNames(property);
                this.ConfigureBindingDefaultQueries(endpoint, property, true);
            }
            System.ServiceModel.Description.ServiceDescription description = endpointDispatcher.ChannelDispatcher.Host.Description;
            WorkflowServiceHost host = endpointDispatcher.ChannelDispatcher.Host as WorkflowServiceHost;
            if (host == null)
            {
                this.ScopeName = XNamespace.Get(description.Namespace).GetName(description.Name);
            }
            else
            {
                this.ScopeName = host.DurableInstancingOptions.ScopeName;
            }
            endpointDispatcher.ChannelDispatcher.ChannelInitializers.Add(this);
            if (this.shouldPreserveMessage)
            {
                endpointDispatcher.DispatchRuntime.PreserveMessage = true;
            }
        }

        public static bool BindingHasDefaultQueries(Binding binding)
        {
            ICorrelationDataSource property = binding.GetProperty<ICorrelationDataSource>(new BindingParameterCollection());
            if (property != null)
            {
                foreach (CorrelationDataDescription description in property.DataSources)
                {
                    if (description.IsDefault)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ConfigureBindingDataNames(ICorrelationDataSource source)
        {
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            foreach (CorrelationDataDescription description in source.DataSources)
            {
                if (description.ReceiveValue)
                {
                    list.Add(description.Name);
                }
                if (description.SendValue && ((description.Name != "http-cookie") || this.IsCookieBasedQueryPresent()))
                {
                    list2.Add(description.Name);
                }
            }
            this.receiveNames = new ReadOnlyCollection<string>(list);
            this.sendNames = new ReadOnlyCollection<string>(list2);
        }

        private void ConfigureBindingDefaultQueries(ServiceEndpoint endpoint, ICorrelationDataSource source, bool dispatch)
        {
            if (CorrelationQuery.IsQueryCollectionSearchable(this.queries))
            {
                if (this.queries.Count <= 0)
                {
                    this.shouldPreserveMessage = false;
                }
                foreach (OperationDescription description in endpoint.Contract.Operations)
                {
                    string action = null;
                    CorrelationQuery query2 = null;
                    CorrelationQuery query3 = null;
                    string str = description.Messages[0].Action;
                    CorrelationQuery query = CorrelationQuery.FindCorrelationQueryForAction(this.queries, str);
                    if (!description.IsOneWay)
                    {
                        action = description.Messages[1].Action;
                        query2 = CorrelationQuery.FindCorrelationQueryForAction(this.queries, action);
                        if (!dispatch)
                        {
                            query3 = CorrelationQuery.FindCorrelationQueryForAction(this.queries, string.Empty);
                        }
                    }
                    bool flag = query == null;
                    bool flag2 = !description.IsOneWay && (query2 == null);
                    bool flag3 = (!description.IsOneWay && !dispatch) && (query3 == null);
                    if (flag && flag2)
                    {
                        if (str == "*")
                        {
                            flag2 = false;
                        }
                        else if (action == "*")
                        {
                            flag = false;
                        }
                        else if (str == action)
                        {
                            flag2 = false;
                        }
                    }
                    if (flag || flag2)
                    {
                        foreach (CorrelationDataDescription description2 in source.DataSources)
                        {
                            if (description2.IsDefault)
                            {
                                if (flag && ((dispatch && description2.ReceiveValue) || description2.SendValue))
                                {
                                    query = CreateDefaultCorrelationQuery(query, str, description2, ref this.shouldPreserveMessage);
                                }
                                if (flag2 && ((dispatch && description2.SendValue) || description2.ReceiveValue))
                                {
                                    query2 = CreateDefaultCorrelationQuery(query2, action, description2, ref this.shouldPreserveMessage);
                                }
                                if (flag3)
                                {
                                    query3 = CreateDefaultCorrelationQuery(query3, string.Empty, description2, ref this.shouldPreserveMessage);
                                }
                            }
                        }
                        if (flag && (query != null))
                        {
                            this.queries.Add(query);
                        }
                        if (flag2 && (query2 != null))
                        {
                            this.queries.Add(query2);
                        }
                        if (flag3 && (query3 != null))
                        {
                            this.queries.Add(query3);
                        }
                    }
                }
            }
        }

        private static CorrelationQuery CreateDefaultCorrelationQuery(CorrelationQuery query, string action, CorrelationDataDescription data, ref bool shouldPreserveMessage)
        {
            XPathMessageQuery query5 = new XPathMessageQuery {
                Expression = string.Format(CultureInfo.InvariantCulture, "sm:correlation-data('{0}')", new object[] { data.Name }),
                Namespaces = new XPathMessageContext()
            };
            MessageQuery query2 = query5;
            if (data.IsOptional)
            {
                OptionalMessageQuery query3 = new OptionalMessageQuery {
                    Query = query2
                };
                query2 = query3;
            }
            if (query == null)
            {
                MessageFilter filter;
                bool flag = data.Name == "wsc-instanceId";
                if (!shouldPreserveMessage && !flag)
                {
                    shouldPreserveMessage = true;
                }
                if (action == "*")
                {
                    filter = new MatchAllMessageFilter();
                }
                else
                {
                    filter = new ActionMessageFilter(new string[] { action });
                }
                CorrelationQuery query4 = new CorrelationQuery {
                    Where = filter,
                    IsDefaultContextQuery = flag
                };
                MessageQuerySet set = new MessageQuerySet();
                set.Add(data.Name, query2);
                query4.Select = set;
                return query4;
            }
            query.Select[data.Name] = query2;
            return query;
        }

        public CorrelationKeyCalculator GetKeyCalculator()
        {
            if (this.correlationKeyCalculator == null)
            {
                CorrelationKeyCalculator calculator = new CorrelationKeyCalculator(this.ScopeName);
                foreach (CorrelationQuery query in this.queries)
                {
                    IDictionary<string, MessageQueryTable<string>> selectAdditional = new Dictionary<string, MessageQueryTable<string>>();
                    int num = 0;
                    foreach (MessageQuerySet set in query.SelectAdditional)
                    {
                        selectAdditional.Add("SelectAdditional_item_" + num, set.GetMessageQueryTable());
                        num++;
                    }
                    calculator.AddQuery(query.Where, (query.Select != null) ? query.Select.GetMessageQueryTable() : new MessageQueryTable<string>(), selectAdditional, query.IsDefaultContextQuery);
                }
                this.correlationKeyCalculator = calculator;
            }
            return this.correlationKeyCalculator;
        }

        internal bool IsCookieBasedQueryPresent()
        {
            if (this.queries.Count > 0)
            {
                foreach (CorrelationQuery query in this.queries)
                {
                    foreach (MessageQuerySet set in query.SelectAdditional)
                    {
                        foreach (KeyValuePair<string, MessageQuery> pair in set)
                        {
                            XPathMessageQuery query2 = pair.Value as XPathMessageQuery;
                            if ((query2 != null) && query2.Expression.Equals(xPathForCookie))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        void IChannelInitializer.Initialize(IClientChannel channel)
        {
            channel.Extensions.Add(this);
        }

        void IExtension<IContextChannel>.Attach(IContextChannel owner)
        {
        }

        void IExtension<IContextChannel>.Detach(IContextChannel owner)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public ICollection<CorrelationQuery> CorrelationQueries
        {
            get
            {
                return this.queries;
            }
        }

        public ICollection<string> ReceiveNames
        {
            get
            {
                return this.receiveNames;
            }
        }

        internal XName ScopeName { get; set; }

        public ICollection<string> SendNames
        {
            get
            {
                return this.sendNames;
            }
        }

        public XName ServiceContractName { get; set; }
    }
}

