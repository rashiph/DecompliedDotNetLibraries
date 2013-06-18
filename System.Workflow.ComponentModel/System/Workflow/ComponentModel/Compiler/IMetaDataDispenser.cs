namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, CoClass(typeof(System.Workflow.ComponentModel.Compiler.MetaDataDispenser)), Guid("809C652E-7396-11d2-9771-00A0C9B4D50C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMetaDataDispenser
    {
        void DefineScope();
        void OpenScope([In, MarshalAs(UnmanagedType.LPWStr)] string scope, uint flags, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object unknown);
        void OpenScopeOnMemory();
    }
}

