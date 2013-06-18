namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;

    internal class SynchronizedDisposablePool<T> where T: class, IDisposable
    {
        private bool disposed;
        private List<T> items;
        private int maxCount;

        public SynchronizedDisposablePool(int maxCount)
        {
            this.items = new List<T>();
            this.maxCount = maxCount;
        }

        public void Dispose()
        {
            T[] localArray;
            lock (this.ThisLock)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    if (this.items.Count > 0)
                    {
                        localArray = new T[this.items.Count];
                        this.items.CopyTo(localArray, 0);
                        this.items.Clear();
                    }
                    else
                    {
                        localArray = null;
                    }
                }
                else
                {
                    localArray = null;
                }
            }
            if (localArray != null)
            {
                for (int i = 0; i < localArray.Length; i++)
                {
                    localArray[i].Dispose();
                }
            }
        }

        public bool Return(T value)
        {
            if (!this.disposed && (this.items.Count < this.maxCount))
            {
                lock (this.ThisLock)
                {
                    if (!this.disposed && (this.items.Count < this.maxCount))
                    {
                        this.items.Add(value);
                        return true;
                    }
                }
            }
            return false;
        }

        public T Take()
        {
            if (!this.disposed && (this.items.Count > 0))
            {
                lock (this.ThisLock)
                {
                    if (!this.disposed && (this.items.Count > 0))
                    {
                        int index = this.items.Count - 1;
                        T local = this.items[index];
                        this.items.RemoveAt(index);
                        return local;
                    }
                }
            }
            return default(T);
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }
    }
}

