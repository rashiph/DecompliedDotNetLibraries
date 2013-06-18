namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;

    public class MessageDescriptionCollection : Collection<MessageDescription>
    {
        internal MessageDescriptionCollection()
        {
        }

        public MessageDescription Find(string action)
        {
            foreach (MessageDescription description in this)
            {
                if ((description != null) && (action == description.Action))
                {
                    return description;
                }
            }
            return null;
        }

        public Collection<MessageDescription> FindAll(string action)
        {
            Collection<MessageDescription> collection = new Collection<MessageDescription>();
            foreach (MessageDescription description in this)
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

