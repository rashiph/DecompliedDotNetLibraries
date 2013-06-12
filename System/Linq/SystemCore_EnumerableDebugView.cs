namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal sealed class SystemCore_EnumerableDebugView
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object[] cachedCollection;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int count;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IEnumerable enumerable;

        public SystemCore_EnumerableDebugView(IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }
            this.enumerable = enumerable;
            this.count = 0;
            this.cachedCollection = null;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items
        {
            get
            {
                List<object> list = new List<object>();
                IEnumerator enumerator = this.enumerable.GetEnumerator();
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
                this.cachedCollection = new object[this.count];
                list.CopyTo(this.cachedCollection, 0);
                return this.cachedCollection;
            }
        }
    }
}

