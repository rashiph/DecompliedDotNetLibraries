namespace System.ServiceModel.Activities
{
    using Microsoft.VisualBasic.Activities;
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.Xaml;

    internal static class MessagingActivityHelper
    {
        public const string ActivityInstanceId = "ActivityInstanceId";
        public const string ActivityName = "ActivityName";
        public const string ActivityNameWorkflowOperationInvoke = "WorkflowOperationInvoke";
        public const string ActivityType = "ActivityType";
        public const string ActivityTypeExecuteUserCode = "ExecuteUserCode";
        public const string E2EActivityId = "E2EActivityId";
        private static System.Type faultExceptionGenericType = typeof(FaultException<>);
        private static System.Type faultExceptionType = typeof(FaultException);
        public const string MessageCorrelationReceiveRecord = "MessageCorrelationReceiveRecord";
        public const string MessageCorrelationSendRecord = "MessageCorrelationSendRecord";
        public const string MessageId = "MessageId";
        public const string MessagingActivityTypeActivityExecution = "MessagingActivityExecution";

        public static void AddRuntimeArgument(Argument messageArgument, string runtimeArgumentName, System.Type runtimeArgumentType, ArgumentDirection runtimeArgumentDirection, ActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument(runtimeArgumentName, runtimeArgumentType, runtimeArgumentDirection);
            metadata.Bind(messageArgument, argument);
            metadata.AddArgument(argument);
        }

        public static bool CompareContextEquality(IDictionary<string, string> context1, IDictionary<string, string> context2)
        {
            if (context1 != context2)
            {
                if (((context1 == null) || (context2 == null)) || (context1.Count != context2.Count))
                {
                    return false;
                }
                foreach (KeyValuePair<string, string> pair in context1)
                {
                    if (!context2.Contains(pair))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static CorrelationCallbackContext CreateCorrelationCallbackContext(MessageProperties messageProperties)
        {
            CallbackContextMessageProperty property;
            if (CallbackContextMessageProperty.TryGet(messageProperties, out property))
            {
                EndpointAddress address;
                IDictionary<string, string> dictionary;
                property.GetListenAddressAndContext(out address, out dictionary);
                return new CorrelationCallbackContext { ListenAddress = EndpointAddress10.FromEndpointAddress(address), Context = dictionary };
            }
            return null;
        }

        public static CorrelationContext CreateCorrelationContext(MessageProperties messageProperties)
        {
            ContextMessageProperty property;
            if (ContextMessageProperty.TryGet(messageProperties, out property))
            {
                IDictionary<string, string> context = property.Context;
                return new CorrelationContext { Context = context };
            }
            return null;
        }

        public static InArgument<CorrelationHandle> CreateReplyCorrelatesWith(InArgument<CorrelationHandle> requestCorrelatesWith)
        {
            VariableValue<CorrelationHandle> expression = requestCorrelatesWith.Expression as VariableValue<CorrelationHandle>;
            if (expression != null)
            {
                return new InArgument<CorrelationHandle>(expression.Variable);
            }
            VisualBasicValue<CorrelationHandle> value3 = requestCorrelatesWith.Expression as VisualBasicValue<CorrelationHandle>;
            if (value3 != null)
            {
                return new InArgument<CorrelationHandle>(new VisualBasicValue<CorrelationHandle>(value3.ExpressionText));
            }
            return new InArgument<CorrelationHandle>(XamlServices.Parse(XamlServices.Save(requestCorrelatesWith.Expression)) as Activity<CorrelationHandle>);
        }

        public static void FixMessageArgument(Argument messageArgument, ArgumentDirection direction, ActivityMetadata metadata)
        {
            System.Type runtimeArgumentType = (messageArgument == null) ? TypeHelper.ObjectType : messageArgument.ArgumentType;
            AddRuntimeArgument(messageArgument, "Message", runtimeArgumentType, direction, metadata);
        }

        public static IList<T> GetCallbacks<T>(ExecutionProperties executionProperties) where T: class
        {
            List<T> list = null;
            if (!executionProperties.IsEmpty)
            {
                foreach (KeyValuePair<string, object> pair in executionProperties)
                {
                    T item = pair.Value as T;
                    if (item != null)
                    {
                        if (list == null)
                        {
                            list = new List<T>();
                        }
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public static void InitializeCorrelationHandles(NativeActivityContext context, CorrelationHandle selectHandle, CorrelationHandle ambientHandle, Collection<CorrelationInitializer> additionalCorrelations, MessageProperties messageProperties)
        {
            CorrelationMessageProperty property;
            if (CorrelationMessageProperty.TryGet(messageProperties, out property))
            {
                InitializeCorrelationHandles(context, selectHandle, ambientHandle, additionalCorrelations, property.CorrelationKey, property.AdditionalKeys);
            }
        }

        private static void InitializeCorrelationHandles(NativeActivityContext context, CorrelationHandle selectHandle, CorrelationHandle ambientHandle, Collection<CorrelationInitializer> additionalCorrelations, InstanceKey instanceKey, ICollection<InstanceKey> additionalKeys)
        {
            bool flag = false;
            if ((instanceKey != null) && instanceKey.IsValid)
            {
                if (selectHandle != null)
                {
                    selectHandle.InitializeBookmarkScope(context, instanceKey);
                }
                else if (ambientHandle != null)
                {
                    ambientHandle.InitializeBookmarkScope(context, instanceKey);
                    flag = true;
                }
                else if (context.DefaultBookmarkScope.IsInitialized)
                {
                    if (context.DefaultBookmarkScope.Id != instanceKey.Value)
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.CorrelationHandleInUse(context.DefaultBookmarkScope.Id, instanceKey.Value)));
                    }
                }
                else
                {
                    context.DefaultBookmarkScope.Initialize(context, instanceKey.Value);
                }
            }
            if ((additionalKeys != null) && (additionalCorrelations != null))
            {
                IEnumerator<CorrelationInitializer> enumerator = additionalCorrelations.GetEnumerator();
                foreach (InstanceKey key in additionalKeys)
                {
                    while (enumerator.MoveNext())
                    {
                        QueryCorrelationInitializer current = enumerator.Current as QueryCorrelationInitializer;
                        if (current != null)
                        {
                            CorrelationHandle handle = (current.CorrelationHandle != null) ? current.CorrelationHandle.Get(context) : null;
                            if (handle == null)
                            {
                                if ((ambientHandle == null) || flag)
                                {
                                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.QueryCorrelationInitializerCannotBeInitialized));
                                }
                                handle = ambientHandle;
                                flag = true;
                            }
                            handle.InitializeBookmarkScope(context, key);
                            break;
                        }
                    }
                }
            }
        }

        public static Message InitializeCorrelationHandles(NativeActivityContext context, CorrelationHandle selectHandle, CorrelationHandle ambientHandle, Collection<CorrelationInitializer> additionalCorrelations, CorrelationKeyCalculator keyCalculator, Message message)
        {
            InstanceKey key;
            ICollection<InstanceKey> is2;
            MessageBuffer buffer = message.CreateBufferedCopy(0x7fffffff);
            if (keyCalculator.CalculateKeys(buffer, message, out key, out is2))
            {
                InitializeCorrelationHandles(context, selectHandle, ambientHandle, additionalCorrelations, key, is2);
            }
            return buffer.CreateMessage();
        }

        public static void ValidateCorrelationInitializer(ActivityMetadata metadata, Collection<CorrelationInitializer> correlationInitializers, bool isReply, string displayName, string operationName)
        {
            if ((correlationInitializers != null) && (correlationInitializers.Count > 0))
            {
                bool flag = false;
                foreach (CorrelationInitializer initializer in correlationInitializers)
                {
                    if ((initializer is RequestReplyCorrelationInitializer) && isReply)
                    {
                        metadata.AddValidationError(System.ServiceModel.Activities.SR.ReplyShouldNotIncludeRequestReplyHandle(displayName, operationName));
                    }
                    QueryCorrelationInitializer initializer2 = initializer as QueryCorrelationInitializer;
                    if ((initializer2 != null) && (initializer2.MessageQuerySet.Count == 0))
                    {
                        metadata.AddValidationError(System.ServiceModel.Activities.SR.QueryCorrelationInitializerWithEmptyMessageQuerySet(displayName, operationName));
                    }
                    if (initializer.CorrelationHandle == null)
                    {
                        if (initializer is QueryCorrelationInitializer)
                        {
                            if (!flag)
                            {
                                flag = true;
                            }
                            else
                            {
                                metadata.AddValidationError(System.ServiceModel.Activities.SR.NullCorrelationHandleInMultipleQueryCorrelation);
                            }
                        }
                        else
                        {
                            metadata.AddValidationError(System.ServiceModel.Activities.SR.NullCorrelationHandleInInitializeCorrelation(initializer.GetType().Name));
                        }
                    }
                }
            }
        }
    }
}

