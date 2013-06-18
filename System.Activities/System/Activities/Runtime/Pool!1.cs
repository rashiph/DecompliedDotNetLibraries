namespace System.Activities.Runtime
{
    using System;

    internal abstract class Pool<T>
    {
        private int count;
        private const int DefaultPoolSize = 10;
        private T[] items;
        private int poolSize;

        public Pool() : this(10)
        {
        }

        public Pool(int poolSize)
        {
            this.items = new T[poolSize];
            this.poolSize = poolSize;
        }

        public T Acquire()
        {
            if (this.count > 0)
            {
                this.count--;
                return this.items[this.count];
            }
            return this.CreateNew();
        }

        protected abstract T CreateNew();
        public void Release(T item)
        {
            if (this.count < this.poolSize)
            {
                this.items[this.count] = item;
                this.count++;
            }
        }
    }
}

