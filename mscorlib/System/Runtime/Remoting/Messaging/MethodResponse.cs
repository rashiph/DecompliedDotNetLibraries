namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, ComVisible(true), CLSCompliant(false), SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class MethodResponse : IMethodReturnMessage, IMethodMessage, IMessage, ISerializable, ISerializationRootObject, IInternalMessage
    {
        private RemotingMethodCachedData _methodCache;
        private int argCount;
        private ArgMapper argMapper;
        private System.Runtime.Remoting.Messaging.LogicalCallContext callContext;
        protected IDictionary ExternalProperties;
        private System.Exception fault;
        private bool fSoap;
        protected IDictionary InternalProperties;
        private string methodName;
        private Type[] methodSignature;
        private System.Reflection.MethodBase MI;
        private object[] outArgs;
        private object retVal;
        private string typeName;
        private string uri;

        [SecurityCritical]
        public MethodResponse(Header[] h1, IMethodCallMessage mcm)
        {
            if (mcm == null)
            {
                throw new ArgumentNullException("mcm");
            }
            Message message = mcm as Message;
            if (message != null)
            {
                this.MI = message.GetMethodBase();
            }
            else
            {
                this.MI = mcm.MethodBase;
            }
            if (this.MI == null)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_MethodMissing"), new object[] { mcm.MethodName, mcm.TypeName }));
            }
            this._methodCache = InternalRemotingServices.GetReflectionCachedData(this.MI);
            this.argCount = this._methodCache.Parameters.Length;
            this.fSoap = true;
            this.FillHeaders(h1);
        }

        [SecurityCritical]
        internal MethodResponse(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.SetObjectData(info, context);
        }

        [SecurityCritical]
        internal MethodResponse(IMethodCallMessage msg, object handlerObject, BinaryMethodReturnMessage smuggledMrm)
        {
            if (msg != null)
            {
                this.MI = msg.MethodBase;
                this._methodCache = InternalRemotingServices.GetReflectionCachedData(this.MI);
                this.methodName = msg.MethodName;
                this.uri = msg.Uri;
                this.typeName = msg.TypeName;
                if (this._methodCache.IsOverloaded())
                {
                    this.methodSignature = (Type[]) msg.MethodSignature;
                }
                this.argCount = this._methodCache.Parameters.Length;
            }
            this.retVal = smuggledMrm.ReturnValue;
            this.outArgs = smuggledMrm.Args;
            this.fault = smuggledMrm.Exception;
            this.callContext = smuggledMrm.LogicalCallContext;
            if (smuggledMrm.HasProperties)
            {
                smuggledMrm.PopulateMessageProperties(this.Properties);
            }
            this.fSoap = false;
        }

        [SecurityCritical]
        internal MethodResponse(IMethodCallMessage msg, SmuggledMethodReturnMessage smuggledMrm, ArrayList deserializedArgs)
        {
            this.MI = msg.MethodBase;
            this._methodCache = InternalRemotingServices.GetReflectionCachedData(this.MI);
            this.methodName = msg.MethodName;
            this.uri = msg.Uri;
            this.typeName = msg.TypeName;
            if (this._methodCache.IsOverloaded())
            {
                this.methodSignature = (Type[]) msg.MethodSignature;
            }
            this.retVal = smuggledMrm.GetReturnValue(deserializedArgs);
            this.outArgs = smuggledMrm.GetArgs(deserializedArgs);
            this.fault = smuggledMrm.GetException(deserializedArgs);
            this.callContext = smuggledMrm.GetCallContext(deserializedArgs);
            if (smuggledMrm.MessagePropertyCount > 0)
            {
                smuggledMrm.PopulateMessageProperties(this.Properties, deserializedArgs);
            }
            this.argCount = this._methodCache.Parameters.Length;
            this.fSoap = false;
        }

        [SecurityCritical]
        internal void FillHeader(string name, object value)
        {
            if (name.Equals("__MethodName"))
            {
                this.methodName = (string) value;
            }
            else if (name.Equals("__Uri"))
            {
                this.uri = (string) value;
            }
            else if (name.Equals("__MethodSignature"))
            {
                this.methodSignature = (Type[]) value;
            }
            else if (name.Equals("__TypeName"))
            {
                this.typeName = (string) value;
            }
            else if (name.Equals("__OutArgs"))
            {
                this.outArgs = (object[]) value;
            }
            else if (name.Equals("__CallContext"))
            {
                if (value is string)
                {
                    this.callContext = new System.Runtime.Remoting.Messaging.LogicalCallContext();
                    this.callContext.RemotingData.LogicalCallID = (string) value;
                }
                else
                {
                    this.callContext = (System.Runtime.Remoting.Messaging.LogicalCallContext) value;
                }
            }
            else if (name.Equals("__Return"))
            {
                this.retVal = value;
            }
            else
            {
                if (this.InternalProperties == null)
                {
                    this.InternalProperties = new Hashtable();
                }
                this.InternalProperties[name] = value;
            }
        }

        [SecurityCritical]
        internal void FillHeaders(Header[] h)
        {
            this.FillHeaders(h, false);
        }

        [SecurityCritical]
        private void FillHeaders(Header[] h, bool bFromHeaderHandler)
        {
            if (h != null)
            {
                if (bFromHeaderHandler && this.fSoap)
                {
                    for (int i = 0; i < h.Length; i++)
                    {
                        Header header = h[i];
                        if (header.HeaderNamespace == "http://schemas.microsoft.com/clr/soap/messageProperties")
                        {
                            this.FillHeader(header.Name, header.Value);
                        }
                        else
                        {
                            string propertyKeyForHeader = System.Runtime.Remoting.Messaging.LogicalCallContext.GetPropertyKeyForHeader(header);
                            this.FillHeader(propertyKeyForHeader, header);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < h.Length; j++)
                    {
                        this.FillHeader(h[j].Name, h[j].Value);
                    }
                }
            }
        }

        [SecurityCritical]
        public object GetArg(int argNum)
        {
            return this.outArgs[argNum];
        }

        [SecurityCritical]
        public string GetArgName(int index)
        {
            if (this.MI == null)
            {
                return ("__param" + index);
            }
            RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(this.MI);
            ParameterInfo[] parameters = reflectionCachedData.Parameters;
            if ((index < 0) || (index >= parameters.Length))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return reflectionCachedData.Parameters[index].Name;
        }

        [SecurityCritical]
        internal System.Runtime.Remoting.Messaging.LogicalCallContext GetLogicalCallContext()
        {
            if (this.callContext == null)
            {
                this.callContext = new System.Runtime.Remoting.Messaging.LogicalCallContext();
            }
            return this.callContext;
        }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
        }

        [SecurityCritical]
        public object GetOutArg(int argNum)
        {
            if (this.argMapper == null)
            {
                this.argMapper = new ArgMapper(this, true);
            }
            return this.argMapper.GetArg(argNum);
        }

        [SecurityCritical]
        public string GetOutArgName(int index)
        {
            if (this.argMapper == null)
            {
                this.argMapper = new ArgMapper(this, true);
            }
            return this.argMapper.GetArgName(index);
        }

        [SecurityCritical]
        public virtual object HeaderHandler(Header[] h)
        {
            SerializationMonkey uninitializedObject = (SerializationMonkey) FormatterServices.GetUninitializedObject(typeof(SerializationMonkey));
            Header[] destinationArray = null;
            if (((h != null) && (h.Length > 0)) && (h[0].Name == "__methodName"))
            {
                if (h.Length > 1)
                {
                    destinationArray = new Header[h.Length - 1];
                    Array.Copy(h, 1, destinationArray, 0, h.Length - 1);
                }
                else
                {
                    destinationArray = null;
                }
            }
            else
            {
                destinationArray = h;
            }
            Type returnType = null;
            MethodInfo mI = this.MI as MethodInfo;
            if (mI != null)
            {
                returnType = mI.ReturnType;
            }
            ParameterInfo[] parameters = this._methodCache.Parameters;
            int length = this._methodCache.MarshalResponseArgMap.Length;
            if ((returnType != null) && !(returnType == typeof(void)))
            {
                length++;
            }
            Type[] typeArray = new Type[length];
            string[] strArray = new string[length];
            int index = 0;
            if ((returnType != null) && !(returnType == typeof(void)))
            {
                typeArray[index++] = returnType;
            }
            foreach (int num3 in this._methodCache.MarshalResponseArgMap)
            {
                strArray[index] = parameters[num3].Name;
                if (parameters[num3].ParameterType.IsByRef)
                {
                    typeArray[index++] = parameters[num3].ParameterType.GetElementType();
                }
                else
                {
                    typeArray[index++] = parameters[num3].ParameterType;
                }
            }
            uninitializedObject.FieldTypes = typeArray;
            uninitializedObject.FieldNames = strArray;
            this.FillHeaders(destinationArray, true);
            uninitializedObject._obj = this;
            return uninitializedObject;
        }

        [SecurityCritical]
        public void RootSetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            this.SetObjectData(info, ctx);
        }

        internal System.Runtime.Remoting.Messaging.LogicalCallContext SetLogicalCallContext(System.Runtime.Remoting.Messaging.LogicalCallContext ctx)
        {
            System.Runtime.Remoting.Messaging.LogicalCallContext callContext = this.callContext;
            this.callContext = ctx;
            return callContext;
        }

        [SecurityCritical]
        internal void SetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            if (this.fSoap)
            {
                this.SetObjectFromSoapData(info);
            }
            else
            {
                SerializationInfoEnumerator enumerator = info.GetEnumerator();
                bool flag = false;
                bool flag2 = false;
                while (enumerator.MoveNext())
                {
                    if (enumerator.Name.Equals("__return"))
                    {
                        flag = true;
                        break;
                    }
                    if (enumerator.Name.Equals("__fault"))
                    {
                        flag2 = true;
                        this.fault = (System.Exception) enumerator.Value;
                        break;
                    }
                    this.FillHeader(enumerator.Name, enumerator.Value);
                }
                if (flag2 && flag)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadSerialization"));
                }
            }
        }

        internal void SetObjectFromSoapData(SerializationInfo info)
        {
            Hashtable keyToNamespaceTable = (Hashtable) info.GetValue("__keyToNamespaceTable", typeof(Hashtable));
            ArrayList list = (ArrayList) info.GetValue("__paramNameList", typeof(ArrayList));
            SoapFault fault = (SoapFault) info.GetValue("__fault", typeof(SoapFault));
            if (fault != null)
            {
                ServerFault detail = fault.Detail as ServerFault;
                if (detail != null)
                {
                    if (detail.Exception != null)
                    {
                        this.fault = detail.Exception;
                    }
                    else
                    {
                        Type type = Type.GetType(detail.ExceptionType, false, false);
                        if (type == null)
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.Append("\nException Type: ");
                            builder.Append(detail.ExceptionType);
                            builder.Append("\n");
                            builder.Append("Exception Message: ");
                            builder.Append(detail.ExceptionMessage);
                            builder.Append("\n");
                            builder.Append(detail.StackTrace);
                            this.fault = new ServerException(builder.ToString());
                        }
                        else
                        {
                            object[] args = new object[] { detail.ExceptionMessage };
                            this.fault = (System.Exception) Activator.CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, args, null, null);
                        }
                    }
                }
                else if (((fault.Detail != null) && (fault.Detail.GetType() == typeof(string))) && (((string) fault.Detail).Length != 0))
                {
                    this.fault = new ServerException((string) fault.Detail);
                }
                else
                {
                    this.fault = new ServerException(fault.FaultString);
                }
            }
            else
            {
                MethodInfo mI = this.MI as MethodInfo;
                int num = 0;
                if (mI != null)
                {
                    Type returnType = mI.ReturnType;
                    if (returnType != typeof(void))
                    {
                        num++;
                        object obj2 = info.GetValue((string) list[0], typeof(object));
                        if (obj2 is string)
                        {
                            this.retVal = Message.SoapCoerceArg(obj2, returnType, keyToNamespaceTable);
                        }
                        else
                        {
                            this.retVal = obj2;
                        }
                    }
                }
                ParameterInfo[] parameters = this._methodCache.Parameters;
                object obj3 = (this.InternalProperties == null) ? null : this.InternalProperties["__UnorderedParams"];
                if (((obj3 != null) && (obj3 is bool)) && ((bool) obj3))
                {
                    for (int i = num; i < list.Count; i++)
                    {
                        string name = (string) list[i];
                        int index = -1;
                        for (int j = 0; j < parameters.Length; j++)
                        {
                            if (name.Equals(parameters[j].Name))
                            {
                                index = parameters[j].Position;
                            }
                        }
                        if (index == -1)
                        {
                            if (!name.StartsWith("__param", StringComparison.Ordinal))
                            {
                                throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadSerialization"));
                            }
                            index = int.Parse(name.Substring(7), CultureInfo.InvariantCulture);
                        }
                        if (index >= this.argCount)
                        {
                            throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadSerialization"));
                        }
                        if (this.outArgs == null)
                        {
                            this.outArgs = new object[this.argCount];
                        }
                        this.outArgs[index] = Message.SoapCoerceArg(info.GetValue(name, typeof(object)), parameters[index].ParameterType, keyToNamespaceTable);
                    }
                }
                else
                {
                    if (this.argMapper == null)
                    {
                        this.argMapper = new ArgMapper(this, true);
                    }
                    for (int k = num; k < list.Count; k++)
                    {
                        string str2 = (string) list[k];
                        if (this.outArgs == null)
                        {
                            this.outArgs = new object[this.argCount];
                        }
                        int num6 = this.argMapper.Map[k - num];
                        this.outArgs[num6] = Message.SoapCoerceArg(info.GetValue(str2, typeof(object)), parameters[num6].ParameterType, keyToNamespaceTable);
                    }
                }
            }
        }

        [SecurityCritical]
        bool IInternalMessage.HasProperties()
        {
            if (this.ExternalProperties == null)
            {
                return (this.InternalProperties != null);
            }
            return true;
        }

        [SecurityCritical]
        void IInternalMessage.SetCallContext(System.Runtime.Remoting.Messaging.LogicalCallContext newCallContext)
        {
            this.callContext = newCallContext;
        }

        [SecurityCritical]
        void IInternalMessage.SetURI(string val)
        {
            this.uri = val;
        }

        public int ArgCount
        {
            [SecurityCritical]
            get
            {
                if (this.outArgs == null)
                {
                    return 0;
                }
                return this.outArgs.Length;
            }
        }

        public object[] Args
        {
            [SecurityCritical]
            get
            {
                return this.outArgs;
            }
        }

        public System.Exception Exception
        {
            [SecurityCritical]
            get
            {
                return this.fault;
            }
        }

        public bool HasVarArgs
        {
            [SecurityCritical]
            get
            {
                return false;
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
                return this.MI;
            }
        }

        public string MethodName
        {
            [SecurityCritical]
            get
            {
                return this.methodName;
            }
        }

        public object MethodSignature
        {
            [SecurityCritical]
            get
            {
                return this.methodSignature;
            }
        }

        public int OutArgCount
        {
            [SecurityCritical]
            get
            {
                if (this.argMapper == null)
                {
                    this.argMapper = new ArgMapper(this, true);
                }
                return this.argMapper.ArgCount;
            }
        }

        public object[] OutArgs
        {
            [SecurityCritical]
            get
            {
                if (this.argMapper == null)
                {
                    this.argMapper = new ArgMapper(this, true);
                }
                return this.argMapper.Args;
            }
        }

        public virtual IDictionary Properties
        {
            [SecurityCritical]
            get
            {
                lock (this)
                {
                    if (this.InternalProperties == null)
                    {
                        this.InternalProperties = new Hashtable();
                    }
                    if (this.ExternalProperties == null)
                    {
                        this.ExternalProperties = new MRMDictionary(this, this.InternalProperties);
                    }
                    return this.ExternalProperties;
                }
            }
        }

        public object ReturnValue
        {
            [SecurityCritical]
            get
            {
                return this.retVal;
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
                return this.typeName;
            }
        }

        public string Uri
        {
            [SecurityCritical]
            get
            {
                return this.uri;
            }
            set
            {
                this.uri = value;
            }
        }
    }
}

