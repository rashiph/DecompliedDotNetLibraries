namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class BitwiseBinary : BinaryOp
    {
        private object metaData;

        public BitwiseBinary(int operatorTok) : base(null, null, null, (JSToken) operatorTok)
        {
        }

        internal BitwiseBinary(Context context, AST operand1, AST operand2, JSToken operatorTok) : base(context, operand1, operand2, operatorTok)
        {
        }

        internal static object DoOp(int i, int j, JSToken operatorTok)
        {
            switch (operatorTok)
            {
                case JSToken.BitwiseOr:
                    return (i | j);

                case JSToken.BitwiseXor:
                    return (i ^ j);

                case JSToken.BitwiseAnd:
                    return (i & j);

                case JSToken.LeftShift:
                    return (i << j);

                case JSToken.RightShift:
                    return (i >> j);

                case JSToken.UnsignedRightShift:
                    return (uint) (i >> j);
            }
            throw new JScriptException(JSError.InternalError);
        }

        internal override object Evaluate()
        {
            return this.EvaluateBitwiseBinary(base.operand1.Evaluate(), base.operand2.Evaluate());
        }

        [DebuggerHidden, DebuggerStepThrough]
        public object EvaluateBitwiseBinary(object v1, object v2)
        {
            if ((v1 is int) && (v2 is int))
            {
                return DoOp((int) v1, (int) v2, base.operatorTok);
            }
            return this.EvaluateBitwiseBinary(v1, v2, base.operatorTok);
        }

        [DebuggerHidden, DebuggerStepThrough]
        private object EvaluateBitwiseBinary(object v1, object v2, JSToken operatorTok)
        {
            int num;
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v1);
            IConvertible ic = Microsoft.JScript.Convert.GetIConvertible(v2);
            TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(v1, iConvertible);
            TypeCode code2 = Microsoft.JScript.Convert.GetTypeCode(v2, ic);
            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return this.EvaluateBitwiseBinary(0, v2, operatorTok);

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    num = iConvertible.ToInt32(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                        case TypeCode.DBNull:
                            return DoOp(num, 0, operatorTok);

                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return DoOp(num, ic.ToInt32(null), operatorTok);

                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp(num, (int) Runtime.DoubleToInt64(ic.ToDouble(null)), operatorTok);
                    }
                    break;

                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    num = (int) Runtime.DoubleToInt64(iConvertible.ToDouble(null));
                    switch (code2)
                    {
                        case TypeCode.Empty:
                        case TypeCode.DBNull:
                            return DoOp(num, 0, operatorTok);

                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return DoOp(num, ic.ToInt32(null), operatorTok);

                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp(num, (int) Runtime.DoubleToInt64(ic.ToDouble(null)), operatorTok);
                    }
                    break;
            }
            if (v2 == null)
            {
                return DoOp(Microsoft.JScript.Convert.ToInt32(v1), 0, base.operatorTok);
            }
            MethodInfo @operator = base.GetOperator(v1.GetType(), v2.GetType());
            if (@operator != null)
            {
                return @operator.Invoke(null, BindingFlags.Default, JSBinder.ob, new object[] { v1, v2 }, null);
            }
            return DoOp(Microsoft.JScript.Convert.ToInt32(v1), Microsoft.JScript.Convert.ToInt32(v2), base.operatorTok);
        }

        internal override IReflect InferType(JSField inference_target)
        {
            MethodInfo @operator;
            if ((base.type1 == null) || (inference_target != null))
            {
                @operator = base.GetOperator(base.operand1.InferType(inference_target), base.operand2.InferType(inference_target));
            }
            else
            {
                @operator = base.GetOperator(base.type1, base.type2);
            }
            if (@operator != null)
            {
                this.metaData = @operator;
                return @operator.ReturnType;
            }
            return ResultType(base.type1, base.type2, base.operatorTok);
        }

        internal static Type Operand2Type(JSToken operatorTok, Type bbrType)
        {
            switch (operatorTok)
            {
                case JSToken.LeftShift:
                case JSToken.RightShift:
                case JSToken.UnsignedRightShift:
                    return Typeob.Int32;
            }
            return bbrType;
        }

        internal static Type ResultType(Type type1, Type type2, JSToken operatorTok)
        {
            switch (operatorTok)
            {
                case JSToken.LeftShift:
                case JSToken.RightShift:
                    if (!Microsoft.JScript.Convert.IsPrimitiveIntegerType(type1))
                    {
                        if (Typeob.JSObject.IsAssignableFrom(type1))
                        {
                            return Typeob.Int32;
                        }
                        return Typeob.Object;
                    }
                    return type1;

                case JSToken.UnsignedRightShift:
                    switch (Type.GetTypeCode(type1))
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                            return Typeob.Byte;

                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                            return Typeob.UInt16;

                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                            return Typeob.UInt32;

                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            return Typeob.UInt64;
                    }
                    if (Typeob.JSObject.IsAssignableFrom(type1))
                    {
                        return Typeob.Int32;
                    }
                    return Typeob.Object;

                default:
                {
                    TypeCode typeCode = Type.GetTypeCode(type1);
                    TypeCode code2 = Type.GetTypeCode(type2);
                    switch (typeCode)
                    {
                        case TypeCode.Empty:
                        case TypeCode.DBNull:
                        case TypeCode.Boolean:
                            switch (code2)
                            {
                                case TypeCode.Empty:
                                case TypeCode.DBNull:
                                case TypeCode.Boolean:
                                case TypeCode.Int32:
                                case TypeCode.Single:
                                case TypeCode.Double:
                                    return Typeob.Int32;

                                case TypeCode.Object:
                                    if (!Typeob.JSObject.IsAssignableFrom(type2))
                                    {
                                        break;
                                    }
                                    return Typeob.Int32;

                                case TypeCode.Char:
                                case TypeCode.UInt16:
                                    return Typeob.UInt16;

                                case TypeCode.SByte:
                                    return Typeob.SByte;

                                case TypeCode.Byte:
                                    return Typeob.Byte;

                                case TypeCode.Int16:
                                    return Typeob.Int16;

                                case TypeCode.UInt32:
                                    return Typeob.UInt32;

                                case TypeCode.Int64:
                                    return Typeob.Int64;

                                case TypeCode.UInt64:
                                    return Typeob.UInt64;
                            }
                            goto Label_05F4;

                        case TypeCode.Object:
                            if (!Typeob.JSObject.IsAssignableFrom(type1))
                            {
                                break;
                            }
                            return Typeob.Int32;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                            switch (code2)
                            {
                                case TypeCode.Empty:
                                case TypeCode.DBNull:
                                case TypeCode.Boolean:
                                case TypeCode.Char:
                                case TypeCode.SByte:
                                case TypeCode.Byte:
                                case TypeCode.Int16:
                                case TypeCode.UInt16:
                                    return Typeob.UInt16;

                                case TypeCode.Object:
                                    if (!Typeob.JSObject.IsAssignableFrom(type2))
                                    {
                                        break;
                                    }
                                    return Typeob.UInt32;

                                case TypeCode.Int32:
                                case TypeCode.UInt32:
                                case TypeCode.Single:
                                case TypeCode.Double:
                                    return Typeob.UInt32;

                                case TypeCode.Int64:
                                case TypeCode.UInt64:
                                    return Typeob.UInt64;
                            }
                            goto Label_05F4;

                        case TypeCode.SByte:
                            switch (code2)
                            {
                                case TypeCode.Empty:
                                case TypeCode.DBNull:
                                case TypeCode.Boolean:
                                case TypeCode.SByte:
                                    return Typeob.SByte;

                                case TypeCode.Object:
                                    if (!Typeob.JSObject.IsAssignableFrom(type2))
                                    {
                                        break;
                                    }
                                    return Typeob.Int32;

                                case TypeCode.Char:
                                case TypeCode.Int16:
                                    return Typeob.Int16;

                                case TypeCode.Byte:
                                    return Typeob.Byte;

                                case TypeCode.UInt16:
                                    return Typeob.UInt16;

                                case TypeCode.Int32:
                                case TypeCode.Single:
                                case TypeCode.Double:
                                    return Typeob.Int32;

                                case TypeCode.UInt32:
                                    return Typeob.UInt32;

                                case TypeCode.Int64:
                                    return Typeob.Int64;

                                case TypeCode.UInt64:
                                    return Typeob.UInt64;
                            }
                            goto Label_05F4;

                        case TypeCode.Byte:
                            switch (code2)
                            {
                                case TypeCode.Empty:
                                case TypeCode.DBNull:
                                case TypeCode.Boolean:
                                case TypeCode.SByte:
                                case TypeCode.Byte:
                                    return Typeob.Byte;

                                case TypeCode.Object:
                                    if (!Typeob.JSObject.IsAssignableFrom(type2))
                                    {
                                        break;
                                    }
                                    return Typeob.UInt32;

                                case TypeCode.Char:
                                case TypeCode.Int16:
                                case TypeCode.UInt16:
                                    return Typeob.UInt16;

                                case TypeCode.Int32:
                                case TypeCode.UInt32:
                                case TypeCode.Single:
                                case TypeCode.Double:
                                    return Typeob.UInt32;

                                case TypeCode.Int64:
                                case TypeCode.UInt64:
                                    return Typeob.UInt64;
                            }
                            goto Label_05F4;

                        case TypeCode.Int16:
                            switch (code2)
                            {
                                case TypeCode.Empty:
                                case TypeCode.DBNull:
                                case TypeCode.Boolean:
                                case TypeCode.SByte:
                                case TypeCode.Int16:
                                    return Typeob.Int16;

                                case TypeCode.Object:
                                    if (!Typeob.JSObject.IsAssignableFrom(type2))
                                    {
                                        break;
                                    }
                                    return Typeob.Int32;

                                case TypeCode.Char:
                                case TypeCode.Byte:
                                case TypeCode.UInt16:
                                    return Typeob.UInt16;

                                case TypeCode.Int32:
                                case TypeCode.Single:
                                case TypeCode.Double:
                                    return Typeob.Int32;

                                case TypeCode.UInt32:
                                    return Typeob.UInt32;

                                case TypeCode.Int64:
                                    return Typeob.Int64;

                                case TypeCode.UInt64:
                                    return Typeob.UInt64;
                            }
                            goto Label_05F4;

                        case TypeCode.Int32:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            switch (code2)
                            {
                                case TypeCode.Empty:
                                case TypeCode.DBNull:
                                case TypeCode.Boolean:
                                case TypeCode.SByte:
                                case TypeCode.Int16:
                                case TypeCode.Int32:
                                case TypeCode.Single:
                                case TypeCode.Double:
                                    return Typeob.Int32;

                                case TypeCode.Object:
                                    if (!Typeob.JSObject.IsAssignableFrom(type2))
                                    {
                                        break;
                                    }
                                    return Typeob.Int32;

                                case TypeCode.Char:
                                case TypeCode.Byte:
                                case TypeCode.UInt16:
                                case TypeCode.UInt32:
                                    return Typeob.UInt32;

                                case TypeCode.Int64:
                                    return Typeob.Int64;

                                case TypeCode.UInt64:
                                    return Typeob.UInt64;
                            }
                            goto Label_05F4;

                        case TypeCode.UInt32:
                            switch (code2)
                            {
                                case TypeCode.Empty:
                                case TypeCode.DBNull:
                                case TypeCode.Boolean:
                                case TypeCode.Char:
                                case TypeCode.SByte:
                                case TypeCode.Byte:
                                case TypeCode.Int16:
                                case TypeCode.UInt16:
                                case TypeCode.Int32:
                                case TypeCode.UInt32:
                                case TypeCode.Single:
                                case TypeCode.Double:
                                    return Typeob.UInt32;

                                case TypeCode.Object:
                                    if (!Typeob.JSObject.IsAssignableFrom(type2))
                                    {
                                        break;
                                    }
                                    return Typeob.UInt32;

                                case TypeCode.Int64:
                                case TypeCode.UInt64:
                                    return Typeob.UInt64;
                            }
                            goto Label_05F4;

                        case TypeCode.Int64:
                            switch (code2)
                            {
                                case TypeCode.Empty:
                                case TypeCode.DBNull:
                                case TypeCode.Boolean:
                                case TypeCode.SByte:
                                case TypeCode.Int16:
                                case TypeCode.Int32:
                                case TypeCode.Int64:
                                case TypeCode.Single:
                                case TypeCode.Double:
                                    return Typeob.Int64;

                                case TypeCode.Object:
                                    if (!Typeob.JSObject.IsAssignableFrom(type2))
                                    {
                                        break;
                                    }
                                    return Typeob.Int64;

                                case TypeCode.Char:
                                case TypeCode.Byte:
                                case TypeCode.UInt16:
                                case TypeCode.UInt32:
                                case TypeCode.UInt64:
                                    return Typeob.UInt64;
                            }
                            goto Label_05F4;

                        case TypeCode.UInt64:
                            switch (code2)
                            {
                                case TypeCode.Empty:
                                case TypeCode.DBNull:
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
                                    return Typeob.UInt64;

                                case TypeCode.Object:
                                    if (Typeob.JSObject.IsAssignableFrom(type2))
                                    {
                                        return Typeob.UInt64;
                                    }
                                    break;
                            }
                            goto Label_05F4;
                    }
                    break;
                }
            }
        Label_05F4:
            return Typeob.Object;
        }

        internal static void TranslateToBitCountMask(ILGenerator il, Type type, AST operand2)
        {
            int arg = 0;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                    arg = 7;
                    break;

                case TypeCode.Int16:
                case TypeCode.UInt16:
                    arg = 15;
                    break;

                case TypeCode.Int32:
                case TypeCode.UInt32:
                    arg = 0x1f;
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    arg = 0x3f;
                    break;
            }
            ConstantWrapper wrapper = operand2 as ConstantWrapper;
            if ((wrapper == null) || (Microsoft.JScript.Convert.ToInt32(wrapper.value) > arg))
            {
                il.Emit(OpCodes.Ldc_I4_S, arg);
                il.Emit(OpCodes.And);
            }
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (this.metaData != null)
            {
                if (this.metaData is MethodInfo)
                {
                    MethodInfo metaData = (MethodInfo) this.metaData;
                    ParameterInfo[] parameters = metaData.GetParameters();
                    base.operand1.TranslateToIL(il, parameters[0].ParameterType);
                    base.operand2.TranslateToIL(il, parameters[1].ParameterType);
                    il.Emit(OpCodes.Call, metaData);
                    Microsoft.JScript.Convert.Emit(this, il, metaData.ReturnType, rtype);
                }
                else
                {
                    il.Emit(OpCodes.Ldloc, (LocalBuilder) this.metaData);
                    base.operand1.TranslateToIL(il, Typeob.Object);
                    base.operand2.TranslateToIL(il, Typeob.Object);
                    il.Emit(OpCodes.Call, CompilerGlobals.evaluateBitwiseBinaryMethod);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                }
            }
            else
            {
                Type type = ResultType(base.type1, base.type2, base.operatorTok);
                if (Microsoft.JScript.Convert.IsPrimitiveNumericType(base.type1))
                {
                    base.operand1.TranslateToIL(il, base.type1);
                    Microsoft.JScript.Convert.Emit(this, il, base.type1, type, true);
                }
                else
                {
                    base.operand1.TranslateToIL(il, Typeob.Double);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Double, type, true);
                }
                Type type2 = Operand2Type(base.operatorTok, type);
                if (Microsoft.JScript.Convert.IsPrimitiveNumericType(base.type2))
                {
                    base.operand2.TranslateToIL(il, base.type2);
                    Microsoft.JScript.Convert.Emit(this, il, base.type2, type2, true);
                }
                else
                {
                    base.operand2.TranslateToIL(il, Typeob.Double);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Double, type2, true);
                }
                switch (base.operatorTok)
                {
                    case JSToken.BitwiseOr:
                        il.Emit(OpCodes.Or);
                        break;

                    case JSToken.BitwiseXor:
                        il.Emit(OpCodes.Xor);
                        break;

                    case JSToken.BitwiseAnd:
                        il.Emit(OpCodes.And);
                        break;

                    case JSToken.LeftShift:
                        TranslateToBitCountMask(il, type, base.operand2);
                        il.Emit(OpCodes.Shl);
                        break;

                    case JSToken.RightShift:
                        TranslateToBitCountMask(il, type, base.operand2);
                        il.Emit(OpCodes.Shr);
                        break;

                    case JSToken.UnsignedRightShift:
                        TranslateToBitCountMask(il, type, base.operand2);
                        il.Emit(OpCodes.Shr_Un);
                        break;

                    default:
                        throw new JScriptException(JSError.InternalError, base.context);
                }
                Microsoft.JScript.Convert.Emit(this, il, type, rtype);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            IReflect reflect = this.InferType(null);
            base.operand1.TranslateToILInitializer(il);
            base.operand2.TranslateToILInitializer(il);
            if (reflect == Typeob.Object)
            {
                this.metaData = il.DeclareLocal(Typeob.BitwiseBinary);
                ConstantWrapper.TranslateToILInt(il, (int) base.operatorTok);
                il.Emit(OpCodes.Newobj, CompilerGlobals.bitwiseBinaryConstructor);
                il.Emit(OpCodes.Stloc, (LocalBuilder) this.metaData);
            }
        }
    }
}

