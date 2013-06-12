namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Security;
    using System.Threading;

    internal static class ActivationServices
    {
        [ThreadStatic]
        internal static ActivationAttributeStack _attributeStack;
        [SecurityCritical]
        private static ProxyAttribute _proxyAttribute = new ProxyAttribute();
        private static Hashtable _proxyTable = new Hashtable();
        internal const string ActivationServiceURI = "RemoteActivationService.rem";
        private static IActivator activator = null;
        internal const string ConnectKey = "Connect";
        internal const string PermissionKey = "Permission";
        private static Type proxyAttributeType = typeof(ProxyAttribute);
        internal const string RemoteActivateKey = "Remote";

        [SecurityCritical]
        internal static IConstructionReturnMessage Activate(RemotingProxy remProxy, IConstructionCallMessage ctorMsg)
        {
            IConstructionReturnMessage message = null;
            if (((ConstructorCallMessage) ctorMsg).ActivateInContext)
            {
                message = ctorMsg.Activator.Activate(ctorMsg);
                if (message.Exception != null)
                {
                    throw message.Exception;
                }
                return message;
            }
            GetPropertiesFromAttributes(ctorMsg, ctorMsg.CallSiteActivationAttributes);
            GetPropertiesFromAttributes(ctorMsg, ((ConstructorCallMessage) ctorMsg).GetWOMAttributes());
            GetPropertiesFromAttributes(ctorMsg, ((ConstructorCallMessage) ctorMsg).GetTypeAttributes());
            IMethodReturnMessage message2 = (IMethodReturnMessage) Thread.CurrentContext.GetClientContextChain().SyncProcessMessage(ctorMsg);
            message = message2 as IConstructionReturnMessage;
            if (message2 == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Activation_Failed"));
            }
            if (message2.Exception != null)
            {
                throw message2.Exception;
            }
            return message;
        }

        [SecurityCritical]
        internal static object ActivateWithMessage(Type serverType, IMessage msg, ServerIdentity srvIdToBind, out Exception e)
        {
            object obj2 = null;
            e = null;
            obj2 = RemotingServices.AllocateUninitializedObject(serverType);
            object proxy = null;
            if (serverType.IsContextful)
            {
                if (msg is ConstructorCallMessage)
                {
                    proxy = ((ConstructorCallMessage) msg).GetThisPtr();
                }
                else
                {
                    proxy = null;
                }
                proxy = RemotingServices.Wrap((ContextBoundObject) obj2, proxy, false);
            }
            else
            {
                if (Thread.CurrentContext != Context.DefaultContext)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Activation_Failed"));
                }
                proxy = obj2;
            }
            IMessageSink sink = new StackBuilderSink(proxy);
            IMethodReturnMessage message = (IMethodReturnMessage) sink.SyncProcessMessage(msg);
            if (message.Exception == null)
            {
                if (serverType.IsContextful)
                {
                    return RemotingServices.Wrap((ContextBoundObject) obj2);
                }
                return obj2;
            }
            e = message.Exception;
            return null;
        }

        [SecurityCritical]
        private static void CheckForInfrastructurePermission(RuntimeAssembly asm)
        {
            if (asm != RemotingServices.s_MscorlibAssembly)
            {
                CodeAccessSecurityEngine.CheckAssembly(asm, RemotingServices.s_RemotingInfrastructurePermission);
            }
        }

        [SecurityCritical]
        internal static object CheckIfConnected(RemotingProxy proxy, IConstructionCallMessage ctorMsg)
        {
            string str = (string) ctorMsg.Properties["Connect"];
            object transparentProxy = null;
            if (str != null)
            {
                transparentProxy = proxy.GetTransparentProxy();
            }
            return transparentProxy;
        }

        [SecurityCritical]
        internal static object ConnectIfNecessary(IConstructionCallMessage ctorMsg)
        {
            string url = (string) ctorMsg.Properties["Connect"];
            object obj2 = null;
            if (url != null)
            {
                obj2 = RemotingServices.Connect(ctorMsg.ActivationType, url);
            }
            return obj2;
        }

        [SecurityCritical]
        internal static MarshalByRefObject CreateInstance(RuntimeType serverType)
        {
            MarshalByRefObject transparentProxy = null;
            ConstructorCallMessage ctorCallMsg = null;
            RemotingProxy realProxy;
            bool flag = IsCurrentContextOK(serverType, null, ref ctorCallMsg);
            if (flag && !serverType.IsContextful)
            {
                return RemotingServices.AllocateUninitializedObject(serverType);
            }
            transparentProxy = (MarshalByRefObject) ConnectIfNecessary(ctorCallMsg);
            if (transparentProxy == null)
            {
                realProxy = new RemotingProxy(serverType);
                transparentProxy = (MarshalByRefObject) realProxy.GetTransparentProxy();
            }
            else
            {
                realProxy = (RemotingProxy) RemotingServices.GetRealProxy(transparentProxy);
            }
            realProxy.ConstructorMessage = ctorCallMsg;
            if (!flag)
            {
                ContextLevelActivator activator = new ContextLevelActivator {
                    NextActivator = ctorCallMsg.Activator
                };
                ctorCallMsg.Activator = activator;
                return transparentProxy;
            }
            ctorCallMsg.ActivateInContext = true;
            return transparentProxy;
        }

        [SecurityCritical]
        private static MarshalByRefObject CreateObjectForCom(RuntimeType serverType, object[] props, bool bNewObj)
        {
            if (PeekActivationAttributes(serverType) != null)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_ActivForCom"));
            }
            InitActivationServices();
            ProxyAttribute proxyAttribute = GetProxyAttribute(serverType);
            if (proxyAttribute is ICustomFactory)
            {
                return ((ICustomFactory) proxyAttribute).CreateInstance(serverType);
            }
            return (MarshalByRefObject) Activator.CreateInstance(serverType, true);
        }

        [SecurityCritical]
        internal static IConstructionReturnMessage DoCrossContextActivation(IConstructionCallMessage reqMsg)
        {
            bool isContextful = reqMsg.ActivationType.IsContextful;
            Context newCtx = null;
            if (isContextful)
            {
                newCtx = new Context();
                ArrayList contextProperties = (ArrayList) reqMsg.ContextProperties;
                RuntimeAssembly asm = null;
                for (int i = 0; i < contextProperties.Count; i++)
                {
                    IContextProperty prop = contextProperties[i] as IContextProperty;
                    if (prop == null)
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_Activation_BadAttribute"));
                    }
                    asm = (RuntimeAssembly) prop.GetType().Assembly;
                    CheckForInfrastructurePermission(asm);
                    if (newCtx.GetProperty(prop.Name) == null)
                    {
                        newCtx.SetProperty(prop);
                    }
                }
                newCtx.Freeze();
                for (int j = 0; j < contextProperties.Count; j++)
                {
                    if (!((IContextProperty) contextProperties[j]).IsNewContextOK(newCtx))
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_Activation_PropertyUnhappy"));
                    }
                }
            }
            InternalCrossContextDelegate ftnToCall = new InternalCrossContextDelegate(ActivationServices.DoCrossContextActivationCallback);
            object[] args = new object[] { reqMsg };
            if (isContextful)
            {
                return (Thread.CurrentThread.InternalCrossContextCallback(newCtx, ftnToCall, args) as IConstructionReturnMessage);
            }
            return (ftnToCall(args) as IConstructionReturnMessage);
        }

        [SecurityCritical]
        internal static object DoCrossContextActivationCallback(object[] args)
        {
            IConstructionCallMessage msg = (IConstructionCallMessage) args[0];
            IConstructionReturnMessage message2 = null;
            IMethodReturnMessage message3 = (IMethodReturnMessage) Thread.CurrentContext.GetServerContextChain().SyncProcessMessage(msg);
            Exception e = null;
            message2 = message3 as IConstructionReturnMessage;
            if (message2 == null)
            {
                if (message3 != null)
                {
                    e = message3.Exception;
                }
                else
                {
                    e = new RemotingException(Environment.GetResourceString("Remoting_Activation_Failed"));
                }
                message2 = new ConstructorReturnMessage(e, null);
                ((ConstructorReturnMessage) message2).SetLogicalCallContext((LogicalCallContext) msg.Properties[Message.CallContextKey]);
            }
            return message2;
        }

        [SecurityCritical]
        internal static IConstructionReturnMessage DoServerContextActivation(IConstructionCallMessage reqMsg)
        {
            Exception e = null;
            return SetupConstructionReply(ActivateWithMessage(reqMsg.ActivationType, reqMsg, null, out e), reqMsg, e);
        }

        [SecurityCritical]
        internal static IActivator GetActivator()
        {
            DomainSpecificRemotingData remotingData = Thread.GetDomain().RemotingData;
            if (remotingData.LocalActivator == null)
            {
                Startup();
            }
            return remotingData.LocalActivator;
        }

        [SecurityCritical]
        internal static IContextAttribute[] GetContextAttributesForType(Type serverType)
        {
            if (!typeof(ContextBoundObject).IsAssignableFrom(serverType) || serverType.IsCOMObject)
            {
                return new ContextAttribute[0];
            }
            Type type = serverType;
            int length = 8;
            IContextAttribute[] sourceArray = new IContextAttribute[length];
            int num2 = 0;
            foreach (IContextAttribute attribute in type.GetCustomAttributes(typeof(IContextAttribute), true))
            {
                Type type2 = attribute.GetType();
                bool flag = false;
                for (int i = 0; i < num2; i++)
                {
                    if (type2.Equals(sourceArray[i].GetType()))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    num2++;
                    if (num2 > (length - 1))
                    {
                        IContextAttribute[] attributeArray2 = new IContextAttribute[2 * length];
                        Array.Copy(sourceArray, 0, attributeArray2, 0, length);
                        sourceArray = attributeArray2;
                        length *= 2;
                    }
                    sourceArray[num2 - 1] = attribute;
                }
            }
            IContextAttribute[] destinationArray = new IContextAttribute[num2];
            Array.Copy(sourceArray, destinationArray, num2);
            return destinationArray;
        }

        [SecurityCritical]
        internal static ContextAttribute GetGlobalAttribute()
        {
            DomainSpecificRemotingData remotingData = Thread.GetDomain().RemotingData;
            if (remotingData.LocalActivator == null)
            {
                Startup();
            }
            return remotingData.LocalActivator;
        }

        [SecurityCritical]
        internal static void GetPropertiesFromAttributes(IConstructionCallMessage ctorMsg, object[] attributes)
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    IContextAttribute attribute = attributes[i] as IContextAttribute;
                    if (attribute == null)
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_Activation_BadAttribute"));
                    }
                    RuntimeAssembly asm = (RuntimeAssembly) attribute.GetType().Assembly;
                    CheckForInfrastructurePermission(asm);
                    attribute.GetPropertiesForNewContext(ctorMsg);
                }
            }
        }

        [SecurityCritical]
        internal static ProxyAttribute GetProxyAttribute(Type serverType)
        {
            if (!serverType.HasProxyAttribute)
            {
                return DefaultProxyAttribute;
            }
            ProxyAttribute attribute = _proxyTable[serverType] as ProxyAttribute;
            if (attribute == null)
            {
                object[] objArray = Attribute.GetCustomAttributes(serverType, proxyAttributeType, true);
                if ((objArray != null) && (objArray.Length != 0))
                {
                    if (!serverType.IsContextful)
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_Activation_MBR_ProxyAttribute"));
                    }
                    attribute = objArray[0] as ProxyAttribute;
                }
                if (_proxyTable.Contains(serverType))
                {
                    return attribute;
                }
                lock (_proxyTable)
                {
                    if (!_proxyTable.Contains(serverType))
                    {
                        _proxyTable.Add(serverType, attribute);
                    }
                }
            }
            return attribute;
        }

        [SecurityCritical]
        private static void InitActivationServices()
        {
            if (activator == null)
            {
                activator = GetActivator();
                if (activator == null)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadInternalState_ActivationFailure"), new object[0]));
                }
            }
        }

        [SecurityCritical]
        internal static void Initialize()
        {
            GetActivator();
        }

        [SecurityCritical]
        private static bool IsCurrentContextOK(RuntimeType serverType, object[] props, ref ConstructorCallMessage ctorCallMsg)
        {
            object[] callSiteActivationAttributes = PeekActivationAttributes(serverType);
            if (callSiteActivationAttributes != null)
            {
                PopActivationAttributes(serverType);
            }
            object[] womAttr = new object[] { GetGlobalAttribute() };
            object[] contextAttributesForType = GetContextAttributesForType(serverType);
            Context currentContext = Thread.CurrentContext;
            ctorCallMsg = new ConstructorCallMessage(callSiteActivationAttributes, womAttr, contextAttributesForType, serverType);
            ctorCallMsg.Activator = new ConstructionLevelActivator();
            bool flag = QueryAttributesIfContextOK(currentContext, ctorCallMsg, womAttr);
            if (flag)
            {
                flag = QueryAttributesIfContextOK(currentContext, ctorCallMsg, callSiteActivationAttributes);
                if (flag)
                {
                    flag = QueryAttributesIfContextOK(currentContext, ctorCallMsg, contextAttributesForType);
                }
            }
            return flag;
        }

        [SecurityCritical]
        private static MarshalByRefObject IsCurrentContextOK(RuntimeType serverType, object[] props, bool bNewObj)
        {
            MarshalByRefObject proxy = null;
            InitActivationServices();
            ProxyAttribute proxyAttribute = GetProxyAttribute(serverType);
            if (object.ReferenceEquals(proxyAttribute, DefaultProxyAttribute))
            {
                return proxyAttribute.CreateInstanceInternal(serverType);
            }
            proxy = proxyAttribute.CreateInstance(serverType);
            if (((proxy != null) && !RemotingServices.IsTransparentProxy(proxy)) && !serverType.IsAssignableFrom(proxy.GetType()))
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Activation_BadObject"), new object[] { serverType }));
            }
            return proxy;
        }

        internal static object[] PeekActivationAttributes(Type serverType)
        {
            if (_attributeStack == null)
            {
                return null;
            }
            return _attributeStack.Peek(serverType);
        }

        internal static void PopActivationAttributes(Type serverType)
        {
            _attributeStack.Pop(serverType);
        }

        internal static void PushActivationAttributes(Type serverType, object[] attributes)
        {
            if (_attributeStack == null)
            {
                _attributeStack = new ActivationAttributeStack();
            }
            _attributeStack.Push(serverType, attributes);
        }

        [SecurityCritical]
        private static bool QueryAttributesIfContextOK(Context ctx, IConstructionCallMessage ctorMsg, object[] attributes)
        {
            bool flag = true;
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    IContextAttribute attribute = attributes[i] as IContextAttribute;
                    if (attribute == null)
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_Activation_BadAttribute"));
                    }
                    RuntimeAssembly asm = (RuntimeAssembly) attribute.GetType().Assembly;
                    CheckForInfrastructurePermission(asm);
                    flag = attribute.IsContextOK(ctx, ctorMsg);
                    if (!flag)
                    {
                        return flag;
                    }
                }
            }
            return flag;
        }

        [SecurityCritical]
        internal static IConstructionReturnMessage SetupConstructionReply(object serverObj, IConstructionCallMessage ctorMsg, Exception e)
        {
            IConstructionReturnMessage message = null;
            if (e == null)
            {
                return new ConstructorReturnMessage((MarshalByRefObject) serverObj, null, 0, (LogicalCallContext) ctorMsg.Properties[Message.CallContextKey], ctorMsg);
            }
            message = new ConstructorReturnMessage(e, null);
            ((ConstructorReturnMessage) message).SetLogicalCallContext((LogicalCallContext) ctorMsg.Properties[Message.CallContextKey]);
            return message;
        }

        [SecurityCritical]
        internal static void StartListeningForRemoteRequests()
        {
            Startup();
            DomainSpecificRemotingData remotingData = Thread.GetDomain().RemotingData;
            if (!remotingData.ActivatorListening)
            {
                object configLock = remotingData.ConfigLock;
                bool lockTaken = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(configLock, ref lockTaken);
                    if (!remotingData.ActivatorListening)
                    {
                        RemotingServices.MarshalInternal(Thread.GetDomain().RemotingData.ActivationListener, "RemoteActivationService.rem", typeof(IActivator));
                        ((ServerIdentity) IdentityHolder.ResolveIdentity("RemoteActivationService.rem")).SetSingletonObjectMode();
                        remotingData.ActivatorListening = true;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(configLock);
                    }
                }
            }
        }

        [SecurityCritical]
        private static void Startup()
        {
            DomainSpecificRemotingData remotingData = Thread.GetDomain().RemotingData;
            if (!remotingData.ActivationInitialized || remotingData.InitializingActivation)
            {
                object configLock = remotingData.ConfigLock;
                bool lockTaken = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(configLock, ref lockTaken);
                    remotingData.InitializingActivation = true;
                    if (!remotingData.ActivationInitialized)
                    {
                        remotingData.LocalActivator = new LocalActivator();
                        remotingData.ActivationListener = new ActivationListener();
                        remotingData.ActivationInitialized = true;
                    }
                    remotingData.InitializingActivation = false;
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(configLock);
                    }
                }
            }
        }

        internal static ProxyAttribute DefaultProxyAttribute
        {
            [SecurityCritical]
            get
            {
                return _proxyAttribute;
            }
        }
    }
}

