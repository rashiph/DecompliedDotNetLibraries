namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Activities.Tracking;
    using System.Collections.ObjectModel;
    using System.Configuration;

    public class TrackingSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties;
        private Collection<TrackingProfile> trackingProfiles;

        [ConfigurationProperty("profiles")]
        public ProfileElementCollection Profiles
        {
            get
            {
                return (ProfileElementCollection) base["profiles"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("profiles", typeof(ProfileElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        public Collection<TrackingProfile> TrackingProfiles
        {
            get
            {
                if (this.trackingProfiles == null)
                {
                    this.trackingProfiles = new Collection<TrackingProfile>();
                    foreach (ProfileElement element in this.Profiles)
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

