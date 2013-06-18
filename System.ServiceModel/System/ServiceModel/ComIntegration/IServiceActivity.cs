namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("67532E0C-9E2F-4450-A354-035633944E17"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IServiceActivity
    {
        void SynchronousCall(IServiceCall pIServiceCall);
        void AsynchronousCall(IServiceCall pIServiceCall);
        void BindToCurrentThread();
        void UnbindFromThread();
    }
}

