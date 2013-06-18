namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class NumericBinaryAssign : BinaryOp
    {
        private NumericBinary binOp;
        private object metaData;

        internal NumericBinaryAssign(Context context, AST operand1, AST operand2, JSToken operatorTok) : base(context, operand1, operand2, operatorTok)
        {
            this.binOp = new NumericBinary(context, operand1, operand2, operatorTok);
            this.metaData = null;
        }

        internal override object Evaluate()
        {
            object obj5;
            object obj2 = base.operand1.Evaluate();
            object obj3 = base.operand2.Evaluate();
            object obj4 = this.binOp.EvaluateNumericBinary(obj2, obj3);
            try
            {
                base.operand1.SetValue(obj4);
                obj5 = obj4;
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
            return obj5;
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
            if (Microsoft.JScript.Convert.IsPrimitiveNumericType(base.type1))
            {
                if (Microsoft.JScript.Convert.IsPromotableTo((IReflect) base.type2, (IReflect) base.type1) || ((base.operand2 is ConstantWrapper) && ((ConstantWrapper) base.operand2).IsAssignableTo(base.type1)))
                {
                    return base.type1;
                }
                if (Microsoft.JScript.Convert.IsPrimitiveNumericType(base.type1) && Microsoft.JScript.Convert.IsPrimitiveNumericTypeFitForDouble(base.type2))
                {
                    return Typeob.Double;
                }
            }
            return Typeob.Object;
        }

        internal override AST PartiallyEvaluate()
        {
            base.operand1 = base.operand1.PartiallyEvaluateAsReference();
            base.operand2 = base.operand2.PartiallyEvaluate();
            this.binOp = new NumericBinary(base.context, base.operand1, base.operand2, base.operatorTok);
            base.operand1.SetPartialValue(this.binOp);
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
                MethodInfo metaData = (MethodInfo) this.metaData;
                Type type = Microsoft.JScript.Convert.ToType(base.operand1.InferType(null));
                ParameterInfo[] parameters = metaData.GetParameters();
                base.operand1.TranslateToILPreSetPlusGet(il);
                Microsoft.JScript.Convert.Emit(this, il, type, parameters[0].ParameterType);
                base.operand2.TranslateToIL(il, parameters[1].ParameterType);
                il.Emit(OpCodes.Call, metaData);
                if (rtype != Typeob.Void)
                {
                    obj2 = il.DeclareLocal(rtype);
                    il.Emit(OpCodes.Dup);
                    Microsoft.JScript.Convert.Emit(this, il, type, rtype);
                    il.Emit(OpCodes.Stloc, (LocalBuilder) obj2);
                }
                Microsoft.JScript.Convert.Emit(this, il, metaData.ReturnType, type);
                base.operand1.TranslateToILSet(il);
                if (rtype != Typeob.Void)
                {
                    il.Emit(OpCodes.Ldloc, (LocalBuilder) obj2);
                }
            }
            else
            {
                Type type2 = Microsoft.JScript.Convert.ToType(base.operand1.InferType(null));
                LocalBuilder local = il.DeclareLocal(Typeob.Object);
                base.operand1.TranslateToILPreSetPlusGet(il);
                Microsoft.JScript.Convert.Emit(this, il, type2, Typeob.Object);
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldloc, (LocalBuilder) this.metaData);
                il.Emit(OpCodes.Ldloc, local);
                base.operand2.TranslateToIL(il, Typeob.Object);
                il.Emit(OpCodes.Call, CompilerGlobals.evaluateNumericBinaryMethod);
                if (rtype != Typeob.Void)
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Stloc, local);
                }
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, type2);
                base.operand1.TranslateToILSet(il);
                if (rtype != Typeob.Void)
                {
                    il.Emit(OpCodes.Ldloc, local);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                }
            }
        }

        private void TranslateToILForNoOverloadCase(ILGenerator il, Type rtype)
        {
            Type ir = Microsoft.JScript.Convert.ToType(base.operand1.InferType(null));
            Type t = Microsoft.JScript.Convert.ToType(base.operand2.InferType(null));
            Type type3 = Typeob.Double;
            if (((base.operatorTok != JSToken.Divide) && (((rtype == Typeob.Void) || (rtype == ir)) || Microsoft.JScript.Convert.IsPrimitiveNumericType(ir))) && (Microsoft.JScript.Convert.IsPromotableTo((IReflect) t, (IReflect) ir) || ((base.operand2 is ConstantWrapper) && ((ConstantWrapper) base.operand2).IsAssignableTo(ir))))
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
            if (base.operand2 is ConstantWrapper)
            {
                if (!((ConstantWrapper) base.operand2).IsAssignableTo(type3))
                {
                    type3 = Typeob.Object;
                }
            }
            else if ((Microsoft.JScript.Convert.IsPrimitiveSignedNumericType(t) && Microsoft.JScript.Convert.IsPrimitiveUnsignedIntegerType(ir)) || (Microsoft.JScript.Convert.IsPrimitiveUnsignedIntegerType(t) && Microsoft.JScript.Convert.IsPrimitiveSignedIntegerType(ir)))
            {
                type3 = Typeob.Object;
            }
            base.operand1.TranslateToILPreSetPlusGet(il);
            Microsoft.JScript.Convert.Emit(this, il, ir, type3);
            base.operand2.TranslateToIL(il, type3);
            if (type3 == Typeob.Object)
            {
                il.Emit(OpCodes.Ldc_I4, (int) base.operatorTok);
                il.Emit(OpCodes.Call, CompilerGlobals.numericbinaryDoOpMethod);
            }
            else
            {
                if ((type3 == Typeob.Double) || (type3 == Typeob.Single))
                {
                    switch (base.operatorTok)
                    {
                        case JSToken.Multiply:
                            il.Emit(OpCodes.Mul);
                            goto Label_030A;

                        case JSToken.Divide:
                            il.Emit(OpCodes.Div);
                            goto Label_030A;

                        case JSToken.Modulo:
                            il.Emit(OpCodes.Rem);
                            goto Label_030A;

                        case JSToken.Minus:
                            il.Emit(OpCodes.Sub);
                            goto Label_030A;
                    }
                    throw new JScriptException(JSError.InternalError, base.context);
                }
                if (((type3 == Typeob.Int32) || (type3 == Typeob.Int64)) || ((type3 == Typeob.Int16) || (type3 == Typeob.SByte)))
                {
                    switch (base.operatorTok)
                    {
                        case JSToken.Multiply:
                            il.Emit(OpCodes.Mul_Ovf);
                            goto Label_030A;

                        case JSToken.Divide:
                            il.Emit(OpCodes.Div);
                            goto Label_030A;

                        case JSToken.Modulo:
                            il.Emit(OpCodes.Rem);
                            goto Label_030A;

                        case JSToken.Minus:
                            il.Emit(OpCodes.Sub_Ovf);
                            goto Label_030A;
                    }
                    throw new JScriptException(JSError.InternalError, base.context);
                }
                switch (base.operatorTok)
                {
                    case JSToken.Multiply:
                        il.Emit(OpCodes.Mul_Ovf_Un);
                        goto Label_030A;

                    case JSToken.Divide:
                        il.Emit(OpCodes.Div);
                        goto Label_030A;

                    case JSToken.Modulo:
                        il.Emit(OpCodes.Rem);
                        goto Label_030A;

                    case JSToken.Minus:
                        il.Emit(OpCodes.Sub_Ovf_Un);
                        goto Label_030A;
                }
                throw new JScriptException(JSError.InternalError, base.context);
            }
        Label_030A:
            if (rtype != Typeob.Void)
            {
                LocalBuilder local = il.DeclareLocal(type3);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc, local);
                Microsoft.JScript.Convert.Emit(this, il, type3, ir);
                base.operand1.TranslateToILSet(il);
                il.Emit(OpCodes.Ldloc, local);
                Microsoft.JScript.Convert.Emit(this, il, type3, rtype);
            }
            else
            {
                Microsoft.JScript.Convert.Emit(this, il, type3, ir);
                base.operand1.TranslateToILSet(il);
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

