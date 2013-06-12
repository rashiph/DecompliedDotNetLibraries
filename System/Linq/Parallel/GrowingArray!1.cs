namespace System.Linq.Parallel
{
    using System;

    internal class GrowingArray<T>
    {
        private const int DEFAULT_ARRAY_SIZE = 0x400;
        private T[] m_array;
        private int m_count;

        internal GrowingArray()
        {
            this.m_array = new T[0x400];
            this.m_count = 0;
        }

        internal void Add(T element)
        {
            if (this.m_count >= this.m_array.Length)
            {
                this.GrowArray(2 * this.m_array.Length);
            }
            this.m_array[this.m_count++] = element;
        }

        internal void CopyFrom(T[] otherArray, int otherCount)
        {
            if ((this.m_count + otherCount) > this.m_array.Length)
            {
                this.GrowArray(this.m_count + otherCount);
            }
            Array.Copy(otherArray, 0, this.m_array, this.m_count, otherCount);
            this.m_count += otherCount;
        }

        private void GrowArray(int newSize)
        {
            T[] array = new T[newSize];
            this.m_array.CopyTo(array, 0);
            this.m_array = array;
        }

        internal int Count
        {
            get
            {
                return this.m_count;
            }
        }

        internal T[] InternalArray
        {
            get
            {
                return this.m_array;
            }
        }
    }
}

