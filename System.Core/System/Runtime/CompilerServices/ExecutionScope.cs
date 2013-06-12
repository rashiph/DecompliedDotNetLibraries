namespace System.Runtime.CompilerServices
{
    using System;
    using System.Linq.Expressions;

    [Obsolete("do not use this type", true)]
    public class ExecutionScope
    {
        public object[] Globals = null;
        public object[] Locals = null;
        public ExecutionScope Parent = null;

        internal ExecutionScope()
        {
        }

        public Delegate CreateDelegate(int indexLambda, object[] locals)
        {
            throw new NotSupportedException();
        }

        public object[] CreateHoistedLocals()
        {
            throw new NotSupportedException();
        }

        public Expression IsolateExpression(Expression expression, object[] locals)
        {
            throw new NotSupportedException();
        }
    }
}

