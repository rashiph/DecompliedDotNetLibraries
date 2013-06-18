namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    internal sealed class Expression : AST
    {
        private Completion completion;
        internal AST operand;

        internal Expression(Context context, AST operand) : base(context)
        {
            this.operand = operand;
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            this.completion.value = this.operand.Evaluate();
            return this.completion;
        }

        internal override AST PartiallyEvaluate()
        {
            this.operand = this.operand.PartiallyEvaluate();
            if (this.operand is ConstantWrapper)
            {
                this.operand.context.HandleError(JSError.UselessExpression);
            }
            else if (this.operand is Binding)
            {
                ((Binding) this.operand).CheckIfUseless();
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.context.EmitLineInfo(il);
            this.operand.TranslateToIL(il, rtype);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.operand.TranslateToILInitializer(il);
        }
    }
}

