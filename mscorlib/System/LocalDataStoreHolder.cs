namespace System
{
    internal sealed class LocalDataStoreHolder
    {
        private LocalDataStore m_Store;

        public LocalDataStoreHolder(LocalDataStore store)
        {
            this.m_Store = store;
        }

        ~LocalDataStoreHolder()
        {
            LocalDataStore store = this.m_Store;
            if (store != null)
            {
                store.Dispose();
            }
        }

        public LocalDataStore Store
        {
            get
            {
                return this.m_Store;
            }
        }
    }
}

