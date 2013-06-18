namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    public class PostOrPrefixOperator : UnaryOp
    {
        private object metaData;
        private MethodInfo operatorMeth;
        private PostOrPrefix operatorTok;
        private Type type;

        public PostOrPrefixOperator(int operatorTok) : this(null, null, (PostOrPrefix) operatorTok)
        {
        }

        internal PostOrPrefixOperator(Context context, AST operand) : base(context, operand)
        {
        }

        internal PostOrPrefixOperator(Context context, AST operand, PostOrPrefix operatorTok) : base(context, operand)
        {
            this.operatorMeth = null;
            this.operatorTok = operatorTok;
            this.metaData = null;
            this.type = null;
        }

        private object DoOp(double d)
        {
            switch (this.operatorTok)
            {
                case PostOrPrefix.PostfixIncrement:
                case PostOrPrefix.PrefixIncrement:
                    return (d + 1.0);
            }
            return (d - 1.0);
        }

        private object DoOp(int i)
        {
            switch (this.operatorTok)
            {
                case PostOrPrefix.PostfixIncrement:
                case PostOrPrefix.PrefixIncrement:
                    if (i != 0x7fffffff)
                    {
                        return (i + 1);
                    }
                    return 2147483648;
            }
            if (i == -2147483648)
            {
                return -2147483649;
            }
            return (i - 1);
        }

        private object DoOp(long i)
        {
            switch (this.operatorTok)
            {
                case PostOrPrefix.PostfixIncrement:
                case PostOrPrefix.PrefixIncrement:
                    if (i != 0x7fffffffffffffffL)
                    {
                        return (i + 1L);
                    }
                    return 9.2233720368547758E+18;
            }
            if (i == -9223372036854775808L)
            {
                return -9.2233720368547758E+18;
            }
            return (i - 1L);
        }

        private object DoOp(uint i)
        {
            switch (this.operatorTok)
            {
                case PostOrPrefix.PostfixIncrement:
                case PostOrPrefix.PrefixIncrement:
                    if (i != uint.MaxValue)
                    {
                        return (i + 1);
                    }
                    return 4294967296;
            }
            if (i == 0)
            {
                return -1.0;
            }
            return (i - 1);
        }

        private object DoOp(ulong i)
        {
            switch (this.operatorTok)
            {
                case PostOrPrefix.PostfixIncrement:
                case PostOrPrefix.PrefixIncrement:
                    if (i != ulong.MaxValue)
                    {
                        return (i + ((ulong) 1L));
                    }
                    return 1.8446744073709552E+19;
            }
            if (i == 0L)
            {
                return -1.0;
            }
            return (i - ((ulong) 1L));
        }

        internal override object Evaluate()
        {
            object obj4;
            try
            {
                object v = base.operand.Evaluate();
                object obj3 = this.EvaluatePostOrPrefix(ref v);
                base.operand.SetValue(obj3);
                switch (this.operatorTok)
                {
                    case PostOrPrefix.PostfixDecrement:
                    case PostOrPrefix.PostfixIncrement:
                        return v;

                    case PostOrPrefix.PrefixDecrement:
                    case PostOrPrefix.PrefixIncrement:
                        return obj3;
                }
                throw new JScriptException(JSError.InternalError, base.context);
            }
            catch (JScriptException exception)
            {
                if (exception.context == null)
                {
                    exception.context = base.context;
                }
                throw exception;
            }
            catch (Exception exception2)
            {
                throw new JScriptException(exception2, base.context);
            }
            return obj4;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object EvaluatePostOrPrefix(ref object v)
        {
            int num;
            double num5;
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(v);
            switch (Microsoft.JScript.Convert.GetTypeCode(v, iConvertible))
            {
                case TypeCode.Empty:
                    v = (double) 1.0 / (double) 0.0;
                    return v;

                case TypeCode.DBNull:
                    v = 0;
                    return this.DoOp(0);

                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    v = num = iConvertible.ToInt32(null);
                    return this.DoOp(num);

                case TypeCode.Char:
                    num = iConvertible.ToInt32(null);
                    return ((IConvertible) this.DoOp(num)).ToChar(null);

                case TypeCode.UInt32:
                    uint num2;
                    v = num2 = iConvertible.ToUInt32(null);
                    return this.DoOp(num2);

                case TypeCode.Int64:
                    long num3;
                    v = num3 = iConvertible.ToInt64(null);
                    return this.DoOp(num3);

                case TypeCode.UInt64:
                    ulong num4;
                    v = num4 = iConvertible.ToUInt64(null);
                    return this.DoOp(num4);

                case TypeCode.Single:
                case TypeCode.Double:
                    v = num5 = iConvertible.ToDouble(null);
                    return this.DoOp(num5);
            }
            MethodInfo @operator = this.GetOperator(v.GetType());
            if (@operator != null)
            {
                return @operator.Invoke(null, BindingFlags.Default, JSBinder.ob, new object[] { v }, null);
            }
            v = num5 = Microsoft.JScript.Convert.ToNumber(v, iConvertible);
            return this.DoOp(num5);
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
                    case PostOrPrefix.PostfixDecrement:
                    case PostOrPrefix.PrefixDecrement:
                        this.operatorMeth = type.GetMethod("op_Decrement", BindingFlags.Public | BindingFlags.Static, JSBinder.ob, new Type[] { type }, null);
                        break;

                    case PostOrPrefix.PostfixIncrement:
                    case PostOrPrefix.PrefixIncrement:
                        this.operatorMeth = type.GetMethod("op_Increment", BindingFlags.Public | BindingFlags.Static, JSBinder.ob, new Type[] { type }, null);
                        break;

                    default:
                        throw new JScriptException(JSError.InternalError, base.context);
                }
                if ((this.operatorMeth != null) && ((!this.operatorMeth.IsStatic || ((this.operatorMeth.Attributes & MethodAttributes.SpecialName) == MethodAttributes.PrivateScope)) || (this.operatorMeth.GetParameters().Length != 1)))
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
            if (Microsoft.JScript.Convert.IsPrimitiveNumericType(this.type))
            {
                return this.type;
            }
            if (this.type == Typeob.Char)
            {
                return this.type;
            }
            if (Typeob.JSObject.IsAssignableFrom(this.type))
            {
                return Typeob.Double;
            }
            return Typeob.Object;
        }

        internal override AST PartiallyEvaluate()
        {
            base.operand = base.operand.PartiallyEvaluateAsReference();
            base.operand.SetPartialValue(this);
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (this.metaData == null)
            {
                this.TranslateToILForNoOverloadCase(il, rtype);
            }
            else if (this.metaData is MethodInfo)
            {
                object obj2 = null;
                Type type = Microsoft.JScript.Convert.ToType(base.operand.InferType(null));
                base.operand.TranslateToILPreSetPlusGet(il);
                if (rtype != Typeob.Void)
                {
                    obj2 = il.DeclareLocal(rtype);
                    if ((this.operatorTok == PostOrPrefix.PostfixDecrement) || (this.operatorTok == PostOrPrefix.PostfixIncrement))
                    {
                        il.Emit(OpCodes.Dup);
                        Microsoft.JScript.Convert.Emit(this, il, type, rtype);
                        il.Emit(OpCodes.Stloc, (LocalBuilder) obj2);
                    }
                }
                MethodInfo metaData = (MethodInfo) this.metaData;
                ParameterInfo[] parameters = metaData.GetParameters();
                Microsoft.JScript.Convert.Emit(this, il, type, parameters[0].ParameterType);
                il.Emit(OpCodes.Call, metaData);
                if ((rtype != Typeob.Void) && ((this.operatorTok == PostOrPrefix.PrefixDecrement) || (this.operatorTok == PostOrPrefix.PrefixIncrement)))
                {
                    il.Emit(OpCodes.Dup);
                    Microsoft.JScript.Convert.Emit(this, il, type, rtype);
                    il.Emit(OpCodes.Stloc, (LocalBuilder) obj2);
                }
                Microsoft.JScript.Convert.Emit(this, il, metaData.ReturnType, type);
                base.operand.TranslateToILSet(il);
                if (rtype != Typeob.Void)
                {
                    il.Emit(OpCodes.Ldloc, (LocalBuilder) obj2);
                }
            }
            else
            {
                Type type2 = Microsoft.JScript.Convert.ToType(base.operand.InferType(null));
                LocalBuilder local = il.DeclareLocal(Typeob.Object);
                base.operand.TranslateToILPreSetPlusGet(il);
                Microsoft.JScript.Convert.Emit(this, il, type2, Typeob.Object);
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldloc, (LocalBuilder) this.metaData);
                il.Emit(OpCodes.Ldloca, local);
                il.Emit(OpCodes.Call, CompilerGlobals.evaluatePostOrPrefixOperatorMethod);
                if ((rtype != Typeob.Void) && ((this.operatorTok == PostOrPrefix.PrefixDecrement) || (this.operatorTok == PostOrPrefix.PrefixIncrement)))
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Stloc, local);
                }
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, type2);
                base.operand.TranslateToILSet(il);
                if (rtype != Typeob.Void)
                {
                    il.Emit(OpCodes.Ldloc, local);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                }
            }
        }

        private void TranslateToILForNoOverloadCase(ILGenerator il, Type rtype)
        {
            Type ir = Microsoft.JScript.Convert.ToType(base.operand.InferType(null));
            base.operand.TranslateToILPreSetPlusGet(il);
            if (rtype == Typeob.Void)
            {
                Type type2 = Typeob.Double;
                if (Microsoft.JScript.Convert.IsPrimitiveNumericType(ir))
                {
                    if ((ir == Typeob.SByte) || (ir == Typeob.Int16))
                    {
                        type2 = Typeob.Int32;
                    }
                    else if (((ir == Typeob.Byte) || (ir == Typeob.UInt16)) || (ir == Typeob.Char))
                    {
                        type2 = Typeob.UInt32;
                    }
                    else
                    {
                        type2 = ir;
                    }
                }
                Microsoft.JScript.Convert.Emit(this, il, ir, type2);
                il.Emit(OpCodes.Ldc_I4_1);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Int32, type2);
                if ((type2 == Typeob.Double) || (type2 == Typeob.Single))
                {
                    if ((this.operatorTok == PostOrPrefix.PostfixDecrement) || (this.operatorTok == PostOrPrefix.PrefixDecrement))
                    {
                        il.Emit(OpCodes.Sub);
                    }
                    else
                    {
                        il.Emit(OpCodes.Add);
                    }
                }
                else if ((type2 == Typeob.Int32) || (type2 == Typeob.Int64))
                {
                    if ((this.operatorTok == PostOrPrefix.PostfixDecrement) || (this.operatorTok == PostOrPrefix.PrefixDecrement))
                    {
                        il.Emit(OpCodes.Sub_Ovf);
                    }
                    else
                    {
                        il.Emit(OpCodes.Add_Ovf);
                    }
                }
                else if ((this.operatorTok == PostOrPrefix.PostfixDecrement) || (this.operatorTok == PostOrPrefix.PrefixDecrement))
                {
                    il.Emit(OpCodes.Sub_Ovf_Un);
                }
                else
                {
                    il.Emit(OpCodes.Add_Ovf_Un);
                }
                Microsoft.JScript.Convert.Emit(this, il, type2, ir);
                base.operand.TranslateToILSet(il);
            }
            else
            {
                Type type3 = Typeob.Double;
                if (Microsoft.JScript.Convert.IsPrimitiveNumericType(rtype) && Microsoft.JScript.Convert.IsPromotableTo((IReflect) ir, (IReflect) rtype))
                {
                    type3 = rtype;
                }
                else if (Microsoft.JScript.Convert.IsPrimitiveNumericType(ir) && Microsoft.JScript.Convert.IsPromotableTo((IReflect) rtype, (IReflect) ir))
                {
                    type3 = ir;
                }
                if ((type3 == Typeob.SByte) || (type3 == Typeob.Int16))
                {
                    type3 = Typeob.Int32;
                }
                else if (((type3 == Typeob.Byte) || (type3 == Typeob.UInt16)) || (type3 == Typeob.Char))
                {
                    type3 = Typeob.UInt32;
                }
                LocalBuilder local = il.DeclareLocal(rtype);
                Microsoft.JScript.Convert.Emit(this, il, ir, type3);
                if (this.operatorTok == PostOrPrefix.PostfixDecrement)
                {
                    il.Emit(OpCodes.Dup);
                    if (ir == Typeob.Char)
                    {
                        Microsoft.JScript.Convert.Emit(this, il, type3, Typeob.Char);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Char, rtype);
                    }
                    else
                    {
                        Microsoft.JScript.Convert.Emit(this, il, type3, rtype);
                    }
                    il.Emit(OpCodes.Stloc, local);
                    il.Emit(OpCodes.Ldc_I4_1);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Int32, type3);
                    if ((type3 == Typeob.Double) || (type3 == Typeob.Single))
                    {
                        il.Emit(OpCodes.Sub);
                    }
                    else if ((type3 == Typeob.Int32) || (type3 == Typeob.Int64))
                    {
                        il.Emit(OpCodes.Sub_Ovf);
                    }
                    else
                    {
                        il.Emit(OpCodes.Sub_Ovf_Un);
                    }
                }
                else if (this.operatorTok == PostOrPrefix.PostfixIncrement)
                {
                    il.Emit(OpCodes.Dup);
                    if (ir == Typeob.Char)
                    {
                        Microsoft.JScript.Convert.Emit(this, il, type3, Typeob.Char);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Char, rtype);
                    }
                    else
                    {
                        Microsoft.JScript.Convert.Emit(this, il, type3, rtype);
                    }
                    il.Emit(OpCodes.Stloc, local);
                    il.Emit(OpCodes.Ldc_I4_1);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Int32, type3);
                    if ((type3 == Typeob.Double) || (type3 == Typeob.Single))
                    {
                        il.Emit(OpCodes.Add);
                    }
                    else if ((type3 == Typeob.Int32) || (type3 == Typeob.Int64))
                    {
                        il.Emit(OpCodes.Add_Ovf);
                    }
                    else
                    {
                        il.Emit(OpCodes.Add_Ovf_Un);
                    }
                }
                else if (this.operatorTok == PostOrPrefix.PrefixDecrement)
                {
                    il.Emit(OpCodes.Ldc_I4_1);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Int32, type3);
                    if ((type3 == Typeob.Double) || (type3 == Typeob.Single))
                    {
                        il.Emit(OpCodes.Sub);
                    }
                    else if ((type3 == Typeob.Int32) || (type3 == Typeob.Int64))
                    {
                        il.Emit(OpCodes.Sub_Ovf);
                    }
                    else
                    {
                        il.Emit(OpCodes.Sub_Ovf_Un);
                    }
                    il.Emit(OpCodes.Dup);
                    if (ir == Typeob.Char)
                    {
                        Microsoft.JScript.Convert.Emit(this, il, type3, Typeob.Char);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Char, rtype);
                    }
                    else
                    {
                        Microsoft.JScript.Convert.Emit(this, il, type3, rtype);
                    }
                    il.Emit(OpCodes.Stloc, local);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I4_1);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Int32, type3);
                    if ((type3 == Typeob.Double) || (type3 == Typeob.Single))
                    {
                        il.Emit(OpCodes.Add);
                    }
                    else if ((type3 == Typeob.Int32) || (type3 == Typeob.Int64))
                    {
                        il.Emit(OpCodes.Add_Ovf);
                    }
                    else
                    {
                        il.Emit(OpCodes.Add_Ovf_Un);
                    }
                    il.Emit(OpCodes.Dup);
                    if (ir == Typeob.Char)
                    {
                        Microsoft.JScript.Convert.Emit(this, il, type3, Typeob.Char);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Char, rtype);
                    }
                    else
                    {
                        Microsoft.JScript.Convert.Emit(this, il, type3, rtype);
                    }
                    il.Emit(OpCodes.Stloc, local);
                }
                Microsoft.JScript.Convert.Emit(this, il, type3, ir);
                base.operand.TranslateToILSet(il);
                il.Emit(OpCodes.Ldloc, local);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            IReflect reflect = this.InferType(null);
            base.operand.TranslateToILInitializer(il);
            if (reflect == Typeob.Object)
            {
                this.metaData = il.DeclareLocal(Typeob.PostOrPrefixOperator);
                ConstantWrapper.TranslateToILInt(il, (int) this.operatorTok);
                il.Emit(OpCodes.Newobj, CompilerGlobals.postOrPrefixConstructor);
                il.Emit(OpCodes.Stloc, (LocalBuilder) this.metaData);
            }
        }
    }
}

