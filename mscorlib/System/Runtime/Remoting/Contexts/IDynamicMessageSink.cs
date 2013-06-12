namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [ComVisible(true)]
    public interface IDynamicMessageSink
    {
        [SecurityCritical]
        void ProcessMessageFinish(IMessage replyMsg, bool bCliSide, bool bAsync);
        [SecurityCritical]
        void ProcessMessageStart(IMessage reqMsg, bool bCliSide, bool bAsync);
    }
}

