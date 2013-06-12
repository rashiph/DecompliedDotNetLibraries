namespace System.Data.OleDb
{
    using System;
    using System.Data.ProviderBase;

    internal sealed class OleDbReferenceCollection : DbReferenceCollection
    {
        internal const int Canceling = -1;
        internal const int Closing = 0;
        internal const int CommandTag = 1;
        internal const int DataReaderTag = 2;

        public override void Add(object value, int tag)
        {
            base.AddItem(value, tag);
        }

        protected override bool NotifyItem(int message, int tag, object value)
        {
            bool canceling = -1 == message;
            if (1 == tag)
            {
                ((OleDbCommand) value).CloseCommandFromConnection(canceling);
            }
            else if (2 == tag)
            {
                ((OleDbDataReader) value).CloseReaderFromConnection(canceling);
            }
            return false;
        }

        public override void Remove(object value)
        {
            base.RemoveItem(value);
        }
    }
}

