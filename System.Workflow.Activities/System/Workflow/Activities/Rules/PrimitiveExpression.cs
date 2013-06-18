namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class PrimitiveExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodePrimitiveExpression expression2 = (CodePrimitiveExpression) expression;
            return new CodePrimitiveExpression(ConditionHelper.CloneObject(expression2.Value));
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodePrimitiveExpression expression2 = (CodePrimitiveExpression) expression;
            RuleDecompiler.DecompileObjectLiteral(stringBuilder, expression2.Value);
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodePrimitiveExpression expression2 = (CodePrimitiveExpression) expression;
            return new RuleLiteralResult(expression2.Value);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodePrimitiveExpression expression2 = (CodePrimitiveExpression) expression;
            CodePrimitiveExpression expression3 = (CodePrimitiveExpression) comperand;
            return ((expression2.Value == expression3.Value) || (((expression2.Value != null) && (expression3.Value != null)) && expression2.Value.Equals(expression3.Value)));
        }

        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            if (isWritten)
            {
                ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, new object[] { typeof(CodePrimitiveExpression).ToString() }), 0x17a);
                item.UserData["ErrorObject"] = expression;
                validation.Errors.Add(item);
                return null;
            }
            CodePrimitiveExpression expression2 = (CodePrimitiveExpression) expression;
            return new RuleExpressionInfo((expression2.Value != null) ? expression2.Value.GetType() : typeof(NullLiteral));
        }
    }
}

