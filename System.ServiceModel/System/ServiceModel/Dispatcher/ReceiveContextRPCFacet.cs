namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Transactions;

    internal class ReceiveContextRPCFacet
    {
        private static AsyncCallback handleEndComplete = Fx.ThunkCallback(new AsyncCallback(ReceiveContextRPCFacet.HandleEndComplete));
        private ReceiveContext receiveContext;

        private ReceiveContextRPCFacet(ReceiveContext receiveContext)
        {
            this.receiveContext = receiveContext;
        }

        public IAsyncResult BeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.receiveContext.BeginAbandon(timeout, callback, state);
        }

        public IAsyncResult BeginComplete(TimeSpan timeout, Transaction transaction, ChannelHandler channelHandler, AsyncCallback callback, object state)
        {
            IAsyncResult result = null;
            if (transaction != null)
            {
                using (TransactionScope scope = new TransactionScope(transaction))
                {
                    TransactionOutcomeListener.EnsureReceiveContextAbandonOnTransactionRollback(this.receiveContext, transaction, channelHandler);
                    result = this.receiveContext.BeginComplete(timeout, callback, state);
                    scope.Complete();
                    return result;
                }
            }
            return this.receiveContext.BeginComplete(timeout, callback, state);
        }

        public void Complete(ImmutableDispatchRuntime dispatchRuntime, ref MessageRpc rpc, TimeSpan timeout, Transaction transaction)
        {
            AcknowledgementCompleteCallbackState state = new AcknowledgementCompleteCallbackState {
                DispatchRuntime = dispatchRuntime,
                Rpc = rpc
            };
            IAsyncResult result = new AcknowledgementCompleteAsyncResult(this.receiveContext, timeout, ref rpc, transaction, handleEndComplete, state);
            if (result.CompletedSynchronously)
            {
                AcknowledgementCompleteAsyncResult.End(result);
            }
        }

        public static void CreateIfRequired(ImmutableDispatchRuntime dispatchRuntime, ref MessageRpc messageRpc)
        {
            if (messageRpc.Operation.ReceiveContextAcknowledgementMode != ReceiveContextAcknowledgementMode.ManualAcknowledgement)
            {
                ReceiveContext property = null;
                if (!ReceiveContext.TryGet(messageRpc.Request, out property))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxReceiveContextPropertyMissing", new object[] { typeof(ReceiveContext).Name })));
                }
                messageRpc.Request.Properties.Remove(ReceiveContext.Name);
                if ((messageRpc.Operation.ReceiveContextAcknowledgementMode == ReceiveContextAcknowledgementMode.AutoAcknowledgeOnReceive) && !messageRpc.Operation.TransactionRequired)
                {
                    AcknowledgementCompleteCallbackState state = new AcknowledgementCompleteCallbackState {
                        DispatchRuntime = dispatchRuntime,
                        Rpc = messageRpc
                    };
                    IAsyncResult result = new AcknowledgementCompleteAsyncResult(property, TimeSpan.MaxValue, ref messageRpc, null, handleEndComplete, state);
                    if (result.CompletedSynchronously)
                    {
                        AcknowledgementCompleteAsyncResult.End(result);
                    }
                }
                else
                {
                    messageRpc.ReceiveContext = new ReceiveContextRPCFacet(property);
                }
            }
        }

        public void EndAbandon(IAsyncResult result)
        {
            this.receiveContext.EndAbandon(result);
        }

        public void EndComplete(IAsyncResult result)
        {
            this.receiveContext.EndComplete(result);
        }

        private static void HandleEndComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                try
                {
                    AcknowledgementCompleteAsyncResult.End(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    AcknowledgementCompleteCallbackState asyncState = (AcknowledgementCompleteCallbackState) result.AsyncState;
                    MessageRpc rpc = asyncState.Rpc;
                    rpc.Error = exception;
                    asyncState.DispatchRuntime.ErrorBehavior.HandleError(ref rpc);
                }
            }
        }

        private class AcknowledgementCompleteAsyncResult : AsyncResult
        {
            private ChannelHandler channelHandler;
            private static AsyncCallback completeCallback = Fx.ThunkCallback(new AsyncCallback(ReceiveContextRPCFacet.AcknowledgementCompleteAsyncResult.CompleteCallback));
            private Transaction currentTransaction;
            private ReceiveContext receiveContext;
            private IResumeMessageRpc resumableRPC;

            public AcknowledgementCompleteAsyncResult(ReceiveContext receiveContext, TimeSpan timeout, ref MessageRpc rpc, Transaction transaction, AsyncCallback callback, object state) : base(callback, state)
            {
                this.receiveContext = receiveContext;
                this.currentTransaction = transaction;
                this.channelHandler = rpc.channelHandler;
                this.resumableRPC = rpc.Pause();
                bool flag = true;
                try
                {
                    bool flag2 = this.Complete(timeout);
                    flag = false;
                    if (flag2)
                    {
                        this.resumableRPC = null;
                        rpc.UnPause();
                        base.Complete(true);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        rpc.UnPause();
                    }
                }
            }

            private bool Complete(TimeSpan timeout)
            {
                IAsyncResult result = null;
                if (this.currentTransaction != null)
                {
                    using (TransactionScope scope = new TransactionScope(this.currentTransaction))
                    {
                        ReceiveContextRPCFacet.TransactionOutcomeListener.EnsureReceiveContextAbandonOnTransactionRollback(this.receiveContext, this.currentTransaction, this.channelHandler);
                        result = this.receiveContext.BeginComplete(timeout, completeCallback, this);
                        scope.Complete();
                        goto Label_006B;
                    }
                }
                result = this.receiveContext.BeginComplete(timeout, completeCallback, this);
            Label_006B:
                if (result.CompletedSynchronously)
                {
                    return HandleComplete(result);
                }
                return false;
            }

            private static void CompleteCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    bool flag = true;
                    try
                    {
                        flag = HandleComplete(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (flag)
                    {
                        ReceiveContextRPCFacet.AcknowledgementCompleteAsyncResult asyncState = (ReceiveContextRPCFacet.AcknowledgementCompleteAsyncResult) result.AsyncState;
                        asyncState.resumableRPC.Resume();
                        asyncState.Complete(false, exception);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReceiveContextRPCFacet.AcknowledgementCompleteAsyncResult>(result);
            }

            private static bool HandleComplete(IAsyncResult result)
            {
                ReceiveContextRPCFacet.AcknowledgementCompleteAsyncResult asyncState = (ReceiveContextRPCFacet.AcknowledgementCompleteAsyncResult) result.AsyncState;
                asyncState.receiveContext.EndComplete(result);
                return true;
            }
        }

        private class AcknowledgementCompleteCallbackState
        {
            public ImmutableDispatchRuntime DispatchRuntime { get; set; }

            public MessageRpc Rpc { get; set; }
        }

        private class TransactionOutcomeListener
        {
            private static AsyncCallback abandonCallback = Fx.ThunkCallback(new AsyncCallback(ReceiveContextRPCFacet.TransactionOutcomeListener.AbandonCallback));
            private ChannelHandler channelHandler;
            private ReceiveContext receiveContext;

            public TransactionOutcomeListener(ReceiveContext receiveContext, Transaction transaction, ChannelHandler handler)
            {
                this.receiveContext = receiveContext;
                transaction.TransactionCompleted += new TransactionCompletedEventHandler(this.OnTransactionComplete);
                this.channelHandler = handler;
            }

            private static void AbandonCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    CallbackState asyncState = (CallbackState) result.AsyncState;
                    try
                    {
                        asyncState.ReceiveContext.EndAbandon(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.ChannelHandler.HandleError(exception);
                    }
                }
            }

            public static void EnsureReceiveContextAbandonOnTransactionRollback(ReceiveContext receiveContext, Transaction transaction, ChannelHandler channelHandler)
            {
                new ReceiveContextRPCFacet.TransactionOutcomeListener(receiveContext, transaction, channelHandler);
            }

            private void OnTransactionComplete(object sender, TransactionEventArgs e)
            {
                if (e.Transaction.TransactionInformation.Status == TransactionStatus.Aborted)
                {
                    try
                    {
                        CallbackState state = new CallbackState {
                            ChannelHandler = this.channelHandler,
                            ReceiveContext = this.receiveContext
                        };
                        IAsyncResult result = this.receiveContext.BeginAbandon(TimeSpan.MaxValue, abandonCallback, state);
                        if (result.CompletedSynchronously)
                        {
                            this.receiveContext.EndAbandon(result);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.channelHandler.HandleError(exception);
                    }
                }
            }

            private class CallbackState
            {
                public System.ServiceModel.Dispatcher.ChannelHandler ChannelHandler { get; set; }

                public System.ServiceModel.Channels.ReceiveContext ReceiveContext { get; set; }
            }
        }
    }
}

