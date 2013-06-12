namespace System.Reflection.Emit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public sealed class GenericTypeParameterBuilder : Type
    {
        internal TypeBuilder m_type;

        internal GenericTypeParameterBuilder(TypeBuilder type)
        {
            this.m_type = type;
        }

        public override bool Equals(object o)
        {
            GenericTypeParameterBuilder builder = o as GenericTypeParameterBuilder;
            if (builder == null)
            {
                return false;
            }
            return object.ReferenceEquals(builder.m_type, this.m_type);
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return TypeAttributes.Public;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotSupportedException();
        }

        [ComVisible(true)]
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        public override EventInfo[] GetEvents()
        {
            throw new NotSupportedException();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        public override Type[] GetGenericArguments()
        {
            throw new InvalidOperationException();
        }

        public override Type GetGenericTypeDefinition()
        {
            throw new InvalidOperationException();
        }

        public override int GetHashCode()
        {
            return this.m_type.GetHashCode();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            throw new NotSupportedException();
        }

        [ComVisible(true)]
        public override InterfaceMapping GetInterfaceMap(Type interfaceType)
        {
            throw new NotSupportedException();
        }

        public override Type[] GetInterfaces()
        {
            throw new NotSupportedException();
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotSupportedException();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            throw new NotSupportedException();
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotSupportedException();
        }

        protected override bool HasElementTypeImpl()
        {
            return false;
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotSupportedException();
        }

        protected override bool IsArrayImpl()
        {
            return false;
        }

        public override bool IsAssignableFrom(Type c)
        {
            throw new NotSupportedException();
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
            throw new NotSupportedException();
        }

        protected override bool IsPointerImpl()
        {
            return false;
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }

        [ComVisible(true)]
        public override bool IsSubclassOf(Type c)
        {
            throw new NotSupportedException();
        }

        protected override bool IsValueTypeImpl()
        {
            return false;
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
            return (SymbolType.FormCompoundType(string.Format(CultureInfo.InvariantCulture, "[{0}]", new object[] { str }).ToCharArray(), this, 0) as SymbolType);
        }

        public override Type MakeByRefType()
        {
            return SymbolType.FormCompoundType("&".ToCharArray(), this, 0);
        }

        public override Type MakeGenericType(params Type[] typeArguments)
        {
            throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericTypeDefinition"));
        }

        public override Type MakePointerType()
        {
            return SymbolType.FormCompoundType("*".ToCharArray(), this, 0);
        }

        [SecuritySafeCritical]
        public void SetBaseTypeConstraint(Type baseTypeConstraint)
        {
            this.m_type.CheckContext(new Type[] { baseTypeConstraint });
            this.m_type.SetParent(baseTypeConstraint);
        }

        public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            if (this.m_type.m_ca == null)
            {
                this.m_type.m_ca = new List<TypeBuilder.CustAttr>();
            }
            this.m_type.m_ca.Add(new TypeBuilder.CustAttr(customBuilder));
        }

        public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
        {
            if (this.m_type.m_ca == null)
            {
                this.m_type.m_ca = new List<TypeBuilder.CustAttr>();
            }
            this.m_type.m_ca.Add(new TypeBuilder.CustAttr(con, binaryAttribute));
        }

        public void SetGenericParameterAttributes(GenericParameterAttributes genericParameterAttributes)
        {
            this.m_type.m_genParamAttributes = genericParameterAttributes;
        }

        [SecuritySafeCritical, ComVisible(true)]
        public void SetInterfaceConstraints(params Type[] interfaceConstraints)
        {
            this.m_type.CheckContext(interfaceConstraints);
            this.m_type.SetInterfaces(interfaceConstraints);
        }

        public override string ToString()
        {
            return this.m_type.Name;
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return this.m_type.Assembly;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return null;
            }
        }

        public override Type BaseType
        {
            get
            {
                return this.m_type.BaseType;
            }
        }

        public override bool ContainsGenericParameters
        {
            get
            {
                return this.m_type.ContainsGenericParameters;
            }
        }

        public override MethodBase DeclaringMethod
        {
            get
            {
                return this.m_type.DeclaringMethod;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_type.DeclaringType;
            }
        }

        public override string FullName
        {
            get
            {
                return null;
            }
        }

        public override int GenericParameterPosition
        {
            get
            {
                return this.m_type.GenericParameterPosition;
            }
        }

        public override Guid GUID
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override bool IsGenericParameter
        {
            get
            {
                return true;
            }
        }

        public override bool IsGenericType
        {
            get
            {
                return false;
            }
        }

        public override bool IsGenericTypeDefinition
        {
            get
            {
                return false;
            }
        }

        internal int MetadataTokenInternal
        {
            get
            {
                return this.m_type.MetadataTokenInternal;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_type.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_type.Name;
            }
        }

        public override string Namespace
        {
            get
            {
                return null;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_type.ReflectedType;
            }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                return this;
            }
        }
    }
}

