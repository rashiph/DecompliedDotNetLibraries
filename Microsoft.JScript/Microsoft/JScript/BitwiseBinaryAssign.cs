namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class BitwiseBinaryAssign : BinaryOp
    {
        private BitwiseBinary binOp;
        private object metaData;

        internal BitwiseBinaryAssign(Context context, AST operand1, AST operand2, JSToken operatorTok) : base(context, operand1, operand2, operatorTok)
        {
            this.binOp = new BitwiseBinary(context, operand1, operand2, operatorTok);
            this.metaData = null;
        }

        internal override object Evaluate()
        {
            object obj5;
            object obj2 = base.operand1.Evaluate();
            object obj3 = base.operand2.Evaluate();
            object obj4 = this.binOp.EvaluateBitwiseBinary(obj2, obj3);
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
            if (base.type1 == null)
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
            if ((base.type1.IsPrimitive || Typeob.JSObject.IsAssignableFrom(base.type1)) && (base.type2.IsPrimitive || Typeob.JSObject.IsAssignableFrom(base.type2)))
            {
                return Typeob.Int32;
            }
            return Typeob.Object;
        }

        internal override AST PartiallyEvaluate()
        {
            base.operand1 = base.operand1.PartiallyEvaluateAsReference();
            base.operand2 = base.operand2.PartiallyEvaluate();
            this.binOp = new BitwiseBinary(base.context, base.operand1, base.operand2, base.operatorTok);
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
                il.Emit(OpCodes.Call, CompilerGlobals.evaluateBitwiseBinaryMethod);
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
            Type type = Microsoft.JScript.Convert.ToType(base.operand1.InferType(null));
            Type type2 = Microsoft.JScript.Convert.ToType(base.operand2.InferType(null));
            Type type3 = BitwiseBinary.ResultType(type, type2, base.operatorTok);
            base.operand1.TranslateToILPreSetPlusGet(il);
            Microsoft.JScript.Convert.Emit(this, il, type, type3, true);
            base.operand2.TranslateToIL(il, type2);
            Microsoft.JScript.Convert.Emit(this, il, type2, BitwiseBinary.Operand2Type(base.operatorTok, type3), true);
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
                    BitwiseBinary.TranslateToBitCountMask(il, type3, base.operand2);
                    il.Emit(OpCodes.Shl);
                    break;

                case JSToken.RightShift:
                    BitwiseBinary.TranslateToBitCountMask(il, type3, base.operand2);
                    il.Emit(OpCodes.Shr);
                    break;

                case JSToken.UnsignedRightShift:
                    BitwiseBinary.TranslateToBitCountMask(il, type3, base.operand2);
                    il.Emit(OpCodes.Shr_Un);
                    break;

                default:
                    throw new JScriptException(JSError.InternalError, base.context);
            }
            if (rtype != Typeob.Void)
            {
                LocalBuilder local = il.DeclareLocal(type3);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc, local);
                Microsoft.JScript.Convert.Emit(this, il, type3, type);
                base.operand1.TranslateToILSet(il);
                il.Emit(OpCodes.Ldloc, local);
                Microsoft.JScript.Convert.Emit(this, il, type3, rtype);
            }
            else
            {
                Microsoft.JScript.Convert.Emit(this, il, type3, type);
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
                this.metaData = il.DeclareLocal(Typeob.BitwiseBinary);
                ConstantWrapper.TranslateToILInt(il, (int) base.operatorTok);
                il.Emit(OpCodes.Newobj, CompilerGlobals.bitwiseBinaryConstructor);
                il.Emit(OpCodes.Stloc, (LocalBuilder) this.metaData);
            }
        }
    }
}

