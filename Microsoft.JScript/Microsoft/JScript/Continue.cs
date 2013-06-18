namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    internal sealed class Continue : AST
    {
        private Completion completion;
        private bool leavesFinally;

        internal Continue(Context context, int count, bool leavesFinally) : base(context)
        {
            this.completion = new Completion();
            this.completion.Continue = count;
            this.leavesFinally = leavesFinally;
        }

        internal override object Evaluate()
        {
            return this.completion;
        }

        internal override AST PartiallyEvaluate()
        {
            if (this.leavesFinally)
            {
                base.context.HandleError(JSError.BadWayToLeaveFinally);
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            Label label = (Label) base.compilerGlobals.ContinueLabelStack.Peek(this.completion.Continue - 1);
            base.context.EmitLineInfo(il);
            if (this.leavesFinally)
            {
                ConstantWrapper.TranslateToILInt(il, base.compilerGlobals.ContinueLabelStack.Size() - this.completion.Continue);
                il.Emit(OpCodes.Newobj, CompilerGlobals.continueOutOfFinallyConstructor);
                il.Emit(OpCodes.Throw);
            }
            else if (base.compilerGlobals.InsideProtectedRegion)
            {
                il.Emit(OpCodes.Leave, label);
            }
            else
            {
                il.Emit(OpCodes.Br, label);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
        }
    }
}

