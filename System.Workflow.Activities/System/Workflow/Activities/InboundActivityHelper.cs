namespace System.Workflow.Activities
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    internal static class InboundActivityHelper
    {
        internal static object DequeueMessage(IComparable queueId, WorkflowQueuingService queueSvcs, Activity activity, out WorkflowQueue queue)
        {
            object obj2 = null;
            queue = queueSvcs.GetWorkflowQueue(queueId);
            if (queue.Count != 0)
            {
                obj2 = queue.Dequeue();
                if (obj2 == null)
                {
                    throw new ArgumentException(SR.GetString("Error_InvalidEventMessage", new object[] { activity.QualifiedName }));
                }
            }
            return obj2;
        }

        internal static ActivityExecutionStatus ExecuteForActivity(HandleExternalEventActivity activity, ActivityExecutionContext context, Type interfaceType, string operation, out object[] args)
        {
            WorkflowQueuingService queueSvcs = (WorkflowQueuingService) context.GetService(typeof(WorkflowQueuingService));
            args = null;
            IComparable queueId = CorrelationService.ResolveQueueName(activity, interfaceType, operation);
            if (queueId != null)
            {
                WorkflowQueue queue;
                object msg = DequeueMessage(queueId, queueSvcs, activity, out queue);
                CorrelationService.UninitializeFollowers(interfaceType, operation, queue);
                if (msg != null)
                {
                    args = ProcessEvent(activity, context, msg, interfaceType, operation);
                    return ActivityExecutionStatus.Closed;
                }
            }
            return ActivityExecutionStatus.Executing;
        }

        private static object[] ProcessEvent(HandleExternalEventActivity activity, ActivityExecutionContext context, object msg, Type interfaceType, string operation)
        {
            IMethodMessage message = msg as IMethodMessage;
            if (message == null)
            {
                Exception exception = msg as Exception;
                if (exception != null)
                {
                    throw exception;
                }
                throw new InvalidOperationException(SR.GetString("Error_InvalidLocalServiceMessage"));
            }
            CorrelationService.InvalidateCorrelationToken(activity, interfaceType, operation, message.Args);
            IdentityContextData data = (IdentityContextData) message.LogicalCallContext.GetData("__identitycontext__");
            ValidateRoles(activity, data.Identity);
            if (ProcessEventParameters(activity.ParameterBindings, message, interfaceType, operation))
            {
                return message.Args;
            }
            return null;
        }

        private static bool ProcessEventParameters(WorkflowParameterBindingCollection parameters, IMethodMessage message, Type interfaceType, string operation)
        {
            bool flag = false;
            if (parameters != null)
            {
                MethodInfo method = interfaceType.GetEvent(operation).EventHandlerType.GetMethod("Invoke");
                int index = 0;
                foreach (ParameterInfo info3 in method.GetParameters())
                {
                    if (typeof(ExternalDataEventArgs).IsAssignableFrom(info3.ParameterType) && (index == 1))
                    {
                        flag = true;
                    }
                    if (parameters.Contains(info3.Name))
                    {
                        WorkflowParameterBinding binding = parameters[info3.Name];
                        binding.Value = message.Args[index];
                    }
                    index++;
                }
            }
            return flag;
        }

        internal static void ValidateRoles(Activity activity, string identity)
        {
            DependencyProperty dependencyProperty = DependencyProperty.FromName("Roles", activity.GetType().BaseType);
            if (dependencyProperty == null)
            {
                dependencyProperty = DependencyProperty.FromName("Roles", activity.GetType());
            }
            if (dependencyProperty != null)
            {
                ActivityBind binding = activity.GetBinding(dependencyProperty);
                if (binding != null)
                {
                    WorkflowRoleCollection runtimeValue = binding.GetRuntimeValue(activity) as WorkflowRoleCollection;
                    if ((runtimeValue != null) && !runtimeValue.IncludesIdentity(identity))
                    {
                        throw new WorkflowAuthorizationException(activity.Name, identity);
                    }
                }
            }
        }
    }
}

