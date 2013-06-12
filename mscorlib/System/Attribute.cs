namespace System
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ClassInterface(ClassInterfaceType.None), AttributeUsage(AttributeTargets.All, Inherited=true, AllowMultiple=false), ComDefaultInterface(typeof(_Attribute)), ComVisible(true)]
    public abstract class Attribute : _Attribute
    {
        protected Attribute()
        {
        }

        private static void AddAttributesToList(List<Attribute> attributeList, Attribute[] attributes, Dictionary<Type, AttributeUsageAttribute> types)
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                Type key = attributes[i].GetType();
                AttributeUsageAttribute attributeUsage = null;
                types.TryGetValue(key, out attributeUsage);
                if (attributeUsage == null)
                {
                    attributeUsage = InternalGetAttributeUsage(key);
                    types[key] = attributeUsage;
                    if (attributeUsage.Inherited)
                    {
                        attributeList.Add(attributes[i]);
                    }
                }
                else if (attributeUsage.Inherited && attributeUsage.AllowMultiple)
                {
                    attributeList.Add(attributes[i]);
                }
            }
        }

        private static bool AreFieldValuesEqual(object thisValue, object thatValue)
        {
            if ((thisValue != null) || (thatValue != null))
            {
                if ((thisValue == null) || (thatValue == null))
                {
                    return false;
                }
                if (thisValue.GetType().IsArray)
                {
                    if (!thisValue.GetType().Equals(thatValue.GetType()))
                    {
                        return false;
                    }
                    Array array = thisValue as Array;
                    Array array2 = thatValue as Array;
                    if (array.Length != array2.Length)
                    {
                        return false;
                    }
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!AreFieldValuesEqual(array.GetValue(i), array2.GetValue(i)))
                        {
                            return false;
                        }
                    }
                }
                else if (!thisValue.Equals(thatValue))
                {
                    return false;
                }
            }
            return true;
        }

        private static void CopyToArrayList(List<Attribute> attributeList, Attribute[] attributes, Dictionary<Type, AttributeUsageAttribute> types)
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                attributeList.Add(attributes[i]);
                Type key = attributes[i].GetType();
                if (!types.ContainsKey(key))
                {
                    types[key] = InternalGetAttributeUsage(key);
                }
            }
        }

        [SecuritySafeCritical]
        private static Attribute[] CreateAttributeArrayHelper(Type elementType, int elementCount)
        {
            return (Attribute[]) Array.UnsafeCreateInstance(elementType, elementCount);
        }

        [SecuritySafeCritical]
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            RuntimeType type = (RuntimeType) base.GetType();
            RuntimeType type2 = (RuntimeType) obj.GetType();
            if (type2 != type)
            {
                return false;
            }
            object obj2 = this;
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                object thisValue = ((RtFieldInfo) fields[i]).InternalGetValue(obj2, false, false);
                object thatValue = ((RtFieldInfo) fields[i]).InternalGetValue(obj, false, false);
                if (!AreFieldValuesEqual(thisValue, thatValue))
                {
                    return false;
                }
            }
            return true;
        }

        public static Attribute GetCustomAttribute(Assembly element, Type attributeType)
        {
            return GetCustomAttribute(element, attributeType, true);
        }

        public static Attribute GetCustomAttribute(MemberInfo element, Type attributeType)
        {
            return GetCustomAttribute(element, attributeType, true);
        }

        public static Attribute GetCustomAttribute(Module element, Type attributeType)
        {
            return GetCustomAttribute(element, attributeType, true);
        }

        public static Attribute GetCustomAttribute(ParameterInfo element, Type attributeType)
        {
            return GetCustomAttribute(element, attributeType, true);
        }

        public static Attribute GetCustomAttribute(Assembly element, Type attributeType, bool inherit)
        {
            Attribute[] attributeArray = GetCustomAttributes(element, attributeType, inherit);
            if ((attributeArray == null) || (attributeArray.Length == 0))
            {
                return null;
            }
            if (attributeArray.Length != 1)
            {
                throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.AmbigCust"));
            }
            return attributeArray[0];
        }

        public static Attribute GetCustomAttribute(MemberInfo element, Type attributeType, bool inherit)
        {
            Attribute[] attributeArray = GetCustomAttributes(element, attributeType, inherit);
            if ((attributeArray == null) || (attributeArray.Length == 0))
            {
                return null;
            }
            if (attributeArray.Length != 1)
            {
                throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.AmbigCust"));
            }
            return attributeArray[0];
        }

        public static Attribute GetCustomAttribute(Module element, Type attributeType, bool inherit)
        {
            Attribute[] attributeArray = GetCustomAttributes(element, attributeType, inherit);
            if ((attributeArray == null) || (attributeArray.Length == 0))
            {
                return null;
            }
            if (attributeArray.Length != 1)
            {
                throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.AmbigCust"));
            }
            return attributeArray[0];
        }

        public static Attribute GetCustomAttribute(ParameterInfo element, Type attributeType, bool inherit)
        {
            Attribute[] attributeArray = GetCustomAttributes(element, attributeType, inherit);
            if ((attributeArray == null) || (attributeArray.Length == 0))
            {
                return null;
            }
            if (attributeArray.Length == 0)
            {
                return null;
            }
            if (attributeArray.Length != 1)
            {
                throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.AmbigCust"));
            }
            return attributeArray[0];
        }

        public static Attribute[] GetCustomAttributes(Assembly element)
        {
            return GetCustomAttributes(element, true);
        }

        public static Attribute[] GetCustomAttributes(MemberInfo element)
        {
            return GetCustomAttributes(element, true);
        }

        public static Attribute[] GetCustomAttributes(Module element)
        {
            return GetCustomAttributes(element, true);
        }

        public static Attribute[] GetCustomAttributes(ParameterInfo element)
        {
            return GetCustomAttributes(element, true);
        }

        public static Attribute[] GetCustomAttributes(Assembly element, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (Attribute[]) element.GetCustomAttributes(typeof(Attribute), inherit);
        }

        public static Attribute[] GetCustomAttributes(Assembly element, Type attributeType)
        {
            return GetCustomAttributes(element, attributeType, true);
        }

        public static Attribute[] GetCustomAttributes(MemberInfo element, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            MemberTypes memberType = element.MemberType;
            if (memberType != MemberTypes.Event)
            {
                if (memberType == MemberTypes.Property)
                {
                    return InternalGetCustomAttributes((PropertyInfo) element, typeof(Attribute), inherit);
                }
                return (element.GetCustomAttributes(typeof(Attribute), inherit) as Attribute[]);
            }
            return InternalGetCustomAttributes((EventInfo) element, typeof(Attribute), inherit);
        }

        public static Attribute[] GetCustomAttributes(MemberInfo element, Type type)
        {
            return GetCustomAttributes(element, type, true);
        }

        public static Attribute[] GetCustomAttributes(Module element, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (Attribute[]) element.GetCustomAttributes(typeof(Attribute), inherit);
        }

        public static Attribute[] GetCustomAttributes(Module element, Type attributeType)
        {
            return GetCustomAttributes(element, attributeType, true);
        }

        [SecuritySafeCritical]
        public static Attribute[] GetCustomAttributes(ParameterInfo element, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (element.Member == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidParameterInfo"), "element");
            }
            if ((element.Member.MemberType == MemberTypes.Method) && inherit)
            {
                return InternalParamGetCustomAttributes(element, null, inherit);
            }
            return (element.GetCustomAttributes(typeof(Attribute), inherit) as Attribute[]);
        }

        public static Attribute[] GetCustomAttributes(ParameterInfo element, Type attributeType)
        {
            return GetCustomAttributes(element, attributeType, true);
        }

        public static Attribute[] GetCustomAttributes(Assembly element, Type attributeType, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (!attributeType.IsSubclassOf(typeof(Attribute)) && (attributeType != typeof(Attribute)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
            }
            return (Attribute[]) element.GetCustomAttributes(attributeType, inherit);
        }

        public static Attribute[] GetCustomAttributes(MemberInfo element, Type type, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!type.IsSubclassOf(typeof(Attribute)) && (type != typeof(Attribute)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
            }
            MemberTypes memberType = element.MemberType;
            if (memberType != MemberTypes.Event)
            {
                if (memberType == MemberTypes.Property)
                {
                    return InternalGetCustomAttributes((PropertyInfo) element, type, inherit);
                }
                return (element.GetCustomAttributes(type, inherit) as Attribute[]);
            }
            return InternalGetCustomAttributes((EventInfo) element, type, inherit);
        }

        public static Attribute[] GetCustomAttributes(Module element, Type attributeType, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (!attributeType.IsSubclassOf(typeof(Attribute)) && (attributeType != typeof(Attribute)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
            }
            return (Attribute[]) element.GetCustomAttributes(attributeType, inherit);
        }

        [SecuritySafeCritical]
        public static Attribute[] GetCustomAttributes(ParameterInfo element, Type attributeType, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (!attributeType.IsSubclassOf(typeof(Attribute)) && (attributeType != typeof(Attribute)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
            }
            if (element.Member == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidParameterInfo"), "element");
            }
            if ((element.Member.MemberType == MemberTypes.Method) && inherit)
            {
                return InternalParamGetCustomAttributes(element, attributeType, inherit);
            }
            return (element.GetCustomAttributes(attributeType, inherit) as Attribute[]);
        }

        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            Type type = base.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            object obj2 = null;
            for (int i = 0; i < fields.Length; i++)
            {
                object obj3 = ((RtFieldInfo) fields[i]).InternalGetValue(this, false, false);
                if ((obj3 != null) && !obj3.GetType().IsArray)
                {
                    obj2 = obj3;
                }
                if (obj2 != null)
                {
                    break;
                }
            }
            if (obj2 != null)
            {
                return obj2.GetHashCode();
            }
            return type.GetHashCode();
        }

        private static EventInfo GetParentDefinition(EventInfo ev)
        {
            RuntimeMethodInfo addMethod = ev.GetAddMethod(true) as RuntimeMethodInfo;
            if (addMethod != null)
            {
                addMethod = addMethod.GetParentDefinition();
                if (addMethod != null)
                {
                    return addMethod.DeclaringType.GetEvent(ev.Name);
                }
            }
            return null;
        }

        private static ParameterInfo GetParentDefinition(ParameterInfo param)
        {
            RuntimeMethodInfo member = param.Member as RuntimeMethodInfo;
            if (member != null)
            {
                member = member.GetParentDefinition();
                if (member != null)
                {
                    return member.GetParameters()[param.Position];
                }
            }
            return null;
        }

        private static PropertyInfo GetParentDefinition(PropertyInfo property)
        {
            MethodInfo getMethod = property.GetGetMethod(true);
            if (getMethod == null)
            {
                getMethod = property.GetSetMethod(true);
            }
            RuntimeMethodInfo parentDefinition = getMethod as RuntimeMethodInfo;
            if (parentDefinition != null)
            {
                parentDefinition = parentDefinition.GetParentDefinition();
                if (parentDefinition != null)
                {
                    return parentDefinition.DeclaringType.GetProperty(property.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, property.PropertyType);
                }
            }
            return null;
        }

        private static AttributeUsageAttribute InternalGetAttributeUsage(Type type)
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            if (customAttributes.Length == 1)
            {
                return (AttributeUsageAttribute) customAttributes[0];
            }
            if (customAttributes.Length != 0)
            {
                throw new FormatException(Environment.GetResourceString("Format_AttributeUsage", new object[] { type }));
            }
            return AttributeUsageAttribute.Default;
        }

        private static Attribute[] InternalGetCustomAttributes(EventInfo element, Type type, bool inherit)
        {
            Attribute[] customAttributes = (Attribute[]) element.GetCustomAttributes(type, inherit);
            if (!inherit)
            {
                return customAttributes;
            }
            Dictionary<Type, AttributeUsageAttribute> types = new Dictionary<Type, AttributeUsageAttribute>(11);
            List<Attribute> attributeList = new List<Attribute>();
            CopyToArrayList(attributeList, customAttributes, types);
            for (EventInfo info = GetParentDefinition(element); info != null; info = GetParentDefinition(info))
            {
                customAttributes = GetCustomAttributes(info, type, false);
                AddAttributesToList(attributeList, customAttributes, types);
            }
            Array destinationArray = CreateAttributeArrayHelper(type, attributeList.Count);
            Array.Copy(attributeList.ToArray(), 0, destinationArray, 0, attributeList.Count);
            return (Attribute[]) destinationArray;
        }

        private static Attribute[] InternalGetCustomAttributes(PropertyInfo element, Type type, bool inherit)
        {
            Attribute[] customAttributes = (Attribute[]) element.GetCustomAttributes(type, inherit);
            if (!inherit)
            {
                return customAttributes;
            }
            Dictionary<Type, AttributeUsageAttribute> types = new Dictionary<Type, AttributeUsageAttribute>(11);
            List<Attribute> attributeList = new List<Attribute>();
            CopyToArrayList(attributeList, customAttributes, types);
            for (PropertyInfo info = GetParentDefinition(element); info != null; info = GetParentDefinition(info))
            {
                customAttributes = GetCustomAttributes(info, type, false);
                AddAttributesToList(attributeList, customAttributes, types);
            }
            Array destinationArray = CreateAttributeArrayHelper(type, attributeList.Count);
            Array.Copy(attributeList.ToArray(), 0, destinationArray, 0, attributeList.Count);
            return (Attribute[]) destinationArray;
        }

        private static bool InternalIsDefined(EventInfo element, Type attributeType, bool inherit)
        {
            if (element.IsDefined(attributeType, inherit))
            {
                return true;
            }
            if (inherit)
            {
                if (!InternalGetAttributeUsage(attributeType).Inherited)
                {
                    return false;
                }
                for (EventInfo info = GetParentDefinition(element); info != null; info = GetParentDefinition(info))
                {
                    if (info.IsDefined(attributeType, false))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool InternalIsDefined(PropertyInfo element, Type attributeType, bool inherit)
        {
            if (element.IsDefined(attributeType, inherit))
            {
                return true;
            }
            if (inherit)
            {
                if (!InternalGetAttributeUsage(attributeType).Inherited)
                {
                    return false;
                }
                for (PropertyInfo info = GetParentDefinition(element); info != null; info = GetParentDefinition(info))
                {
                    if (info.IsDefined(attributeType, false))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static Attribute[] InternalParamGetCustomAttributes(ParameterInfo param, Type type, bool inherit)
        {
            List<Type> list = new List<Type>();
            if (type == null)
            {
                type = typeof(Attribute);
            }
            object[] customAttributes = param.GetCustomAttributes(type, false);
            for (int i = 0; i < customAttributes.Length; i++)
            {
                Type type2 = customAttributes[i].GetType();
                if (!InternalGetAttributeUsage(type2).AllowMultiple)
                {
                    list.Add(type2);
                }
            }
            Attribute[] destinationArray = null;
            if (customAttributes.Length == 0)
            {
                destinationArray = CreateAttributeArrayHelper(type, 0);
            }
            else
            {
                destinationArray = (Attribute[]) customAttributes;
            }
            if (param.Member.DeclaringType != null)
            {
                if (!inherit)
                {
                    return destinationArray;
                }
                for (ParameterInfo info = GetParentDefinition(param); info != null; info = GetParentDefinition(info))
                {
                    customAttributes = info.GetCustomAttributes(type, false);
                    int elementCount = 0;
                    for (int j = 0; j < customAttributes.Length; j++)
                    {
                        Type type3 = customAttributes[j].GetType();
                        AttributeUsageAttribute attributeUsage = InternalGetAttributeUsage(type3);
                        if (attributeUsage.Inherited && !list.Contains(type3))
                        {
                            if (!attributeUsage.AllowMultiple)
                            {
                                list.Add(type3);
                            }
                            elementCount++;
                        }
                        else
                        {
                            customAttributes[j] = null;
                        }
                    }
                    Attribute[] attributeArray2 = CreateAttributeArrayHelper(type, elementCount);
                    elementCount = 0;
                    for (int k = 0; k < customAttributes.Length; k++)
                    {
                        if (customAttributes[k] != null)
                        {
                            attributeArray2[elementCount] = (Attribute) customAttributes[k];
                            elementCount++;
                        }
                    }
                    Attribute[] sourceArray = destinationArray;
                    destinationArray = CreateAttributeArrayHelper(type, sourceArray.Length + elementCount);
                    Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                    int length = sourceArray.Length;
                    for (int m = 0; m < attributeArray2.Length; m++)
                    {
                        destinationArray[length + m] = attributeArray2[m];
                    }
                }
            }
            return destinationArray;
        }

        private static bool InternalParamIsDefined(ParameterInfo param, Type type, bool inherit)
        {
            if (param.IsDefined(type, false))
            {
                return true;
            }
            if ((param.Member.DeclaringType != null) && inherit)
            {
                for (ParameterInfo info = GetParentDefinition(param); info != null; info = GetParentDefinition(info))
                {
                    object[] customAttributes = info.GetCustomAttributes(type, false);
                    for (int i = 0; i < customAttributes.Length; i++)
                    {
                        AttributeUsageAttribute attributeUsage = InternalGetAttributeUsage(customAttributes[i].GetType());
                        if ((customAttributes[i] is Attribute) && attributeUsage.Inherited)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public virtual bool IsDefaultAttribute()
        {
            return false;
        }

        public static bool IsDefined(Assembly element, Type attributeType)
        {
            return IsDefined(element, attributeType, true);
        }

        public static bool IsDefined(MemberInfo element, Type attributeType)
        {
            return IsDefined(element, attributeType, true);
        }

        public static bool IsDefined(Module element, Type attributeType)
        {
            return IsDefined(element, attributeType, false);
        }

        public static bool IsDefined(ParameterInfo element, Type attributeType)
        {
            return IsDefined(element, attributeType, true);
        }

        public static bool IsDefined(Assembly element, Type attributeType, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (!attributeType.IsSubclassOf(typeof(Attribute)) && (attributeType != typeof(Attribute)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
            }
            return element.IsDefined(attributeType, false);
        }

        public static bool IsDefined(MemberInfo element, Type attributeType, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (!attributeType.IsSubclassOf(typeof(Attribute)) && (attributeType != typeof(Attribute)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
            }
            MemberTypes memberType = element.MemberType;
            if (memberType != MemberTypes.Event)
            {
                if (memberType == MemberTypes.Property)
                {
                    return InternalIsDefined((PropertyInfo) element, attributeType, inherit);
                }
                return element.IsDefined(attributeType, inherit);
            }
            return InternalIsDefined((EventInfo) element, attributeType, inherit);
        }

        [SecuritySafeCritical]
        public static bool IsDefined(Module element, Type attributeType, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (!attributeType.IsSubclassOf(typeof(Attribute)) && (attributeType != typeof(Attribute)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
            }
            return element.IsDefined(attributeType, false);
        }

        public static bool IsDefined(ParameterInfo element, Type attributeType, bool inherit)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (!attributeType.IsSubclassOf(typeof(Attribute)) && (attributeType != typeof(Attribute)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
            }
            MemberTypes memberType = element.Member.MemberType;
            if (memberType == MemberTypes.Constructor)
            {
                return element.IsDefined(attributeType, false);
            }
            if (memberType != MemberTypes.Method)
            {
                if (memberType != MemberTypes.Property)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidParamInfo"));
                }
                return element.IsDefined(attributeType, false);
            }
            return InternalParamIsDefined(element, attributeType, inherit);
        }

        public virtual bool Match(object obj)
        {
            return this.Equals(obj);
        }

        void _Attribute.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _Attribute.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _Attribute.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _Attribute.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public virtual object TypeId
        {
            get
            {
                return base.GetType();
            }
        }
    }
}

