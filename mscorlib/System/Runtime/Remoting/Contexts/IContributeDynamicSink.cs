namespace System.Runtime.Remoting.Contexts
{
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IContributeDynamicSink
    {
        [SecurityCritical]
        IDynamicMessageSink GetDynamicSink();
    }
}

