namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class DirectionExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeDirectionExpression expression2 = (CodeDirectionExpression) expression;
            CodeExpression expression3 = expression2.Expression;
            bool flag = false;
            bool flag2 = true;
            RulePathQualifier qualifier2 = null;
            switch (expression2.Direction)
            {
                case FieldDirection.In:
                    flag = false;
                    flag2 = true;
                    qualifier2 = new RulePathQualifier("*", null);
                    break;

                case FieldDirection.Out:
                    flag = true;
                    flag2 = false;
                    qualifier2 = null;
                    break;

                case FieldDirection.Ref:
                    flag = true;
                    flag2 = true;
                    qualifier2 = analysis.ForWrites ? null : new RulePathQualifier("*", null);
                    break;
            }
            RuleExpressionWalker.AnalyzeUsage(analysis, expression3, flag2, flag, qualifier2);
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeDirectionExpression expression2 = (CodeDirectionExpression) expression;
            return new CodeDirectionExpression { Direction = expression2.Direction, Expression = RuleExpressionWalker.Clone(expression2.Expression) };
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeDirectionExpression expression2 = (CodeDirectionExpression) expression;
            string str = null;
            if (expression2.Direction == FieldDirection.Out)
            {
                str = "out ";
            }
            else if (expression2.Direction == FieldDirection.Ref)
            {
                str = "ref ";
            }
            if (str != null)
            {
                stringBuilder.Append(str);
            }
            RuleExpressionWalker.Decompile(stringBuilder, expression2.Expression, expression2);
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeDirectionExpression expression2 = (CodeDirectionExpression) expression;
            return RuleExpressionWalker.Evaluate(execution, expression2.Expression);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeDirectionExpression expression2 = (CodeDirectionExpression) expression;
            CodeDirectionExpression expression3 = (CodeDirectionExpression) comperand;
            return ((expression2.Direction == expression3.Direction) && RuleExpressionWalker.Match(expression2.Expression, expression3.Expression));
        }

        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            RuleExpressionInfo info;
            bool flag;
            CodeDirectionExpression expression2 = (CodeDirectionExpression) expression;
            if (isWritten)
            {
                ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, new object[] { typeof(CodeDirectionExpression).ToString() }), 0x17a);
                item.UserData["ErrorObject"] = expression2;
                validation.Errors.Add(item);
                return null;
            }
            if (expression2.Expression == null)
            {
                ValidationError error2 = new ValidationError(Messages.NullDirectionTarget, 0x53d);
                error2.UserData["ErrorObject"] = expression2;
                validation.Errors.Add(error2);
                return null;
            }
            if (expression2.Expression is CodeTypeReferenceExpression)
            {
                ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, new object[] { expression2.Expression.GetType().FullName }), 0x548);
                error.UserData["ErrorObject"] = expression2.Expression;
                validation.AddError(error);
                return null;
            }
            if (expression2.Direction == FieldDirection.Ref)
            {
                flag = true;
                if (RuleExpressionWalker.Validate(validation, expression2.Expression, false) == null)
                {
                    return null;
                }
                info = RuleExpressionWalker.Validate(validation, expression2.Expression, true);
            }
            else if (expression2.Direction == FieldDirection.Out)
            {
                flag = true;
                info = RuleExpressionWalker.Validate(validation, expression2.Expression, true);
            }
            else
            {
                flag = false;
                info = RuleExpressionWalker.Validate(validation, expression2.Expression, false);
            }
            if (info == null)
            {
                return null;
            }
            Type expressionType = info.ExpressionType;
            if (expressionType == null)
            {
                return null;
            }
            if (((expressionType != typeof(NullLiteral)) && flag) && !expressionType.IsByRef)
            {
                expressionType = expressionType.MakeByRefType();
            }
            return new RuleExpressionInfo(expressionType);
        }
    }
}

