namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct ActivityHandlerDescriptor
    {
        [MarshalAs(UnmanagedType.BStr)]
        public string Name;
        public int Token;
    }
}

