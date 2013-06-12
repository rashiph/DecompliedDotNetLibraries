namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Contexts;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal class ServerContextTerminatorSink : InternalSink, IMessageSink
    {
        private static ServerContextTerminatorSink messageSink;
        private static object staticSyncObject = new object();

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
        {
            IMessageCtrl ctrl = null;
            MarshalByRefObject obj2;
            IDisposable disposable;
            IMessage msg = InternalSink.ValidateMessage(reqMsg);
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
            IMessageSink objectChain = this.GetObjectChain(reqMsg, out obj2);
            if ((obj2 != null) && ((disposable = obj2 as IDisposable) != null))
            {
                DisposeSink sink2 = new DisposeSink(disposable, replySink);
                replySink = sink2;
            }
            return objectChain.AsyncProcessMessage(reqMsg, replySink);
        }

        [SecurityCritical]
        internal virtual IMessageSink GetObjectChain(IMessage reqMsg, out MarshalByRefObject obj)
        {
            return InternalSink.GetServerIdentity(reqMsg).GetServerObjectChain(out obj);
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
            if (reqMsg is IConstructionCallMessage)
            {
                message = currentContext.NotifyActivatorProperties(reqMsg, true);
                if (message == null)
                {
                    message2 = ((IConstructionCallMessage) reqMsg).Activator.Activate((IConstructionCallMessage) reqMsg);
                    message = currentContext.NotifyActivatorProperties(message2, true);
                    if (message == null)
                    {
                        return message2;
                    }
                }
                return message;
            }
            MarshalByRefObject obj2 = null;
            try
            {
                message2 = this.GetObjectChain(reqMsg, out obj2).SyncProcessMessage(reqMsg);
            }
            finally
            {
                IDisposable disposable = null;
                if ((obj2 != null) && ((disposable = obj2 as IDisposable) != null))
                {
                    disposable.Dispose();
                }
            }
            return message2;
        }

        internal static IMessageSink MessageSink
        {
            get
            {
                if (messageSink == null)
                {
                    ServerContextTerminatorSink sink = new ServerContextTerminatorSink();
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

