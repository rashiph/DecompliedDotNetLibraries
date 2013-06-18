namespace System.ServiceModel.Activation
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class HostedTcpTransportManager : SharedTcpTransportManager
    {
        private Action<Uri> onViaCallback;
        private bool settingsApplied;

        public HostedTcpTransportManager(BaseUriWithWildcard baseAddress) : base(baseAddress.BaseAddress)
        {
            base.HostNameComparisonMode = baseAddress.HostNameComparisonMode;
            this.onViaCallback = new Action<Uri>(this.OnVia);
        }

        protected override Action<Uri> GetOnViaCallback()
        {
            return this.onViaCallback;
        }

        internal override void OnAbort()
        {
        }

        internal override void OnClose(TimeSpan timeout)
        {
        }

        internal override void OnOpen()
        {
        }

        protected override void OnSelecting(TcpChannelListener channelListener)
        {
            if (!this.settingsApplied)
            {
                lock (base.ThisLock)
                {
                    if (!this.settingsApplied)
                    {
                        base.ApplyListenerSettings(channelListener);
                        this.settingsApplied = true;
                    }
                }
            }
        }

        private void OnVia(Uri address)
        {
            ServiceHostingEnvironment.EnsureServiceAvailable(address.LocalPath);
        }

        internal void Start(int queueId, Guid token, Action messageReceivedCallback)
        {
            base.SetMessageReceivedCallback(messageReceivedCallback);
            base.OnOpenInternal(queueId, token);
        }

        internal void Stop(TimeSpan timeout)
        {
            base.CleanUp(false, timeout);
            this.settingsApplied = false;
        }
    }
}

