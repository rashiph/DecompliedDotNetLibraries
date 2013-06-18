namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;

    public class BinaryClientFormatterSink : IClientFormatterSink, IMessageSink, IClientChannelSink, IChannelSinkBase
    {
        private SinkChannelProtocol _channelProtocol = SinkChannelProtocol.Other;
        private bool _includeVersioning = true;
        private IClientChannelSink _nextSink;
        private bool _strictBinding;

        public BinaryClientFormatterSink(IClientChannelSink nextSink)
        {
            this._nextSink = nextSink;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            IMethodCallMessage mcm = (IMethodCallMessage) msg;
            try
            {
                ITransportHeaders headers;
                Stream stream;
                this.SerializeMessage(msg, out headers, out stream);
                ClientChannelSinkStack sinkStack = new ClientChannelSinkStack(replySink);
                sinkStack.Push(this, msg);
                this._nextSink.AsyncProcessRequest(sinkStack, msg, headers, stream);
            }
            catch (Exception exception)
            {
                IMessage message2 = new ReturnMessage(exception, mcm);
                if (replySink != null)
                {
                    replySink.SyncProcessMessage(message2);
                }
            }
            return null;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            throw new NotSupportedException();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
        {
            IMethodCallMessage mcm = (IMethodCallMessage) state;
            IMessage msg = this.DeserializeMessage(mcm, headers, stream);
            sinkStack.DispatchReplyMessage(msg);
        }

        private IMessage DeserializeMessage(IMethodCallMessage mcm, ITransportHeaders headers, Stream stream)
        {
            IMessage message = CoreChannel.DeserializeBinaryResponseMessage(stream, mcm, this._strictBinding);
            stream.Close();
            return message;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
        {
            throw new NotSupportedException();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            throw new NotSupportedException();
        }

        private void SerializeMessage(IMessage msg, out ITransportHeaders headers, out Stream stream)
        {
            BaseTransportHeaders headers2 = new BaseTransportHeaders();
            headers = headers2;
            headers2.ContentType = "application/octet-stream";
            if (this._channelProtocol == SinkChannelProtocol.Http)
            {
                headers["__RequestVerb"] = "POST";
            }
            bool flag = false;
            stream = this._nextSink.GetRequestStream(msg, headers);
            if (stream == null)
            {
                stream = new ChunkedMemoryStream(CoreChannel.BufferPool);
                flag = true;
            }
            CoreChannel.SerializeBinaryMessage(msg, stream, this._includeVersioning);
            if (flag)
            {
                stream.Position = 0L;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public IMessage SyncProcessMessage(IMessage msg)
        {
            IMethodCallMessage mcm = msg as IMethodCallMessage;
            try
            {
                ITransportHeaders headers;
                Stream stream;
                Stream stream2;
                ITransportHeaders headers2;
                this.SerializeMessage(msg, out headers, out stream);
                this._nextSink.ProcessMessage(msg, headers, stream, out headers2, out stream2);
                if (headers2 == null)
                {
                    throw new ArgumentNullException("returnHeaders");
                }
                return this.DeserializeMessage(mcm, headers2, stream2);
            }
            catch (Exception exception)
            {
                return new ReturnMessage(exception, mcm);
            }
        }

        internal SinkChannelProtocol ChannelProtocol
        {
            set
            {
                this._channelProtocol = value;
            }
        }

        internal bool IncludeVersioning
        {
            set
            {
                this._includeVersioning = value;
            }
        }

        public IClientChannelSink NextChannelSink
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._nextSink;
            }
        }

        public IMessageSink NextSink
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                throw new NotSupportedException();
            }
        }

        public IDictionary Properties
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return null;
            }
        }

        internal bool StrictBinding
        {
            set
            {
                this._strictBinding = value;
            }
        }
    }
}

