namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;

    public class BinaryServerFormatterSinkProvider : IServerFormatterSinkProvider, IServerChannelSinkProvider
    {
        private System.Runtime.Serialization.Formatters.TypeFilterLevel _formatterSecurityLevel;
        private bool _includeVersioning;
        private IServerChannelSinkProvider _next;
        private bool _strictBinding;

        public BinaryServerFormatterSinkProvider()
        {
            this._includeVersioning = true;
            this._formatterSecurityLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Low;
        }

        public BinaryServerFormatterSinkProvider(IDictionary properties, ICollection providerData)
        {
            this._includeVersioning = true;
            this._formatterSecurityLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Low;
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
                                goto Label_0089;
                            }
                            if (str2 == "typeFilterLevel")
                            {
                                goto Label_00A2;
                            }
                        }
                        else
                        {
                            this._includeVersioning = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                        }
                    }
                    continue;
                Label_0089:
                    this._strictBinding = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                    continue;
                Label_00A2:
                    this._formatterSecurityLevel = (System.Runtime.Serialization.Formatters.TypeFilterLevel) Enum.Parse(typeof(System.Runtime.Serialization.Formatters.TypeFilterLevel), (string) entry.Value);
                }
            }
            CoreChannel.VerifyNoProviderData(base.GetType().Name, providerData);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            IServerChannelSink nextSink = null;
            if (this._next != null)
            {
                nextSink = this._next.CreateSink(channel);
            }
            BinaryServerFormatterSink.Protocol other = BinaryServerFormatterSink.Protocol.Other;
            string strB = channel.GetUrlsForUri("")[0];
            if (string.Compare("http", 0, strB, 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
            {
                other = BinaryServerFormatterSink.Protocol.Http;
            }
            return new BinaryServerFormatterSink(other, nextSink, channel) { TypeFilterLevel = this._formatterSecurityLevel, IncludeVersioning = this._includeVersioning, StrictBinding = this._strictBinding };
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void GetChannelData(IChannelDataStore channelData)
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

        [ComVisible(false)]
        public System.Runtime.Serialization.Formatters.TypeFilterLevel TypeFilterLevel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._formatterSecurityLevel;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._formatterSecurityLevel = value;
            }
        }
    }
}

