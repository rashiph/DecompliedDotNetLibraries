namespace System.Web.UI.Design
{
    using System;

    public class RouteValueExpressionEditor : ExpressionEditor
    {
        public override object EvaluateExpression(string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider)
        {
            return ("RouteValue: " + expression);
        }

        public override ExpressionEditorSheet GetExpressionEditorSheet(string expression, IServiceProvider serviceProvider)
        {
            return new RouteValueExpressionEditorSheet(expression, serviceProvider);
        }
    }
}

