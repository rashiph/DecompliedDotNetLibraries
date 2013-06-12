namespace System.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public sealed class KeyContainerPermissionAccessEntryCollection : ICollection, IEnumerable
    {
        private KeyContainerPermissionFlags m_globalFlags;
        private ArrayList m_list;

        private KeyContainerPermissionAccessEntryCollection()
        {
        }

        internal KeyContainerPermissionAccessEntryCollection(KeyContainerPermissionFlags globalFlags)
        {
            this.m_list = new ArrayList();
            this.m_globalFlags = globalFlags;
        }

        public int Add(KeyContainerPermissionAccessEntry accessEntry)
        {
            if (accessEntry == null)
            {
                throw new ArgumentNullException("accessEntry");
            }
            int index = this.m_list.IndexOf(accessEntry);
            if (index == -1)
            {
                if (accessEntry.Flags != this.m_globalFlags)
                {
                    return this.m_list.Add(accessEntry);
                }
                return -1;
            }
            KeyContainerPermissionAccessEntry entry1 = (KeyContainerPermissionAccessEntry) this.m_list[index];
            entry1.Flags &= accessEntry.Flags;
            return index;
        }

        public void Clear()
        {
            this.m_list.Clear();
        }

        public void CopyTo(KeyContainerPermissionAccessEntry[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public KeyContainerPermissionAccessEntryEnumerator GetEnumerator()
        {
            return new KeyContainerPermissionAccessEntryEnumerator(this);
        }

        public int IndexOf(KeyContainerPermissionAccessEntry accessEntry)
        {
            return this.m_list.IndexOf(accessEntry);
        }

        public void Remove(KeyContainerPermissionAccessEntry accessEntry)
        {
            if (accessEntry == null)
            {
                throw new ArgumentNullException("accessEntry");
            }
            this.m_list.Remove(accessEntry);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
            }
            if ((index < 0) || (index >= array.Length))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((index + this.Count) > array.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], index);
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new KeyContainerPermissionAccessEntryEnumerator(this);
        }

        public int Count
        {
            get
            {
                return this.m_list.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public KeyContainerPermissionAccessEntry this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                }
                if (index >= this.Count)
                {
                    throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                }
                return (KeyContainerPermissionAccessEntry) this.m_list[index];
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

