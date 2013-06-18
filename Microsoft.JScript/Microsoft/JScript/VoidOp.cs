namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class VoidOp : UnaryOp
    {
        internal VoidOp(Context context, AST operand) : base(context, operand)
        {
        }

        internal override object Evaluate()
        {
            base.operand.Evaluate();
            return null;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return Typeob.Empty;
        }

        internal override AST PartiallyEvaluate()
        {
            return new ConstantWrapper(null, base.context);
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.operand.TranslateToIL(il, Typeob.Object);
            if (rtype != Typeob.Void)
            {
                il.Emit(OpCodes.Ldsfld, CompilerGlobals.undefinedField);
                Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
            }
            else
            {
                il.Emit(OpCodes.Pop);
            }
        }
    }
}

