namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal class TransitionCall : IMessage, IInternalMessage, IMessageSink, ISerializable
    {
        private CrossContextDelegate _delegate;
        private IntPtr _eeData;
        private Identity _ID;
        private IDictionary _props;
        private IntPtr _sourceCtxID;
        private ServerIdentity _srvID;
        private IntPtr _targetCtxID;
        private int _targetDomainID;

        [SecurityCritical]
        internal TransitionCall(IntPtr targetCtxID, CrossContextDelegate deleg)
        {
            this._sourceCtxID = Thread.CurrentContext.InternalContextID;
            this._targetCtxID = targetCtxID;
            this._delegate = deleg;
            this._targetDomainID = 0;
            this._eeData = IntPtr.Zero;
            this._srvID = new ServerIdentity(null, Thread.GetContextInternal(this._targetCtxID));
            this._ID = this._srvID;
            this._ID.RaceSetChannelSink(CrossContextChannel.MessageSink);
            this._srvID.RaceSetServerObjectChain(this);
        }

        internal TransitionCall(SerializationInfo info, StreamingContext context)
        {
            if ((info == null) || (context.State != StreamingContextStates.CrossAppDomain))
            {
                throw new ArgumentNullException("info");
            }
            this._props = (IDictionary) info.GetValue("props", typeof(IDictionary));
            this._delegate = (CrossContextDelegate) info.GetValue("delegate", typeof(CrossContextDelegate));
            this._sourceCtxID = (IntPtr) info.GetValue("sourceCtxID", typeof(IntPtr));
            this._targetCtxID = (IntPtr) info.GetValue("targetCtxID", typeof(IntPtr));
            this._eeData = (IntPtr) info.GetValue("eeData", typeof(IntPtr));
            this._targetDomainID = info.GetInt32("targetDomainID");
        }

        [SecurityCritical]
        internal TransitionCall(IntPtr targetCtxID, IntPtr eeData, int targetDomainID)
        {
            string str;
            this._sourceCtxID = Thread.CurrentContext.InternalContextID;
            this._targetCtxID = targetCtxID;
            this._delegate = null;
            this._targetDomainID = targetDomainID;
            this._eeData = eeData;
            this._srvID = null;
            this._ID = new Identity("TransitionCallURI", null);
            CrossAppDomainData data = new CrossAppDomainData(this._targetCtxID, this._targetDomainID, Identity.ProcessGuid);
            IMessageSink channelSink = CrossAppDomainChannel.AppDomainChannel.CreateMessageSink(null, data, out str);
            this._ID.RaceSetChannelSink(channelSink);
        }

        [SecurityCritical]
        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            IMessage message = this.SyncProcessMessage(msg);
            replySink.SyncProcessMessage(message);
            return null;
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if ((info == null) || (context.State != StreamingContextStates.CrossAppDomain))
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("props", this._props, typeof(IDictionary));
            info.AddValue("delegate", this._delegate, typeof(CrossContextDelegate));
            info.AddValue("sourceCtxID", this._sourceCtxID);
            info.AddValue("targetCtxID", this._targetCtxID);
            info.AddValue("targetDomainID", this._targetDomainID);
            info.AddValue("eeData", this._eeData);
        }

        [SecurityCritical]
        public IMessage SyncProcessMessage(IMessage msg)
        {
            try
            {
                LogicalCallContext oldcctx = Message.PropagateCallContextFromMessageToThread(msg);
                if (this._delegate != null)
                {
                    this._delegate();
                }
                else
                {
                    CallBackHelper helper = new CallBackHelper(this._eeData, true, this._targetDomainID);
                    CrossContextDelegate delegate2 = new CrossContextDelegate(helper.Func);
                    delegate2();
                }
                Message.PropagateCallContextFromThreadToMessage(msg, oldcctx);
            }
            catch (Exception exception)
            {
                ReturnMessage message = new ReturnMessage(exception, new ErrorMessage());
                message.SetLogicalCallContext((LogicalCallContext) msg.Properties[Message.CallContextKey]);
                return message;
            }
            return this;
        }

        [SecurityCritical]
        bool IInternalMessage.HasProperties()
        {
            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
        }

        [SecurityCritical]
        void IInternalMessage.SetCallContext(LogicalCallContext callContext)
        {
            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
        }

        [SecurityCritical]
        void IInternalMessage.SetURI(string uri)
        {
            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
        }

        public IMessageSink NextSink
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }

        public IDictionary Properties
        {
            [SecurityCritical]
            get
            {
                if (this._props == null)
                {
                    lock (this)
                    {
                        if (this._props == null)
                        {
                            this._props = new Hashtable();
                        }
                    }
                }
                return this._props;
            }
        }

        Identity IInternalMessage.IdentityObject
        {
            [SecurityCritical]
            get
            {
                return this._ID;
            }
            [SecurityCritical]
            set
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
            }
        }

        ServerIdentity IInternalMessage.ServerIdentityObject
        {
            [SecurityCritical]
            get
            {
                if ((this._targetDomainID != 0) && (this._srvID == null))
                {
                    lock (this)
                    {
                        if (Thread.GetContextInternal(this._targetCtxID) == null)
                        {
                            Context defaultContext = Context.DefaultContext;
                        }
                        this._srvID = new ServerIdentity(null, Thread.GetContextInternal(this._targetCtxID));
                        this._srvID.RaceSetServerObjectChain(this);
                    }
                }
                return this._srvID;
            }
            [SecurityCritical]
            set
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
            }
        }
    }
}

