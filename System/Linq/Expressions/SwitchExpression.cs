namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Reflection;

    [DebuggerTypeProxy(typeof(Expression.SwitchExpressionProxy))]
    public sealed class SwitchExpression : Expression
    {
        private readonly ReadOnlyCollection<SwitchCase> _cases;
        private readonly MethodInfo _comparison;
        private readonly Expression _defaultBody;
        private readonly Expression _switchValue;
        private readonly System.Type _type;

        internal SwitchExpression(System.Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, ReadOnlyCollection<SwitchCase> cases)
        {
            this._type = type;
            this._switchValue = switchValue;
            this._defaultBody = defaultBody;
            this._comparison = comparison;
            this._cases = cases;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitSwitch(this);
        }

        public SwitchExpression Update(Expression switchValue, IEnumerable<SwitchCase> cases, Expression defaultBody)
        {
            if (((switchValue == this.SwitchValue) && (cases == this.Cases)) && (defaultBody == this.DefaultBody))
            {
                return this;
            }
            return Expression.Switch(this.Type, switchValue, defaultBody, this.Comparison, cases);
        }

        public ReadOnlyCollection<SwitchCase> Cases
        {
            get
            {
                return this._cases;
            }
        }

        public MethodInfo Comparison
        {
            get
            {
                return this._comparison;
            }
        }

        public Expression DefaultBody
        {
            get
            {
                return this._defaultBody;
            }
        }

        internal bool IsLifted
        {
            get
            {
                if (!this._switchValue.Type.IsNullableType())
                {
                    return false;
                }
                if (this._comparison != null)
                {
                    return !TypeUtils.AreEquivalent(this._switchValue.Type, this._comparison.GetParametersCached()[0].ParameterType.GetNonRefType());
                }
                return true;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Switch;
            }
        }

        public Expression SwitchValue
        {
            get
            {
                return this._switchValue;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

