namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal struct StateUnion
    {
        [FieldOffset(0)]
        public int AllElementsRequired;
        [FieldOffset(0)]
        public int CurPosIndex;
        [FieldOffset(0)]
        public int NumberOfRunningPos;
        [FieldOffset(0)]
        public int State;
    }
}

