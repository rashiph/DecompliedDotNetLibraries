namespace System.Data
{
    using System;

    public sealed class StatementCompletedEventArgs : EventArgs
    {
        private readonly int _recordCount;

        public StatementCompletedEventArgs(int recordCount)
        {
            this._recordCount = recordCount;
        }

        public int RecordCount
        {
            get
            {
                return this._recordCount;
            }
        }
    }
}

