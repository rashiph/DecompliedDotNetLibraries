namespace System.Web.UI.Design
{
    using System;
    using System.Design;
    using System.Web.Compilation;
    using System.Web.Routing;

    public class RouteUrlExpressionEditor : ExpressionEditor
    {
        public override object EvaluateExpression(string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider)
        {
            string routeName = null;
            RouteValueDictionary routeValues = new RouteValueDictionary();
            if (RouteUrlExpressionBuilder.TryParseRouteExpression(expression, routeValues, out routeName))
            {
                return ("RouteUrl: " + expression);
            }
            return System.Design.SR.GetString("RouteUrlExpressionEditor_InvalidExpression");
        }

        public override ExpressionEditorSheet GetExpressionEditorSheet(string expression, IServiceProvider serviceProvider)
        {
            return new RouteUrlExpressionEditorSheet(expression, serviceProvider);
        }
    }
}

