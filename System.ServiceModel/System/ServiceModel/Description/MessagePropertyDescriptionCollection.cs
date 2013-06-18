namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;

    public class MessagePropertyDescriptionCollection : KeyedCollection<string, MessagePropertyDescription>
    {
        internal MessagePropertyDescriptionCollection() : base(null, 4)
        {
        }

        protected override string GetKeyForItem(MessagePropertyDescription item)
        {
            return item.Name;
        }
    }
}

