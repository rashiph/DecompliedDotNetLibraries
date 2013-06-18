namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;

    public class HttpChannel : BaseChannelWithProperties, IChannelReceiver, IChannelSender, IChannel, IChannelReceiverHook, ISecurableChannel
    {
        private string _channelName;
        private int _channelPriority;
        private HttpClientChannel _clientChannel;
        private bool _secure;
        private HttpServerChannel _serverChannel;
        private static ICollection s_keySet;

        public HttpChannel()
        {
            this._channelPriority = 1;
            this._channelName = "http";
            this._clientChannel = new HttpClientChannel();
            this._serverChannel = new HttpServerChannel();
        }

        public HttpChannel(int port)
        {
            this._channelPriority = 1;
            this._channelName = "http";
            this._clientChannel = new HttpClientChannel();
            this._serverChannel = new HttpServerChannel(port);
        }

        public HttpChannel(IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider)
        {
            this._channelPriority = 1;
            this._channelName = "http";
            Hashtable hashtable = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            if (properties != null)
            {
                foreach (DictionaryEntry entry in properties)
                {
                    string key = (string) entry.Key;
                    if (key == null)
                    {
                        goto Label_00F4;
                    }
                    if (!(key == "name"))
                    {
                        if (key == "priority")
                        {
                            goto Label_0099;
                        }
                        if (key == "secure")
                        {
                            goto Label_00B7;
                        }
                        goto Label_00F4;
                    }
                    this._channelName = (string) entry.Value;
                    continue;
                Label_0099:
                    this._channelPriority = Convert.ToInt32((string) entry.Value, CultureInfo.InvariantCulture);
                    continue;
                Label_00B7:
                    this._secure = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                    hashtable["secure"] = entry.Value;
                    hashtable2["secure"] = entry.Value;
                    continue;
                Label_00F4:
                    hashtable[entry.Key] = entry.Value;
                    hashtable2[entry.Key] = entry.Value;
                }
            }
            this._clientChannel = new HttpClientChannel(hashtable, clientSinkProvider);
            this._serverChannel = new HttpServerChannel(hashtable2, serverSinkProvider);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void AddHookChannelUri(string channelUri)
        {
            this._serverChannel.AddHookChannelUri(channelUri);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
        {
            return this._clientChannel.CreateMessageSink(url, remoteChannelData, out objectURI);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public string[] GetUrlsForUri(string objectURI)
        {
            return this._serverChannel.GetUrlsForUri(objectURI);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public string Parse(string url, out string objectURI)
        {
            return HttpChannelHelper.ParseURL(url, out objectURI);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void StartListening(object data)
        {
            this._serverChannel.StartListening(data);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void StopListening(object data)
        {
            this._serverChannel.StopListening(data);
        }

        public object ChannelData
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._serverChannel.ChannelData;
            }
        }

        public string ChannelName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._channelName;
            }
        }

        public int ChannelPriority
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._channelPriority;
            }
        }

        public string ChannelScheme
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return "http";
            }
        }

        public IServerChannelSink ChannelSinkChain
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._serverChannel.ChannelSinkChain;
            }
        }

        public bool IsSecured
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                if (this._clientChannel != null)
                {
                    return this._clientChannel.IsSecured;
                }
                return ((this._serverChannel != null) && this._serverChannel.IsSecured);
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            set
            {
                if (ChannelServices.RegisteredChannels.Contains(this))
                {
                    throw new InvalidOperationException(CoreChannel.GetResourceString("Remoting_InvalidOperation_IsSecuredCannotBeChangedOnRegisteredChannels"));
                }
                if (this._clientChannel != null)
                {
                    this._clientChannel.IsSecured = value;
                }
                if (this._serverChannel != null)
                {
                    this._serverChannel.IsSecured = value;
                }
            }
        }

        public override object this[object key]
        {
            get
            {
                if (this._clientChannel.Contains(key))
                {
                    return this._clientChannel[key];
                }
                if (this._serverChannel.Contains(key))
                {
                    return this._serverChannel[key];
                }
                return null;
            }
            set
            {
                if (this._clientChannel.Contains(key))
                {
                    this._clientChannel[key] = value;
                }
                else if (this._serverChannel.Contains(key))
                {
                    this._serverChannel[key] = value;
                }
            }
        }

        public override ICollection Keys
        {
            get
            {
                if (s_keySet == null)
                {
                    ICollection keys = this._clientChannel.Keys;
                    ICollection is3 = this._serverChannel.Keys;
                    int capacity = keys.Count + is3.Count;
                    ArrayList list = new ArrayList(capacity);
                    foreach (object obj2 in keys)
                    {
                        list.Add(obj2);
                    }
                    foreach (object obj3 in is3)
                    {
                        list.Add(obj3);
                    }
                    s_keySet = list;
                }
                return s_keySet;
            }
        }

        public override IDictionary Properties
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                ArrayList dictionaries = new ArrayList(2);
                dictionaries.Add(this._clientChannel.Properties);
                dictionaries.Add(this._serverChannel.Properties);
                return new System.Runtime.Remoting.Channels.Http.AggregateDictionary(dictionaries);
            }
        }

        public bool WantsToListen
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._serverChannel.WantsToListen;
            }
            set
            {
                this._serverChannel.WantsToListen = value;
            }
        }
    }
}

