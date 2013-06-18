namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("BBC01830-8D3B-11D1-82EC-00A0C91EEDE9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface _ICompensator
    {
        void _SetLogControl(IntPtr logControl);
        void _BeginPrepare();
        [return: MarshalAs(UnmanagedType.Bool)]
        bool _PrepareRecord(_LogRecord record);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool _EndPrepare();
        void _BeginCommit(bool fRecovery);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool _CommitRecord(_LogRecord record);
        void _EndCommit();
        void _BeginAbort(bool fRecovery);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool _AbortRecord(_LogRecord record);
        void _EndAbort();
    }
}

