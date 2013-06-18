namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    internal class JsonXmlDataContract : JsonDataContract
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public JsonXmlDataContract(XmlDataContract traditionalXmlDataContract) : base(traditionalXmlDataContract)
        {
        }

        private List<Type> GetKnownTypesFromContext(XmlObjectSerializerContext context, IList<Type> serializerKnownTypeList)
        {
            List<Type> list = new List<Type>();
            if (context != null)
            {
                List<XmlQualifiedName> list2 = new List<XmlQualifiedName>();
                Dictionary<XmlQualifiedName, DataContract>[] dataContractDictionaries = context.scopedKnownTypes.dataContractDictionaries;
                if (dataContractDictionaries != null)
                {
                    for (int i = 0; i < dataContractDictionaries.Length; i++)
                    {
                        Dictionary<XmlQualifiedName, DataContract> dictionary = dataContractDictionaries[i];
                        if (dictionary != null)
                        {
                            foreach (KeyValuePair<XmlQualifiedName, DataContract> pair in dictionary)
                            {
                                if (!list2.Contains(pair.Key))
                                {
                                    list2.Add(pair.Key);
                                    list.Add(pair.Value.UnderlyingType);
                                }
                            }
                        }
                    }
                }
                if (serializerKnownTypeList != null)
                {
                    list.AddRange(serializerKnownTypeList);
                }
            }
            return list;
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            object obj2;
            string s = jsonReader.ReadElementContentAsString();
            DataContractSerializer serializer = new DataContractSerializer(base.TraditionalDataContract.UnderlyingType, this.GetKnownTypesFromContext(context, (context == null) ? null : context.SerializerKnownTypeList), 1, false, false, null);
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(s));
            XmlDictionaryReaderQuotas readerQuotas = ((JsonReaderDelegator) jsonReader).ReaderQuotas;
            if (readerQuotas == null)
            {
                obj2 = serializer.ReadObject(stream);
            }
            else
            {
                obj2 = serializer.ReadObject(XmlDictionaryReader.CreateTextReader(stream, readerQuotas));
            }
            if (context != null)
            {
                context.AddNewObject(obj2);
            }
            return obj2;
        }

        public override void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            DataContractSerializer serializer = new DataContractSerializer(Type.GetTypeFromHandle(declaredTypeHandle), this.GetKnownTypesFromContext(context, (context == null) ? null : context.SerializerKnownTypeList), 1, false, false, null);
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, obj);
            stream.Position = 0L;
            string str = new StreamReader(stream).ReadToEnd();
            jsonWriter.WriteString(str);
        }
    }
}

