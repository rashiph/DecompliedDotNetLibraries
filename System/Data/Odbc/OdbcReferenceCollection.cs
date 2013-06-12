namespace System.Data.Odbc
{
    using System;
    using System.Data.ProviderBase;

    internal sealed class OdbcReferenceCollection : DbReferenceCollection
    {
        internal const int Closing = 0;
        internal const int CommandTag = 1;
        internal const int Recover = 1;

        public override void Add(object value, int tag)
        {
            base.AddItem(value, tag);
        }

        protected override bool NotifyItem(int message, int tag, object value)
        {
            switch (message)
            {
                case 0:
                    if (1 == tag)
                    {
                        ((OdbcCommand) value).CloseFromConnection();
                    }
                    break;

                case 1:
                    if (1 == tag)
                    {
                        ((OdbcCommand) value).RecoverFromConnection();
                    }
                    break;
            }
            return false;
        }

        public override void Remove(object value)
        {
            base.RemoveItem(value);
        }
    }
}

