namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Remoting;

    internal class Symbols
    {
        internal static readonly string[] NoArgumentNames = new string[0];
        internal static readonly object[] NoArguments = new object[0];
        internal static readonly Type[] NoTypeArguments = new Type[0];
        internal static readonly Type[] NoTypeParameters = new Type[0];
        internal static readonly string[] OperatorCLSNames = new string[0x1c];
        internal static readonly string[] OperatorNames;

        static Symbols()
        {
            OperatorCLSNames[1] = "op_Explicit";
            OperatorCLSNames[2] = "op_Implicit";
            OperatorCLSNames[3] = "op_True";
            OperatorCLSNames[4] = "op_False";
            OperatorCLSNames[5] = "op_UnaryNegation";
            OperatorCLSNames[6] = "op_OnesComplement";
            OperatorCLSNames[7] = "op_UnaryPlus";
            OperatorCLSNames[8] = "op_Addition";
            OperatorCLSNames[9] = "op_Subtraction";
            OperatorCLSNames[10] = "op_Multiply";
            OperatorCLSNames[11] = "op_Division";
            OperatorCLSNames[12] = "op_Exponent";
            OperatorCLSNames[13] = "op_IntegerDivision";
            OperatorCLSNames[14] = "op_Concatenate";
            OperatorCLSNames[15] = "op_LeftShift";
            OperatorCLSNames[0x10] = "op_RightShift";
            OperatorCLSNames[0x11] = "op_Modulus";
            OperatorCLSNames[0x12] = "op_BitwiseOr";
            OperatorCLSNames[0x13] = "op_ExclusiveOr";
            OperatorCLSNames[20] = "op_BitwiseAnd";
            OperatorCLSNames[0x15] = "op_Like";
            OperatorCLSNames[0x16] = "op_Equality";
            OperatorCLSNames[0x17] = "op_Inequality";
            OperatorCLSNames[0x18] = "op_LessThan";
            OperatorCLSNames[0x19] = "op_LessThanOrEqual";
            OperatorCLSNames[0x1a] = "op_GreaterThanOrEqual";
            OperatorCLSNames[0x1b] = "op_GreaterThan";
            OperatorNames = new string[0x1c];
            OperatorNames[1] = "CType";
            OperatorNames[2] = "CType";
            OperatorNames[3] = "IsTrue";
            OperatorNames[4] = "IsFalse";
            OperatorNames[5] = "-";
            OperatorNames[6] = "Not";
            OperatorNames[7] = "+";
            OperatorNames[8] = "+";
            OperatorNames[9] = "-";
            OperatorNames[10] = "*";
            OperatorNames[11] = "/";
            OperatorNames[12] = "^";
            OperatorNames[13] = @"\";
            OperatorNames[14] = "&";
            OperatorNames[15] = "<<";
            OperatorNames[0x10] = ">>";
            OperatorNames[0x11] = "Mod";
            OperatorNames[0x12] = "Or";
            OperatorNames[0x13] = "Xor";
            OperatorNames[20] = "And";
            OperatorNames[0x15] = "Like";
            OperatorNames[0x16] = "=";
            OperatorNames[0x17] = "<>";
            OperatorNames[0x18] = "<";
            OperatorNames[0x19] = "<=";
            OperatorNames[0x1a] = ">=";
            OperatorNames[0x1b] = ">";
        }

        private Symbols()
        {
        }

        internal static bool AreGenericMethodDefsEqual(MethodBase Method1, MethodBase Method2)
        {
            if ((Method1 != Method2) && (Method1.MetadataToken != Method2.MetadataToken))
            {
                return false;
            }
            return true;
        }

        internal static bool AreParametersAndReturnTypesValid(ParameterInfo[] Parameters, Type ReturnType)
        {
            if ((ReturnType != null) && (ReturnType.IsPointer || ReturnType.IsByRef))
            {
                return false;
            }
            if (Parameters != null)
            {
                foreach (ParameterInfo info in Parameters)
                {
                    if (info.ParameterType.IsPointer)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static void GetAllParameterCounts(ParameterInfo[] Parameters, ref int RequiredParameterCount, ref int MaximumParameterCount, ref int ParamArrayIndex)
        {
            MaximumParameterCount = Parameters.Length;
            for (int i = MaximumParameterCount - 1; i >= 0; i += -1)
            {
                if (!Parameters[i].IsOptional)
                {
                    RequiredParameterCount = i + 1;
                    break;
                }
            }
            if ((MaximumParameterCount != 0) && IsParamArray(Parameters[MaximumParameterCount - 1]))
            {
                ParamArrayIndex = MaximumParameterCount - 1;
                RequiredParameterCount--;
            }
        }

        internal static Type GetClassConstraint(Type GenericParameter)
        {
            Type baseType = GenericParameter.BaseType;
            if (IsRootObjectType(baseType))
            {
                return null;
            }
            return baseType;
        }

        internal static Type GetElementType(Type Type)
        {
            return Type.GetElementType();
        }

        internal static Type[] GetInterfaceConstraints(Type GenericParameter)
        {
            return GenericParameter.GetInterfaces();
        }

        internal static Type[] GetTypeArguments(Type Type)
        {
            return Type.GetGenericArguments();
        }

        internal static TypeCode GetTypeCode(Type Type)
        {
            return Type.GetTypeCode(Type);
        }

        internal static Type[] GetTypeParameters(MemberInfo Member)
        {
            MethodBase base2 = Member as MethodBase;
            if (base2 == null)
            {
                return NoTypeParameters;
            }
            return base2.GetGenericArguments();
        }

        internal static Type[] GetTypeParameters(Type Type)
        {
            return Type.GetGenericArguments();
        }

        internal static bool HasFlag(BindingFlags Flags, BindingFlags FlagToTest)
        {
            return ((Flags & FlagToTest) > BindingFlags.Default);
        }

        internal static bool Implements(Type Implementor, Type Interface)
        {
            foreach (Type type in Implementor.GetInterfaces())
            {
                if ((type == Interface) || IsEquivalentType(type, Interface))
                {
                    return true;
                }
            }
            return false;
        }

        internal static int IndexIn(Type PossibleGenericParameter, MethodBase GenericMethodDef)
        {
            if ((IsGenericParameter(PossibleGenericParameter) && (PossibleGenericParameter.DeclaringMethod != null)) && AreGenericMethodDefsEqual(PossibleGenericParameter.DeclaringMethod, GenericMethodDef))
            {
                return PossibleGenericParameter.GenericParameterPosition;
            }
            return -1;
        }

        internal static bool IsArrayType(Type Type)
        {
            return Type.IsArray;
        }

        internal static bool IsBinaryOperator(UserDefinedOperator Op)
        {
            switch (Op)
            {
                case UserDefinedOperator.Plus:
                case UserDefinedOperator.Minus:
                case UserDefinedOperator.Multiply:
                case UserDefinedOperator.Divide:
                case UserDefinedOperator.Power:
                case UserDefinedOperator.IntegralDivide:
                case UserDefinedOperator.Concatenate:
                case UserDefinedOperator.ShiftLeft:
                case UserDefinedOperator.ShiftRight:
                case UserDefinedOperator.Modulus:
                case UserDefinedOperator.Or:
                case UserDefinedOperator.Xor:
                case UserDefinedOperator.And:
                case UserDefinedOperator.Like:
                case UserDefinedOperator.Equal:
                case UserDefinedOperator.NotEqual:
                case UserDefinedOperator.Less:
                case UserDefinedOperator.LessEqual:
                case UserDefinedOperator.GreaterEqual:
                case UserDefinedOperator.Greater:
                    return true;
            }
            return false;
        }

        internal static bool IsCharArrayRankOne(Type Type)
        {
            return (Type == typeof(char[]));
        }

        internal static bool IsClass(Type Type)
        {
            if (!Type.IsClass && !IsRootEnumType(Type))
            {
                return false;
            }
            return true;
        }

        internal static bool IsClassOrInterface(Type Type)
        {
            if (!IsClass(Type) && !IsInterface(Type))
            {
                return false;
            }
            return true;
        }

        internal static bool IsClassOrValueType(Type Type)
        {
            if (!IsValueType(Type) && !IsClass(Type))
            {
                return false;
            }
            return true;
        }

        internal static bool IsEnum(Type Type)
        {
            return Type.IsEnum;
        }

        internal static bool IsEquivalentType(Type Left, Type Right)
        {
            if ((!IsInstantiatedGeneric(Left) || Left.IsInterface) || (!IsInstantiatedGeneric(Right) || Right.IsInterface))
            {
                return Left.IsEquivalentTo(Right);
            }
            if (!IsEquivalentType(Left.GetGenericTypeDefinition(), Right.GetGenericTypeDefinition()))
            {
                return false;
            }
            Type[] genericArguments = Left.GetGenericArguments();
            Type[] typeArray2 = Right.GetGenericArguments();
            if (genericArguments.Length != typeArray2.Length)
            {
                return false;
            }
            int num2 = genericArguments.Length - 1;
            for (int i = 0; i <= num2; i++)
            {
                if (!IsEquivalentType(genericArguments[i], typeArray2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsGeneric(MemberInfo Member)
        {
            MethodBase method = Member as MethodBase;
            if (method == null)
            {
                return false;
            }
            return IsGeneric(method);
        }

        internal static bool IsGeneric(MethodBase Method)
        {
            return Method.IsGenericMethod;
        }

        internal static bool IsGeneric(Type Type)
        {
            return Type.IsGenericType;
        }

        internal static bool IsGenericParameter(Type Type)
        {
            return Type.IsGenericParameter;
        }

        internal static bool IsInstantiatedGeneric(Type Type)
        {
            return (Type.IsGenericType && !Type.IsGenericTypeDefinition);
        }

        internal static bool IsIntegralType(TypeCode TypeCode)
        {
            switch (TypeCode)
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
            }
            return false;
        }

        internal static bool IsInterface(Type Type)
        {
            return Type.IsInterface;
        }

        internal static bool IsIntrinsicType(Type Type)
        {
            return (IsIntrinsicType(GetTypeCode(Type)) && !IsEnum(Type));
        }

        internal static bool IsIntrinsicType(TypeCode TypeCode)
        {
            switch (TypeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return true;
            }
            return false;
        }

        internal static bool IsNarrowingConversionOperator(MethodBase Method)
        {
            return (Method.IsSpecialName && Method.Name.Equals(OperatorCLSNames[1]));
        }

        internal static bool IsNonPublicRuntimeMember(MemberInfo Member)
        {
            Type declaringType = Member.DeclaringType;
            return (!declaringType.IsPublic && (declaringType.Assembly == Utils.VBRuntimeAssembly));
        }

        internal static bool IsNumericType(Type Type)
        {
            return IsNumericType(GetTypeCode(Type));
        }

        internal static bool IsNumericType(TypeCode TypeCode)
        {
            switch (TypeCode)
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
            }
            return false;
        }

        internal static bool IsOrInheritsFrom(Type Derived, Type Base)
        {
            if (Derived == Base)
            {
                return true;
            }
            if (Derived.IsGenericParameter)
            {
                if ((IsClass(Base) && ((Derived.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) > GenericParameterAttributes.None)) && IsOrInheritsFrom(typeof(ValueType), Base))
                {
                    return true;
                }
                foreach (Type type in Derived.GetGenericParameterConstraints())
                {
                    if (IsOrInheritsFrom(type, Base))
                    {
                        return true;
                    }
                }
            }
            else if (IsInterface(Derived))
            {
                if (IsInterface(Base))
                {
                    foreach (Type type2 in Derived.GetInterfaces())
                    {
                        if (type2 == Base)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (IsClass(Base) && IsClassOrValueType(Derived))
            {
                return Derived.IsSubclassOf(Base);
            }
            return false;
        }

        internal static bool IsParamArray(ParameterInfo Parameter)
        {
            return (IsArrayType(Parameter.ParameterType) && Parameter.IsDefined(typeof(ParamArrayAttribute), false));
        }

        internal static bool IsRawGeneric(MethodBase Method)
        {
            return (Method.IsGenericMethod && Method.IsGenericMethodDefinition);
        }

        internal static bool IsReferenceType(Type Type)
        {
            if (!IsClass(Type) && !IsInterface(Type))
            {
                return false;
            }
            return true;
        }

        internal static bool IsRootEnumType(Type Type)
        {
            return (Type == typeof(Enum));
        }

        internal static bool IsRootObjectType(Type Type)
        {
            return (Type == typeof(object));
        }

        internal static bool IsShadows(MethodBase Method)
        {
            if (Method.IsHideBySig)
            {
                return false;
            }
            if ((Method.IsVirtual && ((Method.Attributes & MethodAttributes.NewSlot) == MethodAttributes.PrivateScope)) && ((((MethodInfo) Method).GetBaseDefinition().Attributes & MethodAttributes.NewSlot) == MethodAttributes.PrivateScope))
            {
                return false;
            }
            return true;
        }

        internal static bool IsShared(MemberInfo Member)
        {
            switch (Member.MemberType)
            {
                case MemberTypes.Constructor:
                    return ((ConstructorInfo) Member).IsStatic;

                case MemberTypes.Field:
                    return ((FieldInfo) Member).IsStatic;

                case MemberTypes.Method:
                    return ((MethodInfo) Member).IsStatic;

                case MemberTypes.Property:
                    return ((PropertyInfo) Member).GetGetMethod().IsStatic;
            }
            return false;
        }

        internal static bool IsStringType(Type Type)
        {
            return (Type == typeof(string));
        }

        internal static bool IsUnaryOperator(UserDefinedOperator Op)
        {
            switch (Op)
            {
                case UserDefinedOperator.Narrow:
                case UserDefinedOperator.Widen:
                case UserDefinedOperator.IsTrue:
                case UserDefinedOperator.IsFalse:
                case UserDefinedOperator.Negate:
                case UserDefinedOperator.Not:
                case UserDefinedOperator.UnaryPlus:
                    return true;
            }
            return false;
        }

        internal static bool IsUserDefinedOperator(MethodBase Method)
        {
            return (Method.IsSpecialName && Method.Name.StartsWith("op_", StringComparison.Ordinal));
        }

        internal static bool IsValueType(Type Type)
        {
            return Type.IsValueType;
        }

        internal static UserDefinedOperator MapToUserDefinedOperator(MethodBase Method)
        {
            int index = 1;
            do
            {
                if (Method.Name.Equals(OperatorCLSNames[index]))
                {
                    int length = Method.GetParameters().Length;
                    UserDefinedOperator op = (UserDefinedOperator) ((sbyte) index);
                    if ((length == 1) && IsUnaryOperator(op))
                    {
                        return op;
                    }
                    if ((length == 2) && IsBinaryOperator(op))
                    {
                        return op;
                    }
                }
                index++;
            }
            while (index <= 0x1b);
            return UserDefinedOperator.UNDEF;
        }

        internal static Type MapTypeCodeToType(TypeCode TypeCode)
        {
            switch (TypeCode)
            {
                case TypeCode.Object:
                    return typeof(object);

                case TypeCode.DBNull:
                    return typeof(DBNull);

                case TypeCode.Boolean:
                    return typeof(bool);

                case TypeCode.Char:
                    return typeof(char);

                case TypeCode.SByte:
                    return typeof(sbyte);

                case TypeCode.Byte:
                    return typeof(byte);

                case TypeCode.Int16:
                    return typeof(short);

                case TypeCode.UInt16:
                    return typeof(ushort);

                case TypeCode.Int32:
                    return typeof(int);

                case TypeCode.UInt32:
                    return typeof(uint);

                case TypeCode.Int64:
                    return typeof(long);

                case TypeCode.UInt64:
                    return typeof(ulong);

                case TypeCode.Single:
                    return typeof(float);

                case TypeCode.Double:
                    return typeof(double);

                case TypeCode.Decimal:
                    return typeof(decimal);

                case TypeCode.DateTime:
                    return typeof(DateTime);

                case TypeCode.String:
                    return typeof(string);
            }
            return null;
        }

        internal static bool RefersToGenericParameter(Type ReferringType, MethodBase Method)
        {
            if (IsRawGeneric(Method))
            {
                if (ReferringType.IsByRef)
                {
                    ReferringType = GetElementType(ReferringType);
                }
                if (IsGenericParameter(ReferringType))
                {
                    if (AreGenericMethodDefsEqual(ReferringType.DeclaringMethod, Method))
                    {
                        return true;
                    }
                }
                else if (IsGeneric(ReferringType))
                {
                    foreach (Type type in GetTypeArguments(ReferringType))
                    {
                        if (RefersToGenericParameter(type, Method))
                        {
                            return true;
                        }
                    }
                }
                else if (IsArrayType(ReferringType))
                {
                    return RefersToGenericParameter(ReferringType.GetElementType(), Method);
                }
            }
            return false;
        }

        internal static bool RefersToGenericParameterCLRSemantics(Type ReferringType, Type Typ)
        {
            if (ReferringType.IsByRef)
            {
                ReferringType = GetElementType(ReferringType);
            }
            if (IsGenericParameter(ReferringType))
            {
                if (ReferringType.DeclaringType == Typ)
                {
                    return true;
                }
            }
            else if (IsGeneric(ReferringType))
            {
                foreach (Type type in GetTypeArguments(ReferringType))
                {
                    if (RefersToGenericParameterCLRSemantics(type, Typ))
                    {
                        return true;
                    }
                }
            }
            else if (IsArrayType(ReferringType))
            {
                return RefersToGenericParameterCLRSemantics(ReferringType.GetElementType(), Typ);
            }
            return false;
        }

        internal sealed class Container
        {
            private const BindingFlags DefaultLookupFlags = (BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            private readonly object m_Instance;
            private readonly IReflect m_IReflect;
            private readonly Type m_Type;
            private readonly bool m_UseCustomReflection;
            private static readonly MemberInfo[] NoMembers = new MemberInfo[0];

            internal Container(object Instance)
            {
                if (Instance == null)
                {
                    throw ExceptionUtils.VbMakeException(0x5b);
                }
                this.m_Instance = Instance;
                this.m_Type = Instance.GetType();
                this.m_UseCustomReflection = false;
                if ((!this.m_Type.IsCOMObject && !RemotingServices.IsTransparentProxy(Instance)) && !(Instance is Type))
                {
                    this.m_IReflect = Instance as IReflect;
                    if (this.m_IReflect != null)
                    {
                        this.m_UseCustomReflection = true;
                    }
                }
                if (!this.m_UseCustomReflection)
                {
                    this.m_IReflect = this.m_Type;
                }
                this.CheckForClassExtendingCOMClass();
            }

            internal Container(Type Type)
            {
                if (Type == null)
                {
                    throw ExceptionUtils.VbMakeException(0x5b);
                }
                this.m_Instance = null;
                this.m_Type = Type;
                this.m_IReflect = Type;
                this.m_UseCustomReflection = false;
                this.CheckForClassExtendingCOMClass();
            }

            private void CheckForClassExtendingCOMClass()
            {
                if ((this.IsCOMObject && (this.m_Type.FullName != "System.__ComObject")) && (this.m_Type.BaseType.FullName != "System.__ComObject"))
                {
                    throw new InvalidOperationException(Utils.GetResourceString("LateboundCallToInheritedComClass"));
                }
            }

            private static MemberInfo[] FilterInvalidMembers(MemberInfo[] Members)
            {
                if ((Members == null) || (Members.Length == 0))
                {
                    return null;
                }
                int num2 = 0;
                int index = 0;
                int num4 = Members.Length - 1;
                for (index = 0; index <= num4; index++)
                {
                    PropertyInfo info3;
                    ParameterInfo[] destinationArray = null;
                    Type returnType = null;
                    switch (Members[index].MemberType)
                    {
                        case MemberTypes.Constructor:
                        case MemberTypes.Method:
                        {
                            MethodInfo info = (MethodInfo) Members[index];
                            destinationArray = info.GetParameters();
                            returnType = info.ReturnType;
                            goto Label_00F9;
                        }
                        case MemberTypes.Field:
                            returnType = ((FieldInfo) Members[index]).FieldType;
                            goto Label_00F9;

                        case MemberTypes.Property:
                        {
                            info3 = (PropertyInfo) Members[index];
                            MethodInfo getMethod = info3.GetGetMethod();
                            if (getMethod == null)
                            {
                                break;
                            }
                            destinationArray = getMethod.GetParameters();
                            goto Label_00DF;
                        }
                        default:
                            goto Label_00F9;
                    }
                    ParameterInfo[] parameters = info3.GetSetMethod().GetParameters();
                    destinationArray = new ParameterInfo[(parameters.Length - 2) + 1];
                    Array.Copy(parameters, destinationArray, destinationArray.Length);
                Label_00DF:
                    returnType = info3.PropertyType;
                Label_00F9:
                    if (Symbols.AreParametersAndReturnTypesValid(destinationArray, returnType))
                    {
                        num2++;
                    }
                    else
                    {
                        Members[index] = null;
                    }
                }
                if (num2 == Members.Length)
                {
                    return Members;
                }
                if (num2 <= 0)
                {
                    return null;
                }
                MemberInfo[] infoArray4 = new MemberInfo[(num2 - 1) + 1];
                int num3 = 0;
                int num5 = Members.Length - 1;
                for (index = 0; index <= num5; index++)
                {
                    if (Members[index] != null)
                    {
                        infoArray4[num3] = Members[index];
                        num3++;
                    }
                }
                return infoArray4;
            }

            internal object GetArrayValue(object[] Indices)
            {
                Array instance = (Array) this.m_Instance;
                int rank = instance.Rank;
                if (Indices.Length != rank)
                {
                    throw new RankException();
                }
                int index = (int) Conversions.ChangeType(Indices[0], typeof(int));
                if (rank == 1)
                {
                    return instance.GetValue(index);
                }
                int num3 = (int) Conversions.ChangeType(Indices[1], typeof(int));
                if (rank == 2)
                {
                    return instance.GetValue(index, num3);
                }
                int num4 = (int) Conversions.ChangeType(Indices[2], typeof(int));
                if (rank == 3)
                {
                    return instance.GetValue(index, num3, num4);
                }
                int[] indices = new int[(rank - 1) + 1];
                indices[0] = index;
                indices[1] = num3;
                indices[2] = num4;
                int num6 = rank - 1;
                for (int i = 3; i <= num6; i++)
                {
                    indices[i] = (int) Conversions.ChangeType(Indices[i], typeof(int));
                }
                return instance.GetValue(indices);
            }

            internal object GetFieldValue(FieldInfo Field)
            {
                if ((this.m_Instance == null) && !Symbols.IsShared(Field))
                {
                    throw new NullReferenceException(Utils.GetResourceString("NullReference_InstanceReqToAccessMember1", new string[] { Utils.FieldToString(Field) }));
                }
                if (Symbols.IsNonPublicRuntimeMember(Field))
                {
                    throw new MissingMemberException();
                }
                return Field.GetValue(this.m_Instance);
            }

            internal MemberInfo[] GetMembers(ref string MemberName, bool ReportErrors)
            {
                MemberInfo[] namedMembers;
                if (MemberName == null)
                {
                    MemberName = "";
                }
                if (MemberName == "")
                {
                    if (this.m_UseCustomReflection)
                    {
                        namedMembers = this.LookupNamedMembers(MemberName);
                    }
                    else
                    {
                        namedMembers = this.LookupDefaultMembers(ref MemberName);
                    }
                    if (namedMembers.Length == 0)
                    {
                        if (ReportErrors)
                        {
                            throw new MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", new string[] { this.VBFriendlyName }));
                        }
                        return namedMembers;
                    }
                    if (this.m_UseCustomReflection)
                    {
                        MemberName = namedMembers[0].Name;
                    }
                    return namedMembers;
                }
                namedMembers = this.LookupNamedMembers(MemberName);
                if ((namedMembers.Length == 0) && ReportErrors)
                {
                    throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", new string[] { MemberName, this.VBFriendlyName }));
                }
                return namedMembers;
            }

            internal object InvokeMethod(Symbols.Method TargetProcedure, object[] Arguments, bool[] CopyBack, BindingFlags Flags)
            {
                object obj3;
                MethodBase callTarget = NewLateBinding.GetCallTarget(TargetProcedure, Flags);
                object[] parameters = NewLateBinding.ConstructCallArguments(TargetProcedure, Arguments, Flags);
                if ((this.m_Instance == null) && !Symbols.IsShared(callTarget))
                {
                    throw new NullReferenceException(Utils.GetResourceString("NullReference_InstanceReqToAccessMember1", new string[] { TargetProcedure.ToString() }));
                }
                if (Symbols.IsNonPublicRuntimeMember(callTarget))
                {
                    throw new MissingMemberException();
                }
                try
                {
                    obj3 = callTarget.Invoke(this.m_Instance, parameters);
                }
                catch when (?)
                {
                    TargetInvocationException exception;
                    throw exception.InnerException;
                }
                OverloadResolution.ReorderArgumentArray(TargetProcedure, parameters, Arguments, CopyBack, Flags);
                return obj3;
            }

            private MemberInfo[] LookupDefaultMembers(ref string DefaultMemberName)
            {
                string name = null;
                Type baseType = this.m_Type;
                do
                {
                    object[] customAttributes = baseType.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        name = ((DefaultMemberAttribute) customAttributes[0]).MemberName;
                        break;
                    }
                    baseType = baseType.BaseType;
                }
                while ((baseType != null) && !Symbols.IsRootObjectType(baseType));
                if (name != null)
                {
                    MemberInfo[] array = FilterInvalidMembers(baseType.GetMember(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase));
                    if (array != null)
                    {
                        DefaultMemberName = name;
                        if (array.Length > 1)
                        {
                            Array.Sort(array, InheritanceSorter.Instance);
                        }
                        return array;
                    }
                }
                return NoMembers;
            }

            internal MemberInfo[] LookupNamedMembers(string MemberName)
            {
                MemberInfo[] member;
                if (Symbols.IsGenericParameter(this.m_Type))
                {
                    Type classConstraint = Symbols.GetClassConstraint(this.m_Type);
                    if (classConstraint != null)
                    {
                        member = classConstraint.GetMember(MemberName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    }
                    else
                    {
                        member = null;
                    }
                }
                else
                {
                    member = this.m_IReflect.GetMember(MemberName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
                }
                member = FilterInvalidMembers(member);
                if (member == null)
                {
                    return NoMembers;
                }
                if (member.Length > 1)
                {
                    Array.Sort(member, InheritanceSorter.Instance);
                }
                return member;
            }

            internal void SetArrayValue(object[] Arguments)
            {
                Array instance = (Array) this.m_Instance;
                int rank = instance.Rank;
                if ((Arguments.Length - 1) != rank)
                {
                    throw new RankException();
                }
                object expression = Arguments[Arguments.Length - 1];
                Type elementType = this.m_Type.GetElementType();
                int index = (int) Conversions.ChangeType(Arguments[0], typeof(int));
                if (rank == 1)
                {
                    instance.SetValue(Conversions.ChangeType(expression, elementType), index);
                }
                else
                {
                    int num3 = (int) Conversions.ChangeType(Arguments[1], typeof(int));
                    if (rank == 2)
                    {
                        instance.SetValue(Conversions.ChangeType(expression, elementType), index, num3);
                    }
                    else
                    {
                        int num4 = (int) Conversions.ChangeType(Arguments[2], typeof(int));
                        if (rank == 3)
                        {
                            instance.SetValue(Conversions.ChangeType(expression, elementType), index, num3, num4);
                        }
                        else
                        {
                            int[] indices = new int[(rank - 1) + 1];
                            indices[0] = index;
                            indices[1] = num3;
                            indices[2] = num4;
                            int num6 = rank - 1;
                            for (int i = 3; i <= num6; i++)
                            {
                                indices[i] = (int) Conversions.ChangeType(Arguments[i], typeof(int));
                            }
                            instance.SetValue(Conversions.ChangeType(expression, elementType), indices);
                        }
                    }
                }
            }

            internal void SetFieldValue(FieldInfo Field, object Value)
            {
                if (Field.IsInitOnly)
                {
                    throw new MissingMemberException(Utils.GetResourceString("MissingMember_ReadOnlyField2", new string[] { Field.Name, this.VBFriendlyName }));
                }
                if ((this.m_Instance == null) && !Symbols.IsShared(Field))
                {
                    throw new NullReferenceException(Utils.GetResourceString("NullReference_InstanceReqToAccessMember1", new string[] { Utils.FieldToString(Field) }));
                }
                if (Symbols.IsNonPublicRuntimeMember(Field))
                {
                    throw new MissingMemberException();
                }
                Field.SetValue(this.m_Instance, Conversions.ChangeType(Value, Field.FieldType));
            }

            internal bool IsArray
            {
                get
                {
                    return (Symbols.IsArrayType(this.m_Type) && (this.m_Instance != null));
                }
            }

            internal bool IsCOMObject
            {
                get
                {
                    return this.m_Type.IsCOMObject;
                }
            }

            internal bool IsValueType
            {
                get
                {
                    return (Symbols.IsValueType(this.m_Type) && (this.m_Instance != null));
                }
            }

            internal string VBFriendlyName
            {
                get
                {
                    return Utils.VBFriendlyName(this.m_Type, this.m_Instance);
                }
            }

            private class InheritanceSorter : IComparer
            {
                internal static readonly Symbols.Container.InheritanceSorter Instance = new Symbols.Container.InheritanceSorter();

                private InheritanceSorter()
                {
                }

                private int Compare(object Left, object Right)
                {
                    Type declaringType = ((MemberInfo) Left).DeclaringType;
                    Type c = ((MemberInfo) Right).DeclaringType;
                    if (declaringType == c)
                    {
                        return 0;
                    }
                    if (declaringType.IsSubclassOf(c))
                    {
                        return -1;
                    }
                    return 1;
                }
            }
        }

        internal sealed class Method
        {
            internal bool AllNarrowingIsFromObject;
            internal bool ArgumentMatchingDone;
            internal bool ArgumentsValidated;
            internal bool LessSpecific;
            private MemberInfo m_Item;
            private ParameterInfo[] m_Parameters;
            private Type m_RawDeclaringType;
            private MethodBase m_RawItem;
            private ParameterInfo[] m_RawParameters;
            private ParameterInfo[] m_RawParametersFromType;
            internal int[] NamedArgumentMapping;
            internal bool NotCallable;
            internal readonly bool ParamArrayExpanded;
            internal readonly int ParamArrayIndex;
            internal bool RequiresNarrowingConversion;
            internal Type[] TypeArguments;

            private Method(ParameterInfo[] Parameters, int ParamArrayIndex, bool ParamArrayExpanded)
            {
                this.m_Parameters = Parameters;
                this.m_RawParameters = Parameters;
                this.ParamArrayIndex = ParamArrayIndex;
                this.ParamArrayExpanded = ParamArrayExpanded;
                this.AllNarrowingIsFromObject = true;
            }

            internal Method(MethodBase Method, ParameterInfo[] Parameters, int ParamArrayIndex, bool ParamArrayExpanded) : this(Parameters, ParamArrayIndex, ParamArrayExpanded)
            {
                this.m_Item = Method;
                this.m_RawItem = Method;
            }

            internal Method(PropertyInfo Property, ParameterInfo[] Parameters, int ParamArrayIndex, bool ParamArrayExpanded) : this(Parameters, ParamArrayIndex, ParamArrayExpanded)
            {
                this.m_Item = Property;
            }

            internal MethodBase AsMethod()
            {
                return (this.m_Item as MethodBase);
            }

            internal PropertyInfo AsProperty()
            {
                return (this.m_Item as PropertyInfo);
            }

            internal bool BindGenericArguments()
            {
                try
                {
                    this.m_Item = ((MethodInfo) this.m_RawItem).MakeGenericMethod(this.TypeArguments);
                    this.m_Parameters = this.AsMethod().GetParameters();
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            public static bool operator ==(Symbols.Method Left, Symbols.Method Right)
            {
                return (Left.m_Item == Right.m_Item);
            }

            public static bool operator ==(MemberInfo Left, Symbols.Method Right)
            {
                return (Left == Right.m_Item);
            }

            public static bool operator !=(Symbols.Method Left, Symbols.Method right)
            {
                return !(Left.m_Item == right.m_Item);
            }

            public static bool operator !=(MemberInfo Left, Symbols.Method Right)
            {
                return !(Left == Right.m_Item);
            }

            public override string ToString()
            {
                return Utils.MemberToString(this.m_Item);
            }

            internal Type DeclaringType
            {
                get
                {
                    return this.m_Item.DeclaringType;
                }
            }

            internal bool HasByRefParameter
            {
                get
                {
                    foreach (ParameterInfo info in this.Parameters)
                    {
                        if (info.ParameterType.IsByRef)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            internal bool HasParamArray
            {
                get
                {
                    return (this.ParamArrayIndex > -1);
                }
            }

            internal bool IsGeneric
            {
                get
                {
                    return Symbols.IsGeneric(this.m_Item);
                }
            }

            internal bool IsMethod
            {
                get
                {
                    if ((this.m_Item.MemberType != MemberTypes.Method) && (this.m_Item.MemberType != MemberTypes.Constructor))
                    {
                        return false;
                    }
                    return true;
                }
            }

            internal bool IsProperty
            {
                get
                {
                    return (this.m_Item.MemberType == MemberTypes.Property);
                }
            }

            internal ParameterInfo[] Parameters
            {
                get
                {
                    return this.m_Parameters;
                }
            }

            internal Type RawDeclaringType
            {
                get
                {
                    if (this.m_RawDeclaringType == null)
                    {
                        Type declaringType = this.m_Item.DeclaringType;
                        int metadataToken = declaringType.MetadataToken;
                        this.m_RawDeclaringType = declaringType.Module.ResolveType(metadataToken, null, null);
                    }
                    return this.m_RawDeclaringType;
                }
            }

            internal ParameterInfo[] RawParameters
            {
                get
                {
                    return this.m_RawParameters;
                }
            }

            internal ParameterInfo[] RawParametersFromType
            {
                get
                {
                    if (this.m_RawParametersFromType == null)
                    {
                        if (!this.IsProperty)
                        {
                            int metadataToken = this.m_Item.MetadataToken;
                            this.m_RawParametersFromType = this.m_Item.DeclaringType.Module.ResolveMethod(metadataToken, null, null).GetParameters();
                        }
                        else
                        {
                            this.m_RawParametersFromType = this.m_RawParameters;
                        }
                    }
                    return this.m_RawParametersFromType;
                }
            }

            internal Type[] TypeParameters
            {
                get
                {
                    return Symbols.GetTypeParameters(this.m_Item);
                }
            }
        }

        internal sealed class TypedNothing
        {
            internal readonly System.Type Type;

            internal TypedNothing(System.Type Type)
            {
                this.Type = Type;
            }
        }

        internal enum UserDefinedOperator : sbyte
        {
            And = 20,
            Concatenate = 14,
            Divide = 11,
            Equal = 0x16,
            Greater = 0x1b,
            GreaterEqual = 0x1a,
            IntegralDivide = 13,
            IsFalse = 4,
            IsTrue = 3,
            Less = 0x18,
            LessEqual = 0x19,
            Like = 0x15,
            MAX = 0x1c,
            Minus = 9,
            Modulus = 0x11,
            Multiply = 10,
            Narrow = 1,
            Negate = 5,
            Not = 6,
            NotEqual = 0x17,
            Or = 0x12,
            Plus = 8,
            Power = 12,
            ShiftLeft = 15,
            ShiftRight = 0x10,
            UnaryPlus = 7,
            UNDEF = 0,
            Widen = 2,
            Xor = 0x13
        }
    }
}

