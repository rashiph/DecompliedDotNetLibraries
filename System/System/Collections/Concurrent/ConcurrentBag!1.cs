namespace System.Collections.Concurrent
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, DebuggerTypeProxy(typeof(SystemThreadingCollection_IProducerConsumerCollectionDebugView<>)), DebuggerDisplay("Count = {Count}"), ComVisible(false), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class ConcurrentBag<T> : IProducerConsumerCollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        [NonSerialized]
        private object m_globalListsLock;
        [NonSerialized]
        private volatile ThreadLocalList<T> m_headList;
        [NonSerialized]
        private ThreadLocal<ThreadLocalList<T>> m_locals;
        [NonSerialized]
        private bool m_needSync;
        private T[] m_serializationArray;
        [NonSerialized]
        private volatile ThreadLocalList<T> m_tailList;

        public ConcurrentBag()
        {
            this.Initialize(null);
        }

        public ConcurrentBag(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection", SR.GetString("ConcurrentBag_Ctor_ArgumentNullException"));
            }
            this.Initialize(collection);
        }

        private void AcquireAllLocks()
        {
            bool lockTaken = false;
            for (ThreadLocalList<T> list = this.m_headList; list != null; list = list.m_nextList)
            {
                try
                {
                    Monitor.Enter(list, ref lockTaken);
                }
                finally
                {
                    if (lockTaken)
                    {
                        list.m_lockTaken = true;
                        lockTaken = false;
                    }
                }
            }
        }

        public void Add(T item)
        {
            ThreadLocalList<T> threadList = this.GetThreadList(true);
            this.AddInternal(threadList, item);
        }

        private void AddInternal(ThreadLocalList<T> list, T item)
        {
            bool lockTaken = false;
            try
            {
                Interlocked.Exchange(ref list.m_currentOp, 1);
                if ((list.Count < 2) || this.m_needSync)
                {
                    list.m_currentOp = 0;
                    Monitor.Enter(list, ref lockTaken);
                }
                list.Add(item, lockTaken);
            }
            finally
            {
                list.m_currentOp = 0;
                if (lockTaken)
                {
                    Monitor.Exit(list);
                }
            }
        }

        private bool CanSteal(ThreadLocalList<T> list)
        {
            if ((list.Count <= 2) && (list.m_currentOp != 0))
            {
                SpinWait wait = new SpinWait();
                while (list.m_currentOp != 0)
                {
                    wait.SpinOnce();
                }
            }
            return (list.Count > 0);
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", SR.GetString("ConcurrentBag_CopyTo_ArgumentNullException"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", SR.GetString("ConcurrentBag_CopyTo_ArgumentOutOfRangeException"));
            }
            if (this.m_headList != null)
            {
                bool lockTaken = false;
                try
                {
                    this.FreezeBag(ref lockTaken);
                    this.ToList().CopyTo(array, index);
                }
                finally
                {
                    this.UnfreezeBag(lockTaken);
                }
            }
        }

        private void FreezeBag(ref bool lockTaken)
        {
            Monitor.Enter(this.m_globalListsLock, ref lockTaken);
            this.m_needSync = true;
            this.AcquireAllLocks();
            this.WaitAllOperations();
        }

        private int GetCountInternal()
        {
            int num = 0;
            for (ThreadLocalList<T> list = this.m_headList; list != null; list = list.m_nextList)
            {
                num += list.Count;
            }
            return num;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>) this.ToArray()).GetEnumerator();
        }

        private ThreadLocalList<T> GetThreadList(bool forceCreate)
        {
            ThreadLocalList<T> unownedList = this.m_locals.Value;
            if (unownedList != null)
            {
                return unownedList;
            }
            if (forceCreate)
            {
                lock (this.m_globalListsLock)
                {
                    if (this.m_headList == null)
                    {
                        unownedList = new ThreadLocalList<T>(Thread.CurrentThread);
                        this.m_headList = unownedList;
                        this.m_tailList = unownedList;
                    }
                    else
                    {
                        unownedList = this.GetUnownedList();
                        if (unownedList == null)
                        {
                            unownedList = new ThreadLocalList<T>(Thread.CurrentThread);
                            this.m_tailList.m_nextList = unownedList;
                            this.m_tailList = unownedList;
                        }
                    }
                    this.m_locals.Value = unownedList;
                    return unownedList;
                }
            }
            return null;
        }

        private ThreadLocalList<T> GetUnownedList()
        {
            for (ThreadLocalList<T> list = this.m_headList; list != null; list = list.m_nextList)
            {
                if (list.m_ownerThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    list.m_ownerThread = Thread.CurrentThread;
                    return list;
                }
            }
            return null;
        }

        private void Initialize(IEnumerable<T> collection)
        {
            this.m_locals = new ThreadLocal<ThreadLocalList<T>>();
            this.m_globalListsLock = new object();
            if (collection != null)
            {
                ThreadLocalList<T> threadList = this.GetThreadList(true);
                foreach (T local in collection)
                {
                    this.AddInternal(threadList, local);
                }
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.m_locals = new ThreadLocal<ThreadLocalList<T>>();
            this.m_globalListsLock = new object();
            ThreadLocalList<T> threadList = this.GetThreadList(true);
            foreach (T local in this.m_serializationArray)
            {
                this.AddInternal(threadList, local);
            }
            this.m_headList = threadList;
            this.m_tailList = threadList;
            this.m_serializationArray = null;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            this.m_serializationArray = this.ToArray();
        }

        private void ReleaseAllLocks()
        {
            for (ThreadLocalList<T> list = this.m_headList; list != null; list = list.m_nextList)
            {
                if (list.m_lockTaken)
                {
                    list.m_lockTaken = false;
                    Monitor.Exit(list);
                }
            }
        }

        private bool Steal(out T result, bool take)
        {
            bool flag;
            if (take)
            {
                CDSCollectionETWBCLProvider.Log.ConcurrentBag_TryTakeSteals();
            }
            else
            {
                CDSCollectionETWBCLProvider.Log.ConcurrentBag_TryPeekSteals();
            }
            do
            {
                ThreadLocalList<T> headList;
                flag = false;
                List<int> list = new List<int>();
                for (headList = this.m_headList; headList != null; headList = headList.m_nextList)
                {
                    list.Add(headList.m_version);
                    if ((headList.m_head != null) && this.TrySteal(headList, out result, take))
                    {
                        return true;
                    }
                }
                headList = this.m_headList;
                foreach (int num in list)
                {
                    if (num != headList.m_version)
                    {
                        flag = true;
                        if ((headList.m_head != null) && this.TrySteal(headList, out result, take))
                        {
                            return true;
                        }
                    }
                    headList = headList.m_nextList;
                }
            }
            while (flag);
            result = default(T);
            return false;
        }

        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            this.Add(item);
            return true;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", SR.GetString("ConcurrentBag_CopyTo_ArgumentNullException"));
            }
            bool lockTaken = false;
            try
            {
                this.FreezeBag(ref lockTaken);
                this.ToList().CopyTo(array, index);
            }
            finally
            {
                this.UnfreezeBag(lockTaken);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public T[] ToArray()
        {
            T[] localArray;
            if (this.m_headList == null)
            {
                return new T[0];
            }
            bool lockTaken = false;
            try
            {
                this.FreezeBag(ref lockTaken);
                localArray = this.ToList().ToArray();
            }
            finally
            {
                this.UnfreezeBag(lockTaken);
            }
            return localArray;
        }

        private List<T> ToList()
        {
            List<T> list = new List<T>();
            for (ThreadLocalList<T> list2 = this.m_headList; list2 != null; list2 = list2.m_nextList)
            {
                for (Node<T> node = list2.m_head; node != null; node = node.m_next)
                {
                    list.Add(node.m_value);
                }
            }
            return list;
        }

        public bool TryPeek(out T result)
        {
            return this.TryTakeOrPeek(out result, false);
        }

        private bool TrySteal(ThreadLocalList<T> list, out T result, bool take)
        {
            lock (list)
            {
                if (this.CanSteal(list))
                {
                    list.Steal(out result, take);
                    return true;
                }
                result = default(T);
                return false;
            }
        }

        public bool TryTake(out T result)
        {
            return this.TryTakeOrPeek(out result, true);
        }

        private bool TryTakeOrPeek(out T result, bool take)
        {
            ThreadLocalList<T> threadList = this.GetThreadList(false);
            if ((threadList == null) || (threadList.Count == 0))
            {
                return this.Steal(out result, take);
            }
            bool lockTaken = false;
            try
            {
                if (take)
                {
                    Interlocked.Exchange(ref threadList.m_currentOp, 2);
                    if ((threadList.Count <= 2) || this.m_needSync)
                    {
                        threadList.m_currentOp = 0;
                        Monitor.Enter(threadList, ref lockTaken);
                        if (threadList.Count == 0)
                        {
                            if (lockTaken)
                            {
                                try
                                {
                                }
                                finally
                                {
                                    lockTaken = false;
                                    Monitor.Exit(threadList);
                                }
                            }
                            return this.Steal(out result, true);
                        }
                    }
                    threadList.Remove(out result);
                }
                else if (!threadList.Peek(out result))
                {
                    return this.Steal(out result, false);
                }
            }
            finally
            {
                threadList.m_currentOp = 0;
                if (lockTaken)
                {
                    Monitor.Exit(threadList);
                }
            }
            return true;
        }

        private void UnfreezeBag(bool lockTaken)
        {
            this.ReleaseAllLocks();
            this.m_needSync = false;
            if (lockTaken)
            {
                Monitor.Exit(this.m_globalListsLock);
            }
        }

        private void WaitAllOperations()
        {
            for (ThreadLocalList<T> list = this.m_headList; list != null; list = list.m_nextList)
            {
                if (list.m_currentOp != 0)
                {
                    SpinWait wait = new SpinWait();
                    while (list.m_currentOp != 0)
                    {
                        wait.SpinOnce();
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                int countInternal;
                if (this.m_headList == null)
                {
                    return 0;
                }
                bool lockTaken = false;
                try
                {
                    this.FreezeBag(ref lockTaken);
                    countInternal = this.GetCountInternal();
                }
                finally
                {
                    this.UnfreezeBag(lockTaken);
                }
                return countInternal;
            }
        }

        public bool IsEmpty
        {
            get
            {
                bool flag2;
                if (this.m_headList == null)
                {
                    return true;
                }
                bool lockTaken = false;
                try
                {
                    this.FreezeBag(ref lockTaken);
                    for (ThreadLocalList<T> list = this.m_headList; list != null; list = list.m_nextList)
                    {
                        if (list.m_head != null)
                        {
                            return false;
                        }
                    }
                    flag2 = true;
                }
                finally
                {
                    this.UnfreezeBag(lockTaken);
                }
                return flag2;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException(SR.GetString("ConcurrentCollection_SyncRoot_NotSupported"));
            }
        }

        internal enum ListOperation
        {
            public const ConcurrentBag<T>.ListOperation Add = ConcurrentBag<T>.ListOperation.Add;,
            public const ConcurrentBag<T>.ListOperation None = ConcurrentBag<T>.ListOperation.None;,
            public const ConcurrentBag<T>.ListOperation Take = ConcurrentBag<T>.ListOperation.Take;
        }

        [Serializable]
        internal class Node
        {
            public ConcurrentBag<T>.Node m_next;
            public ConcurrentBag<T>.Node m_prev;
            public T m_value;

            public Node(T value)
            {
                this.m_value = value;
            }
        }

        internal class ThreadLocalList
        {
            private int m_count;
            internal volatile int m_currentOp;
            internal ConcurrentBag<T>.Node m_head;
            internal bool m_lockTaken;
            internal ConcurrentBag<T>.ThreadLocalList m_nextList;
            internal Thread m_ownerThread;
            internal int m_stealCount;
            private ConcurrentBag<T>.Node m_tail;
            internal volatile int m_version;

            internal ThreadLocalList(Thread ownerThread)
            {
                this.m_ownerThread = ownerThread;
            }

            internal void Add(T item, bool updateCount)
            {
                this.m_count++;
                ConcurrentBag<T>.Node node = new ConcurrentBag<T>.Node(item);
                if (this.m_head == null)
                {
                    this.m_head = node;
                    this.m_tail = node;
                    this.m_version++;
                }
                else
                {
                    node.m_next = this.m_head;
                    this.m_head.m_prev = node;
                    this.m_head = node;
                }
                if (updateCount)
                {
                    this.m_count -= this.m_stealCount;
                    this.m_stealCount = 0;
                }
            }

            internal bool Peek(out T result)
            {
                ConcurrentBag<T>.Node head = this.m_head;
                if (head != null)
                {
                    result = head.m_value;
                    return true;
                }
                result = default(T);
                return false;
            }

            internal void Remove(out T result)
            {
                ConcurrentBag<T>.Node head = this.m_head;
                this.m_head = this.m_head.m_next;
                if (this.m_head != null)
                {
                    this.m_head.m_prev = null;
                }
                else
                {
                    this.m_tail = null;
                }
                this.m_count--;
                result = head.m_value;
            }

            internal void Steal(out T result, bool remove)
            {
                ConcurrentBag<T>.Node tail = this.m_tail;
                if (remove)
                {
                    this.m_tail = this.m_tail.m_prev;
                    if (this.m_tail != null)
                    {
                        this.m_tail.m_next = null;
                    }
                    else
                    {
                        this.m_head = null;
                    }
                    this.m_stealCount++;
                }
                result = tail.m_value;
            }

            internal int Count
            {
                get
                {
                    return (this.m_count - this.m_stealCount);
                }
            }
        }
    }
}

