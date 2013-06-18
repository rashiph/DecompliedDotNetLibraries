namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Configuration;
    using System.Web;
    using System.Web.UI;

    [ExpressionPrefix("ConnectionStrings"), ExpressionEditor("System.Web.UI.Design.ConnectionStringsExpressionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ConnectionStringsExpressionBuilder : ExpressionBuilder
    {
        public override object EvaluateExpression(object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            Pair pair = (Pair) parsedData;
            string first = (string) pair.First;
            bool second = (bool) pair.Second;
            ConnectionStringSettings settings1 = ConfigurationManager.ConnectionStrings[first];
            if (second)
            {
                return GetConnectionString(first);
            }
            return GetConnectionStringProviderName(first);
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            Pair pair = (Pair) parsedData;
            string first = (string) pair.First;
            if ((bool) pair.Second)
            {
                return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(base.GetType()), "GetConnectionString", new CodeExpression[] { new CodePrimitiveExpression(first) });
            }
            return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(base.GetType()), "GetConnectionStringProviderName", new CodeExpression[] { new CodePrimitiveExpression(first) });
        }

        public static string GetConnectionString(string connectionStringName)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Connection_string_not_found", new object[] { connectionStringName }));
            }
            return settings.ConnectionString;
        }

        public static string GetConnectionStringProviderName(string connectionStringName)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Connection_string_not_found", new object[] { connectionStringName }));
            }
            return settings.ProviderName;
        }

        public override object ParseExpression(string expression, Type propertyType, ExpressionBuilderContext context)
        {
            string x = string.Empty;
            bool y = true;
            if (expression != null)
            {
                if (expression.EndsWith(".connectionstring", StringComparison.OrdinalIgnoreCase))
                {
                    x = expression.Substring(0, expression.Length - ".connectionstring".Length);
                }
                else if (expression.EndsWith(".providername", StringComparison.OrdinalIgnoreCase))
                {
                    y = false;
                    x = expression.Substring(0, expression.Length - ".providername".Length);
                }
                else
                {
                    x = expression;
                }
            }
            return new Pair(x, y);
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

