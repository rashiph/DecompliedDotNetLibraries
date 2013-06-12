namespace System.Collections.Generic
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class TreeSet<T> : SortedSet<T>
    {
        public TreeSet()
        {
        }

        public TreeSet(ICollection<T> collection) : base(collection)
        {
        }

        public TreeSet(IComparer<T> comparer) : base(comparer)
        {
        }

        public TreeSet(SerializationInfo siInfo, StreamingContext context) : base(siInfo, context)
        {
        }

        public TreeSet(ICollection<T> collection, IComparer<T> comparer) : base(collection, comparer)
        {
        }

        internal override bool AddIfNotPresent(T item)
        {
            bool flag = base.AddIfNotPresent(item);
            if (!flag)
            {
                System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Argument_AddingDuplicate);
            }
            return flag;
        }
    }
}

