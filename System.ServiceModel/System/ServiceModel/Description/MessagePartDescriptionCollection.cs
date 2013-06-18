namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.Xml;

    public class MessagePartDescriptionCollection : KeyedCollection<XmlQualifiedName, MessagePartDescription>
    {
        internal MessagePartDescriptionCollection() : base(null, 4)
        {
        }

        protected override XmlQualifiedName GetKeyForItem(MessagePartDescription item)
        {
            return new XmlQualifiedName(item.Name, item.Namespace);
        }
    }
}

