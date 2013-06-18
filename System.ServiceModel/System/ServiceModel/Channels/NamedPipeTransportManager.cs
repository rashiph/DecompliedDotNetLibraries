namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.ServiceModel;

    internal abstract class NamedPipeTransportManager : ConnectionOrientedTransportManager<NamedPipeChannelListener>, ITransportManagerRegistration
    {
        private List<SecurityIdentifier> allowedUsers;
        private System.ServiceModel.HostNameComparisonMode hostNameComparisonMode;
        private Uri listenUri;

        protected NamedPipeTransportManager(Uri listenUri)
        {
            this.listenUri = listenUri;
        }

        private bool AreAllowedUsersEqual(List<SecurityIdentifier> otherAllowedUsers)
        {
            return ((this.allowedUsers == otherAllowedUsers) || (IsSubset(this.allowedUsers, otherAllowedUsers) && IsSubset(otherAllowedUsers, this.allowedUsers)));
        }

        private void Cleanup()
        {
            NamedPipeChannelListener.StaticTransportManagerTable.UnregisterUri(this.ListenUri, this.HostNameComparisonMode);
        }

        protected virtual bool IsCompatible(NamedPipeChannelListener channelListener)
        {
            return (channelListener.InheritBaseAddressSettings || ((base.IsCompatible(channelListener) && this.AreAllowedUsersEqual(channelListener.AllowedUsers)) && (this.HostNameComparisonMode == channelListener.HostNameComparisonMode)));
        }

        private static bool IsSubset(List<SecurityIdentifier> users1, List<SecurityIdentifier> users2)
        {
            if (users1 != null)
            {
                foreach (SecurityIdentifier identifier in users1)
                {
                    if (!users2.Contains(identifier))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal override void OnAbort()
        {
            this.Cleanup();
            base.OnAbort();
        }

        internal override void OnClose(TimeSpan timeout)
        {
            this.Cleanup();
        }

        protected virtual void OnSelecting(NamedPipeChannelListener channelListener)
        {
        }

        protected void SetAllowedUsers(List<SecurityIdentifier> allowedUsers)
        {
            this.allowedUsers = allowedUsers;
        }

        protected void SetHostNameComparisonMode(System.ServiceModel.HostNameComparisonMode hostNameComparisonMode)
        {
            this.hostNameComparisonMode = hostNameComparisonMode;
        }

        IList<TransportManager> ITransportManagerRegistration.Select(TransportChannelListener channelListener)
        {
            this.OnSelecting((NamedPipeChannelListener) channelListener);
            IList<TransportManager> list = null;
            if (this.IsCompatible((NamedPipeChannelListener) channelListener))
            {
                list = new List<TransportManager> {
                    this
                };
            }
            return list;
        }

        internal List<SecurityIdentifier> AllowedUsers
        {
            get
            {
                return this.allowedUsers;
            }
        }

        public System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }
            protected set
            {
                HostNameComparisonModeHelper.Validate(value);
                lock (base.ThisLock)
                {
                    base.ThrowIfOpen();
                    this.hostNameComparisonMode = value;
                }
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }

        internal override string Scheme
        {
            get
            {
                return Uri.UriSchemeNetPipe;
            }
        }
    }
}

