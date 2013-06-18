namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    internal class AspNetEnvironment
    {
        private static AspNetEnvironment current;
        private static bool isEnabled;
        private static object thisLock = new object();

        protected AspNetEnvironment()
        {
        }

        public virtual void AddHostingBehavior(ServiceHostBase serviceHost, System.ServiceModel.Description.ServiceDescription description)
        {
        }

        public virtual void ApplyHostedContext(TransportChannelListener listener, BindingContext context)
        {
        }

        public virtual void DecrementBusyCount()
        {
        }

        public virtual void EnsureAllReferencedAssemblyLoaded()
        {
        }

        public virtual void EnsureCompatibilityRequirements(System.ServiceModel.Description.ServiceDescription description)
        {
        }

        public virtual string GetAnnotationFromHost(ServiceHostBase host)
        {
            return string.Empty;
        }

        public virtual AuthenticationSchemes GetAuthenticationSchemes(Uri baseAddress)
        {
            return AuthenticationSchemes.None;
        }

        public virtual List<Uri> GetBaseAddresses(Uri addressTemplate)
        {
            return null;
        }

        public virtual BaseUriWithWildcard GetBaseUri(string transportScheme, Uri listenUri)
        {
            return null;
        }

        public virtual object GetConfigurationSection(string sectionPath)
        {
            return ConfigurationManager.GetSection(sectionPath);
        }

        public virtual IAspNetMessageProperty GetHostingProperty(Message message)
        {
            return null;
        }

        public virtual void IncrementBusyCount()
        {
        }

        public virtual bool IsWebConfigAboveApplication(object configHostingContext)
        {
            return SystemWebHelper.IsWebConfigAboveApplication(configHostingContext);
        }

        public virtual IAspNetMessageProperty PrepareMessageForDispatch(Message message)
        {
            return null;
        }

        public virtual void ProcessBehaviorForMetadataExtension(IServiceBehavior serviceBehavior, BindingParameterCollection bindingParameters)
        {
        }

        public virtual void ProcessNotMatchedEndpointAddress(Uri uri, string endpointName)
        {
        }

        public virtual void TraceDecrementBusyCount(string data)
        {
        }

        public virtual bool TraceDecrementBusyCountIsEnabled()
        {
            return false;
        }

        public virtual void TraceIncrementBusyCount(string data)
        {
        }

        public virtual bool TraceIncrementBusyCountIsEnabled()
        {
            return false;
        }

        public virtual bool TryGetFullVirtualPath(out string virtualPath)
        {
            virtualPath = null;
            return false;
        }

        [SecurityCritical]
        public virtual object UnsafeGetConfigurationSection(string sectionPath)
        {
            return UnsafeGetSectionFromConfigurationManager(sectionPath);
        }

        [SecurityCritical, ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        private static object UnsafeGetSectionFromConfigurationManager(string sectionPath)
        {
            return ConfigurationManager.GetSection(sectionPath);
        }

        public virtual void ValidateCompatibilityRequirements(AspNetCompatibilityRequirementsMode compatibilityMode)
        {
            if (compatibilityMode == AspNetCompatibilityRequirementsMode.Required)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("Hosting_CompatibilityServiceNotHosted")));
            }
        }

        public virtual void ValidateHttpSettings(string virtualPath, bool isMetadataListener, bool usingDefaultSpnList, ref AuthenticationSchemes supportedSchemes, ref ExtendedProtectionPolicy extendedProtectionPolicy, ref string realm)
        {
        }

        public virtual bool ValidateHttpsSettings(string virtualPath, ref bool? requireClientCertificate)
        {
            return false;
        }

        public virtual bool AspNetCompatibilityEnabled
        {
            get
            {
                return false;
            }
        }

        public virtual string ConfigurationPath
        {
            get
            {
                return AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            }
        }

        public static AspNetEnvironment Current
        {
            get
            {
                if (current == null)
                {
                    lock (thisLock)
                    {
                        if (current == null)
                        {
                            current = new AspNetEnvironment();
                        }
                    }
                }
                return current;
            }
            protected set
            {
                current = value;
                isEnabled = true;
            }
        }

        public virtual string CurrentVirtualPath
        {
            get
            {
                return null;
            }
        }

        public static bool Enabled
        {
            get
            {
                return isEnabled;
            }
        }

        public virtual bool IsConfigurationBased
        {
            get
            {
                return false;
            }
        }

        public bool RequiresImpersonation
        {
            get
            {
                return this.AspNetCompatibilityEnabled;
            }
        }

        public virtual string XamlFileBaseLocation
        {
            get
            {
                return null;
            }
        }
    }
}

