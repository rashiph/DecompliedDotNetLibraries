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

    [Serializable, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(SystemCollectionsConcurrent_ProducerConsumerCollectionDebugView<>)), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class ConcurrentStack<T> : IProducerConsumerCollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private const int BACKOFF_MAX_YIELDS = 8;
        [NonSerialized]
        private volatile Node<T> m_head;
        private T[] m_serializationArray;

        public ConcurrentStack()
        {
        }

        public ConcurrentStack(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.InitializeFromCollection(collection);
        }

        public void Clear()
        {
            this.m_head = null;
        }

        private void CopyRemovedItems(Node<T> head, T[] collection, int startIndex, int nodesCount)
        {
            Node<T> next = head;
            for (int i = startIndex; i < (startIndex + nodesCount); i++)
            {
                collection[i] = next.m_value;
                next = next.m_next;
            }
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            this.ToList().CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.GetEnumerator(this.m_head);
        }

        private IEnumerator<T> GetEnumerator(Node<T> head)
        {
            Node<T> next = head;
            while (true)
            {
                if (next == null)
                {
                    yield break;
                }
                yield return next.m_value;
                next = next.m_next;
            }
        }

        private void InitializeFromCollection(IEnumerable<T> collection)
        {
            Node<T> node = null;
            foreach (T local in collection)
            {
                node = new Node<T>(local) {
                    m_next = node
                };
            }
            this.m_head = node;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Node<T> node = null;
            Node<T> node2 = null;
            for (int i = 0; i < this.m_serializationArray.Length; i++)
            {
                Node<T> node3 = new Node<T>(this.m_serializationArray[i]);
                if (node == null)
                {
                    node2 = node3;
                }
                else
                {
                    node.m_next = node3;
                }
                node = node3;
            }
            this.m_head = node2;
            this.m_serializationArray = null;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            this.m_serializationArray = this.ToArray();
        }

        public void Push(T item)
        {
            Node<T> node = new Node<T>(item) {
                m_next = this.m_head
            };
            if (Interlocked.CompareExchange<Node<T>>(ref this.m_head, node, node.m_next) != node.m_next)
            {
                this.PushCore(node, node);
            }
        }

        private void PushCore(Node<T> head, Node<T> tail)
        {
            SpinWait wait = new SpinWait();
            do
            {
                wait.SpinOnce();
                tail.m_next = this.m_head;
            }
            while (Interlocked.CompareExchange<Node<T>>(ref this.m_head, head, tail.m_next) != tail.m_next);
            if (CDSCollectionETWBCLProvider.Log.IsEnabled())
            {
                CDSCollectionETWBCLProvider.Log.ConcurrentStack_FastPushFailed(wait.Count);
            }
        }

        public void PushRange(T[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            this.PushRange(items, 0, items.Length);
        }

        public void PushRange(T[] items, int startIndex, int count)
        {
            this.ValidatePushPopRangeInput(items, startIndex, count);
            if (count != 0)
            {
                Node<T> node2;
                Node<T> node = node2 = new Node<T>(items[startIndex]);
                for (int i = startIndex + 1; i < (startIndex + count); i++)
                {
                    node = new Node<T>(items[i]) {
                        m_next = node
                    };
                }
                node2.m_next = this.m_head;
                if (Interlocked.CompareExchange<Node<T>>(ref this.m_head, node, node2.m_next) != node2.m_next)
                {
                    this.PushCore(node, node2);
                }
            }
        }

        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            this.Push(item);
            return true;
        }

        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            return this.TryPop(out item);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            this.ToList().CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public T[] ToArray()
        {
            return this.ToList().ToArray();
        }

        private List<T> ToList()
        {
            List<T> list = new List<T>();
            for (Node<T> node = this.m_head; node != null; node = node.m_next)
            {
                list.Add(node.m_value);
            }
            return list;
        }

        public bool TryPeek(out T result)
        {
            Node<T> head = this.m_head;
            if (head == null)
            {
                result = default(T);
                return false;
            }
            result = head.m_value;
            return true;
        }

        public bool TryPop(out T result)
        {
            Node<T> head = this.m_head;
            if (head == null)
            {
                result = default(T);
                return false;
            }
            if (Interlocked.CompareExchange<Node<T>>(ref this.m_head, head.m_next, head) == head)
            {
                result = head.m_value;
                return true;
            }
            return this.TryPopCore(out result);
        }

        private bool TryPopCore(out T result)
        {
            Node<T> node;
            if (this.TryPopCore(1, out node) == 1)
            {
                result = node.m_value;
                return true;
            }
            result = default(T);
            return false;
        }

        private int TryPopCore(int count, out Node<T> poppedHead)
        {
            SpinWait wait = new SpinWait();
            int num = 1;
            Random random = new Random(Environment.TickCount & 0x7fffffff);
            while (true)
            {
                Node<T> head = this.m_head;
                if (head == null)
                {
                    if ((count == 1) && CDSCollectionETWBCLProvider.Log.IsEnabled())
                    {
                        CDSCollectionETWBCLProvider.Log.ConcurrentStack_FastPopFailed(wait.Count);
                    }
                    poppedHead = null;
                    return 0;
                }
                Node<T> next = head;
                int num2 = 1;
                while ((num2 < count) && (next.m_next != null))
                {
                    next = next.m_next;
                    num2++;
                }
                if (Interlocked.CompareExchange<Node<T>>(ref this.m_head, next.m_next, head) == head)
                {
                    if ((count == 1) && CDSCollectionETWBCLProvider.Log.IsEnabled())
                    {
                        CDSCollectionETWBCLProvider.Log.ConcurrentStack_FastPopFailed(wait.Count);
                    }
                    poppedHead = head;
                    return num2;
                }
                for (int i = 0; i < num; i++)
                {
                    wait.SpinOnce();
                }
                num = wait.NextSpinWillYield ? random.Next(1, 8) : (num * 2);
            }
        }

        public int TryPopRange(T[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            return this.TryPopRange(items, 0, items.Length);
        }

        public int TryPopRange(T[] items, int startIndex, int count)
        {
            Node<T> node;
            this.ValidatePushPopRangeInput(items, startIndex, count);
            if (count == 0)
            {
                return 0;
            }
            int nodesCount = this.TryPopCore(count, out node);
            if (nodesCount > 0)
            {
                this.CopyRemovedItems(node, items, startIndex, nodesCount);
            }
            return nodesCount;
        }

        private void ValidatePushPopRangeInput(T[] items, int startIndex, int count)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ConcurrentStack_PushPopRange_CountOutOfRange"));
            }
            int length = items.Length;
            if ((startIndex >= length) || (startIndex < 0))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ConcurrentStack_PushPopRange_StartOutOfRange"));
            }
            if ((length - count) < startIndex)
            {
                throw new ArgumentException(Environment.GetResourceString("ConcurrentStack_PushPopRange_InvalidCount"));
            }
        }

        public int Count
        {
            get
            {
                int num = 0;
                for (Node<T> node = this.m_head; node != null; node = node.m_next)
                {
                    num++;
                }
                return num;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.m_head == null);
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
                throw new NotSupportedException(Environment.GetResourceString("ConcurrentCollection_SyncRoot_NotSupported"));
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__0 : IEnumerator<T>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private T <>2__current;
            public ConcurrentStack<T> <>4__this;
            public ConcurrentStack<T>.Node <current>5__1;
            public ConcurrentStack<T>.Node head;

            [DebuggerHidden]
            public <GetEnumerator>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<current>5__1 = this.head;
                        break;

                    case 1:
                        this.<>1__state = -1;
                        this.<current>5__1 = this.<current>5__1.m_next;
                        break;

                    default:
                        goto Label_0066;
                }
                if (this.<current>5__1 != null)
                {
                    this.<>2__current = this.<current>5__1.m_value;
                    this.<>1__state = 1;
                    return true;
                }
            Label_0066:
                return false;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            T IEnumerator<T>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        private class Node
        {
            internal ConcurrentStack<T>.Node m_next;
            internal T m_value;

            internal Node(T value)
            {
                this.m_value = value;
                this.m_next = null;
            }
        }
    }
}

