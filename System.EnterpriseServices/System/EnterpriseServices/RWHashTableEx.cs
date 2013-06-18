namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class RWHashTableEx
    {
        private Hashtable _hashtable = new Hashtable();
        private ReaderWriterLock _rwlock = new ReaderWriterLock();

        public object Get(object o, out bool bFound)
        {
            object obj3;
            bFound = false;
            try
            {
                this._rwlock.AcquireReaderLock(-1);
                object obj2 = this._hashtable[o];
                if (obj2 != null)
                {
                    bFound = true;
                    return ((RWTableEntry) obj2)._realObject;
                }
                obj3 = null;
            }
            finally
            {
                this._rwlock.ReleaseReaderLock();
            }
            return obj3;
        }

        public void Put(object key, object val)
        {
            RWTableEntry entry = new RWTableEntry(val);
            try
            {
                this._rwlock.AcquireWriterLock(-1);
                this._hashtable[key] = entry;
            }
            finally
            {
                this._rwlock.ReleaseWriterLock();
            }
        }

        internal class RWTableEntry
        {
            internal object _realObject;

            public RWTableEntry(object o)
            {
                this._realObject = o;
            }
        }
    }
}

