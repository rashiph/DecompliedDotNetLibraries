namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class MsmqSubqueueLockingQueue : MsmqQueue, ILockingQueue
    {
        private bool disposed;
        private string hostname;
        private TimeSpan lockCollectionInterval;
        private IOThreadTimer lockCollectionTimer;
        private MsmqQueue lockQueueForMove;
        private MsmqQueue lockQueueForReceive;
        private string lockQueueName;
        private const string LockSubqueuePrefix = "lock_";
        private MsmqQueue mainQueueForMove;
        private object timerLock;
        private bool validHostName;

        public MsmqSubqueueLockingQueue(string formatName, string hostname, int accessMode) : base(formatName, accessMode)
        {
            this.lockCollectionInterval = TimeSpan.FromMinutes(5.0);
            this.timerLock = new object();
            if (string.Compare(hostname, string.Empty, StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.validHostName = TryGetHostName(formatName, out hostname);
            }
            else
            {
                this.validHostName = true;
            }
            this.disposed = false;
            this.lockQueueName = base.formatName + ";" + GenerateLockQueueName();
            this.lockQueueForReceive = new MsmqQueue(this.lockQueueName, 1, 1);
            this.lockQueueForMove = new MsmqQueue(this.lockQueueName, 4);
            this.mainQueueForMove = new MsmqQueue(base.formatName, 4);
            this.lockCollectionTimer = new IOThreadTimer(new Action<object>(this.OnCollectionTimer), null, false);
            if (string.Compare(hostname, "localhost", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.hostname = null;
            }
            else
            {
                this.hostname = hostname;
            }
        }

        public override void CloseQueue()
        {
            lock (this.timerLock)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.lockCollectionTimer.Cancel();
                    this.lockCollectionTimer = null;
                }
            }
            this.CollectLocks(this.lockQueueForReceive);
            this.mainQueueForMove.CloseQueue();
            this.lockQueueForMove.CloseQueue();
            this.lockQueueForReceive.CloseQueue();
            base.CloseQueue();
        }

        private void CollectLocks(MsmqQueue lockQueue)
        {
            MsmqQueue.ReceiveResult messageReceived = MsmqQueue.ReceiveResult.MessageReceived;
            while (messageReceived == MsmqQueue.ReceiveResult.MessageReceived)
            {
                using (MsmqMessageLookupId id = new MsmqMessageLookupId())
                {
                    try
                    {
                        messageReceived = lockQueue.TryPeek(id, TimeSpan.FromSeconds(0.0));
                        if (messageReceived == MsmqQueue.ReceiveResult.MessageReceived)
                        {
                            lockQueue.TryMoveMessage(id.lookupId.Value, this.mainQueueForMove, MsmqTransactionMode.None);
                        }
                    }
                    catch (MsmqException exception)
                    {
                        MsmqDiagnostics.ExpectedException(exception);
                        messageReceived = MsmqQueue.ReceiveResult.Unknown;
                    }
                    continue;
                }
            }
        }

        public void DeleteMessage(long lookupId, TimeSpan timeout)
        {
            MsmqQueue.MoveReceiveResult result;
            IPostRollbackErrorStrategy strategy = new SimplePostRollbackErrorStrategy(lookupId);
            do
            {
                using (MsmqEmptyMessage message = new MsmqEmptyMessage())
                {
                    result = this.lockQueueForReceive.TryReceiveByLookupId(lookupId, message, MsmqTransactionMode.CurrentOrNone);
                }
            }
            while ((result == MsmqQueue.MoveReceiveResult.MessageLockedUnderTransaction) && strategy.AnotherTryNeeded());
            if (result != MsmqQueue.MoveReceiveResult.Succeeded)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqReceiveContextMessageNotReceived", new object[] { lookupId.ToString(CultureInfo.InvariantCulture) })));
            }
        }

        internal void EnsureLockQueuesOpen()
        {
            int num = 0;
            while (true)
            {
                try
                {
                    this.lockQueueForReceive.EnsureOpen();
                    break;
                }
                catch (MsmqException exception)
                {
                    if (num >= 3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                    MsmqDiagnostics.ExpectedException(exception);
                }
                this.lockQueueForReceive.Dispose();
                this.lockQueueForMove.Dispose();
                this.lockQueueName = base.formatName + ";" + GenerateLockQueueName();
                this.lockQueueForReceive = new MsmqQueue(this.lockQueueName, 1, 1);
                this.lockQueueForMove = new MsmqQueue(this.lockQueueName, 4);
                num++;
            }
            this.lockQueueForMove.EnsureOpen();
        }

        private static string GenerateLockQueueName()
        {
            string str = Guid.NewGuid().ToString();
            return ("lock_" + str.Substring(str.Length - 8, 8));
        }

        private void OnCollectionTimer(object state)
        {
            lock (this.timerLock)
            {
                if (!this.disposed)
                {
                    List<string> list;
                    if (this.TryEnumerateSubqueues(out list))
                    {
                        foreach (string str in list)
                        {
                            MsmqQueue queue;
                            if (str.StartsWith("lock_", StringComparison.OrdinalIgnoreCase) && this.TryOpenLockQueueForCollection(str, out queue))
                            {
                                this.CollectLocks(queue);
                            }
                        }
                    }
                    this.lockCollectionTimer.Set(this.lockCollectionInterval);
                }
            }
        }

        internal override MsmqQueueHandle OpenQueue()
        {
            if (!this.validHostName)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqOpenError", new object[] { MsmqError.GetErrorString(-1072824288) }), -1072824288));
            }
            this.EnsureLockQueuesOpen();
            this.mainQueueForMove.EnsureOpen();
            this.OnCollectionTimer(null);
            return base.OpenQueue();
        }

        private bool TryEnumerateSubqueues(out List<string> subqueues)
        {
            subqueues = new List<string>();
            int[] numArray = new int[1];
            UnsafeNativeMethods.MQMSGPROPS mqmsgprops = new UnsafeNativeMethods.MQMSGPROPS();
            UnsafeNativeMethods.MQPROPVARIANT mqpropvariant = new UnsafeNativeMethods.MQPROPVARIANT();
            GCHandle handle = GCHandle.Alloc(null, GCHandleType.Pinned);
            GCHandle handle2 = GCHandle.Alloc(null, GCHandleType.Pinned);
            GCHandle handle3 = GCHandle.Alloc(null, GCHandleType.Pinned);
            mqmsgprops.status = IntPtr.Zero;
            mqmsgprops.count = 1;
            numArray[0] = 0x1b;
            mqpropvariant.vt = 1;
            try
            {
                handle.Target = mqmsgprops;
                handle2.Target = numArray;
                handle3.Target = mqpropvariant;
                mqmsgprops.variants = handle3.AddrOfPinnedObject();
                mqmsgprops.ids = handle2.AddrOfPinnedObject();
                if (UnsafeNativeMethods.MQMgmtGetInfo(this.hostname, "queue=" + base.formatName, handle.AddrOfPinnedObject()) == 0)
                {
                    UnsafeNativeMethods.MQPROPVARIANT mqpropvariant2 = (UnsafeNativeMethods.MQPROPVARIANT) Marshal.PtrToStructure(mqmsgprops.variants, typeof(UnsafeNativeMethods.MQPROPVARIANT));
                    IntPtr[] destination = new IntPtr[mqpropvariant2.stringArraysValue.count];
                    Marshal.Copy(mqpropvariant2.stringArraysValue.stringArrays, destination, 0, mqpropvariant2.stringArraysValue.count);
                    for (int i = 0; i < mqpropvariant2.stringArraysValue.count; i++)
                    {
                        subqueues.Add(Marshal.PtrToStringUni(destination[i]));
                        UnsafeNativeMethods.MQFreeMemory(destination[i]);
                    }
                    UnsafeNativeMethods.MQFreeMemory(mqpropvariant2.stringArraysValue.stringArrays);
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                handle2.Target = null;
                handle.Target = null;
                handle3.Target = null;
            }
            return true;
        }

        private static bool TryGetHostName(string formatName, out string hostName)
        {
            string str = "DIRECT=";
            string str2 = "TCP:";
            string str3 = "OS:";
            hostName = null;
            if (!formatName.StartsWith(str, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            string str4 = formatName.Substring(str.Length, formatName.Length - str.Length);
            int startIndex = str4.IndexOf(':') + 1;
            string str5 = str4.Substring(startIndex, str4.IndexOf('\\') - startIndex);
            if (str4.StartsWith(str2, StringComparison.OrdinalIgnoreCase))
            {
                hostName = str5;
                return true;
            }
            if (!str4.StartsWith(str3, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (str5.Equals("."))
            {
                hostName = "localhost";
            }
            else
            {
                hostName = str5;
            }
            return true;
        }

        private bool TryOpenLockQueueForCollection(string subqueueName, out MsmqQueue lockQueue)
        {
            lockQueue = null;
            string formatName = base.formatName + ";" + subqueueName;
            int accessMode = 1;
            int shareMode = 1;
            try
            {
                int error = 0;
                if (MsmqQueue.IsQueueOpenable(formatName, accessMode, shareMode, out error))
                {
                    lockQueue = new MsmqQueue(formatName, accessMode, shareMode);
                    lockQueue.EnsureOpen();
                }
                else
                {
                    if ((error != -1072824311) && (error != -1072824317))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqOpenError", new object[] { MsmqError.GetErrorString(error) }), error));
                    }
                    return false;
                }
            }
            catch (MsmqException)
            {
                return false;
            }
            return true;
        }

        public override MsmqQueue.ReceiveResult TryReceive(NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode)
        {
            MsmqQueue.MoveReceiveResult result3;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            bool flag = false;
            long lookupId = 0L;
            while (!flag)
            {
                using (MsmqMessageLookupId id = new MsmqMessageLookupId())
                {
                    MsmqQueue.ReceiveResult result = base.TryPeek(id, helper.RemainingTime());
                    if (result != MsmqQueue.ReceiveResult.MessageReceived)
                    {
                        return result;
                    }
                    lookupId = id.lookupId.Value;
                }
                try
                {
                    if (base.TryMoveMessage(lookupId, this.lockQueueForMove, MsmqTransactionMode.None) == MsmqQueue.MoveReceiveResult.Succeeded)
                    {
                        flag = true;
                    }
                    continue;
                }
                catch (MsmqException exception)
                {
                    MsmqDiagnostics.ExpectedException(exception);
                    continue;
                }
            }
            try
            {
                result3 = this.lockQueueForReceive.TryReceiveByLookupId(lookupId, message, MsmqTransactionMode.None, 0x40000010);
            }
            catch (MsmqException exception2)
            {
                this.UnlockMessage(lookupId, TimeSpan.Zero);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2);
            }
            if (result3 == MsmqQueue.MoveReceiveResult.Succeeded)
            {
                return MsmqQueue.ReceiveResult.MessageReceived;
            }
            this.UnlockMessage(lookupId, TimeSpan.Zero);
            return MsmqQueue.ReceiveResult.OperationCancelled;
        }

        public void UnlockMessage(long lookupId, TimeSpan timeout)
        {
            MsmqQueue.MoveReceiveResult result;
            IPostRollbackErrorStrategy strategy = new SimplePostRollbackErrorStrategy(lookupId);
            do
            {
                result = this.lockQueueForReceive.TryMoveMessage(lookupId, this.mainQueueForMove, MsmqTransactionMode.None);
            }
            while ((result == MsmqQueue.MoveReceiveResult.MessageLockedUnderTransaction) && strategy.AnotherTryNeeded());
            if (result != MsmqQueue.MoveReceiveResult.Succeeded)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqReceiveContextMessageNotMoved", new object[] { lookupId.ToString(CultureInfo.InvariantCulture) })));
            }
        }

        public MsmqQueue LockQueueForReceive
        {
            get
            {
                return this.lockQueueForReceive;
            }
        }

        private class MsmqMessageLookupId : NativeMsmqMessage
        {
            public NativeMsmqMessage.LongProperty lookupId;

            public MsmqMessageLookupId() : base(1)
            {
                this.lookupId = new NativeMsmqMessage.LongProperty(this, 60);
            }
        }
    }
}

