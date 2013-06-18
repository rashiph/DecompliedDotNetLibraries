namespace System.Web
{
    using System;
    using System.Collections;

    public sealed class TraceContextEventArgs : EventArgs
    {
        private ICollection _records;

        public TraceContextEventArgs(ICollection records)
        {
            this._records = records;
        }

        public ICollection TraceRecords
        {
            get
            {
                return this._records;
            }
        }
    }
}

