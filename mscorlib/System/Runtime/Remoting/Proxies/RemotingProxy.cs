namespace System.Runtime.Remoting.Proxies
{
    using System;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Threading;

    [SecurityCritical]
    internal class RemotingProxy : RealProxy, IRemotingTypeInfo
    {
        private ConstructorCallMessage _ccm;
        private int _ctorThread;
        private static MethodInfo _getHashCodeMethod = typeof(object).GetMethod("GetHashCode");
        private static MethodInfo _getTypeMethod = typeof(object).GetMethod("GetType");
        private static RuntimeType s_typeofMarshalByRefObject = ((RuntimeType) typeof(MarshalByRefObject));
        private static RuntimeType s_typeofObject = ((RuntimeType) typeof(object));

        private RemotingProxy()
        {
        }

        public RemotingProxy(Type serverType) : base(serverType)
        {
        }

        internal static IMessage CallProcessMessage(IMessageSink ms, IMessage reqMsg, ArrayWithSize proxySinks, Thread currentThread, Context currentContext, bool bSkippingContextChain)
        {
            if (proxySinks != null)
            {
                DynamicPropertyHolder.NotifyDynamicSinks(reqMsg, proxySinks, true, true, false);
            }
            bool flag = false;
            if (bSkippingContextChain)
            {
                flag = currentContext.NotifyDynamicSinks(reqMsg, true, true, false, true);
                ChannelServices.NotifyProfiler(reqMsg, RemotingProfilerEvent.ClientSend);
            }
            if (ms == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Proxy_NoChannelSink"));
            }
            IMessage msg = ms.SyncProcessMessage(reqMsg);
            if (bSkippingContextChain)
            {
                ChannelServices.NotifyProfiler(msg, RemotingProfilerEvent.ClientReceive);
                if (flag)
                {
                    currentContext.NotifyDynamicSinks(msg, true, false, false, true);
                }
            }
            IMethodReturnMessage message2 = msg as IMethodReturnMessage;
            if ((msg == null) || (message2 == null))
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
            }
            if (proxySinks != null)
            {
                DynamicPropertyHolder.NotifyDynamicSinks(msg, proxySinks, true, false, false);
            }
            return msg;
        }

        [SecurityCritical]
        public bool CanCastTo(Type castType, object o)
        {
            if (castType == null)
            {
                throw new ArgumentNullException("castType");
            }
            RuntimeType fromType = castType as RuntimeType;
            if (fromType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            bool flag = false;
            if ((fromType == s_typeofObject) || (fromType == s_typeofMarshalByRefObject))
            {
                return true;
            }
            ObjRef objectRef = this.IdentityObject.ObjectRef;
            if (objectRef != null)
            {
                object transparentProxy = this.GetTransparentProxy();
                IRemotingTypeInfo typeInfo = objectRef.TypeInfo;
                if (typeInfo != null)
                {
                    flag = typeInfo.CanCastTo(fromType, transparentProxy);
                    if ((!flag && (typeInfo.GetType() == typeof(TypeInfo))) && objectRef.IsWellKnown())
                    {
                        flag = this.CanCastToWK(fromType);
                    }
                    return flag;
                }
                if (objectRef.IsObjRefLite())
                {
                    flag = MarshalByRefObject.CanCastToXmlTypeHelper(fromType, (MarshalByRefObject) o);
                }
                return flag;
            }
            return this.CanCastToWK(fromType);
        }

        private bool CanCastToWK(Type castType)
        {
            bool flag = false;
            if (castType.IsClass)
            {
                return base.GetProxiedType().IsAssignableFrom(castType);
            }
            if (!(this.IdentityObject is ServerIdentity))
            {
                flag = true;
            }
            return flag;
        }

        [SecurityCritical]
        public override IntPtr GetCOMIUnknown(bool fIsBeingMarshalled)
        {
            object transparentProxy = this.GetTransparentProxy();
            if (RemotingServices.IsObjectOutOfProcess(transparentProxy))
            {
                if (fIsBeingMarshalled)
                {
                    return MarshalByRefObject.GetComIUnknown((MarshalByRefObject) transparentProxy);
                }
                return MarshalByRefObject.GetComIUnknown((MarshalByRefObject) transparentProxy);
            }
            if (RemotingServices.IsObjectOutOfAppDomain(transparentProxy))
            {
                return ((MarshalByRefObject) transparentProxy).GetComIUnknown(fIsBeingMarshalled);
            }
            return MarshalByRefObject.GetComIUnknown((MarshalByRefObject) transparentProxy);
        }

        private IConstructionReturnMessage InternalActivate(IConstructionCallMessage ctorMsg)
        {
            this.CtorThread = Thread.CurrentThread.GetHashCode();
            IConstructionReturnMessage message = ActivationServices.Activate(this, ctorMsg);
            base.Initialized = true;
            return message;
        }

        internal virtual IMessage InternalInvoke(IMethodCallMessage reqMcmMsg, bool useDispatchMessage, int callType)
        {
            Message m = reqMcmMsg as Message;
            if ((m == null) && (callType != 0))
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Proxy_InvalidCallType"));
            }
            IMessage message2 = null;
            Thread currentThread = Thread.CurrentThread;
            LogicalCallContext logicalCallContext = currentThread.GetLogicalCallContext();
            Identity identityObject = this.IdentityObject;
            ServerIdentity identity2 = identityObject as ServerIdentity;
            if ((identity2 != null) && identityObject.IsFullyDisconnected())
            {
                throw new ArgumentException(Environment.GetResourceString("Remoting_ServerObjectNotFound", new object[] { reqMcmMsg.Uri }));
            }
            MethodBase methodBase = reqMcmMsg.MethodBase;
            if (_getTypeMethod == methodBase)
            {
                return new ReturnMessage(base.GetProxiedType(), null, 0, logicalCallContext, reqMcmMsg);
            }
            if (_getHashCodeMethod == methodBase)
            {
                return new ReturnMessage(identityObject.GetHashCode(), null, 0, logicalCallContext, reqMcmMsg);
            }
            if (identityObject.ChannelSink == null)
            {
                IMessageSink chnlSink = null;
                IMessageSink envoySink = null;
                if (!identityObject.ObjectRef.IsObjRefLite())
                {
                    RemotingServices.CreateEnvoyAndChannelSinks(null, identityObject.ObjectRef, out chnlSink, out envoySink);
                }
                else
                {
                    RemotingServices.CreateEnvoyAndChannelSinks(identityObject.ObjURI, null, out chnlSink, out envoySink);
                }
                RemotingServices.SetEnvoyAndChannelSinks(identityObject, chnlSink, envoySink);
                if (identityObject.ChannelSink == null)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Proxy_NoChannelSink"));
                }
            }
            IInternalMessage message3 = (IInternalMessage) reqMcmMsg;
            message3.IdentityObject = identityObject;
            if (identity2 != null)
            {
                message3.ServerIdentityObject = identity2;
            }
            else
            {
                message3.SetURI(identityObject.URI);
            }
            AsyncResult ar = null;
            switch (callType)
            {
                case 0:
                {
                    bool bSkippingContextChain = false;
                    Context currentContextInternal = currentThread.GetCurrentContextInternal();
                    IMessageSink envoyChain = identityObject.EnvoyChain;
                    if (currentContextInternal.IsDefaultContext && (envoyChain is EnvoyTerminatorSink))
                    {
                        bSkippingContextChain = true;
                        envoyChain = identityObject.ChannelSink;
                    }
                    return CallProcessMessage(envoyChain, reqMcmMsg, identityObject.ProxySideDynamicSinks, currentThread, currentContextInternal, bSkippingContextChain);
                }
                case 1:
                case 9:
                    logicalCallContext = (LogicalCallContext) logicalCallContext.Clone();
                    message3.SetCallContext(logicalCallContext);
                    ar = new AsyncResult(m);
                    this.InternalInvokeAsync(ar, m, useDispatchMessage, callType);
                    return new ReturnMessage(ar, null, 0, null, m);

                case 2:
                    return RealProxy.EndInvokeHelper(m, true);

                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    return message2;

                case 8:
                    logicalCallContext = (LogicalCallContext) logicalCallContext.Clone();
                    message3.SetCallContext(logicalCallContext);
                    this.InternalInvokeAsync(null, m, useDispatchMessage, callType);
                    return new ReturnMessage(null, null, 0, null, reqMcmMsg);

                case 10:
                    return new ReturnMessage(null, null, 0, null, reqMcmMsg);
            }
            return message2;
        }

        internal void InternalInvokeAsync(IMessageSink ar, Message reqMsg, bool useDispatchMessage, int callType)
        {
            Identity identityObject = this.IdentityObject;
            ServerIdentity identity2 = identityObject as ServerIdentity;
            MethodCall msg = new MethodCall(reqMsg);
            IInternalMessage message = msg;
            message.IdentityObject = identityObject;
            if (identity2 != null)
            {
                message.ServerIdentityObject = identity2;
            }
            if (useDispatchMessage)
            {
                ChannelServices.AsyncDispatchMessage(msg, ((callType & 8) != 0) ? null : ar);
            }
            else
            {
                if (identityObject.EnvoyChain == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Remoting_Proxy_InvalidState"));
                }
                identityObject.EnvoyChain.AsyncProcessMessage(msg, ((callType & 8) != 0) ? null : ar);
            }
            if (((callType & 1) != 0) && ((callType & 8) != 0))
            {
                ar.SyncProcessMessage(null);
            }
        }

        [SecurityCritical]
        public override IMessage Invoke(IMessage reqMsg)
        {
            IConstructionCallMessage ctorMsg = reqMsg as IConstructionCallMessage;
            if (ctorMsg != null)
            {
                return this.InternalActivate(ctorMsg);
            }
            if (!base.Initialized)
            {
                if (this.CtorThread != Thread.CurrentThread.GetHashCode())
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Proxy_InvalidCall"));
                }
                Identity identityObject = this.IdentityObject;
                RemotingServices.Wrap((ContextBoundObject) base.UnwrappedServerObject);
            }
            int callType = 0;
            Message message2 = reqMsg as Message;
            if (message2 != null)
            {
                callType = message2.GetCallType();
            }
            return this.InternalInvoke((IMethodCallMessage) reqMsg, false, callType);
        }

        private static void Invoke(object NotUsed, ref MessageData msgData)
        {
            Message reqMcmMsg = new Message();
            reqMcmMsg.InitFields(msgData);
            Delegate thisPtr = reqMcmMsg.GetThisPtr() as Delegate;
            if (thisPtr == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
            }
            RemotingProxy realProxy = (RemotingProxy) RemotingServices.GetRealProxy(thisPtr.Target);
            if (realProxy != null)
            {
                realProxy.InternalInvoke(reqMcmMsg, true, reqMcmMsg.GetCallType());
            }
            else
            {
                int callType = reqMcmMsg.GetCallType();
                switch (callType)
                {
                    case 1:
                    case 9:
                    {
                        reqMcmMsg.Properties[Message.CallContextKey] = CallContext.GetLogicalCallContext().Clone();
                        AsyncResult retVal = new AsyncResult(reqMcmMsg);
                        AgileAsyncWorkerItem state = new AgileAsyncWorkerItem(reqMcmMsg, ((callType & 8) != 0) ? null : retVal, thisPtr.Target);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(AgileAsyncWorkerItem.ThreadPoolCallBack), state);
                        if ((callType & 8) != 0)
                        {
                            retVal.SyncProcessMessage(null);
                        }
                        reqMcmMsg.PropagateOutParameters(null, retVal);
                        break;
                    }
                    case 2:
                        RealProxy.EndInvokeHelper(reqMcmMsg, false);
                        return;

                    case 10:
                        break;

                    default:
                        return;
                }
            }
        }

        [SecurityCritical]
        public override void SetCOMIUnknown(IntPtr i)
        {
        }

        internal ConstructorCallMessage ConstructorMessage
        {
            get
            {
                return this._ccm;
            }
            set
            {
                this._ccm = value;
            }
        }

        internal int CtorThread
        {
            get
            {
                return this._ctorThread;
            }
            set
            {
                this._ctorThread = value;
            }
        }

        public string TypeName
        {
            [SecurityCritical]
            get
            {
                return base.GetProxiedType().FullName;
            }
            [SecurityCritical]
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

