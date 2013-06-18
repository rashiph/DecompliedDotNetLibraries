namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class SurrogateDataContract : DataContract
    {
        [SecurityCritical]
        private SurrogateDataContractCriticalHelper helper;

        [SecuritySafeCritical]
        internal SurrogateDataContract(Type type, ISerializationSurrogate serializationSurrogate) : base(new SurrogateDataContractCriticalHelper(type, serializationSurrogate))
        {
            this.helper = base.Helper as SurrogateDataContractCriticalHelper;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        internal static object GetRealObject(IObjectReference obj, StreamingContext context)
        {
            return obj.GetRealObject(context);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        private object GetUninitializedObject(Type objType)
        {
            return FormatterServices.GetUninitializedObject(objType);
        }

        public override object ReadXmlValue(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context)
        {
            xmlReader.Read();
            Type underlyingType = base.UnderlyingType;
            object obj2 = underlyingType.IsArray ? Array.CreateInstance(underlyingType.GetElementType(), 0) : this.GetUninitializedObject(underlyingType);
            context.AddNewObject(obj2);
            string objectId = context.GetObjectId();
            SerializationInfo serInfo = context.ReadSerializationInfo(xmlReader, underlyingType);
            object newObj = this.SerializationSurrogateSetObjectData(obj2, serInfo, context.GetStreamingContext());
            if (newObj == null)
            {
                newObj = obj2;
            }
            if (newObj is IDeserializationCallback)
            {
                ((IDeserializationCallback) newObj).OnDeserialization(null);
            }
            if (newObj is IObjectReference)
            {
                newObj = GetRealObject((IObjectReference) newObj, context.GetStreamingContext());
            }
            context.ReplaceDeserializedObject(objectId, obj2, newObj);
            xmlReader.ReadEndElement();
            return newObj;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        private void SerializationSurrogateGetObjectData(object obj, SerializationInfo serInfo, StreamingContext context)
        {
            this.SerializationSurrogate.GetObjectData(obj, serInfo, context);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        private object SerializationSurrogateSetObjectData(object obj, SerializationInfo serInfo, StreamingContext context)
        {
            return this.SerializationSurrogate.SetObjectData(obj, serInfo, context, null);
        }

        public override void WriteXmlValue(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context)
        {
            SerializationInfo serInfo = new SerializationInfo(base.UnderlyingType, XmlObjectSerializer.FormatterConverter);
            this.SerializationSurrogateGetObjectData(obj, serInfo, context.GetStreamingContext());
            context.WriteSerializationInfo(xmlWriter, base.UnderlyingType, serInfo);
        }

        internal ISerializationSurrogate SerializationSurrogate
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.SerializationSurrogate;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class SurrogateDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            private ISerializationSurrogate serializationSurrogate;

            internal SurrogateDataContractCriticalHelper(Type type, ISerializationSurrogate serializationSurrogate) : base(type)
            {
                string str;
                string str2;
                this.serializationSurrogate = serializationSurrogate;
                DataContract.GetDefaultStableName(DataContract.GetClrTypeFullName(type), out str, out str2);
                base.SetDataContractName(DataContract.CreateQualifiedName(str, str2));
            }

            internal ISerializationSurrogate SerializationSurrogate
            {
                get
                {
                    return this.serializationSurrogate;
                }
            }
        }
    }
}

