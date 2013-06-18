namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class ThisExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            if ((!analysis.ForWrites || isWritten) && (analysis.ForWrites || isRead))
            {
                StringBuilder builder = new StringBuilder("this/");
                for (RulePathQualifier qualifier2 = qualifier; qualifier2 != null; qualifier2 = qualifier2.Next)
                {
                    builder.Append(qualifier2.Name);
                    if (qualifier2.Name == "*")
                    {
                        if (qualifier2.Next != null)
                        {
                            throw new NotSupportedException(Messages.InvalidWildCardInPathQualifier);
                        }
                    }
                    else
                    {
                        builder.Append("/");
                    }
                }
                analysis.AddSymbol(builder.ToString());
            }
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            return new CodeThisReferenceExpression();
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            stringBuilder.Append("this");
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            return execution.ThisLiteralResult;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            return true;
        }

        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            if (isWritten)
            {
                ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, new object[] { typeof(CodeThisReferenceExpression).ToString() }), 0x17a);
                item.UserData["ErrorObject"] = expression;
                validation.Errors.Add(item);
                return null;
            }
            return new RuleExpressionInfo(validation.ThisType);
        }
    }
}

