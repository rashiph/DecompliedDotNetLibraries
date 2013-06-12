namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class WriteObjectInfo
    {
        internal long assemId;
        private string binderAssemblyString;
        private string binderTypeName;
        internal SerObjectInfoCache cache;
        internal StreamingContext context;
        internal bool isArray;
        internal bool isNamed;
        internal bool isSi;
        internal bool isTyped;
        internal object[] memberData;
        internal object obj;
        internal long objectId;
        internal int objectInfoId;
        internal Type objectType;
        private static SecurityPermission serializationPermission = new SecurityPermission(SecurityPermissionFlag.SerializationFormatter);
        internal ISerializationSurrogate serializationSurrogate;
        internal SerObjectInfoInit serObjectInfoInit;
        internal SerializationInfo si;

        internal WriteObjectInfo()
        {
        }

        private static void CheckTypeForwardedFrom(SerObjectInfoCache cache, Type objectType, string binderAssemblyString)
        {
            if ((cache.hasTypeForwardedFrom && (binderAssemblyString == null)) && !FormatterServices.UnsafeTypeForwardersIsEnabled())
            {
                Assembly assembly = objectType.Assembly;
                if (!SerializationInfo.IsAssemblyNameAssignmentSafe(assembly.FullName, cache.assemblyString) && !assembly.IsFullyTrusted)
                {
                    throw new SecurityException(Environment.GetResourceString("Serialization_RequireFullTrust", new object[] { objectType }));
                }
            }
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
            return (this.binderAssemblyString ?? this.cache.assemblyString);
        }

        internal void GetMemberInfo(out string[] outMemberNames, out Type[] outMemberTypes, out object[] outMemberData)
        {
            outMemberNames = this.cache.memberNames;
            outMemberTypes = this.cache.memberTypes;
            outMemberData = this.memberData;
            if (this.isSi && !this.isNamed)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_ISerializableMemberInfo"));
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
                throw new SerializationException(Environment.GetResourceString("Serialization_SerMemberInfo", new object[] { objMember.GetType() }));
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

        internal string GetTypeFullName()
        {
            return (this.binderTypeName ?? this.cache.fullTypeName);
        }

        [SecurityCritical]
        private void InitMemberInfo()
        {
            this.cache = (SerObjectInfoCache) this.serObjectInfoInit.seenBeforeTable[this.objectType];
            if (this.cache == null)
            {
                this.cache = new SerObjectInfoCache(this.objectType);
                this.cache.memberInfos = FormatterServices.GetSerializableMembers(this.objectType, this.context);
                int length = this.cache.memberInfos.Length;
                this.cache.memberNames = new string[length];
                this.cache.memberTypes = new Type[length];
                for (int i = 0; i < length; i++)
                {
                    this.cache.memberNames[i] = this.cache.memberInfos[i].Name;
                    this.cache.memberTypes[i] = this.GetMemberType(this.cache.memberInfos[i]);
                }
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
                this.cache = new SerObjectInfoCache(this.objectType);
                this.serObjectInfoInit.seenBeforeTable.Add(this.objectType, this.cache);
            }
        }

        [SecurityCritical]
        internal void InitSerialize(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, SerializationBinder binder)
        {
            this.objectType = objectType;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;
            if (objectType.IsArray)
            {
                this.InitNoMembers();
            }
            else
            {
                this.InvokeSerializationBinder(binder);
                ISurrogateSelector selector = null;
                if (surrogateSelector != null)
                {
                    this.serializationSurrogate = surrogateSelector.GetSurrogate(objectType, context, out selector);
                }
                if (this.serializationSurrogate != null)
                {
                    this.si = new SerializationInfo(objectType, converter);
                    this.cache = new SerObjectInfoCache(objectType);
                    this.isSi = true;
                }
                else if (!object.ReferenceEquals(objectType, Converter.typeofObject) && Converter.typeofISerializable.IsAssignableFrom(objectType))
                {
                    this.si = new SerializationInfo(objectType, converter, !FormatterServices.UnsafeTypeForwardersIsEnabled());
                    this.cache = new SerObjectInfoCache(objectType);
                    CheckTypeForwardedFrom(this.cache, objectType, this.binderAssemblyString);
                    this.isSi = true;
                }
                if (!this.isSi)
                {
                    this.InitMemberInfo();
                    CheckTypeForwardedFrom(this.cache, objectType, this.binderAssemblyString);
                }
            }
        }

        [SecurityCritical]
        internal void InitSerialize(object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, ObjectWriter objectWriter, SerializationBinder binder)
        {
            this.context = context;
            this.obj = obj;
            this.serObjectInfoInit = serObjectInfoInit;
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
                this.isArray = true;
                this.InitNoMembers();
            }
            else
            {
                ISurrogateSelector selector;
                this.InvokeSerializationBinder(binder);
                objectWriter.ObjectManager.RegisterObject(obj);
                if ((surrogateSelector != null) && ((this.serializationSurrogate = surrogateSelector.GetSurrogate(this.objectType, context, out selector)) != null))
                {
                    this.si = new SerializationInfo(this.objectType, converter);
                    if (!this.objectType.IsPrimitive)
                    {
                        this.serializationSurrogate.GetObjectData(obj, this.si, context);
                    }
                    this.InitSiWrite();
                }
                else if (obj is ISerializable)
                {
                    if (!this.objectType.IsSerializable)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_NonSerType", new object[] { this.objectType.FullName, this.objectType.Assembly.FullName }));
                    }
                    this.si = new SerializationInfo(this.objectType, converter, !FormatterServices.UnsafeTypeForwardersIsEnabled());
                    ((ISerializable) obj).GetObjectData(this.si, context);
                    this.InitSiWrite();
                    CheckTypeForwardedFrom(this.cache, this.objectType, this.binderAssemblyString);
                }
                else
                {
                    this.InitMemberInfo();
                    CheckTypeForwardedFrom(this.cache, this.objectType, this.binderAssemblyString);
                }
            }
        }

        private void InitSiWrite()
        {
            SerializationInfoEnumerator enumerator = null;
            this.isSi = true;
            enumerator = this.si.GetEnumerator();
            int memberCount = this.si.MemberCount;
            TypeInformation typeInformation = null;
            string fullTypeName = this.si.FullTypeName;
            string assemblyName = this.si.AssemblyName;
            bool hasTypeForwardedFrom = false;
            if (!this.si.IsFullTypeNameSetExplicit)
            {
                typeInformation = BinaryFormatter.GetTypeInformation(this.si.ObjectType);
                fullTypeName = typeInformation.FullTypeName;
                hasTypeForwardedFrom = typeInformation.HasTypeForwardedFrom;
            }
            if (!this.si.IsAssemblyNameSetExplicit)
            {
                if (typeInformation == null)
                {
                    typeInformation = BinaryFormatter.GetTypeInformation(this.si.ObjectType);
                }
                assemblyName = typeInformation.AssemblyString;
                hasTypeForwardedFrom = typeInformation.HasTypeForwardedFrom;
            }
            this.cache = new SerObjectInfoCache(fullTypeName, assemblyName, hasTypeForwardedFrom);
            this.cache.memberNames = new string[memberCount];
            this.cache.memberTypes = new Type[memberCount];
            this.memberData = new object[memberCount];
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

        private void InternalInit()
        {
            this.obj = null;
            this.objectType = null;
            this.isSi = false;
            this.isNamed = false;
            this.isTyped = false;
            this.isArray = false;
            this.si = null;
            this.cache = null;
            this.memberData = null;
            this.objectId = 0L;
            this.assemId = 0L;
            this.binderTypeName = null;
            this.binderAssemblyString = null;
        }

        private void InvokeSerializationBinder(SerializationBinder binder)
        {
            if (binder != null)
            {
                binder.BindToName(this.objectType, out this.binderAssemblyString, out this.binderTypeName);
            }
        }

        internal void ObjectEnd()
        {
            PutObjectInfo(this.serObjectInfoInit, this);
        }

        private static void PutObjectInfo(SerObjectInfoInit serObjectInfoInit, WriteObjectInfo objectInfo)
        {
            serObjectInfoInit.oiPool.Push(objectInfo);
        }

        [SecurityCritical]
        internal static WriteObjectInfo Serialize(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, SerializationBinder binder)
        {
            WriteObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
            objectInfo.InitSerialize(objectType, surrogateSelector, context, serObjectInfoInit, converter, binder);
            return objectInfo;
        }

        [SecurityCritical]
        internal static WriteObjectInfo Serialize(object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, ObjectWriter objectWriter, SerializationBinder binder)
        {
            WriteObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
            objectInfo.InitSerialize(obj, surrogateSelector, context, serObjectInfoInit, converter, objectWriter, binder);
            return objectInfo;
        }
    }
}

