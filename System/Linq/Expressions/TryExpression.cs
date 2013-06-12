namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.TryExpressionProxy))]
    public sealed class TryExpression : Expression
    {
        private readonly Expression _body;
        private readonly Expression _fault;
        private readonly Expression _finally;
        private readonly ReadOnlyCollection<CatchBlock> _handlers;
        private readonly System.Type _type;

        internal TryExpression(System.Type type, Expression body, Expression @finally, Expression fault, ReadOnlyCollection<CatchBlock> handlers)
        {
            this._type = type;
            this._body = body;
            this._handlers = handlers;
            this._finally = @finally;
            this._fault = fault;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitTry(this);
        }

        public TryExpression Update(Expression body, IEnumerable<CatchBlock> handlers, Expression @finally, Expression fault)
        {
            if (((body == this.Body) && (handlers == this.Handlers)) && ((@finally == this.Finally) && (fault == this.Fault)))
            {
                return this;
            }
            return Expression.MakeTry(this.Type, body, @finally, fault, handlers);
        }

        public Expression Body
        {
            get
            {
                return this._body;
            }
        }

        public Expression Fault
        {
            get
            {
                return this._fault;
            }
        }

        public Expression Finally
        {
            get
            {
                return this._finally;
            }
        }

        public ReadOnlyCollection<CatchBlock> Handlers
        {
            get
            {
                return this._handlers;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Try;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

