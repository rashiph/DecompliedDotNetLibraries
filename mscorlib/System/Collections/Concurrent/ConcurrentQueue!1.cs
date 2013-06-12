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

    [Serializable, ComVisible(false), DebuggerTypeProxy(typeof(SystemCollectionsConcurrent_ProducerConsumerCollectionDebugView<>)), DebuggerDisplay("Count = {Count}"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class ConcurrentQueue<T> : IProducerConsumerCollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        [NonSerialized]
        private volatile Segment<T> m_head;
        private T[] m_serializationArray;
        [NonSerialized]
        private volatile Segment<T> m_tail;
        private const int SEGMENT_SIZE = 0x20;

        public ConcurrentQueue()
        {
            this.m_head = this.m_tail = new Segment<T>(0L);
        }

        public ConcurrentQueue(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.InitializeFromCollection(collection);
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            this.ToList().CopyTo(array, index);
        }

        public void Enqueue(T item)
        {
            SpinWait wait = new SpinWait();
            while (!this.m_tail.TryAppend(item, ref this.m_tail))
            {
                wait.SpinOnce();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.ToList().GetEnumerator();
        }

        private void GetHeadTailPositions(out Segment<T> head, out Segment<T> tail, out int headLow, out int tailHigh)
        {
            head = this.m_head;
            tail = this.m_tail;
            headLow = head.Low;
            tailHigh = tail.High;
            SpinWait wait = new SpinWait();
            while ((((head != this.m_head) || (tail != this.m_tail)) || ((headLow != head.Low) || (tailHigh != tail.High))) || (head.m_index > tail.m_index))
            {
                wait.SpinOnce();
                head = this.m_head;
                tail = this.m_tail;
                headLow = head.Low;
                tailHigh = tail.High;
            }
        }

        private void InitializeFromCollection(IEnumerable<T> collection)
        {
            this.m_head = this.m_tail = new Segment<T>(0L);
            int num = 0;
            foreach (T local in collection)
            {
                this.m_tail.UnsafeAdd(local);
                num++;
                if (num >= 0x20)
                {
                    this.m_tail = this.m_tail.UnsafeGrow();
                    num = 0;
                }
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.InitializeFromCollection(this.m_serializationArray);
            this.m_serializationArray = null;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            this.m_serializationArray = this.ToArray();
        }

        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            this.Enqueue(item);
            return true;
        }

        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            return this.TryDequeue(out item);
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
            Segment<T> segment;
            Segment<T> segment2;
            int num;
            int num2;
            this.GetHeadTailPositions(out segment, out segment2, out num, out num2);
            if (segment == segment2)
            {
                return segment.ToList(num, num2);
            }
            List<T> list = new List<T>(segment.ToList(num, 0x1f));
            for (Segment<T> segment3 = segment.Next; segment3 != segment2; segment3 = segment3.Next)
            {
                list.AddRange(segment3.ToList(0, 0x1f));
            }
            list.AddRange(segment2.ToList(0, num2));
            return list;
        }

        public bool TryDequeue(out T result)
        {
            while (!this.IsEmpty)
            {
                if (this.m_head.TryRemove(out result, ref this.m_head))
                {
                    return true;
                }
            }
            result = default(T);
            return false;
        }

        public bool TryPeek(out T result)
        {
            while (!this.IsEmpty)
            {
                if (this.m_head.TryPeek(out result))
                {
                    return true;
                }
            }
            result = default(T);
            return false;
        }

        public int Count
        {
            get
            {
                Segment<T> segment;
                Segment<T> segment2;
                int num;
                int num2;
                this.GetHeadTailPositions(out segment, out segment2, out num, out num2);
                if (segment == segment2)
                {
                    return ((num2 - num) + 1);
                }
                int num3 = 0x20 - num;
                num3 += 0x20 * ((int) ((segment2.m_index - segment.m_index) - 1L));
                return (num3 + (num2 + 1));
            }
        }

        public bool IsEmpty
        {
            get
            {
                Segment<T> head = this.m_head;
                if (head.IsEmpty)
                {
                    if (head.Next == null)
                    {
                        return true;
                    }
                    SpinWait wait = new SpinWait();
                    while (head.IsEmpty)
                    {
                        if (head.Next == null)
                        {
                            return true;
                        }
                        wait.SpinOnce();
                        head = this.m_head;
                    }
                }
                return false;
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

        private class Segment
        {
            internal volatile T[] m_array;
            private volatile int m_high;
            internal readonly long m_index;
            private volatile int m_low;
            private volatile ConcurrentQueue<T>.Segment m_next;
            private volatile int[] m_state;

            internal Segment(long index)
            {
                this.m_array = new T[0x20];
                this.m_state = new int[0x20];
                this.m_high = -1;
                this.m_index = index;
            }

            internal void Grow(ref ConcurrentQueue<T>.Segment tail)
            {
                ConcurrentQueue<T>.Segment segment = new ConcurrentQueue<T>.Segment(this.m_index + 1L);
                this.m_next = segment;
                tail = this.m_next;
            }

            internal List<T> ToList(int start, int end)
            {
                List<T> list = new List<T>();
                for (int i = start; i <= end; i++)
                {
                    SpinWait wait = new SpinWait();
                    while (this.m_state[i] == null)
                    {
                        wait.SpinOnce();
                    }
                    list.Add(this.m_array[i]);
                }
                return list;
            }

            internal bool TryAppend(T value, ref ConcurrentQueue<T>.Segment tail)
            {
                if (this.m_high >= 0x1f)
                {
                    return false;
                }
                int index = 0x20;
                try
                {
                }
                finally
                {
                    index = Interlocked.Increment(ref this.m_high);
                    if (index <= 0x1f)
                    {
                        this.m_array[index] = value;
                        this.m_state[index] = 1;
                    }
                    if (index == 0x1f)
                    {
                        this.Grow(ref tail);
                    }
                }
                return (index <= 0x1f);
            }

            internal bool TryPeek(out T result)
            {
                result = default(T);
                int low = this.Low;
                if (low > this.High)
                {
                    return false;
                }
                SpinWait wait = new SpinWait();
                while (this.m_state[low] == null)
                {
                    wait.SpinOnce();
                }
                result = this.m_array[low];
                return true;
            }

            internal bool TryRemove(out T result, ref ConcurrentQueue<T>.Segment head)
            {
                SpinWait wait = new SpinWait();
                int low = this.Low;
                for (int i = this.High; low <= i; i = this.High)
                {
                    if (Interlocked.CompareExchange(ref this.m_low, low + 1, low) == low)
                    {
                        SpinWait wait2 = new SpinWait();
                        while (this.m_state[low] == null)
                        {
                            wait2.SpinOnce();
                        }
                        result = this.m_array[low];
                        if ((low + 1) >= 0x20)
                        {
                            wait2 = new SpinWait();
                            while (this.m_next == null)
                            {
                                wait2.SpinOnce();
                            }
                            head = this.m_next;
                        }
                        return true;
                    }
                    wait.SpinOnce();
                    low = this.Low;
                }
                result = default(T);
                return false;
            }

            internal void UnsafeAdd(T value)
            {
                this.m_high++;
                this.m_array[this.m_high] = value;
                this.m_state[this.m_high] = 1;
            }

            internal ConcurrentQueue<T>.Segment UnsafeGrow()
            {
                ConcurrentQueue<T>.Segment segment = new ConcurrentQueue<T>.Segment(this.m_index + 1L);
                this.m_next = segment;
                return segment;
            }

            internal int High
            {
                get
                {
                    return Math.Min(this.m_high, 0x1f);
                }
            }

            internal bool IsEmpty
            {
                get
                {
                    return (this.Low > this.High);
                }
            }

            internal int Low
            {
                get
                {
                    return Math.Min(this.m_low, 0x20);
                }
            }

            internal ConcurrentQueue<T>.Segment Next
            {
                get
                {
                    return this.m_next;
                }
            }
        }
    }
}

