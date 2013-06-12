namespace System.Collections.Generic
{
    using System;
    using System.Diagnostics;

    internal sealed class System_QueueDebugView<T>
    {
        private Queue<T> queue;

        public System_QueueDebugView(Queue<T> queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("queue");
            }
            this.queue = queue;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                return this.queue.ToArray();
            }
        }
    }
}

