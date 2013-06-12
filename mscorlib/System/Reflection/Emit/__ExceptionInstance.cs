namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct __ExceptionInstance
    {
        internal int m_exceptionClass;
        internal int m_startAddress;
        internal int m_endAddress;
        internal int m_filterAddress;
        internal int m_handleAddress;
        internal int m_handleEndAddress;
        internal int m_type;
        internal __ExceptionInstance(int start, int end, int filterAddr, int handle, int handleEnd, int type, int exceptionClass)
        {
            this.m_startAddress = start;
            this.m_endAddress = end;
            this.m_filterAddress = filterAddr;
            this.m_handleAddress = handle;
            this.m_handleEndAddress = handleEnd;
            this.m_type = type;
            this.m_exceptionClass = exceptionClass;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is __ExceptionInstance))
            {
                return false;
            }
            __ExceptionInstance instance = (__ExceptionInstance) obj;
            return (((((instance.m_exceptionClass == this.m_exceptionClass) && (instance.m_startAddress == this.m_startAddress)) && ((instance.m_endAddress == this.m_endAddress) && (instance.m_filterAddress == this.m_filterAddress))) && (instance.m_handleAddress == this.m_handleAddress)) && (instance.m_handleEndAddress == this.m_handleEndAddress));
        }

        public override int GetHashCode()
        {
            return ((((((this.m_exceptionClass ^ this.m_startAddress) ^ this.m_endAddress) ^ this.m_filterAddress) ^ this.m_handleAddress) ^ this.m_handleEndAddress) ^ this.m_type);
        }
    }
}

