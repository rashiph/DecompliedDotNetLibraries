namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Debugger;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Description;
    using System.ServiceModel.XamlIntegration;
    using System.Windows.Markup;
    using System.Xml;
    using System.Xml.Linq;

    [ContentProperty("Body")]
    public class WorkflowService : IDebuggableWorkflowTree
    {
        private IDictionary<XName, ContractDescription> cachedInferredContracts;
        private IDictionary<XName, Collection<CorrelationQuery>> correlationQueryByContract;
        private Collection<Endpoint> endpoints;
        private XName inferedServiceName;
        private IList<Receive> knownServiceActivities;
        private HashSet<ReceiveAndReplyTuple> receiveAndReplyPairs;
        private System.ServiceModel.Description.ServiceDescription serviceDescription;

        private void CollectCorrelationQuery(ref Collection<CorrelationQuery> queries, XName serviceContractName, CorrelationQuery correlationQuery)
        {
            if (correlationQuery != null)
            {
                if ((queries == null) && !this.correlationQueryByContract.TryGetValue(serviceContractName, out queries))
                {
                    queries = new Collection<CorrelationQuery>();
                    this.correlationQueryByContract.Add(serviceContractName, queries);
                }
                queries.Add(correlationQuery);
            }
        }

        private void CollectCorrelationQueryFromReply(ref Collection<CorrelationQuery> correlationQueries, XName serviceContractName, Activity reply, OperationDescription operation)
        {
            SendReply reply2 = reply as SendReply;
            if (reply2 != null)
            {
                CorrelationQuery correlationQuery = ContractInferenceHelper.CreateServerCorrelationQuery(null, reply2.CorrelationInitializers, operation, true);
                this.CollectCorrelationQuery(ref correlationQueries, serviceContractName, correlationQuery);
            }
        }

        private void CorrectOutMessageForOperationWithFault(Receive receive, OperationInfo operationInfo)
        {
            Receive receive2 = operationInfo.Receive;
            if (((receive != receive2) && receive.HasReply) && (!receive2.HasReply && receive2.HasFault))
            {
                ContractInferenceHelper.CorrectOutMessageForOperation(receive, operationInfo.OperationDescription);
                operationInfo.Receive = receive;
            }
        }

        private XName FixServiceContractName(XName serviceContractName)
        {
            XName name = serviceContractName ?? this.InternalName;
            ContractInferenceHelper.ProvideDefaultNamespace(ref name);
            return name;
        }

        internal IDictionary<XName, ContractDescription> GetContractDescriptions()
        {
            if (this.cachedInferredContracts == null)
            {
                this.WalkActivityTree();
                this.correlationQueryByContract = new Dictionary<XName, Collection<CorrelationQuery>>();
                IDictionary<XName, ContractDescription> dictionary = new Dictionary<XName, ContractDescription>();
                IDictionary<ContractAndOperationNameTuple, OperationInfo> dictionary2 = new Dictionary<ContractAndOperationNameTuple, OperationInfo>();
                foreach (Receive receive in this.knownServiceActivities)
                {
                    OperationInfo info;
                    XName serviceContractXName = this.FixServiceContractName(receive.ServiceContractName);
                    ContractAndOperationNameTuple key = new ContractAndOperationNameTuple(serviceContractXName, receive.OperationName);
                    if (dictionary2.TryGetValue(key, out info))
                    {
                        ContractValidationHelper.ValidateReceiveWithReceive(receive, info.Receive);
                    }
                    else
                    {
                        ContractDescription description;
                        if (!dictionary.TryGetValue(serviceContractXName, out description))
                        {
                            description = new ContractDescription(serviceContractXName.LocalName, serviceContractXName.NamespaceName) {
                                ConfigurationName = serviceContractXName.LocalName
                            };
                            dictionary.Add(serviceContractXName, description);
                        }
                        OperationDescription item = ContractInferenceHelper.CreateOperationDescription(receive, description);
                        description.Operations.Add(item);
                        info = new OperationInfo(receive, item);
                        dictionary2.Add(key, info);
                    }
                    this.CorrectOutMessageForOperationWithFault(receive, info);
                    ContractInferenceHelper.UpdateIsOneWayFlag(receive, info.OperationDescription);
                    ContractInferenceHelper.AddFaultDescription(receive, info.OperationDescription);
                    ContractInferenceHelper.AddKnownTypesToOperation(receive, info.OperationDescription);
                    ContractInferenceHelper.AddReceiveToFormatterBehavior(receive, info.OperationDescription);
                    Collection<CorrelationQuery> queries = null;
                    if (receive.HasCorrelatesOn || receive.HasCorrelationInitializers)
                    {
                        MessageQuerySet select = receive.HasCorrelatesOn ? receive.CorrelatesOn : null;
                        CorrelationQuery correlationQuery = ContractInferenceHelper.CreateServerCorrelationQuery(select, receive.CorrelationInitializers, info.OperationDescription, false);
                        this.CollectCorrelationQuery(ref queries, serviceContractXName, correlationQuery);
                    }
                    if (receive.HasReply)
                    {
                        foreach (SendReply reply in receive.FollowingReplies)
                        {
                            ReceiveAndReplyTuple tuple2 = new ReceiveAndReplyTuple(receive, reply);
                            this.receiveAndReplyPairs.Remove(tuple2);
                            this.CollectCorrelationQueryFromReply(ref queries, serviceContractXName, reply, info.OperationDescription);
                            reply.SetContractName(serviceContractXName);
                        }
                    }
                    if (receive.HasFault)
                    {
                        foreach (Activity activity in receive.FollowingFaults)
                        {
                            ReceiveAndReplyTuple tuple3 = new ReceiveAndReplyTuple(receive, activity);
                            this.receiveAndReplyPairs.Remove(tuple3);
                            this.CollectCorrelationQueryFromReply(ref queries, serviceContractXName, activity, info.OperationDescription);
                        }
                    }
                }
                if (this.receiveAndReplyPairs.Count != 0)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.DanglingReceive));
                }
                if (System.ServiceModel.Activities.TD.InferredContractDescriptionIsEnabled())
                {
                    foreach (ContractDescription description3 in dictionary.Values)
                    {
                        System.ServiceModel.Activities.TD.InferredContractDescription(description3.Name, description3.Namespace);
                        if (System.ServiceModel.Activities.TD.InferredOperationDescriptionIsEnabled())
                        {
                            foreach (OperationDescription description4 in description3.Operations)
                            {
                                System.ServiceModel.Activities.TD.InferredOperationDescription(description4.Name, description3.Name, description4.IsOneWay.ToString());
                            }
                        }
                    }
                }
                this.cachedInferredContracts = dictionary;
            }
            return this.cachedInferredContracts;
        }

        internal System.ServiceModel.Description.ServiceDescription GetEmptyServiceDescription()
        {
            if (this.serviceDescription == null)
            {
                this.WalkActivityTree();
                System.ServiceModel.Description.ServiceDescription description = new System.ServiceModel.Description.ServiceDescription {
                    Name = this.InternalName.LocalName,
                    Namespace = string.IsNullOrEmpty(this.InternalName.NamespaceName) ? "http://tempuri.org/" : this.InternalName.NamespaceName,
                    ConfigurationName = this.ConfigurationName ?? this.InternalName.LocalName
                };
                description.Behaviors.Add(new WorkflowServiceBehavior(this.Body));
                this.serviceDescription = description;
            }
            return this.serviceDescription;
        }

        public Activity GetWorkflowRoot()
        {
            return this.Body;
        }

        internal void ResetServiceDescription()
        {
            this.serviceDescription = null;
            this.cachedInferredContracts = null;
        }

        private void WalkActivityTree()
        {
            if (this.knownServiceActivities == null)
            {
                if (this.Body == null)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.MissingBodyInWorkflowService));
                }
                WorkflowInspectionServices.CacheMetadata(this.Body);
                this.knownServiceActivities = new List<Receive>();
                this.receiveAndReplyPairs = new HashSet<ReceiveAndReplyTuple>();
                Queue<QueueItem> queue = new Queue<QueueItem>();
                queue.Enqueue(new QueueItem(this.Body, null, null));
                while (queue.Count > 0)
                {
                    QueueItem item = queue.Dequeue();
                    Activity activity = item.Activity;
                    TransactedReceiveScope parentTransactedReceiveScope = item.ParentTransactedReceiveScope;
                    TransactedReceiveScope rootTransactedReceiveScope = item.RootTransactedReceiveScope;
                    if (activity is Receive)
                    {
                        Receive receive = (Receive) activity;
                        if (rootTransactedReceiveScope != null)
                        {
                            receive.InternalReceive.AdditionalData.IsInsideTransactedReceiveScope = true;
                            if ((receive == parentTransactedReceiveScope.Request) && (parentTransactedReceiveScope == rootTransactedReceiveScope))
                            {
                                receive.InternalReceive.AdditionalData.IsFirstReceiveOfTransactedReceiveScopeTree = true;
                            }
                        }
                        this.knownServiceActivities.Add(receive);
                    }
                    else if (activity is SendReply)
                    {
                        SendReply reply = (SendReply) activity;
                        Receive request = reply.Request;
                        if (reply.InternalContent.IsFault)
                        {
                            request.FollowingFaults.Add(reply);
                        }
                        else
                        {
                            if (request.HasReply)
                            {
                                SendReply reply2 = request.FollowingReplies[0];
                                ContractValidationHelper.ValidateSendReplyWithSendReply(reply2, reply);
                            }
                            request.FollowingReplies.Add(reply);
                        }
                        ReceiveAndReplyTuple tuple = new ReceiveAndReplyTuple(request, reply);
                        this.receiveAndReplyPairs.Add(tuple);
                    }
                    if (activity is TransactedReceiveScope)
                    {
                        parentTransactedReceiveScope = activity as TransactedReceiveScope;
                        if (rootTransactedReceiveScope == null)
                        {
                            rootTransactedReceiveScope = parentTransactedReceiveScope;
                        }
                    }
                    foreach (Activity activity2 in WorkflowInspectionServices.GetActivities(activity))
                    {
                        QueueItem item2 = new QueueItem(activity2, parentTransactedReceiveScope, rootTransactedReceiveScope);
                        queue.Enqueue(item2);
                    }
                }
            }
        }

        [DefaultValue(false)]
        public bool AllowBufferedReceive { get; set; }

        [DefaultValue((string) null)]
        public Activity Body { get; set; }

        [DefaultValue((string) null)]
        public string ConfigurationName { get; set; }

        internal IDictionary<XName, Collection<CorrelationQuery>> CorrelationQueries
        {
            get
            {
                return this.correlationQueryByContract;
            }
        }

        public Collection<Endpoint> Endpoints
        {
            get
            {
                if (this.endpoints == null)
                {
                    this.endpoints = new Collection<Endpoint>();
                }
                return this.endpoints;
            }
        }

        internal XName InternalName
        {
            get
            {
                if (this.Name != null)
                {
                    return this.Name;
                }
                if (this.inferedServiceName == null)
                {
                    if (this.Body.DisplayName.Length == 0)
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.MissingDisplayNameInRootActivity));
                    }
                    this.inferedServiceName = XName.Get(XmlConvert.EncodeLocalName(this.Body.DisplayName));
                }
                return this.inferedServiceName;
            }
        }

        [DefaultValue((string) null), TypeConverter(typeof(ServiceXNameTypeConverter))]
        public XName Name { get; set; }

        [StructLayout(LayoutKind.Sequential)]
        private struct ContractAndOperationNameTuple
        {
            private XName ServiceContractXName;
            private string OperationName;
            public ContractAndOperationNameTuple(XName serviceContractXName, string operationName)
            {
                this.ServiceContractXName = serviceContractXName;
                this.OperationName = operationName;
            }
        }

        private class OperationInfo
        {
            private System.ServiceModel.Description.OperationDescription operationDescription;
            private System.ServiceModel.Activities.Receive receive;

            public OperationInfo(System.ServiceModel.Activities.Receive receive, System.ServiceModel.Description.OperationDescription operationDescription)
            {
                this.receive = receive;
                this.operationDescription = operationDescription;
            }

            public System.ServiceModel.Description.OperationDescription OperationDescription
            {
                get
                {
                    return this.operationDescription;
                }
            }

            public System.ServiceModel.Activities.Receive Receive
            {
                get
                {
                    return this.receive;
                }
                set
                {
                    this.receive = value;
                }
            }
        }

        private class QueueItem
        {
            private System.Activities.Activity activity;
            private TransactedReceiveScope parent;
            private TransactedReceiveScope rootTransactedReceiveScope;

            public QueueItem(System.Activities.Activity element, TransactedReceiveScope parent, TransactedReceiveScope rootTransactedReceiveScope)
            {
                this.activity = element;
                this.parent = parent;
                this.rootTransactedReceiveScope = rootTransactedReceiveScope;
            }

            public System.Activities.Activity Activity
            {
                get
                {
                    return this.activity;
                }
            }

            public TransactedReceiveScope ParentTransactedReceiveScope
            {
                get
                {
                    return this.parent;
                }
            }

            public TransactedReceiveScope RootTransactedReceiveScope
            {
                get
                {
                    return this.rootTransactedReceiveScope;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ReceiveAndReplyTuple
        {
            private System.ServiceModel.Activities.Receive Receive;
            private Activity Reply;
            public ReceiveAndReplyTuple(System.ServiceModel.Activities.Receive receive, Activity reply)
            {
                this.Receive = receive;
                this.Reply = reply;
            }
        }
    }
}

