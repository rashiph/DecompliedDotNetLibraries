namespace System.Data.ProviderBase
{
    using System;

    internal abstract class DbReferenceCollection
    {
        protected DbReferenceCollection()
        {
        }

        public abstract void Add(object value, int tag);
        public abstract void Notify(int message);
        public abstract void Remove(object value);
    }
}

