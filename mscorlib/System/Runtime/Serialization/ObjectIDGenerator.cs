namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class ObjectIDGenerator
    {
        internal int m_currentCount = 1;
        internal int m_currentSize = sizes[0];
        internal long[] m_ids;
        internal object[] m_objs;
        private const int numbins = 4;
        private static readonly int[] sizes = new int[] { 
            5, 11, 0x1d, 0x2f, 0x61, 0xc5, 0x18d, 0x31d, 0x63d, 0xc83, 0x1915, 0x3235, 0x6475, 0xc8ed, 0x191dd, 0x323bf, 
            0x64787, 0xc8f4d, 0x191e9d, 0x323d49, 0x647a97
         };

        public ObjectIDGenerator()
        {
            this.m_ids = new long[this.m_currentSize * 4];
            this.m_objs = new object[this.m_currentSize * 4];
        }

        private int FindElement(object obj, out bool found)
        {
            int hashCode = RuntimeHelpers.GetHashCode(obj);
            int num2 = 1 + ((hashCode & 0x7fffffff) % (this.m_currentSize - 2));
            while (true)
            {
                int num3 = ((hashCode & 0x7fffffff) % this.m_currentSize) * 4;
                for (int i = num3; i < (num3 + 4); i++)
                {
                    if (this.m_objs[i] == null)
                    {
                        found = false;
                        return i;
                    }
                    if (this.m_objs[i] == obj)
                    {
                        found = true;
                        return i;
                    }
                }
                hashCode += num2;
            }
        }

        [SecuritySafeCritical]
        public virtual long GetId(object obj, out bool firstTime)
        {
            bool flag;
            long num;
            if (obj == null)
            {
                throw new ArgumentNullException("obj", Environment.GetResourceString("ArgumentNull_Obj"));
            }
            int index = this.FindElement(obj, out flag);
            if (!flag)
            {
                this.m_objs[index] = obj;
                this.m_ids[index] = this.m_currentCount++;
                num = this.m_ids[index];
                if (this.m_currentCount > ((this.m_currentSize * 4) / 2))
                {
                    this.Rehash();
                }
            }
            else
            {
                num = this.m_ids[index];
            }
            firstTime = !flag;
            return num;
        }

        public virtual long HasId(object obj, out bool firstTime)
        {
            bool flag;
            if (obj == null)
            {
                throw new ArgumentNullException("obj", Environment.GetResourceString("ArgumentNull_Obj"));
            }
            int index = this.FindElement(obj, out flag);
            if (flag)
            {
                firstTime = false;
                return this.m_ids[index];
            }
            firstTime = true;
            return 0L;
        }

        private void Rehash()
        {
            int index = 0;
            int currentSize = this.m_currentSize;
            while ((index < sizes.Length) && (sizes[index] <= currentSize))
            {
                index++;
            }
            if (index == sizes.Length)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_TooManyElements"));
            }
            this.m_currentSize = sizes[index];
            long[] numArray = new long[this.m_currentSize * 4];
            object[] objArray = new object[this.m_currentSize * 4];
            long[] ids = this.m_ids;
            object[] objs = this.m_objs;
            this.m_ids = numArray;
            this.m_objs = objArray;
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] != null)
                {
                    bool flag;
                    int num2 = this.FindElement(objs[i], out flag);
                    this.m_objs[num2] = objs[i];
                    this.m_ids[num2] = ids[i];
                }
            }
        }
    }
}

