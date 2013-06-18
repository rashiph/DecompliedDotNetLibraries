namespace System.Transactions.Oletx
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    internal sealed class OletxResourceManager
    {
        internal Hashtable enlistmentHashtable;
        internal OletxTransactionManager oletxTransactionManager;
        private bool recoveryCompleteCalledByApplication;
        internal ArrayList reenlistList;
        internal ArrayList reenlistPendingList;
        internal Thread reenlistThread;
        internal Timer reenlistThreadTimer;
        internal Guid resourceManagerIdentifier;
        internal IResourceManagerShim resourceManagerShim = null;
        internal static Hashtable volatileEnlistmentHashtable = new Hashtable();

        internal OletxResourceManager(OletxTransactionManager transactionManager, Guid resourceManagerIdentifier)
        {
            this.oletxTransactionManager = transactionManager;
            this.resourceManagerIdentifier = resourceManagerIdentifier;
            this.enlistmentHashtable = new Hashtable();
            this.reenlistList = new ArrayList();
            this.reenlistPendingList = new ArrayList();
            this.reenlistThreadTimer = null;
            this.reenlistThread = null;
            this.recoveryCompleteCalledByApplication = false;
        }

        internal bool CallProxyReenlistComplete()
        {
            bool flag = false;
            if (this.RecoveryCompleteCalledByApplication)
            {
                IResourceManagerShim resourceManagerShim = null;
                try
                {
                    try
                    {
                        resourceManagerShim = this.ResourceManagerShim;
                        if (resourceManagerShim != null)
                        {
                            resourceManagerShim.ReenlistComplete();
                            flag = true;
                        }
                    }
                    catch (COMException exception)
                    {
                        if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN == exception.ErrorCode) || (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE == exception.ErrorCode))
                        {
                            flag = false;
                            if (DiagnosticTrace.Verbose)
                            {
                                ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                            }
                            return flag;
                        }
                        if (System.Transactions.Oletx.NativeMethods.XACT_E_RECOVERYALREADYDONE != exception.ErrorCode)
                        {
                            OletxTransactionManager.ProxyException(exception);
                            throw;
                        }
                        return true;
                    }
                    return flag;
                }
                finally
                {
                    resourceManagerShim = null;
                }
            }
            return true;
        }

        internal OletxEnlistment EnlistDurable(OletxTransaction oletxTransaction, bool canDoSinglePhase, IEnlistmentNotificationInternal enlistmentNotification, EnlistmentOptions enlistmentOptions)
        {
            IResourceManagerShim resourceManagerShim = null;
            IPhase0EnlistmentShim shim2 = null;
            IEnlistmentShim enlistmentShim = null;
            IntPtr zero = IntPtr.Zero;
            bool flag3 = false;
            bool flag2 = false;
            OletxEnlistment target = new OletxEnlistment(canDoSinglePhase, enlistmentNotification, oletxTransaction.RealTransaction.TxGuid, enlistmentOptions, this, oletxTransaction);
            bool flag = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if ((enlistmentOptions & EnlistmentOptions.EnlistDuringPrepareRequired) != EnlistmentOptions.None)
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        oletxTransaction.RealTransaction.IncrementUndecidedEnlistments();
                        flag2 = true;
                    }
                }
                lock (target)
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        resourceManagerShim = this.ResourceManagerShim;
                        if (resourceManagerShim == null)
                        {
                            throw TransactionManagerCommunicationException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                        }
                        if ((enlistmentOptions & EnlistmentOptions.EnlistDuringPrepareRequired) != EnlistmentOptions.None)
                        {
                            zero = HandleTable.AllocHandle(target);
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try
                            {
                            }
                            finally
                            {
                                oletxTransaction.RealTransaction.TransactionShim.Phase0Enlist(zero, out shim2);
                                flag3 = true;
                            }
                            target.Phase0EnlistmentShim = shim2;
                        }
                        target.phase1Handle = HandleTable.AllocHandle(target);
                        resourceManagerShim.Enlist(oletxTransaction.RealTransaction.TransactionShim, target.phase1Handle, out enlistmentShim);
                        target.EnlistmentShim = enlistmentShim;
                    }
                    catch (COMException exception)
                    {
                        if (System.Transactions.Oletx.NativeMethods.XACT_E_TOOMANY_ENLISTMENTS == exception.ErrorCode)
                        {
                            throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("OletxTooManyEnlistments"), exception);
                        }
                        OletxTransactionManager.ProxyException(exception);
                        throw;
                    }
                    finally
                    {
                        if (target.EnlistmentShim == null)
                        {
                            if ((zero != IntPtr.Zero) && !flag3)
                            {
                                HandleTable.FreeHandle(zero);
                            }
                            if (target.phase1Handle != IntPtr.Zero)
                            {
                                HandleTable.FreeHandle(target.phase1Handle);
                            }
                        }
                    }
                }
                flag = true;
            }
            finally
            {
                if ((!flag && ((enlistmentOptions & EnlistmentOptions.EnlistDuringPrepareRequired) != EnlistmentOptions.None)) && flag2)
                {
                    oletxTransaction.RealTransaction.DecrementUndecidedEnlistments();
                }
            }
            return target;
        }

        internal void RecoveryComplete()
        {
            Timer reenlistThreadTimer = null;
            this.RecoveryCompleteCalledByApplication = true;
            try
            {
                lock (this.reenlistList)
                {
                    lock (this)
                    {
                        if ((this.reenlistList.Count == 0) && (this.reenlistPendingList.Count == 0))
                        {
                            if (this.reenlistThreadTimer != null)
                            {
                                reenlistThreadTimer = this.reenlistThreadTimer;
                                this.reenlistThreadTimer = null;
                            }
                            if (!this.CallProxyReenlistComplete())
                            {
                                this.StartReenlistThread();
                            }
                        }
                        else
                        {
                            this.StartReenlistThread();
                        }
                    }
                }
            }
            finally
            {
                if (reenlistThreadTimer != null)
                {
                    reenlistThreadTimer.Dispose();
                }
            }
        }

        internal OletxEnlistment Reenlist(int prepareInfoLength, byte[] prepareInfo, IEnlistmentNotificationInternal enlistmentNotification)
        {
            OletxRecoveryInformation information;
            OletxTransactionOutcome notKnownYet = OletxTransactionOutcome.NotKnownYet;
            OletxTransactionStatus xactStatus = OletxTransactionStatus.OLETX_TRANSACTION_STATUS_NONE;
            MemoryStream serializationStream = new MemoryStream(prepareInfo);
            IFormatter formatter = new BinaryFormatter();
            try
            {
                information = formatter.Deserialize(serializationStream) as OletxRecoveryInformation;
            }
            catch (SerializationException exception2)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("InvalidArgument"), "prepareInfo", exception2);
            }
            if (information == null)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("InvalidArgument"), "prepareInfo");
            }
            byte[] b = new byte[0x10];
            for (int i = 0; i < 0x10; i++)
            {
                b[i] = information.proxyRecoveryInformation[i + 0x10];
            }
            Guid guid = new Guid(b);
            if (guid != this.resourceManagerIdentifier)
            {
                throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("ResourceManagerIdDoesNotMatchRecoveryInformation"), null);
            }
            IResourceManagerShim resourceManagerShim = null;
            try
            {
                resourceManagerShim = this.ResourceManagerShim;
                if (resourceManagerShim == null)
                {
                    throw new COMException(System.Transactions.SR.GetString("DtcTransactionManagerUnavailable"), System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN);
                }
                resourceManagerShim.Reenlist(Convert.ToUInt32(information.proxyRecoveryInformation.Length, CultureInfo.InvariantCulture), information.proxyRecoveryInformation, out notKnownYet);
                if (OletxTransactionOutcome.Committed == notKnownYet)
                {
                    xactStatus = OletxTransactionStatus.OLETX_TRANSACTION_STATUS_COMMITTED;
                }
                else if (OletxTransactionOutcome.Aborted == notKnownYet)
                {
                    xactStatus = OletxTransactionStatus.OLETX_TRANSACTION_STATUS_ABORTED;
                }
                else
                {
                    xactStatus = OletxTransactionStatus.OLETX_TRANSACTION_STATUS_PREPARED;
                    this.StartReenlistThread();
                }
            }
            catch (COMException exception)
            {
                if (System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception.ErrorCode)
                {
                    throw;
                }
                xactStatus = OletxTransactionStatus.OLETX_TRANSACTION_STATUS_PREPARED;
                this.ResourceManagerShim = null;
                this.StartReenlistThread();
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                }
            }
            finally
            {
                resourceManagerShim = null;
            }
            return new OletxEnlistment(enlistmentNotification, xactStatus, information.proxyRecoveryInformation, this);
        }

        internal void ReenlistThread(object state)
        {
            int count = 0;
            bool flag = false;
            OletxEnlistment enlistment = null;
            IResourceManagerShim resourceManagerShim = null;
            Timer reenlistThreadTimer = null;
            bool flag2 = false;
            OletxResourceManager manager = (OletxResourceManager) state;
            try
            {
                if (DiagnosticTrace.Information)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxResourceManager.ReenlistThread");
                }
                lock (manager)
                {
                    resourceManagerShim = manager.ResourceManagerShim;
                    reenlistThreadTimer = manager.reenlistThreadTimer;
                    manager.reenlistThreadTimer = null;
                    manager.reenlistThread = Thread.CurrentThread;
                }
                if (resourceManagerShim != null)
                {
                    lock (manager.reenlistList)
                    {
                        count = manager.reenlistList.Count;
                    }
                    flag = false;
                    while ((!flag && (count > 0)) && (resourceManagerShim != null))
                    {
                        lock (manager.reenlistList)
                        {
                            enlistment = null;
                            count--;
                            if (manager.reenlistList.Count == 0)
                            {
                                flag = true;
                            }
                            else
                            {
                                enlistment = manager.reenlistList[0] as OletxEnlistment;
                                if (enlistment == null)
                                {
                                    if (DiagnosticTrace.Critical)
                                    {
                                        InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                                    }
                                    throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("InternalError"), null);
                                }
                                manager.reenlistList.RemoveAt(0);
                                object obj7 = enlistment;
                                lock (obj7)
                                {
                                    if (OletxEnlistment.OletxEnlistmentState.Done == enlistment.State)
                                    {
                                        enlistment = null;
                                    }
                                    else if (OletxEnlistment.OletxEnlistmentState.Prepared != enlistment.State)
                                    {
                                        manager.reenlistList.Add(enlistment);
                                        enlistment = null;
                                    }
                                }
                            }
                        }
                        if (enlistment != null)
                        {
                            OletxTransactionOutcome notKnownYet = OletxTransactionOutcome.NotKnownYet;
                            try
                            {
                                if (enlistment.ProxyPrepareInfoByteArray == null)
                                {
                                    if (DiagnosticTrace.Critical)
                                    {
                                        InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                                    }
                                    throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("InternalError"), null);
                                }
                                resourceManagerShim.Reenlist((uint) enlistment.ProxyPrepareInfoByteArray.Length, enlistment.ProxyPrepareInfoByteArray, out notKnownYet);
                                if (notKnownYet == OletxTransactionOutcome.NotKnownYet)
                                {
                                    object obj5 = enlistment;
                                    lock (obj5)
                                    {
                                        if (OletxEnlistment.OletxEnlistmentState.Done == enlistment.State)
                                        {
                                            enlistment = null;
                                        }
                                        else
                                        {
                                            lock (manager.reenlistList)
                                            {
                                                manager.reenlistList.Add(enlistment);
                                                enlistment = null;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (COMException exception)
                            {
                                if (System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception.ErrorCode)
                                {
                                    throw;
                                }
                                if (DiagnosticTrace.Verbose)
                                {
                                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                                }
                                if (System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN == exception.ErrorCode)
                                {
                                    manager.ResourceManagerShim = null;
                                    resourceManagerShim = manager.ResourceManagerShim;
                                }
                            }
                            if (enlistment != null)
                            {
                                object obj3 = enlistment;
                                lock (obj3)
                                {
                                    if (OletxEnlistment.OletxEnlistmentState.Done != enlistment.State)
                                    {
                                        lock (manager.reenlistList)
                                        {
                                            manager.reenlistPendingList.Add(enlistment);
                                        }
                                        if (OletxTransactionOutcome.Committed != notKnownYet)
                                        {
                                            if (OletxTransactionOutcome.Aborted != notKnownYet)
                                            {
                                                if (DiagnosticTrace.Critical)
                                                {
                                                    InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                                                }
                                                throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("InternalError"), null);
                                            }
                                            enlistment.State = OletxEnlistment.OletxEnlistmentState.Aborting;
                                            if (DiagnosticTrace.Verbose)
                                            {
                                                EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), enlistment.EnlistmentTraceId, NotificationCall.Rollback);
                                            }
                                            enlistment.EnlistmentNotification.Rollback(enlistment);
                                        }
                                        else
                                        {
                                            enlistment.State = OletxEnlistment.OletxEnlistmentState.Committing;
                                            if (DiagnosticTrace.Verbose)
                                            {
                                                EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), enlistment.EnlistmentTraceId, NotificationCall.Commit);
                                            }
                                            enlistment.EnlistmentNotification.Commit(enlistment);
                                        }
                                    }
                                    else
                                    {
                                        enlistment = null;
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                }
                resourceManagerShim = null;
                lock (manager.reenlistList)
                {
                    lock (manager)
                    {
                        count = manager.reenlistList.Count;
                        if ((0 >= count) && (0 >= manager.reenlistPendingList.Count))
                        {
                            if (!manager.CallProxyReenlistComplete())
                            {
                                manager.reenlistThreadTimer = reenlistThreadTimer;
                                if (!reenlistThreadTimer.Change(0x2710, -1))
                                {
                                    throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("UnexpectedTimerFailure"), null);
                                }
                            }
                            else
                            {
                                flag2 = true;
                            }
                        }
                        else
                        {
                            manager.reenlistThreadTimer = reenlistThreadTimer;
                            if (!reenlistThreadTimer.Change(0x2710, -1))
                            {
                                throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("UnexpectedTimerFailure"), null);
                            }
                        }
                        manager.reenlistThread = null;
                    }
                    if (DiagnosticTrace.Information)
                    {
                        MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxResourceManager.ReenlistThread");
                    }
                }
            }
            finally
            {
                resourceManagerShim = null;
                if (flag2 && (reenlistThreadTimer != null))
                {
                    reenlistThreadTimer.Dispose();
                }
            }
        }

        internal void RemoveFromReenlistPending(OletxEnlistment enlistment)
        {
            lock (this.reenlistList)
            {
                this.reenlistPendingList.Remove(enlistment);
                lock (this)
                {
                    if (((this.reenlistThreadTimer != null) && (this.reenlistList.Count == 0)) && ((this.reenlistPendingList.Count == 0) && !this.reenlistThreadTimer.Change(0, -1)))
                    {
                        throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("UnexpectedTimerFailure"), null);
                    }
                }
            }
        }

        internal void StartReenlistThread()
        {
            lock (this)
            {
                if ((this.reenlistThreadTimer == null) && (this.reenlistThread == null))
                {
                    this.reenlistThreadTimer = new Timer(new TimerCallback(this.ReenlistThread), this, 10, -1);
                }
            }
        }

        public void TMDown()
        {
            this.StartReenlistThread();
        }

        internal void TMDownFromInternalRM(OletxTransactionManager oletxTM)
        {
            OletxEnlistment enlistment = null;
            IDictionaryEnumerator enumerator = null;
            Hashtable hashtable = null;
            this.ResourceManagerShim = null;
            lock (this.enlistmentHashtable.SyncRoot)
            {
                hashtable = (Hashtable) this.enlistmentHashtable.Clone();
            }
            enumerator = hashtable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enlistment = enumerator.Value as OletxEnlistment;
                if (enlistment != null)
                {
                    enlistment.TMDownFromInternalRM(oletxTM);
                }
            }
        }

        internal bool RecoveryCompleteCalledByApplication
        {
            get
            {
                return this.recoveryCompleteCalledByApplication;
            }
            set
            {
                this.recoveryCompleteCalledByApplication = value;
            }
        }

        internal IResourceManagerShim ResourceManagerShim
        {
            get
            {
                IResourceManagerShim resourceManagerShim = null;
                if (this.resourceManagerShim == null)
                {
                    lock (this)
                    {
                        if (this.resourceManagerShim == null)
                        {
                            this.oletxTransactionManager.dtcTransactionManagerLock.AcquireReaderLock(-1);
                            try
                            {
                                Guid resourceManagerIdentifier = this.resourceManagerIdentifier;
                                IntPtr zero = IntPtr.Zero;
                                RuntimeHelpers.PrepareConstrainedRegions();
                                try
                                {
                                    zero = HandleTable.AllocHandle(this);
                                    this.oletxTransactionManager.DtcTransactionManager.ProxyShimFactory.CreateResourceManager(resourceManagerIdentifier, zero, out resourceManagerShim);
                                }
                                finally
                                {
                                    if ((resourceManagerShim == null) && (zero != IntPtr.Zero))
                                    {
                                        HandleTable.FreeHandle(zero);
                                    }
                                }
                            }
                            catch (COMException exception2)
                            {
                                if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception2.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE != exception2.ErrorCode))
                                {
                                    throw;
                                }
                                resourceManagerShim = null;
                                if (DiagnosticTrace.Verbose)
                                {
                                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception2);
                                }
                            }
                            catch (TransactionException exception3)
                            {
                                COMException innerException = exception3.InnerException as COMException;
                                if (innerException == null)
                                {
                                    throw;
                                }
                                if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != innerException.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE != innerException.ErrorCode))
                                {
                                    throw;
                                }
                                resourceManagerShim = null;
                                if (DiagnosticTrace.Verbose)
                                {
                                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception3);
                                }
                            }
                            finally
                            {
                                this.oletxTransactionManager.dtcTransactionManagerLock.ReleaseReaderLock();
                            }
                            Thread.MemoryBarrier();
                            this.resourceManagerShim = resourceManagerShim;
                        }
                    }
                }
                return this.resourceManagerShim;
            }
            set
            {
                this.resourceManagerShim = value;
            }
        }
    }
}

