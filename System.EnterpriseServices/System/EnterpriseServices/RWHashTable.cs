namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Threading;

    internal sealed class RWHashTable
    {
        private Hashtable _hashtable = new Hashtable();
        private ReaderWriterLock _rwlock = new ReaderWriterLock();

        public object Get(object o)
        {
            object obj2;
            try
            {
                this._rwlock.AcquireReaderLock(-1);
                obj2 = this._hashtable[o];
            }
            finally
            {
                this._rwlock.ReleaseReaderLock();
            }
            return obj2;
        }

        public void Put(object key, object val)
        {
            try
            {
                this._rwlock.AcquireWriterLock(-1);
                this._hashtable[key] = val;
            }
            finally
            {
                this._rwlock.ReleaseWriterLock();
            }
        }
    }
}

