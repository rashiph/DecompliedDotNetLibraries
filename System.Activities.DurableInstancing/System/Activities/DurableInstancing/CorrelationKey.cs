namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal sealed class CorrelationKey
    {
        public CorrelationKey(Guid keyId) : this(keyId, null, InstanceEncodingOption.None)
        {
        }

        public CorrelationKey(Guid keyId, IDictionary<XName, InstanceValue> keyMetadata, InstanceEncodingOption encodingOption)
        {
            this.KeyId = keyId;
            this.BinaryData = SerializationUtilities.SerializeKeyMetadata(keyMetadata, encodingOption);
        }

        public static List<CorrelationKey> BuildKeyList(ICollection<Guid> keys)
        {
            List<CorrelationKey> list = null;
            if (keys != null)
            {
                list = new List<CorrelationKey>(keys.Count);
                foreach (Guid guid in keys)
                {
                    list.Add(new CorrelationKey(guid));
                }
                return list;
            }
            return new List<CorrelationKey>();
        }

        public static List<CorrelationKey> BuildKeyList(IDictionary<Guid, IDictionary<XName, InstanceValue>> keys, InstanceEncodingOption encodingOption)
        {
            List<CorrelationKey> list = new List<CorrelationKey>();
            if (keys != null)
            {
                foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair in keys)
                {
                    list.Add(new CorrelationKey(pair.Key, pair.Value, encodingOption));
                }
            }
            return list;
        }

        public void SerializeToXmlElement(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("CorrelationKey");
            xmlWriter.WriteAttributeString("KeyId", this.KeyId.ToString());
            if (this.BinaryData.Array != null)
            {
                xmlWriter.WriteAttributeString("StartPosition", this.StartPosition.ToString(CultureInfo.InvariantCulture));
                xmlWriter.WriteAttributeString("BinaryLength", this.BinaryData.Count.ToString(CultureInfo.InvariantCulture));
            }
            xmlWriter.WriteEndElement();
        }

        public ArraySegment<byte> BinaryData { get; set; }

        public Guid KeyId { get; set; }

        public long StartPosition { get; set; }
    }
}

