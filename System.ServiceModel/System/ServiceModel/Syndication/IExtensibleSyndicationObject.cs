namespace System.ServiceModel.Syndication
{
    using System.Collections.Generic;

    internal interface IExtensibleSyndicationObject
    {
        Dictionary<XmlQualifiedName, string> AttributeExtensions { get; }

        SyndicationElementExtensionCollection ElementExtensions { get; }
    }
}

