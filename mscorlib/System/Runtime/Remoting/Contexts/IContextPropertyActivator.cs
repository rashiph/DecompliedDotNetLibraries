namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Activation;
    using System.Security;

    [ComVisible(true)]
    public interface IContextPropertyActivator
    {
        [SecurityCritical]
        void CollectFromClientContext(IConstructionCallMessage msg);
        [SecurityCritical]
        void CollectFromServerContext(IConstructionReturnMessage msg);
        [SecurityCritical]
        bool DeliverClientContextToServerContext(IConstructionCallMessage msg);
        [SecurityCritical]
        bool DeliverServerContextToClientContext(IConstructionReturnMessage msg);
        [SecurityCritical]
        bool IsOKToActivate(IConstructionCallMessage msg);
    }
}

