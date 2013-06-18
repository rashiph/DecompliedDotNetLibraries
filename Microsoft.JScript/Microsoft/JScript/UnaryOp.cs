namespace Microsoft.JScript
{
    using System;
    using System.Reflection.Emit;

    public abstract class UnaryOp : AST
    {
        protected AST operand;

        internal UnaryOp(Context context, AST operand) : base(context)
        {
            this.operand = operand;
        }

        internal override void CheckIfOKToUseInSuperConstructorCall()
        {
            this.operand.CheckIfOKToUseInSuperConstructorCall();
        }

        internal override AST PartiallyEvaluate()
        {
            this.operand = this.operand.PartiallyEvaluate();
            if (this.operand is ConstantWrapper)
            {
                try
                {
                    return new ConstantWrapper(this.Evaluate(), base.context);
                }
                catch (JScriptException exception)
                {
                    base.context.HandleError(((JSError) exception.ErrorNumber) & ((JSError) 0xffff));
                }
                catch
                {
                    base.context.HandleError(JSError.TypeMismatch);
                }
            }
            return this;
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.operand.TranslateToILInitializer(il);
        }
    }
}

