namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Web.UI;

    [ExpressionPrefix("Routes"), ExpressionEditor("System.Web.UI.Design.RouteValueExpressionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class RouteValueExpressionBuilder : ExpressionBuilder
    {
        internal static object ConvertRouteValue(object value, Type controlType, string propertyName)
        {
            if ((controlType != null) && !string.IsNullOrEmpty(propertyName))
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(controlType)[propertyName];
                if ((descriptor != null) && (descriptor.PropertyType != typeof(string)))
                {
                    TypeConverter converter = descriptor.Converter;
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        return converter.ConvertFrom(value);
                    }
                }
            }
            return value;
        }

        public override object EvaluateExpression(object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            if (target is Control)
            {
                return GetRouteValue(context.TemplateControl.Page, entry.Expression.Trim(), entry.ControlType, entry.Name);
            }
            return null;
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(base.GetType()), "GetRouteValue", new CodeExpression[] { new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Page"), new CodePrimitiveExpression(entry.Expression.Trim()), new CodeTypeOfExpression(new CodeTypeReference(entry.ControlType)), new CodePrimitiveExpression(entry.Name) });
        }

        public static object GetRouteValue(Page page, string key, Type controlType, string propertyName)
        {
            if (((page != null) && !string.IsNullOrEmpty(key)) && (page.RouteData != null))
            {
                return ConvertRouteValue(page.RouteData.Values[key], controlType, propertyName);
            }
            return null;
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

