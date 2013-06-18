namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection.Emit;

    internal sealed class Switch : AST
    {
        private ASTList cases;
        private Completion completion;
        private int default_case;
        private AST expression;

        internal Switch(Context context, AST expression, ASTList cases) : base(context)
        {
            this.expression = expression;
            this.cases = cases;
            this.default_case = -1;
            int num = 0;
            int count = this.cases.count;
            while (num < count)
            {
                if (((SwitchCase) this.cases[num]).IsDefault())
                {
                    this.default_case = num;
                    break;
                }
                num++;
            }
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            this.completion.Continue = 0;
            this.completion.Exit = 0;
            this.completion.value = null;
            object expression = this.expression.Evaluate();
            Completion completion = null;
            int count = this.cases.count;
            int num = 0;
            while (num < count)
            {
                if (num != this.default_case)
                {
                    completion = ((SwitchCase) this.cases[num]).Evaluate(expression);
                    if (completion != null)
                    {
                        break;
                    }
                }
                num++;
            }
            if (completion == null)
            {
                if (this.default_case < 0)
                {
                    return this.completion;
                }
                num = this.default_case;
                completion = (Completion) ((SwitchCase) this.cases[num]).Evaluate();
            }
        Label_00A6:
            if (completion.value != null)
            {
                this.completion.value = completion.value;
            }
            if (completion.Continue > 0)
            {
                this.completion.Continue = completion.Continue - 1;
            }
            else if (completion.Exit > 0)
            {
                this.completion.Exit = completion.Exit - 1;
            }
            else
            {
                if (completion.Return)
                {
                    return completion;
                }
                if (num >= (count - 1))
                {
                    return this.completion;
                }
                completion = (Completion) ((SwitchCase) this.cases[++num]).Evaluate();
                goto Label_00A6;
            }
            return this.completion;
        }

        internal override Context GetFirstExecutableContext()
        {
            return this.expression.context;
        }

        internal override AST PartiallyEvaluate()
        {
            this.expression = this.expression.PartiallyEvaluate();
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            if (parent is FunctionScope)
            {
                FunctionScope scope = (FunctionScope) parent;
                BitArray definedFlags = scope.DefinedFlags;
                int num = 0;
                int count = this.cases.count;
                while (num < count)
                {
                    this.cases[num] = this.cases[num].PartiallyEvaluate();
                    scope.DefinedFlags = definedFlags;
                    num++;
                }
            }
            else
            {
                int num3 = 0;
                int num4 = this.cases.count;
                while (num3 < num4)
                {
                    this.cases[num3] = this.cases[num3].PartiallyEvaluate();
                    num3++;
                }
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Type type = Microsoft.JScript.Convert.ToType(this.expression.InferType(null));
            this.expression.context.EmitLineInfo(il);
            this.expression.TranslateToIL(il, type);
            LocalBuilder local = il.DeclareLocal(type);
            il.Emit(OpCodes.Stloc, local);
            int count = this.cases.count;
            Label[] labelArray = new Label[this.cases.count];
            for (int i = 0; i < count; i++)
            {
                labelArray[i] = il.DefineLabel();
                if (i != this.default_case)
                {
                    il.Emit(OpCodes.Ldloc, local);
                    ((SwitchCase) this.cases[i]).TranslateToConditionalBranch(il, type, true, labelArray[i], false);
                }
            }
            Label label = il.DefineLabel();
            if (this.default_case >= 0)
            {
                il.Emit(OpCodes.Br, labelArray[this.default_case]);
            }
            else
            {
                il.Emit(OpCodes.Br, label);
            }
            base.compilerGlobals.BreakLabelStack.Push(label);
            base.compilerGlobals.ContinueLabelStack.Push(label);
            for (int j = 0; j < count; j++)
            {
                il.MarkLabel(labelArray[j]);
                this.cases[j].TranslateToIL(il, Typeob.Void);
            }
            il.MarkLabel(label);
            base.compilerGlobals.BreakLabelStack.Pop();
            base.compilerGlobals.ContinueLabelStack.Pop();
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.expression.TranslateToILInitializer(il);
            int num = 0;
            int count = this.cases.count;
            while (num < count)
            {
                this.cases[num].TranslateToILInitializer(il);
                num++;
            }
        }
    }
}

