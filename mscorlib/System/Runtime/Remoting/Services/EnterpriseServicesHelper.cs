namespace System.Runtime.Remoting.Services
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Security;

    [ComVisible(true), SecurityCritical]
    public sealed class EnterpriseServicesHelper
    {
        [ComVisible(true)]
        public static IConstructionReturnMessage CreateConstructionReturnMessage(IConstructionCallMessage ctorMsg, MarshalByRefObject retObj)
        {
            return new ConstructorReturnMessage(retObj, null, 0, null, ctorMsg);
        }

        [SecurityCritical]
        public static void SwitchWrappers(RealProxy oldcp, RealProxy newcp)
        {
            object transparentProxy = oldcp.GetTransparentProxy();
            object tp = newcp.GetTransparentProxy();
            RemotingServices.GetServerContextForProxy(transparentProxy);
            RemotingServices.GetServerContextForProxy(tp);
            Marshal.InternalSwitchCCW(transparentProxy, tp);
        }

        [SecurityCritical]
        public static object WrapIUnknownWithComObject(IntPtr punk)
        {
            return Marshal.InternalWrapIUnknownWithComObject(punk);
        }
    }
}

