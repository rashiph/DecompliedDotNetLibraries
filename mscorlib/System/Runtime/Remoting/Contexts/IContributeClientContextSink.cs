namespace System.Runtime.Remoting.Contexts
{
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [ComVisible(true)]
    public interface IContributeClientContextSink
    {
        [SecurityCritical]
        IMessageSink GetClientContextSink(IMessageSink nextSink);
    }
}

