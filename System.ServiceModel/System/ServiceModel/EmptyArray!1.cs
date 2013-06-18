namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;

    internal class EmptyArray<T>
    {
        private static T[] instance;

        private EmptyArray()
        {
        }

        internal static T[] Allocate(int n)
        {
            if (n == 0)
            {
                return EmptyArray<T>.Instance;
            }
            return new T[n];
        }

        internal static T[] ToArray(IList<T> collection)
        {
            if (collection.Count == 0)
            {
                return EmptyArray<T>.Instance;
            }
            T[] array = new T[collection.Count];
            collection.CopyTo(array, 0);
            return array;
        }

        internal static T[] ToArray(SynchronizedCollection<T> collection)
        {
            lock (collection.SyncRoot)
            {
                return EmptyArray<T>.ToArray((IList<T>) collection);
            }
        }

        internal static T[] Instance
        {
            get
            {
                if (EmptyArray<T>.instance == null)
                {
                    EmptyArray<T>.instance = new T[0];
                }
                return EmptyArray<T>.instance;
            }
        }
    }
}

