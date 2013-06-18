namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class Set<T> : IEnumerable<T>, IEnumerable where T: IComparable
    {
        private List<T> list;

        public Set()
        {
            this.list = new List<T>();
        }

        public Set(int capacity)
        {
            this.list = new List<T>(capacity);
        }

        public void Add(T item)
        {
            int insertPos = -1;
            if (this.Search(item, out insertPos))
            {
                throw new ArgumentException(ExecutionStringManager.ItemAlreadyExist);
            }
            this.list.Insert(insertPos, item);
        }

        public bool Contains(T item)
        {
            int insertPos = -1;
            return this.Search(item, out insertPos);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        private bool Search(T item, out int insertPos)
        {
            insertPos = -1;
            int num = 0;
            int count = this.list.Count;
            int num3 = -1;
            int num4 = 0;
            while ((count - num3) > 1)
            {
                num = (count + num3) / 2;
                num4 = this.list[num].CompareTo(item);
                if (num4 == 0)
                {
                    insertPos = num;
                    return true;
                }
                if (num4 > 0)
                {
                    count = num;
                }
                else
                {
                    num3 = num;
                }
            }
            if (num3 == -1)
            {
                insertPos = 0;
                return false;
            }
            if (num4 != 0)
            {
                insertPos = (num4 < 0) ? (num + 1) : num;
                return false;
            }
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public bool TryAdd(T item)
        {
            int insertPos = -1;
            if (!this.Search(item, out insertPos))
            {
                this.list.Insert(insertPos, item);
                return true;
            }
            return false;
        }

        public bool TryGetValue(T item, out T value)
        {
            int insertPos = -1;
            if (this.Search(item, out insertPos))
            {
                value = this.list[insertPos];
                return true;
            }
            value = default(T);
            return false;
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                return this.list[index];
            }
        }
    }
}

