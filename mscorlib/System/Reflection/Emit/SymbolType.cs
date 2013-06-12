namespace System.Reflection.Emit
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class SymbolType : Type
    {
        internal Type m_baseType;
        private char[] m_bFormat;
        internal int m_cRank;
        internal int[] m_iaLowerBound;
        internal int[] m_iaUpperBound;
        private bool m_isSzArray = true;
        internal TypeKind m_typeKind;

        internal SymbolType(TypeKind typeKind)
        {
            this.m_typeKind = typeKind;
            this.m_iaLowerBound = new int[4];
            this.m_iaUpperBound = new int[4];
        }

        internal static Type FormCompoundType(char[] bFormat, Type baseType, int curIndex)
        {
            SymbolType type;
            if ((bFormat == null) || (curIndex == bFormat.Length))
            {
                return baseType;
            }
            if (bFormat[curIndex] == '&')
            {
                type = new SymbolType(TypeKind.IsByRef);
                type.SetFormat(bFormat, curIndex, 1);
                curIndex++;
                if (curIndex != bFormat.Length)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_BadSigFormat"));
                }
                type.SetElementType(baseType);
                return type;
            }
            if (bFormat[curIndex] == '[')
            {
                type = new SymbolType(TypeKind.IsArray);
                int num3 = curIndex;
                curIndex++;
                int lower = 0;
                int upper = -1;
                while (bFormat[curIndex] != ']')
                {
                    if (bFormat[curIndex] == '*')
                    {
                        type.m_isSzArray = false;
                        curIndex++;
                    }
                    if (((bFormat[curIndex] >= '0') && (bFormat[curIndex] <= '9')) || (bFormat[curIndex] == '-'))
                    {
                        bool flag = false;
                        if (bFormat[curIndex] == '-')
                        {
                            flag = true;
                            curIndex++;
                        }
                        while ((bFormat[curIndex] >= '0') && (bFormat[curIndex] <= '9'))
                        {
                            lower *= 10;
                            lower += bFormat[curIndex] - '0';
                            curIndex++;
                        }
                        if (flag)
                        {
                            lower = -lower;
                        }
                        upper = lower - 1;
                    }
                    if (bFormat[curIndex] == '.')
                    {
                        curIndex++;
                        if (bFormat[curIndex] != '.')
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_BadSigFormat"));
                        }
                        curIndex++;
                        if (((bFormat[curIndex] >= '0') && (bFormat[curIndex] <= '9')) || (bFormat[curIndex] == '-'))
                        {
                            bool flag2 = false;
                            upper = 0;
                            if (bFormat[curIndex] == '-')
                            {
                                flag2 = true;
                                curIndex++;
                            }
                            while ((bFormat[curIndex] >= '0') && (bFormat[curIndex] <= '9'))
                            {
                                upper *= 10;
                                upper += bFormat[curIndex] - '0';
                                curIndex++;
                            }
                            if (flag2)
                            {
                                upper = -upper;
                            }
                            if (upper < lower)
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_BadSigFormat"));
                            }
                        }
                    }
                    if (bFormat[curIndex] == ',')
                    {
                        curIndex++;
                        type.SetBounds(lower, upper);
                        lower = 0;
                        upper = -1;
                    }
                    else if (bFormat[curIndex] != ']')
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_BadSigFormat"));
                    }
                }
                type.SetBounds(lower, upper);
                curIndex++;
                type.SetFormat(bFormat, num3, curIndex - num3);
                type.SetElementType(baseType);
                return FormCompoundType(bFormat, type, curIndex);
            }
            if (bFormat[curIndex] == '*')
            {
                type = new SymbolType(TypeKind.IsPointer);
                type.SetFormat(bFormat, curIndex, 1);
                curIndex++;
                type.SetElementType(baseType);
                return FormCompoundType(bFormat, type, curIndex);
            }
            return null;
        }

        public override int GetArrayRank()
        {
            if (!base.IsArray)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
            }
            return this.m_cRank;
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            Type baseType = this.m_baseType;
            while (baseType is SymbolType)
            {
                baseType = ((SymbolType) baseType).m_baseType;
            }
            return baseType.Attributes;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        [ComVisible(true)]
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override Type GetElementType()
        {
            return this.m_baseType;
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override EventInfo[] GetEvents()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        [ComVisible(true)]
        public override InterfaceMapping GetInterfaceMap(Type interfaceType)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override Type[] GetInterfaces()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        protected override bool HasElementTypeImpl()
        {
            return (this.m_baseType != null);
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        protected override bool IsArrayImpl()
        {
            return (this.m_typeKind == TypeKind.IsArray);
        }

        protected override bool IsByRefImpl()
        {
            return (this.m_typeKind == TypeKind.IsByRef);
        }

        protected override bool IsCOMObjectImpl()
        {
            return false;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
        }

        protected override bool IsPointerImpl()
        {
            return (this.m_typeKind == TypeKind.IsPointer);
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }

        protected override bool IsValueTypeImpl()
        {
            return false;
        }

        public override Type MakeArrayType()
        {
            return FormCompoundType((new string(this.m_bFormat) + "[]").ToCharArray(), this.m_baseType, 0);
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
            string str2 = string.Format(CultureInfo.InvariantCulture, "[{0}]", new object[] { str });
            return (FormCompoundType((new string(this.m_bFormat) + str2).ToCharArray(), this.m_baseType, 0) as SymbolType);
        }

        public override Type MakeByRefType()
        {
            return FormCompoundType((new string(this.m_bFormat) + "&").ToCharArray(), this.m_baseType, 0);
        }

        public override Type MakePointerType()
        {
            return FormCompoundType((new string(this.m_bFormat) + "*").ToCharArray(), this.m_baseType, 0);
        }

        private void SetBounds(int lower, int upper)
        {
            if ((lower != 0) || (upper != -1))
            {
                this.m_isSzArray = false;
            }
            if (this.m_iaLowerBound.Length <= this.m_cRank)
            {
                int[] destinationArray = new int[this.m_cRank * 2];
                Array.Copy(this.m_iaLowerBound, destinationArray, this.m_cRank);
                this.m_iaLowerBound = destinationArray;
                Array.Copy(this.m_iaUpperBound, destinationArray, this.m_cRank);
                this.m_iaUpperBound = destinationArray;
            }
            this.m_iaLowerBound[this.m_cRank] = lower;
            this.m_iaUpperBound[this.m_cRank] = upper;
            this.m_cRank++;
        }

        internal void SetElementType(Type baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException("baseType");
            }
            this.m_baseType = baseType;
        }

        internal void SetFormat(char[] bFormat, int curIndex, int length)
        {
            char[] destinationArray = new char[length];
            Array.Copy(bFormat, curIndex, destinationArray, 0, length);
            this.m_bFormat = destinationArray;
        }

        public override string ToString()
        {
            return TypeNameBuilder.ToString(this, TypeNameBuilder.Format.ToString);
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                Type baseType = this.m_baseType;
                while (baseType is SymbolType)
                {
                    baseType = ((SymbolType) baseType).m_baseType;
                }
                return baseType.Assembly;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return TypeNameBuilder.ToString(this, TypeNameBuilder.Format.AssemblyQualifiedName);
            }
        }

        public override Type BaseType
        {
            get
            {
                return typeof(Array);
            }
        }

        public override string FullName
        {
            get
            {
                return TypeNameBuilder.ToString(this, TypeNameBuilder.Format.FullName);
            }
        }

        public override Guid GUID
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
            }
        }

        internal override bool IsSzArray
        {
            get
            {
                if (this.m_cRank > 1)
                {
                    return false;
                }
                return this.m_isSzArray;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                Type baseType = this.m_baseType;
                while (baseType is SymbolType)
                {
                    baseType = ((SymbolType) baseType).m_baseType;
                }
                return baseType.Module;
            }
        }

        public override string Name
        {
            get
            {
                string str = new string(this.m_bFormat);
                Type baseType = this.m_baseType;
                while (baseType is SymbolType)
                {
                    str = new string(((SymbolType) baseType).m_bFormat) + str;
                    baseType = ((SymbolType) baseType).m_baseType;
                }
                return (baseType.Name + str);
            }
        }

        public override string Namespace
        {
            get
            {
                return this.m_baseType.Namespace;
            }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonReflectedType"));
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

