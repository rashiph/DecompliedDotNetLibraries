namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Activities.Tracking;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Tracking;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public class EtwTrackingBehavior : IServiceBehavior
    {
        public virtual void AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public virtual void ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            WorkflowServiceHost host = serviceHostBase as WorkflowServiceHost;
            if (host != null)
            {
                string displayName = host.Activity.DisplayName;
                string hostReference = string.Empty;
                if (AspNetEnvironment.Enabled)
                {
                    VirtualPathExtension extension = serviceHostBase.Extensions.Find<VirtualPathExtension>();
                    if ((extension != null) && (extension.VirtualPath != null))
                    {
                        string str2 = (serviceDescription != null) ? serviceDescription.Name : string.Empty;
                        string applicationVirtualPath = extension.ApplicationVirtualPath;
                        string str4 = extension.VirtualPath.Replace("~", applicationVirtualPath + "|");
                        hostReference = string.Format(CultureInfo.InvariantCulture, "{0}{1}|{2}", new object[] { extension.SiteName, str4, str2 });
                    }
                }
                TrackingProfile trackingProfile = this.GetProfile(this.ProfileName, displayName);
                host.WorkflowExtensions.Add<EtwTrackingParticipant>(() => new EtwTrackingParticipant { ApplicationReference = hostReference, TrackingProfile = trackingProfile });
            }
        }

        private TrackingProfile GetProfile(string profileName, string displayName)
        {
            DefaultProfileManager manager = new DefaultProfileManager();
            return manager.GetProfile(profileName, displayName);
        }

        public virtual void Validate(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public string ProfileName { get; set; }
    }
}

