namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    internal static class CorrelationService
    {
        private static void CreateFollowerEntry(IServiceProvider context, Type interfaceType, string followermethodName, string initializermethodName)
        {
            if (CorrelationResolver.IsInitializingMember(interfaceType, initializermethodName, null))
            {
                WorkflowQueuingService service = (WorkflowQueuingService) context.GetService(typeof(WorkflowQueuingService));
                FollowerQueueCreator eventListener = new FollowerQueueCreator(followermethodName);
                WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "Creating follower {0} on initializer {1}", new object[] { interfaceType.Name + followermethodName, interfaceType.Name + initializermethodName });
                ICollection<CorrelationProperty> propertyValues = CorrelationResolver.ResolveCorrelationValues(interfaceType, initializermethodName, null, true);
                EventQueueName queueName = new EventQueueName(interfaceType, initializermethodName, propertyValues);
                WorkflowQueue workflowQueue = null;
                if (service.Exists(queueName))
                {
                    workflowQueue = service.GetWorkflowQueue(queueName);
                }
                else
                {
                    workflowQueue = service.CreateWorkflowQueue(queueName, true);
                    workflowQueue.Enabled = false;
                }
                workflowQueue.RegisterForQueueItemArrived(eventListener);
            }
        }

        private static CorrelationToken GetCorrelationToken(Activity activity)
        {
            DependencyProperty dependencyProperty = DependencyProperty.FromName("CorrelationToken", activity.GetType());
            if (dependencyProperty == null)
            {
                dependencyProperty = DependencyProperty.FromName("CorrelationToken", activity.GetType().BaseType);
            }
            CorrelationToken token = activity.GetValue(dependencyProperty) as CorrelationToken;
            if (token == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_CorrelationTokenMissing", new object[] { activity.Name }));
            }
            CorrelationToken token2 = CorrelationTokenCollection.GetCorrelationToken(activity, token.Name, token.OwnerActivityName);
            if (token2 == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_CorrelationTokenMissing", new object[] { activity.Name }));
            }
            return token2;
        }

        internal static void Initialize(IServiceProvider context, Activity activity, Type interfaceType, string methodName, Guid instanceId)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            Subscribe(context, activity, interfaceType, methodName, null, instanceId);
            InitializeFollowers(context, interfaceType, methodName);
        }

        private static void InitializeFollowers(IServiceProvider context, Type interfaceType, string followermethodName)
        {
            if (!CorrelationResolver.IsInitializingMember(interfaceType, followermethodName, null))
            {
                foreach (EventInfo info in interfaceType.GetEvents())
                {
                    CreateFollowerEntry(context, interfaceType, followermethodName, info.Name);
                }
            }
        }

        internal static void InvalidateCorrelationToken(Activity activity, Type interfaceType, string methodName, object[] messageArgs)
        {
            if (!(CorrelationResolver.GetCorrelationProvider(interfaceType) is NonCorrelatedProvider))
            {
                CorrelationToken correlationToken = GetCorrelationToken(activity);
                ICollection<CorrelationProperty> followerProperties = CorrelationResolver.ResolveCorrelationValues(interfaceType, methodName, messageArgs, false);
                if (!CorrelationResolver.IsInitializingMember(interfaceType, methodName, messageArgs))
                {
                    if (!correlationToken.Initialized)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_CorrelationNotInitialized", new object[] { correlationToken.Name, activity.QualifiedName }));
                    }
                    ValidateCorrelation(correlationToken.Properties, followerProperties, correlationToken.Name, activity);
                }
                else
                {
                    correlationToken.Initialize(activity, followerProperties);
                }
            }
        }

        internal static IComparable ResolveQueueName(Activity activity, Type interfaceType, string methodName)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            if (CorrelationResolver.IsInitializingMember(interfaceType, methodName, null))
            {
                return new EventQueueName(interfaceType, methodName, CorrelationResolver.ResolveCorrelationValues(interfaceType, methodName, null, true));
            }
            CorrelationToken correlationToken = GetCorrelationToken(activity);
            if (!correlationToken.Initialized)
            {
                return null;
            }
            return new EventQueueName(interfaceType, methodName, correlationToken.Properties);
        }

        internal static bool Subscribe(IServiceProvider context, Activity activity, Type interfaceType, string methodName, IActivityEventListener<QueueEventArgs> eventListener, Guid instanceId)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            WorkflowQueuingService service = (WorkflowQueuingService) context.GetService(typeof(WorkflowQueuingService));
            IComparable queueName = ResolveQueueName(activity, interfaceType, methodName);
            if (queueName != null)
            {
                WorkflowQueue workflowQueue = null;
                if (service.Exists(queueName))
                {
                    workflowQueue = service.GetWorkflowQueue(queueName);
                    workflowQueue.Enabled = true;
                }
                else
                {
                    workflowQueue = service.CreateWorkflowQueue(queueName, true);
                }
                if (eventListener != null)
                {
                    workflowQueue.RegisterForQueueItemAvailable(eventListener, activity.QualifiedName);
                    WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "CorrelationService: activity '{0}' subscribing to QueueItemAvailable", new object[] { activity.QualifiedName });
                    return true;
                }
                return false;
            }
            SubscribeForCorrelationTokenInvalidation(activity, interfaceType, methodName, eventListener, instanceId);
            return false;
        }

        private static void SubscribeForCorrelationTokenInvalidation(Activity activity, Type interfaceType, string followermethodName, IActivityEventListener<QueueEventArgs> eventListener, Guid instanceId)
        {
            CorrelationToken correlationToken = GetCorrelationToken(activity);
            CorrelationTokenInvalidatedHandler dataChangeListener = new CorrelationTokenInvalidatedHandler(interfaceType, followermethodName, eventListener, instanceId);
            correlationToken.SubscribeForCorrelationTokenInitializedEvent(activity, dataChangeListener);
        }

        internal static void UninitializeFollowers(Type interfaceType, string initializer, WorkflowQueue initializerQueue)
        {
            if (CorrelationResolver.IsInitializingMember(interfaceType, initializer, null))
            {
                foreach (EventInfo info in interfaceType.GetEvents())
                {
                    string name = info.Name;
                    if (!CorrelationResolver.IsInitializingMember(interfaceType, info.Name, null))
                    {
                        initializerQueue.UnregisterForQueueItemArrived(new FollowerQueueCreator(name));
                    }
                }
            }
        }

        internal static bool Unsubscribe(IServiceProvider context, Activity activity, Type interfaceType, string methodName, IActivityEventListener<QueueEventArgs> eventListener)
        {
            if (activity == null)
            {
                throw new ArgumentException("activity");
            }
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            WorkflowQueuingService service = (WorkflowQueuingService) context.GetService(typeof(WorkflowQueuingService));
            IComparable queueName = ResolveQueueName(activity, interfaceType, methodName);
            if ((queueName != null) && service.Exists(queueName))
            {
                service.GetWorkflowQueue(queueName).UnregisterForQueueItemAvailable(eventListener);
                return true;
            }
            return false;
        }

        private static void ValidateCorrelation(ICollection<CorrelationProperty> initializerProperties, ICollection<CorrelationProperty> followerProperties, string memberName, Activity activity)
        {
            if ((followerProperties != null) || (initializerProperties != null))
            {
                if ((followerProperties == null) || (initializerProperties == null))
                {
                    throw new InvalidOperationException(SR.GetString("Error_CorrelationViolationException", new object[] { memberName, activity.QualifiedName }));
                }
                if (initializerProperties.Count != followerProperties.Count)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CorrelationViolationException", new object[] { memberName, activity.QualifiedName }));
                }
                IEnumerator<CorrelationProperty> enumerator = initializerProperties.GetEnumerator();
                IEnumerator<CorrelationProperty> enumerator2 = followerProperties.GetEnumerator();
                while (enumerator.MoveNext() && enumerator2.MoveNext())
                {
                    IComparable comparable = enumerator.Current.Value as IComparable;
                    object obj2 = enumerator2.Current.Value;
                    if ((comparable != null) && (comparable.CompareTo(obj2) != 0))
                    {
                        throw new InvalidOperationException(SR.GetString("Error_CorrelationViolationException", new object[] { memberName, activity.QualifiedName }));
                    }
                    if ((enumerator.Current.Value == null) && (obj2 == null))
                    {
                        return;
                    }
                    if (((comparable == null) && (obj2 != null)) && !obj2.Equals(enumerator.Current.Value))
                    {
                        throw new InvalidOperationException(SR.GetString("Error_CorrelationViolationException", new object[] { memberName, activity.QualifiedName }));
                    }
                }
            }
        }
    }
}

