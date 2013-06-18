namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class NumericUnary : UnaryOp
    {
        private object metaData;
        private MethodInfo operatorMeth;
        private JSToken operatorTok;
        private Type type;

        public NumericUnary(int operatorTok) : this(null, null, (JSToken) operatorTok)
        {
        }

        internal NumericUnary(Context context, AST operand, JSToken operatorTok) : base(context, operand)
        {
            this.operatorTok = operatorTok;
            this.operatorMeth = null;
            this.type = null;
        }

        internal override object Evaluate()
        {
            return this.EvaluateUnary(base.operand.Evaluate());
        }

        [DebuggerHidden, DebuggerStepThrough]
        public object EvaluateUnary(object v)
        {
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v);
            switch (Microsoft.JScript.Convert.GetTypeCode(v, iConvertible))
            {
                case TypeCode.Empty:
                    return this.EvaluateUnary((double) 1.0 / (double) 0.0);

                case TypeCode.DBNull:
                    return this.EvaluateUnary(0);

                case TypeCode.Boolean:
                    return this.EvaluateUnary(iConvertible.ToBoolean(null) ? 1 : 0);

                case TypeCode.Char:
                    return this.EvaluateUnary((int) iConvertible.ToChar(null));

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                {
                    int num = iConvertible.ToInt32(null);
                    switch (this.operatorTok)
                    {
                        case JSToken.FirstOp:
                            return (num == 0);

                        case JSToken.BitwiseNot:
                            return ~num;

                        case JSToken.FirstBinaryOp:
                            return num;

                        case JSToken.Minus:
                            switch (num)
                            {
                                case 0:
                                    return -((double) num);

                                case -2147483648:
                                    return (ulong) -((double) num);
                            }
                            return -num;
                    }
                    throw new JScriptException(JSError.InternalError, base.context);
                }
                case TypeCode.UInt32:
                {
                    uint num2 = iConvertible.ToUInt32(null);
                    switch (this.operatorTok)
                    {
                        case JSToken.FirstOp:
                            return (num2 == 0);

                        case JSToken.BitwiseNot:
                            return ~num2;

                        case JSToken.FirstBinaryOp:
                            return num2;

                        case JSToken.Minus:
                            if ((num2 != 0) && (num2 <= 0x7fffffff))
                            {
                                return (int) -num2;
                            }
                            return -((double) num2);
                    }
                    throw new JScriptException(JSError.InternalError, base.context);
                }
                case TypeCode.Int64:
                {
                    long num3 = iConvertible.ToInt64(null);
                    switch (this.operatorTok)
                    {
                        case JSToken.FirstOp:
                            return (num3 == 0L);

                        case JSToken.BitwiseNot:
                            return ~num3;

                        case JSToken.FirstBinaryOp:
                            return num3;

                        case JSToken.Minus:
                            if ((num3 != 0L) && (num3 != -9223372036854775808L))
                            {
                                return -num3;
                            }
                            return -((double) num3);
                    }
                    throw new JScriptException(JSError.InternalError, base.context);
                }
                case TypeCode.UInt64:
                {
                    ulong num4 = iConvertible.ToUInt64(null);
                    switch (this.operatorTok)
                    {
                        case JSToken.FirstOp:
                            return (num4 == 0L);

                        case JSToken.BitwiseNot:
                            return ~num4;

                        case JSToken.FirstBinaryOp:
                            return num4;

                        case JSToken.Minus:
                            if ((num4 != 0L) && (num4 <= 0x7fffffffffffffffL))
                            {
                                return (long) -num4;
                            }
                            return -((double) num4);
                    }
                    throw new JScriptException(JSError.InternalError, base.context);
                }
                case TypeCode.Single:
                case TypeCode.Double:
                {
                    double d = iConvertible.ToDouble(null);
                    switch (this.operatorTok)
                    {
                        case JSToken.FirstOp:
                            return !Microsoft.JScript.Convert.ToBoolean(d);

                        case JSToken.BitwiseNot:
                            return ~((int) Runtime.DoubleToInt64(d));

                        case JSToken.FirstBinaryOp:
                            return d;

                        case JSToken.Minus:
                            return -d;
                    }
                    throw new JScriptException(JSError.InternalError, base.context);
                }
                case TypeCode.String:
                    break;

                default:
                {
                    MethodInfo @operator = this.GetOperator(v.GetType());
                    if (@operator != null)
                    {
                        return @operator.Invoke(null, BindingFlags.Default, JSBinder.ob, new object[] { v }, null);
                    }
                    break;
                }
            }
            switch (this.operatorTok)
            {
                case JSToken.FirstOp:
                    return !Microsoft.JScript.Convert.ToBoolean(v, iConvertible);

                case JSToken.BitwiseNot:
                    return ~Microsoft.JScript.Convert.ToInt32(v, iConvertible);

                case JSToken.FirstBinaryOp:
                    return Microsoft.JScript.Convert.ToNumber(v, iConvertible);

                case JSToken.Minus:
                    return -Microsoft.JScript.Convert.ToNumber(v, iConvertible);
            }
            throw new JScriptException(JSError.InternalError, base.context);
        }

        private MethodInfo GetOperator(IReflect ir)
        {
            Type type = (ir is Type) ? ((Type) ir) : Typeob.Object;
            if (this.type != type)
            {
                this.type = type;
                if (Microsoft.JScript.Convert.IsPrimitiveNumericType(type) || Typeob.JSObject.IsAssignableFrom(type))
                {
                    this.operatorMeth = null;
                    return null;
                }
                switch (this.operatorTok)
                {
                    case JSToken.FirstOp:
                        this.operatorMeth = type.GetMethod("op_LogicalNot", BindingFlags.Public | BindingFlags.Static, JSBinder.ob, new Type[] { type }, null);
                        break;

                    case JSToken.BitwiseNot:
                        this.operatorMeth = type.GetMethod("op_OnesComplement", BindingFlags.Public | BindingFlags.Static, JSBinder.ob, new Type[] { type }, null);
                        break;

                    case JSToken.FirstBinaryOp:
                        this.operatorMeth = type.GetMethod("op_UnaryPlus", BindingFlags.Public | BindingFlags.Static, JSBinder.ob, new Type[] { type }, null);
                        break;

                    case JSToken.Minus:
                        this.operatorMeth = type.GetMethod("op_UnaryNegation", BindingFlags.Public | BindingFlags.Static, JSBinder.ob, new Type[] { type }, null);
                        break;

                    default:
                        throw new JScriptException(JSError.InternalError, base.context);
                }
                if (((this.operatorMeth == null) || ((this.operatorMeth.Attributes & MethodAttributes.SpecialName) == MethodAttributes.PrivateScope)) || (this.operatorMeth.GetParameters().Length != 1))
                {
                    this.operatorMeth = null;
                }
                if (this.operatorMeth != null)
                {
                    this.operatorMeth = new JSMethodInfo(this.operatorMeth);
                }
            }
            return this.operatorMeth;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            MethodInfo @operator;
            if ((this.type == null) || (inference_target != null))
            {
                @operator = this.GetOperator(base.operand.InferType(inference_target));
            }
            else
            {
                @operator = this.GetOperator(this.type);
            }
            if (@operator != null)
            {
                this.metaData = @operator;
                return @operator.ReturnType;
            }
            if (this.operatorTok == JSToken.FirstOp)
            {
                return Typeob.Boolean;
            }
            switch (Type.GetTypeCode(this.type))
            {
                case TypeCode.Empty:
                    if (this.operatorTok == JSToken.BitwiseNot)
                    {
                        return Typeob.Int32;
                    }
                    return Typeob.Double;

                case TypeCode.Object:
                    return Typeob.Object;

                case TypeCode.DBNull:
                    return Typeob.Int32;

                case TypeCode.Boolean:
                    return Typeob.Int32;

                case TypeCode.Char:
                    return Typeob.Int32;

                case TypeCode.SByte:
                    if (this.operatorTok == JSToken.BitwiseNot)
                    {
                        return Typeob.SByte;
                    }
                    return Typeob.Int32;

                case TypeCode.Byte:
                    if (this.operatorTok == JSToken.BitwiseNot)
                    {
                        return Typeob.Byte;
                    }
                    return Typeob.Int32;

                case TypeCode.Int16:
                    if (this.operatorTok == JSToken.BitwiseNot)
                    {
                        return Typeob.Int16;
                    }
                    return Typeob.Int32;

                case TypeCode.UInt16:
                    if (this.operatorTok == JSToken.BitwiseNot)
                    {
                        return Typeob.UInt16;
                    }
                    return Typeob.Int32;

                case TypeCode.Int32:
                    return Typeob.Int32;

                case TypeCode.UInt32:
                    if (this.operatorTok == JSToken.Minus)
                    {
                        return Typeob.Double;
                    }
                    return Typeob.UInt32;

                case TypeCode.Int64:
                    return Typeob.Int64;

                case TypeCode.UInt64:
                    if (this.operatorTok == JSToken.Minus)
                    {
                        return Typeob.Double;
                    }
                    return Typeob.UInt64;

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.String:
                    if (this.operatorTok == JSToken.BitwiseNot)
                    {
                        return Typeob.Int32;
                    }
                    return Typeob.Double;
            }
            if (Typeob.JSObject.IsAssignableFrom(this.type))
            {
                return Typeob.Double;
            }
            return Typeob.Object;
        }

        internal override void TranslateToConditionalBranch(ILGenerator il, bool branchIfTrue, Label label, bool shortForm)
        {
            if (this.operatorTok == JSToken.FirstOp)
            {
                base.operand.TranslateToConditionalBranch(il, !branchIfTrue, label, shortForm);
            }
            else
            {
                base.TranslateToConditionalBranch(il, branchIfTrue, label, shortForm);
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
                    base.operand.TranslateToIL(il, parameters[0].ParameterType);
                    il.Emit(OpCodes.Call, metaData);
                    Microsoft.JScript.Convert.Emit(this, il, metaData.ReturnType, rtype);
                }
                else
                {
                    il.Emit(OpCodes.Ldloc, (LocalBuilder) this.metaData);
                    base.operand.TranslateToIL(il, Typeob.Object);
                    il.Emit(OpCodes.Call, CompilerGlobals.evaluateUnaryMethod);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                }
            }
            else
            {
                Type t = (this.operatorTok == JSToken.FirstOp) ? Typeob.Boolean : Typeob.Double;
                if (Microsoft.JScript.Convert.IsPrimitiveNumericType(rtype) && Microsoft.JScript.Convert.IsPromotableTo((IReflect) this.type, (IReflect) rtype))
                {
                    t = rtype;
                }
                if ((this.operatorTok == JSToken.BitwiseNot) && !Microsoft.JScript.Convert.IsPrimitiveIntegerType(t))
                {
                    t = this.type;
                    if (!Microsoft.JScript.Convert.IsPrimitiveIntegerType(t))
                    {
                        t = Typeob.Int32;
                    }
                }
                base.operand.TranslateToIL(il, this.type);
                Microsoft.JScript.Convert.Emit(this, il, this.type, t, true);
                switch (this.operatorTok)
                {
                    case JSToken.FirstOp:
                        Microsoft.JScript.Convert.Emit(this, il, t, Typeob.Boolean, true);
                        t = Typeob.Boolean;
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        break;

                    case JSToken.BitwiseNot:
                        il.Emit(OpCodes.Not);
                        break;

                    case JSToken.FirstBinaryOp:
                        break;

                    case JSToken.Minus:
                        il.Emit(OpCodes.Neg);
                        break;

                    default:
                        throw new JScriptException(JSError.InternalError, base.context);
                }
                Microsoft.JScript.Convert.Emit(this, il, t, rtype);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            IReflect reflect = this.InferType(null);
            base.operand.TranslateToILInitializer(il);
            if (reflect == Typeob.Object)
            {
                this.metaData = il.DeclareLocal(Typeob.NumericUnary);
                ConstantWrapper.TranslateToILInt(il, (int) this.operatorTok);
                il.Emit(OpCodes.Newobj, CompilerGlobals.numericUnaryConstructor);
                il.Emit(OpCodes.Stloc, (LocalBuilder) this.metaData);
            }
        }
    }
}

