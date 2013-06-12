namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Security;

    internal class StackBasedReturnMessage : IMethodReturnMessage, IMethodMessage, IMessage, IInternalMessage
    {
        private ArgMapper _argMapper;
        private MRMDictionary _d;
        private Hashtable _h;
        private Message _m;

        internal StackBasedReturnMessage()
        {
        }

        [SecurityCritical]
        public object GetArg(int argNum)
        {
            return this._m.GetArg(argNum);
        }

        [SecurityCritical]
        public string GetArgName(int index)
        {
            return this._m.GetArgName(index);
        }

        [SecurityCritical]
        internal System.Runtime.Remoting.Messaging.LogicalCallContext GetLogicalCallContext()
        {
            return this._m.GetLogicalCallContext();
        }

        [SecurityCritical]
        public object GetOutArg(int argNum)
        {
            if (this._argMapper == null)
            {
                this._argMapper = new ArgMapper(this, true);
            }
            return this._argMapper.GetArg(argNum);
        }

        [SecurityCritical]
        public string GetOutArgName(int index)
        {
            if (this._argMapper == null)
            {
                this._argMapper = new ArgMapper(this, true);
            }
            return this._argMapper.GetArgName(index);
        }

        internal void InitFields(Message m)
        {
            this._m = m;
            if (this._h != null)
            {
                this._h.Clear();
            }
            if (this._d != null)
            {
                this._d.Clear();
            }
        }

        [SecurityCritical]
        internal System.Runtime.Remoting.Messaging.LogicalCallContext SetLogicalCallContext(System.Runtime.Remoting.Messaging.LogicalCallContext callCtx)
        {
            return this._m.SetLogicalCallContext(callCtx);
        }

        [SecurityCritical]
        bool IInternalMessage.HasProperties()
        {
            return (this._h != null);
        }

        [SecurityCritical]
        void IInternalMessage.SetCallContext(System.Runtime.Remoting.Messaging.LogicalCallContext newCallContext)
        {
            this._m.SetLogicalCallContext(newCallContext);
        }

        [SecurityCritical]
        void IInternalMessage.SetURI(string val)
        {
            this._m.Uri = val;
        }

        public int ArgCount
        {
            [SecurityCritical]
            get
            {
                return this._m.ArgCount;
            }
        }

        public object[] Args
        {
            [SecurityCritical]
            get
            {
                return this._m.Args;
            }
        }

        public System.Exception Exception
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }

        public bool HasVarArgs
        {
            [SecurityCritical]
            get
            {
                return this._m.HasVarArgs;
            }
        }

        public System.Runtime.Remoting.Messaging.LogicalCallContext LogicalCallContext
        {
            [SecurityCritical]
            get
            {
                return this._m.GetLogicalCallContext();
            }
        }

        public System.Reflection.MethodBase MethodBase
        {
            [SecurityCritical]
            get
            {
                return this._m.MethodBase;
            }
        }

        public string MethodName
        {
            [SecurityCritical]
            get
            {
                return this._m.MethodName;
            }
        }

        public object MethodSignature
        {
            [SecurityCritical]
            get
            {
                return this._m.MethodSignature;
            }
        }

        public int OutArgCount
        {
            [SecurityCritical]
            get
            {
                if (this._argMapper == null)
                {
                    this._argMapper = new ArgMapper(this, true);
                }
                return this._argMapper.ArgCount;
            }
        }

        public object[] OutArgs
        {
            [SecurityCritical]
            get
            {
                if (this._argMapper == null)
                {
                    this._argMapper = new ArgMapper(this, true);
                }
                return this._argMapper.Args;
            }
        }

        public IDictionary Properties
        {
            [SecurityCritical]
            get
            {
                lock (this)
                {
                    if (this._h == null)
                    {
                        this._h = new Hashtable();
                    }
                    if (this._d == null)
                    {
                        this._d = new MRMDictionary(this, this._h);
                    }
                    return this._d;
                }
            }
        }

        public object ReturnValue
        {
            [SecurityCritical]
            get
            {
                return this._m.GetReturnValue();
            }
        }

        Identity IInternalMessage.IdentityObject
        {
            [SecurityCritical]
            get
            {
                return null;
            }
            [SecurityCritical]
            set
            {
            }
        }

        ServerIdentity IInternalMessage.ServerIdentityObject
        {
            [SecurityCritical]
            get
            {
                return null;
            }
            [SecurityCritical]
            set
            {
            }
        }

        public string TypeName
        {
            [SecurityCritical]
            get
            {
                return this._m.TypeName;
            }
        }

        public string Uri
        {
            [SecurityCritical]
            get
            {
                return this._m.Uri;
            }
        }
    }
}

