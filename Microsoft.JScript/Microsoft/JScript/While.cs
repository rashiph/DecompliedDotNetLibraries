namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class While : AST
    {
        private AST body;
        private Completion completion;
        private AST condition;

        internal While(Context context, AST condition, AST body) : base(context)
        {
            this.condition = condition;
            this.body = body;
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            this.completion.Continue = 0;
            this.completion.Exit = 0;
            this.completion.value = null;
            while (Microsoft.JScript.Convert.ToBoolean(this.condition.Evaluate()))
            {
                Completion completion = (Completion) this.body.Evaluate();
                if (completion.value != null)
                {
                    this.completion.value = completion.value;
                }
                if (completion.Continue > 1)
                {
                    this.completion.Continue = completion.Continue - 1;
                    break;
                }
                if (completion.Exit > 0)
                {
                    this.completion.Exit = completion.Exit - 1;
                    break;
                }
                if (completion.Return)
                {
                    return completion;
                }
            }
            return this.completion;
        }

        internal override AST PartiallyEvaluate()
        {
            this.condition = this.condition.PartiallyEvaluate();
            IReflect reflect = this.condition.InferType(null);
            if ((reflect is FunctionPrototype) || (reflect == Typeob.ScriptFunction))
            {
                base.context.HandleError(JSError.SuspectLoopCondition);
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
                this.body = this.body.PartiallyEvaluate();
                scope.DefinedFlags = definedFlags;
            }
            else
            {
                this.body = this.body.PartiallyEvaluate();
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Label item = il.DefineLabel();
            Label label2 = il.DefineLabel();
            Label loc = il.DefineLabel();
            base.compilerGlobals.BreakLabelStack.Push(label2);
            base.compilerGlobals.ContinueLabelStack.Push(item);
            il.Emit(OpCodes.Br, item);
            il.MarkLabel(loc);
            this.body.TranslateToIL(il, Typeob.Void);
            il.MarkLabel(item);
            base.context.EmitLineInfo(il);
            this.condition.TranslateToConditionalBranch(il, true, loc, false);
            il.MarkLabel(label2);
            base.compilerGlobals.BreakLabelStack.Pop();
            base.compilerGlobals.ContinueLabelStack.Pop();
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.condition.TranslateToILInitializer(il);
            this.body.TranslateToILInitializer(il);
        }
    }
}

