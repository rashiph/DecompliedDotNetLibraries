namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("458AB8A2-A1EA-4d7b-8EBE-DEE5D3D9442C"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IWin32Window
    {
        IntPtr Handle { get; }
    }
}

