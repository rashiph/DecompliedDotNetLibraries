namespace System
{
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    internal sealed class LocalDataStore
    {
        private LocalDataStoreElement[] m_DataTable;
        private LocalDataStoreMgr m_Manager;

        public LocalDataStore(LocalDataStoreMgr mgr, int InitialCapacity)
        {
            this.m_Manager = mgr;
            this.m_DataTable = new LocalDataStoreElement[InitialCapacity];
        }

        internal void Dispose()
        {
            this.m_Manager.DeleteLocalDataStore(this);
        }

        internal void FreeData(int slot, long cookie)
        {
            if (slot < this.m_DataTable.Length)
            {
                LocalDataStoreElement element = this.m_DataTable[slot];
                if ((element != null) && (element.Cookie == cookie))
                {
                    this.m_DataTable[slot] = null;
                }
            }
        }

        public object GetData(LocalDataStoreSlot slot)
        {
            this.m_Manager.ValidateSlot(slot);
            int index = slot.Slot;
            if (index >= 0)
            {
                if (index >= this.m_DataTable.Length)
                {
                    return null;
                }
                LocalDataStoreElement element = this.m_DataTable[index];
                if (element == null)
                {
                    return null;
                }
                if (element.Cookie == slot.Cookie)
                {
                    return element.Value;
                }
            }
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SlotHasBeenFreed"));
        }

        [SecuritySafeCritical]
        private LocalDataStoreElement PopulateElement(LocalDataStoreSlot slot)
        {
            LocalDataStoreElement element;
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this.m_Manager, ref lockTaken);
                int index = slot.Slot;
                if (index < 0)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SlotHasBeenFreed"));
                }
                if (index >= this.m_DataTable.Length)
                {
                    LocalDataStoreElement[] destinationArray = new LocalDataStoreElement[this.m_Manager.GetSlotTableLength()];
                    Array.Copy(this.m_DataTable, destinationArray, this.m_DataTable.Length);
                    this.m_DataTable = destinationArray;
                }
                if (this.m_DataTable[index] == null)
                {
                    this.m_DataTable[index] = new LocalDataStoreElement(slot.Cookie);
                }
                element = this.m_DataTable[index];
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this.m_Manager);
                }
            }
            return element;
        }

        public void SetData(LocalDataStoreSlot slot, object data)
        {
            this.m_Manager.ValidateSlot(slot);
            int index = slot.Slot;
            if (index >= 0)
            {
                LocalDataStoreElement element = (index < this.m_DataTable.Length) ? this.m_DataTable[index] : null;
                if (element == null)
                {
                    element = this.PopulateElement(slot);
                }
                if (element.Cookie == slot.Cookie)
                {
                    element.Value = data;
                    return;
                }
            }
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SlotHasBeenFreed"));
        }
    }
}

