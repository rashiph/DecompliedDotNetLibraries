namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Routing;
    using System.Web.UI;

    [ExpressionEditor("System.Web.UI.Design.RouteUrlExpressionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ExpressionPrefix("Routes")]
    public class RouteUrlExpressionBuilder : ExpressionBuilder
    {
        public override object EvaluateExpression(object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            return GetRouteUrl(context.TemplateControl, entry.Expression.Trim());
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(base.GetType()), "GetRouteUrl", new CodeExpression[] { new CodeThisReferenceExpression(), new CodePrimitiveExpression(entry.Expression.Trim()) });
        }

        public static string GetRouteUrl(Control control, string expression)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            string routeName = null;
            RouteValueDictionary routeValues = new RouteValueDictionary();
            if (!TryParseRouteExpression(expression, routeValues, out routeName))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("RouteUrlExpression_InvalidExpression"));
            }
            return control.GetRouteUrl(routeName, routeValues);
        }

        public static bool TryParseRouteExpression(string expression, RouteValueDictionary routeValues, out string routeName)
        {
            routeName = null;
            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }
            foreach (string str in expression.Split(new char[] { ',' }))
            {
                string[] strArray2 = str.Split(new char[] { '=' });
                if (strArray2.Length != 2)
                {
                    return false;
                }
                string str2 = strArray2[0].Trim();
                string str3 = strArray2[1].Trim();
                if (string.IsNullOrEmpty(str2))
                {
                    return false;
                }
                if (str2.Equals("RouteName", StringComparison.OrdinalIgnoreCase))
                {
                    routeName = str3;
                }
                else
                {
                    routeValues[str2] = str3;
                }
            }
            return true;
        }

        public override bool SupportsEvaluate
        {
            get
            {
                return true;
            }
        }
    }
}

