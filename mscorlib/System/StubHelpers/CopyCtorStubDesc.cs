namespace System.StubHelpers
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CopyCtorStubDesc
    {
        public IntPtr m_pCookie;
        public IntPtr m_pTarget;
    }
}

