namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class FloodAsyncResult : AsyncResult
    {
        private bool doneAdding;
        private Exception exception;
        private bool isCompleted;
        private bool offNode;
        private List<IAsyncResult> pending;
        private PeerNeighborManager pnm;
        private Dictionary<IAsyncResult, IPeerNeighbor> results;
        private bool shouldCallComplete;
        private object thisLock;
        private TimeoutHelper timeoutHelper;

        public event EventHandler OnMessageSent;

        public FloodAsyncResult(PeerNeighborManager owner, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
        {
            this.pending = new List<IAsyncResult>();
            this.results = new Dictionary<IAsyncResult, IPeerNeighbor>();
            this.thisLock = new object();
            this.pnm = owner;
            this.timeoutHelper = new TimeoutHelper(timeout);
        }

        public void AddResult(IAsyncResult result, IPeerNeighbor neighbor)
        {
            lock (this.ThisLock)
            {
                this.results.Add(result, neighbor);
            }
        }

        private void CompleteOp(bool sync)
        {
            this.OnMessageSent(this, EventArgs.Empty);
            base.Complete(sync, this.exception);
        }

        public void End()
        {
            if (!this.doneAdding || !this.shouldCallComplete)
            {
                throw Fx.AssertAndThrow("Unexpected end!");
            }
            if (!this.isCompleted)
            {
                if (!TimeoutHelper.WaitOne(base.AsyncWaitHandle, this.timeoutHelper.RemainingTime()))
                {
                    if (!this.offNode)
                    {
                        try
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                            this.exception = exception;
                        }
                    }
                    lock (this.ThisLock)
                    {
                        if (this.isCompleted)
                        {
                            return;
                        }
                        this.isCompleted = true;
                    }
                    this.CompleteOp(false);
                }
                AsyncResult.End<FloodAsyncResult>(this);
            }
        }

        public void MarkEnd(bool success)
        {
            bool flag = false;
            try
            {
                lock (this.ThisLock)
                {
                    foreach (IAsyncResult result in this.pending)
                    {
                        this.OnSendComplete(result);
                    }
                    this.pending.Clear();
                    this.doneAdding = true;
                    this.shouldCallComplete = success;
                    if (this.results.Count == 0)
                    {
                        this.isCompleted = true;
                        flag = true;
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    this.CompleteOp(true);
                }
            }
        }

        internal void OnSendComplete(IAsyncResult result)
        {
            bool flag = false;
            IPeerNeighbor neighbor = null;
            bool flag2 = false;
            if (!this.isCompleted)
            {
                Message asyncState = (Message) result.AsyncState;
                lock (this.ThisLock)
                {
                    if (this.isCompleted)
                    {
                        return;
                    }
                    if (!this.results.TryGetValue(result, out neighbor))
                    {
                        if (this.doneAdding)
                        {
                            throw Fx.AssertAndThrow("IAsyncResult is un-accounted for.");
                        }
                        this.pending.Add(result);
                        return;
                    }
                    this.results.Remove(result);
                    try
                    {
                        if (!result.CompletedSynchronously)
                        {
                            neighbor.EndSend(result);
                            this.offNode = true;
                            UtilityExtension.OnEndSend(neighbor, this);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            flag2 = true;
                            throw;
                        }
                        Exception exception2 = PeerFlooderBase<Message, UtilityInfo>.CloseNeighborIfKnownException(this.pnm, exception, neighbor);
                        if (((exception2 != null) && this.doneAdding) && !this.shouldCallComplete)
                        {
                            throw;
                        }
                        if (this.exception == null)
                        {
                            this.exception = exception2;
                        }
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    finally
                    {
                        if (((asyncState != null) && !result.CompletedSynchronously) && !flag2)
                        {
                            asyncState.Close();
                        }
                    }
                    if (((this.results.Count == 0) && this.doneAdding) && this.shouldCallComplete)
                    {
                        this.isCompleted = true;
                        flag = true;
                    }
                }
                if (flag && this.shouldCallComplete)
                {
                    this.CompleteOp(false);
                }
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}

