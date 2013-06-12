namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Metadata;
    using System.Security;
    using System.Security.Principal;

    [Serializable]
    internal class StackBuilderSink : IMessageSink
    {
        private bool _bStatic;
        private object _server;
        private static string sIRemoteDispatch = "System.EnterpriseServices.IRemoteDispatch";
        private static string sIRemoteDispatchAssembly = "System.EnterpriseServices";

        public StackBuilderSink(MarshalByRefObject server)
        {
            this._server = server;
        }

        public StackBuilderSink(object server)
        {
            this._server = server;
            if (this._server == null)
            {
                this._bStatic = true;
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern object _PrivateProcessMessage(IntPtr md, object[] args, object server, int methodPtr, bool fExecuteInContext, out object[] outArgs);
        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            IMethodCallMessage message = (IMethodCallMessage) msg;
            IMessageCtrl ctrl = null;
            IMessage message2 = null;
            LogicalCallContext threadCallContext = null;
            bool flag = false;
            try
            {
                try
                {
                    try
                    {
                        LogicalCallContext callCtx = (LogicalCallContext) message.Properties[Message.CallContextKey];
                        object server = this._server;
                        VerifyIsOkToCallMethod(server, message);
                        threadCallContext = CallContext.SetLogicalCallContext(callCtx);
                        flag = true;
                        callCtx.PropagateIncomingHeadersToCallContext(msg);
                        PreserveThreadPrincipalIfNecessary(callCtx, threadCallContext);
                        ServerChannelSinkStack stack = msg.Properties["__SinkStack"] as ServerChannelSinkStack;
                        if (stack != null)
                        {
                            stack.ServerObject = server;
                        }
                        MethodBase methodBase = GetMethodBase(message);
                        object[] outArgs = null;
                        object ret = null;
                        RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(methodBase);
                        object[] args = Message.CoerceArgs(message, reflectionCachedData.Parameters);
                        ret = this.PrivateProcessMessage(methodBase.MethodHandle, args, server, 0, false, out outArgs);
                        this.CopyNonByrefOutArgsFromOriginalArgs(reflectionCachedData, args, ref outArgs);
                        if (replySink != null)
                        {
                            LogicalCallContext logicalCallContext = CallContext.GetLogicalCallContext();
                            if (logicalCallContext != null)
                            {
                                logicalCallContext.RemovePrincipalIfNotSerializable();
                            }
                            message2 = new ReturnMessage(ret, outArgs, (outArgs == null) ? 0 : outArgs.Length, logicalCallContext, message);
                            logicalCallContext.PropagateOutgoingHeadersToMessage(message2);
                        }
                        return ctrl;
                    }
                    catch (Exception exception)
                    {
                        if (replySink != null)
                        {
                            message2 = new ReturnMessage(exception, message);
                            ((ReturnMessage) message2).SetLogicalCallContext((LogicalCallContext) message.Properties[Message.CallContextKey]);
                        }
                    }
                    return ctrl;
                }
                finally
                {
                    if (replySink != null)
                    {
                        replySink.SyncProcessMessage(message2);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    CallContext.SetLogicalCallContext(threadCallContext);
                }
            }
            return ctrl;
        }

        internal void CopyNonByrefOutArgsFromOriginalArgs(RemotingMethodCachedData methodCache, object[] args, ref object[] marshalResponseArgs)
        {
            int[] nonRefOutArgMap = methodCache.NonRefOutArgMap;
            if (nonRefOutArgMap.Length > 0)
            {
                if (marshalResponseArgs == null)
                {
                    marshalResponseArgs = new object[methodCache.Parameters.Length];
                }
                foreach (int num in nonRefOutArgMap)
                {
                    marshalResponseArgs[num] = args[num];
                }
            }
        }

        [SecurityCritical]
        private static MethodBase GetMethodBase(IMethodMessage msg)
        {
            MethodBase methodBase = msg.MethodBase;
            if (null == methodBase)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_MethodMissing"), new object[] { msg.MethodName, msg.TypeName }));
            }
            return methodBase;
        }

        [SecurityCritical]
        internal bool IsOKToStackBlt(IMethodMessage mcMsg, object server)
        {
            bool flag = false;
            Message message = mcMsg as Message;
            if (message == null)
            {
                return flag;
            }
            IInternalMessage message2 = message;
            if ((!(message.GetFramePtr() != IntPtr.Zero) || (message.GetThisPtr() != server)) || ((message2.IdentityObject != null) && ((message2.IdentityObject == null) || (message2.IdentityObject != message2.ServerIdentityObject))))
            {
                return flag;
            }
            return true;
        }

        [SecurityCritical]
        internal static void PreserveThreadPrincipalIfNecessary(LogicalCallContext messageCallContext, LogicalCallContext threadCallContext)
        {
            if ((threadCallContext != null) && (messageCallContext.Principal == null))
            {
                IPrincipal principal = threadCallContext.Principal;
                if (principal != null)
                {
                    messageCallContext.Principal = principal;
                }
            }
        }

        [SecurityCritical]
        public object PrivateProcessMessage(RuntimeMethodHandle md, object[] args, object server, int methodPtr, bool fExecuteInContext, out object[] outArgs)
        {
            return this._PrivateProcessMessage(md.Value, args, server, methodPtr, fExecuteInContext, out outArgs);
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage msg)
        {
            return this.SyncProcessMessage(msg, 0, false);
        }

        [SecurityCritical]
        internal virtual IMessage SyncProcessMessage(IMessage msg, int methodPtr, bool fExecuteInContext)
        {
            IMessage message3;
            IMessage message = InternalSink.ValidateMessage(msg);
            if (message != null)
            {
                return message;
            }
            IMethodCallMessage message2 = msg as IMethodCallMessage;
            LogicalCallContext threadCallContext = null;
            object obj2 = CallContext.GetLogicalCallContext().GetData("__xADCall");
            bool flag = false;
            try
            {
                object server = this._server;
                VerifyIsOkToCallMethod(server, message2);
                LogicalCallContext callCtx = null;
                if (message2 != null)
                {
                    callCtx = message2.LogicalCallContext;
                }
                else
                {
                    callCtx = (LogicalCallContext) msg.Properties["__CallContext"];
                }
                threadCallContext = CallContext.SetLogicalCallContext(callCtx);
                flag = true;
                callCtx.PropagateIncomingHeadersToCallContext(msg);
                PreserveThreadPrincipalIfNecessary(callCtx, threadCallContext);
                if (this.IsOKToStackBlt(message2, server) && ((Message) message2).Dispatch(server, fExecuteInContext))
                {
                    message3 = new StackBasedReturnMessage();
                    ((StackBasedReturnMessage) message3).InitFields((Message) message2);
                    LogicalCallContext context4 = CallContext.GetLogicalCallContext();
                    context4.PropagateOutgoingHeadersToMessage(message3);
                    ((StackBasedReturnMessage) message3).SetLogicalCallContext(context4);
                    return message3;
                }
                MethodBase methodBase = GetMethodBase(message2);
                object[] outArgs = null;
                object ret = null;
                RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(methodBase);
                object[] args = Message.CoerceArgs(message2, reflectionCachedData.Parameters);
                ret = this.PrivateProcessMessage(methodBase.MethodHandle, args, server, methodPtr, fExecuteInContext, out outArgs);
                this.CopyNonByrefOutArgsFromOriginalArgs(reflectionCachedData, args, ref outArgs);
                LogicalCallContext logicalCallContext = CallContext.GetLogicalCallContext();
                if (((obj2 != null) && ((bool) obj2)) && (logicalCallContext != null))
                {
                    logicalCallContext.RemovePrincipalIfNotSerializable();
                }
                message3 = new ReturnMessage(ret, outArgs, (outArgs == null) ? 0 : outArgs.Length, logicalCallContext, message2);
                logicalCallContext.PropagateOutgoingHeadersToMessage(message3);
                CallContext.SetLogicalCallContext(threadCallContext);
            }
            catch (Exception exception)
            {
                message3 = new ReturnMessage(exception, message2);
                ((ReturnMessage) message3).SetLogicalCallContext(message2.LogicalCallContext);
                if (flag)
                {
                    CallContext.SetLogicalCallContext(threadCallContext);
                }
            }
            return message3;
        }

        [SecurityCritical]
        private static void VerifyIsOkToCallMethod(object server, IMethodMessage msg)
        {
            bool flag = false;
            MarshalByRefObject obj2 = server as MarshalByRefObject;
            if (obj2 != null)
            {
                bool flag2;
                Identity identity = MarshalByRefObject.GetIdentity(obj2, out flag2);
                if (identity != null)
                {
                    ServerIdentity identity2 = identity as ServerIdentity;
                    if ((identity2 != null) && identity2.MarshaledAsSpecificType)
                    {
                        Type serverType = identity2.ServerType;
                        if (serverType != null)
                        {
                            MethodBase methodBase = GetMethodBase(msg);
                            RuntimeType declaringType = (RuntimeType) methodBase.DeclaringType;
                            if ((declaringType != serverType) && !declaringType.IsAssignableFrom(serverType))
                            {
                                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_InvalidCallingType"), new object[] { methodBase.DeclaringType.FullName, serverType.FullName }));
                            }
                            if (declaringType.IsInterface)
                            {
                                VerifyNotIRemoteDispatch(declaringType);
                            }
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    RuntimeType reflectedType = (RuntimeType) GetMethodBase(msg).ReflectedType;
                    if (!reflectedType.IsInterface)
                    {
                        if (!reflectedType.IsInstanceOfType(obj2))
                        {
                            throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_InvalidCallingType"), new object[] { reflectedType.FullName, obj2.GetType().FullName }));
                        }
                    }
                    else
                    {
                        VerifyNotIRemoteDispatch(reflectedType);
                    }
                }
            }
        }

        [SecurityCritical]
        private static void VerifyNotIRemoteDispatch(RuntimeType reflectedType)
        {
            if (reflectedType.FullName.Equals(sIRemoteDispatch) && reflectedType.GetRuntimeAssembly().GetSimpleName().Equals(sIRemoteDispatchAssembly))
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_CantInvokeIRemoteDispatch"));
            }
        }

        public IMessageSink NextSink
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }

        internal object ServerObject
        {
            get
            {
                return this._server;
            }
        }
    }
}

