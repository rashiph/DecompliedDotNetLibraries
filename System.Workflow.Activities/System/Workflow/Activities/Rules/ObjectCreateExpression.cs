namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class ObjectCreateExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeObjectCreateExpression expression2 = (CodeObjectCreateExpression) expression;
            foreach (CodeExpression expression3 in expression2.Parameters)
            {
                RuleExpressionWalker.AnalyzeUsage(analysis, expression3, true, false, null);
            }
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeObjectCreateExpression expression2 = (CodeObjectCreateExpression) expression;
            CodeObjectCreateExpression expression3 = new CodeObjectCreateExpression {
                CreateType = TypeReferenceExpression.CloneType(expression2.CreateType)
            };
            foreach (CodeExpression expression4 in expression2.Parameters)
            {
                expression3.Parameters.Add(RuleExpressionWalker.Clone(expression4));
            }
            return expression3;
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeObjectCreateExpression childExpr = (CodeObjectCreateExpression) expression;
            bool flag = RuleDecompiler.MustParenthesize(childExpr, parentExpression);
            if (flag)
            {
                stringBuilder.Append("(");
            }
            stringBuilder.Append("new ");
            RuleDecompiler.DecompileType(stringBuilder, childExpr.CreateType);
            stringBuilder.Append('(');
            for (int i = 0; i < childExpr.Parameters.Count; i++)
            {
                CodeExpression expression3 = childExpr.Parameters[i];
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
            stringBuilder.Append(')');
            if (flag)
            {
                stringBuilder.Append(")");
            }
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            object obj2;
            CodeObjectCreateExpression expression2 = (CodeObjectCreateExpression) expression;
            if (expression2.CreateType == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.NullTypeType);
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            RuleExpressionInfo info = execution.Validation.ExpressionInfo(expression2);
            if (info == null)
            {
                InvalidOperationException exception2 = new InvalidOperationException(Messages.ExpressionNotValidated);
                exception2.Data["ErrorObject"] = expression2;
                throw exception2;
            }
            RuleConstructorExpressionInfo info2 = info as RuleConstructorExpressionInfo;
            if (info2 == null)
            {
                return new RuleLiteralResult(Activator.CreateInstance(info.ExpressionType));
            }
            ConstructorInfo constructorInfo = info2.ConstructorInfo;
            object[] parameters = null;
            RuleExpressionResult[] resultArray = null;
            if ((expression2.Parameters != null) && (expression2.Parameters.Count > 0))
            {
                int count = expression2.Parameters.Count;
                ParameterInfo[] infoArray = constructorInfo.GetParameters();
                parameters = new object[infoArray.Length];
                int length = infoArray.Length;
                if (info2.NeedsParamsExpansion)
                {
                    length--;
                }
                int index = 0;
                while (index < length)
                {
                    Type expressionType = execution.Validation.ExpressionInfo(expression2.Parameters[index]).ExpressionType;
                    RuleExpressionResult result = RuleExpressionWalker.Evaluate(execution, expression2.Parameters[index]);
                    CodeDirectionExpression expression3 = expression2.Parameters[index] as CodeDirectionExpression;
                    if ((expression3 != null) && ((expression3.Direction == FieldDirection.Ref) || (expression3.Direction == FieldDirection.Out)))
                    {
                        if (resultArray == null)
                        {
                            resultArray = new RuleExpressionResult[count];
                        }
                        resultArray[index] = result;
                    }
                    parameters[index] = Executor.AdjustType(expressionType, result.Value, infoArray[index].ParameterType);
                    index++;
                }
                if (length < count)
                {
                    ParameterInfo info4 = infoArray[length];
                    Type elementType = info4.ParameterType.GetElementType();
                    Array array = Array.CreateInstance(elementType, (int) (count - index));
                    while (index < count)
                    {
                        Type operandType = execution.Validation.ExpressionInfo(expression2.Parameters[index]).ExpressionType;
                        RuleExpressionResult result2 = RuleExpressionWalker.Evaluate(execution, expression2.Parameters[index]);
                        array.SetValue(Executor.AdjustType(operandType, result2.Value, elementType), (int) (index - length));
                        index++;
                    }
                    parameters[length] = array;
                }
            }
            try
            {
                obj2 = constructorInfo.Invoke(parameters);
            }
            catch (TargetInvocationException exception3)
            {
                if (exception3.InnerException == null)
                {
                    throw;
                }
                throw new TargetInvocationException(string.Format(CultureInfo.CurrentCulture, Messages.Error_ConstructorInvoke, new object[] { RuleDecompiler.DecompileType(info2.ExpressionType), exception3.InnerException.Message }), exception3.InnerException);
            }
            if (resultArray != null)
            {
                for (int i = 0; i < expression2.Parameters.Count; i++)
                {
                    if (resultArray[i] != null)
                    {
                        resultArray[i].Value = parameters[i];
                    }
                }
            }
            return new RuleLiteralResult(obj2);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeObjectCreateExpression expression2 = (CodeObjectCreateExpression) expression;
            CodeObjectCreateExpression expression3 = comperand as CodeObjectCreateExpression;
            if (expression3 == null)
            {
                return false;
            }
            if (!TypeReferenceExpression.MatchType(expression2.CreateType, expression3.CreateType))
            {
                return false;
            }
            if (expression2.Parameters.Count != expression3.Parameters.Count)
            {
                return false;
            }
            for (int i = 0; i < expression2.Parameters.Count; i++)
            {
                if (!RuleExpressionWalker.Match(expression2.Parameters[i], expression3.Parameters[i]))
                {
                    return false;
                }
            }
            return true;
        }

        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            ValidationError error;
            CodeObjectCreateExpression newParent = (CodeObjectCreateExpression) expression;
            if (isWritten)
            {
                error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, new object[] { typeof(CodeObjectCreateExpression).ToString() }), 0x17a);
                error.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error);
                return null;
            }
            if (newParent.CreateType == null)
            {
                error = new ValidationError(Messages.NullTypeType, 0x53d);
                error.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error);
                return null;
            }
            Type type = validation.ResolveType(newParent.CreateType);
            if (type == null)
            {
                return null;
            }
            List<CodeExpression> argumentExprs = new List<CodeExpression>();
            try
            {
                if (!validation.PushParentExpression(newParent))
                {
                    return null;
                }
                bool flag = false;
                for (int i = 0; i < newParent.Parameters.Count; i++)
                {
                    CodeExpression expression3 = newParent.Parameters[i];
                    if (expression3 == null)
                    {
                        error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullConstructorParameter, new object[] { i.ToString(CultureInfo.CurrentCulture), RuleDecompiler.DecompileType(type) }), 0x53d);
                        error.UserData["ErrorObject"] = newParent;
                        validation.Errors.Add(error);
                        flag = true;
                    }
                    else
                    {
                        if (RuleExpressionWalker.Validate(validation, expression3, false) == null)
                        {
                            flag = true;
                        }
                        argumentExprs.Add(expression3);
                    }
                }
                if (flag)
                {
                    return null;
                }
            }
            finally
            {
                validation.PopParentExpression();
            }
            BindingFlags constructorBindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (validation.AllowInternalMembers(type))
            {
                constructorBindingFlags |= BindingFlags.NonPublic;
            }
            if (type.IsValueType && (argumentExprs.Count == 0))
            {
                return new RuleExpressionInfo(type);
            }
            if (type.IsAbstract)
            {
                error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.UnknownConstructor, new object[] { RuleDecompiler.DecompileType(type) }), 0x137);
                error.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error);
                return null;
            }
            RuleConstructorExpressionInfo info2 = validation.ResolveConstructor(type, constructorBindingFlags, argumentExprs, out error);
            if (info2 == null)
            {
                error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.UnknownConstructor, new object[] { RuleDecompiler.DecompileType(type) }), 0x137);
                error.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error);
                return null;
            }
            return info2;
        }
    }
}

