namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Security.AccessControl;
    using System.Security.Permissions;

    public class IpcChannel : IChannelReceiver, IChannelSender, IChannel, ISecurableChannel
    {
        private string _channelName;
        private int _channelPriority;
        private IpcClientChannel _clientChannel;
        private IpcServerChannel _serverChannel;

        public IpcChannel()
        {
            this._channelPriority = 20;
            this._channelName = "ipc";
            this._clientChannel = new IpcClientChannel();
        }

        public IpcChannel(string portName) : this()
        {
            this._serverChannel = new IpcServerChannel(portName);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IpcChannel(IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider) : this(properties, clientSinkProvider, serverSinkProvider, null)
        {
        }

        public IpcChannel(IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider, CommonSecurityDescriptor securityDescriptor)
        {
            this._channelPriority = 20;
            this._channelName = "ipc";
            Hashtable hashtable = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            bool flag = false;
            if (properties != null)
            {
                foreach (DictionaryEntry entry in properties)
                {
                    string key = (string) entry.Key;
                    if (key == null)
                    {
                        goto Label_00CC;
                    }
                    if (!(key == "name"))
                    {
                        if (key == "priority")
                        {
                            goto Label_0098;
                        }
                        if (key == "portName")
                        {
                            goto Label_00B6;
                        }
                        goto Label_00CC;
                    }
                    this._channelName = (string) entry.Value;
                    continue;
                Label_0098:
                    this._channelPriority = Convert.ToInt32((string) entry.Value, CultureInfo.InvariantCulture);
                    continue;
                Label_00B6:
                    hashtable2["portName"] = entry.Value;
                    flag = true;
                    continue;
                Label_00CC:
                    hashtable[entry.Key] = entry.Value;
                    hashtable2[entry.Key] = entry.Value;
                }
            }
            this._clientChannel = new IpcClientChannel(hashtable, clientSinkProvider);
            if (flag)
            {
                this._serverChannel = new IpcServerChannel(hashtable2, serverSinkProvider, securityDescriptor);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
        {
            return this._clientChannel.CreateMessageSink(url, remoteChannelData, out objectURI);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public string[] GetUrlsForUri(string objectURI)
        {
            if (this._serverChannel != null)
            {
                return this._serverChannel.GetUrlsForUri(objectURI);
            }
            return null;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public string Parse(string url, out string objectURI)
        {
            return IpcChannelHelper.ParseURL(url, out objectURI);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void StartListening(object data)
        {
            if (this._serverChannel != null)
            {
                this._serverChannel.StartListening(data);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void StopListening(object data)
        {
            if (this._serverChannel != null)
            {
                this._serverChannel.StopListening(data);
            }
        }

        public object ChannelData
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                if (this._serverChannel != null)
                {
                    return this._serverChannel.ChannelData;
                }
                return null;
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
    }
}

