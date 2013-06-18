namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class InstanceOwner
    {
        private readonly Dictionary<Guid, InstanceHandle> boundHandles = new Dictionary<Guid, InstanceHandle>();
        private readonly Dictionary<XName, InstanceNormalEvent> events = new Dictionary<XName, InstanceNormalEvent>(1);
        private readonly Queue<InstanceHandleReference> inProgressHandles = new Queue<InstanceHandleReference>();
        private readonly Dictionary<Guid, Queue<InstanceHandleReference>> inProgressHandlesPerInstance = new Dictionary<Guid, Queue<InstanceHandleReference>>();

        internal InstanceOwner(Guid ownerId, Guid lockToken)
        {
            this.InstanceOwnerId = ownerId;
            this.OwnerToken = lockToken;
        }

        internal void CancelBind(ref InstanceHandleReference reference, ref List<InstanceHandleReference> handlesPendingResolution)
        {
            lock (this.HandlesLock)
            {
                this.CancelReference(ref reference, ref handlesPendingResolution);
            }
        }

        private void CancelReference(ref InstanceHandleReference reference, ref List<InstanceHandleReference> handlesPendingResolution)
        {
            Queue<InstanceHandleReference> queue;
            Guid id = reference.InstanceHandle.Id;
            try
            {
                LockResolutionMarker item = reference as LockResolutionMarker;
                if ((item != null) && !item.IsComplete)
                {
                    if (handlesPendingResolution == null)
                    {
                        handlesPendingResolution = new List<InstanceHandleReference>(1);
                    }
                    handlesPendingResolution.Add(item);
                }
            }
            finally
            {
                reference.Cancel();
                reference = null;
            }
            this.ProcessInProgressHandles(ref handlesPendingResolution);
            if ((id != Guid.Empty) && this.InProgressHandlesPerInstance.TryGetValue(id, out queue))
            {
                while (queue.Count > 0)
                {
                    InstanceHandleReference handleRef = queue.Peek();
                    if ((handleRef.InstanceHandle != null) && this.CheckOldestReference(handleRef, ref handlesPendingResolution))
                    {
                        break;
                    }
                    queue.Dequeue();
                }
                if (queue.Count == 0)
                {
                    this.InProgressHandlesPerInstance.Remove(id);
                }
            }
        }

        private bool CheckOldestReference(InstanceHandleReference handleRef, ref List<InstanceHandleReference> handlesPendingResolution)
        {
            bool flag2;
            LockResolutionMarker item = handleRef as LockResolutionMarker;
            if ((item == null) || item.IsComplete)
            {
                return true;
            }
            bool success = true;
            try
            {
                InstanceHandle handle;
                if (this.BoundHandles.TryGetValue(item.InstanceHandle.Id, out handle))
                {
                    Fx.AssertAndFailFast(!object.ReferenceEquals(handle, item.InstanceHandle), "InstanceStore lock state is not correct in CheckOldestReference.");
                    if ((handle.Version <= 0L) || (item.InstanceVersion <= 0L))
                    {
                        if ((handle.Version != 0L) || (item.InstanceVersion != 0L))
                        {
                            item.Reason = new InvalidOperationException(SRCore.InvalidLockToken);
                            success = false;
                        }
                        else
                        {
                            item.ConflictingHandle = handle;
                            success = false;
                        }
                    }
                    else if (handle.Version >= item.InstanceVersion)
                    {
                        item.ConflictingHandle = handle;
                        success = false;
                    }
                }
                flag2 = success;
            }
            finally
            {
                item.NotifyMarkerComplete(success);
                if (handlesPendingResolution == null)
                {
                    handlesPendingResolution = new List<InstanceHandleReference>(1);
                }
                handlesPendingResolution.Add(item);
            }
            return flag2;
        }

        private void EnqueueReference(InstanceHandleReference handleRef)
        {
            if (this.InProgressHandles.Count > 0)
            {
                this.InProgressHandles.Enqueue(handleRef);
            }
            else if (handleRef.InstanceHandle.Id != Guid.Empty)
            {
                Queue<InstanceHandleReference> queue;
                if (!this.InProgressHandlesPerInstance.TryGetValue(handleRef.InstanceHandle.Id, out queue))
                {
                    queue = new Queue<InstanceHandleReference>(2);
                    this.InProgressHandlesPerInstance.Add(handleRef.InstanceHandle.Id, queue);
                }
                queue.Enqueue(handleRef);
            }
            else
            {
                this.InProgressHandles.Enqueue(handleRef);
            }
        }

        internal void FaultBind(ref InstanceHandleReference reference, ref List<InstanceHandleReference> handlesPendingResolution, Exception reason)
        {
            lock (this.HandlesLock)
            {
                LockResolutionMarker item = reference as LockResolutionMarker;
                if ((item != null) && !item.IsComplete)
                {
                    try
                    {
                    }
                    finally
                    {
                        item.Reason = reason ?? new OperationCanceledException(SRCore.HandleFreed);
                        item.NotifyMarkerComplete(false);
                        if (handlesPendingResolution == null)
                        {
                            handlesPendingResolution = new List<InstanceHandleReference>(1);
                        }
                        handlesPendingResolution.Add(item);
                    }
                }
            }
        }

        internal bool FinishBind(ref InstanceHandleReference reference, ref long instanceVersion, ref List<InstanceHandleReference> handlesPendingResolution)
        {
            bool flag2;
            lock (this.HandlesLock)
            {
                LockResolutionMarker marker = (LockResolutionMarker) reference;
                Fx.AssertAndThrow(marker.IsComplete, "Called FinishBind prematurely.");
                if (marker.NonConflicting)
                {
                    instanceVersion = marker.InstanceVersion;
                    flag2 = true;
                }
                else
                {
                    try
                    {
                        if (marker.Reason != null)
                        {
                            throw Fx.Exception.AsError(marker.Reason);
                        }
                        marker.InstanceHandle.ConflictingHandle = marker.ConflictingHandle;
                        flag2 = false;
                    }
                    finally
                    {
                        this.CancelReference(ref reference, ref handlesPendingResolution);
                    }
                }
            }
            return flag2;
        }

        internal AsyncWaitHandle InitiateLockResolution(long instanceVersion, ref InstanceHandleReference reference, ref List<InstanceHandleReference> handlesPendingResolution)
        {
            AsyncWaitHandle markerWaitHandle;
            Fx.AssertAndThrow(!(reference is LockResolutionMarker), "InitiateLockResolution already called.");
            lock (this.HandlesLock)
            {
                InstanceHandleReference reference2 = reference;
                LockResolutionMarker handleRef = null;
                try
                {
                    InstanceHandle handle;
                    if (this.BoundHandles.TryGetValue(reference.InstanceHandle.Id, out handle))
                    {
                        Fx.AssertAndFailFast(!object.ReferenceEquals(handle, reference.InstanceHandle), "InstanceStore lock state is not correct in InitiateLockResolution.");
                        if ((handle.Version <= 0L) || (instanceVersion <= 0L))
                        {
                            if ((handle.Version != 0L) || (instanceVersion != 0L))
                            {
                                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InvalidLockToken));
                            }
                            reference.InstanceHandle.ConflictingHandle = handle;
                            return null;
                        }
                        if (handle.Version >= instanceVersion)
                        {
                            reference.InstanceHandle.ConflictingHandle = handle;
                            return null;
                        }
                    }
                    handleRef = new LockResolutionMarker(reference.InstanceHandle, instanceVersion);
                    this.EnqueueReference(handleRef);
                    reference = handleRef;
                    markerWaitHandle = handleRef.MarkerWaitHandle;
                }
                finally
                {
                    if (!object.ReferenceEquals(handleRef, reference))
                    {
                        this.CancelReference(ref reference, ref handlesPendingResolution);
                        if (handleRef != null)
                        {
                            reference2 = handleRef;
                            this.CancelReference(ref reference2, ref handlesPendingResolution);
                        }
                    }
                    else
                    {
                        this.CancelReference(ref reference2, ref handlesPendingResolution);
                    }
                }
            }
            return markerWaitHandle;
        }

        internal void InstanceBound(ref InstanceHandleReference reference, ref List<InstanceHandleReference> handlesPendingResolution)
        {
            Fx.AssertAndThrow(!(reference is LockResolutionMarker), "InstanceBound called after trying to bind the lock version, which alredy required an instance.");
            lock (this.HandlesLock)
            {
                this.ProcessInProgressHandles(ref handlesPendingResolution);
            }
        }

        private void ProcessInProgressHandles(ref List<InstanceHandleReference> handlesPendingResolution)
        {
            while (this.InProgressHandles.Count > 0)
            {
                InstanceHandleReference handleRef = this.InProgressHandles.Peek();
                if (handleRef.InstanceHandle != null)
                {
                    Queue<InstanceHandleReference> queue;
                    if (handleRef.InstanceHandle.Id == Guid.Empty)
                    {
                        return;
                    }
                    if (!this.InProgressHandlesPerInstance.TryGetValue(handleRef.InstanceHandle.Id, out queue))
                    {
                        if (this.CheckOldestReference(handleRef, ref handlesPendingResolution))
                        {
                            queue = new Queue<InstanceHandleReference>(2);
                            queue.Enqueue(handleRef);
                            this.InProgressHandlesPerInstance.Add(handleRef.InstanceHandle.Id, queue);
                        }
                    }
                    else
                    {
                        queue.Enqueue(handleRef);
                    }
                }
                this.InProgressHandles.Dequeue();
            }
        }

        internal static void ResolveHandles(List<InstanceHandleReference> handlesPendingResolution)
        {
            if (handlesPendingResolution != null)
            {
                foreach (InstanceHandleReference reference in handlesPendingResolution)
                {
                    LockResolutionMarker marker = reference as LockResolutionMarker;
                    marker.MarkerWaitHandle.Set();
                }
            }
        }

        internal void StartBind(InstanceHandle handle, ref InstanceHandleReference reference)
        {
            lock (this.HandlesLock)
            {
                reference = new InstanceHandleReference(handle);
                this.EnqueueReference(reference);
            }
        }

        internal bool TryCompleteBind(ref InstanceHandleReference reference, ref List<InstanceHandleReference> handlesPendingResolution, out InstanceHandle handleToFree)
        {
            bool flag2;
            handleToFree = null;
            lock (this.HandlesLock)
            {
                try
                {
                    InstanceHandle handle;
                    if (this.BoundHandles.TryGetValue(reference.InstanceHandle.Id, out handle))
                    {
                        Fx.AssertAndFailFast(!object.ReferenceEquals(handle, reference.InstanceHandle), "InstanceStore lock state is not correct.");
                        if ((handle.Version <= 0L) || (reference.InstanceHandle.Version <= 0L))
                        {
                            if ((handle.Version != 0L) || (reference.InstanceHandle.Version != 0L))
                            {
                                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InvalidLockToken));
                            }
                            reference.InstanceHandle.ConflictingHandle = handle;
                            return false;
                        }
                        if (handle.Version > reference.InstanceHandle.Version)
                        {
                            reference.InstanceHandle.ConflictingHandle = handle;
                            return false;
                        }
                        if (handle.Version < reference.InstanceHandle.Version)
                        {
                            handle.ConflictingHandle = reference.InstanceHandle;
                            handleToFree = handle;
                            this.BoundHandles[reference.InstanceHandle.Id] = reference.InstanceHandle;
                            return true;
                        }
                        if (handle.Version == reference.InstanceHandle.Version)
                        {
                            throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceStoreBoundSameVersionTwice));
                        }
                        throw Fx.AssertAndThrow("All cases covered above.");
                    }
                    this.BoundHandles.Add(reference.InstanceHandle.Id, reference.InstanceHandle);
                    flag2 = true;
                }
                finally
                {
                    this.CancelReference(ref reference, ref handlesPendingResolution);
                }
            }
            return flag2;
        }

        internal void Unbind(InstanceHandle handle)
        {
            lock (this.HandlesLock)
            {
                InstanceHandle handle2;
                if (this.BoundHandles.TryGetValue(handle.Id, out handle2) && object.ReferenceEquals(handle, handle2))
                {
                    this.BoundHandles.Remove(handle.Id);
                }
            }
        }

        private Dictionary<Guid, InstanceHandle> BoundHandles
        {
            get
            {
                return this.boundHandles;
            }
        }

        internal Dictionary<XName, InstanceNormalEvent> Events
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.events;
            }
        }

        private object HandlesLock
        {
            get
            {
                return this.boundHandles;
            }
        }

        private Queue<InstanceHandleReference> InProgressHandles
        {
            get
            {
                return this.inProgressHandles;
            }
        }

        private Dictionary<Guid, Queue<InstanceHandleReference>> InProgressHandlesPerInstance
        {
            get
            {
                return this.inProgressHandlesPerInstance;
            }
        }

        public Guid InstanceOwnerId
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceOwnerId>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<InstanceOwnerId>k__BackingField = value;
            }
        }

        internal Guid OwnerToken
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<OwnerToken>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<OwnerToken>k__BackingField = value;
            }
        }

        private class LockResolutionMarker : InstanceHandleReference
        {
            private AsyncWaitHandle waitHandle;

            internal LockResolutionMarker(InstanceHandle instanceHandle, long instanceVersion) : base(instanceHandle)
            {
                this.waitHandle = new AsyncWaitHandle(EventResetMode.ManualReset);
                this.InstanceVersion = instanceVersion;
            }

            internal void NotifyMarkerComplete(bool success)
            {
                this.NonConflicting = success;
                this.IsComplete = true;
            }

            internal InstanceHandle ConflictingHandle { get; set; }

            internal long InstanceVersion { get; private set; }

            internal bool IsComplete { get; private set; }

            internal AsyncWaitHandle MarkerWaitHandle
            {
                get
                {
                    return this.waitHandle;
                }
            }

            internal bool NonConflicting { get; private set; }

            internal Exception Reason { get; set; }
        }
    }
}

