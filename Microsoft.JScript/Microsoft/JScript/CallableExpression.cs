namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class CallableExpression : Binding
    {
        internal AST expression;
        private IReflect expressionInferredType;

        internal CallableExpression(AST expression) : base(expression.context, "")
        {
            this.expression = expression;
            JSLocalField field = new JSLocalField("", null, 0, Microsoft.JScript.Missing.Value);
            this.expressionInferredType = expression.InferType(field);
            field.inferred_type = this.expressionInferredType;
            base.member = field;
            base.members = new MemberInfo[] { field };
        }

        internal override LateBinding EvaluateAsLateBinding()
        {
            return new LateBinding(null, this.expression.Evaluate(), VsaEngine.executeForJSEE);
        }

        protected override object GetObject()
        {
            return this.GetObject2();
        }

        internal object GetObject2()
        {
            Call expression = this.expression as Call;
            if ((expression != null) && expression.inBrackets)
            {
                return Microsoft.JScript.Convert.ToObject(expression.func.Evaluate(), base.Engine);
            }
            return Microsoft.JScript.Convert.ToObject(this.expression.Evaluate(), base.Engine);
        }

        protected override void HandleNoSuchMemberError()
        {
            throw new JScriptException(JSError.InternalError, base.context);
        }

        internal override AST PartiallyEvaluate()
        {
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            this.expression.TranslateToIL(il, rtype);
        }

        internal override void TranslateToILCall(ILGenerator il, Type rtype, ASTList argList, bool construct, bool brackets)
        {
            if (((base.defaultMember != null) && construct) && brackets)
            {
                base.TranslateToILCall(il, rtype, argList, construct, brackets);
            }
            else
            {
                JSGlobalField member = base.member as JSGlobalField;
                if (((member != null) && member.IsLiteral) && (argList.count == 1))
                {
                    Type type = Microsoft.JScript.Convert.ToType((IReflect) member.value);
                    argList[0].TranslateToIL(il, type);
                    Microsoft.JScript.Convert.Emit(this, il, type, rtype);
                }
                else
                {
                    this.TranslateToILWithDupOfThisOb(il);
                    argList.TranslateToIL(il, Typeob.ArrayOfObject);
                    if (construct)
                    {
                        il.Emit(OpCodes.Ldc_I4_1);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                    }
                    if (brackets)
                    {
                        il.Emit(OpCodes.Ldc_I4_1);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                    }
                    base.EmitILToLoadEngine(il);
                    il.Emit(OpCodes.Call, CompilerGlobals.callValueMethod);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                }
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.expression.TranslateToILInitializer(il);
            if (!this.expressionInferredType.Equals(this.expression.InferType(null)))
            {
                MemberInfo[] members = base.members;
                base.InvalidateBinding();
                base.members = members;
            }
        }

        protected override void TranslateToILObject(ILGenerator il, Type obType, bool noValue)
        {
            base.EmitILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
            il.Emit(OpCodes.Castclass, Typeob.IActivationObject);
            il.Emit(OpCodes.Callvirt, CompilerGlobals.getGlobalScopeMethod);
        }

        protected override void TranslateToILWithDupOfThisOb(ILGenerator il)
        {
            Call expression = this.expression as Call;
            if ((expression == null) || !expression.inBrackets)
            {
                this.TranslateToILObject(il, null, false);
            }
            else
            {
                if (expression.isConstructor && expression.inBrackets)
                {
                    expression.TranslateToIL(il, Typeob.Object);
                    il.Emit(OpCodes.Dup);
                    return;
                }
                expression.func.TranslateToIL(il, Typeob.Object);
            }
            this.expression.TranslateToIL(il, Typeob.Object);
        }
    }
}

