namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;

    internal class PeerDefaultCustomResolverClient : PeerResolver
    {
        private EndpointAddress address = null;
        private Binding binding = null;
        private string bindingConfigurationName;
        private string bindingName;
        private ChannelFactory<IPeerResolverClient> channelFactory;
        private Guid clientId = Guid.NewGuid();
        private ClientCredentials credentials;
        private TimeSpan defaultLifeTime = TimeSpan.FromHours(1.0);
        private string meshId;
        private PeerNodeAddress nodeAddress;
        private bool opened;
        private PeerReferralPolicy referralPolicy;
        private Guid registrationId;
        private bool? shareReferrals;
        private IOThreadTimer timer;
        private int updateSuccessful = 1;

        internal PeerDefaultCustomResolverClient()
        {
            this.timer = new IOThreadTimer(new Action<object>(this.RegistrationExpired), this, false);
        }

        public override bool Equals(object other)
        {
            PeerDefaultCustomResolverClient client = other as PeerDefaultCustomResolverClient;
            if (((client == null) || (this.referralPolicy != client.referralPolicy)) || !this.address.Equals(client.address))
            {
                return false;
            }
            if ((this.BindingName == null) && (this.BindingConfigurationName == null))
            {
                return this.binding.Equals(client.binding);
            }
            return ((this.BindingName == client.BindingName) && (this.BindingConfigurationName == client.BindingConfigurationName));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private IPeerResolverClient GetProxy()
        {
            return this.channelFactory.CreateChannel();
        }

        public override void Initialize(EndpointAddress address, Binding binding, ClientCredentials credentials, PeerReferralPolicy referralPolicy)
        {
            this.address = address;
            this.binding = binding;
            this.credentials = credentials;
            this.Validate();
            this.channelFactory = new ChannelFactory<IPeerResolverClient>(binding, address);
            this.channelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
            if (credentials != null)
            {
                this.channelFactory.Endpoint.Behaviors.Add(credentials);
            }
            this.channelFactory.Open();
            this.referralPolicy = referralPolicy;
            this.opened = true;
        }

        public override object Register(string meshId, PeerNodeAddress nodeAddress, TimeSpan timeout)
        {
            if (this.opened)
            {
                long scopeId = -1L;
                bool flag = false;
                if (nodeAddress.IPAddresses.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MustRegisterMoreThanZeroAddresses")));
                }
                foreach (IPAddress address in nodeAddress.IPAddresses)
                {
                    if (address.IsIPv6LinkLocal)
                    {
                        if (scopeId == -1L)
                        {
                            scopeId = address.ScopeId;
                        }
                        else if (scopeId != address.ScopeId)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                List<IPAddress> list = new List<IPAddress>();
                foreach (IPAddress address2 in nodeAddress.IPAddresses)
                {
                    if (!flag || (!address2.IsIPv6LinkLocal && !address2.IsIPv6SiteLocal))
                    {
                        list.Add(address2);
                    }
                }
                if (list.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("AmbiguousConnectivitySpec")));
                }
                ReadOnlyCollection<IPAddress> ipAddresses = new ReadOnlyCollection<IPAddress>(list);
                this.meshId = meshId;
                this.nodeAddress = new PeerNodeAddress(nodeAddress.EndpointAddress, ipAddresses);
                RegisterInfo registerInfo = new RegisterInfo(this.clientId, meshId, this.nodeAddress);
                IPeerResolverClient proxy = this.GetProxy();
                try
                {
                    proxy.OperationTimeout = timeout;
                    RegisterResponseInfo info2 = proxy.Register(registerInfo);
                    this.registrationId = info2.RegistrationId;
                    this.timer.Set(info2.RegistrationLifetime);
                    this.defaultLifeTime = info2.RegistrationLifetime;
                    proxy.Close();
                }
                finally
                {
                    proxy.Abort();
                }
            }
            return this.registrationId;
        }

        private void RegistrationExpired(object state)
        {
            if (this.opened)
            {
                try
                {
                    IPeerResolverClient proxy = this.GetProxy();
                    try
                    {
                        if (Interlocked.Exchange(ref this.updateSuccessful, 1) == 0)
                        {
                            this.SendUpdate(new UpdateInfo(this.registrationId, this.clientId, this.meshId, this.nodeAddress), ServiceDefaults.SendTimeout);
                        }
                        else
                        {
                            RefreshInfo refreshInfo = new RefreshInfo(this.meshId, this.registrationId);
                            if (proxy.Refresh(refreshInfo).Result == RefreshResult.RegistrationNotFound)
                            {
                                RegisterInfo registerInfo = new RegisterInfo(this.clientId, this.meshId, this.nodeAddress);
                                RegisterResponseInfo info4 = proxy.Register(registerInfo);
                                this.registrationId = info4.RegistrationId;
                                this.defaultLifeTime = info4.RegistrationLifetime;
                            }
                            proxy.Close();
                        }
                    }
                    finally
                    {
                        proxy.Abort();
                        this.timer.Set(this.defaultLifeTime);
                    }
                }
                catch (CommunicationException exception)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
        }

        public override ReadOnlyCollection<PeerNodeAddress> Resolve(string meshId, int maxAddresses, TimeSpan timeout)
        {
            ResolveResponseInfo info = null;
            IList<PeerNodeAddress> addresses = null;
            List<PeerNodeAddress> list2 = new List<PeerNodeAddress>();
            if (this.opened)
            {
                ResolveInfo resolveInfo = new ResolveInfo(this.clientId, meshId, maxAddresses);
                try
                {
                    IPeerResolverClient proxy = this.GetProxy();
                    try
                    {
                        proxy.OperationTimeout = timeout;
                        info = proxy.Resolve(resolveInfo);
                        proxy.Close();
                    }
                    finally
                    {
                        proxy.Abort();
                    }
                    if ((info != null) && (info.Addresses != null))
                    {
                        addresses = info.Addresses;
                    }
                }
                catch (CommunicationException exception)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    this.opened = false;
                    throw;
                }
            }
            if (addresses != null)
            {
                foreach (PeerNodeAddress address in addresses)
                {
                    bool flag = true;
                    long scopeId = -1L;
                    if (address != null)
                    {
                        foreach (IPAddress address2 in address.IPAddresses)
                        {
                            if (address2.IsIPv6LinkLocal)
                            {
                                if (scopeId == -1L)
                                {
                                    scopeId = address2.ScopeId;
                                }
                                else if (scopeId != address2.ScopeId)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            list2.Add(address);
                        }
                    }
                }
            }
            return new ReadOnlyCollection<PeerNodeAddress>(list2);
        }

        private void SendUpdate(UpdateInfo updateInfo, TimeSpan timeout)
        {
            try
            {
                IPeerResolverClient proxy = this.GetProxy();
                try
                {
                    proxy.OperationTimeout = timeout;
                    RegisterResponseInfo info = proxy.Update(updateInfo);
                    proxy.Close();
                    this.registrationId = info.RegistrationId;
                    this.defaultLifeTime = info.RegistrationLifetime;
                    Interlocked.Exchange(ref this.updateSuccessful, 1);
                    this.timer.Set(this.defaultLifeTime);
                }
                finally
                {
                    proxy.Abort();
                }
            }
            catch (CommunicationException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                Interlocked.Exchange(ref this.updateSuccessful, 0);
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                Interlocked.Exchange(ref this.updateSuccessful, 0);
                throw;
            }
        }

        public override void Unregister(object registrationId, TimeSpan timeout)
        {
            if (this.opened)
            {
                UnregisterInfo unregisterInfo = new UnregisterInfo(this.meshId, this.registrationId);
                try
                {
                    IPeerResolverClient proxy = this.GetProxy();
                    try
                    {
                        proxy.OperationTimeout = timeout;
                        proxy.Unregister(unregisterInfo);
                        proxy.Close();
                    }
                    finally
                    {
                        proxy.Abort();
                    }
                }
                catch (CommunicationException exception)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                finally
                {
                    this.opened = false;
                    this.timer.Cancel();
                }
            }
        }

        public override void Update(object registrationId, PeerNodeAddress updatedNodeAddress, TimeSpan timeout)
        {
            if (this.opened)
            {
                UpdateInfo updateInfo = new UpdateInfo(this.registrationId, this.clientId, this.meshId, updatedNodeAddress);
                this.nodeAddress = updatedNodeAddress;
                this.SendUpdate(updateInfo, timeout);
            }
        }

        private void Validate()
        {
            if ((this.address == null) || (this.binding == null))
            {
                PeerExceptionHelper.ThrowArgument_InsufficientResolverSettings();
            }
        }

        internal string BindingConfigurationName
        {
            get
            {
                return this.bindingName;
            }
            set
            {
                this.bindingConfigurationName = value;
            }
        }

        internal string BindingName
        {
            get
            {
                return this.bindingName;
            }
            set
            {
                this.bindingName = value;
            }
        }

        public override bool CanShareReferrals
        {
            get
            {
                if (!this.shareReferrals.HasValue)
                {
                    if ((this.referralPolicy == PeerReferralPolicy.Service) && this.opened)
                    {
                        IPeerResolverClient proxy = this.GetProxy();
                        try
                        {
                            ServiceSettingsResponseInfo serviceSettings = proxy.GetServiceSettings();
                            this.shareReferrals = new bool?(!serviceSettings.ControlMeshShape);
                            proxy.Close();
                        }
                        finally
                        {
                            proxy.Abort();
                        }
                    }
                    else
                    {
                        this.shareReferrals = new bool?(PeerReferralPolicy.Share == this.referralPolicy);
                    }
                }
                return this.shareReferrals.Value;
            }
        }
    }
}

