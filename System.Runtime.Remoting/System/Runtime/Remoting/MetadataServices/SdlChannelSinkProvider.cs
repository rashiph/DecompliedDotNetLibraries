namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Remoting.Channels;
    using System.Security.Permissions;

    public class SdlChannelSinkProvider : IServerChannelSinkProvider
    {
        private bool _bMetadataEnabled;
        private bool _bRemoteApplicationMetadataEnabled;
        private IServerChannelSinkProvider _next;

        public SdlChannelSinkProvider()
        {
            this._bMetadataEnabled = true;
        }

        public SdlChannelSinkProvider(IDictionary properties, ICollection providerData)
        {
            this._bMetadataEnabled = true;
            if (properties != null)
            {
                foreach (DictionaryEntry entry in properties)
                {
                    string key = (string) entry.Key;
                    if (key == null)
                    {
                        goto Label_0089;
                    }
                    if (!(key == "remoteApplicationMetadataEnabled"))
                    {
                        if (key == "metadataEnabled")
                        {
                            goto Label_0070;
                        }
                        goto Label_0089;
                    }
                    this._bRemoteApplicationMetadataEnabled = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                    continue;
                Label_0070:
                    this._bMetadataEnabled = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                    continue;
                Label_0089:
                    CoreChannel.ReportUnknownProviderConfigProperty(base.GetType().Name, (string) entry.Key);
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            IServerChannelSink nextSink = null;
            if (this._next != null)
            {
                nextSink = this._next.CreateSink(channel);
            }
            return new SdlChannelSink(channel, nextSink) { RemoteApplicationMetadataEnabled = this._bRemoteApplicationMetadataEnabled, MetadataEnabled = this._bMetadataEnabled };
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void GetChannelData(IChannelDataStore localChannelData)
        {
        }

        public IServerChannelSinkProvider Next
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._next;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            set
            {
                this._next = value;
            }
        }
    }
}

