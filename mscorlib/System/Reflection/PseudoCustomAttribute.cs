namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    internal static class PseudoCustomAttribute
    {
        private static Dictionary<RuntimeType, RuntimeType> s_pca;
        private static int s_pcasCount;

        [SecurityCritical]
        static PseudoCustomAttribute()
        {
            RuntimeType[] typeArray = new RuntimeType[] { typeof(FieldOffsetAttribute) as RuntimeType, typeof(SerializableAttribute) as RuntimeType, typeof(MarshalAsAttribute) as RuntimeType, typeof(ComImportAttribute) as RuntimeType, typeof(NonSerializedAttribute) as RuntimeType, typeof(InAttribute) as RuntimeType, typeof(OutAttribute) as RuntimeType, typeof(OptionalAttribute) as RuntimeType, typeof(DllImportAttribute) as RuntimeType, typeof(PreserveSigAttribute) as RuntimeType, typeof(TypeForwardedToAttribute) as RuntimeType };
            s_pcasCount = typeArray.Length;
            Dictionary<RuntimeType, RuntimeType> dictionary = new Dictionary<RuntimeType, RuntimeType>(s_pcasCount);
            for (int i = 0; i < s_pcasCount; i++)
            {
                dictionary[typeArray[i]] = typeArray[i];
            }
            s_pca = dictionary;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetSecurityAttributes(RuntimeModule module, int token, bool assembly, out object[] securityAttributes);
        internal static Attribute[] GetCustomAttributes(RuntimeEventInfo e, RuntimeType caType, out int count)
        {
            count = 0;
            return null;
        }

        [SecurityCritical]
        internal static Attribute[] GetCustomAttributes(RuntimeFieldInfo field, RuntimeType caType, out int count)
        {
            count = 0;
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if (!flag && (s_pca.GetValueOrDefault(caType) == null))
            {
                return null;
            }
            Attribute[] attributeArray = new Attribute[s_pcasCount];
            Attribute customAttribute = null;
            if (flag || (caType == ((RuntimeType) typeof(MarshalAsAttribute))))
            {
                customAttribute = MarshalAsAttribute.GetCustomAttribute(field);
                if (customAttribute != null)
                {
                    attributeArray[count++] = customAttribute;
                }
            }
            if (flag || (caType == ((RuntimeType) typeof(FieldOffsetAttribute))))
            {
                customAttribute = FieldOffsetAttribute.GetCustomAttribute(field);
                if (customAttribute != null)
                {
                    attributeArray[count++] = customAttribute;
                }
            }
            if (flag || (caType == ((RuntimeType) typeof(NonSerializedAttribute))))
            {
                customAttribute = NonSerializedAttribute.GetCustomAttribute(field);
                if (customAttribute != null)
                {
                    attributeArray[count++] = customAttribute;
                }
            }
            return attributeArray;
        }

        internal static Attribute[] GetCustomAttributes(RuntimeModule module, RuntimeType caType, out int count)
        {
            count = 0;
            return null;
        }

        [SecurityCritical]
        internal static Attribute[] GetCustomAttributes(RuntimeParameterInfo parameter, RuntimeType caType, out int count)
        {
            count = 0;
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if (!flag && (s_pca.GetValueOrDefault(caType) == null))
            {
                return null;
            }
            Attribute[] attributeArray = new Attribute[s_pcasCount];
            Attribute customAttribute = null;
            if (flag || (caType == ((RuntimeType) typeof(InAttribute))))
            {
                customAttribute = InAttribute.GetCustomAttribute(parameter);
                if (customAttribute != null)
                {
                    attributeArray[count++] = customAttribute;
                }
            }
            if (flag || (caType == ((RuntimeType) typeof(OutAttribute))))
            {
                customAttribute = OutAttribute.GetCustomAttribute(parameter);
                if (customAttribute != null)
                {
                    attributeArray[count++] = customAttribute;
                }
            }
            if (flag || (caType == ((RuntimeType) typeof(OptionalAttribute))))
            {
                customAttribute = OptionalAttribute.GetCustomAttribute(parameter);
                if (customAttribute != null)
                {
                    attributeArray[count++] = customAttribute;
                }
            }
            if (flag || (caType == ((RuntimeType) typeof(MarshalAsAttribute))))
            {
                customAttribute = MarshalAsAttribute.GetCustomAttribute(parameter);
                if (customAttribute != null)
                {
                    attributeArray[count++] = customAttribute;
                }
            }
            return attributeArray;
        }

        internal static Attribute[] GetCustomAttributes(RuntimePropertyInfo property, RuntimeType caType, out int count)
        {
            count = 0;
            return null;
        }

        [SecurityCritical]
        internal static Attribute[] GetCustomAttributes(RuntimeAssembly assembly, RuntimeType caType, bool includeSecCa, out int count)
        {
            count = 0;
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if ((!flag && (s_pca.GetValueOrDefault(caType) == null)) && !IsSecurityAttribute(caType))
            {
                return new Attribute[0];
            }
            List<Attribute> list = new List<Attribute>();
            if (includeSecCa && (flag || IsSecurityAttribute(caType)))
            {
                object[] objArray;
                GetSecurityAttributes(assembly.ManifestModule.ModuleHandle.GetRuntimeModule(), RuntimeAssembly.GetToken(assembly.GetNativeHandle()), true, out objArray);
                if (objArray != null)
                {
                    foreach (object obj2 in objArray)
                    {
                        if ((caType == obj2.GetType()) || obj2.GetType().IsSubclassOf(caType))
                        {
                            list.Add((Attribute) obj2);
                        }
                    }
                }
            }
            count = list.Count;
            return list.ToArray();
        }

        [SecurityCritical]
        internal static Attribute[] GetCustomAttributes(RuntimeConstructorInfo ctor, RuntimeType caType, bool includeSecCa, out int count)
        {
            count = 0;
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if ((!flag && (s_pca.GetValueOrDefault(caType) == null)) && !IsSecurityAttribute(caType))
            {
                return new Attribute[0];
            }
            List<Attribute> list = new List<Attribute>();
            if (includeSecCa && (flag || IsSecurityAttribute(caType)))
            {
                object[] objArray;
                GetSecurityAttributes(ctor.Module.ModuleHandle.GetRuntimeModule(), ctor.MetadataToken, false, out objArray);
                if (objArray != null)
                {
                    foreach (object obj2 in objArray)
                    {
                        if ((caType == obj2.GetType()) || obj2.GetType().IsSubclassOf(caType))
                        {
                            list.Add((Attribute) obj2);
                        }
                    }
                }
            }
            count = list.Count;
            return list.ToArray();
        }

        [SecurityCritical]
        internal static Attribute[] GetCustomAttributes(RuntimeMethodInfo method, RuntimeType caType, bool includeSecCa, out int count)
        {
            count = 0;
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if ((!flag && (s_pca.GetValueOrDefault(caType) == null)) && !IsSecurityAttribute(caType))
            {
                return new Attribute[0];
            }
            List<Attribute> list = new List<Attribute>();
            Attribute item = null;
            if (flag || (caType == ((RuntimeType) typeof(DllImportAttribute))))
            {
                item = DllImportAttribute.GetCustomAttribute(method);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            if (flag || (caType == ((RuntimeType) typeof(PreserveSigAttribute))))
            {
                item = PreserveSigAttribute.GetCustomAttribute(method);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            if (includeSecCa && (flag || IsSecurityAttribute(caType)))
            {
                object[] objArray;
                GetSecurityAttributes(method.Module.ModuleHandle.GetRuntimeModule(), method.MetadataToken, false, out objArray);
                if (objArray != null)
                {
                    foreach (object obj2 in objArray)
                    {
                        if ((caType == obj2.GetType()) || obj2.GetType().IsSubclassOf(caType))
                        {
                            list.Add((Attribute) obj2);
                        }
                    }
                }
            }
            count = list.Count;
            return list.ToArray();
        }

        [SecurityCritical]
        internal static Attribute[] GetCustomAttributes(RuntimeType type, RuntimeType caType, bool includeSecCa, out int count)
        {
            count = 0;
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if ((!flag && (s_pca.GetValueOrDefault(caType) == null)) && !IsSecurityAttribute(caType))
            {
                return new Attribute[0];
            }
            List<Attribute> list = new List<Attribute>();
            Attribute item = null;
            if (flag || (caType == ((RuntimeType) typeof(SerializableAttribute))))
            {
                item = SerializableAttribute.GetCustomAttribute(type);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            if (flag || (caType == ((RuntimeType) typeof(ComImportAttribute))))
            {
                item = ComImportAttribute.GetCustomAttribute(type);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            if ((includeSecCa && (flag || IsSecurityAttribute(caType))) && (!type.IsGenericParameter && (type.GetElementType() == null)))
            {
                object[] objArray;
                if (type.IsGenericType)
                {
                    type = (RuntimeType) type.GetGenericTypeDefinition();
                }
                GetSecurityAttributes(type.Module.ModuleHandle.GetRuntimeModule(), type.MetadataToken, false, out objArray);
                if (objArray != null)
                {
                    foreach (object obj2 in objArray)
                    {
                        if ((caType == obj2.GetType()) || obj2.GetType().IsSubclassOf(caType))
                        {
                            list.Add((Attribute) obj2);
                        }
                    }
                }
            }
            count = list.Count;
            return list.ToArray();
        }

        [SecurityCritical]
        internal static void GetSecurityAttributes(RuntimeModule module, int token, bool assembly, out object[] securityAttributes)
        {
            _GetSecurityAttributes(module.GetNativeHandle(), token, assembly, out securityAttributes);
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeAssembly assembly, RuntimeType caType)
        {
            int num;
            return (GetCustomAttributes(assembly, caType, true, out num).Length > 0);
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeConstructorInfo ctor, RuntimeType caType)
        {
            int num;
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if (!flag && (s_pca.GetValueOrDefault(caType) == null))
            {
                return false;
            }
            return ((flag || IsSecurityAttribute(caType)) && (GetCustomAttributes(ctor, caType, true, out num).Length != 0));
        }

        internal static bool IsDefined(RuntimeEventInfo e, RuntimeType caType)
        {
            return false;
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeFieldInfo field, RuntimeType caType)
        {
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if (!flag && (s_pca.GetValueOrDefault(caType) == null))
            {
                return false;
            }
            return (((flag || (caType == ((RuntimeType) typeof(MarshalAsAttribute)))) && MarshalAsAttribute.IsDefined(field)) || (((flag || (caType == ((RuntimeType) typeof(FieldOffsetAttribute)))) && FieldOffsetAttribute.IsDefined(field)) || ((flag || (caType == ((RuntimeType) typeof(NonSerializedAttribute)))) && NonSerializedAttribute.IsDefined(field))));
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeMethodInfo method, RuntimeType caType)
        {
            int num;
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if (!flag && (s_pca.GetValueOrDefault(caType) == null))
            {
                return false;
            }
            return (((flag || (caType == ((RuntimeType) typeof(DllImportAttribute)))) && DllImportAttribute.IsDefined(method)) || (((flag || (caType == ((RuntimeType) typeof(PreserveSigAttribute)))) && PreserveSigAttribute.IsDefined(method)) || ((flag || IsSecurityAttribute(caType)) && (GetCustomAttributes(method, caType, true, out num).Length != 0))));
        }

        internal static bool IsDefined(RuntimeModule module, RuntimeType caType)
        {
            return false;
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeParameterInfo parameter, RuntimeType caType)
        {
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if (!flag && (s_pca.GetValueOrDefault(caType) == null))
            {
                return false;
            }
            return (((flag || (caType == ((RuntimeType) typeof(InAttribute)))) && InAttribute.IsDefined(parameter)) || (((flag || (caType == ((RuntimeType) typeof(OutAttribute)))) && OutAttribute.IsDefined(parameter)) || (((flag || (caType == ((RuntimeType) typeof(OptionalAttribute)))) && OptionalAttribute.IsDefined(parameter)) || ((flag || (caType == ((RuntimeType) typeof(MarshalAsAttribute)))) && MarshalAsAttribute.IsDefined(parameter)))));
        }

        internal static bool IsDefined(RuntimePropertyInfo property, RuntimeType caType)
        {
            return false;
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeType type, RuntimeType caType)
        {
            int num;
            bool flag = (caType == ((RuntimeType) typeof(object))) || (caType == ((RuntimeType) typeof(Attribute)));
            if ((!flag && (s_pca.GetValueOrDefault(caType) == null)) && !IsSecurityAttribute(caType))
            {
                return false;
            }
            return (((flag || (caType == ((RuntimeType) typeof(SerializableAttribute)))) && SerializableAttribute.IsDefined(type)) || (((flag || (caType == ((RuntimeType) typeof(ComImportAttribute)))) && ComImportAttribute.IsDefined(type)) || ((flag || IsSecurityAttribute(caType)) && (GetCustomAttributes(type, caType, true, out num).Length != 0))));
        }

        internal static bool IsSecurityAttribute(RuntimeType type)
        {
            if (!(type == ((RuntimeType) typeof(SecurityAttribute))))
            {
                return type.IsSubclassOf(typeof(SecurityAttribute));
            }
            return true;
        }

        [SecurityCritical, Conditional("_DEBUG")]
        private static void VerifyPseudoCustomAttribute(RuntimeType pca)
        {
            CustomAttribute.GetAttributeUsage(pca);
        }
    }
}

