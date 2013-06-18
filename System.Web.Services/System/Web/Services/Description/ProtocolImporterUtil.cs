namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;
    using System.Configuration;
    using System.Web.Services;

    internal class ProtocolImporterUtil
    {
        private ProtocolImporterUtil()
        {
        }

        internal static void GenerateConstructorStatements(CodeConstructor ctor, string url, string appSettingUrlKey, string appSettingBaseUrl, bool soap11)
        {
            bool flag = (url != null) && (url.Length > 0);
            bool flag2 = (appSettingUrlKey != null) && (appSettingUrlKey.Length > 0);
            CodeAssignStatement statement = null;
            if (flag || flag2)
            {
                CodeExpression expression;
                CodePropertyReferenceExpression left = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Url");
                if (flag)
                {
                    expression = new CodePrimitiveExpression(url);
                    statement = new CodeAssignStatement(left, expression);
                }
                if (flag && !flag2)
                {
                    ctor.Statements.Add(statement);
                }
                else if (flag2)
                {
                    CodeVariableReferenceExpression expression3 = new CodeVariableReferenceExpression("urlSetting");
                    CodeTypeReferenceExpression targetObject = new CodeTypeReferenceExpression(typeof(ConfigurationManager));
                    CodePropertyReferenceExpression expression5 = new CodePropertyReferenceExpression(targetObject, "AppSettings");
                    expression = new CodeIndexerExpression(expression5, new CodeExpression[] { new CodePrimitiveExpression(appSettingUrlKey) });
                    ctor.Statements.Add(new CodeVariableDeclarationStatement(typeof(string), "urlSetting", expression));
                    if ((appSettingBaseUrl == null) || (appSettingBaseUrl.Length == 0))
                    {
                        expression = expression3;
                    }
                    else
                    {
                        if ((url == null) || (url.Length == 0))
                        {
                            throw new ArgumentException(Res.GetString("IfAppSettingBaseUrlArgumentIsSpecifiedThen0"));
                        }
                        string str = new Uri(appSettingBaseUrl).MakeRelative(new Uri(url));
                        CodeExpression[] parameters = new CodeExpression[] { expression3, new CodePrimitiveExpression(str) };
                        expression = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(string)), "Concat", parameters);
                    }
                    CodeStatement[] trueStatements = new CodeStatement[] { new CodeAssignStatement(left, expression) };
                    CodeBinaryOperatorExpression condition = new CodeBinaryOperatorExpression(expression3, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
                    if (flag)
                    {
                        ctor.Statements.Add(new CodeConditionStatement(condition, trueStatements, new CodeStatement[] { statement }));
                    }
                    else
                    {
                        ctor.Statements.Add(new CodeConditionStatement(condition, trueStatements));
                    }
                }
            }
        }
    }
}

