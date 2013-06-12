namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.ConditionalExpressionProxy))]
    public class ConditionalExpression : Expression
    {
        private readonly Expression _test;
        private readonly Expression _true;

        internal ConditionalExpression(Expression test, Expression ifTrue)
        {
            this._test = test;
            this._true = ifTrue;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitConditional(this);
        }

        internal virtual Expression GetFalse()
        {
            return Expression.Empty();
        }

        internal static ConditionalExpression Make(Expression test, Expression ifTrue, Expression ifFalse, System.Type type)
        {
            if ((ifTrue.Type != type) || (ifFalse.Type != type))
            {
                return new FullConditionalExpressionWithType(test, ifTrue, ifFalse, type);
            }
            if ((ifFalse is DefaultExpression) && (ifFalse.Type == typeof(void)))
            {
                return new ConditionalExpression(test, ifTrue);
            }
            return new FullConditionalExpression(test, ifTrue, ifFalse);
        }

        public ConditionalExpression Update(Expression test, Expression ifTrue, Expression ifFalse)
        {
            if (((test == this.Test) && (ifTrue == this.IfTrue)) && (ifFalse == this.IfFalse))
            {
                return this;
            }
            return Expression.Condition(test, ifTrue, ifFalse, this.Type);
        }

        public Expression IfFalse
        {
            get
            {
                return this.GetFalse();
            }
        }

        public Expression IfTrue
        {
            get
            {
                return this._true;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Conditional;
            }
        }

        public Expression Test
        {
            get
            {
                return this._test;
            }
        }

        public override System.Type Type
        {
            get
            {
                return this.IfTrue.Type;
            }
        }
    }
}

