namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    public class Relational : BinaryOp
    {
        private object metaData;

        public Relational(int operatorTok) : base(null, null, null, (JSToken) operatorTok)
        {
        }

        internal Relational(Context context, AST operand1, AST operand2, JSToken operatorTok) : base(context, operand1, operand2, operatorTok)
        {
        }

        internal override object Evaluate()
        {
            object obj2 = base.operand1.Evaluate();
            object obj3 = base.operand2.Evaluate();
            double num = this.EvaluateRelational(obj2, obj3);
            switch (base.operatorTok)
            {
                case JSToken.GreaterThan:
                    return (num > 0.0);

                case JSToken.LessThan:
                    return (num < 0.0);

                case JSToken.LessThanEqual:
                    return (num <= 0.0);

                case JSToken.GreaterThanEqual:
                    return (num >= 0.0);
            }
            throw new JScriptException(JSError.InternalError, base.context);
        }

        [DebuggerHidden, DebuggerStepThrough]
        public double EvaluateRelational(object v1, object v2)
        {
            if (v1 is int)
            {
                if (v2 is int)
                {
                    return (((int) v1) - ((int) v2));
                }
                if (v2 is double)
                {
                    return (((int) v1) - ((double) v2));
                }
            }
            else if (v1 is double)
            {
                if (v2 is double)
                {
                    double num = (double) v1;
                    double num2 = (double) v2;
                    if (num == num2)
                    {
                        return 0.0;
                    }
                    return (num - num2);
                }
                if (v2 is int)
                {
                    return (((double) v1) - ((int) v2));
                }
            }
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v1);
            IConvertible ic = Microsoft.JScript.Convert.GetIConvertible(v2);
            TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(v1, iConvertible);
            TypeCode code2 = Microsoft.JScript.Convert.GetTypeCode(v2, ic);
            if ((typeCode == TypeCode.Object) && (code2 == TypeCode.Object))
            {
                MethodInfo @operator = base.GetOperator(v1.GetType(), v2.GetType());
                if (@operator != null)
                {
                    bool flag = Microsoft.JScript.Convert.ToBoolean(@operator.Invoke(null, BindingFlags.Default, JSBinder.ob, new object[] { v1, v2 }, null));
                    switch (base.operatorTok)
                    {
                        case JSToken.GreaterThan:
                        case JSToken.GreaterThanEqual:
                            return (flag ? ((double) 1) : ((double) (-1)));

                        case JSToken.LessThan:
                        case JSToken.LessThanEqual:
                            return (flag ? ((double) (-1)) : ((double) 1));
                    }
                    throw new JScriptException(JSError.InternalError, base.context);
                }
            }
            return JScriptCompare2(v1, v2, iConvertible, ic, typeCode, code2);
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return Typeob.Boolean;
        }

        public static double JScriptCompare(object v1, object v2)
        {
            if (v1 is int)
            {
                if (v2 is int)
                {
                    return (double) (((int) v1) - ((int) v2));
                }
                if (v2 is double)
                {
                    return (((int) v1) - ((double) v2));
                }
            }
            else if (v1 is double)
            {
                if (v2 is double)
                {
                    double num = (double) v1;
                    double num2 = (double) v2;
                    if (num == num2)
                    {
                        return 0.0;
                    }
                    return (num - num2);
                }
                if (v2 is int)
                {
                    return (((double) v1) - ((int) v2));
                }
            }
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v1);
            IConvertible ic = Microsoft.JScript.Convert.GetIConvertible(v2);
            TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(v1, iConvertible);
            TypeCode code2 = Microsoft.JScript.Convert.GetTypeCode(v2, ic);
            return JScriptCompare2(v1, v2, iConvertible, ic, typeCode, code2);
        }

        private static double JScriptCompare2(object v1, object v2, IConvertible ic1, IConvertible ic2, TypeCode t1, TypeCode t2)
        {
            double num7;
            if (t1 == TypeCode.Object)
            {
                v1 = Microsoft.JScript.Convert.ToPrimitive(v1, PreferredType.Number, ref ic1);
                t1 = Microsoft.JScript.Convert.GetTypeCode(v1, ic1);
            }
            if (t2 == TypeCode.Object)
            {
                v2 = Microsoft.JScript.Convert.ToPrimitive(v2, PreferredType.Number, ref ic2);
                t2 = Microsoft.JScript.Convert.GetTypeCode(v2, ic2);
            }
            switch (t1)
            {
                case TypeCode.Char:
                    if (t2 != TypeCode.String)
                    {
                        break;
                    }
                    return (double) string.CompareOrdinal(Microsoft.JScript.Convert.ToString(v1, ic1), ic2.ToString(null));

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                    break;

                case TypeCode.UInt64:
                {
                    ulong num3 = ic1.ToUInt64(null);
                    switch (t2)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        {
                            long num4 = ic2.ToInt64(null);
                            if (num4 < 0L)
                            {
                                return 1.0;
                            }
                            if (num3 == num4)
                            {
                                return 0.0;
                            }
                            return -1.0;
                        }
                        case TypeCode.UInt64:
                        {
                            ulong num5 = ic2.ToUInt64(null);
                            if (num3 < num5)
                            {
                                return -1.0;
                            }
                            if (num3 == num5)
                            {
                                return 0.0;
                            }
                            return 1.0;
                        }
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return (num3 - ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (double) (new decimal(num3) - ic2.ToDecimal(null));
                    }
                    object obj3 = Microsoft.JScript.Convert.ToNumber(v2, ic2);
                    return JScriptCompare2(v1, obj3, ic1, Microsoft.JScript.Convert.GetIConvertible(obj3), t1, TypeCode.Double);
                }
                case TypeCode.Decimal:
                {
                    decimal num6 = ic1.ToDecimal(null);
                    switch (t2)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (double) (num6 - new decimal(ic2.ToInt64(null)));

                        case TypeCode.UInt64:
                            return (double) (num6 - new decimal(ic2.ToUInt64(null)));

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return (double) (num6 - new decimal(ic2.ToDouble(null)));

                        case TypeCode.Decimal:
                            return (double) (num6 - ic2.ToDecimal(null));
                    }
                    return (double) (num6 - new decimal(Microsoft.JScript.Convert.ToNumber(v2, ic2)));
                }
                case TypeCode.String:
                {
                    TypeCode code5 = t2;
                    if (code5 == TypeCode.Char)
                    {
                        return (double) string.CompareOrdinal(ic1.ToString(null), Microsoft.JScript.Convert.ToString(v2, ic2));
                    }
                    if (code5 != TypeCode.String)
                    {
                        goto Label_0355;
                    }
                    return (double) string.CompareOrdinal(ic1.ToString(null), ic2.ToString(null));
                }
                default:
                    goto Label_0355;
            }
            long num = ic1.ToInt64(null);
            switch (t2)
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                    return (double) (num - ic2.ToInt64(null));

                case TypeCode.UInt64:
                    if (num >= 0L)
                    {
                        ulong num2 = ic2.ToUInt64(null);
                        if (num < num2)
                        {
                            return -1.0;
                        }
                        if (num == num2)
                        {
                            return 0.0;
                        }
                        return 1.0;
                    }
                    return -1.0;

                case TypeCode.Single:
                case TypeCode.Double:
                    return (num - ic2.ToDouble(null));

                case TypeCode.Decimal:
                    return (double) (new decimal(num) - ic2.ToDecimal(null));

                default:
                {
                    object obj2 = Microsoft.JScript.Convert.ToNumber(v2, ic2);
                    return JScriptCompare2(v1, obj2, ic1, Microsoft.JScript.Convert.GetIConvertible(obj2), t1, TypeCode.Double);
                }
            }
        Label_0355:
            num7 = Microsoft.JScript.Convert.ToNumber(v1, ic1);
            double num8 = Microsoft.JScript.Convert.ToNumber(v2, ic2);
            if (num7 == num8)
            {
                return 0.0;
            }
            return (num7 - num8);
        }

        internal override void TranslateToConditionalBranch(ILGenerator il, bool branchIfTrue, Label label, bool shortForm)
        {
            Type type = base.type1;
            Type type2 = base.type2;
            Type rtype = Typeob.Object;
            if (type.IsPrimitive && type2.IsPrimitive)
            {
                rtype = Typeob.Double;
                if (Microsoft.JScript.Convert.IsPromotableTo((IReflect) type, (IReflect) type2))
                {
                    rtype = type2;
                }
                else if (Microsoft.JScript.Convert.IsPromotableTo((IReflect) type2, (IReflect) type))
                {
                    rtype = type;
                }
                else if (((type == Typeob.Int64) || (type == Typeob.UInt64)) || ((type2 == Typeob.Int64) || (type2 == Typeob.UInt64)))
                {
                    rtype = Typeob.Object;
                }
            }
            if ((rtype == Typeob.SByte) || (rtype == Typeob.Int16))
            {
                rtype = Typeob.Int32;
            }
            else if ((rtype == Typeob.Byte) || (rtype == Typeob.UInt16))
            {
                rtype = Typeob.UInt32;
            }
            if (this.metaData == null)
            {
                base.operand1.TranslateToIL(il, rtype);
                base.operand2.TranslateToIL(il, rtype);
                if (rtype == Typeob.Object)
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.jScriptCompareMethod);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Conv_R8);
                    rtype = Typeob.Double;
                }
            }
            else
            {
                if (this.metaData is MethodInfo)
                {
                    MethodInfo metaData = (MethodInfo) this.metaData;
                    ParameterInfo[] parameters = metaData.GetParameters();
                    base.operand1.TranslateToIL(il, parameters[0].ParameterType);
                    base.operand2.TranslateToIL(il, parameters[1].ParameterType);
                    il.Emit(OpCodes.Call, metaData);
                    if (branchIfTrue)
                    {
                        il.Emit(shortForm ? OpCodes.Brtrue_S : OpCodes.Brtrue, label);
                        return;
                    }
                    il.Emit(shortForm ? OpCodes.Brfalse_S : OpCodes.Brfalse, label);
                    return;
                }
                il.Emit(OpCodes.Ldloc, (LocalBuilder) this.metaData);
                base.operand1.TranslateToIL(il, Typeob.Object);
                base.operand2.TranslateToIL(il, Typeob.Object);
                il.Emit(OpCodes.Call, CompilerGlobals.evaluateRelationalMethod);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Conv_R8);
                rtype = Typeob.Double;
            }
            if (branchIfTrue)
            {
                if ((rtype == Typeob.UInt32) || (rtype == Typeob.UInt64))
                {
                    switch (base.operatorTok)
                    {
                        case JSToken.GreaterThan:
                            il.Emit(shortForm ? OpCodes.Bgt_Un_S : OpCodes.Bgt_Un, label);
                            return;

                        case JSToken.LessThan:
                            il.Emit(shortForm ? OpCodes.Blt_Un_S : OpCodes.Blt_Un, label);
                            return;

                        case JSToken.LessThanEqual:
                            il.Emit(shortForm ? OpCodes.Ble_Un_S : OpCodes.Ble_Un, label);
                            return;

                        case JSToken.GreaterThanEqual:
                            il.Emit(shortForm ? OpCodes.Bge_Un_S : OpCodes.Bge_Un, label);
                            return;
                    }
                    throw new JScriptException(JSError.InternalError, base.context);
                }
                switch (base.operatorTok)
                {
                    case JSToken.GreaterThan:
                        il.Emit(shortForm ? OpCodes.Bgt_S : OpCodes.Bgt, label);
                        return;

                    case JSToken.LessThan:
                        il.Emit(shortForm ? OpCodes.Blt_S : OpCodes.Blt, label);
                        return;

                    case JSToken.LessThanEqual:
                        il.Emit(shortForm ? OpCodes.Ble_S : OpCodes.Ble, label);
                        return;

                    case JSToken.GreaterThanEqual:
                        il.Emit(shortForm ? OpCodes.Bge_S : OpCodes.Bge, label);
                        return;
                }
                throw new JScriptException(JSError.InternalError, base.context);
            }
            if ((rtype == Typeob.Int32) || (rtype == Typeob.Int64))
            {
                switch (base.operatorTok)
                {
                    case JSToken.GreaterThan:
                        il.Emit(shortForm ? OpCodes.Ble_S : OpCodes.Ble, label);
                        return;

                    case JSToken.LessThan:
                        il.Emit(shortForm ? OpCodes.Bge_S : OpCodes.Bge, label);
                        return;

                    case JSToken.LessThanEqual:
                        il.Emit(shortForm ? OpCodes.Bgt_S : OpCodes.Bgt, label);
                        return;

                    case JSToken.GreaterThanEqual:
                        il.Emit(shortForm ? OpCodes.Blt_S : OpCodes.Blt, label);
                        return;
                }
                throw new JScriptException(JSError.InternalError, base.context);
            }
            switch (base.operatorTok)
            {
                case JSToken.GreaterThan:
                    il.Emit(shortForm ? OpCodes.Ble_Un_S : OpCodes.Ble_Un, label);
                    return;

                case JSToken.LessThan:
                    il.Emit(shortForm ? OpCodes.Bge_Un_S : OpCodes.Bge_Un, label);
                    return;

                case JSToken.LessThanEqual:
                    il.Emit(shortForm ? OpCodes.Bgt_Un_S : OpCodes.Bgt_Un, label);
                    return;

                case JSToken.GreaterThanEqual:
                    il.Emit(shortForm ? OpCodes.Blt_Un_S : OpCodes.Blt_Un, label);
                    return;
            }
            throw new JScriptException(JSError.InternalError, base.context);
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Label label = il.DefineLabel();
            Label label2 = il.DefineLabel();
            this.TranslateToConditionalBranch(il, true, label, true);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Br_S, label2);
            il.MarkLabel(label);
            il.Emit(OpCodes.Ldc_I4_1);
            il.MarkLabel(label2);
            Microsoft.JScript.Convert.Emit(this, il, Typeob.Boolean, rtype);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            base.operand1.TranslateToILInitializer(il);
            base.operand2.TranslateToILInitializer(il);
            MethodInfo @operator = base.GetOperator(base.operand1.InferType(null), base.operand2.InferType(null));
            if (@operator != null)
            {
                this.metaData = @operator;
            }
            else if ((!base.type1.IsPrimitive && !Typeob.JSObject.IsAssignableFrom(base.type1)) || (!base.type2.IsPrimitive && !Typeob.JSObject.IsAssignableFrom(base.type2)))
            {
                this.metaData = il.DeclareLocal(Typeob.Relational);
                ConstantWrapper.TranslateToILInt(il, (int) base.operatorTok);
                il.Emit(OpCodes.Newobj, CompilerGlobals.relationalConstructor);
                il.Emit(OpCodes.Stloc, (LocalBuilder) this.metaData);
            }
        }
    }
}

