namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.SwitchCaseProxy))]
    public sealed class SwitchCase
    {
        private readonly Expression _body;
        private readonly ReadOnlyCollection<Expression> _testValues;

        internal SwitchCase(Expression body, ReadOnlyCollection<Expression> testValues)
        {
            this._body = body;
            this._testValues = testValues;
        }

        public override string ToString()
        {
            return ExpressionStringBuilder.SwitchCaseToString(this);
        }

        public SwitchCase Update(IEnumerable<Expression> testValues, Expression body)
        {
            if ((testValues == this.TestValues) && (body == this.Body))
            {
                return this;
            }
            return Expression.SwitchCase(body, testValues);
        }

        public Expression Body
        {
            get
            {
                return this._body;
            }
        }

        public ReadOnlyCollection<Expression> TestValues
        {
            get
            {
                return this._testValues;
            }
        }
    }
}

