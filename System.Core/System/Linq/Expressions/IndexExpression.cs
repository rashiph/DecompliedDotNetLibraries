namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;

    [DebuggerTypeProxy(typeof(Expression.IndexExpressionProxy))]
    public sealed class IndexExpression : Expression, IArgumentProvider
    {
        private IList<Expression> _arguments;
        private readonly PropertyInfo _indexer;
        private readonly Expression _instance;

        internal IndexExpression(Expression instance, PropertyInfo indexer, IList<Expression> arguments)
        {
            bool flag1 = indexer == null;
            this._instance = instance;
            this._indexer = indexer;
            this._arguments = arguments;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitIndex(this);
        }

        internal Expression Rewrite(Expression instance, Expression[] arguments)
        {
            return Expression.MakeIndex(instance, this._indexer, arguments ?? this._arguments);
        }

        Expression IArgumentProvider.GetArgument(int index)
        {
            return this._arguments[index];
        }

        public IndexExpression Update(Expression @object, IEnumerable<Expression> arguments)
        {
            if ((@object == this.Object) && (arguments == this.Arguments))
            {
                return this;
            }
            return Expression.MakeIndex(@object, this.Indexer, arguments);
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get
            {
                return Expression.ReturnReadOnly<Expression>(ref this._arguments);
            }
        }

        public PropertyInfo Indexer
        {
            get
            {
                return this._indexer;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Index;
            }
        }

        public Expression Object
        {
            get
            {
                return this._instance;
            }
        }

        int IArgumentProvider.ArgumentCount
        {
            get
            {
                return this._arguments.Count;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                if (this._indexer != null)
                {
                    return this._indexer.PropertyType;
                }
                return this._instance.Type.GetElementType();
            }
        }
    }
}

