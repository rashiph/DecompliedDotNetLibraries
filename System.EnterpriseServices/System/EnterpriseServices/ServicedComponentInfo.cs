namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal static class ServicedComponentInfo
    {
        private static Hashtable _ExecuteMessageCache = new Hashtable();
        private static RWHashTable _MICache = new RWHashTable();
        private static RWHashTable _SCICache = new RWHashTable();
        internal const int MI_AUTODONE = 2;
        internal const int MI_EXECUTEMESSAGEVALID = 8;
        internal const int MI_HASSPECIALATTRIBUTES = 4;
        internal const int MI_PRESENT = 1;
        internal const int SCI_CLASSINTERFACE = 0x40;
        internal const int SCI_EVENTSOURCE = 4;
        internal const int SCI_JIT = 8;
        internal const int SCI_METHODSSECURE = 0x20;
        internal const int SCI_OBJECTPOOLED = 0x10;
        internal const int SCI_PRESENT = 1;
        internal const int SCI_SERVICEDCOMPONENT = 2;

        static ServicedComponentInfo()
        {
            AddExecuteMethodValidTypes();
        }

        private static void AddExecuteMethodValidTypes()
        {
            _ExecuteMessageCache.Add(typeof(bool), true);
            _ExecuteMessageCache.Add(typeof(byte), true);
            _ExecuteMessageCache.Add(typeof(char), true);
            _ExecuteMessageCache.Add(typeof(DateTime), true);
            _ExecuteMessageCache.Add(typeof(decimal), true);
            _ExecuteMessageCache.Add(typeof(double), true);
            _ExecuteMessageCache.Add(typeof(Guid), true);
            _ExecuteMessageCache.Add(typeof(short), true);
            _ExecuteMessageCache.Add(typeof(int), true);
            _ExecuteMessageCache.Add(typeof(long), true);
            _ExecuteMessageCache.Add(typeof(IntPtr), true);
            _ExecuteMessageCache.Add(typeof(sbyte), true);
            _ExecuteMessageCache.Add(typeof(float), true);
            _ExecuteMessageCache.Add(typeof(string), true);
            _ExecuteMessageCache.Add(typeof(TimeSpan), true);
            _ExecuteMessageCache.Add(typeof(ushort), true);
            _ExecuteMessageCache.Add(typeof(uint), true);
            _ExecuteMessageCache.Add(typeof(ulong), true);
            _ExecuteMessageCache.Add(typeof(UIntPtr), true);
            _ExecuteMessageCache.Add(typeof(void), true);
        }

        internal static bool AreMethodsSecure(Type t)
        {
            return ((SCICachedLookup(t) & 0x20) != 0);
        }

        private static bool AreMethodsSecure2(Type t)
        {
            object[] customAttributes = t.GetCustomAttributes(typeof(SecureMethodAttribute), true);
            return ((customAttributes != null) && (customAttributes.Length > 0));
        }

        internal static ClassInterfaceType GetClassInterfaceType(Type t)
        {
            object[] customAttributes = t.GetCustomAttributes(typeof(ClassInterfaceAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length == 0))
            {
                customAttributes = t.Assembly.GetCustomAttributes(typeof(ClassInterfaceAttribute), true);
                if ((customAttributes == null) || (customAttributes.Length == 0))
                {
                    return ClassInterfaceType.None;
                }
            }
            return ((ClassInterfaceAttribute) customAttributes[0]).Value;
        }

        private static bool HasClassInterface2(Type t)
        {
            object[] customAttributes = t.GetCustomAttributes(typeof(ClassInterfaceAttribute), false);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                ClassInterfaceAttribute attribute = (ClassInterfaceAttribute) customAttributes[0];
                if ((attribute.Value == ClassInterfaceType.AutoDual) || (attribute.Value == ClassInterfaceType.AutoDispatch))
                {
                    return true;
                }
            }
            customAttributes = t.Assembly.GetCustomAttributes(typeof(ClassInterfaceAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                ClassInterfaceAttribute attribute2 = (ClassInterfaceAttribute) customAttributes[0];
                if ((attribute2.Value == ClassInterfaceType.AutoDual) || (attribute2.Value == ClassInterfaceType.AutoDispatch))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasSpecialMethodAttributes(MemberInfo m)
        {
            return ((MICachedLookup(m) & 4) != 0);
        }

        private static bool HasSpecialMethodAttributes2(MemberInfo m)
        {
            foreach (object obj2 in m.GetCustomAttributes(true))
            {
                if ((obj2 is IConfigurationAttribute) && !(obj2 is AutoCompleteAttribute))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsExecuteMessageValid2(MemberInfo m)
        {
            if (ReflectionCache.ConvertToInterfaceMI(m) == null)
            {
                return false;
            }
            MethodInfo info2 = m as MethodInfo;
            if (info2 == null)
            {
                return false;
            }
            foreach (ParameterInfo info3 in info2.GetParameters())
            {
                if (!IsTypeExecuteMethodValid(info3.ParameterType))
                {
                    return false;
                }
            }
            if (!IsTypeExecuteMethodValid(info2.ReturnType))
            {
                return false;
            }
            return true;
        }

        public static bool IsMethodAutoDone(MemberInfo m)
        {
            return ((MICachedLookup(m) & 2) != 0);
        }

        private static bool IsMethodAutoDone2(MemberInfo m)
        {
            object[] customAttributes = m.GetCustomAttributes(typeof(AutoCompleteAttribute), true);
            int index = 0;
            while (index < customAttributes.Length)
            {
                object obj2 = customAttributes[index];
                return ((AutoCompleteAttribute) obj2).Value;
            }
            return false;
        }

        public static bool IsTypeEventSource(Type t)
        {
            return ((SCICachedLookup(t) & 4) != 0);
        }

        private static bool IsTypeEventSource2(Type t)
        {
            foreach (object obj2 in t.GetCustomAttributes(true))
            {
                if (obj2 is EventClassAttribute)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsTypeExecuteMethodValid(Type t)
        {
            if (!t.IsEnum)
            {
                Type elementType = t.GetElementType();
                if ((elementType != null) && (t.IsByRef || t.IsArray))
                {
                    if (_ExecuteMessageCache[elementType] == null)
                    {
                        return false;
                    }
                }
                else if (_ExecuteMessageCache[t] == null)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsTypeJITActivated(Type t)
        {
            return ((SCICachedLookup(t) & 8) != 0);
        }

        private static bool IsTypeJITActivated2(Type t)
        {
            foreach (object obj2 in t.GetCustomAttributes(true))
            {
                if (obj2 is JustInTimeActivationAttribute)
                {
                    return ((JustInTimeActivationAttribute) obj2).Value;
                }
                if ((obj2 is TransactionAttribute) && (((TransactionAttribute) obj2).Value >= TransactionOption.Supported))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsTypeObjectPooled(Type t)
        {
            return ((SCICachedLookup(t) & 0x10) != 0);
        }

        private static bool IsTypeObjectPooled2(Type t)
        {
            object[] customAttributes = t.GetCustomAttributes(typeof(ObjectPoolingAttribute), true);
            return (((customAttributes != null) && (customAttributes.Length > 0)) && ((ObjectPoolingAttribute) customAttributes[0]).Enabled);
        }

        public static bool IsTypeServicedComponent(Type t)
        {
            return ((SCICachedLookup(t) & 2) != 0);
        }

        private static bool IsTypeServicedComponent2(Type t)
        {
            return t.IsSubclassOf(typeof(ServicedComponent));
        }

        internal static int MICachedLookup(MemberInfo m)
        {
            object obj2 = _MICache.Get(m);
            if (obj2 != null)
            {
                return (int) obj2;
            }
            int val = 0;
            if (IsMethodAutoDone2(m))
            {
                val |= 2;
            }
            if (HasSpecialMethodAttributes2(m))
            {
                val |= 4;
            }
            if (IsExecuteMessageValid2(m))
            {
                val |= 8;
            }
            _MICache.Put(m, val);
            return val;
        }

        internal static int SCICachedLookup(Type t)
        {
            object obj2 = _SCICache.Get(t);
            if (obj2 != null)
            {
                return (int) obj2;
            }
            int val = 0;
            if (IsTypeServicedComponent2(t))
            {
                val |= 2;
                if (IsTypeEventSource2(t))
                {
                    val |= 4;
                }
                if (IsTypeJITActivated2(t))
                {
                    val |= 8;
                }
                if (IsTypeObjectPooled2(t))
                {
                    val |= 0x10;
                }
            }
            if (AreMethodsSecure2(t))
            {
                val |= 0x20;
            }
            if (HasClassInterface2(t))
            {
                val |= 0x40;
            }
            _SCICache.Put(t, val);
            return val;
        }
    }
}

