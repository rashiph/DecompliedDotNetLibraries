namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(ProfileElement), AddItemName="trackingProfile", RemoveItemName="remove", ClearItemsName="clear")]
    public sealed class ProfileElementCollection : TrackingConfigurationCollection<ProfileElement>
    {
        internal ProfileElementCollection()
        {
            base.AddElementName = "trackingProfile";
            base.RemoveElementName = "remove";
            base.ClearElementName = "clear";
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }
    }
}

