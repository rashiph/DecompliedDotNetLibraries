namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;

    internal class SparselyPopulatedArray<T> where T: class
    {
        private readonly SparselyPopulatedArrayFragment<T> m_head;
        private volatile SparselyPopulatedArrayFragment<T> m_tail;

        internal SparselyPopulatedArray(int initialSize)
        {
            this.m_head = this.m_tail = new SparselyPopulatedArrayFragment<T>(initialSize);
        }

        internal SparselyPopulatedArrayAddInfo<T> Add(T element)
        {
            while (true)
            {
                SparselyPopulatedArrayFragment<T> tail = this.m_tail;
                while (tail.m_next != null)
                {
                    this.m_tail = tail = tail.m_next;
                }
                for (SparselyPopulatedArrayFragment<T> fragment2 = tail; fragment2 != null; fragment2 = fragment2.m_prev)
                {
                    if (fragment2.m_freeCount < 1)
                    {
                        fragment2.m_freeCount--;
                    }
                    if ((fragment2.m_freeCount > 0) || (fragment2.m_freeCount < -10))
                    {
                        int length = fragment2.Length;
                        int num2 = (length - fragment2.m_freeCount) % length;
                        if (num2 < 0)
                        {
                            num2 = 0;
                            fragment2.m_freeCount--;
                        }
                        for (int i = 0; i < length; i++)
                        {
                            int index = (num2 + i) % length;
                            if (fragment2.m_elements[index] == null)
                            {
                                T comparand = default(T);
                                if (Interlocked.CompareExchange<T>(ref fragment2.m_elements[index], element, comparand) == null)
                                {
                                    int num5 = fragment2.m_freeCount - 1;
                                    fragment2.m_freeCount = (num5 > 0) ? num5 : 0;
                                    return new SparselyPopulatedArrayAddInfo<T>(fragment2, index);
                                }
                            }
                        }
                    }
                }
                SparselyPopulatedArrayFragment<T> fragment3 = new SparselyPopulatedArrayFragment<T>((tail.m_elements.Length == 0x1000) ? 0x1000 : (tail.m_elements.Length * 2), tail);
                if (Interlocked.CompareExchange<SparselyPopulatedArrayFragment<T>>(ref tail.m_next, fragment3, null) == null)
                {
                    this.m_tail = fragment3;
                }
            }
        }

        internal SparselyPopulatedArrayFragment<T> Head
        {
            get
            {
                return this.m_head;
            }
        }

        internal SparselyPopulatedArrayFragment<T> Tail
        {
            get
            {
                return this.m_tail;
            }
        }
    }
}

