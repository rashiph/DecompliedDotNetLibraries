namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class ReliableOutputConnection
    {
        private OperationWithTimeoutBeginCallback beginSendAckRequestedHandler;
        private System.ServiceModel.Channels.BeginSendHandler beginSendHandler;
        private bool closed;
        private OperationEndCallback endSendAckRequestedHandler;
        private System.ServiceModel.Channels.EndSendHandler endSendHandler;
        public ComponentFaultedHandler Faulted;
        private UniqueId id;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private object mutex = new object();
        public ComponentExceptionHandler OnException;
        private static AsyncCallback onSendRetriesComplete = Fx.ThunkCallback(new AsyncCallback(ReliableOutputConnection.OnSendRetriesComplete));
        private static AsyncCallback onSendRetryComplete = Fx.ThunkCallback(new AsyncCallback(ReliableOutputConnection.OnSendRetryComplete));
        private ReliableMessagingVersion reliableMessagingVersion;
        private OperationWithTimeoutCallback sendAckRequestedHandler;
        private Guard sendGuard = new Guard(0x7fffffff);
        private System.ServiceModel.Channels.SendHandler sendHandler;
        private static Action<object> sendRetries = new Action<object>(ReliableOutputConnection.SendRetries);
        private TimeSpan sendTimeout;
        private InterruptibleWaitObject shutdownHandle = new InterruptibleWaitObject(false);
        private TransmissionStrategy strategy;
        private bool terminated;

        public ReliableOutputConnection(UniqueId id, int maxTransferWindowSize, System.ServiceModel.Channels.MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, TimeSpan initialRtt, bool requestAcks, TimeSpan sendTimeout)
        {
            this.id = id;
            this.messageVersion = messageVersion;
            this.reliableMessagingVersion = reliableMessagingVersion;
            this.sendTimeout = sendTimeout;
            this.strategy = new TransmissionStrategy(reliableMessagingVersion, initialRtt, maxTransferWindowSize, requestAcks, id);
            this.strategy.RetryTimeoutElapsed = new RetryHandler(this.OnRetryTimeoutElapsed);
            this.strategy.OnException = new ComponentExceptionHandler(this.RaiseOnException);
        }

        public void Abort(ChannelBase channel)
        {
            this.sendGuard.Abort();
            this.shutdownHandle.Abort(channel);
            this.strategy.Abort(channel);
        }

        public bool AddMessage(Message message, TimeSpan timeout, object state)
        {
            return this.InternalAddMessage(message, timeout, state, false);
        }

        public IAsyncResult BeginAddMessage(Message message, TimeSpan timeout, object state, AsyncCallback callback, object asyncState)
        {
            return new AddAsyncResult(message, false, timeout, state, this, callback, asyncState);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool flag = false;
            lock (this.ThisLock)
            {
                flag = !this.closed;
                this.closed = true;
            }
            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] { flag ? new OperationWithTimeoutBeginCallback(this.BeginCompleteTransfer) : null, new OperationWithTimeoutBeginCallback(this.shutdownHandle.BeginWait), new OperationWithTimeoutBeginCallback(this.sendGuard.BeginClose), this.beginSendAckRequestedHandler };
            OperationEndCallback[] endOperations = new OperationEndCallback[] { flag ? new OperationEndCallback(this.EndCompleteTransfer) : null, new OperationEndCallback(this.shutdownHandle.EndWait), new OperationEndCallback(this.sendGuard.EndClose), this.endSendAckRequestedHandler };
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
        }

        private IAsyncResult BeginCompleteTransfer(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                Message message = Message.CreateMessage(this.MessageVersion, "http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage");
                message.Properties.AllowOutputBatching = false;
                return new AddAsyncResult(message, true, timeout, null, this, callback, state);
            }
            if (this.reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Unsupported version.");
            }
            if (this.strategy.SetLast())
            {
                this.shutdownHandle.Set();
                return new AlreadyCompletedTransferAsyncResult(callback, state);
            }
            return this.beginSendAckRequestedHandler(timeout, callback, state);
        }

        public bool CheckForTermination()
        {
            return this.strategy.DoneTransmitting;
        }

        public void Close(TimeSpan timeout)
        {
            bool flag = false;
            lock (this.ThisLock)
            {
                flag = !this.closed;
                this.closed = true;
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (flag)
            {
                this.CompleteTransfer(helper.RemainingTime());
            }
            this.shutdownHandle.Wait(helper.RemainingTime());
            this.sendGuard.Close(helper.RemainingTime());
            this.strategy.Close();
        }

        private void CompleteSendRetries(IAsyncResult result)
        {
            do
            {
                this.endSendHandler(result);
                this.sendGuard.Exit();
                this.strategy.DequeuePending();
                if (!this.sendGuard.Enter())
                {
                    break;
                }
                MessageAttemptInfo messageInfoForRetry = this.strategy.GetMessageInfoForRetry(true);
                if (messageInfoForRetry.Message == null)
                {
                    this.sendGuard.Exit();
                    this.OnTransferComplete();
                    return;
                }
                result = this.beginSendHandler(messageInfoForRetry, this.sendTimeout, true, onSendRetriesComplete, this);
            }
            while (result.CompletedSynchronously);
        }

        private void CompleteSendRetry(IAsyncResult result)
        {
            try
            {
                this.endSendHandler(result);
            }
            finally
            {
                this.sendGuard.Exit();
            }
        }

        private void CompleteTransfer(TimeSpan timeout)
        {
            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                Message message = Message.CreateMessage(this.MessageVersion, "http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage");
                message.Properties.AllowOutputBatching = false;
                this.InternalAddMessage(message, timeout, null, true);
            }
            else
            {
                if (this.reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
                {
                    throw Fx.AssertAndThrow("Unsupported version.");
                }
                if (this.strategy.SetLast())
                {
                    this.shutdownHandle.Set();
                }
                else
                {
                    this.sendAckRequestedHandler(timeout);
                }
            }
        }

        public bool EndAddMessage(IAsyncResult result)
        {
            return AddAsyncResult.End(result);
        }

        public void EndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
            this.strategy.Close();
        }

        private void EndCompleteTransfer(IAsyncResult result)
        {
            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                AddAsyncResult.End(result);
            }
            else
            {
                if (this.reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
                {
                    throw Fx.AssertAndThrow("Unsupported version.");
                }
                AlreadyCompletedTransferAsyncResult result2 = result as AlreadyCompletedTransferAsyncResult;
                if (result2 != null)
                {
                    result2.End();
                }
                else
                {
                    this.endSendAckRequestedHandler(result);
                }
            }
        }

        public void Fault(ChannelBase channel)
        {
            this.sendGuard.Abort();
            this.shutdownHandle.Fault(channel);
            this.strategy.Fault(channel);
        }

        private bool InternalAddMessage(Message message, TimeSpan timeout, object state, bool isLast)
        {
            MessageAttemptInfo info;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            try
            {
                if (isLast)
                {
                    if (state != null)
                    {
                        throw Fx.AssertAndThrow("The isLast overload does not take a state.");
                    }
                    info = this.strategy.AddLast(message, helper.RemainingTime(), null);
                }
                else if (!this.strategy.Add(message, helper.RemainingTime(), state, out info))
                {
                    return false;
                }
            }
            catch (TimeoutException)
            {
                if (isLast)
                {
                    this.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.id, System.ServiceModel.SR.GetString("SequenceTerminatedAddLastToWindowTimedOut"), null));
                }
                throw;
            }
            catch (Exception exception)
            {
                if (!Fx.IsFatal(exception))
                {
                    this.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.id, System.ServiceModel.SR.GetString("SequenceTerminatedUnknownAddToWindowError"), null));
                }
                throw;
            }
            if (this.sendGuard.Enter())
            {
                try
                {
                    this.sendHandler(info, helper.RemainingTime(), false);
                }
                catch (QuotaExceededException)
                {
                    this.RaiseFault(null, SequenceTerminatedFault.CreateQuotaExceededFault(this.id));
                    throw;
                }
                finally
                {
                    this.sendGuard.Exit();
                }
            }
            return true;
        }

        public bool IsFinalAckConsistent(SequenceRangeCollection ranges)
        {
            return this.strategy.IsFinalAckConsistent(ranges);
        }

        private void OnRetryTimeoutElapsed(MessageAttemptInfo attemptInfo)
        {
            if (this.sendGuard.Enter())
            {
                IAsyncResult result = this.beginSendHandler(attemptInfo, this.sendTimeout, true, onSendRetryComplete, this);
                if (result.CompletedSynchronously)
                {
                    this.CompleteSendRetry(result);
                }
            }
        }

        private static void OnSendRetriesComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableOutputConnection asyncState = (ReliableOutputConnection) result.AsyncState;
                try
                {
                    asyncState.CompleteSendRetries(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.RaiseOnException(exception);
                }
            }
        }

        private static void OnSendRetryComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableOutputConnection asyncState = (ReliableOutputConnection) result.AsyncState;
                try
                {
                    asyncState.CompleteSendRetry(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.RaiseOnException(exception);
                }
            }
        }

        private void OnTransferComplete()
        {
            this.strategy.DequeuePending();
            if (this.strategy.DoneTransmitting)
            {
                this.Terminate();
            }
        }

        public void ProcessTransferred(SequenceRangeCollection ranges, int quotaRemaining)
        {
            bool flag;
            bool flag2;
            this.strategy.ProcessAcknowledgement(ranges, out flag, out flag2);
            if (!flag && !flag2)
            {
                if (this.strategy.ProcessTransferred(ranges, quotaRemaining))
                {
                    ActionItem.Schedule(sendRetries, this);
                }
                else
                {
                    this.OnTransferComplete();
                }
            }
            else
            {
                WsrmFault fault = new InvalidAcknowledgementFault(this.id, ranges);
                this.RaiseFault(fault.CreateException(), fault);
            }
        }

        public void ProcessTransferred(long transferred, SequenceRangeCollection ranges, int quotaRemaining)
        {
            bool flag;
            bool flag2;
            if (transferred < 0L)
            {
                throw Fx.AssertAndThrow("Argument transferred must be a valid sequence number or 0 for protocol messages.");
            }
            this.strategy.ProcessAcknowledgement(ranges, out flag, out flag2);
            if (!flag && ((transferred == 0L) || ranges.Contains(transferred)))
            {
                if ((transferred > 0L) && this.strategy.ProcessTransferred(transferred, quotaRemaining))
                {
                    ActionItem.Schedule(sendRetries, this);
                }
                else
                {
                    this.OnTransferComplete();
                }
            }
            else
            {
                WsrmFault fault = new InvalidAcknowledgementFault(this.id, ranges);
                this.RaiseFault(fault.CreateException(), fault);
            }
        }

        private void RaiseFault(Exception faultException, WsrmFault fault)
        {
            ComponentFaultedHandler faulted = this.Faulted;
            if (faulted != null)
            {
                faulted(faultException, fault);
            }
        }

        private void RaiseOnException(Exception exception)
        {
            ComponentExceptionHandler onException = this.OnException;
            if (onException != null)
            {
                onException(exception);
            }
        }

        private void SendRetries()
        {
            IAsyncResult result = null;
            if (this.sendGuard.Enter())
            {
                MessageAttemptInfo messageInfoForRetry = this.strategy.GetMessageInfoForRetry(false);
                if (messageInfoForRetry.Message != null)
                {
                    result = this.beginSendHandler(messageInfoForRetry, this.sendTimeout, true, onSendRetriesComplete, this);
                }
                if (result != null)
                {
                    if (result.CompletedSynchronously)
                    {
                        this.CompleteSendRetries(result);
                    }
                }
                else
                {
                    this.sendGuard.Exit();
                    this.OnTransferComplete();
                }
            }
        }

        private static void SendRetries(object state)
        {
            ReliableOutputConnection connection = (ReliableOutputConnection) state;
            try
            {
                connection.SendRetries();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                connection.RaiseOnException(exception);
            }
        }

        public void Terminate()
        {
            lock (this.ThisLock)
            {
                if (this.terminated)
                {
                    return;
                }
                this.terminated = true;
            }
            this.shutdownHandle.Set();
        }

        public OperationWithTimeoutBeginCallback BeginSendAckRequestedHandler
        {
            set
            {
                this.beginSendAckRequestedHandler = value;
            }
        }

        public System.ServiceModel.Channels.BeginSendHandler BeginSendHandler
        {
            set
            {
                this.beginSendHandler = value;
            }
        }

        public bool Closed
        {
            get
            {
                return this.closed;
            }
        }

        public OperationEndCallback EndSendAckRequestedHandler
        {
            set
            {
                this.endSendAckRequestedHandler = value;
            }
        }

        public System.ServiceModel.Channels.EndSendHandler EndSendHandler
        {
            set
            {
                this.endSendHandler = value;
            }
        }

        public long Last
        {
            get
            {
                return this.strategy.Last;
            }
        }

        private System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public OperationWithTimeoutCallback SendAckRequestedHandler
        {
            set
            {
                this.sendAckRequestedHandler = value;
            }
        }

        public System.ServiceModel.Channels.SendHandler SendHandler
        {
            set
            {
                this.sendHandler = value;
            }
        }

        public TransmissionStrategy Strategy
        {
            get
            {
                return this.strategy;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.mutex;
            }
        }

        private sealed class AddAsyncResult : AsyncResult
        {
            private static AsyncCallback addCompleteStatic = Fx.ThunkCallback(new AsyncCallback(ReliableOutputConnection.AddAsyncResult.AddComplete));
            private ReliableOutputConnection connection;
            private bool isLast;
            private static AsyncCallback sendCompleteStatic = Fx.ThunkCallback(new AsyncCallback(ReliableOutputConnection.AddAsyncResult.SendComplete));
            private TimeoutHelper timeoutHelper;
            private bool validAdd;

            public AddAsyncResult(Message message, bool isLast, TimeSpan timeout, object state, ReliableOutputConnection connection, AsyncCallback callback, object asyncState) : base(callback, asyncState)
            {
                IAsyncResult result;
                this.connection = connection;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.isLast = isLast;
                bool flag = false;
                try
                {
                    if (isLast)
                    {
                        if (state != null)
                        {
                            throw Fx.AssertAndThrow("The isLast overload does not take a state.");
                        }
                        result = this.connection.strategy.BeginAddLast(message, this.timeoutHelper.RemainingTime(), state, addCompleteStatic, this);
                    }
                    else
                    {
                        result = this.connection.strategy.BeginAdd(message, this.timeoutHelper.RemainingTime(), state, addCompleteStatic, this);
                    }
                }
                catch (TimeoutException)
                {
                    if (isLast)
                    {
                        this.connection.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.connection.id, System.ServiceModel.SR.GetString("SequenceTerminatedAddLastToWindowTimedOut"), null));
                    }
                    throw;
                }
                catch (Exception exception)
                {
                    if (!Fx.IsFatal(exception))
                    {
                        this.connection.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.connection.id, System.ServiceModel.SR.GetString("SequenceTerminatedUnknownAddToWindowError"), null));
                    }
                    throw;
                }
                if (result.CompletedSynchronously)
                {
                    flag = this.CompleteAdd(result);
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private static void AddComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ReliableOutputConnection.AddAsyncResult asyncState = (ReliableOutputConnection.AddAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.CompleteAdd(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (flag || (exception != null))
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private bool CompleteAdd(IAsyncResult result)
            {
                MessageAttemptInfo attemptInfo = new MessageAttemptInfo();
                this.validAdd = true;
                try
                {
                    if (this.isLast)
                    {
                        attemptInfo = this.connection.strategy.EndAddLast(result);
                    }
                    else if (!this.connection.strategy.EndAdd(result, out attemptInfo))
                    {
                        this.validAdd = false;
                        return true;
                    }
                }
                catch (TimeoutException)
                {
                    if (this.isLast)
                    {
                        this.connection.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.connection.id, System.ServiceModel.SR.GetString("SequenceTerminatedAddLastToWindowTimedOut"), null));
                    }
                    throw;
                }
                catch (Exception exception)
                {
                    if (!Fx.IsFatal(exception))
                    {
                        this.connection.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.connection.id, System.ServiceModel.SR.GetString("SequenceTerminatedUnknownAddToWindowError"), null));
                    }
                    throw;
                }
                if (this.connection.sendGuard.Enter())
                {
                    bool flag = true;
                    try
                    {
                        result = this.connection.beginSendHandler(attemptInfo, this.timeoutHelper.RemainingTime(), false, sendCompleteStatic, this);
                        flag = false;
                    }
                    catch (QuotaExceededException)
                    {
                        this.connection.RaiseFault(null, SequenceTerminatedFault.CreateQuotaExceededFault(this.connection.id));
                        throw;
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.connection.sendGuard.Exit();
                        }
                    }
                }
                else
                {
                    return true;
                }
                if (result.CompletedSynchronously)
                {
                    this.CompleteSend(result);
                    return true;
                }
                return false;
            }

            private void CompleteSend(IAsyncResult result)
            {
                try
                {
                    this.connection.endSendHandler(result);
                }
                catch (QuotaExceededException)
                {
                    this.connection.RaiseFault(null, SequenceTerminatedFault.CreateQuotaExceededFault(this.connection.id));
                    throw;
                }
                finally
                {
                    this.connection.sendGuard.Exit();
                }
            }

            public static bool End(IAsyncResult result)
            {
                AsyncResult.End<ReliableOutputConnection.AddAsyncResult>(result);
                return ((ReliableOutputConnection.AddAsyncResult) result).validAdd;
            }

            private static void SendComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ReliableOutputConnection.AddAsyncResult asyncState = (ReliableOutputConnection.AddAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.CompleteSend(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }
        }

        private class AlreadyCompletedTransferAsyncResult : CompletedAsyncResult
        {
            public AlreadyCompletedTransferAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
            }

            public void End()
            {
                AsyncResult.End<ReliableOutputConnection.AlreadyCompletedTransferAsyncResult>(this);
            }
        }
    }
}

