namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class Comma : BinaryOp
    {
        internal Comma(Context context, AST operand1, AST operand2) : base(context, operand1, operand2)
        {
        }

        internal override object Evaluate()
        {
            base.operand1.Evaluate();
            return base.operand2.Evaluate();
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return base.operand2.InferType(inference_target);
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.operand1.TranslateToIL(il, Typeob.Void);
            base.operand2.TranslateToIL(il, rtype);
        }
    }
}

