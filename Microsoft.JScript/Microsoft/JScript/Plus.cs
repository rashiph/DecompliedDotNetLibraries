namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Plus : BinaryOp
    {
        private object metaData;

        public Plus() : base(null, null, null, JSToken.FirstBinaryOp)
        {
        }

        internal Plus(Context context, AST operand1, AST operand2) : base(context, operand1, operand2, JSToken.FirstBinaryOp)
        {
        }

        private static object DoOp(double x, double y)
        {
            return (x + y);
        }

        private static object DoOp(int x, int y)
        {
            int num = x + y;
            if ((num < x) == (y < 0))
            {
                return num;
            }
            return (x + y);
        }

        private static object DoOp(long x, long y)
        {
            long num = x + y;
            if ((num < x) == (y < 0L))
            {
                return num;
            }
            return (x + y);
        }

        public static object DoOp(object v1, object v2)
        {
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v1);
            IConvertible ic = Microsoft.JScript.Convert.GetIConvertible(v2);
            v1 = Microsoft.JScript.Convert.ToPrimitive(v1, PreferredType.Either, ref iConvertible);
            v2 = Microsoft.JScript.Convert.ToPrimitive(v2, PreferredType.Either, ref ic);
            TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(v1, iConvertible);
            TypeCode tc = Microsoft.JScript.Convert.GetTypeCode(v2, ic);
            if (typeCode == TypeCode.String)
            {
                if (v1 is ConcatString)
                {
                    return new ConcatString((ConcatString) v1, Microsoft.JScript.Convert.ToString(v2, ic));
                }
                return new ConcatString(iConvertible.ToString(null), Microsoft.JScript.Convert.ToString(v2, ic));
            }
            if (tc == TypeCode.String)
            {
                return (Microsoft.JScript.Convert.ToString(v1, iConvertible) + ic.ToString(null));
            }
            if ((typeCode == TypeCode.Char) && (tc == TypeCode.Char))
            {
                return (iConvertible.ToString(null) + ic.ToString(null));
            }
            if (((typeCode != TypeCode.Char) || (!Microsoft.JScript.Convert.IsPrimitiveNumericTypeCode(tc) && (tc != TypeCode.Boolean))) && ((tc != TypeCode.Char) || (!Microsoft.JScript.Convert.IsPrimitiveNumericTypeCode(typeCode) && (typeCode != TypeCode.Boolean))))
            {
                return (Microsoft.JScript.Convert.ToNumber(v1, iConvertible) + Microsoft.JScript.Convert.ToNumber(v2, ic));
            }
            return (char) ((int) Runtime.DoubleToInt64(Microsoft.JScript.Convert.ToNumber(v1, iConvertible) + Microsoft.JScript.Convert.ToNumber(v2, ic)));
        }

        private static object DoOp(uint x, uint y)
        {
            uint num = x + y;
            if (num >= x)
            {
                return num;
            }
            return (x + y);
        }

        private static object DoOp(ulong x, ulong y)
        {
            ulong num = x + y;
            if (num >= x)
            {
                return num;
            }
            return (x + y);
        }

        internal override object Evaluate()
        {
            return this.EvaluatePlus(base.operand1.Evaluate(), base.operand2.Evaluate());
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object EvaluatePlus(object v1, object v2)
        {
            if ((v1 is int) && (v2 is int))
            {
                return DoOp((int) v1, (int) v2);
            }
            if ((v1 is double) && (v2 is double))
            {
                return DoOp((double) v1, (double) v2);
            }
            return this.EvaluatePlus2(v1, v2);
        }

        [DebuggerStepThrough, DebuggerHidden]
        private object EvaluatePlus2(object v1, object v2)
        {
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v1);
            IConvertible ic = Microsoft.JScript.Convert.GetIConvertible(v2);
            TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(v1, iConvertible);
            TypeCode code2 = Microsoft.JScript.Convert.GetTypeCode(v2, ic);
            switch (typeCode)
            {
                case TypeCode.Empty:
                    return DoOp(v1, v2);

                case TypeCode.DBNull:
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.DBNull:
                            return 0;

                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return ic.ToInt32(null);

                        case TypeCode.UInt32:
                            return ic.ToUInt32(null);

                        case TypeCode.Int64:
                            return ic.ToInt64(null);

                        case TypeCode.UInt64:
                            return ic.ToUInt64(null);

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return ic.ToDouble(null);

                        case TypeCode.String:
                            return ("null" + ic.ToString(null));
                    }
                    break;

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
                            return x;

                        case TypeCode.Boolean:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return DoOp(x, ic.ToInt32(null));

                        case TypeCode.Char:
                            return ((IConvertible) DoOp(x, ic.ToInt32(null))).ToChar(null);

                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return DoOp((long) x, ic.ToInt64(null));

                        case TypeCode.UInt64:
                            if (x >= 0)
                            {
                                return DoOp((ulong) x, ic.ToUInt64(null));
                            }
                            return DoOp((double) x, ic.ToDouble(null));

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp((double) x, ic.ToDouble(null));

                        case TypeCode.String:
                            return (Microsoft.JScript.Convert.ToString(v1) + ic.ToString(null));
                    }
                    break;
                }
                case TypeCode.Char:
                {
                    int num = iConvertible.ToInt32(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.Object:
                        case TypeCode.Decimal:
                        case TypeCode.DateTime:
                            return DoOp(v1, v2);

                        case TypeCode.DBNull:
                            return num;

                        case TypeCode.Boolean:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return ((IConvertible) DoOp(num, ic.ToInt32(null))).ToChar(null);

                        case TypeCode.Char:
                        case TypeCode.String:
                            return (iConvertible.ToString(null) + ic.ToString(null));

                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return ((IConvertible) DoOp((long) num, ic.ToInt64(null))).ToChar(null);

                        case TypeCode.UInt64:
                            return ((IConvertible) DoOp((ulong) num, ic.ToUInt64(null))).ToChar(null);

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return (char) ((int) Microsoft.JScript.Convert.CheckIfDoubleIsInteger((double) DoOp((double) num, ic.ToDouble(null))));
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
                            return num3;

                        case TypeCode.Boolean:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                            return DoOp(num3, ic.ToUInt32(null));

                        case TypeCode.Char:
                            return ((IConvertible) DoOp(num3, ic.ToUInt32(null))).ToChar(null);

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        {
                            int num4 = ic.ToInt32(null);
                            if (num4 >= 0)
                            {
                                return DoOp(num3, (uint) num4);
                            }
                            return DoOp((long) num3, (long) num4);
                        }
                        case TypeCode.Int64:
                            return DoOp((long) num3, ic.ToInt64(null));

                        case TypeCode.UInt64:
                            return DoOp((ulong) num3, ic.ToUInt64(null));

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp((double) num3, ic.ToDouble(null));

                        case TypeCode.String:
                            return (Microsoft.JScript.Convert.ToString(v1) + ic.ToString(null));
                    }
                    break;
                }
                case TypeCode.Int64:
                {
                    long num5 = iConvertible.ToInt64(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.DBNull:
                            return num5;

                        case TypeCode.Boolean:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return DoOp(num5, ic.ToInt64(null));

                        case TypeCode.Char:
                            return ((IConvertible) DoOp(num5, ic.ToInt64(null))).ToChar(null);

                        case TypeCode.UInt64:
                            if (num5 >= 0L)
                            {
                                return DoOp((ulong) num5, ic.ToUInt64(null));
                            }
                            return DoOp((double) num5, ic.ToDouble(null));

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp((double) num5, ic.ToDouble(null));

                        case TypeCode.String:
                            return (Microsoft.JScript.Convert.ToString(v1) + ic.ToString(null));
                    }
                    break;
                }
                case TypeCode.UInt64:
                {
                    ulong num6 = iConvertible.ToUInt64(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.DBNull:
                            return num6;

                        case TypeCode.Boolean:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return DoOp(num6, ic.ToUInt64(null));

                        case TypeCode.Char:
                            return ((IConvertible) DoOp(num6, ic.ToUInt64(null))).ToChar(null);

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        {
                            long num7 = ic.ToInt64(null);
                            if (num7 >= 0L)
                            {
                                return DoOp(num6, (ulong) num7);
                            }
                            return DoOp((double) num6, (double) num7);
                        }
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return DoOp((double) num6, ic.ToDouble(null));

                        case TypeCode.String:
                            return (Microsoft.JScript.Convert.ToString(v1) + ic.ToString(null));
                    }
                    break;
                }
                case TypeCode.Single:
                case TypeCode.Double:
                {
                    double d = iConvertible.ToDouble(null);
                    switch (code2)
                    {
                        case TypeCode.Empty:
                            return (double) 1.0 / (double) 0.0;

                        case TypeCode.DBNull:
                            return iConvertible.ToDouble(null);

                        case TypeCode.Boolean:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return (d + ic.ToInt32(null));

                        case TypeCode.Char:
                            return System.Convert.ToChar(System.Convert.ToInt32((double) (d + ic.ToInt32(null))));

                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return (d + ic.ToDouble(null));

                        case TypeCode.String:
                            return new ConcatString(Microsoft.JScript.Convert.ToString(d), ic.ToString(null));
                    }
                    break;
                }
                case TypeCode.String:
                {
                    TypeCode code11 = code2;
                    if (code11 == TypeCode.Object)
                    {
                        break;
                    }
                    if (code11 != TypeCode.String)
                    {
                        if (v1 is ConcatString)
                        {
                            return new ConcatString((ConcatString) v1, Microsoft.JScript.Convert.ToString(v2));
                        }
                        return new ConcatString(iConvertible.ToString(null), Microsoft.JScript.Convert.ToString(v2));
                    }
                    if (v1 is ConcatString)
                    {
                        return new ConcatString((ConcatString) v1, ic.ToString(null));
                    }
                    return new ConcatString(iConvertible.ToString(null), ic.ToString(null));
                }
            }
            MethodInfo @operator = this.GetOperator((v1 == null) ? Typeob.Empty : v1.GetType(), (v2 == null) ? Typeob.Empty : v2.GetType());
            if (@operator != null)
            {
                return @operator.Invoke(null, BindingFlags.Default, JSBinder.ob, new object[] { v1, v2 }, null);
            }
            return DoOp(v1, v2);
        }

        private MethodInfo GetOperator(IReflect ir1, IReflect ir2)
        {
            Type ir = (ir1 is Type) ? ((Type) ir1) : Typeob.Object;
            Type type2 = (ir2 is Type) ? ((Type) ir2) : Typeob.Object;
            if ((base.type1 == ir) && (base.type2 == type2))
            {
                return base.operatorMeth;
            }
            if (((ir != Typeob.String) && (type2 != Typeob.String)) && ((!Microsoft.JScript.Convert.IsPrimitiveNumericType(ir) && !Typeob.JSObject.IsAssignableFrom(ir)) || (!Microsoft.JScript.Convert.IsPrimitiveNumericType(type2) && !Typeob.JSObject.IsAssignableFrom(type2))))
            {
                return base.GetOperator(ir, type2);
            }
            base.operatorMeth = null;
            base.type1 = ir;
            base.type2 = type2;
            return null;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            MethodInfo @operator;
            if ((base.type1 == null) || (inference_target != null))
            {
                @operator = this.GetOperator(base.operand1.InferType(inference_target), base.operand2.InferType(inference_target));
            }
            else
            {
                @operator = this.GetOperator(base.type1, base.type2);
            }
            if (@operator != null)
            {
                this.metaData = @operator;
                return @operator.ReturnType;
            }
            if ((base.type1 == Typeob.String) || (base.type2 == Typeob.String))
            {
                return Typeob.String;
            }
            if ((base.type1 == Typeob.Char) && (base.type2 == Typeob.Char))
            {
                return Typeob.String;
            }
            if (Microsoft.JScript.Convert.IsPrimitiveNumericTypeFitForDouble(base.type1))
            {
                if (base.type2 == Typeob.Char)
                {
                    return Typeob.Char;
                }
                if (Microsoft.JScript.Convert.IsPrimitiveNumericTypeFitForDouble(base.type2))
                {
                    return Typeob.Double;
                }
                return Typeob.Object;
            }
            if (Microsoft.JScript.Convert.IsPrimitiveNumericTypeFitForDouble(base.type2))
            {
                if (base.type1 == Typeob.Char)
                {
                    return Typeob.Char;
                }
                if (Microsoft.JScript.Convert.IsPrimitiveNumericTypeFitForDouble(base.type1))
                {
                    return Typeob.Double;
                }
                return Typeob.Object;
            }
            if ((base.type1 == Typeob.Boolean) && (base.type2 == Typeob.Char))
            {
                return Typeob.Char;
            }
            if ((base.type1 == Typeob.Char) && (base.type2 == Typeob.Boolean))
            {
                return Typeob.Char;
            }
            return Typeob.Object;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Type type = Microsoft.JScript.Convert.ToType(this.InferType(null));
            if (this.metaData == null)
            {
                Type type2 = Typeob.Object;
                if (rtype == Typeob.Double)
                {
                    type2 = rtype;
                }
                else if ((base.type1 == Typeob.Char) && (base.type2 == Typeob.Char))
                {
                    type2 = Typeob.String;
                }
                else if ((Microsoft.JScript.Convert.IsPrimitiveNumericType(rtype) && Microsoft.JScript.Convert.IsPromotableTo((IReflect) base.type1, (IReflect) rtype)) && Microsoft.JScript.Convert.IsPromotableTo((IReflect) base.type2, (IReflect) rtype))
                {
                    type2 = rtype;
                }
                else if ((base.type1 != Typeob.String) && (base.type2 != Typeob.String))
                {
                    type2 = Typeob.Double;
                }
                else
                {
                    type2 = Typeob.String;
                }
                if ((type2 == Typeob.SByte) || (type2 == Typeob.Int16))
                {
                    type2 = Typeob.Int32;
                }
                else if (((type2 == Typeob.Byte) || (type2 == Typeob.UInt16)) || (type2 == Typeob.Char))
                {
                    type2 = Typeob.UInt32;
                }
                if (type2 == Typeob.String)
                {
                    if ((base.operand1 is Plus) && (base.type1 == type2))
                    {
                        Plus plus = (Plus) base.operand1;
                        if ((plus.operand1 is Plus) && (plus.type1 == type2))
                        {
                            Plus plus2 = (Plus) plus.operand1;
                            if ((plus2.operand1 is Plus) && (plus2.type1 == type2))
                            {
                                int num = plus.TranslateToILArrayOfStrings(il, 1);
                                il.Emit(OpCodes.Dup);
                                ConstantWrapper.TranslateToILInt(il, num - 1);
                                base.operand2.TranslateToIL(il, type2);
                                il.Emit(OpCodes.Stelem_Ref);
                                il.Emit(OpCodes.Call, CompilerGlobals.stringConcatArrMethod);
                                Microsoft.JScript.Convert.Emit(this, il, type2, rtype);
                            }
                            else
                            {
                                TranslateToStringWithSpecialCaseForNull(il, plus2.operand1);
                                TranslateToStringWithSpecialCaseForNull(il, plus2.operand2);
                                TranslateToStringWithSpecialCaseForNull(il, plus.operand2);
                                TranslateToStringWithSpecialCaseForNull(il, base.operand2);
                                il.Emit(OpCodes.Call, CompilerGlobals.stringConcat4Method);
                                Microsoft.JScript.Convert.Emit(this, il, type2, rtype);
                            }
                        }
                        else
                        {
                            TranslateToStringWithSpecialCaseForNull(il, plus.operand1);
                            TranslateToStringWithSpecialCaseForNull(il, plus.operand2);
                            TranslateToStringWithSpecialCaseForNull(il, base.operand2);
                            il.Emit(OpCodes.Call, CompilerGlobals.stringConcat3Method);
                            Microsoft.JScript.Convert.Emit(this, il, type2, rtype);
                        }
                    }
                    else
                    {
                        TranslateToStringWithSpecialCaseForNull(il, base.operand1);
                        TranslateToStringWithSpecialCaseForNull(il, base.operand2);
                        il.Emit(OpCodes.Call, CompilerGlobals.stringConcat2Method);
                        Microsoft.JScript.Convert.Emit(this, il, type2, rtype);
                    }
                }
                else
                {
                    base.operand1.TranslateToIL(il, type2);
                    base.operand2.TranslateToIL(il, type2);
                    if (type2 == Typeob.Object)
                    {
                        il.Emit(OpCodes.Call, CompilerGlobals.plusDoOpMethod);
                    }
                    else if ((type2 == Typeob.Double) || (type2 == Typeob.Single))
                    {
                        il.Emit(OpCodes.Add);
                    }
                    else if ((type2 == Typeob.Int32) || (type2 == Typeob.Int64))
                    {
                        il.Emit(OpCodes.Add_Ovf);
                    }
                    else
                    {
                        il.Emit(OpCodes.Add_Ovf_Un);
                    }
                    if (type == Typeob.Char)
                    {
                        Microsoft.JScript.Convert.Emit(this, il, type2, Typeob.Char);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Char, rtype);
                    }
                    else
                    {
                        Microsoft.JScript.Convert.Emit(this, il, type2, rtype);
                    }
                }
            }
            else if (this.metaData is MethodInfo)
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
                il.Emit(OpCodes.Callvirt, CompilerGlobals.evaluatePlusMethod);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
            }
        }

        private int TranslateToILArrayOfStrings(ILGenerator il, int n)
        {
            int i = n + 2;
            if ((base.operand1 is Plus) && (base.type1 == Typeob.String))
            {
                i = ((Plus) base.operand1).TranslateToILArrayOfStrings(il, n + 1);
            }
            else
            {
                ConstantWrapper.TranslateToILInt(il, i);
                il.Emit(OpCodes.Newarr, Typeob.String);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_0);
                TranslateToStringWithSpecialCaseForNull(il, base.operand1);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Dup);
            ConstantWrapper.TranslateToILInt(il, (i - 1) - n);
            TranslateToStringWithSpecialCaseForNull(il, base.operand2);
            il.Emit(OpCodes.Stelem_Ref);
            return i;
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            IReflect reflect = this.InferType(null);
            base.operand1.TranslateToILInitializer(il);
            base.operand2.TranslateToILInitializer(il);
            if (reflect == Typeob.Object)
            {
                this.metaData = il.DeclareLocal(Typeob.Plus);
                il.Emit(OpCodes.Newobj, CompilerGlobals.plusConstructor);
                il.Emit(OpCodes.Stloc, (LocalBuilder) this.metaData);
            }
        }

        private static void TranslateToStringWithSpecialCaseForNull(ILGenerator il, AST operand)
        {
            ConstantWrapper wrapper = operand as ConstantWrapper;
            if (wrapper != null)
            {
                if (wrapper.value is DBNull)
                {
                    il.Emit(OpCodes.Ldstr, "null");
                }
                else if (wrapper.value == Microsoft.JScript.Empty.Value)
                {
                    il.Emit(OpCodes.Ldstr, "undefined");
                }
                else
                {
                    wrapper.TranslateToIL(il, Typeob.String);
                }
            }
            else
            {
                operand.TranslateToIL(il, Typeob.String);
            }
        }
    }
}

