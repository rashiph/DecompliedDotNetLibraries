namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct ArrayWithOffset
    {
        [ForceTokenStabilization]
        private object m_array;
        [ForceTokenStabilization]
        private int m_offset;
        [ForceTokenStabilization]
        private int m_count;
        [SecuritySafeCritical]
        public ArrayWithOffset(object array, int offset)
        {
            this.m_array = array;
            this.m_offset = offset;
            this.m_count = 0;
            this.m_count = this.CalculateCount();
        }

        public object GetArray()
        {
            return this.m_array;
        }

        public int GetOffset()
        {
            return this.m_offset;
        }

        public override int GetHashCode()
        {
            return (this.m_count + this.m_offset);
        }

        public override bool Equals(object obj)
        {
            return ((obj is ArrayWithOffset) && this.Equals((ArrayWithOffset) obj));
        }

        public bool Equals(ArrayWithOffset obj)
        {
            return (((obj.m_array == this.m_array) && (obj.m_offset == this.m_offset)) && (obj.m_count == this.m_count));
        }

        public static bool operator ==(ArrayWithOffset a, ArrayWithOffset b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ArrayWithOffset a, ArrayWithOffset b)
        {
            return !(a == b);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern int CalculateCount();
    }
}

