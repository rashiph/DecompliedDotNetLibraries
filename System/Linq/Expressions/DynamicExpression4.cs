namespace System.Linq.Expressions
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal class DynamicExpression4 : DynamicExpression, IArgumentProvider
    {
        private object _arg0;
        private readonly Expression _arg1;
        private readonly Expression _arg2;
        private readonly Expression _arg3;

        internal DynamicExpression4(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3) : base(delegateType, binder)
        {
            this._arg0 = arg0;
            this._arg1 = arg1;
            this._arg2 = arg2;
            this._arg3 = arg3;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments()
        {
            return Expression.ReturnReadOnly(this, ref this._arg0);
        }

        internal override DynamicExpression Rewrite(Expression[] args)
        {
            return Expression.MakeDynamic(base.DelegateType, base.Binder, args[0], args[1], args[2], args[3]);
        }

        Expression IArgumentProvider.GetArgument(int index)
        {
            switch (index)
            {
                case 0:
                    return Expression.ReturnObject<Expression>(this._arg0);

                case 1:
                    return this._arg1;

                case 2:
                    return this._arg2;

                case 3:
                    return this._arg3;
            }
            throw new InvalidOperationException();
        }

        int IArgumentProvider.ArgumentCount
        {
            get
            {
                return 4;
            }
        }
    }
}

