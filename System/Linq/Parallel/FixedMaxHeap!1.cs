namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class FixedMaxHeap<TElement>
    {
        private IComparer<TElement> m_comparer;
        private int m_count;
        private TElement[] m_elements;

        internal FixedMaxHeap(int maximumSize) : this(maximumSize, Util.GetDefaultComparer<TElement>())
        {
        }

        internal FixedMaxHeap(int maximumSize, IComparer<TElement> comparer)
        {
            this.m_elements = new TElement[maximumSize];
            this.m_comparer = comparer;
        }

        internal void Clear()
        {
            this.m_count = 0;
        }

        private void HeapifyLastLeaf()
        {
            int num2;
            for (int i = this.m_count - 1; i > 0; i = num2)
            {
                num2 = ((i + 1) / 2) - 1;
                if (this.m_comparer.Compare(this.m_elements[i], this.m_elements[num2]) <= 0)
                {
                    break;
                }
                this.Swap(i, num2);
            }
        }

        private void HeapifyRoot()
        {
            int index = 0;
            int count = this.m_count;
            while (index < count)
            {
                int num3 = ((index + 1) * 2) - 1;
                int num4 = num3 + 1;
                if ((num3 < count) && (this.m_comparer.Compare(this.m_elements[index], this.m_elements[num3]) < 0))
                {
                    if ((num4 < count) && (this.m_comparer.Compare(this.m_elements[num3], this.m_elements[num4]) < 0))
                    {
                        this.Swap(index, num4);
                        index = num4;
                    }
                    else
                    {
                        this.Swap(index, num3);
                        index = num3;
                    }
                }
                else
                {
                    if ((num4 >= count) || (this.m_comparer.Compare(this.m_elements[index], this.m_elements[num4]) >= 0))
                    {
                        break;
                    }
                    this.Swap(index, num4);
                    index = num4;
                }
            }
        }

        internal bool Insert(TElement e)
        {
            if (this.m_count < this.m_elements.Length)
            {
                this.m_elements[this.m_count] = e;
                this.m_count++;
                this.HeapifyLastLeaf();
                return true;
            }
            if (this.m_comparer.Compare(e, this.m_elements[0]) < 0)
            {
                this.m_elements[0] = e;
                this.HeapifyRoot();
                return true;
            }
            return false;
        }

        internal void RemoveMax()
        {
            this.m_count--;
            if (this.m_count > 0)
            {
                this.m_elements[0] = this.m_elements[this.m_count];
                this.HeapifyRoot();
            }
        }

        internal void ReplaceMax(TElement newValue)
        {
            this.m_elements[0] = newValue;
            this.HeapifyRoot();
        }

        private void Swap(int i, int j)
        {
            TElement local = this.m_elements[i];
            this.m_elements[i] = this.m_elements[j];
            this.m_elements[j] = local;
        }

        internal int Count
        {
            get
            {
                return this.m_count;
            }
        }

        internal TElement MaxValue
        {
            get
            {
                if (this.m_count == 0)
                {
                    throw new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
                }
                return this.m_elements[0];
            }
        }

        internal int Size
        {
            get
            {
                return this.m_elements.Length;
            }
        }
    }
}

