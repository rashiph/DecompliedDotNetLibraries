namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class NumericBinary : BinaryOp
    {
        private object metaData;

        public NumericBinary(int operatorTok) : base(null, null, null, (JSToken) operatorTok)
        {
        }

        internal NumericBinary(Context context, AST operand1, AST operand2, JSToken operatorTok) : base(context, operand1, operand2, operatorTok)
        {
        }

        private static object DoOp(double x, double y, JSToken operatorTok)
        {
            switch (operatorTok)
            {
                case JSToken.Multiply:
                    return (x * y);

                case JSToken.Divide:
                    return (x / y);

                case JSToken.Modulo:
                    return (x % y);

                case JSToken.Minus:
                    return (x - y);
            }
            throw new JScriptException(JSError.InternalError);
        }

        private static object DoOp(int x, int y, JSToken operatorTok)
        {
            switch (operatorTok)
            {
                case JSToken.Multiply:
                    if ((x != 0) && (y != 0))
                    {
                        try
                        {
                            return (x * y);
                        }
                        catch (OverflowException)
                        {
                            return (x * y);
                        }
                    }
                    return (x * y);

                case JSToken.Divide:
                    return (((double) x) / ((double) y));

                case JSToken.Modulo:
                    if ((x > 0) && (y > 0))
                    {
                        return (x % y);
                    }
                    return (((double) x) % ((double) y));

                case JSToken.Minus:
                {
                    int num = x - y;
                    if ((num < x) == (y > 0))
                    {
                        return num;
                    }
                    return (x - y);
                }
            }
            throw new JScriptException(JSError.InternalError);
        }

        private static object DoOp(long x, long y, JSToken operatorTok)
        {
            switch (operatorTok)
            {
                case JSToken.Multiply:
                    if ((x != 0L) && (y != 0L))
                    {
                        try
                        {
                            return (x * y);
                        }
                        catch (OverflowException)
                        {
                            return (x * y);
                        }
                    }
                    return (x * y);

                case JSToken.Divide:
                    return (((double) x) / ((double) y));

                case JSToken.Modulo:
                    if (y != 0L)
                    {
                        long num2 = x % y;
                        if (num2 != 0L)
                        {
                            return num2;
                        }
                        if (x < 0L)
                        {
                            if (y < 0L)
                            {
                                return 0;
                            }
                            return 0.0;
                        }
                        if (y < 0L)
                        {
                            return 0.0;
                        }
                        return 0;
                    }
                    return (double) 1.0 / (double) 0.0;

                case JSToken.Minus:
                {
                    long num = x - y;
                    if ((num < x) == (y > 0L))
                    {
                        return num;
                    }
                    return (x - y);
                }
            }
            throw new JScriptException(JSError.InternalError);
        }

        public static object DoOp(object v1, object v2, JSToken operatorTok)
        {
            return DoOp(v1, v2, Microsoft.JScript.Convert.GetIConvertible(v1), Microsoft.JScript.Convert.GetIConvertible(v2), operatorTok);
        }

        private static object DoOp(uint x, uint y, JSToken operatorTok)
        {
            switch (operatorTok)
            {
                case JSToken.Multiply:
                    try
                    {
                        return (x * y);
                    }
                    catch (OverflowException)
                    {
                        return (x * y);
                    }
                    break;

                case JSToken.Divide:
                    return (((double) x) / ((double) y));

                case JSToken.Modulo:
                    if (y != 0)
                    {
                        return (x % y);
                    }
                    return (double) 1.0 / (double) 0.0;

                case JSToken.Minus:
                {
                    uint num = x - y;
                    if (num <= x)
                    {
                        return num;
                    }
                    return (x - y);
                }
            }
            throw new JScriptException(JSError.InternalError);
        }

        private static object DoOp(ulong x, ulong y, JSToken operatorTok)
        {
            switch (operatorTok)
            {
                case JSToken.Multiply:
                    try
                    {
                        return (x * y);
                    }
                    catch (OverflowException)
                    {
                        return (x * y);
                    }
                    break;

                case JSToken.Divide:
                    return (((double) x) / ((double) y));

                case JSToken.Modulo:
                    if (y != 0L)
                    {
                        return (x % y);
                    }
                    return (double) 1.0 / (double) 0.0;

                case JSToken.Minus:
                {
                    ulong num = x - y;
                    if (num <= x)
                    {
                        return num;
                    }
                    return (x - y);
                }
            }
            throw new JScriptException(JSError.InternalError);
        }

        private static object DoOp(object v1, object v2, IConvertible ic1, IConvertible ic2, JSToken operatorTok)
        {
            if (operatorTok == JSToken.Minus)
            {
                IConvertible ic = ic1;
                object ob = Microsoft.JScript.Convert.ToPrimitive(v1, PreferredType.Either, ref ic);
                if (Microsoft.JScript.Convert.GetTypeCode(ob, ic) == TypeCode.Char)
                {
                    IConvertible iConvertible = ic2;
                    object obj3 = Microsoft.JScript.Convert.ToPrimitive(v2, PreferredType.Either, ref iConvertible);
                    TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(obj3, iConvertible);
                    if (typeCode == TypeCode.String)
                    {
                        string str = iConvertible.ToString(null);
                        if (str.Length == 1)
                        {
                            typeCode = TypeCode.Char;
                            obj3 = str[0];
                            iConvertible = Microsoft.JScript.Convert.GetIConvertible(obj3);
                        }
                    }
                    object obj4 = DoOp(Microsoft.JScript.Convert.ToNumber(ob, ic), Microsoft.JScript.Convert.ToNumber(obj3, iConvertible), operatorTok);
                    if (typeCode != TypeCode.Char)
                    {
                        obj4 = Microsoft.JScript.Convert.Coerce2(obj4, TypeCode.Char, false);
                    }
                    return obj4;
                }
            }
            return DoOp(Microsoft.JScript.Convert.ToNumber(v1, ic1), Microsoft.JScript.Convert.ToNumber(v2, ic2), operatorTok);
        }

        internal override object Evaluate()
        {
            return this.EvaluateNumericBinary(base.operand1.Evaluate(), base.operand2.Evaluate());
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object EvaluateNumericBinary(object v1, object v2)
        {
            if ((v1 is int) && (v2 is int))
            {
                return DoOp((int) v1, (int) v2, base.operatorTok);
            }
            if ((v1 is double) && (v2 is double))
            {
                return DoOp((double) v1, (double) v2, base.operatorTok);
            }
            return this.EvaluateNumericBinary(v1, v2, base.operatorTok);
        }

        [DebuggerHidden, DebuggerStepThrough]
        private object EvaluateNumericBinary(object v1, object v2, JSToken operatorTok)
        {
            object obj2;
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v1);
            IConvertible ic = Microsoft.JScript.Convert.GetIConvertible(v2);
            TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(v1, iConvertible);
            TypeCode code2 = Microsoft.JScript.Convert.GetTypeCode(v2, ic);
            switch (typeCode)
            {
                case TypeCode.Empty:
                    return (double) 1.0 / (double) 0.0;

                case TypeCode.DBNull:
                    return this.EvaluateNumericBinary(0, v2, operatorTok);

                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                {
                    int x = iConvertible.ToInt32(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.DBNull:
                            return DoOp(x, 0, operatorTok);

                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return DoOp(x, ic.ToInt32(null), operatorTok);

                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return DoOp((long) x, ic.ToInt64(null), operatorTok);

                        case TypeCode.UInt64:
                            if (x >= 0)
                            {
                                return DoOp((ulong) x, ic.ToUInt64(null), operatorTok);
                            }
                            return DoOp((double) x, ic.ToDouble(null), operatorTok);

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp((double) x, ic.ToDouble(null), operatorTok);
                    }
                    goto Label_0593;
                }
                case TypeCode.Char:
                {
                    int num = iConvertible.ToInt32(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.DBNull:
                            return DoOp(num, 0, operatorTok);

                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            obj2 = DoOp(num, ic.ToInt32(null), operatorTok);
                            goto Label_017F;

                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            obj2 = DoOp((long) num, ic.ToInt64(null), operatorTok);
                            goto Label_017F;

                        case TypeCode.UInt64:
                            obj2 = DoOp((double) num, ic.ToDouble(null), operatorTok);
                            goto Label_017F;

                        case TypeCode.Single:
                        case TypeCode.Double:
                            obj2 = DoOp((double) iConvertible.ToInt32(null), ic.ToDouble(null), operatorTok);
                            goto Label_017F;

                        case TypeCode.String:
                            obj2 = DoOp((double) num, Microsoft.JScript.Convert.ToNumber(v2, ic), operatorTok);
                            goto Label_017F;
                    }
                    break;
                }
                case TypeCode.UInt32:
                {
                    uint num3 = iConvertible.ToUInt32(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.DBNull:
                            return DoOp(num3, 0, operatorTok);

                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                            return DoOp(num3, ic.ToUInt32(null), operatorTok);

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        {
                            int num4 = ic.ToInt32(null);
                            if (num4 >= 0)
                            {
                                return DoOp(num3, (uint) num4, operatorTok);
                            }
                            return DoOp((long) num3, (long) num4, operatorTok);
                        }
                        case TypeCode.Int64:
                            return DoOp((long) num3, ic.ToInt64(null), operatorTok);

                        case TypeCode.UInt64:
                            return DoOp((ulong) num3, ic.ToUInt64(null), operatorTok);

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp((double) num3, ic.ToDouble(null), operatorTok);
                    }
                    goto Label_0593;
                }
                case TypeCode.Int64:
                {
                    long num5 = iConvertible.ToInt64(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.DBNull:
                            return DoOp(num5, 0L, operatorTok);

                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return DoOp(num5, ic.ToInt64(null), operatorTok);

                        case TypeCode.UInt64:
                            if (num5 >= 0L)
                            {
                                return DoOp((ulong) num5, ic.ToUInt64(null), operatorTok);
                            }
                            return DoOp((double) num5, ic.ToDouble(null), operatorTok);

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp((double) num5, ic.ToDouble(null), operatorTok);
                    }
                    goto Label_0593;
                }
                case TypeCode.UInt64:
                {
                    ulong num6 = iConvertible.ToUInt64(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.DBNull:
                            return DoOp(num6, 0L, operatorTok);

                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return DoOp(num6, ic.ToUInt64(null), operatorTok);

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        {
                            long num7 = ic.ToInt64(null);
                            if (num7 >= 0L)
                            {
                                return DoOp(num6, (ulong) num7, operatorTok);
                            }
                            return DoOp((double) num6, (double) num7, operatorTok);
                        }
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp((double) num6, ic.ToDouble(null), operatorTok);
                    }
                    goto Label_0593;
                }
                case TypeCode.Single:
                case TypeCode.Double:
                {
                    double num8 = iConvertible.ToDouble(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.DBNull:
                            return DoOp(num8, 0.0, operatorTok);

                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return DoOp(num8, (double) ic.ToInt32(null), operatorTok);

                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp(num8, ic.ToDouble(null), operatorTok);
                    }
                    goto Label_0593;
                }
                default:
                    goto Label_0593;
            }
            obj2 = null;
        Label_017F:
            if (((base.operatorTok == JSToken.Minus) && (obj2 != null)) && (code2 != TypeCode.Char))
            {
                return Microsoft.JScript.Convert.Coerce2(obj2, TypeCode.Char, false);
            }
            if (obj2 != null)
            {
                return obj2;
            }
        Label_0593:
            if (v2 == null)
            {
                return (double) 1.0 / (double) 0.0;
            }
            MethodInfo @operator = base.GetOperator(v1.GetType(), v2.GetType());
            if (@operator != null)
            {
                return @operator.Invoke(null, BindingFlags.Default, JSBinder.ob, new object[] { v1, v2 }, null);
            }
            return DoOp(v1, v2, iConvertible, ic, operatorTok);
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
            if ((base.type1 == Typeob.Char) && (base.operatorTok == JSToken.Minus))
            {
                TypeCode typeCode = Type.GetTypeCode(base.type2);
                if (Microsoft.JScript.Convert.IsPrimitiveNumericTypeCode(typeCode) || (typeCode == TypeCode.Boolean))
                {
                    return Typeob.Char;
                }
                if (typeCode == TypeCode.Char)
                {
                    return Typeob.Int32;
                }
            }
            if ((Microsoft.JScript.Convert.IsPrimitiveNumericTypeFitForDouble(base.type1) || Typeob.JSObject.IsAssignableFrom(base.type1)) && (Microsoft.JScript.Convert.IsPrimitiveNumericTypeFitForDouble(base.type2) || Typeob.JSObject.IsAssignableFrom(base.type2)))
            {
                return Typeob.Double;
            }
            return Typeob.Object;
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
                    il.Emit(OpCodes.Call, CompilerGlobals.evaluateNumericBinaryMethod);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                }
                return;
            }
            Type type = Typeob.Double;
            if ((Microsoft.JScript.Convert.IsPrimitiveNumericType(rtype) && Microsoft.JScript.Convert.IsPromotableTo((IReflect) base.type1, (IReflect) rtype)) && Microsoft.JScript.Convert.IsPromotableTo((IReflect) base.type2, (IReflect) rtype))
            {
                type = rtype;
            }
            if (base.operatorTok == JSToken.Divide)
            {
                type = Typeob.Double;
            }
            else if ((type == Typeob.SByte) || (type == Typeob.Int16))
            {
                type = Typeob.Int32;
            }
            else if (((type == Typeob.Byte) || (type == Typeob.UInt16)) || (type == Typeob.Char))
            {
                type = Typeob.UInt32;
            }
            base.operand1.TranslateToIL(il, type);
            base.operand2.TranslateToIL(il, type);
            if ((type == Typeob.Double) || (type == Typeob.Single))
            {
                switch (base.operatorTok)
                {
                    case JSToken.Multiply:
                        il.Emit(OpCodes.Mul);
                        goto Label_0230;

                    case JSToken.Divide:
                        il.Emit(OpCodes.Div);
                        goto Label_0230;

                    case JSToken.Modulo:
                        il.Emit(OpCodes.Rem);
                        goto Label_0230;

                    case JSToken.Minus:
                        il.Emit(OpCodes.Sub);
                        goto Label_0230;
                }
                throw new JScriptException(JSError.InternalError, base.context);
            }
            if ((type == Typeob.Int32) || (type == Typeob.Int64))
            {
                switch (base.operatorTok)
                {
                    case JSToken.Multiply:
                        il.Emit(OpCodes.Mul_Ovf);
                        goto Label_0230;

                    case JSToken.Divide:
                        il.Emit(OpCodes.Div);
                        goto Label_0230;

                    case JSToken.Modulo:
                        il.Emit(OpCodes.Rem);
                        goto Label_0230;

                    case JSToken.Minus:
                        il.Emit(OpCodes.Sub_Ovf);
                        goto Label_0230;
                }
                throw new JScriptException(JSError.InternalError, base.context);
            }
            switch (base.operatorTok)
            {
                case JSToken.Multiply:
                    il.Emit(OpCodes.Mul_Ovf_Un);
                    break;

                case JSToken.Divide:
                    il.Emit(OpCodes.Div);
                    break;

                case JSToken.Modulo:
                    il.Emit(OpCodes.Rem);
                    break;

                case JSToken.Minus:
                    il.Emit(OpCodes.Sub_Ovf_Un);
                    break;

                default:
                    throw new JScriptException(JSError.InternalError, base.context);
            }
        Label_0230:
            if (Microsoft.JScript.Convert.ToType(this.InferType(null)) == Typeob.Char)
            {
                Microsoft.JScript.Convert.Emit(this, il, type, Typeob.Char);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Char, rtype);
            }
            else
            {
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
                this.metaData = il.DeclareLocal(Typeob.NumericBinary);
                ConstantWrapper.TranslateToILInt(il, (int) base.operatorTok);
                il.Emit(OpCodes.Newobj, CompilerGlobals.numericBinaryConstructor);
                il.Emit(OpCodes.Stloc, (LocalBuilder) this.metaData);
            }
        }
    }
}

