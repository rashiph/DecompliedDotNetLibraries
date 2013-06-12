namespace System.Runtime.Remoting.Proxies
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;

    [ComVisible(true), SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public abstract class RealProxy
    {
        private static IntPtr _defaultStub = GetDefaultStub();
        private static object _defaultStubData = _defaultStubValue;
        private static IntPtr _defaultStubValue = new IntPtr(-1);
        internal int _domainID;
        private RealProxyFlags _flags;
        private object _identity;
        internal int _optFlags;
        private MarshalByRefObject _serverObject;
        internal GCHandle _srvIdentity;
        private object _tp;

        protected RealProxy()
        {
        }

        [SecurityCritical]
        protected RealProxy(Type classToProxy) : this(classToProxy, IntPtr.Zero, null)
        {
        }

        [SecurityCritical]
        protected RealProxy(Type classToProxy, IntPtr stub, object stubData)
        {
            if (!classToProxy.IsMarshalByRef && !classToProxy.IsInterface)
            {
                throw new ArgumentException(Environment.GetResourceString("Remoting_Proxy_ProxyTypeIsNotMBR"));
            }
            if (IntPtr.Zero == stub)
            {
                stub = _defaultStub;
                stubData = _defaultStubData;
            }
            this._tp = null;
            if (stubData == null)
            {
                throw new ArgumentNullException("stubdata");
            }
            this._tp = RemotingServices.CreateTransparentProxy(this, classToProxy, stub, stubData);
            if (this is RemotingProxy)
            {
                this._flags |= RealProxyFlags.RemotingProxy;
            }
        }

        [SecurityCritical]
        protected void AttachServer(MarshalByRefObject s)
        {
            object transparentProxy = this.GetTransparentProxy();
            if (transparentProxy != null)
            {
                RemotingServices.ResetInterfaceCache(transparentProxy);
            }
            this.AttachServerHelper(s);
        }

        [SecurityCritical]
        internal void AttachServerHelper(MarshalByRefObject s)
        {
            if ((s == null) || (this._serverObject != null))
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentNull_Generic"), "s");
            }
            this._serverObject = s;
            this.SetupIdentity();
        }

        [SecurityCritical]
        public virtual ObjRef CreateObjRef(Type requestedType)
        {
            if (this._identity == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_NoIdentityEntry"));
            }
            return new ObjRef((MarshalByRefObject) this.GetTransparentProxy(), requestedType);
        }

        [SecurityCritical]
        protected MarshalByRefObject DetachServer()
        {
            object transparentProxy = this.GetTransparentProxy();
            if (transparentProxy != null)
            {
                RemotingServices.ResetInterfaceCache(transparentProxy);
            }
            MarshalByRefObject obj3 = this._serverObject;
            this._serverObject = null;
            obj3.__ResetServerIdentity();
            return obj3;
        }

        [SecurityCritical]
        internal bool DoContextsMatch()
        {
            bool flag = false;
            if (this.GetStub() == _defaultStub)
            {
                object stubData = GetStubData(this);
                if (stubData is IntPtr)
                {
                    IntPtr ptr = (IntPtr) stubData;
                    if (ptr.Equals(Thread.CurrentContext.InternalContextID))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        [SecurityCritical]
        internal static IMessage EndInvokeHelper(Message reqMsg, bool bProxyCase)
        {
            AsyncResult asyncResult = reqMsg.GetAsyncResult() as AsyncResult;
            IMessage message = null;
            if (asyncResult == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadAsyncResult"));
            }
            if (asyncResult.AsyncDelegate != reqMsg.GetThisPtr())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MismatchedAsyncResult"));
            }
            if (!asyncResult.IsCompleted)
            {
                asyncResult.AsyncWaitHandle.WaitOne(0x7fffffff, Thread.CurrentContext.IsThreadPoolAware);
            }
            lock (asyncResult)
            {
                if (asyncResult.EndInvokeCalled)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EndInvokeCalledMultiple"));
                }
                asyncResult.EndInvokeCalled = true;
                IMethodReturnMessage replyMessage = (IMethodReturnMessage) asyncResult.GetReplyMessage();
                if (!bProxyCase)
                {
                    Exception exception = replyMessage.Exception;
                    if (exception != null)
                    {
                        throw exception.PrepForRemoting();
                    }
                    reqMsg.PropagateOutParameters(replyMessage.Args, replyMessage.ReturnValue);
                }
                else
                {
                    message = replyMessage;
                }
                CallContext.GetLogicalCallContext().Merge(replyMessage.LogicalCallContext);
            }
            return message;
        }

        [SecurityCritical]
        public virtual IntPtr GetCOMIUnknown(bool fIsMarshalled)
        {
            return MarshalByRefObject.GetComIUnknown((MarshalByRefObject) this.GetTransparentProxy());
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern IntPtr GetDefaultStub();
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            RemotingServices.GetObjectData(this.GetTransparentProxy(), info, context);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public extern Type GetProxiedType();
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr GetStub();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern object GetStubData(RealProxy rp);
        public virtual object GetTransparentProxy()
        {
            return this._tp;
        }

        [SecurityCritical]
        protected MarshalByRefObject GetUnwrappedServer()
        {
            return this.UnwrappedServerObject;
        }

        [SecurityCritical]
        private static void HandleReturnMessage(IMessage reqMsg, IMessage retMsg)
        {
            IMethodReturnMessage message = retMsg as IMethodReturnMessage;
            if ((retMsg == null) || (message == null))
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
            }
            Exception exception = message.Exception;
            if (exception != null)
            {
                throw exception.PrepForRemoting();
            }
            if (!(retMsg is StackBasedReturnMessage))
            {
                if (reqMsg is Message)
                {
                    PropagateOutParameters(reqMsg, message.Args, message.ReturnValue);
                }
                else if (reqMsg is ConstructorCallMessage)
                {
                    PropagateOutParameters(reqMsg, message.Args, null);
                }
            }
        }

        [ComVisible(true), SecurityCritical]
        public IConstructionReturnMessage InitializeServerObject(IConstructionCallMessage ctorMsg)
        {
            IConstructionReturnMessage message = null;
            if (this._serverObject != null)
            {
                return message;
            }
            Type proxiedType = this.GetProxiedType();
            if ((ctorMsg != null) && (ctorMsg.ActivationType != proxiedType))
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Proxy_BadTypeForActivation"), new object[] { proxiedType.FullName, ctorMsg.ActivationType }));
            }
            this._serverObject = RemotingServices.AllocateUninitializedObject(proxiedType);
            this.SetContextForDefaultStub();
            MarshalByRefObject transparentProxy = (MarshalByRefObject) this.GetTransparentProxy();
            IMethodReturnMessage message2 = null;
            Exception e = null;
            if (ctorMsg != null)
            {
                message2 = RemotingServices.ExecuteMessage(transparentProxy, ctorMsg);
                e = message2.Exception;
            }
            else
            {
                try
                {
                    RemotingServices.CallDefaultCtor(transparentProxy);
                }
                catch (Exception exception2)
                {
                    e = exception2;
                }
            }
            if (e == null)
            {
                object[] outArgs = (message2 == null) ? null : message2.OutArgs;
                int outArgsCount = (outArgs == null) ? 0 : outArgs.Length;
                LogicalCallContext callCtx = (message2 == null) ? null : message2.LogicalCallContext;
                message = new ConstructorReturnMessage(transparentProxy, outArgs, outArgsCount, callCtx, ctorMsg);
                this.SetupIdentity();
                if (this.IsRemotingProxy())
                {
                    ((RemotingProxy) this).Initialized = true;
                }
                return message;
            }
            return new ConstructorReturnMessage(e, ctorMsg);
        }

        public abstract IMessage Invoke(IMessage msg);
        internal bool IsRemotingProxy()
        {
            return ((this._flags & RealProxyFlags.RemotingProxy) == RealProxyFlags.RemotingProxy);
        }

        [SecurityCritical]
        private void PrivateInvoke(ref MessageData msgData, int type)
        {
            IMessage reqMsg = null;
            CallType type2 = (CallType) type;
            IMessage retMsg = null;
            int msgFlags = -1;
            RemotingProxy proxy = null;
            if (CallType.MethodCall == type2)
            {
                Message message3 = new Message();
                message3.InitFields(msgData);
                reqMsg = message3;
                msgFlags = message3.GetCallType();
            }
            else if (CallType.ConstructorCall == type2)
            {
                msgFlags = 0;
                proxy = this as RemotingProxy;
                ConstructorCallMessage ccm = null;
                bool flag = false;
                if (!this.IsRemotingProxy())
                {
                    ccm = new ConstructorCallMessage(null, null, null, (RuntimeType) this.GetProxiedType());
                }
                else
                {
                    ccm = proxy.ConstructorMessage;
                    Identity identityObject = proxy.IdentityObject;
                    if (identityObject != null)
                    {
                        flag = identityObject.IsWellKnown();
                    }
                }
                if ((ccm == null) || flag)
                {
                    ccm = new ConstructorCallMessage(null, null, null, (RuntimeType) this.GetProxiedType());
                    ccm.SetFrame(msgData);
                    reqMsg = ccm;
                    if (flag)
                    {
                        proxy.ConstructorMessage = null;
                        if (ccm.ArgCount != 0)
                        {
                            throw new RemotingException(Environment.GetResourceString("Remoting_Activation_WellKnownCTOR"));
                        }
                    }
                    retMsg = new ConstructorReturnMessage((MarshalByRefObject) this.GetTransparentProxy(), null, 0, null, ccm);
                }
                else
                {
                    ccm.SetFrame(msgData);
                    reqMsg = ccm;
                }
            }
            ChannelServices.IncrementRemoteCalls();
            if (!this.IsRemotingProxy() && ((msgFlags & 2) == 2))
            {
                Message message5 = reqMsg as Message;
                retMsg = EndInvokeHelper(message5, true);
            }
            if (retMsg == null)
            {
                LogicalCallContext cctx = null;
                Thread currentThread = Thread.CurrentThread;
                cctx = currentThread.GetLogicalCallContext();
                this.SetCallContextInMessage(reqMsg, msgFlags, cctx);
                cctx.PropagateOutgoingHeadersToMessage(reqMsg);
                retMsg = this.Invoke(reqMsg);
                this.ReturnCallContextToThread(currentThread, retMsg, msgFlags, cctx);
                CallContext.GetLogicalCallContext().PropagateIncomingHeadersToCallContext(retMsg);
            }
            if (!this.IsRemotingProxy() && ((msgFlags & 1) == 1))
            {
                Message m = reqMsg as Message;
                AsyncResult ret = new AsyncResult(m);
                ret.SyncProcessMessage(retMsg);
                retMsg = new ReturnMessage(ret, null, 0, null, m);
            }
            HandleReturnMessage(reqMsg, retMsg);
            if (CallType.ConstructorCall == type2)
            {
                MarshalByRefObject obj2 = null;
                IConstructionReturnMessage message7 = retMsg as IConstructionReturnMessage;
                if (message7 == null)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Proxy_BadReturnTypeForActivation"));
                }
                ConstructorReturnMessage message8 = message7 as ConstructorReturnMessage;
                if (message8 != null)
                {
                    obj2 = (MarshalByRefObject) message8.GetObject();
                    if (obj2 == null)
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_Activation_NullReturnValue"));
                    }
                }
                else
                {
                    obj2 = (MarshalByRefObject) RemotingServices.InternalUnmarshal((ObjRef) message7.ReturnValue, this.GetTransparentProxy(), true);
                    if (obj2 == null)
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_Activation_NullFromInternalUnmarshal"));
                    }
                }
                if (obj2 != ((MarshalByRefObject) this.GetTransparentProxy()))
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Activation_InconsistentState"));
                }
                if (this.IsRemotingProxy())
                {
                    proxy.ConstructorMessage = null;
                }
            }
        }

        [SecurityCritical]
        internal static void PropagateOutParameters(IMessage msg, object[] outArgs, object returnValue)
        {
            Message message = msg as Message;
            if (message == null)
            {
                ConstructorCallMessage message2 = msg as ConstructorCallMessage;
                if (message2 != null)
                {
                    message = message2.GetMessage();
                }
            }
            if (message == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Remoting_Proxy_ExpectedOriginalMessage"));
            }
            RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(message.GetMethodBase());
            if ((outArgs != null) && (outArgs.Length > 0))
            {
                object[] args = message.Args;
                ParameterInfo[] parameters = reflectionCachedData.Parameters;
                foreach (int num in reflectionCachedData.MarshalRequestArgMap)
                {
                    ParameterInfo info = parameters[num];
                    if ((info.IsIn && info.ParameterType.IsByRef) && !info.IsOut)
                    {
                        outArgs[num] = args[num];
                    }
                }
                if (reflectionCachedData.NonRefOutArgMap.Length > 0)
                {
                    foreach (int num2 in reflectionCachedData.NonRefOutArgMap)
                    {
                        Array destinationArray = args[num2] as Array;
                        if (destinationArray != null)
                        {
                            Array.Copy((Array) outArgs[num2], destinationArray, destinationArray.Length);
                        }
                    }
                }
                int[] outRefArgMap = reflectionCachedData.OutRefArgMap;
                if (outRefArgMap.Length > 0)
                {
                    foreach (int num3 in outRefArgMap)
                    {
                        ValidateReturnArg(outArgs[num3], parameters[num3].ParameterType);
                    }
                }
            }
            if ((message.GetCallType() & 15) != 1)
            {
                Type returnType = reflectionCachedData.ReturnType;
                if (returnType != null)
                {
                    ValidateReturnArg(returnValue, returnType);
                }
            }
            message.PropagateOutParameters(outArgs, returnValue);
        }

        [SecurityCritical]
        private void ReturnCallContextToThread(Thread currentThread, IMessage retMsg, int msgFlags, LogicalCallContext currCtx)
        {
            if ((msgFlags == 0) && (retMsg != null))
            {
                IMethodReturnMessage message = retMsg as IMethodReturnMessage;
                if (message != null)
                {
                    LogicalCallContext logicalCallContext = message.LogicalCallContext;
                    if (logicalCallContext == null)
                    {
                        currentThread.SetLogicalCallContext(currCtx);
                    }
                    else if (!(message is StackBasedReturnMessage))
                    {
                        LogicalCallContext context2 = currentThread.SetLogicalCallContext(logicalCallContext);
                        if (context2 != logicalCallContext)
                        {
                            IPrincipal principal = context2.Principal;
                            if (principal != null)
                            {
                                logicalCallContext.Principal = principal;
                            }
                        }
                    }
                }
            }
        }

        private void SetCallContextInMessage(IMessage reqMsg, int msgFlags, LogicalCallContext cctx)
        {
            Message message = reqMsg as Message;
            if (msgFlags == 0)
            {
                if (message != null)
                {
                    message.SetLogicalCallContext(cctx);
                }
                else
                {
                    ((ConstructorCallMessage) reqMsg).SetLogicalCallContext(cctx);
                }
            }
        }

        public virtual void SetCOMIUnknown(IntPtr i)
        {
        }

        [SecurityCritical]
        private void SetContextForDefaultStub()
        {
            if (this.GetStub() == _defaultStub)
            {
                object stubData = GetStubData(this);
                if (stubData is IntPtr)
                {
                    IntPtr ptr = (IntPtr) stubData;
                    if (ptr.Equals(_defaultStubValue))
                    {
                        SetStubData(this, Thread.CurrentContext.InternalContextID);
                    }
                }
            }
        }

        internal void SetSrvInfo(GCHandle srvIdentity, int domainID)
        {
            this._srvIdentity = srvIdentity;
            this._domainID = domainID;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern void SetStubData(RealProxy rp, object stubData);
        [SecurityCritical]
        private void SetupIdentity()
        {
            if (this._identity == null)
            {
                this._identity = IdentityHolder.FindOrCreateServerIdentity(this._serverObject, null, 0);
                ((Identity) this._identity).RaceSetTransparentProxy(this.GetTransparentProxy());
            }
        }

        public virtual IntPtr SupportsInterface(ref Guid iid)
        {
            return IntPtr.Zero;
        }

        private static void ValidateReturnArg(object arg, Type paramType)
        {
            if (paramType.IsByRef)
            {
                paramType = paramType.GetElementType();
            }
            if (paramType.IsValueType)
            {
                if (arg != null)
                {
                    if (!paramType.IsInstanceOfType(arg))
                    {
                        throw new InvalidCastException(Environment.GetResourceString("Remoting_Proxy_BadReturnType"));
                    }
                }
                else if (!paramType.IsGenericType || (paramType.GetGenericTypeDefinition() != typeof(Nullable<>)))
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Proxy_ReturnValueTypeCannotBeNull"));
                }
            }
            else if ((arg != null) && !paramType.IsInstanceOfType(arg))
            {
                throw new InvalidCastException(Environment.GetResourceString("Remoting_Proxy_BadReturnType"));
            }
        }

        [SecurityCritical]
        internal virtual void Wrap()
        {
            ServerIdentity identity = this._identity as ServerIdentity;
            if ((identity != null) && (this is RemotingProxy))
            {
                SetStubData(this, identity.ServerContext.InternalContextID);
            }
        }

        internal virtual Identity IdentityObject
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return (Identity) this._identity;
            }
            set
            {
                this._identity = value;
            }
        }

        internal bool Initialized
        {
            get
            {
                return ((this._flags & RealProxyFlags.Initialized) == RealProxyFlags.Initialized);
            }
            set
            {
                if (value)
                {
                    this._flags |= RealProxyFlags.Initialized;
                }
                else
                {
                    this._flags &= ~RealProxyFlags.Initialized;
                }
            }
        }

        internal MarshalByRefObject UnwrappedServerObject
        {
            get
            {
                return this._serverObject;
            }
        }
    }
}

