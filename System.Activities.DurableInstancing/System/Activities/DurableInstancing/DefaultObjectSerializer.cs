namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;

    internal class DefaultObjectSerializer : IObjectSerializer
    {
        private NetDataContractSerializer serializer = new NetDataContractSerializer();

        public Dictionary<XName, object> DeserializePropertyBag(byte[] serializedValue)
        {
            using (MemoryStream stream = new MemoryStream(serializedValue))
            {
                return this.DeserializePropertyBag(stream);
            }
        }

        protected virtual Dictionary<XName, object> DeserializePropertyBag(Stream stream)
        {
            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                Dictionary<XName, object> dictionary = new Dictionary<XName, object>();
                if (reader.ReadToDescendant("Property"))
                {
                    do
                    {
                        reader.Read();
                        KeyValuePair<XName, object> pair = (KeyValuePair<XName, object>) this.serializer.ReadObject(reader);
                        dictionary.Add(pair.Key, pair.Value);
                    }
                    while (reader.ReadToNextSibling("Property"));
                }
                return dictionary;
            }
        }

        public object DeserializeValue(byte[] serializedValue)
        {
            using (MemoryStream stream = new MemoryStream(serializedValue))
            {
                return this.DeserializeValue(stream);
            }
        }

        protected virtual object DeserializeValue(Stream stream)
        {
            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                return this.serializer.ReadObject(reader);
            }
        }

        public ArraySegment<byte> SerializePropertyBag(Dictionary<XName, object> value)
        {
            using (MemoryStream stream = new MemoryStream(0x1000))
            {
                this.SerializePropertyBag(stream, value);
                return new ArraySegment<byte>(stream.GetBuffer(), 0, Convert.ToInt32(stream.Length));
            }
        }

        protected virtual void SerializePropertyBag(Stream stream, Dictionary<XName, object> propertyBag)
        {
            using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, false))
            {
                writer.WriteStartElement("Properties");
                foreach (KeyValuePair<XName, object> pair in propertyBag)
                {
                    writer.WriteStartElement("Property");
                    this.serializer.WriteObject(writer, pair);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        public ArraySegment<byte> SerializeValue(object value)
        {
            using (MemoryStream stream = new MemoryStream(0x1000))
            {
                this.SerializeValue(stream, value);
                return new ArraySegment<byte>(stream.GetBuffer(), 0, Convert.ToInt32(stream.Length));
            }
        }

        protected virtual void SerializeValue(Stream stream, object value)
        {
            using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, false))
            {
                this.serializer.WriteObject(writer, value);
            }
        }
    }
}

