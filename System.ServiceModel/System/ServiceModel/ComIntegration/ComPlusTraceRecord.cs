namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal static class ComPlusTraceRecord
    {
        public static void SerializeRecord(XmlWriter xmlWriter, object o)
        {
            DataContractSerializerDefaults.CreateSerializer((o == null) ? typeof(object) : o.GetType(), 0x10000).WriteObject(xmlWriter, o);
        }
    }
}

