namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class PropertyReferenceExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodePropertyReferenceExpression expression2 = (CodePropertyReferenceExpression) expression;
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
            analysis.AnalyzeRuleAttributes(propertyInfo, targetObject, qualifier, null, null, attributedExprs);
            if (!attributedExprs.Contains(targetObject))
            {
                RuleExpressionWalker.AnalyzeUsage(analysis, targetObject, isRead, isWritten, new RulePathQualifier(propertyInfo.Name, qualifier));
            }
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodePropertyReferenceExpression expression2 = (CodePropertyReferenceExpression) expression;
            return new CodePropertyReferenceExpression { PropertyName = expression2.PropertyName, TargetObject = RuleExpressionWalker.Clone(expression2.TargetObject) };
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodePropertyReferenceExpression expression2 = (CodePropertyReferenceExpression) expression;
            CodeExpression targetObject = expression2.TargetObject;
            if (targetObject == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.NullPropertyTarget, new object[] { expression2.PropertyName }));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            RuleExpressionWalker.Decompile(stringBuilder, targetObject, expression2);
            stringBuilder.Append('.');
            stringBuilder.Append(expression2.PropertyName);
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodePropertyReferenceExpression expression2 = (CodePropertyReferenceExpression) expression;
            object targetObject = RuleExpressionWalker.Evaluate(execution, expression2.TargetObject).Value;
            RulePropertyExpressionInfo info = execution.Validation.ExpressionInfo(expression2) as RulePropertyExpressionInfo;
            if (info == null)
            {
                InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated, new object[0]));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            return new RulePropertyResult(info.PropertyInfo, targetObject, null);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodePropertyReferenceExpression expression2 = (CodePropertyReferenceExpression) expression;
            CodePropertyReferenceExpression expression3 = (CodePropertyReferenceExpression) comperand;
            return ((expression2.PropertyName == expression3.PropertyName) && RuleExpressionWalker.Match(expression2.TargetObject, expression3.TargetObject));
        }

        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            CodePropertyReferenceExpression newParent = (CodePropertyReferenceExpression) expression;
            if (newParent.TargetObject == null)
            {
                ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullPropertyTarget, new object[] { newParent.PropertyName }), 0x53d);
                error.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error);
                return null;
            }
            if (!validation.PushParentExpression(newParent))
            {
                return null;
            }
            RuleExpressionInfo info = RuleExpressionWalker.Validate(validation, newParent.TargetObject, false);
            validation.PopParentExpression();
            if (info == null)
            {
                return null;
            }
            Type expressionType = info.ExpressionType;
            if (expressionType == null)
            {
                return null;
            }
            if (expressionType == typeof(NullLiteral))
            {
                ValidationError error2 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullPropertyTarget, new object[] { newParent.PropertyName }), 0x546);
                error2.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error2);
                return null;
            }
            bool nonPublic = false;
            BindingFlags bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            if (validation.AllowInternalMembers(expressionType))
            {
                bindingFlags |= BindingFlags.NonPublic;
                nonPublic = true;
            }
            PropertyInfo item = validation.ResolveProperty(expressionType, newParent.PropertyName, bindingFlags);
            if (item == null)
            {
                ValidationError error3 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.UnknownProperty, new object[] { newParent.PropertyName, RuleDecompiler.DecompileType(expressionType) }), 0x54a);
                error3.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error3);
                return null;
            }
            if (item.PropertyType == null)
            {
                ValidationError error4 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CouldNotDetermineMemberType, new object[] { newParent.PropertyName }), 0x194);
                error4.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error4);
                return null;
            }
            MethodInfo accessorMethod = isWritten ? item.GetSetMethod(nonPublic) : item.GetGetMethod(nonPublic);
            if (accessorMethod == null)
            {
                string format = isWritten ? Messages.UnknownPropertySet : Messages.UnknownPropertyGet;
                ValidationError error5 = new ValidationError(string.Format(CultureInfo.CurrentCulture, format, new object[] { newParent.PropertyName, RuleDecompiler.DecompileType(expressionType) }), 0x54a);
                error5.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error5);
                return null;
            }
            if (!validation.ValidateMemberAccess(newParent.TargetObject, expressionType, accessorMethod, newParent.PropertyName, newParent))
            {
                return null;
            }
            object[] customAttributes = item.GetCustomAttributes(typeof(RuleAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                Stack<MemberInfo> stack = new Stack<MemberInfo>();
                stack.Push(item);
                bool flag2 = true;
                foreach (RuleAttribute attribute in customAttributes)
                {
                    if (!attribute.Validate(validation, item, expressionType, null))
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
            return new RulePropertyExpressionInfo(item, item.PropertyType, false);
        }
    }
}

