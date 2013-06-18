namespace System.Web.Services.Protocols
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web.Services.Diagnostics;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class WebClientAsyncResult : IAsyncResult
    {
        internal byte[] Buffer;
        internal WebClientProtocol ClientProtocol;
        private bool completedSynchronously;
        internal bool EndSendCalled;
        internal System.Exception Exception;
        internal object InternalAsyncState;
        private bool isCompleted;
        private ManualResetEvent manualResetEvent;
        internal WebRequest Request;
        internal WebResponse Response;
        internal Stream ResponseBufferedStream;
        internal Stream ResponseStream;
        private object userAsyncState;
        private AsyncCallback userCallback;

        internal WebClientAsyncResult(WebClientProtocol clientProtocol, object internalAsyncState, WebRequest request, AsyncCallback userCallback, object userAsyncState)
        {
            this.ClientProtocol = clientProtocol;
            this.InternalAsyncState = internalAsyncState;
            this.userAsyncState = userAsyncState;
            this.userCallback = userCallback;
            this.Request = request;
            this.completedSynchronously = true;
        }

        public void Abort()
        {
            WebRequest request = this.Request;
            if (request != null)
            {
                request.Abort();
            }
        }

        internal void CombineCompletedSynchronously(bool innerCompletedSynchronously)
        {
            this.completedSynchronously = this.completedSynchronously && innerCompletedSynchronously;
        }

        internal void Complete()
        {
            try
            {
                if (this.ResponseStream != null)
                {
                    this.ResponseStream.Close();
                    this.ResponseStream = null;
                }
                if (this.ResponseBufferedStream != null)
                {
                    this.ResponseBufferedStream.Position = 0L;
                }
            }
            catch (System.Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (this.Exception == null)
                {
                    this.Exception = exception;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Error, this, "Complete", exception);
                }
            }
            this.isCompleted = true;
            try
            {
                if (this.manualResetEvent != null)
                {
                    this.manualResetEvent.Set();
                }
            }
            catch (System.Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                if (this.Exception == null)
                {
                    this.Exception = exception2;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Error, this, "Complete", exception2);
                }
            }
            if (this.userCallback != null)
            {
                this.userCallback(this);
            }
        }

        internal void Complete(System.Exception e)
        {
            this.Exception = e;
            this.Complete();
        }

        internal WebResponse WaitForResponse()
        {
            if (!this.isCompleted)
            {
                this.AsyncWaitHandle.WaitOne();
            }
            if (this.Exception != null)
            {
                throw this.Exception;
            }
            return this.Response;
        }

        public object AsyncState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.userAsyncState;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                bool isCompleted = this.isCompleted;
                if (this.manualResetEvent == null)
                {
                    lock (this)
                    {
                        if (this.manualResetEvent == null)
                        {
                            this.manualResetEvent = new ManualResetEvent(isCompleted);
                        }
                    }
                }
                if (!isCompleted && this.isCompleted)
                {
                    this.manualResetEvent.Set();
                }
                return this.manualResetEvent;
            }
        }

        public bool CompletedSynchronously
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completedSynchronously;
            }
        }

        public bool IsCompleted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isCompleted;
            }
        }
    }
}

