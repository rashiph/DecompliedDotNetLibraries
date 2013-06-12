namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Channels;
    using System.Security;

    internal class DelayLoadClientChannelEntry
    {
        private bool _bRegistered;
        private IChannelSender _channel;
        private bool _ensureSecurity;
        private RemotingXmlConfigFileData.ChannelEntry _entry;

        internal DelayLoadClientChannelEntry(RemotingXmlConfigFileData.ChannelEntry entry, bool ensureSecurity)
        {
            this._entry = entry;
            this._channel = null;
            this._bRegistered = false;
            this._ensureSecurity = ensureSecurity;
        }

        internal void RegisterChannel()
        {
            ChannelServices.RegisterChannel(this._channel, this._ensureSecurity);
            this._bRegistered = true;
            this._channel = null;
        }

        internal IChannelSender Channel
        {
            [SecurityCritical]
            get
            {
                if ((this._channel == null) && !this._bRegistered)
                {
                    this._channel = (IChannelSender) RemotingConfigHandler.CreateChannelFromConfigEntry(this._entry);
                    this._entry = null;
                }
                return this._channel;
            }
        }
    }
}

