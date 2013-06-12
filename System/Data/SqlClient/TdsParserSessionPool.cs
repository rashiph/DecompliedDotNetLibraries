namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Generic;

    internal class TdsParserSessionPool
    {
        private readonly List<TdsParserStateObject> _cache;
        private int _cachedCount;
        private TdsParserStateObjectListStack _freeStack;
        private readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
        private static int _objectTypeCount;
        private readonly TdsParser _parser;
        private const int MaxInactiveCount = 10;

        internal TdsParserSessionPool(TdsParser parser)
        {
            this._parser = parser;
            this._cache = new List<TdsParserStateObject>();
            this._freeStack = new TdsParserStateObjectListStack();
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.TdsParserSessionPool.ctor|ADV> %d# created session pool for parser %d\n", this.ObjectID, parser.ObjectID);
            }
        }

        internal TdsParserStateObject CreateSession()
        {
            TdsParserStateObject item = this._parser.CreateSession();
            lock (this._cache)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.TdsParserSessionPool.CreateSession|ADV> %d# adding session %d to pool\n", this.ObjectID, item.ObjectID);
                }
                this._cache.Add(item);
                this._cachedCount = this._cache.Count;
            }
            return item;
        }

        internal void Deactivate()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.TdsParserSessionPool.Deactivate|ADV> %d# deactivating cachedCount=%d\n", this.ObjectID, this._cachedCount);
            try
            {
                lock (this._cache)
                {
                    for (int i = this._cache.Count - 1; i >= 0; i--)
                    {
                        TdsParserStateObject session = this._cache[i];
                        if ((session != null) && session.IsOrphaned)
                        {
                            if (Bid.AdvancedOn)
                            {
                                Bid.Trace("<sc.TdsParserSessionPool.Deactivate|ADV> %d# reclaiming session %d\n", this.ObjectID, session.ObjectID);
                            }
                            this.PutSession(session);
                        }
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void Dispose()
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.TdsParserSessionPool.Dispose|ADV> %d# disposing cachedCount=%d\n", this.ObjectID, this._cachedCount);
            }
            this._freeStack = null;
            lock (this._cache)
            {
                for (int i = 0; i < this._cache.Count; i++)
                {
                    TdsParserStateObject obj2 = this._cache[i];
                    if (obj2 != null)
                    {
                        obj2.Dispose();
                    }
                }
                this._cache.Clear();
            }
        }

        internal TdsParserStateObject GetSession(object owner)
        {
            TdsParserStateObject obj2 = this._freeStack.SynchronizedPop();
            if (obj2 == null)
            {
                obj2 = this.CreateSession();
            }
            obj2.Activate(owner);
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.TdsParserSessionPool.GetSession|ADV> %d# using session %d\n", this.ObjectID, obj2.ObjectID);
            }
            return obj2;
        }

        internal void PutSession(TdsParserStateObject session)
        {
            bool flag2 = session.Deactivate();
            if (!this.IsDisposed)
            {
                if (flag2 && (this._cachedCount < 10))
                {
                    if (Bid.AdvancedOn)
                    {
                        Bid.Trace("<sc.TdsParserSessionPool.PutSession|ADV> %d# keeping session %d cachedCount=%d\n", this.ObjectID, session.ObjectID, this._cachedCount);
                    }
                    this._freeStack.SynchronizedPush(session);
                }
                else
                {
                    if (Bid.AdvancedOn)
                    {
                        Bid.Trace("<sc.TdsParserSessionPool.PutSession|ADV> %d# disposing session %d cachedCount=%d\n", this.ObjectID, session.ObjectID, this._cachedCount);
                    }
                    lock (this._cache)
                    {
                        this._cache.Remove(session);
                        this._cachedCount = this._cache.Count;
                    }
                    session.Dispose();
                }
            }
        }

        internal string TraceString()
        {
            return string.Format(null, "(ObjID={0}, free={1}, cached={2}, total={3})", new object[] { this._objectID, (this._freeStack == null) ? "(null)" : this._freeStack.CountDebugOnly.ToString((IFormatProvider) null), this._cachedCount, this._cache.Count });
        }

        private bool IsDisposed
        {
            get
            {
                return (null == this._freeStack);
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        private class TdsParserStateObjectListStack
        {
            private TdsParserStateObject _stack;

            internal TdsParserStateObjectListStack()
            {
            }

            internal TdsParserStateObject SynchronizedPop()
            {
                TdsParserStateObject obj2;
                lock (this)
                {
                    obj2 = this._stack;
                    if (obj2 != null)
                    {
                        this._stack = obj2.NextPooledObject;
                        obj2.NextPooledObject = null;
                    }
                }
                return obj2;
            }

            internal void SynchronizedPush(TdsParserStateObject value)
            {
                lock (this)
                {
                    value.NextPooledObject = this._stack;
                    this._stack = value;
                }
            }

            internal int CountDebugOnly
            {
                get
                {
                    return -1;
                }
            }
        }
    }
}

