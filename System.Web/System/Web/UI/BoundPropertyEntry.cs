namespace System.Web.UI
{
    using System;
    using System.Web.Compilation;

    public class BoundPropertyEntry : PropertyEntry
    {
        private string _controlID;
        private Type _controlType;
        private string _expression;
        private System.Web.Compilation.ExpressionBuilder _expressionBuilder;
        private string _expressionPrefix;
        private string _fieldName;
        private string _formatString;
        private bool _generated;
        private object _parsedExpressionData;
        private bool _readOnlyProperty;
        private bool _twoWayBound;
        private bool _useSetAttribute;

        internal BoundPropertyEntry()
        {
        }

        internal void ParseExpression(ExpressionBuilderContext context)
        {
            if (((this.Expression != null) && (this.ExpressionPrefix != null)) && (this.ExpressionBuilder != null))
            {
                this._parsedExpressionData = this.ExpressionBuilder.ParseExpression(this.Expression, base.Type, context);
            }
        }

        public string ControlID
        {
            get
            {
                return this._controlID;
            }
            set
            {
                this._controlID = value;
            }
        }

        public Type ControlType
        {
            get
            {
                return this._controlType;
            }
            set
            {
                this._controlType = value;
            }
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

        public System.Web.Compilation.ExpressionBuilder ExpressionBuilder
        {
            get
            {
                return this._expressionBuilder;
            }
            set
            {
                this._expressionBuilder = value;
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

        public string FieldName
        {
            get
            {
                return this._fieldName;
            }
            set
            {
                this._fieldName = value;
            }
        }

        public string FormatString
        {
            get
            {
                return this._formatString;
            }
            set
            {
                this._formatString = value;
            }
        }

        public bool Generated
        {
            get
            {
                return this._generated;
            }
            set
            {
                this._generated = value;
            }
        }

        internal bool IsDataBindingEntry
        {
            get
            {
                return string.IsNullOrEmpty(this.ExpressionPrefix);
            }
        }

        public object ParsedExpressionData
        {
            get
            {
                return this._parsedExpressionData;
            }
            set
            {
                this._parsedExpressionData = value;
            }
        }

        public bool ReadOnlyProperty
        {
            get
            {
                return this._readOnlyProperty;
            }
            set
            {
                this._readOnlyProperty = value;
            }
        }

        public bool TwoWayBound
        {
            get
            {
                return this._twoWayBound;
            }
            set
            {
                this._twoWayBound = value;
            }
        }

        public bool UseSetAttribute
        {
            get
            {
                return this._useSetAttribute;
            }
            set
            {
                this._useSetAttribute = value;
            }
        }
    }
}

