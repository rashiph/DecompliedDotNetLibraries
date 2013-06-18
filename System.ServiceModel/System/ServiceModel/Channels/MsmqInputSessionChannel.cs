namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;
    using System.Transactions;

    internal sealed class MsmqInputSessionChannel : InputChannel, IInputSessionChannel, IInputChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
    {
        private Transaction associatedTx;
        private int incompleteMessageCount;
        private bool receiveContextEnabled;
        private IInputSession session;
        private bool sessiongramDoomed;
        private ReceiveContext sessiongramReceiveContext;
        private int uncommittedMessageCount;

        public MsmqInputSessionChannel(MsmqInputSessionChannelListener listener, Transaction associatedTx, ReceiveContext sessiongramReceiveContext) : base(listener, new EndpointAddress(listener.Uri, new AddressHeader[0]))
        {
            this.session = new InputSession();
            this.incompleteMessageCount = 0;
            if (sessiongramReceiveContext == null)
            {
                this.receiveContextEnabled = false;
                this.associatedTx = associatedTx;
                this.associatedTx.EnlistVolatile(new TransactionEnlistment(this, this.associatedTx), EnlistmentOptions.None);
            }
            else
            {
                this.receiveContextEnabled = true;
                this.sessiongramReceiveContext = sessiongramReceiveContext;
                this.sessiongramDoomed = false;
            }
        }

        private void AbandonMessage(TimeSpan timeout)
        {
            base.ThrowIfFaulted();
            this.sessiongramDoomed = true;
        }

        public override IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
        }

        public override IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InputChannel.HelpBeginReceive(this, timeout, callback, state);
        }

        public override IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfFaulted();
            if ((CommunicationState.Closed == base.State) || (CommunicationState.Closing == base.State))
            {
                return new CompletedAsyncResult<bool, Message>(true, null, callback, state);
            }
            if (!this.receiveContextEnabled)
            {
                this.VerifyTransaction();
            }
            return base.BeginTryReceive(timeout, callback, state);
        }

        private void CompleteMessage(TimeSpan timeout)
        {
            base.ThrowIfFaulted();
            this.EnsureReceiveContextTransaction();
            Interlocked.Increment(ref this.uncommittedMessageCount);
            Interlocked.Decrement(ref this.incompleteMessageCount);
        }

        private void DetachTransaction(bool aborted)
        {
            this.associatedTx = null;
            if (aborted)
            {
                this.incompleteMessageCount += this.uncommittedMessageCount;
            }
            this.uncommittedMessageCount = 0;
        }

        public override bool EndTryReceive(IAsyncResult result, out Message message)
        {
            if (result is CompletedAsyncResult<bool, Message>)
            {
                return CompletedAsyncResult<bool, Message>.End(result, out message);
            }
            bool flag = base.EndTryReceive(result, out message);
            if ((flag && (message != null)) && this.receiveContextEnabled)
            {
                message.Properties[ReceiveContext.Name] = new MsmqSessionReceiveContext(this);
                Interlocked.Increment(ref this.incompleteMessageCount);
            }
            return flag;
        }

        private void EnsureReceiveContextTransaction()
        {
            if (Transaction.Current == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTransactionRequired")));
            }
            if (this.associatedTx == null)
            {
                this.associatedTx = Transaction.Current;
                this.associatedTx.EnlistVolatile(new ReceiveContextTransactionEnlistment(this, this.associatedTx, this.sessiongramReceiveContext), EnlistmentOptions.EnlistDuringPrepareRequired);
            }
            else
            {
                if (this.associatedTx != Transaction.Current)
                {
                    this.RollbackTransaction(null);
                    base.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqSameTransactionExpected")));
                }
                if (Transaction.Current.TransactionInformation.Status != TransactionStatus.Active)
                {
                    base.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTransactionNotActive")));
                }
            }
        }

        public void FaultChannel()
        {
            base.Fault();
        }

        protected override void OnAbort()
        {
            this.OnCloseCore(true);
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnCloseCore(false);
            return base.OnBeginClose(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.OnCloseCore(false);
            base.OnClose(timeout);
        }

        private void OnCloseCore(bool isAborting)
        {
            if (this.receiveContextEnabled)
            {
                this.OnCloseReceiveContext(isAborting);
            }
            else
            {
                this.OnCloseTransactional(isAborting);
            }
        }

        private void OnCloseReceiveContext(bool isAborting)
        {
            if (isAborting)
            {
                if (this.associatedTx != null)
                {
                    Exception exception = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqSessionChannelAbort")));
                    this.RollbackTransaction(exception);
                }
                this.sessiongramReceiveContext.Abandon(TimeSpan.MaxValue);
            }
            else if (this.TotalPendingItems > 0)
            {
                base.Fault();
                this.sessiongramReceiveContext.Abandon(TimeSpan.MaxValue);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqSessionPrematureClose")));
            }
        }

        private void OnCloseTransactional(bool isAborting)
        {
            if (isAborting)
            {
                this.RollbackTransaction(null);
            }
            else
            {
                this.VerifyTransaction();
                if (base.InternalPendingItems > 0)
                {
                    this.RollbackTransaction(null);
                    base.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqSessionMessagesNotConsumed")));
                }
            }
        }

        public override Message Receive()
        {
            return this.Receive(base.DefaultReceiveTimeout);
        }

        public override Message Receive(TimeSpan timeout)
        {
            return InputChannel.HelpReceive(this, timeout);
        }

        private void RollbackTransaction(Exception exception)
        {
            try
            {
                if (this.associatedTx.TransactionInformation.Status == TransactionStatus.Active)
                {
                    this.associatedTx.Rollback(exception);
                }
            }
            catch (TransactionAbortedException exception2)
            {
                MsmqDiagnostics.ExpectedException(exception2);
            }
            catch (ObjectDisposedException exception3)
            {
                MsmqDiagnostics.ExpectedException(exception3);
            }
        }

        public override bool TryReceive(TimeSpan timeout, out Message message)
        {
            base.ThrowIfFaulted();
            if ((CommunicationState.Closed == base.State) || (CommunicationState.Closing == base.State))
            {
                message = null;
                return true;
            }
            if (!this.receiveContextEnabled)
            {
                this.VerifyTransaction();
            }
            bool flag = base.TryReceive(timeout, out message);
            if ((flag && (message != null)) && this.receiveContextEnabled)
            {
                message.Properties[ReceiveContext.Name] = new MsmqSessionReceiveContext(this);
                Interlocked.Increment(ref this.incompleteMessageCount);
            }
            return flag;
        }

        private void VerifyTransaction()
        {
            if (base.InternalPendingItems > 0)
            {
                if (this.associatedTx != Transaction.Current)
                {
                    this.RollbackTransaction(null);
                    base.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqSameTransactionExpected")));
                }
                if (Transaction.Current.TransactionInformation.Status != TransactionStatus.Active)
                {
                    this.RollbackTransaction(null);
                    base.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTransactionNotActive")));
                }
            }
        }

        public IInputSession Session
        {
            get
            {
                return this.session;
            }
        }

        private int TotalPendingItems
        {
            get
            {
                return (base.InternalPendingItems + this.incompleteMessageCount);
            }
        }

        private class InputSession : IInputSession, ISession
        {
            private string id = ("uuid://session-gram/" + Guid.NewGuid().ToString());

            public string Id
            {
                get
                {
                    return this.id;
                }
            }
        }

        private class MsmqSessionReceiveContext : ReceiveContext
        {
            private MsmqInputSessionChannel channel;

            public MsmqSessionReceiveContext(MsmqInputSessionChannel channel)
            {
                this.channel = channel;
            }

            protected override void OnAbandon(TimeSpan timeout)
            {
                this.channel.AbandonMessage(timeout);
            }

            protected override IAsyncResult OnBeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return SessionReceiveContextAsyncResult.CreateAbandon(this, timeout, callback, state);
            }

            protected override IAsyncResult OnBeginComplete(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return SessionReceiveContextAsyncResult.CreateComplete(this, timeout, callback, state);
            }

            protected override void OnComplete(TimeSpan timeout)
            {
                this.channel.CompleteMessage(timeout);
            }

            protected override void OnEndAbandon(IAsyncResult result)
            {
                SessionReceiveContextAsyncResult.End(result);
            }

            protected override void OnEndComplete(IAsyncResult result)
            {
                SessionReceiveContextAsyncResult.End(result);
            }

            private class SessionReceiveContextAsyncResult : AsyncResult
            {
                private Transaction completionTransaction;
                private static Action<object> onAbandon;
                private static Action<object> onComplete;
                private MsmqInputSessionChannel.MsmqSessionReceiveContext receiveContext;
                private TimeoutHelper timeoutHelper;

                private SessionReceiveContextAsyncResult(MsmqInputSessionChannel.MsmqSessionReceiveContext receiveContext, TimeSpan timeout, AsyncCallback callback, object state, Action<object> target) : base(callback, state)
                {
                    this.completionTransaction = Transaction.Current;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.receiveContext = receiveContext;
                    ActionItem.Schedule(target, this);
                }

                public static IAsyncResult CreateAbandon(MsmqInputSessionChannel.MsmqSessionReceiveContext receiveContext, TimeSpan timeout, AsyncCallback callback, object state)
                {
                    if (onAbandon == null)
                    {
                        onAbandon = new Action<object>(MsmqInputSessionChannel.MsmqSessionReceiveContext.SessionReceiveContextAsyncResult.OnAbandon);
                    }
                    return new MsmqInputSessionChannel.MsmqSessionReceiveContext.SessionReceiveContextAsyncResult(receiveContext, timeout, callback, state, onAbandon);
                }

                public static IAsyncResult CreateComplete(MsmqInputSessionChannel.MsmqSessionReceiveContext receiveContext, TimeSpan timeout, AsyncCallback callback, object state)
                {
                    if (onComplete == null)
                    {
                        onComplete = new Action<object>(MsmqInputSessionChannel.MsmqSessionReceiveContext.SessionReceiveContextAsyncResult.OnComplete);
                    }
                    return new MsmqInputSessionChannel.MsmqSessionReceiveContext.SessionReceiveContextAsyncResult(receiveContext, timeout, callback, state, onComplete);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<MsmqInputSessionChannel.MsmqSessionReceiveContext.SessionReceiveContextAsyncResult>(result);
                }

                private static void OnAbandon(object parameter)
                {
                    MsmqInputSessionChannel.MsmqSessionReceiveContext.SessionReceiveContextAsyncResult result = parameter as MsmqInputSessionChannel.MsmqSessionReceiveContext.SessionReceiveContextAsyncResult;
                    Exception exception = null;
                    try
                    {
                        result.receiveContext.OnAbandon(result.timeoutHelper.RemainingTime());
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    result.Complete(false, exception);
                }

                private static void OnComplete(object parameter)
                {
                    MsmqInputSessionChannel.MsmqSessionReceiveContext.SessionReceiveContextAsyncResult result = parameter as MsmqInputSessionChannel.MsmqSessionReceiveContext.SessionReceiveContextAsyncResult;
                    Transaction current = Transaction.Current;
                    Transaction.Current = result.completionTransaction;
                    try
                    {
                        Exception exception = null;
                        try
                        {
                            result.receiveContext.OnComplete(result.timeoutHelper.RemainingTime());
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        result.Complete(false, exception);
                    }
                    finally
                    {
                        Transaction.Current = current;
                    }
                }
            }
        }

        private class ReceiveContextTransactionEnlistment : IEnlistmentNotification
        {
            private MsmqInputSessionChannel channel;
            private ReceiveContext sessiongramReceiveContext;
            private Transaction transaction;

            public ReceiveContextTransactionEnlistment(MsmqInputSessionChannel channel, Transaction transaction, ReceiveContext receiveContext)
            {
                this.channel = channel;
                this.transaction = transaction;
                this.sessiongramReceiveContext = receiveContext;
            }

            public void Commit(Enlistment enlistment)
            {
                this.channel.DetachTransaction(false);
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                if ((this.channel.TotalPendingItems > 0) || this.channel.sessiongramDoomed)
                {
                    Exception e = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqSessionChannelHasPendingItems")));
                    this.sessiongramReceiveContext.Abandon(TimeSpan.MaxValue);
                    preparingEnlistment.ForceRollback(e);
                    this.channel.Fault();
                }
                else
                {
                    Transaction current = Transaction.Current;
                    try
                    {
                        Transaction.Current = this.transaction;
                        try
                        {
                            this.sessiongramReceiveContext.Complete(TimeSpan.MaxValue);
                            preparingEnlistment.Done();
                        }
                        catch (MsmqException exception2)
                        {
                            preparingEnlistment.ForceRollback(exception2);
                            this.channel.Fault();
                        }
                    }
                    finally
                    {
                        Transaction.Current = current;
                    }
                }
            }

            public void Rollback(Enlistment enlistment)
            {
                this.channel.DetachTransaction(true);
                enlistment.Done();
            }
        }

        private class TransactionEnlistment : IEnlistmentNotification
        {
            private MsmqInputSessionChannel channel;
            private Transaction transaction;

            public TransactionEnlistment(MsmqInputSessionChannel channel, Transaction transaction)
            {
                this.channel = channel;
                this.transaction = transaction;
            }

            public void Commit(Enlistment enlistment)
            {
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                if ((this.channel.State == CommunicationState.Opened) && (this.channel.InternalPendingItems > 0))
                {
                    Exception e = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqSessionChannelsMustBeClosed")));
                    preparingEnlistment.ForceRollback(e);
                    this.channel.Fault();
                }
                else
                {
                    preparingEnlistment.Done();
                }
            }

            public void Rollback(Enlistment enlistment)
            {
                this.channel.Fault();
                enlistment.Done();
            }
        }
    }
}

