namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.Transactions;

    internal class MsmqReceiveContextLockManager : IDisposable
    {
        private bool disposed = false;
        private object internalStateLock = new object();
        private Dictionary<long, MsmqReceiveContext> messageExpiryMap;
        private IOThreadTimer messageExpiryTimer;
        private TimeSpan messageTimeoutInterval = TimeSpan.FromSeconds(60.0);
        private MsmqQueue queue;
        private MsmqReceiveContextSettings receiveContextSettings;
        private TransactionCompletedEventHandler transactionCompletedHandler;
        private Dictionary<Guid, List<MsmqReceiveContext>> transMessages;

        public MsmqReceiveContextLockManager(MsmqReceiveContextSettings receiveContextSettings, MsmqQueue queue)
        {
            this.queue = queue;
            this.receiveContextSettings = receiveContextSettings;
            this.messageExpiryMap = new Dictionary<long, MsmqReceiveContext>();
            this.transMessages = new Dictionary<Guid, List<MsmqReceiveContext>>();
            this.transactionCompletedHandler = new TransactionCompletedEventHandler(this.OnTransactionCompleted);
            this.messageExpiryTimer = new IOThreadTimer(new Action<object>(this.CleanupExpiredLocks), null, false);
            this.messageExpiryTimer.Set(this.messageTimeoutInterval);
        }

        private void CleanupExpiredLocks(object state)
        {
            lock (this.internalStateLock)
            {
                if (!this.disposed)
                {
                    if (this.messageExpiryMap.Count < 1)
                    {
                        this.messageExpiryTimer.Set(this.messageTimeoutInterval);
                    }
                    else
                    {
                        List<MsmqReceiveContext> list = new List<MsmqReceiveContext>();
                        try
                        {
                            foreach (KeyValuePair<long, MsmqReceiveContext> pair in this.messageExpiryMap)
                            {
                                if (DateTime.UtcNow > pair.Value.ExpiryTime)
                                {
                                    list.Add(pair.Value);
                                }
                            }
                            try
                            {
                                foreach (MsmqReceiveContext context in list)
                                {
                                    context.MarkContextExpired();
                                }
                            }
                            catch (MsmqException exception)
                            {
                                MsmqDiagnostics.ExpectedException(exception);
                            }
                        }
                        finally
                        {
                            this.messageExpiryTimer.Set(this.messageTimeoutInterval);
                        }
                    }
                }
            }
        }

        public MsmqReceiveContext CreateMsmqReceiveContext(long lookupId)
        {
            DateTime expiryTime = TimeoutHelper.Add(DateTime.UtcNow, this.receiveContextSettings.ValidityDuration);
            MsmqReceiveContext context = new MsmqReceiveContext(lookupId, expiryTime, this);
            context.Faulted += new EventHandler(this.OnReceiveContextFaulted);
            lock (this.internalStateLock)
            {
                this.messageExpiryMap.Add(lookupId, context);
            }
            return context;
        }

        public void DeleteMessage(MsmqReceiveContext receiveContext, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            long lookupId = receiveContext.LookupId;
            lock (this.internalStateLock)
            {
                if (!this.messageExpiryMap.ContainsKey(lookupId))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MessageValidityExpired", new object[] { lookupId })));
                }
                MsmqReceiveContext item = this.messageExpiryMap[lookupId];
                if (DateTime.UtcNow > item.ExpiryTime)
                {
                    item.MarkContextExpired();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MessageValidityExpired", new object[] { lookupId })));
                }
                ((ILockingQueue) this.queue).DeleteMessage(lookupId, helper.RemainingTime());
                if (Transaction.Current != null)
                {
                    List<MsmqReceiveContext> list;
                    if (!this.transMessages.TryGetValue(Transaction.Current.TransactionInformation.DistributedIdentifier, out list))
                    {
                        list = new List<MsmqReceiveContext>();
                        this.transMessages.Add(Transaction.Current.TransactionInformation.DistributedIdentifier, list);
                        Transaction.Current.TransactionCompleted += this.transactionCompletedHandler;
                    }
                    list.Add(item);
                }
                else
                {
                    this.messageExpiryMap.Remove(lookupId);
                }
            }
        }

        public void Dispose()
        {
            lock (this.internalStateLock)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.messageExpiryTimer.Cancel();
                    this.messageExpiryTimer = null;
                }
            }
        }

        private void OnReceiveContextFaulted(object sender, EventArgs e)
        {
            try
            {
                MsmqReceiveContext receiveContext = (MsmqReceiveContext) sender;
                this.UnlockMessage(receiveContext, TimeSpan.Zero);
            }
            catch (MsmqException exception)
            {
                MsmqDiagnostics.ExpectedException(exception);
            }
        }

        private void OnTransactionCompleted(object sender, TransactionEventArgs e)
        {
            e.Transaction.TransactionCompleted -= this.transactionCompletedHandler;
            lock (this.internalStateLock)
            {
                List<MsmqReceiveContext> list;
                if ((e.Transaction.TransactionInformation.Status == TransactionStatus.Committed) && this.transMessages.TryGetValue(e.Transaction.TransactionInformation.DistributedIdentifier, out list))
                {
                    foreach (MsmqReceiveContext context in list)
                    {
                        this.messageExpiryMap.Remove(context.LookupId);
                    }
                }
                this.transMessages.Remove(e.Transaction.TransactionInformation.DistributedIdentifier);
            }
        }

        private bool ReceiveContextExists(MsmqReceiveContext receiveContext)
        {
            MsmqReceiveContext context = null;
            return (this.messageExpiryMap.TryGetValue(receiveContext.LookupId, out context) && object.ReferenceEquals(receiveContext, context));
        }

        public void UnlockMessage(MsmqReceiveContext receiveContext, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            long lookupId = receiveContext.LookupId;
            lock (this.internalStateLock)
            {
                if (this.ReceiveContextExists(receiveContext))
                {
                    ((ILockingQueue) this.queue).UnlockMessage(lookupId, helper.RemainingTime());
                    this.messageExpiryMap.Remove(lookupId);
                }
            }
        }

        public MsmqQueue Queue
        {
            get
            {
                return this.queue;
            }
        }
    }
}

