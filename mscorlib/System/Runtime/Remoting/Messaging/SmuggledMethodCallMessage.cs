namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Security;

    internal class SmuggledMethodCallMessage : MessageSmuggler
    {
        private object[] _args;
        private object _callContext;
        private MessageSmuggler.SerializedArg _instantiation;
        private string _methodName;
        private MessageSmuggler.SerializedArg _methodSignature;
        private int _propertyCount;
        private byte[] _serializedArgs;
        private string _typeName;
        private string _uri;

        private SmuggledMethodCallMessage()
        {
        }

        [SecurityCritical]
        private SmuggledMethodCallMessage(IMethodCallMessage mcm)
        {
            this._uri = mcm.Uri;
            this._methodName = mcm.MethodName;
            this._typeName = mcm.TypeName;
            ArrayList argsToSerialize = null;
            IInternalMessage message = mcm as IInternalMessage;
            if ((message == null) || message.HasProperties())
            {
                this._propertyCount = MessageSmuggler.StoreUserPropertiesForMethodMessage(mcm, ref argsToSerialize);
            }
            if (mcm.MethodBase.IsGenericMethod)
            {
                Type[] genericArguments = mcm.MethodBase.GetGenericArguments();
                if ((genericArguments != null) && (genericArguments.Length > 0))
                {
                    if (argsToSerialize == null)
                    {
                        argsToSerialize = new ArrayList();
                    }
                    this._instantiation = new MessageSmuggler.SerializedArg(argsToSerialize.Count);
                    argsToSerialize.Add(genericArguments);
                }
            }
            if (RemotingServices.IsMethodOverloaded(mcm))
            {
                if (argsToSerialize == null)
                {
                    argsToSerialize = new ArrayList();
                }
                this._methodSignature = new MessageSmuggler.SerializedArg(argsToSerialize.Count);
                argsToSerialize.Add(mcm.MethodSignature);
            }
            LogicalCallContext logicalCallContext = mcm.LogicalCallContext;
            if (logicalCallContext == null)
            {
                this._callContext = null;
            }
            else if (logicalCallContext.HasInfo)
            {
                if (argsToSerialize == null)
                {
                    argsToSerialize = new ArrayList();
                }
                this._callContext = new MessageSmuggler.SerializedArg(argsToSerialize.Count);
                argsToSerialize.Add(logicalCallContext);
            }
            else
            {
                this._callContext = logicalCallContext.RemotingData.LogicalCallID;
            }
            this._args = MessageSmuggler.FixupArgs(mcm.Args, ref argsToSerialize);
            if (argsToSerialize != null)
            {
                this._serializedArgs = CrossAppDomainSerializer.SerializeMessageParts(argsToSerialize).GetBuffer();
            }
        }

        [SecurityCritical]
        internal ArrayList FixupForNewAppDomain()
        {
            ArrayList list = null;
            if (this._serializedArgs != null)
            {
                list = CrossAppDomainSerializer.DeserializeMessageParts(new MemoryStream(this._serializedArgs));
                this._serializedArgs = null;
            }
            return list;
        }

        [SecurityCritical]
        internal object[] GetArgs(ArrayList deserializedArgs)
        {
            return MessageSmuggler.UndoFixupArgs(this._args, deserializedArgs);
        }

        [SecurityCritical]
        internal LogicalCallContext GetCallContext(ArrayList deserializedArgs)
        {
            if (this._callContext == null)
            {
                return null;
            }
            if (this._callContext is string)
            {
                return new LogicalCallContext { RemotingData = { LogicalCallID = (string) this._callContext } };
            }
            return (LogicalCallContext) deserializedArgs[((MessageSmuggler.SerializedArg) this._callContext).Index];
        }

        internal Type[] GetInstantiation(ArrayList deserializedArgs)
        {
            if (this._instantiation != null)
            {
                return (Type[]) deserializedArgs[this._instantiation.Index];
            }
            return null;
        }

        internal object[] GetMethodSignature(ArrayList deserializedArgs)
        {
            if (this._methodSignature != null)
            {
                return (object[]) deserializedArgs[this._methodSignature.Index];
            }
            return null;
        }

        internal void PopulateMessageProperties(IDictionary dict, ArrayList deserializedArgs)
        {
            for (int i = 0; i < this._propertyCount; i++)
            {
                DictionaryEntry entry = (DictionaryEntry) deserializedArgs[i];
                dict[entry.Key] = entry.Value;
            }
        }

        [SecurityCritical]
        internal static SmuggledMethodCallMessage SmuggleIfPossible(IMessage msg)
        {
            IMethodCallMessage mcm = msg as IMethodCallMessage;
            if (mcm == null)
            {
                return null;
            }
            return new SmuggledMethodCallMessage(mcm);
        }

        internal int MessagePropertyCount
        {
            get
            {
                return this._propertyCount;
            }
        }

        internal string MethodName
        {
            get
            {
                return this._methodName;
            }
        }

        internal string TypeName
        {
            get
            {
                return this._typeName;
            }
        }

        internal string Uri
        {
            get
            {
                return this._uri;
            }
        }
    }
}

