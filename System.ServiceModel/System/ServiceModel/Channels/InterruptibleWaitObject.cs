namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class InterruptibleWaitObject
    {
        private bool aborted;
        private CommunicationObject communicationObject;
        private ManualResetEvent handle;
        private bool set;
        private int syncWaiters;
        private object thisLock;
        private bool throwTimeoutByDefault;

        private event WaitAsyncResult.AbortHandler Aborted;

        private event WaitAsyncResult.AbortHandler Faulted;

        private event WaitAsyncResult.SignaledHandler Signaled;

        public InterruptibleWaitObject(bool signaled) : this(signaled, true)
        {
        }

        public InterruptibleWaitObject(bool signaled, bool throwTimeoutByDefault)
        {
            this.thisLock = new object();
            this.throwTimeoutByDefault = true;
            this.set = signaled;
            this.throwTimeoutByDefault = throwTimeoutByDefault;
        }

        public void Abort(CommunicationObject communicationObject)
        {
            if (communicationObject == null)
            {
                throw Fx.AssertAndThrow("Argument communicationObject cannot be null.");
            }
            lock (this.thisLock)
            {
                if (this.aborted)
                {
                    return;
                }
                this.communicationObject = communicationObject;
                this.aborted = true;
                this.InternalSet();
            }
            WaitAsyncResult.AbortHandler aborted = this.Aborted;
            if (aborted != null)
            {
                aborted(communicationObject);
            }
        }

        public IAsyncResult BeginTryWait(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginWait(timeout, false, callback, state);
        }

        public IAsyncResult BeginWait(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginWait(timeout, this.throwTimeoutByDefault, callback, state);
        }

        public IAsyncResult BeginWait(TimeSpan timeout, bool throwTimeoutException, AsyncCallback callback, object state)
        {
            Exception exception = null;
            lock (this.thisLock)
            {
                if (!this.set)
                {
                    WaitAsyncResult result = new WaitAsyncResult(timeout, throwTimeoutException, callback, state);
                    this.Aborted += new WaitAsyncResult.AbortHandler(result.OnAborted);
                    this.Faulted += new WaitAsyncResult.AbortHandler(result.OnFaulted);
                    this.Signaled += new WaitAsyncResult.SignaledHandler(result.OnSignaled);
                    result.Begin();
                    return result;
                }
                if (this.communicationObject != null)
                {
                    exception = this.GetException();
                }
            }
            if (exception != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return new CompletedAsyncResult(callback, state);
        }

        public bool EndTryWait(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
                return true;
            }
            return WaitAsyncResult.End(result);
        }

        public void EndWait(IAsyncResult result)
        {
            this.EndTryWait(result);
        }

        public void Fault(CommunicationObject communicationObject)
        {
            if (communicationObject == null)
            {
                throw Fx.AssertAndThrow("Argument communicationObject cannot be null.");
            }
            lock (this.thisLock)
            {
                if (this.aborted)
                {
                    return;
                }
                this.communicationObject = communicationObject;
                this.aborted = false;
                this.InternalSet();
            }
            WaitAsyncResult.AbortHandler faulted = this.Faulted;
            if (faulted != null)
            {
                faulted(communicationObject);
            }
        }

        private Exception GetException()
        {
            CommunicationObject communicationObject = this.communicationObject;
            if (!this.aborted)
            {
                return this.communicationObject.GetTerminalException();
            }
            return this.communicationObject.CreateAbortedException();
        }

        private void InternalSet()
        {
            lock (this.thisLock)
            {
                this.set = true;
                if (this.handle != null)
                {
                    this.handle.Set();
                }
            }
        }

        public void Reset()
        {
            lock (this.thisLock)
            {
                this.communicationObject = null;
                this.aborted = false;
                this.set = false;
                if (this.handle != null)
                {
                    this.handle.Reset();
                }
            }
        }

        public void Set()
        {
            this.InternalSet();
            WaitAsyncResult.SignaledHandler signaled = this.Signaled;
            if (signaled != null)
            {
                signaled();
            }
        }

        public bool Wait(TimeSpan timeout)
        {
            return this.Wait(timeout, this.throwTimeoutByDefault);
        }

        public bool Wait(TimeSpan timeout, bool throwTimeoutException)
        {
            lock (this.thisLock)
            {
                if (this.set)
                {
                    if (this.communicationObject != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.GetException());
                    }
                    return true;
                }
                if (this.handle == null)
                {
                    this.handle = new ManualResetEvent(false);
                }
                this.syncWaiters++;
            }
            try
            {
                if (!TimeoutHelper.WaitOne(this.handle, timeout))
                {
                    if (throwTimeoutException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnOperation", new object[] { timeout })));
                    }
                    return false;
                }
            }
            finally
            {
                lock (this.thisLock)
                {
                    this.syncWaiters--;
                    if (this.syncWaiters == 0)
                    {
                        this.handle.Close();
                        this.handle = null;
                    }
                }
            }
            if (this.communicationObject != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.GetException());
            }
            return true;
        }
    }
}

