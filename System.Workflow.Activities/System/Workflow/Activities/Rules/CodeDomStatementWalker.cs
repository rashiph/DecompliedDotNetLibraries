namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal static class CodeDomStatementWalker
    {
        internal static void AnalyzeUsage(RuleAnalysis analysis, CodeStatement statement)
        {
            GetStatement(statement).AnalyzeUsage(analysis);
        }

        internal static CodeStatement Clone(CodeStatement statement)
        {
            if (statement == null)
            {
                return null;
            }
            return GetStatement(statement).Clone();
        }

        internal static void Decompile(StringBuilder stringBuilder, CodeStatement statement)
        {
            GetStatement(statement).Decompile(stringBuilder);
        }

        internal static void Execute(RuleExecution execution, CodeStatement statement)
        {
            GetStatement(statement).Execute(execution);
        }

        private static RuleCodeDomStatement GetStatement(CodeStatement statement)
        {
            Type type = statement.GetType();
            if (type == typeof(CodeExpressionStatement))
            {
                return ExpressionStatement.Create(statement);
            }
            if (type == typeof(CodeAssignStatement))
            {
                return AssignmentStatement.Create(statement);
            }
            NotSupportedException exception = new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Messages.CodeStatementNotHandled, new object[] { statement.GetType().FullName }));
            exception.Data["ErrorObject"] = statement;
            throw exception;
        }

        internal static bool Match(CodeStatement firstStatement, CodeStatement secondStatement)
        {
            if ((firstStatement == null) && (secondStatement == null))
            {
                return true;
            }
            if ((firstStatement == null) || (secondStatement == null))
            {
                return false;
            }
            if (firstStatement.GetType() != secondStatement.GetType())
            {
                return false;
            }
            return GetStatement(firstStatement).Match(secondStatement);
        }

        internal static bool Validate(RuleValidation validation, CodeStatement statement)
        {
            return GetStatement(statement).Validate(validation);
        }

        private delegate RuleCodeDomStatement WrapperCreator(CodeStatement statement);
    }
}

