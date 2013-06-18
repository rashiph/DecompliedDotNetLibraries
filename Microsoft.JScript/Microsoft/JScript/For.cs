namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class For : AST
    {
        private AST body;
        private Completion completion;
        private AST condition;
        private AST incrementer;
        private AST initializer;

        internal For(Context context, AST initializer, AST condition, AST incrementer, AST body) : base(context)
        {
            this.initializer = initializer;
            this.condition = condition;
            this.incrementer = incrementer;
            this.body = body;
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            this.completion.Continue = 0;
            this.completion.Exit = 0;
            this.completion.value = null;
            this.initializer.Evaluate();
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
                this.incrementer.Evaluate();
            }
            return this.completion;
        }

        internal override AST PartiallyEvaluate()
        {
            this.initializer = this.initializer.PartiallyEvaluate();
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            if (parent is FunctionScope)
            {
                FunctionScope scope = (FunctionScope) parent;
                BitArray definedFlags = scope.DefinedFlags;
                this.condition = this.condition.PartiallyEvaluate();
                this.body = this.body.PartiallyEvaluate();
                scope.DefinedFlags = definedFlags;
                this.incrementer = this.incrementer.PartiallyEvaluate();
                scope.DefinedFlags = definedFlags;
            }
            else
            {
                this.condition = this.condition.PartiallyEvaluate();
                this.body = this.body.PartiallyEvaluate();
                this.incrementer = this.incrementer.PartiallyEvaluate();
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
            il.DefineLabel();
            Label label3 = il.DefineLabel();
            bool flag = false;
            base.compilerGlobals.BreakLabelStack.Push(label3);
            base.compilerGlobals.ContinueLabelStack.Push(item);
            if (!(this.initializer is EmptyLiteral))
            {
                this.initializer.context.EmitLineInfo(il);
                this.initializer.TranslateToIL(il, Typeob.Void);
            }
            il.MarkLabel(loc);
            if ((!(this.condition is ConstantWrapper) || !(this.condition.Evaluate() is bool)) || !((bool) this.condition.Evaluate()))
            {
                this.condition.context.EmitLineInfo(il);
                this.condition.TranslateToConditionalBranch(il, false, label3, false);
            }
            else if ((this.condition.context.StartPosition + 1) == this.condition.context.EndPosition)
            {
                flag = true;
            }
            this.body.TranslateToIL(il, Typeob.Void);
            il.MarkLabel(item);
            if (this.incrementer is EmptyLiteral)
            {
                if (flag)
                {
                    base.context.EmitLineInfo(il);
                }
            }
            else
            {
                this.incrementer.context.EmitLineInfo(il);
                this.incrementer.TranslateToIL(il, Typeob.Void);
            }
            il.Emit(OpCodes.Br, loc);
            il.MarkLabel(label3);
            base.compilerGlobals.BreakLabelStack.Pop();
            base.compilerGlobals.ContinueLabelStack.Pop();
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.initializer.TranslateToILInitializer(il);
            this.condition.TranslateToILInitializer(il);
            this.incrementer.TranslateToILInitializer(il);
            this.body.TranslateToILInitializer(il);
        }
    }
}

