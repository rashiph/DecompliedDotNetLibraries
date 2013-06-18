namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class MappableObjectManager
    {
        [DataMember(EmitDefaultValue=false)]
        private List<MappableLocation> mappableLocations;

        public IDictionary<string, LocationInfo> GatherMappableVariables()
        {
            Dictionary<string, LocationInfo> dictionary = null;
            if ((this.mappableLocations != null) && (this.mappableLocations.Count > 0))
            {
                dictionary = new Dictionary<string, LocationInfo>(this.mappableLocations.Count);
                for (int i = 0; i < this.mappableLocations.Count; i++)
                {
                    MappableLocation location = this.mappableLocations[i];
                    dictionary.Add(location.MappingKeyName, new LocationInfo(location.Name, location.OwnerDisplayName, location.Location.Value));
                }
            }
            return dictionary;
        }

        public void Register(Location location, Activity activity, LocationReference locationOwner, System.Activities.ActivityInstance activityInstance)
        {
            if (this.mappableLocations == null)
            {
                this.mappableLocations = new List<MappableLocation>();
            }
            this.mappableLocations.Add(new MappableLocation(locationOwner, activity, activityInstance, location));
        }

        public void Unregister(Location location)
        {
            int count = this.mappableLocations.Count;
            for (int i = 0; i < count; i++)
            {
                if (object.ReferenceEquals(this.mappableLocations[i].Location, location))
                {
                    this.mappableLocations.RemoveAt(i);
                    return;
                }
            }
        }

        public int Count
        {
            get
            {
                int num = 0;
                if (this.mappableLocations != null)
                {
                    num += this.mappableLocations.Count;
                }
                return num;
            }
        }

        [DataContract]
        private class MappableLocation
        {
            public MappableLocation(LocationReference locationOwner, Activity activity, System.Activities.ActivityInstance activityInstance, System.Activities.Location location)
            {
                this.Name = locationOwner.Name;
                this.OwnerDisplayName = activity.DisplayName;
                this.Location = location;
                this.MappingKeyName = string.Format(CultureInfo.InvariantCulture, "activity.{0}-{1}_{2}", new object[] { activity.Id, locationOwner.Id, activityInstance.Id });
            }

            [DataMember]
            internal System.Activities.Location Location { get; private set; }

            [DataMember]
            internal string MappingKeyName { get; private set; }

            [DataMember]
            public string Name { get; private set; }

            [DataMember(EmitDefaultValue=false)]
            public string OwnerDisplayName { get; private set; }
        }
    }
}

