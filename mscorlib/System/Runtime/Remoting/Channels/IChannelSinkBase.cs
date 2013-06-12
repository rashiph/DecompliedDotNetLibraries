namespace System.Runtime.Remoting.Channels
{
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IChannelSinkBase
    {
        IDictionary Properties { [SecurityCritical] get; }
    }
}

