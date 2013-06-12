namespace System.Threading
{
    using System;
    using System.Security;

    internal sealed class QueueUserWorkItemCallback : IThreadPoolWorkItem
    {
        private WaitCallback callback;
        internal static ContextCallback ccb = new ContextCallback(QueueUserWorkItemCallback.WaitCallback_Context);
        private ExecutionContext context;
        private object state;

        internal QueueUserWorkItemCallback(WaitCallback waitCallback, object stateObj, ExecutionContext ec)
        {
            this.callback = waitCallback;
            this.state = stateObj;
            this.context = ec;
        }

        [SecurityCritical]
        internal QueueUserWorkItemCallback(WaitCallback waitCallback, object stateObj, bool compressStack, ref StackCrawlMark stackMark)
        {
            this.callback = waitCallback;
            this.state = stateObj;
            if (compressStack && !ExecutionContext.IsFlowSuppressed())
            {
                this.context = ExecutionContext.Capture(ref stackMark, ExecutionContext.CaptureOptions.OptimizeDefaultCase | ExecutionContext.CaptureOptions.IgnoreSyncCtx);
            }
        }

        [SecurityCritical]
        void IThreadPoolWorkItem.ExecuteWorkItem()
        {
            if (this.context == null)
            {
                WaitCallback callback = this.callback;
                this.callback = null;
                callback(this.state);
            }
            else
            {
                ExecutionContext.Run(this.context, ccb, this, true);
            }
        }

        [SecurityCritical]
        void IThreadPoolWorkItem.MarkAborted(ThreadAbortException tae)
        {
        }

        internal static void WaitCallback_Context(object state)
        {
            QueueUserWorkItemCallback callback = (QueueUserWorkItemCallback) state;
            callback.callback(callback.state);
        }
    }
}

