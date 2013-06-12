namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.LoopExpressionProxy))]
    public sealed class LoopExpression : Expression
    {
        private readonly Expression _body;
        private readonly LabelTarget _break;
        private readonly LabelTarget _continue;

        internal LoopExpression(Expression body, LabelTarget @break, LabelTarget @continue)
        {
            this._body = body;
            this._break = @break;
            this._continue = @continue;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitLoop(this);
        }

        public LoopExpression Update(LabelTarget breakLabel, LabelTarget continueLabel, Expression body)
        {
            if (((breakLabel == this.BreakLabel) && (continueLabel == this.ContinueLabel)) && (body == this.Body))
            {
                return this;
            }
            return Expression.Loop(body, breakLabel, continueLabel);
        }

        public Expression Body
        {
            get
            {
                return this._body;
            }
        }

        public LabelTarget BreakLabel
        {
            get
            {
                return this._break;
            }
        }

        public LabelTarget ContinueLabel
        {
            get
            {
                return this._continue;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Loop;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                if (this._break != null)
                {
                    return this._break.Type;
                }
                return typeof(void);
            }
        }
    }
}

