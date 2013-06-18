namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;

    public class RouteValueExpressionEditorSheet : ExpressionEditorSheet
    {
        private string _routeValue;

        public RouteValueExpressionEditorSheet(string expression, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            if (!string.IsNullOrEmpty(expression))
            {
                this.RouteValue = expression;
            }
        }

        public override string GetExpression()
        {
            return this.RouteValue;
        }

        public override bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(this.RouteValue);
            }
        }

        [DefaultValue(""), System.Design.SRDescription("RouteValueExpressionEditorSheet_RouteValue")]
        public string RouteValue
        {
            get
            {
                if (this._routeValue == null)
                {
                    return string.Empty;
                }
                return this._routeValue;
            }
            set
            {
                this._routeValue = value;
            }
        }
    }
}

