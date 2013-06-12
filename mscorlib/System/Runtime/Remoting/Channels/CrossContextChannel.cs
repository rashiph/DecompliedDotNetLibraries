namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Threading;

    internal class CrossContextChannel : InternalSink, IMessageSink
    {
        private const int _channelCapability = 0;
        private const string _channelName = "XCTX";
        private const string _channelURI = "XCTX_URI";
        private static InternalCrossContextDelegate s_xctxDel = new InternalCrossContextDelegate(CrossContextChannel.SyncProcessMessageCallback);
        private static object staticSyncObject = new object();

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
        {
            IMessage msg = InternalSink.ValidateMessage(reqMsg);
            object[] args = new object[4];
            IMessageCtrl ctrl = null;
            if (msg != null)
            {
                if (replySink != null)
                {
                    replySink.SyncProcessMessage(msg);
                }
                return ctrl;
            }
            ServerIdentity serverIdentity = InternalSink.GetServerIdentity(reqMsg);
            if (RemotingServices.CORProfilerTrackRemotingAsync())
            {
                Guid empty = Guid.Empty;
                if (RemotingServices.CORProfilerTrackRemotingCookie())
                {
                    object obj2 = reqMsg.Properties["CORProfilerCookie"];
                    if (obj2 != null)
                    {
                        empty = (Guid) obj2;
                    }
                }
                RemotingServices.CORProfilerRemotingServerReceivingMessage(empty, true);
                if (replySink != null)
                {
                    IMessageSink sink = new ServerAsyncReplyTerminatorSink(replySink);
                    replySink = sink;
                }
            }
            Context serverContext = serverIdentity.ServerContext;
            if (serverContext.IsThreadPoolAware)
            {
                args[0] = reqMsg;
                args[1] = replySink;
                args[2] = Thread.CurrentContext;
                args[3] = serverContext;
                InternalCrossContextDelegate ftnToCall = new InternalCrossContextDelegate(CrossContextChannel.AsyncProcessMessageCallback);
                return (IMessageCtrl) Thread.CurrentThread.InternalCrossContextCallback(serverContext, ftnToCall, args);
            }
            AsyncWorkItem item = null;
            item = new AsyncWorkItem(reqMsg, replySink, Thread.CurrentContext, serverIdentity);
            WaitCallback callBack = new WaitCallback(item.FinishAsyncWork);
            ThreadPool.QueueUserWorkItem(callBack);
            return ctrl;
        }

        [SecurityCritical]
        internal static object AsyncProcessMessageCallback(object[] args)
        {
            AsyncWorkItem replySink = null;
            IMessage msg = (IMessage) args[0];
            IMessageSink sink = (IMessageSink) args[1];
            Context oldCtx = (Context) args[2];
            Context context2 = (Context) args[3];
            if (sink != null)
            {
                replySink = new AsyncWorkItem(sink, oldCtx);
            }
            context2.NotifyDynamicSinks(msg, false, true, true, true);
            return context2.GetServerContextChain().AsyncProcessMessage(msg, replySink);
        }

        [SecurityCritical]
        internal static IMessageCtrl DoAsyncDispatch(IMessage reqMsg, IMessageSink replySink)
        {
            object[] args = new object[4];
            ServerIdentity serverIdentity = InternalSink.GetServerIdentity(reqMsg);
            if (RemotingServices.CORProfilerTrackRemotingAsync())
            {
                Guid empty = Guid.Empty;
                if (RemotingServices.CORProfilerTrackRemotingCookie())
                {
                    object obj2 = reqMsg.Properties["CORProfilerCookie"];
                    if (obj2 != null)
                    {
                        empty = (Guid) obj2;
                    }
                }
                RemotingServices.CORProfilerRemotingServerReceivingMessage(empty, true);
                if (replySink != null)
                {
                    IMessageSink sink = new ServerAsyncReplyTerminatorSink(replySink);
                    replySink = sink;
                }
            }
            Context serverContext = serverIdentity.ServerContext;
            args[0] = reqMsg;
            args[1] = replySink;
            args[2] = Thread.CurrentContext;
            args[3] = serverContext;
            InternalCrossContextDelegate ftnToCall = new InternalCrossContextDelegate(CrossContextChannel.DoAsyncDispatchCallback);
            return (IMessageCtrl) Thread.CurrentThread.InternalCrossContextCallback(serverContext, ftnToCall, args);
        }

        [SecurityCritical]
        internal static object DoAsyncDispatchCallback(object[] args)
        {
            AsyncWorkItem replySink = null;
            IMessage msg = (IMessage) args[0];
            IMessageSink sink = (IMessageSink) args[1];
            Context oldCtx = (Context) args[2];
            Context context2 = (Context) args[3];
            if (sink != null)
            {
                replySink = new AsyncWorkItem(sink, oldCtx);
            }
            return context2.GetServerContextChain().AsyncProcessMessage(msg, replySink);
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage reqMsg)
        {
            object[] args = new object[2];
            IMessage message = null;
            try
            {
                IMessage message2 = InternalSink.ValidateMessage(reqMsg);
                if (message2 != null)
                {
                    return message2;
                }
                ServerIdentity serverIdentity = InternalSink.GetServerIdentity(reqMsg);
                args[0] = reqMsg;
                args[1] = serverIdentity.ServerContext;
                message = (IMessage) Thread.CurrentThread.InternalCrossContextCallback(serverIdentity.ServerContext, s_xctxDel, args);
            }
            catch (Exception exception)
            {
                message = new ReturnMessage(exception, (IMethodCallMessage) reqMsg);
                if (reqMsg != null)
                {
                    ((ReturnMessage) message).SetLogicalCallContext((LogicalCallContext) reqMsg.Properties[Message.CallContextKey]);
                }
            }
            return message;
        }

        [SecurityCritical]
        internal static object SyncProcessMessageCallback(object[] args)
        {
            IMessage msg = args[0] as IMessage;
            Context context = args[1] as Context;
            IMessage message2 = null;
            if (RemotingServices.CORProfilerTrackRemoting())
            {
                Guid empty = Guid.Empty;
                if (RemotingServices.CORProfilerTrackRemotingCookie())
                {
                    object obj2 = msg.Properties["CORProfilerCookie"];
                    if (obj2 != null)
                    {
                        empty = (Guid) obj2;
                    }
                }
                RemotingServices.CORProfilerRemotingServerReceivingMessage(empty, false);
            }
            context.NotifyDynamicSinks(msg, false, true, false, true);
            message2 = context.GetServerContextChain().SyncProcessMessage(msg);
            context.NotifyDynamicSinks(message2, false, false, false, true);
            if (RemotingServices.CORProfilerTrackRemoting())
            {
                Guid guid2;
                RemotingServices.CORProfilerRemotingServerSendingReply(out guid2, false);
                if (RemotingServices.CORProfilerTrackRemotingCookie())
                {
                    message2.Properties["CORProfilerCookie"] = guid2;
                }
            }
            return message2;
        }

        private static CrossContextChannel messageSink
        {
            get
            {
                return Thread.GetDomain().RemotingData.ChannelServicesData.xctxmessageSink;
            }
            set
            {
                Thread.GetDomain().RemotingData.ChannelServicesData.xctxmessageSink = value;
            }
        }

        internal static IMessageSink MessageSink
        {
            get
            {
                if (messageSink == null)
                {
                    CrossContextChannel channel = new CrossContextChannel();
                    lock (staticSyncObject)
                    {
                        if (messageSink == null)
                        {
                            messageSink = channel;
                        }
                    }
                }
                return messageSink;
            }
        }

        public IMessageSink NextSink
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }
    }
}

