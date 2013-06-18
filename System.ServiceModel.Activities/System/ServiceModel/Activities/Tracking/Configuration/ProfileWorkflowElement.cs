namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Collections.ObjectModel;
    using System.Configuration;

    public class ProfileWorkflowElement : TrackingConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void AddQueries(Collection<TrackingQuery> queries)
        {
            AddQueryCollection(queries, this.WorkflowInstanceQueries);
            AddQueryCollection(queries, this.ActivityStateQueries);
            AddQueryCollection(queries, this.ActivityScheduledQueries);
            AddQueryCollection(queries, this.CancelRequestedQueries);
            AddQueryCollection(queries, this.FaultPropagationQueries);
            AddQueryCollection(queries, this.BookmarkResumptionQueries);
            AddQueryCollection(queries, this.CustomTrackingQueries);
        }

        private static void AddQueryCollection(Collection<TrackingQuery> queries, ConfigurationElementCollection elements)
        {
            foreach (TrackingQueryElement element in elements)
            {
                queries.Add(element.CreateTrackingQuery());
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("activityDefinitionId", IsKey=true, DefaultValue="*")]
        public string ActivityDefinitionId
        {
            get
            {
                return (string) base["activityDefinitionId"];
            }
            set
            {
                base["activityDefinitionId"] = value;
            }
        }

        [ConfigurationProperty("activityScheduledQueries")]
        public ActivityScheduledQueryElementCollection ActivityScheduledQueries
        {
            get
            {
                return (ActivityScheduledQueryElementCollection) base["activityScheduledQueries"];
            }
        }

        [ConfigurationProperty("activityStateQueries")]
        public ActivityStateQueryElementCollection ActivityStateQueries
        {
            get
            {
                return (ActivityStateQueryElementCollection) base["activityStateQueries"];
            }
        }

        [ConfigurationProperty("bookmarkResumptionQueries")]
        public BookmarkResumptionQueryElementCollection BookmarkResumptionQueries
        {
            get
            {
                return (BookmarkResumptionQueryElementCollection) base["bookmarkResumptionQueries"];
            }
        }

        [ConfigurationProperty("cancelRequestedQueries")]
        public CancelRequestedQueryElementCollection CancelRequestedQueries
        {
            get
            {
                return (CancelRequestedQueryElementCollection) base["cancelRequestedQueries"];
            }
        }

        [ConfigurationProperty("customTrackingQueries")]
        public CustomTrackingQueryElementCollection CustomTrackingQueries
        {
            get
            {
                return (CustomTrackingQueryElementCollection) base["customTrackingQueries"];
            }
        }

        public override object ElementKey
        {
            get
            {
                return this.ActivityDefinitionId;
            }
        }

        [ConfigurationProperty("faultPropagationQueries")]
        public FaultPropagationQueryElementCollection FaultPropagationQueries
        {
            get
            {
                return (FaultPropagationQueryElementCollection) base["faultPropagationQueries"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("activityDefinitionId", typeof(string), "*", null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("workflowInstanceQueries", typeof(WorkflowInstanceQueryElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("activityStateQueries", typeof(ActivityStateQueryElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("activityScheduledQueries", typeof(ActivityScheduledQueryElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("cancelRequestedQueries", typeof(CancelRequestedQueryElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("faultPropagationQueries", typeof(FaultPropagationQueryElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("bookmarkResumptionQueries", typeof(BookmarkResumptionQueryElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("customTrackingQueries", typeof(CustomTrackingQueryElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("workflowInstanceQueries")]
        public WorkflowInstanceQueryElementCollection WorkflowInstanceQueries
        {
            get
            {
                return (WorkflowInstanceQueryElementCollection) base["workflowInstanceQueries"];
            }
        }
    }
}

