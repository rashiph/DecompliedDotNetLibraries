namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class ConcurrencyBehavior
    {
        private ConcurrencyMode mode;
        private bool supportsTransactedBatch;

        internal ConcurrencyBehavior(DispatchRuntime runtime)
        {
            this.mode = runtime.ConcurrencyMode;
            this.supportsTransactedBatch = SupportsTransactedBatch(runtime.ChannelDispatcher);
        }

        internal bool IsConcurrent(ref MessageRpc rpc)
        {
            return IsConcurrent(this.mode, rpc.Channel.HasSession, this.supportsTransactedBatch);
        }

        internal static bool IsConcurrent(ChannelDispatcher runtime, bool hasSession)
        {
            if (!SupportsTransactedBatch(runtime))
            {
                if (!hasSession)
                {
                    return true;
                }
                foreach (EndpointDispatcher dispatcher in runtime.Endpoints)
                {
                    if (dispatcher.DispatchRuntime.ConcurrencyMode != ConcurrencyMode.Single)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool IsConcurrent(ConcurrencyMode mode, bool hasSession, bool supportsTransactedBatch)
        {
            if (supportsTransactedBatch)
            {
                return false;
            }
            if (mode == ConcurrencyMode.Single)
            {
                return !hasSession;
            }
            return true;
        }

        internal void LockInstance(ref MessageRpc rpc)
        {
            if (this.mode != ConcurrencyMode.Multiple)
            {
                ConcurrencyInstanceContextFacet concurrency = rpc.InstanceContext.Concurrency;
                lock (rpc.InstanceContext.ThisLock)
                {
                    if (!concurrency.Locked)
                    {
                        concurrency.Locked = true;
                    }
                    else
                    {
                        MessageRpcWaiter waiter = new MessageRpcWaiter(rpc.Pause());
                        concurrency.EnqueueNewMessage(waiter);
                    }
                }
                if (this.mode == ConcurrencyMode.Reentrant)
                {
                    rpc.OperationContext.IsServiceReentrant = true;
                }
            }
        }

        internal static void LockInstanceAfterCallout(OperationContext operationContext)
        {
            if (operationContext != null)
            {
                InstanceContext instanceContext = operationContext.InstanceContext;
                if (operationContext.IsServiceReentrant)
                {
                    ConcurrencyInstanceContextFacet concurrency = instanceContext.Concurrency;
                    ThreadWaiter waiter = null;
                    lock (instanceContext.ThisLock)
                    {
                        if (!concurrency.Locked)
                        {
                            concurrency.Locked = true;
                        }
                        else
                        {
                            waiter = new ThreadWaiter();
                            concurrency.EnqueueCalloutMessage(waiter);
                        }
                    }
                    if (waiter != null)
                    {
                        waiter.Wait();
                    }
                }
            }
        }

        private static bool SupportsTransactedBatch(ChannelDispatcher channelDispatcher)
        {
            return (channelDispatcher.IsTransactedReceive && (channelDispatcher.MaxTransactedBatchSize > 0));
        }

        internal void UnlockInstance(ref MessageRpc rpc)
        {
            if (this.mode != ConcurrencyMode.Multiple)
            {
                UnlockInstance(rpc.InstanceContext);
            }
        }

        private static void UnlockInstance(InstanceContext instanceContext)
        {
            ConcurrencyInstanceContextFacet concurrency = instanceContext.Concurrency;
            lock (instanceContext.ThisLock)
            {
                if (concurrency.HasWaiters)
                {
                    concurrency.DequeueWaiter().Signal();
                }
                else
                {
                    concurrency.Locked = false;
                }
            }
        }

        internal static void UnlockInstanceBeforeCallout(OperationContext operationContext)
        {
            if ((operationContext != null) && operationContext.IsServiceReentrant)
            {
                UnlockInstance(operationContext.InstanceContext);
            }
        }

        internal interface IWaiter
        {
            void Signal();
        }

        private class MessageRpcWaiter : ConcurrencyBehavior.IWaiter
        {
            private IResumeMessageRpc resume;

            internal MessageRpcWaiter(IResumeMessageRpc resume)
            {
                this.resume = resume;
            }

            void ConcurrencyBehavior.IWaiter.Signal()
            {
                try
                {
                    bool flag;
                    this.resume.Resume(out flag);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        private class ThreadWaiter : ConcurrencyBehavior.IWaiter
        {
            private ManualResetEvent wait = new ManualResetEvent(false);

            void ConcurrencyBehavior.IWaiter.Signal()
            {
                this.wait.Set();
            }

            internal void Wait()
            {
                this.wait.WaitOne();
                this.wait.Close();
            }
        }
    }
}

