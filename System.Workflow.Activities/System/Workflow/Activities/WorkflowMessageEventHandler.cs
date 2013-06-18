namespace System.Workflow.Activities
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Security.Principal;
    using System.Threading;
    using System.Workflow.Runtime;

    [Serializable]
    internal sealed class WorkflowMessageEventHandler
    {
        [NonSerialized]
        private IDeliverMessage enqueueWrapper;
        [NonSerialized]
        private Type eventHandlerType;
        private string eventName;
        private Type proxiedType;

        internal WorkflowMessageEventHandler(Type proxiedType, EventInfo eventInfo, IDeliverMessage enqueueWrapper)
        {
            this.proxiedType = proxiedType;
            this.eventName = eventInfo.Name;
            this.eventHandlerType = eventInfo.EventHandlerType;
            this.enqueueWrapper = enqueueWrapper;
        }

        public void EventHandler(object sender, ExternalDataEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                throw new ArgumentNullException("eventArgs");
            }
            try
            {
                object obj2;
                IPendingWork work;
                object[] objArray = this.enqueueWrapper.PrepareEventArgsArray(sender, eventArgs, out obj2, out work);
                EventQueueName key = this.GetKey(objArray);
                string name = null;
                if (eventArgs.Identity == null)
                {
                    IIdentity identity = Thread.CurrentPrincipal.Identity;
                    WindowsIdentity identity2 = identity as WindowsIdentity;
                    if ((identity2 != null) && (identity2.User != null))
                    {
                        name = identity2.User.Translate(typeof(NTAccount)).ToString();
                    }
                    else if (identity != null)
                    {
                        name = identity.Name;
                    }
                    eventArgs.Identity = name;
                }
                else
                {
                    name = eventArgs.Identity;
                }
                MethodMessage message = new MethodMessage(this.proxiedType, this.eventName, objArray, name);
                WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "Firing event {0} for instance {1}", new object[] { this.eventName, eventArgs.InstanceId });
                this.enqueueWrapper.DeliverMessage(eventArgs, key, message, obj2, work);
            }
            catch (Exception exception)
            {
                if (ExternalDataExchangeService.IsIrrecoverableException(exception))
                {
                    throw;
                }
                throw new EventDeliveryFailedException(SR.GetString("Error_EventDeliveryFailedException", new object[] { this.proxiedType, this.eventName, eventArgs.InstanceId }), exception);
            }
        }

        private EventQueueName GetKey(object[] eventArgs)
        {
            bool provideInitializerTokens = CorrelationResolver.IsInitializingMember(this.proxiedType, this.eventName, eventArgs);
            return new EventQueueName(this.proxiedType, this.eventName, CorrelationResolver.ResolveCorrelationValues(this.proxiedType, this.eventName, eventArgs, provideInitializerTokens));
        }

        internal System.Delegate Delegate
        {
            get
            {
                ParameterInfo[] parameters = this.eventHandlerType.GetMethod("Invoke").GetParameters();
                bool flag = false;
                if ((parameters.Length == 2) && (parameters[1].ParameterType.IsSubclassOf(typeof(ExternalDataEventArgs)) || (parameters[1].ParameterType == typeof(ExternalDataEventArgs))))
                {
                    flag = true;
                }
                if (flag)
                {
                    MethodInfo method = typeof(WorkflowMessageEventHandler).GetMethod("EventHandler");
                    return (System.Delegate) Activator.CreateInstance(this.eventHandlerType, new object[] { this, method.MethodHandle.GetFunctionPointer() });
                }
                return null;
            }
        }

        internal IDeliverMessage EnqueueWrapper
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enqueueWrapper;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.enqueueWrapper = value;
            }
        }
    }
}

