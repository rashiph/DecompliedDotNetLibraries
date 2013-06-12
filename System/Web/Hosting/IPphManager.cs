namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1cc9099d-0a8d-41cb-87d6-845e4f8c4e91")]
    public interface IPphManager
    {
        void StartProcessProtocolListenerChannel([In, MarshalAs(UnmanagedType.LPWStr)] string protocolId, IListenerChannelCallback listenerChannelCallback);
        void StopProcessProtocolListenerChannel([In, MarshalAs(UnmanagedType.LPWStr)] string protocolId, int listenerChannelId, bool immediate);
        void StopProcessProtocol([In, MarshalAs(UnmanagedType.LPWStr)] string protocolId, bool immediate);
    }
}

