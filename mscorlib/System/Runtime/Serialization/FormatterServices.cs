namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Text;
    using System.Threading;

    [ComVisible(true)]
    public sealed class FormatterServices
    {
        private static readonly Type[] advancedTypes = new Type[] { typeof(DelegateSerializationHolder), typeof(ObjRef), typeof(IEnvoyInfo), typeof(ISponsor) };
        internal static Dictionary<MemberHolder, MemberInfo[]> m_MemberInfoTable = new Dictionary<MemberHolder, MemberInfo[]>(0x20);
        private static Binder s_binder = Type.DefaultBinder;
        private static object s_FormatterServicesSyncObject = null;
        [SecurityCritical]
        private static bool unsafeTypeForwardersIsEnabled = false;
        [SecurityCritical]
        private static volatile bool unsafeTypeForwardersIsEnabledInitialized = false;

        private FormatterServices()
        {
            throw new NotSupportedException();
        }

        private static bool CheckSerializable(RuntimeType type)
        {
            return type.IsSerializable;
        }

        public static void CheckTypeSecurity(Type t, TypeFilterLevel securityLevel)
        {
            if (securityLevel == TypeFilterLevel.Low)
            {
                for (int i = 0; i < advancedTypes.Length; i++)
                {
                    if (advancedTypes[i].IsAssignableFrom(t))
                    {
                        throw new SecurityException(Environment.GetResourceString("Serialization_TypeSecurity", new object[] { advancedTypes[i].FullName, t.FullName }));
                    }
                }
            }
        }

        internal static string GetClrAssemblyName(Type type, out bool hasTypeForwardedFrom)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            object[] customAttributes = type.GetCustomAttributes(typeof(TypeForwardedFromAttribute), false);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                hasTypeForwardedFrom = true;
                TypeForwardedFromAttribute attribute = (TypeForwardedFromAttribute) customAttributes[0];
                return attribute.AssemblyFullName;
            }
            hasTypeForwardedFrom = false;
            return type.Assembly.FullName;
        }

        internal static string GetClrTypeFullName(Type type)
        {
            if (type.IsArray)
            {
                return GetClrTypeFullNameForArray(type);
            }
            return GetClrTypeFullNameForNonArrayTypes(type);
        }

        private static string GetClrTypeFullNameForArray(Type type)
        {
            int arrayRank = type.GetArrayRank();
            if (arrayRank == 1)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { GetClrTypeFullName(type.GetElementType()), "[]" });
            }
            StringBuilder builder = new StringBuilder(GetClrTypeFullName(type.GetElementType())).Append("[");
            for (int i = 1; i < arrayRank; i++)
            {
                builder.Append(",");
            }
            builder.Append("]");
            return builder.ToString();
        }

        private static string GetClrTypeFullNameForNonArrayTypes(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.FullName;
            }
            Type[] genericArguments = type.GetGenericArguments();
            StringBuilder builder = new StringBuilder(type.GetGenericTypeDefinition().FullName).Append("[");
            foreach (Type type2 in genericArguments)
            {
                bool flag;
                builder.Append("[").Append(GetClrTypeFullName(type2)).Append(", ");
                builder.Append(GetClrAssemblyName(type2, out flag)).Append("],");
            }
            return builder.Remove(builder.Length - 1, 1).Append("]").ToString();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool GetEnableUnsafeTypeForwarders();
        [SecurityCritical]
        public static object[] GetObjectData(object obj, MemberInfo[] members)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (members == null)
            {
                throw new ArgumentNullException("members");
            }
            int length = members.Length;
            object[] objArray = new object[length];
            for (int i = 0; i < length; i++)
            {
                MemberInfo info = members[i];
                if (info == null)
                {
                    throw new ArgumentNullException("members", Environment.GetResourceString("ArgumentNull_NullMember", new object[] { i }));
                }
                if (info.MemberType != MemberTypes.Field)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMemberInfo"));
                }
                RtFieldInfo info2 = info as RtFieldInfo;
                if (info2 != null)
                {
                    objArray[i] = info2.InternalGetValue(obj, false);
                }
                else
                {
                    objArray[i] = ((SerializationFieldInfo) info).InternalGetValue(obj, false);
                }
            }
            return objArray;
        }

        private static bool GetParentTypes(RuntimeType parentType, out RuntimeType[] parentTypes, out int parentTypeCount)
        {
            parentTypes = null;
            parentTypeCount = 0;
            bool flag = true;
            RuntimeType type = (RuntimeType) typeof(object);
            for (RuntimeType type2 = parentType; type2 != type; type2 = (RuntimeType) type2.BaseType)
            {
                if (type2.IsInterface)
                {
                    continue;
                }
                string name = type2.Name;
                for (int i = 0; flag && (i < parentTypeCount); i++)
                {
                    string str2 = parentTypes[i].Name;
                    if (((str2.Length == name.Length) && (str2[0] == name[0])) && (name == str2))
                    {
                        flag = false;
                        break;
                    }
                }
                if ((parentTypes == null) || (parentTypeCount == parentTypes.Length))
                {
                    RuntimeType[] destinationArray = new RuntimeType[Math.Max(parentTypeCount * 2, 12)];
                    if (parentTypes != null)
                    {
                        Array.Copy(parentTypes, 0, destinationArray, 0, parentTypeCount);
                    }
                    parentTypes = destinationArray;
                }
                parentTypes[parentTypeCount++] = type2;
            }
            return flag;
        }

        [SecurityCritical]
        public static object GetSafeUninitializedObject(Type type)
        {
            object obj2;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!(type is RuntimeType))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidType", new object[] { type.ToString() }));
            }
            if ((object.ReferenceEquals(type, typeof(ConstructionCall)) || object.ReferenceEquals(type, typeof(LogicalCallContext))) || object.ReferenceEquals(type, typeof(SynchronizationAttribute)))
            {
                return nativeGetUninitializedObject((RuntimeType) type);
            }
            try
            {
                obj2 = nativeGetSafeUninitializedObject((RuntimeType) type);
            }
            catch (SecurityException exception)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_Security", new object[] { type.FullName }), exception);
            }
            return obj2;
        }

        private static MemberInfo[] GetSerializableMembers(RuntimeType type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            int index = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                if ((fields[i].Attributes & FieldAttributes.NotSerialized) != FieldAttributes.NotSerialized)
                {
                    index++;
                }
            }
            if (index == fields.Length)
            {
                return fields;
            }
            FieldInfo[] infoArray2 = new FieldInfo[index];
            index = 0;
            for (int j = 0; j < fields.Length; j++)
            {
                if ((fields[j].Attributes & FieldAttributes.NotSerialized) != FieldAttributes.NotSerialized)
                {
                    infoArray2[index] = fields[j];
                    index++;
                }
            }
            return infoArray2;
        }

        [SecurityCritical]
        public static MemberInfo[] GetSerializableMembers(Type type)
        {
            return GetSerializableMembers(type, new StreamingContext(StreamingContextStates.All));
        }

        [SecurityCritical]
        public static MemberInfo[] GetSerializableMembers(Type type, StreamingContext context)
        {
            MemberInfo[] serializableMembers;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!(type is RuntimeType))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidType", new object[] { type.ToString() }));
            }
            MemberHolder key = new MemberHolder(type, context);
            if (m_MemberInfoTable.ContainsKey(key))
            {
                return m_MemberInfoTable[key];
            }
            lock (formatterServicesSyncObject)
            {
                if (m_MemberInfoTable.ContainsKey(key))
                {
                    return m_MemberInfoTable[key];
                }
                serializableMembers = InternalGetSerializableMembers((RuntimeType) type);
                m_MemberInfoTable[key] = serializableMembers;
            }
            return serializableMembers;
        }

        [SecurityCritical, ComVisible(false)]
        public static ISerializationSurrogate GetSurrogateForCyclicalReference(ISerializationSurrogate innerSurrogate)
        {
            if (innerSurrogate == null)
            {
                throw new ArgumentNullException("innerSurrogate");
            }
            return new SurrogateForCyclicalReference(innerSurrogate);
        }

        [SecurityCritical]
        public static Type GetTypeFromAssembly(Assembly assem, string name)
        {
            if (assem == null)
            {
                throw new ArgumentNullException("assem");
            }
            return assem.GetType(name, false, false);
        }

        [SecurityCritical]
        public static object GetUninitializedObject(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!(type is RuntimeType))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidType", new object[] { type.ToString() }));
            }
            return nativeGetUninitializedObject((RuntimeType) type);
        }

        private static MemberInfo[] InternalGetSerializableMembers(RuntimeType type)
        {
            List<SerializationFieldInfo> list = null;
            if (type.IsInterface)
            {
                return new MemberInfo[0];
            }
            if (!CheckSerializable(type))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_NonSerType", new object[] { type.FullName, type.Module.Assembly.FullName }));
            }
            MemberInfo[] serializableMembers = GetSerializableMembers(type);
            RuntimeType baseType = (RuntimeType) type.BaseType;
            if ((baseType != null) && (baseType != ((RuntimeType) typeof(object))))
            {
                RuntimeType[] parentTypes = null;
                int parentTypeCount = 0;
                bool flag = GetParentTypes(baseType, out parentTypes, out parentTypeCount);
                if (parentTypeCount <= 0)
                {
                    return serializableMembers;
                }
                list = new List<SerializationFieldInfo>();
                for (int i = 0; i < parentTypeCount; i++)
                {
                    baseType = parentTypes[i];
                    if (!CheckSerializable(baseType))
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_NonSerType", new object[] { baseType.FullName, baseType.Module.Assembly.FullName }));
                    }
                    FieldInfo[] fields = baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    string namePrefix = flag ? baseType.Name : baseType.FullName;
                    foreach (FieldInfo info in fields)
                    {
                        if (!info.IsNotSerialized)
                        {
                            list.Add(new SerializationFieldInfo((RuntimeFieldInfo) info, namePrefix));
                        }
                    }
                }
                if ((list != null) && (list.Count > 0))
                {
                    MemberInfo[] destinationArray = new MemberInfo[list.Count + serializableMembers.Length];
                    Array.Copy(serializableMembers, destinationArray, serializableMembers.Length);
                    list.CopyTo(destinationArray, serializableMembers.Length);
                    serializableMembers = destinationArray;
                }
            }
            return serializableMembers;
        }

        internal static Assembly LoadAssemblyFromString(string assemblyName)
        {
            return Assembly.Load(assemblyName);
        }

        internal static Assembly LoadAssemblyFromStringNoThrow(string assemblyName)
        {
            try
            {
                return LoadAssemblyFromString(assemblyName);
            }
            catch (Exception)
            {
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern object nativeGetSafeUninitializedObject(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern object nativeGetUninitializedObject(RuntimeType type);
        [SecurityCritical]
        public static object PopulateObjectMembers(object obj, MemberInfo[] members, object[] data)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (members == null)
            {
                throw new ArgumentNullException("members");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (members.Length != data.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_DataLengthDifferent"));
            }
            for (int i = 0; i < members.Length; i++)
            {
                MemberInfo fi = members[i];
                if (fi == null)
                {
                    throw new ArgumentNullException("members", Environment.GetResourceString("ArgumentNull_NullMember", new object[] { i }));
                }
                if (data[i] != null)
                {
                    if (fi.MemberType != MemberTypes.Field)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMemberInfo"));
                    }
                    SerializationSetValue(fi, obj, data[i]);
                }
            }
            return obj;
        }

        [SecurityCritical]
        internal static void SerializationSetValue(MemberInfo fi, object target, object value)
        {
            RtFieldInfo info = fi as RtFieldInfo;
            if (info != null)
            {
                info.InternalSetValue(target, value, BindingFlags.Default, s_binder, null, false);
            }
            else
            {
                SerializationFieldInfo info2 = fi as SerializationFieldInfo;
                if (info2 == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFieldInfo"));
                }
                info2.InternalSetValue(target, value, BindingFlags.Default, s_binder, null, false, true);
            }
        }

        [SecuritySafeCritical]
        internal static bool UnsafeTypeForwardersIsEnabled()
        {
            if (!unsafeTypeForwardersIsEnabledInitialized)
            {
                unsafeTypeForwardersIsEnabled = GetEnableUnsafeTypeForwarders();
                unsafeTypeForwardersIsEnabledInitialized = true;
            }
            return unsafeTypeForwardersIsEnabled;
        }

        private static object formatterServicesSyncObject
        {
            get
            {
                if (s_FormatterServicesSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange<object>(ref s_FormatterServicesSyncObject, obj2, null);
                }
                return s_FormatterServicesSyncObject;
            }
        }
    }
}

