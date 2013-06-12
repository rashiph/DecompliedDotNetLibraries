namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IChannelDataStore
    {
        string[] ChannelUris { [SecurityCritical] get; }

        object this[object key] { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

