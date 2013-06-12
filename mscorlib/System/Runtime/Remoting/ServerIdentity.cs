namespace System.Runtime.Remoting
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Threading;

    internal class ServerIdentity : Identity
    {
        internal bool _bMarshaledAsSpecificType;
        internal DynamicPropertyHolder _dphSrv;
        internal int _firstCallDispatched;
        private LastCalledType _lastCalledType;
        internal IMessageSink _serverObjectChain;
        internal Context _srvCtx;
        internal GCHandle _srvIdentityHandle;
        internal Type _srvType;
        internal StackBuilderSink _stackBuilderSink;

        [SecurityCritical]
        internal ServerIdentity(MarshalByRefObject obj, Context serverCtx) : base(obj is ContextBoundObject)
        {
            if (obj != null)
            {
                if (!RemotingServices.IsTransparentProxy(obj))
                {
                    this._srvType = obj.GetType();
                }
                else
                {
                    this._srvType = RemotingServices.GetRealProxy(obj).GetProxiedType();
                }
            }
            this._srvCtx = serverCtx;
            this._serverObjectChain = null;
            this._stackBuilderSink = null;
        }

        [SecurityCritical]
        internal ServerIdentity(MarshalByRefObject obj, Context serverCtx, string uri) : this(obj, serverCtx)
        {
            base.SetOrCreateURI(uri, true);
        }

        [SecurityCritical]
        internal bool AddServerSideDynamicProperty(IDynamicProperty prop)
        {
            if (this._dphSrv == null)
            {
                DynamicPropertyHolder holder = new DynamicPropertyHolder();
                bool lockTaken = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(this, ref lockTaken);
                    if (this._dphSrv == null)
                    {
                        this._dphSrv = holder;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(this);
                    }
                }
            }
            return this._dphSrv.AddDynamicProperty(prop);
        }

        [SecurityCritical]
        internal override void AssertValid()
        {
            if (base.TPOrObject != null)
            {
                RemotingServices.IsTransparentProxy(base.TPOrObject);
            }
        }

        internal GCHandle GetHandle()
        {
            return this._srvIdentityHandle;
        }

        internal Type GetLastCalledType(string newTypeName)
        {
            LastCalledType type = this._lastCalledType;
            if (type != null)
            {
                string typeName = type.typeName;
                Type type2 = type.type;
                if ((typeName == null) || (type2 == null))
                {
                    return null;
                }
                if (typeName.Equals(newTypeName))
                {
                    return type2;
                }
            }
            return null;
        }

        [SecurityCritical]
        internal IMessageSink GetServerObjectChain(out MarshalByRefObject obj)
        {
            obj = null;
            if (!this.IsSingleCall())
            {
                if (this._serverObjectChain == null)
                {
                    bool lockTaken = false;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        Monitor.Enter(this, ref lockTaken);
                        if (this._serverObjectChain == null)
                        {
                            MarshalByRefObject tPOrObject = base.TPOrObject;
                            this._serverObjectChain = this._srvCtx.CreateServerObjectChain(tPOrObject);
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(this);
                        }
                    }
                }
                return this._serverObjectChain;
            }
            MarshalByRefObject serverObj = null;
            IMessageSink sink = null;
            if (((base._tpOrObject != null) && (this._firstCallDispatched == 0)) && (Interlocked.CompareExchange(ref this._firstCallDispatched, 1, 0) == 0))
            {
                serverObj = (MarshalByRefObject) base._tpOrObject;
                sink = this._serverObjectChain;
                if (sink == null)
                {
                    sink = this._srvCtx.CreateServerObjectChain(serverObj);
                }
            }
            else
            {
                serverObj = (MarshalByRefObject) Activator.CreateInstance(this._srvType, true);
                if (RemotingServices.GetObjectUri(serverObj) != null)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_WellKnown_CtorCantMarshal"), new object[] { base.URI }));
                }
                if (!RemotingServices.IsTransparentProxy(serverObj))
                {
                    serverObj.__RaceSetServerIdentity(this);
                }
                else
                {
                    RemotingServices.GetRealProxy(serverObj).IdentityObject = this;
                }
                sink = this._srvCtx.CreateServerObjectChain(serverObj);
            }
            obj = serverObj;
            return sink;
        }

        internal bool IsSingleCall()
        {
            return ((base._flags & 0x200) != 0);
        }

        internal bool IsSingleton()
        {
            return ((base._flags & 0x400) != 0);
        }

        [SecurityCritical]
        internal IMessageSink RaceSetServerObjectChain(IMessageSink serverObjectChain)
        {
            if (this._serverObjectChain == null)
            {
                bool lockTaken = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(this, ref lockTaken);
                    if (this._serverObjectChain == null)
                    {
                        this._serverObjectChain = serverObjectChain;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(this);
                    }
                }
            }
            return this._serverObjectChain;
        }

        [SecurityCritical]
        internal bool RemoveServerSideDynamicProperty(string name)
        {
            if (this._dphSrv == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_PropNotFound"));
            }
            return this._dphSrv.RemoveDynamicProperty(name);
        }

        [SecurityCritical]
        internal void ResetHandle()
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref lockTaken);
                this._srvIdentityHandle.Target = null;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
        }

        [SecurityCritical]
        internal void SetHandle()
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref lockTaken);
                if (!this._srvIdentityHandle.IsAllocated)
                {
                    this._srvIdentityHandle = new GCHandle(this, GCHandleType.Normal);
                }
                else
                {
                    this._srvIdentityHandle.Target = this;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
        }

        internal void SetLastCalledType(string newTypeName, Type newType)
        {
            LastCalledType type = new LastCalledType {
                typeName = newTypeName,
                type = newType
            };
            this._lastCalledType = type;
        }

        internal void SetSingleCallObjectMode()
        {
            base._flags |= 0x200;
        }

        internal void SetSingletonObjectMode()
        {
            base._flags |= 0x400;
        }

        internal bool MarshaledAsSpecificType
        {
            get
            {
                return this._bMarshaledAsSpecificType;
            }
            set
            {
                this._bMarshaledAsSpecificType = value;
            }
        }

        internal Context ServerContext
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._srvCtx;
            }
        }

        internal ArrayWithSize ServerSideDynamicSinks
        {
            [SecurityCritical]
            get
            {
                if (this._dphSrv == null)
                {
                    return null;
                }
                return this._dphSrv.DynamicSinks;
            }
        }

        internal Type ServerType
        {
            get
            {
                return this._srvType;
            }
            set
            {
                this._srvType = value;
            }
        }

        private class LastCalledType
        {
            public Type type;
            public string typeName;
        }
    }
}

