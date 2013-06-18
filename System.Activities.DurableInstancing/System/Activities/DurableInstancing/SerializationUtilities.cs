namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.DurableInstancing;
    using System.Text;
    using System.Xml;

    internal static class SerializationUtilities
    {
        private static bool CanDecimalBeStoredAsSqlVariant(decimal value)
        {
            string str = value.ToString("G", CultureInfo.InvariantCulture);
            return (((str.Length - str.IndexOf(".", StringComparison.Ordinal)) - 1) <= 0x12);
        }

        public static object CreateCorrelationKeyXmlBlob(List<CorrelationKey> correlationKeys)
        {
            if ((correlationKeys == null) || (correlationKeys.Count == 0))
            {
                return DBNull.Value;
            }
            StringBuilder output = new StringBuilder(0x200);
            using (XmlWriter writer = XmlWriter.Create(output))
            {
                writer.WriteStartElement("CorrelationKeys");
                foreach (CorrelationKey key in correlationKeys)
                {
                    key.SerializeToXmlElement(writer);
                }
                writer.WriteEndElement();
            }
            return output.ToString();
        }

        public static byte[] CreateKeyBinaryBlob(List<CorrelationKey> correlationKeys)
        {
            long num = correlationKeys.Sum<CorrelationKey>((Func<CorrelationKey, int>) (i => i.BinaryData.Count));
            byte[] dst = null;
            if (num > 0L)
            {
                dst = new byte[num];
                long num2 = 0L;
                foreach (CorrelationKey key in correlationKeys)
                {
                    Buffer.BlockCopy(key.BinaryData.Array, 0, dst, Convert.ToInt32(num2), Convert.ToInt32(key.BinaryData.Count));
                    key.StartPosition = num2;
                    num2 += key.BinaryData.Count;
                }
            }
            return dst;
        }

        public static Dictionary<XName, InstanceValue> DeserializeKeyMetadata(byte[] serializedKeyMetadata, InstanceEncodingOption encodingOption)
        {
            return DeserializeMetadataPropertyBag(serializedKeyMetadata, encodingOption);
        }

        public static Dictionary<XName, InstanceValue> DeserializeMetadataPropertyBag(byte[] serializedMetadataProperties, InstanceEncodingOption instanceEncodingOption)
        {
            Dictionary<XName, InstanceValue> dictionary = new Dictionary<XName, InstanceValue>();
            if (serializedMetadataProperties != null)
            {
                foreach (KeyValuePair<XName, object> pair in ObjectSerializerFactory.GetObjectSerializer(instanceEncodingOption).DeserializePropertyBag(serializedMetadataProperties))
                {
                    dictionary.Add(pair.Key, new InstanceValue(pair.Value));
                }
            }
            return dictionary;
        }

        public static object DeserializeObject(byte[] serializedObject, InstanceEncodingOption encodingOption)
        {
            return ObjectSerializerFactory.GetObjectSerializer(encodingOption).DeserializeValue(serializedObject);
        }

        public static Dictionary<XName, InstanceValue> DeserializePropertyBag(byte[] primitiveDataProperties, byte[] complexDataProperties, InstanceEncodingOption encodingOption)
        {
            IObjectSerializer objectSerializer = ObjectSerializerFactory.GetObjectSerializer(encodingOption);
            Dictionary<XName, InstanceValue> dictionary = new Dictionary<XName, InstanceValue>();
            Dictionary<XName, object>[] dictionaryArray = new Dictionary<XName, object>[2];
            if (primitiveDataProperties != null)
            {
                dictionaryArray[0] = (Dictionary<XName, object>) objectSerializer.DeserializeValue(primitiveDataProperties);
            }
            if (complexDataProperties != null)
            {
                dictionaryArray[1] = objectSerializer.DeserializePropertyBag(complexDataProperties);
            }
            foreach (Dictionary<XName, object> dictionary2 in dictionaryArray)
            {
                if (dictionary2 != null)
                {
                    foreach (KeyValuePair<XName, object> pair in dictionary2)
                    {
                        dictionary.Add(pair.Key, new InstanceValue(pair.Value));
                    }
                }
            }
            return dictionary;
        }

        public static bool IsPropertyTypeSqlVariantCompatible(InstanceValue value)
        {
            if (((!value.IsDeletedValue && (value.Value != null)) && (!(value.Value is string) || (((string) value.Value).Length > 0xfa0))) && (((!(value.Value is Guid) && !(value.Value is DateTime)) && (!(value.Value is int) && !(value.Value is double))) && ((!(value.Value is float) && !(value.Value is long)) && ((!(value.Value is short) && !(value.Value is byte)) && (!(value.Value is decimal) || !CanDecimalBeStoredAsSqlVariant((decimal) value.Value))))))
            {
                return false;
            }
            return true;
        }

        public static ArraySegment<byte> SerializeKeyMetadata(IDictionary<XName, InstanceValue> metadataProperties, InstanceEncodingOption encodingOption)
        {
            if ((metadataProperties == null) || (metadataProperties.Count <= 0))
            {
                return new ArraySegment<byte>();
            }
            Dictionary<XName, object> dictionary = new Dictionary<XName, object>();
            foreach (KeyValuePair<XName, InstanceValue> pair in metadataProperties)
            {
                if ((pair.Value.Options & InstanceValueOptions.WriteOnly) != InstanceValueOptions.WriteOnly)
                {
                    dictionary.Add(pair.Key, pair.Value.Value);
                }
            }
            return ObjectSerializerFactory.GetObjectSerializer(encodingOption).SerializePropertyBag(dictionary);
        }

        public static ArraySegment<byte> SerializeMetadataPropertyBag(SaveWorkflowCommand saveWorkflowCommand, InstancePersistenceContext context, InstanceEncodingOption instanceEncodingOption)
        {
            IObjectSerializer objectSerializer = ObjectSerializerFactory.GetObjectSerializer(instanceEncodingOption);
            Dictionary<XName, object> dictionary = new Dictionary<XName, object>();
            if (context.InstanceView.InstanceMetadataConsistency == InstanceValueConsistency.None)
            {
                foreach (KeyValuePair<XName, InstanceValue> pair in context.InstanceView.InstanceMetadata)
                {
                    if ((pair.Value.Options & InstanceValueOptions.WriteOnly) == InstanceValueOptions.None)
                    {
                        dictionary.Add(pair.Key, pair.Value.Value);
                    }
                }
            }
            foreach (KeyValuePair<XName, InstanceValue> pair2 in saveWorkflowCommand.InstanceMetadataChanges)
            {
                if (pair2.Value.IsDeletedValue)
                {
                    if (context.InstanceView.InstanceMetadataConsistency == InstanceValueConsistency.None)
                    {
                        dictionary.Remove(pair2.Key);
                    }
                    else
                    {
                        DeletedMetadataValue value2 = new DeletedMetadataValue();
                        dictionary[pair2.Key] = value2;
                    }
                }
                else if ((pair2.Value.Options & InstanceValueOptions.WriteOnly) == InstanceValueOptions.None)
                {
                    dictionary[pair2.Key] = pair2.Value.Value;
                }
            }
            if (dictionary.Count > 0)
            {
                return objectSerializer.SerializePropertyBag(dictionary);
            }
            return new ArraySegment<byte>();
        }

        public static ArraySegment<byte> SerializeObject(object objectToSerialize, InstanceEncodingOption encodingOption)
        {
            return ObjectSerializerFactory.GetObjectSerializer(encodingOption).SerializeValue(objectToSerialize);
        }

        public static ArraySegment<byte>[] SerializePropertyBag(IDictionary<XName, InstanceValue> properties, InstanceEncodingOption encodingOption)
        {
            ArraySegment<byte>[] segmentArray = new ArraySegment<byte>[4];
            if (properties.Count > 0)
            {
                IObjectSerializer objectSerializer = ObjectSerializerFactory.GetObjectSerializer(encodingOption);
                XmlPropertyBag bag = new XmlPropertyBag();
                XmlPropertyBag bag2 = new XmlPropertyBag();
                Dictionary<XName, object> dictionary = new Dictionary<XName, object>();
                Dictionary<XName, object> dictionary2 = new Dictionary<XName, object>();
                Dictionary<XName, object>[] dictionaryArray = new Dictionary<XName, object>[] { bag, dictionary, bag2, dictionary2 };
                foreach (KeyValuePair<XName, InstanceValue> pair in properties)
                {
                    bool flag = XmlPropertyBag.GetPrimitiveType(pair.Value.Value) == PrimitiveType.Unavailable;
                    int index = (((pair.Value.Options & InstanceValueOptions.WriteOnly) == InstanceValueOptions.WriteOnly) ? 2 : 0) + (flag ? 1 : 0);
                    dictionaryArray[index].Add(pair.Key, pair.Value.Value);
                }
                bag2.Remove(SqlWorkflowInstanceStoreConstants.StatusPropertyName);
                bag2.Remove(SqlWorkflowInstanceStoreConstants.LastUpdatePropertyName);
                bag2.Remove(SqlWorkflowInstanceStoreConstants.PendingTimerExpirationPropertyName);
                dictionary2.Remove(SqlWorkflowInstanceStoreConstants.BinaryBlockingBookmarksPropertyName);
                for (int i = 0; i < dictionaryArray.Length; i++)
                {
                    if (dictionaryArray[i].Count > 0)
                    {
                        if (dictionaryArray[i] is XmlPropertyBag)
                        {
                            segmentArray[i] = objectSerializer.SerializeValue(dictionaryArray[i]);
                        }
                        else
                        {
                            segmentArray[i] = objectSerializer.SerializePropertyBag(dictionaryArray[i]);
                        }
                    }
                }
            }
            return segmentArray;
        }
    }
}

