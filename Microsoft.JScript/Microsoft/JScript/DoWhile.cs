namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class DoWhile : AST
    {
        private AST body;
        private Completion completion;
        private AST condition;

        internal DoWhile(Context context, AST body, AST condition) : base(context)
        {
            this.body = body;
            this.condition = condition;
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            this.completion.Continue = 0;
            this.completion.Exit = 0;
            this.completion.value = null;
            do
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
            while (Microsoft.JScript.Convert.ToBoolean(this.condition.Evaluate()));
            return this.completion;
        }

        internal override Context GetFirstExecutableContext()
        {
            return this.body.GetFirstExecutableContext();
        }

        internal override AST PartiallyEvaluate()
        {
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
                this.condition = this.condition.PartiallyEvaluate();
                scope.DefinedFlags = definedFlags;
            }
            else
            {
                this.body = this.body.PartiallyEvaluate();
                this.condition = this.condition.PartiallyEvaluate();
            }
            IReflect reflect = this.condition.InferType(null);
            if ((reflect is FunctionPrototype) || (reflect == Typeob.ScriptFunction))
            {
                base.context.HandleError(JSError.SuspectLoopCondition);
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Label loc = il.DefineLabel();
            Label item = il.DefineLabel();
            Label label3 = il.DefineLabel();
            base.compilerGlobals.BreakLabelStack.Push(label3);
            base.compilerGlobals.ContinueLabelStack.Push(item);
            il.MarkLabel(loc);
            this.body.TranslateToIL(il, Typeob.Void);
            il.MarkLabel(item);
            base.context.EmitLineInfo(il);
            this.condition.TranslateToConditionalBranch(il, true, loc, false);
            il.MarkLabel(label3);
            base.compilerGlobals.BreakLabelStack.Pop();
            base.compilerGlobals.ContinueLabelStack.Pop();
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.body.TranslateToILInitializer(il);
            this.condition.TranslateToILInitializer(il);
        }
    }
}

