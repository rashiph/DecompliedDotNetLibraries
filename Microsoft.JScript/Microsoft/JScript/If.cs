namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection.Emit;

    internal sealed class If : AST
    {
        private Completion completion;
        private AST condition;
        private AST operand1;
        private AST operand2;

        internal If(Context context, AST condition, AST true_branch, AST false_branch) : base(context)
        {
            this.condition = condition;
            this.operand1 = true_branch;
            this.operand2 = false_branch;
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            if ((this.operand1 != null) || (this.operand2 != null))
            {
                Completion completion = null;
                if (this.condition != null)
                {
                    if (Microsoft.JScript.Convert.ToBoolean(this.condition.Evaluate()))
                    {
                        completion = (Completion) this.operand1.Evaluate();
                    }
                    else if (this.operand2 != null)
                    {
                        completion = (Completion) this.operand2.Evaluate();
                    }
                    else
                    {
                        completion = new Completion();
                    }
                }
                else if (this.operand1 != null)
                {
                    completion = (Completion) this.operand1.Evaluate();
                }
                else
                {
                    completion = (Completion) this.operand2.Evaluate();
                }
                this.completion.value = completion.value;
                if (completion.Continue > 1)
                {
                    this.completion.Continue = completion.Continue - 1;
                }
                else
                {
                    this.completion.Continue = 0;
                }
                if (completion.Exit > 0)
                {
                    this.completion.Exit = completion.Exit - 1;
                }
                else
                {
                    this.completion.Exit = 0;
                }
                if (completion.Return)
                {
                    return completion;
                }
            }
            return this.completion;
        }

        internal override bool HasReturn()
        {
            if (this.operand1 != null)
            {
                return ((this.operand1.HasReturn() && (this.operand2 != null)) && this.operand2.HasReturn());
            }
            return ((this.operand2 != null) && this.operand2.HasReturn());
        }

        internal override AST PartiallyEvaluate()
        {
            this.condition = this.condition.PartiallyEvaluate();
            if (this.condition is ConstantWrapper)
            {
                if (Microsoft.JScript.Convert.ToBoolean(this.condition.Evaluate()))
                {
                    this.operand2 = null;
                }
                else
                {
                    this.operand1 = null;
                }
                this.condition = null;
            }
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            if (parent is FunctionScope)
            {
                FunctionScope scope = (FunctionScope) parent;
                BitArray definedFlags = scope.DefinedFlags;
                BitArray array2 = definedFlags;
                if (this.operand1 != null)
                {
                    this.operand1 = this.operand1.PartiallyEvaluate();
                    array2 = scope.DefinedFlags;
                    scope.DefinedFlags = definedFlags;
                }
                if (this.operand2 != null)
                {
                    this.operand2 = this.operand2.PartiallyEvaluate();
                    BitArray array3 = scope.DefinedFlags;
                    int length = array2.Length;
                    int num2 = array3.Length;
                    if (length < num2)
                    {
                        array2.Length = num2;
                    }
                    if (num2 < length)
                    {
                        array3.Length = length;
                    }
                    definedFlags = array2.And(array3);
                }
                scope.DefinedFlags = definedFlags;
            }
            else
            {
                if (this.operand1 != null)
                {
                    this.operand1 = this.operand1.PartiallyEvaluate();
                }
                if (this.operand2 != null)
                {
                    this.operand2 = this.operand2.PartiallyEvaluate();
                }
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if ((this.operand1 != null) || (this.operand2 != null))
            {
                Label label = il.DefineLabel();
                Label item = il.DefineLabel();
                base.compilerGlobals.BreakLabelStack.Push(item);
                base.compilerGlobals.ContinueLabelStack.Push(item);
                if (this.condition != null)
                {
                    base.context.EmitLineInfo(il);
                    if (this.operand2 != null)
                    {
                        this.condition.TranslateToConditionalBranch(il, false, label, false);
                    }
                    else
                    {
                        this.condition.TranslateToConditionalBranch(il, false, item, false);
                    }
                    if (this.operand1 != null)
                    {
                        this.operand1.TranslateToIL(il, Typeob.Void);
                    }
                    if (this.operand2 != null)
                    {
                        if ((this.operand1 != null) && !this.operand1.HasReturn())
                        {
                            il.Emit(OpCodes.Br, item);
                        }
                        il.MarkLabel(label);
                        this.operand2.TranslateToIL(il, Typeob.Void);
                    }
                }
                else if (this.operand1 != null)
                {
                    this.operand1.TranslateToIL(il, Typeob.Void);
                }
                else
                {
                    this.operand2.TranslateToIL(il, Typeob.Void);
                }
                il.MarkLabel(item);
                base.compilerGlobals.BreakLabelStack.Pop();
                base.compilerGlobals.ContinueLabelStack.Pop();
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            if (this.condition != null)
            {
                this.condition.TranslateToILInitializer(il);
            }
            if (this.operand1 != null)
            {
                this.operand1.TranslateToILInitializer(il);
            }
            if (this.operand2 != null)
            {
                this.operand2.TranslateToILInitializer(il);
            }
        }
    }
}

