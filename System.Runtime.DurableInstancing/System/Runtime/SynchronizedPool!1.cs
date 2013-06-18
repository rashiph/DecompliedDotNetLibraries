namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    internal class SynchronizedPool<T> where T: class
    {
        private Entry<T>[] entries;
        private GlobalPool<T> globalPool;
        private int maxCount;
        private const int maxPendingEntries = 0x80;
        private const int maxPromotionFailures = 0x40;
        private const int maxReturnsBeforePromotion = 0x40;
        private const int maxThreadItemsPerProcessor = 0x10;
        private PendingEntry<T>[] pending;
        private int promotionFailures;

        public SynchronizedPool(int maxCount)
        {
            int num = maxCount;
            int num2 = 0x10 + SynchronizedPoolHelper<T>.ProcessorCount;
            if (num > num2)
            {
                num = num2;
            }
            this.maxCount = maxCount;
            this.entries = new Entry<T>[num];
            this.pending = new PendingEntry<T>[4];
            this.globalPool = new GlobalPool<T>(maxCount);
        }

        public void Clear()
        {
            Entry<T>[] entries = this.entries;
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i].value = default(T);
            }
            this.globalPool.Clear();
        }

        private void HandlePromotionFailure(int thisThreadID)
        {
            int num = this.promotionFailures + 1;
            if (num >= 0x40)
            {
                lock (this.ThisLock)
                {
                    this.entries = new Entry<T>[this.entries.Length];
                    this.globalPool.MaxCount = this.maxCount;
                }
                this.PromoteThread(thisThreadID);
            }
            else
            {
                this.promotionFailures = num;
            }
        }

        private bool PromoteThread(int thisThreadID)
        {
            lock (this.ThisLock)
            {
                for (int i = 0; i < this.entries.Length; i++)
                {
                    int threadID = this.entries[i].threadID;
                    if (threadID == thisThreadID)
                    {
                        return true;
                    }
                    if (threadID == 0)
                    {
                        this.globalPool.DecrementMaxCount();
                        this.entries[i].threadID = thisThreadID;
                        return true;
                    }
                }
            }
            return false;
        }

        private void RecordReturnToGlobalPool(int thisThreadID)
        {
            PendingEntry<T>[] pending = this.pending;
            for (int i = 0; i < pending.Length; i++)
            {
                int threadID = pending[i].threadID;
                if (threadID == thisThreadID)
                {
                    int num3 = pending[i].returnCount + 1;
                    if (num3 < 0x40)
                    {
                        pending[i].returnCount = num3;
                        return;
                    }
                    pending[i].returnCount = 0;
                    if (!this.PromoteThread(thisThreadID))
                    {
                        this.HandlePromotionFailure(thisThreadID);
                        return;
                    }
                    break;
                }
                if (threadID == 0)
                {
                    return;
                }
            }
        }

        private void RecordTakeFromGlobalPool(int thisThreadID)
        {
            PendingEntry<T>[] pending = this.pending;
            for (int i = 0; i < pending.Length; i++)
            {
                int threadID = pending[i].threadID;
                if (threadID == thisThreadID)
                {
                    return;
                }
                if (threadID == 0)
                {
                    lock (pending)
                    {
                        if (pending[i].threadID == 0)
                        {
                            pending[i].threadID = thisThreadID;
                            return;
                        }
                    }
                }
            }
            if (pending.Length >= 0x80)
            {
                this.pending = new PendingEntry<T>[pending.Length];
            }
            else
            {
                PendingEntry<T>[] destinationArray = new PendingEntry<T>[pending.Length * 2];
                Array.Copy(pending, destinationArray, pending.Length);
                this.pending = destinationArray;
            }
        }

        public bool Return(T value)
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (managedThreadId == 0)
            {
                return false;
            }
            return (this.ReturnToPerThreadPool(managedThreadId, value) || this.ReturnToGlobalPool(managedThreadId, value));
        }

        private bool ReturnToGlobalPool(int thisThreadID, T value)
        {
            this.RecordReturnToGlobalPool(thisThreadID);
            return this.globalPool.Return(value);
        }

        private bool ReturnToPerThreadPool(int thisThreadID, T value)
        {
            Entry<T>[] entries = this.entries;
            for (int i = 0; i < entries.Length; i++)
            {
                int threadID = entries[i].threadID;
                if (threadID == thisThreadID)
                {
                    if (entries[i].value == null)
                    {
                        entries[i].value = value;
                        return true;
                    }
                    return false;
                }
                if (threadID == 0)
                {
                    break;
                }
            }
            return false;
        }

        public T Take()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (managedThreadId == 0)
            {
                return default(T);
            }
            T local = this.TakeFromPerThreadPool(managedThreadId);
            if (local != null)
            {
                return local;
            }
            return this.TakeFromGlobalPool(managedThreadId);
        }

        private T TakeFromGlobalPool(int thisThreadID)
        {
            this.RecordTakeFromGlobalPool(thisThreadID);
            return this.globalPool.Take();
        }

        private T TakeFromPerThreadPool(int thisThreadID)
        {
            Entry<T>[] entries = this.entries;
            for (int i = 0; i < entries.Length; i++)
            {
                int threadID = entries[i].threadID;
                if (threadID == thisThreadID)
                {
                    T local = entries[i].value;
                    if (local != null)
                    {
                        entries[i].value = default(T);
                        return local;
                    }
                    return default(T);
                }
                if (threadID == 0)
                {
                    break;
                }
            }
            return default(T);
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            public int threadID;
            public T value;
        }

        private class GlobalPool
        {
            private Stack<T> items;
            private int maxCount;

            public GlobalPool(int maxCount)
            {
                this.items = new Stack<T>();
                this.maxCount = maxCount;
            }

            public void Clear()
            {
                lock (this.ThisLock)
                {
                    this.items.Clear();
                }
            }

            public void DecrementMaxCount()
            {
                lock (this.ThisLock)
                {
                    if (this.items.Count == this.maxCount)
                    {
                        this.items.Pop();
                    }
                    this.maxCount--;
                }
            }

            public bool Return(T value)
            {
                if (this.items.Count < this.MaxCount)
                {
                    lock (this.ThisLock)
                    {
                        if (this.items.Count < this.MaxCount)
                        {
                            this.items.Push(value);
                            return true;
                        }
                    }
                }
                return false;
            }

            public T Take()
            {
                if (this.items.Count > 0)
                {
                    lock (this.ThisLock)
                    {
                        if (this.items.Count > 0)
                        {
                            return this.items.Pop();
                        }
                    }
                }
                return default(T);
            }

            public int MaxCount
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.maxCount;
                }
                set
                {
                    lock (this.ThisLock)
                    {
                        while (this.items.Count > value)
                        {
                            this.items.Pop();
                        }
                        this.maxCount = value;
                    }
                }
            }

            private object ThisLock
            {
                get
                {
                    return this;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PendingEntry
        {
            public int returnCount;
            public int threadID;
        }

        private static class SynchronizedPoolHelper
        {
            public static readonly int ProcessorCount;

            static SynchronizedPoolHelper()
            {
                SynchronizedPool<T>.SynchronizedPoolHelper.ProcessorCount = SynchronizedPool<T>.SynchronizedPoolHelper.GetProcessorCount();
            }

            [SecuritySafeCritical, EnvironmentPermission(SecurityAction.Assert, Read="NUMBER_OF_PROCESSORS")]
            private static int GetProcessorCount()
            {
                return Environment.ProcessorCount;
            }
        }
    }
}

