namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class FieldReferenceExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeFieldReferenceExpression expression2 = (CodeFieldReferenceExpression) expression;
            CodeExpression targetObject = expression2.TargetObject;
            RuleExpressionWalker.AnalyzeUsage(analysis, targetObject, isRead, isWritten, new RulePathQualifier(expression2.FieldName, qualifier));
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeFieldReferenceExpression expression2 = (CodeFieldReferenceExpression) expression;
            return new CodeFieldReferenceExpression { FieldName = expression2.FieldName, TargetObject = RuleExpressionWalker.Clone(expression2.TargetObject) };
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeFieldReferenceExpression expression2 = (CodeFieldReferenceExpression) expression;
            CodeExpression targetObject = expression2.TargetObject;
            if (targetObject == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.NullFieldTarget, new object[] { expression2.FieldName }));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            RuleExpressionWalker.Decompile(stringBuilder, targetObject, expression2);
            stringBuilder.Append('.');
            stringBuilder.Append(expression2.FieldName);
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeFieldReferenceExpression expression2 = (CodeFieldReferenceExpression) expression;
            object targetObject = RuleExpressionWalker.Evaluate(execution, expression2.TargetObject).Value;
            RuleFieldExpressionInfo info = execution.Validation.ExpressionInfo(expression2) as RuleFieldExpressionInfo;
            if (info == null)
            {
                InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated, new object[0]));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            return new RuleFieldResult(targetObject, info.FieldInfo);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeFieldReferenceExpression expression2 = (CodeFieldReferenceExpression) expression;
            CodeFieldReferenceExpression expression3 = (CodeFieldReferenceExpression) comperand;
            return ((expression2.FieldName == expression3.FieldName) && RuleExpressionWalker.Match(expression2.TargetObject, expression3.TargetObject));
        }

        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            CodeFieldReferenceExpression newParent = (CodeFieldReferenceExpression) expression;
            if (newParent.TargetObject == null)
            {
                ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullFieldTarget, new object[] { newParent.FieldName }), 0x53d);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
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
                ValidationError error2 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.NullFieldTarget, new object[] { newParent.FieldName }), 0x546);
                error2.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error2);
                return null;
            }
            BindingFlags @public = BindingFlags.Public;
            if (newParent.TargetObject is CodeTypeReferenceExpression)
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
            FieldInfo field = expressionType.GetField(newParent.FieldName, @public);
            if (field == null)
            {
                ValidationError error3 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.UnknownField, new object[] { newParent.FieldName, RuleDecompiler.DecompileType(expressionType) }), 0x54a);
                error3.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error3);
                return null;
            }
            if (field.FieldType == null)
            {
                ValidationError error4 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CouldNotDetermineMemberType, new object[] { newParent.FieldName }), 0x194);
                error4.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error4);
                return null;
            }
            if (isWritten && field.IsLiteral)
            {
                ValidationError error5 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.FieldSetNotAllowed, new object[] { newParent.FieldName, RuleDecompiler.DecompileType(expressionType) }), 0x17a);
                error5.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(error5);
                return null;
            }
            if (!validation.ValidateMemberAccess(newParent.TargetObject, expressionType, field, field.Name, newParent))
            {
                return null;
            }
            validation.IsAuthorized(field.FieldType);
            return new RuleFieldExpressionInfo(field);
        }
    }
}

