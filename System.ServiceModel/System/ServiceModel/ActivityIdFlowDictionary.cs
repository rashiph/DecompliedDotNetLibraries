namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class ActivityIdFlowDictionary
    {
        public XmlDictionaryString ActivityId;
        public XmlDictionaryString ActivityIdNamespace;

        public ActivityIdFlowDictionary(ServiceModelDictionary dictionary)
        {
            this.ActivityId = dictionary.CreateString("ActivityId", 0x1a9);
            this.ActivityIdNamespace = dictionary.CreateString("http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics", 0x1aa);
        }
    }
}

