namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct QueryBuffer<T>
    {
        internal T[] buffer;
        internal int count;
        internal static T[] EmptyBuffer;
        internal QueryBuffer(int capacity)
        {
            if (capacity == 0)
            {
                this.buffer = QueryBuffer<T>.EmptyBuffer;
            }
            else
            {
                this.buffer = new T[capacity];
            }
            this.count = 0;
        }

        internal int Count
        {
            get
            {
                return this.count;
            }
        }
        internal T this[int index]
        {
            get
            {
                return this.buffer[index];
            }
            set
            {
                this.buffer[index] = value;
            }
        }
        internal void Add(T t)
        {
            if (this.count == this.buffer.Length)
            {
                Array.Resize<T>(ref this.buffer, (this.count > 0) ? (this.count * 2) : 0x10);
            }
            this.buffer[this.count++] = t;
        }

        internal void Add(ref QueryBuffer<T> addBuffer)
        {
            if (1 == addBuffer.count)
            {
                this.Add(addBuffer.buffer[0]);
            }
            else
            {
                int capacity = this.count + addBuffer.count;
                if (capacity >= this.buffer.Length)
                {
                    this.Grow(capacity);
                }
                Array.Copy(addBuffer.buffer, 0, this.buffer, this.count, addBuffer.count);
                this.count = capacity;
            }
        }

        internal void Clear()
        {
            this.count = 0;
        }

        internal void CopyFrom(ref QueryBuffer<T> addBuffer)
        {
            int count = addBuffer.count;
            switch (count)
            {
                case 0:
                    this.count = 0;
                    return;

                case 1:
                    if (this.buffer.Length == 0)
                    {
                        this.buffer = new T[1];
                    }
                    this.buffer[0] = addBuffer.buffer[0];
                    this.count = 1;
                    return;
            }
            if (count > this.buffer.Length)
            {
                this.buffer = new T[count];
            }
            Array.Copy(addBuffer.buffer, 0, this.buffer, 0, count);
            this.count = count;
        }

        internal void CopyTo(T[] dest)
        {
            Array.Copy(this.buffer, dest, this.count);
        }

        private void Grow(int capacity)
        {
            int num = this.buffer.Length * 2;
            Array.Resize<T>(ref this.buffer, (capacity > num) ? capacity : num);
        }

        internal int IndexOf(T t)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (t.Equals(this.buffer[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        internal int IndexOf(T t, int startAt)
        {
            for (int i = startAt; i < this.count; i++)
            {
                if (t.Equals(this.buffer[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        internal bool IsValidIndex(int index)
        {
            return ((index >= 0) && (index < this.count));
        }

        internal void Reserve(int reserveCount)
        {
            int capacity = this.count + reserveCount;
            if (capacity >= this.buffer.Length)
            {
                this.Grow(capacity);
            }
            this.count = capacity;
        }

        internal void ReserveAt(int index, int reserveCount)
        {
            if (index == this.count)
            {
                this.Reserve(reserveCount);
            }
            else
            {
                int num;
                if (index > this.count)
                {
                    num = (index + reserveCount) + 1;
                    if (num >= this.buffer.Length)
                    {
                        this.Grow(num);
                    }
                }
                else
                {
                    num = this.count + reserveCount;
                    if (num >= this.buffer.Length)
                    {
                        this.Grow(num);
                    }
                    Array.Copy(this.buffer, index, this.buffer, index + reserveCount, this.count - index);
                }
                this.count = num;
            }
        }

        internal void Remove(T t)
        {
            int index = this.IndexOf(t);
            if (index >= 0)
            {
                this.RemoveAt(index);
            }
        }

        internal void RemoveAt(int index)
        {
            if (index < (this.count - 1))
            {
                Array.Copy(this.buffer, index + 1, this.buffer, index, (this.count - index) - 1);
            }
            this.count--;
        }

        internal void Sort(IComparer<T> comparer)
        {
            Array.Sort<T>(this.buffer, 0, this.count, comparer);
        }

        internal void TrimToCount()
        {
            if (this.count < this.buffer.Length)
            {
                if (this.count == 0)
                {
                    this.buffer = QueryBuffer<T>.EmptyBuffer;
                }
                else
                {
                    T[] destinationArray = new T[this.count];
                    Array.Copy(this.buffer, destinationArray, this.count);
                }
            }
        }

        static QueryBuffer()
        {
            QueryBuffer<T>.EmptyBuffer = new T[0];
        }
    }
}

