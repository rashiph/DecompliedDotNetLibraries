namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class ThreadBehavior
    {
        private static Action<object> cleanThreadCallback;
        private readonly SynchronizationContext context;
        private SendOrPostCallback threadAffinityEndCallback;
        private SendOrPostCallback threadAffinityStartCallback;

        internal ThreadBehavior(DispatchRuntime dispatch)
        {
            this.context = dispatch.SynchronizationContext;
        }

        private void BindCore(ref MessageRpc rpc, bool startOperation)
        {
            SynchronizationContext syncContext = this.GetSyncContext(rpc.InstanceContext);
            if (syncContext != null)
            {
                IResumeMessageRpc state = rpc.Pause();
                if (startOperation)
                {
                    syncContext.OperationStarted();
                    syncContext.Post(this.ThreadAffinityStartCallbackDelegate, state);
                }
                else
                {
                    syncContext.Post(this.ThreadAffinityEndCallbackDelegate, state);
                }
            }
            else if (rpc.SwitchedThreads)
            {
                IResumeMessageRpc rpc3 = rpc.Pause();
                ActionItem.Schedule(CleanThreadCallbackDelegate, rpc3);
            }
        }

        internal void BindEndThread(ref MessageRpc rpc)
        {
            this.BindCore(ref rpc, false);
        }

        internal void BindThread(ref MessageRpc rpc)
        {
            this.BindCore(ref rpc, true);
        }

        private static void CleanThreadCallback(object state)
        {
            bool flag;
            ((IResumeMessageRpc) state).Resume(out flag);
        }

        internal static SynchronizationContext GetCurrentSynchronizationContext()
        {
            return SynchronizationContext.Current;
        }

        private SynchronizationContext GetSyncContext(InstanceContext instanceContext)
        {
            return (instanceContext.SynchronizationContext ?? this.context);
        }

        private void ResumeProcessing(IResumeMessageRpc resume)
        {
            bool flag;
            resume.Resume(out flag);
            if (flag)
            {
                string message = System.ServiceModel.SR.GetString("SFxMultipleCallbackFromSynchronizationContext", new object[] { this.context.GetType().ToString() });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(message));
            }
        }

        private void SynchronizationContextEndCallback(object state)
        {
            IResumeMessageRpc resume = (IResumeMessageRpc) state;
            this.ResumeProcessing(resume);
            this.GetSyncContext(resume.GetMessageInstanceContext()).OperationCompleted();
        }

        private void SynchronizationContextStartCallback(object state)
        {
            this.ResumeProcessing((IResumeMessageRpc) state);
        }

        private static Action<object> CleanThreadCallbackDelegate
        {
            get
            {
                if (cleanThreadCallback == null)
                {
                    cleanThreadCallback = new Action<object>(ThreadBehavior.CleanThreadCallback);
                }
                return cleanThreadCallback;
            }
        }

        private SendOrPostCallback ThreadAffinityEndCallbackDelegate
        {
            get
            {
                if (this.threadAffinityEndCallback == null)
                {
                    this.threadAffinityEndCallback = new SendOrPostCallback(this.SynchronizationContextEndCallback);
                }
                return this.threadAffinityEndCallback;
            }
        }

        private SendOrPostCallback ThreadAffinityStartCallbackDelegate
        {
            get
            {
                if (this.threadAffinityStartCallback == null)
                {
                    this.threadAffinityStartCallback = new SendOrPostCallback(this.SynchronizationContextStartCallback);
                }
                return this.threadAffinityStartCallback;
            }
        }
    }
}

