namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.Xml;

    public class MessageHeaderDescriptionCollection : KeyedCollection<XmlQualifiedName, MessageHeaderDescription>
    {
        internal MessageHeaderDescriptionCollection() : base(null, 4)
        {
        }

        protected override XmlQualifiedName GetKeyForItem(MessageHeaderDescription item)
        {
            return new XmlQualifiedName(item.Name, item.Namespace);
        }
    }
}

