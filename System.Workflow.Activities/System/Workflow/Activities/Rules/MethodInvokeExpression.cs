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

    internal class MethodInvokeExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeMethodInvokeExpression expression2 = (CodeMethodInvokeExpression) expression;
            CodeExpression targetObject = expression2.Method.TargetObject;
            if (analysis.Validation.ExpressionInfo(targetObject) == null)
            {
                InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated, new object[0]));
                exception.Data["ErrorObject"] = targetObject;
                throw exception;
            }
            RuleMethodInvokeExpressionInfo info2 = analysis.Validation.ExpressionInfo(expression2) as RuleMethodInvokeExpressionInfo;
            if (info2 == null)
            {
                InvalidOperationException exception2 = new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated, new object[0]));
                exception2.Data["ErrorObject"] = expression2;
                throw exception2;
            }
            MethodInfo methodInfo = info2.MethodInfo;
            List<CodeExpression> attributedExprs = new List<CodeExpression>();
            analysis.AnalyzeRuleAttributes(methodInfo, targetObject, qualifier, expression2.Parameters, methodInfo.GetParameters(), attributedExprs);
            if (!attributedExprs.Contains(targetObject))
            {
                RuleExpressionWalker.AnalyzeUsage(analysis, targetObject, true, false, null);
            }
            for (int i = 0; i < expression2.Parameters.Count; i++)
            {
                CodeExpression item = expression2.Parameters[i];
                if (!attributedExprs.Contains(item))
                {
                    RuleExpressionWalker.AnalyzeUsage(analysis, item, true, false, null);
                }
            }
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeMethodInvokeExpression expression2 = (CodeMethodInvokeExpression) expression;
            CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression {
                Method = CloneMethodReference(expression2.Method)
            };
            foreach (CodeExpression expression4 in expression2.Parameters)
            {
                expression3.Parameters.Add(RuleExpressionWalker.Clone(expression4));
            }
            return expression3;
        }

        private static CodeMethodReferenceExpression CloneMethodReference(CodeMethodReferenceExpression oldReference)
        {
            CodeMethodReferenceExpression result = new CodeMethodReferenceExpression {
                MethodName = oldReference.MethodName,
                TargetObject = RuleExpressionWalker.Clone(oldReference.TargetObject)
            };
            foreach (CodeTypeReference reference in oldReference.TypeArguments)
            {
                result.TypeArguments.Add(TypeReferenceExpression.CloneType(reference));
            }
            ConditionHelper.CloneUserData(oldReference, result);
            return result;
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeMethodInvokeExpression expression2 = (CodeMethodInvokeExpression) expression;
            if ((expression2.Method == null) || (expression2.Method.TargetObject == null))
            {
                RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.NullMethodTarget, new object[] { expression2.Method.MethodName }));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            CodeExpression targetObject = expression2.Method.TargetObject;
            RuleExpressionWalker.Decompile(stringBuilder, targetObject, expression2);
            stringBuilder.Append('.');
            stringBuilder.Append(expression2.Method.MethodName);
            stringBuilder.Append('(');
            if (expression2.Parameters != null)
            {
                for (int i = 0; i < expression2.Parameters.Count; i++)
                {
                    CodeExpression expression4 = expression2.Parameters[i];
                    if (expression4 == null)
                    {
                        RuleEvaluationException exception2 = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.NullMethodTypeParameter, new object[] { i.ToString(CultureInfo.CurrentCulture), expression2.Method.MethodName }));
                        exception2.Data["ErrorObject"] = expression2;
                        throw exception2;
                    }
                    if (i > 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    RuleExpressionWalker.Decompile(stringBuilder, expression4, null);
                }
            }
            stringBuilder.Append(')');
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            object obj3;
            CodeMethodInvokeExpression expression2 = (CodeMethodInvokeExpression) expression;
            object obj2 = RuleExpressionWalker.Evaluate(execution, expression2.Method.TargetObject).Value;
            RuleMethodInvokeExpressionInfo info = execution.Validation.ExpressionInfo(expression2) as RuleMethodInvokeExpressionInfo;
            if (info == null)
            {
                InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated, new object[0]));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            MethodInfo methodInfo = info.MethodInfo;
            if (!methodInfo.IsStatic && (obj2 == null))
            {
                RuleEvaluationException exception2 = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullMethod, new object[] { expression2.Method.MethodName }));
                exception2.Data["ErrorObject"] = expression2;
                throw exception2;
            }
            object[] parameters = null;
            RuleExpressionResult[] resultArray = null;
            if ((expression2.Parameters != null) && (expression2.Parameters.Count > 0))
            {
                int count = expression2.Parameters.Count;
                ParameterInfo[] infoArray = methodInfo.GetParameters();
                parameters = new object[infoArray.Length];
                int length = infoArray.Length;
                if (info.NeedsParamsExpansion)
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
                            resultArray = new RuleExpressionResult[expression2.Parameters.Count];
                        }
                        resultArray[index] = result;
                        if (expression3.Direction != FieldDirection.Out)
                        {
                            parameters[index] = Executor.AdjustType(expressionType, result.Value, infoArray[index].ParameterType);
                        }
                    }
                    else
                    {
                        parameters[index] = Executor.AdjustType(expressionType, result.Value, infoArray[index].ParameterType);
                    }
                    index++;
                }
                if (length < count)
                {
                    ParameterInfo info3 = infoArray[length];
                    Type parameterType = info3.ParameterType;
                    Type elementType = parameterType.GetElementType();
                    Array array = (Array) parameterType.InvokeMember(parameterType.Name, BindingFlags.CreateInstance, null, null, new object[] { count - index }, CultureInfo.CurrentCulture);
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
                obj3 = methodInfo.Invoke(obj2, parameters);
            }
            catch (TargetInvocationException exception3)
            {
                if (exception3.InnerException == null)
                {
                    throw;
                }
                throw new TargetInvocationException(string.Format(CultureInfo.CurrentCulture, Messages.Error_MethodInvoke, new object[] { RuleDecompiler.DecompileType(methodInfo.ReflectedType), methodInfo.Name, exception3.InnerException.Message }), exception3.InnerException);
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
            return new RuleLiteralResult(obj3);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeMethodInvokeExpression expression2 = (CodeMethodInvokeExpression) expression;
            CodeMethodInvokeExpression expression3 = (CodeMethodInvokeExpression) comperand;
            if (expression2.Method.MethodName != expression3.Method.MethodName)
            {
                return false;
            }
            if (!RuleExpressionWalker.Match(expression2.Method.TargetObject, expression3.Method.TargetObject))
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

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            Type expressionType = null;
            RuleMethodInvokeExpressionInfo info = null;
            ValidationError item = null;
            BindingFlags @public = BindingFlags.Public;
            CodeMethodInvokeExpression newParent = (CodeMethodInvokeExpression) expression;
            if (isWritten)
            {
                item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, new object[] { typeof(CodeMethodInvokeExpression).ToString() }), 0x17a);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            if ((newParent.Method == null) || (newParent.Method.TargetObject == null))
            {
                item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullMethodTarget, new object[] { newParent.Method.MethodName }), 0x53d);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            if ((newParent.Method.TypeArguments != null) && (newParent.Method.TypeArguments.Count > 0))
            {
                item = new ValidationError(Messages.GenericMethodsNotSupported, 0x548);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            try
            {
                if (!validation.PushParentExpression(newParent))
                {
                    return null;
                }
                RuleExpressionInfo info2 = RuleExpressionWalker.Validate(validation, newParent.Method.TargetObject, false);
                if (info2 == null)
                {
                    return null;
                }
                expressionType = info2.ExpressionType;
                if (expressionType == null)
                {
                    return null;
                }
                if (expressionType == typeof(NullLiteral))
                {
                    item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullMethodTarget, new object[] { newParent.Method.MethodName }), 0x546);
                    item.UserData["ErrorObject"] = newParent;
                    validation.Errors.Add(item);
                    expressionType = null;
                }
                List<CodeExpression> argumentExprs = new List<CodeExpression>();
                bool flag = false;
                if (newParent.Parameters != null)
                {
                    for (int i = 0; i < newParent.Parameters.Count; i++)
                    {
                        CodeExpression expression3 = newParent.Parameters[i];
                        if (expression3 == null)
                        {
                            item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullMethodParameter, new object[] { i.ToString(CultureInfo.CurrentCulture), newParent.Method.MethodName }), 0x53d);
                            item.UserData["ErrorObject"] = newParent;
                            validation.Errors.Add(item);
                            expressionType = null;
                        }
                        else
                        {
                            if (expression3 is CodeTypeReferenceExpression)
                            {
                                item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, new object[] { expression3.GetType().FullName }), 0x548);
                                item.UserData["ErrorObject"] = expression3;
                                validation.AddError(item);
                                flag = true;
                            }
                            if (RuleExpressionWalker.Validate(validation, expression3, false) == null)
                            {
                                flag = true;
                            }
                            argumentExprs.Add(expression3);
                        }
                    }
                }
                if (expressionType == null)
                {
                    return null;
                }
                if (flag)
                {
                    return null;
                }
                if (newParent.Method.TargetObject is CodeTypeReferenceExpression)
                {
                    @public |= BindingFlags.FlattenHierarchy | BindingFlags.Static;
                }
                else
                {
                    @public |= BindingFlags.Instance;
                }
                if (validation.AllowInternalMembers(expressionType))
                {
                    @public |= BindingFlags.NonPublic;
                }
                info = validation.ResolveMethod(expressionType, newParent.Method.MethodName, @public, argumentExprs, out item);
                if ((info == null) && newParent.UserData.Contains("QualifiedName"))
                {
                    string qualifiedName = newParent.UserData["QualifiedName"] as string;
                    Type type2 = validation.ResolveType(qualifiedName);
                    if (type2 != null)
                    {
                        validation.DetermineExtensionMethods(type2.Assembly);
                        info = validation.ResolveMethod(expressionType, newParent.Method.MethodName, @public, argumentExprs, out item);
                    }
                }
                if (info == null)
                {
                    item.UserData["ErrorObject"] = newParent;
                    validation.Errors.Add(item);
                    return null;
                }
            }
            finally
            {
                validation.PopParentExpression();
            }
            MethodInfo methodInfo = info.MethodInfo;
            if (methodInfo.ReturnType == null)
            {
                item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CouldNotDetermineMemberType, new object[] { newParent.Method.MethodName }), 0x194);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            if (!validation.ValidateMemberAccess(newParent.Method.TargetObject, expressionType, methodInfo, newParent.Method.MethodName, newParent))
            {
                return null;
            }
            object[] customAttributes = methodInfo.GetCustomAttributes(typeof(RuleAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                Stack<MemberInfo> stack = new Stack<MemberInfo>();
                stack.Push(methodInfo);
                bool flag2 = true;
                foreach (RuleAttribute attribute in customAttributes)
                {
                    if (!attribute.Validate(validation, methodInfo, expressionType, methodInfo.GetParameters()))
                    {
                        flag2 = false;
                    }
                }
                stack.Pop();
                if (!flag2)
                {
                    return null;
                }
            }
            if (methodInfo is ExtensionMethodInfo)
            {
                newParent.UserData["QualifiedName"] = methodInfo.DeclaringType.AssemblyQualifiedName;
            }
            return info;
        }
    }
}

