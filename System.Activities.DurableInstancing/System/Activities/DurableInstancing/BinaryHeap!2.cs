namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;

    internal sealed class BinaryHeap<TKey, TValue> where TKey: IComparable<TKey>
    {
        private const int defaultCapacity = 0x80;
        private readonly KeyValuePair<TKey, TValue> EmptyItem;
        private int itemCount;
        private KeyValuePair<TKey, TValue>[] items;

        public BinaryHeap() : this(0x80)
        {
        }

        public BinaryHeap(int capacity)
        {
            this.EmptyItem = new KeyValuePair<TKey, TValue>();
            this.items = new KeyValuePair<TKey, TValue>[capacity];
        }

        private void BubbleDown(int startIndex)
        {
            int num3;
            int index = startIndex;
            int num2 = startIndex;
        Label_0004:
            num3 = (index << 1) + 1;
            int num4 = num3 + 1;
            if (num3 < this.itemCount)
            {
                if (this.items[index].Key.CompareTo(this.items[num3].Key) > 0)
                {
                    num2 = num3;
                }
                if ((num4 < this.itemCount) && (this.items[num2].Key.CompareTo(this.items[num4].Key) > 0))
                {
                    num2 = num4;
                }
                if (index != num2)
                {
                    KeyValuePair<TKey, TValue> pair = this.items[index];
                    this.items[index] = this.items[num2];
                    this.items[num2] = pair;
                    index = num2;
                    goto Label_0004;
                }
            }
        }

        private int BubbleUp(int startIndex)
        {
            while (startIndex > 0)
            {
                int index = (startIndex - 1) >> 1;
                if (this.items[index].Key.CompareTo(this.items[startIndex].Key) <= 0)
                {
                    return startIndex;
                }
                KeyValuePair<TKey, TValue> pair = this.items[startIndex];
                this.items[startIndex] = this.items[index];
                this.items[index] = pair;
                startIndex = index;
            }
            return startIndex;
        }

        public void Clear()
        {
            this.itemCount = 0;
            this.items = new KeyValuePair<TKey, TValue>[0x80];
        }

        public KeyValuePair<TKey, TValue> Dequeue()
        {
            return this.Dequeue(true);
        }

        private KeyValuePair<TKey, TValue> Dequeue(bool shrink)
        {
            KeyValuePair<TKey, TValue> pair = this.items[0];
            if (this.itemCount == 1)
            {
                this.itemCount = 0;
                this.items[0] = this.EmptyItem;
            }
            else
            {
                this.itemCount--;
                this.items[0] = this.items[this.itemCount];
                this.items[this.itemCount] = this.EmptyItem;
                this.BubbleDown(0);
            }
            if (shrink)
            {
                this.ShrinkStore();
            }
            return pair;
        }

        public bool Enqueue(TKey key, TValue item)
        {
            if (this.itemCount == this.items.Length)
            {
                this.ResizeItemStore(this.items.Length * 2);
            }
            this.items[this.itemCount++] = new KeyValuePair<TKey, TValue>(key, item);
            return (this.BubbleUp(this.itemCount - 1) == 0);
        }

        public KeyValuePair<TKey, TValue> Peek()
        {
            return this.items[0];
        }

        public ICollection<KeyValuePair<TKey, TValue>> RemoveAll(Predicate<KeyValuePair<TKey, TValue>> func)
        {
            ICollection<KeyValuePair<TKey, TValue>> is2 = new List<KeyValuePair<TKey, TValue>>();
            for (int i = 0; i < this.itemCount; i++)
            {
                while (func(this.items[i]) && (i < this.itemCount))
                {
                    is2.Add(this.items[i]);
                    int index = this.itemCount - 1;
                    while (func(this.items[index]) && (i < index))
                    {
                        is2.Add(this.items[index]);
                        this.items[index] = this.EmptyItem;
                        index--;
                    }
                    this.items[i] = this.items[index];
                    this.items[index] = this.EmptyItem;
                    this.itemCount = index;
                    if (i < index)
                    {
                        this.BubbleDown(this.BubbleUp(i));
                    }
                }
            }
            this.ShrinkStore();
            return is2;
        }

        private void ResizeItemStore(int newSize)
        {
            KeyValuePair<TKey, TValue>[] destinationArray = new KeyValuePair<TKey, TValue>[newSize];
            Array.Copy(this.items, 0, destinationArray, 0, this.itemCount);
            this.items = destinationArray;
        }

        private void ShrinkStore()
        {
            if ((this.items.Length > 0x80) && (this.itemCount < (this.items.Length >> 1)))
            {
                int newSize = Math.Max(0x80, ((this.itemCount / 0x80) + 1) * 0x80);
                this.ResizeItemStore(newSize);
            }
        }

        public ICollection<KeyValuePair<TKey, TValue>> TakeWhile(Predicate<TKey> func)
        {
            ICollection<KeyValuePair<TKey, TValue>> is2 = new List<KeyValuePair<TKey, TValue>>();
            while (!this.IsEmpty && func(this.Peek().Key))
            {
                is2.Add(this.Dequeue(false));
            }
            this.ShrinkStore();
            return is2;
        }

        public int Count
        {
            get
            {
                return this.itemCount;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.itemCount == 0);
            }
        }
    }
}

