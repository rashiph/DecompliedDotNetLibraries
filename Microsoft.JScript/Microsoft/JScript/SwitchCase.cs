namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class SwitchCase : AST
    {
        private AST case_value;
        private Completion completion;
        private AST statements;

        internal SwitchCase(Context context, AST statements) : this(context, null, statements)
        {
        }

        internal SwitchCase(Context context, AST case_value, AST statements) : base(context)
        {
            this.case_value = case_value;
            this.statements = statements;
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            return this.statements.Evaluate();
        }

        internal Completion Evaluate(object expression)
        {
            if (StrictEquality.JScriptStrictEquals(this.case_value.Evaluate(), expression))
            {
                return (Completion) this.statements.Evaluate();
            }
            return null;
        }

        internal bool IsDefault()
        {
            return (this.case_value == null);
        }

        internal override AST PartiallyEvaluate()
        {
            if (this.case_value != null)
            {
                this.case_value = this.case_value.PartiallyEvaluate();
            }
            this.statements = this.statements.PartiallyEvaluate();
            return this;
        }

        internal void TranslateToConditionalBranch(ILGenerator il, Type etype, bool branchIfTrue, Label label, bool shortForm)
        {
            Type type = etype;
            Type single = Microsoft.JScript.Convert.ToType(this.case_value.InferType(null));
            if (((type != single) && type.IsPrimitive) && single.IsPrimitive)
            {
                if ((type == Typeob.Single) && (single == Typeob.Double))
                {
                    single = Typeob.Single;
                }
                else if (Microsoft.JScript.Convert.IsPromotableTo((IReflect) single, (IReflect) type))
                {
                    single = type;
                }
                else if (Microsoft.JScript.Convert.IsPromotableTo((IReflect) type, (IReflect) single))
                {
                    type = single;
                }
            }
            bool flag = true;
            if ((type == single) && (type != Typeob.Object))
            {
                Microsoft.JScript.Convert.Emit(this, il, etype, type);
                if (!type.IsPrimitive && type.IsValueType)
                {
                    il.Emit(OpCodes.Box, type);
                }
                this.case_value.context.EmitLineInfo(il);
                this.case_value.TranslateToIL(il, type);
                if (type == Typeob.String)
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.stringEqualsMethod);
                }
                else if (!type.IsPrimitive)
                {
                    if (type.IsValueType)
                    {
                        il.Emit(OpCodes.Box, type);
                    }
                    il.Emit(OpCodes.Callvirt, CompilerGlobals.equalsMethod);
                }
                else
                {
                    flag = false;
                }
            }
            else
            {
                Microsoft.JScript.Convert.Emit(this, il, etype, Typeob.Object);
                this.case_value.context.EmitLineInfo(il);
                this.case_value.TranslateToIL(il, Typeob.Object);
                il.Emit(OpCodes.Call, CompilerGlobals.jScriptStrictEqualsMethod);
            }
            if (branchIfTrue)
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

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            this.statements.TranslateToIL(il, Typeob.Void);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            if (this.case_value != null)
            {
                this.case_value.TranslateToILInitializer(il);
            }
            this.statements.TranslateToILInitializer(il);
        }
    }
}

