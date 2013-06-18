namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class Delete : UnaryOp
    {
        internal Delete(Context context, AST operand) : base(context, operand)
        {
        }

        internal override void CheckIfOKToUseInSuperConstructorCall()
        {
            base.context.HandleError(JSError.NotAllowedInSuperConstructorCall);
        }

        internal override object Evaluate()
        {
            try
            {
                return base.operand.Delete();
            }
            catch (JScriptException)
            {
                return true;
            }
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return Typeob.Boolean;
        }

        internal override AST PartiallyEvaluate()
        {
            base.operand = base.operand.PartiallyEvaluate();
            if (base.operand is Binding)
            {
                ((Binding) base.operand).CheckIfDeletable();
            }
            else if (base.operand is Call)
            {
                ((Call) base.operand).MakeDeletable();
            }
            else
            {
                base.operand.context.HandleError(JSError.NotDeletable);
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            base.operand.TranslateToILDelete(il, rtype);
        }
    }
}

