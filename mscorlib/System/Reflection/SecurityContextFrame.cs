namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityContextFrame
    {
        private IntPtr m_GSCookie;
        private IntPtr __VFN_table;
        private IntPtr m_Next;
        private IntPtr m_Assembly;
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public extern void Push(RuntimeAssembly assembly);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public extern void Pop();
    }
}

