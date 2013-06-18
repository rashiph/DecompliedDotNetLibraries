namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;

    public class FaultDescriptionCollection : Collection<FaultDescription>
    {
        internal FaultDescriptionCollection()
        {
        }

        public FaultDescription Find(string action)
        {
            foreach (FaultDescription description in this)
            {
                if ((description != null) && (action == description.Action))
                {
                    return description;
                }
            }
            return null;
        }

        public Collection<FaultDescription> FindAll(string action)
        {
            Collection<FaultDescription> collection = new Collection<FaultDescription>();
            foreach (FaultDescription description in this)
            {
                if ((description != null) && (action == description.Action))
                {
                    collection.Add(description);
                }
            }
            return collection;
        }
    }
}

