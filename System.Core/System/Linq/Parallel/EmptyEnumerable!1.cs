namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class EmptyEnumerable<T> : ParallelQuery<T>
    {
        private static EmptyEnumerator<T> s_enumeratorInstance;
        private static System.Linq.Parallel.EmptyEnumerable<T> s_instance;

        private EmptyEnumerable() : base(QuerySettings.Empty)
        {
        }

        public override IEnumerator<T> GetEnumerator()
        {
            if (System.Linq.Parallel.EmptyEnumerable<T>.s_enumeratorInstance == null)
            {
                System.Linq.Parallel.EmptyEnumerable<T>.s_enumeratorInstance = new EmptyEnumerator<T>();
            }
            return System.Linq.Parallel.EmptyEnumerable<T>.s_enumeratorInstance;
        }

        internal static System.Linq.Parallel.EmptyEnumerable<T> Instance
        {
            get
            {
                if (System.Linq.Parallel.EmptyEnumerable<T>.s_instance == null)
                {
                    System.Linq.Parallel.EmptyEnumerable<T>.s_instance = new System.Linq.Parallel.EmptyEnumerable<T>();
                }
                return System.Linq.Parallel.EmptyEnumerable<T>.s_instance;
            }
        }
    }
}

