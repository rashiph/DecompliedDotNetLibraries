namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class CastExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeCastExpression expression2 = (CodeCastExpression) expression;
            RuleExpressionWalker.AnalyzeUsage(analysis, expression2.Expression, true, false, null);
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeCastExpression expression2 = (CodeCastExpression) expression;
            return new CodeCastExpression { TargetType = TypeReferenceExpression.CloneType(expression2.TargetType), Expression = RuleExpressionWalker.Clone(expression2.Expression) };
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeCastExpression childExpr = (CodeCastExpression) expression;
            CodeExpression expression3 = childExpr.Expression;
            if (expression3 == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.NullCastExpr);
                exception.Data["ErrorObject"] = childExpr;
                throw exception;
            }
            if (childExpr.TargetType == null)
            {
                RuleEvaluationException exception2 = new RuleEvaluationException(Messages.NullCastType);
                exception2.Data["ErrorObject"] = childExpr;
                throw exception2;
            }
            bool flag = RuleDecompiler.MustParenthesize(childExpr, parentExpression);
            if (flag)
            {
                stringBuilder.Append("(");
            }
            stringBuilder.Append("(");
            RuleDecompiler.DecompileType(stringBuilder, childExpr.TargetType);
            stringBuilder.Append(")");
            RuleExpressionWalker.Decompile(stringBuilder, expression3, childExpr);
            if (flag)
            {
                stringBuilder.Append(")");
            }
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeCastExpression expression2 = (CodeCastExpression) expression;
            object operandValue = RuleExpressionWalker.Evaluate(execution, expression2.Expression).Value;
            RuleExpressionInfo info = execution.Validation.ExpressionInfo(expression2);
            if (info == null)
            {
                InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated, new object[0]));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            Type expressionType = info.ExpressionType;
            if (operandValue == null)
            {
                if (ConditionHelper.IsNonNullableValueType(expressionType))
                {
                    RuleEvaluationException exception2 = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.CastIncompatibleTypes, new object[] { Messages.NullValue, RuleDecompiler.DecompileType(expressionType) }));
                    exception2.Data["ErrorObject"] = expression2;
                    throw exception2;
                }
            }
            else
            {
                operandValue = Executor.AdjustTypeWithCast(execution.Validation.ExpressionInfo(expression2.Expression).ExpressionType, operandValue, expressionType);
            }
            return new RuleLiteralResult(operandValue);
        }

        private static bool IsNumeric(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
            }
            return false;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeCastExpression expression2 = (CodeCastExpression) expression;
            CodeCastExpression expression3 = (CodeCastExpression) comperand;
            return (TypeReferenceExpression.MatchType(expression2.TargetType, expression3.TargetType) && RuleExpressionWalker.Match(expression2.Expression, expression3.Expression));
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            CodeCastExpression expression2 = (CodeCastExpression) expression;
            if (isWritten)
            {
                ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, new object[] { typeof(CodeCastExpression).ToString() }), 0x17a);
                item.UserData["ErrorObject"] = expression2;
                validation.Errors.Add(item);
                return null;
            }
            if (expression2.Expression == null)
            {
                ValidationError error2 = new ValidationError(Messages.NullCastExpr, 0x53d);
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
            if (expression2.TargetType == null)
            {
                ValidationError error4 = new ValidationError(Messages.NullCastType, 0x53d);
                error4.UserData["ErrorObject"] = expression2;
                validation.Errors.Add(error4);
                return null;
            }
            RuleExpressionInfo info = RuleExpressionWalker.Validate(validation, expression2.Expression, false);
            if (info == null)
            {
                return null;
            }
            Type expressionType = info.ExpressionType;
            Type type = validation.ResolveType(expression2.TargetType);
            if (type == null)
            {
                return null;
            }
            if (expressionType == typeof(NullLiteral))
            {
                if (ConditionHelper.IsNonNullableValueType(type))
                {
                    ValidationError error5 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CastOfNullInvalid, new object[] { RuleDecompiler.DecompileType(type) }), 0x53d);
                    error5.UserData["ErrorObject"] = expression2;
                    validation.Errors.Add(error5);
                    return null;
                }
            }
            else
            {
                Type type3 = expressionType;
                if (ConditionHelper.IsNullableValueType(type3))
                {
                    type3 = type3.GetGenericArguments()[0];
                }
                Type type4 = type;
                if (ConditionHelper.IsNullableValueType(type4))
                {
                    type4 = type4.GetGenericArguments()[0];
                }
                bool flag = false;
                if (type3.IsValueType && type4.IsValueType)
                {
                    if (type3.IsEnum)
                    {
                        flag = type4.IsEnum || IsNumeric(type4);
                    }
                    else if (type4.IsEnum)
                    {
                        flag = IsNumeric(type3);
                    }
                    else if (type3 == typeof(char))
                    {
                        flag = IsNumeric(type4);
                    }
                    else if (type4 == typeof(char))
                    {
                        flag = IsNumeric(type3);
                    }
                    else if (type3.IsPrimitive && type4.IsPrimitive)
                    {
                        try
                        {
                            Convert.ChangeType(Activator.CreateInstance(type3), type4, CultureInfo.CurrentCulture);
                            flag = true;
                        }
                        catch (Exception)
                        {
                            flag = false;
                        }
                    }
                }
                if (!flag)
                {
                    ValidationError error6;
                    flag = RuleValidation.ExplicitConversionSpecified(expressionType, type, out error6);
                    if (error6 != null)
                    {
                        error6.UserData["ErrorObject"] = expression2;
                        validation.Errors.Add(error6);
                        return null;
                    }
                }
                if (!flag)
                {
                    ValidationError error7 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CastIncompatibleTypes, new object[] { RuleDecompiler.DecompileType(expressionType), RuleDecompiler.DecompileType(type) }), 0x53d);
                    error7.UserData["ErrorObject"] = expression2;
                    validation.Errors.Add(error7);
                    return null;
                }
            }
            return new RuleExpressionInfo(type);
        }
    }
}

