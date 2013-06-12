namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal class MethodCallExpression2 : MethodCallExpression, IArgumentProvider
    {
        private object _arg0;
        private readonly Expression _arg1;

        public MethodCallExpression2(MethodInfo method, Expression arg0, Expression arg1) : base(method)
        {
            this._arg0 = arg0;
            this._arg1 = arg1;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments()
        {
            return Expression.ReturnReadOnly(this, ref this._arg0);
        }

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args)
        {
            if (args != null)
            {
                return Expression.Call(base.Method, args[0], args[1]);
            }
            return Expression.Call(base.Method, Expression.ReturnObject<Expression>(this._arg0), this._arg1);
        }

        Expression IArgumentProvider.GetArgument(int index)
        {
            switch (index)
            {
                case 0:
                    return Expression.ReturnObject<Expression>(this._arg0);

                case 1:
                    return this._arg1;
            }
            throw new InvalidOperationException();
        }

        int IArgumentProvider.ArgumentCount
        {
            get
            {
                return 2;
            }
        }
    }
}

