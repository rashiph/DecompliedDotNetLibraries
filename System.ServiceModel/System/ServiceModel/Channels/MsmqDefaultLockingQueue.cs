namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.EnterpriseServices;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;
    using System.Transactions;

    internal class MsmqDefaultLockingQueue : MsmqQueue, ILockingQueue
    {
        private Dictionary<Guid, List<long>> dtcTransMap;
        private object internalStateLock;
        private Dictionary<long, TransactionLookupEntry> lockMap;
        private object receiveLock;
        private TransactionCompletedEventHandler transactionCompletedHandler;

        public MsmqDefaultLockingQueue(string formatName, int accessMode) : base(formatName, accessMode)
        {
            this.receiveLock = new object();
            this.lockMap = new Dictionary<long, TransactionLookupEntry>();
            this.dtcTransMap = new Dictionary<Guid, List<long>>();
            this.internalStateLock = new object();
            this.transactionCompletedHandler = new TransactionCompletedEventHandler(this.Current_TransactionCompleted);
        }

        public override void CloseQueue()
        {
            long[] numArray;
            lock (this.internalStateLock)
            {
                numArray = new long[this.lockMap.Keys.Count];
                this.lockMap.Keys.CopyTo(numArray, 0);
            }
            foreach (long num in numArray)
            {
                this.UnlockMessage(num, TimeSpan.Zero);
            }
            base.CloseQueue();
        }

        private void Current_TransactionCompleted(object sender, TransactionEventArgs e)
        {
            e.Transaction.TransactionCompleted -= this.transactionCompletedHandler;
            if (e.Transaction.TransactionInformation.Status == System.Transactions.TransactionStatus.Aborted)
            {
                List<long> list = null;
                lock (this.internalStateLock)
                {
                    if (this.dtcTransMap.TryGetValue(e.Transaction.TransactionInformation.DistributedIdentifier, out list))
                    {
                        this.dtcTransMap.Remove(e.Transaction.TransactionInformation.DistributedIdentifier);
                    }
                }
                if (list != null)
                {
                    foreach (long num in list)
                    {
                        this.TryRelockMessage(num);
                    }
                }
            }
            else if (e.Transaction.TransactionInformation.Status == System.Transactions.TransactionStatus.Committed)
            {
                List<long> list2 = null;
                lock (this.internalStateLock)
                {
                    if (this.dtcTransMap.TryGetValue(e.Transaction.TransactionInformation.DistributedIdentifier, out list2))
                    {
                        this.dtcTransMap.Remove(e.Transaction.TransactionInformation.DistributedIdentifier);
                    }
                    if (list2 != null)
                    {
                        foreach (long num2 in list2)
                        {
                            this.lockMap.Remove(num2);
                        }
                    }
                }
            }
        }

        public void DeleteMessage(long lookupId, TimeSpan timeout)
        {
            TransactionLookupEntry entry;
            if ((Transaction.Current != null) && (Transaction.Current.TransactionInformation.Status != System.Transactions.TransactionStatus.Active))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqAmbientTransactionInactive")));
            }
            lock (this.internalStateLock)
            {
                if (!this.lockMap.TryGetValue(lookupId, out entry))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MessageNotInLockedState", new object[] { lookupId })));
                }
                if (entry.MsmqInternalTransaction == null)
                {
                    this.lockMap.Remove(entry.LookupId);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MessageNotInLockedState", new object[] { lookupId })));
                }
            }
            if (Transaction.Current == null)
            {
                entry.MsmqInternalTransaction.Commit(0, 0, 0);
                lock (this.internalStateLock)
                {
                    this.lockMap.Remove(lookupId);
                    return;
                }
            }
            lock (this.receiveLock)
            {
                MsmqQueueHandle handle = base.GetHandle();
                BOID pboidReason = new BOID();
                entry.MsmqInternalTransaction.Abort(ref pboidReason, 0, 0);
                entry.MsmqInternalTransaction = null;
                using (MsmqEmptyMessage message = new MsmqEmptyMessage())
                {
                    int error = 0;
                    try
                    {
                        error = base.ReceiveByLookupIdCoreDtcTransacted(handle, lookupId, message, MsmqTransactionMode.CurrentOrThrow, 0x40000020);
                    }
                    catch (ObjectDisposedException exception)
                    {
                        MsmqDiagnostics.ExpectedException(exception);
                    }
                    if (error != 0)
                    {
                        if (MsmqQueue.IsErrorDueToStaleHandle(error))
                        {
                            base.HandleIsStale(handle);
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqCannotReacquireLock"), error));
                    }
                }
            }
            lock (this.internalStateLock)
            {
                List<long> list;
                if (!this.dtcTransMap.TryGetValue(Transaction.Current.TransactionInformation.DistributedIdentifier, out list))
                {
                    list = new List<long>();
                    this.dtcTransMap.Add(Transaction.Current.TransactionInformation.DistributedIdentifier, list);
                    Transaction.Current.TransactionCompleted += this.transactionCompletedHandler;
                }
                list.Add(lookupId);
            }
        }

        private int PeekLockCore(MsmqQueueHandle handle, MsmqInputMessage message, TimeSpan timeout)
        {
            int num = 0;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            IntPtr properties = message.Pin();
            try
            {
                bool flag = false;
                while (!flag)
                {
                    ITransaction transaction;
                    bool flag2;
                    object obj2;
                    num = UnsafeNativeMethods.MQBeginTransaction(out transaction);
                    if (num != 0)
                    {
                        return num;
                    }
                    int num3 = (TimeoutHelper.ToMilliseconds(helper.RemainingTime()) == 0) ? 0 : 100;
                Label_0045:
                    flag2 = false;
                    try
                    {
                        Monitor.Enter(obj2 = this.receiveLock, ref flag2);
                        num = UnsafeNativeMethods.MQReceiveMessage(handle.DangerousGetHandle(), num3, 0, properties, null, IntPtr.Zero, IntPtr.Zero, transaction);
                        if (num == -1072824293)
                        {
                            if (TimeoutHelper.ToMilliseconds(helper.RemainingTime()) == 0)
                            {
                                return num;
                            }
                            goto Label_0045;
                        }
                        if (num != 0)
                        {
                            BOID pboidReason = new BOID();
                            transaction.Abort(ref pboidReason, 0, 0);
                            return num;
                        }
                    }
                    finally
                    {
                        if (flag2)
                        {
                            Monitor.Exit(obj2);
                        }
                    }
                    lock (this.internalStateLock)
                    {
                        TransactionLookupEntry entry;
                        if (!this.lockMap.TryGetValue(message.LookupId.Value, out entry))
                        {
                            this.lockMap.Add(message.LookupId.Value, new TransactionLookupEntry(message.LookupId.Value, transaction));
                            flag = true;
                        }
                        else
                        {
                            entry.MsmqInternalTransaction = transaction;
                        }
                        continue;
                    }
                }
            }
            finally
            {
                message.Unpin();
            }
            return num;
        }

        public override MsmqQueue.ReceiveResult TryReceive(NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            MsmqQueueHandle handle = base.GetHandle();
            while (true)
            {
                int error = this.PeekLockCore(handle, (MsmqInputMessage) message, helper.RemainingTime());
                if (error == 0)
                {
                    return MsmqQueue.ReceiveResult.MessageReceived;
                }
                if (!MsmqQueue.IsReceiveErrorDueToInsufficientBuffer(error))
                {
                    if (error == -1072824293)
                    {
                        return MsmqQueue.ReceiveResult.Timeout;
                    }
                    if (error == -1072824312)
                    {
                        return MsmqQueue.ReceiveResult.OperationCancelled;
                    }
                    if (error == -1072824313)
                    {
                        return MsmqQueue.ReceiveResult.OperationCancelled;
                    }
                    if (MsmqQueue.IsErrorDueToStaleHandle(error))
                    {
                        base.HandleIsStale(handle);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqReceiveError", new object[] { MsmqError.GetErrorString(error) }), error));
                }
                message.GrowBuffers();
            }
        }

        private int TryRelockMessage(long lookupId)
        {
            int num = 0;
            using (MsmqEmptyMessage message = new MsmqEmptyMessage())
            {
                IntPtr properties = message.Pin();
                try
                {
                    lock (this.receiveLock)
                    {
                        MsmqQueueHandle handle = base.GetHandle();
                        lock (this.internalStateLock)
                        {
                            TransactionLookupEntry entry;
                            if (this.lockMap.TryGetValue(lookupId, out entry))
                            {
                                ITransaction transaction;
                                if (entry.MsmqInternalTransaction != null)
                                {
                                    return num;
                                }
                                num = UnsafeNativeMethods.MQBeginTransaction(out transaction);
                                if (num != 0)
                                {
                                    return num;
                                }
                                num = UnsafeNativeMethods.MQReceiveMessageByLookupId(handle, lookupId, 0x40000020, properties, null, IntPtr.Zero, transaction);
                                if (num != 0)
                                {
                                    BOID pboidReason = new BOID();
                                    transaction.Abort(ref pboidReason, 0, 0);
                                    return num;
                                }
                                entry.MsmqInternalTransaction = transaction;
                            }
                            return num;
                        }
                        return num;
                    }
                    return num;
                }
                finally
                {
                    message.Unpin();
                }
            }
            return num;
        }

        public void UnlockMessage(long lookupId, TimeSpan timeout)
        {
            lock (this.internalStateLock)
            {
                TransactionLookupEntry entry;
                if (this.lockMap.TryGetValue(lookupId, out entry))
                {
                    if (entry.MsmqInternalTransaction != null)
                    {
                        BOID pboidReason = new BOID();
                        entry.MsmqInternalTransaction.Abort(ref pboidReason, 0, 0);
                    }
                    this.lockMap.Remove(lookupId);
                }
            }
        }

        private class TransactionLookupEntry
        {
            public long LookupId;
            public ITransaction MsmqInternalTransaction;

            public TransactionLookupEntry(long lookupId, ITransaction transaction)
            {
                this.LookupId = lookupId;
                this.MsmqInternalTransaction = transaction;
            }
        }
    }
}

