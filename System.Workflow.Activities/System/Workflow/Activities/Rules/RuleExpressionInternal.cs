namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Runtime;
    using System.Text;

    internal abstract class RuleExpressionInternal
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RuleExpressionInternal()
        {
        }

        internal abstract void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier);
        internal abstract CodeExpression Clone(CodeExpression expression);
        internal abstract void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression);
        internal abstract RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution);
        internal abstract bool Match(CodeExpression leftExpression, CodeExpression rightExpression);
        internal abstract RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten);
    }
}

