namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class Set<TElement>
    {
        private int[] buckets;
        private IEqualityComparer<TElement> comparer;
        private int count;
        private int freeList;
        private Slot<TElement>[] slots;

        internal Set() : this(null)
        {
        }

        internal Set(IEqualityComparer<TElement> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TElement>.Default;
            }
            this.comparer = comparer;
            this.buckets = new int[7];
            this.slots = new Slot<TElement>[7];
            this.freeList = -1;
        }

        internal bool Add(TElement value)
        {
            return !this.Find(value, true);
        }

        internal bool Contains(TElement value)
        {
            return this.Find(value, false);
        }

        internal bool Find(TElement value, bool add)
        {
            int num = this.comparer.GetHashCode(value) & 0x7fffffff;
            for (int i = this.buckets[num % this.buckets.Length] - 1; i >= 0; i = this.slots[i].next)
            {
                if ((this.slots[i].hashCode == num) && this.comparer.Equals(this.slots[i].value, value))
                {
                    return true;
                }
            }
            if (add)
            {
                int freeList;
                if (this.freeList >= 0)
                {
                    freeList = this.freeList;
                    this.freeList = this.slots[freeList].next;
                }
                else
                {
                    if (this.count == this.slots.Length)
                    {
                        this.Resize();
                    }
                    freeList = this.count;
                    this.count++;
                }
                int index = num % this.buckets.Length;
                this.slots[freeList].hashCode = num;
                this.slots[freeList].value = value;
                this.slots[freeList].next = this.buckets[index] - 1;
                this.buckets[index] = freeList + 1;
            }
            return false;
        }

        internal bool Remove(TElement value)
        {
            int num = this.comparer.GetHashCode(value) & 0x7fffffff;
            int index = num % this.buckets.Length;
            int num3 = -1;
            for (int i = this.buckets[index] - 1; i >= 0; i = this.slots[i].next)
            {
                if ((this.slots[i].hashCode == num) && this.comparer.Equals(this.slots[i].value, value))
                {
                    if (num3 < 0)
                    {
                        this.buckets[index] = this.slots[i].next + 1;
                    }
                    else
                    {
                        this.slots[num3].next = this.slots[i].next;
                    }
                    this.slots[i].hashCode = -1;
                    this.slots[i].value = default(TElement);
                    this.slots[i].next = this.freeList;
                    this.freeList = i;
                    return true;
                }
                num3 = i;
            }
            return false;
        }

        private void Resize()
        {
            int num = (this.count * 2) + 1;
            int[] numArray = new int[num];
            Slot<TElement>[] destinationArray = new Slot<TElement>[num];
            Array.Copy(this.slots, 0, destinationArray, 0, this.count);
            for (int i = 0; i < this.count; i++)
            {
                int index = destinationArray[i].hashCode % num;
                destinationArray[i].next = numArray[index] - 1;
                numArray[index] = i + 1;
            }
            this.buckets = numArray;
            this.slots = destinationArray;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Slot
        {
            internal int hashCode;
            internal TElement value;
            internal int next;
        }
    }
}

