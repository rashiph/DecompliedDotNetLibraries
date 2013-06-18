namespace System.Activities.Tracking
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Queries")]
    public class TrackingProfile
    {
        private Collection<TrackingQuery> queries;

        [DefaultValue((string) null)]
        public string ActivityDefinitionId { get; set; }

        [DefaultValue(0)]
        public System.Activities.Tracking.ImplementationVisibility ImplementationVisibility { get; set; }

        [DefaultValue((string) null)]
        public string Name { get; set; }

        public Collection<TrackingQuery> Queries
        {
            get
            {
                if (this.queries == null)
                {
                    this.queries = new Collection<TrackingQuery>();
                }
                return this.queries;
            }
        }
    }
}

