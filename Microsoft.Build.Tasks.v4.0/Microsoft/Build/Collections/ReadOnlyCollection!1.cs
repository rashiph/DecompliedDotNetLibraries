namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class ReadOnlyCollection<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private IEnumerable<T> backing;

        internal ReadOnlyCollection(IEnumerable<T> backing)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(backing != null, "Need backing collection");
            this.backing = backing;
        }

        public void Add(T item)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public void Clear()
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public bool Contains(T item)
        {
            if (this.backing is ICollection<T>)
            {
                return this.BackingCollection.Contains(item);
            }
            return this.backing.Contains<T>(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(array, "array");
            ICollection<T> backing = this.backing as ICollection<T>;
            if (backing != null)
            {
                backing.CopyTo(array, arrayIndex);
            }
            else
            {
                int index = arrayIndex;
                foreach (T local in this.backing)
                {
                    array[index] = local;
                    index++;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.backing.GetEnumerator();
        }

        public bool Remove(T item)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            return false;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(array, "array");
            int num = index;
            foreach (T local in this.backing)
            {
                array.SetValue(local, num);
                num++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.backing.GetEnumerator();
        }

        private ICollection<T> BackingCollection
        {
            get
            {
                ICollection<T> backing = this.backing as ICollection<T>;
                if (backing == null)
                {
                    backing = new List<T>(this.backing);
                    this.backing = backing;
                }
                return backing;
            }
        }

        public int Count
        {
            get
            {
                return this.BackingCollection.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

