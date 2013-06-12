namespace System.Security.AccessControl
{
    using System;
    using System.Collections;
    using System.Reflection;

    public abstract class GenericAcl : ICollection, IEnumerable
    {
        public static readonly byte AclRevision = 2;
        public static readonly byte AclRevisionDS = 4;
        internal const int HeaderLength = 8;
        public static readonly int MaxBinaryLength = 0xffff;

        protected GenericAcl()
        {
        }

        public void CopyTo(GenericAce[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public abstract void GetBinaryForm(byte[] binaryForm, int offset);
        public AceEnumerator GetEnumerator()
        {
            return (((IEnumerable) this).GetEnumerator() as AceEnumerator);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - index) < this.Count)
            {
                throw new ArgumentOutOfRangeException("array", Environment.GetResourceString("ArgumentOutOfRange_ArrayTooSmall"));
            }
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], (int) (index + i));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AceEnumerator(this);
        }

        public abstract int BinaryLength { get; }

        public abstract int Count { get; }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public abstract GenericAce this[int index] { get; set; }

        public abstract byte Revision { get; }

        public virtual object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

