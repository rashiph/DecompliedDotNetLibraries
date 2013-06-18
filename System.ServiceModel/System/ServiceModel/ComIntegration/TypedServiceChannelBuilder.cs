namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Proxies;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;

    internal class TypedServiceChannelBuilder : IProxyCreator, IDisposable, IProvideChannelBuilderSettings, ICreateServiceChannel
    {
        private string address;
        private KeyedByTypeCollection<IEndpointBehavior> behaviors = new KeyedByTypeCollection<IEndpointBehavior>();
        private Binding binding;
        private string configurationName;
        private System.Type contractType;
        private bool dispatchEnabled;
        private EndpointIdentity identity;
        private ServiceChannelFactory serviceChannelFactory;
        private ServiceEndpoint serviceEndpoint;
        private RealProxy serviceProxy;

        internal TypedServiceChannelBuilder(Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            string str = null;
            string str2 = null;
            string str3 = null;
            string str4 = null;
            string str5 = null;
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Address, out this.address);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Binding, out str);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.BindingConfiguration, out str2);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.SpnIdentity, out str3);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.UpnIdentity, out str4);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.DnsIdentity, out str5);
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    this.binding = ConfigLoader.LookupBinding(str, str2);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("BindingLoadFromConfigFailedWith", new object[] { str, exception.Message })));
                }
                if (this.binding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("BindingNotFoundInConfig", new object[] { str, str2 })));
                }
            }
            if (this.binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("BindingNotSpecified")));
            }
            if (string.IsNullOrEmpty(this.address))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("AddressNotSpecified")));
            }
            if (!string.IsNullOrEmpty(str3))
            {
                if (!string.IsNullOrEmpty(str4) || !string.IsNullOrEmpty(str5))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentity")));
                }
                this.identity = EndpointIdentity.CreateSpnIdentity(str3);
            }
            else if (!string.IsNullOrEmpty(str4))
            {
                if (!string.IsNullOrEmpty(str3) || !string.IsNullOrEmpty(str5))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentity")));
                }
                this.identity = EndpointIdentity.CreateUpnIdentity(str4);
            }
            else if (!string.IsNullOrEmpty(str5))
            {
                if (!string.IsNullOrEmpty(str3) || !string.IsNullOrEmpty(str4))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentity")));
                }
                this.identity = EndpointIdentity.CreateDnsIdentity(str5);
            }
            else
            {
                this.identity = null;
            }
            this.ResolveTypeIfPossible(propertyTable);
        }

        private bool CheckDispatch(ref Guid riid)
        {
            return (this.dispatchEnabled && (riid == InterfaceID.idIDispatch));
        }

        private ServiceChannelFactory CreateServiceChannelFactory()
        {
            ServiceChannelFactory factory = ServiceChannelFactory.BuildChannelFactory(this.serviceEndpoint);
            if (factory == null)
            {
                throw Fx.AssertAndThrow("We should get a ServiceChannelFactory back");
            }
            return factory;
        }

        private ServiceEndpoint CreateServiceEndpoint()
        {
            TypeLoader loader = new TypeLoader();
            ServiceEndpoint serviceEndpoint = new ServiceEndpoint(loader.LoadContractDescription(this.contractType));
            if (this.address != null)
            {
                serviceEndpoint.Address = new EndpointAddress(new Uri(this.address), this.identity, new AddressHeader[0]);
            }
            if (this.binding != null)
            {
                serviceEndpoint.Binding = this.binding;
            }
            if (this.configurationName != null)
            {
                new ConfigLoader().LoadChannelBehaviors(serviceEndpoint, this.configurationName);
            }
            ComPlusTypedChannelBuilderTrace.Trace(TraceEventType.Verbose, 0x5001e, "TraceCodeComIntegrationTypedChannelBuilderLoaded", this.contractType, this.binding);
            return serviceEndpoint;
        }

        private void FaultInserviceChannelFactory()
        {
            if (this.contractType == null)
            {
                throw Fx.AssertAndThrow("contractType should not be null");
            }
            if (this.serviceEndpoint == null)
            {
                this.serviceEndpoint = this.CreateServiceEndpoint();
            }
            foreach (IEndpointBehavior behavior in this.behaviors)
            {
                this.serviceEndpoint.Behaviors.Add(behavior);
            }
            this.serviceChannelFactory = this.CreateServiceChannelFactory();
        }

        internal void ResolveTypeIfPossible(Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            string str;
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Contract, out str);
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    this.dispatchEnabled = true;
                    Guid riid = new Guid(str);
                    TypeCacheManager.Provider.FindOrCreateType(riid, out this.contractType, true, false);
                    this.serviceEndpoint = this.CreateServiceEndpoint();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("TypeLoadForContractTypeIIDFailedWith", new object[] { str, exception.Message })));
                }
            }
        }

        void IDisposable.Dispose()
        {
            if (this.serviceProxy != null)
            {
                IChannel transparentProxy = this.serviceProxy.GetTransparentProxy() as IChannel;
                if (transparentProxy == null)
                {
                    throw Fx.AssertAndThrow("serviceProxy MUST support IChannel");
                }
                transparentProxy.Close();
            }
        }

        RealProxy ICreateServiceChannel.CreateChannel()
        {
            if (this.serviceProxy == null)
            {
                lock (this)
                {
                    if (this.serviceProxy == null)
                    {
                        try
                        {
                            if (this.serviceChannelFactory == null)
                            {
                                this.FaultInserviceChannelFactory();
                            }
                            if (this.serviceChannelFactory == null)
                            {
                                throw Fx.AssertAndThrow("ServiceChannelFactory cannot be null at this point");
                            }
                            this.serviceChannelFactory.Open();
                            if (this.contractType == null)
                            {
                                throw Fx.AssertAndThrow("contractType cannot be null");
                            }
                            if (this.serviceEndpoint == null)
                            {
                                throw Fx.AssertAndThrow("serviceEndpoint cannot be null");
                            }
                            object obj2 = this.serviceChannelFactory.CreateChannel(this.contractType, new EndpointAddress(this.serviceEndpoint.Address.Uri, this.serviceEndpoint.Address.Identity, this.serviceEndpoint.Address.Headers), this.serviceEndpoint.Address.Uri);
                            ComPlusChannelCreatedTrace.Trace(TraceEventType.Verbose, 0x5001f, "TraceCodeComIntegrationChannelCreated", this.serviceEndpoint.Address.Uri, this.contractType);
                            RealProxy realProxy = RemotingServices.GetRealProxy(obj2);
                            Thread.MemoryBarrier();
                            this.serviceProxy = realProxy;
                            if (this.serviceProxy == null)
                            {
                                throw Fx.AssertAndThrow("serviceProxy MUST derive from RealProxy");
                            }
                        }
                        finally
                        {
                            if ((this.serviceProxy == null) && (this.serviceChannelFactory != null))
                            {
                                this.serviceChannelFactory.Close();
                            }
                        }
                    }
                }
            }
            return this.serviceProxy;
        }

        ComProxy IProxyCreator.CreateProxy(IntPtr outer, ref Guid riid)
        {
            ComProxy proxy3;
            if (outer == IntPtr.Zero)
            {
                throw Fx.AssertAndThrow("OuterProxy cannot be null");
            }
            if (this.contractType == null)
            {
                TypeCacheManager.Provider.FindOrCreateType(riid, out this.contractType, true, false);
            }
            if ((this.contractType.GUID != riid) && !this.CheckDispatch(ref riid))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCastException(System.ServiceModel.SR.GetString("NoInterface", new object[] { (Guid) riid })));
            }
            System.Type proxiedType = EmitterCache.TypeEmitter.FindOrCreateType(this.contractType);
            ComProxy proxy = null;
            TearOffProxy disp = null;
            try
            {
                disp = new TearOffProxy(this, proxiedType);
                proxy = ComProxy.Create(outer, disp.GetTransparentProxy(), disp);
                proxy3 = proxy;
            }
            finally
            {
                if ((proxy == null) && (disp != null))
                {
                    ((IDisposable) disp).Dispose();
                }
            }
            return proxy3;
        }

        bool IProxyCreator.SupportsDispatch()
        {
            return this.dispatchEnabled;
        }

        bool IProxyCreator.SupportsErrorInfo(ref Guid riid)
        {
            if (this.contractType == null)
            {
                return false;
            }
            if ((this.contractType.GUID != riid) && !this.CheckDispatch(ref riid))
            {
                return false;
            }
            return true;
        }

        bool IProxyCreator.SupportsIntrinsics()
        {
            return true;
        }

        KeyedByTypeCollection<IEndpointBehavior> IProvideChannelBuilderSettings.Behaviors
        {
            get
            {
                if (this.serviceProxy != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("TooLate"), HR.RPC_E_TOO_LATE));
                }
                return this.behaviors;
            }
        }

        ServiceChannel IProvideChannelBuilderSettings.ServiceChannel
        {
            get
            {
                return null;
            }
        }

        ServiceChannelFactory IProvideChannelBuilderSettings.ServiceChannelFactoryReadOnly
        {
            get
            {
                return this.serviceChannelFactory;
            }
        }

        ServiceChannelFactory IProvideChannelBuilderSettings.ServiceChannelFactoryReadWrite
        {
            get
            {
                if (this.serviceProxy != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("TooLate"), HR.RPC_E_TOO_LATE));
                }
                return this.serviceChannelFactory;
            }
        }
    }
}

