namespace System.ServiceModel.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("d860d655-0b79-4aa6-a741-ab216007ef55"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITransactionBridge
    {
        void Init([MarshalAs(UnmanagedType.IUnknown)] object bridgeNetworkConfiguration);
        void Start();
        void Shutdown();
    }
}

