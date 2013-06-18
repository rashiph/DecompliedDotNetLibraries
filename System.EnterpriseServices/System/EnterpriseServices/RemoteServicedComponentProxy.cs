namespace System.EnterpriseServices
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Remoting.Services;
    using System.Security.Permissions;

    internal class RemoteServicedComponentProxy : RealProxy
    {
        private bool _fAttachedServer;
        private bool _fUseIntfDispatch;
        private static MethodInfo _getHashCodeMethod = typeof(object).GetMethod("GetHashCode");
        private static MethodBase _getIDisposableDispose = typeof(IDisposable).GetMethod("Dispose", new Type[0]);
        private static MethodBase _getServicedComponentDispose = typeof(ServicedComponent).GetMethod("Dispose", new Type[0]);
        private static MethodInfo _getTypeMethod = typeof(object).GetMethod("GetType");
        private volatile System.EnterpriseServices.RemotingIntermediary _intermediary;
        private static MethodInfo _isInstanceOfTypeMethod = typeof(MarshalByRefObject).GetMethod("IsInstanceOfType");
        private Type _pt;
        private IntPtr _pUnk;
        private object _server;

        private RemoteServicedComponentProxy()
        {
        }

        internal RemoteServicedComponentProxy(Type serverType, IntPtr pUnk, bool fAttachServer) : base(serverType)
        {
            this._fUseIntfDispatch = ServicedComponentInfo.IsTypeEventSource(serverType) || ServicedComponentInfo.AreMethodsSecure(serverType);
            if (pUnk != IntPtr.Zero)
            {
                this._pUnk = pUnk;
                this._server = EnterpriseServicesHelper.WrapIUnknownWithComObject(pUnk);
                if (fAttachServer)
                {
                    base.AttachServer((MarshalByRefObject) this._server);
                    this._fAttachedServer = true;
                }
            }
        }

        private void AssertValid()
        {
            if (this._server == null)
            {
                throw new ObjectDisposedException("ServicedComponent");
            }
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            return new ServicedComponentMarshaler((MarshalByRefObject) this.GetTransparentProxy(), requestedType);
        }

        internal void Dispose(bool disposing)
        {
            object o = this._server;
            this._server = null;
            if (o != null)
            {
                this._pUnk = IntPtr.Zero;
                if (disposing)
                {
                    Marshal.ReleaseComObject(o);
                }
                if (this._fAttachedServer)
                {
                    base.DetachServer();
                    this._fAttachedServer = false;
                }
            }
        }

        ~RemoteServicedComponentProxy()
        {
            this.Dispose(false);
        }

        public override IntPtr GetCOMIUnknown(bool fIsMarshalled)
        {
            if (this._server != null)
            {
                return Marshal.GetIUnknownForObject(this._server);
            }
            return IntPtr.Zero;
        }

        public override IMessage Invoke(IMessage reqMsg)
        {
            this.AssertValid();
            IMessage message = null;
            if (reqMsg is IConstructionCallMessage)
            {
                if (((IConstructionCallMessage) reqMsg).ArgCount > 0)
                {
                    throw new ServicedComponentException(Resource.FormatString("ServicedComponentException_ConstructorArguments"));
                }
                MarshalByRefObject transparentProxy = (MarshalByRefObject) this.GetTransparentProxy();
                return EnterpriseServicesHelper.CreateConstructionReturnMessage((IConstructionCallMessage) reqMsg, transparentProxy);
            }
            MethodBase methodBase = ((IMethodMessage) reqMsg).MethodBase;
            MemberInfo mi = methodBase;
            if (methodBase == _getTypeMethod)
            {
                IMethodCallMessage mcm = (IMethodCallMessage) reqMsg;
                return new ReturnMessage(this.ProxiedType, null, 0, mcm.LogicalCallContext, mcm);
            }
            if (methodBase == _getHashCodeMethod)
            {
                int hashCode = this.GetHashCode();
                IMethodCallMessage message3 = (IMethodCallMessage) reqMsg;
                return new ReturnMessage(hashCode, null, 0, message3.LogicalCallContext, message3);
            }
            if (methodBase == _isInstanceOfTypeMethod)
            {
                IMethodCallMessage message4 = (IMethodCallMessage) reqMsg;
                Type inArg = (Type) message4.GetInArg(0);
                return new ReturnMessage(inArg.IsInstanceOfType(this.ProxiedType), null, 0, message4.LogicalCallContext, message4);
            }
            MemberInfo m = ReflectionCache.ConvertToClassMI(this.ProxiedType, mi);
            try
            {
                int num2;
                if ((this._fUseIntfDispatch || (((num2 = ServicedComponentInfo.MICachedLookup(m)) & 4) != 0)) || ((num2 & 8) != 0))
                {
                    MemberInfo info3 = ReflectionCache.ConvertToInterfaceMI(mi);
                    if (info3 == null)
                    {
                        throw new ServicedComponentException(Resource.FormatString("ServicedComponentException_SecurityMapping"));
                    }
                    MethodCallMessageWrapperEx ex = new MethodCallMessageWrapperEx((IMethodCallMessage) reqMsg, (MethodBase) info3);
                    message = RemotingServices.ExecuteMessage((MarshalByRefObject) this._server, ex);
                }
                else
                {
                    string str2;
                    bool flag = (num2 & 2) != 0;
                    string s = ComponentServices.ConvertToString(reqMsg);
                    IRemoteDispatch dispatch = (IRemoteDispatch) this._server;
                    if (flag)
                    {
                        str2 = dispatch.RemoteDispatchAutoDone(s);
                    }
                    else
                    {
                        str2 = dispatch.RemoteDispatchNotAutoDone(s);
                    }
                    message = ComponentServices.ConvertToReturnMessage(str2, reqMsg);
                }
            }
            catch (COMException exception)
            {
                if ((exception.ErrorCode != -2147164158) && (exception.ErrorCode != -2147164157))
                {
                    throw;
                }
                if (!this.IsDisposeRequest(reqMsg))
                {
                    throw;
                }
                IMethodCallMessage message5 = reqMsg as IMethodCallMessage;
                message = new ReturnMessage(null, null, 0, message5.LogicalCallContext, message5);
            }
            if (this.IsDisposeRequest(reqMsg))
            {
                this.Dispose(true);
            }
            return message;
        }

        private bool IsDisposeRequest(IMessage msg)
        {
            IMethodCallMessage message = msg as IMethodCallMessage;
            if (message != null)
            {
                MethodBase methodBase = message.MethodBase;
                if ((methodBase == _getServicedComponentDispose) || (methodBase == _getIDisposableDispose))
                {
                    return true;
                }
            }
            return false;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public override void SetCOMIUnknown(IntPtr pUnk)
        {
            if (this._server == null)
            {
                this._pUnk = pUnk;
                this._server = EnterpriseServicesHelper.WrapIUnknownWithComObject(pUnk);
            }
        }

        private Type ProxiedType
        {
            get
            {
                if (this._pt == null)
                {
                    this._pt = base.GetProxiedType();
                }
                return this._pt;
            }
        }

        internal System.EnterpriseServices.RemotingIntermediary RemotingIntermediary
        {
            get
            {
                if (this._intermediary == null)
                {
                    lock (this)
                    {
                        if (this._intermediary == null)
                        {
                            this._intermediary = new System.EnterpriseServices.RemotingIntermediary(this);
                        }
                    }
                }
                return this._intermediary;
            }
        }
    }
}

