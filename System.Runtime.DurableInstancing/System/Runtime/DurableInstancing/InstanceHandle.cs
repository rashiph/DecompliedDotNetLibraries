namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Transactions;

    public sealed class InstanceHandle
    {
        private HashSet<XName> boundOwnerEvents;
        private Guid id;
        private volatile bool idIsSet;
        private InstanceHandleReference inProgressBind;
        private bool needFreedNotification;
        private HashSet<InstancePersistenceEvent> pendingOwnerEvents;
        private PreparingEnlistment pendingPreparingEnlistment;
        private AcquireContextAsyncResult pendingRollback;
        private object providerObject;
        private bool providerObjectSet;
        private readonly object thisLock;
        private WaitForEventsAsyncResult waitResult;

        internal InstanceHandle(InstanceStore store, InstanceOwner owner)
        {
            this.thisLock = new object();
            this.Version = -1L;
            this.Store = store;
            this.Owner = owner;
            this.View = new InstanceView(owner);
            this.IsValid = true;
        }

        internal InstanceHandle(InstanceStore store, InstanceOwner owner, Guid instanceId)
        {
            this.thisLock = new object();
            this.Version = -1L;
            this.Store = store;
            this.Owner = owner;
            this.Id = instanceId;
            this.View = new InstanceView(owner, instanceId);
            this.IsValid = true;
        }

        internal InstancePersistenceContext AcquireExecutionContext(Transaction hostTransaction, TimeSpan timeout)
        {
            InstancePersistenceContext context2;
            bool setOperationPending = false;
            InstancePersistenceContext context = null;
            try
            {
                context = AcquireContextAsyncResult.End(new AcquireContextAsyncResult(this, hostTransaction, timeout, out setOperationPending));
                Fx.AssertAndThrow(context != null, "Null result returned from AcquireContextAsyncResult (synchronous).");
                context2 = context;
            }
            finally
            {
                if ((context == null) && setOperationPending)
                {
                    this.FinishOperation();
                }
            }
            return context2;
        }

        internal IAsyncResult BeginAcquireExecutionContext(Transaction hostTransaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            bool setOperationPending = false;
            IAsyncResult result = null;
            try
            {
                result = new AcquireContextAsyncResult(this, hostTransaction, timeout, out setOperationPending, callback, state);
                result2 = result;
            }
            finally
            {
                if ((result == null) && setOperationPending)
                {
                    this.FinishOperation();
                }
            }
            return result2;
        }

        internal static IAsyncResult BeginWaitForEvents(InstanceHandle handle, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new WaitForEventsAsyncResult(handle, timeout, callback, state);
        }

        internal void Bind(long instanceVersion)
        {
            Fx.AssertAndThrow(instanceVersion >= 0L, "Negative instanceVersion passed to Bind.");
            lock (this.ThisLock)
            {
                Fx.AssertAndThrow(this.Version == -1L, "This should only be reachable once per handle.");
                this.Version = instanceVersion;
                if (this.inProgressBind == null)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.BindLockRequiresCommandFlag));
                }
            }
        }

        internal void BindInstance(Guid instanceId)
        {
            List<InstanceHandleReference> handlesPendingResolution = null;
            try
            {
                lock (this.ThisLock)
                {
                    this.Id = instanceId;
                    if (this.inProgressBind != null)
                    {
                        this.Owner.InstanceBound(ref this.inProgressBind, ref handlesPendingResolution);
                    }
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
        }

        internal void BindOwner(InstanceOwner owner)
        {
            lock (this.ThisLock)
            {
                this.Owner = owner;
            }
        }

        internal void BindOwnerEvent(InstancePersistenceEvent persistenceEvent)
        {
            lock (this.ThisLock)
            {
                if (this.IsValid && ((this.boundOwnerEvents == null) || !this.boundOwnerEvents.Contains(persistenceEvent.Name)))
                {
                    if (this.pendingOwnerEvents == null)
                    {
                        this.pendingOwnerEvents = new HashSet<InstancePersistenceEvent>();
                    }
                    else if (this.pendingOwnerEvents.Contains(persistenceEvent))
                    {
                        goto Label_0085;
                    }
                    this.pendingOwnerEvents.Add(persistenceEvent);
                    this.Store.PendHandleToEvent(this, persistenceEvent, this.Owner);
                }
            Label_0085:;
            }
        }

        internal void CancelReclaim(Exception reason)
        {
            List<InstanceHandleReference> handlesPendingResolution = null;
            try
            {
                lock (this.ThisLock)
                {
                    if (this.inProgressBind == null)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.DoNotCompleteTryCommandWithPendingReclaim));
                    }
                    this.Owner.FaultBind(ref this.inProgressBind, ref handlesPendingResolution, reason);
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
        }

        private bool CancelWaiting(WaitForEventsAsyncResult result)
        {
            lock (this.ThisLock)
            {
                if (!object.ReferenceEquals(this.waitResult, result))
                {
                    return false;
                }
                this.waitResult = null;
                return true;
            }
        }

        internal InstanceView Commit(InstanceView newState)
        {
            InstanceView view;
            newState.MakeReadOnly();
            this.View = newState;
            List<InstanceHandleReference> handlesPendingResolution = null;
            InstanceHandle handleToFree = null;
            List<InstancePersistenceEvent> persistenceEvents = null;
            WaitForEventsAsyncResult waitResult = null;
            try
            {
                lock (this.ThisLock)
                {
                    if (this.inProgressBind != null)
                    {
                        if (this.Version != -1L)
                        {
                            if (!this.Owner.TryCompleteBind(ref this.inProgressBind, ref handlesPendingResolution, out handleToFree))
                            {
                                return null;
                            }
                        }
                        else
                        {
                            this.Owner.CancelBind(ref this.inProgressBind, ref handlesPendingResolution);
                        }
                    }
                    if ((this.pendingOwnerEvents != null) && this.IsValid)
                    {
                        if (this.boundOwnerEvents == null)
                        {
                            this.boundOwnerEvents = new HashSet<XName>();
                        }
                        foreach (InstancePersistenceEvent event2 in this.pendingOwnerEvents)
                        {
                            if (this.boundOwnerEvents.Add(event2.Name))
                            {
                                InstancePersistenceEvent item = this.Store.AddHandleToEvent(this, event2, this.Owner);
                                if (item != null)
                                {
                                    if (persistenceEvents == null)
                                    {
                                        persistenceEvents = new List<InstancePersistenceEvent>(this.pendingOwnerEvents.Count);
                                    }
                                    persistenceEvents.Add(item);
                                }
                            }
                        }
                        this.pendingOwnerEvents = null;
                        if ((persistenceEvents != null) && (this.waitResult != null))
                        {
                            waitResult = this.waitResult;
                            this.waitResult = null;
                        }
                    }
                    view = this.View;
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
                if (handleToFree != null)
                {
                    handleToFree.Free();
                }
                if (waitResult != null)
                {
                    waitResult.Signaled(persistenceEvents);
                }
            }
            return view;
        }

        internal InstancePersistenceContext EndAcquireExecutionContext(IAsyncResult result)
        {
            return AcquireContextAsyncResult.End(result);
        }

        internal static List<InstancePersistenceEvent> EndWaitForEvents(IAsyncResult result)
        {
            return WaitForEventsAsyncResult.End(result);
        }

        internal void EventReady(InstancePersistenceEvent persistenceEvent)
        {
            WaitForEventsAsyncResult waitResult = null;
            lock (this.ThisLock)
            {
                if (this.waitResult != null)
                {
                    waitResult = this.waitResult;
                    this.waitResult = null;
                }
            }
            if (waitResult != null)
            {
                waitResult.Signaled(persistenceEvent);
            }
        }

        private void FinishOperation()
        {
            List<InstanceHandleReference> handlesPendingResolution = null;
            try
            {
                bool needFreedNotification;
                PreparingEnlistment pendingPreparingEnlistment;
                AcquireContextAsyncResult pendingRollback;
                lock (this.ThisLock)
                {
                    this.OperationPending = false;
                    this.AcquirePending = null;
                    this.CurrentExecutionContext = null;
                    if ((this.inProgressBind != null) && ((this.Version == -1L) || !this.IsValid))
                    {
                        this.Owner.CancelBind(ref this.inProgressBind, ref handlesPendingResolution);
                    }
                    else if ((this.Version != -1L) && !this.IsValid)
                    {
                        this.Owner.Unbind(this);
                    }
                    needFreedNotification = this.needFreedNotification;
                    this.needFreedNotification = false;
                    pendingPreparingEnlistment = this.pendingPreparingEnlistment;
                    this.pendingPreparingEnlistment = null;
                    pendingRollback = this.pendingRollback;
                    this.pendingRollback = null;
                }
                try
                {
                    if (needFreedNotification)
                    {
                        this.Store.FreeInstanceHandle(this, this.ProviderObject);
                    }
                }
                finally
                {
                    if (pendingRollback != null)
                    {
                        pendingRollback.RollBack();
                    }
                    else if (pendingPreparingEnlistment != null)
                    {
                        pendingPreparingEnlistment.Prepared();
                    }
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
        }

        internal bool FinishReclaim(ref long instanceVersion)
        {
            List<InstanceHandleReference> handlesPendingResolution = null;
            bool flag2;
            try
            {
                lock (this.ThisLock)
                {
                    if (this.inProgressBind == null)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.DoNotCompleteTryCommandWithPendingReclaim));
                    }
                    if (!this.Owner.FinishBind(ref this.inProgressBind, ref instanceVersion, ref handlesPendingResolution))
                    {
                        return false;
                    }
                    Fx.AssertAndThrow(this.Version == -1L, "Should only be able to set the version once per handle.");
                    Fx.AssertAndThrow(instanceVersion >= 0L, "Incorrect version resulting from conflict resolution.");
                    this.Version = instanceVersion;
                    flag2 = true;
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
            return flag2;
        }

        public void Free()
        {
            if (!this.providerObjectSet)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.HandleFreedBeforeInitialized));
            }
            if (this.IsValid)
            {
                List<InstanceHandleReference> handlesPendingResolution = null;
                WaitForEventsAsyncResult waitResult = null;
                try
                {
                    bool flag = false;
                    InstancePersistenceContext currentExecutionContext = null;
                    lock (this.ThisLock)
                    {
                        if (!this.IsValid)
                        {
                            return;
                        }
                        this.IsValid = false;
                        IEnumerable<XName> first = null;
                        if ((this.pendingOwnerEvents != null) && (this.pendingOwnerEvents.Count > 0))
                        {
                            first = from persistenceEvent in this.pendingOwnerEvents select persistenceEvent.Name;
                        }
                        if ((this.boundOwnerEvents != null) && (this.boundOwnerEvents.Count > 0))
                        {
                            first = (first == null) ? this.boundOwnerEvents : first.Concat<XName>(this.boundOwnerEvents);
                        }
                        if (first != null)
                        {
                            this.Store.RemoveHandleFromEvents(this, first, this.Owner);
                        }
                        if (this.waitResult != null)
                        {
                            waitResult = this.waitResult;
                            this.waitResult = null;
                        }
                        if (this.OperationPending)
                        {
                            if (this.AcquirePending != null)
                            {
                                this.CurrentTransactionalAsyncResult.WaitForHostTransaction.Set();
                                this.needFreedNotification = true;
                            }
                            else
                            {
                                currentExecutionContext = this.CurrentExecutionContext;
                            }
                        }
                        else
                        {
                            flag = true;
                            if (this.inProgressBind != null)
                            {
                                this.Owner.CancelBind(ref this.inProgressBind, ref handlesPendingResolution);
                            }
                            else if (this.Version != -1L)
                            {
                                this.Owner.Unbind(this);
                            }
                        }
                    }
                    if (currentExecutionContext != null)
                    {
                        currentExecutionContext.NotifyHandleFree();
                        lock (this.ThisLock)
                        {
                            if (this.OperationPending)
                            {
                                this.needFreedNotification = true;
                                if (this.inProgressBind != null)
                                {
                                    this.Owner.FaultBind(ref this.inProgressBind, ref handlesPendingResolution, null);
                                }
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        this.Store.FreeInstanceHandle(this, this.ProviderObject);
                    }
                }
                finally
                {
                    if (waitResult != null)
                    {
                        waitResult.Canceled();
                    }
                    InstanceOwner.ResolveHandles(handlesPendingResolution);
                }
            }
        }

        private void OnPrepare(PreparingEnlistment preparingEnlistment)
        {
            bool flag = false;
            lock (this.ThisLock)
            {
                if (this.TooLateToEnlist)
                {
                    return;
                }
                this.TooLateToEnlist = true;
                if (this.OperationPending && (this.AcquirePending == null))
                {
                    this.pendingPreparingEnlistment = preparingEnlistment;
                }
                else
                {
                    flag = true;
                }
            }
            if (flag)
            {
                preparingEnlistment.Prepared();
            }
        }

        private void OnRollBack(AcquireContextAsyncResult rollingBack)
        {
            bool flag = false;
            lock (this.ThisLock)
            {
                this.TooLateToEnlist = true;
                if (this.OperationPending && (this.AcquirePending == null))
                {
                    this.pendingRollback = rollingBack;
                    this.pendingPreparingEnlistment = null;
                }
                else
                {
                    flag = true;
                }
            }
            if (flag)
            {
                rollingBack.RollBack();
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void ReleaseExecutionContext()
        {
            this.FinishOperation();
        }

        internal void StartPotentialBind()
        {
            lock (this.ThisLock)
            {
                Fx.AssertAndThrow(this.Version == -1L, "Handle already bound to a lock.");
                this.Owner.StartBind(this, ref this.inProgressBind);
            }
        }

        internal AsyncWaitHandle StartReclaim(long instanceVersion)
        {
            List<InstanceHandleReference> handlesPendingResolution = null;
            AsyncWaitHandle handle;
            try
            {
                lock (this.ThisLock)
                {
                    Fx.AssertAndThrow(this.Version == -1L, "StartReclaim should only be reachable if the lock hasn't been bound.");
                    if (this.inProgressBind == null)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.BindLockRequiresCommandFlag));
                    }
                    handle = this.Owner.InitiateLockResolution(instanceVersion, ref this.inProgressBind, ref handlesPendingResolution);
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
            return handle;
        }

        private List<InstancePersistenceEvent> StartWaiting(WaitForEventsAsyncResult result, IOThreadTimer timeoutTimer, TimeSpan timeout)
        {
            lock (this.ThisLock)
            {
                if (this.waitResult != null)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.WaitAlreadyInProgress));
                }
                if (!this.IsValid)
                {
                    throw Fx.Exception.AsError(new OperationCanceledException(SRCore.HandleFreed));
                }
                if ((this.boundOwnerEvents != null) && (this.boundOwnerEvents.Count > 0))
                {
                    List<InstancePersistenceEvent> list = this.Store.SelectSignaledEvents(this.boundOwnerEvents, this.Owner);
                    if (list != null)
                    {
                        return list;
                    }
                }
                this.waitResult = result;
                if (timeoutTimer != null)
                {
                    timeoutTimer.Set(timeout);
                }
                return null;
            }
        }

        private AcquireContextAsyncResult AcquirePending { get; set; }

        internal InstanceHandle ConflictingHandle
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ConflictingHandle>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ConflictingHandle>k__BackingField = value;
            }
        }

        private InstancePersistenceContext CurrentExecutionContext { get; set; }

        private AcquireContextAsyncResult CurrentTransactionalAsyncResult { get; set; }

        internal Guid Id
        {
            get
            {
                if (!this.idIsSet)
                {
                    return Guid.Empty;
                }
                return this.id;
            }
            private set
            {
                this.id = value;
                this.idIsSet = true;
            }
        }

        public bool IsValid
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<IsValid>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<IsValid>k__BackingField = value;
            }
        }

        private bool OperationPending { get; set; }

        internal InstanceOwner Owner
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Owner>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Owner>k__BackingField = value;
            }
        }

        internal object ProviderObject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.providerObject;
            }
            set
            {
                this.providerObject = value;
                this.providerObjectSet = true;
            }
        }

        internal InstanceStore Store
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Store>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Store>k__BackingField = value;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        private bool TooLateToEnlist { get; set; }

        internal long Version
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Version>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Version>k__BackingField = value;
            }
        }

        internal InstanceView View
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<View>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<View>k__BackingField = value;
            }
        }

        private class AcquireContextAsyncResult : AsyncResult, IEnlistmentNotification
        {
            private InstancePersistenceContext executionContext;
            private readonly InstanceHandle handle;
            private static Action<object, TimeoutException> onHostTransaction = new Action<object, TimeoutException>(InstanceHandle.AcquireContextAsyncResult.OnHostTransaction);
            private readonly TimeoutHelper timeoutHelper;

            public AcquireContextAsyncResult(InstanceHandle handle, Transaction hostTransaction, TimeSpan timeout, out bool setOperationPending) : this(handle, hostTransaction, timeout, out setOperationPending, true, null, null)
            {
            }

            public AcquireContextAsyncResult(InstanceHandle handle, Transaction hostTransaction, TimeSpan timeout, out bool setOperationPending, AsyncCallback callback, object state) : this(handle, hostTransaction, timeout, out setOperationPending, false, callback, state)
            {
            }

            private AcquireContextAsyncResult(InstanceHandle handle, Transaction hostTransaction, TimeSpan timeout, out bool setOperationPending, bool synchronous, AsyncCallback callback, object state) : base(callback, state)
            {
                InstanceHandle.AcquireContextAsyncResult currentTransactionalAsyncResult;
                setOperationPending = false;
                this.handle = handle;
                this.HostTransaction = hostTransaction;
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool flag = false;
                lock (this.handle.ThisLock)
                {
                    if (!this.handle.IsValid)
                    {
                        throw Fx.Exception.AsError(new OperationCanceledException(SRCore.HandleFreed));
                    }
                    if (this.handle.OperationPending)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CommandExecutionCannotOverlap));
                    }
                    setOperationPending = true;
                    this.handle.OperationPending = true;
                    currentTransactionalAsyncResult = this.handle.CurrentTransactionalAsyncResult;
                    if (currentTransactionalAsyncResult != null)
                    {
                        if (currentTransactionalAsyncResult.HostTransaction.Equals(hostTransaction) && !this.handle.TooLateToEnlist)
                        {
                            flag = true;
                            this.executionContext = currentTransactionalAsyncResult.ReuseContext();
                            this.handle.CurrentExecutionContext = this.executionContext;
                        }
                        else
                        {
                            this.handle.AcquirePending = this;
                        }
                    }
                }
                if (currentTransactionalAsyncResult != null)
                {
                    if (flag)
                    {
                        base.Complete(true);
                        return;
                    }
                    TimeSpan span = this.timeoutHelper.RemainingTime();
                    if (synchronous)
                    {
                        if (!currentTransactionalAsyncResult.WaitForHostTransaction.Wait(span))
                        {
                            throw Fx.Exception.AsError(new TimeoutException(SRCore.TimeoutOnOperation(span)));
                        }
                    }
                    else if (!currentTransactionalAsyncResult.WaitForHostTransaction.WaitAsync(onHostTransaction, this, span))
                    {
                        return;
                    }
                }
                if (this.DoAfterTransaction())
                {
                    base.Complete(true);
                }
            }

            private bool DoAfterTransaction()
            {
                InstanceHandle.AcquireContextAsyncResult result = null;
                try
                {
                    lock (this.handle.ThisLock)
                    {
                        if (!this.handle.IsValid)
                        {
                            throw Fx.Exception.AsError(new OperationCanceledException(SRCore.HandleFreed));
                        }
                        if (this.HostTransaction == null)
                        {
                            this.executionContext = new InstancePersistenceContext(this.handle, this.timeoutHelper.RemainingTime());
                        }
                        else
                        {
                            this.executionContext = new InstancePersistenceContext(this.handle, this.HostTransaction);
                        }
                        this.handle.AcquirePending = null;
                        this.handle.CurrentExecutionContext = this.executionContext;
                        this.handle.TooLateToEnlist = false;
                    }
                    if (this.HostTransaction != null)
                    {
                        this.WaitForHostTransaction = new AsyncWaitHandle(EventResetMode.ManualReset);
                        this.HostTransaction.EnlistVolatile(this, EnlistmentOptions.None);
                        result = this;
                    }
                }
                finally
                {
                    this.handle.CurrentTransactionalAsyncResult = result;
                }
                return true;
            }

            public static InstancePersistenceContext End(IAsyncResult result)
            {
                return AsyncResult.End<InstanceHandle.AcquireContextAsyncResult>(result).executionContext;
            }

            private static void OnHostTransaction(object state, TimeoutException timeoutException)
            {
                InstanceHandle.AcquireContextAsyncResult result = (InstanceHandle.AcquireContextAsyncResult) state;
                Exception exception = timeoutException;
                bool flag = exception != null;
                if (!flag)
                {
                    try
                    {
                        if (result.DoAfterTransaction())
                        {
                            flag = true;
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                        flag = true;
                    }
                }
                if (flag)
                {
                    if (exception != null)
                    {
                        result.handle.FinishOperation();
                    }
                    result.Complete(false, exception);
                }
            }

            private InstancePersistenceContext ReuseContext()
            {
                this.executionContext.PrepareForReuse();
                return this.executionContext;
            }

            internal void RollBack()
            {
                if (this.executionContext.IsHandleDoomedByRollback)
                {
                    this.handle.Free();
                }
                else
                {
                    this.WaitForHostTransaction.Set();
                }
            }

            void IEnlistmentNotification.Commit(Enlistment enlistment)
            {
                Fx.AssertAndThrow(this.handle.CurrentExecutionContext == null, "Prepare should have been called first and waited until after command processing.");
                bool flag = this.handle.Commit(this.executionContext.InstanceView) != null;
                enlistment.Done();
                if (flag)
                {
                    this.WaitForHostTransaction.Set();
                }
                else
                {
                    this.handle.Free();
                }
            }

            void IEnlistmentNotification.InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
                this.handle.Free();
            }

            void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
            {
                this.handle.OnPrepare(preparingEnlistment);
            }

            void IEnlistmentNotification.Rollback(Enlistment enlistment)
            {
                enlistment.Done();
                this.handle.OnRollBack(this);
            }

            public Transaction HostTransaction
            {
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.<HostTransaction>k__BackingField;
                }
                [CompilerGenerated]
                private set
                {
                    this.<HostTransaction>k__BackingField = value;
                }
            }

            public AsyncWaitHandle WaitForHostTransaction
            {
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.<WaitForHostTransaction>k__BackingField;
                }
                [CompilerGenerated]
                private set
                {
                    this.<WaitForHostTransaction>k__BackingField = value;
                }
            }
        }

        private class WaitForEventsAsyncResult : AsyncResult
        {
            private readonly InstanceHandle handle;
            private List<InstancePersistenceEvent> readyEvents;
            private readonly TimeSpan timeout;
            private static readonly Action<object> timeoutCallback = new Action<object>(InstanceHandle.WaitForEventsAsyncResult.OnTimeout);
            private IOThreadTimer timer;

            internal WaitForEventsAsyncResult(InstanceHandle handle, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.handle = handle;
                this.timeout = timeout;
                if ((this.timeout != TimeSpan.Zero) && (this.timeout != TimeSpan.MaxValue))
                {
                    this.timer = new IOThreadTimer(timeoutCallback, this, false);
                }
                List<InstancePersistenceEvent> list = this.handle.StartWaiting(this, this.timer, this.timeout);
                if (list == null)
                {
                    if (this.timeout == TimeSpan.Zero)
                    {
                        this.handle.CancelWaiting(this);
                        throw Fx.Exception.AsError(new TimeoutException(SRCore.WaitForEventsTimedOut(TimeSpan.Zero)));
                    }
                }
                else
                {
                    this.readyEvents = list;
                    base.Complete(true);
                }
            }

            internal void Canceled()
            {
                if (this.timer != null)
                {
                    this.timer.Cancel();
                }
                base.Complete(false, new OperationCanceledException(SRCore.HandleFreed));
            }

            internal static List<InstancePersistenceEvent> End(IAsyncResult result)
            {
                return AsyncResult.End<InstanceHandle.WaitForEventsAsyncResult>(result).readyEvents;
            }

            private static void OnTimeout(object state)
            {
                InstanceHandle.WaitForEventsAsyncResult result = (InstanceHandle.WaitForEventsAsyncResult) state;
                if (result.handle.CancelWaiting(result))
                {
                    result.Complete(false, new TimeoutException(SRCore.WaitForEventsTimedOut(result.timeout)));
                }
            }

            internal void Signaled(InstancePersistenceEvent persistenceEvent)
            {
                this.Signaled(new List<InstancePersistenceEvent>(1) { persistenceEvent });
            }

            internal void Signaled(List<InstancePersistenceEvent> persistenceEvents)
            {
                if (this.timer != null)
                {
                    this.timer.Cancel();
                }
                this.readyEvents = persistenceEvents;
                base.Complete(false);
            }
        }
    }
}

