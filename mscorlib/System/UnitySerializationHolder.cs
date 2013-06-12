namespace System
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal class UnitySerializationHolder : ISerializable, IObjectReference
    {
        internal const int Array = 2;
        internal const int AssemblyUnity = 6;
        internal const int ByRef = 4;
        internal const int EmptyUnity = 1;
        internal const int GenericParameterTypeUnity = 7;
        private string m_assemblyName;
        private string m_data;
        private MethodBase m_declaringMethod;
        private Type m_declaringType;
        private int[] m_elementTypes;
        private int m_genericParameterPosition;
        private Type[] m_instantiation;
        private int m_unityType;
        internal const int MissingUnity = 3;
        internal const int ModuleUnity = 5;
        internal const int NullUnity = 2;
        internal const int PartialInstantiationTypeUnity = 8;
        internal const int Pointer = 1;
        internal const int RuntimeTypeUnity = 4;
        internal const int SzArray = 3;

        internal UnitySerializationHolder(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.m_unityType = info.GetInt32("UnityType");
            if (this.m_unityType != 3)
            {
                if (this.m_unityType == 7)
                {
                    this.m_declaringMethod = info.GetValue("DeclaringMethod", typeof(MethodBase)) as MethodBase;
                    this.m_declaringType = info.GetValue("DeclaringType", typeof(Type)) as Type;
                    this.m_genericParameterPosition = info.GetInt32("GenericParameterPosition");
                    this.m_elementTypes = info.GetValue("ElementTypes", typeof(int[])) as int[];
                }
                else
                {
                    if (this.m_unityType == 8)
                    {
                        this.m_instantiation = info.GetValue("GenericArguments", typeof(Type[])) as Type[];
                        this.m_elementTypes = info.GetValue("ElementTypes", typeof(int[])) as int[];
                    }
                    this.m_data = info.GetString("Data");
                    this.m_assemblyName = info.GetString("AssemblyName");
                }
            }
        }

        internal static RuntimeType AddElementTypes(SerializationInfo info, RuntimeType type)
        {
            List<int> list = new List<int>();
            while (type.HasElementType)
            {
                if (type.IsSzArray)
                {
                    list.Add(3);
                }
                else if (type.IsArray)
                {
                    list.Add(type.GetArrayRank());
                    list.Add(2);
                }
                else if (type.IsPointer)
                {
                    list.Add(1);
                }
                else if (type.IsByRef)
                {
                    list.Add(4);
                }
                type = (RuntimeType) type.GetElementType();
            }
            info.AddValue("ElementTypes", list.ToArray(), typeof(int[]));
            return type;
        }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnitySerHolder"));
        }

        [SecurityCritical]
        public virtual object GetRealObject(StreamingContext context)
        {
            switch (this.m_unityType)
            {
                case 1:
                    return Empty.Value;

                case 2:
                    return DBNull.Value;

                case 3:
                    return Missing.Value;

                case 4:
                    if ((this.m_data == null) || (this.m_data.Length == 0))
                    {
                        this.ThrowInsufficientInformation("Data");
                    }
                    if (this.m_assemblyName == null)
                    {
                        this.ThrowInsufficientInformation("AssemblyName");
                    }
                    if (this.m_assemblyName.Length == 0)
                    {
                        return Type.GetType(this.m_data, true, false);
                    }
                    return Assembly.Load(this.m_assemblyName).GetType(this.m_data, true, false);

                case 5:
                {
                    if ((this.m_data == null) || (this.m_data.Length == 0))
                    {
                        this.ThrowInsufficientInformation("Data");
                    }
                    if (this.m_assemblyName == null)
                    {
                        this.ThrowInsufficientInformation("AssemblyName");
                    }
                    Module module = Assembly.Load(this.m_assemblyName).GetModule(this.m_data);
                    if (module == null)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_UnableToFindModule", new object[] { this.m_data, this.m_assemblyName }));
                    }
                    return module;
                }
                case 6:
                    if ((this.m_data == null) || (this.m_data.Length == 0))
                    {
                        this.ThrowInsufficientInformation("Data");
                    }
                    if (this.m_assemblyName == null)
                    {
                        this.ThrowInsufficientInformation("AssemblyName");
                    }
                    return Assembly.Load(this.m_assemblyName);

                case 7:
                    if ((this.m_declaringMethod == null) && (this.m_declaringType == null))
                    {
                        this.ThrowInsufficientInformation("DeclaringMember");
                    }
                    if (this.m_declaringMethod != null)
                    {
                        return this.m_declaringMethod.GetGenericArguments()[this.m_genericParameterPosition];
                    }
                    return this.MakeElementTypes(this.m_declaringType.GetGenericArguments()[this.m_genericParameterPosition]);

                case 8:
                {
                    this.m_unityType = 4;
                    Type realObject = this.GetRealObject(context) as Type;
                    this.m_unityType = 8;
                    if (this.m_instantiation[0] != null)
                    {
                        return this.MakeElementTypes(realObject.MakeGenericType(this.m_instantiation));
                    }
                    return null;
                }
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidUnity"));
        }

        internal static void GetUnitySerializationInfo(SerializationInfo info, Missing missing)
        {
            info.SetType(typeof(UnitySerializationHolder));
            info.AddValue("UnityType", 3);
        }

        internal static void GetUnitySerializationInfo(SerializationInfo info, RuntimeType type)
        {
            if (type.GetRootElementType().IsGenericParameter)
            {
                type = AddElementTypes(info, type);
                info.SetType(typeof(UnitySerializationHolder));
                info.AddValue("UnityType", 7);
                info.AddValue("GenericParameterPosition", type.GenericParameterPosition);
                info.AddValue("DeclaringMethod", type.DeclaringMethod, typeof(MethodBase));
                info.AddValue("DeclaringType", type.DeclaringType, typeof(Type));
            }
            else
            {
                int unityType = 4;
                if (!type.IsGenericTypeDefinition && type.ContainsGenericParameters)
                {
                    unityType = 8;
                    type = AddElementTypes(info, type);
                    info.AddValue("GenericArguments", type.GetGenericArguments(), typeof(Type[]));
                    type = (RuntimeType) type.GetGenericTypeDefinition();
                }
                GetUnitySerializationInfo(info, unityType, type.FullName, type.GetRuntimeAssembly());
            }
        }

        internal static void GetUnitySerializationInfo(SerializationInfo info, int unityType, string data, RuntimeAssembly assembly)
        {
            string fullName;
            info.SetType(typeof(UnitySerializationHolder));
            info.AddValue("Data", data, typeof(string));
            info.AddValue("UnityType", unityType);
            if (assembly == null)
            {
                fullName = string.Empty;
            }
            else
            {
                fullName = assembly.FullName;
            }
            info.AddValue("AssemblyName", fullName);
        }

        internal Type MakeElementTypes(Type type)
        {
            for (int i = this.m_elementTypes.Length - 1; i >= 0; i--)
            {
                if (this.m_elementTypes[i] == 3)
                {
                    type = type.MakeArrayType();
                }
                else if (this.m_elementTypes[i] == 2)
                {
                    type = type.MakeArrayType(this.m_elementTypes[--i]);
                }
                else if (this.m_elementTypes[i] == 1)
                {
                    type = type.MakePointerType();
                }
                else if (this.m_elementTypes[i] == 4)
                {
                    type = type.MakeByRefType();
                }
            }
            return type;
        }

        private void ThrowInsufficientInformation(string field)
        {
            throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientDeserializationState", new object[] { field }));
        }
    }
}

