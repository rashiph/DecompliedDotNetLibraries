namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [ComVisible(true)]
    public interface IContributeObjectSink
    {
        [SecurityCritical]
        IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink nextSink);
    }
}

