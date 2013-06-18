namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Web.UI;

    internal sealed class DesignTimeDataBinding
    {
        private string _expression;
        private string _field;
        private string _format;
        private bool _parsed;
        private DataBinding _runtimeDataBinding;
        private bool _twoWayBinding;
        private static readonly Regex BindExpressionRegex = new System.Web.RegularExpressions.BindExpressionRegex();
        private static readonly Regex BindParametersRegex = new System.Web.RegularExpressions.BindParametersRegex();
        private static readonly Regex EvalRegex = new EvalExpressionRegex();

        public DesignTimeDataBinding(DataBinding runtimeDataBinding)
        {
            this._runtimeDataBinding = runtimeDataBinding;
        }

        public DesignTimeDataBinding(PropertyDescriptor propDesc, string expression)
        {
            this._expression = expression;
            this._runtimeDataBinding = new DataBinding(propDesc.Name, propDesc.PropertyType, expression);
        }

        public DesignTimeDataBinding(PropertyDescriptor propDesc, string field, string format, bool twoWayBinding)
        {
            this._field = field;
            this._format = format;
            if (twoWayBinding)
            {
                this._expression = CreateBindExpression(field, format);
            }
            else
            {
                this._expression = CreateEvalExpression(field, format);
            }
            this._parsed = true;
            this._twoWayBinding = twoWayBinding;
            this._runtimeDataBinding = new DataBinding(propDesc.Name, propDesc.PropertyType, this._expression);
        }

        public static string CreateBindExpression(string field, string format)
        {
            return CreateExpression("Bind", field, format);
        }

        public static string CreateEvalExpression(string field, string format)
        {
            return CreateExpression("Eval", field, format);
        }

        private static string CreateExpression(string method, string field, string format)
        {
            string str = field;
            for (int i = 0; i < field.Length; i++)
            {
                char c = field[i];
                if ((!char.IsLetterOrDigit(c) && (c != '_')) && (c != '.'))
                {
                    str = "[" + field + "]";
                    break;
                }
            }
            if ((format != null) && (format.Length != 0))
            {
                return string.Format(CultureInfo.InvariantCulture, method + "(\"{0}\", \"{1}\")", new object[] { str, format });
            }
            return string.Format(CultureInfo.InvariantCulture, method + "(\"{0}\")", new object[] { str });
        }

        private void EnsureParsed()
        {
            if (!this._parsed)
            {
                this._expression = this._runtimeDataBinding.Expression.Trim();
                if (this._expression.Length != 0)
                {
                    try
                    {
                        bool flag = false;
                        Match match = EvalRegex.Match(this._expression);
                        if (match.Success)
                        {
                            flag = true;
                        }
                        else
                        {
                            match = BindExpressionRegex.Match(this._expression);
                        }
                        if (match.Success)
                        {
                            string input = match.Groups["params"].Value;
                            if ((match = BindParametersRegex.Match(input, 0)).Success)
                            {
                                this._field = match.Groups["fieldName"].Value;
                                Group group = match.Groups["formatString"];
                                if (group != null)
                                {
                                    this._format = group.Value;
                                }
                                if (!flag)
                                {
                                    this._twoWayBinding = true;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            this._parsed = true;
        }

        public string Expression
        {
            get
            {
                this.EnsureParsed();
                return this._expression;
            }
        }

        public string Field
        {
            get
            {
                this.EnsureParsed();
                return this._field;
            }
        }

        public string Format
        {
            get
            {
                this.EnsureParsed();
                return this._format;
            }
        }

        public bool IsCustom
        {
            get
            {
                this.EnsureParsed();
                return (this._field == null);
            }
        }

        public bool IsTwoWayBound
        {
            get
            {
                this.EnsureParsed();
                return this._twoWayBinding;
            }
        }

        public DataBinding RuntimeDataBinding
        {
            get
            {
                return this._runtimeDataBinding;
            }
        }
    }
}

