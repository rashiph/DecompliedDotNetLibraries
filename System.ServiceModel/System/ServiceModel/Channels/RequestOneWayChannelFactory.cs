namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal class RequestOneWayChannelFactory : LayeredChannelFactory<IOutputChannel>
    {
        private PacketRoutableHeader packetRoutableHeader;

        public RequestOneWayChannelFactory(OneWayBindingElement bindingElement, BindingContext context) : base(context.Binding, context.BuildInnerChannelFactory<IRequestChannel>())
        {
            if (bindingElement.PacketRoutable)
            {
                this.packetRoutableHeader = PacketRoutableHeader.Create();
            }
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            return new RequestOutputChannel(this, ((IChannelFactory<IRequestChannel>) base.InnerChannelFactory).CreateChannel(to, via), this.packetRoutableHeader);
        }

        private class RequestOutputChannel : OutputChannel
        {
            private IRequestChannel innerChannel;
            private MessageHeader packetRoutableHeader;

            public RequestOutputChannel(ChannelManagerBase factory, IRequestChannel innerChannel, MessageHeader packetRoutableHeader) : base(factory)
            {
                this.innerChannel = innerChannel;
                this.packetRoutableHeader = packetRoutableHeader;
            }

            protected override void AddHeadersTo(Message message)
            {
                base.AddHeadersTo(message);
                if (this.packetRoutableHeader != null)
                {
                    PacketRoutableHeader.AddHeadersTo(message, this.packetRoutableHeader);
                }
            }

            public override T GetProperty<T>() where T: class
            {
                T property = base.GetProperty<T>();
                if (property == null)
                {
                    property = this.innerChannel.GetProperty<T>();
                }
                return property;
            }

            protected override void OnAbort()
            {
                this.innerChannel.Abort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginClose(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginOpen(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginRequest(message, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.innerChannel.Close(timeout);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                this.innerChannel.EndClose(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                this.innerChannel.EndOpen(result);
            }

            protected override void OnEndSend(IAsyncResult result)
            {
                Message response = this.innerChannel.EndRequest(result);
                using (response)
                {
                    this.ValidateResponse(response);
                }
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.innerChannel.Open(timeout);
            }

            protected override void OnSend(Message message, TimeSpan timeout)
            {
                Message response = this.innerChannel.Request(message, timeout);
                using (response)
                {
                    this.ValidateResponse(response);
                }
            }

            private void ValidateResponse(Message response)
            {
                if (response != null)
                {
                    if ((response.Version == MessageVersion.None) && (response is NullMessage))
                    {
                        response.Close();
                    }
                    else
                    {
                        Exception innerException = null;
                        if (response.IsFault)
                        {
                            try
                            {
                                innerException = new FaultException(MessageFault.CreateFault(response, 0x10000));
                            }
                            catch (Exception exception2)
                            {
                                if (Fx.IsFatal(exception2))
                                {
                                    throw;
                                }
                                if ((!(exception2 is CommunicationException) && !(exception2 is TimeoutException)) && (!(exception2 is XmlException) && !(exception2 is IOException)))
                                {
                                    throw;
                                }
                                innerException = exception2;
                            }
                        }
                        throw TraceUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("OneWayUnexpectedResponse"), innerException), response);
                    }
                }
            }

            public override EndpointAddress RemoteAddress
            {
                get
                {
                    return this.innerChannel.RemoteAddress;
                }
            }

            public override Uri Via
            {
                get
                {
                    return this.innerChannel.Via;
                }
            }
        }
    }
}

