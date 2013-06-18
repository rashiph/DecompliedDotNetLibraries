namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Text;

    internal sealed class RTTypeWrapper : Type, ICloneable
    {
        private Hashtable boundedTypes;
        private Hashtable memberMapping;
        private Type runtimeType;
        private Type[] typeArgs;
        private ITypeProvider typeProvider;

        internal RTTypeWrapper(ITypeProvider typeProvider, Type runtimeType)
        {
            this.memberMapping = new Hashtable();
            this.boundedTypes = new Hashtable(new TypeArrayComparer());
            if (runtimeType == null)
            {
                throw new ArgumentNullException("runtimeType");
            }
            if (runtimeType.Assembly == null)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidRuntimeType"), "runtimeType");
            }
            this.typeProvider = typeProvider;
            this.runtimeType = runtimeType;
        }

        private RTTypeWrapper(ITypeProvider typeProvider, Type runtimeType, Type[] typeArgs)
        {
            this.memberMapping = new Hashtable();
            this.boundedTypes = new Hashtable(new TypeArrayComparer());
            if (runtimeType == null)
            {
                throw new ArgumentNullException("runtimeType");
            }
            if (runtimeType.Assembly == null)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "runtimeType");
            }
            this.typeProvider = typeProvider;
            this.runtimeType = runtimeType;
            if (!this.IsGenericTypeDefinition)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "runtimeType");
            }
            this.typeArgs = new Type[typeArgs.Length];
            for (int i = 0; i < typeArgs.Length; i++)
            {
                this.typeArgs[i] = typeArgs[i];
                if (this.typeArgs[i] == null)
                {
                    throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "typeArgs");
                }
            }
        }

        public object Clone()
        {
            return this;
        }

        private ConstructorInfo EnsureConstructorWrapped(ConstructorInfo realInfo)
        {
            ConstructorInfo info = (ConstructorInfo) this.memberMapping[realInfo];
            if (info == null)
            {
                info = new RTConstructorInfoWrapper(this, realInfo);
                this.memberMapping.Add(realInfo, info);
            }
            return info;
        }

        private EventInfo EnsureEventWrapped(EventInfo realInfo)
        {
            EventInfo info = (EventInfo) this.memberMapping[realInfo];
            if (info == null)
            {
                info = new RTEventInfoWrapper(this, realInfo);
                this.memberMapping.Add(realInfo, info);
            }
            return info;
        }

        private FieldInfo EnsureFieldWrapped(FieldInfo realInfo)
        {
            FieldInfo info = (FieldInfo) this.memberMapping[realInfo];
            if (info == null)
            {
                info = new RTFieldInfoWrapper(this, realInfo);
                this.memberMapping.Add(realInfo, info);
            }
            return info;
        }

        private MemberInfo EnsureMemberWrapped(MemberInfo memberInfo)
        {
            MemberInfo info = null;
            if (memberInfo is PropertyInfo)
            {
                return this.EnsurePropertyWrapped(memberInfo as PropertyInfo);
            }
            if (memberInfo is ConstructorInfo)
            {
                return this.EnsureConstructorWrapped(memberInfo as ConstructorInfo);
            }
            if (memberInfo is EventInfo)
            {
                return this.EnsureEventWrapped(memberInfo as EventInfo);
            }
            if (memberInfo is FieldInfo)
            {
                return this.EnsureFieldWrapped(memberInfo as FieldInfo);
            }
            if (memberInfo is MethodInfo)
            {
                info = this.EnsureMethodWrapped(memberInfo as MethodInfo);
            }
            return info;
        }

        internal MethodInfo EnsureMethodWrapped(MethodInfo realInfo)
        {
            MethodInfo info = (MethodInfo) this.memberMapping[realInfo];
            if (info == null)
            {
                info = new RTMethodInfoWrapper(this, realInfo);
                this.memberMapping.Add(realInfo, info);
            }
            return info;
        }

        private PropertyInfo EnsurePropertyWrapped(PropertyInfo realInfo)
        {
            PropertyInfo info = (PropertyInfo) this.memberMapping[realInfo];
            if (info == null)
            {
                info = new RTPropertyInfoWrapper(this, realInfo);
                this.memberMapping.Add(realInfo, info);
            }
            return info;
        }

        public override bool Equals(object obj)
        {
            Type runtimeType = obj as Type;
            if (runtimeType is RTTypeWrapper)
            {
                runtimeType = ((RTTypeWrapper) runtimeType).runtimeType;
            }
            return (this.runtimeType == runtimeType);
        }

        public override int GetArrayRank()
        {
            return this.runtimeType.GetArrayRank();
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return this.runtimeType.Attributes;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            foreach (ConstructorInfo info in this.runtimeType.GetConstructors(bindingAttr))
            {
                bool flag = false;
                if (types != null)
                {
                    ParameterInfo[] parameters = info.GetParameters();
                    if (parameters.GetLength(0) == types.Length)
                    {
                        for (int i = 0; !flag && (i < parameters.Length); i++)
                        {
                            flag = !this.IsAssignable(parameters[i].ParameterType, types[i]);
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    return this.EnsureConstructorWrapped(info);
                }
            }
            return null;
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            List<ConstructorInfo> list = new List<ConstructorInfo>();
            foreach (ConstructorInfo info in this.runtimeType.GetConstructors(bindingAttr))
            {
                list.Add(this.EnsureConstructorWrapped(info));
            }
            return list.ToArray();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.runtimeType.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.runtimeType.GetCustomAttributes(attributeType, inherit);
        }

        public override Type GetElementType()
        {
            return this.ResolveTypeFromTypeSystem(this.runtimeType.GetElementType());
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            EventInfo realInfo = this.runtimeType.GetEvent(name, bindingAttr);
            if (realInfo != null)
            {
                realInfo = this.EnsureEventWrapped(realInfo);
            }
            return realInfo;
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            List<EventInfo> list = new List<EventInfo>();
            foreach (EventInfo info in this.runtimeType.GetEvents(bindingAttr))
            {
                list.Add(this.EnsureEventWrapped(info));
            }
            return list.ToArray();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            FieldInfo field = this.runtimeType.GetField(name, bindingAttr);
            if (field != null)
            {
                field = this.EnsureFieldWrapped(field);
            }
            return field;
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            List<FieldInfo> list = new List<FieldInfo>();
            foreach (FieldInfo info in this.runtimeType.GetFields(bindingAttr))
            {
                list.Add(this.EnsureFieldWrapped(info));
            }
            return list.ToArray();
        }

        public override Type[] GetGenericArguments()
        {
            return this.typeArgs;
        }

        public override Type GetGenericTypeDefinition()
        {
            if (this.IsGenericType)
            {
                return this.runtimeType;
            }
            return this;
        }

        public override int GetHashCode()
        {
            return this.runtimeType.GetHashCode();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            Type type = this.runtimeType.GetInterface(name, ignoreCase);
            if (type != null)
            {
                type = this.ResolveTypeFromTypeSystem(type);
            }
            return type;
        }

        public override Type[] GetInterfaces()
        {
            List<Type> list = new List<Type>();
            foreach (Type type in this.runtimeType.GetInterfaces())
            {
                Type item = this.ResolveTypeFromTypeSystem(type);
                list.Add(item);
            }
            return list.ToArray();
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            List<MemberInfo> list = new List<MemberInfo>();
            foreach (MemberInfo info in this.runtimeType.GetMember(name, type, bindingAttr))
            {
                list.Add(this.EnsureMemberWrapped(info));
            }
            return list.ToArray();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            List<MemberInfo> list = new List<MemberInfo>();
            foreach (MemberInfo info in this.runtimeType.GetMembers(bindingAttr))
            {
                list.Add(this.EnsureMemberWrapped(info));
            }
            return list.ToArray();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            foreach (MethodInfo info in this.runtimeType.GetMethods(bindingAttr))
            {
                if (((bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase) ? (string.Compare(info.Name, name, StringComparison.OrdinalIgnoreCase) == 0) : (string.Compare(info.Name, name, StringComparison.Ordinal) == 0))
                {
                    bool flag2 = false;
                    if (types != null)
                    {
                        ParameterInfo[] parameters = info.GetParameters();
                        if (parameters.GetLength(0) == types.Length)
                        {
                            for (int i = 0; !flag2 && (i < parameters.Length); i++)
                            {
                                flag2 = !this.IsAssignable(parameters[i].ParameterType, types[i]);
                            }
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (!flag2)
                    {
                        return this.EnsureMethodWrapped(info);
                    }
                }
            }
            return null;
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            List<MethodInfo> list = new List<MethodInfo>();
            foreach (MethodInfo info in this.runtimeType.GetMethods(bindingAttr))
            {
                list.Add(this.EnsureMethodWrapped(info));
            }
            return list.ToArray();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            Type nestedType = this.runtimeType.GetNestedType(name, bindingAttr);
            if (nestedType != null)
            {
                nestedType = this.ResolveTypeFromTypeSystem(nestedType);
            }
            return nestedType;
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            List<Type> list = new List<Type>();
            foreach (Type type in this.runtimeType.GetNestedTypes(bindingAttr))
            {
                list.Add(this.ResolveTypeFromTypeSystem(type));
            }
            return list.ToArray();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            foreach (PropertyInfo info in this.runtimeType.GetProperties(bindingAttr))
            {
                list.Add(this.EnsurePropertyWrapped(info));
            }
            return list.ToArray();
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            foreach (PropertyInfo info in this.runtimeType.GetProperties(bindingAttr))
            {
                if ((((bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase) ? (string.Compare(info.Name, name, StringComparison.OrdinalIgnoreCase) == 0) : (string.Compare(info.Name, name, StringComparison.Ordinal) == 0)) && ((returnType == null) || returnType.Equals(info.PropertyType)))
                {
                    bool flag2 = false;
                    if (types != null)
                    {
                        ParameterInfo[] indexParameters = info.GetIndexParameters();
                        if (indexParameters.GetLength(0) == types.Length)
                        {
                            for (int i = 0; !flag2 && (i < indexParameters.Length); i++)
                            {
                                flag2 = !this.IsAssignable(indexParameters[i].ParameterType, types[i]);
                            }
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (!flag2)
                    {
                        return this.EnsurePropertyWrapped(info);
                    }
                }
            }
            return null;
        }

        protected override bool HasElementTypeImpl()
        {
            return this.runtimeType.HasElementType;
        }

        public override object InvokeMember(string name, BindingFlags bindingFlags, Binder binder, object target, object[] providedArgs, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParams)
        {
            return this.runtimeType.InvokeMember(name, bindingFlags, binder, target, providedArgs, modifiers, culture, namedParams);
        }

        protected override bool IsArrayImpl()
        {
            return this.runtimeType.IsArray;
        }

        private bool IsAssignable(Type type1, Type type2)
        {
            Type toType = this.ResolveTypeFromTypeSystem(type1);
            Type fromType = this.ResolveTypeFromTypeSystem(type2);
            return TypeProvider.IsAssignable(toType, fromType);
        }

        public override bool IsAssignableFrom(Type c)
        {
            Type runtimeType = this.runtimeType;
            if (runtimeType.IsGenericTypeDefinition && this.IsGenericType)
            {
                runtimeType = this.ResolveGenericTypeFromTypeSystem(runtimeType);
            }
            return TypeProvider.IsAssignable(runtimeType, c);
        }

        protected override bool IsByRefImpl()
        {
            return this.runtimeType.IsByRef;
        }

        protected override bool IsCOMObjectImpl()
        {
            return this.runtimeType.IsCOMObject;
        }

        protected override bool IsContextfulImpl()
        {
            return this.runtimeType.IsContextful;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.runtimeType.IsDefined(attributeType, inherit);
        }

        protected override bool IsMarshalByRefImpl()
        {
            return this.runtimeType.IsMarshalByRef;
        }

        protected override bool IsPointerImpl()
        {
            return this.runtimeType.IsPointer;
        }

        protected override bool IsPrimitiveImpl()
        {
            return this.runtimeType.IsPrimitive;
        }

        public override bool IsSubclassOf(Type potentialBaseType)
        {
            return TypeProvider.IsSubclassOf(this.runtimeType, potentialBaseType);
        }

        public override Type MakeByRefType()
        {
            return this.typeProvider.GetType(this.FullName + "&");
        }

        public override Type MakeGenericType(params Type[] typeArgs)
        {
            if (typeArgs == null)
            {
                throw new ArgumentNullException("typeArgs");
            }
            Type[] typeArray = new Type[typeArgs.Length];
            if (!this.IsGenericTypeDefinition)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "typeArgs");
            }
            for (int i = 0; i < typeArgs.Length; i++)
            {
                typeArray[i] = typeArgs[i];
                if (typeArray[i] == null)
                {
                    throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "typeArgs");
                }
            }
            Type type = this.boundedTypes[typeArgs] as Type;
            if (type != null)
            {
                return type;
            }
            if (((typeArgs.Length == 1) && (this.runtimeType == typeof(Nullable<>))) && !typeArgs[0].IsEnum)
            {
                switch (Type.GetTypeCode(typeArgs[0]))
                {
                    case TypeCode.Boolean:
                        type = typeof(bool?);
                        goto Label_01F7;

                    case TypeCode.Char:
                        type = typeof(char?);
                        goto Label_01F7;

                    case TypeCode.SByte:
                        type = typeof(sbyte?);
                        goto Label_01F7;

                    case TypeCode.Byte:
                        type = typeof(byte?);
                        goto Label_01F7;

                    case TypeCode.Int16:
                        type = typeof(short?);
                        goto Label_01F7;

                    case TypeCode.UInt16:
                        type = typeof(ushort?);
                        goto Label_01F7;

                    case TypeCode.Int32:
                        type = typeof(int?);
                        goto Label_01F7;

                    case TypeCode.UInt32:
                        type = typeof(uint?);
                        goto Label_01F7;

                    case TypeCode.Int64:
                        type = typeof(long?);
                        goto Label_01F7;

                    case TypeCode.UInt64:
                        type = typeof(ulong?);
                        goto Label_01F7;

                    case TypeCode.Single:
                        type = typeof(float?);
                        goto Label_01F7;

                    case TypeCode.Double:
                        type = typeof(double?);
                        goto Label_01F7;

                    case TypeCode.Decimal:
                        type = typeof(decimal?);
                        goto Label_01F7;

                    case TypeCode.DateTime:
                        type = typeof(DateTime?);
                        goto Label_01F7;
                }
                type = new RTTypeWrapper(this.typeProvider, this.runtimeType, typeArgs);
            }
            else
            {
                type = new RTTypeWrapper(this.typeProvider, this.runtimeType, typeArgs);
            }
        Label_01F7:
            this.boundedTypes[typeArgs] = type;
            return type;
        }

        public override Type MakePointerType()
        {
            return this.typeProvider.GetType(this.FullName + "*");
        }

        internal void OnAssemblyRemoved(System.Reflection.Assembly removedAssembly)
        {
            ArrayList list = new ArrayList(this.boundedTypes.Keys);
            foreach (Type[] typeArray in list)
            {
                foreach (Type type in typeArray)
                {
                    if (type.Assembly == removedAssembly)
                    {
                        this.boundedTypes.Remove(typeArray);
                        break;
                    }
                }
            }
        }

        internal Type ResolveGenericTypeFromTypeSystem(Type type)
        {
            if (this.runtimeType.IsGenericTypeDefinition)
            {
                Type declaringType = null;
                if (!type.IsNested)
                {
                    declaringType = this.typeProvider.GetType(type.Namespace + "." + type.Name);
                }
                else
                {
                    declaringType = type;
                    string name = type.Name;
                    while (declaringType.DeclaringType != null)
                    {
                        declaringType = declaringType.DeclaringType;
                        name = declaringType.Name + "+" + name;
                    }
                    name = declaringType.Namespace + "." + name;
                    declaringType = this.typeProvider.GetType(name);
                }
                if (declaringType != null)
                {
                    return declaringType.MakeGenericType(this.typeArgs);
                }
            }
            return type;
        }

        internal Type ResolveTypeFromTypeSystem(Type type)
        {
            if (type == null)
            {
                return null;
            }
            if (type.IsGenericParameter)
            {
                if (this.typeArgs == null)
                {
                    return type;
                }
                type = this.typeArgs[type.GenericParameterPosition];
            }
            Type type2 = null;
            try
            {
                if (!string.IsNullOrEmpty(type.AssemblyQualifiedName))
                {
                    type2 = this.typeProvider.GetType(type.AssemblyQualifiedName);
                }
            }
            catch
            {
            }
            if (type2 == null)
            {
                type2 = type;
            }
            if (type2.IsGenericType)
            {
                type2 = this.ResolveGenericTypeFromTypeSystem(type2);
            }
            return type2;
        }

        public override string ToString()
        {
            return this.runtimeType.ToString();
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                if (this.typeArgs != null)
                {
                    foreach (Type type in this.typeArgs)
                    {
                        if (type.Assembly == null)
                        {
                            return null;
                        }
                    }
                }
                return this.runtimeType.Assembly;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return (this.FullName + ", " + this.runtimeType.Assembly.FullName);
            }
        }

        public override Type BaseType
        {
            get
            {
                return this.ResolveTypeFromTypeSystem(this.runtimeType.BaseType);
            }
        }

        public override bool ContainsGenericParameters
        {
            get
            {
                if ((this.typeArgs != null) && (this.typeArgs.GetLength(0) > 0))
                {
                    return false;
                }
                return this.runtimeType.ContainsGenericParameters;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                if (this.runtimeType.DeclaringType == null)
                {
                    return null;
                }
                return this.typeProvider.GetType(this.runtimeType.DeclaringType.AssemblyQualifiedName);
            }
        }

        public override string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder(this.runtimeType.FullName);
                if ((this.typeArgs != null) && (this.typeArgs.Length > 0))
                {
                    builder.Append("[");
                    for (int i = 0; i < this.typeArgs.Length; i++)
                    {
                        builder.Append("[");
                        builder.Append(this.typeArgs[i].AssemblyQualifiedName);
                        builder.Append("]");
                        if (i < (this.typeArgs.Length - 1))
                        {
                            builder.Append(",");
                        }
                    }
                    builder.Append("]");
                }
                return builder.ToString();
            }
        }

        public override int GenericParameterPosition
        {
            get
            {
                return this.runtimeType.GenericParameterPosition;
            }
        }

        public override Guid GUID
        {
            get
            {
                return this.runtimeType.GUID;
            }
        }

        public override bool IsGenericParameter
        {
            get
            {
                return this.runtimeType.IsGenericParameter;
            }
        }

        public override bool IsGenericType
        {
            get
            {
                return (((this.typeArgs != null) && (this.typeArgs.GetLength(0) > 0)) || this.runtimeType.IsGenericType);
            }
        }

        public override bool IsGenericTypeDefinition
        {
            get
            {
                if ((this.typeArgs != null) && (this.typeArgs.GetLength(0) > 0))
                {
                    return false;
                }
                return this.runtimeType.IsGenericTypeDefinition;
            }
        }

        public override int MetadataToken
        {
            get
            {
                return this.runtimeType.MetadataToken;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.runtimeType.Module;
            }
        }

        public override string Name
        {
            get
            {
                if (this.IsGenericType && !this.IsGenericTypeDefinition)
                {
                    return this.GetGenericTypeDefinition().FullName.Substring(this.Namespace.Length + 1);
                }
                if (this.Namespace != null)
                {
                    return this.FullName.Substring(this.Namespace.Length + 1);
                }
                return this.FullName;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.runtimeType.Namespace;
            }
        }

        internal ITypeProvider Provider
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.typeProvider;
            }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                return this.runtimeType.TypeHandle;
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                return this.runtimeType.UnderlyingSystemType;
            }
        }

        private class RTConstructorInfoWrapper : ConstructorInfo
        {
            private ConstructorInfo ctorInfo;
            private RTTypeWrapper rtTypeWrapper;
            private ParameterInfo[] wrappedParameters;

            public RTConstructorInfoWrapper(RTTypeWrapper rtTypeWrapper, ConstructorInfo ctorInfo)
            {
                this.rtTypeWrapper = rtTypeWrapper;
                this.ctorInfo = ctorInfo;
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.ctorInfo.GetCustomAttributes(inherit);
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this.ctorInfo.GetCustomAttributes(attributeType, inherit);
            }

            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                return this.ctorInfo.GetMethodImplementationFlags();
            }

            public override ParameterInfo[] GetParameters()
            {
                if (this.wrappedParameters == null)
                {
                    List<ParameterInfo> list = new List<ParameterInfo>();
                    foreach (ParameterInfo info in this.ctorInfo.GetParameters())
                    {
                        list.Add(new RTTypeWrapper.RTParameterInfoWrapper(this.rtTypeWrapper, this.ctorInfo, info));
                    }
                    this.wrappedParameters = list.ToArray();
                }
                return this.wrappedParameters;
            }

            public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            {
                return this.ctorInfo.Invoke(invokeAttr, binder, parameters, culture);
            }

            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            {
                return this.ctorInfo.Invoke(obj, invokeAttr, binder, parameters, culture);
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.ctorInfo.IsDefined(attributeType, inherit);
            }

            public override MethodAttributes Attributes
            {
                get
                {
                    return this.ctorInfo.Attributes;
                }
            }

            public override Type DeclaringType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.ctorInfo.DeclaringType);
                }
            }

            public override MemberTypes MemberType
            {
                get
                {
                    return this.ctorInfo.MemberType;
                }
            }

            public override RuntimeMethodHandle MethodHandle
            {
                get
                {
                    return this.ctorInfo.MethodHandle;
                }
            }

            public override string Name
            {
                get
                {
                    return this.ctorInfo.Name;
                }
            }

            public override Type ReflectedType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.ctorInfo.ReflectedType);
                }
            }
        }

        private class RTEventInfoWrapper : EventInfo
        {
            private EventInfo eventInfo;
            private RTTypeWrapper rtTypeWrapper;

            public RTEventInfoWrapper(RTTypeWrapper rtTypeWrapper, EventInfo eventInfo)
            {
                this.rtTypeWrapper = rtTypeWrapper;
                this.eventInfo = eventInfo;
            }

            public override MethodInfo GetAddMethod(bool nonPublic)
            {
                MethodInfo addMethod = this.eventInfo.GetAddMethod(nonPublic);
                if (addMethod == null)
                {
                    return null;
                }
                return this.rtTypeWrapper.EnsureMethodWrapped(addMethod);
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.eventInfo.GetCustomAttributes(inherit);
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this.eventInfo.GetCustomAttributes(attributeType, inherit);
            }

            public override MethodInfo GetRaiseMethod(bool nonPublic)
            {
                MethodInfo raiseMethod = this.eventInfo.GetRaiseMethod(nonPublic);
                if (raiseMethod == null)
                {
                    return null;
                }
                return this.rtTypeWrapper.EnsureMethodWrapped(raiseMethod);
            }

            public override MethodInfo GetRemoveMethod(bool nonPublic)
            {
                MethodInfo removeMethod = this.eventInfo.GetRemoveMethod(nonPublic);
                if (removeMethod == null)
                {
                    return null;
                }
                return this.rtTypeWrapper.EnsureMethodWrapped(removeMethod);
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.eventInfo.IsDefined(attributeType, inherit);
            }

            public override EventAttributes Attributes
            {
                get
                {
                    return this.eventInfo.Attributes;
                }
            }

            public override Type DeclaringType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.eventInfo.DeclaringType);
                }
            }

            public override MemberTypes MemberType
            {
                get
                {
                    return this.eventInfo.MemberType;
                }
            }

            public override int MetadataToken
            {
                get
                {
                    return this.eventInfo.MetadataToken;
                }
            }

            public override System.Reflection.Module Module
            {
                get
                {
                    return this.eventInfo.Module;
                }
            }

            public override string Name
            {
                get
                {
                    return this.eventInfo.Name;
                }
            }

            public override Type ReflectedType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.eventInfo.ReflectedType);
                }
            }
        }

        private class RTFieldInfoWrapper : FieldInfo
        {
            private FieldInfo fieldInfo;
            private RTTypeWrapper rtTypeWrapper;

            public RTFieldInfoWrapper(RTTypeWrapper rtTypeWrapper, FieldInfo fieldInfo)
            {
                this.rtTypeWrapper = rtTypeWrapper;
                this.fieldInfo = fieldInfo;
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.fieldInfo.GetCustomAttributes(inherit);
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this.fieldInfo.GetCustomAttributes(attributeType, inherit);
            }

            public override object GetValue(object obj)
            {
                return this.fieldInfo.GetValue(obj);
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.fieldInfo.IsDefined(attributeType, inherit);
            }

            public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
            {
                this.fieldInfo.SetValue(obj, value, invokeAttr, binder, culture);
            }

            public override FieldAttributes Attributes
            {
                get
                {
                    return this.fieldInfo.Attributes;
                }
            }

            public override Type DeclaringType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.fieldInfo.DeclaringType);
                }
            }

            public override RuntimeFieldHandle FieldHandle
            {
                get
                {
                    return this.fieldInfo.FieldHandle;
                }
            }

            public override Type FieldType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.fieldInfo.FieldType);
                }
            }

            public override MemberTypes MemberType
            {
                get
                {
                    return this.fieldInfo.MemberType;
                }
            }

            public override int MetadataToken
            {
                get
                {
                    return this.fieldInfo.MetadataToken;
                }
            }

            public override System.Reflection.Module Module
            {
                get
                {
                    return this.fieldInfo.Module;
                }
            }

            public override string Name
            {
                get
                {
                    return this.fieldInfo.Name;
                }
            }

            public override Type ReflectedType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.fieldInfo.ReflectedType);
                }
            }
        }

        private class RTMethodInfoWrapper : MethodInfo
        {
            private MethodInfo methodInfo;
            private RTTypeWrapper rtTypeWrapper;
            private ParameterInfo[] wrappedParameters;

            public RTMethodInfoWrapper(RTTypeWrapper rtTypeWrapper, MethodInfo methodInfo)
            {
                this.rtTypeWrapper = rtTypeWrapper;
                this.methodInfo = methodInfo;
            }

            public override MethodInfo GetBaseDefinition()
            {
                return this.methodInfo.GetBaseDefinition();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.methodInfo.GetCustomAttributes(inherit);
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this.methodInfo.GetCustomAttributes(attributeType, inherit);
            }

            public override MethodBody GetMethodBody()
            {
                return this.methodInfo.GetMethodBody();
            }

            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                return this.methodInfo.GetMethodImplementationFlags();
            }

            public override ParameterInfo[] GetParameters()
            {
                if (this.wrappedParameters == null)
                {
                    List<ParameterInfo> list = new List<ParameterInfo>();
                    foreach (ParameterInfo info in this.methodInfo.GetParameters())
                    {
                        list.Add(new RTTypeWrapper.RTParameterInfoWrapper(this.rtTypeWrapper, this.methodInfo, info));
                    }
                    this.wrappedParameters = list.ToArray();
                }
                return this.wrappedParameters;
            }

            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            {
                return this.methodInfo.Invoke(obj, invokeAttr, binder, parameters, culture);
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.methodInfo.IsDefined(attributeType, inherit);
            }

            public override MethodAttributes Attributes
            {
                get
                {
                    return this.methodInfo.Attributes;
                }
            }

            public override CallingConventions CallingConvention
            {
                get
                {
                    return this.methodInfo.CallingConvention;
                }
            }

            public override Type DeclaringType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.methodInfo.DeclaringType);
                }
            }

            public override MemberTypes MemberType
            {
                get
                {
                    return this.methodInfo.MemberType;
                }
            }

            public override int MetadataToken
            {
                get
                {
                    return this.methodInfo.MetadataToken;
                }
            }

            public override RuntimeMethodHandle MethodHandle
            {
                get
                {
                    return this.methodInfo.MethodHandle;
                }
            }

            public override System.Reflection.Module Module
            {
                get
                {
                    return this.methodInfo.Module;
                }
            }

            public override string Name
            {
                get
                {
                    return this.methodInfo.Name;
                }
            }

            public override Type ReflectedType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.methodInfo.ReflectedType);
                }
            }

            public override ParameterInfo ReturnParameter
            {
                get
                {
                    return new RTTypeWrapper.RTParameterInfoWrapper(this.rtTypeWrapper, this, this.methodInfo.ReturnParameter);
                }
            }

            public override Type ReturnType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.methodInfo.ReturnType);
                }
            }

            public override ICustomAttributeProvider ReturnTypeCustomAttributes
            {
                get
                {
                    return this.methodInfo.ReturnTypeCustomAttributes;
                }
            }
        }

        private class RTParameterInfoWrapper : ParameterInfo
        {
            private ParameterInfo paramInfo;
            private MemberInfo parentMember;
            private RTTypeWrapper rtTypeWrapper;

            public RTParameterInfoWrapper(RTTypeWrapper rtTypeWrapper, MemberInfo parentMember, ParameterInfo paramInfo)
            {
                this.parentMember = parentMember;
                this.rtTypeWrapper = rtTypeWrapper;
                this.paramInfo = paramInfo;
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.paramInfo.GetCustomAttributes(inherit);
            }

            public override Type[] GetOptionalCustomModifiers()
            {
                return this.paramInfo.GetOptionalCustomModifiers();
            }

            public override Type[] GetRequiredCustomModifiers()
            {
                return this.paramInfo.GetRequiredCustomModifiers();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.paramInfo.IsDefined(attributeType, inherit);
            }

            public override ParameterAttributes Attributes
            {
                get
                {
                    return this.paramInfo.Attributes;
                }
            }

            public override object DefaultValue
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override MemberInfo Member
            {
                get
                {
                    return this.parentMember;
                }
            }

            public override string Name
            {
                get
                {
                    return this.paramInfo.Name;
                }
            }

            public override Type ParameterType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.paramInfo.ParameterType);
                }
            }

            public override int Position
            {
                get
                {
                    return this.paramInfo.Position;
                }
            }
        }

        private class RTPropertyInfoWrapper : PropertyInfo
        {
            private PropertyInfo propertyInfo;
            private RTTypeWrapper rtTypeWrapper;
            private ParameterInfo[] wrappedParameters;

            public RTPropertyInfoWrapper(RTTypeWrapper rtTypeWrapper, PropertyInfo propertyInfo)
            {
                this.rtTypeWrapper = rtTypeWrapper;
                this.propertyInfo = propertyInfo;
            }

            public override MethodInfo[] GetAccessors(bool nonPublic)
            {
                List<MethodInfo> list = new List<MethodInfo>();
                foreach (MethodInfo info in this.propertyInfo.GetAccessors(nonPublic))
                {
                    list.Add(this.rtTypeWrapper.EnsureMethodWrapped(info));
                }
                return list.ToArray();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.propertyInfo.GetCustomAttributes(inherit);
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return this.propertyInfo.GetCustomAttributes(attributeType, inherit);
            }

            public override MethodInfo GetGetMethod(bool nonPublic)
            {
                MethodInfo getMethod = this.propertyInfo.GetGetMethod(nonPublic);
                if (getMethod == null)
                {
                    return null;
                }
                return this.rtTypeWrapper.EnsureMethodWrapped(getMethod);
            }

            public override ParameterInfo[] GetIndexParameters()
            {
                if (this.wrappedParameters == null)
                {
                    List<ParameterInfo> list = new List<ParameterInfo>();
                    foreach (ParameterInfo info in this.propertyInfo.GetIndexParameters())
                    {
                        list.Add(new RTTypeWrapper.RTParameterInfoWrapper(this.rtTypeWrapper, this.propertyInfo, info));
                    }
                    this.wrappedParameters = list.ToArray();
                }
                return this.wrappedParameters;
            }

            public override MethodInfo GetSetMethod(bool nonPublic)
            {
                MethodInfo setMethod = this.propertyInfo.GetSetMethod(nonPublic);
                if (setMethod == null)
                {
                    return null;
                }
                return this.rtTypeWrapper.EnsureMethodWrapped(setMethod);
            }

            public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
            {
                return this.propertyInfo.GetValue(obj, invokeAttr, binder, index, culture);
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return this.propertyInfo.IsDefined(attributeType, inherit);
            }

            public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
            {
                this.propertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
            }

            public override PropertyAttributes Attributes
            {
                get
                {
                    return this.propertyInfo.Attributes;
                }
            }

            public override bool CanRead
            {
                get
                {
                    return this.propertyInfo.CanRead;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return this.propertyInfo.CanWrite;
                }
            }

            public override Type DeclaringType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.propertyInfo.DeclaringType);
                }
            }

            public override MemberTypes MemberType
            {
                get
                {
                    return this.propertyInfo.MemberType;
                }
            }

            public override int MetadataToken
            {
                get
                {
                    return this.propertyInfo.MetadataToken;
                }
            }

            public override System.Reflection.Module Module
            {
                get
                {
                    return this.propertyInfo.Module;
                }
            }

            public override string Name
            {
                get
                {
                    return this.propertyInfo.Name;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.propertyInfo.PropertyType);
                }
            }

            public override Type ReflectedType
            {
                get
                {
                    return this.rtTypeWrapper.ResolveTypeFromTypeSystem(this.propertyInfo.ReflectedType);
                }
            }
        }

        private class TypeArrayComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
            {
                Array array = x as Array;
                Array array2 = y as Array;
                if (((array == null) || (array2 == null)) || ((array.Rank != 1) || (array2.Rank != 1)))
                {
                    return false;
                }
                bool flag = false;
                if (array.Length == array2.Length)
                {
                    for (int i = 0; !flag && (i < array.Length); i++)
                    {
                        flag = array.GetValue(i) != array2.GetValue(i);
                    }
                }
                else
                {
                    flag = true;
                }
                return !flag;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return 0;
            }
        }
    }
}

