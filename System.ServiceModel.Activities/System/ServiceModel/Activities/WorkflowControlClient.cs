namespace System.ServiceModel.Activities
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Transactions;

    public class WorkflowControlClient : ClientBase<IWorkflowInstanceManagement>
    {
        private bool checkedBinding;
        private SendOrPostCallback onAbandonCompleteDelegate;
        private ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate onBeginAbandonDelegate;
        private ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate onBeginCancelDelegate;
        private ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate onBeginRunDelegate;
        private ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate onBeginSuspendDelegate;
        private ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate onBeginTerminateDelegate;
        private ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate onBeginUnsuspendDelegate;
        private SendOrPostCallback onCancelCompleteDelegate;
        private ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate onEndAbandonDelegate;
        private ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate onEndCancelDelegate;
        private ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate onEndRunDelegate;
        private ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate onEndSuspendDelegate;
        private ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate onEndTerminateDelegate;
        private ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate onEndUnsuspendDelegate;
        private SendOrPostCallback onRunCompleteDelegate;
        private SendOrPostCallback onSuspendCompleteDelegate;
        private SendOrPostCallback onTerminateCompleteDelegate;
        private SendOrPostCallback onUnsuspendCompleteDelegate;
        private bool supportsTransactedInvoke;

        public event EventHandler<AsyncCompletedEventArgs> AbandonCompleted;

        public event EventHandler<AsyncCompletedEventArgs> CancelCompleted;

        public event EventHandler<AsyncCompletedEventArgs> RunCompleted;

        public event EventHandler<AsyncCompletedEventArgs> SuspendCompleted;

        public event EventHandler<AsyncCompletedEventArgs> TerminateCompleted;

        public event EventHandler<AsyncCompletedEventArgs> UnsuspendCompleted;

        public WorkflowControlClient()
        {
        }

        public WorkflowControlClient(WorkflowControlEndpoint workflowEndpoint) : base(workflowEndpoint.Binding, workflowEndpoint.Address)
        {
        }

        public WorkflowControlClient(string endpointConfigurationName) : base(endpointConfigurationName)
        {
        }

        public WorkflowControlClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        public WorkflowControlClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
        {
        }

        public WorkflowControlClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress)
        {
        }

        public void Abandon(Guid instanceId)
        {
            this.Abandon(instanceId, null);
        }

        public void Abandon(Guid instanceId, string reason)
        {
            base.Channel.Abandon(instanceId, reason);
        }

        public void AbandonAsync(Guid instanceId)
        {
            this.AbandonAsync(instanceId, null, null);
        }

        public void AbandonAsync(Guid instanceId, object userState)
        {
            this.AbandonAsync(instanceId, null, userState);
        }

        public void AbandonAsync(Guid instanceId, string reason)
        {
            this.AbandonAsync(instanceId, reason, null);
        }

        public void AbandonAsync(Guid instanceId, string reason, object userState)
        {
            if (this.onBeginAbandonDelegate == null)
            {
                this.onBeginAbandonDelegate = new ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate(this.OnBeginAbandon);
                this.onEndAbandonDelegate = new ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate(this.OnEndAbandon);
                this.onAbandonCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(this.OnAbandonCompleted));
            }
            base.InvokeAsync(this.onBeginAbandonDelegate, new object[] { instanceId, reason }, this.onEndAbandonDelegate, this.onAbandonCompleteDelegate, userState);
        }

        public IAsyncResult BeginAbandon(Guid instanceId, AsyncCallback callback, object state)
        {
            return this.BeginAbandon(instanceId, null, callback, state);
        }

        public IAsyncResult BeginAbandon(Guid instanceId, string reason, AsyncCallback callback, object state)
        {
            return base.Channel.BeginAbandon(instanceId, reason, callback, state);
        }

        public IAsyncResult BeginCancel(Guid instanceId, AsyncCallback callback, object state)
        {
            return new CancelAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, callback, state);
        }

        public IAsyncResult BeginRun(Guid instanceId, AsyncCallback callback, object state)
        {
            return new RunAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, callback, state);
        }

        public IAsyncResult BeginSuspend(Guid instanceId, AsyncCallback callback, object state)
        {
            return this.BeginSuspend(instanceId, System.ServiceModel.Activities.SR.DefaultSuspendReason, callback, state);
        }

        public IAsyncResult BeginSuspend(Guid instanceId, string reason, AsyncCallback callback, object state)
        {
            return new SuspendAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, reason, callback, state);
        }

        public IAsyncResult BeginTerminate(Guid instanceId, AsyncCallback callback, object state)
        {
            return this.BeginTerminate(instanceId, System.ServiceModel.Activities.SR.DefaultTerminationReason, callback, state);
        }

        public IAsyncResult BeginTerminate(Guid instanceId, string reason, AsyncCallback callback, object state)
        {
            return new TerminateAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, reason, callback, state);
        }

        public IAsyncResult BeginUnsuspend(Guid instanceId, AsyncCallback callback, object state)
        {
            return new UnsuspendAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, callback, state);
        }

        public void Cancel(Guid instanceId)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedCancel(instanceId);
            }
            else
            {
                base.Channel.Cancel(instanceId);
            }
        }

        public void CancelAsync(Guid instanceId)
        {
            this.CancelAsync(instanceId, null);
        }

        public void CancelAsync(Guid instanceId, object userState)
        {
            if (this.onBeginCancelDelegate == null)
            {
                this.onBeginCancelDelegate = new ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate(this.OnBeginCancel);
                this.onEndCancelDelegate = new ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate(this.OnEndCancel);
                this.onCancelCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(this.OnCancelCompleted));
            }
            base.InvokeAsync(this.onBeginCancelDelegate, new object[] { instanceId }, this.onEndCancelDelegate, this.onCancelCompleteDelegate, userState);
        }

        public void EndAbandon(IAsyncResult result)
        {
            base.Channel.EndAbandon(result);
        }

        public void EndCancel(IAsyncResult result)
        {
            CancelAsyncResult.End(result);
        }

        public void EndRun(IAsyncResult result)
        {
            RunAsyncResult.End(result);
        }

        public void EndSuspend(IAsyncResult result)
        {
            SuspendAsyncResult.End(result);
        }

        public void EndTerminate(IAsyncResult result)
        {
            TerminateAsyncResult.End(result);
        }

        public void EndUnsuspend(IAsyncResult result)
        {
            UnsuspendAsyncResult.End(result);
        }

        private void OnAbandonCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> abandonCompleted = this.AbandonCompleted;
            if (abandonCompleted != null)
            {
                ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs args = (ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs) state;
                abandonCompleted(this, new AsyncCompletedEventArgs(args.Error, args.Cancelled, args.UserState));
            }
        }

        private IAsyncResult OnBeginAbandon(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginAbandon((Guid) inputs[0], (string) inputs[1], callback, state);
        }

        private IAsyncResult OnBeginCancel(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginCancel((Guid) inputs[0], callback, state);
        }

        private IAsyncResult OnBeginRun(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginRun((Guid) inputs[0], callback, state);
        }

        private IAsyncResult OnBeginSuspend(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginSuspend((Guid) inputs[0], (string) inputs[1], callback, state);
        }

        private IAsyncResult OnBeginTerminate(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginTerminate((Guid) inputs[0], (string) inputs[1], callback, state);
        }

        private IAsyncResult OnBeginUnsuspend(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginUnsuspend((Guid) inputs[0], callback, state);
        }

        private void OnCancelCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> cancelCompleted = this.CancelCompleted;
            if (cancelCompleted != null)
            {
                ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs args = (ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs) state;
                cancelCompleted(this, new AsyncCompletedEventArgs(args.Error, args.Cancelled, args.UserState));
            }
        }

        private object[] OnEndAbandon(IAsyncResult result)
        {
            this.EndAbandon(result);
            return null;
        }

        private object[] OnEndCancel(IAsyncResult result)
        {
            this.EndCancel(result);
            return null;
        }

        private object[] OnEndRun(IAsyncResult result)
        {
            this.EndRun(result);
            return null;
        }

        private object[] OnEndSuspend(IAsyncResult result)
        {
            this.EndSuspend(result);
            return null;
        }

        private object[] OnEndTerminate(IAsyncResult result)
        {
            this.EndTerminate(result);
            return null;
        }

        private object[] OnEndUnsuspend(IAsyncResult result)
        {
            this.EndUnsuspend(result);
            return null;
        }

        private void OnRunCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> runCompleted = this.RunCompleted;
            if (runCompleted != null)
            {
                ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs args = (ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs) state;
                runCompleted(this, new AsyncCompletedEventArgs(args.Error, args.Cancelled, args.UserState));
            }
        }

        private void OnSuspendCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> suspendCompleted = this.SuspendCompleted;
            if (suspendCompleted != null)
            {
                ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs args = (ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs) state;
                suspendCompleted(this, new AsyncCompletedEventArgs(args.Error, args.Cancelled, args.UserState));
            }
        }

        private void OnTerminateCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> terminateCompleted = this.TerminateCompleted;
            if (terminateCompleted != null)
            {
                ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs args = (ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs) state;
                terminateCompleted(this, new AsyncCompletedEventArgs(args.Error, args.Cancelled, args.UserState));
            }
        }

        private void OnUnsuspendCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> unsuspendCompleted = this.UnsuspendCompleted;
            if (unsuspendCompleted != null)
            {
                ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs args = (ClientBase<IWorkflowInstanceManagement>.InvokeAsyncCompletedEventArgs) state;
                unsuspendCompleted(this, new AsyncCompletedEventArgs(args.Error, args.Cancelled, args.UserState));
            }
        }

        public void Run(Guid instanceId)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedRun(instanceId);
            }
            else
            {
                base.Channel.Run(instanceId);
            }
        }

        public void RunAsync(Guid instanceId)
        {
            this.RunAsync(instanceId, null);
        }

        public void RunAsync(Guid instanceId, object userState)
        {
            if (this.onBeginRunDelegate == null)
            {
                this.onBeginRunDelegate = new ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate(this.OnBeginRun);
                this.onEndRunDelegate = new ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate(this.OnEndRun);
                this.onRunCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(this.OnRunCompleted));
            }
            base.InvokeAsync(this.onBeginRunDelegate, new object[] { instanceId }, this.onEndRunDelegate, this.onRunCompleteDelegate, userState);
        }

        public void Suspend(Guid instanceId)
        {
            this.Suspend(instanceId, System.ServiceModel.Activities.SR.DefaultSuspendReason);
        }

        public void Suspend(Guid instanceId, string reason)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedSuspend(instanceId, reason);
            }
            else
            {
                base.Channel.Suspend(instanceId, reason);
            }
        }

        public void SuspendAsync(Guid instanceId)
        {
            this.SuspendAsync(instanceId, System.ServiceModel.Activities.SR.DefaultSuspendReason);
        }

        public void SuspendAsync(Guid instanceId, object userState)
        {
            this.SuspendAsync(instanceId, System.ServiceModel.Activities.SR.DefaultSuspendReason, userState);
        }

        public void SuspendAsync(Guid instanceId, string reason)
        {
            this.SuspendAsync(instanceId, reason, null);
        }

        public void SuspendAsync(Guid instanceId, string reason, object userState)
        {
            if (this.onBeginSuspendDelegate == null)
            {
                this.onEndSuspendDelegate = new ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate(this.OnEndSuspend);
                this.onSuspendCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(this.OnSuspendCompleted));
                this.onBeginSuspendDelegate = new ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate(this.OnBeginSuspend);
            }
            base.InvokeAsync(this.onBeginSuspendDelegate, new object[] { instanceId, reason }, this.onEndSuspendDelegate, this.onSuspendCompleteDelegate, userState);
        }

        public void Terminate(Guid instanceId)
        {
            this.Terminate(instanceId, System.ServiceModel.Activities.SR.DefaultTerminationReason);
        }

        public void Terminate(Guid instanceId, string reason)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedTerminate(instanceId, reason);
            }
            else
            {
                base.Channel.Terminate(instanceId, reason);
            }
        }

        public void TerminateAsync(Guid instanceId)
        {
            this.TerminateAsync(instanceId, System.ServiceModel.Activities.SR.DefaultTerminationReason);
        }

        public void TerminateAsync(Guid instanceId, object userState)
        {
            this.TerminateAsync(instanceId, System.ServiceModel.Activities.SR.DefaultTerminationReason, userState);
        }

        public void TerminateAsync(Guid instanceId, string reason)
        {
            this.TerminateAsync(instanceId, reason, null);
        }

        public void TerminateAsync(Guid instanceId, string reason, object userState)
        {
            if (this.onBeginTerminateDelegate == null)
            {
                this.onEndTerminateDelegate = new ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate(this.OnEndTerminate);
                this.onTerminateCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(this.OnTerminateCompleted));
                this.onBeginTerminateDelegate = new ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate(this.OnBeginTerminate);
            }
            base.InvokeAsync(this.onBeginTerminateDelegate, new object[] { instanceId, reason }, this.onEndTerminateDelegate, this.onTerminateCompleteDelegate, userState);
        }

        public void Unsuspend(Guid instanceId)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedUnsuspend(instanceId);
            }
            else
            {
                base.Channel.Unsuspend(instanceId);
            }
        }

        public void UnsuspendAsync(Guid instanceId)
        {
            this.UnsuspendAsync(instanceId, null);
        }

        public void UnsuspendAsync(Guid instanceId, object userState)
        {
            if (this.onBeginUnsuspendDelegate == null)
            {
                this.onBeginUnsuspendDelegate = new ClientBase<IWorkflowInstanceManagement>.BeginOperationDelegate(this.OnBeginUnsuspend);
                this.onEndUnsuspendDelegate = new ClientBase<IWorkflowInstanceManagement>.EndOperationDelegate(this.OnEndUnsuspend);
                this.onUnsuspendCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(this.OnUnsuspendCompleted));
            }
            base.InvokeAsync(this.onBeginUnsuspendDelegate, new object[] { instanceId }, this.onEndUnsuspendDelegate, this.onUnsuspendCompleteDelegate, userState);
        }

        private bool IsTransactedInvoke
        {
            get
            {
                return (this.SupportsTransactedInvoke && (Transaction.Current != null));
            }
        }

        private bool SupportsTransactedInvoke
        {
            get
            {
                if (!this.checkedBinding)
                {
                    foreach (BindingElement element in base.Endpoint.Binding.CreateBindingElements())
                    {
                        if (element is TransactionFlowBindingElement)
                        {
                            this.supportsTransactedInvoke = true;
                            break;
                        }
                    }
                    this.checkedBinding = true;
                }
                return this.supportsTransactedInvoke;
            }
        }

        private class CancelAsyncResult : AsyncResult
        {
            private IWorkflowInstanceManagement channel;
            private static AsyncResult.AsyncCompletion handleEndCancel = new AsyncResult.AsyncCompletion(WorkflowControlClient.CancelAsyncResult.HandleEndCancel);
            private bool isTransacted;

            public CancelAsyncResult(IWorkflowInstanceManagement channel, bool isTransacted, Guid instanceId, AsyncCallback callback, object state) : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;
                if (this.Cancel(instanceId))
                {
                    base.Complete(true);
                }
            }

            private bool Cancel(Guid instanceId)
            {
                IAsyncResult result;
                AsyncCallback callback = base.PrepareAsyncCompletion(handleEndCancel);
                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedCancel(instanceId, callback, this);
                }
                else
                {
                    result = this.channel.BeginCancel(instanceId, callback, this);
                }
                return (result.CompletedSynchronously && HandleEndCancel(result));
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowControlClient.CancelAsyncResult>(result);
            }

            private static bool HandleEndCancel(IAsyncResult result)
            {
                WorkflowControlClient.CancelAsyncResult asyncState = (WorkflowControlClient.CancelAsyncResult) result.AsyncState;
                if (asyncState.isTransacted)
                {
                    asyncState.channel.EndTransactedCancel(result);
                }
                else
                {
                    asyncState.channel.EndCancel(result);
                }
                return true;
            }
        }

        private class RunAsyncResult : AsyncResult
        {
            private IWorkflowInstanceManagement channel;
            private static AsyncResult.AsyncCompletion handleEndResume = new AsyncResult.AsyncCompletion(WorkflowControlClient.RunAsyncResult.HandleEndRun);
            private bool isTransacted;

            public RunAsyncResult(IWorkflowInstanceManagement channel, bool isTransacted, Guid instanceId, AsyncCallback callback, object state) : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;
                if (this.Run(instanceId))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowControlClient.RunAsyncResult>(result);
            }

            private static bool HandleEndRun(IAsyncResult result)
            {
                WorkflowControlClient.RunAsyncResult asyncState = (WorkflowControlClient.RunAsyncResult) result.AsyncState;
                if (asyncState.isTransacted)
                {
                    asyncState.channel.EndTransactedRun(result);
                }
                else
                {
                    asyncState.channel.EndRun(result);
                }
                return true;
            }

            private bool Run(Guid instanceId)
            {
                IAsyncResult result;
                AsyncCallback callback = base.PrepareAsyncCompletion(handleEndResume);
                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedRun(instanceId, callback, this);
                }
                else
                {
                    result = this.channel.BeginRun(instanceId, callback, this);
                }
                return (result.CompletedSynchronously && HandleEndRun(result));
            }
        }

        private class SuspendAsyncResult : AsyncResult
        {
            private IWorkflowInstanceManagement channel;
            private static AsyncResult.AsyncCompletion handleEndSuspend = new AsyncResult.AsyncCompletion(WorkflowControlClient.SuspendAsyncResult.HandleEndSuspend);
            private bool isTransacted;

            public SuspendAsyncResult(IWorkflowInstanceManagement channel, bool isTransacted, Guid instanceId, string reason, AsyncCallback callback, object state) : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;
                if (this.Suspend(instanceId, reason))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowControlClient.SuspendAsyncResult>(result);
            }

            private static bool HandleEndSuspend(IAsyncResult result)
            {
                WorkflowControlClient.SuspendAsyncResult asyncState = (WorkflowControlClient.SuspendAsyncResult) result.AsyncState;
                if (asyncState.isTransacted)
                {
                    asyncState.channel.EndTransactedSuspend(result);
                }
                else
                {
                    asyncState.channel.EndSuspend(result);
                }
                return true;
            }

            private bool Suspend(Guid instanceId, string reason)
            {
                IAsyncResult result;
                AsyncCallback callback = base.PrepareAsyncCompletion(handleEndSuspend);
                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedSuspend(instanceId, reason, callback, this);
                }
                else
                {
                    result = this.channel.BeginSuspend(instanceId, reason, callback, this);
                }
                return (result.CompletedSynchronously && HandleEndSuspend(result));
            }
        }

        private class TerminateAsyncResult : AsyncResult
        {
            private IWorkflowInstanceManagement channel;
            private static AsyncResult.AsyncCompletion handleEndTerminate = new AsyncResult.AsyncCompletion(WorkflowControlClient.TerminateAsyncResult.HandleEndTerminate);
            private bool isTransacted;

            public TerminateAsyncResult(IWorkflowInstanceManagement channel, bool isTransacted, Guid instanceId, string reason, AsyncCallback callback, object state) : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;
                if (this.Terminate(instanceId, reason))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowControlClient.TerminateAsyncResult>(result);
            }

            private static bool HandleEndTerminate(IAsyncResult result)
            {
                WorkflowControlClient.TerminateAsyncResult asyncState = (WorkflowControlClient.TerminateAsyncResult) result.AsyncState;
                if (asyncState.isTransacted)
                {
                    asyncState.channel.EndTransactedTerminate(result);
                }
                else
                {
                    asyncState.channel.EndTerminate(result);
                }
                return true;
            }

            private bool Terminate(Guid instanceId, string reason)
            {
                IAsyncResult result;
                AsyncCallback callback = base.PrepareAsyncCompletion(handleEndTerminate);
                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedTerminate(instanceId, reason, callback, this);
                }
                else
                {
                    result = this.channel.BeginTerminate(instanceId, reason, callback, this);
                }
                return (result.CompletedSynchronously && HandleEndTerminate(result));
            }
        }

        private class UnsuspendAsyncResult : AsyncResult
        {
            private IWorkflowInstanceManagement channel;
            private static AsyncResult.AsyncCompletion handleEndUnsuspend = new AsyncResult.AsyncCompletion(WorkflowControlClient.UnsuspendAsyncResult.HandleEndUnsuspend);
            private bool isTransacted;

            public UnsuspendAsyncResult(IWorkflowInstanceManagement channel, bool isTransacted, Guid instanceId, AsyncCallback callback, object state) : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;
                if (this.Unsuspend(instanceId))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowControlClient.UnsuspendAsyncResult>(result);
            }

            private static bool HandleEndUnsuspend(IAsyncResult result)
            {
                WorkflowControlClient.UnsuspendAsyncResult asyncState = (WorkflowControlClient.UnsuspendAsyncResult) result.AsyncState;
                if (asyncState.isTransacted)
                {
                    asyncState.channel.EndTransactedUnsuspend(result);
                }
                else
                {
                    asyncState.channel.EndUnsuspend(result);
                }
                return true;
            }

            private bool Unsuspend(Guid instanceId)
            {
                IAsyncResult result;
                AsyncCallback callback = base.PrepareAsyncCompletion(handleEndUnsuspend);
                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedUnsuspend(instanceId, callback, this);
                }
                else
                {
                    result = this.channel.BeginUnsuspend(instanceId, callback, this);
                }
                return (result.CompletedSynchronously && HandleEndUnsuspend(result));
            }
        }
    }
}

