namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Threading;

    internal class Cachetable
    {
        private Hashtable _cache = new Hashtable();
        private ReaderWriterLock _rwlock = new ReaderWriterLock();

        public object Get(object key)
        {
            object obj2;
            this._rwlock.AcquireReaderLock(-1);
            try
            {
                obj2 = this._cache[key];
            }
            finally
            {
                this._rwlock.ReleaseReaderLock();
            }
            return obj2;
        }

        public void Reset(object key, object nv)
        {
            this._rwlock.AcquireWriterLock(-1);
            try
            {
                this._cache[key] = nv;
            }
            finally
            {
                this._rwlock.ReleaseWriterLock();
            }
        }

        public object Set(object key, object nv)
        {
            object obj3;
            this._rwlock.AcquireWriterLock(-1);
            try
            {
                object obj2 = this._cache[key];
                if (obj2 == null)
                {
                    this._cache[key] = nv;
                    return nv;
                }
                obj3 = obj2;
            }
            finally
            {
                this._rwlock.ReleaseWriterLock();
            }
            return obj3;
        }
    }
}

