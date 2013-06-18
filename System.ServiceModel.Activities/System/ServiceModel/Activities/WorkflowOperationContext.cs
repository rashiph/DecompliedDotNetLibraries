namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.Interop;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;

    internal class WorkflowOperationContext : AsyncResult
    {
        private Guid ambientActivityId;
        private long beginOperation;
        private long beginTime;
        private Bookmark bookmark;
        private BookmarkScope bookmarkScope;
        private object bookmarkValue;
        private Guid e2eActivityId;
        private static readonly ReadOnlyDictionary<string, string> emptyDictionary = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(), false);
        private static readonly object[] emptyObjectArray = new object[0];
        private static AsyncResult.AsyncCompletion handleEndProcessReceiveContext;
        private static AsyncResult.AsyncCompletion handleEndResumeBookmark = new AsyncResult.AsyncCompletion(WorkflowOperationContext.HandleEndResumeBookmark);
        private static AsyncResult.AsyncCompletion handleEndWaitForPendingOperations = new AsyncResult.AsyncCompletion(WorkflowOperationContext.HandleEndWaitForPendingOperations);
        private bool hasDecrementedBusyCount;
        private object[] inputs;
        private IInvokeReceivedNotification notification;
        private static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(WorkflowOperationContext.Finally);
        private string operationName;
        private object operationReturnValue;
        private object[] outputs;
        private IAsyncResult pendingAsyncResult;
        private Exception pendingException;
        private bool performanceCountersEnabled;
        private bool propagateActivity;
        private ReceiveContext receiveContext;
        private object thisLock;
        private TimeoutHelper timeoutHelper;
        private WorkflowServiceInstance workflowInstance;

        private WorkflowOperationContext(object[] inputs, System.ServiceModel.OperationContext operationContext, string operationName, bool performanceCountersEnabled, bool propagateActivity, Transaction currentTransaction, WorkflowServiceInstance workflowInstance, IInvokeReceivedNotification notification, WorkflowOperationBehavior behavior, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
        {
            this.inputs = inputs;
            this.operationName = operationName;
            this.OperationContext = operationContext;
            this.CurrentTransaction = currentTransaction;
            this.performanceCountersEnabled = performanceCountersEnabled;
            this.propagateActivity = propagateActivity;
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.workflowInstance = workflowInstance;
            this.thisLock = new object();
            this.notification = notification;
            base.OnCompleting = onCompleting;
            this.bookmark = behavior.OnResolveBookmark(this, out this.bookmarkScope, out this.bookmarkValue);
            bool flag = false;
            try
            {
                if (TraceUtility.MessageFlowTracingOnly)
                {
                    this.e2eActivityId = TraceUtility.GetReceivedActivityId(this.OperationContext);
                    DiagnosticTrace.ActivityId = this.e2eActivityId;
                }
                if (this.workflowInstance.BufferedReceiveManager != null)
                {
                    ReceiveContext.TryGet(this.OperationContext.IncomingMessageProperties, out this.receiveContext);
                    this.OperationContext.IncomingMessageProperties.Remove(ReceiveContext.Name);
                }
                flag = this.ProcessRequest();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.OnCompleting(this, exception);
                throw;
            }
            if (flag)
            {
                base.Complete(true);
            }
        }

        public static IAsyncResult BeginProcessRequest(WorkflowServiceInstance workflowInstance, System.ServiceModel.OperationContext operationContext, string operationName, object[] inputs, bool performanceCountersEnabled, bool propagateActivity, Transaction currentTransaction, IInvokeReceivedNotification notification, WorkflowOperationBehavior behavior, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new WorkflowOperationContext(inputs, operationContext, operationName, performanceCountersEnabled, propagateActivity, currentTransaction, workflowInstance, notification, behavior, timeout, callback, state);
        }

        private void DecrementBusyCount()
        {
            lock (this.thisLock)
            {
                if (!this.hasDecrementedBusyCount)
                {
                    AspNetEnvironment.Current.DecrementBusyCount();
                    if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                    {
                        AspNetEnvironment.Current.TraceDecrementBusyCount(System.ServiceModel.Activities.SR.BusyCountTraceFormatString(this.workflowInstance.Id));
                    }
                    this.hasDecrementedBusyCount = true;
                }
            }
        }

        private void EmitTransferFromInstanceId()
        {
            if (TraceUtility.MessageFlowTracing)
            {
                if (DiagnosticTrace.ActivityId != this.workflowInstance.Id)
                {
                    DiagnosticTrace.ActivityId = this.workflowInstance.Id;
                }
                System.ServiceModel.Activities.FxTrace.Trace.SetAndTraceTransfer(this.E2EActivityId, true);
            }
        }

        public static object EndProcessRequest(IAsyncResult result, out object[] outputs)
        {
            WorkflowOperationContext context = AsyncResult.End<WorkflowOperationContext>(result);
            outputs = context.outputs;
            return context.operationReturnValue;
        }

        private static void Finally(AsyncResult result, Exception completionException)
        {
            WorkflowOperationContext context = (WorkflowOperationContext) result;
            context.EmitTransferFromInstanceId();
            if (completionException != null)
            {
                if (completionException is FaultException)
                {
                    context.TrackMethodFaulted();
                }
                else
                {
                    context.TrackMethodFailed();
                }
            }
            else
            {
                context.TrackMethodCompleted(context.operationReturnValue);
            }
            context.ProcessFinalizationTraces();
            context.workflowInstance.ReleaseContext(context);
            context.RemovePendingOperation();
        }

        private long GetDuration()
        {
            long time = 0L;
            long num2 = 0L;
            if ((this.beginTime >= 0L) && (System.Runtime.Interop.UnsafeNativeMethods.QueryPerformanceCounter(out time) != 0))
            {
                num2 = time - this.beginTime;
            }
            return num2;
        }

        private static bool HandleEndProcessReceiveContext(IAsyncResult result)
        {
            ReceiveContextAsyncResult.EndProcessReceiveContext(result);
            return true;
        }

        private static bool HandleEndResumeBookmark(IAsyncResult result)
        {
            WorkflowOperationContext asyncState = (WorkflowOperationContext) result.AsyncState;
            bool flag = false;
            bool flag2 = true;
            try
            {
                if (asyncState.workflowInstance.EndResumeProtocolBookmark(result) != BookmarkResumptionResult.Success)
                {
                    asyncState.OperationContext.Host.RaiseUnknownMessageReceived(asyncState.OperationContext.IncomingMessage);
                    if ((asyncState.workflowInstance.BufferedReceiveManager != null) && asyncState.workflowInstance.BufferedReceiveManager.BufferReceive(asyncState.OperationContext, asyncState.receiveContext, asyncState.bookmark.Name, BufferedReceiveState.WaitingOnBookmark, false))
                    {
                        if (System.ServiceModel.Activities.TD.BufferOutOfOrderMessageNoBookmarkIsEnabled())
                        {
                            System.ServiceModel.Activities.TD.BufferOutOfOrderMessageNoBookmark(asyncState.workflowInstance.Id.ToString(), asyncState.bookmark.Name);
                        }
                        flag2 = false;
                    }
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateOperationNotAvailableFault(asyncState.workflowInstance.Id, asyncState.bookmark.Name)));
                }
                lock (asyncState.thisLock)
                {
                    if (asyncState.CurrentState == State.ResultReceived)
                    {
                        asyncState.CurrentState = State.Completed;
                        if (asyncState.pendingException != null)
                        {
                            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(asyncState.pendingException);
                        }
                        flag = true;
                    }
                    else
                    {
                        asyncState.CurrentState = State.WaitForResult;
                        flag = false;
                    }
                    if (flag)
                    {
                        flag = asyncState.ProcessReceiveContext();
                    }
                    flag2 = false;
                }
            }
            finally
            {
                if (flag2)
                {
                    BufferedReceiveManager.AbandonReceiveContext(asyncState.receiveContext);
                }
                asyncState.RemovePendingOperation();
            }
            return flag;
        }

        private static bool HandleEndWaitForPendingOperations(IAsyncResult result)
        {
            bool flag3;
            WorkflowOperationContext asyncState = (WorkflowOperationContext) result.AsyncState;
            asyncState.pendingAsyncResult = result;
            bool flag = false;
            try
            {
                asyncState.workflowInstance.EndWaitForPendingOperations(result);
                bool flag2 = asyncState.OnResumeBookmark();
                flag = true;
                flag3 = flag2;
            }
            finally
            {
                if (!flag)
                {
                    asyncState.RemovePendingOperation();
                }
            }
            return flag3;
        }

        private void IncrementBusyCount()
        {
            AspNetEnvironment.Current.IncrementBusyCount();
            if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
            {
                AspNetEnvironment.Current.TraceIncrementBusyCount(System.ServiceModel.Activities.SR.BusyCountTraceFormatString(this.workflowInstance.Id));
            }
        }

        private bool OnResumeBookmark()
        {
            bool flag3;
            bool flag = false;
            try
            {
                bool flag2;
                IAsyncResult result = this.workflowInstance.BeginResumeProtocolBookmark(this.bookmark, this.bookmarkScope, this, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndResumeBookmark), this);
                if (result.CompletedSynchronously)
                {
                    flag2 = HandleEndResumeBookmark(result);
                }
                else
                {
                    flag2 = false;
                }
                flag = true;
                flag3 = flag2;
            }
            finally
            {
                if (!flag)
                {
                    this.RemovePendingOperation();
                }
            }
            return flag3;
        }

        private void ProcessFinalizationTraces()
        {
            try
            {
                if (this.propagateActivity)
                {
                    Guid activityId = Trace.CorrelationManager.ActivityId;
                    if (System.ServiceModel.Activities.TD.StopSignpostEventIsEnabled())
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
                        dictionary.Add("ActivityName", "WorkflowOperationInvoke");
                        dictionary.Add("ActivityType", "ExecuteUserCode");
                        System.ServiceModel.Activities.TD.StopSignpostEvent(new DictionaryTraceRecord(dictionary));
                    }
                    System.ServiceModel.Activities.FxTrace.Trace.SetAndTraceTransfer(this.ambientActivityId, true);
                    this.ambientActivityId = Guid.Empty;
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
            this.DecrementBusyCount();
        }

        private void ProcessInitializationTraces()
        {
            this.IncrementBusyCount();
            try
            {
                if (TraceUtility.MessageFlowTracingOnly)
                {
                    DiagnosticTrace.ActivityId = this.E2EActivityId;
                    this.propagateActivity = false;
                }
                if (TraceUtility.ActivityTracing || (!TraceUtility.MessageFlowTracing && this.propagateActivity))
                {
                    this.e2eActivityId = TraceUtility.GetReceivedActivityId(this.OperationContext);
                    if ((this.E2EActivityId != Guid.Empty) && (this.E2EActivityId != Trace.CorrelationManager.ActivityId))
                    {
                        this.propagateActivity = true;
                        this.OperationContext.IncomingMessageProperties["E2EActivityId"] = this.E2EActivityId;
                        this.ambientActivityId = Trace.CorrelationManager.ActivityId;
                        System.ServiceModel.Activities.FxTrace.Trace.SetAndTraceTransfer(this.E2EActivityId, true);
                        if (System.ServiceModel.Activities.TD.StartSignpostEventIsEnabled())
                        {
                            Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
                            dictionary.Add("ActivityName", "WorkflowOperationInvoke");
                            dictionary.Add("ActivityType", "ExecuteUserCode");
                            System.ServiceModel.Activities.TD.StartSignpostEvent(new DictionaryTraceRecord(dictionary));
                        }
                    }
                    else
                    {
                        this.propagateActivity = false;
                    }
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

        private bool ProcessReceiveContext()
        {
            if (this.receiveContext == null)
            {
                return true;
            }
            if (handleEndProcessReceiveContext == null)
            {
                handleEndProcessReceiveContext = new AsyncResult.AsyncCompletion(WorkflowOperationContext.HandleEndProcessReceiveContext);
            }
            IAsyncResult result = ReceiveContextAsyncResult.BeginProcessReceiveContext(this, this.receiveContext, base.PrepareAsyncCompletion(handleEndProcessReceiveContext), this);
            return base.SyncContinue(result);
        }

        private bool ProcessReply()
        {
            bool flag = false;
            this.workflowInstance.ReleaseContext(this);
            this.RemovePendingOperation();
            if (this.CurrentState == State.BookmarkResumption)
            {
                this.CurrentState = State.ResultReceived;
            }
            else if (this.CurrentState == State.WaitForResult)
            {
                this.CurrentState = State.Completed;
                flag = true;
            }
            if (flag)
            {
                if (this.pendingException == null)
                {
                    return this.ProcessReceiveContext();
                }
                BufferedReceiveManager.AbandonReceiveContext(this.receiveContext);
            }
            return flag;
        }

        private bool ProcessRequest()
        {
            this.TrackMethodCalled();
            this.ProcessInitializationTraces();
            if (this.notification == null)
            {
                return this.OnResumeBookmark();
            }
            string sessionId = this.OperationContext.SessionId;
            if (sessionId == null)
            {
                return this.OnResumeBookmark();
            }
            IAsyncResult result = this.workflowInstance.BeginWaitForPendingOperations(sessionId, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndWaitForPendingOperations), this);
            this.notification.NotifyInvokeReceived();
            return (result.CompletedSynchronously && HandleEndWaitForPendingOperations(result));
        }

        private void RemovePendingOperation()
        {
            if (this.pendingAsyncResult != null)
            {
                this.workflowInstance.RemovePendingOperation(this.OperationContext.SessionId, this.pendingAsyncResult);
                this.pendingAsyncResult = null;
            }
        }

        public void SendFault(Exception exception)
        {
            bool flag;
            this.pendingException = exception;
            lock (this.thisLock)
            {
                flag = this.ProcessReply();
            }
            if (flag)
            {
                base.Complete(false, exception);
            }
        }

        public void SendReply(Message returnValue)
        {
            bool flag;
            lock (this.thisLock)
            {
                this.outputs = emptyObjectArray;
                this.operationReturnValue = returnValue;
                flag = this.ProcessReply();
            }
            if (flag)
            {
                base.Complete(false);
            }
        }

        public void SendReply(object returnValue, object[] outputs)
        {
            bool flag;
            lock (this.thisLock)
            {
                this.outputs = outputs ?? emptyObjectArray;
                this.operationReturnValue = returnValue;
                flag = this.ProcessReply();
            }
            if (flag)
            {
                base.Complete(false);
            }
        }

        public void SetOperationCompleted()
        {
            bool flag;
            lock (this.thisLock)
            {
                flag = this.ProcessReply();
            }
            if (flag)
            {
                base.Complete(false);
            }
        }

        private void TrackMethodCalled()
        {
            if (this.performanceCountersEnabled)
            {
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    PerformanceCounters.MethodCalled(this.operationName);
                }
                try
                {
                    if (System.Runtime.Interop.UnsafeNativeMethods.QueryPerformanceCounter(out this.beginTime) == 0)
                    {
                        this.beginTime = -1L;
                    }
                }
                catch (SecurityException exception)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new SecurityException(System.ServiceModel.Activities.SR.PartialTrustPerformanceCounterNotEnabled, exception));
                }
            }
            if ((System.ServiceModel.Diagnostics.Application.TD.OperationCompletedIsEnabled() || System.ServiceModel.Diagnostics.Application.TD.OperationFaultedIsEnabled()) || System.ServiceModel.Diagnostics.Application.TD.OperationFailedIsEnabled())
            {
                this.beginOperation = DateTime.UtcNow.Ticks;
            }
            if (System.ServiceModel.Diagnostics.Application.TD.OperationInvokedIsEnabled())
            {
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    System.ServiceModel.Diagnostics.Application.TD.OperationInvoked(this.operationName, TraceUtility.GetCallerInfo(this.OperationContext));
                }
            }
        }

        private void TrackMethodCompleted(object returnValue)
        {
            Message message = returnValue as Message;
            if ((message != null) && message.IsFault)
            {
                this.TrackMethodFaulted();
            }
            else
            {
                this.TrackMethodSucceeded();
            }
        }

        private void TrackMethodFailed()
        {
            if (this.performanceCountersEnabled)
            {
                long duration = this.GetDuration();
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    PerformanceCounters.MethodReturnedError(this.operationName, duration);
                }
            }
            if (System.ServiceModel.Diagnostics.Application.TD.OperationFailedIsEnabled())
            {
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    System.ServiceModel.Diagnostics.Application.TD.OperationFailed(this.operationName, TraceUtility.GetUtcBasedDurationForTrace(this.beginOperation));
                }
            }
        }

        private void TrackMethodFaulted()
        {
            if (this.performanceCountersEnabled)
            {
                long duration = this.GetDuration();
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    PerformanceCounters.MethodReturnedFault(this.operationName, duration);
                }
            }
            if (System.ServiceModel.Diagnostics.Application.TD.OperationFaultedIsEnabled())
            {
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    System.ServiceModel.Diagnostics.Application.TD.OperationFaulted(this.operationName, TraceUtility.GetUtcBasedDurationForTrace(this.beginOperation));
                }
            }
        }

        private void TrackMethodSucceeded()
        {
            if (this.performanceCountersEnabled)
            {
                long duration = this.GetDuration();
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    PerformanceCounters.MethodReturnedSuccess(this.operationName, duration);
                }
            }
            if (System.ServiceModel.Diagnostics.Application.TD.OperationCompletedIsEnabled())
            {
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    System.ServiceModel.Diagnostics.Application.TD.OperationCompleted(this.operationName, TraceUtility.GetUtcBasedDurationForTrace(this.beginOperation));
                }
            }
        }

        public object BookmarkValue
        {
            get
            {
                return this.bookmarkValue;
            }
        }

        private State CurrentState { get; set; }

        public Transaction CurrentTransaction { get; private set; }

        public Guid E2EActivityId
        {
            get
            {
                return this.e2eActivityId;
            }
        }

        public bool HasResponse
        {
            get
            {
                lock (this.thisLock)
                {
                    return ((this.CurrentState == State.Completed) || (this.CurrentState == State.ResultReceived));
                }
            }
        }

        public object[] Inputs
        {
            get
            {
                return this.inputs;
            }
        }

        public System.ServiceModel.OperationContext OperationContext { get; private set; }

        private class OperationContextScopeHelper : IDisposable
        {
            private OperationContext currentOperationContext = OperationContext.Current;

            public OperationContextScopeHelper(OperationContext operationContext)
            {
                OperationContext.Current = operationContext;
            }

            void IDisposable.Dispose()
            {
                OperationContext.Current = this.currentOperationContext;
            }
        }

        private class ReceiveContextAsyncResult : AsyncResult
        {
            private WorkflowOperationContext context;
            private static AsyncResult.AsyncCompletion handleEndComplete = new AsyncResult.AsyncCompletion(WorkflowOperationContext.ReceiveContextAsyncResult.HandleEndComplete);
            private ReceiveContext receiveContext;

            private ReceiveContextAsyncResult(WorkflowOperationContext context, ReceiveContext receiveContext, AsyncCallback callback, object state) : base(callback, state)
            {
                this.context = context;
                this.receiveContext = receiveContext;
                if (this.ProcessReceiveContext())
                {
                    base.Complete(true);
                }
            }

            public static IAsyncResult BeginProcessReceiveContext(WorkflowOperationContext context, ReceiveContext receiveContext, AsyncCallback callback, object state)
            {
                return new WorkflowOperationContext.ReceiveContextAsyncResult(context, receiveContext, callback, state);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowOperationContext.ReceiveContextAsyncResult>(result);
            }

            public static void EndProcessReceiveContext(IAsyncResult result)
            {
                End(result);
            }

            private static bool HandleEndComplete(IAsyncResult result)
            {
                WorkflowOperationContext.ReceiveContextAsyncResult asyncState = (WorkflowOperationContext.ReceiveContextAsyncResult) result.AsyncState;
                asyncState.receiveContext.EndComplete(result);
                return true;
            }

            private void OnTransactionComplete(object sender, TransactionEventArgs e)
            {
                if (e.Transaction.TransactionInformation.Status != TransactionStatus.Committed)
                {
                    BufferedReceiveManager.AbandonReceiveContext(this.context.receiveContext);
                }
            }

            private bool ProcessReceiveContext()
            {
                IAsyncResult result;
                using (base.PrepareTransactionalCall(this.context.CurrentTransaction))
                {
                    if (this.context.CurrentTransaction != null)
                    {
                        this.context.CurrentTransaction.TransactionCompleted += new TransactionCompletedEventHandler(this.OnTransactionComplete);
                    }
                    result = this.receiveContext.BeginComplete(this.context.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndComplete), this);
                }
                return base.SyncContinue(result);
            }
        }

        private enum State
        {
            BookmarkResumption,
            WaitForResult,
            ResultReceived,
            Completed
        }
    }
}

