namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Http;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;

    public class SdlChannelSink : IServerChannelSink, IChannelSinkBase
    {
        private bool _bMetadataEnabled;
        private bool _bRemoteApplicationMetadataEnabled;
        private IServerChannelSink _nextSink;
        private IChannelReceiver _receiver;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SdlChannelSink(IChannelReceiver receiver, IServerChannelSink nextSink)
        {
            this._receiver = receiver;
            this._nextSink = nextSink;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
        {
        }

        private void GenerateSdl(SdlType sdlType, IServerResponseChannelSinkStack sinkStack, ITransportHeaders requestHeaders, ITransportHeaders responseHeaders, out Stream outputStream)
        {
            if (!this.MetadataEnabled)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_MetadataNotEnabled"));
            }
            string uri = requestHeaders["__RequestUri"] as string;
            string objectUriFromRequestUri = HttpChannelHelper.GetObjectUriFromRequestUri(uri);
            if (!this.RemoteApplicationMetadataEnabled && (string.Compare(objectUriFromRequestUri, "RemoteApplicationMetadata.rem", StringComparison.OrdinalIgnoreCase) == 0))
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_RemoteApplicationMetadataNotEnabled"));
            }
            string hostName = (string) requestHeaders["Host"];
            if (hostName != null)
            {
                int index = hostName.IndexOf(':');
                if (index != -1)
                {
                    hostName = hostName.Substring(0, index);
                }
            }
            string channelUri = SetupUrlBashingForIisIfNecessary(hostName);
            ServiceType[] serviceTypes = null;
            if (string.Compare(objectUriFromRequestUri, "RemoteApplicationMetadata.rem", StringComparison.OrdinalIgnoreCase) == 0)
            {
                ActivatedServiceTypeEntry[] registeredActivatedServiceTypes = RemotingConfiguration.GetRegisteredActivatedServiceTypes();
                WellKnownServiceTypeEntry[] registeredWellKnownServiceTypes = RemotingConfiguration.GetRegisteredWellKnownServiceTypes();
                int num2 = 0;
                if (registeredActivatedServiceTypes != null)
                {
                    num2 += registeredActivatedServiceTypes.Length;
                }
                if (registeredWellKnownServiceTypes != null)
                {
                    num2 += registeredWellKnownServiceTypes.Length;
                }
                serviceTypes = new ServiceType[num2];
                int num3 = 0;
                if (registeredActivatedServiceTypes != null)
                {
                    foreach (ActivatedServiceTypeEntry entry in registeredActivatedServiceTypes)
                    {
                        serviceTypes[num3++] = new ServiceType(entry.ObjectType, null);
                    }
                }
                if (registeredWellKnownServiceTypes != null)
                {
                    foreach (WellKnownServiceTypeEntry entry2 in registeredWellKnownServiceTypes)
                    {
                        string url = this._receiver.GetUrlsForUri(entry2.ObjectUri)[0];
                        if (channelUri != null)
                        {
                            url = HttpChannelHelper.ReplaceChannelUriWithThisString(url, channelUri);
                        }
                        else if (hostName != null)
                        {
                            url = HttpChannelHelper.ReplaceMachineNameWithThisString(url, hostName);
                        }
                        serviceTypes[num3++] = new ServiceType(entry2.ObjectType, url);
                    }
                }
            }
            else
            {
                Type serverTypeForUri = RemotingServices.GetServerTypeForUri(objectUriFromRequestUri);
                if (serverTypeForUri == null)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, "Object with uri '{0}' does not exist at server.", new object[] { objectUriFromRequestUri }));
                }
                string str6 = this._receiver.GetUrlsForUri(objectUriFromRequestUri)[0];
                if (channelUri != null)
                {
                    str6 = HttpChannelHelper.ReplaceChannelUriWithThisString(str6, channelUri);
                }
                else if (hostName != null)
                {
                    str6 = HttpChannelHelper.ReplaceMachineNameWithThisString(str6, hostName);
                }
                serviceTypes = new ServiceType[] { new ServiceType(serverTypeForUri, str6) };
            }
            responseHeaders["Content-Type"] = "text/xml";
            bool flag = false;
            outputStream = sinkStack.GetResponseStream(null, responseHeaders);
            if (outputStream == null)
            {
                outputStream = new MemoryStream(0x400);
                flag = true;
            }
            MetaData.ConvertTypesToSchemaToStream(serviceTypes, sdlType, outputStream);
            if (flag)
            {
                outputStream.Position = 0L;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
        {
            throw new NotSupportedException();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            SdlType type;
            if (requestMsg != null)
            {
                return this._nextSink.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
            }
            if (!this.ShouldIntercept(requestHeaders, out type))
            {
                return this._nextSink.ProcessMessage(sinkStack, null, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
            }
            responseHeaders = new TransportHeaders();
            this.GenerateSdl(type, sinkStack, requestHeaders, responseHeaders, out responseStream);
            responseMsg = null;
            return ServerProcessing.Complete;
        }

        internal static string SetupUrlBashingForIisIfNecessary(string hostName)
        {
            string str = null;
            if (!CoreChannel.IsClientSKUInstallation)
            {
                str = SetupUrlBashingForIisIfNecessaryWorker(hostName);
            }
            return str;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string SetupUrlBashingForIisIfNecessaryWorker(string hostName)
        {
            string str = null;
            HttpContext current = HttpContext.Current;
            if (current == null)
            {
                return str;
            }
            HttpRequest request = current.Request;
            string str2 = null;
            if (request.IsSecureConnection)
            {
                str2 = "https";
            }
            else
            {
                str2 = "http";
            }
            int port = current.Request.Url.Port;
            StringBuilder builder = new StringBuilder(100);
            builder.Append(str2);
            builder.Append("://");
            if (hostName != null)
            {
                builder.Append(hostName);
            }
            else
            {
                builder.Append(CoreChannel.GetMachineName());
            }
            builder.Append(":");
            builder.Append(port.ToString(CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private bool ShouldIntercept(ITransportHeaders requestHeaders, out SdlType sdlType)
        {
            sdlType = SdlType.Sdl;
            string str = requestHeaders["__RequestVerb"] as string;
            string str2 = requestHeaders["__RequestUri"] as string;
            if (((str2 != null) && (str != null)) && str.Equals("GET"))
            {
                int startIndex = str2.LastIndexOf('?');
                if (startIndex == -1)
                {
                    return false;
                }
                string strA = str2.Substring(startIndex).ToLower(CultureInfo.InvariantCulture);
                if ((string.CompareOrdinal(strA, "?sdl") == 0) || (string.CompareOrdinal(strA, "?sdlx") == 0))
                {
                    sdlType = SdlType.Sdl;
                    return true;
                }
                if (string.CompareOrdinal(strA, "?wsdl") == 0)
                {
                    sdlType = SdlType.Wsdl;
                    return true;
                }
            }
            return false;
        }

        internal bool MetadataEnabled
        {
            get
            {
                return this._bMetadataEnabled;
            }
            set
            {
                this._bMetadataEnabled = value;
            }
        }

        public IServerChannelSink NextChannelSink
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._nextSink;
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

        internal bool RemoteApplicationMetadataEnabled
        {
            get
            {
                return this._bRemoteApplicationMetadataEnabled;
            }
            set
            {
                this._bRemoteApplicationMetadataEnabled = value;
            }
        }
    }
}

