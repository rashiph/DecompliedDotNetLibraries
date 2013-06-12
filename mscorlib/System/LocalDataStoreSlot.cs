namespace System
{
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public sealed class LocalDataStoreSlot
    {
        private long m_cookie;
        private LocalDataStoreMgr m_mgr;
        private int m_slot;

        internal LocalDataStoreSlot(LocalDataStoreMgr mgr, int slot, long cookie)
        {
            this.m_mgr = mgr;
            this.m_slot = slot;
            this.m_cookie = cookie;
        }

        ~LocalDataStoreSlot()
        {
            LocalDataStoreMgr mgr = this.m_mgr;
            if (mgr != null)
            {
                int slot = this.m_slot;
                this.m_slot = -1;
                mgr.FreeDataSlot(slot, this.m_cookie);
            }
        }

        internal long Cookie
        {
            get
            {
                return this.m_cookie;
            }
        }

        internal LocalDataStoreMgr Manager
        {
            get
            {
                return this.m_mgr;
            }
        }

        internal int Slot
        {
            get
            {
                return this.m_slot;
            }
        }
    }
}

