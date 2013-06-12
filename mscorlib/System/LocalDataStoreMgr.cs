namespace System
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    internal sealed class LocalDataStoreMgr
    {
        private const int InitialSlotTableSize = 0x40;
        private const int LargeSlotTableSizeIncrease = 0x80;
        private long m_CookieGenerator;
        private int m_FirstAvailableSlot;
        private Dictionary<string, LocalDataStoreSlot> m_KeyToSlotMap = new Dictionary<string, LocalDataStoreSlot>();
        private List<LocalDataStore> m_ManagedLocalDataStores = new List<LocalDataStore>();
        private bool[] m_SlotInfoTable = new bool[0x40];
        private const int SlotTableDoubleThreshold = 0x200;

        [SecuritySafeCritical]
        public LocalDataStoreSlot AllocateDataSlot()
        {
            LocalDataStoreSlot slot2;
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                long num4;
                Monitor.Enter(this, ref lockTaken);
                int length = this.m_SlotInfoTable.Length;
                if (this.m_FirstAvailableSlot >= length)
                {
                    int num2;
                    if (length < 0x200)
                    {
                        num2 = length * 2;
                    }
                    else
                    {
                        num2 = length + 0x80;
                    }
                    bool[] destinationArray = new bool[num2];
                    Array.Copy(this.m_SlotInfoTable, destinationArray, length);
                    this.m_SlotInfoTable = destinationArray;
                    this.m_FirstAvailableSlot = length;
                    length = num2;
                }
                int firstAvailableSlot = this.m_FirstAvailableSlot;
                while (true)
                {
                    if (!this.m_SlotInfoTable[firstAvailableSlot])
                    {
                        break;
                    }
                    firstAvailableSlot++;
                }
                this.m_SlotInfoTable[firstAvailableSlot] = true;
                this.m_CookieGenerator = (num4 = this.m_CookieGenerator) + 1L;
                LocalDataStoreSlot slot = new LocalDataStoreSlot(this, firstAvailableSlot, num4);
                firstAvailableSlot++;
                while (firstAvailableSlot < length)
                {
                    if (this.m_SlotInfoTable[firstAvailableSlot])
                    {
                        break;
                    }
                    firstAvailableSlot++;
                }
                this.m_FirstAvailableSlot = firstAvailableSlot;
                slot2 = slot;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
            return slot2;
        }

        [SecuritySafeCritical]
        public LocalDataStoreSlot AllocateNamedDataSlot(string name)
        {
            LocalDataStoreSlot slot2;
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref lockTaken);
                LocalDataStoreSlot slot = this.AllocateDataSlot();
                this.m_KeyToSlotMap.Add(name, slot);
                slot2 = slot;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
            return slot2;
        }

        [SecuritySafeCritical]
        public LocalDataStoreHolder CreateLocalDataStore()
        {
            LocalDataStore store = new LocalDataStore(this, this.m_SlotInfoTable.Length);
            LocalDataStoreHolder holder = new LocalDataStoreHolder(store);
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref lockTaken);
                this.m_ManagedLocalDataStores.Add(store);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
            return holder;
        }

        [SecuritySafeCritical]
        public void DeleteLocalDataStore(LocalDataStore store)
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref lockTaken);
                this.m_ManagedLocalDataStores.Remove(store);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
        }

        [SecuritySafeCritical]
        internal void FreeDataSlot(int slot, long cookie)
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref lockTaken);
                for (int i = 0; i < this.m_ManagedLocalDataStores.Count; i++)
                {
                    this.m_ManagedLocalDataStores[i].FreeData(slot, cookie);
                }
                this.m_SlotInfoTable[slot] = false;
                if (slot < this.m_FirstAvailableSlot)
                {
                    this.m_FirstAvailableSlot = slot;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
        }

        [SecuritySafeCritical]
        public void FreeNamedDataSlot(string name)
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref lockTaken);
                this.m_KeyToSlotMap.Remove(name);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
        }

        [SecuritySafeCritical]
        public LocalDataStoreSlot GetNamedDataSlot(string name)
        {
            LocalDataStoreSlot slot2;
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref lockTaken);
                LocalDataStoreSlot valueOrDefault = this.m_KeyToSlotMap.GetValueOrDefault(name);
                if (valueOrDefault == null)
                {
                    return this.AllocateNamedDataSlot(name);
                }
                slot2 = valueOrDefault;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
            return slot2;
        }

        internal int GetSlotTableLength()
        {
            return this.m_SlotInfoTable.Length;
        }

        public void ValidateSlot(LocalDataStoreSlot slot)
        {
            if ((slot == null) || (slot.Manager != this))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ALSInvalidSlot"));
            }
        }
    }
}

