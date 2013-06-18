namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;

    internal class JsonDataContract
    {
        [SecurityCritical]
        private JsonDataContractCriticalHelper helper;

        [SecuritySafeCritical]
        protected JsonDataContract(DataContract traditionalDataContract)
        {
            this.helper = new JsonDataContractCriticalHelper(traditionalDataContract);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected JsonDataContract(JsonDataContractCriticalHelper helper)
        {
            this.helper = helper;
        }

        [SecuritySafeCritical]
        public static JsonDataContract GetJsonDataContract(DataContract traditionalDataContract)
        {
            return JsonDataContractCriticalHelper.GetJsonDataContract(traditionalDataContract);
        }

        protected static object HandleReadValue(object obj, XmlObjectSerializerReadContext context)
        {
            context.AddNewObject(obj);
            return obj;
        }

        protected void PopKnownDataContracts(XmlObjectSerializerContext context)
        {
            if (this.KnownDataContracts != null)
            {
                context.scopedKnownTypes.Pop();
            }
        }

        protected void PushKnownDataContracts(XmlObjectSerializerContext context)
        {
            if (this.KnownDataContracts != null)
            {
                context.scopedKnownTypes.Push(this.KnownDataContracts);
            }
        }

        public object ReadJsonValue(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            this.PushKnownDataContracts(context);
            object obj2 = this.ReadJsonValueCore(jsonReader, context);
            this.PopKnownDataContracts(context);
            return obj2;
        }

        public virtual object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            return this.TraditionalDataContract.ReadXmlValue(jsonReader, context);
        }

        protected static bool TryReadNullAtTopLevel(XmlReaderDelegator reader)
        {
            while (reader.MoveToAttribute("type") && (reader.Value == "null"))
            {
                reader.Skip();
                reader.MoveToElement();
                return true;
            }
            reader.MoveToElement();
            return false;
        }

        public void WriteJsonValue(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            this.PushKnownDataContracts(context);
            this.WriteJsonValueCore(jsonWriter, obj, context, declaredTypeHandle);
            this.PopKnownDataContracts(context);
        }

        public virtual void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            this.TraditionalDataContract.WriteXmlValue(jsonWriter, obj, context);
        }

        protected JsonDataContractCriticalHelper Helper
        {
            [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.helper;
            }
        }

        private Dictionary<XmlQualifiedName, DataContract> KnownDataContracts
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.KnownDataContracts;
            }
        }

        protected DataContract TraditionalDataContract
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.TraditionalDataContract;
            }
        }

        internal virtual string TypeName
        {
            get
            {
                return null;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        internal class JsonDataContractCriticalHelper
        {
            private static object cacheLock = new object();
            private static object createDataContractLock = new object();
            private static JsonDataContract[] dataContractCache = new JsonDataContract[0x20];
            private static int dataContractID = 0;
            private Dictionary<XmlQualifiedName, DataContract> knownDataContracts;
            private DataContract traditionalDataContract;
            private static TypeHandleRef typeHandleRef = new TypeHandleRef();
            private string typeName;
            private static Dictionary<TypeHandleRef, IntRef> typeToIDCache = new Dictionary<TypeHandleRef, IntRef>(new TypeHandleRefEqualityComparer());

            internal JsonDataContractCriticalHelper(DataContract traditionalDataContract)
            {
                this.traditionalDataContract = traditionalDataContract;
                this.AddCollectionItemContractsToKnownDataContracts();
                this.typeName = string.IsNullOrEmpty(traditionalDataContract.Namespace.Value) ? traditionalDataContract.Name.Value : (traditionalDataContract.Name.Value + ":" + XmlObjectSerializerWriteContextComplexJson.TruncateDefaultDataContractNamespace(traditionalDataContract.Namespace.Value));
            }

            private void AddCollectionItemContractsToKnownDataContracts()
            {
                if (this.traditionalDataContract.KnownDataContracts != null)
                {
                    foreach (KeyValuePair<XmlQualifiedName, DataContract> pair in this.traditionalDataContract.KnownDataContracts)
                    {
                        if (!object.ReferenceEquals(pair, null))
                        {
                            DataContract itemContract;
                            for (CollectionDataContract contract = pair.Value as CollectionDataContract; contract != null; contract = itemContract as CollectionDataContract)
                            {
                                itemContract = contract.ItemContract;
                                if (this.knownDataContracts == null)
                                {
                                    this.knownDataContracts = new Dictionary<XmlQualifiedName, DataContract>();
                                }
                                if (!this.knownDataContracts.ContainsKey(itemContract.StableName))
                                {
                                    this.knownDataContracts.Add(itemContract.StableName, itemContract);
                                }
                                if (contract.ItemType.IsGenericType && (contract.ItemType.GetGenericTypeDefinition() == typeof(KeyValue<,>)))
                                {
                                    DataContract dataContract = DataContract.GetDataContract(Globals.TypeOfKeyValuePair.MakeGenericType(contract.ItemType.GetGenericArguments()));
                                    if (!this.knownDataContracts.ContainsKey(dataContract.StableName))
                                    {
                                        this.knownDataContracts.Add(dataContract.StableName, dataContract);
                                    }
                                }
                                if (!(itemContract is CollectionDataContract))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            private static JsonDataContract CreateJsonDataContract(int id, DataContract traditionalDataContract)
            {
                lock (createDataContractLock)
                {
                    JsonDataContract contract = dataContractCache[id];
                    if (contract == null)
                    {
                        Type type = traditionalDataContract.GetType();
                        if (type == typeof(ObjectDataContract))
                        {
                            contract = new JsonObjectDataContract(traditionalDataContract);
                        }
                        else if (type == typeof(StringDataContract))
                        {
                            contract = new JsonStringDataContract((StringDataContract) traditionalDataContract);
                        }
                        else if (type == typeof(UriDataContract))
                        {
                            contract = new JsonUriDataContract((UriDataContract) traditionalDataContract);
                        }
                        else if (type == typeof(QNameDataContract))
                        {
                            contract = new JsonQNameDataContract((QNameDataContract) traditionalDataContract);
                        }
                        else if (type == typeof(ByteArrayDataContract))
                        {
                            contract = new JsonByteArrayDataContract((ByteArrayDataContract) traditionalDataContract);
                        }
                        else if (traditionalDataContract.IsPrimitive || (traditionalDataContract.UnderlyingType == Globals.TypeOfXmlQualifiedName))
                        {
                            contract = new JsonDataContract(traditionalDataContract);
                        }
                        else if (type == typeof(ClassDataContract))
                        {
                            contract = new JsonClassDataContract((ClassDataContract) traditionalDataContract);
                        }
                        else if (type == typeof(EnumDataContract))
                        {
                            contract = new JsonEnumDataContract((EnumDataContract) traditionalDataContract);
                        }
                        else if ((type == typeof(GenericParameterDataContract)) || (type == typeof(SpecialTypeDataContract)))
                        {
                            contract = new JsonDataContract(traditionalDataContract);
                        }
                        else if (type == typeof(CollectionDataContract))
                        {
                            contract = new JsonCollectionDataContract((CollectionDataContract) traditionalDataContract);
                        }
                        else
                        {
                            if (type != typeof(XmlDataContract))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("traditionalDataContract", System.Runtime.Serialization.SR.GetString("JsonTypeNotSupportedByDataContractJsonSerializer", new object[] { traditionalDataContract.UnderlyingType }));
                            }
                            contract = new JsonXmlDataContract((XmlDataContract) traditionalDataContract);
                        }
                    }
                    return contract;
                }
            }

            internal static int GetId(RuntimeTypeHandle typeHandle)
            {
                lock (cacheLock)
                {
                    IntRef ref2;
                    typeHandleRef.Value = typeHandle;
                    if (!typeToIDCache.TryGetValue(typeHandleRef, out ref2))
                    {
                        int num = dataContractID++;
                        if (num >= dataContractCache.Length)
                        {
                            int newSize = (num < 0x3fffffff) ? (num * 2) : 0x7fffffff;
                            if (newSize <= num)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("DataContractCacheOverflow")));
                            }
                            Array.Resize<JsonDataContract>(ref dataContractCache, newSize);
                        }
                        ref2 = new IntRef(num);
                        try
                        {
                            typeToIDCache.Add(new TypeHandleRef(typeHandle), ref2);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(exception.Message, exception);
                        }
                    }
                    return ref2.Value;
                }
            }

            public static JsonDataContract GetJsonDataContract(DataContract traditionalDataContract)
            {
                int id = GetId(traditionalDataContract.UnderlyingType.TypeHandle);
                JsonDataContract contract = dataContractCache[id];
                if (contract == null)
                {
                    contract = CreateJsonDataContract(id, traditionalDataContract);
                    dataContractCache[id] = contract;
                }
                return contract;
            }

            internal Dictionary<XmlQualifiedName, DataContract> KnownDataContracts
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.knownDataContracts;
                }
            }

            internal DataContract TraditionalDataContract
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.traditionalDataContract;
                }
            }

            internal virtual string TypeName
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.typeName;
                }
            }
        }
    }
}

