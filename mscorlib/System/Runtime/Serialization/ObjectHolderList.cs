namespace System.Runtime.Serialization
{
    using System;

    internal class ObjectHolderList
    {
        internal const int DefaultInitialSize = 8;
        internal int m_count;
        internal ObjectHolder[] m_values;

        internal ObjectHolderList() : this(8)
        {
        }

        internal ObjectHolderList(int startingSize)
        {
            this.m_count = 0;
            this.m_values = new ObjectHolder[startingSize];
        }

        internal virtual void Add(ObjectHolder value)
        {
            if (this.m_count == this.m_values.Length)
            {
                this.EnlargeArray();
            }
            this.m_values[this.m_count++] = value;
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
            ObjectHolder[] destinationArray = new ObjectHolder[num];
            Array.Copy(this.m_values, destinationArray, this.m_count);
            this.m_values = destinationArray;
        }

        internal ObjectHolderListEnumerator GetFixupEnumerator()
        {
            return new ObjectHolderListEnumerator(this, true);
        }

        internal int Count
        {
            get
            {
                return this.m_count;
            }
        }

        internal int Version
        {
            get
            {
                return this.m_count;
            }
        }
    }
}

