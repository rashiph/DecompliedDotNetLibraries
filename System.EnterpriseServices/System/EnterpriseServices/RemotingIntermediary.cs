namespace System.EnterpriseServices
{
    using System;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Security.Permissions;

    internal class RemotingIntermediary : RealProxy
    {
        private BlindMBRO _blind;
        private static MethodInfo _getCOMIUnknownMethod = typeof(MarshalByRefObject).GetMethod("GetComIUnknown", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(bool) }, null);
        private static MethodInfo _getLifetimeServiceMethod = typeof(MarshalByRefObject).GetMethod("GetLifetimeService", new Type[0]);
        private static MethodInfo _initializeLifetimeServiceMethod = typeof(MarshalByRefObject).GetMethod("InitializeLifetimeService", new Type[0]);
        private RealProxy _pxy;
        private static MethodInfo _setCOMIUnknownMethod = typeof(ServicedComponent).GetMethod("DoSetCOMIUnknown", BindingFlags.NonPublic | BindingFlags.Instance);

        internal RemotingIntermediary(RealProxy pxy) : base(pxy.GetProxiedType())
        {
            this._pxy = pxy;
            this._blind = new BlindMBRO((MarshalByRefObject) this.GetTransparentProxy());
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            return new IntermediaryObjRef((MarshalByRefObject) this.GetTransparentProxy(), requestedType, this._pxy);
        }

        public override IntPtr GetCOMIUnknown(bool fIsMarshalled)
        {
            return this._pxy.GetCOMIUnknown(fIsMarshalled);
        }

        private IMessage HandleSpecialMessages(IMessage reqmsg)
        {
            IMethodCallMessage mcm = reqmsg as IMethodCallMessage;
            MethodBase methodBase = mcm.MethodBase;
            if (methodBase == _initializeLifetimeServiceMethod)
            {
                return new ReturnMessage(this._blind.InitializeLifetimeService(), null, 0, mcm.LogicalCallContext, mcm);
            }
            if (methodBase == _getLifetimeServiceMethod)
            {
                return new ReturnMessage(this._blind.GetLifetimeService(), null, 0, mcm.LogicalCallContext, mcm);
            }
            return null;
        }

        public override IMessage Invoke(IMessage reqmsg)
        {
            IMessage message = this.HandleSpecialMessages(reqmsg);
            if (message != null)
            {
                return message;
            }
            return this._pxy.Invoke(reqmsg);
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public override void SetCOMIUnknown(IntPtr pUnk)
        {
            this._pxy.SetCOMIUnknown(pUnk);
        }
    }
}

