namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    internal static class FormatterServicesNoSerializableCheck
    {
        internal static readonly string FakeNameSeparatorString = "+";
        private static Dictionary<Type, MemberInfoName> m_MemberInfoTable = new Dictionary<Type, MemberInfoName>(0x20);
        private static object s_FormatterServicesSyncObject = null;

        private static bool CheckSerializable(Type type)
        {
            return true;
        }

        private static bool GetParentTypes(Type parentType, out Type[] parentTypes, out int parentTypeCount)
        {
            parentTypes = null;
            parentTypeCount = 0;
            bool flag = true;
            for (Type type = parentType; type != typeof(object); type = type.BaseType)
            {
                if (type.IsInterface)
                {
                    continue;
                }
                string name = type.Name;
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
                    Type[] destinationArray = new Type[Math.Max(parentTypeCount * 2, 12)];
                    if (parentTypes != null)
                    {
                        Array.Copy(parentTypes, 0, destinationArray, 0, parentTypeCount);
                    }
                    parentTypes = destinationArray;
                }
                parentTypes[parentTypeCount++] = type;
            }
            return flag;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public static MemberInfo[] GetSerializableMembers(Type type, out string[] names)
        {
            MemberInfoName name;
            names = null;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            lock (formatterServicesSyncObject)
            {
                if (m_MemberInfoTable.TryGetValue(type, out name))
                {
                    names = name.Names;
                    return name.MemberInfo;
                }
            }
            name.MemberInfo = InternalGetSerializableMembers(type, out name.Names);
            lock (formatterServicesSyncObject)
            {
                MemberInfoName name2;
                if (m_MemberInfoTable.TryGetValue(type, out name2))
                {
                    names = name2.Names;
                    return name2.MemberInfo;
                }
                m_MemberInfoTable[type] = name;
            }
            names = name.Names;
            return name.MemberInfo;
        }

        private static MemberInfo[] GetSerializableMembers2(Type type)
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

        private static MemberInfo[] InternalGetSerializableMembers(Type type, out string[] typeNames)
        {
            typeNames = null;
            ArrayList list = null;
            ArrayList list2 = null;
            if (type.IsInterface)
            {
                return new MemberInfo[0];
            }
            MemberInfo[] sourceArray = GetSerializableMembers2(type);
            if (sourceArray != null)
            {
                typeNames = new string[sourceArray.Length];
                for (int i = 0; i < sourceArray.Length; i++)
                {
                    typeNames[i] = sourceArray[i].Name;
                }
            }
            Type baseType = type.BaseType;
            if ((baseType != null) && (baseType != typeof(object)))
            {
                Type[] parentTypes = null;
                int parentTypeCount = 0;
                bool flag = GetParentTypes(baseType, out parentTypes, out parentTypeCount);
                if (parentTypeCount <= 0)
                {
                    return sourceArray;
                }
                list = new ArrayList();
                list2 = new ArrayList();
                for (int j = 0; j < parentTypeCount; j++)
                {
                    baseType = parentTypes[j];
                    if (!CheckSerializable(baseType))
                    {
                        throw new SerializationException();
                    }
                    FieldInfo[] fields = baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    string str = flag ? baseType.Name : baseType.FullName;
                    foreach (FieldInfo info in fields)
                    {
                        if (info.IsPrivate && !info.IsNotSerialized)
                        {
                            list.Add(info);
                            list2.Add(str + FakeNameSeparatorString + info.Name);
                        }
                    }
                }
                if ((list != null) && (list.Count > 0))
                {
                    MemberInfo[] destinationArray = new MemberInfo[list.Count + sourceArray.Length];
                    Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                    list.CopyTo(destinationArray, sourceArray.Length);
                    sourceArray = destinationArray;
                    string[] strArray = new string[list2.Count + typeNames.Length];
                    Array.Copy(typeNames, strArray, typeNames.Length);
                    list2.CopyTo(strArray, typeNames.Length);
                    typeNames = strArray;
                }
            }
            return sourceArray;
        }

        private static object formatterServicesSyncObject
        {
            get
            {
                if (s_FormatterServicesSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_FormatterServicesSyncObject, obj2, null);
                }
                return s_FormatterServicesSyncObject;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MemberInfoName
        {
            public System.Reflection.MemberInfo[] MemberInfo;
            public string[] Names;
        }
    }
}

