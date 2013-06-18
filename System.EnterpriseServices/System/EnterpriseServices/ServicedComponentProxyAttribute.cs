namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.Class)]
    internal class ServicedComponentProxyAttribute : ProxyAttribute, ICustomFactory
    {
        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public override MarshalByRefObject CreateInstance(Type serverType)
        {
            RealProxy realProxy = null;
            MarshalByRefObject transparentProxy = null;
            ServicedComponentProxy.CleanupQueues(false);
            if ((RemotingConfiguration.IsWellKnownClientType(serverType) != null) || (RemotingConfiguration.IsRemotelyActivatedClientType(serverType) != null))
            {
                transparentProxy = base.CreateInstance(serverType);
                realProxy = RemotingServices.GetRealProxy(transparentProxy);
            }
            else
            {
                bool bIsAnotherProcess = false;
                string uri = "";
                bool flag2 = ServicedComponentInfo.IsTypeEventSource(serverType);
                IntPtr pUnk = Proxy.CoCreateObject(serverType, !flag2, ref bIsAnotherProcess, ref uri);
                if (pUnk != IntPtr.Zero)
                {
                    try
                    {
                        if (flag2)
                        {
                            realProxy = new RemoteServicedComponentProxy(serverType, pUnk, true);
                            transparentProxy = (MarshalByRefObject) realProxy.GetTransparentProxy();
                        }
                        else
                        {
                            bool flag3 = (RemotingConfiguration.IsWellKnownClientType(serverType) != null) || (null != RemotingConfiguration.IsRemotelyActivatedClientType(serverType));
                            if (bIsAnotherProcess && !flag3)
                            {
                                FastRSCPObjRef objectRef = new FastRSCPObjRef(pUnk, serverType, uri);
                                transparentProxy = (MarshalByRefObject) RemotingServices.Unmarshal(objectRef);
                            }
                            else
                            {
                                transparentProxy = (MarshalByRefObject) Marshal.GetObjectForIUnknown(pUnk);
                                if (!serverType.IsInstanceOfType(transparentProxy))
                                {
                                    throw new InvalidCastException(Resource.FormatString("ServicedComponentException_UnexpectedType", serverType, transparentProxy.GetType()));
                                }
                                realProxy = RemotingServices.GetRealProxy(transparentProxy);
                                if ((!bIsAnotherProcess && !(realProxy is ServicedComponentProxy)) && !(realProxy is RemoteServicedComponentProxy))
                                {
                                    ((ServicedComponent) transparentProxy).DoSetCOMIUnknown(pUnk);
                                }
                            }
                        }
                    }
                    finally
                    {
                        Marshal.Release(pUnk);
                    }
                }
            }
            if (realProxy is ServicedComponentProxy)
            {
                ServicedComponentProxy proxy2 = (ServicedComponentProxy) realProxy;
                if (proxy2.HomeToken == Proxy.GetCurrentContextToken())
                {
                    proxy2.FilterConstructors();
                }
            }
            return transparentProxy;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public override RealProxy CreateProxy(ObjRef objRef, Type serverType, object serverObject, Context serverContext)
        {
            if (objRef == null)
            {
                return base.CreateProxy(objRef, serverType, serverObject, serverContext);
            }
            if (!(objRef is FastRSCPObjRef) && (!(objRef is ServicedComponentMarshaler) || (objRef.IsFromThisProcess() && !ServicedComponentInfo.IsTypeEventSource(serverType))))
            {
                return base.CreateProxy(objRef, serverType, serverObject, serverContext);
            }
            return RemotingServices.GetRealProxy(objRef.GetRealObject(new StreamingContext(StreamingContextStates.Remoting)));
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        MarshalByRefObject ICustomFactory.CreateInstance(Type serverType)
        {
            RealProxy realProxy = null;
            ServicedComponentProxy.CleanupQueues(false);
            int num = ServicedComponentInfo.SCICachedLookup(serverType);
            bool fIsJitActivated = (num & 8) != 0;
            bool fIsPooled = (num & 0x10) != 0;
            bool fAreMethodsSecure = (num & 0x20) != 0;
            if (fIsJitActivated)
            {
                object obj2 = IdentityTable.FindObject(Proxy.GetCurrentContextToken());
                if (obj2 != null)
                {
                    realProxy = RemotingServices.GetRealProxy(obj2);
                }
            }
            if (realProxy == null)
            {
                realProxy = new ServicedComponentProxy(serverType, fIsJitActivated, fIsPooled, fAreMethodsSecure, true);
            }
            else if (realProxy is ServicedComponentProxy)
            {
                ((ServicedComponentProxy) realProxy).ConstructServer();
            }
            return (MarshalByRefObject) realProxy.GetTransparentProxy();
        }
    }
}

