namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Runtime.Remoting.Messaging;

    internal interface IProxyInvoke
    {
        IntPtr GetOuterIUnknown();
        IMessage LocalInvoke(IMessage msg);
    }
}

