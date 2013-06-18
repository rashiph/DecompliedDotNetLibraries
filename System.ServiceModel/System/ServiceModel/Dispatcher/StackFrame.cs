namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StackFrame
    {
        internal int basePtr;
        internal int endPtr;
        internal int Count
        {
            get
            {
                return ((this.endPtr - this.basePtr) + 1);
            }
        }
        internal int EndPtr
        {
            set
            {
                this.endPtr = value;
            }
        }
        internal int this[int offset]
        {
            get
            {
                return (this.basePtr + offset);
            }
        }
        internal bool IsValidPtr(int ptr)
        {
            return ((ptr >= this.basePtr) && (ptr <= this.endPtr));
        }
    }
}

