namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.ConstrainedExecution;

    [Serializable]
    internal sealed class CerArrayList<V>
    {
        private V[] m_array;
        private int m_count;
        private const int MinSize = 4;

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal CerArrayList(List<V> list)
        {
            this.m_array = new V[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                this.m_array[i] = list[i];
            }
            this.m_count = list.Count;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal CerArrayList(int length)
        {
            if (length < 4)
            {
                length = 4;
            }
            this.m_array = new V[length];
            this.m_count = 0;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Add(V value)
        {
            this.m_array[this.m_count] = value;
            this.m_count++;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal void Preallocate(int addition)
        {
            if ((this.m_array.Length - this.m_count) <= addition)
            {
                int num = ((this.m_array.Length * 2) > (this.m_array.Length + addition)) ? (this.m_array.Length * 2) : (this.m_array.Length + addition);
                V[] localArray = new V[num];
                for (int i = 0; i < this.m_count; i++)
                {
                    localArray[i] = this.m_array[i];
                }
                this.m_array = localArray;
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Replace(int index, V value)
        {
            if (index >= this.Count)
            {
                throw new InvalidOperationException();
            }
            this.m_array[index] = value;
        }

        internal int Count
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this.m_count;
            }
        }

        internal V this[int index]
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this.m_array[index];
            }
        }
    }
}

