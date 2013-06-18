namespace System.Data.SqlClient
{
    using System;
    using System.Data.ProviderBase;

    internal sealed class SqlReferenceCollection : DbReferenceCollection
    {
        private int _dataReaderCount;
        internal const int DataReaderTag = 1;

        public override void Add(object value, int tag)
        {
            this._dataReaderCount++;
            base.AddItem(value, tag);
        }

        internal void Deactivate()
        {
            if (this.MayHaveDataReader)
            {
                base.Notify(0);
                this._dataReaderCount = 0;
            }
            base.Purge();
        }

        internal SqlDataReader FindLiveReader(SqlCommand command)
        {
            if (this.MayHaveDataReader)
            {
                foreach (SqlDataReader reader in base.Filter(1))
                {
                    if (((reader != null) && !reader.IsClosed) && ((command == null) || (command == reader.Command)))
                    {
                        return reader;
                    }
                }
            }
            return null;
        }

        protected override bool NotifyItem(int message, int tag, object value)
        {
            SqlDataReader reader = (SqlDataReader) value;
            if (!reader.IsClosed)
            {
                reader.CloseReaderFromConnection();
            }
            return false;
        }

        public override void Remove(object value)
        {
            this._dataReaderCount--;
            base.RemoveItem(value);
        }

        internal bool MayHaveDataReader
        {
            get
            {
                return (0 != this._dataReaderCount);
            }
        }
    }
}

