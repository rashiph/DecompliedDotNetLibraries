namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal class Message : IMethodCallMessage, IMethodMessage, IMessage, IInternalMessage, ISerializable
    {
        private ArgMapper _argMapper;
        [SecurityCritical]
        private System.Runtime.Remoting.Messaging.LogicalCallContext _callContext;
        private IntPtr _delegateMD;
        private Exception _Fault;
        private int _flags;
        private IntPtr _frame;
        private IntPtr _governingType;
        private Identity _ID;
        private bool _initDone;
        private IntPtr _metaSigHolder;
        private System.Reflection.MethodBase _MethodBase;
        private IntPtr _methodDesc;
        private string _MethodName;
        private Type[] _MethodSignature;
        private object _properties;
        private ServerIdentity _srvID;
        private string _typeName;
        private string _URI;
        internal const int BeginAsync = 1;
        internal static string CallContextKey = "__CallContext";
        internal const int CallMask = 15;
        internal const int Ctor = 4;
        internal const int EndAsync = 2;
        internal const int FixedArgs = 0x10;
        internal const int OneWay = 8;
        internal const int Sync = 0;
        internal static string UriKey = "__Uri";
        internal const int VarArgs = 0x20;

        internal Message()
        {
        }

        [SecurityCritical]
        internal static object CoerceArg(object value, Type pt)
        {
            object obj2 = null;
            if (value == null)
            {
                return obj2;
            }
            Exception innerException = null;
            try
            {
                if (pt.IsByRef)
                {
                    pt = pt.GetElementType();
                }
                if (pt.IsInstanceOfType(value))
                {
                    obj2 = value;
                }
                else
                {
                    obj2 = Convert.ChangeType(value, pt, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception exception2)
            {
                innerException = exception2;
            }
            if (obj2 != null)
            {
                return obj2;
            }
            string str = null;
            if (RemotingServices.IsTransparentProxy(value))
            {
                str = typeof(MarshalByRefObject).ToString();
            }
            else
            {
                str = value.ToString();
            }
            throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_CoercionFailed"), new object[] { str, pt }), innerException);
        }

        [SecurityCritical]
        internal static object[] CoerceArgs(IMethodMessage m)
        {
            RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(m.MethodBase);
            return CoerceArgs(m, reflectionCachedData.Parameters);
        }

        [SecurityCritical]
        internal static object[] CoerceArgs(IMethodMessage m, ParameterInfo[] pi)
        {
            return CoerceArgs(m.MethodBase, m.Args, pi);
        }

        [SecurityCritical]
        internal static object[] CoerceArgs(System.Reflection.MethodBase mb, object[] args, ParameterInfo[] pi)
        {
            if (pi == null)
            {
                throw new ArgumentNullException("pi");
            }
            if (pi.Length != args.Length)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_ArgMismatch"), new object[] { mb.DeclaringType.FullName, mb.Name, args.Length, pi.Length }));
            }
            for (int i = 0; i < pi.Length; i++)
            {
                ParameterInfo info = pi[i];
                Type parameterType = info.ParameterType;
                object obj2 = args[i];
                if (obj2 != null)
                {
                    args[i] = CoerceArg(obj2, parameterType);
                }
                else if (parameterType.IsByRef)
                {
                    Type elementType = parameterType.GetElementType();
                    if (elementType.IsValueType)
                    {
                        if (!info.IsOut)
                        {
                            if (!elementType.IsGenericType || (elementType.GetGenericTypeDefinition() != typeof(Nullable<>)))
                            {
                                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_MissingArgValue"), new object[] { elementType.FullName, i }));
                            }
                        }
                        else
                        {
                            args[i] = Activator.CreateInstance(elementType, true);
                        }
                    }
                }
                else if (parameterType.IsValueType && (!parameterType.IsGenericType || (parameterType.GetGenericTypeDefinition() != typeof(Nullable<>))))
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_MissingArgValue"), new object[] { parameterType.FullName, i }));
                }
            }
            return args;
        }

        [SecurityCritical, Conditional("_REMOTING_DEBUG")]
        public static void DebugOut(string s)
        {
            OutToUnmanagedDebugger(string.Concat(new object[] { "\nRMTING: Thrd ", Thread.CurrentThread.GetHashCode(), " : ", s }));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public extern bool Dispatch(object target, bool fExecuteInContext);
        [SecurityCritical]
        internal static Type[] GenerateMethodSignature(System.Reflection.MethodBase mb)
        {
            ParameterInfo[] parameters = InternalRemotingServices.GetReflectionCachedData(mb).Parameters;
            Type[] typeArray = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                typeArray[i] = parameters[i].ParameterType;
            }
            return typeArray;
        }

        [SecurityCritical]
        public object GetArg(int argNum)
        {
            return this.InternalGetArg(argNum);
        }

        [SecurityCritical]
        public string GetArgName(int index)
        {
            if (index >= this.ArgCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ParameterInfo[] parameters = InternalRemotingServices.GetReflectionCachedData(this.GetMethodBase()).Parameters;
            if (index < parameters.Length)
            {
                return parameters[index].Name;
            }
            return ("VarArg" + (index - parameters.Length));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public extern void GetAsyncBeginInfo(out AsyncCallback acbd, out object state);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public extern IAsyncResult GetAsyncResult();
        public virtual int GetCallType()
        {
            this.InitIfNecessary();
            return this._flags;
        }

        public virtual Exception GetFault()
        {
            return this._Fault;
        }

        internal IntPtr GetFramePtr()
        {
            return this._frame;
        }

        [SecurityCritical]
        public object GetInArg(int argNum)
        {
            if (this._argMapper == null)
            {
                this._argMapper = new ArgMapper(this, false);
            }
            return this._argMapper.GetArg(argNum);
        }

        [SecurityCritical]
        public string GetInArgName(int index)
        {
            if (this._argMapper == null)
            {
                this._argMapper = new ArgMapper(this, false);
            }
            return this._argMapper.GetArgName(index);
        }

        [SecurityCritical]
        internal System.Runtime.Remoting.Messaging.LogicalCallContext GetLogicalCallContext()
        {
            if (this._callContext == null)
            {
                this._callContext = new System.Runtime.Remoting.Messaging.LogicalCallContext();
            }
            return this._callContext;
        }

        [SecurityCritical]
        internal System.Reflection.MethodBase GetMethodBase()
        {
            if (null == this._MethodBase)
            {
                IRuntimeMethodInfo methodHandle = new RuntimeMethodInfoStub(this._methodDesc, null);
                this._MethodBase = RuntimeType.GetMethodBase(Type.GetTypeFromHandleUnsafe(this._governingType), methodHandle);
            }
            return this._MethodBase;
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public extern object GetReturnValue();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public extern object GetThisPtr();
        public void Init()
        {
        }

        [SecurityCritical]
        internal void InitFields(MessageData msgData)
        {
            this._frame = msgData.pFrame;
            this._delegateMD = msgData.pDelegateMD;
            this._methodDesc = msgData.pMethodDesc;
            this._flags = msgData.iFlags;
            this._initDone = true;
            this._metaSigHolder = msgData.pSig;
            this._governingType = msgData.thGoverningType;
            this._MethodName = null;
            this._MethodSignature = null;
            this._MethodBase = null;
            this._URI = null;
            this._Fault = null;
            this._ID = null;
            this._srvID = null;
            this._callContext = null;
            if (this._properties != null)
            {
                ((IDictionary) this._properties).Clear();
            }
        }

        private void InitIfNecessary()
        {
            if (!this._initDone)
            {
                this.Init();
                this._initDone = true;
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern object InternalGetArg(int argNum);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern int InternalGetArgCount();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern object[] InternalGetArgs();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern bool InternalHasVarArgs();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void OutToUnmanagedDebugger(string s);
        [SecurityCritical]
        internal static System.Runtime.Remoting.Messaging.LogicalCallContext PropagateCallContextFromMessageToThread(IMessage msg)
        {
            return CallContext.SetLogicalCallContext((System.Runtime.Remoting.Messaging.LogicalCallContext) msg.Properties[CallContextKey]);
        }

        [SecurityCritical]
        internal static void PropagateCallContextFromThreadToMessage(IMessage msg)
        {
            System.Runtime.Remoting.Messaging.LogicalCallContext logicalCallContext = CallContext.GetLogicalCallContext();
            msg.Properties[CallContextKey] = logicalCallContext;
        }

        [SecurityCritical]
        internal static void PropagateCallContextFromThreadToMessage(IMessage msg, System.Runtime.Remoting.Messaging.LogicalCallContext oldcctx)
        {
            PropagateCallContextFromThreadToMessage(msg);
            CallContext.SetLogicalCallContext(oldcctx);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public extern void PropagateOutParameters(object[] OutArgs, object retVal);
        public virtual void SetFault(Exception e)
        {
            this._Fault = e;
        }

        [SecurityCritical]
        internal System.Runtime.Remoting.Messaging.LogicalCallContext SetLogicalCallContext(System.Runtime.Remoting.Messaging.LogicalCallContext callCtx)
        {
            System.Runtime.Remoting.Messaging.LogicalCallContext context = this._callContext;
            this._callContext = callCtx;
            return context;
        }

        internal virtual void SetOneWay()
        {
            this._flags |= 8;
        }

        [SecurityCritical]
        internal static object SoapCoerceArg(object value, Type pt, Hashtable keyToNamespaceTable)
        {
            object obj2 = null;
            if (value == null)
            {
                return obj2;
            }
            try
            {
                if (pt.IsByRef)
                {
                    pt = pt.GetElementType();
                }
                if (pt.IsInstanceOfType(value))
                {
                    obj2 = value;
                }
                else
                {
                    string s = value as string;
                    if (s != null)
                    {
                        if (pt == typeof(double))
                        {
                            if (s == "INF")
                            {
                                obj2 = (double) 1.0 / (double) 0.0;
                            }
                            else if (s == "-INF")
                            {
                                obj2 = (double) -1.0 / (double) 0.0;
                            }
                            else
                            {
                                obj2 = double.Parse(s, CultureInfo.InvariantCulture);
                            }
                        }
                        else if (pt == typeof(float))
                        {
                            if (s == "INF")
                            {
                                obj2 = (float) 1.0 / (float) 0.0;
                            }
                            else if (s == "-INF")
                            {
                                obj2 = (float) -1.0 / (float) 0.0;
                            }
                            else
                            {
                                obj2 = float.Parse(s, CultureInfo.InvariantCulture);
                            }
                        }
                        else if (SoapType.typeofISoapXsd.IsAssignableFrom(pt))
                        {
                            if (pt == SoapType.typeofSoapTime)
                            {
                                obj2 = SoapTime.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapDate)
                            {
                                obj2 = SoapDate.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapYearMonth)
                            {
                                obj2 = SoapYearMonth.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapYear)
                            {
                                obj2 = SoapYear.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapMonthDay)
                            {
                                obj2 = SoapMonthDay.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapDay)
                            {
                                obj2 = SoapDay.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapMonth)
                            {
                                obj2 = SoapMonth.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapHexBinary)
                            {
                                obj2 = SoapHexBinary.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapBase64Binary)
                            {
                                obj2 = SoapBase64Binary.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapInteger)
                            {
                                obj2 = SoapInteger.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapPositiveInteger)
                            {
                                obj2 = SoapPositiveInteger.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapNonPositiveInteger)
                            {
                                obj2 = SoapNonPositiveInteger.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapNonNegativeInteger)
                            {
                                obj2 = SoapNonNegativeInteger.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapNegativeInteger)
                            {
                                obj2 = SoapNegativeInteger.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapAnyUri)
                            {
                                obj2 = SoapAnyUri.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapQName)
                            {
                                obj2 = SoapQName.Parse(s);
                                SoapQName name = (SoapQName) obj2;
                                if (name.Key.Length == 0)
                                {
                                    name.Namespace = (string) keyToNamespaceTable["xmlns"];
                                }
                                else
                                {
                                    name.Namespace = (string) keyToNamespaceTable["xmlns:" + name.Key];
                                }
                            }
                            else if (pt == SoapType.typeofSoapNotation)
                            {
                                obj2 = SoapNotation.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapNormalizedString)
                            {
                                obj2 = SoapNormalizedString.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapToken)
                            {
                                obj2 = SoapToken.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapLanguage)
                            {
                                obj2 = SoapLanguage.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapName)
                            {
                                obj2 = SoapName.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapIdrefs)
                            {
                                obj2 = SoapIdrefs.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapEntities)
                            {
                                obj2 = SoapEntities.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapNmtoken)
                            {
                                obj2 = SoapNmtoken.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapNmtokens)
                            {
                                obj2 = SoapNmtokens.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapNcName)
                            {
                                obj2 = SoapNcName.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapId)
                            {
                                obj2 = SoapId.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapIdref)
                            {
                                obj2 = SoapIdref.Parse(s);
                            }
                            else if (pt == SoapType.typeofSoapEntity)
                            {
                                obj2 = SoapEntity.Parse(s);
                            }
                        }
                        else
                        {
                            if (pt == typeof(bool))
                            {
                                switch (s)
                                {
                                    case "1":
                                    case "true":
                                        obj2 = true;
                                        goto Label_055E;

                                    case "0":
                                    case "false":
                                        obj2 = false;
                                        goto Label_055E;
                                }
                                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_CoercionFailed"), new object[] { s, pt }));
                            }
                            if (pt == typeof(DateTime))
                            {
                                obj2 = SoapDateTime.Parse(s);
                            }
                            else if (pt.IsPrimitive)
                            {
                                obj2 = Convert.ChangeType(value, pt, CultureInfo.InvariantCulture);
                            }
                            else if (pt == typeof(TimeSpan))
                            {
                                obj2 = SoapDuration.Parse(s);
                            }
                            else if (pt == typeof(char))
                            {
                                obj2 = s[0];
                            }
                            else
                            {
                                obj2 = Convert.ChangeType(value, pt, CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    else
                    {
                        obj2 = Convert.ChangeType(value, pt, CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception)
            {
            }
        Label_055E:
            if (obj2 != null)
            {
                return obj2;
            }
            string str2 = null;
            if (RemotingServices.IsTransparentProxy(value))
            {
                str2 = typeof(MarshalByRefObject).ToString();
            }
            else
            {
                str2 = value.ToString();
            }
            throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_CoercionFailed"), new object[] { str2, pt }));
        }

        [SecurityCritical]
        bool IInternalMessage.HasProperties()
        {
            return (this._properties != null);
        }

        [SecurityCritical]
        void IInternalMessage.SetCallContext(System.Runtime.Remoting.Messaging.LogicalCallContext callContext)
        {
            this._callContext = callContext;
        }

        [SecurityCritical]
        void IInternalMessage.SetURI(string URI)
        {
            this._URI = URI;
        }

        [SecurityCritical]
        private void UpdateNames()
        {
            RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(this.GetMethodBase());
            this._typeName = reflectionCachedData.TypeAndAssemblyName;
            this._MethodName = reflectionCachedData.MethodName;
        }

        public int ArgCount
        {
            [SecurityCritical]
            get
            {
                return this.InternalGetArgCount();
            }
        }

        public object[] Args
        {
            [SecurityCritical]
            get
            {
                return this.InternalGetArgs();
            }
        }

        public bool HasVarArgs
        {
            [SecurityCritical]
            get
            {
                if (((this._flags & 0x10) == 0) && ((this._flags & 0x20) == 0))
                {
                    if (!this.InternalHasVarArgs())
                    {
                        this._flags |= 0x10;
                    }
                    else
                    {
                        this._flags |= 0x20;
                    }
                }
                return (1 == (this._flags & 0x20));
            }
        }

        public int InArgCount
        {
            [SecurityCritical]
            get
            {
                if (this._argMapper == null)
                {
                    this._argMapper = new ArgMapper(this, false);
                }
                return this._argMapper.ArgCount;
            }
        }

        public object[] InArgs
        {
            [SecurityCritical]
            get
            {
                if (this._argMapper == null)
                {
                    this._argMapper = new ArgMapper(this, false);
                }
                return this._argMapper.Args;
            }
        }

        public System.Runtime.Remoting.Messaging.LogicalCallContext LogicalCallContext
        {
            [SecurityCritical]
            get
            {
                return this.GetLogicalCallContext();
            }
        }

        public System.Reflection.MethodBase MethodBase
        {
            [SecurityCritical]
            get
            {
                return this.GetMethodBase();
            }
        }

        public string MethodName
        {
            [SecurityCritical]
            get
            {
                if (this._MethodName == null)
                {
                    this.UpdateNames();
                }
                return this._MethodName;
            }
        }

        public object MethodSignature
        {
            [SecurityCritical]
            get
            {
                if (this._MethodSignature == null)
                {
                    this._MethodSignature = GenerateMethodSignature(this.GetMethodBase());
                }
                return this._MethodSignature;
            }
        }

        public IDictionary Properties
        {
            [SecurityCritical]
            get
            {
                if (this._properties == null)
                {
                    Interlocked.CompareExchange(ref this._properties, new MCMDictionary(this, null), null);
                }
                return (IDictionary) this._properties;
            }
        }

        Identity IInternalMessage.IdentityObject
        {
            [SecurityCritical]
            get
            {
                return this._ID;
            }
            [SecurityCritical]
            set
            {
                this._ID = value;
            }
        }

        ServerIdentity IInternalMessage.ServerIdentityObject
        {
            [SecurityCritical]
            get
            {
                return this._srvID;
            }
            [SecurityCritical]
            set
            {
                this._srvID = value;
            }
        }

        public string TypeName
        {
            [SecurityCritical]
            get
            {
                if (this._typeName == null)
                {
                    this.UpdateNames();
                }
                return this._typeName;
            }
        }

        public string Uri
        {
            [SecurityCritical]
            get
            {
                return this._URI;
            }
            set
            {
                this._URI = value;
            }
        }
    }
}

