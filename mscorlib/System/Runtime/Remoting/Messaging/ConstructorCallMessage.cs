namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Proxies;
    using System.Security;
    using System.Threading;

    internal class ConstructorCallMessage : IConstructionCallMessage, IMethodCallMessage, IMethodMessage, IMessage
    {
        [NonSerialized]
        private RuntimeType _activationType;
        private string _activationTypeName;
        private IActivator _activator;
        private ArgMapper _argMapper;
        private object[] _callSiteActivationAttributes;
        private IList _contextProperties;
        private int _iFlags;
        private Message _message;
        private object _properties;
        private object[] _typeAttributes;
        private object[] _womGlobalAttributes;
        private const int CCM_ACTIVATEINCONTEXT = 1;

        private ConstructorCallMessage()
        {
        }

        [SecurityCritical]
        internal ConstructorCallMessage(object[] callSiteActivationAttributes, object[] womAttr, object[] typeAttr, RuntimeType serverType)
        {
            this._activationType = serverType;
            this._activationTypeName = RemotingServices.GetDefaultQualifiedTypeName(this._activationType);
            this._callSiteActivationAttributes = callSiteActivationAttributes;
            this._womGlobalAttributes = womAttr;
            this._typeAttributes = typeAttr;
        }

        [SecurityCritical]
        public object GetArg(int argNum)
        {
            if (this._message == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
            }
            return this._message.GetArg(argNum);
        }

        [SecurityCritical]
        public string GetArgName(int index)
        {
            if (this._message == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
            }
            return this._message.GetArgName(index);
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
            if (this._message == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
            }
            return this._message.GetLogicalCallContext();
        }

        internal Message GetMessage()
        {
            return this._message;
        }

        [SecurityCritical]
        public object GetThisPtr()
        {
            if (this._message == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
            }
            return this._message.GetThisPtr();
        }

        internal object[] GetTypeAttributes()
        {
            return this._typeAttributes;
        }

        internal object[] GetWOMAttributes()
        {
            return this._womGlobalAttributes;
        }

        [SecurityCritical]
        internal void SetFrame(MessageData msgData)
        {
            this._message = new Message();
            this._message.InitFields(msgData);
        }

        [SecurityCritical]
        internal System.Runtime.Remoting.Messaging.LogicalCallContext SetLogicalCallContext(System.Runtime.Remoting.Messaging.LogicalCallContext ctx)
        {
            if (this._message == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
            }
            return this._message.SetLogicalCallContext(ctx);
        }

        internal bool ActivateInContext
        {
            get
            {
                return ((this._iFlags & 1) != 0);
            }
            set
            {
                this._iFlags = value ? (this._iFlags | 1) : (this._iFlags & -2);
            }
        }

        public Type ActivationType
        {
            [SecurityCritical]
            get
            {
                if ((this._activationType == null) && (this._activationTypeName != null))
                {
                    this._activationType = RemotingServices.InternalGetTypeFromQualifiedTypeName(this._activationTypeName, false);
                }
                return this._activationType;
            }
        }

        public string ActivationTypeName
        {
            [SecurityCritical]
            get
            {
                return this._activationTypeName;
            }
        }

        public IActivator Activator
        {
            [SecurityCritical]
            get
            {
                return this._activator;
            }
            [SecurityCritical]
            set
            {
                this._activator = value;
            }
        }

        public int ArgCount
        {
            [SecurityCritical]
            get
            {
                if (this._message == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                return this._message.ArgCount;
            }
        }

        public object[] Args
        {
            [SecurityCritical]
            get
            {
                if (this._message == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                return this._message.Args;
            }
        }

        public object[] CallSiteActivationAttributes
        {
            [SecurityCritical]
            get
            {
                return this._callSiteActivationAttributes;
            }
        }

        public IList ContextProperties
        {
            [SecurityCritical]
            get
            {
                if (this._contextProperties == null)
                {
                    this._contextProperties = new ArrayList();
                }
                return this._contextProperties;
            }
        }

        public bool HasVarArgs
        {
            [SecurityCritical]
            get
            {
                if (this._message == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                return this._message.HasVarArgs;
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
                if (this._message == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                return this._message.MethodBase;
            }
        }

        public string MethodName
        {
            [SecurityCritical]
            get
            {
                if (this._message == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                return this._message.MethodName;
            }
        }

        public object MethodSignature
        {
            [SecurityCritical]
            get
            {
                if (this._message == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                return this._message.MethodSignature;
            }
        }

        public IDictionary Properties
        {
            [SecurityCritical]
            get
            {
                if (this._properties == null)
                {
                    object obj2 = new CCMDictionary(this, new Hashtable());
                    Interlocked.CompareExchange(ref this._properties, obj2, null);
                }
                return (IDictionary) this._properties;
            }
        }

        public string TypeName
        {
            [SecurityCritical]
            get
            {
                if (this._message == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                return this._message.TypeName;
            }
        }

        public string Uri
        {
            [SecurityCritical]
            get
            {
                if (this._message == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                return this._message.Uri;
            }
            set
            {
                if (this._message == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                this._message.Uri = value;
            }
        }
    }
}

