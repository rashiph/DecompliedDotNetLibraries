namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal class InstanceMethodCallExpressionN : MethodCallExpression, IArgumentProvider
    {
        private IList<Expression> _arguments;
        private readonly Expression _instance;

        public InstanceMethodCallExpressionN(MethodInfo method, Expression instance, IList<Expression> args) : base(method)
        {
            this._instance = instance;
            this._arguments = args;
        }

        internal override Expression GetInstance()
        {
            return this._instance;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments()
        {
            return Expression.ReturnReadOnly<Expression>(ref this._arguments);
        }

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args)
        {
            return Expression.Call(instance, base.Method, args ?? this._arguments);
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

