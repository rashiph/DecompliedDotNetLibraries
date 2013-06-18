namespace System.ServiceModel
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    public sealed class PeerNode : IOnlineStatus
    {
        private MessageEncodingBindingElement encoderElement;
        private PeerNodeImplementation innerNode;
        private SynchronizationContext synchronizationContext;

        public event EventHandler Offline;

        public event EventHandler Online;

        internal PeerNode(PeerNodeImplementation peerNode)
        {
            this.innerNode = peerNode;
        }

        private void FireEvent(EventHandler handler, object source, EventArgs args)
        {
            SendOrPostCallback d = null;
            if (handler != null)
            {
                try
                {
                    SynchronizationContext synchronizationContext = this.synchronizationContext;
                    if (synchronizationContext != null)
                    {
                        if (d == null)
                        {
                            d = state => handler(source, args);
                        }
                        synchronizationContext.Send(d, null);
                    }
                    else
                    {
                        handler(source, args);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(System.ServiceModel.SR.GetString("NotificationException"), exception);
                }
            }
        }

        internal void FireOffline(object source, EventArgs args)
        {
            this.FireEvent(this.Offline, source, args);
        }

        internal void FireOnline(object source, EventArgs args)
        {
            this.FireEvent(this.Online, source, args);
        }

        internal void OnClose()
        {
            this.innerNode.Offline -= new EventHandler(this.FireOffline);
            this.innerNode.Online -= new EventHandler(this.FireOnline);
            this.synchronizationContext = null;
        }

        internal void OnOpen()
        {
            this.synchronizationContext = ThreadBehavior.GetCurrentSynchronizationContext();
            this.innerNode.Offline += new EventHandler(this.FireOffline);
            this.innerNode.Online += new EventHandler(this.FireOnline);
            this.innerNode.EncodingElement = this.encoderElement;
        }

        public void RefreshConnection()
        {
            PeerNodeImplementation innerNode = this.InnerNode;
            if (innerNode != null)
            {
                innerNode.RefreshConnection();
            }
        }

        public override string ToString()
        {
            if (this.IsOpen)
            {
                return System.ServiceModel.SR.GetString("PeerNodeToStringFormat", new object[] { this.InnerNode.MeshId, this.InnerNode.NodeId, this.IsOnline, this.IsOpen, this.Port });
            }
            return System.ServiceModel.SR.GetString("PeerNodeToStringFormat", new object[] { "", -1, this.IsOnline, this.IsOpen, -1 });
        }

        private MessageEncodingBindingElement EncodingElement
        {
            get
            {
                return this.encoderElement;
            }
            set
            {
                this.encoderElement = value;
            }
        }

        internal PeerNodeImplementation InnerNode
        {
            get
            {
                return this.innerNode;
            }
        }

        public bool IsOnline
        {
            get
            {
                return this.InnerNode.IsOnline;
            }
        }

        internal bool IsOpen
        {
            get
            {
                return this.InnerNode.IsOpen;
            }
        }

        public PeerMessagePropagationFilter MessagePropagationFilter
        {
            get
            {
                return this.InnerNode.MessagePropagationFilter;
            }
            set
            {
                this.InnerNode.MessagePropagationFilter = value;
            }
        }

        public int Port
        {
            get
            {
                return this.InnerNode.ListenerPort;
            }
        }
    }
}

