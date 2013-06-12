namespace System.Reflection
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal sealed class CerHashtable<K, V>
    {
        private int m_count;
        private K[] m_key;
        private V[] m_value;
        private const int MinSize = 7;

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal CerHashtable() : this(7)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal CerHashtable(int size)
        {
            size = HashHelpers.GetPrime(size);
            this.m_key = new K[size];
            this.m_value = new V[size];
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static void Insert(K[] keys, V[] values, ref int count, K key, V value)
        {
            int hashCode = key.GetHashCode();
            if (hashCode < 0)
            {
                hashCode = ~hashCode;
            }
            int index = hashCode % keys.Length;
            while (true)
            {
                K local = keys[index];
                if (local == null)
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        return;
                    }
                    finally
                    {
                        keys[index] = key;
                        values[index] = value;
                        count++;
                    }
                }
                if (local.Equals(key))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_AddingDuplicate__", new object[] { local, key }));
                }
                index++;
                index = index % keys.Length;
            }
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal void Preallocate(int count)
        {
            bool lockTaken = false;
            bool flag2 = false;
            K[] keys = null;
            V[] values = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref lockTaken);
                int min = (count + this.m_count) * 2;
                if (min >= this.m_value.Length)
                {
                    min = HashHelpers.GetPrime(min);
                    keys = new K[min];
                    values = new V[min];
                    for (int i = 0; i < this.m_key.Length; i++)
                    {
                        K key = this.m_key[i];
                        if (key != null)
                        {
                            int num3 = 0;
                            CerHashtable<K, V>.Insert(keys, values, ref num3, key, this.m_value[i]);
                        }
                    }
                    flag2 = true;
                }
            }
            finally
            {
                if (flag2)
                {
                    this.m_key = keys;
                    this.m_value = values;
                }
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
        }

        internal V this[K key]
        {
            [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            get
            {
                V local2;
                bool lockTaken = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(this, ref lockTaken);
                    int hashCode = key.GetHashCode();
                    if (hashCode < 0)
                    {
                        hashCode = ~hashCode;
                    }
                    int index = hashCode % this.m_key.Length;
                    while (true)
                    {
                        K local = this.m_key[index];
                        if (local == null)
                        {
                            break;
                        }
                        if (local.Equals(key))
                        {
                            return this.m_value[index];
                        }
                        index++;
                        index = index % this.m_key.Length;
                    }
                    local2 = default(V);
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(this);
                    }
                }
                return local2;
            }
            [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            set
            {
                bool lockTaken = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(this, ref lockTaken);
                    CerHashtable<K, V>.Insert(this.m_key, this.m_value, ref this.m_count, key, value);
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(this);
                    }
                }
            }
        }
    }
}

