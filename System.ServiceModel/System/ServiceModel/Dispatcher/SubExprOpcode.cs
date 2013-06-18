namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;

    internal class SubExprOpcode : Opcode
    {
        protected SubExpr expr;

        internal SubExprOpcode(SubExpr expr) : base(OpcodeID.SubExpr)
        {
            this.expr = expr;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                SubExprOpcode opcode = op as SubExprOpcode;
                if (opcode != null)
                {
                    return (this.expr == opcode.expr);
                }
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            if (!context.LoadVariable(this.expr.Variable))
            {
                context.PushSequenceFrame();
                NodeSequence seq = context.CreateSequence();
                seq.Add(context.Processor.ContextNode);
                context.PushSequence(seq);
                int counterMarker = context.Processor.CounterMarker;
                try
                {
                    this.expr.Eval(context);
                }
                catch (XPathNavigatorException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Process(this));
                }
                catch (NavigatorInvalidBodyAccessException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2.Process(this));
                }
                context.Processor.CounterMarker = counterMarker;
                context.PopSequenceFrame();
                context.PopSequenceFrame();
                context.LoadVariable(this.expr.Variable);
            }
            return base.next;
        }

        internal override Opcode EvalSpecial(ProcessingContext context)
        {
            try
            {
                this.expr.EvalSpecial(context);
            }
            catch (XPathNavigatorException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Process(this));
            }
            catch (NavigatorInvalidBodyAccessException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2.Process(this));
            }
            return base.next;
        }

        internal SubExpr Expr
        {
            get
            {
                return this.expr;
            }
        }
    }
}

