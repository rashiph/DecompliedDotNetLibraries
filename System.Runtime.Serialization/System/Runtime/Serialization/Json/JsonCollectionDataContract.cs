namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Xml;

    internal class JsonCollectionDataContract : JsonDataContract
    {
        [SecurityCritical]
        private JsonCollectionDataContractCriticalHelper helper;

        [SecuritySafeCritical]
        public JsonCollectionDataContract(CollectionDataContract traditionalDataContract) : base(new JsonCollectionDataContractCriticalHelper(traditionalDataContract))
        {
            this.helper = base.Helper as JsonCollectionDataContractCriticalHelper;
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            jsonReader.Read();
            object obj2 = null;
            if (context.IsGetOnlyCollection)
            {
                context.IsGetOnlyCollection = false;
                this.JsonFormatGetOnlyReaderDelegate(jsonReader, context, XmlDictionaryString.Empty, JsonGlobals.itemDictionaryString, this.TraditionalCollectionDataContract);
            }
            else
            {
                obj2 = this.JsonFormatReaderDelegate(jsonReader, context, XmlDictionaryString.Empty, JsonGlobals.itemDictionaryString, this.TraditionalCollectionDataContract);
            }
            jsonReader.ReadEndElement();
            return obj2;
        }

        public override void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            context.IsGetOnlyCollection = false;
            this.JsonFormatWriterDelegate(jsonWriter, obj, context, this.TraditionalCollectionDataContract);
        }

        internal JsonFormatGetOnlyCollectionReaderDelegate JsonFormatGetOnlyReaderDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (this.helper.JsonFormatGetOnlyReaderDelegate == null)
                {
                    lock (this)
                    {
                        if (this.helper.JsonFormatGetOnlyReaderDelegate == null)
                        {
                            JsonFormatGetOnlyCollectionReaderDelegate delegate2 = new JsonFormatReaderGenerator().GenerateGetOnlyCollectionReader(this.TraditionalCollectionDataContract);
                            Thread.MemoryBarrier();
                            this.helper.JsonFormatGetOnlyReaderDelegate = delegate2;
                        }
                    }
                }
                return this.helper.JsonFormatGetOnlyReaderDelegate;
            }
        }

        internal JsonFormatCollectionReaderDelegate JsonFormatReaderDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (this.helper.JsonFormatReaderDelegate == null)
                {
                    lock (this)
                    {
                        if (this.helper.JsonFormatReaderDelegate == null)
                        {
                            JsonFormatCollectionReaderDelegate delegate2 = new JsonFormatReaderGenerator().GenerateCollectionReader(this.TraditionalCollectionDataContract);
                            Thread.MemoryBarrier();
                            this.helper.JsonFormatReaderDelegate = delegate2;
                        }
                    }
                }
                return this.helper.JsonFormatReaderDelegate;
            }
        }

        internal JsonFormatCollectionWriterDelegate JsonFormatWriterDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (this.helper.JsonFormatWriterDelegate == null)
                {
                    lock (this)
                    {
                        if (this.helper.JsonFormatWriterDelegate == null)
                        {
                            JsonFormatCollectionWriterDelegate delegate2 = new JsonFormatWriterGenerator().GenerateCollectionWriter(this.TraditionalCollectionDataContract);
                            Thread.MemoryBarrier();
                            this.helper.JsonFormatWriterDelegate = delegate2;
                        }
                    }
                }
                return this.helper.JsonFormatWriterDelegate;
            }
        }

        private CollectionDataContract TraditionalCollectionDataContract
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.TraditionalCollectionDataContract;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class JsonCollectionDataContractCriticalHelper : JsonDataContract.JsonDataContractCriticalHelper
        {
            private JsonFormatGetOnlyCollectionReaderDelegate jsonFormatGetOnlyReaderDelegate;
            private JsonFormatCollectionReaderDelegate jsonFormatReaderDelegate;
            private JsonFormatCollectionWriterDelegate jsonFormatWriterDelegate;
            private CollectionDataContract traditionalCollectionDataContract;

            public JsonCollectionDataContractCriticalHelper(CollectionDataContract traditionalDataContract) : base(traditionalDataContract)
            {
                this.traditionalCollectionDataContract = traditionalDataContract;
            }

            internal JsonFormatGetOnlyCollectionReaderDelegate JsonFormatGetOnlyReaderDelegate
            {
                get
                {
                    return this.jsonFormatGetOnlyReaderDelegate;
                }
                set
                {
                    this.jsonFormatGetOnlyReaderDelegate = value;
                }
            }

            internal JsonFormatCollectionReaderDelegate JsonFormatReaderDelegate
            {
                get
                {
                    return this.jsonFormatReaderDelegate;
                }
                set
                {
                    this.jsonFormatReaderDelegate = value;
                }
            }

            internal JsonFormatCollectionWriterDelegate JsonFormatWriterDelegate
            {
                get
                {
                    return this.jsonFormatWriterDelegate;
                }
                set
                {
                    this.jsonFormatWriterDelegate = value;
                }
            }

            internal CollectionDataContract TraditionalCollectionDataContract
            {
                get
                {
                    return this.traditionalCollectionDataContract;
                }
            }
        }
    }
}

