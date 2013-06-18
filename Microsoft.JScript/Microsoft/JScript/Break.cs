namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    internal sealed class Break : AST
    {
        private Completion completion;
        private bool leavesFinally;

        internal Break(Context context, int count, bool leavesFinally) : base(context)
        {
            this.completion = new Completion();
            this.completion.Exit = count;
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
            Label label = (Label) base.compilerGlobals.BreakLabelStack.Peek(this.completion.Exit - 1);
            base.context.EmitLineInfo(il);
            if (this.leavesFinally)
            {
                ConstantWrapper.TranslateToILInt(il, base.compilerGlobals.BreakLabelStack.Size() - this.completion.Exit);
                il.Emit(OpCodes.Newobj, CompilerGlobals.breakOutOfFinallyConstructor);
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

