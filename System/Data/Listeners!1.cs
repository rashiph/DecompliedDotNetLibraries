namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class Listeners<TElem> where TElem: class
    {
        private int _listenerReaderCount;
        private readonly Func<TElem, TElem, bool> filter;
        private readonly List<TElem> listeners;
        private readonly int ObjectID;

        internal Listeners(int ObjectID, Func<TElem, TElem, bool> notifyFilter)
        {
            this.listeners = new List<TElem>();
            this.filter = notifyFilter;
            this.ObjectID = ObjectID;
            this._listenerReaderCount = 0;
        }

        internal void Add(TElem listener)
        {
            this.listeners.Add(listener);
        }

        internal int IndexOfReference(TElem listener)
        {
            return Index.IndexOfReference<TElem>(this.listeners, listener);
        }

        internal void Notify<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3, Action<TElem, TElem, T1, T2, T3> action)
        {
            int count = this.listeners.Count;
            if (0 < count)
            {
                int nullIndex = -1;
                this._listenerReaderCount++;
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        TElem local = this.listeners[i];
                        if (this.filter(local))
                        {
                            action(local, arg1, arg2, arg3);
                        }
                        else
                        {
                            TElem local2 = default(TElem);
                            this.listeners[i] = local2;
                            nullIndex = i;
                        }
                    }
                }
                finally
                {
                    this._listenerReaderCount--;
                }
                if (this._listenerReaderCount == 0)
                {
                    this.RemoveNullListeners(nullIndex);
                }
            }
        }

        internal void Remove(TElem listener)
        {
            int index = this.IndexOfReference(listener);
            this.listeners[index] = default(TElem);
            if (this._listenerReaderCount == 0)
            {
                this.listeners.RemoveAt(index);
                this.listeners.TrimExcess();
            }
        }

        private void RemoveNullListeners(int nullIndex)
        {
            for (int i = nullIndex; 0 <= i; i--)
            {
                if (this.listeners[i] == null)
                {
                    this.listeners.RemoveAt(i);
                }
            }
        }

        internal bool HasListeners
        {
            get
            {
                return (0 < this.listeners.Count);
            }
        }

        internal delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

        internal delegate TResult Func<T1, TResult>(T1 arg1);
    }
}

