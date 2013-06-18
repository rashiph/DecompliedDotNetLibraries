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

    internal class IndexerPropertyExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeIndexerExpression expression2 = (CodeIndexerExpression) expression;
            CodeExpression targetObject = expression2.TargetObject;
            if (analysis.Validation.ExpressionInfo(targetObject) == null)
            {
                InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated, new object[0]));
                exception.Data["ErrorObject"] = targetObject;
                throw exception;
            }
            RulePropertyExpressionInfo info2 = analysis.Validation.ExpressionInfo(expression2) as RulePropertyExpressionInfo;
            if (info2 == null)
            {
                InvalidOperationException exception2 = new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated, new object[0]));
                exception2.Data["ErrorObject"] = expression2;
                throw exception2;
            }
            PropertyInfo propertyInfo = info2.PropertyInfo;
            List<CodeExpression> attributedExprs = new List<CodeExpression>();
            analysis.AnalyzeRuleAttributes(propertyInfo, targetObject, qualifier, expression2.Indices, propertyInfo.GetIndexParameters(), attributedExprs);
            if (!attributedExprs.Contains(targetObject))
            {
                RuleExpressionWalker.AnalyzeUsage(analysis, targetObject, isRead, isWritten, qualifier);
            }
            for (int i = 0; i < expression2.Indices.Count; i++)
            {
                CodeExpression item = expression2.Indices[i];
                if (!attributedExprs.Contains(item))
                {
                    RuleExpressionWalker.AnalyzeUsage(analysis, item, true, false, null);
                }
            }
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeIndexerExpression expression2 = (CodeIndexerExpression) expression;
            CodeExpression targetObject = RuleExpressionWalker.Clone(expression2.TargetObject);
            CodeExpression[] indices = new CodeExpression[expression2.Indices.Count];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = RuleExpressionWalker.Clone(expression2.Indices[i]);
            }
            return new CodeIndexerExpression(targetObject, indices);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeIndexerExpression expression2 = (CodeIndexerExpression) expression;
            CodeExpression targetObject = expression2.TargetObject;
            if (targetObject == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.NullIndexerTarget, new object[0]));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            if ((expression2.Indices == null) || (expression2.Indices.Count == 0))
            {
                RuleEvaluationException exception2 = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.MissingIndexExpressions, new object[0]));
                exception2.Data["ErrorObject"] = expression2;
                throw exception2;
            }
            RuleExpressionWalker.Decompile(stringBuilder, targetObject, expression2);
            stringBuilder.Append('[');
            RuleExpressionWalker.Decompile(stringBuilder, expression2.Indices[0], null);
            for (int i = 1; i < expression2.Indices.Count; i++)
            {
                stringBuilder.Append(", ");
                RuleExpressionWalker.Decompile(stringBuilder, expression2.Indices[i], null);
            }
            stringBuilder.Append(']');
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeIndexerExpression expression2 = (CodeIndexerExpression) expression;
            RulePropertyExpressionInfo info = execution.Validation.ExpressionInfo(expression2) as RulePropertyExpressionInfo;
            if (info == null)
            {
                InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated, new object[0]));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            PropertyInfo propertyInfo = info.PropertyInfo;
            object targetObject = RuleExpressionWalker.Evaluate(execution, expression2.TargetObject).Value;
            if (targetObject == null)
            {
                RuleEvaluationException exception2 = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullIndexer, new object[0]));
                exception2.Data["ErrorObject"] = expression2;
                throw exception2;
            }
            int count = expression2.Indices.Count;
            ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
            object[] indexerArguments = new object[indexParameters.Length];
            int length = indexParameters.Length;
            if (info.NeedsParamsExpansion)
            {
                length--;
            }
            int index = 0;
            while (index < length)
            {
                Type expressionType = execution.Validation.ExpressionInfo(expression2.Indices[index]).ExpressionType;
                RuleExpressionResult result = RuleExpressionWalker.Evaluate(execution, expression2.Indices[index]);
                indexerArguments[index] = Executor.AdjustType(expressionType, result.Value, indexParameters[index].ParameterType);
                index++;
            }
            if (length < count)
            {
                ParameterInfo info3 = indexParameters[length];
                Type parameterType = info3.ParameterType;
                Type elementType = parameterType.GetElementType();
                Array array = (Array) parameterType.InvokeMember(parameterType.Name, BindingFlags.CreateInstance, null, null, new object[] { count - index }, CultureInfo.CurrentCulture);
                while (index < count)
                {
                    Type operandType = execution.Validation.ExpressionInfo(expression2.Indices[index]).ExpressionType;
                    RuleExpressionResult result2 = RuleExpressionWalker.Evaluate(execution, expression2.Indices[index]);
                    array.SetValue(Executor.AdjustType(operandType, result2.Value, elementType), (int) (index - length));
                    index++;
                }
                indexerArguments[length] = array;
            }
            return new RulePropertyResult(propertyInfo, targetObject, indexerArguments);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeIndexerExpression expression2 = (CodeIndexerExpression) expression;
            CodeIndexerExpression expression3 = (CodeIndexerExpression) comperand;
            if (!RuleExpressionWalker.Match(expression2.TargetObject, expression3.TargetObject))
            {
                return false;
            }
            if (expression2.Indices.Count != expression3.Indices.Count)
            {
                return false;
            }
            for (int i = 0; i < expression2.Indices.Count; i++)
            {
                if (!RuleExpressionWalker.Match(expression2.Indices[i], expression3.Indices[i]))
                {
                    return false;
                }
            }
            return true;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            ValidationError item = null;
            RulePropertyExpressionInfo info = null;
            bool nonPublic = false;
            Type expressionType = null;
            CodeIndexerExpression newParent = (CodeIndexerExpression) expression;
            CodeExpression targetObject = newParent.TargetObject;
            if (targetObject == null)
            {
                item = new ValidationError(Messages.NullIndexerTarget, 0x53d);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            if (targetObject is CodeTypeReferenceExpression)
            {
                item = new ValidationError(Messages.IndexersCannotBeStatic, 0x53d);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            if ((newParent.Indices == null) || (newParent.Indices.Count == 0))
            {
                item = new ValidationError(Messages.MissingIndexExpressions, 0x53d);
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
                RuleExpressionInfo info2 = RuleExpressionWalker.Validate(validation, newParent.TargetObject, false);
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
                    item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullIndexerTarget, new object[0]), 0x53d);
                    item.UserData["ErrorObject"] = newParent;
                    validation.Errors.Add(item);
                    expressionType = null;
                }
                List<CodeExpression> argumentExprs = new List<CodeExpression>();
                bool flag2 = false;
                for (int i = 0; i < newParent.Indices.Count; i++)
                {
                    CodeExpression expression4 = newParent.Indices[i];
                    if (expression4 == null)
                    {
                        item = new ValidationError(Messages.NullIndexExpression, 0x53d);
                        item.UserData["ErrorObject"] = newParent;
                        validation.Errors.Add(item);
                        flag2 = true;
                    }
                    else
                    {
                        CodeDirectionExpression expression5 = expression4 as CodeDirectionExpression;
                        if ((expression5 != null) && (expression5.Direction != FieldDirection.In))
                        {
                            item = new ValidationError(Messages.IndexerArgCannotBeRefOrOut, 0x19d);
                            item.UserData["ErrorObject"] = newParent;
                            validation.Errors.Add(item);
                            flag2 = true;
                        }
                        if (expression4 is CodeTypeReferenceExpression)
                        {
                            item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, new object[] { expression4.GetType().FullName }), 0x548);
                            item.UserData["ErrorObject"] = expression4;
                            validation.AddError(item);
                            flag2 = true;
                        }
                        if (RuleExpressionWalker.Validate(validation, expression4, false) == null)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            argumentExprs.Add(expression4);
                        }
                    }
                }
                if (expressionType == null)
                {
                    return null;
                }
                if (flag2)
                {
                    return null;
                }
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                if (validation.AllowInternalMembers(expressionType))
                {
                    bindingFlags |= BindingFlags.NonPublic;
                    nonPublic = true;
                }
                info = validation.ResolveIndexerProperty(expressionType, bindingFlags, argumentExprs, out item);
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
            PropertyInfo propertyInfo = info.PropertyInfo;
            MethodInfo accessorMethod = isWritten ? propertyInfo.GetSetMethod(nonPublic) : propertyInfo.GetGetMethod(nonPublic);
            if (accessorMethod == null)
            {
                string format = isWritten ? Messages.UnknownPropertySet : Messages.UnknownPropertyGet;
                item = new ValidationError(string.Format(CultureInfo.CurrentCulture, format, new object[] { propertyInfo.Name, RuleDecompiler.DecompileType(expressionType) }), 0x54a);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            if (!validation.ValidateMemberAccess(targetObject, expressionType, accessorMethod, propertyInfo.Name, newParent))
            {
                return null;
            }
            object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(RuleAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                Stack<MemberInfo> stack = new Stack<MemberInfo>();
                stack.Push(propertyInfo);
                bool flag3 = true;
                foreach (RuleAttribute attribute in customAttributes)
                {
                    if (!attribute.Validate(validation, propertyInfo, expressionType, propertyInfo.GetIndexParameters()))
                    {
                        flag3 = false;
                    }
                }
                stack.Pop();
                if (!flag3)
                {
                    return null;
                }
            }
            return info;
        }
    }
}

