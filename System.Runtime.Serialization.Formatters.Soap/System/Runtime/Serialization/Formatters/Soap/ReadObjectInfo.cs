namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal sealed class ReadObjectInfo
    {
        private ReadObjectInfo arrayElemObjectInfo;
        internal bool bfake;
        internal bool bSoapFault;
        internal SerObjectInfoCache cache;
        internal StreamingContext context;
        internal int count;
        internal IFormatterConverter formatterConverter;
        internal bool isNamed;
        internal bool isSi;
        internal bool isTyped;
        private int lastPosition;
        private int majorVersion;
        internal object[] memberData;
        internal string[] memberNames;
        internal ArrayList memberTypesList;
        private int minorVersion;
        private int numberMembersSeen;
        internal object obj;
        internal int objectInfoId;
        internal ObjectManager objectManager;
        internal Type objectType;
        internal ArrayList paramNameList;
        internal ISerializationSurrogate serializationSurrogate;
        internal SerObjectInfoInit serObjectInfoInit;
        internal SerializationInfo si;
        internal ISurrogateSelector surrogateSelector;
        internal SoapAttributeInfo typeAttributeInfo;
        internal string[] wireMemberNames;
        internal Type[] wireMemberTypes;

        internal ReadObjectInfo()
        {
        }

        internal void AddMemberSeen()
        {
            this.numberMembersSeen++;
        }

        internal void AddParamName(string name)
        {
            if (this.bfake)
            {
                if ((name[0] == '_') && (name[1] == '_'))
                {
                    if (name == "__fault")
                    {
                        this.bSoapFault = true;
                        return;
                    }
                    if (((name == "__methodName") || (name == "__keyToNamespaceTable")) || ((name == "__paramNameList") || (name == "__xmlNameSpace")))
                    {
                        return;
                    }
                }
                this.paramNameList.Add(name);
            }
        }

        internal void AddValue(string name, object value)
        {
            if (this.isSi)
            {
                if (this.bfake)
                {
                    this.AddParamName(name);
                }
                this.si.AddValue(name, value);
            }
            else
            {
                int index = this.Position(name);
                this.memberData[index] = value;
                this.memberNames[index] = name;
            }
        }

        internal static ReadObjectInfo Create(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, string assemblyName)
        {
            ReadObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
            objectInfo.Init(objectType, surrogateSelector, context, objectManager, serObjectInfoInit, converter, assemblyName);
            return objectInfo;
        }

        internal static ReadObjectInfo Create(Type objectType, string[] memberNames, Type[] memberTypes, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, string assemblyName)
        {
            ReadObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
            objectInfo.Init(objectType, memberNames, memberTypes, surrogateSelector, context, objectManager, serObjectInfoInit, converter, assemblyName);
            return objectInfo;
        }

        [Conditional("SER_LOGGING")]
        private void DumpPopulate(MemberInfo[] memberInfos, object[] memberData)
        {
            for (int i = 0; i < memberInfos.Length; i++)
            {
            }
        }

        [Conditional("SER_LOGGING")]
        private void DumpPopulateSi()
        {
            SerializationInfoEnumerator enumerator = this.si.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); i++)
            {
            }
        }

        internal MemberInfo GetMemberInfo(string name)
        {
            if (this.isSi)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_MemberInfo"), new object[] { this.objectType + " " + name }));
            }
            if (this.cache.memberInfos == null)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_NoMemberInfo"), new object[] { this.objectType + " " + name }));
            }
            return this.cache.memberInfos[this.Position(name)];
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

        private static ReadObjectInfo GetObjectInfo(SerObjectInfoInit serObjectInfoInit)
        {
            ReadObjectInfo info = null;
            if (!serObjectInfoInit.oiPool.IsEmpty())
            {
                info = (ReadObjectInfo) serObjectInfoInit.oiPool.Pop();
                info.InternalInit();
                return info;
            }
            return new ReadObjectInfo { objectInfoId = serObjectInfoInit.objectInfoIdCount++ };
        }

        internal Type GetType(int position)
        {
            Type type = null;
            if (!this.isTyped)
            {
                return type;
            }
            if (position >= this.cache.memberTypes.Length)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_ISerializableTypes"), new object[] { this.objectType + " " + position }));
            }
            return this.cache.memberTypes[position];
        }

        internal Type GetType(string name)
        {
            Type type = null;
            if (this.isTyped)
            {
                type = this.cache.memberTypes[this.Position(name)];
            }
            else
            {
                type = (Type) this.memberTypesList[this.Position(name)];
            }
            if (type == null)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_ISerializableTypes"), new object[] { this.objectType + " " + name }));
            }
            return type;
        }

        private SoapAttributeInfo GetTypeAttributeInfo()
        {
            if (this.arrayElemObjectInfo != null)
            {
                return this.arrayElemObjectInfo.GetTypeAttributeInfo();
            }
            SoapAttributeInfo attributeInfo = null;
            attributeInfo = new SoapAttributeInfo();
            Attr.ProcessTypeAttribute(this.objectType, attributeInfo);
            return attributeInfo;
        }

        internal void Init(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, string assemblyName)
        {
            this.objectType = objectType;
            this.objectManager = objectManager;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;
            this.formatterConverter = converter;
            this.InitReadConstructor(objectType, surrogateSelector, context, assemblyName);
        }

        internal void Init(Type objectType, string[] memberNames, Type[] memberTypes, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, string assemblyName)
        {
            this.objectType = objectType;
            this.objectManager = objectManager;
            this.wireMemberNames = memberNames;
            this.wireMemberTypes = memberTypes;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;
            this.formatterConverter = converter;
            if (memberNames != null)
            {
                this.isNamed = true;
            }
            if (memberTypes != null)
            {
                this.isTyped = true;
            }
            this.InitReadConstructor(objectType, surrogateSelector, context, assemblyName);
        }

        private void InitMemberInfo()
        {
            this.cache = (SerObjectInfoCache) this.serObjectInfoInit.seenBeforeTable[this.objectType];
            if (this.cache == null)
            {
                this.cache = new SerObjectInfoCache();
                this.cache.memberInfos = FormatterServices.GetSerializableMembers(this.objectType, this.context);
                this.count = this.cache.memberInfos.Length;
                this.cache.memberNames = new string[this.count];
                this.cache.memberTypes = new Type[this.count];
                this.cache.memberAttributeInfos = new SoapAttributeInfo[this.count];
                for (int i = 0; i < this.count; i++)
                {
                    this.cache.memberNames[i] = this.cache.memberInfos[i].Name;
                    this.cache.memberTypes[i] = this.GetMemberType(this.cache.memberInfos[i]);
                    this.cache.memberAttributeInfos[i] = Attr.GetMemberAttributeInfo(this.cache.memberInfos[i], this.cache.memberNames[i], this.cache.memberTypes[i]);
                }
                this.cache.fullTypeName = this.objectType.FullName;
                this.cache.assemblyString = this.objectType.Module.Assembly.FullName;
                this.serObjectInfoInit.seenBeforeTable.Add(this.objectType, this.cache);
            }
            this.memberData = new object[this.cache.memberNames.Length];
            this.memberNames = new string[this.cache.memberNames.Length];
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

        private void InitReadConstructor(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, string assemblyName)
        {
            if (objectType.IsArray)
            {
                this.arrayElemObjectInfo = Create(objectType.GetElementType(), surrogateSelector, context, this.objectManager, this.serObjectInfoInit, this.formatterConverter, assemblyName);
                this.typeAttributeInfo = this.GetTypeAttributeInfo();
                this.InitNoMembers();
            }
            else
            {
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
                    this.si = new SerializationInfo(objectType, this.formatterConverter);
                    this.InitSiRead(assemblyName);
                }
                else
                {
                    this.InitMemberInfo();
                }
            }
        }

        private void InitSiRead(string assemblyName)
        {
            if (assemblyName != null)
            {
                this.si.AssemblyName = assemblyName;
            }
            this.cache = new SerObjectInfoCache();
            this.cache.fullTypeName = this.si.FullTypeName;
            this.cache.assemblyString = this.si.AssemblyName;
            this.cache.memberNames = this.wireMemberNames;
            this.cache.memberTypes = this.wireMemberTypes;
            if (this.memberTypesList != null)
            {
                this.memberTypesList = new ArrayList(20);
            }
            if ((this.wireMemberNames != null) && (this.wireMemberTypes != null))
            {
                this.isTyped = true;
            }
        }

        private void InternalInit()
        {
            this.obj = null;
            this.objectType = null;
            this.count = 0;
            this.isSi = false;
            this.isNamed = false;
            this.isTyped = false;
            this.si = null;
            this.wireMemberNames = null;
            this.wireMemberTypes = null;
            this.cache = null;
            this.lastPosition = 0;
            this.numberMembersSeen = 0;
            this.bfake = false;
            this.bSoapFault = false;
            this.majorVersion = 0;
            this.minorVersion = 0;
            this.typeAttributeInfo = null;
            this.arrayElemObjectInfo = null;
            if (this.memberTypesList != null)
            {
                this.memberTypesList.Clear();
            }
        }

        internal void ObjectEnd()
        {
            PutObjectInfo(this.serObjectInfoInit, this);
        }

        internal void PopulateObjectMembers()
        {
            if (!this.isSi)
            {
                MemberInfo[] members = null;
                object[] data = null;
                int index = 0;
                if (this.numberMembersSeen < this.memberNames.Length)
                {
                    members = new MemberInfo[this.numberMembersSeen];
                    data = new object[this.numberMembersSeen];
                    for (int i = 0; i < this.memberNames.Length; i++)
                    {
                        if (this.memberNames[i] == null)
                        {
                            object[] customAttributes = this.cache.memberInfos[i].GetCustomAttributes(typeof(OptionalFieldAttribute), false);
                            if (((customAttributes == null) || (customAttributes.Length == 0)) && ((this.majorVersion >= 1) && (this.minorVersion >= 0)))
                            {
                                throw new SerializationException(SoapUtil.GetResourceString("Serialization_WrongNumberOfMembers", new object[] { this.objectType, this.cache.memberInfos.Length, this.numberMembersSeen }));
                            }
                        }
                        else
                        {
                            if (this.memberNames[i] != this.cache.memberInfos[i].Name)
                            {
                                throw new SerializationException(SoapUtil.GetResourceString("Serialization_WrongNumberOfMembers", new object[] { this.objectType, this.cache.memberInfos.Length, this.numberMembersSeen }));
                            }
                            members[index] = this.cache.memberInfos[i];
                            data[index] = this.memberData[i];
                            index++;
                        }
                    }
                }
                else
                {
                    members = this.cache.memberInfos;
                    data = this.memberData;
                }
                FormatterServices.PopulateObjectMembers(this.obj, members, data);
                this.numberMembersSeen = 0;
            }
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

        private static void PutObjectInfo(SerObjectInfoInit serObjectInfoInit, ReadObjectInfo objectInfo)
        {
            serObjectInfoInit.oiPool.Push(objectInfo);
        }

        internal void RecordFixup(long objectId, string name, long idRef)
        {
            if (this.isSi)
            {
                this.objectManager.RecordDelayedFixup(objectId, name, idRef);
            }
            else
            {
                this.objectManager.RecordFixup(objectId, this.cache.memberInfos[this.Position(name)], idRef);
            }
        }

        internal ArrayList SetFakeObject()
        {
            this.bfake = true;
            this.paramNameList = new ArrayList(10);
            return this.paramNameList;
        }

        internal void SetVersion(int major, int minor)
        {
            this.majorVersion = major;
            this.minorVersion = minor;
        }
    }
}

