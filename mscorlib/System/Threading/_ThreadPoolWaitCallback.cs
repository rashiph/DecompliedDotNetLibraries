namespace System.Threading
{
    using System;
    using System.Security;

    internal class _ThreadPoolWaitCallback
    {
        internal static ContextCallback _ccb = new ContextCallback(_ThreadPoolWaitCallback.WaitCallback_Context);
        private ExecutionContext _executionContext;
        protected internal _ThreadPoolWaitCallback _next;
        private object _state;
        private WaitCallback _waitCallback;

        [SecurityCritical]
        internal _ThreadPoolWaitCallback(WaitCallback waitCallback, object state, bool compressStack, ref StackCrawlMark stackMark)
        {
            this._waitCallback = waitCallback;
            this._state = state;
            if (compressStack && !ExecutionContext.IsFlowSuppressed())
            {
                this._executionContext = ExecutionContext.Capture(ref stackMark, ExecutionContext.CaptureOptions.OptimizeDefaultCase | ExecutionContext.CaptureOptions.IgnoreSyncCtx);
            }
        }

        [SecurityCritical]
        internal static bool PerformWaitCallback()
        {
            if (ThreadPoolGlobals.useNewWorkerPool)
            {
                return ThreadPoolWorkQueue.Dispatch();
            }
            int num = 0;
            _ThreadPoolWaitCallback callback = null;
            int tickCount = Environment.TickCount;
            do
            {
                int num3 = ThreadPoolGlobals.tpQueue.DeQueue(ref callback);
                if (callback == null)
                {
                    break;
                }
                ThreadPool.CompleteThreadPoolRequest((uint) num3);
                PerformWaitCallbackInternal(callback);
                num = Environment.TickCount - tickCount;
            }
            while ((num <= ThreadPoolGlobals.tpQuantum) || !ThreadPool.ShouldReturnToVm());
            return true;
        }

        [SecurityCritical]
        internal static void PerformWaitCallbackInternal(_ThreadPoolWaitCallback tpWaitCallBack)
        {
            if (tpWaitCallBack._executionContext == null)
            {
                tpWaitCallBack._waitCallback(tpWaitCallBack._state);
            }
            else
            {
                ExecutionContext.Run(tpWaitCallBack._executionContext, _ccb, tpWaitCallBack, true);
            }
        }

        internal static void WaitCallback_Context(object state)
        {
            _ThreadPoolWaitCallback callback = (_ThreadPoolWaitCallback) state;
            callback._waitCallback(callback._state);
        }
    }
}

