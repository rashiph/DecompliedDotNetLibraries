namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal class CrossAppDomainChannel : IChannelSender, IChannelReceiver, IChannel
    {
        private const string _channelName = "XAPPDMN";
        private const string _channelURI = "XAPPDMN_URI";
        private static PermissionSet s_fullTrust = new PermissionSet(PermissionState.Unrestricted);
        private static object staticSyncObject = new object();

        [SecurityCritical]
        public virtual IMessageSink CreateMessageSink(string url, object data, out string objectURI)
        {
            objectURI = null;
            IMessageSink sink = null;
            if ((url != null) && (data == null))
            {
                if (url.StartsWith("XAPPDMN", StringComparison.Ordinal))
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_AppDomains_NYI"));
                }
                return sink;
            }
            CrossAppDomainData xadData = data as CrossAppDomainData;
            if ((xadData != null) && xadData.ProcessGuid.Equals(Identity.ProcessGuid))
            {
                sink = CrossAppDomainSink.FindOrCreateSink(xadData);
            }
            return sink;
        }

        [SecurityCritical]
        public virtual string[] GetUrlsForUri(string objectURI)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
        }

        [SecurityCritical]
        public string Parse(string url, out string objectURI)
        {
            objectURI = url;
            return null;
        }

        [SecurityCritical]
        internal static void RegisterChannel()
        {
            ChannelServices.RegisterChannelInternal(AppDomainChannel, false);
        }

        [SecurityCritical]
        public virtual void StartListening(object data)
        {
        }

        [SecurityCritical]
        public virtual void StopListening(object data)
        {
        }

        internal static CrossAppDomainChannel AppDomainChannel
        {
            get
            {
                if (gAppDomainChannel == null)
                {
                    CrossAppDomainChannel channel = new CrossAppDomainChannel();
                    lock (staticSyncObject)
                    {
                        if (gAppDomainChannel == null)
                        {
                            gAppDomainChannel = channel;
                        }
                    }
                }
                return gAppDomainChannel;
            }
        }

        public virtual object ChannelData
        {
            [SecurityCritical]
            get
            {
                return new CrossAppDomainData(Context.DefaultContext.InternalContextID, Thread.GetDomain().GetId(), Identity.ProcessGuid);
            }
        }

        public virtual string ChannelName
        {
            [SecurityCritical]
            get
            {
                return "XAPPDMN";
            }
        }

        public virtual int ChannelPriority
        {
            [SecurityCritical]
            get
            {
                return 100;
            }
        }

        public virtual string ChannelURI
        {
            get
            {
                return "XAPPDMN_URI";
            }
        }

        private static CrossAppDomainChannel gAppDomainChannel
        {
            get
            {
                return Thread.GetDomain().RemotingData.ChannelServicesData.xadmessageSink;
            }
            set
            {
                Thread.GetDomain().RemotingData.ChannelServicesData.xadmessageSink = value;
            }
        }
    }
}

