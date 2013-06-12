namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [SecurityCritical, ComVisible(true)]
    public class ClientChannelSinkStack : IClientChannelSinkStack, IClientResponseChannelSinkStack
    {
        private IMessageSink _replySink;
        private SinkStack _stack;

        public ClientChannelSinkStack()
        {
        }

        public ClientChannelSinkStack(IMessageSink replySink)
        {
            this._replySink = replySink;
        }

        [SecurityCritical]
        public void AsyncProcessResponse(ITransportHeaders headers, Stream stream)
        {
            if (this._replySink != null)
            {
                if (this._stack == null)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Channel_CantCallAPRWhenStackEmpty"));
                }
                IClientChannelSink sink = this._stack.Sink;
                object state = this._stack.State;
                this._stack = this._stack.PrevStack;
                sink.AsyncProcessResponse(this, state, headers, stream);
            }
        }

        [SecurityCritical]
        public void DispatchException(Exception e)
        {
            this.DispatchReplyMessage(new ReturnMessage(e, null));
        }

        [SecurityCritical]
        public void DispatchReplyMessage(IMessage msg)
        {
            if (this._replySink != null)
            {
                this._replySink.SyncProcessMessage(msg);
            }
        }

        [SecurityCritical]
        public object Pop(IClientChannelSink sink)
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
        public void Push(IClientChannelSink sink, object state)
        {
            SinkStack stack = new SinkStack {
                PrevStack = this._stack,
                Sink = sink,
                State = state
            };
            this._stack = stack;
        }

        private class SinkStack
        {
            public ClientChannelSinkStack.SinkStack PrevStack;
            public IClientChannelSink Sink;
            public object State;
        }
    }
}

