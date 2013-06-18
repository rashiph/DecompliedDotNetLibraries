namespace System.Data.OleDb
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Reflection;

    [Serializable, ListBindable(false)]
    public sealed class OleDbErrorCollection : ICollection, IEnumerable
    {
        private readonly ArrayList items;

        internal OleDbErrorCollection(UnsafeNativeMethods.IErrorInfo errorInfo)
        {
            ArrayList list = new ArrayList();
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OS> IErrorRecords\n");
            UnsafeNativeMethods.IErrorRecords errorRecords = errorInfo as UnsafeNativeMethods.IErrorRecords;
            if (errorRecords != null)
            {
                int recordCount = errorRecords.GetRecordCount();
                Bid.Trace("<oledb.IErrorRecords.GetRecordCount|API|OS|RET> RecordCount=%d\n", recordCount);
                for (int i = 0; i < recordCount; i++)
                {
                    OleDbError error = new OleDbError(errorRecords, i);
                    list.Add(error);
                }
            }
            this.items = list;
        }

        internal void AddRange(ICollection c)
        {
            this.items.AddRange(c);
        }

        public void CopyTo(Array array, int index)
        {
            this.items.CopyTo(array, index);
        }

        public void CopyTo(OleDbError[] array, int index)
        {
            this.items.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        public int Count
        {
            get
            {
                ArrayList items = this.items;
                if (items == null)
                {
                    return 0;
                }
                return items.Count;
            }
        }

        public OleDbError this[int index]
        {
            get
            {
                return (this.items[index] as OleDbError);
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

