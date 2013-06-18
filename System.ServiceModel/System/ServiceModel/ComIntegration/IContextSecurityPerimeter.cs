namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("A7549A29-A7C4-42e1-8DC1-7E3D748DC24A")]
    internal interface IContextSecurityPerimeter
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetPerimeterFlag();
        void SetPerimeterFlag([MarshalAs(UnmanagedType.Bool)] bool flag);
    }
}

