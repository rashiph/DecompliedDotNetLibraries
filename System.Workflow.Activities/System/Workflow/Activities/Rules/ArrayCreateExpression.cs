namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class ArrayCreateExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeArrayCreateExpression expression2 = (CodeArrayCreateExpression) expression;
            if (expression2.SizeExpression != null)
            {
                RuleExpressionWalker.AnalyzeUsage(analysis, expression2.SizeExpression, true, false, null);
            }
            foreach (CodeExpression expression3 in expression2.Initializers)
            {
                RuleExpressionWalker.AnalyzeUsage(analysis, expression3, true, false, null);
            }
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeArrayCreateExpression expression2 = (CodeArrayCreateExpression) expression;
            CodeArrayCreateExpression expression3 = new CodeArrayCreateExpression {
                CreateType = TypeReferenceExpression.CloneType(expression2.CreateType),
                Size = expression2.Size
            };
            if (expression2.SizeExpression != null)
            {
                expression3.SizeExpression = RuleExpressionWalker.Clone(expression2.SizeExpression);
            }
            foreach (CodeExpression expression4 in expression2.Initializers)
            {
                expression3.Initializers.Add(RuleExpressionWalker.Clone(expression4));
            }
            return expression3;
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeArrayCreateExpression childExpr = (CodeArrayCreateExpression) expression;
            bool flag = RuleDecompiler.MustParenthesize(childExpr, parentExpression);
            if (flag)
            {
                stringBuilder.Append("(");
            }
            stringBuilder.Append("new ");
            RuleDecompiler.DecompileType(stringBuilder, childExpr.CreateType);
            stringBuilder.Append('[');
            if (childExpr.SizeExpression != null)
            {
                RuleExpressionWalker.Decompile(stringBuilder, childExpr.SizeExpression, null);
            }
            else if ((childExpr.Size != 0) || (childExpr.Initializers.Count == 0))
            {
                stringBuilder.Append(childExpr.Size);
            }
            stringBuilder.Append(']');
            if (childExpr.Initializers.Count > 0)
            {
                stringBuilder.Append(" { ");
                for (int i = 0; i < childExpr.Initializers.Count; i++)
                {
                    CodeExpression expression3 = childExpr.Initializers[i];
                    if (expression3 == null)
                    {
                        RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.NullConstructorTypeParameter, new object[] { i.ToString(CultureInfo.CurrentCulture), childExpr.CreateType }));
                        exception.Data["ErrorObject"] = childExpr;
                        throw exception;
                    }
                    if (i > 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    RuleExpressionWalker.Decompile(stringBuilder, expression3, null);
                }
                stringBuilder.Append('}');
            }
            if (flag)
            {
                stringBuilder.Append(")");
            }
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeArrayCreateExpression expression2 = (CodeArrayCreateExpression) expression;
            if (expression2.CreateType == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.NullTypeType);
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            RuleExpressionInfo info = execution.Validation.ExpressionInfo(expression2);
            if (expression2 == null)
            {
                InvalidOperationException exception2 = new InvalidOperationException(Messages.ExpressionNotValidated);
                exception2.Data["ErrorObject"] = expression2;
                throw exception2;
            }
            Type elementType = info.ExpressionType.GetElementType();
            int length = 0;
            if (expression2.SizeExpression != null)
            {
                Type expressionType = execution.Validation.ExpressionInfo(expression2.SizeExpression).ExpressionType;
                RuleExpressionResult result = RuleExpressionWalker.Evaluate(execution, expression2.SizeExpression);
                if (expressionType == typeof(int))
                {
                    length = (int) result.Value;
                }
                else if (expressionType == typeof(long))
                {
                    length = (int) ((long) result.Value);
                }
                else if (expressionType == typeof(uint))
                {
                    length = (int) ((uint) result.Value);
                }
                else if (expressionType == typeof(ulong))
                {
                    length = (int) ((ulong) result.Value);
                }
            }
            else if (expression2.Size != 0)
            {
                length = expression2.Size;
            }
            else
            {
                length = expression2.Initializers.Count;
            }
            Array literal = Array.CreateInstance(elementType, length);
            if (expression2.Initializers != null)
            {
                for (int i = 0; i < expression2.Initializers.Count; i++)
                {
                    CodeExpression expression3 = expression2.Initializers[i];
                    Type operandType = execution.Validation.ExpressionInfo(expression3).ExpressionType;
                    RuleExpressionResult result2 = RuleExpressionWalker.Evaluate(execution, expression3);
                    literal.SetValue(Executor.AdjustType(operandType, result2.Value, elementType), i);
                }
            }
            return new RuleLiteralResult(literal);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeArrayCreateExpression expression2 = (CodeArrayCreateExpression) expression;
            CodeArrayCreateExpression expression3 = comperand as CodeArrayCreateExpression;
            if (((expression3 == null) || (expression2.Size != expression3.Size)) || !TypeReferenceExpression.MatchType(expression2.CreateType, expression3.CreateType))
            {
                return false;
            }
            if (expression2.SizeExpression != null)
            {
                if (expression3.SizeExpression == null)
                {
                    return false;
                }
                if (!RuleExpressionWalker.Match(expression2.SizeExpression, expression3.SizeExpression))
                {
                    return false;
                }
            }
            else if (expression3.SizeExpression != null)
            {
                return false;
            }
            if (expression2.Initializers.Count != expression3.Initializers.Count)
            {
                return false;
            }
            for (int i = 0; i < expression2.Initializers.Count; i++)
            {
                if (!RuleExpressionWalker.Match(expression2.Initializers[i], expression3.Initializers[i]))
                {
                    return false;
                }
            }
            return true;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            CodeArrayCreateExpression newParent = (CodeArrayCreateExpression) expression;
            if (isWritten)
            {
                ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, new object[] { typeof(CodeObjectCreateExpression).ToString() }), 0x17a);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            if (newParent.CreateType == null)
            {
                ValidationError error2 = new ValidationError(Messages.NullTypeType, 0x53d);
                error2.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error2);
                return null;
            }
            Type lhsType = validation.ResolveType(newParent.CreateType);
            if (lhsType == null)
            {
                return null;
            }
            if (lhsType.IsArray)
            {
                ValidationError error3 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.ArrayTypeInvalid, new object[] { lhsType.Name }), 0x53d);
                error3.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error3);
                return null;
            }
            try
            {
                if (!validation.PushParentExpression(newParent))
                {
                    return null;
                }
                if (newParent.Size < 0)
                {
                    ValidationError error4 = new ValidationError(Messages.ArraySizeInvalid, 0x53d);
                    error4.UserData["ErrorObject"] = newParent;
                    validation.Errors.Add(error4);
                    return null;
                }
                if (newParent.SizeExpression != null)
                {
                    RuleExpressionInfo info = RuleExpressionWalker.Validate(validation, newParent.SizeExpression, false);
                    if (info == null)
                    {
                        return null;
                    }
                    if (((info.ExpressionType != typeof(int)) && (info.ExpressionType != typeof(uint))) && ((info.ExpressionType != typeof(long)) && (info.ExpressionType != typeof(ulong))))
                    {
                        ValidationError error5 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.ArraySizeTypeInvalid, new object[] { info.ExpressionType.Name }), 0x53d);
                        error5.UserData["ErrorObject"] = newParent;
                        validation.Errors.Add(error5);
                        return null;
                    }
                }
                bool flag = false;
                for (int i = 0; i < newParent.Initializers.Count; i++)
                {
                    CodeExpression expression3 = newParent.Initializers[i];
                    if (expression3 == null)
                    {
                        ValidationError error6 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.MissingInitializer, new object[] { lhsType.Name }), 0x53d);
                        error6.UserData["ErrorObject"] = newParent;
                        validation.Errors.Add(error6);
                        return null;
                    }
                    RuleExpressionInfo info2 = RuleExpressionWalker.Validate(validation, expression3, false);
                    if (info2 == null)
                    {
                        flag = true;
                    }
                    else
                    {
                        ValidationError error7;
                        if (!RuleValidation.StandardImplicitConversion(info2.ExpressionType, lhsType, expression3, out error7))
                        {
                            if (error7 != null)
                            {
                                error7.UserData["ErrorObject"] = newParent;
                                validation.Errors.Add(error7);
                            }
                            error7 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.InitializerMismatch, new object[] { i, lhsType.Name }), 0x545);
                            error7.UserData["ErrorObject"] = newParent;
                            validation.Errors.Add(error7);
                            return null;
                        }
                    }
                }
                if (flag)
                {
                    return null;
                }
                double size = -1.0;
                if (newParent.SizeExpression != null)
                {
                    CodePrimitiveExpression sizeExpression = newParent.SizeExpression as CodePrimitiveExpression;
                    if ((sizeExpression != null) && (sizeExpression.Value != null))
                    {
                        size = (double) Executor.AdjustType(sizeExpression.Value.GetType(), sizeExpression.Value, typeof(double));
                    }
                    if (newParent.Size > 0)
                    {
                        ValidationError error8 = new ValidationError(Messages.ArraySizeBoth, 0x53d);
                        error8.UserData["ErrorObject"] = newParent;
                        validation.Errors.Add(error8);
                        return null;
                    }
                }
                else if (newParent.Size > 0)
                {
                    size = newParent.Size;
                }
                if ((size >= 0.0) && (newParent.Initializers.Count > size))
                {
                    ValidationError error9 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.InitializerCountMismatch, new object[] { newParent.Initializers.Count, size }), 0x545);
                    error9.UserData["ErrorObject"] = newParent;
                    validation.Errors.Add(error9);
                    return null;
                }
            }
            finally
            {
                validation.PopParentExpression();
            }
            return new RuleExpressionInfo(lhsType.MakeArrayType());
        }
    }
}

