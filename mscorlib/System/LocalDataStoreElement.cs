namespace System
{
    internal sealed class LocalDataStoreElement
    {
        private long m_cookie;
        private object m_value;

        public LocalDataStoreElement(long cookie)
        {
            this.m_cookie = cookie;
        }

        public long Cookie
        {
            get
            {
                return this.m_cookie;
            }
        }

        public object Value
        {
            get
            {
                return this.m_value;
            }
            set
            {
                this.m_value = value;
            }
        }
    }
}

