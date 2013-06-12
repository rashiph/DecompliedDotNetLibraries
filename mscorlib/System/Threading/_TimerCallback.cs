namespace System.Threading
{
    using System;
    using System.Security;

    internal class _TimerCallback
    {
        internal static ContextCallback _ccb = new ContextCallback(_TimerCallback.TimerCallback_Context);
        private ExecutionContext _executionContext;
        private object _state;
        private TimerCallback _timerCallback;

        [SecurityCritical]
        internal _TimerCallback(TimerCallback timerCallback, object state, ref StackCrawlMark stackMark)
        {
            this._timerCallback = timerCallback;
            this._state = state;
            if (!ExecutionContext.IsFlowSuppressed())
            {
                this._executionContext = ExecutionContext.Capture(ref stackMark, ExecutionContext.CaptureOptions.OptimizeDefaultCase | ExecutionContext.CaptureOptions.IgnoreSyncCtx);
            }
        }

        [SecurityCritical]
        internal static void PerformTimerCallback(object state)
        {
            _TimerCallback callback = (_TimerCallback) state;
            if (callback._executionContext == null)
            {
                callback._timerCallback(callback._state);
            }
            else
            {
                using (ExecutionContext context = callback._executionContext.CreateCopy())
                {
                    ExecutionContext.Run(context, _ccb, callback, true);
                }
            }
        }

        internal static void TimerCallback_Context(object state)
        {
            _TimerCallback callback = (_TimerCallback) state;
            callback._timerCallback(callback._state);
        }
    }
}

