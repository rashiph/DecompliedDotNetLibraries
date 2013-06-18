namespace System.IdentityModel
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MostlySingletonList<T> where T: class
    {
        private int count;
        private T singleton;
        private List<T> list;
        public T this[int index]
        {
            get
            {
                if (this.list == null)
                {
                    this.EnsureValidSingletonIndex(index);
                    return this.singleton;
                }
                return this.list[index];
            }
        }
        public int Count
        {
            get
            {
                return this.count;
            }
        }
        public void Add(T item)
        {
            if (this.list == null)
            {
                if (this.count == 0)
                {
                    this.singleton = item;
                    this.count = 1;
                    return;
                }
                this.list = new List<T>();
                this.list.Add(this.singleton);
                this.singleton = default(T);
            }
            this.list.Add(item);
            this.count++;
        }

        private static bool Compare(T x, T y)
        {
            if (x != null)
            {
                return x.Equals(y);
            }
            return (y == null);
        }

        public bool Contains(T item)
        {
            return (this.IndexOf(item) >= 0);
        }

        private void EnsureValidSingletonIndex(int index)
        {
            if (this.count != 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.IdentityModel.SR.GetString("ValueMustBeOne")));
            }
            if (index != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", System.IdentityModel.SR.GetString("ValueMustBeZero")));
            }
        }

        private bool MatchesSingleton(T item)
        {
            return ((this.count == 1) && MostlySingletonList<T>.Compare(this.singleton, item));
        }

        public int IndexOf(T item)
        {
            if (this.list != null)
            {
                return this.list.IndexOf(item);
            }
            if (!this.MatchesSingleton(item))
            {
                return -1;
            }
            return 0;
        }

        public bool Remove(T item)
        {
            if (this.list == null)
            {
                if (this.MatchesSingleton(item))
                {
                    this.singleton = default(T);
                    this.count = 0;
                    return true;
                }
                return false;
            }
            bool flag = this.list.Remove(item);
            if (flag)
            {
                this.count--;
            }
            return flag;
        }

        public void RemoveAt(int index)
        {
            if (this.list == null)
            {
                this.EnsureValidSingletonIndex(index);
                this.singleton = default(T);
                this.count = 0;
            }
            else
            {
                this.list.RemoveAt(index);
                this.count--;
            }
        }
    }
}

