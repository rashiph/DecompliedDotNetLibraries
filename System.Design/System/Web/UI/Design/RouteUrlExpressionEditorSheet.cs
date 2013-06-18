namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Text;
    using System.Web.Compilation;
    using System.Web.Routing;

    public class RouteUrlExpressionEditorSheet : ExpressionEditorSheet
    {
        private string _routeName;
        private string _routeValues;

        public RouteUrlExpressionEditorSheet(string expression, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            if (!string.IsNullOrEmpty(expression))
            {
                string routeName = null;
                RouteValueDictionary routeValues = new RouteValueDictionary();
                if (RouteUrlExpressionBuilder.TryParseRouteExpression(expression, routeValues, out routeName))
                {
                    this.RouteName = routeName;
                    StringBuilder builder = new StringBuilder();
                    foreach (string str2 in routeValues.Keys)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(",");
                        }
                        builder.Append(str2).Append("=").Append(routeValues[str2]);
                    }
                    this.RouteValues = builder.ToString();
                }
            }
        }

        public override string GetExpression()
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(this.RouteName))
            {
                builder.Append("RouteName=").Append(this.RouteName);
            }
            if (!string.IsNullOrEmpty(this.RouteValues))
            {
                if (builder.Length > 0)
                {
                    builder.Append(",");
                }
                builder.Append(this.RouteValues);
            }
            return builder.ToString();
        }

        public override bool IsValid
        {
            get
            {
                string routeName = null;
                RouteValueDictionary routeValues = new RouteValueDictionary();
                return RouteUrlExpressionBuilder.TryParseRouteExpression(this.GetExpression(), routeValues, out routeName);
            }
        }

        [System.Design.SRDescription("RouteUrlExpressionEditorSheet_RouteName"), DefaultValue("")]
        public string RouteName
        {
            get
            {
                if (this._routeName == null)
                {
                    return string.Empty;
                }
                return this._routeName;
            }
            set
            {
                this._routeName = value;
            }
        }

        [System.Design.SRDescription("RouteUrlExpressionEditorSheet_RouteValues"), DefaultValue("")]
        public string RouteValues
        {
            get
            {
                if (this._routeValues == null)
                {
                    return string.Empty;
                }
                return this._routeValues;
            }
            set
            {
                this._routeValues = value;
            }
        }
    }
}

