namespace System.IdentityModel
{
    using System;

    internal class Pool<T> where T: class
    {
        private int count;
        private T[] items;

        public Pool(int maxCount)
        {
            this.items = new T[maxCount];
        }

        public void Clear()
        {
            for (int i = 0; i < this.count; i++)
            {
                this.items[i] = default(T);
            }
            this.count = 0;
        }

        public bool Return(T item)
        {
            if (this.count < this.items.Length)
            {
                this.items[this.count++] = item;
                return true;
            }
            return false;
        }

        public T Take()
        {
            if (this.count > 0)
            {
                T local = this.items[--this.count];
                this.items[this.count] = default(T);
                return local;
            }
            return default(T);
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }
    }
}

