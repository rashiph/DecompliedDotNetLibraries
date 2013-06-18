namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BD3E2E12-42DD-40f4-A09A-95A50C58304B")]
    public interface IServiceCall
    {
        void OnCall();
    }
}

