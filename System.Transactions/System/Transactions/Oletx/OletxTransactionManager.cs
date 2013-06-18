namespace System.Transactions.Oletx
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    internal class OletxTransactionManager
    {
        private static object classSyncObject;
        private TransactionOptions configuredTransactionOptions = new TransactionOptions();
        private System.Transactions.Oletx.DtcTransactionManager dtcTransactionManager;
        internal ReaderWriterLock dtcTransactionManagerLock;
        internal OletxInternalResourceManager internalResourceManager;
        private IsolationLevel isolationLevelProperty;
        private string nodeNameField;
        internal static volatile bool processingTmDown = false;
        internal static IDtcProxyShimFactory proxyShimFactory = null;
        internal static Hashtable resourceManagerHashTable;
        internal static ReaderWriterLock resourceManagerHashTableLock;
        internal static EventWaitHandle shimWaitHandle = null;
        private TimeSpan timeoutProperty;

        internal OletxTransactionManager(string nodeName)
        {
            lock (ClassSyncObject)
            {
                if (proxyShimFactory == null)
                {
                    if (System.Transactions.Oletx.NativeMethods.GetNotificationFactory(ShimWaitHandle.SafeWaitHandle, out proxyShimFactory) != 0)
                    {
                        throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("UnableToGetNotificationShimFactory"), null);
                    }
                    ThreadPool.UnsafeRegisterWaitForSingleObject(ShimWaitHandle, new WaitOrTimerCallback(OletxTransactionManager.ShimNotificationCallback), null, -1, false);
                }
            }
            this.dtcTransactionManagerLock = new ReaderWriterLock();
            this.nodeNameField = nodeName;
            if ((this.nodeNameField != null) && (this.nodeNameField.Length == 0))
            {
                this.nodeNameField = null;
            }
            if (DiagnosticTrace.Verbose)
            {
                DistributedTransactionManagerCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.GetType(), this.nodeNameField);
            }
            this.configuredTransactionOptions.IsolationLevel = this.isolationLevelProperty = TransactionManager.DefaultIsolationLevel;
            this.configuredTransactionOptions.Timeout = this.timeoutProperty = TransactionManager.DefaultTimeout;
            this.internalResourceManager = new OletxInternalResourceManager(this);
            this.dtcTransactionManagerLock.AcquireWriterLock(-1);
            try
            {
                this.dtcTransactionManager = new System.Transactions.Oletx.DtcTransactionManager(this.nodeNameField, this);
            }
            finally
            {
                this.dtcTransactionManagerLock.ReleaseWriterLock();
            }
            if (resourceManagerHashTable == null)
            {
                resourceManagerHashTable = new Hashtable(2);
                resourceManagerHashTableLock = new ReaderWriterLock();
            }
        }

        internal static OletxTransactionIsolationLevel ConvertIsolationLevel(IsolationLevel isolationLevel)
        {
            switch (isolationLevel)
            {
                case IsolationLevel.Serializable:
                    return OletxTransactionIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;

                case IsolationLevel.RepeatableRead:
                    return OletxTransactionIsolationLevel.ISOLATIONLEVEL_REPEATABLEREAD;

                case IsolationLevel.ReadCommitted:
                    return OletxTransactionIsolationLevel.ISOLATIONLEVEL_CURSORSTABILITY;

                case IsolationLevel.ReadUncommitted:
                    return OletxTransactionIsolationLevel.ISOLATIONLEVEL_READUNCOMMITTED;

                case IsolationLevel.Chaos:
                    return OletxTransactionIsolationLevel.ISOLATIONLEVEL_CHAOS;

                case IsolationLevel.Unspecified:
                    return OletxTransactionIsolationLevel.ISOLATIONLEVEL_UNSPECIFIED;
            }
            return OletxTransactionIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;
        }

        internal static IsolationLevel ConvertIsolationLevelFromProxyValue(OletxTransactionIsolationLevel proxyIsolationLevel)
        {
            OletxTransactionIsolationLevel level2 = proxyIsolationLevel;
            if (level2 <= OletxTransactionIsolationLevel.ISOLATIONLEVEL_READUNCOMMITTED)
            {
                switch (level2)
                {
                    case OletxTransactionIsolationLevel.ISOLATIONLEVEL_UNSPECIFIED:
                        return IsolationLevel.Unspecified;

                    case OletxTransactionIsolationLevel.ISOLATIONLEVEL_CHAOS:
                        return IsolationLevel.Chaos;

                    case OletxTransactionIsolationLevel.ISOLATIONLEVEL_READUNCOMMITTED:
                        return IsolationLevel.ReadUncommitted;
                }
            }
            else if (level2 != OletxTransactionIsolationLevel.ISOLATIONLEVEL_CURSORSTABILITY)
            {
                if (level2 == OletxTransactionIsolationLevel.ISOLATIONLEVEL_REPEATABLEREAD)
                {
                    return IsolationLevel.RepeatableRead;
                }
                if (level2 == OletxTransactionIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE)
                {
                    return IsolationLevel.Serializable;
                }
            }
            else
            {
                return IsolationLevel.ReadCommitted;
            }
            return IsolationLevel.Serializable;
        }

        internal OletxCommittableTransaction CreateTransaction(TransactionOptions properties)
        {
            OletxCommittableTransaction transaction = null;
            ITransactionShim transactionShim = null;
            RealOletxTransaction realOletxTransaction = null;
            Guid empty = Guid.Empty;
            OutcomeEnlistment target = null;
            new DistributedTransactionPermission(PermissionState.Unrestricted).Demand();
            TransactionManager.ValidateIsolationLevel(properties.IsolationLevel);
            if (IsolationLevel.Unspecified == properties.IsolationLevel)
            {
                properties.IsolationLevel = this.configuredTransactionOptions.IsolationLevel;
            }
            properties.Timeout = TransactionManager.ValidateTimeout(properties.Timeout);
            this.dtcTransactionManagerLock.AcquireReaderLock(-1);
            try
            {
                OletxTransactionIsolationLevel isolationLevel = ConvertIsolationLevel(properties.IsolationLevel);
                uint timeout = System.Transactions.Oletx.DtcTransactionManager.AdjustTimeout(properties.Timeout);
                target = new OutcomeEnlistment();
                IntPtr zero = IntPtr.Zero;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    zero = HandleTable.AllocHandle(target);
                    this.dtcTransactionManager.ProxyShimFactory.BeginTransaction(timeout, isolationLevel, zero, out empty, out transactionShim);
                }
                catch (COMException exception)
                {
                    ProxyException(exception);
                    throw;
                }
                finally
                {
                    if ((transactionShim == null) && (zero != IntPtr.Zero))
                    {
                        HandleTable.FreeHandle(zero);
                    }
                }
                realOletxTransaction = new RealOletxTransaction(this, transactionShim, target, empty, isolationLevel, true);
                transaction = new OletxCommittableTransaction(realOletxTransaction);
                if (DiagnosticTrace.Information)
                {
                    TransactionCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), transaction.TransactionTraceId);
                }
            }
            finally
            {
                this.dtcTransactionManagerLock.ReleaseReaderLock();
            }
            return transaction;
        }

        internal OletxResourceManager FindOrRegisterResourceManager(Guid resourceManagerIdentifier)
        {
            if (resourceManagerIdentifier == Guid.Empty)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("BadResourceManagerId"), "resourceManagerIdentifier");
            }
            OletxResourceManager manager = null;
            resourceManagerHashTableLock.AcquireReaderLock(-1);
            try
            {
                manager = resourceManagerHashTable[resourceManagerIdentifier] as OletxResourceManager;
            }
            finally
            {
                resourceManagerHashTableLock.ReleaseReaderLock();
            }
            if (manager == null)
            {
                return this.RegisterResourceManager(resourceManagerIdentifier);
            }
            return manager;
        }

        internal static void ProxyException(COMException comException)
        {
            if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN == comException.ErrorCode) || (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE == comException.ErrorCode))
            {
                throw TransactionManagerCommunicationException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TransactionManagerCommunicationException"), comException);
            }
            if (System.Transactions.Oletx.NativeMethods.XACT_E_NETWORK_TX_DISABLED == comException.ErrorCode)
            {
                throw TransactionManagerCommunicationException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("NetworkTransactionsDisabled"), comException);
            }
            if ((System.Transactions.Oletx.NativeMethods.XACT_E_FIRST <= comException.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_LAST >= comException.ErrorCode))
            {
                if (System.Transactions.Oletx.NativeMethods.XACT_E_NOTRANSACTION == comException.ErrorCode)
                {
                    throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TransactionAlreadyOver"), comException);
                }
                throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), comException.Message, comException);
            }
        }

        internal OletxEnlistment ReenlistTransaction(Guid resourceManagerIdentifier, byte[] recoveryInformation, IEnlistmentNotificationInternal enlistmentNotification)
        {
            if (recoveryInformation == null)
            {
                throw new ArgumentNullException("recoveryInformation");
            }
            if (enlistmentNotification == null)
            {
                throw new ArgumentNullException("enlistmentNotification");
            }
            OletxResourceManager manager = this.RegisterResourceManager(resourceManagerIdentifier);
            if (manager == null)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("InvalidArgument"), "resourceManagerIdentifier");
            }
            if (manager.RecoveryCompleteCalledByApplication)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("ReenlistAfterRecoveryComplete"));
            }
            return manager.Reenlist(recoveryInformation.Length, recoveryInformation, enlistmentNotification);
        }

        internal OletxResourceManager RegisterResourceManager(Guid resourceManagerIdentifier)
        {
            OletxResourceManager manager = null;
            resourceManagerHashTableLock.AcquireWriterLock(-1);
            try
            {
                manager = resourceManagerHashTable[resourceManagerIdentifier] as OletxResourceManager;
                if (manager != null)
                {
                    return manager;
                }
                manager = new OletxResourceManager(this, resourceManagerIdentifier);
                resourceManagerHashTable.Add(resourceManagerIdentifier, manager);
            }
            finally
            {
                resourceManagerHashTableLock.ReleaseWriterLock();
            }
            return manager;
        }

        internal void ReinitializeProxy()
        {
            this.dtcTransactionManagerLock.AcquireWriterLock(-1);
            try
            {
                if (this.dtcTransactionManager != null)
                {
                    this.dtcTransactionManager.ReleaseProxy();
                }
            }
            finally
            {
                this.dtcTransactionManagerLock.ReleaseWriterLock();
            }
        }

        internal void ResourceManagerRecoveryComplete(Guid resourceManagerIdentifier)
        {
            OletxResourceManager manager = this.RegisterResourceManager(resourceManagerIdentifier);
            if (manager.RecoveryCompleteCalledByApplication)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("DuplicateRecoveryComplete"));
            }
            manager.RecoveryComplete();
        }

        internal static void ShimNotificationCallback(object state, bool timeout)
        {
            IntPtr zero = IntPtr.Zero;
            ShimNotificationType none = ShimNotificationType.None;
            bool isSinglePhase = false;
            bool abortingHint = false;
            uint prepareInfoSize = 0;
            CoTaskMemHandle prepareInfo = null;
            bool releaseRequired = false;
            bool flag3 = false;
            IDtcProxyShimFactory factory = null;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransactionManager.ShimNotificationCallback");
            }
            Thread.BeginCriticalRegion();
            try
            {
            Label_003D:
                factory = proxyShimFactory;
                try
                {
                    object obj2;
                    OletxResourceManager manager2;
                    byte[] buffer;
                    Thread.BeginThreadAffinity();
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        factory.GetNotification(out zero, out none, out isSinglePhase, out abortingHint, out releaseRequired, out prepareInfoSize, out prepareInfo);
                    }
                    finally
                    {
                        if (releaseRequired)
                        {
                            if (HandleTable.FindHandle(zero) is OletxInternalResourceManager)
                            {
                                processingTmDown = true;
                                Monitor.Enter(proxyShimFactory);
                            }
                            else
                            {
                                releaseRequired = false;
                            }
                            factory.ReleaseNotificationLock();
                        }
                        Thread.EndThreadAffinity();
                    }
                    if (processingTmDown)
                    {
                        lock (proxyShimFactory)
                        {
                        }
                    }
                    if (none != ShimNotificationType.None)
                    {
                        obj2 = HandleTable.FindHandle(zero);
                        switch (none)
                        {
                            case ShimNotificationType.Phase0RequestNotify:
                                try
                                {
                                    OletxPhase0VolatileEnlistmentContainer container5 = obj2 as OletxPhase0VolatileEnlistmentContainer;
                                    if (container5 != null)
                                    {
                                        DiagnosticTrace.SetActivityId(container5.TransactionIdentifier);
                                        container5.Phase0Request(abortingHint);
                                    }
                                    else
                                    {
                                        OletxEnlistment enlistment8 = obj2 as OletxEnlistment;
                                        if (enlistment8 != null)
                                        {
                                            DiagnosticTrace.SetActivityId(enlistment8.TransactionIdentifier);
                                            enlistment8.Phase0Request(abortingHint);
                                        }
                                        else
                                        {
                                            Environment.FailFast(System.Transactions.SR.GetString("InternalError"));
                                        }
                                    }
                                    goto Label_0444;
                                }
                                finally
                                {
                                    HandleTable.FreeHandle(zero);
                                }
                                break;

                            case ShimNotificationType.VoteRequestNotify:
                                break;

                            case ShimNotificationType.PrepareRequestNotify:
                                goto Label_02B0;

                            case ShimNotificationType.CommitRequestNotify:
                                goto Label_031A;

                            case ShimNotificationType.AbortRequestNotify:
                                goto Label_0358;

                            case ShimNotificationType.CommittedNotify:
                                try
                                {
                                    OutcomeEnlistment enlistment7 = obj2 as OutcomeEnlistment;
                                    if (enlistment7 != null)
                                    {
                                        DiagnosticTrace.SetActivityId(enlistment7.TransactionIdentifier);
                                        enlistment7.Committed();
                                    }
                                    else
                                    {
                                        OletxPhase1VolatileEnlistmentContainer container3 = obj2 as OletxPhase1VolatileEnlistmentContainer;
                                        if (container3 != null)
                                        {
                                            DiagnosticTrace.SetActivityId(container3.TransactionIdentifier);
                                            container3.Committed();
                                        }
                                        else
                                        {
                                            Environment.FailFast(System.Transactions.SR.GetString("InternalError"));
                                        }
                                    }
                                    goto Label_0444;
                                }
                                finally
                                {
                                    HandleTable.FreeHandle(zero);
                                }
                                goto Label_0203;

                            case ShimNotificationType.AbortedNotify:
                                goto Label_0203;

                            case ShimNotificationType.InDoubtNotify:
                                goto Label_0251;

                            case ShimNotificationType.EnlistmentTmDownNotify:
                                goto Label_0396;

                            case ShimNotificationType.ResourceManagerTmDownNotify:
                                goto Label_03CD;

                            default:
                                goto Label_0410;
                        }
                        OletxPhase1VolatileEnlistmentContainer container4 = obj2 as OletxPhase1VolatileEnlistmentContainer;
                        if (container4 != null)
                        {
                            DiagnosticTrace.SetActivityId(container4.TransactionIdentifier);
                            container4.VoteRequest();
                        }
                        else
                        {
                            Environment.FailFast(System.Transactions.SR.GetString("InternalError"));
                        }
                    }
                    goto Label_0444;
                Label_0203:;
                    try
                    {
                        OutcomeEnlistment enlistment6 = obj2 as OutcomeEnlistment;
                        if (enlistment6 != null)
                        {
                            DiagnosticTrace.SetActivityId(enlistment6.TransactionIdentifier);
                            enlistment6.Aborted();
                        }
                        else
                        {
                            OletxPhase1VolatileEnlistmentContainer container2 = obj2 as OletxPhase1VolatileEnlistmentContainer;
                            if (container2 != null)
                            {
                                DiagnosticTrace.SetActivityId(container2.TransactionIdentifier);
                                container2.Aborted();
                            }
                        }
                        goto Label_0444;
                    }
                    finally
                    {
                        HandleTable.FreeHandle(zero);
                    }
                Label_0251:;
                    try
                    {
                        OutcomeEnlistment enlistment5 = obj2 as OutcomeEnlistment;
                        if (enlistment5 != null)
                        {
                            DiagnosticTrace.SetActivityId(enlistment5.TransactionIdentifier);
                            enlistment5.InDoubt();
                        }
                        else
                        {
                            OletxPhase1VolatileEnlistmentContainer container = obj2 as OletxPhase1VolatileEnlistmentContainer;
                            if (container != null)
                            {
                                DiagnosticTrace.SetActivityId(container.TransactionIdentifier);
                                container.InDoubt();
                            }
                            else
                            {
                                Environment.FailFast(System.Transactions.SR.GetString("InternalError"));
                            }
                        }
                        goto Label_0444;
                    }
                    finally
                    {
                        HandleTable.FreeHandle(zero);
                    }
                Label_02B0:
                    buffer = new byte[prepareInfoSize];
                    Marshal.Copy(prepareInfo.DangerousGetHandle(), buffer, 0, Convert.ToInt32(prepareInfoSize));
                    bool flag2 = true;
                    try
                    {
                        OletxEnlistment enlistment4 = obj2 as OletxEnlistment;
                        if (enlistment4 != null)
                        {
                            DiagnosticTrace.SetActivityId(enlistment4.TransactionIdentifier);
                            flag2 = enlistment4.PrepareRequest(isSinglePhase, buffer);
                        }
                        else
                        {
                            Environment.FailFast(System.Transactions.SR.GetString("InternalError"));
                        }
                        goto Label_0444;
                    }
                    finally
                    {
                        if (flag2)
                        {
                            HandleTable.FreeHandle(zero);
                        }
                    }
                Label_031A:;
                    try
                    {
                        OletxEnlistment enlistment3 = obj2 as OletxEnlistment;
                        if (enlistment3 != null)
                        {
                            DiagnosticTrace.SetActivityId(enlistment3.TransactionIdentifier);
                            enlistment3.CommitRequest();
                        }
                        else
                        {
                            Environment.FailFast(System.Transactions.SR.GetString("InternalError"));
                        }
                        goto Label_0444;
                    }
                    finally
                    {
                        HandleTable.FreeHandle(zero);
                    }
                Label_0358:;
                    try
                    {
                        OletxEnlistment enlistment2 = obj2 as OletxEnlistment;
                        if (enlistment2 != null)
                        {
                            DiagnosticTrace.SetActivityId(enlistment2.TransactionIdentifier);
                            enlistment2.AbortRequest();
                        }
                        else
                        {
                            Environment.FailFast(System.Transactions.SR.GetString("InternalError"));
                        }
                        goto Label_0444;
                    }
                    finally
                    {
                        HandleTable.FreeHandle(zero);
                    }
                Label_0396:;
                    try
                    {
                        OletxEnlistment enlistment = obj2 as OletxEnlistment;
                        if (enlistment != null)
                        {
                            DiagnosticTrace.SetActivityId(enlistment.TransactionIdentifier);
                            enlistment.TMDown();
                        }
                        else
                        {
                            Environment.FailFast(System.Transactions.SR.GetString("InternalError"));
                        }
                        goto Label_0444;
                    }
                    finally
                    {
                        HandleTable.FreeHandle(zero);
                    }
                Label_03CD:
                    manager2 = obj2 as OletxResourceManager;
                    try
                    {
                        if (manager2 != null)
                        {
                            manager2.TMDown();
                        }
                        else
                        {
                            OletxInternalResourceManager manager = obj2 as OletxInternalResourceManager;
                            if (manager != null)
                            {
                                manager.TMDown();
                            }
                            else
                            {
                                Environment.FailFast(System.Transactions.SR.GetString("InternalError"));
                            }
                        }
                        goto Label_0444;
                    }
                    finally
                    {
                        HandleTable.FreeHandle(zero);
                    }
                Label_0410:
                    Environment.FailFast(System.Transactions.SR.GetString("InternalError"));
                }
                finally
                {
                    if (prepareInfo != null)
                    {
                        prepareInfo.Close();
                    }
                    if (releaseRequired)
                    {
                        releaseRequired = false;
                        processingTmDown = false;
                        Monitor.Exit(proxyShimFactory);
                    }
                }
            Label_0444:
                if (none != ShimNotificationType.None)
                {
                    goto Label_003D;
                }
                flag3 = true;
            }
            finally
            {
                if (releaseRequired)
                {
                    releaseRequired = false;
                    processingTmDown = false;
                    Monitor.Exit(proxyShimFactory);
                }
                if (!flag3 && (zero != IntPtr.Zero))
                {
                    HandleTable.FreeHandle(zero);
                }
                Thread.EndCriticalRegion();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransactionManager.ShimNotificationCallback");
            }
        }

        internal static object ClassSyncObject
        {
            get
            {
                if (classSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref classSyncObject, obj2, null);
                }
                return classSyncObject;
            }
        }

        internal string CreationNodeName
        {
            get
            {
                return this.nodeNameField;
            }
        }

        internal System.Transactions.Oletx.DtcTransactionManager DtcTransactionManager
        {
            get
            {
                if (!this.dtcTransactionManagerLock.IsReaderLockHeld && !this.dtcTransactionManagerLock.IsWriterLockHeld)
                {
                    throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("InternalError"), null);
                }
                if (this.dtcTransactionManager == null)
                {
                    throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("DtcTransactionManagerUnavailable"), null);
                }
                return this.dtcTransactionManager;
            }
        }

        internal string NodeName
        {
            get
            {
                return this.nodeNameField;
            }
        }

        internal static EventWaitHandle ShimWaitHandle
        {
            get
            {
                if (shimWaitHandle == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (shimWaitHandle == null)
                        {
                            shimWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                        }
                    }
                }
                return shimWaitHandle;
            }
        }
    }
}

