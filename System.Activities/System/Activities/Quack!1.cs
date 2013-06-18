namespace System.Activities
{
    using System;
    using System.Reflection;

    internal class Quack<T>
    {
        private int count;
        private int head;
        private T[] items;
        private int tail;

        public Quack()
        {
            this.items = new T[4];
        }

        public Quack(T[] items)
        {
            this.items = items;
            this.count = this.items.Length;
        }

        public T Dequeue()
        {
            T local = this.items[this.head];
            this.items[this.head] = default(T);
            if (++this.head == this.items.Length)
            {
                this.head = 0;
            }
            this.count--;
            return local;
        }

        private void Enlarge()
        {
            int capacity = this.items.Length * 2;
            this.SetCapacity(capacity);
        }

        public void Enqueue(T item)
        {
            if (this.count == this.items.Length)
            {
                this.Enlarge();
            }
            this.items[this.tail] = item;
            if (++this.tail == this.items.Length)
            {
                this.tail = 0;
            }
            this.count++;
        }

        public void PushFront(T item)
        {
            if (this.count == this.items.Length)
            {
                this.Enlarge();
            }
            if (--this.head == -1)
            {
                this.head = this.items.Length - 1;
            }
            this.items[this.head] = item;
            this.count++;
        }

        public bool Remove(T item)
        {
            int index = -1;
            for (int i = 0; i < this.count; i++)
            {
                int num3 = (this.head + i) % this.items.Length;
                if (object.Equals(this.items[num3], item))
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                return false;
            }
            this.Remove(index);
            return true;
        }

        public void Remove(int index)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                int num2 = (this.head + i) % this.items.Length;
                int num3 = num2 + 1;
                if (num3 == this.items.Length)
                {
                    num3 = 0;
                }
                this.items[num3] = this.items[num2];
            }
            this.count--;
            this.head++;
            if (this.head == this.items.Length)
            {
                this.head = 0;
            }
        }

        private void SetCapacity(int capacity)
        {
            T[] destinationArray = new T[capacity];
            if (this.count > 0)
            {
                if (this.head < this.tail)
                {
                    Array.Copy(this.items, this.head, destinationArray, 0, this.count);
                }
                else
                {
                    Array.Copy(this.items, this.head, destinationArray, 0, this.items.Length - this.head);
                    Array.Copy(this.items, 0, destinationArray, this.items.Length - this.head, this.tail);
                }
            }
            this.items = destinationArray;
            this.head = 0;
            this.tail = (this.count == capacity) ? 0 : this.count;
        }

        public T[] ToArray()
        {
            T[] localArray = new T[this.count];
            for (int i = 0; i < this.count; i++)
            {
                localArray[i] = this.items[(this.head + i) % this.items.Length];
            }
            return localArray;
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public T this[int index]
        {
            get
            {
                int num = (this.head + index) % this.items.Length;
                return this.items[num];
            }
        }
    }
}

