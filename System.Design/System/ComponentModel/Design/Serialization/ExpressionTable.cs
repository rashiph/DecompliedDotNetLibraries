namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;

    internal sealed class ExpressionTable
    {
        private Hashtable _expressions;

        internal bool ContainsPresetExpression(object value)
        {
            ExpressionInfo info = this.Expressions[value] as ExpressionInfo;
            return ((info != null) && info.IsPreset);
        }

        internal CodeExpression GetExpression(object value)
        {
            CodeExpression expression = null;
            ExpressionInfo info = this.Expressions[value] as ExpressionInfo;
            if (info != null)
            {
                expression = info.Expression;
            }
            return expression;
        }

        internal void SetExpression(object value, CodeExpression expression, bool isPreset)
        {
            this.Expressions[value] = new ExpressionInfo(expression, isPreset);
        }

        private Hashtable Expressions
        {
            get
            {
                if (this._expressions == null)
                {
                    this._expressions = new Hashtable(new ReferenceComparer());
                }
                return this._expressions;
            }
        }

        private class ExpressionInfo
        {
            private CodeExpression _expression;
            private bool _isPreset;

            internal ExpressionInfo(CodeExpression expression, bool isPreset)
            {
                this._expression = expression;
                this._isPreset = isPreset;
            }

            internal CodeExpression Expression
            {
                get
                {
                    return this._expression;
                }
            }

            internal bool IsPreset
            {
                get
                {
                    return this._isPreset;
                }
            }
        }

        private class ReferenceComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
            {
                return object.ReferenceEquals(x, y);
            }

            int IEqualityComparer.GetHashCode(object x)
            {
                if (x != null)
                {
                    return x.GetHashCode();
                }
                return 0;
            }
        }
    }
}

