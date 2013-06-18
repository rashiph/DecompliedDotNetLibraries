namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;

    internal sealed class WriteObjectInfo
    {
        internal WriteObjectInfo arrayElemObjectInfo;
        internal long assemId;
        internal SerObjectInfoCache cache;
        internal StreamingContext context;
        internal IFormatterConverter converter;
        internal bool isArray;
        internal bool isNamed;
        internal bool isSi;
        internal bool isTyped;
        private int lastPosition;
        internal object[] memberData;
        internal object obj;
        internal long objectId;
        internal int objectInfoId;
        internal Type objectType;
        private SoapAttributeInfo parentMemberAttributeInfo;
        internal ISerializationSurrogate serializationSurrogate;
        internal SerObjectInfoInit serObjectInfoInit;
        internal SerializationInfo si;
        internal ISurrogateSelector surrogateSelector;
        internal SoapAttributeInfo typeAttributeInfo;

        internal WriteObjectInfo()
        {
        }

        [Conditional("SER_LOGGING")]
        private void DumpMemberInfo()
        {
            for (int i = 0; i < this.cache.memberInfos.Length; i++)
            {
            }
        }

        internal string GetAssemblyString()
        {
            if (this.arrayElemObjectInfo != null)
            {
                return this.arrayElemObjectInfo.GetAssemblyString();
            }
            if (this.IsAttributeNameSpace())
            {
                return this.typeAttributeInfo.m_nameSpace;
            }
            return this.cache.assemblyString;
        }

        internal void GetMemberInfo(out string[] outMemberNames, out Type[] outMemberTypes, out object[] outMemberData, out SoapAttributeInfo[] outAttributeInfo)
        {
            outMemberNames = this.cache.memberNames;
            outMemberTypes = this.cache.memberTypes;
            outMemberData = this.memberData;
            outAttributeInfo = this.cache.memberAttributeInfos;
            if (this.isSi && !this.isNamed)
            {
                throw new SerializationException(SoapUtil.GetResourceString("Serialization_ISerializableMemberInfo"));
            }
        }

        internal Type GetMemberType(MemberInfo objMember)
        {
            if (objMember is FieldInfo)
            {
                return ((FieldInfo) objMember).FieldType;
            }
            if (!(objMember is PropertyInfo))
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_SerMemberInfo"), new object[] { objMember.GetType() }));
            }
            return ((PropertyInfo) objMember).PropertyType;
        }

        private static WriteObjectInfo GetObjectInfo(SerObjectInfoInit serObjectInfoInit)
        {
            WriteObjectInfo info = null;
            if (!serObjectInfoInit.oiPool.IsEmpty())
            {
                info = (WriteObjectInfo) serObjectInfoInit.oiPool.Pop();
                info.InternalInit();
                return info;
            }
            return new WriteObjectInfo { objectInfoId = serObjectInfoInit.objectInfoIdCount++ };
        }

        private SoapAttributeInfo GetTypeAttributeInfo()
        {
            if (this.arrayElemObjectInfo != null)
            {
                return this.arrayElemObjectInfo.GetTypeAttributeInfo();
            }
            SoapAttributeInfo attributeInfo = null;
            if (this.parentMemberAttributeInfo != null)
            {
                attributeInfo = this.parentMemberAttributeInfo;
            }
            else
            {
                attributeInfo = new SoapAttributeInfo();
            }
            Attr.ProcessTypeAttribute(this.objectType, attributeInfo);
            return attributeInfo;
        }

        internal string GetTypeFullName()
        {
            return this.cache.fullTypeName;
        }

        private void InitMemberInfo()
        {
            this.cache = (SerObjectInfoCache) this.serObjectInfoInit.seenBeforeTable[this.objectType];
            if (this.cache == null)
            {
                this.cache = new SerObjectInfoCache();
                int length = 0;
                if (!this.objectType.IsByRef)
                {
                    this.cache.memberInfos = FormatterServices.GetSerializableMembers(this.objectType, this.context);
                    length = this.cache.memberInfos.Length;
                }
                this.cache.memberNames = new string[length];
                this.cache.memberTypes = new Type[length];
                this.cache.memberAttributeInfos = new SoapAttributeInfo[length];
                for (int i = 0; i < length; i++)
                {
                    this.cache.memberNames[i] = this.cache.memberInfos[i].Name;
                    this.cache.memberTypes[i] = this.GetMemberType(this.cache.memberInfos[i]);
                    this.cache.memberAttributeInfos[i] = Attr.GetMemberAttributeInfo(this.cache.memberInfos[i], this.cache.memberNames[i], this.cache.memberTypes[i]);
                }
                this.cache.fullTypeName = this.objectType.FullName;
                this.cache.assemblyString = this.objectType.Module.Assembly.FullName;
                this.serObjectInfoInit.seenBeforeTable.Add(this.objectType, this.cache);
            }
            if (this.obj != null)
            {
                this.memberData = FormatterServices.GetObjectData(this.obj, this.cache.memberInfos);
            }
            this.isTyped = true;
            this.isNamed = true;
        }

        private void InitNoMembers()
        {
            this.cache = (SerObjectInfoCache) this.serObjectInfoInit.seenBeforeTable[this.objectType];
            if (this.cache == null)
            {
                this.cache = new SerObjectInfoCache();
                this.cache.fullTypeName = this.objectType.FullName;
                this.cache.assemblyString = this.objectType.Module.Assembly.FullName;
                this.serObjectInfoInit.seenBeforeTable.Add(this.objectType, this.cache);
            }
        }

        internal void InitSerialize(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, SoapAttributeInfo attributeInfo)
        {
            this.objectType = objectType;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;
            this.parentMemberAttributeInfo = attributeInfo;
            this.surrogateSelector = surrogateSelector;
            this.converter = converter;
            if (objectType.IsArray)
            {
                this.arrayElemObjectInfo = Serialize(objectType.GetElementType(), surrogateSelector, context, serObjectInfoInit, converter, null);
                this.typeAttributeInfo = this.GetTypeAttributeInfo();
                this.InitNoMembers();
            }
            else
            {
                this.typeAttributeInfo = this.GetTypeAttributeInfo();
                ISurrogateSelector selector = null;
                if (surrogateSelector != null)
                {
                    this.serializationSurrogate = surrogateSelector.GetSurrogate(objectType, context, out selector);
                }
                if (this.serializationSurrogate != null)
                {
                    this.isSi = true;
                }
                else if (!(objectType == Converter.typeofObject) && Converter.typeofISerializable.IsAssignableFrom(objectType))
                {
                    this.isSi = true;
                }
                if (this.isSi)
                {
                    this.si = new SerializationInfo(objectType, converter);
                    this.cache = new SerObjectInfoCache();
                    this.cache.fullTypeName = this.si.FullTypeName;
                    this.cache.assemblyString = this.si.AssemblyName;
                }
                else
                {
                    this.InitMemberInfo();
                }
            }
        }

        internal void InitSerialize(object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, SoapAttributeInfo attributeInfo, ObjectWriter objectWriter)
        {
            this.context = context;
            this.obj = obj;
            this.serObjectInfoInit = serObjectInfoInit;
            this.parentMemberAttributeInfo = attributeInfo;
            this.surrogateSelector = surrogateSelector;
            this.converter = converter;
            if (RemotingServices.IsTransparentProxy(obj))
            {
                this.objectType = Converter.typeofMarshalByRefObject;
            }
            else
            {
                this.objectType = obj.GetType();
            }
            if (this.objectType.IsArray)
            {
                this.arrayElemObjectInfo = Serialize(this.objectType.GetElementType(), surrogateSelector, context, serObjectInfoInit, converter, null);
                this.typeAttributeInfo = this.GetTypeAttributeInfo();
                this.isArray = true;
                this.InitNoMembers();
            }
            else
            {
                ISurrogateSelector selector;
                this.typeAttributeInfo = this.GetTypeAttributeInfo();
                objectWriter.ObjectManager.RegisterObject(obj);
                if ((surrogateSelector != null) && ((this.serializationSurrogate = surrogateSelector.GetSurrogate(this.objectType, context, out selector)) != null))
                {
                    this.si = new SerializationInfo(this.objectType, converter);
                    if (!this.objectType.IsPrimitive)
                    {
                        this.serializationSurrogate.GetObjectData(obj, this.si, context);
                    }
                    this.InitSiWrite(objectWriter);
                }
                else if (obj is ISerializable)
                {
                    if (!this.objectType.IsSerializable)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_NonSerType"), new object[] { this.objectType.FullName, this.objectType.Module.Assembly.FullName }));
                    }
                    this.si = new SerializationInfo(this.objectType, converter);
                    ((ISerializable) obj).GetObjectData(this.si, context);
                    this.InitSiWrite(objectWriter);
                }
                else
                {
                    this.InitMemberInfo();
                }
            }
        }

        private void InitSiWrite(ObjectWriter objectWriter)
        {
            if (this.si.FullTypeName.Equals("FormatterWrapper"))
            {
                this.obj = this.si.GetValue("__WrappedObject", Converter.typeofObject);
                this.InitSerialize(this.obj, this.surrogateSelector, this.context, this.serObjectInfoInit, this.converter, null, objectWriter);
            }
            else
            {
                SerializationInfoEnumerator enumerator = null;
                this.isSi = true;
                enumerator = this.si.GetEnumerator();
                int memberCount = this.si.MemberCount;
                this.cache = new SerObjectInfoCache();
                this.cache.memberNames = new string[memberCount];
                this.cache.memberTypes = new Type[memberCount];
                this.memberData = new object[memberCount];
                this.cache.fullTypeName = this.si.FullTypeName;
                this.cache.assemblyString = this.si.AssemblyName;
                enumerator = this.si.GetEnumerator();
                for (int i = 0; enumerator.MoveNext(); i++)
                {
                    this.cache.memberNames[i] = enumerator.Name;
                    this.cache.memberTypes[i] = enumerator.ObjectType;
                    this.memberData[i] = enumerator.Value;
                }
                this.isNamed = true;
                this.isTyped = false;
            }
        }

        private void InternalInit()
        {
            this.obj = null;
            this.objectType = null;
            this.isSi = false;
            this.isNamed = false;
            this.isTyped = false;
            this.si = null;
            this.cache = null;
            this.memberData = null;
            this.isArray = false;
            this.objectId = 0L;
            this.assemId = 0L;
            this.lastPosition = 0;
            this.typeAttributeInfo = null;
            this.parentMemberAttributeInfo = null;
            this.arrayElemObjectInfo = null;
        }

        internal bool IsAttributeNameSpace()
        {
            if (this.arrayElemObjectInfo != null)
            {
                return this.arrayElemObjectInfo.IsAttributeNameSpace();
            }
            return ((this.typeAttributeInfo != null) && (this.typeAttributeInfo.m_nameSpace != null));
        }

        internal bool IsCallElement()
        {
            if ((((this.objectType == Converter.typeofObject) || !Converter.typeofIMethodCallMessage.IsAssignableFrom(this.objectType)) || Converter.typeofIConstructionCallMessage.IsAssignableFrom(this.objectType)) && (!(this.objectType == Converter.typeofReturnMessage) && !(this.objectType == Converter.typeofInternalSoapMessage)))
            {
                return false;
            }
            return true;
        }

        internal bool IsCustomXmlAttribute()
        {
            if (this.arrayElemObjectInfo != null)
            {
                return this.arrayElemObjectInfo.IsCustomXmlAttribute();
            }
            return ((this.typeAttributeInfo != null) && ((this.typeAttributeInfo.m_attributeType & SoapAttributeType.XmlAttribute) != SoapAttributeType.None));
        }

        internal bool IsCustomXmlElement()
        {
            if (this.arrayElemObjectInfo != null)
            {
                return this.arrayElemObjectInfo.IsCustomXmlElement();
            }
            return ((this.typeAttributeInfo != null) && ((this.typeAttributeInfo.m_attributeType & SoapAttributeType.XmlElement) != SoapAttributeType.None));
        }

        internal bool IsEmbeddedAttribute(string name)
        {
            if (this.arrayElemObjectInfo != null)
            {
                return this.arrayElemObjectInfo.IsEmbeddedAttribute(name);
            }
            bool flag = false;
            if ((this.cache.memberAttributeInfos != null) && (this.cache.memberAttributeInfos.Length > 0))
            {
                flag = this.cache.memberAttributeInfos[this.Position(name)].IsEmbedded();
            }
            return flag;
        }

        internal bool IsInteropNameSpace()
        {
            if (this.arrayElemObjectInfo != null)
            {
                return this.arrayElemObjectInfo.IsInteropNameSpace();
            }
            if (!this.IsAttributeNameSpace() && !this.IsCallElement())
            {
                return false;
            }
            return true;
        }

        internal void ObjectEnd()
        {
            PutObjectInfo(this.serObjectInfoInit, this);
        }

        private int Position(string name)
        {
            if (this.cache.memberNames[this.lastPosition].Equals(name))
            {
                return this.lastPosition;
            }
            if ((++this.lastPosition < this.cache.memberNames.Length) && this.cache.memberNames[this.lastPosition].Equals(name))
            {
                return this.lastPosition;
            }
            for (int i = 0; i < this.cache.memberNames.Length; i++)
            {
                if (this.cache.memberNames[i].Equals(name))
                {
                    this.lastPosition = i;
                    return this.lastPosition;
                }
            }
            throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Position"), new object[] { this.objectType + " " + name }));
        }

        private static void PutObjectInfo(SerObjectInfoInit serObjectInfoInit, WriteObjectInfo objectInfo)
        {
            serObjectInfoInit.oiPool.Push(objectInfo);
        }

        internal static WriteObjectInfo Serialize(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, SoapAttributeInfo attributeInfo)
        {
            WriteObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
            objectInfo.InitSerialize(objectType, surrogateSelector, context, serObjectInfoInit, converter, attributeInfo);
            return objectInfo;
        }

        internal static WriteObjectInfo Serialize(object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, SoapAttributeInfo attributeInfo, ObjectWriter objectWriter)
        {
            WriteObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
            objectInfo.InitSerialize(obj, surrogateSelector, context, serObjectInfoInit, converter, attributeInfo, objectWriter);
            return objectInfo;
        }
    }
}

