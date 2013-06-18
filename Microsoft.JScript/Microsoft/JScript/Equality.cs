namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    public class Equality : BinaryOp
    {
        private object metaData;

        public Equality(int operatorTok) : base(null, null, null, (JSToken) operatorTok)
        {
        }

        internal Equality(Context context, AST operand1, AST operand2, JSToken operatorTok) : base(context, operand1, operand2, operatorTok)
        {
        }

        internal override object Evaluate()
        {
            bool flag = this.EvaluateEquality(base.operand1.Evaluate(), base.operand2.Evaluate(), VsaEngine.executeForJSEE);
            if (base.operatorTok == JSToken.Equal)
            {
                return flag;
            }
            return !flag;
        }

        [DebuggerHidden, DebuggerStepThrough]
        public bool EvaluateEquality(object v1, object v2)
        {
            return this.EvaluateEquality(v1, v2, false);
        }

        [DebuggerHidden, DebuggerStepThrough]
        private bool EvaluateEquality(object v1, object v2, bool checkForDebuggerObjects)
        {
            if ((v1 is string) && (v2 is string))
            {
                return v1.Equals(v2);
            }
            if ((v1 is int) && (v2 is int))
            {
                return (((int) v1) == ((int) v2));
            }
            if ((v1 is double) && (v2 is double))
            {
                return (((double) v1) == ((double) v2));
            }
            if ((((v2 == null) || (v2 is DBNull)) || (v2 is Microsoft.JScript.Missing)) && !checkForDebuggerObjects)
            {
                if ((v1 != null) && !(v1 is DBNull))
                {
                    return (v1 is Microsoft.JScript.Missing);
                }
                return true;
            }
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v1);
            IConvertible ic = Microsoft.JScript.Convert.GetIConvertible(v2);
            TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(v1, iConvertible);
            TypeCode code2 = Microsoft.JScript.Convert.GetTypeCode(v2, ic);
            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    goto Label_0190;

                case TypeCode.Object:
                    switch (code2)
                    {
                        case TypeCode.Empty:
                        case TypeCode.DBNull:
                            goto Label_0190;
                    }
                    break;

                default:
                    switch (code2)
                    {
                        case TypeCode.Object:
                        {
                            MethodInfo info2 = base.GetOperator(v1.GetType(), v2.GetType());
                            if (info2 != null)
                            {
                                bool flag2 = (bool) info2.Invoke(null, BindingFlags.Default, JSBinder.ob, new object[] { v1, v2 }, null);
                                if (base.operatorTok == JSToken.NotEqual)
                                {
                                    return !flag2;
                                }
                                return flag2;
                            }
                        }
                    }
                    goto Label_0190;
            }
            MethodInfo @operator = base.GetOperator(v1.GetType(), v2.GetType());
            if (@operator != null)
            {
                bool flag = (bool) @operator.Invoke(null, BindingFlags.Default, JSBinder.ob, new object[] { v1, v2 }, null);
                if (base.operatorTok == JSToken.NotEqual)
                {
                    return !flag;
                }
                return flag;
            }
        Label_0190:
            return JScriptEquals(v1, v2, iConvertible, ic, typeCode, code2, checkForDebuggerObjects);
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return Typeob.Boolean;
        }

        public static bool JScriptEquals(object v1, object v2)
        {
            if ((v1 is string) && (v2 is string))
            {
                return v1.Equals(v2);
            }
            if ((v1 is int) && (v2 is int))
            {
                return (((int) v1) == ((int) v2));
            }
            if ((v1 is double) && (v2 is double))
            {
                return (((double) v1) == ((double) v2));
            }
            if (((v2 == null) || (v2 is DBNull)) || (v2 is Microsoft.JScript.Missing))
            {
                if ((v1 != null) && !(v1 is DBNull))
                {
                    return (v1 is Microsoft.JScript.Missing);
                }
                return true;
            }
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v1);
            IConvertible ic = Microsoft.JScript.Convert.GetIConvertible(v2);
            TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(v1, iConvertible);
            TypeCode code2 = Microsoft.JScript.Convert.GetTypeCode(v2, ic);
            return JScriptEquals(v1, v2, iConvertible, ic, typeCode, code2, false);
        }

        private static bool JScriptEquals(object v1, object v2, IConvertible ic1, IConvertible ic2, TypeCode t1, TypeCode t2, bool checkForDebuggerObjects)
        {
            if (StrictEquality.JScriptStrictEquals(v1, v2, ic1, ic2, t1, t2, checkForDebuggerObjects))
            {
                return true;
            }
            if (t2 == TypeCode.Boolean)
            {
                v2 = ic2.ToBoolean(null) ? 1 : 0;
                ic2 = Microsoft.JScript.Convert.GetIConvertible(v2);
                return JScriptEquals(v1, v2, ic1, ic2, t1, TypeCode.Int32, false);
            }
            switch (t1)
            {
                case TypeCode.Empty:
                    if ((t2 == TypeCode.Empty) || (t2 == TypeCode.DBNull))
                    {
                        return true;
                    }
                    if (t2 != TypeCode.Object)
                    {
                        return false;
                    }
                    return (v2 is Microsoft.JScript.Missing);

                case TypeCode.Object:
                    switch (t2)
                    {
                        case TypeCode.Empty:
                        case TypeCode.DBNull:
                            return (v1 is Microsoft.JScript.Missing);

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
                        {
                            IConvertible ic = ic1;
                            object obj2 = Microsoft.JScript.Convert.ToPrimitive(v1, PreferredType.Either, ref ic);
                            return (((ic != null) && (obj2 != v1)) && JScriptEquals(obj2, v2, ic, ic2, ic.GetTypeCode(), t2, false));
                        }
                    }
                    break;

                case TypeCode.DBNull:
                    if ((t2 == TypeCode.DBNull) || (t2 == TypeCode.Empty))
                    {
                        return true;
                    }
                    if (t2 != TypeCode.Object)
                    {
                        return false;
                    }
                    return (v2 is Microsoft.JScript.Missing);

                case TypeCode.Boolean:
                    v1 = ic1.ToBoolean(null) ? 1 : 0;
                    ic1 = Microsoft.JScript.Convert.GetIConvertible(v1);
                    return JScriptEquals(v1, v2, ic1, ic2, TypeCode.Int32, t2, false);

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
                {
                    if (t2 != TypeCode.Object)
                    {
                        if (t2 != TypeCode.String)
                        {
                            return false;
                        }
                        if (v1 is Enum)
                        {
                            return Microsoft.JScript.Convert.ToString(v1).Equals(ic2.ToString(null));
                        }
                        v2 = Microsoft.JScript.Convert.ToNumber(v2, ic2);
                        ic2 = Microsoft.JScript.Convert.GetIConvertible(v2);
                        return StrictEquality.JScriptStrictEquals(v1, v2, ic1, ic2, t1, TypeCode.Double, false);
                    }
                    IConvertible convertible2 = ic2;
                    object obj3 = Microsoft.JScript.Convert.ToPrimitive(v2, PreferredType.Either, ref convertible2);
                    if ((convertible2 == null) || (obj3 == v2))
                    {
                        return false;
                    }
                    return JScriptEquals(v1, obj3, ic1, convertible2, t1, convertible2.GetTypeCode(), false);
                }
                case TypeCode.DateTime:
                {
                    if (t2 != TypeCode.Object)
                    {
                        goto Label_0236;
                    }
                    IConvertible convertible3 = ic2;
                    object obj4 = Microsoft.JScript.Convert.ToPrimitive(v2, PreferredType.Either, ref convertible3);
                    if ((obj4 == null) || (obj4 == v2))
                    {
                        goto Label_0236;
                    }
                    return StrictEquality.JScriptStrictEquals(v1, obj4, ic1, convertible3, t1, convertible3.GetTypeCode(), false);
                }
                case TypeCode.String:
                    switch (t2)
                    {
                        case TypeCode.Object:
                        {
                            IConvertible convertible4 = ic2;
                            object obj5 = Microsoft.JScript.Convert.ToPrimitive(v2, PreferredType.Either, ref convertible4);
                            return (((convertible4 != null) && (obj5 != v2)) && JScriptEquals(v1, obj5, ic1, convertible4, t1, convertible4.GetTypeCode(), false));
                        }
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
                            if (v2 is Enum)
                            {
                                return Microsoft.JScript.Convert.ToString(v2).Equals(ic1.ToString(null));
                            }
                            v1 = Microsoft.JScript.Convert.ToNumber(v1, ic1);
                            ic1 = Microsoft.JScript.Convert.GetIConvertible(v1);
                            return StrictEquality.JScriptStrictEquals(v1, v2, ic1, ic2, TypeCode.Double, t2, false);
                    }
                    return false;

                default:
                    return false;
            }
            return false;
        Label_0236:
            return false;
        }

        internal override void TranslateToConditionalBranch(ILGenerator il, bool branchIfTrue, Label label, bool shortForm)
        {
            if (this.metaData == null)
            {
                Type type = base.type1;
                Type type2 = base.type2;
                Type rtype = Typeob.Object;
                bool flag = true;
                if (type.IsPrimitive && type2.IsPrimitive)
                {
                    rtype = Typeob.Double;
                    if ((type == Typeob.Single) || (type2 == Typeob.Single))
                    {
                        rtype = Typeob.Single;
                    }
                    else if (Microsoft.JScript.Convert.IsPromotableTo((IReflect) type, (IReflect) type2))
                    {
                        rtype = type2;
                    }
                    else if (Microsoft.JScript.Convert.IsPromotableTo((IReflect) type2, (IReflect) type))
                    {
                        rtype = type;
                    }
                }
                else if ((type == Typeob.String) && (((type2 == Typeob.String) || (type2 == Typeob.Empty)) || (type2 == Typeob.Null)))
                {
                    rtype = Typeob.String;
                    if (type2 != Typeob.String)
                    {
                        flag = false;
                        branchIfTrue = !branchIfTrue;
                    }
                }
                else if (((type == Typeob.Empty) || (type == Typeob.Null)) && (type2 == Typeob.String))
                {
                    rtype = Typeob.String;
                    flag = false;
                    branchIfTrue = !branchIfTrue;
                }
                if ((rtype == Typeob.SByte) || (rtype == Typeob.Int16))
                {
                    rtype = Typeob.Int32;
                }
                else if ((rtype == Typeob.Byte) || (rtype == Typeob.UInt16))
                {
                    rtype = Typeob.UInt32;
                }
                if (flag)
                {
                    base.operand1.TranslateToIL(il, rtype);
                    base.operand2.TranslateToIL(il, rtype);
                    if (rtype == Typeob.Object)
                    {
                        il.Emit(OpCodes.Call, CompilerGlobals.jScriptEqualsMethod);
                    }
                    else if (rtype == Typeob.String)
                    {
                        il.Emit(OpCodes.Call, CompilerGlobals.stringEqualsMethod);
                    }
                }
                else if (type == Typeob.String)
                {
                    base.operand1.TranslateToIL(il, rtype);
                }
                else if (type2 == Typeob.String)
                {
                    base.operand2.TranslateToIL(il, rtype);
                }
                if (branchIfTrue)
                {
                    if (base.operatorTok == JSToken.Equal)
                    {
                        if ((rtype == Typeob.String) || (rtype == Typeob.Object))
                        {
                            il.Emit(shortForm ? OpCodes.Brtrue_S : OpCodes.Brtrue, label);
                        }
                        else
                        {
                            il.Emit(shortForm ? OpCodes.Beq_S : OpCodes.Beq, label);
                        }
                    }
                    else if ((rtype == Typeob.String) || (rtype == Typeob.Object))
                    {
                        il.Emit(shortForm ? OpCodes.Brfalse_S : OpCodes.Brfalse, label);
                    }
                    else
                    {
                        il.Emit(shortForm ? OpCodes.Bne_Un_S : OpCodes.Bne_Un, label);
                    }
                }
                else if (base.operatorTok == JSToken.Equal)
                {
                    if ((rtype == Typeob.String) || (rtype == Typeob.Object))
                    {
                        il.Emit(shortForm ? OpCodes.Brfalse_S : OpCodes.Brfalse, label);
                    }
                    else
                    {
                        il.Emit(shortForm ? OpCodes.Bne_Un_S : OpCodes.Bne_Un, label);
                    }
                }
                else if ((rtype == Typeob.String) || (rtype == Typeob.Object))
                {
                    il.Emit(shortForm ? OpCodes.Brtrue_S : OpCodes.Brtrue, label);
                }
                else
                {
                    il.Emit(shortForm ? OpCodes.Beq_S : OpCodes.Beq, label);
                }
            }
            else if (this.metaData is MethodInfo)
            {
                MethodInfo metaData = (MethodInfo) this.metaData;
                ParameterInfo[] parameters = metaData.GetParameters();
                base.operand1.TranslateToIL(il, parameters[0].ParameterType);
                base.operand2.TranslateToIL(il, parameters[1].ParameterType);
                il.Emit(OpCodes.Call, metaData);
                if (branchIfTrue)
                {
                    il.Emit(shortForm ? OpCodes.Brtrue_S : OpCodes.Brtrue, label);
                }
                else
                {
                    il.Emit(shortForm ? OpCodes.Brfalse_S : OpCodes.Brfalse, label);
                }
            }
            else
            {
                il.Emit(OpCodes.Ldloc, (LocalBuilder) this.metaData);
                base.operand1.TranslateToIL(il, Typeob.Object);
                base.operand2.TranslateToIL(il, Typeob.Object);
                il.Emit(OpCodes.Call, CompilerGlobals.evaluateEqualityMethod);
                if (branchIfTrue)
                {
                    if (base.operatorTok == JSToken.Equal)
                    {
                        il.Emit(shortForm ? OpCodes.Brtrue_S : OpCodes.Brtrue, label);
                    }
                    else
                    {
                        il.Emit(shortForm ? OpCodes.Brfalse_S : OpCodes.Brfalse, label);
                    }
                }
                else if (base.operatorTok == JSToken.Equal)
                {
                    il.Emit(shortForm ? OpCodes.Brfalse_S : OpCodes.Brfalse, label);
                }
                else
                {
                    il.Emit(shortForm ? OpCodes.Brtrue_S : OpCodes.Brtrue, label);
                }
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

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            base.operand1.TranslateToILInitializer(il);
            base.operand2.TranslateToILInitializer(il);
            MethodInfo @operator = base.GetOperator(base.operand1.InferType(null), base.operand2.InferType(null));
            if (@operator != null)
            {
                this.metaData = @operator;
            }
            else
            {
                if (base.operand1 is ConstantWrapper)
                {
                    object obj2 = base.operand1.Evaluate();
                    if (obj2 == null)
                    {
                        base.type1 = Typeob.Empty;
                    }
                    else if (obj2 is DBNull)
                    {
                        base.type1 = Typeob.Null;
                    }
                }
                if (base.operand2 is ConstantWrapper)
                {
                    object obj3 = base.operand2.Evaluate();
                    if (obj3 == null)
                    {
                        base.type2 = Typeob.Empty;
                    }
                    else if (obj3 is DBNull)
                    {
                        base.type2 = Typeob.Null;
                    }
                }
                if ((((base.type1 != Typeob.Empty) && (base.type1 != Typeob.Null)) && ((base.type2 != Typeob.Empty) && (base.type2 != Typeob.Null))) && (((!base.type1.IsPrimitive && (base.type1 != Typeob.String)) && !Typeob.JSObject.IsAssignableFrom(base.type1)) || ((!base.type2.IsPrimitive && (base.type2 != Typeob.String)) && !Typeob.JSObject.IsAssignableFrom(base.type2))))
                {
                    this.metaData = il.DeclareLocal(Typeob.Equality);
                    ConstantWrapper.TranslateToILInt(il, (int) base.operatorTok);
                    il.Emit(OpCodes.Newobj, CompilerGlobals.equalityConstructor);
                    il.Emit(OpCodes.Stloc, (LocalBuilder) this.metaData);
                }
            }
        }
    }
}

