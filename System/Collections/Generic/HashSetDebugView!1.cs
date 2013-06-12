namespace System.Collections.Generic
{
    using System;
    using System.Diagnostics;

    internal class HashSetDebugView<T>
    {
        private HashSet<T> set;

        public HashSetDebugView(HashSet<T> set)
        {
            if (set == null)
            {
                throw new ArgumentNullException("set");
            }
            this.set = set;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                return this.set.ToArray();
            }
        }
    }
}

