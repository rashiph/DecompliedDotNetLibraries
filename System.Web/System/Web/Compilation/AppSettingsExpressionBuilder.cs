namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Configuration;
    using System.Web;
    using System.Web.UI;

    [ExpressionPrefix("AppSettings"), ExpressionEditor("System.Web.UI.Design.AppSettingsExpressionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class AppSettingsExpressionBuilder : ExpressionBuilder
    {
        public override object EvaluateExpression(object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            return GetAppSetting(entry.Expression, target.GetType(), entry.PropertyInfo.Name);
        }

        public static object GetAppSetting(string key)
        {
            string str = ConfigurationManager.AppSettings[key];
            if (str == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("AppSetting_not_found", new object[] { key }));
            }
            return str;
        }

        public static object GetAppSetting(string key, Type targetType, string propertyName)
        {
            string str = ConfigurationManager.AppSettings[key];
            if (targetType != null)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(targetType)[propertyName];
                if ((descriptor != null) && (descriptor.PropertyType != typeof(string)))
                {
                    TypeConverter converter = descriptor.Converter;
                    if (!converter.CanConvertFrom(typeof(string)))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("AppSetting_not_convertible", new object[] { str, descriptor.PropertyType.Name, descriptor.Name }));
                    }
                    return converter.ConvertFrom(str);
                }
            }
            if (str == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("AppSetting_not_found", new object[] { key }));
            }
            return str;
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            if ((entry.DeclaringType == null) || (entry.PropertyInfo == null))
            {
                return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(base.GetType()), "GetAppSetting", new CodeExpression[] { new CodePrimitiveExpression(entry.Expression.Trim()) });
            }
            return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(base.GetType()), "GetAppSetting", new CodeExpression[] { new CodePrimitiveExpression(entry.Expression.Trim()), new CodeTypeOfExpression(entry.DeclaringType), new CodePrimitiveExpression(entry.PropertyInfo.Name) });
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

