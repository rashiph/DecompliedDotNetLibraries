namespace System.Runtime.Serialization
{
    using System;

    internal class ObjectHolderListEnumerator
    {
        private int m_currPos;
        private bool m_isFixupEnumerator;
        private ObjectHolderList m_list;
        private int m_startingVersion;

        internal ObjectHolderListEnumerator(ObjectHolderList list, bool isFixupEnumerator)
        {
            this.m_list = list;
            this.m_startingVersion = this.m_list.Version;
            this.m_currPos = -1;
            this.m_isFixupEnumerator = isFixupEnumerator;
        }

        internal bool MoveNext()
        {
            if (this.m_isFixupEnumerator)
            {
                while ((++this.m_currPos < this.m_list.Count) && this.m_list.m_values[this.m_currPos].CompletelyFixed)
                {
                }
                if (this.m_currPos == this.m_list.Count)
                {
                    return false;
                }
                return true;
            }
            this.m_currPos++;
            if (this.m_currPos == this.m_list.Count)
            {
                return false;
            }
            return true;
        }

        internal ObjectHolder Current
        {
            get
            {
                return this.m_list.m_values[this.m_currPos];
            }
        }
    }
}

