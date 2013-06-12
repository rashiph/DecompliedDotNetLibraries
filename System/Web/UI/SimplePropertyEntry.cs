namespace System.Web.UI
{
    using System;
    using System.CodeDom;
    using System.Web.Compilation;

    public class SimplePropertyEntry : PropertyEntry
    {
        private string _persistedValue;
        private bool _useSetAttribute;
        private object _value;

        internal SimplePropertyEntry()
        {
        }

        internal CodeStatement GetCodeStatement(BaseTemplateCodeDomTreeGenerator generator, CodeExpression ctrlRefExpr)
        {
            CodeExpression expression2;
            if (this.UseSetAttribute)
            {
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeCastExpression(typeof(IAttributeAccessor), ctrlRefExpr), "SetAttribute", new CodeExpression[0]);
                expression.Parameters.Add(new CodePrimitiveExpression(base.Name));
                expression.Parameters.Add(new CodePrimitiveExpression(this.Value));
                return new CodeExpressionStatement(expression);
            }
            CodeExpression right = null;
            if (base.PropertyInfo != null)
            {
                expression2 = CodeDomUtility.BuildPropertyReferenceExpression(ctrlRefExpr, base.Name);
            }
            else
            {
                expression2 = new CodeFieldReferenceExpression(ctrlRefExpr, base.Name);
            }
            if (base.Type == typeof(string))
            {
                right = generator.BuildStringPropertyExpression(this);
            }
            else
            {
                right = CodeDomUtility.GenerateExpressionForValue(base.PropertyInfo, this.Value, base.Type);
            }
            return new CodeAssignStatement(expression2, right);
        }

        public string PersistedValue
        {
            get
            {
                return this._persistedValue;
            }
            set
            {
                this._persistedValue = value;
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

        public object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }
    }
}

