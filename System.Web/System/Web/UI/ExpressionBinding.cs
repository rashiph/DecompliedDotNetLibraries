namespace System.Web.UI
{
    using System;
    using System.Globalization;
    using System.Web.Util;

    public sealed class ExpressionBinding
    {
        private string _expression;
        private string _expressionPrefix;
        private bool _generated;
        private object _parsedExpressionData;
        private string _propertyName;
        private Type _propertyType;

        public ExpressionBinding(string propertyName, Type propertyType, string expressionPrefix, string expression) : this(propertyName, propertyType, expressionPrefix, expression, false, null)
        {
        }

        internal ExpressionBinding(string propertyName, Type propertyType, string expressionPrefix, string expression, bool generated, object parsedExpressionData)
        {
            this._propertyName = propertyName;
            this._propertyType = propertyType;
            this._expression = expression;
            this._expressionPrefix = expressionPrefix;
            this._generated = generated;
            this._parsedExpressionData = parsedExpressionData;
        }

        public override bool Equals(object obj)
        {
            if ((obj != null) && (obj is ExpressionBinding))
            {
                ExpressionBinding binding = (ExpressionBinding) obj;
                return StringUtil.EqualsIgnoreCase(this._propertyName, binding.PropertyName);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this._propertyName.ToLower(CultureInfo.InvariantCulture).GetHashCode();
        }

        public string Expression
        {
            get
            {
                return this._expression;
            }
            set
            {
                this._expression = value;
            }
        }

        public string ExpressionPrefix
        {
            get
            {
                return this._expressionPrefix;
            }
            set
            {
                this._expressionPrefix = value;
            }
        }

        public bool Generated
        {
            get
            {
                return this._generated;
            }
        }

        public object ParsedExpressionData
        {
            get
            {
                return this._parsedExpressionData;
            }
        }

        public string PropertyName
        {
            get
            {
                return this._propertyName;
            }
        }

        public Type PropertyType
        {
            get
            {
                return this._propertyType;
            }
        }
    }
}

