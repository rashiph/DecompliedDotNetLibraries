namespace System.Reflection
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal class MemberInfoSerializationHolder : ISerializable, IObjectReference
    {
        private SerializationInfo m_info;
        private string m_memberName;
        private MemberTypes m_memberType;
        private RuntimeType m_reflectedType;
        private string m_signature;

        internal MemberInfoSerializationHolder(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            string assemblyName = info.GetString("AssemblyName");
            string name = info.GetString("ClassName");
            if ((assemblyName == null) || (name == null))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
            }
            Assembly assembly = FormatterServices.LoadAssemblyFromString(assemblyName);
            this.m_reflectedType = assembly.GetType(name, true, false) as RuntimeType;
            this.m_memberName = info.GetString("Name");
            this.m_signature = info.GetString("Signature");
            this.m_memberType = (MemberTypes) info.GetInt32("MemberType");
            this.m_info = info;
        }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
        }

        [SecurityCritical]
        public virtual object GetRealObject(StreamingContext context)
        {
            MethodInfo info;
            Type[] valueNoThrow;
            if (((this.m_memberName == null) || (this.m_reflectedType == null)) || (this.m_memberType == 0))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
            }
            BindingFlags bindingAttr = BindingFlags.OptionalParamBinding | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            switch (this.m_memberType)
            {
                case MemberTypes.Constructor:
                {
                    if (this.m_signature == null)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_NullSignature"));
                    }
                    ConstructorInfo[] infoArray4 = this.m_reflectedType.GetMember(this.m_memberName, MemberTypes.Constructor, bindingAttr) as ConstructorInfo[];
                    if (infoArray4.Length == 1)
                    {
                        return infoArray4[0];
                    }
                    if (infoArray4.Length > 1)
                    {
                        for (int i = 0; i < infoArray4.Length; i++)
                        {
                            if (infoArray4[i].ToString().Equals(this.m_signature))
                            {
                                return infoArray4[i];
                            }
                        }
                    }
                    throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMember", new object[] { this.m_memberName }));
                }
                case MemberTypes.Event:
                {
                    EventInfo[] infoArray2 = this.m_reflectedType.GetMember(this.m_memberName, MemberTypes.Event, bindingAttr) as EventInfo[];
                    if (infoArray2.Length == 0)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMember", new object[] { this.m_memberName }));
                    }
                    return infoArray2[0];
                }
                case MemberTypes.Field:
                {
                    FieldInfo[] infoArray = this.m_reflectedType.GetMember(this.m_memberName, MemberTypes.Field, bindingAttr) as FieldInfo[];
                    if (infoArray.Length == 0)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMember", new object[] { this.m_memberName }));
                    }
                    return infoArray[0];
                }
                case MemberTypes.Method:
                {
                    info = null;
                    if (this.m_signature == null)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_NullSignature"));
                    }
                    valueNoThrow = this.m_info.GetValueNoThrow("GenericArguments", typeof(Type[])) as Type[];
                    MethodInfo[] infoArray5 = this.m_reflectedType.GetMember(this.m_memberName, MemberTypes.Method, bindingAttr) as MethodInfo[];
                    if (infoArray5.Length == 1)
                    {
                        info = infoArray5[0];
                    }
                    else if (infoArray5.Length > 1)
                    {
                        for (int j = 0; j < infoArray5.Length; j++)
                        {
                            if (infoArray5[j].ToString().Equals(this.m_signature))
                            {
                                info = infoArray5[j];
                                break;
                            }
                            if (((valueNoThrow != null) && infoArray5[j].IsGenericMethod) && (infoArray5[j].GetGenericArguments().Length == valueNoThrow.Length))
                            {
                                MethodInfo info2 = infoArray5[j].MakeGenericMethod(valueNoThrow);
                                if (info2.ToString().Equals(this.m_signature))
                                {
                                    info = info2;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
                case MemberTypes.Property:
                {
                    PropertyInfo[] infoArray3 = this.m_reflectedType.GetMember(this.m_memberName, MemberTypes.Property, bindingAttr) as PropertyInfo[];
                    if (infoArray3.Length == 0)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMember", new object[] { this.m_memberName }));
                    }
                    if (infoArray3.Length == 1)
                    {
                        return infoArray3[0];
                    }
                    if (infoArray3.Length > 1)
                    {
                        for (int k = 0; k < infoArray3.Length; k++)
                        {
                            if (infoArray3[k].ToString().Equals(this.m_signature))
                            {
                                return infoArray3[k];
                            }
                        }
                    }
                    throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMember", new object[] { this.m_memberName }));
                }
                default:
                    throw new ArgumentException(Environment.GetResourceString("Serialization_MemberTypeNotRecognized"));
            }
            if (info == null)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMember", new object[] { this.m_memberName }));
            }
            if (!info.IsGenericMethodDefinition)
            {
                return info;
            }
            if (valueNoThrow == null)
            {
                return info;
            }
            if (valueNoThrow[0] == null)
            {
                return null;
            }
            return info.MakeGenericMethod(valueNoThrow);
        }

        public static void GetSerializationInfo(SerializationInfo info, string name, Type reflectedClass, string signature, MemberTypes type)
        {
            GetSerializationInfo(info, name, reflectedClass, signature, type, null);
        }

        public static void GetSerializationInfo(SerializationInfo info, string name, Type reflectedClass, string signature, MemberTypes type, Type[] genericArguments)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            string fullName = reflectedClass.Module.Assembly.FullName;
            string str2 = reflectedClass.FullName;
            info.SetType(typeof(MemberInfoSerializationHolder));
            info.AddValue("Name", name, typeof(string));
            info.AddValue("AssemblyName", fullName, typeof(string));
            info.AddValue("ClassName", str2, typeof(string));
            info.AddValue("Signature", signature, typeof(string));
            info.AddValue("MemberType", (int) type);
            info.AddValue("GenericArguments", genericArguments, typeof(Type[]));
        }
    }
}

