namespace System.ServiceModel.Activities.Tracking
{
    using System;
    using System.Activities.Tracking;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Tracking.Configuration;
    using System.ServiceModel.Configuration;

    internal class DefaultProfileManager : TrackingProfileManager
    {
        private ConfigFileProfileStore profileStore;

        internal TrackingProfile GetProfile(string profileName, string activityDefinitionId)
        {
            Collection<TrackingProfile> collection = this.ProfileStore.ReadProfiles();
            TrackingProfile profile = null;
            if (collection != null)
            {
                foreach (TrackingProfile profile2 in collection)
                {
                    if (string.Compare(profileName, profile2.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (string.Compare("*", profile2.ActivityDefinitionId, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (profile == null)
                            {
                                profile = profile2;
                            }
                        }
                        else if (string.Compare(activityDefinitionId, profile2.ActivityDefinitionId, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            profile = profile2;
                            break;
                        }
                    }
                }
            }
            if (profile != null)
            {
                return profile;
            }
            if (TD.TrackingProfileNotFoundIsEnabled())
            {
                TD.TrackingProfileNotFound(profileName, activityDefinitionId);
            }
            return new TrackingProfile { ActivityDefinitionId = activityDefinitionId };
        }

        public override TrackingProfile Load(string profileName, string activityDefinitionId, TimeSpan timeout)
        {
            if (profileName == null)
            {
                throw FxTrace.Exception.ArgumentNull("profileName");
            }
            return this.GetProfile(profileName, activityDefinitionId);
        }

        private ConfigFileProfileStore ProfileStore
        {
            get
            {
                if (this.profileStore == null)
                {
                    this.profileStore = new ConfigFileProfileStore();
                }
                return this.profileStore;
            }
        }

        private class ConfigFileProfileStore
        {
            private Collection<TrackingProfile> trackingProfiles;

            public Collection<TrackingProfile> ReadProfiles()
            {
                if (this.trackingProfiles == null)
                {
                    TrackingSection section = null;
                    try
                    {
                        section = (TrackingSection) ConfigurationHelpers.GetSection(ConfigurationHelpers.GetSectionPath("tracking"));
                    }
                    catch (ConfigurationErrorsException exception)
                    {
                        if (!Fx.IsFatal(exception))
                        {
                            FxTrace.Exception.TraceUnhandledException(exception);
                        }
                        throw;
                    }
                    if (section == null)
                    {
                        return null;
                    }
                    this.trackingProfiles = new Collection<TrackingProfile>();
                    foreach (ProfileElement element in section.Profiles)
                    {
                        if (element.Workflows != null)
                        {
                            foreach (ProfileWorkflowElement element2 in element.Workflows)
                            {
                                TrackingProfile item = new TrackingProfile {
                                    Name = element.Name,
                                    ImplementationVisibility = element.ImplementationVisibility,
                                    ActivityDefinitionId = element2.ActivityDefinitionId
                                };
                                element2.AddQueries(item.Queries);
                                this.trackingProfiles.Add(item);
                            }
                        }
                    }
                }
                return this.trackingProfiles;
            }
        }
    }
}

