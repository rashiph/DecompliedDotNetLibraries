namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.Remoting.Channels;
    using System.Security;

    internal class SmuggledMethodReturnMessage : MessageSmuggler
    {
        private object[] _args;
        private object _callContext;
        private MessageSmuggler.SerializedArg _exception;
        private int _propertyCount;
        private object _returnValue;
        private byte[] _serializedArgs;

        private SmuggledMethodReturnMessage()
        {
        }

        [SecurityCritical]
        private SmuggledMethodReturnMessage(IMethodReturnMessage mrm)
        {
            ArrayList argsToSerialize = null;
            ReturnMessage message = mrm as ReturnMessage;
            if ((message == null) || message.HasProperties())
            {
                this._propertyCount = MessageSmuggler.StoreUserPropertiesForMethodMessage(mrm, ref argsToSerialize);
            }
            Exception exception = mrm.Exception;
            if (exception != null)
            {
                if (argsToSerialize == null)
                {
                    argsToSerialize = new ArrayList();
                }
                this._exception = new MessageSmuggler.SerializedArg(argsToSerialize.Count);
                argsToSerialize.Add(exception);
            }
            LogicalCallContext logicalCallContext = mrm.LogicalCallContext;
            if (logicalCallContext == null)
            {
                this._callContext = null;
            }
            else if (logicalCallContext.HasInfo)
            {
                if (logicalCallContext.Principal != null)
                {
                    logicalCallContext.Principal = null;
                }
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
            this._returnValue = MessageSmuggler.FixupArg(mrm.ReturnValue, ref argsToSerialize);
            this._args = MessageSmuggler.FixupArgs(mrm.Args, ref argsToSerialize);
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

        internal Exception GetException(ArrayList deserializedArgs)
        {
            if (this._exception != null)
            {
                return (Exception) deserializedArgs[this._exception.Index];
            }
            return null;
        }

        [SecurityCritical]
        internal object GetReturnValue(ArrayList deserializedArgs)
        {
            return MessageSmuggler.UndoFixupArg(this._returnValue, deserializedArgs);
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
        internal static SmuggledMethodReturnMessage SmuggleIfPossible(IMessage msg)
        {
            IMethodReturnMessage mrm = msg as IMethodReturnMessage;
            if (mrm == null)
            {
                return null;
            }
            return new SmuggledMethodReturnMessage(mrm);
        }

        internal int MessagePropertyCount
        {
            get
            {
                return this._propertyCount;
            }
        }
    }
}

