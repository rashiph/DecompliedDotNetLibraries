namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;

    [Serializable]
    internal sealed class MethodMessage : IMethodMessage, IMessage, IMethodResponseMessage
    {
        [NonSerialized]
        private object[] args;
        private Guid callbackCookie;
        private LogicalCallContext callContext;
        private object[] clonedArgs;
        private System.Exception exception;
        [NonSerialized]
        private Type interfaceType;
        [NonSerialized]
        private string methodName;
        private ICollection outArgs;
        [NonSerialized]
        private MethodMessage previousMessage;
        [NonSerialized]
        private bool responseSet;
        [NonSerialized]
        private ManualResetEvent returnValueSignalEvent;
        private static LogicalCallContext singletonCallContext;
        private static Dictionary<Guid, MethodMessage> staticMethodMessageMap = new Dictionary<Guid, MethodMessage>();
        private static object syncObject = new object();
        private static object syncRoot = new object();

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal MethodMessage(Type interfaceType, string methodName, object[] args, string identity) : this(interfaceType, methodName, args, identity, false)
        {
        }

        internal MethodMessage(Type interfaceType, string methodName, object[] args, string identity, bool responseRequired)
        {
            this.interfaceType = interfaceType;
            this.methodName = methodName;
            this.args = args;
            this.callContext = GetLogicalCallContext();
            if (responseRequired)
            {
                this.returnValueSignalEvent = new ManualResetEvent(false);
            }
            this.PopulateIdentity(this.callContext, identity);
            this.Clone();
        }

        private object Clone()
        {
            object[] objArray = new object[this.args.Length];
            for (int i = 0; i < this.args.Length; i++)
            {
                objArray[i] = this.Clone(this.args[i]);
            }
            this.clonedArgs = objArray;
            return objArray;
        }

        private object Clone(object source)
        {
            if ((source == null) || source.GetType().IsValueType)
            {
                return source;
            }
            ICloneable cloneable = source as ICloneable;
            if (cloneable != null)
            {
                return cloneable.Clone();
            }
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream(0x400);
            try
            {
                formatter.Serialize(serializationStream, source);
            }
            catch (SerializationException exception)
            {
                throw new InvalidOperationException(SR.GetString("Error_EventArgumentSerializationException"), exception);
            }
            serializationStream.Position = 0L;
            return formatter.Deserialize(serializationStream);
        }

        private static LogicalCallContext GetLogicalCallContext()
        {
            lock (syncObject)
            {
                if (singletonCallContext == null)
                {
                    CallContextProxy proxy = new CallContextProxy(typeof(IDisposable));
                    ((IDisposable) proxy.GetTransparentProxy()).Dispose();
                    singletonCallContext = proxy.CallContext;
                }
                return (singletonCallContext.Clone() as LogicalCallContext);
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (this.callbackCookie != Guid.Empty)
            {
                lock (syncRoot)
                {
                    if (staticMethodMessageMap.TryGetValue(this.callbackCookie, out this.previousMessage))
                    {
                        staticMethodMessageMap.Remove(this.callbackCookie);
                    }
                }
                if (this.previousMessage != null)
                {
                    this.responseSet = this.previousMessage.responseSet;
                    this.returnValueSignalEvent = this.previousMessage.returnValueSignalEvent;
                }
            }
            this.callbackCookie = Guid.Empty;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if ((this.returnValueSignalEvent != null) && !this.responseSet)
            {
                this.callbackCookie = Guid.NewGuid();
                lock (syncRoot)
                {
                    staticMethodMessageMap.Add(this.callbackCookie, this.previousMessage ?? this);
                }
            }
        }

        private void PopulateIdentity(LogicalCallContext callContext, string identity)
        {
            callContext.SetData("__identitycontext__", new IdentityContextData(identity));
        }

        public void SendException(System.Exception exception)
        {
            if (this.returnValueSignalEvent == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_InstanceDehydratedBeforeSendingResponse"), new object[0]));
            }
            if (!this.responseSet)
            {
                this.Exception = exception;
                this.returnValueSignalEvent.Set();
                this.responseSet = true;
            }
        }

        public void SendResponse(ICollection outArgs)
        {
            if (this.returnValueSignalEvent == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_InstanceDehydratedBeforeSendingResponse"), new object[0]));
            }
            if (!this.responseSet)
            {
                this.OutArgs = outArgs;
                this.returnValueSignalEvent.Set();
                this.responseSet = true;
            }
        }

        object IMethodMessage.GetArg(int argNum)
        {
            return this.clonedArgs[argNum];
        }

        string IMethodMessage.GetArgName(int index)
        {
            throw new NotImplementedException();
        }

        internal IMethodResponseMessage WaitForResponseMessage()
        {
            this.returnValueSignalEvent.WaitOne();
            this.returnValueSignalEvent = null;
            return this;
        }

        public System.Exception Exception
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.exception;
            }
            private set
            {
                if (this.previousMessage != null)
                {
                    this.previousMessage.Exception = value;
                }
                this.exception = value;
            }
        }

        public ICollection OutArgs
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.outArgs;
            }
            private set
            {
                if (this.previousMessage != null)
                {
                    this.previousMessage.OutArgs = value;
                }
                this.outArgs = value;
            }
        }

        IDictionary IMessage.Properties
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        int IMethodMessage.ArgCount
        {
            get
            {
                return this.clonedArgs.Length;
            }
        }

        object[] IMethodMessage.Args
        {
            get
            {
                return this.clonedArgs;
            }
        }

        bool IMethodMessage.HasVarArgs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        LogicalCallContext IMethodMessage.LogicalCallContext
        {
            get
            {
                return this.callContext;
            }
        }

        MethodBase IMethodMessage.MethodBase
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        string IMethodMessage.MethodName
        {
            get
            {
                return this.methodName;
            }
        }

        object IMethodMessage.MethodSignature
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        string IMethodMessage.TypeName
        {
            get
            {
                return this.interfaceType.ToString();
            }
        }

        string IMethodMessage.Uri
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private sealed class CallContextProxy : RealProxy
        {
            private LogicalCallContext callContext;

            internal CallContextProxy(Type proxiedType) : base(proxiedType)
            {
            }

            public override IMessage Invoke(IMessage msg)
            {
                IMethodCallMessage mcm = msg as IMethodCallMessage;
                this.callContext = mcm.LogicalCallContext.Clone() as LogicalCallContext;
                return new ReturnMessage(null, null, 0, mcm.LogicalCallContext, mcm);
            }

            internal LogicalCallContext CallContext
            {
                get
                {
                    return this.callContext;
                }
            }
        }
    }
}

