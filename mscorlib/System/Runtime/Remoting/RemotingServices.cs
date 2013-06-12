namespace System.Runtime.Remoting
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Lifetime;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Remoting.Services;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [ComVisible(true)]
    public static class RemotingServices
    {
        private const string CanCastToXmlTypeName = "CanCastToXmlType";
        private const string FieldGetterName = "FieldGetter";
        private const string FieldSetterName = "FieldSetter";
        private const string InvokeMemberName = "InvokeMember";
        private const string IsInstanceOfTypeName = "IsInstanceOfType";
        private const BindingFlags LookupAll = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        private static bool s_bInProcessOfRegisteringWellKnownChannels;
        private static bool s_bRegisteredWellKnownChannels;
        private static bool s_bRemoteActivationConfigured;
        private static MethodBase s_CanCastToXmlTypeMB;
        private static object s_delayLoadChannelLock;
        private static MethodBase s_FieldGetterMB;
        private static MethodBase s_FieldSetterMB;
        private static MethodBase s_InvokeMemberMB;
        private static MethodBase s_IsInstanceOfTypeMB;
        internal static Assembly s_MscorlibAssembly;
        internal static SecurityPermission s_RemotingInfrastructurePermission;

        [SecuritySafeCritical]
        static RemotingServices()
        {
            CodeAccessPermission.Assert(true);
            s_RemotingInfrastructurePermission = new SecurityPermission(SecurityPermissionFlag.Infrastructure);
            s_MscorlibAssembly = typeof(RemotingServices).Assembly;
            s_FieldGetterMB = null;
            s_FieldSetterMB = null;
            s_bRemoteActivationConfigured = false;
            s_bRegisteredWellKnownChannels = false;
            s_bInProcessOfRegisteringWellKnownChannels = false;
            s_delayLoadChannelLock = new object();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern MarshalByRefObject AllocateInitializedObject(RuntimeType objectType);
        [SecurityCritical]
        internal static MarshalByRefObject AllocateInitializedObject(Type objectType)
        {
            RuntimeType type = objectType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongType"), new object[] { "objectType" }));
            }
            return AllocateInitializedObject(type);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern MarshalByRefObject AllocateUninitializedObject(RuntimeType objectType);
        [SecurityCritical]
        internal static MarshalByRefObject AllocateUninitializedObject(Type objectType)
        {
            RuntimeType type = objectType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongType"), new object[] { "objectType" }));
            }
            return AllocateUninitializedObject(type);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object AlwaysUnwrap(ContextBoundObject obj);
        private static bool AreChannelDataElementsNull(object[] channelData)
        {
            object[] objArray = channelData;
            for (int i = 0; i < objArray.Length; i++)
            {
                if (objArray[i] != null)
                {
                    return false;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void CallDefaultCtor(object o);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object CheckCast(object objToExpand, RuntimeType type);
        [SecurityCritical]
        private static bool CheckCast(RealProxy rp, RuntimeType castType)
        {
            bool flag = false;
            if (castType == typeof(object))
            {
                return true;
            }
            if (!castType.IsInterface && !castType.IsMarshalByRef)
            {
                return false;
            }
            if (castType != typeof(IObjectReference))
            {
                IRemotingTypeInfo typeInfo = rp as IRemotingTypeInfo;
                if (typeInfo != null)
                {
                    return typeInfo.CanCastTo(castType, rp.GetTransparentProxy());
                }
                Identity identityObject = rp.IdentityObject;
                if (identityObject != null)
                {
                    ObjRef objectRef = identityObject.ObjectRef;
                    if (objectRef != null)
                    {
                        typeInfo = objectRef.TypeInfo;
                        if (typeInfo != null)
                        {
                            flag = typeInfo.CanCastTo(castType, rp.GetTransparentProxy());
                        }
                    }
                }
            }
            return flag;
        }

        private static bool CompareParameterList(ArrayList params1, ParameterInfo[] params2)
        {
            if (params1.Count != params2.Length)
            {
                return false;
            }
            int index = 0;
            foreach (object obj2 in params1)
            {
                ParameterInfo info = params2[index];
                ParameterInfo info2 = obj2 as ParameterInfo;
                if (info2 != null)
                {
                    if (((info2.ParameterType != info.ParameterType) || (info2.IsIn != info.IsIn)) || (info2.IsOut != info.IsOut))
                    {
                        return false;
                    }
                }
                else if ((((Type) obj2) != info.ParameterType) && info.IsIn)
                {
                    return false;
                }
                index++;
            }
            return true;
        }

        [SecurityCritical, ComVisible(true)]
        public static object Connect(Type classToProxy, string url)
        {
            return Unmarshal(classToProxy, url, null);
        }

        [ComVisible(true), SecurityCritical]
        public static object Connect(Type classToProxy, string url, object data)
        {
            return Unmarshal(classToProxy, url, data);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void CORProfilerRemotingClientReceivingReply(Guid id, bool fIsAsync);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void CORProfilerRemotingClientSendingMessage(out Guid id, bool fIsAsync);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void CORProfilerRemotingServerReceivingMessage(Guid id, bool fIsAsync);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void CORProfilerRemotingServerSendingReply(out Guid id, bool fIsAsync);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool CORProfilerTrackRemoting();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool CORProfilerTrackRemotingAsync();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool CORProfilerTrackRemotingCookie();
        [SecurityCritical]
        private static string CreateChannelSink(string url, object data, out IMessageSink chnlSink)
        {
            string objectURI = null;
            chnlSink = ChannelServices.CreateMessageSink(url, data, out objectURI);
            if (chnlSink == null)
            {
                lock (s_delayLoadChannelLock)
                {
                    chnlSink = ChannelServices.CreateMessageSink(url, data, out objectURI);
                    if (chnlSink == null)
                    {
                        chnlSink = RemotingConfigHandler.FindDelayLoadChannelForCreateMessageSink(url, data, out objectURI);
                    }
                }
            }
            return objectURI;
        }

        [SecurityCritical]
        internal static ObjRef CreateDataForDomain(int appDomainId, IntPtr defCtxID)
        {
            RegisterWellKnownChannels();
            InternalCrossContextDelegate ftnToCall = new InternalCrossContextDelegate(RemotingServices.CreateDataForDomainCallback);
            return (ObjRef) Thread.CurrentThread.InternalCrossContextCallback(null, defCtxID, appDomainId, ftnToCall, null);
        }

        [SecurityCritical]
        internal static object CreateDataForDomainCallback(object[] args)
        {
            RegisterWellKnownChannels();
            ObjRef ref2 = MarshalInternal(Thread.CurrentContext.AppDomain, null, null, false);
            ServerIdentity identity = (ServerIdentity) MarshalByRefObject.GetIdentity(Thread.CurrentContext.AppDomain);
            identity.SetHandle();
            ref2.SetServerIdentity(identity.GetHandle());
            ref2.SetDomainID(AppDomain.CurrentDomain.GetId());
            return ref2;
        }

        [SecurityCritical]
        internal static GCHandle CreateDelegateInvocation(WaitCallback waitDelegate, object state)
        {
            return GCHandle.Alloc(new object[] { waitDelegate, state });
        }

        [SecurityCritical]
        internal static void CreateEnvoyAndChannelSinks(MarshalByRefObject tpOrObject, ObjRef objectRef, out IMessageSink chnlSink, out IMessageSink envoySink)
        {
            chnlSink = null;
            envoySink = null;
            if (objectRef == null)
            {
                chnlSink = ChannelServices.GetCrossContextChannelSink();
                envoySink = Thread.CurrentContext.CreateEnvoyChain(tpOrObject);
                return;
            }
            object[] channelData = objectRef.ChannelInfo.ChannelData;
            if ((channelData != null) && !AreChannelDataElementsNull(channelData))
            {
                for (int i = 0; i < channelData.Length; i++)
                {
                    chnlSink = ChannelServices.CreateMessageSink(channelData[i]);
                    if (chnlSink != null)
                    {
                        break;
                    }
                }
                if (chnlSink == null)
                {
                    lock (s_delayLoadChannelLock)
                    {
                        for (int j = 0; j < channelData.Length; j++)
                        {
                            chnlSink = ChannelServices.CreateMessageSink(channelData[j]);
                            if (chnlSink != null)
                            {
                                break;
                            }
                        }
                        if (chnlSink == null)
                        {
                            foreach (object obj2 in channelData)
                            {
                                string str;
                                chnlSink = RemotingConfigHandler.FindDelayLoadChannelForCreateMessageSink(null, obj2, out str);
                                if (chnlSink != null)
                                {
                                    goto Label_00C6;
                                }
                            }
                        }
                    }
                }
            }
        Label_00C6:
            if ((objectRef.EnvoyInfo != null) && (objectRef.EnvoyInfo.EnvoySinks != null))
            {
                envoySink = objectRef.EnvoyInfo.EnvoySinks;
            }
            else
            {
                envoySink = EnvoyTerminatorSink.MessageSink;
            }
        }

        [SecurityCritical]
        internal static string CreateEnvoyAndChannelSinks(string url, object data, out IMessageSink chnlSink, out IMessageSink envoySink)
        {
            string str = null;
            str = CreateChannelSink(url, data, out chnlSink);
            envoySink = EnvoyTerminatorSink.MessageSink;
            return str;
        }

        [SecurityCritical]
        internal static object CreateProxyForDomain(int appDomainId, IntPtr defCtxID)
        {
            return (AppDomain) Unmarshal(CreateDataForDomain(appDomainId, defCtxID));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object CreateTransparentProxy(RealProxy rp, RuntimeType typeToProxy, IntPtr stub, object stubData);
        [SecurityCritical]
        internal static object CreateTransparentProxy(RealProxy rp, Type typeToProxy, IntPtr stub, object stubData)
        {
            RuntimeType type = typeToProxy as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongType"), new object[] { "typeToProxy" }));
            }
            return CreateTransparentProxy(rp, type, stub, stubData);
        }

        [SecurityCritical]
        internal static string DetermineDefaultQualifiedTypeName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            string xmlType = null;
            string xmlTypeNamespace = null;
            if (SoapServices.GetXmlTypeForInteropType(type, out xmlType, out xmlTypeNamespace))
            {
                return ("soap:" + xmlType + ", " + xmlTypeNamespace);
            }
            return type.AssemblyQualifiedName;
        }

        [SecurityCritical]
        public static bool Disconnect(MarshalByRefObject obj)
        {
            return Disconnect(obj, true);
        }

        [SecurityCritical]
        internal static bool Disconnect(MarshalByRefObject obj, bool bResetURI)
        {
            bool flag;
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            Identity identity = MarshalByRefObject.GetIdentity(obj, out flag);
            bool flag2 = false;
            if (identity != null)
            {
                if (!(identity is ServerIdentity))
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_CantDisconnectClientProxy"));
                }
                if (identity.IsInIDTable())
                {
                    IdentityHolder.RemoveIdentity(identity.URI, bResetURI);
                    flag2 = true;
                }
                TrackingServices.DisconnectedObject(obj);
            }
            return flag2;
        }

        [SecurityCritical]
        internal static void DisposeDelegateInvocation(GCHandle delegateCallToken)
        {
            delegateCallToken.Free();
        }

        [SecurityCritical]
        internal static void DomainUnloaded(int domainID)
        {
            IdentityHolder.FlushIdentityTable();
            CrossAppDomainSink.DomainUnloaded(domainID);
        }

        [SecurityCritical]
        public static IMethodReturnMessage ExecuteMessage(MarshalByRefObject target, IMethodCallMessage reqMsg)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            RealProxy realProxy = GetRealProxy(target);
            if ((realProxy is RemotingProxy) && !realProxy.DoContextsMatch())
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Proxy_WrongContext"));
            }
            StackBuilderSink sink = new StackBuilderSink(target);
            return (IMethodReturnMessage) sink.SyncProcessMessage(reqMsg, 0, true);
        }

        internal static bool FindAsyncMethodVersion(MethodInfo method, out MethodInfo beginMethod, out MethodInfo endMethod)
        {
            beginMethod = null;
            endMethod = null;
            string str = "Begin" + method.Name;
            string str2 = "End" + method.Name;
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            Type type = typeof(IAsyncResult);
            Type returnType = method.ReturnType;
            foreach (ParameterInfo info in method.GetParameters())
            {
                if (info.IsOut)
                {
                    list2.Add(info);
                }
                else if (info.ParameterType.IsByRef)
                {
                    list.Add(info);
                    list2.Add(info);
                }
                else
                {
                    list.Add(info);
                }
            }
            list.Add(typeof(AsyncCallback));
            list.Add(typeof(object));
            list2.Add(typeof(IAsyncResult));
            foreach (MethodInfo info2 in method.DeclaringType.GetMethods())
            {
                ParameterInfo[] parameters = info2.GetParameters();
                if ((info2.Name.Equals(str) && (info2.ReturnType == type)) && CompareParameterList(list, parameters))
                {
                    beginMethod = info2;
                }
                else if ((info2.Name.Equals(str2) && (info2.ReturnType == returnType)) && CompareParameterList(list2, parameters))
                {
                    endMethod = info2;
                }
            }
            return ((beginMethod != null) && (endMethod != null));
        }

        [SecurityCritical]
        internal static string GetDefaultQualifiedTypeName(RuntimeType type)
        {
            return InternalRemotingServices.GetReflectionCachedData(type).QualifiedTypeName;
        }

        [SecurityCritical]
        public static IMessageSink GetEnvoyChainForProxy(MarshalByRefObject obj)
        {
            IMessageSink envoyChain = null;
            if (IsObjectOutOfContext(obj))
            {
                Identity identityObject = GetRealProxy(obj).IdentityObject;
                if (identityObject != null)
                {
                    envoyChain = identityObject.EnvoyChain;
                }
            }
            return envoyChain;
        }

        [SecuritySafeCritical]
        public static object GetLifetimeService(MarshalByRefObject obj)
        {
            if (obj != null)
            {
                return obj.GetLifetimeService();
            }
            return null;
        }

        [SecurityCritical]
        private static MethodBase GetMethodBase(IMethodMessage msg, Type t, Type[] signature)
        {
            MethodBase base2 = null;
            if ((msg is IConstructionCallMessage) || (msg is IConstructionReturnMessage))
            {
                if (signature == null)
                {
                    ConstructorInfo[] constructors;
                    RuntimeType type = t as RuntimeType;
                    if (type == null)
                    {
                        constructors = t.GetConstructors();
                    }
                    else
                    {
                        constructors = type.GetConstructors();
                    }
                    if (1 != constructors.Length)
                    {
                        throw new AmbiguousMatchException(Environment.GetResourceString("Remoting_AmbiguousCTOR"));
                    }
                    return constructors[0];
                }
                RuntimeType type2 = t as RuntimeType;
                if (type2 == null)
                {
                    return t.GetConstructor(signature);
                }
                return type2.GetConstructor(signature);
            }
            if (!(msg is IMethodCallMessage) && !(msg is IMethodReturnMessage))
            {
                return base2;
            }
            if (signature == null)
            {
                RuntimeType type3 = t as RuntimeType;
                if (type3 == null)
                {
                    return t.GetMethod(msg.MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return type3.GetMethod(msg.MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            }
            RuntimeType type4 = t as RuntimeType;
            if (type4 == null)
            {
                return t.GetMethod(msg.MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, signature, null);
            }
            return type4.GetMethod(msg.MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, CallingConventions.Any, signature, null);
        }

        [SecurityCritical]
        public static MethodBase GetMethodBaseFromMethodMessage(IMethodMessage msg)
        {
            return InternalGetMethodBaseFromMethodMessage(msg);
        }

        [SecurityCritical]
        public static void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            MarshalInternal((MarshalByRefObject) obj, null, null).GetObjectData(info, context);
        }

        [SecurityCritical]
        public static string GetObjectUri(MarshalByRefObject obj)
        {
            bool flag;
            Identity identity = MarshalByRefObject.GetIdentity(obj, out flag);
            if (identity != null)
            {
                return identity.URI;
            }
            return null;
        }

        internal static string GetObjectUriFromFullUri(string fullUri)
        {
            if (fullUri == null)
            {
                return null;
            }
            int num = fullUri.LastIndexOf('/');
            if (num == -1)
            {
                return fullUri;
            }
            return fullUri.Substring(num + 1);
        }

        [SecurityCritical]
        public static ObjRef GetObjRefForProxy(MarshalByRefObject obj)
        {
            ObjRef objectRef = null;
            if (!IsTransparentProxy(obj))
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Proxy_BadType"));
            }
            Identity identityObject = GetRealProxy(obj).IdentityObject;
            if (identityObject != null)
            {
                objectRef = identityObject.ObjectRef;
            }
            return objectRef;
        }

        [SecurityCritical]
        private static Identity GetOrCreateIdentity(MarshalByRefObject Obj, string ObjURI)
        {
            Identity identityObject = null;
            if (IsTransparentProxy(Obj))
            {
                identityObject = GetRealProxy(Obj).IdentityObject;
                if (identityObject == null)
                {
                    identityObject = IdentityHolder.FindOrCreateServerIdentity(Obj, ObjURI, 2);
                    identityObject.RaceSetTransparentProxy(Obj);
                }
                ServerIdentity identity2 = identityObject as ServerIdentity;
                if (identity2 != null)
                {
                    identityObject = IdentityHolder.FindOrCreateServerIdentity(identity2.TPOrObject, ObjURI, 2);
                    if ((ObjURI != null) && (ObjURI != Identity.RemoveAppNameOrAppGuidIfNecessary(identityObject.ObjURI)))
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_URIExists"));
                    }
                    return identityObject;
                }
                if ((ObjURI != null) && (ObjURI != identityObject.ObjURI))
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_URIToProxy"));
                }
                return identityObject;
            }
            return IdentityHolder.FindOrCreateServerIdentity(Obj, ObjURI, 2);
        }

        [SecurityCritical]
        private static object GetOrCreateProxy(Type classToProxy, Identity idObj)
        {
            object tPOrObject = idObj.TPOrObject;
            if (tPOrObject == null)
            {
                tPOrObject = SetOrCreateProxy(idObj, classToProxy, null);
            }
            ServerIdentity identity = idObj as ServerIdentity;
            if (identity != null)
            {
                Type serverType = identity.ServerType;
                if (!classToProxy.IsAssignableFrom(serverType))
                {
                    throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidCast_FromTo"), new object[] { serverType.FullName, classToProxy.FullName }));
                }
            }
            return tPOrObject;
        }

        [SecurityCritical]
        private static object GetOrCreateProxy(Identity idObj, object proxy, bool fRefine)
        {
            if (proxy == null)
            {
                Type serverType;
                ServerIdentity identity = idObj as ServerIdentity;
                if (identity != null)
                {
                    serverType = identity.ServerType;
                }
                else
                {
                    IRemotingTypeInfo typeInfo = idObj.ObjectRef.TypeInfo;
                    serverType = null;
                    if (((typeInfo is TypeInfo) && !fRefine) || (typeInfo == null))
                    {
                        serverType = typeof(MarshalByRefObject);
                    }
                    else
                    {
                        string typeName = typeInfo.TypeName;
                        if (typeName != null)
                        {
                            string str2 = null;
                            string assemName = null;
                            TypeInfo.ParseTypeAndAssembly(typeName, out str2, out assemName);
                            Assembly assembly = FormatterServices.LoadAssemblyFromStringNoThrow(assemName);
                            if (assembly != null)
                            {
                                serverType = assembly.GetType(str2, false, false);
                            }
                        }
                    }
                    if (null == serverType)
                    {
                        throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadType"), new object[] { typeInfo.TypeName }));
                    }
                }
                proxy = SetOrCreateProxy(idObj, serverType, null);
            }
            else
            {
                proxy = SetOrCreateProxy(idObj, null, proxy);
            }
            if (proxy == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_UnexpectedNullTP"));
            }
            return proxy;
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        public static extern RealProxy GetRealProxy(object proxy);
        [SecurityCritical]
        internal static Context GetServerContext(MarshalByRefObject obj)
        {
            Context serverContext = null;
            if (!IsTransparentProxy(obj) && (obj is ContextBoundObject))
            {
                return Thread.CurrentContext;
            }
            ServerIdentity identityObject = GetRealProxy(obj).IdentityObject as ServerIdentity;
            if (identityObject != null)
            {
                serverContext = identityObject.ServerContext;
            }
            return serverContext;
        }

        [SecurityCritical]
        internal static void GetServerContextAndDomainIdForProxy(object tp, out IntPtr contextId, out int domainId)
        {
            ObjRef ref2;
            bool flag;
            contextId = GetServerContextForProxy(tp, out ref2, out flag, out domainId);
        }

        [SecurityCritical]
        internal static IntPtr GetServerContextForProxy(object tp)
        {
            ObjRef objRef = null;
            bool flag;
            int num;
            return GetServerContextForProxy(tp, out objRef, out flag, out num);
        }

        [SecurityCritical]
        private static IntPtr GetServerContextForProxy(object tp, out ObjRef objRef, out bool bSameDomain, out int domainId)
        {
            IntPtr zero = IntPtr.Zero;
            objRef = null;
            bSameDomain = false;
            domainId = 0;
            if (!IsTransparentProxy(tp))
            {
                return zero;
            }
            Identity identityObject = GetRealProxy(tp).IdentityObject;
            if (identityObject != null)
            {
                ServerIdentity identity2 = identityObject as ServerIdentity;
                if (identity2 != null)
                {
                    bSameDomain = true;
                    zero = identity2.ServerContext.InternalContextID;
                    domainId = Thread.GetDomain().GetId();
                    return zero;
                }
                objRef = identityObject.ObjectRef;
                if (objRef != null)
                {
                    return objRef.GetServerContext(out domainId);
                }
                return IntPtr.Zero;
            }
            return Context.DefaultContext.InternalContextID;
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static int GetServerDomainIdForProxy(object tp)
        {
            return GetRealProxy(tp).IdentityObject.ObjectRef.GetServerDomainId();
        }

        [SecurityCritical]
        public static Type GetServerTypeForUri(string URI)
        {
            Type type = null;
            if (URI == null)
            {
                return type;
            }
            ServerIdentity identity = (ServerIdentity) IdentityHolder.ResolveIdentity(URI);
            if (identity == null)
            {
                return RemotingConfigHandler.GetServerTypeForUri(URI);
            }
            return identity.ServerType;
        }

        [SecurityCritical]
        public static string GetSessionIdForMethodMessage(IMethodMessage msg)
        {
            return msg.Uri;
        }

        [SecurityCritical]
        private static object GetType(object tp)
        {
            Type typeFromQualifiedTypeName = null;
            Identity identityObject = GetRealProxy(tp).IdentityObject;
            if (((identityObject != null) && (identityObject.ObjectRef != null)) && (identityObject.ObjectRef.TypeInfo != null))
            {
                string typeName = identityObject.ObjectRef.TypeInfo.TypeName;
                if (typeName != null)
                {
                    typeFromQualifiedTypeName = InternalGetTypeFromQualifiedTypeName(typeName);
                }
            }
            return typeFromQualifiedTypeName;
        }

        internal static string InternalGetClrTypeNameFromQualifiedTypeName(string qualifiedTypeName)
        {
            if ((qualifiedTypeName.Length > 4) && (string.CompareOrdinal(qualifiedTypeName, 0, "clr:", 0, 4) == 0))
            {
                return qualifiedTypeName.Substring(4);
            }
            return null;
        }

        [SecurityCritical]
        internal static MethodBase InternalGetMethodBaseFromMethodMessage(IMethodMessage msg)
        {
            if (msg == null)
            {
                return null;
            }
            Type typeFromQualifiedTypeName = InternalGetTypeFromQualifiedTypeName(msg.TypeName);
            if (typeFromQualifiedTypeName == null)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadType"), new object[] { msg.TypeName }));
            }
            Type[] methodSignature = (Type[]) msg.MethodSignature;
            return GetMethodBase(msg, typeFromQualifiedTypeName, methodSignature);
        }

        [SecurityCritical]
        internal static string InternalGetSoapTypeNameFromQualifiedTypeName(string xmlTypeName, string xmlTypeNamespace)
        {
            string str;
            string str2;
            if (SoapServices.DecodeXmlNamespaceForClrTypeNamespace(xmlTypeNamespace, out str, out str2))
            {
                string str3;
                if ((str != null) && (str.Length > 0))
                {
                    str3 = str + "." + xmlTypeName;
                }
                else
                {
                    str3 = xmlTypeName;
                }
                try
                {
                    return (str3 + ", " + str2);
                }
                catch
                {
                }
            }
            return null;
        }

        [SecurityCritical]
        internal static Type InternalGetTypeFromQualifiedTypeName(string qualifiedTypeName)
        {
            return InternalGetTypeFromQualifiedTypeName(qualifiedTypeName, true);
        }

        [SecurityCritical]
        internal static RuntimeType InternalGetTypeFromQualifiedTypeName(string qualifiedTypeName, bool partialFallback)
        {
            if (qualifiedTypeName == null)
            {
                throw new ArgumentNullException("qualifiedTypeName");
            }
            string clrTypeNameFromQualifiedTypeName = InternalGetClrTypeNameFromQualifiedTypeName(qualifiedTypeName);
            if (clrTypeNameFromQualifiedTypeName != null)
            {
                return LoadClrTypeWithPartialBindFallback(clrTypeNameFromQualifiedTypeName, partialFallback);
            }
            int num = IsSoapType(qualifiedTypeName);
            if (num != -1)
            {
                string xmlType = qualifiedTypeName.Substring(5, num - 5);
                string xmlTypeNamespace = qualifiedTypeName.Substring(num + 2, qualifiedTypeName.Length - (num + 2));
                RuntimeType interopTypeFromXmlType = (RuntimeType) SoapServices.GetInteropTypeFromXmlType(xmlType, xmlTypeNamespace);
                if (interopTypeFromXmlType != null)
                {
                    return interopTypeFromXmlType;
                }
                clrTypeNameFromQualifiedTypeName = InternalGetSoapTypeNameFromQualifiedTypeName(xmlType, xmlTypeNamespace);
                if (clrTypeNameFromQualifiedTypeName != null)
                {
                    return LoadClrTypeWithPartialBindFallback(clrTypeNameFromQualifiedTypeName, true);
                }
            }
            return LoadClrTypeWithPartialBindFallback(qualifiedTypeName, partialFallback);
        }

        [SecurityCritical]
        internal static string InternalGetTypeNameFromQualifiedTypeName(string qualifiedTypeName)
        {
            if (qualifiedTypeName == null)
            {
                throw new ArgumentNullException("qualifiedTypeName");
            }
            string clrTypeNameFromQualifiedTypeName = InternalGetClrTypeNameFromQualifiedTypeName(qualifiedTypeName);
            if (clrTypeNameFromQualifiedTypeName != null)
            {
                return clrTypeNameFromQualifiedTypeName;
            }
            int num = IsSoapType(qualifiedTypeName);
            if (num != -1)
            {
                string xmlTypeName = qualifiedTypeName.Substring(5, num - 5);
                string xmlTypeNamespace = qualifiedTypeName.Substring(num + 2, qualifiedTypeName.Length - (num + 2));
                clrTypeNameFromQualifiedTypeName = InternalGetSoapTypeNameFromQualifiedTypeName(xmlTypeName, xmlTypeNamespace);
                if (clrTypeNameFromQualifiedTypeName != null)
                {
                    return clrTypeNameFromQualifiedTypeName;
                }
            }
            return qualifiedTypeName;
        }

        [SecurityCritical]
        internal static void InternalSetRemoteActivationConfigured()
        {
            if (!s_bRemoteActivationConfigured)
            {
                nSetRemoteActivationConfigured();
                s_bRemoteActivationConfigured = true;
            }
        }

        [SecurityCritical]
        internal static object InternalUnmarshal(ObjRef objectRef, object proxy, bool fRefine)
        {
            object tPOrObject = null;
            Identity idObj = null;
            Context currentContext = Thread.CurrentContext;
            if (!ObjRef.IsWellFormed(objectRef))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadObjRef"), new object[] { "Unmarshal" }));
            }
            if (objectRef.IsWellKnown())
            {
                tPOrObject = Unmarshal(typeof(MarshalByRefObject), objectRef.URI);
                idObj = IdentityHolder.ResolveIdentity(objectRef.URI);
                if (idObj.ObjectRef == null)
                {
                    idObj.RaceSetObjRef(objectRef);
                }
                return tPOrObject;
            }
            idObj = IdentityHolder.FindOrCreateIdentity(objectRef.URI, null, objectRef);
            Context context2 = Thread.CurrentContext;
            ServerIdentity identity2 = idObj as ServerIdentity;
            if (identity2 != null)
            {
                Context context3 = Thread.CurrentContext;
                if (!identity2.IsContextBound)
                {
                    if (proxy != null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadInternalState_ProxySameAppDomain"), new object[0]));
                    }
                    tPOrObject = identity2.TPOrObject;
                }
                else
                {
                    IMessageSink chnlSink = null;
                    IMessageSink envoySink = null;
                    CreateEnvoyAndChannelSinks(identity2.TPOrObject, null, out chnlSink, out envoySink);
                    SetEnvoyAndChannelSinks(idObj, chnlSink, envoySink);
                    tPOrObject = GetOrCreateProxy(idObj, proxy, true);
                }
            }
            else
            {
                IMessageSink sink3 = null;
                IMessageSink sink4 = null;
                if (!objectRef.IsObjRefLite())
                {
                    CreateEnvoyAndChannelSinks(null, objectRef, out sink3, out sink4);
                }
                else
                {
                    CreateEnvoyAndChannelSinks(objectRef.URI, null, out sink3, out sink4);
                }
                SetEnvoyAndChannelSinks(idObj, sink3, sink4);
                if (objectRef.HasProxyAttribute())
                {
                    fRefine = true;
                }
                tPOrObject = GetOrCreateProxy(idObj, proxy, fRefine);
            }
            TrackingServices.UnmarshaledObject(tPOrObject, objectRef);
            return tPOrObject;
        }

        internal static bool IsClientProxy(object obj)
        {
            bool flag2;
            MarshalByRefObject obj2 = obj as MarshalByRefObject;
            if (obj2 == null)
            {
                return false;
            }
            bool flag = false;
            Identity identity = MarshalByRefObject.GetIdentity(obj2, out flag2);
            if ((identity != null) && !(identity is ServerIdentity))
            {
                flag = true;
            }
            return flag;
        }

        [SecurityCritical]
        internal static bool IsMethodAllowedRemotely(MethodBase method)
        {
            if (((s_FieldGetterMB == null) || (s_FieldSetterMB == null)) || (((s_IsInstanceOfTypeMB == null) || (s_InvokeMemberMB == null)) || (s_CanCastToXmlTypeMB == null)))
            {
                CodeAccessPermission.Assert(true);
                if (s_FieldGetterMB == null)
                {
                    s_FieldGetterMB = typeof(object).GetMethod("FieldGetter", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                if (s_FieldSetterMB == null)
                {
                    s_FieldSetterMB = typeof(object).GetMethod("FieldSetter", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                if (s_IsInstanceOfTypeMB == null)
                {
                    s_IsInstanceOfTypeMB = typeof(MarshalByRefObject).GetMethod("IsInstanceOfType", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                if (s_CanCastToXmlTypeMB == null)
                {
                    s_CanCastToXmlTypeMB = typeof(MarshalByRefObject).GetMethod("CanCastToXmlType", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                if (s_InvokeMemberMB == null)
                {
                    s_InvokeMemberMB = typeof(MarshalByRefObject).GetMethod("InvokeMember", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
            }
            if ((!(method == s_FieldGetterMB) && !(method == s_FieldSetterMB)) && (!(method == s_IsInstanceOfTypeMB) && !(method == s_InvokeMemberMB)))
            {
                return (method == s_CanCastToXmlTypeMB);
            }
            return true;
        }

        [SecurityCritical]
        public static bool IsMethodOverloaded(IMethodMessage msg)
        {
            return InternalRemotingServices.GetReflectionCachedData(msg.MethodBase).IsOverloaded();
        }

        public static bool IsObjectOutOfAppDomain(object tp)
        {
            return IsClientProxy(tp);
        }

        [SecuritySafeCritical]
        public static bool IsObjectOutOfContext(object tp)
        {
            if (!IsTransparentProxy(tp))
            {
                return false;
            }
            RealProxy realProxy = GetRealProxy(tp);
            ServerIdentity identityObject = realProxy.IdentityObject as ServerIdentity;
            if ((identityObject != null) && (realProxy is RemotingProxy))
            {
                return (Thread.CurrentContext != identityObject.ServerContext);
            }
            return true;
        }

        [SecurityCritical]
        internal static bool IsObjectOutOfProcess(object tp)
        {
            if (!IsTransparentProxy(tp))
            {
                return false;
            }
            Identity identityObject = GetRealProxy(tp).IdentityObject;
            if (identityObject is ServerIdentity)
            {
                return false;
            }
            if (identityObject != null)
            {
                ObjRef objectRef = identityObject.ObjectRef;
                if ((objectRef != null) && objectRef.IsFromThisProcess())
                {
                    return false;
                }
            }
            return true;
        }

        [SecurityCritical]
        public static bool IsOneWay(MethodBase method)
        {
            if (method == null)
            {
                return false;
            }
            return InternalRemotingServices.GetReflectionCachedData(method).IsOneWayMethod();
        }

        private static int IsSoapType(string qualifiedTypeName)
        {
            if ((qualifiedTypeName.Length > 5) && (string.CompareOrdinal(qualifiedTypeName, 0, "soap:", 0, 5) == 0))
            {
                return qualifiedTypeName.IndexOf(',', 5);
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern bool IsTransparentProxy(object proxy);
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static RuntimeType LoadClrTypeWithPartialBindFallback(string typeName, bool partialFallback)
        {
            if (!partialFallback)
            {
                return (RuntimeType) Type.GetType(typeName, false);
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeTypeHandle.GetTypeByName(typeName, false, false, false, ref lookForMyCaller, true);
        }

        [Obsolete("Use of this method is not recommended. The LogRemotingStage existed for internal diagnostic purposes only."), SecurityCritical, Conditional("REMOTING_PERF")]
        public static void LogRemotingStage(int stage)
        {
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ObjRef Marshal(MarshalByRefObject Obj)
        {
            return MarshalInternal(Obj, null, null);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ObjRef Marshal(MarshalByRefObject Obj, string URI)
        {
            return MarshalInternal(Obj, URI, null);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static ObjRef Marshal(MarshalByRefObject Obj, string ObjURI, Type RequestedType)
        {
            return MarshalInternal(Obj, ObjURI, RequestedType);
        }

        [SecurityCritical]
        internal static ObjRef MarshalInternal(MarshalByRefObject Obj, string ObjURI, Type RequestedType)
        {
            return MarshalInternal(Obj, ObjURI, RequestedType, true);
        }

        [SecurityCritical]
        internal static ObjRef MarshalInternal(MarshalByRefObject Obj, string ObjURI, Type RequestedType, bool updateChannelData)
        {
            if (Obj == null)
            {
                return null;
            }
            ObjRef objRefGiven = null;
            Identity orCreateIdentity = null;
            orCreateIdentity = GetOrCreateIdentity(Obj, ObjURI);
            if (RequestedType != null)
            {
                ServerIdentity identity2 = orCreateIdentity as ServerIdentity;
                if (identity2 != null)
                {
                    identity2.ServerType = RequestedType;
                    identity2.MarshaledAsSpecificType = true;
                }
            }
            objRefGiven = orCreateIdentity.ObjectRef;
            if (objRefGiven == null)
            {
                if (IsTransparentProxy(Obj))
                {
                    objRefGiven = GetRealProxy(Obj).CreateObjRef(RequestedType);
                }
                else
                {
                    objRefGiven = Obj.CreateObjRef(RequestedType);
                }
                if ((orCreateIdentity == null) || (objRefGiven == null))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidMarshalByRefObject"), "Obj");
                }
                objRefGiven = orCreateIdentity.RaceSetObjRef(objRefGiven);
            }
            ServerIdentity identity3 = orCreateIdentity as ServerIdentity;
            if (identity3 != null)
            {
                MarshalByRefObject obj2 = null;
                identity3.GetServerObjectChain(out obj2);
                Lease lease = orCreateIdentity.Lease;
                if (lease != null)
                {
                    lock (lease)
                    {
                        if (lease.CurrentState == LeaseState.Expired)
                        {
                            lease.ActivateLease();
                        }
                        else
                        {
                            lease.RenewInternal(orCreateIdentity.Lease.InitialLeaseTime);
                        }
                    }
                }
                if (updateChannelData && (objRefGiven.ChannelInfo != null))
                {
                    object[] currentChannelData = ChannelServices.CurrentChannelData;
                    if (!(Obj is AppDomain))
                    {
                        objRefGiven.ChannelInfo.ChannelData = currentChannelData;
                    }
                    else
                    {
                        int length = currentChannelData.Length;
                        object[] destinationArray = new object[length];
                        Array.Copy(currentChannelData, destinationArray, length);
                        for (int i = 0; i < length; i++)
                        {
                            if (!(destinationArray[i] is CrossAppDomainData))
                            {
                                destinationArray[i] = null;
                            }
                        }
                        objRefGiven.ChannelInfo.ChannelData = destinationArray;
                    }
                }
            }
            TrackingServices.MarshaledObject(Obj, objRefGiven);
            return objRefGiven;
        }

        [SecurityCritical]
        internal static byte[] MarshalToBuffer(object o, bool crossRuntime)
        {
            if (crossRuntime)
            {
                if (IsTransparentProxy(o))
                {
                    if ((GetRealProxy(o) is RemotingProxy) && (ChannelServices.RegisteredChannels.Length == 0))
                    {
                        return null;
                    }
                }
                else
                {
                    MarshalByRefObject obj2 = o as MarshalByRefObject;
                    if (((obj2 != null) && (ActivationServices.GetProxyAttribute(obj2.GetType()) == ActivationServices.DefaultProxyAttribute)) && (ChannelServices.RegisteredChannels.Length == 0))
                    {
                        return null;
                    }
                }
            }
            MemoryStream serializationStream = new MemoryStream();
            RemotingSurrogateSelector selector = new RemotingSurrogateSelector();
            new BinaryFormatter { SurrogateSelector = selector, Context = new StreamingContext(StreamingContextStates.Other) }.Serialize(serializationStream, o, null, false);
            return serializationStream.GetBuffer();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void nSetRemoteActivationConfigured();
        [SecurityCritical]
        internal static bool ProxyCheckCast(RealProxy rp, RuntimeType castType)
        {
            return CheckCast(rp, castType);
        }

        [SecurityCritical]
        internal static bool RegisterWellKnownChannels()
        {
            if (!s_bRegisteredWellKnownChannels)
            {
                bool lockTaken = false;
                object configLock = Thread.GetDomain().RemotingData.ConfigLock;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(configLock, ref lockTaken);
                    if (!s_bRegisteredWellKnownChannels && !s_bInProcessOfRegisteringWellKnownChannels)
                    {
                        s_bInProcessOfRegisteringWellKnownChannels = true;
                        CrossAppDomainChannel.RegisterChannel();
                        s_bRegisteredWellKnownChannels = true;
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
            return true;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void ResetInterfaceCache(object proxy);
        internal static void SetEnvoyAndChannelSinks(Identity idObj, IMessageSink chnlSink, IMessageSink envoySink)
        {
            if ((idObj.ChannelSink == null) && (chnlSink != null))
            {
                idObj.RaceSetChannelSink(chnlSink);
            }
            if (idObj.EnvoyChain == null)
            {
                if (envoySink == null)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadInternalState_FailEnvoySink"), new object[0]));
                }
                idObj.RaceSetEnvoyChain(envoySink);
            }
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void SetObjectUriForMarshal(MarshalByRefObject obj, string uri)
        {
            Identity identity = null;
            Identity identity2 = null;
            bool flag;
            identity = MarshalByRefObject.GetIdentity(obj, out flag);
            identity2 = identity as ServerIdentity;
            if ((identity != null) && (identity2 == null))
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_SetObjectUriForMarshal__ObjectNeedsToBeLocal"));
            }
            if ((identity != null) && (identity.URI != null))
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_SetObjectUriForMarshal__UriExists"));
            }
            if (identity == null)
            {
                Context serverCtx = null;
                serverCtx = Thread.GetDomain().GetDefaultContext();
                ServerIdentity id = new ServerIdentity(obj, serverCtx, uri);
                if (obj.__RaceSetServerIdentity(id) != id)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_SetObjectUriForMarshal__UriExists"));
                }
            }
            else
            {
                identity.SetOrCreateURI(uri, true);
            }
        }

        [SecurityCritical]
        private static MarshalByRefObject SetOrCreateProxy(Identity idObj, Type classToProxy, object proxy)
        {
            RealProxy realProxy = null;
            if (proxy == null)
            {
                ServerIdentity identity = idObj as ServerIdentity;
                if (idObj.ObjectRef != null)
                {
                    realProxy = ActivationServices.GetProxyAttribute(classToProxy).CreateProxy(idObj.ObjectRef, classToProxy, null, null);
                }
                if (realProxy == null)
                {
                    realProxy = ActivationServices.DefaultProxyAttribute.CreateProxy(idObj.ObjectRef, classToProxy, null, (identity == null) ? null : identity.ServerContext);
                }
            }
            else
            {
                realProxy = GetRealProxy(proxy);
            }
            realProxy.IdentityObject = idObj;
            proxy = realProxy.GetTransparentProxy();
            proxy = idObj.RaceSetTransparentProxy(proxy);
            return (MarshalByRefObject) proxy;
        }

        [SecurityCritical]
        public static object Unmarshal(ObjRef objectRef)
        {
            return InternalUnmarshal(objectRef, null, false);
        }

        [SecurityCritical]
        public static object Unmarshal(ObjRef objectRef, bool fRefine)
        {
            return InternalUnmarshal(objectRef, null, fRefine);
        }

        [SecurityCritical]
        internal static object Unmarshal(Type classToProxy, string url)
        {
            return Unmarshal(classToProxy, url, null);
        }

        [SecurityCritical]
        internal static object Unmarshal(Type classToProxy, string url, object data)
        {
            if (null == classToProxy)
            {
                throw new ArgumentNullException("classToProxy");
            }
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            if (!classToProxy.IsMarshalByRef && !classToProxy.IsInterface)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_NotRemotableByReference"));
            }
            Identity idObj = IdentityHolder.ResolveIdentity(url);
            if (((idObj == null) || (idObj.ChannelSink == null)) || (idObj.EnvoyChain == null))
            {
                string objURI = null;
                IMessageSink chnlSink = null;
                IMessageSink envoySink = null;
                objURI = CreateEnvoyAndChannelSinks(url, data, out chnlSink, out envoySink);
                if (chnlSink == null)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Connect_CantCreateChannelSink"), new object[] { url }));
                }
                if (objURI == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUrl"));
                }
                idObj = IdentityHolder.FindOrCreateIdentity(objURI, url, null);
                SetEnvoyAndChannelSinks(idObj, chnlSink, envoySink);
            }
            return GetOrCreateProxy(classToProxy, idObj);
        }

        [SecurityCritical]
        internal static object UnmarshalFromBuffer(byte[] b, bool crossRuntime)
        {
            MemoryStream serializationStream = new MemoryStream(b);
            object proxy = new BinaryFormatter { AssemblyFormat = FormatterAssemblyStyle.Simple, SurrogateSelector = null, Context = new StreamingContext(StreamingContextStates.Other) }.Deserialize(serializationStream, null, false);
            if (crossRuntime && IsTransparentProxy(proxy))
            {
                if (!(GetRealProxy(proxy) is RemotingProxy))
                {
                    return proxy;
                }
                if (ChannelServices.RegisteredChannels.Length == 0)
                {
                    return null;
                }
                proxy.GetHashCode();
            }
            return proxy;
        }

        internal static object UnmarshalReturnMessageFromBuffer(byte[] b, IMethodCallMessage msg)
        {
            MemoryStream serializationStream = new MemoryStream(b);
            BinaryFormatter formatter = new BinaryFormatter {
                SurrogateSelector = null,
                Context = new StreamingContext(StreamingContextStates.Other)
            };
            return formatter.DeserializeMethodResponse(serializationStream, null, msg);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object Unwrap(ContextBoundObject obj);
        [SecurityCritical]
        internal static object Wrap(ContextBoundObject obj)
        {
            return Wrap(obj, null, true);
        }

        [SecurityCritical]
        internal static object Wrap(ContextBoundObject obj, object proxy, bool fCreateSinks)
        {
            if ((obj == null) || IsTransparentProxy(obj))
            {
                return obj;
            }
            Identity idObj = null;
            if (proxy != null)
            {
                RealProxy proxy2 = GetRealProxy(proxy);
                if (proxy2.UnwrappedServerObject == null)
                {
                    proxy2.AttachServerHelper(obj);
                }
                idObj = MarshalByRefObject.GetIdentity(obj);
            }
            else
            {
                idObj = IdentityHolder.FindOrCreateServerIdentity(obj, null, 0);
            }
            proxy = GetOrCreateProxy(idObj, proxy, true);
            GetRealProxy(proxy).Wrap();
            if (fCreateSinks)
            {
                IMessageSink chnlSink = null;
                IMessageSink envoySink = null;
                CreateEnvoyAndChannelSinks((MarshalByRefObject) proxy, null, out chnlSink, out envoySink);
                SetEnvoyAndChannelSinks(idObj, chnlSink, envoySink);
            }
            RealProxy realProxy = GetRealProxy(proxy);
            if (realProxy.UnwrappedServerObject == null)
            {
                realProxy.AttachServerHelper(obj);
            }
            return proxy;
        }
    }
}

