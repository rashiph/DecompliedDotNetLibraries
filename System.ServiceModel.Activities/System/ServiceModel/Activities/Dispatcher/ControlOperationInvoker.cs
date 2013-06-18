namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;
    using System.Xml.Linq;

    internal class ControlOperationInvoker : IManualConcurrencyOperationInvoker, IOperationInvoker
    {
        private readonly System.ServiceModel.Activities.Dispatcher.BufferedReceiveManager bufferedReceiveManager;
        protected static readonly object[] emptyObjectArray = new object[0];
        private readonly ServiceEndpoint endpoint;
        private readonly WorkflowServiceHost host;
        private readonly IOperationInvoker innerInvoker;
        private readonly int inputParameterCount;
        private readonly DurableInstanceManager instanceManager;
        private readonly bool isControlOperation;
        private readonly bool isOneWay;
        private readonly CorrelationKeyCalculator keyCalculator;
        private readonly string operationName;
        private readonly TimeSpan persistTimeout;

        public ControlOperationInvoker(OperationDescription description, ServiceEndpoint endpoint, CorrelationKeyCalculator correlationKeyCalculator, ServiceHostBase host) : this(description, endpoint, correlationKeyCalculator, null, host)
        {
        }

        public ControlOperationInvoker(OperationDescription description, ServiceEndpoint endpoint, CorrelationKeyCalculator correlationKeyCalculator, IOperationInvoker innerInvoker, ServiceHostBase host)
        {
            this.host = (WorkflowServiceHost) host;
            this.instanceManager = this.host.DurableInstanceManager;
            this.operationName = description.Name;
            this.isOneWay = description.IsOneWay;
            this.endpoint = endpoint;
            this.innerInvoker = innerInvoker;
            this.keyCalculator = correlationKeyCalculator;
            this.persistTimeout = this.host.PersistTimeout;
            if (description.DeclaringContract == WorkflowControlEndpoint.WorkflowControlServiceContract)
            {
                this.isControlOperation = true;
                switch (this.operationName)
                {
                    case "Cancel":
                    case "TransactedCancel":
                    case "Run":
                    case "TransactedRun":
                    case "Unsuspend":
                    case "TransactedUnsuspend":
                        this.inputParameterCount = 1;
                        return;

                    case "Abandon":
                    case "Suspend":
                    case "TransactedSuspend":
                    case "Terminate":
                    case "TransactedTerminate":
                        this.inputParameterCount = 2;
                        return;
                }
                throw Fx.AssertAndThrow("Unreachable code");
            }
            if (endpoint is WorkflowHostingEndpoint)
            {
                this.CanCreateInstance = true;
            }
            else
            {
                this.bufferedReceiveManager = this.host.Extensions.Find<System.ServiceModel.Activities.Dispatcher.BufferedReceiveManager>();
            }
        }

        public virtual object[] AllocateInputs()
        {
            if (this.innerInvoker != null)
            {
                return this.innerInvoker.AllocateInputs();
            }
            if (!this.isControlOperation)
            {
                throw Fx.AssertAndThrow("Derived invoker should have handled this case");
            }
            if (this.inputParameterCount == 0)
            {
                return emptyObjectArray;
            }
            return new object[this.inputParameterCount];
        }

        protected void GetInstanceKeys(OperationContext operationContext, out InstanceKey instanceKey, out ICollection<InstanceKey> additionalKeys)
        {
            CorrelationMessageProperty property = null;
            instanceKey = InstanceKey.InvalidKey;
            additionalKeys = new ReadOnlyCollection<InstanceKey>(new InstanceKey[0]);
            if (!CorrelationMessageProperty.TryGet(operationContext.IncomingMessageProperties, out property))
            {
                if (this.keyCalculator != null)
                {
                    InstanceKey key;
                    ICollection<InstanceKey> is2;
                    MessageBuffer buffer;
                    bool flag;
                    if (operationContext.IncomingMessageProperties.TryGetValue<MessageBuffer>("_RequestMessageBuffer_", out buffer))
                    {
                        flag = this.keyCalculator.CalculateKeys(buffer, operationContext.IncomingMessage, out key, out is2);
                    }
                    else
                    {
                        flag = this.keyCalculator.CalculateKeys(operationContext.IncomingMessage, out key, out is2);
                    }
                    if (flag)
                    {
                        if (key != null)
                        {
                            instanceKey = key;
                        }
                        if (is2 != null)
                        {
                            additionalKeys = is2;
                        }
                        property = new CorrelationMessageProperty(instanceKey, additionalKeys);
                        operationContext.IncomingMessageProperties.Add(CorrelationMessageProperty.Name, property);
                    }
                }
            }
            else
            {
                instanceKey = property.CorrelationKey;
                additionalKeys = property.AdditionalKeys;
            }
            if (((instanceKey == null) || !instanceKey.IsValid) && !this.CanCreateInstance)
            {
                this.host.RaiseUnknownMessageReceived(operationContext.IncomingMessage);
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new FaultException(new DurableDispatcherAddressingFault()));
            }
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            return this.Invoke(instance, inputs, null, out outputs);
        }

        public object Invoke(object instance, object[] inputs, IInvokeReceivedNotification notification, out object[] outputs)
        {
            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new NotImplementedException());
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            return this.InvokeBegin(instance, inputs, null, callback, state);
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, IInvokeReceivedNotification notification, AsyncCallback callback, object state)
        {
            if (inputs == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("inputs");
            }
            return new ControlOperationAsyncResult(this, inputs, notification, TimeSpan.MaxValue, callback, state);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            return ControlOperationAsyncResult.End(out outputs, result);
        }

        protected virtual IAsyncResult OnBeginServiceOperation(WorkflowServiceInstance durableInstance, OperationContext operationContext, object[] inputs, Transaction currentTransaction, IInvokeReceivedNotification notification, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ServiceOperationAsyncResult(this.innerInvoker, durableInstance, inputs, operationContext, currentTransaction, notification, callback, state);
        }

        protected virtual object OnEndServiceOperation(WorkflowServiceInstance durableInstance, out object[] outputs, IAsyncResult result)
        {
            return ServiceOperationAsyncResult.End(out outputs, result);
        }

        public System.ServiceModel.Activities.Dispatcher.BufferedReceiveManager BufferedReceiveManager
        {
            get
            {
                return this.bufferedReceiveManager;
            }
        }

        protected bool CanCreateInstance { get; set; }

        public DurableInstanceManager InstanceManager
        {
            get
            {
                return this.instanceManager;
            }
        }

        public bool IsSynchronous
        {
            get
            {
                return false;
            }
        }

        protected string OperationName
        {
            get
            {
                return this.operationName;
            }
        }

        protected string StaticBookmarkName { get; set; }

        bool IManualConcurrencyOperationInvoker.OwnsFormatter
        {
            get
            {
                return this.isOneWay;
            }
        }

        private class ControlOperationAsyncResult : AsyncResult
        {
            private ICollection<InstanceKey> additionalKeys;
            private static ReadOnlyCollection<InstanceKey> emptyKeyCollection = new ReadOnlyCollection<InstanceKey>(new InstanceKey[0]);
            private WorkflowGetInstanceContext getInstanceContext;
            private static AsyncResult.AsyncCompletion handleEndAbandonReceiveContext;
            private static AsyncResult.AsyncCompletion handleEndGetInstance = new AsyncResult.AsyncCompletion(ControlOperationInvoker.ControlOperationAsyncResult.HandleEndGetInstance);
            private static AsyncResult.AsyncCompletion handleEndOperation = new AsyncResult.AsyncCompletion(ControlOperationInvoker.ControlOperationAsyncResult.HandleEndOperation);
            private object[] inputs;
            private Guid instanceId;
            private InstanceKey instanceKey;
            private ControlOperationInvoker invoker;
            private IInvokeReceivedNotification notification;
            private static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(ControlOperationInvoker.ControlOperationAsyncResult.Finally);
            private OperationContext operationContext;
            private Exception operationException;
            private object[] outputs;
            private ReceiveContext receiveContext;
            private object returnValue;
            private TimeoutHelper timeoutHelper;
            private Transaction transaction;
            private WorkflowServiceInstance workflowServiceInstance;

            public ControlOperationAsyncResult(ControlOperationInvoker invoker, object[] inputs, IInvokeReceivedNotification notification, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instanceKey = InstanceKey.InvalidKey;
                this.additionalKeys = emptyKeyCollection;
                this.outputs = ControlOperationInvoker.emptyObjectArray;
                this.invoker = invoker;
                this.inputs = inputs;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.transaction = Transaction.Current;
                this.operationContext = OperationContext.Current;
                base.OnCompleting = onCompleting;
                bool flag = false;
                bool flag2 = false;
                try
                {
                    if (notification != null)
                    {
                        if (this.operationContext.SessionId == null)
                        {
                            notification.NotifyInvokeReceived();
                        }
                        else
                        {
                            this.notification = notification;
                        }
                    }
                    if (invoker.BufferedReceiveManager != null)
                    {
                        ReceiveContext.TryGet(this.operationContext.IncomingMessageProperties, out this.receiveContext);
                    }
                    flag = this.Process();
                    flag2 = true;
                }
                finally
                {
                    if (!flag2)
                    {
                        Finally(this, null);
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private bool AbandonReceiveContext(Exception operationException)
            {
                if (handleEndAbandonReceiveContext == null)
                {
                    handleEndAbandonReceiveContext = new AsyncResult.AsyncCompletion(ControlOperationInvoker.ControlOperationAsyncResult.HandleEndAbandonReceiveContext);
                }
                this.operationException = operationException;
                IAsyncResult result = this.receiveContext.BeginAbandon(TimeSpan.MaxValue, base.PrepareAsyncCompletion(handleEndAbandonReceiveContext), this);
                return base.SyncContinue(result);
            }

            private IAsyncResult BeginRunAndGetResponse(TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return RunAndGetResponseAsyncResult.Create(this, timeoutHelper, callback, state);
            }

            private void BufferReceiveHelper(ref bool shouldAbandon, bool retry)
            {
                if ((this.invoker.BufferedReceiveManager != null) && this.invoker.BufferedReceiveManager.BufferReceive(this.operationContext, this.receiveContext, this.invoker.StaticBookmarkName, BufferedReceiveState.WaitingOnInstance, retry))
                {
                    if (System.ServiceModel.Activities.TD.BufferOutOfOrderMessageNoInstanceIsEnabled())
                    {
                        System.ServiceModel.Activities.TD.BufferOutOfOrderMessageNoInstance(this.invoker.StaticBookmarkName);
                    }
                    shouldAbandon = false;
                }
            }

            private static FaultException CreateFaultException(InstancePersistenceException exception)
            {
                return new FaultException(OperationExecutionFault.CreateInstanceNotFoundFault(exception.Message));
            }

            public static object End(out object[] outputs, IAsyncResult result)
            {
                ControlOperationInvoker.ControlOperationAsyncResult result2 = AsyncResult.End<ControlOperationInvoker.ControlOperationAsyncResult>(result);
                outputs = result2.outputs;
                return result2.returnValue;
            }

            private object EndRunAndGetResponse(IAsyncResult result, out object[] outputs)
            {
                return RunAndGetResponseAsyncResult.End(result, out outputs);
            }

            private void EnsureInstanceId()
            {
                if (this.invoker.isControlOperation)
                {
                    switch (this.invoker.operationName)
                    {
                        case "Abandon":
                        case "Cancel":
                        case "TransactedCancel":
                        case "Run":
                        case "TransactedRun":
                        case "Suspend":
                        case "TransactedSuspend":
                        case "Terminate":
                        case "TransactedTerminate":
                        case "Unsuspend":
                        case "TransactedUnsuspend":
                            this.instanceId = this.GetInstanceIdForControlOperation(this.inputs);
                            return;
                    }
                    throw Fx.AssertAndThrow("Unreachable code");
                }
                if (this.invoker.endpoint is WorkflowHostingEndpoint)
                {
                    this.instanceId = ((WorkflowHostingEndpoint) this.invoker.endpoint).OnGetInstanceId(this.inputs, this.operationContext);
                    if (this.instanceId == Guid.Empty)
                    {
                        this.invoker.GetInstanceKeys(this.operationContext, out this.instanceKey, out this.additionalKeys);
                    }
                }
                else
                {
                    this.invoker.GetInstanceKeys(this.operationContext, out this.instanceKey, out this.additionalKeys);
                }
            }

            private static void Finally(AsyncResult result, Exception completionException)
            {
                ControlOperationInvoker.ControlOperationAsyncResult result2 = (ControlOperationInvoker.ControlOperationAsyncResult) result;
                if (result2.workflowServiceInstance != null)
                {
                    result2.workflowServiceInstance.ReleaseReference();
                    result2.workflowServiceInstance = null;
                }
                if (completionException != null)
                {
                    result2.invoker.host.FaultServiceHostIfNecessary(completionException);
                }
            }

            private Guid GetInstanceIdForControlOperation(object[] args)
            {
                object obj2 = args[0];
                if ((obj2 == null) || !(obj2 is Guid))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.FailedToGetInstanceIdForControlOperation));
                }
                return (Guid) obj2;
            }

            private static bool HandleEndAbandonReceiveContext(IAsyncResult result)
            {
                ControlOperationInvoker.ControlOperationAsyncResult asyncState = (ControlOperationInvoker.ControlOperationAsyncResult) result.AsyncState;
                asyncState.receiveContext.EndAbandon(result);
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(asyncState.operationException);
            }

            private static bool HandleEndGetInstance(IAsyncResult result)
            {
                ControlOperationInvoker.ControlOperationAsyncResult asyncState = (ControlOperationInvoker.ControlOperationAsyncResult) result.AsyncState;
                bool shouldAbandon = true;
                try
                {
                    try
                    {
                        asyncState.workflowServiceInstance = asyncState.invoker.instanceManager.EndGetInstance(result);
                        shouldAbandon = false;
                    }
                    catch (InstanceLockedException exception)
                    {
                        RedirectionException exception2;
                        if (asyncState.TryCreateRedirectionException(exception, out exception2))
                        {
                            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(exception2);
                        }
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(CreateFaultException(exception));
                    }
                    catch (OperationCanceledException exception3)
                    {
                        asyncState.BufferReceiveHelper(ref shouldAbandon, true);
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new RetryException(null, exception3));
                    }
                    catch (InstancePersistenceException exception4)
                    {
                        asyncState.BufferReceiveHelper(ref shouldAbandon, false);
                        if (exception4 is InstanceKeyNotReadyException)
                        {
                            asyncState.invoker.host.RaiseUnknownMessageReceived(asyncState.operationContext.IncomingMessage);
                        }
                        asyncState.invoker.host.FaultServiceHostIfNecessary(exception4);
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(CreateFaultException(exception4));
                    }
                }
                catch (Exception exception5)
                {
                    if (Fx.IsFatal(exception5))
                    {
                        throw;
                    }
                    if (!shouldAbandon || !asyncState.ShouldAbandonReceiveContext())
                    {
                        throw;
                    }
                    return asyncState.AbandonReceiveContext(exception5);
                }
                if (!asyncState.instanceKey.IsValid && (asyncState.instanceId == Guid.Empty))
                {
                    ContextMessageProperty contextMessageProperty = null;
                    if (!ContextMessageProperty.TryGet(asyncState.operationContext.OutgoingMessageProperties, out contextMessageProperty))
                    {
                        contextMessageProperty = new ContextMessageProperty();
                        contextMessageProperty.Context.Add("instanceId", Guid.NewGuid().ToString());
                        contextMessageProperty.AddOrReplaceInMessageProperties(asyncState.operationContext.OutgoingMessageProperties);
                    }
                    else
                    {
                        contextMessageProperty.Context["instanceId"] = Guid.NewGuid().ToString();
                    }
                }
                return asyncState.PerformOperation();
            }

            private static bool HandleEndOperation(IAsyncResult result)
            {
                ControlOperationInvoker.ControlOperationAsyncResult asyncState = (ControlOperationInvoker.ControlOperationAsyncResult) result.AsyncState;
                if (asyncState.invoker.isControlOperation)
                {
                    switch (asyncState.invoker.operationName)
                    {
                        case "Suspend":
                        case "TransactedSuspend":
                            asyncState.workflowServiceInstance.EndSuspend(result);
                            goto Label_01E6;

                        case "Unsuspend":
                        case "TransactedUnsuspend":
                            asyncState.workflowServiceInstance.EndUnsuspend(result);
                            goto Label_01E6;

                        case "Terminate":
                        case "TransactedTerminate":
                            asyncState.workflowServiceInstance.EndTerminate(result);
                            goto Label_01E6;

                        case "Run":
                        case "TransactedRun":
                            asyncState.workflowServiceInstance.EndRun(result);
                            goto Label_01E6;

                        case "Cancel":
                        case "TransactedCancel":
                            asyncState.workflowServiceInstance.EndCancel(result);
                            goto Label_01E6;

                        case "Abandon":
                            asyncState.workflowServiceInstance.EndAbandon(result);
                            goto Label_01E6;
                    }
                    throw Fx.AssertAndThrow("Unreachable code");
                }
                if (asyncState.getInstanceContext.WorkflowCreationContext != null)
                {
                    asyncState.returnValue = asyncState.EndRunAndGetResponse(result, out asyncState.outputs);
                }
                else
                {
                    try
                    {
                        asyncState.returnValue = asyncState.invoker.OnEndServiceOperation(asyncState.workflowServiceInstance, out asyncState.outputs, result);
                    }
                    catch (FaultException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (!asyncState.ShouldAbandonReceiveContext())
                        {
                            throw;
                        }
                        return asyncState.AbandonReceiveContext(exception);
                    }
                }
            Label_01E6:
                return true;
            }

            private bool PerformOperation()
            {
                IAsyncResult result = null;
                bool flag = false;
                if (!this.invoker.isControlOperation)
                {
                    if (this.getInstanceContext.WorkflowCreationContext != null)
                    {
                        result = this.BeginRunAndGetResponse(this.timeoutHelper, base.PrepareAsyncCompletion(handleEndOperation), this);
                        if (this.notification != null)
                        {
                            this.notification.NotifyInvokeReceived();
                        }
                    }
                    else
                    {
                        try
                        {
                            result = this.invoker.OnBeginServiceOperation(this.workflowServiceInstance, this.operationContext, this.inputs, this.transaction, this.notification, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndOperation), this);
                        }
                        catch (FaultException)
                        {
                            throw;
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            if (!this.ShouldAbandonReceiveContext())
                            {
                                throw;
                            }
                            return this.AbandonReceiveContext(exception);
                        }
                    }
                }
                else
                {
                    switch (this.invoker.operationName)
                    {
                        case "Suspend":
                        case "TransactedSuspend":
                            result = this.workflowServiceInstance.BeginSuspend(false, ((string) this.inputs[1]) ?? System.ServiceModel.Activities.SR.DefaultSuspendReason, this.transaction, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndOperation), this);
                            break;

                        case "Unsuspend":
                        case "TransactedUnsuspend":
                            result = this.workflowServiceInstance.BeginUnsuspend(this.transaction, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndOperation), this);
                            break;

                        case "Terminate":
                        case "TransactedTerminate":
                            result = this.workflowServiceInstance.BeginTerminate(((string) this.inputs[1]) ?? System.ServiceModel.Activities.SR.DefaultTerminationReason, this.transaction, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndOperation), this);
                            break;

                        case "Run":
                        case "TransactedRun":
                            result = this.workflowServiceInstance.BeginRun(this.transaction, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndOperation), this);
                            break;

                        case "Cancel":
                        case "TransactedCancel":
                            result = this.workflowServiceInstance.BeginCancel(this.transaction, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndOperation), this);
                            break;

                        case "Abandon":
                        {
                            string str = (string) this.inputs[1];
                            result = this.workflowServiceInstance.BeginAbandon(new WorkflowApplicationAbortedException(!string.IsNullOrEmpty(str) ? str : System.ServiceModel.Activities.SR.DefaultAbortReason), this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndOperation), this);
                            break;
                        }
                        default:
                            throw Fx.AssertAndThrow("Unreachable code");
                    }
                    if (this.notification != null)
                    {
                        this.notification.NotifyInvokeReceived();
                    }
                }
                if ((result != null) && result.CompletedSynchronously)
                {
                    flag = HandleEndOperation(result);
                }
                return flag;
            }

            private bool Process()
            {
                IAsyncResult result;
                this.EnsureInstanceId();
                WorkflowGetInstanceContext context = new WorkflowGetInstanceContext {
                    WorkflowHostingEndpoint = this.invoker.endpoint as WorkflowHostingEndpoint,
                    CanCreateInstance = this.invoker.CanCreateInstance,
                    Inputs = this.inputs,
                    OperationContext = this.operationContext
                };
                this.getInstanceContext = context;
                bool shouldAbandon = true;
                try
                {
                    try
                    {
                        if ((!this.invoker.isControlOperation && (this.instanceKey != null)) && this.instanceKey.IsValid)
                        {
                            result = this.invoker.instanceManager.BeginGetInstance(this.instanceKey, this.additionalKeys, this.getInstanceContext, this.invoker.persistTimeout, base.PrepareAsyncCompletion(handleEndGetInstance), this);
                        }
                        else
                        {
                            result = this.invoker.instanceManager.BeginGetInstance(this.instanceId, this.getInstanceContext, this.invoker.persistTimeout, base.PrepareAsyncCompletion(handleEndGetInstance), this);
                        }
                        shouldAbandon = false;
                    }
                    catch (InstanceLockedException exception)
                    {
                        RedirectionException exception2;
                        if (this.TryCreateRedirectionException(exception, out exception2))
                        {
                            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(exception2);
                        }
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(CreateFaultException(exception));
                    }
                    catch (OperationCanceledException exception3)
                    {
                        this.BufferReceiveHelper(ref shouldAbandon, true);
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new RetryException(null, exception3));
                    }
                    catch (InstancePersistenceException exception4)
                    {
                        this.BufferReceiveHelper(ref shouldAbandon, false);
                        if (exception4 is InstanceKeyNotReadyException)
                        {
                            this.invoker.host.RaiseUnknownMessageReceived(this.operationContext.IncomingMessage);
                        }
                        this.invoker.host.FaultServiceHostIfNecessary(exception4);
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(CreateFaultException(exception4));
                    }
                }
                catch (Exception exception5)
                {
                    if (Fx.IsFatal(exception5))
                    {
                        throw;
                    }
                    if (!shouldAbandon || !this.ShouldAbandonReceiveContext())
                    {
                        throw;
                    }
                    return this.AbandonReceiveContext(exception5);
                }
                return (result.CompletedSynchronously && HandleEndGetInstance(result));
            }

            private bool ShouldAbandonReceiveContext()
            {
                return ((this.receiveContext != null) && (this.receiveContext.State != ReceiveContextState.Faulted));
            }

            private bool TryCreateRedirectionException(InstanceLockedException exception, out RedirectionException redirectionException)
            {
                Uri address = null;
                object obj2;
                string localName = (this.invoker.endpoint != null) ? this.invoker.endpoint.Name : null;
                XName key = (localName == null) ? null : WorkflowServiceNamespace.EndpointsPath.GetName(localName);
                if (((key != null) && (exception.SerializableInstanceOwnerMetadata != null)) && exception.SerializableInstanceOwnerMetadata.TryGetValue(key, out obj2))
                {
                    address = obj2 as Uri;
                }
                if (address == null)
                {
                    redirectionException = null;
                    return false;
                }
                redirectionException = new RedirectionException(RedirectionType.Resource, RedirectionDuration.Permanent, RedirectionScope.Session, new RedirectionLocation[] { new RedirectionLocation(address) });
                return true;
            }

            private class RunAndGetResponseAsyncResult : AsyncResult
            {
                private ControlOperationInvoker.ControlOperationAsyncResult control;
                private static AsyncResult.AsyncCompletion handleEndGetResponse = new AsyncResult.AsyncCompletion(ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult.HandleEndGetResponse);
                private static AsyncResult.AsyncCompletion handleEndRun = new AsyncResult.AsyncCompletion(ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult.HandleEndRun);
                private static AsyncResult.AsyncCompletion handleEndSuspend = new AsyncResult.AsyncCompletion(ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult.HandleEndSuspend);
                private object[] outputs;
                private object returnValue;
                private TimeoutHelper timeoutHelper;

                private RunAndGetResponseAsyncResult(ControlOperationInvoker.ControlOperationAsyncResult control, TimeoutHelper timeoutHelper, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.control = control;
                    this.timeoutHelper = timeoutHelper;
                    bool flag = true;
                    if (control.getInstanceContext.WorkflowCreationContext.CreateOnly)
                    {
                        flag = this.Suspend();
                    }
                    else
                    {
                        flag = this.Run();
                    }
                    if (flag)
                    {
                        base.Complete(true);
                    }
                }

                public static ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult Create(ControlOperationInvoker.ControlOperationAsyncResult control, TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
                {
                    return new ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult(control, timeoutHelper, callback, state);
                }

                public static object End(IAsyncResult result, out object[] outputs)
                {
                    ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult result2 = AsyncResult.End<ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult>(result);
                    outputs = result2.outputs;
                    return result2.returnValue;
                }

                private bool GetResponse()
                {
                    IAsyncResult result = this.control.getInstanceContext.WorkflowHostingResponseContext.BeginGetResponse(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndGetResponse), this);
                    return base.SyncContinue(result);
                }

                private static bool HandleEndGetResponse(IAsyncResult result)
                {
                    ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult asyncState = (ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult) result.AsyncState;
                    asyncState.returnValue = asyncState.control.getInstanceContext.WorkflowHostingResponseContext.EndGetResponse(result, out asyncState.outputs);
                    return true;
                }

                private static bool HandleEndRun(IAsyncResult result)
                {
                    ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult asyncState = (ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult) result.AsyncState;
                    asyncState.control.workflowServiceInstance.EndRun(result);
                    return asyncState.GetResponse();
                }

                private static bool HandleEndSuspend(IAsyncResult result)
                {
                    ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult asyncState = (ControlOperationInvoker.ControlOperationAsyncResult.RunAndGetResponseAsyncResult) result.AsyncState;
                    asyncState.control.workflowServiceInstance.EndSuspend(result);
                    return asyncState.GetResponse();
                }

                private bool Run()
                {
                    IAsyncResult result = this.control.workflowServiceInstance.BeginRun(this.control.transaction, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndRun), this);
                    return base.SyncContinue(result);
                }

                private bool Suspend()
                {
                    IAsyncResult result = this.control.workflowServiceInstance.BeginSuspend(false, System.ServiceModel.Activities.SR.DefaultCreateOnlyReason, this.control.transaction, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndSuspend), this);
                    return base.SyncContinue(result);
                }
            }
        }

        private class ServiceOperationAsyncResult : AsyncResult
        {
            private Transaction currentTransaction;
            private WorkflowServiceInstance durableInstance;
            private static AsyncResult.AsyncCompletion handleEndInvoke = new AsyncResult.AsyncCompletion(ControlOperationInvoker.ServiceOperationAsyncResult.HandleEndInvoke);
            private IOperationInvoker innerInvoker;
            private object[] inputs;
            private IInvokeReceivedNotification notification;
            private OperationContext operationContext;
            private object[] outputs;
            private object returnValue;

            public ServiceOperationAsyncResult(IOperationInvoker innerInvoker, WorkflowServiceInstance durableInstance, object[] inputs, OperationContext operationContext, Transaction currentTransaction, IInvokeReceivedNotification notification, AsyncCallback callback, object state) : base(callback, state)
            {
                this.durableInstance = durableInstance;
                this.operationContext = operationContext;
                this.inputs = inputs;
                this.innerInvoker = innerInvoker;
                this.currentTransaction = currentTransaction;
                this.notification = notification;
                if (innerInvoker == null)
                {
                    throw Fx.AssertAndThrow("Cannot reach this path without innerInvoker");
                }
                if (this.innerInvoker.IsSynchronous)
                {
                    TransactionScope scope = Fx.CreateTransactionScope(this.currentTransaction);
                    try
                    {
                        using (new OperationContextScopeHelper(this.operationContext))
                        {
                            IManualConcurrencyOperationInvoker invoker = this.innerInvoker as IManualConcurrencyOperationInvoker;
                            if (invoker != null)
                            {
                                this.returnValue = invoker.Invoke(this.durableInstance, this.inputs, this.notification, out this.outputs);
                            }
                            else
                            {
                                this.returnValue = this.innerInvoker.Invoke(this.durableInstance, this.inputs, out this.outputs);
                            }
                        }
                    }
                    finally
                    {
                        Fx.CompleteTransactionScope(ref scope);
                    }
                    base.Complete(true);
                }
                else
                {
                    IAsyncResult result;
                    using (base.PrepareTransactionalCall(this.currentTransaction))
                    {
                        using (new OperationContextScopeHelper(this.operationContext))
                        {
                            IManualConcurrencyOperationInvoker invoker2 = this.innerInvoker as IManualConcurrencyOperationInvoker;
                            if (invoker2 != null)
                            {
                                result = invoker2.InvokeBegin(this.durableInstance, this.inputs, this.notification, base.PrepareAsyncCompletion(handleEndInvoke), this);
                            }
                            else
                            {
                                result = this.innerInvoker.InvokeBegin(this.durableInstance, this.inputs, base.PrepareAsyncCompletion(handleEndInvoke), this);
                            }
                        }
                    }
                    if (base.SyncContinue(result))
                    {
                        base.Complete(true);
                    }
                }
            }

            public static object End(out object[] outputs, IAsyncResult result)
            {
                ControlOperationInvoker.ServiceOperationAsyncResult result2 = AsyncResult.End<ControlOperationInvoker.ServiceOperationAsyncResult>(result);
                outputs = result2.outputs;
                return result2.returnValue;
            }

            private static bool HandleEndInvoke(IAsyncResult result)
            {
                bool flag;
                ControlOperationInvoker.ServiceOperationAsyncResult asyncState = (ControlOperationInvoker.ServiceOperationAsyncResult) result.AsyncState;
                TransactionScope scope = Fx.CreateTransactionScope(asyncState.currentTransaction);
                try
                {
                    using (new OperationContextScopeHelper(asyncState.operationContext))
                    {
                        asyncState.returnValue = asyncState.innerInvoker.InvokeEnd(asyncState.durableInstance, out asyncState.outputs, result);
                        flag = true;
                    }
                }
                finally
                {
                    Fx.CompleteTransactionScope(ref scope);
                }
                return flag;
            }

            private class OperationContextScopeHelper : IDisposable
            {
                private OperationContext current = OperationContext.Current;

                public OperationContextScopeHelper(OperationContext newContext)
                {
                    OperationContext.Current = newContext;
                }

                void IDisposable.Dispose()
                {
                    OperationContext.Current = this.current;
                }
            }
        }
    }
}

