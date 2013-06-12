namespace System.Runtime.Remoting.Messaging
{
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IMessageSink
    {
        [SecurityCritical]
        IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink);
        [SecurityCritical]
        IMessage SyncProcessMessage(IMessage msg);

        IMessageSink NextSink { [SecurityCritical] get; }
    }
}

