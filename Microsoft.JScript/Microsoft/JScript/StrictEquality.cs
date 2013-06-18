namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public class StrictEquality : BinaryOp
    {
        internal StrictEquality(Context context, AST operand1, AST operand2, JSToken operatorTok) : base(context, operand1, operand2, operatorTok)
        {
        }

        internal override object Evaluate()
        {
            bool flag = JScriptStrictEquals(base.operand1.Evaluate(), base.operand2.Evaluate(), VsaEngine.executeForJSEE);
            if (base.operatorTok == JSToken.StrictEqual)
            {
                return flag;
            }
            return !flag;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return Typeob.Boolean;
        }

        public static bool JScriptStrictEquals(object v1, object v2)
        {
            return JScriptStrictEquals(v1, v2, false);
        }

        internal static bool JScriptStrictEquals(object v1, object v2, bool checkForDebuggerObjects)
        {
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v1);
            IConvertible ic = Microsoft.JScript.Convert.GetIConvertible(v2);
            TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(v1, iConvertible);
            TypeCode code2 = Microsoft.JScript.Convert.GetTypeCode(v2, ic);
            return JScriptStrictEquals(v1, v2, iConvertible, ic, typeCode, code2, checkForDebuggerObjects);
        }

        internal static bool JScriptStrictEquals(object v1, object v2, IConvertible ic1, IConvertible ic2, TypeCode t1, TypeCode t2, bool checkForDebuggerObjects)
        {
            long num7;
            switch (t1)
            {
                case TypeCode.Empty:
                    return (t2 == TypeCode.Empty);

                case TypeCode.Object:
                    if (v1 != v2)
                    {
                        if ((v1 is Microsoft.JScript.Missing) || (v1 is System.Reflection.Missing))
                        {
                            v1 = null;
                        }
                        if (v1 == v2)
                        {
                            return true;
                        }
                        if ((v2 is Microsoft.JScript.Missing) || (v2 is System.Reflection.Missing))
                        {
                            v2 = null;
                        }
                        if (checkForDebuggerObjects)
                        {
                            IDebuggerObject obj2 = v1 as IDebuggerObject;
                            if (obj2 != null)
                            {
                                IDebuggerObject o = v2 as IDebuggerObject;
                                if (o != null)
                                {
                                    return obj2.IsEqual(o);
                                }
                            }
                        }
                        return (v1 == v2);
                    }
                    return true;

                case TypeCode.DBNull:
                    return (t2 == TypeCode.DBNull);

                case TypeCode.Boolean:
                    if (t2 != TypeCode.Boolean)
                    {
                        return false;
                    }
                    return (ic1.ToBoolean(null) == ic2.ToBoolean(null));

                case TypeCode.Char:
                {
                    char ch = ic1.ToChar(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (ch == ic2.ToChar(null));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (ch == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return (ch == ic2.ToUInt64(null));

                        case TypeCode.Single:
                        case TypeCode.Double:
                            return (((double) ch) == ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (ch == ic2.ToDecimal(null));

                        case TypeCode.String:
                        {
                            string str = ic2.ToString(null);
                            return ((str.Length == 1) && (ch == str[0]));
                        }
                    }
                    break;
                }
                case TypeCode.SByte:
                {
                    sbyte num = ic1.ToSByte(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num == ic2.ToChar(null));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (num == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return ((num >= 0) && (num == ic2.ToUInt64(null)));

                        case TypeCode.Single:
                            return (num == ic2.ToSingle(null));

                        case TypeCode.Double:
                            return (num == ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (num == ic2.ToDecimal(null));
                    }
                    return false;
                }
                case TypeCode.Byte:
                {
                    byte num2 = ic1.ToByte(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num2 == ic2.ToChar(null));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (num2 == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return (num2 == ic2.ToUInt64(null));

                        case TypeCode.Single:
                            return (num2 == ic2.ToSingle(null));

                        case TypeCode.Double:
                            return (num2 == ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (num2 == ic2.ToDecimal(null));
                    }
                    return false;
                }
                case TypeCode.Int16:
                {
                    short num3 = ic1.ToInt16(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num3 == ic2.ToChar(null));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (num3 == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return ((num3 >= 0) && (num3 == ic2.ToUInt64(null)));

                        case TypeCode.Single:
                            return (num3 == ic2.ToSingle(null));

                        case TypeCode.Double:
                            return (num3 == ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (num3 == ic2.ToDecimal(null));
                    }
                    return false;
                }
                case TypeCode.UInt16:
                {
                    ushort num4 = ic1.ToUInt16(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num4 == ic2.ToChar(null));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (num4 == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return (num4 == ic2.ToUInt64(null));

                        case TypeCode.Single:
                            return (num4 == ic2.ToSingle(null));

                        case TypeCode.Double:
                            return (num4 == ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (num4 == ic2.ToDecimal(null));
                    }
                    return false;
                }
                case TypeCode.Int32:
                {
                    int num5 = ic1.ToInt32(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num5 == ic2.ToChar(null));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (num5 == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return ((num5 >= 0) && (num5 == ic2.ToUInt64(null)));

                        case TypeCode.Single:
                            return (num5 == ic2.ToSingle(null));

                        case TypeCode.Double:
                            return (num5 == ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (num5 == ic2.ToDecimal(null));
                    }
                    return false;
                }
                case TypeCode.UInt32:
                {
                    uint num6 = ic1.ToUInt32(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num6 == ic2.ToChar(null));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (num6 == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return (num6 == ic2.ToUInt64(null));

                        case TypeCode.Single:
                            return (num6 == ic2.ToSingle(null));

                        case TypeCode.Double:
                            return (num6 == ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (num6 == ic2.ToDecimal(null));
                    }
                    return false;
                }
                case TypeCode.Int64:
                    num7 = ic1.ToInt64(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num7 == ic2.ToChar(null));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (num7 == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return ((num7 >= 0L) && (num7 == ic2.ToUInt64(null)));

                        case TypeCode.Single:
                            return (num7 == ic2.ToSingle(null));

                        case TypeCode.Double:
                            return (num7 == ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (num7 == ic2.ToDecimal(null));
                    }
                    return false;

                case TypeCode.UInt64:
                {
                    ulong num8 = ic1.ToUInt64(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num8 == ic2.ToChar(null));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            num7 = ic2.ToInt64(null);
                            return ((num7 >= 0L) && (num8 == num7));

                        case TypeCode.UInt64:
                            return (num8 == ic2.ToUInt64(null));

                        case TypeCode.Single:
                            return (num8 == ic2.ToSingle(null));

                        case TypeCode.Double:
                            return (num8 == ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (num8 == ic2.ToDecimal(null));
                    }
                    return false;
                }
                case TypeCode.Single:
                {
                    float num9 = ic1.ToSingle(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num9 == ((float) ic2.ToChar(null)));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (num9 == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return (num9 == ic2.ToUInt64(null));

                        case TypeCode.Single:
                            return (num9 == ic2.ToSingle(null));

                        case TypeCode.Double:
                            return (num9 == ic2.ToSingle(null));

                        case TypeCode.Decimal:
                            return (((decimal) num9) == ic2.ToDecimal(null));
                    }
                    return false;
                }
                case TypeCode.Double:
                {
                    double num10 = ic1.ToDouble(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num10 == ((double) ic2.ToChar(null)));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (num10 == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return (num10 == ic2.ToUInt64(null));

                        case TypeCode.Single:
                            return (((float) num10) == ic2.ToSingle(null));

                        case TypeCode.Double:
                            return (num10 == ic2.ToDouble(null));

                        case TypeCode.Decimal:
                            return (((decimal) num10) == ic2.ToDecimal(null));
                    }
                    return false;
                }
                case TypeCode.Decimal:
                {
                    decimal num11 = ic1.ToDecimal(null);
                    switch (t2)
                    {
                        case TypeCode.Char:
                            return (num11 == ic2.ToChar(null));

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return (num11 == ic2.ToInt64(null));

                        case TypeCode.UInt64:
                            return (num11 == ic2.ToUInt64(null));

                        case TypeCode.Single:
                            return (num11 == ((decimal) ic2.ToSingle(null)));

                        case TypeCode.Double:
                            return (num11 == ((decimal) ic2.ToDouble(null)));

                        case TypeCode.Decimal:
                            return (num11 == ic2.ToDecimal(null));
                    }
                    return false;
                }
                case TypeCode.DateTime:
                    if (t2 != TypeCode.DateTime)
                    {
                        return false;
                    }
                    return (ic1.ToDateTime(null) == ic2.ToDateTime(null));

                case TypeCode.String:
                {
                    if (t2 != TypeCode.Char)
                    {
                        if (t2 != TypeCode.String)
                        {
                            return false;
                        }
                        if (v1 != v2)
                        {
                            return ic1.ToString(null).Equals(ic2.ToString(null));
                        }
                        return true;
                    }
                    string str2 = ic1.ToString(null);
                    if (str2.Length != 1)
                    {
                        return false;
                    }
                    return (str2[0] == ic2.ToChar(null));
                }
                default:
                    return false;
            }
            return false;
        }

        internal override void TranslateToConditionalBranch(ILGenerator il, bool branchIfTrue, Label label, bool shortForm)
        {
            Type empty = Microsoft.JScript.Convert.ToType(base.operand1.InferType(null));
            Type type2 = Microsoft.JScript.Convert.ToType(base.operand2.InferType(null));
            if ((base.operand1 is ConstantWrapper) && (base.operand1.Evaluate() == null))
            {
                empty = Typeob.Empty;
            }
            if ((base.operand2 is ConstantWrapper) && (base.operand2.Evaluate() == null))
            {
                type2 = Typeob.Empty;
            }
            if (((empty != type2) && empty.IsPrimitive) && type2.IsPrimitive)
            {
                if (empty == Typeob.Single)
                {
                    type2 = empty;
                }
                else if (type2 == Typeob.Single)
                {
                    empty = type2;
                }
                else if (Microsoft.JScript.Convert.IsPromotableTo((IReflect) type2, (IReflect) empty))
                {
                    type2 = empty;
                }
                else if (Microsoft.JScript.Convert.IsPromotableTo((IReflect) empty, (IReflect) type2))
                {
                    empty = type2;
                }
            }
            bool flag = true;
            if ((empty == type2) && (empty != Typeob.Object))
            {
                Type rtype = empty;
                if (!empty.IsPrimitive)
                {
                    rtype = Typeob.Object;
                }
                base.operand1.TranslateToIL(il, rtype);
                base.operand2.TranslateToIL(il, rtype);
                if (empty == Typeob.String)
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.stringEqualsMethod);
                }
                else if (!empty.IsPrimitive)
                {
                    il.Emit(OpCodes.Callvirt, CompilerGlobals.equalsMethod);
                }
                else
                {
                    flag = false;
                }
            }
            else if (empty == Typeob.Empty)
            {
                base.operand2.TranslateToIL(il, Typeob.Object);
                branchIfTrue = !branchIfTrue;
            }
            else if (type2 == Typeob.Empty)
            {
                base.operand1.TranslateToIL(il, Typeob.Object);
                branchIfTrue = !branchIfTrue;
            }
            else
            {
                base.operand1.TranslateToIL(il, Typeob.Object);
                base.operand2.TranslateToIL(il, Typeob.Object);
                il.Emit(OpCodes.Call, CompilerGlobals.jScriptStrictEqualsMethod);
            }
            if (branchIfTrue)
            {
                if (base.operatorTok == JSToken.StrictEqual)
                {
                    if (flag)
                    {
                        il.Emit(shortForm ? OpCodes.Brtrue_S : OpCodes.Brtrue, label);
                    }
                    else
                    {
                        il.Emit(shortForm ? OpCodes.Beq_S : OpCodes.Beq, label);
                    }
                }
                else if (flag)
                {
                    il.Emit(shortForm ? OpCodes.Brfalse_S : OpCodes.Brfalse, label);
                }
                else
                {
                    il.Emit(shortForm ? OpCodes.Bne_Un_S : OpCodes.Bne_Un, label);
                }
            }
            else if (base.operatorTok == JSToken.StrictEqual)
            {
                if (flag)
                {
                    il.Emit(shortForm ? OpCodes.Brfalse_S : OpCodes.Brfalse, label);
                }
                else
                {
                    il.Emit(shortForm ? OpCodes.Bne_Un_S : OpCodes.Bne_Un, label);
                }
            }
            else if (flag)
            {
                il.Emit(shortForm ? OpCodes.Brtrue_S : OpCodes.Brtrue, label);
            }
            else
            {
                il.Emit(shortForm ? OpCodes.Beq_S : OpCodes.Beq, label);
            }
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
    }
}

