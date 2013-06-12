namespace System.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal sealed class SystemCore_EnumerableDebugView<T>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private T[] cachedCollection;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int count;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IEnumerable<T> enumerable;

        public SystemCore_EnumerableDebugView(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }
            this.enumerable = enumerable;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                List<T> list = new List<T>();
                IEnumerator<T> enumerator = this.enumerable.GetEnumerator();
                if (enumerator != null)
                {
                    this.count = 0;
                    while (enumerator.MoveNext())
                    {
                        list.Add(enumerator.Current);
                        this.count++;
                    }
                }
                if (this.count == 0)
                {
                    throw new SystemCore_EnumerableDebugViewEmptyException();
                }
                this.cachedCollection = new T[this.count];
                list.CopyTo(this.cachedCollection, 0);
                return this.cachedCollection;
            }
        }
    }
}

