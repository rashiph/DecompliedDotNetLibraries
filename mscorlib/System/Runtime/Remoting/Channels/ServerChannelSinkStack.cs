namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Security;

    [SecurityCritical, ComVisible(true)]
    public class ServerChannelSinkStack : IServerChannelSinkStack, IServerResponseChannelSinkStack
    {
        private MethodInfo _asyncEnd;
        private IMessage _asyncMsg;
        private IMethodCallMessage _msg;
        private SinkStack _rememberedStack;
        private object _serverObject;
        private SinkStack _stack;

        [SecurityCritical]
        public void AsyncProcessResponse(IMessage msg, ITransportHeaders headers, Stream stream)
        {
            if (this._stack == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Channel_CantCallAPRWhenStackEmpty"));
            }
            IServerChannelSink sink = this._stack.Sink;
            object state = this._stack.State;
            this._stack = this._stack.PrevStack;
            sink.AsyncProcessResponse(this, state, msg, headers, stream);
        }

        private void FlipRememberedStack()
        {
            if (this._stack != null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Channel_CantCallFRSWhenStackEmtpy"));
            }
            while (this._rememberedStack != null)
            {
                SinkStack stack = new SinkStack {
                    PrevStack = this._stack,
                    Sink = this._rememberedStack.Sink,
                    State = this._rememberedStack.State
                };
                this._stack = stack;
                this._rememberedStack = this._rememberedStack.PrevStack;
            }
        }

        [SecurityCritical]
        public Stream GetResponseStream(IMessage msg, ITransportHeaders headers)
        {
            if (this._stack == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Channel_CantCallGetResponseStreamWhenStackEmpty"));
            }
            IServerChannelSink sink = this._stack.Sink;
            object state = this._stack.State;
            this._stack = this._stack.PrevStack;
            Stream stream = sink.GetResponseStream(this, state, msg, headers);
            this.Push(sink, state);
            return stream;
        }

        [SecurityCritical]
        public object Pop(IServerChannelSink sink)
        {
            if (this._stack == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Channel_PopOnEmptySinkStack"));
            }
        Label_0018:
            if (this._stack.Sink != sink)
            {
                this._stack = this._stack.PrevStack;
                if (this._stack != null)
                {
                    goto Label_0018;
                }
            }
            if (this._stack.Sink == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Channel_PopFromSinkStackWithoutPush"));
            }
            object state = this._stack.State;
            this._stack = this._stack.PrevStack;
            return state;
        }

        [SecurityCritical]
        public void Push(IServerChannelSink sink, object state)
        {
            SinkStack stack = new SinkStack {
                PrevStack = this._stack,
                Sink = sink,
                State = state
            };
            this._stack = stack;
        }

        [SecurityCritical]
        public void ServerCallback(IAsyncResult ar)
        {
            if (this._asyncEnd != null)
            {
                object[] objArray3;
                RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData((MethodBase) this._asyncEnd);
                MethodInfo methodBase = (MethodInfo) this._msg.MethodBase;
                RemotingMethodCachedData syncMethod = InternalRemotingServices.GetReflectionCachedData((MethodBase) methodBase);
                ParameterInfo[] parameters = reflectionCachedData.Parameters;
                object[] endArgs = new object[parameters.Length];
                endArgs[parameters.Length - 1] = ar;
                object[] args = this._msg.Args;
                AsyncMessageHelper.GetOutArgs(syncMethod.Parameters, args, endArgs);
                StackBuilderSink sink = new StackBuilderSink(this._serverObject);
                object ret = sink.PrivateProcessMessage(this._asyncEnd.MethodHandle, Message.CoerceArgs(this._asyncEnd, endArgs, parameters), this._serverObject, 0, false, out objArray3);
                if (objArray3 != null)
                {
                    objArray3 = ArgMapper.ExpandAsyncEndArgsToSyncArgs(syncMethod, objArray3);
                }
                sink.CopyNonByrefOutArgsFromOriginalArgs(syncMethod, args, ref objArray3);
                IMessage msg = new ReturnMessage(ret, objArray3, this._msg.ArgCount, CallContext.GetLogicalCallContext(), this._msg);
                this.AsyncProcessResponse(msg, null, null);
            }
        }

        [SecurityCritical]
        public void Store(IServerChannelSink sink, object state)
        {
            if (this._stack == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Channel_StoreOnEmptySinkStack"));
            }
        Label_0018:
            if (this._stack.Sink != sink)
            {
                this._stack = this._stack.PrevStack;
                if (this._stack != null)
                {
                    goto Label_0018;
                }
            }
            if (this._stack.Sink == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Channel_StoreOnSinkStackWithoutPush"));
            }
            SinkStack stack = new SinkStack {
                PrevStack = this._rememberedStack,
                Sink = sink,
                State = state
            };
            this._rememberedStack = stack;
            this.Pop(sink);
        }

        [SecurityCritical]
        public void StoreAndDispatch(IServerChannelSink sink, object state)
        {
            this.Store(sink, state);
            this.FlipRememberedStack();
            CrossContextChannel.DoAsyncDispatch(this._asyncMsg, null);
        }

        internal object ServerObject
        {
            set
            {
                this._serverObject = value;
            }
        }

        private class SinkStack
        {
            public ServerChannelSinkStack.SinkStack PrevStack;
            public IServerChannelSink Sink;
            public object State;
        }
    }
}

