namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class ObjectToIdCache
    {
        internal int m_currentCount = 1;
        internal int[] m_ids = new int[GetPrime(1)];
        internal object[] m_objs;
        internal static readonly int[] primes = new int[] { 
            3, 7, 0x11, 0x25, 0x59, 0xc5, 0x1af, 0x397, 0x78b, 0xfd1, 0x20e3, 0x446f, 0x8e01, 0x126a7, 0x26315, 0x4f361, 
            0xa443b, 0x154a3f, 0x2c25c1, 0x5b8b6f
         };

        public ObjectToIdCache()
        {
            this.m_objs = new object[this.m_ids.Length];
        }

        private int FindElement(object obj, out bool isEmpty)
        {
            int num2 = (RuntimeHelpers.GetHashCode(obj) & 0x7fffffff) % this.m_objs.Length;
            for (int i = num2; i != (num2 - 1); i++)
            {
                if (this.m_objs[i] == null)
                {
                    isEmpty = true;
                    return i;
                }
                if (this.m_objs[i] == obj)
                {
                    isEmpty = false;
                    return i;
                }
                if (i == (this.m_objs.Length - 1))
                {
                    i = -1;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ObjectTableOverflow")));
        }

        public int GetId(object obj, ref bool newId)
        {
            bool flag;
            int index = this.FindElement(obj, out flag);
            if (!flag)
            {
                newId = false;
                return this.m_ids[index];
            }
            if (!newId)
            {
                return -1;
            }
            int num2 = this.m_currentCount++;
            this.m_objs[index] = obj;
            this.m_ids[index] = num2;
            if (this.m_currentCount >= (this.m_objs.Length - 1))
            {
                this.Rehash();
            }
            return num2;
        }

        private static int GetPrime(int min)
        {
            for (int i = 0; i < primes.Length; i++)
            {
                int num2 = primes[i];
                if (num2 >= min)
                {
                    return num2;
                }
            }
            for (int j = min | 1; j < 0x7fffffff; j += 2)
            {
                if (IsPrime(j))
                {
                    return j;
                }
            }
            return min;
        }

        private static bool IsPrime(int candidate)
        {
            if ((candidate & 1) == 0)
            {
                return (candidate == 2);
            }
            int num = (int) Math.Sqrt((double) candidate);
            for (int i = 3; i <= num; i += 2)
            {
                if ((candidate % i) == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public int ReassignId(int oldObjId, object oldObj, object newObj)
        {
            bool flag;
            int index = this.FindElement(oldObj, out flag);
            if (flag)
            {
                return 0;
            }
            int num2 = this.m_ids[index];
            if (oldObjId > 0)
            {
                this.m_ids[index] = oldObjId;
            }
            else
            {
                this.RemoveAt(index);
            }
            index = this.FindElement(newObj, out flag);
            int num3 = 0;
            if (!flag)
            {
                num3 = this.m_ids[index];
            }
            this.m_objs[index] = newObj;
            this.m_ids[index] = num2;
            return num3;
        }

        private void Rehash()
        {
            int prime = GetPrime(this.m_objs.Length * 2);
            int[] ids = this.m_ids;
            object[] objs = this.m_objs;
            this.m_ids = new int[prime];
            this.m_objs = new object[prime];
            for (int i = 0; i < objs.Length; i++)
            {
                object obj2 = objs[i];
                if (obj2 != null)
                {
                    bool flag;
                    int index = this.FindElement(obj2, out flag);
                    this.m_objs[index] = obj2;
                    this.m_ids[index] = ids[i];
                }
            }
        }

        private void RemoveAt(int pos)
        {
            int num3;
            int hashCode = RuntimeHelpers.GetHashCode(this.m_objs[pos]);
            for (int i = pos; i != (pos - 1); i = num3)
            {
                num3 = (i + 1) % this.m_objs.Length;
                if ((this.m_objs[num3] == null) || (RuntimeHelpers.GetHashCode(this.m_objs[num3]) != hashCode))
                {
                    this.m_objs[pos] = this.m_objs[i];
                    this.m_ids[pos] = this.m_ids[i];
                    this.m_objs[i] = null;
                    this.m_ids[i] = 0;
                    return;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ObjectTableOverflow")));
        }
    }
}

