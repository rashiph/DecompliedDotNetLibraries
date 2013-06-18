namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Principal;

    internal class IpcClientTransportSink : BaseChannelSinkWithProperties, IClientChannelSink, IChannelSinkBase
    {
        private IpcClientChannel _channel;
        private string _portName;
        private int _timeout;
        private TokenImpersonationLevel _tokenImpersonationLevel;
        private bool authSet;
        private const string ConnectionTimeoutKey = "connectiontimeout";
        private ConnectionCache portCache;
        private static ICollection s_keySet;
        private const string TokenImpersonationLevelKey = "tokenimpersonationlevel";

        internal IpcClientTransportSink(string channelURI, IpcClientChannel channel)
        {
            string str;
            this.portCache = new ConnectionCache();
            this._tokenImpersonationLevel = TokenImpersonationLevel.Identification;
            this._timeout = 0x3e8;
            this._channel = channel;
            string str2 = IpcChannelHelper.ParseURL(channelURI, out str);
            int startIndex = str2.IndexOf("://") + 3;
            this._portName = str2.Substring(startIndex);
        }

        private void AsyncFinishedCallback(IAsyncResult ar)
        {
            IClientChannelSinkStack stack = null;
            try
            {
                ITransportHeaders headers;
                Stream stream;
                stack = ((AsyncMessageDelegate) ((AsyncResult) ar).AsyncDelegate).EndInvoke(out headers, out stream, ar);
                stack.AsyncProcessResponse(headers, stream);
            }
            catch (Exception exception)
            {
                try
                {
                    if (stack != null)
                    {
                        stack.DispatchException(exception);
                    }
                }
                catch
                {
                }
            }
        }

        private IClientChannelSinkStack AsyncProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream, IClientChannelSinkStack sinkStack)
        {
            this.ProcessMessage(msg, requestHeaders, requestStream, out responseHeaders, out responseStream);
            return sinkStack;
        }

        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream requestStream)
        {
            ITransportHeaders headers2;
            Stream stream;
            AsyncCallback callback = new AsyncCallback(this.AsyncFinishedCallback);
            new AsyncMessageDelegate(this.AsyncProcessMessage).BeginInvoke(msg, headers, requestStream, out headers2, out stream, sinkStack, callback, null);
        }

        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
        {
            throw new NotSupportedException();
        }

        public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            IpcPort port = null;
            Debugger.NotifyOfCrossThreadDependency();
            if (this.authSet && !this._channel.IsSecured)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Ipc_AuthenticationConfig"));
            }
            port = this.portCache.GetConnection(this._portName, this._channel.IsSecured, this._tokenImpersonationLevel, this._timeout);
            IMethodCallMessage message1 = (IMethodCallMessage) msg;
            long length = requestStream.Length;
            Stream stream = new PipeStream(port);
            IpcClientHandler handler = new IpcClientHandler(port, stream, this);
            handler.SendRequest(msg, requestHeaders, requestStream);
            responseHeaders = handler.ReadHeaders();
            responseStream = handler.GetResponseStream();
        }

        internal ConnectionCache Cache
        {
            get
            {
                return this.portCache;
            }
        }

        public override object this[object key]
        {
            get
            {
                string str = key as string;
                if (str != null)
                {
                    switch (str.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "tokenimpersonationlevel":
                            return this._tokenImpersonationLevel.ToString();

                        case "connectiontimeout":
                            return this._timeout;
                    }
                }
                return null;
            }
            set
            {
                string str2;
                string str = key as string;
                if ((str != null) && ((str2 = str.ToLower(CultureInfo.InvariantCulture)) != null))
                {
                    if (!(str2 == "tokenimpersonationlevel"))
                    {
                        if (str2 == "connectiontimeout")
                        {
                            this._timeout = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        this._tokenImpersonationLevel = (value is TokenImpersonationLevel) ? ((TokenImpersonationLevel) value) : ((TokenImpersonationLevel) Enum.Parse(typeof(TokenImpersonationLevel), (string) value, true));
                        this.authSet = true;
                    }
                }
            }
        }

        public override ICollection Keys
        {
            get
            {
                ICollection collection1 = s_keySet;
                return s_keySet;
            }
        }

        public IClientChannelSink NextChannelSink
        {
            get
            {
                return null;
            }
        }
    }
}

