namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    internal static class ILGen
    {
        internal static bool CanEmitConstant(object value, Type type)
        {
            if ((value == null) || CanEmitILConstant(type))
            {
                return true;
            }
            Type t = value as Type;
            if ((t != null) && ShouldLdtoken(t))
            {
                return true;
            }
            MethodBase mb = value as MethodBase;
            return ((mb != null) && ShouldLdtoken(mb));
        }

        private static bool CanEmitILConstant(Type type)
        {
            switch (Type.GetTypeCode(type))
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
                case TypeCode.String:
                    return true;
            }
            return false;
        }

        internal static void Emit(this ILGenerator il, OpCode opcode, MethodBase methodBase)
        {
            if (methodBase.MemberType == MemberTypes.Constructor)
            {
                il.Emit(opcode, (ConstructorInfo) methodBase);
            }
            else
            {
                il.Emit(opcode, (MethodInfo) methodBase);
            }
        }

        internal static void EmitArray<T>(this ILGenerator il, IList<T> items)
        {
            ContractUtils.RequiresNotNull(items, "items");
            il.EmitInt(items.Count);
            il.Emit(OpCodes.Newarr, typeof(T));
            for (int i = 0; i < items.Count; i++)
            {
                il.Emit(OpCodes.Dup);
                il.EmitInt(i);
                il.EmitConstant(items[i], typeof(T));
                il.EmitStoreElement(typeof(T));
            }
        }

        internal static void EmitArray(this ILGenerator il, Type arrayType)
        {
            ContractUtils.RequiresNotNull(arrayType, "arrayType");
            if (!arrayType.IsArray)
            {
                throw Error.ArrayTypeMustBeArray();
            }
            int arrayRank = arrayType.GetArrayRank();
            if (arrayRank == 1)
            {
                il.Emit(OpCodes.Newarr, arrayType.GetElementType());
            }
            else
            {
                Type[] paramTypes = new Type[arrayRank];
                for (int i = 0; i < arrayRank; i++)
                {
                    paramTypes[i] = typeof(int);
                }
                il.EmitNew(arrayType, paramTypes);
            }
        }

        internal static void EmitArray(this ILGenerator il, Type elementType, int count, Action<int> emit)
        {
            ContractUtils.RequiresNotNull(elementType, "elementType");
            ContractUtils.RequiresNotNull(emit, "emit");
            if (count < 0)
            {
                throw Error.CountCannotBeNegative();
            }
            il.EmitInt(count);
            il.Emit(OpCodes.Newarr, elementType);
            for (int i = 0; i < count; i++)
            {
                il.Emit(OpCodes.Dup);
                il.EmitInt(i);
                emit(i);
                il.EmitStoreElement(elementType);
            }
        }

        internal static void EmitBoolean(this ILGenerator il, bool value)
        {
            if (value)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
        }

        internal static void EmitByte(this ILGenerator il, byte value)
        {
            il.EmitInt(value);
            il.Emit(OpCodes.Conv_U1);
        }

        private static void EmitCastToType(this ILGenerator il, Type typeFrom, Type typeTo)
        {
            if (!typeFrom.IsValueType && typeTo.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, typeTo);
            }
            else if (typeFrom.IsValueType && !typeTo.IsValueType)
            {
                il.Emit(OpCodes.Box, typeFrom);
                if (typeTo != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, typeTo);
                }
            }
            else
            {
                if (typeFrom.IsValueType || typeTo.IsValueType)
                {
                    throw Error.InvalidCast(typeFrom, typeTo);
                }
                il.Emit(OpCodes.Castclass, typeTo);
            }
        }

        internal static void EmitChar(this ILGenerator il, char value)
        {
            il.EmitInt(value);
            il.Emit(OpCodes.Conv_U2);
        }

        internal static void EmitConstant(this ILGenerator il, object value)
        {
            il.EmitConstant(value, value.GetType());
        }

        internal static void EmitConstant(this ILGenerator il, object value, Type type)
        {
            if (value == null)
            {
                il.EmitDefault(type);
            }
            else if (!il.TryEmitILConstant(value, type))
            {
                Type t = value as Type;
                if ((t != null) && ShouldLdtoken(t))
                {
                    il.EmitType(t);
                    if (type != typeof(Type))
                    {
                        il.Emit(OpCodes.Castclass, type);
                    }
                }
                else
                {
                    MethodBase mb = value as MethodBase;
                    if ((mb == null) || !ShouldLdtoken(mb))
                    {
                        throw ContractUtils.Unreachable;
                    }
                    il.Emit(OpCodes.Ldtoken, mb);
                    Type declaringType = mb.DeclaringType;
                    if ((declaringType != null) && declaringType.IsGenericType)
                    {
                        il.Emit(OpCodes.Ldtoken, declaringType);
                        il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) }));
                    }
                    if (type != typeof(MethodBase))
                    {
                        il.Emit(OpCodes.Castclass, type);
                    }
                }
            }
        }

        internal static void EmitConvertToType(this ILGenerator il, Type typeFrom, Type typeTo, bool isChecked)
        {
            if (!TypeUtils.AreEquivalent(typeFrom, typeTo))
            {
                if ((typeFrom == typeof(void)) || (typeTo == typeof(void)))
                {
                    throw ContractUtils.Unreachable;
                }
                bool flag = typeFrom.IsNullableType();
                bool flag2 = typeTo.IsNullableType();
                Type nonNullableType = typeFrom.GetNonNullableType();
                Type c = typeTo.GetNonNullableType();
                if ((typeFrom.IsInterface || typeTo.IsInterface) || (((typeFrom == typeof(object)) || (typeTo == typeof(object))) || TypeUtils.IsLegalExplicitVariantDelegateConversion(typeFrom, typeTo)))
                {
                    il.EmitCastToType(typeFrom, typeTo);
                }
                else if (flag || flag2)
                {
                    il.EmitNullableConversion(typeFrom, typeTo, isChecked);
                }
                else if ((!TypeUtils.IsConvertible(typeFrom) || !TypeUtils.IsConvertible(typeTo)) && (nonNullableType.IsAssignableFrom(c) || c.IsAssignableFrom(nonNullableType)))
                {
                    il.EmitCastToType(typeFrom, typeTo);
                }
                else if (typeFrom.IsArray && typeTo.IsArray)
                {
                    il.EmitCastToType(typeFrom, typeTo);
                }
                else
                {
                    il.EmitNumericConversion(typeFrom, typeTo, isChecked);
                }
            }
        }

        internal static void EmitDecimal(this ILGenerator il, decimal value)
        {
            if (decimal.Truncate(value) == value)
            {
                if ((-2147483648M <= value) && (value <= 2147483647M))
                {
                    int num = decimal.ToInt32(value);
                    il.EmitInt(num);
                    il.EmitNew(typeof(decimal).GetConstructor(new Type[] { typeof(int) }));
                }
                else if ((-9223372036854775808M <= value) && (value <= 9223372036854775807M))
                {
                    long num2 = decimal.ToInt64(value);
                    il.EmitLong(num2);
                    il.EmitNew(typeof(decimal).GetConstructor(new Type[] { typeof(long) }));
                }
                else
                {
                    il.EmitDecimalBits(value);
                }
            }
            else
            {
                il.EmitDecimalBits(value);
            }
        }

        private static void EmitDecimalBits(this ILGenerator il, decimal value)
        {
            int[] bits = decimal.GetBits(value);
            il.EmitInt(bits[0]);
            il.EmitInt(bits[1]);
            il.EmitInt(bits[2]);
            il.EmitBoolean((bits[3] & 0x80000000L) != 0L);
            il.EmitByte((byte) (bits[3] >> 0x10));
            il.EmitNew(typeof(decimal).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte) }));
        }

        internal static void EmitDefault(this ILGenerator il, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                case TypeCode.String:
                    il.Emit(OpCodes.Ldnull);
                    return;

                case TypeCode.Object:
                case TypeCode.DateTime:
                {
                    if (!type.IsValueType)
                    {
                        il.Emit(OpCodes.Ldnull);
                        return;
                    }
                    LocalBuilder local = il.DeclareLocal(type);
                    il.Emit(OpCodes.Ldloca, local);
                    il.Emit(OpCodes.Initobj, type);
                    il.Emit(OpCodes.Ldloc, local);
                    return;
                }
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Conv_I8);
                    return;

                case TypeCode.Single:
                    il.Emit(OpCodes.Ldc_R4, (float) 0f);
                    return;

                case TypeCode.Double:
                    il.Emit(OpCodes.Ldc_R8, (double) 0.0);
                    return;

                case TypeCode.Decimal:
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Newobj, typeof(decimal).GetConstructor(new Type[] { typeof(int) }));
                    return;
            }
            throw ContractUtils.Unreachable;
        }

        internal static void EmitDouble(this ILGenerator il, double value)
        {
            il.Emit(OpCodes.Ldc_R8, value);
        }

        internal static void EmitFieldAddress(this ILGenerator il, FieldInfo fi)
        {
            ContractUtils.RequiresNotNull(fi, "fi");
            if (fi.IsStatic)
            {
                il.Emit(OpCodes.Ldsflda, fi);
            }
            else
            {
                il.Emit(OpCodes.Ldflda, fi);
            }
        }

        internal static void EmitFieldGet(this ILGenerator il, FieldInfo fi)
        {
            ContractUtils.RequiresNotNull(fi, "fi");
            if (fi.IsStatic)
            {
                il.Emit(OpCodes.Ldsfld, fi);
            }
            else
            {
                il.Emit(OpCodes.Ldfld, fi);
            }
        }

        internal static void EmitFieldSet(this ILGenerator il, FieldInfo fi)
        {
            ContractUtils.RequiresNotNull(fi, "fi");
            if (fi.IsStatic)
            {
                il.Emit(OpCodes.Stsfld, fi);
            }
            else
            {
                il.Emit(OpCodes.Stfld, fi);
            }
        }

        internal static void EmitGetValue(this ILGenerator il, Type nullableType)
        {
            MethodInfo method = nullableType.GetMethod("get_Value", BindingFlags.Public | BindingFlags.Instance);
            il.Emit(OpCodes.Call, method);
        }

        internal static void EmitGetValueOrDefault(this ILGenerator il, Type nullableType)
        {
            MethodInfo method = nullableType.GetMethod("GetValueOrDefault", Type.EmptyTypes);
            il.Emit(OpCodes.Call, method);
        }

        internal static void EmitHasValue(this ILGenerator il, Type nullableType)
        {
            MethodInfo method = nullableType.GetMethod("get_HasValue", BindingFlags.Public | BindingFlags.Instance);
            il.Emit(OpCodes.Call, method);
        }

        internal static void EmitInt(this ILGenerator il, int value)
        {
            OpCode code;
            switch (value)
            {
                case -1:
                    code = OpCodes.Ldc_I4_M1;
                    break;

                case 0:
                    code = OpCodes.Ldc_I4_0;
                    break;

                case 1:
                    code = OpCodes.Ldc_I4_1;
                    break;

                case 2:
                    code = OpCodes.Ldc_I4_2;
                    break;

                case 3:
                    code = OpCodes.Ldc_I4_3;
                    break;

                case 4:
                    code = OpCodes.Ldc_I4_4;
                    break;

                case 5:
                    code = OpCodes.Ldc_I4_5;
                    break;

                case 6:
                    code = OpCodes.Ldc_I4_6;
                    break;

                case 7:
                    code = OpCodes.Ldc_I4_7;
                    break;

                case 8:
                    code = OpCodes.Ldc_I4_8;
                    break;

                default:
                    if ((value >= -128) && (value <= 0x7f))
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte) value);
                        return;
                    }
                    il.Emit(OpCodes.Ldc_I4, value);
                    return;
            }
            il.Emit(code);
        }

        internal static void EmitLoadArg(this ILGenerator il, int index)
        {
            switch (index)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    return;

                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    return;

                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    return;

                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    return;
            }
            if (index <= 0xff)
            {
                il.Emit(OpCodes.Ldarg_S, (byte) index);
            }
            else
            {
                il.Emit(OpCodes.Ldarg, index);
            }
        }

        internal static void EmitLoadArgAddress(this ILGenerator il, int index)
        {
            if (index <= 0xff)
            {
                il.Emit(OpCodes.Ldarga_S, (byte) index);
            }
            else
            {
                il.Emit(OpCodes.Ldarga, index);
            }
        }

        internal static void EmitLoadElement(this ILGenerator il, Type type)
        {
            ContractUtils.RequiresNotNull(type, "type");
            if (!type.IsValueType)
            {
                il.Emit(OpCodes.Ldelem_Ref);
            }
            else if (type.IsEnum)
            {
                il.Emit(OpCodes.Ldelem, type);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                        il.Emit(OpCodes.Ldelem_I1);
                        return;

                    case TypeCode.Char:
                    case TypeCode.UInt16:
                        il.Emit(OpCodes.Ldelem_U2);
                        return;

                    case TypeCode.Byte:
                        il.Emit(OpCodes.Ldelem_U1);
                        return;

                    case TypeCode.Int16:
                        il.Emit(OpCodes.Ldelem_I2);
                        return;

                    case TypeCode.Int32:
                        il.Emit(OpCodes.Ldelem_I4);
                        return;

                    case TypeCode.UInt32:
                        il.Emit(OpCodes.Ldelem_U4);
                        return;

                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        il.Emit(OpCodes.Ldelem_I8);
                        return;

                    case TypeCode.Single:
                        il.Emit(OpCodes.Ldelem_R4);
                        return;

                    case TypeCode.Double:
                        il.Emit(OpCodes.Ldelem_R8);
                        return;
                }
                il.Emit(OpCodes.Ldelem, type);
            }
        }

        internal static void EmitLoadValueIndirect(this ILGenerator il, Type type)
        {
            ContractUtils.RequiresNotNull(type, "type");
            if (type.IsValueType)
            {
                if (type == typeof(int))
                {
                    il.Emit(OpCodes.Ldind_I4);
                }
                else if (type == typeof(uint))
                {
                    il.Emit(OpCodes.Ldind_U4);
                }
                else if (type == typeof(short))
                {
                    il.Emit(OpCodes.Ldind_I2);
                }
                else if (type == typeof(ushort))
                {
                    il.Emit(OpCodes.Ldind_U2);
                }
                else if ((type == typeof(long)) || (type == typeof(ulong)))
                {
                    il.Emit(OpCodes.Ldind_I8);
                }
                else if (type == typeof(char))
                {
                    il.Emit(OpCodes.Ldind_I2);
                }
                else if (type == typeof(bool))
                {
                    il.Emit(OpCodes.Ldind_I1);
                }
                else if (type == typeof(float))
                {
                    il.Emit(OpCodes.Ldind_R4);
                }
                else if (type == typeof(double))
                {
                    il.Emit(OpCodes.Ldind_R8);
                }
                else
                {
                    il.Emit(OpCodes.Ldobj, type);
                }
            }
            else
            {
                il.Emit(OpCodes.Ldind_Ref);
            }
        }

        internal static void EmitLong(this ILGenerator il, long value)
        {
            il.Emit(OpCodes.Ldc_I8, value);
            il.Emit(OpCodes.Conv_I8);
        }

        internal static void EmitNew(this ILGenerator il, ConstructorInfo ci)
        {
            ContractUtils.RequiresNotNull(ci, "ci");
            if (ci.DeclaringType.ContainsGenericParameters)
            {
                throw Error.IllegalNewGenericParams(ci.DeclaringType);
            }
            il.Emit(OpCodes.Newobj, ci);
        }

        internal static void EmitNew(this ILGenerator il, Type type, Type[] paramTypes)
        {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(paramTypes, "paramTypes");
            ConstructorInfo constructor = type.GetConstructor(paramTypes);
            if (constructor == null)
            {
                throw Error.TypeDoesNotHaveConstructorForTheSignature();
            }
            il.EmitNew(constructor);
        }

        private static void EmitNonNullableToNullableConversion(this ILGenerator il, Type typeFrom, Type typeTo, bool isChecked)
        {
            LocalBuilder local = null;
            local = il.DeclareLocal(typeTo);
            Type nonNullableType = typeTo.GetNonNullableType();
            il.EmitConvertToType(typeFrom, nonNullableType, isChecked);
            ConstructorInfo constructor = typeTo.GetConstructor(new Type[] { nonNullableType });
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Stloc, local);
            il.Emit(OpCodes.Ldloc, local);
        }

        internal static void EmitNull(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldnull);
        }

        private static void EmitNullableConversion(this ILGenerator il, Type typeFrom, Type typeTo, bool isChecked)
        {
            bool flag = typeFrom.IsNullableType();
            bool flag2 = typeTo.IsNullableType();
            if (flag && flag2)
            {
                il.EmitNullableToNullableConversion(typeFrom, typeTo, isChecked);
            }
            else if (flag)
            {
                il.EmitNullableToNonNullableConversion(typeFrom, typeTo, isChecked);
            }
            else
            {
                il.EmitNonNullableToNullableConversion(typeFrom, typeTo, isChecked);
            }
        }

        private static void EmitNullableToNonNullableConversion(this ILGenerator il, Type typeFrom, Type typeTo, bool isChecked)
        {
            if (typeTo.IsValueType)
            {
                il.EmitNullableToNonNullableStructConversion(typeFrom, typeTo, isChecked);
            }
            else
            {
                il.EmitNullableToReferenceConversion(typeFrom);
            }
        }

        private static void EmitNullableToNonNullableStructConversion(this ILGenerator il, Type typeFrom, Type typeTo, bool isChecked)
        {
            LocalBuilder local = null;
            local = il.DeclareLocal(typeFrom);
            il.Emit(OpCodes.Stloc, local);
            il.Emit(OpCodes.Ldloca, local);
            il.EmitGetValue(typeFrom);
            Type nonNullableType = typeFrom.GetNonNullableType();
            il.EmitConvertToType(nonNullableType, typeTo, isChecked);
        }

        private static void EmitNullableToNullableConversion(this ILGenerator il, Type typeFrom, Type typeTo, bool isChecked)
        {
            Label label = new Label();
            Label label2 = new Label();
            LocalBuilder local = null;
            LocalBuilder builder2 = null;
            local = il.DeclareLocal(typeFrom);
            il.Emit(OpCodes.Stloc, local);
            builder2 = il.DeclareLocal(typeTo);
            il.Emit(OpCodes.Ldloca, local);
            il.EmitHasValue(typeFrom);
            label = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, label);
            il.Emit(OpCodes.Ldloca, local);
            il.EmitGetValueOrDefault(typeFrom);
            Type nonNullableType = typeFrom.GetNonNullableType();
            Type type2 = typeTo.GetNonNullableType();
            il.EmitConvertToType(nonNullableType, type2, isChecked);
            ConstructorInfo constructor = typeTo.GetConstructor(new Type[] { type2 });
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Stloc, builder2);
            label2 = il.DefineLabel();
            il.Emit(OpCodes.Br_S, label2);
            il.MarkLabel(label);
            il.Emit(OpCodes.Ldloca, builder2);
            il.Emit(OpCodes.Initobj, typeTo);
            il.MarkLabel(label2);
            il.Emit(OpCodes.Ldloc, builder2);
        }

        private static void EmitNullableToReferenceConversion(this ILGenerator il, Type typeFrom)
        {
            il.Emit(OpCodes.Box, typeFrom);
        }

        private static void EmitNumericConversion(this ILGenerator il, Type typeFrom, Type typeTo, bool isChecked)
        {
            bool flag = TypeUtils.IsUnsigned(typeFrom);
            bool flag2 = TypeUtils.IsFloatingPoint(typeFrom);
            if (typeTo == typeof(float))
            {
                if (flag)
                {
                    il.Emit(OpCodes.Conv_R_Un);
                }
                il.Emit(OpCodes.Conv_R4);
            }
            else if (typeTo == typeof(double))
            {
                if (flag)
                {
                    il.Emit(OpCodes.Conv_R_Un);
                }
                il.Emit(OpCodes.Conv_R8);
            }
            else
            {
                TypeCode typeCode = Type.GetTypeCode(typeTo);
                if (isChecked)
                {
                    if (flag)
                    {
                        switch (typeCode)
                        {
                            case TypeCode.Char:
                            case TypeCode.UInt16:
                                il.Emit(OpCodes.Conv_Ovf_U2_Un);
                                return;

                            case TypeCode.SByte:
                                il.Emit(OpCodes.Conv_Ovf_I1_Un);
                                return;

                            case TypeCode.Byte:
                                il.Emit(OpCodes.Conv_Ovf_U1_Un);
                                return;

                            case TypeCode.Int16:
                                il.Emit(OpCodes.Conv_Ovf_I2_Un);
                                return;

                            case TypeCode.Int32:
                                il.Emit(OpCodes.Conv_Ovf_I4_Un);
                                return;

                            case TypeCode.UInt32:
                                il.Emit(OpCodes.Conv_Ovf_U4_Un);
                                return;

                            case TypeCode.Int64:
                                il.Emit(OpCodes.Conv_Ovf_I8_Un);
                                return;

                            case TypeCode.UInt64:
                                il.Emit(OpCodes.Conv_Ovf_U8_Un);
                                return;
                        }
                        throw Error.UnhandledConvert(typeTo);
                    }
                    switch (typeCode)
                    {
                        case TypeCode.Char:
                        case TypeCode.UInt16:
                            il.Emit(OpCodes.Conv_Ovf_U2);
                            return;

                        case TypeCode.SByte:
                            il.Emit(OpCodes.Conv_Ovf_I1);
                            return;

                        case TypeCode.Byte:
                            il.Emit(OpCodes.Conv_Ovf_U1);
                            return;

                        case TypeCode.Int16:
                            il.Emit(OpCodes.Conv_Ovf_I2);
                            return;

                        case TypeCode.Int32:
                            il.Emit(OpCodes.Conv_Ovf_I4);
                            return;

                        case TypeCode.UInt32:
                            il.Emit(OpCodes.Conv_Ovf_U4);
                            return;

                        case TypeCode.Int64:
                            il.Emit(OpCodes.Conv_Ovf_I8);
                            return;

                        case TypeCode.UInt64:
                            il.Emit(OpCodes.Conv_Ovf_U8);
                            return;
                    }
                    throw Error.UnhandledConvert(typeTo);
                }
                switch (typeCode)
                {
                    case TypeCode.Char:
                    case TypeCode.UInt16:
                        il.Emit(OpCodes.Conv_U2);
                        return;

                    case TypeCode.SByte:
                        il.Emit(OpCodes.Conv_I1);
                        return;

                    case TypeCode.Byte:
                        il.Emit(OpCodes.Conv_U1);
                        return;

                    case TypeCode.Int16:
                        il.Emit(OpCodes.Conv_I2);
                        return;

                    case TypeCode.Int32:
                        il.Emit(OpCodes.Conv_I4);
                        return;

                    case TypeCode.UInt32:
                        il.Emit(OpCodes.Conv_U4);
                        return;

                    case TypeCode.Int64:
                        if (!flag)
                        {
                            il.Emit(OpCodes.Conv_I8);
                            return;
                        }
                        il.Emit(OpCodes.Conv_U8);
                        return;

                    case TypeCode.UInt64:
                        if (!flag && !flag2)
                        {
                            il.Emit(OpCodes.Conv_I8);
                            return;
                        }
                        il.Emit(OpCodes.Conv_U8);
                        return;
                }
                throw Error.UnhandledConvert(typeTo);
            }
        }

        internal static void EmitSByte(this ILGenerator il, sbyte value)
        {
            il.EmitInt(value);
            il.Emit(OpCodes.Conv_I1);
        }

        internal static void EmitShort(this ILGenerator il, short value)
        {
            il.EmitInt(value);
            il.Emit(OpCodes.Conv_I2);
        }

        internal static void EmitSingle(this ILGenerator il, float value)
        {
            il.Emit(OpCodes.Ldc_R4, value);
        }

        internal static void EmitStoreArg(this ILGenerator il, int index)
        {
            if (index <= 0xff)
            {
                il.Emit(OpCodes.Starg_S, (byte) index);
            }
            else
            {
                il.Emit(OpCodes.Starg, index);
            }
        }

        internal static void EmitStoreElement(this ILGenerator il, Type type)
        {
            ContractUtils.RequiresNotNull(type, "type");
            if (type.IsEnum)
            {
                il.Emit(OpCodes.Stelem, type);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                        il.Emit(OpCodes.Stelem_I1);
                        return;

                    case TypeCode.Char:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        il.Emit(OpCodes.Stelem_I2);
                        return;

                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        il.Emit(OpCodes.Stelem_I4);
                        return;

                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        il.Emit(OpCodes.Stelem_I8);
                        return;

                    case TypeCode.Single:
                        il.Emit(OpCodes.Stelem_R4);
                        return;

                    case TypeCode.Double:
                        il.Emit(OpCodes.Stelem_R8);
                        return;
                }
                if (type.IsValueType)
                {
                    il.Emit(OpCodes.Stelem, type);
                }
                else
                {
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }
        }

        internal static void EmitStoreValueIndirect(this ILGenerator il, Type type)
        {
            ContractUtils.RequiresNotNull(type, "type");
            if (type.IsValueType)
            {
                if (type == typeof(int))
                {
                    il.Emit(OpCodes.Stind_I4);
                }
                else if (type == typeof(short))
                {
                    il.Emit(OpCodes.Stind_I2);
                }
                else if ((type == typeof(long)) || (type == typeof(ulong)))
                {
                    il.Emit(OpCodes.Stind_I8);
                }
                else if (type == typeof(char))
                {
                    il.Emit(OpCodes.Stind_I2);
                }
                else if (type == typeof(bool))
                {
                    il.Emit(OpCodes.Stind_I1);
                }
                else if (type == typeof(float))
                {
                    il.Emit(OpCodes.Stind_R4);
                }
                else if (type == typeof(double))
                {
                    il.Emit(OpCodes.Stind_R8);
                }
                else
                {
                    il.Emit(OpCodes.Stobj, type);
                }
            }
            else
            {
                il.Emit(OpCodes.Stind_Ref);
            }
        }

        internal static void EmitString(this ILGenerator il, string value)
        {
            ContractUtils.RequiresNotNull(value, "value");
            il.Emit(OpCodes.Ldstr, value);
        }

        internal static void EmitType(this ILGenerator il, Type type)
        {
            ContractUtils.RequiresNotNull(type, "type");
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
        }

        internal static void EmitUInt(this ILGenerator il, uint value)
        {
            il.EmitInt((int) value);
            il.Emit(OpCodes.Conv_U4);
        }

        internal static void EmitULong(this ILGenerator il, ulong value)
        {
            il.Emit(OpCodes.Ldc_I8, (long) value);
            il.Emit(OpCodes.Conv_U8);
        }

        internal static void EmitUShort(this ILGenerator il, ushort value)
        {
            il.EmitInt(value);
            il.Emit(OpCodes.Conv_U2);
        }

        internal static bool ShouldLdtoken(MethodBase mb)
        {
            if (mb is DynamicMethod)
            {
                return false;
            }
            Type declaringType = mb.DeclaringType;
            if (declaringType != null)
            {
                return ShouldLdtoken(declaringType);
            }
            return true;
        }

        internal static bool ShouldLdtoken(Type t)
        {
            if (!(t is TypeBuilder) && !t.IsGenericParameter)
            {
                return t.IsVisible;
            }
            return true;
        }

        private static bool TryEmitILConstant(this ILGenerator il, object value, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    il.EmitBoolean((bool) value);
                    return true;

                case TypeCode.Char:
                    il.EmitChar((char) value);
                    return true;

                case TypeCode.SByte:
                    il.EmitSByte((sbyte) value);
                    return true;

                case TypeCode.Byte:
                    il.EmitByte((byte) value);
                    return true;

                case TypeCode.Int16:
                    il.EmitShort((short) value);
                    return true;

                case TypeCode.UInt16:
                    il.EmitUShort((ushort) value);
                    return true;

                case TypeCode.Int32:
                    il.EmitInt((int) value);
                    return true;

                case TypeCode.UInt32:
                    il.EmitUInt((uint) value);
                    return true;

                case TypeCode.Int64:
                    il.EmitLong((long) value);
                    return true;

                case TypeCode.UInt64:
                    il.EmitULong((ulong) value);
                    return true;

                case TypeCode.Single:
                    il.EmitSingle((float) value);
                    return true;

                case TypeCode.Double:
                    il.EmitDouble((double) value);
                    return true;

                case TypeCode.Decimal:
                    il.EmitDecimal((decimal) value);
                    return true;

                case TypeCode.String:
                    il.EmitString((string) value);
                    return true;
            }
            return false;
        }
    }
}

