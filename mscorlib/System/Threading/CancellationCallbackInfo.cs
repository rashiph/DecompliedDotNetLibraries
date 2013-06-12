namespace System.Threading
{
    using System;
    using System.Security;

    internal class CancellationCallbackInfo
    {
        internal readonly Action<object> Callback;
        internal readonly System.Threading.CancellationTokenSource CancellationTokenSource;
        internal readonly object StateForCallback;
        internal readonly ExecutionContext TargetExecutionContext;
        internal readonly SynchronizationContext TargetSyncContext;

        internal CancellationCallbackInfo(Action<object> callback, object stateForCallback, SynchronizationContext targetSyncContext, ExecutionContext targetExecutionContext, System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            this.Callback = callback;
            this.StateForCallback = stateForCallback;
            this.TargetSyncContext = targetSyncContext;
            this.TargetExecutionContext = targetExecutionContext;
            this.CancellationTokenSource = cancellationTokenSource;
        }

        [SecuritySafeCritical]
        internal void ExecuteCallback()
        {
            if (this.TargetExecutionContext != null)
            {
                ExecutionContext.Run(this.TargetExecutionContext, new ContextCallback(CancellationCallbackInfo.ExecutionContextCallback), this);
            }
            else
            {
                ExecutionContextCallback(this);
            }
        }

        private static void ExecutionContextCallback(object obj)
        {
            CancellationCallbackInfo info = obj as CancellationCallbackInfo;
            info.Callback(info.StateForCallback);
        }
    }
}

