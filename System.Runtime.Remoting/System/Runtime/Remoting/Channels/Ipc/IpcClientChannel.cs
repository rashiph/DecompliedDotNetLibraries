namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;

    public class IpcClientChannel : IChannelSender, IChannel, ISecurableChannel
    {
        private string _channelName;
        private int _channelPriority;
        private IDictionary _prop;
        private bool _secure;
        private IClientChannelSinkProvider _sinkProvider;

        public IpcClientChannel()
        {
            this._channelPriority = 1;
            this._channelName = "ipc client";
            this.SetupChannel();
        }

        public IpcClientChannel(IDictionary properties, IClientChannelSinkProvider sinkProvider)
        {
            this._channelPriority = 1;
            this._channelName = "ipc client";
            if (properties != null)
            {
                this._prop = properties;
                foreach (DictionaryEntry entry in properties)
                {
                    string key = (string) entry.Key;
                    if (key != null)
                    {
                        if (!(key == "name"))
                        {
                            if (key == "priority")
                            {
                                goto Label_008A;
                            }
                            if (key == "secure")
                            {
                                goto Label_00A3;
                            }
                        }
                        else
                        {
                            this._channelName = (string) entry.Value;
                        }
                    }
                    continue;
                Label_008A:
                    this._channelPriority = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                    continue;
                Label_00A3:
                    this._secure = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                }
            }
            this._sinkProvider = sinkProvider;
            this.SetupChannel();
        }

        public IpcClientChannel(string name, IClientChannelSinkProvider sinkProvider)
        {
            this._channelPriority = 1;
            this._channelName = "ipc client";
            this._channelName = name;
            this._sinkProvider = sinkProvider;
            this.SetupChannel();
        }

        private IClientChannelSinkProvider CreateDefaultClientProviderChain()
        {
            IClientChannelSinkProvider provider = new BinaryClientFormatterSinkProvider();
            IClientChannelSinkProvider provider2 = provider;
            provider2.Next = new IpcClientTransportSinkProvider(this._prop);
            return provider;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public virtual IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
        {
            objectURI = null;
            string str = null;
            if (url != null)
            {
                str = this.Parse(url, out objectURI);
            }
            else if ((remoteChannelData != null) && (remoteChannelData is IChannelDataStore))
            {
                IChannelDataStore store = (IChannelDataStore) remoteChannelData;
                if (this.Parse(store.ChannelUris[0], out objectURI) != null)
                {
                    str = store.ChannelUris[0];
                }
            }
            if (str == null)
            {
                return null;
            }
            if (url == null)
            {
                url = str;
            }
            IClientChannelSink sink = this._sinkProvider.CreateSink(this, url, remoteChannelData);
            IMessageSink sink2 = sink as IMessageSink;
            if ((sink != null) && (sink2 == null))
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Channels_ChannelSinkNotMsgSink"));
            }
            return sink2;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public string Parse(string url, out string objectURI)
        {
            return IpcChannelHelper.ParseURL(url, out objectURI);
        }

        private void SetupChannel()
        {
            if (this._sinkProvider != null)
            {
                CoreChannel.AppendProviderToClientProviderChain(this._sinkProvider, new IpcClientTransportSinkProvider(this._prop));
            }
            else
            {
                this._sinkProvider = this.CreateDefaultClientProviderChain();
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

        public bool IsSecured
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._secure;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            set
            {
                this._secure = value;
            }
        }
    }
}

