namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Transactions;
    using System.Xml.Linq;

    public abstract class InstanceStore
    {
        private int ownerKeysIndexToScan;
        private Guid[] ownerKeysToScan = new Guid[0];
        private readonly Dictionary<Guid, WeakReference> owners = new Dictionary<Guid, WeakReference>(1);

        protected InstanceStore()
        {
        }

        internal InstancePersistenceEvent AddHandleToEvent(InstanceHandle handle, InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
        {
            lock (this.ThisLock)
            {
                InstanceNormalEvent ownerEventHelper = this.GetOwnerEventHelper(persistenceEvent, owner);
                ownerEventHelper.BoundHandles.Add(handle);
                ownerEventHelper.PendingHandles.Remove(handle);
                return (ownerEventHelper.IsSignaled ? ownerEventHelper : null);
            }
        }

        public IAsyncResult BeginExecute(InstanceHandle handle, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (command == null)
            {
                throw Fx.Exception.ArgumentNull("command");
            }
            if (handle == null)
            {
                throw Fx.Exception.ArgumentNull("handle");
            }
            if (!object.ReferenceEquals(this, handle.Store))
            {
                throw Fx.Exception.Argument("handle", SRCore.ContextNotFromThisStore);
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return InstancePersistenceContext.BeginOuterExecute(handle, command, Transaction.Current, timeout, callback, state);
        }

        protected internal virtual IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<bool>(false, callback, state);
        }

        public IAsyncResult BeginWaitForEvents(InstanceHandle handle, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (handle == null)
            {
                throw Fx.Exception.ArgumentNull("handle");
            }
            if (!object.ReferenceEquals(this, handle.Store))
            {
                throw Fx.Exception.Argument("handle", SRCore.ContextNotFromThisStore);
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return InstanceHandle.BeginWaitForEvents(handle, timeout, callback, state);
        }

        public InstanceHandle CreateInstanceHandle()
        {
            return this.CreateInstanceHandle(this.DefaultInstanceOwner);
        }

        public InstanceHandle CreateInstanceHandle(Guid instanceId)
        {
            return this.CreateInstanceHandle(this.DefaultInstanceOwner, instanceId);
        }

        public InstanceHandle CreateInstanceHandle(InstanceOwner owner)
        {
            return this.PrepareInstanceHandle(new InstanceHandle(this, owner));
        }

        public InstanceHandle CreateInstanceHandle(InstanceOwner owner, Guid instanceId)
        {
            if (instanceId == Guid.Empty)
            {
                throw Fx.Exception.Argument("instanceId", SRCore.CannotCreateContextWithNullId);
            }
            return this.PrepareInstanceHandle(new InstanceHandle(this, owner, instanceId));
        }

        public InstanceView EndExecute(IAsyncResult result)
        {
            return InstancePersistenceContext.EndOuterExecute(result);
        }

        protected internal virtual bool EndTryCommand(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        public List<InstancePersistenceEvent> EndWaitForEvents(IAsyncResult result)
        {
            return InstanceHandle.EndWaitForEvents(result);
        }

        public InstanceView Execute(InstanceHandle handle, InstancePersistenceCommand command, TimeSpan timeout)
        {
            if (command == null)
            {
                throw Fx.Exception.ArgumentNull("command");
            }
            if (handle == null)
            {
                throw Fx.Exception.ArgumentNull("handle");
            }
            if (!object.ReferenceEquals(this, handle.Store))
            {
                throw Fx.Exception.Argument("handle", SRCore.ContextNotFromThisStore);
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return InstancePersistenceContext.OuterExecute(handle, command, Transaction.Current, timeout);
        }

        internal void FreeInstanceHandle(InstanceHandle handle, object providerObject)
        {
            try
            {
                this.OnFreeInstanceHandle(handle, providerObject);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw Fx.Exception.AsError(new CallbackException(SRCore.OnFreeInstanceHandleThrew, exception));
            }
        }

        protected InstancePersistenceEvent[] GetEvents(InstanceOwner owner)
        {
            if (owner == null)
            {
                throw Fx.Exception.ArgumentNull("owner");
            }
            lock (this.ThisLock)
            {
                WeakReference reference;
                if (!this.owners.TryGetValue(owner.InstanceOwnerId, out reference) || !object.ReferenceEquals(reference.Target, owner))
                {
                    throw Fx.Exception.Argument("owner", SRCore.OwnerBelongsToWrongStore);
                }
                return owner.Events.Values.ToArray<InstanceNormalEvent>();
            }
        }

        protected InstanceOwner[] GetInstanceOwners()
        {
            lock (this.ThisLock)
            {
                return (from weakReference in this.owners.Values
                    select (InstanceOwner) weakReference.Target into owner
                    where owner != null
                    select owner).ToArray<InstanceOwner>();
            }
        }

        internal InstanceOwner GetOrCreateOwner(Guid instanceOwnerId, Guid lockToken)
        {
            lock (this.ThisLock)
            {
                WeakReference reference;
                InstanceOwner target;
                if (this.owners.TryGetValue(instanceOwnerId, out reference))
                {
                    target = (InstanceOwner) reference.Target;
                    if (target != null)
                    {
                        if (target.OwnerToken != lockToken)
                        {
                            throw Fx.Exception.AsError(new InvalidOperationException(SRCore.StoreReportedConflictingLockTokens));
                        }
                    }
                    else
                    {
                        target = new InstanceOwner(instanceOwnerId, lockToken);
                        reference.Target = target;
                    }
                }
                else
                {
                    target = new InstanceOwner(instanceOwnerId, lockToken);
                    this.owners.Add(instanceOwnerId, new WeakReference(target));
                }
            Label_007F:
                if (this.ownerKeysToScan.Length == this.ownerKeysIndexToScan)
                {
                    this.ownerKeysToScan = new Guid[this.owners.Count];
                    this.owners.Keys.CopyTo(this.ownerKeysToScan, 0);
                    this.ownerKeysIndexToScan = 0;
                }
                else
                {
                    Guid key = this.ownerKeysToScan[this.ownerKeysIndexToScan++];
                    if (!this.owners.TryGetValue(key, out reference))
                    {
                        goto Label_007F;
                    }
                    if (reference.Target == null)
                    {
                        this.owners.Remove(key);
                        goto Label_007F;
                    }
                }
                return target;
            }
        }

        private InstanceNormalEvent GetOwnerEventHelper(InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
        {
            InstanceNormalEvent event2;
            if (!owner.Events.TryGetValue(persistenceEvent.Name, out event2))
            {
                event2 = new InstanceNormalEvent(persistenceEvent);
                owner.Events.Add(persistenceEvent.Name, event2);
            }
            return event2;
        }

        protected virtual void OnFreeInstanceHandle(InstanceHandle instanceHandle, object userContext)
        {
        }

        protected virtual object OnNewInstanceHandle(InstanceHandle instanceHandle)
        {
            return null;
        }

        internal void PendHandleToEvent(InstanceHandle handle, InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
        {
            lock (this.ThisLock)
            {
                this.GetOwnerEventHelper(persistenceEvent, owner).PendingHandles.Add(handle);
            }
        }

        private InstanceHandle PrepareInstanceHandle(InstanceHandle handle)
        {
            handle.ProviderObject = this.OnNewInstanceHandle(handle);
            return handle;
        }

        internal void RemoveHandleFromEvents(InstanceHandle handle, IEnumerable<XName> eventNames, InstanceOwner owner)
        {
            Func<XName, InstanceNormalEvent> selector = null;
            lock (this.ThisLock)
            {
                if (selector == null)
                {
                    selector = name => owner.Events[name];
                }
                foreach (InstanceNormalEvent event2 in eventNames.Select<XName, InstanceNormalEvent>(selector))
                {
                    event2.PendingHandles.Remove(handle);
                    event2.BoundHandles.Remove(handle);
                    if ((!event2.IsSignaled && (event2.BoundHandles.Count == 0)) && (event2.PendingHandles.Count == 0))
                    {
                        owner.Events.Remove(event2.Name);
                    }
                }
            }
        }

        protected void ResetEvent(InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
        {
            if (persistenceEvent == null)
            {
                throw Fx.Exception.ArgumentNull("persistenceEvent");
            }
            if (owner == null)
            {
                throw Fx.Exception.ArgumentNull("owner");
            }
            lock (this.ThisLock)
            {
                InstanceNormalEvent event2;
                WeakReference reference;
                if (!this.owners.TryGetValue(owner.InstanceOwnerId, out reference) || !object.ReferenceEquals(reference.Target, owner))
                {
                    throw Fx.Exception.Argument("owner", SRCore.OwnerBelongsToWrongStore);
                }
                if (owner.Events.TryGetValue(persistenceEvent.Name, out event2) && event2.IsSignaled)
                {
                    event2.IsSignaled = false;
                    if ((event2.BoundHandles.Count == 0) && (event2.PendingHandles.Count == 0))
                    {
                        owner.Events.Remove(persistenceEvent.Name);
                    }
                }
            }
        }

        internal List<InstancePersistenceEvent> SelectSignaledEvents(IEnumerable<XName> eventNames, InstanceOwner owner)
        {
            Func<XName, InstanceNormalEvent> selector = null;
            List<InstancePersistenceEvent> list = null;
            lock (this.ThisLock)
            {
                if (selector == null)
                {
                    selector = name => owner.Events[name];
                }
                foreach (InstanceNormalEvent event2 in eventNames.Select<XName, InstanceNormalEvent>(selector))
                {
                    if (event2.IsSignaled)
                    {
                        if (list == null)
                        {
                            list = new List<InstancePersistenceEvent>(1);
                        }
                        list.Add(event2);
                    }
                }
            }
            return list;
        }

        protected void SignalEvent(InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
        {
            InstanceNormalEvent ownerEventHelper;
            if (persistenceEvent == null)
            {
                throw Fx.Exception.ArgumentNull("persistenceEvent");
            }
            if (owner == null)
            {
                throw Fx.Exception.ArgumentNull("owner");
            }
            InstanceHandle[] handleArray = null;
            lock (this.ThisLock)
            {
                WeakReference reference;
                if (!this.owners.TryGetValue(owner.InstanceOwnerId, out reference) || !object.ReferenceEquals(reference.Target, owner))
                {
                    throw Fx.Exception.Argument("owner", SRCore.OwnerBelongsToWrongStore);
                }
                ownerEventHelper = this.GetOwnerEventHelper(persistenceEvent, owner);
                if (!ownerEventHelper.IsSignaled)
                {
                    ownerEventHelper.IsSignaled = true;
                    if (ownerEventHelper.BoundHandles.Count > 0)
                    {
                        handleArray = ownerEventHelper.BoundHandles.ToArray<InstanceHandle>();
                    }
                }
            }
            if (handleArray != null)
            {
                foreach (InstanceHandle handle in handleArray)
                {
                    handle.EventReady(ownerEventHelper);
                }
            }
        }

        protected internal virtual bool TryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout)
        {
            return this.EndTryCommand(this.BeginTryCommand(context, command, timeout, null, null));
        }

        public List<InstancePersistenceEvent> WaitForEvents(InstanceHandle handle, TimeSpan timeout)
        {
            return this.EndWaitForEvents(this.BeginWaitForEvents(handle, timeout, null, null));
        }

        public InstanceOwner DefaultInstanceOwner
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<DefaultInstanceOwner>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<DefaultInstanceOwner>k__BackingField = value;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.owners;
            }
        }
    }
}

