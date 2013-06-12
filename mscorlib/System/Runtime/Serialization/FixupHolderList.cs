namespace System.Runtime.Serialization
{
    using System;

    [Serializable]
    internal class FixupHolderList
    {
        internal const int InitialSize = 2;
        internal int m_count;
        internal FixupHolder[] m_values;

        internal FixupHolderList() : this(2)
        {
        }

        internal FixupHolderList(int startingSize)
        {
            this.m_count = 0;
            this.m_values = new FixupHolder[startingSize];
        }

        internal virtual void Add(FixupHolder fixup)
        {
            if (this.m_count == this.m_values.Length)
            {
                this.EnlargeArray();
            }
            this.m_values[this.m_count++] = fixup;
        }

        internal virtual void Add(long id, object fixupInfo)
        {
            if (this.m_count == this.m_values.Length)
            {
                this.EnlargeArray();
            }
            this.m_values[this.m_count].m_id = id;
            this.m_values[this.m_count++].m_fixupInfo = fixupInfo;
        }

        private void EnlargeArray()
        {
            int num = this.m_values.Length * 2;
            if (num < 0)
            {
                if (num == 0x7fffffff)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_TooManyElements"));
                }
                num = 0x7fffffff;
            }
            FixupHolder[] destinationArray = new FixupHolder[num];
            Array.Copy(this.m_values, destinationArray, this.m_count);
            this.m_values = destinationArray;
        }
    }
}

