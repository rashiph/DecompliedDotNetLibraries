namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal class DynamicExpressionN : DynamicExpression, IArgumentProvider
    {
        private IList<Expression> _arguments;

        internal DynamicExpressionN(Type delegateType, CallSiteBinder binder, IList<Expression> arguments) : base(delegateType, binder)
        {
            this._arguments = arguments;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments()
        {
            return Expression.ReturnReadOnly<Expression>(ref this._arguments);
        }

        internal override DynamicExpression Rewrite(Expression[] args)
        {
            return Expression.MakeDynamic(base.DelegateType, base.Binder, args);
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

