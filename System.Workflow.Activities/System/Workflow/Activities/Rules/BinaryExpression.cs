namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class BinaryExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeBinaryOperatorExpression expression2 = (CodeBinaryOperatorExpression) expression;
            RuleBinaryExpressionInfo info = analysis.Validation.ExpressionInfo(expression2) as RuleBinaryExpressionInfo;
            if (info != null)
            {
                MethodInfo methodInfo = info.MethodInfo;
                if (methodInfo != null)
                {
                    List<CodeExpression> attributedExprs = new List<CodeExpression>();
                    CodeExpressionCollection argExprs = new CodeExpressionCollection();
                    argExprs.Add(expression2.Left);
                    argExprs.Add(expression2.Right);
                    CodeExpression targetExpr = new CodeTypeReferenceExpression(methodInfo.DeclaringType);
                    analysis.AnalyzeRuleAttributes(methodInfo, targetExpr, qualifier, argExprs, methodInfo.GetParameters(), attributedExprs);
                }
            }
            RuleExpressionWalker.AnalyzeUsage(analysis, expression2.Left, true, false, null);
            RuleExpressionWalker.AnalyzeUsage(analysis, expression2.Right, true, false, null);
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeBinaryOperatorExpression expression2 = (CodeBinaryOperatorExpression) expression;
            return new CodeBinaryOperatorExpression { Operator = expression2.Operator, Left = RuleExpressionWalker.Clone(expression2.Left), Right = RuleExpressionWalker.Clone(expression2.Right) };
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            string str3;
            bool flag = false;
            CodeBinaryOperatorExpression childExpr = (CodeBinaryOperatorExpression) expression;
            if (childExpr.Left == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.NullBinaryOpLHS, new object[] { childExpr.Operator.ToString() }));
                exception.Data["ErrorObject"] = childExpr;
                throw exception;
            }
            if (childExpr.Right == null)
            {
                RuleEvaluationException exception2 = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.NullBinaryOpRHS, new object[] { childExpr.Operator.ToString() }));
                exception2.Data["ErrorObject"] = childExpr;
                throw exception2;
            }
            switch (childExpr.Operator)
            {
                case CodeBinaryOperatorType.Add:
                    str3 = " + ";
                    break;

                case CodeBinaryOperatorType.Subtract:
                    str3 = " - ";
                    break;

                case CodeBinaryOperatorType.Multiply:
                    str3 = " * ";
                    break;

                case CodeBinaryOperatorType.Divide:
                    str3 = " / ";
                    break;

                case CodeBinaryOperatorType.Modulus:
                    str3 = " % ";
                    break;

                case CodeBinaryOperatorType.IdentityInequality:
                    str3 = " != ";
                    break;

                case CodeBinaryOperatorType.IdentityEquality:
                case CodeBinaryOperatorType.ValueEquality:
                    str3 = " == ";
                    break;

                case CodeBinaryOperatorType.BitwiseOr:
                    str3 = " | ";
                    break;

                case CodeBinaryOperatorType.BitwiseAnd:
                    str3 = " & ";
                    break;

                case CodeBinaryOperatorType.BooleanOr:
                    str3 = " || ";
                    break;

                case CodeBinaryOperatorType.BooleanAnd:
                    str3 = " && ";
                    break;

                case CodeBinaryOperatorType.LessThan:
                    str3 = " < ";
                    break;

                case CodeBinaryOperatorType.LessThanOrEqual:
                    str3 = " <= ";
                    break;

                case CodeBinaryOperatorType.GreaterThan:
                    str3 = " > ";
                    break;

                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    str3 = " >= ";
                    break;

                default:
                {
                    NotSupportedException exception3 = new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpNotSupported, new object[] { childExpr.Operator.ToString() }));
                    exception3.Data["ErrorObject"] = childExpr;
                    throw exception3;
                }
            }
            CodeExpression left = childExpr.Left;
            CodeExpression right = childExpr.Right;
            if (childExpr.Operator == CodeBinaryOperatorType.ValueEquality)
            {
                CodePrimitiveExpression expression5 = right as CodePrimitiveExpression;
                if (expression5 != null)
                {
                    object obj2 = expression5.Value;
                    if (((obj2 != null) && (obj2.GetType() == typeof(bool))) && !((bool) obj2))
                    {
                        CodeBinaryOperatorExpression expression6 = left as CodeBinaryOperatorExpression;
                        if ((expression6 == null) || (expression6.Operator != CodeBinaryOperatorType.ValueEquality))
                        {
                            flag = RuleDecompiler.MustParenthesize(left, parentExpression);
                            if (flag)
                            {
                                stringBuilder.Append("(");
                            }
                            stringBuilder.Append("!");
                            RuleExpressionWalker.Decompile(stringBuilder, left, new CodeCastExpression());
                            if (flag)
                            {
                                stringBuilder.Append(")");
                            }
                            return;
                        }
                        str3 = " != ";
                        left = expression6.Left;
                        right = expression6.Right;
                    }
                }
            }
            else if (childExpr.Operator == CodeBinaryOperatorType.Subtract)
            {
                CodePrimitiveExpression expression7 = left as CodePrimitiveExpression;
                if ((expression7 != null) && (expression7.Value != null))
                {
                    object obj3 = expression7.Value;
                    TypeCode typeCode = Type.GetTypeCode(obj3.GetType());
                    bool flag2 = false;
                    switch (typeCode)
                    {
                        case TypeCode.Int32:
                            flag2 = ((int) obj3) == 0;
                            break;

                        case TypeCode.Int64:
                            flag2 = ((long) obj3) == 0L;
                            break;

                        case TypeCode.Single:
                            flag2 = ((float) obj3) == 0f;
                            break;

                        case TypeCode.Double:
                            flag2 = ((double) obj3) == 0.0;
                            break;

                        case TypeCode.Decimal:
                            flag2 = ((decimal) obj3) == 0M;
                            break;
                    }
                    if (flag2)
                    {
                        flag = RuleDecompiler.MustParenthesize(right, parentExpression);
                        if (flag)
                        {
                            stringBuilder.Append("(");
                        }
                        stringBuilder.Append("-");
                        RuleExpressionWalker.Decompile(stringBuilder, right, new CodeCastExpression());
                        if (flag)
                        {
                            stringBuilder.Append(")");
                        }
                        return;
                    }
                }
            }
            flag = RuleDecompiler.MustParenthesize(childExpr, parentExpression);
            if (flag)
            {
                stringBuilder.Append("(");
            }
            RuleExpressionWalker.Decompile(stringBuilder, left, childExpr);
            stringBuilder.Append(str3);
            RuleExpressionWalker.Decompile(stringBuilder, right, childExpr);
            if (flag)
            {
                stringBuilder.Append(")");
            }
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            object obj5;
            CodeBinaryOperatorExpression expression2 = (CodeBinaryOperatorExpression) expression;
            object operandValue = RuleExpressionWalker.Evaluate(execution, expression2.Left).Value;
            CodeBinaryOperatorType @operator = expression2.Operator;
            switch (@operator)
            {
                case CodeBinaryOperatorType.BooleanAnd:
                    if ((bool) operandValue)
                    {
                        return new RuleLiteralResult(RuleExpressionWalker.Evaluate(execution, expression2.Right).Value);
                    }
                    return new RuleLiteralResult(false);

                case CodeBinaryOperatorType.BooleanOr:
                    if ((bool) operandValue)
                    {
                        return new RuleLiteralResult(true);
                    }
                    return new RuleLiteralResult(RuleExpressionWalker.Evaluate(execution, expression2.Right).Value);
            }
            object obj6 = RuleExpressionWalker.Evaluate(execution, expression2.Right).Value;
            RuleBinaryExpressionInfo info = execution.Validation.ExpressionInfo(expression2) as RuleBinaryExpressionInfo;
            if (info == null)
            {
                InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated, new object[0]));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            MethodInfo methodInfo = info.MethodInfo;
            if (methodInfo != null)
            {
                if (methodInfo == Literal.ObjectEquality)
                {
                    obj5 = operandValue == obj6;
                }
                else
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object[] objArray = new object[] { Executor.AdjustType(info.LeftType, operandValue, parameters[0].ParameterType), Executor.AdjustType(info.RightType, obj6, parameters[1].ParameterType) };
                    obj5 = methodInfo.Invoke(null, objArray);
                }
            }
            else
            {
                obj5 = EvaluateBinaryOperation(expression2, info.LeftType, operandValue, @operator, info.RightType, obj6);
            }
            return new RuleLiteralResult(obj5);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static object EvaluateBinaryOperation(CodeBinaryOperatorExpression binaryExpr, Type lhsType, object lhsValue, CodeBinaryOperatorType operation, Type rhsType, object rhsValue)
        {
            Literal literal;
            Literal literal2;
            ArithmeticLiteral literal3;
            ArithmeticLiteral literal4;
            RuleEvaluationException exception;
            switch (operation)
            {
                case CodeBinaryOperatorType.Add:
                    literal3 = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (literal3 == null)
                    {
                        break;
                    }
                    literal4 = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (literal4 == null)
                    {
                        break;
                    }
                    return literal3.Add(literal4);

                case CodeBinaryOperatorType.Subtract:
                    literal3 = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (literal3 == null)
                    {
                        break;
                    }
                    literal4 = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (literal4 == null)
                    {
                        break;
                    }
                    return literal3.Subtract(literal4);

                case CodeBinaryOperatorType.Multiply:
                    literal3 = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (literal3 == null)
                    {
                        break;
                    }
                    literal4 = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (literal4 == null)
                    {
                        break;
                    }
                    return literal3.Multiply(literal4);

                case CodeBinaryOperatorType.Divide:
                    literal3 = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (literal3 == null)
                    {
                        break;
                    }
                    literal4 = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (literal4 == null)
                    {
                        break;
                    }
                    return literal3.Divide(literal4);

                case CodeBinaryOperatorType.Modulus:
                    literal3 = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (literal3 == null)
                    {
                        break;
                    }
                    literal4 = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (literal4 == null)
                    {
                        break;
                    }
                    return literal3.Modulus(literal4);

                case CodeBinaryOperatorType.IdentityInequality:
                    return (lhsValue != rhsValue);

                case CodeBinaryOperatorType.IdentityEquality:
                    return (lhsValue == rhsValue);

                case CodeBinaryOperatorType.ValueEquality:
                    literal = Literal.MakeLiteral(lhsType, lhsValue);
                    if (literal == null)
                    {
                        break;
                    }
                    literal2 = Literal.MakeLiteral(rhsType, rhsValue);
                    if (literal2 == null)
                    {
                        break;
                    }
                    return literal.Equal(literal2);

                case CodeBinaryOperatorType.BitwiseOr:
                    literal3 = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (literal3 == null)
                    {
                        break;
                    }
                    literal4 = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (literal4 == null)
                    {
                        break;
                    }
                    return literal3.BitOr(literal4);

                case CodeBinaryOperatorType.BitwiseAnd:
                    literal3 = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (literal3 == null)
                    {
                        break;
                    }
                    literal4 = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (literal4 == null)
                    {
                        break;
                    }
                    return literal3.BitAnd(literal4);

                case CodeBinaryOperatorType.LessThan:
                    literal = Literal.MakeLiteral(lhsType, lhsValue);
                    if (literal == null)
                    {
                        break;
                    }
                    literal2 = Literal.MakeLiteral(rhsType, rhsValue);
                    if (literal2 == null)
                    {
                        break;
                    }
                    return literal.LessThan(literal2);

                case CodeBinaryOperatorType.LessThanOrEqual:
                    literal = Literal.MakeLiteral(lhsType, lhsValue);
                    if (literal == null)
                    {
                        break;
                    }
                    literal2 = Literal.MakeLiteral(rhsType, rhsValue);
                    if (literal2 == null)
                    {
                        break;
                    }
                    return literal.LessThanOrEqual(literal2);

                case CodeBinaryOperatorType.GreaterThan:
                    literal = Literal.MakeLiteral(lhsType, lhsValue);
                    if (literal == null)
                    {
                        break;
                    }
                    literal2 = Literal.MakeLiteral(rhsType, rhsValue);
                    if (literal2 == null)
                    {
                        break;
                    }
                    return literal.GreaterThan(literal2);

                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    literal = Literal.MakeLiteral(lhsType, lhsValue);
                    if (literal == null)
                    {
                        break;
                    }
                    literal2 = Literal.MakeLiteral(rhsType, rhsValue);
                    if (literal2 == null)
                    {
                        break;
                    }
                    return literal.GreaterThanOrEqual(literal2);

                default:
                    exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpNotSupported, new object[] { operation.ToString() }));
                    exception.Data["ErrorObject"] = binaryExpr;
                    throw exception;
            }
            exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpFails, new object[] { operation.ToString(), RuleDecompiler.DecompileType(lhsType), RuleDecompiler.DecompileType(rhsType) }));
            exception.Data["ErrorObject"] = binaryExpr;
            throw exception;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeBinaryOperatorExpression expression2 = (CodeBinaryOperatorExpression) expression;
            CodeBinaryOperatorExpression expression3 = (CodeBinaryOperatorExpression) comperand;
            return (((expression2.Operator == expression3.Operator) && RuleExpressionWalker.Match(expression2.Left, expression3.Left)) && RuleExpressionWalker.Match(expression2.Right, expression3.Right));
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static bool PromotionPossible(Type type, CodeExpression expression)
        {
            if (type == typeof(int))
            {
                CodePrimitiveExpression expression2 = expression as CodePrimitiveExpression;
                if (expression2 != null)
                {
                    int num = (int) expression2.Value;
                    return (num >= 0);
                }
            }
            else if (type == typeof(long))
            {
                CodePrimitiveExpression expression3 = expression as CodePrimitiveExpression;
                if (expression3 != null)
                {
                    long num2 = (long) expression3.Value;
                    return (num2 >= 0L);
                }
            }
            return false;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            ValidationError error;
            CodeBinaryOperatorExpression newParent = (CodeBinaryOperatorExpression) expression;
            if (!validation.PushParentExpression(newParent))
            {
                return null;
            }
            if (isWritten)
            {
                error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, new object[] { typeof(CodeBinaryOperatorExpression).ToString() }), 0x17a);
                error.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error);
            }
            RuleExpressionInfo info = null;
            RuleExpressionInfo info2 = null;
            if (newParent.Left == null)
            {
                error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullBinaryOpLHS, new object[] { newParent.Operator.ToString() }), 0x541);
                error.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error);
            }
            else
            {
                if (newParent.Left is CodeTypeReferenceExpression)
                {
                    error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, new object[] { newParent.Left.GetType().FullName }), 0x548);
                    error.UserData["ErrorObject"] = newParent.Left;
                    validation.AddError(error);
                    return null;
                }
                info = RuleExpressionWalker.Validate(validation, newParent.Left, false);
            }
            if (newParent.Right == null)
            {
                error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullBinaryOpRHS, new object[] { newParent.Operator.ToString() }), 0x543);
                error.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error);
            }
            else
            {
                if (newParent.Right is CodeTypeReferenceExpression)
                {
                    error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, new object[] { newParent.Right.GetType().FullName }), 0x548);
                    error.UserData["ErrorObject"] = newParent.Right;
                    validation.AddError(error);
                    return null;
                }
                info2 = RuleExpressionWalker.Validate(validation, newParent.Right, false);
            }
            validation.PopParentExpression();
            RuleBinaryExpressionInfo info3 = null;
            if ((info != null) && (info2 != null))
            {
                Type expressionType = info.ExpressionType;
                Type rhs = info2.ExpressionType;
                switch (newParent.Operator)
                {
                    case CodeBinaryOperatorType.Add:
                    case CodeBinaryOperatorType.Subtract:
                    case CodeBinaryOperatorType.Multiply:
                    case CodeBinaryOperatorType.Divide:
                    case CodeBinaryOperatorType.Modulus:
                    case CodeBinaryOperatorType.BitwiseOr:
                    case CodeBinaryOperatorType.BitwiseAnd:
                        info3 = ArithmeticLiteral.ResultType(newParent.Operator, expressionType, newParent.Left, rhs, newParent.Right, validation, out error);
                        if (info3 == null)
                        {
                            if ((!(expressionType == typeof(ulong)) || !PromotionPossible(rhs, newParent.Right)) && (!(rhs == typeof(ulong)) || !PromotionPossible(expressionType, newParent.Left)))
                            {
                                error.UserData["ErrorObject"] = newParent;
                                validation.Errors.Add(error);
                            }
                            else
                            {
                                info3 = new RuleBinaryExpressionInfo(expressionType, rhs, typeof(ulong));
                            }
                        }
                        goto Label_063E;

                    case CodeBinaryOperatorType.IdentityInequality:
                    case CodeBinaryOperatorType.IdentityEquality:
                        info3 = new RuleBinaryExpressionInfo(expressionType, rhs, typeof(bool));
                        goto Label_063E;

                    case CodeBinaryOperatorType.ValueEquality:
                        info3 = Literal.AllowedComparison(expressionType, newParent.Left, rhs, newParent.Right, newParent.Operator, validation, out error);
                        if (info3 == null)
                        {
                            if ((!(expressionType == typeof(ulong)) || !PromotionPossible(rhs, newParent.Right)) && (!(rhs == typeof(ulong)) || !PromotionPossible(expressionType, newParent.Left)))
                            {
                                error.UserData["ErrorObject"] = newParent;
                                validation.Errors.Add(error);
                            }
                            else
                            {
                                info3 = new RuleBinaryExpressionInfo(expressionType, rhs, typeof(bool));
                            }
                        }
                        goto Label_063E;

                    case CodeBinaryOperatorType.BooleanOr:
                    case CodeBinaryOperatorType.BooleanAnd:
                        info3 = new RuleBinaryExpressionInfo(expressionType, rhs, typeof(bool));
                        if (expressionType != typeof(bool))
                        {
                            error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.LogicalOpBadTypeLHS, new object[] { newParent.Operator.ToString(), (expressionType == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(expressionType) }), 0x542);
                            error.UserData["ErrorObject"] = newParent;
                            validation.Errors.Add(error);
                            info3 = null;
                        }
                        if (rhs != typeof(bool))
                        {
                            error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.LogicalOpBadTypeRHS, new object[] { newParent.Operator.ToString(), (rhs == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(rhs) }), 0x544);
                            error.UserData["ErrorObject"] = newParent;
                            validation.Errors.Add(error);
                            info3 = null;
                        }
                        goto Label_063E;

                    case CodeBinaryOperatorType.LessThan:
                    case CodeBinaryOperatorType.LessThanOrEqual:
                    case CodeBinaryOperatorType.GreaterThan:
                    case CodeBinaryOperatorType.GreaterThanOrEqual:
                        info3 = Literal.AllowedComparison(expressionType, newParent.Left, rhs, newParent.Right, newParent.Operator, validation, out error);
                        if (info3 == null)
                        {
                            if ((!(expressionType == typeof(ulong)) || !PromotionPossible(rhs, newParent.Right)) && (!(rhs == typeof(ulong)) || !PromotionPossible(expressionType, newParent.Left)))
                            {
                                error.UserData["ErrorObject"] = newParent;
                                validation.Errors.Add(error);
                            }
                            else
                            {
                                info3 = new RuleBinaryExpressionInfo(expressionType, rhs, typeof(bool));
                            }
                        }
                        goto Label_063E;
                }
                error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpNotSupported, new object[] { newParent.Operator.ToString() }), 0x548);
                error.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error);
            }
        Label_063E:
            if (info3 != null)
            {
                MethodInfo methodInfo = info3.MethodInfo;
                if (methodInfo == null)
                {
                    return info3;
                }
                object[] customAttributes = methodInfo.GetCustomAttributes(typeof(RuleAttribute), true);
                if ((customAttributes == null) || (customAttributes.Length <= 0))
                {
                    return info3;
                }
                Stack<MemberInfo> stack = new Stack<MemberInfo>();
                stack.Push(methodInfo);
                bool flag = true;
                foreach (RuleAttribute attribute in customAttributes)
                {
                    if (!attribute.Validate(validation, methodInfo, methodInfo.DeclaringType, methodInfo.GetParameters()))
                    {
                        flag = false;
                    }
                }
                stack.Pop();
                if (!flag)
                {
                    return null;
                }
            }
            return info3;
        }
    }
}

