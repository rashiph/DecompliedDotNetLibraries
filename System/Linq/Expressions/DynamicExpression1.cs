namespace System.Linq.Expressions
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal class DynamicExpression1 : DynamicExpression, IArgumentProvider
    {
        private object _arg0;

        internal DynamicExpression1(Type delegateType, CallSiteBinder binder, Expression arg0) : base(delegateType, binder)
        {
            this._arg0 = arg0;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments()
        {
            return Expression.ReturnReadOnly(this, ref this._arg0);
        }

        internal override DynamicExpression Rewrite(Expression[] args)
        {
            return Expression.MakeDynamic(base.DelegateType, base.Binder, args[0]);
        }

        Expression IArgumentProvider.GetArgument(int index)
        {
            if (index != 0)
            {
                throw new InvalidOperationException();
            }
            return Expression.ReturnObject<Expression>(this._arg0);
        }

        int IArgumentProvider.ArgumentCount
        {
            get
            {
                return 1;
            }
        }
    }
}

