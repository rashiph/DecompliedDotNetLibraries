namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    public interface IRuleExpression
    {
        void AnalyzeUsage(RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier);
        CodeExpression Clone();
        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId="0#")]
        void Decompile(StringBuilder stringBuilder, CodeExpression parentExpression);
        RuleExpressionResult Evaluate(RuleExecution execution);
        bool Match(CodeExpression expression);
        RuleExpressionInfo Validate(RuleValidation validation, bool isWritten);
    }
}

