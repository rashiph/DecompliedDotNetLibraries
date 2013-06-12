namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal class MethodCallExpression4 : MethodCallExpression, IArgumentProvider
    {
        private object _arg0;
        private readonly Expression _arg1;
        private readonly Expression _arg2;
        private readonly Expression _arg3;

        public MethodCallExpression4(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3) : base(method)
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

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args)
        {
            if (args != null)
            {
                return Expression.Call(base.Method, args[0], args[1], args[2], args[3]);
            }
            return Expression.Call(base.Method, Expression.ReturnObject<Expression>(this._arg0), this._arg1, this._arg2, this._arg3);
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

