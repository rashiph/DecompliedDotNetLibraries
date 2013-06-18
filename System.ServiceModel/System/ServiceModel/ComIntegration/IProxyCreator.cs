namespace System.ServiceModel.ComIntegration
{
    using System;

    internal interface IProxyCreator : IDisposable
    {
        ComProxy CreateProxy(IntPtr outer, ref Guid riid);
        bool SupportsDispatch();
        bool SupportsErrorInfo(ref Guid riid);
        bool SupportsIntrinsics();
    }
}

