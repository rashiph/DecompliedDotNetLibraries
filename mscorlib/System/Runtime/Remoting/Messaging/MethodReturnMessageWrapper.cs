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
    public class MethodReturnMessageWrapper : InternalMessageWrapper, IMethodReturnMessage, IMethodMessage, IMessage
    {
        private ArgMapper _argMapper;
        private object[] _args;
        private System.Exception _exception;
        private IMethodReturnMessage _msg;
        private IDictionary _properties;
        private object _returnValue;

        public MethodReturnMessageWrapper(IMethodReturnMessage msg) : base(msg)
        {
            this._msg = msg;
            this._args = this._msg.Args;
            this._returnValue = this._msg.ReturnValue;
            this._exception = this._msg.Exception;
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
        public virtual object GetOutArg(int argNum)
        {
            if (this._argMapper == null)
            {
                this._argMapper = new ArgMapper(this, true);
            }
            return this._argMapper.GetArg(argNum);
        }

        [SecurityCritical]
        public virtual string GetOutArgName(int index)
        {
            if (this._argMapper == null)
            {
                this._argMapper = new ArgMapper(this, true);
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

        public virtual System.Exception Exception
        {
            [SecurityCritical]
            get
            {
                return this._exception;
            }
            set
            {
                this._exception = value;
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

        public virtual int OutArgCount
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

        public virtual object[] OutArgs
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

        public virtual IDictionary Properties
        {
            [SecurityCritical]
            get
            {
                if (this._properties == null)
                {
                    this._properties = new MRMWrapperDictionary(this, this._msg.Properties);
                }
                return this._properties;
            }
        }

        public virtual object ReturnValue
        {
            [SecurityCritical]
            get
            {
                return this._returnValue;
            }
            set
            {
                this._returnValue = value;
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

        public string Uri
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

        private class MRMWrapperDictionary : Hashtable
        {
            private IDictionary _idict;
            private IMethodReturnMessage _mrmsg;

            public MRMWrapperDictionary(IMethodReturnMessage msg, IDictionary idict)
            {
                this._mrmsg = msg;
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
                            return this._mrmsg.Uri;
                        }
                        if (str2 == "__MethodName")
                        {
                            return this._mrmsg.MethodName;
                        }
                        if (str2 == "__MethodSignature")
                        {
                            return this._mrmsg.MethodSignature;
                        }
                        if (str2 == "__TypeName")
                        {
                            return this._mrmsg.TypeName;
                        }
                        if (str2 == "__Return")
                        {
                            return this._mrmsg.ReturnValue;
                        }
                        if (str2 == "__OutArgs")
                        {
                            return this._mrmsg.OutArgs;
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
                        if (((str2 = str) != null) && (((str2 == "__MethodName") || (str2 == "__MethodSignature")) || (((str2 == "__TypeName") || (str2 == "__Return")) || (str2 == "__OutArgs"))))
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

