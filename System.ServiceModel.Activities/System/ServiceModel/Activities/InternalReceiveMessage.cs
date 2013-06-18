namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Tracking;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Transactions;
    using System.Xml.Linq;

    internal sealed class InternalReceiveMessage : NativeActivity
    {
        private ServiceDescriptionData additionalData;
        private Collection<CorrelationInitializer> correlationInitializers;
        private CompletionCallback onClientReceiveMessageComplete;
        private BookmarkCallback onMessageBookmarkCallback;
        private string operationBookmarkName;
        private Variable<VolatileReceiveMessageInstance> receiveMessageInstance;
        private static string runtimeTransactionHandlePropertyName = typeof(RuntimeTransactionHandle).FullName;
        private WaitForReply waitForReply;

        public InternalReceiveMessage()
        {
            this.CorrelatesWith = new InArgument<CorrelationHandle>(context => null);
            this.receiveMessageInstance = new Variable<VolatileReceiveMessageInstance>();
            WaitForReply reply = new WaitForReply {
                Instance = this.receiveMessageInstance
            };
            this.waitForReply = reply;
            this.onClientReceiveMessageComplete = new CompletionCallback(this.ClientScheduleOnReceiveMessageCallback);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("CorrelatesWith", System.ServiceModel.Activities.Constants.CorrelationHandleType, ArgumentDirection.In);
            metadata.Bind(this.CorrelatesWith, argument);
            metadata.AddArgument(argument);
            if (this.correlationInitializers != null)
            {
                int num = 0;
                foreach (CorrelationInitializer initializer in this.correlationInitializers)
                {
                    if (initializer.CorrelationHandle != null)
                    {
                        RuntimeArgument argument2 = new RuntimeArgument("Parameter" + num, initializer.CorrelationHandle.ArgumentType, initializer.CorrelationHandle.Direction, true);
                        metadata.Bind(initializer.CorrelationHandle, argument2);
                        metadata.AddArgument(argument2);
                        num++;
                    }
                }
            }
            RuntimeArgument argument3 = new RuntimeArgument("Message", System.ServiceModel.Activities.Constants.MessageType, ArgumentDirection.Out);
            metadata.Bind(this.Message, argument3);
            metadata.AddArgument(argument3);
            RuntimeArgument argument4 = new RuntimeArgument("noPersistHandle", System.ServiceModel.Activities.Constants.NoPersistHandleType, ArgumentDirection.In);
            metadata.Bind(this.NoPersistHandle, argument4);
            metadata.AddArgument(argument4);
            metadata.AddImplementationVariable(this.receiveMessageInstance);
            metadata.AddImplementationChild(this.waitForReply);
        }

        private void ClientScheduleOnReceivedMessage(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            if (instance.CorrelationRequestContext.TryGetReply())
            {
                this.ClientScheduleOnReceiveMessageCore(executionContext, instance);
                this.FinalizeScheduleOnReceivedMessage(executionContext, instance);
            }
            else
            {
                VolatileReceiveMessageInstance instance2 = new VolatileReceiveMessageInstance {
                    Instance = instance
                };
                this.receiveMessageInstance.Set(executionContext, instance2);
                if (this.onClientReceiveMessageComplete == null)
                {
                    this.onClientReceiveMessageComplete = new CompletionCallback(this.ClientScheduleOnReceiveMessageCallback);
                }
                executionContext.ScheduleActivity(this.waitForReply, this.onClientReceiveMessageComplete);
            }
        }

        private void ClientScheduleOnReceiveMessageCallback(NativeActivityContext executionContext, System.Activities.ActivityInstance completedInstance)
        {
            ReceiveMessageInstanceData instance = this.receiveMessageInstance.Get(executionContext).Instance;
            if (instance.CorrelationRequestContext.TryGetReply())
            {
                this.ClientScheduleOnReceiveMessageCore(executionContext, instance);
            }
            this.FinalizeScheduleOnReceivedMessage(executionContext, instance);
        }

        private void ClientScheduleOnReceiveMessageCore(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            instance.InitializeContextAndCallbackContext();
            CorrelationHandle ambientCorrelation = instance.GetAmbientCorrelation(executionContext);
            if (instance.CorrelationRequestContext.CorrelationKeyCalculator != null)
            {
                instance.CorrelationRequestContext.Reply = MessagingActivityHelper.InitializeCorrelationHandles(executionContext, null, ambientCorrelation, this.correlationInitializers, instance.CorrelationRequestContext.CorrelationKeyCalculator, instance.CorrelationRequestContext.Reply);
            }
            if (instance.CorrelationContext != null)
            {
                CorrelationHandle explicitContextCorrelation = CorrelationHandle.GetExplicitContextCorrelation(executionContext, this.correlationInitializers);
                if (explicitContextCorrelation == null)
                {
                    explicitContextCorrelation = ambientCorrelation;
                }
                if (explicitContextCorrelation != null)
                {
                    explicitContextCorrelation.Context = instance.CorrelationContext;
                }
            }
            System.ServiceModel.Channels.Message reply = instance.CorrelationRequestContext.Reply;
            this.Message.Set(executionContext, reply);
        }

        protected override void Execute(NativeActivityContext executionContext)
        {
            CorrelationRequestContext context;
            CorrelationHandle handle = (this.CorrelatesWith == null) ? null : this.CorrelatesWith.Get(executionContext);
            bool flag = false;
            CorrelationHandle ambientCorrelation = null;
            if (handle == null)
            {
                ambientCorrelation = executionContext.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                flag = true;
                if (ambientCorrelation != null)
                {
                    handle = ambientCorrelation;
                }
            }
            if ((handle != null) && handle.TryAcquireRequestContext(executionContext, out context))
            {
                ReceiveMessageInstanceData instance = new ReceiveMessageInstanceData(context);
                if (flag)
                {
                    instance.SetAmbientCorrelation(ambientCorrelation);
                }
                this.ClientScheduleOnReceivedMessage(executionContext, instance);
            }
            else
            {
                if (ambientCorrelation == null)
                {
                    ambientCorrelation = executionContext.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                }
                if ((!this.IsOneWay && (ambientCorrelation == null)) && (CorrelationHandle.GetExplicitChannelCorrelation(executionContext, this.correlationInitializers) == null))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.ReceiveMessageNeedsToPairWithSendMessageForTwoWayContract(this.OperationName)));
                }
                BookmarkScope scope = (handle != null) ? handle.EnsureBookmarkScope(executionContext) : executionContext.DefaultBookmarkScope;
                if (this.onMessageBookmarkCallback == null)
                {
                    this.onMessageBookmarkCallback = new BookmarkCallback(this.OnMessage);
                }
                executionContext.CreateBookmark(this.OperationBookmarkName, this.onMessageBookmarkCallback, scope);
            }
        }

        private void FinalizeReceiveMessageCore(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            if (instance != null)
            {
                if (((instance.CorrelationRequestContext == null) || (instance.CorrelationRequestContext.Reply == null)) && ((instance.CorrelationResponseContext != null) && this.IsOneWay))
                {
                    instance.CorrelationResponseContext.WorkflowOperationContext.SetOperationCompleted();
                    if (instance.CorrelationResponseContext.Exception != null)
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(instance.CorrelationResponseContext.Exception);
                    }
                }
                if (TraceUtility.ActivityTracing)
                {
                    if (System.ServiceModel.Activities.TD.StopSignpostEventIsEnabled())
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                        dictionary.Add("ActivityName", base.DisplayName);
                        dictionary.Add("ActivityType", "MessagingActivityExecution");
                        dictionary.Add("ActivityInstanceId", executionContext.ActivityInstanceId);
                        System.ServiceModel.Activities.TD.StopSignpostEvent(new DictionaryTraceRecord(dictionary));
                    }
                    System.ServiceModel.Activities.FxTrace.Trace.SetAndTraceTransfer(instance.AmbientActivityId, true);
                    instance.AmbientActivityId = Guid.Empty;
                }
            }
        }

        private void FinalizeScheduleOnReceivedMessage(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            this.ProcessReceiveMessageTrace(executionContext, instance);
            IList<IReceiveMessageCallback> callbacks = MessagingActivityHelper.GetCallbacks<IReceiveMessageCallback>(executionContext.Properties);
            if ((callbacks != null) && (callbacks.Count > 0))
            {
                OperationContext operationContext = instance.GetOperationContext();
                foreach (IReceiveMessageCallback callback in callbacks)
                {
                    callback.OnReceiveMessage(operationContext, executionContext.Properties);
                }
            }
            this.FinalizeReceiveMessageCore(executionContext, instance);
        }

        private void OnMessage(NativeActivityContext executionContext, Bookmark bookmark, object state)
        {
            WorkflowOperationContext context = state as WorkflowOperationContext;
            if (context == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.WorkflowMustBeHosted));
            }
            CorrelationResponseContext responseContext = new CorrelationResponseContext {
                WorkflowOperationContext = context
            };
            ReceiveMessageInstanceData instance = new ReceiveMessageInstanceData(responseContext);
            this.SetupTransaction(executionContext, instance);
        }

        private void ProcessReceiveMessageTrace(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            if (TraceUtility.MessageFlowTracing)
            {
                try
                {
                    if (TraceUtility.ActivityTracing)
                    {
                        instance.AmbientActivityId = Trace.CorrelationManager.ActivityId;
                    }
                    Guid empty = Guid.Empty;
                    if (instance.CorrelationRequestContext != null)
                    {
                        empty = TraceUtility.GetReceivedActivityId(instance.CorrelationRequestContext.OperationContext);
                    }
                    else if (instance.CorrelationResponseContext != null)
                    {
                        empty = instance.CorrelationResponseContext.WorkflowOperationContext.E2EActivityId;
                    }
                    ReceiveMessageRecord record = new ReceiveMessageRecord("MessageCorrelationReceiveRecord") {
                        E2EActivityId = empty
                    };
                    executionContext.Track(record);
                    if ((empty != Guid.Empty) && (DiagnosticTrace.ActivityId != empty))
                    {
                        DiagnosticTrace.ActivityId = empty;
                    }
                    System.ServiceModel.Activities.FxTrace.Trace.SetAndTraceTransfer(executionContext.WorkflowInstanceId, true);
                    if (TraceUtility.ActivityTracing && System.ServiceModel.Activities.TD.StartSignpostEventIsEnabled())
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                        dictionary.Add("ActivityName", base.DisplayName);
                        dictionary.Add("ActivityType", "MessagingActivityExecution");
                        dictionary.Add("ActivityInstanceId", executionContext.ActivityInstanceId);
                        System.ServiceModel.Activities.TD.StartSignpostEvent(new DictionaryTraceRecord(dictionary));
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    System.ServiceModel.Activities.FxTrace.Exception.AsInformation(exception);
                }
            }
        }

        private void RequireContextCallback(NativeActivityTransactionContext transactionContext, object state)
        {
            ReceiveMessageState state2 = state as ReceiveMessageState;
            transactionContext.SetRuntimeTransaction(state2.CurrentTransaction);
            NativeActivityContext executionContext = transactionContext;
            this.ServerScheduleOnReceivedMessage(executionContext, state2.Instance);
        }

        private void ServerScheduleOnReceivedMessage(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            System.ServiceModel.Channels.Message message = instance.CorrelationResponseContext.WorkflowOperationContext.Inputs[0] as System.ServiceModel.Channels.Message;
            this.Message.Set(executionContext, message);
            instance.CorrelationResponseContext.MessageVersion = ((System.ServiceModel.Channels.Message) instance.CorrelationResponseContext.WorkflowOperationContext.Inputs[0]).Version;
            CorrelationHandle ambientCorrelation = instance.GetAmbientCorrelation(executionContext);
            CorrelationHandle selectHandle = (this.CorrelatesWith == null) ? null : this.CorrelatesWith.Get(executionContext);
            MessagingActivityHelper.InitializeCorrelationHandles(executionContext, selectHandle, ambientCorrelation, this.correlationInitializers, instance.CorrelationResponseContext.WorkflowOperationContext.OperationContext.IncomingMessageProperties);
            CorrelationHandle explicitChannelCorrelation = CorrelationHandle.GetExplicitChannelCorrelation(executionContext, this.correlationInitializers);
            if (this.IsOneWay)
            {
                if (explicitChannelCorrelation != null)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.RequestReplyHandleShouldNotBePresentForOneWay));
                }
                if (this.NoPersistHandle != null)
                {
                    System.Activities.NoPersistHandle handle4 = this.NoPersistHandle.Get(executionContext);
                    if (handle4 != null)
                    {
                        handle4.Enter(executionContext);
                    }
                }
            }
            else if (explicitChannelCorrelation != null)
            {
                if (!explicitChannelCorrelation.TryRegisterResponseContext(executionContext, instance.CorrelationResponseContext))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.TryRegisterRequestContextFailed));
                }
            }
            else if (!ambientCorrelation.TryRegisterResponseContext(executionContext, instance.CorrelationResponseContext))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.ReceiveMessageNeedsToPairWithSendMessageForTwoWayContract(this.OperationName)));
            }
            if (instance.CorrelationCallbackContext != null)
            {
                CorrelationHandle explicitCallbackCorrelation = CorrelationHandle.GetExplicitCallbackCorrelation(executionContext, this.correlationInitializers);
                if (explicitCallbackCorrelation == null)
                {
                    explicitCallbackCorrelation = ambientCorrelation;
                }
                if (explicitCallbackCorrelation != null)
                {
                    explicitCallbackCorrelation.CallbackContext = instance.CorrelationCallbackContext;
                }
            }
            this.FinalizeScheduleOnReceivedMessage(executionContext, instance);
        }

        private void SetupTransaction(NativeActivityContext executionContext, ReceiveMessageInstanceData instance)
        {
            WorkflowOperationContext workflowOperationContext = instance.CorrelationResponseContext.WorkflowOperationContext;
            if (workflowOperationContext.CurrentTransaction != null)
            {
                RuntimeTransactionHandle handle = null;
                handle = executionContext.Properties.Find(runtimeTransactionHandlePropertyName) as RuntimeTransactionHandle;
                if (handle == null)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.ReceiveNotWithinATransactedReceiveScope));
                }
                TransactedReceiveData data = executionContext.Properties.Find(TransactedReceiveData.TransactedReceiveDataExecutionPropertyName) as TransactedReceiveData;
                if ((data != null) && this.AdditionalData.IsFirstReceiveOfTransactedReceiveScopeTree)
                {
                    data.InitiatingTransaction = workflowOperationContext.OperationContext.TransactionFacet.Current;
                }
                Transaction currentTransaction = handle.GetCurrentTransaction(executionContext);
                if (currentTransaction != null)
                {
                    if (!currentTransaction.Equals(workflowOperationContext.CurrentTransaction))
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.FlowedTransactionDifferentFromAmbient));
                    }
                    this.ServerScheduleOnReceivedMessage(executionContext, instance);
                }
                else
                {
                    ReceiveMessageState state = new ReceiveMessageState {
                        CurrentTransaction = workflowOperationContext.CurrentTransaction.Clone(),
                        Instance = instance
                    };
                    handle.RequireTransactionContext(executionContext, new Action<NativeActivityTransactionContext, object>(this.RequireContextCallback), state);
                }
            }
            else
            {
                this.ServerScheduleOnReceivedMessage(executionContext, instance);
            }
        }

        internal ServiceDescriptionData AdditionalData
        {
            get
            {
                if (this.additionalData == null)
                {
                    this.additionalData = new ServiceDescriptionData();
                }
                return this.additionalData;
            }
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public InArgument<CorrelationHandle> CorrelatesWith { get; set; }

        public Collection<CorrelationInitializer> CorrelationInitializers
        {
            get
            {
                if (this.correlationInitializers == null)
                {
                    this.correlationInitializers = new Collection<CorrelationInitializer>();
                }
                return this.correlationInitializers;
            }
        }

        internal bool IsOneWay { get; set; }

        public OutArgument<System.ServiceModel.Channels.Message> Message { get; set; }

        public InArgument<System.Activities.NoPersistHandle> NoPersistHandle { get; set; }

        internal string OperationBookmarkName
        {
            get
            {
                if (this.operationBookmarkName == null)
                {
                    this.operationBookmarkName = BookmarkNameHelper.CreateBookmarkName(this.OperationName, this.ServiceContractName);
                }
                return this.operationBookmarkName;
            }
        }

        public string OperationName { get; set; }

        public XName ServiceContractName { get; set; }

        private class ReceiveMessageInstanceData
        {
            private CorrelationHandle ambientCorrelation;
            private bool triedAmbientCorrelation;

            public ReceiveMessageInstanceData(System.ServiceModel.Activities.CorrelationRequestContext requestContext)
            {
                this.CorrelationRequestContext = requestContext;
            }

            public ReceiveMessageInstanceData(System.ServiceModel.Activities.CorrelationResponseContext responseContext)
            {
                this.CorrelationResponseContext = responseContext;
                this.CorrelationCallbackContext = MessagingActivityHelper.CreateCorrelationCallbackContext(responseContext.WorkflowOperationContext.OperationContext.IncomingMessageProperties);
            }

            public CorrelationHandle GetAmbientCorrelation(NativeActivityContext context)
            {
                if (!this.triedAmbientCorrelation)
                {
                    this.triedAmbientCorrelation = true;
                    this.ambientCorrelation = context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                }
                return this.ambientCorrelation;
            }

            internal OperationContext GetOperationContext()
            {
                if (this.CorrelationRequestContext != null)
                {
                    return this.CorrelationRequestContext.OperationContext;
                }
                if (this.CorrelationResponseContext != null)
                {
                    return this.CorrelationResponseContext.WorkflowOperationContext.OperationContext;
                }
                return null;
            }

            public void InitializeContextAndCallbackContext()
            {
                this.CorrelationCallbackContext = MessagingActivityHelper.CreateCorrelationCallbackContext(this.CorrelationRequestContext.Reply.Properties);
                this.CorrelationContext = MessagingActivityHelper.CreateCorrelationContext(this.CorrelationRequestContext.Reply.Properties);
            }

            public void SetAmbientCorrelation(CorrelationHandle ambientCorrelation)
            {
                this.ambientCorrelation = ambientCorrelation;
                this.triedAmbientCorrelation = true;
            }

            public Guid AmbientActivityId { get; set; }

            public System.ServiceModel.Activities.CorrelationCallbackContext CorrelationCallbackContext { get; private set; }

            public System.ServiceModel.Activities.CorrelationContext CorrelationContext { get; private set; }

            public System.ServiceModel.Activities.CorrelationRequestContext CorrelationRequestContext { get; private set; }

            public System.ServiceModel.Activities.CorrelationResponseContext CorrelationResponseContext { get; private set; }
        }

        private class ReceiveMessageState
        {
            public Transaction CurrentTransaction { get; set; }

            public InternalReceiveMessage.ReceiveMessageInstanceData Instance { get; set; }
        }

        [DataContract]
        private class VolatileReceiveMessageInstance
        {
            public InternalReceiveMessage.ReceiveMessageInstanceData Instance { get; set; }
        }

        private class WaitForReply : AsyncCodeActivity
        {
            protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
            {
                return new WaitForReplyAsyncResult(this.Instance.Get(context).Instance, callback, state);
            }

            protected override void Cancel(AsyncCodeActivityContext context)
            {
                this.Instance.Get(context).Instance.CorrelationRequestContext.Cancel();
                base.Cancel(context);
            }

            protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
            {
                WaitForReplyAsyncResult.End(result);
            }

            public InArgument<InternalReceiveMessage.VolatileReceiveMessageInstance> Instance { get; set; }

            private class WaitForReplyAsyncResult : AsyncResult
            {
                private static Action<object, TimeoutException> onReceiveReply;

                public WaitForReplyAsyncResult(InternalReceiveMessage.ReceiveMessageInstanceData instance, AsyncCallback callback, object state) : base(callback, state)
                {
                    if (onReceiveReply == null)
                    {
                        onReceiveReply = new Action<object, TimeoutException>(InternalReceiveMessage.WaitForReply.WaitForReplyAsyncResult.OnReceiveReply);
                    }
                    if (instance.CorrelationRequestContext.WaitForReplyAsync(onReceiveReply, this))
                    {
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<InternalReceiveMessage.WaitForReply.WaitForReplyAsyncResult>(result);
                }

                private static void OnReceiveReply(object state, TimeoutException timeoutException)
                {
                    ((InternalReceiveMessage.WaitForReply.WaitForReplyAsyncResult) state).Complete(false);
                }
            }
        }
    }
}

