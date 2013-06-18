namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class InternalSubExprOpcode : SubExprOpcode
    {
        internal InternalSubExprOpcode(SubExpr expr) : base(expr)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            if (!context.LoadVariable(base.expr.Variable))
            {
                base.expr.Eval(context);
            }
            return base.next;
        }

        internal override Opcode EvalSpecial(ProcessingContext context)
        {
            base.expr.EvalSpecial(context);
            return base.next;
        }
    }
}

