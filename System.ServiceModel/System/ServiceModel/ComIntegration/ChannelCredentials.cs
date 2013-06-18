namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    internal class ChannelCredentials : IChannelCredentials, IDisposable
    {
        protected IProvideChannelBuilderSettings channelBuilderSettings;

        internal ChannelCredentials(IProvideChannelBuilderSettings channelBuilderSettings)
        {
            this.channelBuilderSettings = channelBuilderSettings;
        }

        internal static ComProxy Create(IntPtr outer, IProvideChannelBuilderSettings channelBuilderSettings)
        {
            ComProxy proxy2;
            if (channelBuilderSettings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotCreateChannelOption")));
            }
            ChannelCredentials credentials = null;
            ComProxy proxy = null;
            try
            {
                credentials = new ChannelCredentials(channelBuilderSettings);
                proxy = ComProxy.Create(outer, credentials, credentials);
                proxy2 = proxy;
            }
            finally
            {
                if ((proxy == null) && (credentials != null))
                {
                    ((IDisposable) credentials).Dispose();
                }
            }
            return proxy2;
        }

        void IDisposable.Dispose()
        {
        }

        void IChannelCredentials.SetClientCertificateFromFile(string fileName, string password, string keyStorageFlags)
        {
            lock (this.channelBuilderSettings)
            {
                X509Certificate2 certificate;
                KeyedByTypeCollection<IEndpointBehavior> behaviors = this.channelBuilderSettings.Behaviors;
                if (!string.IsNullOrEmpty(keyStorageFlags))
                {
                    X509KeyStorageFlags flags = (X509KeyStorageFlags) System.Enum.Parse(typeof(X509KeyStorageFlags), keyStorageFlags);
                    certificate = new X509Certificate2(fileName, password, flags);
                }
                else
                {
                    certificate = new X509Certificate2(fileName, password);
                }
                ClientCredentials item = behaviors.Find<ClientCredentials>();
                if (item == null)
                {
                    item = new ClientCredentials();
                    behaviors.Add(item);
                }
                item.ClientCertificate.Certificate = certificate;
            }
        }

        void IChannelCredentials.SetClientCertificateFromStore(string storeLocation, string storeName, string findType, object findValue)
        {
            lock (this.channelBuilderSettings)
            {
                StoreLocation location = (StoreLocation) System.Enum.Parse(typeof(StoreLocation), storeLocation);
                StoreName name = (StoreName) System.Enum.Parse(typeof(StoreName), storeName);
                X509FindType type = (X509FindType) System.Enum.Parse(typeof(X509FindType), findType);
                KeyedByTypeCollection<IEndpointBehavior> behaviors = this.channelBuilderSettings.Behaviors;
                ClientCredentials item = behaviors.Find<ClientCredentials>();
                if (item == null)
                {
                    item = new ClientCredentials();
                    behaviors.Add(item);
                }
                item.ClientCertificate.SetCertificate(location, name, type, findValue);
            }
        }

        void IChannelCredentials.SetClientCertificateFromStoreByName(string subjectName, string storeLocation, string storeName)
        {
            ((IChannelCredentials) this).SetClientCertificateFromStore(storeLocation, storeName, X509FindType.FindBySubjectDistinguishedName.ToString("G"), subjectName);
        }

        void IChannelCredentials.SetDefaultServiceCertificateFromFile(string fileName, string password, string keyStorageFlags)
        {
            lock (this.channelBuilderSettings)
            {
                X509Certificate2 certificate;
                KeyedByTypeCollection<IEndpointBehavior> behaviors = this.channelBuilderSettings.Behaviors;
                if (!string.IsNullOrEmpty(keyStorageFlags))
                {
                    X509KeyStorageFlags flags = (X509KeyStorageFlags) System.Enum.Parse(typeof(X509KeyStorageFlags), keyStorageFlags);
                    certificate = new X509Certificate2(fileName, password, flags);
                }
                else
                {
                    certificate = new X509Certificate2(fileName, password);
                }
                ClientCredentials item = behaviors.Find<ClientCredentials>();
                if (item == null)
                {
                    item = new ClientCredentials();
                    behaviors.Add(item);
                }
                item.ServiceCertificate.DefaultCertificate = certificate;
            }
        }

        void IChannelCredentials.SetDefaultServiceCertificateFromStore(string storeLocation, string storeName, string findType, object findValue)
        {
            lock (this.channelBuilderSettings)
            {
                StoreLocation location = (StoreLocation) System.Enum.Parse(typeof(StoreLocation), storeLocation);
                StoreName name = (StoreName) System.Enum.Parse(typeof(StoreName), storeName);
                X509FindType type = (X509FindType) System.Enum.Parse(typeof(X509FindType), findType);
                KeyedByTypeCollection<IEndpointBehavior> behaviors = this.channelBuilderSettings.Behaviors;
                ClientCredentials item = behaviors.Find<ClientCredentials>();
                if (item == null)
                {
                    item = new ClientCredentials();
                    behaviors.Add(item);
                }
                item.ServiceCertificate.SetDefaultCertificate(location, name, type, findValue);
            }
        }

        void IChannelCredentials.SetDefaultServiceCertificateFromStoreByName(string subjectName, string storeLocation, string storeName)
        {
            ((IChannelCredentials) this).SetDefaultServiceCertificateFromStore(storeLocation, storeName, X509FindType.FindBySubjectDistinguishedName.ToString("G"), subjectName);
        }

        void IChannelCredentials.SetIssuedToken(string localIssuerAddres, string localIssuerBindingType, string localIssuerBinding)
        {
            lock (this.channelBuilderSettings)
            {
                Binding binding = null;
                binding = ConfigLoader.LookupBinding(localIssuerBindingType, localIssuerBinding);
                KeyedByTypeCollection<IEndpointBehavior> behaviors = this.channelBuilderSettings.Behaviors;
                ClientCredentials item = behaviors.Find<ClientCredentials>();
                if (item == null)
                {
                    item = new ClientCredentials();
                    behaviors.Add(item);
                }
                item.IssuedToken.LocalIssuerAddress = new EndpointAddress(localIssuerAddres);
                item.IssuedToken.LocalIssuerBinding = binding;
            }
        }

        void IChannelCredentials.SetServiceCertificateAuthentication(string storeLocation, string revocationMode, string certificationValidationMode)
        {
            lock (this.channelBuilderSettings)
            {
                StoreLocation location = (StoreLocation) System.Enum.Parse(typeof(StoreLocation), storeLocation);
                X509RevocationMode mode = (X509RevocationMode) System.Enum.Parse(typeof(X509RevocationMode), revocationMode);
                X509CertificateValidationMode chainTrust = X509CertificateValidationMode.ChainTrust;
                if (!string.IsNullOrEmpty(certificationValidationMode))
                {
                    chainTrust = (X509CertificateValidationMode) System.Enum.Parse(typeof(X509CertificateValidationMode), certificationValidationMode);
                }
                KeyedByTypeCollection<IEndpointBehavior> behaviors = this.channelBuilderSettings.Behaviors;
                ClientCredentials item = behaviors.Find<ClientCredentials>();
                if (item == null)
                {
                    item = new ClientCredentials();
                    behaviors.Add(item);
                }
                item.ServiceCertificate.Authentication.TrustedStoreLocation = location;
                item.ServiceCertificate.Authentication.RevocationMode = mode;
                item.ServiceCertificate.Authentication.CertificateValidationMode = chainTrust;
            }
        }

        void IChannelCredentials.SetUserNameCredential(string userName, string password)
        {
            lock (this.channelBuilderSettings)
            {
                KeyedByTypeCollection<IEndpointBehavior> behaviors = this.channelBuilderSettings.Behaviors;
                ClientCredentials item = behaviors.Find<ClientCredentials>();
                if (item == null)
                {
                    item = new ClientCredentials();
                    behaviors.Add(item);
                }
                item.UserName.UserName = userName;
                item.UserName.Password = password;
            }
        }

        void IChannelCredentials.SetWindowsCredential(string domain, string userName, string password, int impersonationLevel, bool allowNtlm)
        {
            lock (this.channelBuilderSettings)
            {
                KeyedByTypeCollection<IEndpointBehavior> behaviors = this.channelBuilderSettings.Behaviors;
                NetworkCredential credential = null;
                if ((!string.IsNullOrEmpty(domain) || !string.IsNullOrEmpty(userName)) || !string.IsNullOrEmpty(password))
                {
                    if (string.IsNullOrEmpty(userName))
                    {
                        userName = "";
                    }
                    System.ServiceModel.Security.SecurityUtils.PrepareNetworkCredential();
                    credential = new NetworkCredential(userName, password, domain);
                }
                ClientCredentials item = behaviors.Find<ClientCredentials>();
                if (item == null)
                {
                    item = new ClientCredentials();
                    behaviors.Add(item);
                }
                item.Windows.AllowedImpersonationLevel = (TokenImpersonationLevel) impersonationLevel;
                item.Windows.AllowNtlm = allowNtlm;
                item.Windows.ClientCredential = credential;
            }
        }
    }
}

