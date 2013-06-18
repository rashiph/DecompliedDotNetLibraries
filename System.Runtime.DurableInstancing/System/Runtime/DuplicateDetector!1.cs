namespace System.Runtime
{
    using System;
    using System.Collections.Generic;

    internal class DuplicateDetector<T> where T: class
    {
        private int capacity;
        private LinkedList<T> fifoList;
        private Dictionary<T, LinkedListNode<T>> items;
        private object thisLock;

        public DuplicateDetector(int capacity)
        {
            this.capacity = capacity;
            this.items = new Dictionary<T, LinkedListNode<T>>();
            this.fifoList = new LinkedList<T>();
            this.thisLock = new object();
        }

        private void Add(T value)
        {
            if (this.items.Count == this.capacity)
            {
                LinkedListNode<T> last = this.fifoList.Last;
                this.items.Remove(last.Value);
                this.fifoList.Remove(last);
            }
            this.items.Add(value, this.fifoList.AddFirst(value));
        }

        public bool AddIfNotDuplicate(T value)
        {
            bool flag = false;
            lock (this.thisLock)
            {
                if (!this.items.ContainsKey(value))
                {
                    this.Add(value);
                    flag = true;
                }
            }
            return flag;
        }

        public void Clear()
        {
            lock (this.thisLock)
            {
                this.fifoList.Clear();
                this.items.Clear();
            }
        }

        public bool Remove(T value)
        {
            bool flag = false;
            lock (this.thisLock)
            {
                LinkedListNode<T> node;
                if (this.items.TryGetValue(value, out node))
                {
                    this.items.Remove(value);
                    this.fifoList.Remove(node);
                    flag = true;
                }
            }
            return flag;
        }
    }
}

