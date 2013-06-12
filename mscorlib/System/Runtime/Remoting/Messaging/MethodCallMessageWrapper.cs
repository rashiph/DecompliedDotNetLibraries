namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Security.Permissions;

    [SecurityCritical, ComVisible(true), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class MethodCallMessageWrapper : InternalMessageWrapper, IMethodCallMessage, IMethodMessage, IMessage
    {
        private ArgMapper _argMapper;
        private object[] _args;
        private IMethodCallMessage _msg;
        private IDictionary _properties;

        public MethodCallMessageWrapper(IMethodCallMessage msg) : base(msg)
        {
            this._msg = msg;
            this._args = this._msg.Args;
        }

        [SecurityCritical]
        public virtual object GetArg(int argNum)
        {
            return this._args[argNum];
        }

        [SecurityCritical]
        public virtual string GetArgName(int index)
        {
            return this._msg.GetArgName(index);
        }

        [SecurityCritical]
        public virtual object GetInArg(int argNum)
        {
            if (this._argMapper == null)
            {
                this._argMapper = new ArgMapper(this, false);
            }
            return this._argMapper.GetArg(argNum);
        }

        [SecurityCritical]
        public virtual string GetInArgName(int index)
        {
            if (this._argMapper == null)
            {
                this._argMapper = new ArgMapper(this, false);
            }
            return this._argMapper.GetArgName(index);
        }

        public virtual int ArgCount
        {
            [SecurityCritical]
            get
            {
                if (this._args != null)
                {
                    return this._args.Length;
                }
                return 0;
            }
        }

        public virtual object[] Args
        {
            [SecurityCritical]
            get
            {
                return this._args;
            }
            set
            {
                this._args = value;
            }
        }

        public virtual bool HasVarArgs
        {
            [SecurityCritical]
            get
            {
                return this._msg.HasVarArgs;
            }
        }

        public virtual int InArgCount
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

        public virtual object[] InArgs
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

        public virtual System.Runtime.Remoting.Messaging.LogicalCallContext LogicalCallContext
        {
            [SecurityCritical]
            get
            {
                return this._msg.LogicalCallContext;
            }
        }

        public virtual System.Reflection.MethodBase MethodBase
        {
            [SecurityCritical]
            get
            {
                return this._msg.MethodBase;
            }
        }

        public virtual string MethodName
        {
            [SecurityCritical]
            get
            {
                return this._msg.MethodName;
            }
        }

        public virtual object MethodSignature
        {
            [SecurityCritical]
            get
            {
                return this._msg.MethodSignature;
            }
        }

        public virtual IDictionary Properties
        {
            [SecurityCritical]
            get
            {
                if (this._properties == null)
                {
                    this._properties = new MCMWrapperDictionary(this, this._msg.Properties);
                }
                return this._properties;
            }
        }

        public virtual string TypeName
        {
            [SecurityCritical]
            get
            {
                return this._msg.TypeName;
            }
        }

        public virtual string Uri
        {
            [SecurityCritical]
            get
            {
                return this._msg.Uri;
            }
            set
            {
                this._msg.Properties[Message.UriKey] = value;
            }
        }

        private class MCMWrapperDictionary : Hashtable
        {
            private IDictionary _idict;
            private IMethodCallMessage _mcmsg;

            public MCMWrapperDictionary(IMethodCallMessage msg, IDictionary idict)
            {
                this._mcmsg = msg;
                this._idict = idict;
            }

            public override object this[object key]
            {
                [SecuritySafeCritical]
                get
                {
                    string str2;
                    string str = key as string;
                    if ((str != null) && ((str2 = str) != null))
                    {
                        if (str2 == "__Uri")
                        {
                            return this._mcmsg.Uri;
                        }
                        if (str2 == "__MethodName")
                        {
                            return this._mcmsg.MethodName;
                        }
                        if (str2 == "__MethodSignature")
                        {
                            return this._mcmsg.MethodSignature;
                        }
                        if (str2 == "__TypeName")
                        {
                            return this._mcmsg.TypeName;
                        }
                        if (str2 == "__Args")
                        {
                            return this._mcmsg.Args;
                        }
                    }
                    return this._idict[key];
                }
                [SecuritySafeCritical]
                set
                {
                    string str = key as string;
                    if (str != null)
                    {
                        string str2;
                        if (((str2 = str) != null) && (((str2 == "__MethodName") || (str2 == "__MethodSignature")) || ((str2 == "__TypeName") || (str2 == "__Args"))))
                        {
                            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
                        }
                        this._idict[key] = value;
                    }
                }
            }
        }
    }
}

