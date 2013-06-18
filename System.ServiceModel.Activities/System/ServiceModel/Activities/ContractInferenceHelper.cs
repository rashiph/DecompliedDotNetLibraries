namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Xml.Linq;

    internal static class ContractInferenceHelper
    {
        private static System.ServiceModel.DataContractFormatAttribute dataContractFormatAttribute;
        private static System.Type exceptionType;
        private static System.Type faultExceptionType;
        private static System.ServiceModel.XmlSerializerFormatAttribute xmlSerializerFormatAttribute;

        private static void AddDataContractSerializerFormat(OperationDescription operation)
        {
            if (operation.Behaviors.Find<DataContractSerializerOperationBehavior>() != null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.OperationHasSerializerBehavior(operation.Name, operation.DeclaringContract.Name, typeof(DataContractSerializerOperationBehavior))));
            }
            operation.Behaviors.Add(new DataContractSerializerOperationBehavior(operation, DataContractFormatAttribute));
            if (!operation.Behaviors.Contains(typeof(DataContractSerializerOperationGenerator)))
            {
                operation.Behaviors.Add(new DataContractSerializerOperationGenerator());
            }
        }

        public static void AddFaultDescription(Receive activity, OperationDescription operation)
        {
            if (activity.HasFault)
            {
                foreach (SendReply reply in activity.FollowingFaults)
                {
                    string overridingAction = null;
                    System.Type internalDeclaredMessageType = null;
                    overridingAction = reply.Action;
                    SendMessageContent internalContent = reply.InternalContent as SendMessageContent;
                    if (internalContent != null)
                    {
                        internalDeclaredMessageType = internalContent.InternalDeclaredMessageType;
                    }
                    else
                    {
                        SendParametersContent content2 = reply.InternalContent as SendParametersContent;
                        if (content2 != null)
                        {
                            internalDeclaredMessageType = content2.ArgumentTypes[0];
                        }
                    }
                    if (internalDeclaredMessageType.IsGenericType && (internalDeclaredMessageType.GetGenericTypeDefinition() == FaultExceptionType))
                    {
                        System.Type faultType = internalDeclaredMessageType.GetGenericArguments()[0];
                        bool flag = false;
                        foreach (FaultDescription description in operation.Faults)
                        {
                            if (description.DetailType == faultType)
                            {
                                if (description.Action != overridingAction)
                                {
                                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.SendRepliesHaveSameFaultTypeDifferentAction));
                                }
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            FaultDescription item = MessageBuilder.CreateFaultDescription(operation, faultType, overridingAction);
                            operation.Faults.Add(item);
                        }
                    }
                }
            }
        }

        public static void AddInputMessage(OperationDescription operation, string overridingAction, System.Type type, SerializerOption serializerOption)
        {
            bool isResponse = false;
            MessageDescription item = MessageBuilder.CreateMessageDescription(operation, isResponse, MessageDirection.Input, overridingAction, type, serializerOption);
            operation.Messages.Add(item);
        }

        public static void AddInputMessage(OperationDescription operation, string overridingAction, string[] argumentNames, System.Type[] argumentTypes)
        {
            bool isResponse = false;
            MessageDescription item = MessageBuilder.CreateMessageDescription(operation, isResponse, MessageDirection.Input, overridingAction, argumentNames, argumentTypes);
            operation.Messages.Add(item);
        }

        public static void AddKnownTypesToOperation(Receive receive, OperationDescription operation)
        {
            Collection<System.Type> internalKnownTypes = receive.InternalKnownTypes;
            if (internalKnownTypes != null)
            {
                foreach (System.Type type in internalKnownTypes)
                {
                    if (!operation.KnownTypes.Contains(type))
                    {
                        operation.KnownTypes.Add(type);
                    }
                }
            }
        }

        private static void AddKnownTypesToOperation(OperationDescription operation, Collection<System.Type> knownTypes)
        {
            if (knownTypes != null)
            {
                foreach (System.Type type in knownTypes)
                {
                    operation.KnownTypes.Add(type);
                }
            }
        }

        public static void AddOutputMessage(OperationDescription operation, string overridingAction, System.Type type, SerializerOption serializerOption)
        {
            bool isResponse = true;
            MessageDescription item = MessageBuilder.CreateMessageDescription(operation, isResponse, MessageDirection.Output, overridingAction, type, serializerOption);
            operation.Messages.Add(item);
        }

        public static void AddOutputMessage(OperationDescription operation, string overridingAction, string[] argumentNames, System.Type[] argumentTypes)
        {
            bool isResponse = true;
            MessageDescription item = MessageBuilder.CreateMessageDescription(operation, isResponse, MessageDirection.Output, overridingAction, argumentNames, argumentTypes);
            operation.Messages.Add(item);
        }

        public static void AddReceiveToFormatterBehavior(Receive receive, OperationDescription operation)
        {
            KeyedByTypeCollection<IOperationBehavior> behaviors = operation.Behaviors;
            WorkflowFormatterBehavior item = behaviors.Find<WorkflowFormatterBehavior>();
            if (item == null)
            {
                item = new WorkflowFormatterBehavior();
                behaviors.Add(item);
            }
            item.Receives.Add(receive);
        }

        private static void AddSerializerProvider(OperationDescription operation, SerializerOption serializerOption)
        {
            switch (serializerOption)
            {
                case SerializerOption.DataContractSerializer:
                    AddDataContractSerializerFormat(operation);
                    return;

                case SerializerOption.XmlSerializer:
                    AddXmlSerializerFormat(operation);
                    return;
            }
        }

        private static void AddWorkflowOperationBehaviors(OperationDescription operation, string bookmarkName, bool canCreateInstance)
        {
            KeyedByTypeCollection<IOperationBehavior> behaviors = operation.Behaviors;
            WorkflowOperationBehavior behavior = behaviors.Find<WorkflowOperationBehavior>();
            if (behavior == null)
            {
                behaviors.Add(new WorkflowOperationBehavior(new Bookmark(bookmarkName), canCreateInstance));
            }
            else
            {
                behavior.CanCreateInstance = behavior.CanCreateInstance || canCreateInstance;
            }
        }

        private static void AddXmlSerializerFormat(OperationDescription operation)
        {
            if (operation.Behaviors.Find<XmlSerializerOperationBehavior>() != null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.OperationHasSerializerBehavior(operation.Name, operation.DeclaringContract.Name, typeof(XmlSerializerOperationBehavior))));
            }
            operation.Behaviors.Add(new XmlSerializerOperationBehavior(operation, XmlSerializerFormatAttribute));
            if (!operation.Behaviors.Contains(typeof(XmlSerializerOperationGenerator)))
            {
                operation.Behaviors.Add(new XmlSerializerOperationGenerator(new XmlSerializerImportOptions()));
            }
        }

        public static void CheckForDisposableParameters(OperationDescription operation, System.Type type)
        {
            if (type == null)
            {
                operation.HasNoDisposableParameters = true;
            }
            else
            {
                operation.HasNoDisposableParameters = !ServiceReflector.IsParameterDisposable(type);
            }
        }

        public static void CheckForDisposableParameters(OperationDescription operation, System.Type[] types)
        {
            operation.HasNoDisposableParameters = true;
            foreach (System.Type type in types)
            {
                if (ServiceReflector.IsParameterDisposable(type))
                {
                    operation.HasNoDisposableParameters = false;
                    return;
                }
            }
        }

        public static void CorrectOutMessageForOperation(Receive receive, OperationDescription operation)
        {
            operation.Messages.RemoveAt(1);
            SendReply owner = receive.FollowingReplies[0];
            owner.InternalContent.InferMessageDescription(operation, owner, MessageDirection.Output);
            PostProcessOperation(operation);
        }

        public static Collection<CorrelationQuery> CreateClientCorrelationQueries(MessageQuerySet select, Collection<CorrelationInitializer> correlationInitializers, string overridingAction, XName serviceContractName, string operationName, bool isResponse)
        {
            Collection<CorrelationQuery> collection = new Collection<CorrelationQuery>();
            CorrelationQuery item = CreateCorrelationQueryCore(select, correlationInitializers);
            if (item != null)
            {
                if (overridingAction != null)
                {
                    CorrelationActionMessageFilter filter = new CorrelationActionMessageFilter {
                        Action = overridingAction
                    };
                    item.Where = filter;
                }
                else
                {
                    ProvideDefaultNamespace(ref serviceContractName);
                    string str = NamingHelper.GetMessageAction(new XmlQualifiedName(serviceContractName.LocalName, serviceContractName.NamespaceName), operationName, null, isResponse);
                    CorrelationActionMessageFilter filter2 = new CorrelationActionMessageFilter {
                        Action = str
                    };
                    item.Where = filter2;
                }
                collection.Add(item);
                if (isResponse)
                {
                    CorrelationQuery query2 = item.Clone();
                    CorrelationActionMessageFilter filter3 = new CorrelationActionMessageFilter {
                        Action = string.Empty
                    };
                    query2.Where = filter3;
                    collection.Add(query2);
                }
            }
            return collection;
        }

        public static ContractDescription CreateContractFromOperation(XName serviceContractName, OperationDescription operation)
        {
            ProvideDefaultNamespace(ref serviceContractName);
            ContractDescription description = new ContractDescription(serviceContractName.LocalName, serviceContractName.NamespaceName) {
                ConfigurationName = serviceContractName.LocalName,
                SessionMode = SessionMode.Allowed
            };
            description.Operations.Add(operation);
            return description;
        }

        private static CorrelationQuery CreateCorrelationQueryCore(MessageQuerySet select, Collection<CorrelationInitializer> correlationInitializers)
        {
            CorrelationQuery query = null;
            if (select != null)
            {
                query = new CorrelationQuery {
                    Select = select
                };
            }
            if ((correlationInitializers != null) && (correlationInitializers.Count > 0))
            {
                foreach (CorrelationInitializer initializer in correlationInitializers)
                {
                    QueryCorrelationInitializer initializer2 = initializer as QueryCorrelationInitializer;
                    if (initializer2 != null)
                    {
                        query = query ?? new CorrelationQuery();
                        query.SelectAdditional.Add(initializer2.MessageQuerySet);
                    }
                }
            }
            return query;
        }

        public static OperationDescription CreateOneWayOperationDescription(Send send)
        {
            return CreateOperationDescriptionCore(send, null);
        }

        public static OperationDescription CreateOperationDescription(Receive receive, ContractDescription contract)
        {
            OperationDescription operation = new OperationDescription(NamingHelper.XmlName(receive.OperationName), contract);
            if (receive.ProtectionLevel.HasValue)
            {
                operation.ProtectionLevel = receive.ProtectionLevel.Value;
            }
            receive.InternalContent.InferMessageDescription(operation, receive, MessageDirection.Input);
            if (receive.HasReply)
            {
                SendReply owner = receive.FollowingReplies[0];
                owner.InternalContent.InferMessageDescription(operation, owner, MessageDirection.Output);
            }
            else if (receive.HasFault)
            {
                CheckForDisposableParameters(operation, System.ServiceModel.Activities.Constants.EmptyTypeArray);
                AddOutputMessage(operation, null, System.ServiceModel.Activities.Constants.EmptyStringArray, System.ServiceModel.Activities.Constants.EmptyTypeArray);
            }
            PostProcessOperation(operation);
            AddSerializerProvider(operation, receive.SerializerOption);
            AddWorkflowOperationBehaviors(operation, receive.OperationBookmarkName, receive.CanCreateInstance);
            if (receive.InternalReceive.AdditionalData.IsInsideTransactedReceiveScope)
            {
                operation.IsInsideTransactedReceiveScope = true;
                EnableTransactionBehavior(operation);
                if (receive.InternalReceive.AdditionalData.IsFirstReceiveOfTransactedReceiveScopeTree)
                {
                    operation.IsFirstReceiveOfTransactedReceiveScopeTree = true;
                }
            }
            return operation;
        }

        private static OperationDescription CreateOperationDescriptionCore(Send send, ReceiveReply receiveReply)
        {
            XName serviceContractName = send.ServiceContractName;
            ProvideDefaultNamespace(ref serviceContractName);
            ContractDescription declaringContract = new ContractDescription(serviceContractName.LocalName, serviceContractName.NamespaceName) {
                ConfigurationName = send.EndpointConfigurationName
            };
            OperationDescription operation = new OperationDescription(NamingHelper.XmlName(send.OperationName), declaringContract);
            if (send.ProtectionLevel.HasValue)
            {
                operation.ProtectionLevel = send.ProtectionLevel.Value;
            }
            AddKnownTypesToOperation(operation, send.KnownTypes);
            send.InternalContent.InferMessageDescription(operation, send, MessageDirection.Input);
            if (receiveReply != null)
            {
                receiveReply.InternalContent.InferMessageDescription(operation, receiveReply, MessageDirection.Output);
            }
            PostProcessOperation(operation);
            AddSerializerProvider(operation, send.SerializerOption);
            declaringContract.Operations.Add(operation);
            return operation;
        }

        public static ContractDescription CreateOutputChannelContractDescription(XName serviceContractName, ProtectionLevel? protectionLevel)
        {
            System.Type type = typeof(IOutputChannel);
            ProvideDefaultNamespace(ref serviceContractName);
            ContractDescription declaringContract = new ContractDescription(serviceContractName.LocalName, serviceContractName.NamespaceName) {
                ContractType = type,
                ConfigurationName = serviceContractName.LocalName,
                SessionMode = SessionMode.Allowed
            };
            OperationDescription item = new OperationDescription("Send", declaringContract);
            MessageDescription description3 = new MessageDescription("*", MessageDirection.Input);
            item.Messages.Add(description3);
            if (protectionLevel.HasValue)
            {
                item.ProtectionLevel = protectionLevel.Value;
            }
            declaringContract.Operations.Add(item);
            return declaringContract;
        }

        public static ContractDescription CreateRequestChannelContractDescription(XName serviceContractName, ProtectionLevel? protectionLevel)
        {
            System.Type type = typeof(IRequestChannel);
            ProvideDefaultNamespace(ref serviceContractName);
            ContractDescription declaringContract = new ContractDescription(serviceContractName.LocalName, serviceContractName.NamespaceName) {
                ContractType = type,
                ConfigurationName = serviceContractName.LocalName,
                SessionMode = SessionMode.Allowed
            };
            OperationDescription item = new OperationDescription("Request", declaringContract);
            MessageDescription description3 = new MessageDescription("*", MessageDirection.Input);
            MessageDescription description4 = new MessageDescription("*", MessageDirection.Output);
            item.Messages.Add(description3);
            item.Messages.Add(description4);
            if (protectionLevel.HasValue)
            {
                item.ProtectionLevel = protectionLevel.Value;
            }
            declaringContract.Operations.Add(item);
            return declaringContract;
        }

        public static CorrelationQuery CreateServerCorrelationQuery(MessageQuerySet select, Collection<CorrelationInitializer> correlationInitializers, OperationDescription operation, bool isResponse)
        {
            CorrelationQuery query = CreateCorrelationQueryCore(select, correlationInitializers);
            if (query != null)
            {
                string str = !isResponse ? operation.Messages[0].Action : operation.Messages[1].Action;
                CorrelationActionMessageFilter filter = new CorrelationActionMessageFilter {
                    Action = str
                };
                query.Where = filter;
            }
            return query;
        }

        public static OperationDescription CreateTwoWayOperationDescription(Send send, ReceiveReply receiveReply)
        {
            return CreateOperationDescriptionCore(send, receiveReply);
        }

        private static void EnableTransactionBehavior(OperationDescription operationDescription)
        {
            OperationBehaviorAttribute attribute = operationDescription.Behaviors.Find<OperationBehaviorAttribute>();
            if (attribute != null)
            {
                attribute.TransactionScopeRequired = true;
                attribute.TransactionAutoComplete = false;
            }
            else
            {
                OperationBehaviorAttribute item = new OperationBehaviorAttribute {
                    TransactionAutoComplete = false,
                    TransactionScopeRequired = true
                };
                operationDescription.Behaviors.Add(item);
            }
            TransactionFlowAttribute attribute4 = operationDescription.Behaviors.Find<TransactionFlowAttribute>();
            if (attribute4 != null)
            {
                if (attribute4.Transactions != TransactionFlowOption.Allowed)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.ContractInferenceValidationForTransactionFlowBehavior));
                }
            }
            else if (!operationDescription.IsOneWay)
            {
                operationDescription.Behaviors.Add(new TransactionFlowAttribute(TransactionFlowOption.Allowed));
            }
        }

        public static void EnsureTransactionFlowOnContract(ref ServiceEndpoint serviceEndpoint, XName serviceContractName, string operationName, string action, ProtectionLevel? protectionLevel)
        {
            if (serviceEndpoint.Contract.ContractType == null)
            {
                serviceEndpoint.Contract.Operations[0].Behaviors.Add(new TransactionFlowAttribute(TransactionFlowOption.Allowed));
            }
            else
            {
                ContractDescription declaringContract = null;
                OperationDescription operation = null;
                MessageDescription item = null;
                MessageDescription description4 = null;
                System.Type type = typeof(IRequestChannel);
                ProvideDefaultNamespace(ref serviceContractName);
                declaringContract = new ContractDescription(serviceContractName.LocalName, serviceContractName.NamespaceName) {
                    ContractType = type,
                    SessionMode = SessionMode.Allowed
                };
                operation = new OperationDescription(operationName, declaringContract) {
                    Behaviors = { new TransactionFlowAttribute(TransactionFlowOption.Allowed) }
                };
                string messageAction = null;
                string str2 = null;
                if (string.IsNullOrEmpty(action))
                {
                    messageAction = NamingHelper.GetMessageAction(operation, false);
                    str2 = NamingHelper.GetMessageAction(operation, true);
                }
                else
                {
                    messageAction = action;
                    str2 = action + "Response";
                }
                item = new MessageDescription(messageAction, MessageDirection.Input);
                description4 = new MessageDescription(str2, MessageDirection.Output);
                operation.Messages.Add(item);
                operation.Messages.Add(description4);
                if (protectionLevel.HasValue)
                {
                    operation.ProtectionLevel = protectionLevel.Value;
                }
                declaringContract.Operations.Add(operation);
                Uri listenUri = serviceEndpoint.ListenUri;
                ServiceEndpoint endpoint = new ServiceEndpoint(declaringContract) {
                    Binding = serviceEndpoint.Binding,
                    Address = serviceEndpoint.Address,
                    Name = serviceEndpoint.Name
                };
                serviceEndpoint = endpoint;
                if (listenUri != null)
                {
                    serviceEndpoint.ListenUri = listenUri;
                }
            }
        }

        private static void PostProcessOperation(OperationDescription operation)
        {
            MessageBuilder.ClearWrapperNames(operation);
        }

        public static void ProvideDefaultNamespace(ref XName serviceContractName)
        {
            if (string.IsNullOrEmpty(serviceContractName.NamespaceName))
            {
                serviceContractName = XName.Get(serviceContractName.LocalName, "http://tempuri.org/");
            }
        }

        public static void UpdateIsOneWayFlag(Receive receive, OperationDescription operation)
        {
            if (!operation.IsOneWay)
            {
                receive.SetIsOneWay(false);
            }
        }

        public static System.ServiceModel.DataContractFormatAttribute DataContractFormatAttribute
        {
            get
            {
                if (dataContractFormatAttribute == null)
                {
                    dataContractFormatAttribute = new System.ServiceModel.DataContractFormatAttribute();
                }
                return dataContractFormatAttribute;
            }
        }

        public static System.Type ExceptionType
        {
            get
            {
                if (exceptionType == null)
                {
                    exceptionType = typeof(Exception);
                }
                return exceptionType;
            }
        }

        public static System.Type FaultExceptionType
        {
            get
            {
                if (faultExceptionType == null)
                {
                    faultExceptionType = typeof(FaultException<>);
                }
                return faultExceptionType;
            }
        }

        public static System.ServiceModel.XmlSerializerFormatAttribute XmlSerializerFormatAttribute
        {
            get
            {
                if (xmlSerializerFormatAttribute == null)
                {
                    System.ServiceModel.XmlSerializerFormatAttribute attribute = new System.ServiceModel.XmlSerializerFormatAttribute {
                        SupportFaults = true
                    };
                    xmlSerializerFormatAttribute = attribute;
                }
                return xmlSerializerFormatAttribute;
            }
        }
    }
}

