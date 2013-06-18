namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("2BE74789-F70B-42a3-80CA-E91743385844"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IWDEProgramPublisher
    {
        void Publish([MarshalAs(UnmanagedType.IUnknown)] object ProgramNode);
        void Unpublish([MarshalAs(UnmanagedType.IUnknown)] object ProgramNode);
    }
}

