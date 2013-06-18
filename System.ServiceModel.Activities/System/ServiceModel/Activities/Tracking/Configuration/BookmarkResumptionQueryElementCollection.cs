namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(BookmarkResumptionQueryElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="bookmarkResumptionQuery")]
    public class BookmarkResumptionQueryElementCollection : TrackingConfigurationCollection<BookmarkResumptionQueryElement>
    {
        protected override string ElementName
        {
            get
            {
                return "bookmarkResumptionQuery";
            }
        }
    }
}

