namespace System.ServiceModel.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;

    public sealed class WorkflowHostingResponseContext
    {
        private WorkflowOperationContext context;
        private object[] outputs;
        private AsyncWaitHandle responseWaitHandle;
        private object returnValue;

        internal WorkflowHostingResponseContext()
        {
            this.responseWaitHandle = new AsyncWaitHandle(EventResetMode.AutoReset);
        }

        internal WorkflowHostingResponseContext(WorkflowOperationContext context)
        {
            this.context = context;
        }

        internal IAsyncResult BeginGetResponse(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return GetResponseAsyncResult.Create(this, timeout, callback, state);
        }

        internal object EndGetResponse(IAsyncResult result, out object[] outputs)
        {
            return GetResponseAsyncResult.End(result, out outputs);
        }

        private object GetResponse(out object[] outputs)
        {
            if (this.returnValue is Exception)
            {
                throw FxTrace.Exception.AsError((Exception) this.returnValue);
            }
            outputs = this.outputs;
            return this.returnValue;
        }

        public void SendResponse(object returnValue, object[] outputs)
        {
            this.returnValue = returnValue;
            this.outputs = outputs ?? EmptyArray.Allocate(0);
            if (this.responseWaitHandle != null)
            {
                this.responseWaitHandle.Set();
            }
            else if (this.returnValue is Exception)
            {
                this.context.SendFault((Exception) this.returnValue);
            }
            else
            {
                this.context.SendReply(this.returnValue, this.outputs);
            }
        }

        private class GetResponseAsyncResult : AsyncResult
        {
            private WorkflowHostingResponseContext context;
            private static Action<object, TimeoutException> handleEndWait = new Action<object, TimeoutException>(WorkflowHostingResponseContext.GetResponseAsyncResult.HandleEndWait);

            private GetResponseAsyncResult(WorkflowHostingResponseContext context, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.context = context;
                if (context.responseWaitHandle.WaitAsync(handleEndWait, this, timeout))
                {
                    base.Complete(true);
                }
            }

            public static WorkflowHostingResponseContext.GetResponseAsyncResult Create(WorkflowHostingResponseContext context, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new WorkflowHostingResponseContext.GetResponseAsyncResult(context, timeout, callback, state);
            }

            public static object End(IAsyncResult result, out object[] outputs)
            {
                return AsyncResult.End<WorkflowHostingResponseContext.GetResponseAsyncResult>(result).context.GetResponse(out outputs);
            }

            private static void HandleEndWait(object state, TimeoutException e)
            {
                ((WorkflowHostingResponseContext.GetResponseAsyncResult) state).Complete(false, e);
            }
        }
    }
}

