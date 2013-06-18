namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml;

    internal static class DataContractSerializerDefaults
    {
        internal const bool IgnoreExtensionDataObject = false;
        internal const int MaxItemsInObjectGraph = 0x10000;

        internal static DataContractSerializer CreateSerializer(Type type, int maxItems)
        {
            return CreateSerializer(type, null, maxItems);
        }

        internal static DataContractSerializer CreateSerializer(Type type, IList<Type> knownTypes, int maxItems)
        {
            return new DataContractSerializer(type, knownTypes, maxItems, false, false, null);
        }

        internal static DataContractSerializer CreateSerializer(Type type, string rootName, string rootNs, int maxItems)
        {
            return CreateSerializer(type, null, rootName, rootNs, maxItems);
        }

        internal static DataContractSerializer CreateSerializer(Type type, XmlDictionaryString rootName, XmlDictionaryString rootNs, int maxItems)
        {
            return CreateSerializer(type, null, rootName, rootNs, maxItems);
        }

        internal static DataContractSerializer CreateSerializer(Type type, IList<Type> knownTypes, string rootName, string rootNs, int maxItems)
        {
            return new DataContractSerializer(type, rootName, rootNs, knownTypes, maxItems, false, false, null);
        }

        internal static DataContractSerializer CreateSerializer(Type type, IList<Type> knownTypes, XmlDictionaryString rootName, XmlDictionaryString rootNs, int maxItems)
        {
            return new DataContractSerializer(type, rootName, rootNs, knownTypes, maxItems, false, false, null);
        }
    }
}

