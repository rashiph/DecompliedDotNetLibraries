namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("186d89bc-f277-4bcc-80d5-4df7b836ef4a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IServiceThreadPoolConfig
    {
        void SelectThreadPool(ThreadPoolOption threadPool);
        void SetBindingInfo(BindingOption binding);
    }
}

