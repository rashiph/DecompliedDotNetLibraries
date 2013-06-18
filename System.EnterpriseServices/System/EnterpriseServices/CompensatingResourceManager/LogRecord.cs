namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;

    public sealed class LogRecord
    {
        internal object _data;
        internal LogRecordFlags _flags;
        internal int _seq;

        internal LogRecord()
        {
            this._flags = 0;
            this._seq = 0;
            this._data = null;
        }

        internal LogRecord(_LogRecord r)
        {
            this._flags = (LogRecordFlags) r.dwCrmFlags;
            this._seq = r.dwSequenceNumber;
            this._data = Packager.Deserialize(new BlobPackage(r.blobUserData));
        }

        public LogRecordFlags Flags
        {
            get
            {
                return this._flags;
            }
        }

        public object Record
        {
            get
            {
                return this._data;
            }
        }

        public int Sequence
        {
            get
            {
                return this._seq;
            }
        }
    }
}

