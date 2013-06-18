namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("da91b74e-5388-4783-949d-c1cd5fb00506"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IManagedPoolAction
    {
        void LastRelease();
    }
}

