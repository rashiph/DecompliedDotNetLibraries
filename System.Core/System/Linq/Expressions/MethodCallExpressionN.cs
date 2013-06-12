namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal class MethodCallExpressionN : MethodCallExpression, IArgumentProvider
    {
        private IList<Expression> _arguments;

        public MethodCallExpressionN(MethodInfo method, IList<Expression> args) : base(method)
        {
            this._arguments = args;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments()
        {
            return Expression.ReturnReadOnly<Expression>(ref this._arguments);
        }

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args)
        {
            return Expression.Call(base.Method, args ?? this._arguments);
        }

        Expression IArgumentProvider.GetArgument(int index)
        {
            return this._arguments[index];
        }

        int IArgumentProvider.ArgumentCount
        {
            get
            {
                return this._arguments.Count;
            }
        }
    }
}

