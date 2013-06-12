namespace System.Reflection.Emit
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComDefaultInterface(typeof(_EnumBuilder)), ComVisible(true), ClassInterface(ClassInterfaceType.None), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EnumBuilder : Type, _EnumBuilder
    {
        private Type m_runtimeType;
        internal TypeBuilder m_typeBuilder;
        private FieldBuilder m_underlyingField;
        private Type m_underlyingType;

        private EnumBuilder()
        {
        }

        [SecurityCritical]
        internal EnumBuilder(string name, Type underlyingType, TypeAttributes visibility, System.Reflection.Module module)
        {
            if ((visibility & ~TypeAttributes.NestedFamORAssem) != TypeAttributes.AnsiClass)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ShouldOnlySetVisibilityFlags"), "name");
            }
            this.m_typeBuilder = new TypeBuilder(name, visibility | TypeAttributes.Sealed, typeof(Enum), null, module, PackingSize.Unspecified, null);
            this.m_underlyingType = underlyingType;
            this.m_underlyingField = this.m_typeBuilder.DefineField("value__", underlyingType, FieldAttributes.RTSpecialName | FieldAttributes.SpecialName | FieldAttributes.Public);
        }

        public Type CreateType()
        {
            this.m_runtimeType = this.m_typeBuilder.CreateType();
            return this.m_runtimeType;
        }

        public FieldBuilder DefineLiteral(string literalName, object literalValue)
        {
            FieldBuilder builder = this.m_typeBuilder.DefineField(literalName, this, FieldAttributes.Literal | FieldAttributes.Static | FieldAttributes.Public);
            builder.SetConstant(literalValue);
            return builder;
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return this.m_typeBuilder.m_iAttr;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return this.m_typeBuilder.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        [ComVisible(true)]
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetConstructors(bindingAttr);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.m_typeBuilder.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.m_typeBuilder.GetCustomAttributes(attributeType, inherit);
        }

        public override Type GetElementType()
        {
            return this.m_typeBuilder.GetElementType();
        }

        public override Type GetEnumUnderlyingType()
        {
            return this.UnderlyingSystemType;
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetEvent(name, bindingAttr);
        }

        public override EventInfo[] GetEvents()
        {
            return this.m_typeBuilder.GetEvents();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetEvents(bindingAttr);
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetField(name, bindingAttr);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetFields(bindingAttr);
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            return this.m_typeBuilder.GetInterface(name, ignoreCase);
        }

        [ComVisible(true)]
        public override InterfaceMapping GetInterfaceMap(Type interfaceType)
        {
            return this.m_typeBuilder.GetInterfaceMap(interfaceType);
        }

        public override Type[] GetInterfaces()
        {
            return this.m_typeBuilder.GetInterfaces();
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetMember(name, type, bindingAttr);
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetMembers(bindingAttr);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            if (types == null)
            {
                return this.m_typeBuilder.GetMethod(name, bindingAttr);
            }
            return this.m_typeBuilder.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetMethods(bindingAttr);
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetNestedType(name, bindingAttr);
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetNestedTypes(bindingAttr);
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return this.m_typeBuilder.GetProperties(bindingAttr);
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        protected override bool HasElementTypeImpl()
        {
            return this.m_typeBuilder.HasElementType;
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            return this.m_typeBuilder.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        protected override bool IsArrayImpl()
        {
            return false;
        }

        protected override bool IsByRefImpl()
        {
            return false;
        }

        protected override bool IsCOMObjectImpl()
        {
            return false;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.m_typeBuilder.IsDefined(attributeType, inherit);
        }

        protected override bool IsPointerImpl()
        {
            return false;
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }

        protected override bool IsValueTypeImpl()
        {
            return true;
        }

        public override Type MakeArrayType()
        {
            return SymbolType.FormCompoundType("[]".ToCharArray(), this, 0);
        }

        public override Type MakeArrayType(int rank)
        {
            if (rank <= 0)
            {
                throw new IndexOutOfRangeException();
            }
            string str = "";
            if (rank == 1)
            {
                str = "*";
            }
            else
            {
                for (int i = 1; i < rank; i++)
                {
                    str = str + ",";
                }
            }
            return SymbolType.FormCompoundType(string.Format(CultureInfo.InvariantCulture, "[{0}]", new object[] { str }).ToCharArray(), this, 0);
        }

        public override Type MakeByRefType()
        {
            return SymbolType.FormCompoundType("&".ToCharArray(), this, 0);
        }

        public override Type MakePointerType()
        {
            return SymbolType.FormCompoundType("*".ToCharArray(), this, 0);
        }

        public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            this.m_typeBuilder.SetCustomAttribute(customBuilder);
        }

        [ComVisible(true), SecuritySafeCritical]
        public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
        {
            this.m_typeBuilder.SetCustomAttribute(con, binaryAttribute);
        }

        void _EnumBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _EnumBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _EnumBuilder.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _EnumBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return this.m_typeBuilder.Assembly;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return this.m_typeBuilder.AssemblyQualifiedName;
            }
        }

        public override Type BaseType
        {
            get
            {
                return this.m_typeBuilder.BaseType;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_typeBuilder.DeclaringType;
            }
        }

        public override string FullName
        {
            get
            {
                return this.m_typeBuilder.FullName;
            }
        }

        public override Guid GUID
        {
            get
            {
                return this.m_typeBuilder.GUID;
            }
        }

        internal int MetadataTokenInternal
        {
            get
            {
                return this.m_typeBuilder.MetadataTokenInternal;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_typeBuilder.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_typeBuilder.Name;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.m_typeBuilder.Namespace;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_typeBuilder.ReflectedType;
            }
        }

        internal Type RuntimeEnumType
        {
            get
            {
                return this.m_runtimeType;
            }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                return this.m_typeBuilder.TypeHandle;
            }
        }

        public System.Reflection.Emit.TypeToken TypeToken
        {
            get
            {
                return this.m_typeBuilder.TypeToken;
            }
        }

        public FieldBuilder UnderlyingField
        {
            get
            {
                return this.m_underlyingField;
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                return this.m_underlyingType;
            }
        }
    }
}

