namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;
    using System.Security.Permissions;

    public class SoapClientFormatterSinkProvider : IClientFormatterSinkProvider, IClientChannelSinkProvider
    {
        private bool _includeVersioning;
        private IClientChannelSinkProvider _next;
        private bool _strictBinding;

        public SoapClientFormatterSinkProvider()
        {
            this._includeVersioning = true;
        }

        public SoapClientFormatterSinkProvider(IDictionary properties, ICollection providerData)
        {
            this._includeVersioning = true;
            if (properties != null)
            {
                foreach (DictionaryEntry entry in properties)
                {
                    string str2 = entry.Key.ToString();
                    if (str2 != null)
                    {
                        if (!(str2 == "includeVersions"))
                        {
                            if (str2 == "strictBinding")
                            {
                                goto Label_006F;
                            }
                        }
                        else
                        {
                            this._includeVersioning = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                        }
                    }
                    continue;
                Label_006F:
                    this._strictBinding = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                }
            }
            CoreChannel.VerifyNoProviderData(base.GetType().Name, providerData);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
        {
            IClientChannelSink nextSink = null;
            if (this._next != null)
            {
                nextSink = this._next.CreateSink(channel, url, remoteChannelData);
                if (nextSink == null)
                {
                    return null;
                }
            }
            SinkChannelProtocol protocol = CoreChannel.DetermineChannelProtocol(channel);
            return new SoapClientFormatterSink(nextSink) { IncludeVersioning = this._includeVersioning, StrictBinding = this._strictBinding, ChannelProtocol = protocol };
        }

        public IClientChannelSinkProvider Next
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

