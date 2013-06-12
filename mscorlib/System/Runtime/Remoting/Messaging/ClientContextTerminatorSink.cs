namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Contexts;
    using System.Security;
    using System.Threading;

    internal class ClientContextTerminatorSink : InternalSink, IMessageSink
    {
        private static ClientContextTerminatorSink messageSink;
        private static object staticSyncObject = new object();

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
        {
            IMessage msg = InternalSink.ValidateMessage(reqMsg);
            IMessageCtrl ctrl = null;
            if (msg == null)
            {
                msg = InternalSink.DisallowAsyncActivation(reqMsg);
            }
            if (msg != null)
            {
                if (replySink != null)
                {
                    replySink.SyncProcessMessage(msg);
                }
                return ctrl;
            }
            if (RemotingServices.CORProfilerTrackRemotingAsync())
            {
                Guid guid;
                RemotingServices.CORProfilerRemotingClientSendingMessage(out guid, true);
                if (RemotingServices.CORProfilerTrackRemotingCookie())
                {
                    reqMsg.Properties["CORProfilerCookie"] = guid;
                }
                if (replySink != null)
                {
                    IMessageSink sink = new ClientAsyncReplyTerminatorSink(replySink);
                    replySink = sink;
                }
            }
            Context currentContext = Thread.CurrentContext;
            currentContext.NotifyDynamicSinks(reqMsg, true, true, true, true);
            if (replySink != null)
            {
                replySink = new System.Runtime.Remoting.Messaging.AsyncReplySink(replySink, currentContext);
            }
            object[] args = new object[3];
            InternalCrossContextDelegate ftnToCall = new InternalCrossContextDelegate(ClientContextTerminatorSink.AsyncProcessMessageCallback);
            IMessageSink channelSink = this.GetChannelSink(reqMsg);
            args[0] = reqMsg;
            args[1] = replySink;
            args[2] = channelSink;
            if (channelSink != CrossContextChannel.MessageSink)
            {
                return (IMessageCtrl) Thread.CurrentThread.InternalCrossContextCallback(Context.DefaultContext, ftnToCall, args);
            }
            return (IMessageCtrl) ftnToCall(args);
        }

        [SecurityCritical]
        internal static object AsyncProcessMessageCallback(object[] args)
        {
            IMessage msg = (IMessage) args[0];
            IMessageSink replySink = (IMessageSink) args[1];
            IMessageSink sink2 = (IMessageSink) args[2];
            return sink2.AsyncProcessMessage(msg, replySink);
        }

        [SecurityCritical]
        private IMessageSink GetChannelSink(IMessage reqMsg)
        {
            return InternalSink.GetIdentity(reqMsg).ChannelSink;
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage reqMsg)
        {
            IMessage message2;
            IMessage message = InternalSink.ValidateMessage(reqMsg);
            if (message != null)
            {
                return message;
            }
            Context currentContext = Thread.CurrentContext;
            bool flag = currentContext.NotifyDynamicSinks(reqMsg, true, true, false, true);
            if (reqMsg is IConstructionCallMessage)
            {
                message = currentContext.NotifyActivatorProperties(reqMsg, false);
                if (message != null)
                {
                    return message;
                }
                message2 = ((IConstructionCallMessage) reqMsg).Activator.Activate((IConstructionCallMessage) reqMsg);
                message = currentContext.NotifyActivatorProperties(message2, false);
                if (message != null)
                {
                    return message;
                }
            }
            else
            {
                message2 = null;
                ChannelServices.NotifyProfiler(reqMsg, RemotingProfilerEvent.ClientSend);
                object[] args = new object[2];
                IMessageSink channelSink = this.GetChannelSink(reqMsg);
                args[0] = reqMsg;
                args[1] = channelSink;
                InternalCrossContextDelegate ftnToCall = new InternalCrossContextDelegate(ClientContextTerminatorSink.SyncProcessMessageCallback);
                if (channelSink != CrossContextChannel.MessageSink)
                {
                    message2 = (IMessage) Thread.CurrentThread.InternalCrossContextCallback(Context.DefaultContext, ftnToCall, args);
                }
                else
                {
                    message2 = (IMessage) ftnToCall(args);
                }
                ChannelServices.NotifyProfiler(message2, RemotingProfilerEvent.ClientReceive);
            }
            if (flag)
            {
                currentContext.NotifyDynamicSinks(reqMsg, true, false, false, true);
            }
            return message2;
        }

        [SecurityCritical]
        internal static object SyncProcessMessageCallback(object[] args)
        {
            IMessage msg = (IMessage) args[0];
            IMessageSink sink = (IMessageSink) args[1];
            return sink.SyncProcessMessage(msg);
        }

        internal static IMessageSink MessageSink
        {
            get
            {
                if (messageSink == null)
                {
                    ClientContextTerminatorSink sink = new ClientContextTerminatorSink();
                    lock (staticSyncObject)
                    {
                        if (messageSink == null)
                        {
                            messageSink = sink;
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

