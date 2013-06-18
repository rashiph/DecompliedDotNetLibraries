namespace System.Activities
{
    using System;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class WorkflowInvoker
    {
        private static AsyncCallback cancelCallback;
        private WorkflowInstanceExtensionManager extensions;
        private static AsyncCallback invokeCallback;
        private Dictionary<object, AsyncInvokeContext> pendingInvokes;
        private SendOrPostCallback raiseInvokeCompletedCallback;
        private object thisLock;
        private Activity workflow;

        public event EventHandler<InvokeCompletedEventArgs> InvokeCompleted;

        public WorkflowInvoker(Activity workflow)
        {
            if (workflow == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflow");
            }
            this.workflow = workflow;
            this.thisLock = new object();
        }

        private void AddToPendingInvokes(AsyncInvokeContext context)
        {
            lock (this.ThisLock)
            {
                if (this.PendingInvokes.ContainsKey(context.UserState))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.SameUserStateUsedForMultipleInvokes));
                }
                this.PendingInvokes.Add(context.UserState, context);
            }
        }

        public IAsyncResult BeginInvoke(AsyncCallback callback, object state)
        {
            return this.BeginInvoke(this.workflow, ActivityDefaults.InvokeTimeout, this.extensions, callback, state);
        }

        public IAsyncResult BeginInvoke(IDictionary<string, object> inputs, AsyncCallback callback, object state)
        {
            return this.BeginInvoke(this.workflow, inputs, ActivityDefaults.InvokeTimeout, this.extensions, callback, state);
        }

        public IAsyncResult BeginInvoke(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return this.BeginInvoke(this.workflow, timeout, this.extensions, callback, state);
        }

        public IAsyncResult BeginInvoke(IDictionary<string, object> inputs, TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return this.BeginInvoke(this.workflow, inputs, timeout, this.extensions, callback, state);
        }

        private IAsyncResult BeginInvoke(Activity workflow, TimeSpan timeout, WorkflowInstanceExtensionManager extensions, AsyncCallback callback, object state)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return System.Activities.WorkflowApplication.BeginInvoke(workflow, null, extensions, timeout, null, null, callback, state);
        }

        private IAsyncResult BeginInvoke(Activity workflow, IDictionary<string, object> inputs, TimeSpan timeout, WorkflowInstanceExtensionManager extensions, AsyncCallback callback, object state)
        {
            if (inputs == null)
            {
                throw FxTrace.Exception.ArgumentNull("inputs");
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return System.Activities.WorkflowApplication.BeginInvoke(workflow, inputs, extensions, timeout, null, null, callback, state);
        }

        public void CancelAsync(object userState)
        {
            if (userState == null)
            {
                throw FxTrace.Exception.ArgumentNull("userState");
            }
            AsyncInvokeContext state = this.RemoveFromPendingInvokes(userState);
            if (state != null)
            {
                if (cancelCallback == null)
                {
                    cancelCallback = Fx.ThunkCallback(new AsyncCallback(this.CancelCallback));
                }
                IAsyncResult result = state.WorkflowApplication.BeginCancel(TimeSpan.MaxValue, cancelCallback, state);
                if (result.CompletedSynchronously)
                {
                    state.WorkflowApplication.EndCancel(result);
                }
            }
        }

        private void CancelCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                AsyncInvokeContext asyncState = (AsyncInvokeContext) result.AsyncState;
                asyncState.WorkflowApplication.EndCancel(result);
            }
        }

        public IDictionary<string, object> EndInvoke(IAsyncResult result)
        {
            return System.Activities.WorkflowApplication.EndInvoke(result);
        }

        private void InternalInvokeAsync(IDictionary<string, object> inputs, TimeSpan timeout, object userState)
        {
            AsyncInvokeContext context = new AsyncInvokeContext(userState, this);
            if (userState != null)
            {
                this.AddToPendingInvokes(context);
            }
            Exception error = null;
            bool flag = false;
            try
            {
                if (invokeCallback == null)
                {
                    invokeCallback = Fx.ThunkCallback(new AsyncCallback(this.InvokeCallback));
                }
                context.Operation.OperationStarted();
                IAsyncResult result = System.Activities.WorkflowApplication.BeginInvoke(this.workflow, inputs, this.extensions, timeout, SynchronizationContext.Current, context, invokeCallback, context);
                if (result.CompletedSynchronously)
                {
                    context.Outputs = this.EndInvoke(result);
                    flag = true;
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                error = exception2;
            }
            if ((error != null) || flag)
            {
                this.PostInvokeCompletedAndRemove(context, error);
            }
        }

        public IDictionary<string, object> Invoke()
        {
            return Invoke(this.workflow, ActivityDefaults.InvokeTimeout, this.extensions);
        }

        public static IDictionary<string, object> Invoke(Activity workflow)
        {
            return Invoke(workflow, ActivityDefaults.InvokeTimeout);
        }

        public static TResult Invoke<TResult>(Activity<TResult> workflow)
        {
            return Invoke<TResult>(workflow, null);
        }

        public IDictionary<string, object> Invoke(TimeSpan timeout)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return Invoke(this.workflow, timeout, this.extensions);
        }

        public IDictionary<string, object> Invoke(IDictionary<string, object> inputs)
        {
            return Invoke(this.workflow, inputs, ActivityDefaults.InvokeTimeout, this.extensions);
        }

        public static IDictionary<string, object> Invoke(Activity workflow, IDictionary<string, object> inputs)
        {
            return Invoke(workflow, inputs, ActivityDefaults.InvokeTimeout, null);
        }

        public static IDictionary<string, object> Invoke(Activity workflow, TimeSpan timeout)
        {
            return Invoke(workflow, timeout, null);
        }

        public static TResult Invoke<TResult>(Activity<TResult> workflow, IDictionary<string, object> inputs)
        {
            return Invoke<TResult>(workflow, inputs, ActivityDefaults.InvokeTimeout);
        }

        public IDictionary<string, object> Invoke(IDictionary<string, object> inputs, TimeSpan timeout)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return Invoke(this.workflow, inputs, timeout, this.extensions);
        }

        private static IDictionary<string, object> Invoke(Activity workflow, TimeSpan timeout, WorkflowInstanceExtensionManager extensions)
        {
            if (workflow == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflow");
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            IDictionary<string, object> dictionary = System.Activities.WorkflowApplication.Invoke(workflow, null, extensions, timeout);
            if (dictionary == null)
            {
                return ActivityUtilities.EmptyParameters;
            }
            return dictionary;
        }

        public static IDictionary<string, object> Invoke(Activity workflow, IDictionary<string, object> inputs, TimeSpan timeout)
        {
            return Invoke(workflow, inputs, timeout, null);
        }

        public static TResult Invoke<TResult>(Activity<TResult> workflow, IDictionary<string, object> inputs, TimeSpan timeout)
        {
            IDictionary<string, object> dictionary;
            return Invoke<TResult>(workflow, inputs, out dictionary, timeout);
        }

        private static IDictionary<string, object> Invoke(Activity workflow, IDictionary<string, object> inputs, TimeSpan timeout, WorkflowInstanceExtensionManager extensions)
        {
            if (workflow == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflow");
            }
            if (inputs == null)
            {
                throw FxTrace.Exception.ArgumentNull("inputs");
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            IDictionary<string, object> dictionary = System.Activities.WorkflowApplication.Invoke(workflow, inputs, extensions, timeout);
            if (dictionary == null)
            {
                return ActivityUtilities.EmptyParameters;
            }
            return dictionary;
        }

        public static TResult Invoke<TResult>(Activity<TResult> workflow, IDictionary<string, object> inputs, out IDictionary<string, object> additionalOutputs, TimeSpan timeout)
        {
            object obj2;
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            if (inputs != null)
            {
                additionalOutputs = Invoke(workflow, inputs, timeout, null);
            }
            else
            {
                additionalOutputs = Invoke(workflow, timeout, null);
            }
            if (!additionalOutputs.TryGetValue("Result", out obj2))
            {
                throw Fx.AssertAndThrow("Activity<TResult> should always have a output named \"Result\"");
            }
            additionalOutputs.Remove("Result");
            return (TResult) obj2;
        }

        public void InvokeAsync()
        {
            this.InvokeAsync(ActivityDefaults.InvokeTimeout, null);
        }

        public void InvokeAsync(object userState)
        {
            this.InvokeAsync(ActivityDefaults.InvokeTimeout, userState);
        }

        public void InvokeAsync(TimeSpan timeout)
        {
            this.InvokeAsync(timeout, null);
        }

        public void InvokeAsync(IDictionary<string, object> inputs)
        {
            this.InvokeAsync(inputs, null);
        }

        public void InvokeAsync(TimeSpan timeout, object userState)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            this.InternalInvokeAsync(null, timeout, userState);
        }

        public void InvokeAsync(IDictionary<string, object> inputs, object userState)
        {
            this.InvokeAsync(inputs, ActivityDefaults.InvokeTimeout, userState);
        }

        public void InvokeAsync(IDictionary<string, object> inputs, TimeSpan timeout)
        {
            this.InvokeAsync(inputs, timeout, null);
        }

        public void InvokeAsync(IDictionary<string, object> inputs, TimeSpan timeout, object userState)
        {
            if (inputs == null)
            {
                throw FxTrace.Exception.ArgumentNull("inputs");
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            this.InternalInvokeAsync(inputs, timeout, userState);
        }

        private void InvokeCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                AsyncInvokeContext asyncState = (AsyncInvokeContext) result.AsyncState;
                WorkflowInvoker invoker = asyncState.Invoker;
                Exception error = null;
                try
                {
                    asyncState.Outputs = invoker.EndInvoke(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    error = exception2;
                }
                invoker.PostInvokeCompletedAndRemove(asyncState, error);
            }
        }

        private void PostInvokeCompleted(AsyncInvokeContext context, Exception error)
        {
            bool flag;
            if (error == null)
            {
                context.WorkflowApplication.GetCompletionStatus(out error, out flag);
            }
            else
            {
                flag = false;
            }
            this.PostInvokeCompleted(context, flag, error);
        }

        private void PostInvokeCompleted(AsyncInvokeContext context, bool cancelled, Exception error)
        {
            InvokeCompletedEventArgs arg = new InvokeCompletedEventArgs(error, cancelled, context);
            if (this.InvokeCompleted == null)
            {
                context.Operation.OperationCompleted();
            }
            else
            {
                context.Operation.PostOperationCompleted(this.RaiseInvokeCompletedCallback, arg);
            }
        }

        private void PostInvokeCompletedAndRemove(AsyncInvokeContext context, Exception error)
        {
            if (context.UserState != null)
            {
                this.RemoveFromPendingInvokes(context.UserState);
            }
            this.PostInvokeCompleted(context, error);
        }

        private void RaiseInvokeCompleted(object state)
        {
            EventHandler<InvokeCompletedEventArgs> invokeCompleted = this.InvokeCompleted;
            if (invokeCompleted != null)
            {
                invokeCompleted(this, (InvokeCompletedEventArgs) state);
            }
        }

        private AsyncInvokeContext RemoveFromPendingInvokes(object userState)
        {
            AsyncInvokeContext context;
            lock (this.ThisLock)
            {
                if (this.PendingInvokes.TryGetValue(userState, out context))
                {
                    this.PendingInvokes.Remove(userState);
                }
            }
            return context;
        }

        public WorkflowInstanceExtensionManager Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new WorkflowInstanceExtensionManager();
                }
                return this.extensions;
            }
        }

        private Dictionary<object, AsyncInvokeContext> PendingInvokes
        {
            get
            {
                if (this.pendingInvokes == null)
                {
                    this.pendingInvokes = new Dictionary<object, AsyncInvokeContext>();
                }
                return this.pendingInvokes;
            }
        }

        private SendOrPostCallback RaiseInvokeCompletedCallback
        {
            get
            {
                if (this.raiseInvokeCompletedCallback == null)
                {
                    this.raiseInvokeCompletedCallback = Fx.ThunkCallback(new SendOrPostCallback(this.RaiseInvokeCompleted));
                }
                return this.raiseInvokeCompletedCallback;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}

