namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [Serializable]
    internal sealed class BinaryMethodCallMessage
    {
        private object[] _args;
        private object[] _inargs;
        private Type[] _instArgs;
        [SecurityCritical]
        private System.Runtime.Remoting.Messaging.LogicalCallContext _logicalCallContext;
        private string _methodName;
        private object _methodSignature;
        private object[] _properties;
        private string _typeName;

        [SecurityCritical]
        internal BinaryMethodCallMessage(string uri, string methodName, string typeName, Type[] instArgs, object[] args, object methodSignature, System.Runtime.Remoting.Messaging.LogicalCallContext callContext, object[] properties)
        {
            this._methodName = methodName;
            this._typeName = typeName;
            if (args == null)
            {
                args = new object[0];
            }
            this._inargs = args;
            this._args = args;
            this._instArgs = instArgs;
            this._methodSignature = methodSignature;
            if (callContext == null)
            {
                this._logicalCallContext = new System.Runtime.Remoting.Messaging.LogicalCallContext();
            }
            else
            {
                this._logicalCallContext = callContext;
            }
            this._properties = properties;
        }

        internal void PopulateMessageProperties(IDictionary dict)
        {
            foreach (DictionaryEntry entry in this._properties)
            {
                dict[entry.Key] = entry.Value;
            }
        }

        public object[] Args
        {
            get
            {
                return this._args;
            }
        }

        public bool HasProperties
        {
            get
            {
                return (this._properties != null);
            }
        }

        public Type[] InstantiationArgs
        {
            get
            {
                return this._instArgs;
            }
        }

        public System.Runtime.Remoting.Messaging.LogicalCallContext LogicalCallContext
        {
            [SecurityCritical]
            get
            {
                return this._logicalCallContext;
            }
        }

        public string MethodName
        {
            get
            {
                return this._methodName;
            }
        }

        public object MethodSignature
        {
            get
            {
                return this._methodSignature;
            }
        }

        public string TypeName
        {
            get
            {
                return this._typeName;
            }
        }
    }
}

