namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    internal abstract class TraceAsyncResult : AsyncResult
    {
        private static Action<AsyncCallback, IAsyncResult> waitResultCallback = new Action<AsyncCallback, IAsyncResult>(TraceAsyncResult.DoCallback);

        protected TraceAsyncResult(AsyncCallback callback, object state) : base(callback, state)
        {
            if (TraceUtility.MessageFlowTracingOnly)
            {
                this.CallbackActivity = ServiceModelActivity.CreateLightWeightAsyncActivity(Trace.CorrelationManager.ActivityId);
                base.VirtualCallback = waitResultCallback;
            }
            else if (DiagnosticUtility.ShouldUseActivity)
            {
                this.CallbackActivity = ServiceModelActivity.Current;
                if (this.CallbackActivity != null)
                {
                    base.VirtualCallback = waitResultCallback;
                }
            }
        }

        private static void DoCallback(AsyncCallback callback, IAsyncResult result)
        {
            if (result is TraceAsyncResult)
            {
                TraceAsyncResult result2 = result as TraceAsyncResult;
                if (TraceUtility.MessageFlowTracingOnly)
                {
                    Trace.CorrelationManager.ActivityId = result2.CallbackActivity.Id;
                    result2.CallbackActivity = null;
                }
                using (ServiceModelActivity.BoundOperation(result2.CallbackActivity))
                {
                    callback(result);
                }
            }
        }

        public ServiceModelActivity CallbackActivity { get; private set; }
    }
}

