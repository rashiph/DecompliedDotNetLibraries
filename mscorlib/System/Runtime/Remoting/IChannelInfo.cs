namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IChannelInfo
    {
        object[] ChannelData { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

