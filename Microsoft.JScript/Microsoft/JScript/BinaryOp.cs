namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public abstract class BinaryOp : AST
    {
        protected AST operand1;
        protected AST operand2;
        protected MethodInfo operatorMeth;
        protected JSToken operatorTok;
        protected Type type1;
        protected Type type2;

        internal BinaryOp(Context context, AST operand1, AST operand2) : this(context, operand1, operand2, JSToken.EndOfFile)
        {
        }

        internal BinaryOp(Context context, AST operand1, AST operand2, JSToken operatorTok) : base(context)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
            this.operatorTok = operatorTok;
            this.type1 = null;
            this.type2 = null;
            this.operatorMeth = null;
        }

        internal override void CheckIfOKToUseInSuperConstructorCall()
        {
            this.operand1.CheckIfOKToUseInSuperConstructorCall();
            this.operand2.CheckIfOKToUseInSuperConstructorCall();
        }

        protected MethodInfo GetOperator(IReflect ir1, IReflect ir2)
        {
            if (ir1 is ClassScope)
            {
                ir1 = ((ClassScope) ir1).GetUnderlyingTypeIfEnum();
            }
            if (ir2 is ClassScope)
            {
                ir2 = ((ClassScope) ir2).GetUnderlyingTypeIfEnum();
            }
            Type c = (ir1 is Type) ? ((Type) ir1) : Typeob.Object;
            Type type2 = (ir2 is Type) ? ((Type) ir2) : Typeob.Object;
            if ((this.type1 != c) || (this.type2 != type2))
            {
                this.type1 = c;
                this.type2 = type2;
                this.operatorMeth = null;
                if (((c == Typeob.String) || Microsoft.JScript.Convert.IsPrimitiveNumericType(ir1)) || Typeob.JSObject.IsAssignableFrom(c))
                {
                    c = null;
                }
                if (((type2 == Typeob.String) || Microsoft.JScript.Convert.IsPrimitiveNumericType(ir2)) || Typeob.JSObject.IsAssignableFrom(type2))
                {
                    type2 = null;
                }
                if ((c == null) && (type2 == null))
                {
                    return null;
                }
                string name = "op_NoSuchOp";
                switch (this.operatorTok)
                {
                    case JSToken.FirstBinaryOp:
                        name = "op_Addition";
                        break;

                    case JSToken.Minus:
                        name = "op_Subtraction";
                        break;

                    case JSToken.BitwiseOr:
                        name = "op_BitwiseOr";
                        break;

                    case JSToken.BitwiseXor:
                        name = "op_ExclusiveOr";
                        break;

                    case JSToken.BitwiseAnd:
                        name = "op_BitwiseAnd";
                        break;

                    case JSToken.Equal:
                        name = "op_Equality";
                        break;

                    case JSToken.NotEqual:
                        name = "op_Inequality";
                        break;

                    case JSToken.GreaterThan:
                        name = "op_GreaterThan";
                        break;

                    case JSToken.LessThan:
                        name = "op_LessThan";
                        break;

                    case JSToken.LessThanEqual:
                        name = "op_LessThanOrEqual";
                        break;

                    case JSToken.GreaterThanEqual:
                        name = "op_GreaterThanOrEqual";
                        break;

                    case JSToken.LeftShift:
                        name = "op_LeftShift";
                        break;

                    case JSToken.RightShift:
                        name = "op_RightShift";
                        break;

                    case JSToken.Multiply:
                        name = "op_Multiply";
                        break;

                    case JSToken.Divide:
                        name = "op_Division";
                        break;

                    case JSToken.Modulo:
                        name = "op_Modulus";
                        break;
                }
                Type[] types = new Type[] { this.type1, this.type2 };
                if (c == type2)
                {
                    MethodInfo info = c.GetMethod(name, BindingFlags.Public | BindingFlags.Static, JSBinder.ob, types, null);
                    if (((info != null) && ((info.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope)) && (info.GetParameters().Length == 2))
                    {
                        this.operatorMeth = info;
                    }
                }
                else
                {
                    MethodInfo info2 = (c == null) ? null : c.GetMethod(name, BindingFlags.Public | BindingFlags.Static, JSBinder.ob, types, null);
                    MethodInfo info3 = (type2 == null) ? null : type2.GetMethod(name, BindingFlags.Public | BindingFlags.Static, JSBinder.ob, types, null);
                    this.operatorMeth = JSBinder.SelectOperator(info2, info3, this.type1, this.type2);
                }
                if (this.operatorMeth != null)
                {
                    this.operatorMeth = new JSMethodInfo(this.operatorMeth);
                }
            }
            return this.operatorMeth;
        }

        internal override AST PartiallyEvaluate()
        {
            this.operand1 = this.operand1.PartiallyEvaluate();
            this.operand2 = this.operand2.PartiallyEvaluate();
            try
            {
                if (this.operand1 is ConstantWrapper)
                {
                    if (this.operand2 is ConstantWrapper)
                    {
                        return new ConstantWrapper(this.Evaluate(), base.context);
                    }
                    object obj2 = ((ConstantWrapper) this.operand1).value;
                    if (((obj2 is string) && (((string) obj2).Length == 1)) && (this.operand2.InferType(null) == Typeob.Char))
                    {
                        ((ConstantWrapper) this.operand1).value = ((string) obj2)[0];
                    }
                }
                else if (this.operand2 is ConstantWrapper)
                {
                    object obj3 = ((ConstantWrapper) this.operand2).value;
                    if (((obj3 is string) && (((string) obj3).Length == 1)) && (this.operand1.InferType(null) == Typeob.Char))
                    {
                        ((ConstantWrapper) this.operand2).value = ((string) obj3)[0];
                    }
                }
            }
            catch (JScriptException exception)
            {
                base.context.HandleError(((JSError) exception.ErrorNumber) & ((JSError) 0xffff));
            }
            catch
            {
                base.context.HandleError(JSError.TypeMismatch);
            }
            return this;
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.operand1.TranslateToILInitializer(il);
            this.operand2.TranslateToILInitializer(il);
        }
    }
}

