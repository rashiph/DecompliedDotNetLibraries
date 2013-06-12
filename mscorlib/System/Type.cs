namespace System
{
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    [Serializable, ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_Type)), ComVisible(true)]
    public abstract class Type : MemberInfo, _Type, IReflect
    {
        private static Binder defaultBinder;
        private const BindingFlags DefaultLookup = (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        public static readonly char Delimiter = '.';
        public static readonly Type[] EmptyTypes = new Type[0];
        public static readonly MemberFilter FilterAttribute;
        public static readonly MemberFilter FilterName;
        public static readonly MemberFilter FilterNameIgnoreCase;
        public static readonly object Missing = System.Reflection.Missing.Value;

        static Type()
        {
            System.__Filters filters = new System.__Filters();
            FilterAttribute = new MemberFilter(filters.FilterAttribute);
            FilterName = new MemberFilter(filters.FilterName);
            FilterNameIgnoreCase = new MemberFilter(filters.FilterIgnoreCase);
        }

        protected Type()
        {
        }

        private static int BinarySearch(Array array, object value)
        {
            ulong[] numArray = new ulong[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                numArray[i] = Enum.ToUInt64(array.GetValue(i));
            }
            ulong num2 = Enum.ToUInt64(value);
            return Array.BinarySearch<ulong>(numArray, num2);
        }

        private static void CreateBinder()
        {
            if (defaultBinder == null)
            {
                System.DefaultBinder binder = new System.DefaultBinder();
                Interlocked.CompareExchange<Binder>(ref defaultBinder, binder, null);
            }
        }

        public override bool Equals(object o)
        {
            if (o == null)
            {
                return false;
            }
            return this.Equals(o as Type);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual bool Equals(Type o)
        {
            if (o == null)
            {
                return false;
            }
            return object.ReferenceEquals(this.UnderlyingSystemType, o.UnderlyingSystemType);
        }

        public virtual Type[] FindInterfaces(TypeFilter filter, object filterCriteria)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }
            Type[] interfaces = this.GetInterfaces();
            int num = 0;
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (!filter(interfaces[i], filterCriteria))
                {
                    interfaces[i] = null;
                }
                else
                {
                    num++;
                }
            }
            if (num == interfaces.Length)
            {
                return interfaces;
            }
            Type[] typeArray2 = new Type[num];
            num = 0;
            for (int j = 0; j < interfaces.Length; j++)
            {
                if (interfaces[j] != null)
                {
                    typeArray2[num++] = interfaces[j];
                }
            }
            return typeArray2;
        }

        public virtual MemberInfo[] FindMembers(MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, object filterCriteria)
        {
            MethodInfo[] methods = null;
            ConstructorInfo[] constructors = null;
            FieldInfo[] fields = null;
            PropertyInfo[] properties = null;
            EventInfo[] events = null;
            Type[] nestedTypes = null;
            int index = 0;
            int num2 = 0;
            if ((memberType & MemberTypes.Method) != 0)
            {
                methods = this.GetMethods(bindingAttr);
                if (filter != null)
                {
                    for (index = 0; index < methods.Length; index++)
                    {
                        if (!filter(methods[index], filterCriteria))
                        {
                            methods[index] = null;
                        }
                        else
                        {
                            num2++;
                        }
                    }
                }
                else
                {
                    num2 += methods.Length;
                }
            }
            if ((memberType & MemberTypes.Constructor) != 0)
            {
                constructors = this.GetConstructors(bindingAttr);
                if (filter != null)
                {
                    for (index = 0; index < constructors.Length; index++)
                    {
                        if (!filter(constructors[index], filterCriteria))
                        {
                            constructors[index] = null;
                        }
                        else
                        {
                            num2++;
                        }
                    }
                }
                else
                {
                    num2 += constructors.Length;
                }
            }
            if ((memberType & MemberTypes.Field) != 0)
            {
                fields = this.GetFields(bindingAttr);
                if (filter != null)
                {
                    for (index = 0; index < fields.Length; index++)
                    {
                        if (!filter(fields[index], filterCriteria))
                        {
                            fields[index] = null;
                        }
                        else
                        {
                            num2++;
                        }
                    }
                }
                else
                {
                    num2 += fields.Length;
                }
            }
            if ((memberType & MemberTypes.Property) != 0)
            {
                properties = this.GetProperties(bindingAttr);
                if (filter != null)
                {
                    for (index = 0; index < properties.Length; index++)
                    {
                        if (!filter(properties[index], filterCriteria))
                        {
                            properties[index] = null;
                        }
                        else
                        {
                            num2++;
                        }
                    }
                }
                else
                {
                    num2 += properties.Length;
                }
            }
            if ((memberType & MemberTypes.Event) != 0)
            {
                events = this.GetEvents();
                if (filter != null)
                {
                    for (index = 0; index < events.Length; index++)
                    {
                        if (!filter(events[index], filterCriteria))
                        {
                            events[index] = null;
                        }
                        else
                        {
                            num2++;
                        }
                    }
                }
                else
                {
                    num2 += events.Length;
                }
            }
            if ((memberType & MemberTypes.NestedType) != 0)
            {
                nestedTypes = this.GetNestedTypes(bindingAttr);
                if (filter != null)
                {
                    for (index = 0; index < nestedTypes.Length; index++)
                    {
                        if (!filter(nestedTypes[index], filterCriteria))
                        {
                            nestedTypes[index] = null;
                        }
                        else
                        {
                            num2++;
                        }
                    }
                }
                else
                {
                    num2 += nestedTypes.Length;
                }
            }
            MemberInfo[] infoArray6 = new MemberInfo[num2];
            num2 = 0;
            if (methods != null)
            {
                for (index = 0; index < methods.Length; index++)
                {
                    if (methods[index] != null)
                    {
                        infoArray6[num2++] = methods[index];
                    }
                }
            }
            if (constructors != null)
            {
                for (index = 0; index < constructors.Length; index++)
                {
                    if (constructors[index] != null)
                    {
                        infoArray6[num2++] = constructors[index];
                    }
                }
            }
            if (fields != null)
            {
                for (index = 0; index < fields.Length; index++)
                {
                    if (fields[index] != null)
                    {
                        infoArray6[num2++] = fields[index];
                    }
                }
            }
            if (properties != null)
            {
                for (index = 0; index < properties.Length; index++)
                {
                    if (properties[index] != null)
                    {
                        infoArray6[num2++] = properties[index];
                    }
                }
            }
            if (events != null)
            {
                for (index = 0; index < events.Length; index++)
                {
                    if (events[index] != null)
                    {
                        infoArray6[num2++] = events[index];
                    }
                }
            }
            if (nestedTypes != null)
            {
                for (index = 0; index < nestedTypes.Length; index++)
                {
                    if (nestedTypes[index] != null)
                    {
                        infoArray6[num2++] = nestedTypes[index];
                    }
                }
            }
            return infoArray6;
        }

        public virtual int GetArrayRank()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        protected abstract TypeAttributes GetAttributeFlagsImpl();
        [ComVisible(true)]
        public ConstructorInfo GetConstructor(Type[] types)
        {
            return this.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, types, null);
        }

        [ComVisible(true)]
        public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null)
                {
                    throw new ArgumentNullException("types");
                }
            }
            return this.GetConstructorImpl(bindingAttr, binder, CallingConventions.Any, types, modifiers);
        }

        [ComVisible(true)]
        public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null)
                {
                    throw new ArgumentNullException("types");
                }
            }
            return this.GetConstructorImpl(bindingAttr, binder, callConvention, types, modifiers);
        }

        protected abstract ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);
        [ComVisible(true)]
        public ConstructorInfo[] GetConstructors()
        {
            return this.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        }

        [ComVisible(true)]
        public abstract ConstructorInfo[] GetConstructors(BindingFlags bindingAttr);
        [SecuritySafeCritical]
        public virtual MemberInfo[] GetDefaultMembers()
        {
            throw new NotImplementedException();
        }

        public abstract Type GetElementType();
        private void GetEnumData(out string[] enumNames, out Array enumValues)
        {
            FieldInfo[] fields = this.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            object[] objArray = new object[fields.Length];
            string[] strArray = new string[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                strArray[i] = fields[i].Name;
                objArray[i] = fields[i].GetRawConstantValue();
            }
            IComparer comparer = Comparer.Default;
            for (int j = 1; j < objArray.Length; j++)
            {
                int index = j;
                string str = strArray[j];
                object y = objArray[j];
                bool flag = false;
                while (comparer.Compare(objArray[index - 1], y) > 0)
                {
                    strArray[index] = strArray[index - 1];
                    objArray[index] = objArray[index - 1];
                    index--;
                    flag = true;
                    if (index == 0)
                    {
                        break;
                    }
                }
                if (flag)
                {
                    strArray[index] = str;
                    objArray[index] = y;
                }
            }
            enumNames = strArray;
            enumValues = objArray;
        }

        public virtual string GetEnumName(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!this.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            Type t = value.GetType();
            if (!t.IsEnum && !IsIntegerType(t))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnumBaseTypeOrEnum"), "value");
            }
            int index = BinarySearch(this.GetEnumRawConstantValues(), value);
            if (index >= 0)
            {
                return this.GetEnumNames()[index];
            }
            return null;
        }

        public virtual string[] GetEnumNames()
        {
            string[] strArray;
            Array array;
            if (!this.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            this.GetEnumData(out strArray, out array);
            return strArray;
        }

        private Array GetEnumRawConstantValues()
        {
            string[] strArray;
            Array array;
            this.GetEnumData(out strArray, out array);
            return array;
        }

        public virtual Type GetEnumUnderlyingType()
        {
            if (!this.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            FieldInfo[] fields = this.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if ((fields == null) || (fields.Length != 1))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidEnum"), "enumType");
            }
            return fields[0].FieldType;
        }

        public virtual Array GetEnumValues()
        {
            if (!this.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            throw new NotImplementedException();
        }

        public EventInfo GetEvent(string name)
        {
            return this.GetEvent(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public abstract EventInfo GetEvent(string name, BindingFlags bindingAttr);
        public virtual EventInfo[] GetEvents()
        {
            return this.GetEvents(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public abstract EventInfo[] GetEvents(BindingFlags bindingAttr);
        public FieldInfo GetField(string name)
        {
            return this.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public abstract FieldInfo GetField(string name, BindingFlags bindingAttr);
        public FieldInfo[] GetFields()
        {
            return this.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public abstract FieldInfo[] GetFields(BindingFlags bindingAttr);
        public virtual Type[] GetGenericArguments()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        public virtual Type[] GetGenericParameterConstraints()
        {
            if (!this.IsGenericParameter)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
            }
            throw new InvalidOperationException();
        }

        public virtual Type GetGenericTypeDefinition()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override int GetHashCode()
        {
            Type underlyingSystemType = this.UnderlyingSystemType;
            if (!object.ReferenceEquals(underlyingSystemType, this))
            {
                return underlyingSystemType.GetHashCode();
            }
            return base.GetHashCode();
        }

        public Type GetInterface(string name)
        {
            return this.GetInterface(name, false);
        }

        public abstract Type GetInterface(string name, bool ignoreCase);
        [ComVisible(true)]
        public virtual InterfaceMapping GetInterfaceMap(Type interfaceType)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        public abstract Type[] GetInterfaces();
        public MemberInfo[] GetMember(string name)
        {
            return this.GetMember(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public virtual MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            return this.GetMember(name, MemberTypes.All, bindingAttr);
        }

        public virtual MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        public MemberInfo[] GetMembers()
        {
            return this.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public abstract MemberInfo[] GetMembers(BindingFlags bindingAttr);
        public MethodInfo GetMethod(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return this.GetMethodImpl(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, CallingConventions.Any, null, null);
        }

        public MethodInfo GetMethod(string name, Type[] types)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null)
                {
                    throw new ArgumentNullException("types");
                }
            }
            return this.GetMethodImpl(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, CallingConventions.Any, types, null);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return this.GetMethodImpl(name, bindingAttr, null, CallingConventions.Any, null, null);
        }

        public MethodInfo GetMethod(string name, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null)
                {
                    throw new ArgumentNullException("types");
                }
            }
            return this.GetMethodImpl(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, CallingConventions.Any, types, modifiers);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null)
                {
                    throw new ArgumentNullException("types");
                }
            }
            return this.GetMethodImpl(name, bindingAttr, binder, CallingConventions.Any, types, modifiers);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null)
                {
                    throw new ArgumentNullException("types");
                }
            }
            return this.GetMethodImpl(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        protected abstract MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);
        public MethodInfo[] GetMethods()
        {
            return this.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public abstract MethodInfo[] GetMethods(BindingFlags bindingAttr);
        public Type GetNestedType(string name)
        {
            return this.GetNestedType(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public abstract Type GetNestedType(string name, BindingFlags bindingAttr);
        public Type[] GetNestedTypes()
        {
            return this.GetNestedTypes(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public abstract Type[] GetNestedTypes(BindingFlags bindingAttr);
        public PropertyInfo[] GetProperties()
        {
            return this.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public abstract PropertyInfo[] GetProperties(BindingFlags bindingAttr);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public PropertyInfo GetProperty(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return this.GetPropertyImpl(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, null, null, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return this.GetPropertyImpl(name, bindingAttr, null, null, null, null);
        }

        public PropertyInfo GetProperty(string name, Type[] types)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            return this.GetPropertyImpl(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, null, types, null);
        }

        public PropertyInfo GetProperty(string name, Type returnType)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (returnType == null)
            {
                throw new ArgumentNullException("returnType");
            }
            return this.GetPropertyImpl(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, returnType, null, null);
        }

        internal PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Type returnType)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (returnType == null)
            {
                throw new ArgumentNullException("returnType");
            }
            return this.GetPropertyImpl(name, bindingAttr, null, returnType, null, null);
        }

        public PropertyInfo GetProperty(string name, Type returnType, Type[] types)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            return this.GetPropertyImpl(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, returnType, types, null);
        }

        public PropertyInfo GetProperty(string name, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            return this.GetPropertyImpl(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, returnType, types, modifiers);
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            return this.GetPropertyImpl(name, bindingAttr, binder, returnType, types, modifiers);
        }

        protected abstract PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal virtual Type GetRootElementType()
        {
            Type elementType = this;
            while (elementType.HasElementType)
            {
                elementType = elementType.GetElementType();
            }
            return elementType;
        }

        public Type GetType()
        {
            return base.GetType();
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Type GetType(string typeName)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeType.GetType(typeName, false, false, false, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Type GetType(string typeName, bool throwOnError)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeType.GetType(typeName, throwOnError, false, false, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeType.GetType(typeName, throwOnError, ignoreCase, false, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Type GetType(string typeName, Func<AssemblyName, System.Reflection.Assembly> assemblyResolver, Func<System.Reflection.Assembly, string, bool, Type> typeResolver)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TypeNameParser.GetType(typeName, assemblyResolver, typeResolver, false, false, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Type GetType(string typeName, Func<AssemblyName, System.Reflection.Assembly> assemblyResolver, Func<System.Reflection.Assembly, string, bool, Type> typeResolver, bool throwOnError)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TypeNameParser.GetType(typeName, assemblyResolver, typeResolver, throwOnError, false, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Type GetType(string typeName, Func<AssemblyName, System.Reflection.Assembly> assemblyResolver, Func<System.Reflection.Assembly, string, bool, Type> typeResolver, bool throwOnError, bool ignoreCase)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TypeNameParser.GetType(typeName, assemblyResolver, typeResolver, throwOnError, ignoreCase, ref lookForMyCaller);
        }

        public static Type[] GetTypeArray(object[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            Type[] typeArray = new Type[args.Length];
            for (int i = 0; i < typeArray.Length; i++)
            {
                if (args[i] == null)
                {
                    throw new ArgumentNullException();
                }
                typeArray[i] = args[i].GetType();
            }
            return typeArray;
        }

        public static TypeCode GetTypeCode(Type type)
        {
            if (type == null)
            {
                return TypeCode.Empty;
            }
            return type.GetTypeCodeImpl();
        }

        protected virtual TypeCode GetTypeCodeImpl()
        {
            if ((this != this.UnderlyingSystemType) && (this.UnderlyingSystemType != null))
            {
                return GetTypeCode(this.UnderlyingSystemType);
            }
            return TypeCode.Object;
        }

        [SecuritySafeCritical]
        public static Type GetTypeFromCLSID(Guid clsid)
        {
            return RuntimeType.GetTypeFromCLSIDImpl(clsid, null, false);
        }

        [SecuritySafeCritical]
        public static Type GetTypeFromCLSID(Guid clsid, bool throwOnError)
        {
            return RuntimeType.GetTypeFromCLSIDImpl(clsid, null, throwOnError);
        }

        [SecuritySafeCritical]
        public static Type GetTypeFromCLSID(Guid clsid, string server)
        {
            return RuntimeType.GetTypeFromCLSIDImpl(clsid, server, false);
        }

        [SecuritySafeCritical]
        public static Type GetTypeFromCLSID(Guid clsid, string server, bool throwOnError)
        {
            return RuntimeType.GetTypeFromCLSIDImpl(clsid, server, throwOnError);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern Type GetTypeFromHandle(RuntimeTypeHandle handle);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern RuntimeType GetTypeFromHandleUnsafe(IntPtr handle);
        [SecurityCritical]
        public static Type GetTypeFromProgID(string progID)
        {
            return RuntimeType.GetTypeFromProgIDImpl(progID, null, false);
        }

        [SecurityCritical]
        public static Type GetTypeFromProgID(string progID, bool throwOnError)
        {
            return RuntimeType.GetTypeFromProgIDImpl(progID, null, throwOnError);
        }

        [SecurityCritical]
        public static Type GetTypeFromProgID(string progID, string server)
        {
            return RuntimeType.GetTypeFromProgIDImpl(progID, server, false);
        }

        [SecurityCritical]
        public static Type GetTypeFromProgID(string progID, string server, bool throwOnError)
        {
            return RuntimeType.GetTypeFromProgIDImpl(progID, server, throwOnError);
        }

        public static RuntimeTypeHandle GetTypeHandle(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException(null, Environment.GetResourceString("Arg_InvalidHandle"));
            }
            return new RuntimeTypeHandle((RuntimeType) o.GetType());
        }

        internal virtual RuntimeTypeHandle GetTypeHandleInternal()
        {
            return this.TypeHandle;
        }

        protected abstract bool HasElementTypeImpl();
        internal virtual bool HasProxyAttributeImpl()
        {
            return false;
        }

        internal bool ImplementInterface(Type ifaceType)
        {
            for (Type type = this; type != null; type = type.BaseType)
            {
                Type[] interfaces = type.GetInterfaces();
                if (interfaces != null)
                {
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if ((interfaces[i] == ifaceType) || ((interfaces[i] != null) && interfaces[i].ImplementInterface(ifaceType)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args)
        {
            return this.InvokeMember(name, invokeAttr, binder, target, args, null, null, null);
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, CultureInfo culture)
        {
            return this.InvokeMember(name, invokeAttr, binder, target, args, null, culture, null);
        }

        public abstract object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters);
        protected abstract bool IsArrayImpl();
        [SecuritySafeCritical]
        public virtual bool IsAssignableFrom(Type c)
        {
            if (c == null)
            {
                return false;
            }
            if (this != c)
            {
                RuntimeType underlyingSystemType = this.UnderlyingSystemType as RuntimeType;
                if (underlyingSystemType != null)
                {
                    return underlyingSystemType.IsAssignableFrom(c);
                }
                if (c.IsSubclassOf(this))
                {
                    return true;
                }
                if (this.IsInterface)
                {
                    return c.ImplementInterface(this);
                }
                if (!this.IsGenericParameter)
                {
                    return false;
                }
                Type[] genericParameterConstraints = this.GetGenericParameterConstraints();
                for (int i = 0; i < genericParameterConstraints.Length; i++)
                {
                    if (!genericParameterConstraints[i].IsAssignableFrom(c))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected abstract bool IsByRefImpl();
        protected abstract bool IsCOMObjectImpl();
        protected virtual bool IsContextfulImpl()
        {
            return typeof(ContextBoundObject).IsAssignableFrom(this);
        }

        public virtual bool IsEnumDefined(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!this.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            Type t = value.GetType();
            if (t.IsEnum)
            {
                if (!t.IsEquivalentTo(this))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_EnumAndObjectMustBeSameType", new object[] { t.ToString(), this.ToString() }));
                }
                t = t.GetEnumUnderlyingType();
            }
            if (t == typeof(string))
            {
                return (Array.IndexOf<object>(this.GetEnumNames(), value) >= 0);
            }
            if (!IsIntegerType(t))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
            }
            Type enumUnderlyingType = this.GetEnumUnderlyingType();
            if (enumUnderlyingType.GetTypeCodeImpl() != t.GetTypeCodeImpl())
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumUnderlyingTypeAndObjectMustBeSameType", new object[] { t.ToString(), enumUnderlyingType.ToString() }));
            }
            return (BinarySearch(this.GetEnumRawConstantValues(), value) >= 0);
        }

        public virtual bool IsEquivalentTo(Type other)
        {
            return (this == other);
        }

        [SecuritySafeCritical]
        public virtual bool IsInstanceOfType(object o)
        {
            if (o == null)
            {
                return false;
            }
            return this.IsAssignableFrom(o.GetType());
        }

        internal static bool IsIntegerType(Type t)
        {
            if (((!(t == typeof(int)) && !(t == typeof(short))) && (!(t == typeof(ushort)) && !(t == typeof(byte)))) && ((!(t == typeof(sbyte)) && !(t == typeof(uint))) && !(t == typeof(long))))
            {
                return (t == typeof(ulong));
            }
            return true;
        }

        protected virtual bool IsMarshalByRefImpl()
        {
            return typeof(MarshalByRefObject).IsAssignableFrom(this);
        }

        protected abstract bool IsPointerImpl();
        protected abstract bool IsPrimitiveImpl();
        [ComVisible(true)]
        public virtual bool IsSubclassOf(Type c)
        {
            Type baseType = this;
            if (!(baseType == c))
            {
                while (baseType != null)
                {
                    if (baseType == c)
                    {
                        return true;
                    }
                    baseType = baseType.BaseType;
                }
                return false;
            }
            return false;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected virtual bool IsValueTypeImpl()
        {
            return this.IsSubclassOf(RuntimeType.ValueType);
        }

        public virtual Type MakeArrayType()
        {
            throw new NotSupportedException();
        }

        public virtual Type MakeArrayType(int rank)
        {
            throw new NotSupportedException();
        }

        public virtual Type MakeByRefType()
        {
            throw new NotSupportedException();
        }

        public virtual Type MakeGenericType(params Type[] typeArguments)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        public virtual Type MakePointerType()
        {
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern bool operator ==(Type left, Type right);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern bool operator !=(Type left, Type right);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Type ReflectionOnlyGetType(string typeName, bool throwIfNotFound, bool ignoreCase)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeType.GetType(typeName, throwIfNotFound, ignoreCase, true, ref lookForMyCaller);
        }

        internal string SigToString()
        {
            Type elementType = this;
            while (elementType.HasElementType)
            {
                elementType = elementType.GetElementType();
            }
            if (elementType.IsNested)
            {
                return this.Name;
            }
            string str = this.ToString();
            if ((!elementType.IsPrimitive && !(elementType == typeof(void))) && !(elementType == typeof(TypedReference)))
            {
                return str;
            }
            return str.Substring("System.".Length);
        }

        void _Type.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _Type.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _Type.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _Type.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return ("Type: " + this.Name);
        }

        public abstract System.Reflection.Assembly Assembly { get; }

        public abstract string AssemblyQualifiedName { get; }

        public TypeAttributes Attributes
        {
            get
            {
                return this.GetAttributeFlagsImpl();
            }
        }

        public abstract Type BaseType { get; }

        public virtual bool ContainsGenericParameters
        {
            get
            {
                if (this.HasElementType)
                {
                    return this.GetRootElementType().ContainsGenericParameters;
                }
                if (this.IsGenericParameter)
                {
                    return true;
                }
                if (this.IsGenericType)
                {
                    Type[] genericArguments = this.GetGenericArguments();
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        if (genericArguments[i].ContainsGenericParameters)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public virtual MethodBase DeclaringMethod
        {
            get
            {
                return null;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return null;
            }
        }

        public static Binder DefaultBinder
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (defaultBinder == null)
                {
                    CreateBinder();
                }
                return defaultBinder;
            }
        }

        public abstract string FullName { get; }

        public virtual System.Reflection.GenericParameterAttributes GenericParameterAttributes
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public virtual int GenericParameterPosition
        {
            get
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
            }
        }

        public abstract Guid GUID { get; }

        public bool HasElementType
        {
            get
            {
                return this.HasElementTypeImpl();
            }
        }

        internal bool HasProxyAttribute
        {
            get
            {
                return this.HasProxyAttributeImpl();
            }
        }

        public bool IsAbstract
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.Abstract) != TypeAttributes.AnsiClass);
            }
        }

        public bool IsAnsiClass
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.CustomFormatClass) == TypeAttributes.AnsiClass);
            }
        }

        public bool IsArray
        {
            get
            {
                return this.IsArrayImpl();
            }
        }

        public bool IsAutoClass
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.CustomFormatClass) == TypeAttributes.AutoClass);
            }
        }

        public bool IsAutoLayout
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.AnsiClass);
            }
        }

        public bool IsByRef
        {
            get
            {
                return this.IsByRefImpl();
            }
        }

        public bool IsClass
        {
            get
            {
                return (((this.GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.AnsiClass) && !this.IsValueType);
            }
        }

        public bool IsCOMObject
        {
            get
            {
                return this.IsCOMObjectImpl();
            }
        }

        public bool IsContextful
        {
            get
            {
                return this.IsContextfulImpl();
            }
        }

        public virtual bool IsEnum
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.IsSubclassOf(RuntimeType.EnumType);
            }
        }

        public bool IsExplicitLayout
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout);
            }
        }

        public virtual bool IsGenericParameter
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsGenericType
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsGenericTypeDefinition
        {
            get
            {
                return false;
            }
        }

        public bool IsImport
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.Import) != TypeAttributes.AnsiClass);
            }
        }

        public bool IsInterface
        {
            [SecuritySafeCritical]
            get
            {
                RuntimeType type = this as RuntimeType;
                if (type != null)
                {
                    return RuntimeTypeHandle.IsInterface(type);
                }
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask);
            }
        }

        public bool IsLayoutSequential
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout);
            }
        }

        public bool IsMarshalByRef
        {
            get
            {
                return this.IsMarshalByRefImpl();
            }
        }

        public bool IsNested
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.DeclaringType != null);
            }
        }

        public bool IsNestedAssembly
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedAssembly);
            }
        }

        public bool IsNestedFamANDAssem
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedFamANDAssem);
            }
        }

        public bool IsNestedFamily
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedFamily);
            }
        }

        public bool IsNestedFamORAssem
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedFamORAssem);
            }
        }

        public bool IsNestedPrivate
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedPrivate);
            }
        }

        public bool IsNestedPublic
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedPublic);
            }
        }

        public bool IsNotPublic
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.NestedFamORAssem) == TypeAttributes.AnsiClass);
            }
        }

        public bool IsPointer
        {
            get
            {
                return this.IsPointerImpl();
            }
        }

        public bool IsPrimitive
        {
            get
            {
                return this.IsPrimitiveImpl();
            }
        }

        public bool IsPublic
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.NestedFamORAssem) == TypeAttributes.Public);
            }
        }

        internal virtual bool IsRuntimeType
        {
            get
            {
                return false;
            }
        }

        public bool IsSealed
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.Sealed) != TypeAttributes.AnsiClass);
            }
        }

        public virtual bool IsSecurityCritical
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsSecuritySafeCritical
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsSecurityTransparent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsSerializable
        {
            get
            {
                if ((this.GetAttributeFlagsImpl() & TypeAttributes.Serializable) != TypeAttributes.AnsiClass)
                {
                    return true;
                }
                RuntimeType underlyingSystemType = this.UnderlyingSystemType as RuntimeType;
                return ((underlyingSystemType != null) && underlyingSystemType.IsSpecialSerializableType());
            }
        }

        public bool IsSpecialName
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.SpecialName) != TypeAttributes.AnsiClass);
            }
        }

        internal virtual bool IsSzArray
        {
            get
            {
                return false;
            }
        }

        public bool IsUnicodeClass
        {
            get
            {
                return ((this.GetAttributeFlagsImpl() & TypeAttributes.CustomFormatClass) == TypeAttributes.UnicodeClass);
            }
        }

        public bool IsValueType
        {
            get
            {
                return this.IsValueTypeImpl();
            }
        }

        public bool IsVisible
        {
            [SecuritySafeCritical]
            get
            {
                RuntimeType type = this as RuntimeType;
                if (type != null)
                {
                    return RuntimeTypeHandle.IsVisible(type);
                }
                if (!this.IsGenericParameter)
                {
                    if (this.HasElementType)
                    {
                        return this.GetElementType().IsVisible;
                    }
                    Type declaringType = this;
                    while (declaringType.IsNested)
                    {
                        if (!declaringType.IsNestedPublic)
                        {
                            return false;
                        }
                        declaringType = declaringType.DeclaringType;
                    }
                    if (!declaringType.IsPublic)
                    {
                        return false;
                    }
                    if (this.IsGenericType && !this.IsGenericTypeDefinition)
                    {
                        foreach (Type type3 in this.GetGenericArguments())
                        {
                            if (!type3.IsVisible)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.TypeInfo;
            }
        }

        public abstract System.Reflection.Module Module { get; }

        public abstract string Namespace { get; }

        internal bool NeedsReflectionSecurityCheck
        {
            get
            {
                if (!this.IsVisible)
                {
                    return true;
                }
                if (this.IsSecurityCritical && !this.IsSecuritySafeCritical)
                {
                    return true;
                }
                if (this.IsGenericType)
                {
                    foreach (Type type in this.GetGenericArguments())
                    {
                        if (type.NeedsReflectionSecurityCheck)
                        {
                            return true;
                        }
                    }
                }
                else if (this.IsArray || this.IsPointer)
                {
                    return this.GetElementType().NeedsReflectionSecurityCheck;
                }
                return false;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return null;
            }
        }

        public virtual System.Runtime.InteropServices.StructLayoutAttribute StructLayoutAttribute
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public virtual RuntimeTypeHandle TypeHandle
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        [ComVisible(true)]
        public ConstructorInfo TypeInitializer
        {
            get
            {
                return this.GetConstructorImpl(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any, EmptyTypes, null);
            }
        }

        public abstract Type UnderlyingSystemType { get; }
    }
}

