namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Principal;
    using System.Threading;

    internal class IpcServerTransportSink : IServerChannelSink, IChannelSinkBase
    {
        private bool _impersonate;
        private IServerChannelSink _nextSink;
        private bool _secure;
        private const int s_MaxSize = 0x2000000;

        public IpcServerTransportSink(IServerChannelSink nextSink, bool secure, bool impersonate)
        {
            this._nextSink = nextSink;
            this._secure = secure;
            this._impersonate = impersonate;
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            ((IpcServerHandler) state).SendResponse(headers, stream);
        }

        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            throw new NotSupportedException();
        }

        internal void ServiceRequest(object state)
        {
            IpcServerHandler handler = (IpcServerHandler) state;
            ITransportHeaders requestHeaders = handler.ReadHeaders();
            Stream requestStream = handler.GetRequestStream();
            requestHeaders["__CustomErrorsEnabled"] = false;
            ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();
            sinkStack.Push(this, handler);
            IMessage responseMsg = null;
            ITransportHeaders responseHeaders = null;
            Stream responseStream = null;
            WindowsIdentity current = null;
            IPrincipal currentPrincipal = null;
            bool flag = false;
            bool flag2 = false;
            ServerProcessing complete = ServerProcessing.Complete;
            try
            {
                if (this._secure)
                {
                    handler.Port.ImpersonateClient();
                    currentPrincipal = Thread.CurrentPrincipal;
                    flag2 = true;
                    flag = true;
                    current = WindowsIdentity.GetCurrent();
                    if (!this._impersonate)
                    {
                        NativePipe.RevertToSelf();
                        Thread.CurrentPrincipal = new GenericPrincipal(current, null);
                        flag = false;
                    }
                    else
                    {
                        if ((current.ImpersonationLevel != TokenImpersonationLevel.Impersonation) && (current.ImpersonationLevel != TokenImpersonationLevel.Delegation))
                        {
                            throw new RemotingException(CoreChannel.GetResourceString("Remoting_Ipc_TokenImpersonationFailure"));
                        }
                        Thread.CurrentPrincipal = new WindowsPrincipal(current);
                    }
                }
                complete = this._nextSink.ProcessMessage(sinkStack, null, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
            }
            catch (Exception exception)
            {
                handler.CloseOnFatalError(exception);
            }
            finally
            {
                if (flag2)
                {
                    Thread.CurrentPrincipal = currentPrincipal;
                }
                if (flag)
                {
                    NativePipe.RevertToSelf();
                    flag = false;
                }
            }
            switch (complete)
            {
                case ServerProcessing.Complete:
                    sinkStack.Pop(this);
                    handler.SendResponse(responseHeaders, responseStream);
                    break;

                case ServerProcessing.OneWay:
                    handler.SendResponse(responseHeaders, responseStream);
                    break;

                case ServerProcessing.Async:
                    sinkStack.StoreAndDispatch(this, handler);
                    break;
            }
            if (complete != ServerProcessing.Async)
            {
                handler.BeginReadMessage();
            }
        }

        internal bool IsSecured
        {
            get
            {
                return this._secure;
            }
            set
            {
                this._secure = value;
            }
        }

        public IServerChannelSink NextChannelSink
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._nextSink;
            }
        }

        public IDictionary Properties
        {
            get
            {
                return null;
            }
        }
    }
}

